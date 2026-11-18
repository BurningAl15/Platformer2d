"""Intro illustration: the fox, seen from behind on a mossy rock, looks over a
jungle valley at an ancient overgrown temple, with the golden-hour sea behind.
256x144, SNES-style limited palette. Writes one PNG per parallax layer.
Run: uv run --with pillow python ArtSource/Story/intro_valley.py OUT_DIR [--preview]
"""
import math, random, sys
from pathlib import Path
from PIL import Image, ImageDraw

W, H = 256, 144
R = random.Random(7)

def c(h): h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4)) + (255,)

# Palette ramps (dark -> light)
SKY = [c('#3a1f4a'), c('#6a2e52'), c('#a8434f'), c('#d8664a'), c('#f0954a'), c('#fbc062'), c('#ffe29a')]
SUN = [c('#ffd27a'), c('#fff0b8'), c('#fffbe8')]
SEA = [c('#3a2a4a'), c('#5e3a52'), c('#8a4e52'), c('#c8784e'), c('#ffd27a'), c('#fff0b8')]
DIST = [c('#5a3a5c'), c('#7a4a62')]            # far headlands
JUNGLE_FAR = [c('#2e3448'), c('#3e4a52'), c('#6a5a52')]
JUNGLE = [c('#16241e'), c('#22382a'), c('#365234'), c('#5a7038'), c('#c89a48')]  # last = gold rim light
STONE = [c('#2a2230'), c('#463a44'), c('#6a5a5a'), c('#9a8470'), c('#e0b878')]   # last = rim light
VINE = [c('#1e3020'), c('#2f4a2a'), c('#4e6a34')]
ROCK = [c('#141018'), c('#241c28'), c('#3a2e3a'), c('#56463e')]
MOSS = [c('#2a3a22'), c('#4a5e2a'), c('#8a8a3a')]

def layer(): return Image.new("RGBA", (W, H), (0, 0, 0, 0))

def put(img, x, y, col):
    if 0 <= x < W and 0 <= y < H: img.putpixel((x, y), col)

def dither_bands(img, ramp, y0, y1, order=None):
    """Horizontal bands with 2x2 ordered dither on each transition."""
    n = len(ramp)
    bayer = [[0, 2], [3, 1]]
    for y in range(y0, y1):
        t = (y - y0) / (y1 - y0) * (n - 1)
        b = int(t); f = t - b
        for x in range(W):
            k = b + (1 if f > (bayer[y % 2][x % 2] + .5) / 4 and b + 1 < n else 0)
            img.putpixel((x, y), ramp[k])

def blob(dr, cx, cy, r, col):
    dr.ellipse([cx - r, cy - r, cx + r, cy + r], fill=col)

# ── Sky + sun + sea ────────────────────────────────────────────────
HORIZON = 74
SUN_X, SUN_Y = 168, 66

