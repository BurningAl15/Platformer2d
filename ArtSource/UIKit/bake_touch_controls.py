"""Bakes the on-screen touch controls in the Blue16 kit style.

Usage: python bake_touch_controls.py <project root> [preview.png]
Writes Assets/Sprites/UIKit/Blue16/Touch_{Stick,Jump,Pause,Play}{,_Pressed}.png (64x64).
"""
import math, os, re, sys, uuid
from PIL import Image

H = lambda h: tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) + (255,)
OUTLINE, RIM, RIM_SHADE = H('#0a0e30'), H('#f4f6ff'), H('#a8b0d8')
ICON, ICON_SHADOW = H('#f4f6ff'), H('#0a0e30')
BASE = (H('#3b5fd0'), H('#141c58'))      # panel gradient
ACCENT = (H('#4f8cff'), H('#1f3fa8'))    # primary action
SIZE = 64


def lerp(a, b, t):
    return tuple(int(round(x + (y - x) * t)) for x, y in zip(a, b))


def disc(fill, pressed):
    im = Image.new('RGBA', (SIZE, SIZE), (0, 0, 0, 0)); px = im.load()
    c = (SIZE - 1) / 2.0
    for y in range(SIZE):
        for x in range(SIZE):
            d = math.hypot(x - c, y - c)
            if d > 31.5: continue
            if d > 29.5: px[x, y] = OUTLINE
            elif d > 26.5:
                px[x, y] = RIM_SHADE if (y > c + 6) != pressed else RIM   # light rim on top, shaded below
            elif d > 25.5: px[x, y] = OUTLINE
            else:
                t = (y - (c - 25)) / 50.0
                if pressed: t = 1 - t * 0.6
                px[x, y] = lerp(fill[0], fill[1], max(0.0, min(1.0, t)))
    if not pressed:   # glossy arc near the top of the face
        for x in range(20, 44):
            y = 11 + int(((x - 31.5) / 12.0) ** 2 * 3)
            if px[x, y][3]: px[x, y] = lerp(px[x, y], RIM, 0.45)
    return im


def stamp(im, pixels, dy=0):
    px = im.load()
    for x, y in pixels:          # 1-pixel drop shadow, then the icon
        if 0 <= x + 1 < SIZE and 0 <= y + 2 + dy < SIZE: px[x + 1, y + 2 + dy] = ICON_SHADOW
    for x, y in pixels:
        px[x, y + dy] = ICON


def rect(x0, y0, x1, y1):
    return [(x, y) for x in range(x0, x1 + 1) for y in range(y0, y1 + 1)]


def tri(cx, cy, size, direction):
    pts = []
    for i in range(size):
        for j in range(-i, i + 1):
            if direction == 'up': pts.append((cx + j, cy - size + 1 + i))
            if direction == 'down': pts.append((cx + j, cy + size - 1 - i))
            if direction == 'left': pts.append((cx - size + 1 + i, cy + j))
            if direction == 'right': pts.append((cx + size - 1 - i, cy + j))
    return pts


def icon_stick():
    # Chunky D-pad arrows with a centre stud.
    pts = tri(32, 18, 10, 'up') + tri(32, 45, 10, 'down') + tri(18, 32, 10, 'left') + tri(45, 32, 10, 'right')
    return pts + rect(29, 29, 34, 34)


def icon_jump():
    return tri(32, 25, 13, 'up') + rect(27, 25, 36, 44)


def icon_pause():
    return rect(21, 18, 28, 45) + rect(35, 18, 42, 45)


def icon_play():
    # Wide right-pointing triangle, optically centred.
    pts = []
    for i in range(24):
        half = round(16 * (1 - i / 24))
        for j in range(-half, half + 1):
            pts.append((22 + i, 32 + j))
    return pts


def build():
    out = {}
    for name, fill, icon, pressable in (('Stick', BASE, icon_stick, False), ('Jump', ACCENT, icon_jump, True),
                                         ('Pause', BASE, icon_pause, True), ('Play', ACCENT, icon_play, True)):
        im = disc(fill, False); stamp(im, icon()); out['Touch_' + name] = im
        if pressable:
            pr = disc(fill, True); stamp(pr, icon(), dy=1); out['Touch_' + name + '_Pressed'] = pr
    return out


def main(root, preview=None):
    template = open(os.path.join(root, 'Assets/2D Platformer Assets/Graphics/UI/UI_Panel.png.meta')).read()
    folder = os.path.join(root, 'Assets/Sprites/UIKit/Blue16')
    sprites = build()
    for name, im in sprites.items():
        path = os.path.join(folder, name + '.png'); im.save(path)
        if not os.path.exists(path + '.meta'):
            meta = re.sub(r'^guid: \w+', 'guid: ' + uuid.uuid4().hex, template, count=1, flags=re.M)
            meta = re.sub(r'spriteBorder: \{[^}]*\}', 'spriteBorder: {x: 0, y: 0, z: 0, w: 0}', meta)
            open(path + '.meta', 'w').write(meta)
    if preview:
        sc = 4; sheet = Image.new('RGBA', (len(sprites) * (SIZE * sc + 16) + 16, SIZE * sc + 32), (120, 170, 200, 255))
        for i, im in enumerate(sprites.values()):
            big = im.resize((SIZE * sc, SIZE * sc), Image.NEAREST); sheet.paste(big, (16 + i * (SIZE * sc + 16), 16), big)
        sheet.save(preview)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
