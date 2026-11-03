import json
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

import animations as A
from featherkit import (PROJECT_ROOT, MeshBuilder, add_action, export_fbx, make_rig, material,
                        render_strip, reset_scene)

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Characters")

BIRD_BONES = [
    ("root", (0, 0, 0), None),
    ("body", (0, 0, 0.08), "root"),
    ("head", (0, 0, 0.72), "body"),
    ("wing.L", (0.36, 0, 0.62), "body"),
    ("wing.R", (-0.36, 0, 0.62), "body"),
    ("offhand", (0.47, -0.12, 0.36), "wing.L"),
    ("weapon", (-0.47, -0.12, 0.36), "wing.R"),
    ("foot.L", (0.14, -0.02, 0.02), "root"),
    ("foot.R", (-0.14, -0.02, 0.02), "root"),
    ("tail", (0, 0.3, 0.35), "body"),
]

LIZARD_BONES = [
    ("root", (0, 0, 0), None),
    ("hips", (0, 0, 0.42), "root"),
    ("torso", (0, 0, 0.5), "hips"),
    ("head", (0, -0.04, 1.02), "torso"),
    ("arm.L", (0.28, -0.02, 0.92), "torso"),
    ("arm.R", (-0.28, -0.02, 0.92), "torso"),
    ("offhand", (0.36, -0.12, 0.52), "arm.L"),
    ("weapon", (-0.36, -0.12, 0.52), "arm.R"),
    ("leg.L", (0.15, 0, 0.42), "hips"),
    ("leg.R", (-0.15, 0, 0.42), "hips"),
    ("tail", (0, 0.18, 0.42), "hips"),
    ("tail2", (0, 0.55, 0.22), "tail"),
]


def common_mats():
    return dict(
        white=material("Eye", (0.97, 0.97, 0.95), rough=0.3),
        black=material("Pupil", (0.03, 0.03, 0.04), rough=0.2),
        dark=material("Brow", (0.12, 0.08, 0.06)),
        steel=material("Steel", (0.72, 0.75, 0.8), rough=0.35, metal=0.8),
        gold=material("Gold", (0.95, 0.72, 0.2), rough=0.35, metal=0.9),
        wood=material("Wood", (0.45, 0.28, 0.14)),
        leather=material("Leather", (0.35, 0.2, 0.1)),
    )


def bird_base(b, m, body, belly, wing, beak, big_eyes=False, head_scale=(0.3, 0.29, 0.29)):
    b.sphere("body", body, (0, 0, 0.42), (0.42, 0.4, 0.42))
    b.sphere("body", belly, (0, -0.21, 0.38), (0.3, 0.24, 0.3))
    b.sphere("head", body, (0, -0.02, 0.95), head_scale)
    if not big_eyes:
        for side in (1, -1):
            b.sphere("head", m["white"], (0.11 * side, -0.25, 1.0), (0.085, 0.06, 0.095))
            b.sphere("head", m["black"], (0.1 * side, -0.305, 0.995), (0.04, 0.02, 0.05))
        b.cone("head", beak, (0, -0.33, 0.92), 0.075, 0.0, 0.16, rot=(90, 0, 0))
    for side in (1, -1):
        bone = "wing.L" if side > 0 else "wing.R"
        b.sphere(bone, wing, (0.43 * side, 0.02, 0.45), (0.09, 0.2, 0.24), rot=(0, -15 * side, 0))
        foot = "foot.L" if side > 0 else "foot.R"
        b.sphere(foot, beak, (0.14 * side, -0.1, 0.035), (0.09, 0.15, 0.04))
    for x, spread in ((0, 0), (0.07, 18), (-0.07, -18)):
        b.sphere("tail", wing, (x, 0.42, 0.33), (0.06, 0.17, 0.035), rot=(30, 0, spread))


def brows(b, mat, z, y=-0.28, tilt=18, bone="head", width=0.12, x=0.12):
    for side in (1, -1):
        b.cube(bone, mat, (x * side, y, z), (width, 0.03, 0.035), rot=(0, -tilt * side, 0))


