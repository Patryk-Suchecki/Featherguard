import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

from featherkit import PROJECT_ROOT, export_fbx

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Characters")
CLIPS = ("Idle", "Attack", "Cast", "Hit", "Death", "Victory")
OPTIONAL_CLIPS = ("AttackWindup",)
LOOPING = {"Idle", "Victory", "AttackWindup"}
HIT_EVENT = "OnHitFrame"
NEEDS_HIT_EVENT = {"Attack", "Cast"}
FPS = 30
MAX_TRIANGLES = 12000
MAX_BONES = 60


class ExportError(Exception):
    pass


def find_rig():
    rigs = [o for o in bpy.context.scene.objects if o.type == "ARMATURE"]
    if len(rigs) != 1:
        raise ExportError(f"expected exactly one armature in the scene, found {len(rigs)}")
    return rigs[0]


def skinned_meshes(rig):
    meshes = [o for o in rig.children_recursive if o.type == "MESH"]
    if not meshes:
        raise ExportError(f"no mesh is parented to the armature '{rig.name}'")
    for mesh in meshes:
        if not any(m.type == "ARMATURE" and m.object == rig for m in mesh.modifiers):
            raise ExportError(f"mesh '{mesh.name}' has no Armature modifier pointing to '{rig.name}'")
    return meshes


def is_rotated(obj):
    if obj.rotation_mode == "QUATERNION":
        return obj.rotation_quaternion.angle > 1e-4
    if obj.rotation_mode == "AXIS_ANGLE":
        return abs(obj.rotation_axis_angle[0]) > 1e-4
    return any(abs(r) > 1e-4 for r in obj.rotation_euler)


def check_transforms(objects, warnings):
    for obj in objects:
        if any(abs(s - 1.0) > 1e-4 for s in obj.scale) or is_rotated(obj):
            warnings.append(f"'{obj.name}' has unapplied rotation or scale; apply them (Ctrl+A) before export")


def check_materials(meshes, warnings):
    used = set()
    for mesh in meshes:
        slots = mesh.material_slots
        for polygon in mesh.data.polygons:
            if polygon.material_index < len(slots) and slots[polygon.material_index].material:
                used.add(slots[polygon.material_index].material.name)
    if len(used) > 1:
        warnings.append(f"{len(used)} materials are used ({', '.join(sorted(used))}); a character should have one")


def drop_preview_outlines(meshes, warnings):
    for mesh in meshes:
        for modifier in mesh.modifiers:
            if modifier.type == "SOLIDIFY" and (modifier.use_flip_normals or "outline" in modifier.name.lower()):
                modifier.show_render = False
                modifier.show_viewport = False
                warnings.append(f"preview outline '{modifier.name}' on '{mesh.name}' is left out; the toon shader draws the outline")


def clip_actions(warnings):
    missing = [name for name in CLIPS if name not in bpy.data.actions]
    if missing:
        raise ExportError("missing actions: " + ", ".join(missing))
    names = CLIPS + tuple(name for name in OPTIONAL_CLIPS if name in bpy.data.actions)
    extra = [a for a in bpy.data.actions if a.name not in names]
    for action in extra:
        warnings.append(f"action '{action.name}' is not a game clip and is left out of the export")
        bpy.data.actions.remove(action)
    return [bpy.data.actions[name] for name in names]


def clip_meta(action):
    start, end = (int(round(f)) for f in action.frame_range)
    events = [{"name": m.name, "frame": m.frame} for m in sorted(action.pose_markers, key=lambda m: m.frame)]
    if action.name in NEEDS_HIT_EVENT and not any(e["name"] == HIT_EVENT for e in events):
        raise ExportError(f"action '{action.name}' needs a pose marker named '{HIT_EVENT}' on the frame the hit lands")
    for event in events:
        if not start <= event["frame"] <= end:
            raise ExportError(f"marker '{event['name']}' in '{action.name}' is outside frames {start}-{end}")
    return {"name": action.name, "start": start, "end": end, "loop": action.name in LOOPING, "events": events}


