"""Turns the Stronghold render scene into game-ready models and a layout.

Run on the camp render scene (the scene itself is never saved):
    blender -b <camp>.blend -P Art/Blender/Environment/Stronghold/convert_camp.py -- [Name ...]

For every model it joins the visible parts of the model's group, decimates them to the
budget of docs/ART_PIPELINE.md, bakes the colour of the full-detail parts (with their
emission and a light ambient occlusion) into one albedo atlas, and writes:
    Art/Blender/Environment/Stronghold/<Name>.blend
    Assets/Art/Environment/Stronghold/<Name>.fbx
    Assets/Art/Environment/Stronghold/Textures/<Name>_Albedo.png
With no names given it converts every model and also writes
Assets/Art/Environment/Stronghold/Stronghold.layout.json: where every model, seat,
light and label stands and the camera, in Unity coordinates (x, z, y of Blender), and
renders the sky, the moon, the mountains, the forest and the far ground into the 360 degree skybox
Assets/Art/Environment/Stronghold/Textures/Sky_Panorama.png.
"""

import json
import math
import os
import re
import sys

import bmesh
import bpy
import numpy as np
from mathutils import Matrix, Vector

REPO = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", ".."))
BLEND_DIR = os.path.join(REPO, "Art", "Blender", "Environment", "Stronghold")
ASSET_DIR = os.path.join(REPO, "Assets", "Art", "Environment", "Stronghold")
TEXTURE_DIR = os.path.join(ASSET_DIR, "Textures")
LAYOUT = os.path.join(ASSET_DIR, "Stronghold.layout.json")

AO_STRENGTH = 0.45
NEAR_GROUND = dict(x=20.0, y=26.0)


def objects():
    return [o for o in bpy.context.scene.objects if not o.hide_render and o.visible_get() and o.type in {"MESH", "CURVE"}]


def descendants(obj):
    for child in obj.children:
        yield child
        yield from descendants(child)


def under(site_name):
    root = bpy.data.objects[site_name]

    def collect():
        shown = set(objects())
        return [o for o in descendants(root) if o in shown]

    return collect


def starting(*prefixes, exclude=()):
    return lambda: [o for o in objects() if o.name.startswith(prefixes) and not o.name.startswith(exclude) and not under_site(o)]


def under_site(obj):
    while obj.parent:
        obj = obj.parent
    return obj.name.startswith("Site")


def emits(obj):
    for slot in obj.material_slots:
        mat = slot.material
        if mat is None or mat.node_tree is None:
            continue
        for node in mat.node_tree.nodes:
            if node.type == "EMISSION":
                return True
            if node.type == "BSDF_PRINCIPLED" and not node.inputs["Emission Strength"].is_linked and node.inputs["Emission Strength"].default_value > 0:
                return True
    return False


def unlit(collect):
    return lambda: [o for o in collect() if not emits(o)]


def near(face_center):
    return abs(face_center.x) < NEAR_GROUND["x"] and face_center.y < NEAR_GROUND["y"]


