"""Bakes the 9-slice UI kit sprites for every theme into Assets/Sprites/UIKit/<Theme>.

Usage: python bake_ui_kit.py <project root> [preview.png]
Pieces (all point-filtered, 16 px per unit):
  Panel 32x32 (border 8), Header 32x16 (border 6), Slot 24x24 (border 5),
  Button_{Normal,Focused,Pressed,Disabled} 32x20 (border 6), Cursor 8x9.
"""
import os, re, sys, uuid
from PIL import Image, ImageDraw

H = lambda h: tuple(int(h[i:i + 2], 16) for i in (1, 3, 5)) + (255,)
CLEAR = (0, 0, 0, 0)

THEMES = {
    'Blue16': dict(outline='#0a0e30', border='#f4f6ff', border_dark='#a8b0d8', fill_top='#3b5fd0', fill_bottom='#141c58',
                   btn='#1e2c78', btn_lite='#4a60c0', btn_shade='#101848',
                   focus='#f4f6ff', focus_lite='#ffffff', focus_shade='#a8b0d8',
                   head='#1e2c78', head_lite='#4a60c0', head_shade='#0a0e30', slot='#16205c', cursor='#f2c230', corner=None),
    'Wood': dict(outline='#2b1a1e', border='#8a5a2e', border_dark='#5a3520', fill_top='#b07840', fill_bottom='#9a6634',
                 btn='#3f8f4f', btn_lite='#6fc070', btn_shade='#28603a',
                 focus='#f2c230', focus_lite='#fff0a0', focus_shade='#b07818',
                 head='#5a3520', head_lite='#8a5a2e', head_shade='#2b1a1e', slot='#6b4226', cursor='#ffffff', corner='#f2c230'),
    'Parchment': dict(outline='#3a2618', border='#b8854a', border_dark='#7a5028', fill_top='#f4e4b8', fill_bottom='#ead4a0',
                      btn='#c8a050', btn_lite='#f0d488', btn_shade='#8a6428',
                      focus='#2aa7a0', focus_lite='#7fe0d0', focus_shade='#1a6a70',
                      head='#c8353c', head_lite='#ee6a5c', head_shade='#8f2630', slot='#e3cc98', cursor='#c8353c', corner=None),
    'Stone': dict(outline='#1c1f30', border='#7a8298', border_dark='#4a5068', fill_top='#626a82', fill_bottom='#525a72',
                  btn='#3a4058', btn_lite='#6a7290', btn_shade='#22263a',
                  focus='#3fd8c8', focus_lite='#b8fff0', focus_shade='#1c8a88',
                  head='#3a4058', head_lite='#6a7290', head_shade='#1c1f30', slot='#3a4058', cursor='#3fd8c8', corner='#3fd8c8'),
}


def lerp(a, b, t):
    return tuple(int(round(x + (y - x) * t)) for x, y in zip(a, b))


def clip_corners(im):
    w, h = im.size
    for x, y in ((0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)):
        im.putpixel((x, y), CLEAR)


def panel(t):
    im = Image.new('RGBA', (32, 32), CLEAR); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 31, 31], fill=H(t['outline']))
    d.rectangle([1, 1, 30, 30], fill=H(t['border']))
    d.line([1, 30, 30, 30], fill=H(t['border_dark'])); d.line([30, 1, 30, 30], fill=H(t['border_dark']))
    d.rectangle([3, 3, 28, 28], fill=H(t['outline']))
    for y in range(4, 28):
        d.line([4, y, 27, y], fill=lerp(H(t['fill_top']), H(t['fill_bottom']), (y - 4) / 23))
    if t['corner']:
        for cx, cy in ((5, 5), (26, 5), (5, 26), (26, 26)):
            d.rectangle([cx - 1, cy - 1, cx + 1, cy + 1], fill=H(t['outline'])); d.point((cx, cy), fill=H(t['corner']))
    clip_corners(im)
    return im


