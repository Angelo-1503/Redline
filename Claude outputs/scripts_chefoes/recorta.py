# recorta uma folha do ChatGPT (grade 4x2, feita em cima de 1_poses.png) nos quadros do chefão
# uso: recorta.py <folha> <NomeChefe> <CorpoDaFolha> <pasta_previa> [gravar]
import sys
from PIL import Image
C='C:/Users/angel/Documents/GitHub/Redline/Assets/Art/Characters/'
folha,nome,fac,out=sys.argv[1:5]; gravar=len(sys.argv)>5
BOT=5; PADU=14                      # folga em cima (em pixels 1x) porque a cabeça é mais alta que o lenço
src=Image.open(folha).convert('RGBA')
assert src.size==(1536,1024),src.size
fs=[('idle',fac+'.png')]+[('run_%d'%k,'%s_run_%d.png'%(fac,k)) for k in range(6)]
if fac=='Player_blue': fs=fs[1:]
def quadro(i,f):
    w,h=Image.open(C+f).size
    x=(i%4)*384+(384-3*w)//2; y=(i//4)*512+512-3*h-40
    box=(x,y-3*PADU,x+3*w,y+3*(h+BOT))   # BOT: o ChatGPT desenha os pés um pouco abaixo da grade
    c=src.crop(box); H=h+PADU+BOT
    P=c.load()
    # pré-multiplica para o branco do fundo não sujar a borda
    r=Image.new('RGBA',(w,H)); R=r.load()
    for v in range(H):
        for u in range(w):
            sr=sg=sb=sa=0
            for j in range(3):
                for k in range(3):
                    p=P[u*3+k,v*3+j]
                    a=p[3]
                    if min(p[:3])>238: a=0          # fundo branco
                    sr+=p[0]*a; sg+=p[1]*a; sb+=p[2]*a; sa+=a
            if sa/9>=120: R[u,v]=(sr//sa,sg//sa,sb//sa,255)
    # fica só o boneco (some sujeira solta)
    vis=set(); il=[]
    for v in range(H):
        for u in range(w):
            if R[u,v][3] and (u,v) not in vis:
                st=[(u,v)]; vis.add((u,v)); g=[]
                while st:
                    a,b=st.pop(); g.append((a,b))
                    for n in ((a+1,b),(a-1,b),(a,b+1),(a,b-1)):
                        if 0<=n[0]<w and 0<=n[1]<H and n not in vis and R[n[0],n[1]][3]: vis.add(n); st.append(n)
                il.append(g)
    il.sort(key=len)
    for g in il[:-1]:
        for (a,b) in g: R[a,b]=(0,0,0,0)
    return r.resize((w*2,H*2),Image.NEAREST)
fr=[(t,quadro(i,f)) for i,(t,f) in enumerate(fs)]
# corta a folga de baixo igual em todos, até o pé mais baixo: o pivô fica no chão
fr=[(t,q.crop((0,0,q.width,q.height-(q.height-q.getchannel('A').getbbox()[3]))) ) if t=='idle' else (t,q) for t,q in fr]
fundo=max(q.getchannel('A').getbbox()[3]-q.height for t,q in fr if t!='idle')
maxb=max(q.getchannel('A').getbbox()[3] for t,q in fr if t!='idle')
fr=[(t,q if t=='idle' else q.crop((0,0,q.width,maxb))) for t,q in fr]
bg=(120,110,95,255)
s=Image.new('RGBA',(sum(q.width for _,q in fr),fr[1][1].height),bg); x=0
for t,q in fr: s.alpha_composite(q,(x,s.height-q.height)); x+=q.width
s.save(out+'/folha_%s.png'%nome)
for t,q in fr:
    print(t,q.size,q.getchannel('A').getbbox())
    if gravar: q.save(C+'Chefoes/Chefe%s_%s.png'%(nome,t))
print('gravado' if gravar else 'previa')
