"""Cut the views out of a character concept sheet and place them as reference images in a new .blend.

Run from the repository root:
    blender -b --factory-startup --python .claude/skills/featherguard-character/scripts/make_refs.py -- \
        --image Art/Concepts/<Name>.png --blend Art/Blender/Characters/<Name>.blend --name <Name> --length 1.6

The sheet must have three views in its top row (side facing left, front, back) standing on one thin dark
ground line, and may have one extra pose below on its own ground line. Pass --length (side view end to end)
or --height (top of the side view above the ground) in metres to set the scale.
"""
import argparse
import math
import os
import sys

import bpy
import numpy as np


def parse_args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--image", required=True)
    parser.add_argument("--blend", required=True)
    parser.add_argument("--name", required=True)
    scale = parser.add_mutually_exclusive_group(required=True)
    scale.add_argument("--length", type=float, help="side view from end to end, in metres")
    scale.add_argument("--height", type=float, help="top of the side view above the ground, in metres")
    return parser.parse_args(argv)


def main():
    args = parse_args()
    image_path = os.path.abspath(args.image)
    blend_path = os.path.abspath(args.blend)
    if os.path.exists(blend_path):
        raise SystemExit(f"{blend_path} already exists; refusing to overwrite it")

    src = bpy.data.images.load(image_path)
    width, height = src.size
    px = np.array(src.pixels[:], dtype=np.float32).reshape(height, width, 4)[::-1]
    rgb = px[:, :, :3]
    corners = np.concatenate([rgb[:20, :20].reshape(-1, 3), rgb[:20, -20:].reshape(-1, 3)])
    background = np.median(corners, axis=0)
    mask = np.abs(rgb - background).max(axis=2) > 0.07

    dark = rgb.max(axis=2) < background.max() * 0.6

    def longest_run(row):
        best = run = 0
        for value in row:
            run = run + 1 if value else 0
            best = max(best, run)
        return best

    def is_ground_line(r):
        # a thin, long, dark line: much darker than the rows a few pixels above and below it
        count = dark[r].sum()
        if count < width * 0.2 or longest_run(dark[r]) < width * 0.15:
            return False
        above = dark[max(0, r - 6)].sum()
        below = dark[min(height - 1, r + 6)].sum()
        return above < count * 0.5 and below < count * 0.5

    lines = [r for r in range(height // 4, height) if is_ground_line(r)]
    if not lines:
        raise SystemExit("no ground line found under the top row of views")
    ground = lines[0]
    later = [r for r in lines if r > ground + 20]
    ground2 = later[0] if later else None
    print("ground rows", ground, ground2)

    def bounds(r0, r1, c0, c1):
        m = mask[r0:r1, c0:c1]
        rows = np.where(m.any(axis=1))[0]
        cols = np.where(m.any(axis=0))[0]
        return r0 + rows.min(), r0 + rows.max(), c0 + cols.min(), c0 + cols.max()

    def contact(r_ground, c0, c1):
        band = mask[r_ground - 14:r_ground - 3, c0:c1]
        cols = np.where(band.any(axis=0))[0]
        return c0 + cols.min(), c0 + cols.max()

    used = mask[:ground - 3].any(axis=0)
    segments, start = [], None
    for c in range(width):
        if used[c] and start is None:
            start = c
        if not used[c] and start is not None:
            if c - start > 40:
                segments.append((start, c))
            start = None
    if start is not None:
        segments.append((start, width))
    if len(segments) != 3:
        raise SystemExit(f"expected three views in the top row, found {len(segments)}: {segments}")

    views = {}
    for view, (c0, c1) in zip(["Side", "Front", "Back"], segments):
        top, _, left, right = bounds(0, ground - 3, c0, c1)
        if view == "Side":
            p0, p1 = contact(ground, c0, c1)
            middle = (p0 + p1) / 2
        else:
            middle = (left + right) / 2
        views[view] = dict(top=top, left=left, right=right, middle=middle)

    side = views["Side"]
    if args.length:
        ppm = (side["right"] - side["left"]) / args.length
    else:
        ppm = (ground - side["top"]) / args.height
    print("pixels per metre", round(ppm, 1))
    for view, v in views.items():
        print(view, "top", round((ground - v["top"]) / ppm, 3), "m, width", round((v["right"] - v["left"]) / ppm, 3), "m")

    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    collection = bpy.data.collections.new(f"{args.name}_Refs")
    bpy.context.scene.collection.children.link(collection)
    pad = 8

    def crop(view, top, bottom, left, right):
        top, left, right = max(0, top - pad), max(0, left - pad), min(width, right + pad)
        region = px[top:bottom, left:right][::-1].copy()
        h, w = region.shape[:2]
        img = bpy.data.images.new(f"{args.name}_Ref_{view}", w, h, alpha=True)
        img.pixels.foreach_set(region.ravel())
        img.update()
        img.pack()
        return img, left, right, w, h

    def place(view, img, w, h, rotation, location):
        empty = bpy.data.objects.new(f"{args.name}_Ref_{view}", None)
        empty.empty_display_type = 'IMAGE'
        empty.data = img
        empty.empty_display_size = max(w, h) / ppm
        empty.empty_image_offset = (-0.5, 0.0)
        empty.empty_image_side = 'FRONT'
        empty.show_empty_image_perspective = False
        empty.use_empty_image_alpha = True
        empty.color[3] = 0.6
        empty.rotation_euler = [math.radians(a) for a in rotation]
        empty.location = location
        collection.objects.link(empty)
        return empty

    rotations = {"Side": (90, 0, 90), "Front": (90, 0, 0), "Back": (90, 0, 180)}
    for view, v in views.items():
        img, left, right, w, h = crop(view, v["top"], ground + 1, v["left"], v["right"])
        shift = (v["middle"] - (left + right) / 2) / ppm
        location = {"Side": (0.0, -shift, 0.0), "Front": (-shift, 0.0, 0.0), "Back": (shift, 0.0, 0.0)}[view]
        place(view, img, w, h, rotations[view], location)

    if ground2 is not None:
        top, _, left, right = bounds(ground + 6, ground2 - 3, 0, width)
        img, left, right, w, h = crop("Pose", top, ground2 + 1, left, right)
        offset = (side["right"] - side["left"]) / ppm + 0.6
        place("Pose", img, w, h, rotations["Side"], (0.0, offset, 0.0))
        print("extra pose placed at Y", round(offset, 2))

    bpy.data.images.remove(src)
    bpy.context.scene.unit_settings.system = 'METRIC'
    os.makedirs(os.path.dirname(blend_path), exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=blend_path, compress=True)
    print("saved", blend_path)


main()
