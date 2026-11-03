import math
import os
import random
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy

from featherkit import PROJECT_ROOT, MeshBuilder, export_static_fbx, material, render_strip, reset_scene

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "Art", "Environment")


def tree(b, mats, x, y, scale, kind):
    trunk_h = 0.9 * scale
    b.between("static", mats["bark"], (x, y, 0), (x, y, trunk_h), 0.12 * scale, 0.08 * scale, seg=7, smooth=False)
    if kind == "pine":
        for i in range(3):
            z = trunk_h * 0.6 + i * 0.55 * scale
            r = (0.75 - i * 0.18) * scale
            b.cone("static", mats["pine"], (x, y, z + 0.4 * scale), r, 0.0, 0.9 * scale, seg=8)
    else:
        for dx, dy, dz, r in ((0, 0, 0.55, 0.62), (0.35, 0.1, 0.3, 0.45), (-0.32, -0.05, 0.35, 0.48), (0.05, 0.2, 0.85, 0.42)):
            b.ico("static", mats["leaf"], (x + dx * scale, y + dy * scale, trunk_h + dz * scale), (r * scale,) * 3, sub=1)


def rock(b, mats, x, y, s, rng):
    b.ico("static", mats["rock"], (x, y, s * 0.35), (s * rng.uniform(0.9, 1.3), s * rng.uniform(0.8, 1.1), s * 0.65),
          rot=(rng.uniform(0, 30), rng.uniform(0, 30), rng.uniform(0, 360)), sub=1)


def bush(b, mats, x, y, s):
    for dx, dz, r in ((0, 0.25, 0.4), (0.3, 0.18, 0.3), (-0.28, 0.2, 0.32)):
        b.ico("static", mats["bush"], (x + dx * s, y, dz * s), (r * s, r * s * 0.9, r * s), sub=1)


def flower(b, mats, x, y, col):
    b.between("static", mats["leaf"], (x, y, 0), (x, y, 0.18), 0.012, 0.01, seg=4, smooth=False)
    b.ico("static", mats[col], (x, y, 0.2), (0.05, 0.05, 0.035), sub=1)


