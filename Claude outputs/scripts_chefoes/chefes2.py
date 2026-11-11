import sys
from collections import deque
from PIL import Image
ROOT='C:/Users/angel/Documents/GitHub/Redline/'
C=ROOT+'Assets/Art/Characters/'
IMG='C:/Users/angel/AppData/Local/Temp/claude/C--Users-angel-Documents-GitHub-Redline/cb706b53-90a0-44f1-97d7-25a9f124110b/images/'
HEAD_H=58; PAD=12; DX=0; DY=0
OUT=sys.argv[1]
def head_img(n,arredonda=False):
    im=Image.open(IMG+'%d.webp'%n).convert('RGBA'); im=im.crop(im.getchannel('A').getbbox())
    w=round(im.width*HEAD_H/im.height); h=im.resize((w,HEAD_H),Image.BOX)
    r,g,b,a=h.split(); a=a.point(lambda v:0 if v<110 else 255)
    if not arredonda: return Image.merge('RGBA',(r,g,b,a))
    h=Image.merge('RGBA',(r,g,b,a)); Pm=h.load(); W,Hh=h.size
    bb=h.getchannel('A').getbbox(); xr=bb[2]-1; top=bb[1]; R=15
    for y in range(top,top+R):
        d=R-(y-top); lim=xr-int(round(R-(max(R*R-d*d,0))**.5))
        xs=[x for x in range(W) if Pm[x,y][3]>0]
        if not xs: continue
        for x in range(lim+1,W): Pm[x,y]=(0,0,0,0)
        e=min(max(xs),lim)
        for x in (e,e-1):
            if x>=0 and Pm[x,y][3]>0: Pm[x,y]=(30,20,16,255)
    return h
def warm(p):           # camisa, braçadeira, coronha, pele: tons quentes que param a limpeza
    r,g,b,a=p; return r-g>=24 and r>=60
def compose(path,head,modo='lenco',ponte=(96,56,44,255),ancora=None):
    b=Image.open(path).convert('RGBA'); b=b.resize((b.width*2,b.height*2),Image.NEAREST)
    P=b.load(); W,H=b.size
    t=next(y for y in range(H) if any(P[x,y][3]>0 for x in range(W)))
    cols=[x for x in range(W) if any(P[x,y][3]>0 for y in range(t,t+16))]
    hx0,hx1=min(cols),max(cols); cx=(hx0+hx1)//2+2
    # lenço = tudo ligado ao topo da cabeça que não seja tom quente; a arma (escura, à direita) fica
    ymax=t+46
    orig=b.copy(); O=orig.load()
    def is_gun(x,y):
        p=O[x,y]; return y>=t+30 and x>cx+6 and max(p[:3])<75
    if modo=='elipse':
        gy=next((y for y in range(t,H) if any(O[x,y][3]>0 for x in range(min(W-1,hx1+22),W))),t+40)
        ex=(hx0+hx1)/2+2; rx=(hx1-hx0)/2+7; ey=t+20; ry=34
        seen=set()
        for y in range(t,gy):
            for x in range(W):
                if O[x,y][3]>0 and ((x-ex)/rx)**2+((y-ey)/ry)**2<=1: seen.add((x,y))
        for y in range(t,gy+8):
            for x in range(hx0-4,min(W,hx1+10)):
                if O[x,y][3]>0 and warm(O[x,y]): seen.add((x,y))
        for (x,y) in seen: P[x,y]=(0,0,0,0)
    else:
        seen=set(); q=deque()
        for x in range(hx0,hx1+1):
            for y in range(t,t+16):
                if P[x,y][3]>0: q.append((x,y))
        while q:
            x,y=q.popleft()
            if (x,y) in seen or not(0<=x<W and t<=y<=ymax): continue
            p=O[x,y]
            if p[3]==0 or warm(p) or is_gun(x,y): continue
            seen.add((x,y))
            q.extend(((x+1,y),(x-1,y),(x,y+1),(x,y-1)))
        for (x,y) in seen: P[x,y]=(0,0,0,0)
    # lenço claro preso entre o rosto e a arma, que a limpeza por contato não alcança
    for y in (range(t,ymax+1) if modo=='lenco' else ()):
        for x in range(hx0-4,min(W,hx1+30)):
            p=O[x,y]
            if p[3]>0 and not warm(p) and min(p[:3])>=70 and p[0]-p[2]>=14:
                P[x,y]=(0,0,0,0); seen.add((x,y))
    # riscos finos do contorno do lenço em cima da arma: abertura 5x5 só nessa região
    if modo=='lenco':
        ac0=(ancora or (cx,t))[0]; y0=t+12; y1=t+52; x0=ac0+6; x1=min(W-1,ac0+56)
        cheio=lambda x,y:0<=x<W and 0<=y<H and P[x,y][3]>0
        fica=set()
        for y in range(y0-4,y1+5):
            for x in range(x0-4,x1+5):
                if all(cheio(x+i,y+j) for i in range(5) for j in range(5)):
                    fica.update((x+i,y+j) for i in range(-1,6) for j in range(-1,6))
        for y in range(y0,y1):
            for x in range(x0,x1):
                if P[x,y][3]>0 and (x,y) not in fica: P[x,y]=(0,0,0,0)
    # sobras soltas do lenço: fica só a maior ilha do boneco
    vis=set(); ilhas=[]
    for y in range(H):
        for x in range(W):
            if P[x,y][3]>0 and (x,y) not in vis:
                st=[(x,y)]; vis.add((x,y)); ilha=[]
                while st:
                    a,c=st.pop(); ilha.append((a,c))
                    for n in ((a+1,c),(a-1,c),(a,c+1),(a,c-1)):
                        if 0<=n[0]<W and 0<=n[1]<H and n not in vis and P[n[0],n[1]][3]>0:
                            vis.add(n); st.append(n)
                ilhas.append(ilha)
    ilhas.sort(key=len)
    for ilha in ilhas[:-1]:
        if len(ilha)<80 or min(c for _,c in ilha)<t+40:
            for (x,y) in ilha: P[x,y]=(0,0,0,0)
    # contorno escuro de 2 px onde o lenço saiu
    borda=[(x,y) for (x,y) in seen for dx,dy in ((1,0),(-1,0),(0,1),(0,-1))
           if 0<=x+dx<W and 0<=y+dy<H and P[x+dx,y+dy][3]>0]
    for (x,y) in borda:
        for dx in (-1,0,1):
            for dy in (-1,0,1):
                u,v=x+dx,y+dy
                if 0<=u<W and 0<=v<H and P[u,v][3]>0 and (u,v) not in seen:
                    pass
    out=Image.new('RGBA',(W,H+PAD)); out.paste(b,(0,PAD))
    Q=out.load()
    for (x,y) in set(borda):
        for dx,dy in ((1,0),(-1,0),(0,1),(0,-1),(2,0),(-2,0),(0,2),(0,-2)):
            u,v=x+dx,y+dy+PAD
            if 0<=u<W and 0<=v<H+PAD and Q[u,v][3]>0: Q[u,v]=(28,20,18,255)
    ac,at=ancora if ancora else (cx,t)
    hy=PAD+at+50-HEAD_H+DY; hx=ac-head.width//2+DX
    out.alpha_composite(head,(hx,hy))
    Q=out.load()
    # fecha o vão entre o ombro e o pescoço
    novos=[]
    for y in range(hy+HEAD_H-14,hy+HEAD_H+4):
        xs=[x for x in range(hx-34,hx+34) if Q[x,y][3]>0]
        for a,c in zip(xs,xs[1:]):
            if 1<c-a<=26:
                novos+= [(x,y) for x in range(a+1,c)]
    for (x,y) in novos: Q[x,y]=ponte
    S_=set(novos)
    for (x,y) in novos:
        if (x,y-1) not in S_ and Q[x,y-1][3]==0:
            Q[x,y]=(28,20,18,255); Q[x,y+1]=(28,20,18,255)
    return out,t,cx,seen
