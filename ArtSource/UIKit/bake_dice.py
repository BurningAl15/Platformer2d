"""Bakes the dice faces: Wood d4 (0-3), Blue d6 (1-4), Gold d4 (3-6), Secret d6 (4-6).

d4 dice are triangles and d6 dice are squares so the die type reads at a glance.
Usage: python bake_dice.py <project root> [preview.png]
Writes Assets/Sprites/Dice/<Set>_<value>.png (64x64).
"""
import os, re, sys, uuid
from PIL import Image

H = lambda h: tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) + (255,)
SIZE = 64
SETS = {   # name: (shape, values, top, bottom, outline, highlight, pip)
    'Wood':   ('tri', range(0, 4), '#dba468', '#a8703a', '#3a2216', '#f2cf98', '#3a2216'),
    'Blue':   ('sq',  range(1, 5), '#5a96ff', '#2450b0', '#0a0e30', '#b4d0ff', '#f4f6ff'),
    'Gold':   ('tri', range(3, 7), '#ffdc5a', '#c88a1a', '#4a2a08', '#fff6c0', '#8f2630'),
    'Secret': ('sq',  range(4, 7), '#9a6be8', '#4a2a8a', '#1a0e30', '#d4b8ff', '#f2c230'),
}
SQ_PIPS = {1: [(.5, .5)], 2: [(.3, .3), (.7, .7)], 3: [(.3, .3), (.5, .5), (.7, .7)],
           4: [(.3, .3), (.7, .3), (.3, .7), (.7, .7)], 5: [(.3, .3), (.7, .3), (.5, .5), (.3, .7), (.7, .7)],
           6: [(.3, .27), (.7, .27), (.3, .5), (.7, .5), (.3, .73), (.7, .73)]}
TRI_PIPS = {1: [(.5, .68)], 2: [(.4, .74), (.6, .74)], 3: [(.5, .52), (.38, .76), (.62, .76)],
            4: [(.5, .46), (.42, .63), (.58, .63), (.5, .8)], 5: [(.5, .44), (.41, .62), (.59, .62), (.33, .8), (.67, .8)],
            6: [(.5, .42), (.42, .6), (.58, .6), (.32, .79), (.5, .79), (.68, .79)]}


def lerp(a, b, t):
    return tuple(int(round(x + (y - x) * t)) for x, y in zip(a, b))


def inside(shape, x, y, inset=0):
    if shape == 'sq':
        return 4 + inset <= x <= 59 - inset and 4 + inset <= y <= 59 - inset
    top, bottom = 4 + inset * 2, 58 - inset          # apex at top, base at bottom
    if not top <= y <= bottom: return False
    half = (y - 4) / (58 - 4) * 30 - inset
    return abs(x - 31.5) <= half


def face(name, value):
    shape, _, top, bottom, outline, highlight, pip = SETS[name]
    im = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0)); px = im.load()
    for y in range(SIZE):
        for x in range(SIZE):
            if not inside(shape, x, y): continue
            if not inside(shape, x, y, 2): px[x, y] = H(outline); continue
            t = (y - 6) / 52
            px[x, y] = lerp(H(top), H(bottom), max(0, min(1, t)))
            if not inside(shape, x, y - 2, 2) or (shape == 'sq' and not inside(shape, x - 2, y, 2)):
                px[x, y] = H(highlight)                      # lit top/left edge
    if shape == 'sq':                                         # rounded corners
        for cx, cy in ((4, 4), (59, 4), (4, 59), (59, 59)):
            px[cx, cy] = (0, 0, 0, 0)
    if value == 0:                                            # zero reads as a small cross
        for d in range(-4, 5):
            for w in (0, 1):
                px[31 + d + w, 44 + d] = H(pip); px[31 + d + w, 44 - d] = H(pip)
        return im
    layout = (SQ_PIPS if shape == 'sq' else TRI_PIPS)[value]
    for fx, fy in layout:
        cx, cy = int(round(fx * 63)), int(round(fy * 63))
        for dy in range(-3, 3):
            for dx in range(-3, 3):
                if abs(dx + .5) + abs(dy + .5) > 4.5: continue  # chunky round pip
                px[cx + dx, cy + dy] = H(pip)
        px[cx - 2, cy - 2] = lerp(H(pip), (255, 255, 255, 255), .5)
    return im


def main(root, preview=None):
    template = open(os.path.join(root, 'Assets/2D Platformer Assets/Graphics/UI/UI_Panel.png.meta')).read()
    folder = os.path.join(root, 'Assets/Sprites/Dice')
    os.makedirs(folder, exist_ok=True)
    if not os.path.exists(folder + '.meta'):
        open(folder + '.meta', 'w').write("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % uuid.uuid4().hex)
    made = []
    for name, (_, values, *rest) in SETS.items():
        for v in values:
            im = face(name, v); path = os.path.join(folder, f'{name}_{v}.png'); im.save(path); made.append(im)
            if not os.path.exists(path + '.meta'):
                meta = re.sub(r'^guid: \w+', 'guid: ' + uuid.uuid4().hex, template, count=1, flags=re.M)
                meta = re.sub(r'spriteBorder: \{[^}]*\}', 'spriteBorder: {x: 0, y: 0, z: 0, w: 0}', meta)
                open(path + '.meta', 'w').write(meta)
    if preview:
        sc = 3; cols = 15; sheet = Image.new('RGBA', (cols * (SIZE * sc + 8) + 8, SIZE * sc + 16), (40, 50, 110, 255))
        for i, im in enumerate(made):
            big = im.resize((SIZE * sc, SIZE * sc), Image.NEAREST); sheet.paste(big, (8 + i * (SIZE * sc + 8), 8), big)
        sheet.save(preview)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
