"""Bakes the tutorial input prompts that replace the keyboard space bar on touch and gamepad.

Usage: python bake_input_prompts.py <project root> [preview.png]
Writes Assets/Sprites/UIKit/Prompts/Prompt_{Touch,Pad}_{0,1}.png (18x18). Each sprite's pivot puts
its centre on the centre of the 42x7 SpaceBar sprite (pivot bottom-left), so the board layout holds.
"""
import os, re, sys, uuid
from PIL import Image

H = lambda h: tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) + (255,)
SIZE = 18
ARROW = ["....#....", "...###...", "..#####..", ".#######.", "...###...", "...###...", "...###..."]
LETTER_A = [".###.", "#...#", "#...#", "#####", "#...#", "#...#"]


def button(face_top, face_bottom, outline, glyph, glyph_color, pressed):
    im = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0)); px = im.load()
    c = (SIZE - 1) / 2
    for y in range(SIZE):
        for x in range(SIZE):
            d = ((x - c) ** 2 + (y - c) ** 2) ** 0.5
            if d > 8.6: continue
            if d > 7.4: px[x, y] = H(outline); continue
            t = y / SIZE
            if pressed: t = 1 - t * 0.5
            a, b = H(face_top), H(face_bottom)
            px[x, y] = tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(4))
    gw, gh = len(glyph[0]), len(glyph)
    ox, oy = (SIZE - gw) // 2, (SIZE - gh) // 2 + (1 if pressed else 0)
    for j, row in enumerate(glyph):
        for i, ch in enumerate(row):
            if ch == '#':
                if 0 <= oy + j + 1 < SIZE: px[ox + i, oy + j + 1] = H(outline)
    for j, row in enumerate(glyph):
        for i, ch in enumerate(row):
            if ch == '#': px[ox + i, oy + j] = H(glyph_color)
    return im


SPRITES = {
    'Prompt_Touch_0': button('#4f8cff', '#1f3fa8', '#0a0e30', ARROW, '#f4f6ff', False),
    'Prompt_Touch_1': button('#4f8cff', '#1f3fa8', '#0a0e30', ARROW, '#f4f6ff', True),
    'Prompt_Pad_0': button('#4fd36a', '#1f8a3a', '#0a2410', LETTER_A, '#f4fff4', False),
    'Prompt_Pad_1': button('#4fd36a', '#1f8a3a', '#0a2410', LETTER_A, '#f4fff4', True),
}
# SpaceBar is 42x7 with a bottom-left pivot: its centre is (21, 3.5) px from the pivot.
PIVOT = ((SIZE / 2 - 21) / SIZE, (SIZE / 2 - 3.5) / SIZE)


def main(root, preview=None):
    template = open(os.path.join(root, 'Assets/2D Platformer Assets/Graphics/UI/UI_Panel.png.meta')).read()
    folder = os.path.join(root, 'Assets/Sprites/UIKit/Prompts')
    os.makedirs(folder, exist_ok=True)
    if not os.path.exists(folder + '.meta'):
        open(folder + '.meta', 'w').write("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % uuid.uuid4().hex)
    for name, im in SPRITES.items():
        path = os.path.join(folder, name + '.png'); im.save(path)
        if not os.path.exists(path + '.meta'):
            meta = re.sub(r'^guid: \w+', 'guid: ' + uuid.uuid4().hex, template, count=1, flags=re.M)
            meta = re.sub(r'spriteBorder: \{[^}]*\}', 'spriteBorder: {x: 0, y: 0, z: 0, w: 0}', meta)
            meta = re.sub(r'(\n  alignment: )\d+', r'\g<1>9', meta, count=1)
            meta = re.sub(r'spritePivot: \{[^}]*\}', 'spritePivot: {x: %.5f, y: %.5f}' % PIVOT, meta, count=1)
            open(path + '.meta', 'w').write(meta)
    if preview:
        sc = 10; sheet = Image.new('RGBA', (len(SPRITES) * (SIZE * sc + 10) + 10, SIZE * sc + 20), (240, 230, 200, 255))
        for i, im in enumerate(SPRITES.values()):
            big = im.resize((SIZE * sc, SIZE * sc), Image.NEAREST); sheet.paste(big, (10 + i * (SIZE * sc + 10), 10), big)
        sheet.save(preview)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