def report(rig, meshes, warnings):
    scene = bpy.context.scene
    rig.animation_data_create()
    action, frame = rig.animation_data.action, scene.frame_current
    idle = bpy.data.actions["Idle"]
    rig.animation_data.action = idle
    scene.frame_set(int(round(idle.frame_range[0])))
    depsgraph = bpy.context.evaluated_depsgraph_get()
    triangles = 0
    top = 0.0
    for mesh in meshes:
        evaluated = mesh.evaluated_get(depsgraph)
        data = evaluated.to_mesh()
        data.calc_loop_triangles()
        triangles += len(data.loop_triangles)
        top = max([top] + [(evaluated.matrix_world @ v.co).z for v in data.vertices])
        evaluated.to_mesh_clear()
    rig.animation_data.action = action
    scene.frame_set(frame)
    bones = len(rig.data.bones)
    if triangles > MAX_TRIANGLES:
        warnings.append(f"{triangles} triangles is over the budget of {MAX_TRIANGLES}")
    if bones > MAX_BONES:
        warnings.append(f"{bones} bones is over the budget of {MAX_BONES}")
    return triangles, bones, top


def writable(path):
    if os.path.exists(path) and not os.access(path, os.W_OK):
        rel = os.path.relpath(path, PROJECT_ROOT).replace(os.sep, "/")
        raise ExportError(f"{rel} is read-only; lock it first: git lfs lock {rel}")


def expose_principled(meshes):
    for material in {slot.material for mesh in meshes for slot in mesh.material_slots if slot.material and slot.material.node_tree}:
        nodes = material.node_tree.nodes
        principled = next((n for n in nodes if n.type == "BSDF_PRINCIPLED"), None)
        output = next((n for n in nodes if n.type == "OUTPUT_MATERIAL" and n.is_active_output), None)
        if principled and output:
            material.node_tree.links.new(principled.outputs["BSDF"], output.inputs["Surface"])


def export_albedo(meshes, path, warnings):
    images = {node.image
              for mesh in meshes
              for slot in mesh.material_slots
              if slot.material and slot.material.node_tree
              for node in slot.material.node_tree.nodes
              if node.type == "TEX_IMAGE" and node.image}
    if not images:
        warnings.append("no image texture on the materials; the model is exported without one")
        return
    if len(images) > 1:
        raise ExportError("the materials use more than one image; a character has one albedo texture")
    image = images.pop()
    writable(path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    image.save(filepath=path)
    image.filepath = path
    if image.packed_file:
        image.unpack(method="REMOVE")


def main():
    if not bpy.app.background:
        raise ExportError("run it from the command line with -b; in an open Blender it would change your file")
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = os.path.abspath(argv[0]) if argv else OUT_DIR
    name = os.path.splitext(os.path.basename(bpy.data.filepath))[0]
    if not name:
        raise ExportError("open a saved .blend file; its name becomes the character name")

    warnings = []
    scene = bpy.context.scene
    if scene.render.fps != FPS:
        warnings.append(f"scene runs at {scene.render.fps} fps; clips are expected at {FPS} fps")

    rig = find_rig()
    meshes = skinned_meshes(rig)
    check_transforms([rig] + meshes, warnings)
    check_materials(meshes, warnings)
    drop_preview_outlines(meshes, warnings)
    actions = clip_actions(warnings)
    meta = {"clips": [clip_meta(action) for action in actions]}
    triangles, bones, height = report(rig, meshes, warnings)

    fbx_path = os.path.join(out_dir, f"{name}.fbx")
    json_path = os.path.join(out_dir, f"{name}.clips.json")
    writable(fbx_path)
    writable(json_path)
    export_albedo(meshes, os.path.join(out_dir, "Textures", f"{name}_Albedo.png"), warnings)
    expose_principled(meshes)

    rig.animation_data_create()
    rig.animation_data.action = actions[0]
    export_fbx(fbx_path, [rig] + meshes)
    with open(json_path, "w") as f:
        json.dump(meta, f, indent=2)

    for warning in warnings:
        print(f"WARNING {warning}")
    print(f"EXPORTED {name}: {triangles} triangles, {bones} bones, height {height:.2f} m -> {out_dir}")


try:
    main()
except ExportError as error:
    print(f"EXPORT FAILED {error}")
    sys.exit(1)
