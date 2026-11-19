"""Gold copy of the overworld sheet for levels cleared without losing a life.

Only the ten numbered level-sign tiles change: their blue hues shift to gold,
keeping each pixel's brightness. Everything else (sea, paths, grass) is copied
untouched. The copy keeps
the original slicing and sprite names (its .meta mirrors the
source meta with a new guid). Output: Assets/Resources/MapGold/.
Run: uv run --with pillow python ArtSource/Map/bake_gold_signs.py
"""
import colorsys, re, uuid
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SRC = ROOT / "Assets/2D Platformer Assets/Graphics/Overworld/Overworld_tiles_Padded.png"
OUT = ROOT / "Assets/Resources/MapGold/Overworld_tiles_Padded_Gold.png"

# Level sign tiles (Unity sprite rects, origin bottom-left): levels 1-5 on y=19, 6-10 on y=1.
SIGN_RECTS = [(x, y, 16, 16) for y in (19, 1) for x in (1, 19, 37, 55, 73)]

def main():
    im = Image.open(SRC).convert("RGBA"); px = im.load()
    for rx, ry, rw, rh in SIGN_RECTS:
      top = im.height - ry - rh
      for y in range(top, top + rh):
        for x in range(rx, rx + rw):
          r, g, b, a = px[x, y]
          if a == 0: continue
          h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
          if 0.52 <= h <= 0.72 and s > 0.25:          # the sign blues
              ns = min(1.0, s * 1.05); nv = min(1.0, v * 1.12 + 0.05)
              nr, ng, nb = colorsys.hsv_to_rgb(0.12, ns, nv)   # gold
              px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    OUT.parent.mkdir(parents=True, exist_ok=True)
    im.save(OUT)
    meta_src = SRC.with_suffix(".png.meta").read_text()
    meta_dst = OUT.with_suffix(".png.meta")
    guid = meta_dst.read_text().split("guid: ")[1].split()[0] if meta_dst.exists() else uuid.uuid4().hex
    meta_dst.write_text(re.sub(r"guid: [0-9a-f]{32}", f"guid: {guid}", meta_src, count=1))

if __name__ == "__main__":
    main()