def sky_layer():
    img = layer()
    dither_bands(img, SKY, 0, HORIZON)
    dr = ImageDraw.Draw(img)
    # sun halo rings (hard, SNES-like)
    for r, col in ((26, SKY[5]), (18, SKY[6]), (11, SUN[0]), (8, SUN[1]), (5, SUN[2])):
        for y in range(SUN_Y - r, min(HORIZON, SUN_Y + r + 1)):
            for x in range(SUN_X - r, SUN_X + r + 1):
                d2 = (x - SUN_X) ** 2 + (y - SUN_Y) ** 2
                if d2 <= r * r and (r > 18 or True):
                    if r == 26 and (x + y) % 2: continue  # dithered outer halo
                    put(img, x, y, col)
    # long thin clouds lit from below
    for cx, cy, w in ((40, 22, 60), (110, 14, 44), (210, 30, 70), (70, 44, 50), (230, 52, 40)):
        for i in range(w):
            x = cx - w // 2 + i; th = 2 if abs(i - w // 2) < w // 3 else 1
            for k in range(th): put(img, x, cy + k, SKY[1] if cy < 30 else SKY[2])
            put(img, x, cy + th, SKY[4] if cy < 30 else SKY[5])
    # sea
    dither_bands(img, SEA[:4], HORIZON, H)
    for y in range(HORIZON + 2, H, 2):  # sun glitter: short dashes, wider toward the viewer
        spread = 2 + (y - HORIZON) // 3
        for _ in range(2):
            x = SUN_X + R.randint(-spread, spread); L = R.randint(2, 5)
            for k in range(L): put(img, x + k, y, SEA[5] if k == L // 2 else SEA[4])
    for _ in range(90):               # wave lines elsewhere
        x, y = R.randrange(W), R.randrange(HORIZON + 2, H)
        for k in range(R.randint(2, 5)): put(img, x + k, y, SEA[3])
    return img

def far_layer():
    """Distant headlands on the horizon and the far jungle ridge."""
    img = layer(); dr = ImageDraw.Draw(img)
    # headland left, small islet right of the sun
    dr.polygon([(0, HORIZON), (0, 62), (14, 58), (36, 60), (58, 68), (70, HORIZON)], fill=DIST[0])
    dr.line([(0, 62), (14, 58), (36, 60)], fill=DIST[1])
    dr.polygon([(206, HORIZON), (214, 69), (226, 68), (236, HORIZON)], fill=DIST[0])
    # far jungle ridge across the valley (just below horizon), with bumpy canopy
    base = 104
    tops = []
    for x in range(0, W + 6, 6):
        y = base - int(3 * math.sin(x * .05) + 2 * math.sin(x * .13 + 1)); r = R.randint(4, 6)
        blob(dr, x, y, r, JUNGLE_FAR[0]); tops.append((x, y - r))
    for y in range(base, H): dr.line([(0, y), (W, y)], fill=JUNGLE_FAR[0])
    for x, y in tops:                       # thin warm sheen on the crowns facing the sun
        for k in range(-2, 2): put(img, x + k, y, JUNGLE_FAR[1])
        put(img, x + 1, y, JUNGLE_FAR[2])
    return img

# ── The temple ─────────────────────────────────────────────────────
TX, TBASE = 90, 122

def temple_layer():
    img = layer(); dr = ImageDraw.Draw(img)
    tiers = [(90, 12), (72, 12), (56, 11), (40, 11)]
    tier_rects = []
    y = TBASE
    for i, (w, h) in enumerate(tiers):
        x0, x1 = TX - w // 2, TX + w // 2
        dr.rectangle([x0, y - h, x1, y], fill=STONE[2])
        dr.rectangle([x0, y - h, x0 + w // 3, y], fill=STONE[1])            # shadowed left third
        dr.line([(x0, y - h), (x0, y)], fill=STONE[0])
        tier_rects.append((x0, x1, y - h))
        dr.line([(x0, y - h), (x1, y - h)], fill=STONE[2])                  # top ledge
        for xx in range(x0 + w // 3, x1 + 1): put(img, xx, y - h, STONE[3])
        for xx in range(x1 - 1, x1 + 1):                                    # rim light (sun at right)
            for yy in range(y - h, y + 1): put(img, xx, yy, STONE[4])
        for by in range(y - h + 3, y, 3):                                   # block courses
            off = 2 if (by // 3) % 2 else 0
            for bx in range(x0 + 2 + off, x1 - 2, 6):
                put(img, bx, by, STONE[1]); put(img, bx, by - 1, STONE[1])
            for bx in range(x0 + 1, x1 - 1):
                if (bx + by) % 7 == 0: put(img, bx, by, STONE[1])
        y -= h
    # central stair with lit edges
    dr.rectangle([TX - 7, y, TX + 7, TBASE], fill=STONE[2])
    for sy in range(y + 1, TBASE, 2): dr.line([(TX - 7, sy), (TX + 7, sy)], fill=STONE[1])
    dr.line([(TX + 7, y), (TX + 7, TBASE)], fill=STONE[4]); dr.line([(TX - 7, y), (TX - 7, TBASE)], fill=STONE[0])
    # shrine on top with a dark empty niche and a faint amber glow
    top = y
    dr.rectangle([TX - 13, top - 16, TX + 13, top], fill=STONE[1])
    dr.rectangle([TX - 13, top - 16, TX - 6, top], fill=STONE[0])
    dr.line([(TX + 13, top - 16), (TX + 13, top)], fill=STONE[4])
    dr.polygon([(TX - 17, top - 16), (TX, top - 25), (TX + 17, top - 16)], fill=STONE[2])
    dr.polygon([(TX - 17, top - 16), (TX, top - 25), (TX - 4, top - 16)], fill=STONE[1])
    dr.line([(TX, top - 25), (TX + 17, top - 16)], fill=STONE[4])
    dr.rectangle([TX - 4, top - 12, TX + 4, top], fill=c('#120c14'))   # empty niche
    dr.rectangle([TX - 2, top - 3, TX + 2, top - 1], fill=STONE[2])     # bare pedestal
    for (gx, gy, col) in ((0, -6, '#f0954a'), (-1, -5, '#a8434f'), (1, -5, '#a8434f'), (0, -8, '#6a2e52')):
        put(img, TX + gx, top + gy, c(col))                               # faint leftover glow
    for xx in (TX - 15, TX + 9):                                          # carved columns flanking the niche
        dr.rectangle([xx, top - 14, xx + 5, top], fill=STONE[2]); dr.line([(xx + 5, top - 14), (xx + 5, top)], fill=STONE[3])
    # vines hang from each tier's top ledge, heavier on the shaded left side
    for (x0, x1, ty) in tier_rects:
        for vx in (x0 + 3, x0 + 9, x1 - 6):
            if TX - 8 <= vx <= TX + 8: continue      # keep the stair clear
            L = R.randint(5, 11)
            for k in range(L):
                put(img, vx + (k // 5) % 2, ty + 1 + k, VINE[1] if k % 3 else VINE[2])
            put(img, vx - 1, ty + 2 + L // 2, VINE[2]); put(img, vx + 1, ty + 1, VINE[2])
    # broken corner: missing blocks on the right of tier 2
    dr.rectangle([TX + 30, TBASE - 24, TX + 37, TBASE - 19], fill=(0, 0, 0, 0))
    dr.rectangle([TX - 46, TBASE - 4, TX - 40, TBASE], fill=(0, 0, 0, 0))
    return img

# ── Jungle canopy around the temple base (mid layer) ───────────────
def canopy(img, dr, xs, base_y, radius, ramp, rim=True):
    for x in xs:
        r = radius + R.randint(-2, 3); y = base_y + R.randint(-3, 3)
        blob(dr, x, y, r, ramp[1])
        blob(dr, x - 1, y + 2, r - 2, ramp[0])           # shade low-left
        if rim:                                           # gold rim on upper-right
            for a in range(-70, -10, 6):
                px = x + int(math.cos(math.radians(a)) * r); py = y + int(math.sin(math.radians(a)) * r)
                put(img, px, py, ramp[3])
            for a in range(-55, -30, 8):
                px = x + int(math.cos(math.radians(a)) * r); py = y + int(math.sin(math.radians(a)) * r)
                put(img, px, py, ramp[4])
            for _ in range(3):                            # leaf clumps texture
                px, py = x + R.randint(-r + 3, r - 3), y + R.randint(-r + 3, -1)
                put(img, px, py, ramp[2]); put(img, px + 1, py, ramp[2]); put(img, px, py + 1, ramp[1])

def mid_layer():
    img = layer(); dr = ImageDraw.Draw(img)
    for y in range(124, H): dr.line([(0, y), (W, y)], fill=JUNGLE[0])
    canopy(img, dr, range(-4, W + 8, 12), 124, 9, JUNGLE)
    canopy(img, dr, range(4, W + 8, 14), 134, 10, JUNGLE)
    return img

# ── Foreground: mossy rock + fox from behind + framing leaves ──────
FOX_BACK = [
    "....k........k....",
    "...kok......kok...",
    "...koOk....kOok...",
    "...kOOOkkkkOOOk...",
    "....kOOOOOOOOk....",
    "...kbbbbbbbbbbk...",
    "...kbgggggggbbk...",
    "...kbbbbbbbbbbk...",
    "....kOOOOOOOOk....",
    "....krrrrrrrrrk...",
    "...krRRRRRRRRrrk..",
    "...kOOOOOOOOOrRrk.",
    "..kOOOOOOOOOOkrRk.",
    "..kOOoOOOOOOOk.rk.",
    "..kOOooOOOOOOk....",
    "...kOOoOOOOOk.....",
    "..kkOOOOOOOOkkk...",
    ".kWWkOOOOOOkOOOk..",
    ".kWWWkkkkkkOOOOOk.",
    "..kWWWWkOOOOOOOk..",
    "...kkkkWWWkkkkk...",
]
FOX_COL = {'k': c('#1a0e10'), 'O': c('#c8642a'), 'o': c('#f0954a'), 'b': c('#5a3a24'),
           'g': c('#8ab0c8'), 'r': c('#a82a2a'), 'R': c('#e04a3a'), 'W': c('#f4e8d8')}

def near_layer():
    img = layer(); dr = ImageDraw.Draw(img)
    # rock outcrop bottom-right
    rock = [(150, H), (162, 126), (178, 116), (206, 110), (236, 112), (256, 118), (256, H)]
    dr.polygon(rock, fill=ROCK[1])
    dr.polygon([(150, H), (162, 126), (178, 116), (190, 122), (176, H)], fill=ROCK[0])
    for i in range(0, 80, 1):                       # lit top edge
        x = 178 + i; y = int(116 - 6 * math.sin(i / 80 * math.pi)) if i < 60 else 111 + (i - 60) // 3
    dr.line([(178, 116), (206, 110), (236, 112), (256, 118)], fill=ROCK[3])
    for _ in range(40):                              # rock texture
        x, y = R.randint(170, 254), R.randint(120, H - 1)
        put(img, x, y, ROCK[2])
    # moss cap
    for x in range(180, 256):
        y = 110 + (abs(x - 206) // 9) + (1 if x > 236 else 0)
        put(img, x, y, MOSS[1]); put(img, x, y - 1, MOSS[0] if x % 3 else MOSS[2])
    # fox sitting on the rock, seen from behind (3/4, slightly from above)
    fx, fy = 196, 110 - len(FOX_BACK) + 2
    for j, row in enumerate(FOX_BACK):
        for i, ch in enumerate(row):
            if ch in FOX_COL: put(img, fx + i, fy + j, FOX_COL[ch])
    # foreground framing: a few broad dark leaves in the bottom-left corner
    for (bx, by, ang, L) in ((-4, H + 2, -40, 40), (8, H + 4, -62, 34), (-6, 126, -12, 30)):
        pts = []
        for i in range(L):
            a = math.radians(ang + i * 0.9)
            pts.append((bx + int(math.cos(a) * i), by + int(math.sin(a) * i)))
        for i, (x, y) in enumerate(pts):
            w = max(1, int(6 * math.sin(i / L * math.pi)))
            for k in range(-w, w + 1): put(img, x, y + k, ROCK[0])
            put(img, x, y - w, JUNGLE[1])
            if i % 3 == 0: put(img, x, y, JUNGLE[0])   # midrib
    return img

LAYERS = [("Sky", sky_layer), ("Far", far_layer), ("Temple", temple_layer), ("Mid", mid_layer), ("Near", near_layer)]

def main():
    out = Path(sys.argv[1]); out.mkdir(parents=True, exist_ok=True)
    comp = layer()
    for name, fn in LAYERS:
        img = fn(); comp.alpha_composite(img)
        if "--preview" not in sys.argv: img.save(out / f"{name}.png")
    comp.resize((W * 5, H * 5), Image.NEAREST).save(out / "preview.png")

if __name__ == "__main__":
    main()
