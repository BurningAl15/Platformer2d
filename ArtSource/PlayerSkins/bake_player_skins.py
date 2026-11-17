"""Bakes player skin sheets (style x color) from the original fox sheets."""
import sys, os
from PIL import Image
C=lambda h:tuple(int(h[i:i+2],16) for i in (1,3,5))+(255,)
A=C('#322c43'); FUR,HL,BELLY,CREAM,WHITE=C('#b5422d'),C('#e87838'),C('#a36f4e'),C('#f0b989'),C('#ffffff')
GOLD=C('#5a3418'); GLASS=C('#8fd0e8'); STRAP=C('#7a4a2e')
SC,SCS=C('#fe01fe'),C('#fe02fe')            # scarf key colors, replaced per palette
COLORS={ # fur, highlight, belly, cream, scarf, scarf shade
 'Original':('#b5422d','#e87838','#a36f4e','#f0b989','#d8453c','#a02c33'),
 'Amber':   ('#e08a2c','#f7b84a','#c98b52','#fbe3b5','#2aa7a0','#1c7a80'),
 'Arctic':  ('#7f93b8','#b8c7e0','#b9c4d6','#f4f7fb','#d8453c','#a02c33'),
 'Smoke':   ('#5b4a6e','#8a73a0','#a58a8c','#f0d9c2','#f2c230','#c8922a'),
 'Crimson': ('#c9352f','#f0623d','#d9b08a','#fff1dc','#3b6fd8','#2a4fa0'),
}
TAILS={ # scarf tail shapes relative to the scarf's back end: (main, shade, outline)
 'down':([(-4,1),(-5,1),(-6,1),(-6,2),(-7,2),(-8,3)],[(-2,0),(-1,0),(-5,2),(-7,3)],
         [(-3,1),(-7,1),(-8,2),(-9,3),(-9,4),(-8,4),(-7,4),(-6,3),(-5,3),(-4,2),(-6,0),(-5,0),(-4,0)]),
 'flat':([(-3,0),(-4,0),(-5,0),(-6,-1),(-7,-1)],[(-2,0),(-1,0),(-6,0)],
         [(-3,-1),(-4,-1),(-5,-1),(-6,-2),(-7,-2),(-8,-1),(-8,0),(-7,0),(-3,1),(-4,1),(-5,1),(-6,1),(-7,1)]),
 'up':  ([(-2,-1),(-3,-2),(-3,-3),(-4,-4)],[(-1,0),(-2,-2)],
         [(-3,-1),(-4,-2),(-4,-3),(-5,-4),(-4,-5),(-3,-4),(-2,-3),(-1,-1),(-5,-5)]),
 None:([],[],[]),
}
# Per frame: goggles=(hx, brow_row) or back strap row; scarf rows [(y,x1,x2),...]; tail kind + anchor
SPEC={
 'player-idle':[
  dict(g=(11,16),s=[(24,11,19),(25,12,17)],t=('down',11,24)),
  dict(g=(11,17),s=[(25,12,19),(26,12,17)],t=('down',12,25)),
  dict(g=(11,18),s=[(26,11,19),(27,11,17)],t=('down',11,26)),
  dict(g=(11,17),s=[(25,12,19),(26,12,17)],t=('down',12,25))],
 'player-run':[
  dict(g=(11,16),s=[(24,10,19),(25,9,19)],t=('flat',10,24)),
  dict(g=(10,15),s=[(23,12,20),(24,13,19)],t=('flat',12,23)),
  dict(g=(11,16),s=[(24,10,19),(25,9,18)],t=('flat',10,24)),
  dict(g=(12,16),s=[(22,12,17),(23,11,16)],t=('flat',12,22)),
  dict(g=(13,15),s=[(21,11,18),(22,11,17)],t=('flat',11,21)),
  dict(g=(12,16),s=[(23,10,16),(24,10,16)],t=('flat',10,23))],
 'player-jump':[
  dict(g=(13,14),s=[(20,11,22),(21,12,20)],t=('flat',11,20)),
  dict(g=(12,15),s=[(22,11,20),(23,12,19)],t=('up',11,22))],
 'player-hurt':[
  dict(g=(9,14),s=[(22,9,23),(23,12,20)],t=(None,0,0)),
  dict(g=(9,14),s=[(22,9,23),(23,12,20)],t=(None,0,0))],
 'player-crouch':[
  dict(g=(15,22),s=[(28,14,18),(29,14,21)],t=('up',14,28)),
  dict(g=(15,22),s=[(28,14,18),(29,14,21)],t=('up',14,28))],
 'player-climb':[
  dict(b=13,s=[(15,10,23)],e=[(20,16),(21,16),(21,17)]),
  dict(b=13,s=[(15,10,23)],e=[(12,16),(13,16),(12,17)]),
  dict(b=13,s=[(15,10,23)],e=[(12,16),(20,16)])],
}
def aviator_frame(fr,spec):
    px=fr.load()
    def body(x,y): return 0<=x<33 and 0<=y<32 and px[x,y][3]>0 and px[x,y] not in (A,WHITE)
    def put(c,pts,need_body=True):
        for x,y in pts:
            if 0<=x<33 and 0<=y<32 and (not need_body or body(x,y)): px[x,y]=c
    # scarf first, so its tail sits behind the head accessories
    for i,(y,x1,x2) in enumerate(spec['s']):
        put(SC,[(x,y) for x in range(x1,x2+1)])
        if i==0: put(SCS,[(x1,y),(x2,y)])
        else: put(SCS,[(x,y) for x in range(x1,x2+1)][::3])
    if 't' in spec:
        kind,ax,ay=spec['t']; main,shade,outline=TAILS[kind]
        put(A,[(ax+dx,ay+dy) for dx,dy in outline],False)
        put(SC,[(ax+dx,ay+dy) for dx,dy in main],False); put(SCS,[(ax+dx,ay+dy) for dx,dy in shade],False)
    put(SC,spec.get('e',[]))
    if 'b' in spec:   # seen from behind: only the goggle strap shows
        put(STRAP,[(x,spec['b']) for x in range(9,25)])
    else:
        hx,yb=spec['g']
        put(STRAP,[(hx+1,yb-1),(hx+2,yb-1),(hx+7,yb-1),(hx+7,yb-2),(hx+12,yb-1)])
        for x0 in (hx+3,hx+8):
            put(GOLD,[(x0,yb-2),(x0,yb-1),(x0+3,yb-2),(x0+3,yb-1),(x0+1,yb-3),(x0+2,yb-3)])
            put(GLASS,[(x0+1,yb-2),(x0+1,yb-1),(x0+2,yb-1)]); put(WHITE,[(x0+2,yb-2)])
