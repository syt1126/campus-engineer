using System.Collections.Generic;
using UnityEngine;

namespace CampusEngineer
{
    // Rounded surfaces are actual mesh geometry, with matching collision volumes.
    public static class CampusGeometry
    {
        static readonly Dictionary<string,Mesh> rounded=new Dictionary<string,Mesh>();
        public static GameObject SoftBox(string name,Vector3 position,Vector3 size,float radius,Material material,Transform parent,bool collision=true)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
            string key=size.ToString("F4")+"/"+radius;
            Mesh mesh;
            if(!rounded.TryGetValue(key,out mesh) || mesh==null){mesh=RoundedMesh(size,radius);rounded[key]=mesh;}
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            if(collision)go.AddComponent<BoxCollider>().size=size;
            return go;
        }
        static Mesh RoundedMesh(Vector3 size,float radius)
        {
            var half=size*.5f;var radii=new Vector3(Mathf.Min(radius,half.x*.95f),Mathf.Min(radius,half.y*.95f),Mathf.Min(radius,half.z*.95f));
            var inner=half-radii;var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            foreach(var normal in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back})
            {
                Vector3 u=Mathf.Abs(normal.x)>.5f?Vector3.forward:Vector3.right,v=Vector3.Cross(normal,u);
                float hu=Vector3.Dot(half,Abs(u)),hv=Vector3.Dot(half,Abs(v)),hn=Vector3.Dot(half,Abs(normal));
                float[] us=Samples(hu,Vector3.Dot(radii,Abs(u))),vs=Samples(hv,Vector3.Dot(radii,Abs(v)));int offset=vertices.Count;
                for(int y=0;y<vs.Length;y++)for(int x=0;x<us.Length;x++)
                {
                    Vector3 p=normal*hn+u*us[x]+v*vs[y];
                    Vector3 clamped=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
                    Vector3 delta=p-clamped,n=new Vector3(delta.x/radii.x,delta.y/radii.y,delta.z/radii.z).normalized;
                    vertices.Add(clamped+Vector3.Scale(n,radii));normals.Add(new Vector3(n.x/radii.x,n.y/radii.y,n.z/radii.z).normalized);uv.Add(new Vector2((us[x]+hu)/(2*hu),(vs[y]+hv)/(2*hv)));
                }
                for(int y=0;y<vs.Length-1;y++)for(int x=0;x<us.Length-1;x++)
                {int a=offset+y*us.Length+x,b=a+1,c=a+us.Length,d=c+1;triangles.AddRange(new[]{a,b,c,b,d,c});}
            }
            var mesh=new Mesh{name="Rounded furniture surface"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
        static Vector3 Abs(Vector3 v){return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));}
        static float[] Samples(float h,float r){return new[]{-h,-h+r*.134f,-h+r*.5f,-h+r,h-r,h-r*.5f,h-r*.134f,h};}

        public static void Tube(string name,Vector3 a,Vector3 b,float radius,Material material,Transform parent,bool collision=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=(a+b)*.5f;
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);go.transform.localScale=new Vector3(radius*2,(b-a).magnitude*.5f,radius*2);
            go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)Object.Destroy(go.GetComponent<Collider>());
        }
        public static GameObject CurvedChairBack(Vector3 position,Material material,Transform parent)
        {
            var go=new GameObject("Curved ergonomic backrest");go.transform.SetParent(parent,false);go.transform.localPosition=position;
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();const int columns=12,rows=4;
            for(int side=0;side<2;side++)for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float u=x/(float)columns,v=y/(float)rows,px=(u-.5f)*.5f;
                vertices.Add(new Vector3(px,(v-.5f)*.36f,.065f*Mathf.Pow(px/.25f,2)-.045f*v+(side==0?-.017f:.017f)));
                uv.Add(new Vector2(u,v));
            }
            int stride=columns+1,layer=(rows+1)*stride;
            for(int side=0;side<2;side++)for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {int a=side*layer+y*stride+x,b=a+1,c=a+stride,d=c+1;triangles.AddRange(side==0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            for(int y=0;y<rows;y++){Bridge(triangles,y*stride,(y+1)*stride,layer);Bridge(triangles,(y+1)*stride+columns,y*stride+columns,layer);}
            for(int x=0;x<columns;x++){Bridge(triangles,x+1,x,layer);Bridge(triangles,rows*stride+x,rows*stride+x+1,layer);}
            var mesh=new Mesh{name="Bent plywood shell"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            var box=go.AddComponent<BoxCollider>();box.size=new Vector3(.5f,.36f,.09f);return go;
        }
        static void Bridge(List<int> t,int a,int b,int layer){t.AddRange(new[]{a,b,a+layer,b,b+layer,a+layer});}

        public static Mesh Leaf(float width,float length)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            const int rows=10;
            for(int side=0;side<2;side++)for(int y=0;y<=rows;y++)for(int x=0;x<3;x++)
            {
                float t=y/(float)rows,s=x-1;
                vertices.Add(new Vector3(s*width*.5f*Mathf.Sin(Mathf.PI*t),.07f*Mathf.Sin(Mathf.PI*t)-Mathf.Abs(s)*.035f*Mathf.Sin(Mathf.PI*t),t*length));uv.Add(new Vector2(x*.5f,t));
            }
            int layer=(rows+1)*3;
            for(int side=0;side<2;side++)for(int y=0;y<rows;y++)for(int x=0;x<2;x++)
            {int a=side*layer+y*3+x,b=a+1,c=a+3,d=c+1;triangles.AddRange(side==0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            var mesh=new Mesh{name="Curved leaf"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Mesh Curtain(float width,float height)
        {
            const int columns=32,rows=8;var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            for(int side=0;side<2;side++)for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float u=x/(float)columns,v=y/(float)rows;
                vertices.Add(new Vector3((u-.5f)*width,(v-.5f)*height,Mathf.Sin(u*Mathf.PI*10)*.045f*(1+.3f*(1-v))+(side==0?-.002f:.002f)));
                uv.Add(new Vector2(u,v));
            }
            int stride=columns+1,layer=(rows+1)*stride;
            for(int side=0;side<2;side++)for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)
            {int a=side*layer+y*stride+x,b=a+1,c=a+stride,d=c+1;triangles.AddRange(side==0?new[]{a,b,c,b,d,c}:new[]{a,c,b,b,c,d});}
            var mesh=new Mesh{name="Soft linen folds"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Texture2D Terrazzo()
        {
            const int size=256;var pixels=new Color[size*size];var random=new System.Random(203);Color baseColor=new Color(.79f,.79f,.73f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)pixels[y*size+x]=baseColor+Color.white*((Mathf.PerlinNoise(x*.09f,y*.09f)-.5f)*.035f);
            var colors=new[]{new Color(.7f,.73f,.67f),new Color(.85f,.82f,.73f),new Color(.75f,.76f,.71f),new Color(.9f,.88f,.8f)};
            for(int i=0;i<320;i++)
            {
                int cx=random.Next(size),cy=random.Next(size),radius=random.Next(1,3);Color chip=colors[random.Next(colors.Length)];
                for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++)if(x*x+y*y<=radius*radius)pixels[((cy+y+size)%size)*size+(cx+x+size)%size]=chip;
            }
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true);texture.name="Original warm terrazzo";texture.SetPixels(pixels);texture.Apply();return texture;
        }
    }
}
