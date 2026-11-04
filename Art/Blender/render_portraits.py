import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy
from mathutils import Vector

from featherkit import PROJECT_ROOT

BLEND_DIR = os.path.join(PROJECT_ROOT, "Art", "Blender", "Characters")
OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "UI", "Portraits")
NAMES = ["Knight", "Archer", "Owl", "Grunt", "Shaman", "Brute", "Spitter", "Guard", "Chieftain"]


def bounds():
    lo = Vector((1e9, 1e9, 1e9))
    hi = Vector((-1e9, -1e9, -1e9))
    depsgraph = bpy.context.evaluated_depsgraph_get()
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        evaluated = obj.evaluated_get(depsgraph)
        for v in evaluated.to_mesh().vertices:
            w = evaluated.matrix_world @ v.co
            lo = Vector(map(min, lo, w))
            hi = Vector(map(max, hi, w))
        evaluated.to_mesh_clear()
    return lo, hi


def render(name, size):
    bpy.ops.wm.open_mainfile(filepath=os.path.join(BLEND_DIR, f"{name}.blend"))
    scene = bpy.context.scene
    scene.frame_set(1)
    lo, hi = bounds()
    height = hi.z - lo.z
    target = Vector(((lo.x + hi.x) * 0.5, (lo.y + hi.y) * 0.5, hi.z - height * 0.3))

    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.show_shadows = False
    scene.display.shading.show_cavity = True
    scene.display.shading.show_object_outline = True
    scene.display.shading.object_outline_color = (0.05, 0.04, 0.06)
    scene.render.resolution_x = size
    scene.render.resolution_y = size
    scene.render.film_transparent = True
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"

    cam_data = bpy.data.cameras.new("PortraitCam")
    cam_data.type = "ORTHO"
    cam_data.ortho_scale = height * 0.62
    cam = bpy.data.objects.new("PortraitCam", cam_data)
    scene.collection.objects.link(cam)
    cam.location = target + Vector((0.9, -3.0, 0.25))
    cam.rotation_euler = (target - cam.location).to_track_quat("-Z", "Y").to_euler()
    scene.camera = cam

    scene.render.filepath = os.path.join(OUT_DIR, f"{name}.png")
    bpy.ops.render.render(write_still=True)


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    for name in NAMES:
        render(name, 512)
        print(f"PORTRAIT {name}")


main()
