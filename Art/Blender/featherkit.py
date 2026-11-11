import math
import os

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))


def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.render.fps = 30
    return scene


def material(name, rgb, emit=0.0, rough=0.65, metal=0.0):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    try:
        mat.use_nodes = True
    except AttributeError:
        pass
    bsdf = next(n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Metallic"].default_value = metal
    if emit > 0:
        bsdf.inputs["Emission Color"].default_value = (*rgb, 1.0)
        bsdf.inputs["Emission Strength"].default_value = emit
    mat.diffuse_color = (*rgb, 1.0)
    return mat


def _matrix(loc, rot, scl):
    quat = Euler([math.radians(a) for a in rot]).to_quaternion()
    return Matrix.LocRotScale(Vector(loc), quat, Vector(scl))


class MeshBuilder:
    def __init__(self):
        self.bm = bmesh.new()
        self.deform = self.bm.verts.layers.deform.verify()
        self.groups = []
        self.materials = []

    def _index(self, items, item):
        if item not in items:
            items.append(item)
        return items.index(item)

    def _finish(self, verts, bone, mat, smooth, cut_below=None):
        if cut_below is not None:
            dead = [v for v in verts if v.co.z < cut_below]
            bmesh.ops.delete(self.bm, geom=dead, context="VERTS")
            verts = [v for v in verts if v.is_valid]
        gi = self._index(self.groups, bone)
        mi = self._index(self.materials, mat)
        for v in verts:
            v[self.deform][gi] = 1.0
        for f in {f for v in verts for f in v.link_faces}:
            f.material_index = mi
            f.smooth = smooth

    def sphere(self, bone, mat, loc, scl, rot=(0, 0, 0), seg=16, rings=10, smooth=True, cut_below=None):
        r = bmesh.ops.create_uvsphere(self.bm, u_segments=seg, v_segments=rings, radius=1.0, matrix=_matrix(loc, rot, scl))
        self._finish(r["verts"], bone, mat, smooth, cut_below)

    def ico(self, bone, mat, loc, scl, rot=(0, 0, 0), sub=1, smooth=False):
        r = bmesh.ops.create_icosphere(self.bm, subdivisions=sub, radius=1.0, matrix=_matrix(loc, rot, scl))
        self._finish(r["verts"], bone, mat, smooth)

    def cube(self, bone, mat, loc, scl, rot=(0, 0, 0), smooth=False):
        r = bmesh.ops.create_cube(self.bm, size=1.0, matrix=_matrix(loc, rot, scl))
        self._finish(r["verts"], bone, mat, smooth)

    def cone(self, bone, mat, loc, r1, r2, depth, rot=(0, 0, 0), seg=12, smooth=False):
        r = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=seg,
                                  radius1=r1, radius2=r2, depth=depth, matrix=_matrix(loc, rot, (1, 1, 1)))
        self._finish(r["verts"], bone, mat, smooth)

    def between(self, bone, mat, p1, p2, r1, r2, seg=10, smooth=True):
        a, b = Vector(p1), Vector(p2)
        d = b - a
        quat = Vector((0, 0, 1)).rotation_difference(d.normalized())
        m = Matrix.LocRotScale((a + b) / 2, quat, Vector((1, 1, 1)))
        r = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=seg,
                                  radius1=r1, radius2=r2, depth=d.length, matrix=m)
        self._finish(r["verts"], bone, mat, smooth)

    def build(self, name, rig):
        mesh = bpy.data.meshes.new(name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        for g in self.groups:
            obj.vertex_groups.new(name=g)
        for m in self.materials:
            mesh.materials.append(m)
        if rig is not None:
            obj.parent = rig
            mod = obj.modifiers.new("Armature", "ARMATURE")
            mod.object = rig
        return obj


def make_rig(bones):
    arm = bpy.data.armatures.new("Rig")
    rig = bpy.data.objects.new("Rig", arm)
    bpy.context.scene.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode="EDIT")
    for name, head, parent in bones:
        eb = arm.edit_bones.new(name)
        eb.head = head
        eb.tail = (head[0], head[1], head[2] + 0.15)
        eb.roll = 0.0
        if parent:
            eb.parent = arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode="OBJECT")
    for pb in rig.pose.bones:
        pb.rotation_mode = "XYZ"
    return rig


def add_action(rig, name, keys):
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    rig.animation_data_create()
    rig.animation_data.action = act
    for frame, pose in keys:
        for pb in rig.pose.bones:
            p = pose.get(pb.name, {})
            pb.location = p.get("loc", (0, 0, 0))
            pb.rotation_euler = [math.radians(a) for a in p.get("rot", (0, 0, 0))]
            pb.scale = p.get("scl", (1, 1, 1))
            for path in ("location", "rotation_euler", "scale"):
                pb.keyframe_insert(path, frame=frame, group=pb.name)
    act.use_frame_range = True
    act.frame_start = keys[0][0]
    act.frame_end = keys[-1][0]
    return act


def export_fbx(path, objects=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    for obj in bpy.context.scene.objects:
        obj.select_set(obj in objects if objects is not None else obj.type in {"ARMATURE", "MESH"})
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={"ARMATURE", "MESH"},
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        bake_anim=True,
        bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False,
        bake_anim_force_startend_keying=True,
        bake_anim_simplify_factor=0.0,
    )


def export_static_fbx(path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    for obj in bpy.context.scene.objects:
        obj.select_set(obj.type == "MESH")
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={"MESH"},
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        bake_anim=False,
    )


def render_strip(path, target, frames, cam_loc, size=360):
    import numpy as np

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = True
    scene.render.resolution_x = size
    scene.render.resolution_y = size
    scene.render.film_transparent = False
    scene.render.image_settings.file_format = "PNG"

    cam_data = bpy.data.cameras.new("PreviewCam")
    cam = bpy.data.objects.new("PreviewCam", cam_data)
    scene.collection.objects.link(cam)
    cam.location = cam_loc
    direction = Vector(target) - cam.location
    cam.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    cam_data.lens = 50
    scene.camera = cam

    tiles = []
    tmp = os.path.join(os.path.dirname(path), "_tile.png")
    for frame in frames:
        scene.frame_set(frame)
        scene.render.filepath = tmp
        bpy.ops.render.render(write_still=True)
        img = bpy.data.images.load(tmp)
        px = np.array(img.pixels[:], dtype=np.float32).reshape(size, size, 4)
        tiles.append(px)
        bpy.data.images.remove(img)
    strip = np.concatenate(tiles, axis=1)
    out = bpy.data.images.new("strip", width=size * len(frames), height=size, alpha=True)
    out.pixels = strip.ravel()
    out.filepath_raw = path
    out.file_format = "PNG"
    out.save()
    os.remove(tmp)
    bpy.data.objects.remove(cam)
