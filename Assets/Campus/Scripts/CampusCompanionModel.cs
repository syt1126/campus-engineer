using UnityEngine;
using System.Collections.Generic;

namespace CampusEngineer
{
    // Original companion refined as connected sculpt surfaces with painted markings and a short coat.
    public static class CampusCompanionModel
    {
        static Transform Group(string name,Transform parent,Vector3 p)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        static Material Mat(string name,Color color,float gloss=.15f)
        {var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",gloss);return m;}
        static Transform Detail(string name,Transform parent,Vector3 p,Vector3 size,Material material)
        {var g=CampusAdventureBuilder.Ellipsoid(name,parent,p,size,material);return g.transform;}
        static Material Fabric(string name,Color color)
        {
            var m=Mat(name,color,.08f);const int size=128;var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {float a=Mathf.Sin(x*Mathf.PI*.5f)*.16f,b=Mathf.Sin(y*Mathf.PI*.5f)*.16f;pixels[y*size+x]=new Color(a*.5f+.5f,b*.5f+.5f,1,a*.5f+.5f);}
            var t=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Fine woven thread relief",wrapMode=TextureWrapMode.Repeat};t.SetPixels(pixels);t.Apply();m.SetTexture("_BumpMap",t);m.SetTextureScale("_BumpMap",new Vector2(6,6));m.SetFloat("_BumpScale",.3f);m.EnableKeyword("_NORMALMAP");return m;
        }
        public static CampusMascot Create(Transform parent,Vector3 position,Transform viewer)
        {
            var root=Group("Lan / Refined continuous companion",parent,position);var actor=root.gameObject.AddComponent<CampusMascot>();
            var coat=CampusSculptLibrary.Fur(.0055f);var tailCoat=CampusSculptLibrary.Fur(.009f);
            var denim=Fabric("Companion / tailored indigo canvas",new Color(.14f,.28f,.32f));
            var mint=Fabric("Companion / woven jade scarf",new Color(.19f,.48f,.40f));
            var dark=Mat("Companion / warm dark paw",new Color(.18f,.105f,.068f));
            var brass=Mat("Companion / antique brass",new Color(.77f,.50f,.19f),.45f);
            var thread=Mat("Companion / warm topstitch",new Color(.61f,.57f,.39f));
            var cream=Mat("Companion / warm eye ivory",new Color(.85f,.79f,.63f),.30f);
            var iris=Mat("Companion / textured amber iris",Color.white,.55f);iris.mainTexture=Iris();
            var pupil=Mat("Companion / pupils",new Color(.045f,.025f,.015f),.48f);
            var glint=Mat("Companion / eye reflection",new Color(1,.94f,.79f),.9f);
            actor.body=Group("Articulated breathing chest",root,new Vector3(0,.60f,0));
            CampusSculptLibrary.Part("Body",actor.body,Vector3.zero,coat);
            CampusSculptLibrary.Part("Overalls",actor.body,Vector3.zero,denim);
            var pocket=CampusGeometry.SoftBox("Shaped bib pocket",new Vector3(0,.075f,-.218f),new Vector3(.16f,.14f,.013f),.018f,mint,actor.body,false);
            Stitch(new Vector3(-.065f,.13f,-.228f),new Vector3(-.065f,.023f,-.228f),thread,actor.body);
            Stitch(new Vector3(.065f,.13f,-.228f),new Vector3(.065f,.023f,-.228f),thread,actor.body);
            Stitch(new Vector3(-.065f,.023f,-.228f),new Vector3(.065f,.023f,-.228f),thread,actor.body);
            for(int s=-1;s<=1;s+=2)
            {
                Ribbon("Fitted canvas shoulder strap",actor.body,new Vector3(s*.145f,.31f,-.12f),new Vector3(s*.143f,.12f,-.222f),.048f,denim);
                Detail("Engraved overall fastener",actor.body,new Vector3(s*.143f,.132f,-.232f),new Vector3(.028f,.028f,.012f),brass);
                for(int h=-1;h<=1;h+=2)Detail("Button thread hole",actor.body,new Vector3(s*.143f+h*.005f,.132f,-.239f),Vector3.one*.0035f,dark);
                var leg=Detail("Soft trouser ankle",root,new Vector3(s*.13f,.18f,0),new Vector3(.18f,.28f,.19f),denim);
                var shoe=Detail("Canvas shoe upper",root,new Vector3(s*.13f,.075f,-.055f),new Vector3(.21f,.13f,.29f),denim);
                CampusGeometry.SoftBox("Layered rubber shoe sole",new Vector3(s*.13f,.025f,-.055f),new Vector3(.211f,.033f,.29f),.018f,thread,root,false);
                for(int lace=0;lace<3;lace++)CampusGeometry.Tube("Canvas shoe lace",new Vector3(s*.13f-.045f,.132f,-.11f+lace*.028f),new Vector3(s*.13f+.045f,.132f,-.093f+lace*.028f),.003f,thread,root);
                var arm=Group(s<0?"Left articulated shoulder":"Right articulated shoulder",actor.body,new Vector3(s*.225f,.19f,.006f));
                CampusSculptLibrary.Part("Arm",arm,Vector3.zero,coat);
                var paw=CampusSculptLibrary.Part("Paw",arm,new Vector3(0,-.34f,-.015f),coat);
                Detail("Soft central paw pad",paw,new Vector3(0,-.017f,-.055f),new Vector3(.066f,.066f,.012f),Mat("Companion / muted rose paw pads",new Color(.42f,.23f,.18f)));
                for(int toe=0;toe<4;toe++)Detail("Individual toe pad",paw,new Vector3(-.045f+toe*.03f,-.074f,-.022f),new Vector3(.018f,.022f,.011f),dark);
                if(s<0)actor.leftArm=arm;else actor.rightArm=arm;
            }
            actor.head=Group("Connected expressive head",root,new Vector3(0,1.13f,-.015f));
            CampusSculptLibrary.Part("Head",actor.head,Vector3.zero,coat);
            var eyes=new Transform[2];
            for(int s=-1;s<=1;s+=2)
            {
                var eye=Group("Blinking almond eye",actor.head,new Vector3(s*.145f,.034f,-.284f));eyes[s<0?0:1]=eye;
                Detail("Inset eye white",eye,Vector3.zero,new Vector3(.112f,.108f,.032f),cream);
                EyeDome(eye,new Vector3(-s*.003f,-.002f,-.020f),.094f,.096f,.011f,iris);
                Detail("Calm rounded pupil",eye,new Vector3(-s*.003f,-.002f,-.029f),new Vector3(.038f,.052f,.010f),pupil);
                Detail("Soft window reflection",eye,new Vector3(-.017f,.020f,-.036f),new Vector3(.018f,.021f,.006f),glint);
                Detail("Lower eye reflection",eye,new Vector3(.015f,-.019f,-.035f),Vector3.one*.007f,glint);
                // Short tapered whiskers follow the cheek instead of floating cheek balls.
                for(int k=0;k<3;k++)CampusGeometry.Tube("Fine cheek whisker",new Vector3(s*.225f,-.10f-k*.018f,-.257f),new Vector3(s*(.32f+k*.012f),-.09f-k*.028f,-.27f),.0013f,thread,actor.head);
            }
            var nose=Detail("Soft triangular nose",actor.head,new Vector3(0,-.083f,-.338f),new Vector3(.082f,.052f,.032f),dark);nose.localRotation=Quaternion.Euler(0,0,180);
            Detail("Nose highlight",actor.head,new Vector3(-.014f,-.074f,-.354f),new Vector3(.021f,.007f,.003f),Mat("Companion / nose sheen",new Color(.35f,.23f,.15f),.7f));
            CampusGeometry.Tube("Muzzle philtrum",new Vector3(0,-.106f,-.353f),new Vector3(0,-.132f,-.348f),.003f,dark,actor.head);
            Vector3 previous=new Vector3(-.062f,-.126f,-.340f);
            for(int k=1;k<=18;k++){float x=-.062f+k*.124f/18;var next=new Vector3(x,-.145f+Mathf.Pow(x/.062f,2)*.019f,-.347f+Mathf.Pow(x/.062f,2)*.007f);CampusGeometry.Tube("Subtle smiling mouth",previous,next,.0033f,dark,actor.head);previous=next;}
            Collar(root,mint);Ribbon("Folded scarf end",root,new Vector3(.12f,.89f,-.145f),new Vector3(.20f,.65f,-.20f),.115f,mint);
            Detail("Small satchel",actor.body,new Vector3(.225f,-.14f,.10f),new Vector3(.16f,.23f,.18f),denim);
            actor.tail=Group("Single continuous striped tail",root,new Vector3(0,.39f,.16f));CampusSculptLibrary.Part("Tail",actor.tail,Vector3.zero,tailCoat);
            var c=root.gameObject.AddComponent<CapsuleCollider>();c.center=new Vector3(0,.74f,0);c.height=1.5f;c.radius=.34f;
            var i=root.gameObject.AddComponent<CampusInteractable>();i.action=CampusAction.Mentor;i.title="和小嵐打招呼";
            actor.Configure(viewer,position,eyes);return actor;
        }
        static void Stitch(Vector3 a,Vector3 b,Material m,Transform parent)
        {for(int i=0;i<12;i++)CampusGeometry.Tube("Canvas topstitch",Vector3.Lerp(a,b,i/12f),Vector3.Lerp(a,b,(i+.52f)/12f),.0012f,m,parent);}
        static void EyeDome(Transform parent,Vector3 position,float width,float height,float depth,Material m)
        {
            const int rings=10,sides=48;var vertices=new List<Vector3>{new Vector3(0,0,-depth)};var uv=new List<Vector2>{new Vector2(.5f,.5f)};var normals=new List<Vector3>{Vector3.back};var triangles=new List<int>();
            for(int r=1;r<=rings;r++)for(int j=0;j<sides;j++){float t=r/(float)rings,a=j/(float)sides*Mathf.PI*2,x=Mathf.Cos(a)*t,y=Mathf.Sin(a)*t;vertices.Add(new Vector3(x*width*.5f,y*height*.5f,-depth*Mathf.Sqrt(1-t*t)));uv.Add(new Vector2(x*.5f+.5f,y*.5f+.5f));normals.Add(new Vector3(x*.35f,y*.35f,-1).normalized);}
            for(int j=0;j<sides;j++){triangles.AddRange(new[]{0,1+(j+1)%sides,1+j});}
            for(int r=0;r<rings-1;r++)for(int j=0;j<sides;j++){int a=1+r*sides+j,b=1+r*sides+(j+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,b,c,b,d,c});}
            var mesh=new Mesh{name="Planar-mapped amber iris"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();var t0=Group(mesh.name,parent,position);t0.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t0.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;
        }
        static void Ribbon(string name,Transform parent,Vector3 a,Vector3 b,float width,Material m)
        {
            var t=Group(name,parent,Vector3.zero);var v=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int side=0;side<2;side++)for(int i=0;i<=18;i++)for(int j=0;j<5;j++)
            {float s=i/18f,x=j/4f-.5f;var p=Vector3.Lerp(a,b,s)+new Vector3(x*width,0,Mathf.Sin(s*Mathf.PI)*.025f+Mathf.Cos(x*8)*.004f+(side==0?-.0025f:.0025f));v.Add(p);uv.Add(new Vector2(j/4f,s));}
            int layer=19*5;for(int side=0;side<2;side++)for(int i=0;i<18;i++)for(int j=0;j<4;j++){int x=side*layer+i*5+j;triangles.AddRange(side==0?new[]{x,x+5,x+1,x+1,x+5,x+6}:new[]{x,x+1,x+5,x+1,x+6,x+5});}
            var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;
        }
        static void Collar(Transform parent,Material mat)
        {
            var v=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();int sides=48,rings=12;
            for(int i=0;i<=sides;i++)for(int j=0;j<=rings;j++){float a=i/(float)sides*Mathf.PI*2,b=j/(float)rings*Mathf.PI*2;v.Add(new Vector3(Mathf.Cos(a)*(.185f+.026f*Mathf.Cos(b)),.91f+.041f*Mathf.Sin(b),Mathf.Sin(a)*(.153f+.024f*Mathf.Cos(b))));uv.Add(new Vector2(i/(float)sides,j/(float)rings));}
            for(int i=0;i<sides;i++)for(int j=0;j<rings;j++){int x=i*(rings+1)+j;indices.AddRange(new[]{x,x+1,x+rings+1,x+1,x+rings+2,x+rings+1});}
            var mesh=new Mesh{name="Soft folded scarf collar"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();var t=Group(mesh.name,parent,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
        static Texture2D Iris()
        {
            const int s=256;var pixels=new Color[s*s];
            for(int y=0;y<s;y++)for(int x=0;x<s;x++){float dx=(x+.5f)/s-.5f,dy=(y+.5f)/s-.5f,r=Mathf.Sqrt(dx*dx+dy*dy)*2,a=Mathf.Atan2(dy,dx);float fibers=Mathf.Sin(a*94+r*19)*Mathf.Sin(a*43-r*8)*.5f+.5f;pixels[y*s+x]=Color.Lerp(new Color(.20f,.075f,.025f),new Color(.62f,.36f,.095f),fibers*(1-Mathf.SmoothStep(.75f,1,r))*.75f+.1f);}
            var tex=new Texture2D(s,s,TextureFormat.RGB24,true){name="Original radial amber iris"};tex.SetPixels(pixels);tex.Apply();return tex;
        }
    }
}