MODELS = {
    "Forge": dict(parts=under("Site_Kuznia"), tris=5000, origin="Site_Kuznia", place="Forge"),
    "WiseWomansHut": dict(parts=under("Site_Chata"), tris=5500, origin="Site_Chata", place="WiseWomansHut"),
    "Tavern": dict(parts=under("Site_Karczma"), tris=5500, origin="Site_Karczma", place="Tavern"),
    "Shop": dict(parts=under("Site_Stragan_V5"), tris=4000, origin="Site_Stragan_V5", place="Shop"),
    "Signpost": dict(parts=under("Site_Drogowskaz"), tris=1500, origin="Site_Drogowskaz", place="Embark"),
    "Campfire": dict(parts=unlit(under("Site_Fire")), tris=800, origin="Site_Fire", place="Camp"),
    "SacredOak": dict(parts=lambda: starting("Dab | pien", "Dab | swiece", "V3 | rzezbiony opiekun", "V3 | kaptur posagu")() + under("Site_Oak")(),
                      tris=4500, origin="base:Dab | pien, konary i korzenie", place="SacredOak", ray=0.6),
    "SacredOakCrown": dict(parts=starting("Dab | korona"), tris=2000, origin="base:Dab | pien, konary i korzenie", clumps=True, ray=1.2, tint="Dab_liscie_atlas"),
    "Stump": dict(parts=starting("V4 | pieniek 0"), tris=175, origin="base", size=256),
    "LanternPost": dict(parts=starting("Camp_LanternPost"), tris=540, origin="base", size=256),
    "Graveyard": dict(parts=starting("Camp_Graveyard", "Nagrobek_"), tris=800, origin="world", size=512),
    "Palisade": dict(parts=starting("Plot2_", "V4 | wiazania palisady", "V4 | rozszczepiona palisada", "V4 | krzywe poziome zerdzie"),
                     tris=6000, origin="world"),
    "DeadTree": dict(parts=starting("V4 | galaziste drzewo"), tris=2500, origin="base", ray=0.4, size=512, remesh=0.04),
    "Ground": dict(parts=starting("Teren", "V4 | zwirowe pobocza", "Sciezka_do_wiedzmy"), tris=9000, origin="world", size=2048,
                   faces=near, ray=0.4),
    "GroundDetails": dict(parts=starting("Kamien", "V4 | kamienie osadzone", "V4 | kepka", "V3 | skala w zboczu", "V3 | lupki",
                                         "V4 | przyziemne rozety", "Stopien", "Karczma | trawa"),
                          tris=9000, origin="world", ray=0.15),
}

SKY_PARTS = ("Ksiezyc", "V4 | rzezba odleglych gor", "V4 | dalsza warstwa lasu", "V4 | las z galezistymi")
SKY_GROUND = "Teren"
SKY_LIGHT = "Swiatlo ksiezyca"
SKY_SIZE = (4096, 2048)
FLAME_IMAGES = {"Campfire_Flame_8x8": "Campfire_Flame.png"}

SEATS = "V4 | pieniek "
LIGHTS = {
    "Campfire VFX light": "Fire",
    "Kuznia | palenisko.001": "Forge",
    "Karczma | swiatlo_okna_1": "Tavern",
    "Karczma | swiatlo_latarni": "Tavern",
    "Chata | swiatlo_latarni": "Hut",
    "Chata | swiatlo_okna": "Hut",
    "Shrine candle light": "Oak",
    "Lantern warm glow": "Lantern",
    "V3 | ciepla latarnia straganu": "Shop",
}


def triangles(mesh):
    return sum(len(p.vertices) - 2 for p in mesh.polygons)


def select_only(objs, active=None):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objs:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = active or (objs[0] if objs else None)


def evaluated_copy(obj, depsgraph, keep=None):
    evaluated = obj.evaluated_get(depsgraph)
    mesh = bpy.data.meshes.new_from_object(evaluated, depsgraph=depsgraph)
    mesh.transform(obj.matrix_world)
    if keep is not None:
        bm = bmesh.new()
        bm.from_mesh(mesh)
        bmesh.ops.delete(bm, geom=[f for f in bm.faces if not keep(f.calc_center_median())], context="FACES")
        bm.to_mesh(mesh)
        bm.free()
    copy = bpy.data.objects.new(obj.name + "_low", mesh)
    bpy.context.scene.collection.objects.link(copy)
    return copy


def clumps(low, radius=0.55, every=3):
    centers = [p.center.copy() for i, p in enumerate(low.data.polygons) if i % every == 0]
    meta = bpy.data.metaballs.new("Clumps")
    meta.resolution = 0.18
    meta.threshold = 0.6
    for center in centers:
        element = meta.elements.new()
        element.co = center
        element.radius = radius
    ball = bpy.data.objects.new("Clumps", meta)
    bpy.context.scene.collection.objects.link(ball)
    depsgraph = bpy.context.evaluated_depsgraph_get()
    mesh = bpy.data.meshes.new_from_object(ball.evaluated_get(depsgraph), depsgraph=depsgraph)
    bpy.data.objects.remove(ball)
    old = low.data
    low.data = mesh
    bpy.data.meshes.remove(old)


