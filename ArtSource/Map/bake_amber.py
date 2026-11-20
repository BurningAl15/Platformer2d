"""Amber collectibles recoloured from existing pack art, keeping slicing and names.

gem.png -> Assets/Sprites/Collectibles/AmberShard.png (purple gem hues to amber)
UI_Heart.png -> Assets/Sprites/UI/AmberHeart.png (red heart hues to amber)
AmberGlow.png: soft glow drawn behind the Amber Heart relic.
Each .meta mirrors its source meta with its own guid.
Run: uv run --with pillow python ArtSource/Map/bake_amber.py
"""
import colorsys, re, uuid
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
JOBS = [
    (ROOT / "Assets/2D Platformer Assets/Graphics/Collectibles/gem.png", ROOT / "Assets/Sprites/Collectibles/AmberShard.png"),
    (ROOT / "Assets/2D Platformer Assets/Graphics/UI/UI_Heart.png", ROOT / "Assets/Sprites/UI/AmberHeart.png"),
]
AMBER_HUE = 0.095

def recolor(src, dst):
    im = Image.open(src).convert("RGBA"); px = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = px[x, y]
            if a == 0: continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if s < 0.15: continue                      # keep whites, greys and outline darks
            ns = min(1, max(0.62, s * 1.5)) if "gem" in src.name else min(1, s * 1.05)
            nv = v * 0.97 if "gem" in src.name else min(1, v * 1.08)
            nr, ng, nb = colorsys.hsv_to_rgb(AMBER_HUE, ns, nv)
            px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    dst.parent.mkdir(parents=True, exist_ok=True)
    im.save(dst)
    meta_dst = dst.with_suffix(".png.meta")
    guid = meta_dst.read_text().split("guid: ")[1].split()[0] if meta_dst.exists() else uuid.uuid4().hex
    meta_dst.write_text(re.sub(r"guid: [0-9a-f]{32}", f"guid: {guid}", src.with_suffix(".png.meta").read_text(), count=1))

for src, dst in JOBS:
    recolor(src, dst)

# Soft stepped glow behind the Amber Heart (32x32, three alpha rings).
glow = Image.new("RGBA", (32, 32), (0, 0, 0, 0)); gp = glow.load()
for y in range(32):
    for x in range(32):
        d = ((x - 15.5) ** 2 + (y - 15.5) ** 2) ** 0.5
        a = 150 if d < 7 else 90 if d < 11 else 40 if d < 15 else 0
        if a: gp[x, y] = (255, 196, 92, a)
glow.save(ROOT / "Assets/Sprites/Collectibles/AmberGlow.png")
