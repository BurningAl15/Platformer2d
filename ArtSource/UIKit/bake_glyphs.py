"""Button glyphs for menu prompts: gamepad face buttons and keyboard keycaps.

Gamepad glyphs are 18x18 circles (same style as Prompt_Pad); keycaps are
dark rounded keys with a 3x5 pixel label. Output: Assets/Sprites/UIKit/Glyphs/.
Run: uv run --with pillow python ArtSource/UIKit/bake_glyphs.py
"""
from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parents[2] / "Assets/Sprites/UIKit/Glyphs"
FONT = {  # 3x5 glyphs, rows top to bottom
    "A": ["010", "101", "111", "101", "101"], "B": ["110", "101", "110", "101", "110"],
    "C": ["011", "100", "100", "100", "011"], "E": ["111", "100", "110", "100", "111"],
    "S": ["011", "100", "010", "001", "110"], "T": ["111", "010", "010", "010", "010"],
    "X": ["101", "101", "010", "101", "101"], "Y": ["101", "101", "010", "010", "010"],
}

def text(dr, x, y, s, col, scale=1):
    for ch in s:
        for r, row in enumerate(FONT[ch]):
            for c, bit in enumerate(row):
                if bit == "1":
                    dr.rectangle([x + c * scale, y + r * scale, x + c * scale + scale - 1, y + r * scale + scale - 1], fill=col)
        x += 4 * scale

def face(name, fill, rim, letter):
    im = Image.new("RGBA", (18, 18), (0, 0, 0, 0)); dr = ImageDraw.Draw(im)
    dr.ellipse([0, 1, 17, 17], fill=(20, 22, 40, 255))          # outline / shadow
    dr.ellipse([1, 1, 16, 15], fill=rim)
    dr.ellipse([2, 2, 15, 14], fill=fill)
    text(dr, 7, 6, letter, (255, 255, 255, 255))
    im.save(OUT / f"Pad_{name}.png")

def start():
    im = Image.new("RGBA", (22, 14), (0, 0, 0, 0)); dr = ImageDraw.Draw(im)
    dr.rounded_rectangle([0, 1, 21, 13], 5, fill=(20, 22, 40, 255))
    dr.rounded_rectangle([1, 1, 20, 12], 5, fill=(140, 150, 175, 255))
    dr.rounded_rectangle([2, 2, 19, 11], 4, fill=(96, 104, 130, 255))
    for y in (4, 6, 8): dr.line([(7, y), (14, y)], fill=(255, 255, 255, 255))
    im.save(OUT / "Pad_Start.png")

def keycap(label):
    w = max(14, 4 * len(label) + 7)
    im = Image.new("RGBA", (w, 14), (0, 0, 0, 0)); dr = ImageDraw.Draw(im)
    dr.rounded_rectangle([0, 0, w - 1, 13], 2, fill=(20, 22, 40, 255))
    dr.rounded_rectangle([1, 1, w - 2, 12], 2, fill=(200, 205, 225, 255))
    dr.rounded_rectangle([1, 1, w - 2, 10], 2, fill=(236, 240, 252, 255))
    text(dr, (w - (4 * len(label) - 1)) // 2, 3, label, (30, 34, 60, 255))
    im.save(OUT / f"Key_{label}.png")

OUT.mkdir(parents=True, exist_ok=True)
face("B", (214, 64, 64, 255), (150, 36, 44, 255), "B")
face("X", (64, 120, 220, 255), (36, 76, 160, 255), "X")
face("Y", (236, 190, 48, 255), (176, 128, 24, 255), "Y")
face("A", (72, 176, 96, 255), (40, 120, 60, 255), "A")
start()
for k in ("ESC", "TAB", "C"): keycap(k)
