using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace CampusEngineer
{
    public static class CampusSculptLibrary
    {
        static Dictionary<string,Mesh> meshes;
        public static Mesh Get(string name)
        {
            if(meshes==null || meshes.Count==0 || meshes["Head"]==null)Load();
            return meshes[name];
        }
        static void Load()
        {
            meshes=new Dictionary<string,Mesh>();var data=Resources.Load<TextAsset>("MascotSculpt");
            using(var r=new BinaryReader(new MemoryStream(data.bytes)))
            {
                if(r.ReadInt32()!=0x434D5331)throw new InvalidDataException("Mascot sculpt format");
                int count=r.ReadInt32();
                for(int m=0;m<count;m++)
                {
                    string name=Encoding.UTF8.GetString(r.ReadBytes(r.ReadInt32()));int n=r.ReadInt32(),t=r.ReadInt32();
                    var vertices=new Vector3[n];var normals=new Vector3[n];var uv=new Vector2[n];var colors=new Color[n];
                    for(int i=0;i<n;i++){vertices[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());normals[i]=new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());uv[i]=new Vector2(r.ReadSingle(),r.ReadSingle());colors[i]=new Color(r.ReadSingle(),r.ReadSingle(),r.ReadSingle(),r.ReadSingle());}
                    var indices=new int[t];for(int i=0;i<t;i++)indices[i]=r.ReadInt32();
                    var mesh=new Mesh{name="Continuous companion sculpt / "+name,indexFormat=IndexFormat.UInt32};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.colors=colors;mesh.triangles=indices;mesh.RecalculateBounds();meshes[name]=mesh;
                }
            }
        }
        public static Transform Part(string part,Transform parent,Vector3 p,Material material)
        {var go=new GameObject("Sculpted "+part);go.transform.SetParent(parent,false);go.transform.localPosition=p;go.AddComponent<MeshFilter>().sharedMesh=Get(part);go.AddComponent<MeshRenderer>().sharedMaterial=material;return go.transform;}
        public static Material Fur(float length)
        {
            var m=new Material(Resources.Load<Shader>("CampusCompanionFur"));m.name="Soft directional cinnamon coat";m.SetFloat("_FurLength",length);
            const int s=256;var pixels=new Color[s*s];var random=new System.Random(4103);
            for(int y=0;y<s;y++)for(int x=0;x<s;x++)
            {float n=(float)random.NextDouble();pixels[y*s+x]=new Color(n,n,n,n);}
            var tex=new Texture2D(s,s,TextureFormat.RGBA32,false){name="Original short fur density",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};tex.SetPixels(pixels);tex.Apply();m.SetTexture("_FurNoise",tex);return m;
        }
    }
}