def knight():
    m = common_mats()
    body = material("RobinBody", (0.62, 0.22, 0.12))
    belly = material("RobinBreast", (0.98, 0.52, 0.18))
    wing = material("RobinWing", (0.42, 0.16, 0.09))
    beak = material("Beak", (0.98, 0.7, 0.15))
    plume = material("Plume", (0.85, 0.1, 0.12))
    shield = material("ShieldBlue", (0.15, 0.3, 0.75))

    b = MeshBuilder()
    bird_base(b, m, body, belly, wing, beak)
    b.sphere("head", m["steel"], (0, -0.02, 0.97), (0.315, 0.305, 0.31), cut_below=1.06)
    b.cube("head", m["steel"], (0, -0.315, 1.04), (0.03, 0.025, 0.1))
    b.sphere("head", plume, (0, 0.06, 1.3), (0.05, 0.2, 0.1), rot=(20, 0, 0))
    brows(b, m["dark"], 1.075, y=-0.29)

    b.cube("weapon", m["steel"], (-0.47, -0.12, 0.73), (0.07, 0.02, 0.54))
    b.cone("weapon", m["steel"], (-0.47, -0.12, 1.03), 0.05, 0.0, 0.08, seg=4, rot=(0, 0, 45))
    b.cube("weapon", m["gold"], (-0.47, -0.12, 0.45), (0.2, 0.05, 0.04))
    b.cone("weapon", m["leather"], (-0.47, -0.12, 0.38), 0.025, 0.025, 0.12, seg=8, smooth=True)
    b.sphere("weapon", m["gold"], (-0.47, -0.12, 0.31), (0.035, 0.035, 0.035), seg=8, rings=6)

    b.cone("offhand", shield, (0.5, -0.2, 0.38), 0.2, 0.2, 0.04, seg=20, rot=(90, 0, -15), smooth=False)
    b.cone("offhand", m["gold"], (0.5, -0.205, 0.38), 0.215, 0.215, 0.03, seg=20, rot=(90, 0, -15))
    b.sphere("offhand", m["gold"], (0.505, -0.235, 0.38), (0.06, 0.03, 0.06), seg=10, rings=6)
    return b, BIRD_BONES, A.BIRD, A.melee


def archer():
    m = common_mats()
    body = material("JayBody", (0.2, 0.45, 0.88))
    belly = material("JayBelly", (0.88, 0.92, 0.97))
    wing = material("JayWing", (0.12, 0.28, 0.65))
    beak = material("BeakGrey", (0.3, 0.3, 0.35))
    band = material("Bandana", (0.25, 0.6, 0.25))

    b = MeshBuilder()
    bird_base(b, m, body, belly, wing, beak)
    for y, tilt in ((0.02, -20), (0.08, -38), (0.14, -55)):
        b.cone("head", body, (0, y, 1.22), 0.05, 0.0, 0.22, rot=(tilt, 0, 0), smooth=True)
    b.sphere("head", band, (0, -0.02, 1.12), (0.31, 0.3, 0.035))
    for side in (1, -1):
        b.sphere("head", band, (0.05 * side, 0.32, 1.08), (0.03, 0.12, 0.025), rot=(-20, 0, 20 * side))
    b.sphere("body", m["black"], (0, -0.02, 0.73), (0.3, 0.29, 0.035))
    brows(b, m["dark"], 1.085, y=-0.29)

    b.between("body", m["leather"], (0.1, 0.3, 0.38), (0.22, 0.42, 0.82), 0.07, 0.07)
    for dx in (-0.03, 0.0, 0.03):
        b.cone("body", m["white"], (0.23 + dx, 0.43, 0.88), 0.02, 0.0, 0.08, seg=4)

    x0, y0, z0, radius = 0.47, -0.12, 0.36, 0.38
    points = []
    for i in range(9):
        a = math.radians(-70 + 140 * i / 8)
        points.append((x0, y0 + radius * (1 - math.cos(a)) - 0.12, z0 + radius * math.sin(a)))
    for p1, p2 in zip(points, points[1:]):
        b.between("offhand", m["wood"], p1, p2, 0.022, 0.022, seg=6)
    b.between("offhand", m["white"], points[0], points[-1], 0.006, 0.006, seg=4)
    return b, BIRD_BONES, A.BIRD, A.shoot


