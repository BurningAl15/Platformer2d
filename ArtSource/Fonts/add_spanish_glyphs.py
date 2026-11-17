"""Adds Spanish accented letters to Pixelnauts.ttf in the font's own pixel style.

Each new glyph is the base letter plus accent pixels on the font's 64-unit grid,
so they match the existing letters. New glyphs are appended at the end of the
glyph order, which keeps the indices that TextMeshPro already cached.
Usage: python add_spanish_glyphs.py <source.ttf> <output.ttf>
"""
import sys
from fontTools.ttLib import TTFont
from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.pens.recordingPen import RecordingPen

PX = 64
src, out = sys.argv[1], sys.argv[2]
font = TTFont(src)
glyf, hmtx, cmap = font['glyf'], font['hmtx'], font.getBestCmap()

def rect(pen, x, y):
    pen.moveTo((x * PX, y * PX)); pen.lineTo((x * PX, (y + 1) * PX))
    pen.lineTo(((x + 1) * PX, (y + 1) * PX)); pen.lineTo(((x + 1) * PX, y * PX)); pen.closePath()

def base_contours(name, drop_above=None):
    rec = RecordingPen(); glyf[name].draw(rec, glyf)
    contours, current = [], []
    for op, args in rec.value:
        current.append((op, args))
        if op in ('closePath', 'endPath'):
            contours.append(current); current = []
    if drop_above is not None:   # remove the dot of i
        contours = [c for c in contours if min(p[1] for op, a in c for p in a) < drop_above]
    return contours

def accent_pixels(kind, width_px, top_px):
    c, b = width_px // 2, top_px + 1
    if kind == 'acute': return [(c, b + 1), (c - 1, b)]
    if kind == 'tilde': return [(c - 2, b), (c - 1, b + 1), (c, b), (c + 1, b + 1)]
    if kind == 'diaeresis': return [(1, b), (width_px - 2, b)]
    raise ValueError(kind)

def add(char, base_char, kind, drop_dot=False):
    if ord(char) in cmap: return
    base = cmap[ord(base_char)]
    g = glyf[base]
    width_px = round((g.xMax - g.xMin) / PX)
    top_px = 5 if base_char.islower() else 7
    pen = TTGlyphPen(glyf)
    for contour in base_contours(base, 320 if drop_dot else None):
        for op, args in contour: getattr(pen, op)(*args)
    for x, y in accent_pixels(kind, width_px, top_px): rect(pen, x, y)
    name = 'uni%04X' % ord(char)
    glyf[name] = pen.glyph(); hmtx[name] = hmtx[base]
    for table in font['cmap'].tables:
        if table.isUnicode(): table.cmap[ord(char)] = name

for upper, lower, base in (('Á', 'á', 'a'), ('É', 'é', 'e'), ('Í', 'í', 'i'), ('Ó', 'ó', 'o'), ('Ú', 'ú', 'u')):
    add(upper, base.upper(), 'acute'); add(lower, base, 'acute', drop_dot=(base == 'i'))
add('Ñ', 'N', 'tilde'); add('ñ', 'n', 'tilde')
add('Ü', 'U', 'diaeresis'); add('ü', 'u', 'diaeresis')

# ¿ is the question mark turned upside down within the same box.
if ord('¿') not in cmap:
    q = cmap[ord('?')]; g = glyf[q]
    rec = RecordingPen(); g.draw(rec, glyf)
    pen = TTGlyphPen(glyf)
    flip = lambda p: (g.xMax + g.xMin - p[0], g.yMax + g.yMin - p[1])
    for op, args in rec.value: getattr(pen, op)(*[flip(p) for p in args])
    glyf['uni00BF'] = pen.glyph(); hmtx['uni00BF'] = hmtx[q]
    for table in font['cmap'].tables:
        if table.isUnicode(): table.cmap[0xBF] = 'uni00BF'

# En and em dashes reuse the hyphen glyph.
for code in (0x2013, 0x2014):
    for table in font['cmap'].tables:
        if table.isUnicode() and code not in table.cmap: table.cmap[code] = cmap[ord('-')]

font['post'].formatType = 3.0   # glyph names are derived from the cmap
font.save(out)
print('glyphs', len(font.getBestCmap()))