def build_low(name, parts, spec):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    copies = [evaluated_copy(p, depsgraph, spec.get("faces")) for p in parts]
    for copy in [c for c in copies if len(c.data.polygons) == 0]:
        bpy.data.objects.remove(copy)
    copies = [c for c in copies if c.name in bpy.data.objects]
    select_only(copies)
    bpy.ops.object.join()
    low = bpy.context.view_layer.objects.active
    low.name = name
    low.data.name = name
    if spec.get("clumps"):
        clumps(low)
    low.data.materials.clear()
    for polygon in low.data.polygons:
        polygon.material_index = 0

    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.mesh.remove_doubles(threshold=0.002)
    bpy.ops.object.mode_set(mode="OBJECT")

    before = triangles(low.data)
    if spec.get("remesh"):
        modifier = low.modifiers.new("Remesh", "REMESH")
        modifier.mode = "VOXEL"
        modifier.voxel_size = spec["remesh"]
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    for _ in range(4):
        current = triangles(low.data)
        if current <= spec["tris"] * 1.05:
            break
        modifier = low.modifiers.new("Decimate", "DECIMATE")
        modifier.ratio = spec["tris"] / current
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    print(f"  {name}: {before} -> {triangles(low.data)} triangles from {len(copies)} parts")

    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(60), island_margin=0.004, area_weight=0.0, scale_to_bounds=False)
    bpy.ops.uv.pack_islands(margin=0.004)
    bpy.ops.object.mode_set(mode="OBJECT")
    return low


def bake_pass(low, sources, bake_type, size, ray, selected_to_active=True):
    image = bpy.data.images.new(f"{low.name}_{bake_type}", size, size, alpha=False, float_buffer=True)
    material = low.data.materials[0]
    node = material.node_tree.nodes.new("ShaderNodeTexImage")
    node.image = image
    material.node_tree.nodes.active = node
    select_only(sources + [low] if selected_to_active else [low], low)
    bake = bpy.context.scene.render.bake
    bake.use_selected_to_active = selected_to_active
    bake.cage_extrusion = ray * 0.4
    bake.max_ray_distance = ray
    bake.margin = 6
    bake.use_pass_direct = False
    bake.use_pass_indirect = False
    bake.use_pass_color = True
    bpy.ops.object.bake(type=bake_type)
    pixels = np.array(image.pixels[:], dtype=np.float32).reshape(size, size, 4)
    material.node_tree.nodes.remove(node)
    bpy.data.images.remove(image)
    return pixels


def average_colour(image_name):
    image = bpy.data.images[image_name]
    pixels = np.array(image.pixels[:], dtype=np.float32).reshape(-1, 4)
    weights = pixels[:, 3:4]
    srgb = (pixels[:, :3] * weights).sum(axis=0) / max(float(weights.sum()), 1.0)
    return np.where(srgb <= 0.04045, srgb / 12.92, np.power((srgb + 0.055) / 1.055, 2.4))