def owl():
    m = common_mats()
    body = material("OwlBody", (0.5, 0.34, 0.2))
    belly = material("OwlBelly", (0.88, 0.78, 0.6))
    wing = material("OwlWing", (0.38, 0.25, 0.14))
    beak = material("BeakDark", (0.85, 0.6, 0.2))
    iris = material("OwlIris", (1.0, 0.8, 0.1), rough=0.3)
    crystal = material("Crystal", (0.2, 1.0, 0.55), emit=3.0, rough=0.1)
    scarf = material("Scarf", (0.75, 0.2, 0.45))

    b = MeshBuilder()
    bird_base(b, m, body, belly, wing, beak, big_eyes=True, head_scale=(0.33, 0.31, 0.3))
    b.sphere("head", belly, (0, -0.15, 0.97), (0.27, 0.17, 0.24))
    for side in (1, -1):
        b.sphere("head", m["white"], (0.115 * side, -0.28, 1.0), (0.1, 0.05, 0.11))
        b.sphere("head", iris, (0.115 * side, -0.315, 1.0), (0.065, 0.02, 0.07))
        b.sphere("head", m["black"], (0.112 * side, -0.33, 1.0), (0.035, 0.015, 0.04))
        b.cone("head", body, (0.19 * side, 0.0, 1.24), 0.07, 0.0, 0.2, rot=(0, 25 * side, 0))
    b.cone("head", beak, (0, -0.33, 0.88), 0.05, 0.0, 0.12, rot=(115, 0, 0))
    brows(b, m["dark"], 1.13, y=-0.3, tilt=-10, width=0.13)
    b.sphere("body", scarf, (0, -0.02, 0.73), (0.32, 0.31, 0.05))
    b.sphere("body", scarf, (0.12, -0.3, 0.6), (0.05, 0.03, 0.14), rot=(0, 10, 0))

    b.between("weapon", m["wood"], (-0.47, -0.12, 0.05), (-0.47, -0.12, 0.98), 0.025, 0.02, seg=8)
    b.ico("weapon", crystal, (-0.47, -0.12, 1.07), (0.07, 0.07, 0.12))
    for side in (1, -1):
        b.sphere("weapon", m["wood"], (-0.47 + 0.05 * side, -0.12, 0.97), (0.02, 0.02, 0.07), rot=(0, -30 * side, 0), seg=6, rings=4)
    return b, BIRD_BONES, A.BIRD, A.cast


