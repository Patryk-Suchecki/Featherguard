import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

from featherkit import PROJECT_ROOT, export_fbx

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Characters")
CLIPS = ("Idle", "Attack", "Cast", "Hit", "Death", "Victory")
LOOPING = {"Idle", "Victory"}
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


def check_transforms(objects, warnings):
    for obj in objects:
        if any(abs(s - 1.0) > 1e-4 for s in obj.scale) or any(abs(r) > 1e-4 for r in obj.rotation_euler):
            warnings.append(f"'{obj.name}' has unapplied rotation or scale; apply them (Ctrl+A) before export")


def clip_actions(warnings):
    missing = [name for name in CLIPS if name not in bpy.data.actions]
    if missing:
        raise ExportError("missing actions: " + ", ".join(missing))
    extra = [a for a in bpy.data.actions if a.name not in CLIPS]
    for action in extra:
        warnings.append(f"action '{action.name}' is not a game clip and is left out of the export")
        bpy.data.actions.remove(action)
    return [bpy.data.actions[name] for name in CLIPS]


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


def main():
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
    actions = clip_actions(warnings)
    meta = {"clips": [clip_meta(action) for action in actions]}
    triangles, bones, height = report(rig, meshes, warnings)

    fbx_path = os.path.join(out_dir, f"{name}.fbx")
    json_path = os.path.join(out_dir, f"{name}.clips.json")
    writable(fbx_path)
    writable(json_path)

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