# posição da cabeça em cada quadro = base do corpo + deslocamento do tronco (medido com mede.py / mede2.py)
TRONCO={'Insurgencia':[(0,0),(-2,2),(0,2),(-2,10),(0,6),(2,4)],
        'Vanguarda':[(0,0),(0,2),(0,0),(0,6),(2,6),(2,4)],
        'Player_blue':[(0,0),(2,2),(0,0),(0,14),(2,0),(2,1)]}
BASE={'Insurgencia':(103,8),'Vanguarda':(112,8),'Player_blue':(110,4)}
BOSSES=[('Donk',97,'Insurgencia','lenco',(96,56,44,255)),('Olof',93,'Insurgencia','lenco',(96,56,44,255)),('S1mple',95,'Insurgencia','lenco',(96,56,44,255)),
        ('Device',94,'Vanguarda','elipse',(30,36,58,255)),('Zywoo',96,'Vanguarda','elipse',(30,36,58,255)),
        ('Fallen',98,'Player_blue','elipse',(30,36,58,255))]
if __name__=='__main__':
    gravar=len(sys.argv)>2 and sys.argv[2]=='gravar'
    sheet=Image.new('RGBA',(7*220,6*120),(120,110,95,255)); Z=[]
    for i,(nome,n,fac,modo,ponte) in enumerate(BOSSES):
        head=head_img(n,nome=='Donk')
        fr=[('idle',fac+'.png' if fac!='Player_blue' else fac+'_run_0.png')]+[('run_%d'%k,'%s_run_%d.png'%(fac,k)) for k in range(6)]
        for j,(tag,f) in enumerate(fr):
            anc=None
            if tag.startswith('run') or fac=='Player_blue':
                k=int(tag[-1]) if tag.startswith('run') else 0
                bx,by=BASE[fac]; dx,dy=TRONCO[fac][k]; anc=(bx+dx,by+dy)
            o,t,cx,seen=compose(C+f,head,modo,ponte,anc)
            dst=C+'Chefoes/Chefe%s_%s.png'%(nome,tag)
            a=Image.open(dst); assert a.size==o.size,(dst,a.size,o.size)
            sheet.alpha_composite(o.crop((0,0,o.width,120)),(j*220,i*120))
            if nome in('Donk','Device') and tag!='idle': Z.append(o.crop((70,6,190,86)).resize((360,240),Image.NEAREST))
            if gravar: o.save(dst)
    zz=Image.new('RGBA',(4*370,3*250),(120,110,95,255))
    for q,c in enumerate(Z): zz.alpha_composite(c,((q%4)*370,(q//4)*250))
    zz.save(OUT+'/zoom3.png')
    sheet.save(OUT+'/chefes_novos.png'); print('gravado' if gravar else 'previa')
