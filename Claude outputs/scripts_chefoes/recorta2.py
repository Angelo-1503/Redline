# recorta folha do ChatGPT mesmo se o boneco veio maior ou fora da grade: acha o boneco em cada célula,
# ajusta a escala e encosta a ponta da arma e os pés onde estavam no corpo original.
# uso: recorta2.py <folha> <NomeChefe> <Corpo> <pasta_previa> [gravar]
import sys
from PIL import Image
C='C:/Users/angel/Documents/GitHub/Redline/Assets/Art/Characters/'
REF='C:/Users/angel/Documents/GitHub/Redline/Claude outputs/chatgpt_chefoes/folhas_prontas/Device_fase2_azul.webp'
folha,nome,fac,out=sys.argv[1:5]; gravar=len(sys.argv)>5
PADU=16; BOT=0
def limpa(p):
    im=Image.open(p).convert('RGBA'); P=im.load()
    for y in range(im.height):
        for x in range(im.width):
            q=P[x,y]
            if q[3]<128 or min(q[:3])>232: P[x,y]=(q[0],q[1],q[2],0)
            else: P[x,y]=(q[0],q[1],q[2],255)
    return im
def caixas(im,n):
    r=[]
    for i in range(n):
        c=im.crop(((i%4)*384,(i//4)*512,(i%4)*384+384,(i//4)*512+512))
        # ignora sujeira: usa só colunas/linhas com massa
        a=c.getchannel('A'); bb=a.point(lambda v:255 if v else 0).getbbox()
        r.append(((i%4)*384+bb[0],(i//4)*512+bb[1],(i%4)*384+bb[2],(i//4)*512+bb[3]))
    return r
n=7 if fac!='Player_blue' else 6
src=limpa(folha); cx=caixas(src,n)
ref=limpa(REF); rx=caixas(ref,7)
hs=[b[3]-b[1] for b in cx]; hr=[b[3]-b[1] for b in rx]
f=(sum(hr)/len(hr))/(sum(hs)/len(hs))
print('alturas',hs,'ref',hr,'fator',round(f,3))
fs=[('idle',fac+'.png')]+[('run_%d'%k,'%s_run_%d.png'%(fac,k)) for k in range(6)]
if fac=='Player_blue': fs=fs[1:]
fr=[]
for i,(t,f0) in enumerate(fs):
    o=Image.open(C+f0).convert('RGBA'); w,h=o.size; ob=o.getchannel('A').getbbox()
    b=cx[i]; c=src.crop(b)
    tw=max(1,round(c.width*f/3)); th=max(1,round(c.height*f/3))
    r=c.convert('RGBa').resize((tw,th),Image.BOX).convert('RGBA')
    R=r.load()
    for y in range(th):
        for x in range(tw):
            q=R[x,y]; R[x,y]=(q[0],q[1],q[2],255) if q[3]>=120 else (0,0,0,0)
    H=h+PADU+BOT
    cv=Image.new('RGBA',(w,H))
    px=ob[2]-tw; py=PADU+ob[3]-th
    cv.alpha_composite(r,(max(px,0),max(py,0))) if px>=0 and py>=0 else cv.paste(r,(px,py),r)
    # só o boneco
    P=cv.load(); vis=set(); il=[]
    for y in range(H):
        for x in range(w):
            if P[x,y][3] and (x,y) not in vis:
                st=[(x,y)]; vis.add((x,y)); g=[]
                while st:
                    a,d=st.pop(); g.append((a,d))
                    for m in ((a+1,d),(a-1,d),(a,d+1),(a,d-1)):
                        if 0<=m[0]<w and 0<=m[1]<H and m not in vis and P[m[0],m[1]][3]: vis.add(m); st.append(m)
                il.append(g)
    il.sort(key=len)
    for g in il[:-1]:
        for (a,d) in g: P[a,d]=(0,0,0,0)
    print(t,'cv',cv.size,'cola',px,py,'tam',tw,th)
    fr.append((t,cv.resize((w*2,H*2),Image.NEAREST)))
s=Image.new('RGBA',(sum(q.width for _,q in fr),max(q.height for _,q in fr)),(120,110,95,255)); x=0
for t,q in fr: s.alpha_composite(q,(x,s.height-q.height)); x+=q.width
s.save(out+'/folha_%s.png'%nome)
if gravar:
    for t,q in fr: q.save(C+'Chefoes/Chefe%s_%s.png'%(nome,t))
    print('gravado')