def bake(src_dir,out_dir):
    made=[]
    for style in ('Classic','Aviator'):
        for color,(f,h,b,c,s,ss) in COLORS.items():
            if style=='Classic' and color=='Original': continue   # the untouched sheets
            m={FUR:C(f),HL:C(h),BELLY:C(b),CREAM:C(c),SC:C(s),SCS:C(ss)}
            d=os.path.join(out_dir,f'{style}_{color}'); os.makedirs(d,exist_ok=True)
            for name,frames in SPEC.items():
                im=Image.open(os.path.join(src_dir,name+'.png')).convert('RGBA')
                if style=='Aviator':
                    for i,spec in enumerate(frames):
                        fr=im.crop((i*33,0,i*33+33,32)); aviator_frame(fr,spec); im.paste(fr,(i*33,0))
                px=im.load()
                for y in range(im.height):
                    for x in range(im.width):
                        if px[x,y] in m: px[x,y]=m[px[x,y]]
                im.save(os.path.join(d,name+'.png')); made.append(os.path.join(d,name+'.png'))
    return made
def contact(out_dir,path,sets,sc=4):
    rows=[]
    for st in sets:
        ims=[Image.open(os.path.join(out_dir,st,n+'.png')) for n in SPEC]
        w=sum(i.width for i in ims); row=Image.new('RGBA',(w,32),(0,0,0,0)); x=0
        for i in ims: row.paste(i,(x,0)); x+=i.width
        rows.append(row)
    W=rows[0].width*sc; sheet=Image.new('RGBA',(W,len(rows)*(32*sc+6)+6),(120,170,200,255))
    for k,r in enumerate(rows):
        big=r.resize((r.width*sc,32*sc),Image.NEAREST); sheet.paste(big,(0,6+k*(32*sc+6)),big)
    sheet.save(path)
if __name__=='__main__':
    made=bake(sys.argv[1],sys.argv[2]); print(len(made),'files')
    contact(sys.argv[2],sys.argv[3],['Aviator_Original','Aviator_Amber'],sc=5)
    contact(sys.argv[2],sys.argv[4],['Classic_Amber','Classic_Arctic','Classic_Smoke','Classic_Crimson','Aviator_Arctic','Aviator_Smoke','Aviator_Crimson'],sc=3)
