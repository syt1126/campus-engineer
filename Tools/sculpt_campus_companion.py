"""Original continuous sculpt meshes, replacing intersecting primitive anatomy."""
from pathlib import Path
import numpy as np, struct, json, hashlib
R=Path(__file__).resolve().parents[1];P=R if (R/'Assets').exists() else R/'unity/CampusEngineer'
OUT=P/'Assets/Campus/Resources/MascotSculpt.bytes'
meshes=[]

def ellipsoid(p,c,r):
    q=(p-np.array(c))/r
    k0=np.linalg.norm(q,axis=-1);k1=np.linalg.norm(q/np.array(r),axis=-1)
    return k0*(k0-1)/(k1+1e-8)
def union(a,b,k=.04):
    h=np.maximum(k-np.abs(a-b),0)/k
    return np.minimum(a,b)-h*h*k*.25
def sculpt(name,lo,hi,res,field,paint):
    axes=[np.linspace(lo[i],hi[i],res[i],dtype=np.float32) for i in range(3)]
    xyz=np.stack(np.meshgrid(*axes,indexing='ij'),axis=-1)
    values=field(xyz)
    offsets=np.array([[0,0,0],[1,0,0],[1,1,0],[0,1,0],[0,0,1],[1,0,1],[1,1,1],[0,1,1]])
    corners=np.stack([values[o[0]:res[0]-1+o[0],o[1]:res[1]-1+o[1],o[2]:res[2]-1+o[2]] for o in offsets],axis=-1)
    active=np.argwhere((corners.min(-1)<0)&(corners.max(-1)>=0))
    tetra=[[0,5,1,6],[0,1,2,6],[0,2,3,6],[0,3,7,6],[0,7,4,6],[0,4,5,6]]
    triangles=[]
    for cell in active:
        idx=cell+offsets
        p=xyz[idx[:,0],idx[:,1],idx[:,2]];v=values[idx[:,0],idx[:,1],idx[:,2]]
        for tet in tetra:
            inside=[i for i in tet if v[i]<0];outside=[i for i in tet if v[i]>=0]
            if not inside or not outside:continue
            def edge(a,b):return p[a]+(p[b]-p[a])*(v[a]/(v[a]-v[b]))
            if len(inside)==1:triangles.append([edge(inside[0],i) for i in outside])
            elif len(inside)==3:triangles.append([edge(outside[0],i) for i in inside])
            else:
                a,b=inside;c,d=outside;ac=edge(a,c);ad=edge(a,d);bc=edge(b,c);bd=edge(b,d)
                triangles.extend([[ac,ad,bc],[ad,bd,bc]])
    tri=np.array(triangles,dtype=np.float32)
    points=tri.reshape(-1,3)
    eps=.0005
    grad=np.stack([(field(points+np.eye(3)[i]*eps)-field(points-np.eye(3)[i]*eps))/(2*eps) for i in range(3)],-1)
    normals=grad/np.maximum(np.linalg.norm(grad,axis=-1,keepdims=True),1e-8)
    fn=np.cross(tri[:,1]-tri[:,0],tri[:,2]-tri[:,0]);avg=normals.reshape(-1,3,3).mean(axis=1)
    swap=(fn*avg).sum(axis=1)<0;tri[swap]=tri[swap][:,[0,2,1]]
    points=tri.reshape(-1,3)
    grad=np.stack([(field(points+np.eye(3)[i]*eps)-field(points-np.eye(3)[i]*eps))/(2*eps) for i in range(3)],-1)
    normals=grad/np.maximum(np.linalg.norm(grad,axis=-1,keepdims=True),1e-8)
    # Weld exact interpolated edge positions so the sculpt is a continuous manifold surface.
    verts,inv=np.unique(np.round(points,6),axis=0,return_inverse=True)
    norm=np.zeros_like(verts);np.add.at(norm,inv,normals);norm/=np.maximum(np.linalg.norm(norm,axis=-1,keepdims=True),1e-8)
    uv=np.stack([np.arctan2(verts[:,2],verts[:,0])/(2*np.pi)+.5,(verts[:,1]-lo[1])/(hi[1]-lo[1])],axis=-1)
    colors=paint(verts)
    meshes.append((name,verts,norm,uv,colors,inv.astype(np.int32)))
    print(name,len(verts),'vertices',len(inv)//3,'triangles',flush=True)

def mix(a,b,t):return a*(1-t[:,None])+b*t[:,None]
def smooth(a,b,x):
    t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
orange=np.array([.80,.40,.16]);cream=np.array([.93,.82,.63]);brown=np.array([.24,.12,.07]);denim=np.array([.12,.26,.30])
def head_field(p):
    d=ellipsoid(p,[0,0,0],[.344,.273,.251])
    for side in [-1,1]:
        d=union(d,ellipsoid(p,[side*.205,-.078,-.117],[.171,.135,.194]),.06)
        # Ears are joined into the head rather than stacked spheres.
        d=union(d,ellipsoid(p,[side*.267,.256,.035],[.107,.174,.074]),.055)
    d=union(d,ellipsoid(p,[0,-.108,-.242],[.137,.090,.104]),.065)
    return d
def head_paint(p):
    x,y,z=p.T;front=1-smooth(-.13,.04,z)
    c=np.tile(orange,(len(p),1))
    forehead=smooth(.06,.23,y)*(.30+smooth(-.1,.2,z)*.15)
    c=mix(c,np.array([.64,.26,.11]),forehead)
    cheeks=np.maximum(np.exp(-((x-.235)/.124)**4-((y+.087)/.085)**4),np.exp(-((x+.235)/.124)**4-((y+.087)/.085)**4))*front
    muzzle=np.exp(-(x/.16)**4-((y+.11)/.082)**4)*front
    c=mix(c,cream,np.clip(np.maximum(cheeks,muzzle),0,1))
    mask=np.maximum(np.exp(-((x-.144)/.087)**4-((y-.032)/.093)**4),np.exp(-((x+.144)/.087)**4-((y-.032)/.093)**4))*front
    c=mix(c,brown,mask*.86)
    ears=smooth(.20,.31,y)*np.exp(-((np.abs(x)-.267)/.082)**4)*(1-smooth(.005,.07,z))
    c=mix(c,cream,ears*.93)
    inner=smooth(.24,.29,y)*(1-smooth(.34,.41,y))*np.exp(-((np.abs(x)-.267)/.055)**4)*(1-smooth(-.035,.015,z))
    c=mix(c,np.array([.49,.29,.21]),inner*.8)
    return c
sculpt('Head',[-.43,-.32,-.39],[.43,.46,.30],[100,94,82],head_field,head_paint)

def torso_field(p):
    return union(ellipsoid(p,[0,-.04,.025],[.25,.335,.185]),ellipsoid(p,[0,.21,.0],[.19,.16,.17]),.075)
sculpt('Body',[-.30,-.41,-.23],[.30,.40,.24],[58,68,48],torso_field,lambda p:np.tile(orange,(len(p),1)))
def arm_field(p):
    return union(ellipsoid(p,[0,-.12,0],[.086,.19,.09]),ellipsoid(p,[0,-.24,-.012],[.075,.13,.085]),.05)
sculpt('Arm',[-.115,-.39,-.13],[.115,.10,.12],[40,62,40],arm_field,lambda p:mix(np.tile(orange,(len(p),1)),brown,1-smooth(-.28,-.15,p[:,1])))
def paw_field(p):
    d=ellipsoid(p,[0,0,.0],[.075,.082,.058])
    for i in range(4):d=union(d,ellipsoid(p,[-.047+i*.031,-.058,-.002],[.021,.054,.025]),.022)
    return union(d,ellipsoid(p,[-.070,-.008,.005],[.030,.052,.04]),.025)
sculpt('Paw',[-.115,-.135,-.085],[.105,.105,.08],[46,48,36],paw_field,lambda p:np.tile(brown,(len(p),1)))
def clothes_field(p):
    d=ellipsoid(p,[0,-.11,0],[.255,.235,.203])
    # Waist and short trouser openings blend into a fitted garment, not a spherical belly.
    d=np.maximum(d,p[...,1]-.035)
    for s in [-1,1]:d=union(d,ellipsoid(p,[s*.13,-.29,-.005],[.12,.115,.167]),.045)
    q=np.abs(p-np.array([0,.115,-.178]))-np.array([.12,.145,.028])
    box=np.linalg.norm(np.maximum(q,0),axis=-1)+np.minimum(np.max(q,axis=-1),0)-.018
    return union(d,box,.028)
sculpt('Overalls',[-.30,-.43,-.26],[.30,.31,.24],[62,68,50],clothes_field,lambda p:np.tile(denim,(len(p),1)))

# The tail is one tapered swept surface. Painted bands replace visibly separate balls.
points=[];normals=[];uv=[];colors=[];indices=[];rings=55;segs=28
def center(t):return np.array([.08+.48*np.sin(t*1.5),.02+.17*t,.05+.65*t])
for i in range(rings):
    t=i/(rings-1);c=center(t);direction=center(min(1,t+.001))-center(max(0,t-.001));direction/=np.linalg.norm(direction)
    u=np.cross(direction,[0,1,0]);u/=np.linalg.norm(u);v=np.cross(direction,u)
    radius=.112*np.sin(np.pi*(.08+.9*t))**.45*(1-.25*t)
    for j in range(segs):
        a=j/segs*2*np.pi;n=np.cos(a)*u+np.sin(a)*v
        points.append(c+n*radius);normals.append(n);uv.append([j/segs,t]);band=.5+.5*np.cos(t*np.pi*13)
        colors.append(orange*(1-smooth(.35,.65,np.array([band]))[0])+np.array([.39,.16,.07])*smooth(.35,.65,np.array([band]))[0])
        if i<rings-1:
            a0=i*segs+j;b0=i*segs+(j+1)%segs;c0=(i+1)*segs+j;d0=(i+1)*segs+(j+1)%segs;indices.extend([a0,b0,c0,b0,d0,c0])
meshes.append(('Tail',np.array(points),np.array(normals),np.array(uv),np.array(colors),np.array(indices)))

OUT.parent.mkdir(exist_ok=True,parents=True)
with OUT.open('wb') as f:
 f.write(struct.pack('<ii',0x434D5331,len(meshes)))
 for name,vertices,normals,uv,colors,indices in meshes:
    encoded=name.encode();f.write(struct.pack('<i',len(encoded)));f.write(encoded);f.write(struct.pack('<ii',len(vertices),len(indices)))
    data=np.column_stack([vertices,normals,uv,colors,np.ones(len(vertices))]).astype('<f4');f.write(data.tobytes());f.write(indices.astype('<i4').tobytes())
(P/'Documentation/MASCOT_SCULPT.json').write_text(json.dumps({'type':'original continuous signed-distance sculpt, connected facial/ear/muzzle topology','file':OUT.relative_to(P).as_posix(),'meshes':[{'name':m[0],'vertices':len(m[1]),'triangles':len(m[-1])//3} for m in meshes],'sha256':hashlib.sha256(OUT.read_bytes()).hexdigest()},indent=2),encoding='utf8')