def plate(size, face, lite, shade, outline, shade_rows=2, pressed=False):
    w, h = size
    im = Image.new('RGBA', size, CLEAR); d = ImageDraw.Draw(im)
    top = 1 if pressed else 0
    d.rectangle([0, top, w - 1, h - 1], fill=H(outline))
    d.rectangle([1, top + 1, w - 2, h - 2], fill=H(face))
    if not pressed:
        d.line([1, 1, w - 2, 1], fill=H(lite))
        if shade_rows > 0:
            d.rectangle([1, h - 1 - shade_rows, w - 2, h - 2], fill=H(shade))
    else:
        d.line([1, top + 1, w - 2, top + 1], fill=H(shade))
    for x, y in ((0, top), (w - 1, top), (0, h - 1), (w - 1, h - 1)):
        im.putpixel((x, y), CLEAR)
    return im


def desaturate(im):
    out = im.copy(); px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a:
                v = int(r * 0.3 + g * 0.5 + b * 0.2)
                px[x, y] = (int(v * 0.55 + 40), int(v * 0.55 + 42), int(v * 0.55 + 52), a)
    return out


def cursor(t):
    im = Image.new('RGBA', (8, 9), CLEAR); d = ImageDraw.Draw(im)
    d.polygon([(0, 0), (6, 4), (0, 8)], fill=H(t['outline']))
    d.polygon([(1, 2), (4, 4), (1, 6)], fill=H(t['cursor']))
    return im


def pieces(t):
    normal = plate((32, 20), t['btn'], t['btn_lite'], t['btn_shade'], t['outline'], 3)
    return {
        'Panel': (panel(t), 8),
        'Header': (plate((32, 16), t['head'], t['head_lite'], t['head_shade'], t['outline'], 2), 6),
        'Slot': (plate((24, 24), t['slot'], t['outline'], t['slot'], t['outline'], 0), 5),
        'Button_Normal': (normal, 6),
        'Button_Focused': (plate((32, 20), t['focus'], t['focus_lite'], t['focus_shade'], t['outline'], 3), 6),
        'Button_Pressed': (plate((32, 20), t['focus_shade'], t['focus_lite'], t['focus_shade'], t['outline'], 3, pressed=True), 6),
        'Button_Disabled': (desaturate(normal), 6),
        'Cursor': (cursor(t), 0),
    }


FOLDER_META = ("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
               "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


def ensure_folder(path):
    os.makedirs(path, exist_ok=True)
    if not os.path.exists(path + '.meta'):
        open(path + '.meta', 'w').write(FOLDER_META % uuid.uuid4().hex)


def main(root, preview=None):
    template = open(os.path.join(root, 'Assets/2D Platformer Assets/Graphics/UI/UI_Panel.png.meta')).read()
    base = os.path.join(root, 'Assets/Sprites/UIKit'); ensure_folder(base)
    rows = []
    for theme, t in THEMES.items():
        folder = os.path.join(base, theme); ensure_folder(folder)
        made = pieces(t); rows.append((theme, made))
        for name, (im, border) in made.items():
            path = os.path.join(folder, name + '.png'); im.save(path)
            if not os.path.exists(path + '.meta'):      # keep GUIDs stable across rebakes
                meta = re.sub(r'^guid: \w+', 'guid: ' + uuid.uuid4().hex, template, count=1, flags=re.M)
                meta = re.sub(r'spriteBorder: \{[^}]*\}', 'spriteBorder: {x: %d, y: %d, z: %d, w: %d}' % ((border,) * 4), meta)
                open(path + '.meta', 'w').write(meta)
    if preview:
        sc = 6; sheet = Image.new('RGBA', (220 * sc, len(rows) * 40 * sc), (60, 70, 90, 255))
        for r, (theme, made) in enumerate(rows):
            x = 4
            for name, (im, _) in made.items():
                big = im.resize((im.width * sc, im.height * sc), Image.NEAREST)
                sheet.paste(big, (x * sc, (r * 40 + 4) * sc), big); x += im.width + 4
        sheet.save(preview)


if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