def lizard_base(b, m, skin, belly, spikes, eye, torso_scale=(0.29, 0.25, 0.36)):
    b.sphere("torso", skin, (0, 0, 0.78), torso_scale)
    b.sphere("torso", belly, (0, -torso_scale[1] * 0.62, 0.74), (0.21, 0.15, 0.29))
    b.sphere("hips", skin, (0, 0.02, 0.48), (0.25, 0.22, 0.16))
    b.sphere("head", skin, (0, -0.12, 1.2), (0.2, 0.24, 0.18))
    b.sphere("head", skin, (0, -0.32, 1.15), (0.13, 0.15, 0.09))
    b.cube("head", m["dark"], (0, -0.3, 1.105), (0.24, 0.26, 0.012))
    for side in (1, -1):
        b.sphere("head", m["black"], (0.04 * side, -0.46, 1.18), (0.015, 0.012, 0.012), seg=6, rings=4)
        b.sphere("head", eye, (0.11 * side, -0.2, 1.33), (0.075, 0.07, 0.075))
        b.sphere("head", m["black"], (0.115 * side, -0.265, 1.335), (0.015, 0.02, 0.05), seg=8, rings=6)
        arm = "arm.L" if side > 0 else "arm.R"
        b.sphere(arm, skin, (0.32 * side, -0.04, 0.76), (0.075, 0.075, 0.17), rot=(0, 10 * side, 0))
        b.sphere(arm, skin, (0.36 * side, -0.1, 0.54), (0.075, 0.075, 0.075), seg=10, rings=8)
        leg = "leg.L" if side > 0 else "leg.R"
        b.sphere(leg, skin, (0.17 * side, -0.02, 0.3), (0.11, 0.12, 0.16))
        b.sphere(leg, skin, (0.18 * side, -0.05, 0.15), (0.075, 0.085, 0.12))
        b.sphere(leg, skin, (0.18 * side, -0.1, 0.05), (0.1, 0.17, 0.05))
    brows(b, m["dark"], 1.385, y=-0.255, tilt=22, x=0.11)
    b.cone("head", spikes, (0, 0.1, 1.33), 0.05, 0.0, 0.14, rot=(-30, 0, 0))
    for y, z in ((0.22, 1.0), (0.26, 0.82), (0.25, 0.64)):
        b.cone("torso", spikes, (0, y, z), 0.05, 0.0, 0.14, rot=(-60, 0, 0))
    b.between("tail", skin, (0, 0.15, 0.45), (0, 0.55, 0.25), 0.15, 0.07)
    b.between("tail2", skin, (0, 0.55, 0.25), (0, 0.88, 0.1), 0.07, 0.0)


def grunt():
    m = common_mats()
    skin = material("GruntSkin", (0.36, 0.62, 0.25))
    belly = material("GruntBelly", (0.85, 0.82, 0.5))
    spikes = material("GruntSpikes", (0.88, 0.45, 0.15))
    eye = material("LizardEye", (1.0, 0.85, 0.2), rough=0.3)

    b = MeshBuilder()
    lizard_base(b, m, skin, belly, spikes, eye)
    b.sphere("hips", m["leather"], (0, -0.01, 0.55), (0.27, 0.24, 0.04))
    b.cube("hips", m["gold"], (0, -0.25, 0.55), (0.08, 0.02, 0.06))
    b.between("weapon", m["wood"], (-0.36, -0.12, 0.42), (-0.36, -0.12, 1.08), 0.035, 0.09, seg=8)
    for z, ang in ((0.92, 0), (1.0, 120), (0.85, 240)):
        x = -0.36 + 0.09 * math.cos(math.radians(ang))
        y = -0.12 + 0.09 * math.sin(math.radians(ang))
        b.cone("weapon", m["steel"], (x, y, z), 0.02, 0.0, 0.07, rot=(0, 90, ang), seg=4)
    return b, LIZARD_BONES, A.LIZARD, A.melee


def shaman():
    m = common_mats()
    skin = material("ShamanSkin", (0.22, 0.55, 0.55))
    belly = material("ShamanBelly", (0.8, 0.85, 0.7))
    spikes = material("ShamanSpikes", (0.55, 0.25, 0.7))
    eye = material("ShamanEye", (1.0, 0.45, 0.2), rough=0.3)
    cloth = material("ShamanCloth", (0.55, 0.25, 0.6))
    feather = material("Feather", (0.9, 0.2, 0.15))
    orb = material("Orb", (0.75, 0.3, 1.0), emit=3.0, rough=0.1)

    b = MeshBuilder()
    lizard_base(b, m, skin, belly, spikes, eye)
    b.cone("hips", cloth, (0, 0.0, 0.38), 0.32, 0.24, 0.28, seg=14)
    for i, ang in enumerate((-30, 0, 30)):
        col = feather if i != 1 else m["gold"]
        b.cone("head", col, (0.0 + 0.05 * math.sin(math.radians(ang)), 0.12, 1.42), 0.04, 0.0, 0.3, rot=(-25, ang, 0), smooth=True)
    for i in range(7):
        a = math.radians(200 + i * 20)
        b.sphere("torso", m["white"], (0.2 * math.cos(a), 0.2 * math.sin(a) - 0.02, 1.0), (0.03, 0.03, 0.03), seg=6, rings=4)
    b.between("weapon", m["wood"], (-0.36, -0.12, 0.08), (-0.36, -0.12, 1.2), 0.025, 0.02, seg=8)
    b.ico("weapon", orb, (-0.36, -0.12, 1.28), (0.08, 0.08, 0.08))
    return b, LIZARD_BONES, A.LIZARD, A.cast