def leaves(image_name, size, tiles=8, shade=0.45):
    image = bpy.data.images[image_name]
    width, height = image.size
    atlas = np.array(image.pixels[:], dtype=np.float32).reshape(height, width, 4)
    rows = (np.arange(size) * tiles * height // size) % height
    cols = (np.arange(size) * tiles * width // size) % width
    tiled = atlas[rows][:, cols]
    linear = np.where(tiled[..., :3] <= 0.04045, tiled[..., :3] / 12.92, np.power((tiled[..., :3] + 0.055) / 1.055, 2.4))
    alpha = tiled[..., 3:4]
    color = np.ones((size, size, 4), dtype=np.float32)
    color[..., :3] = linear * alpha + average_colour(image_name) * shade * (1.0 - alpha)
    return color


def bake_albedo(name, low, sources, size, ray, tint=None):
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    low.data.materials.append(material)

    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.cycles.samples = 4
    if tint:
        color = leaves(tint, size)
    else:
        color = bake_pass(low, sources, "DIFFUSE", size, ray)
    if any(emits(s) for s in sources):
        emission = bake_pass(low, sources, "EMIT", size, ray)
        color[..., :3] = np.maximum(color[..., :3], np.clip(emission[..., :3], 0.0, 1.0))
    scene.cycles.samples = 32
    ao = bake_pass(low, sources, "AO", size, ray, selected_to_active=False)
    color[..., :3] *= 1.0 - AO_STRENGTH * (1.0 - ao[..., :1])

    srgb = np.where(color <= 0.0031308, color * 12.92, 1.055 * np.power(np.clip(color, 0.0031308, None), 1 / 2.4) - 0.055)
    srgb[..., 3] = 1.0
    image = bpy.data.images.new(f"{name}_Albedo", size, size, alpha=False)
    image.pixels[:] = np.clip(srgb, 0.0, 1.0).ravel()
    os.makedirs(TEXTURE_DIR, exist_ok=True)
    image.filepath_raw = os.path.join(TEXTURE_DIR, f"{name}_Albedo.png")
    image.file_format = "PNG"
    image.save()
    image.pack()

    nodes = material.node_tree.nodes
    texture = nodes.new("ShaderNodeTexImage")
    texture.image = image
    bsdf = next(n for n in nodes if n.type == "BSDF_PRINCIPLED")
    material.node_tree.links.new(texture.outputs["Color"], bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value = 1.0


def bottom_centre(objs):
    corners = [o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    return Vector(((min(c.x for c in corners) + max(c.x for c in corners)) / 2,
                   (min(c.y for c in corners) + max(c.y for c in corners)) / 2,
                   min(c.z for c in corners)))


def origin_matrix(spec, parts):
    origin = spec["origin"]
    if origin == "world":
        return Matrix.Identity(4)
    if origin == "base":
        return Matrix.Translation(bottom_centre(parts))
    if origin.startswith("base:"):
        return Matrix.Translation(bottom_centre([bpy.data.objects[origin[5:]]]))
    return bpy.data.objects[origin].matrix_world.copy()


def write_outputs(name, low):
    os.makedirs(BLEND_DIR, exist_ok=True)
    model_scene = bpy.data.scenes.new(name)
    model_scene.collection.objects.link(low)
    bpy.data.libraries.write(os.path.join(BLEND_DIR, f"{name}.blend"), {model_scene}, fake_user=True, compress=True)
    bpy.data.scenes.remove(model_scene)
    select_only([low])
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(ASSET_DIR, f"{name}.fbx"),
        use_selection=True,
        object_types={"MESH"},
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        mesh_smooth_type="FACE",
        bake_anim=False,
        path_mode="STRIP",
    )


def unity(v):
    return [round(v.x, 3), round(v.z, 3), round(v.y, 3)]


def placement(matrix):
    location, rotation, scale = matrix.decompose()
    return dict(position=unity(location), yaw=round(-math.degrees(rotation.to_euler().z), 2), scale=round(scale.x, 4))


def convert(name):
    spec = MODELS[name]
    parts = spec["parts"]()
    print(f"{name}: {len(parts)} parts")
    matrix = origin_matrix(spec, parts)
    low = build_low(name, parts, spec)
    bake_albedo(name, low, parts, spec.get("size", 1024), spec.get("ray", 0.25), spec.get("tint"))
    low.data.transform(matrix.inverted())
    low.matrix_world = Matrix.Identity(4)
    corners = [Vector(c) for c in low.bound_box]
    top = max(c.z for c in corners) * matrix.to_scale().z
    write_outputs(name, low)
    entry = dict(name=name, **placement(matrix), height=round(top, 3))
    if spec.get("place"):
        entry["place"] = spec["place"]
    return entry


def seats():
    stumps = sorted([o for o in objects() if o.name.startswith(SEATS)], key=lambda o: o.name)
    fire = bpy.data.objects["Site_Fire"].matrix_world.translation
    result = []
    for stump in stumps:
        base = bottom_centre([stump])
        towards = fire - base
        result.append(dict(position=unity(base), yaw=round(math.degrees(math.atan2(towards.x, towards.y)), 2)))
    result.sort(key=lambda s: -math.atan2(s["position"][2] - fire.y, s["position"][0] - fire.x))
    return result


def lights():
    result = []
    for obj in bpy.context.scene.objects:
        if obj.type == "LIGHT" and obj.name in LIGHTS and not obj.hide_render:
            result.append(dict(name=obj.name, group=LIGHTS[obj.name], position=unity(obj.matrix_world.translation),
                               color=[round(c, 3) for c in obj.data.color], energy=round(obj.data.energy, 1)))
    return result


def camera():
    cam = bpy.context.scene.camera
    render = bpy.context.scene.render
    forward = cam.matrix_world.to_3x3() @ Vector((0, 0, -1))
    up = cam.matrix_world.to_3x3() @ Vector((0, 1, 0))
    horizontal = 2 * math.atan(cam.data.sensor_width / 2 / cam.data.lens)
    vertical = 2 * math.atan(math.tan(horizontal / 2) * render.resolution_y / render.resolution_x)
    return dict(position=unity(cam.matrix_world.translation), forward=unity(forward), up=unity(up), fieldOfView=round(math.degrees(vertical), 2))


def flame():
    for name, target in FLAME_IMAGES.items():
        image = bpy.data.images[name]
        image.filepath_raw = os.path.join(TEXTURE_DIR, target)
        image.file_format = "PNG"
        image.save()
        print("Flame:", image.filepath_raw)


def sky():
    scene = bpy.context.scene
    keep = {o for o in scene.objects if o.name.startswith(SKY_PARTS) or o.name == SKY_LIGHT or (o.type == "LIGHT" and o.data.type == "AREA")}
    for obj in scene.objects:
        obj.hide_render = obj not in keep
    far = evaluated_copy(bpy.data.objects[SKY_GROUND], bpy.context.evaluated_depsgraph_get(), lambda c: not near(c))
    far.hide_render = False
    cam = bpy.data.objects.new("SkyCamera", bpy.data.cameras.new("SkyCamera"))
    scene.collection.objects.link(cam)
    cam.location = scene.camera.matrix_world.translation
    cam.rotation_euler = (math.radians(90), 0, 0)
    cam.data.type = "PANO"
    cam.data.panorama_type = "EQUIRECTANGULAR"
    cam.data.clip_end = 20000
    scene.camera = cam
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.cycles.samples = 32
    scene.cycles.use_denoising = True
    scene.render.use_compositing = False
    scene.render.resolution_x, scene.render.resolution_y = SKY_SIZE
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = os.path.join(TEXTURE_DIR, "Sky_Panorama.png")
    bpy.ops.render.render(write_still=True)
    print("Sky:", scene.render.filepath)


def main():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    names = args or list(MODELS) + ["Sky", "Flame"]
    models = [convert(name) for name in names if name in MODELS]
    if args and os.path.exists(LAYOUT):
        with open(LAYOUT, encoding="utf-8") as file:
            converted = {m["name"]: m for m in models}
            models = [converted.pop(m["name"], m) for m in json.load(file)["models"] if m["name"] in MODELS] + list(converted.values())
    fire = bpy.data.objects["Site_Fire"].matrix_world.translation
    layout = dict(camera=camera(), campfire=unity(fire), models=models, seats=seats(), lights=lights())
    with open(LAYOUT, "w", encoding="utf-8") as file:
        json.dump(layout, file, indent=2)
    print("Layout:", LAYOUT)
    if "Flame" in names:
        flame()
    if "Sky" in names:
        sky()


main()