def build_arena():
    reset_scene()
    rng = random.Random(7)
    mats = dict(
        grass=material("Grass", (0.42, 0.68, 0.25), rough=0.9),
        grass_dark=material("GrassDark", (0.3, 0.55, 0.2), rough=0.9),
        dirt=material("Dirt", (0.72, 0.56, 0.36), rough=0.95),
        bark=material("Bark", (0.42, 0.27, 0.15)),
        leaf=material("Leaf", (0.3, 0.6, 0.22)),
        pine=material("Pine", (0.17, 0.42, 0.25)),
        bush=material("Bush", (0.35, 0.62, 0.2)),
        rock=material("Rock", (0.6, 0.6, 0.62), rough=0.9),
        hill=material("Hill", (0.48, 0.72, 0.35), rough=0.9),
        hill_far=material("HillFar", (0.55, 0.72, 0.55), rough=0.9),
        cliff=material("Cliff", (0.62, 0.52, 0.42), rough=0.9),
        red=material("FlowerRed", (0.95, 0.25, 0.25)),
        yellow=material("FlowerYellow", (1.0, 0.85, 0.2)),
        white=material("FlowerWhite", (0.95, 0.95, 0.95)),
        tent=material("Tent", (0.55, 0.3, 0.55)),
        banner=material("Banner", (0.25, 0.5, 0.3)),
    )

    b = MeshBuilder()
    b.cube("static", mats["grass"], (0, 2, -0.25), (34, 20, 0.5))
    b.sphere("static", mats["dirt"], (0, 0, -0.06), (8.5, 2.4, 0.08), seg=32, rings=8)

    for x, y, s in ((-12, 9, 6), (-3, 12, 7), (7, 11, 6.5), (15, 9, 5.5)):
        b.sphere("static", mats["hill_far"], (x, y, -0.5), (s * 1.6, s * 0.5, s * 0.55), seg=24, rings=12)
    for x, y, s in ((-9, 6.5, 3.2), (0, 7.5, 3.6), (9, 6.8, 3.0)):
        b.sphere("static", mats["hill"], (x, y, -0.4), (s * 1.5, s * 0.6, s * 0.5), seg=24, rings=12)

    for x in range(-15, 16, 2):
        y = 4.2 + rng.uniform(-0.6, 0.8)
        tree(b, mats, x + rng.uniform(-0.6, 0.6), y, rng.uniform(0.9, 1.4), rng.choice(("round", "pine")))
    for x in (-13, -9.5, 9.5, 13):
        tree(b, mats, x, 1.0 + rng.uniform(-0.5, 1.0), rng.uniform(1.0, 1.3), "round")

    for x, y, s in ((-8.6, -1.0, 0.5), (-6.9, 2.6, 0.35), (6.5, 2.8, 0.45), (8.8, -0.6, 0.6), (0.3, 3.0, 0.3), (-2.8, 3.2, 0.25)):
        rock(b, mats, x, y, s, rng)
    for x, y, s in ((-10.5, 2.5, 1.1), (-4.5, 3.4, 0.8), (3.8, 3.5, 0.9), (11, 2.4, 1.2)):
        bush(b, mats, x, y, s)

    for _ in range(70):
        x = rng.uniform(-14, 14)
        y = rng.uniform(-3.5, 3.8)
        if abs(x) < 8.5 and abs(y) < 2.2:
            continue
        flower(b, mats, x, y, rng.choice(("red", "yellow", "white")))
    for _ in range(90):
        x = rng.uniform(-14, 14)
        y = rng.uniform(-4, 4)
        if abs(x) < 8 and abs(y) < 2.0:
            continue
        b.cone("static", mats["grass_dark"], (x, y, 0.08), 0.06, 0.0, 0.2, rot=(rng.uniform(-15, 15), rng.uniform(-15, 15), 0), seg=3)

    b.cone("static", mats["tent"], (10.5, 2.2, 0.75), 1.1, 0.0, 1.5, seg=6)
    b.cone("static", mats["tent"], (7.8, 3.4, 0.6), 0.85, 0.0, 1.2, seg=6)
    b.between("static", mats["bark"], (-9.2, 1.8, 0), (-9.2, 1.8, 2.4), 0.05, 0.05, seg=6, smooth=False)
    b.cube("static", mats["banner"], (-8.85, 1.8, 2.05), (0.7, 0.03, 0.55))

    b.build("Arena", None)
    export_static_fbx(os.path.join(OUT_DIR, "Arena.fbx"))
    return b


def build_arrow():
    reset_scene()
    wood = material("Wood", (0.45, 0.28, 0.14))
    steel = material("Steel", (0.72, 0.75, 0.8), rough=0.35, metal=0.8)
    fletch = material("Fletch", (0.9, 0.2, 0.15))
    b = MeshBuilder()
    b.between("static", wood, (0, 0.3, 0), (0, -0.3, 0), 0.012, 0.012, seg=6)
    b.cone("static", steel, (0, -0.34, 0), 0.03, 0.0, 0.09, rot=(90, 0, 0), seg=4)
    for ang in (0, 120, 240):
        b.cube("static", fletch, (0, 0.25, 0), (0.002, 0.1, 0.05), rot=(0, ang, 0))
    b.build("Arrow", None)
    export_static_fbx(os.path.join(OUT_DIR, "Arrow.fbx"))


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    preview_dir = argv[0] if argv else ""
    build_arena()
    if preview_dir:
        render_strip(os.path.join(preview_dir, "Arena.png"), (0, 1.5, 0.8), [1], (0, -15, 3.2), size=900)
    build_arrow()
    print("BUILT environment")


main()