def brute():
    m = common_mats()
    skin = material("BruteSkin", (0.75, 0.35, 0.18))
    belly = material("BruteBelly", (0.95, 0.75, 0.5))
    spikes = material("BruteSpikes", (0.25, 0.18, 0.15))
    eye = material("BruteEye", (1.0, 0.9, 0.3), rough=0.3)
    stone = material("Stone", (0.5, 0.5, 0.52), rough=0.9)

    b = MeshBuilder()
    lizard_base(b, m, skin, belly, spikes, eye, torso_scale=(0.34, 0.29, 0.38))
    for side in (1, -1):
        b.cone("head", m["white"], (0.15 * side, -0.05, 1.36), 0.05, 0.0, 0.18, rot=(0, 35 * side, 0), smooth=True)
        arm = "arm.L" if side > 0 else "arm.R"
        b.sphere(arm, m["steel"], (0.33 * side, -0.02, 0.92), (0.12, 0.12, 0.08), cut_below=0.9)
    b.between("weapon", m["wood"], (-0.36, -0.12, 0.38), (-0.36, -0.12, 1.15), 0.03, 0.03, seg=8)
    b.ico("weapon", stone, (-0.36, -0.12, 1.2), (0.17, 0.14, 0.13))
    return b, LIZARD_BONES, A.LIZARD, A.melee


CHARACTERS = {
    "Knight": knight,
    "Archer": archer,
    "Owl": owl,
    "Grunt": grunt,
    "Shaman": shaman,
    "Brute": brute,
}


def build(name, preview_dir):
    reset_scene()
    builder, bones, profile, attack = CHARACTERS[name]()
    rig = make_rig(bones)
    builder.build(name, rig)

    meta = {"clips": []}
    clips = A.build_set(profile, attack)
    for clip, (keys, events, loop) in clips.items():
        add_action(rig, clip, keys)
        meta["clips"].append({
            "name": clip,
            "start": keys[0][0],
            "end": keys[-1][0],
            "loop": loop,
            "events": [{"name": n, "frame": f} for n, f in events.items()],
        })
    rig.animation_data.action = bpy.data.actions["Idle"]

    os.makedirs(OUT_DIR, exist_ok=True)
    export_fbx(os.path.join(OUT_DIR, f"{name}.fbx"))
    with open(os.path.join(OUT_DIR, f"{name}.clips.json"), "w") as f:
        json.dump(meta, f, indent=2)

    if preview_dir:
        os.makedirs(preview_dir, exist_ok=True)
        for clip in ("Idle", "Attack", "Cast"):
            rig.animation_data.action = bpy.data.actions[clip]
            keys = [k[0] for k in clips[clip][0]][:5]
            render_strip(os.path.join(preview_dir, f"{name}_{clip}.png"), (0, 0, 0.65), keys, (1.9, -3.2, 1.3), size=300)
        rig.animation_data.action = bpy.data.actions["Idle"]
        blend_dir = os.path.join(PROJECT_ROOT, "Art", "Blender", "Characters")
        os.makedirs(blend_dir, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(blend_dir, f"{name}.blend"))


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    preview_dir = argv[0] if argv else ""
    names = argv[1:] or list(CHARACTERS)
    for name in names:
        build(name, preview_dir)
        print(f"BUILT {name}")


main()
