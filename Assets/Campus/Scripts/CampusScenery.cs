using UnityEngine;

namespace CampusEngineer
{
    // Original background courtyard, layered skyline and landscape geometry.
    public sealed class CampusScenery
    {
        readonly Transform root;
        Material grass,stone,bark,leaf,leafLight,cream,roof,window;
        public CampusScenery(Transform parent){root=new GameObject("School garden / Layered exterior").transform;root.SetParent(parent,false);}
        Material Mat(string name,Color color,float smooth=.25f)
        {var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",smooth);return m;}
        GameObject Form(PrimitiveType type,string name,Vector3 p,Vector3 s,Material m)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root,false);go.transform.localPosition=p;go.transform.localScale=s;go.GetComponent<Renderer>().sharedMaterial=m;Object.Destroy(go.GetComponent<Collider>());
            if(name=="Rounded tree canopy")
            {
                go.GetComponent<MeshFilter>().sharedMesh=Foliage(Mathf.RoundToInt(p.x*13+p.z*17));
            }
            return go;
        }
        public static Mesh Foliage(int seed)
        {
            var vertices=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();var uv=new System.Collections.Generic.List<Vector2>();var normals=new System.Collections.Generic.List<Vector3>();var random=new System.Random(seed);
            for(int leaf=0;leaf<240;leaf++)
            {
                float a=(float)random.NextDouble()*Mathf.PI*2,y=(float)random.NextDouble()*2-1,r=Mathf.Sqrt(1-y*y)*(.35f+(float)random.NextDouble()*.14f);
                var center=new Vector3(Mathf.Cos(a)*r,y*.42f,Mathf.Sin(a)*r);var rotation=Quaternion.Euler((float)random.NextDouble()*130-65,(float)random.NextDouble()*360,(float)random.NextDouble()*100-50);
                float size=.035f+(float)random.NextDouble()*.035f;
                for(int side=0;side<2;side++)
                {
                    int start=vertices.Count;var n=(center.normalized+Vector3.up*.8f).normalized*(side==0?1:-1);
                    vertices.Add(center+rotation*new Vector3(0,.08f*size,0));uv.Add(new Vector2(.5f,.5f));normals.Add(n);
                    for(int edge=0;edge<12;edge++)
                    {float theta=edge*Mathf.PI/6;var v=new Vector3(Mathf.Sin(theta)*.58f,0,Mathf.Cos(theta));vertices.Add(center+rotation*(v*size));uv.Add(new Vector2(v.x*.5f+.5f,v.z*.5f+.5f));normals.Add(n);}
                    for(int edge=0;edge<12;edge++){triangles.Add(start);triangles.Add(start+1+(side==0?edge:(edge+1)%12));triangles.Add(start+1+(side==0?(edge+1)%12:edge));}
                }
            }
            var mesh=new Mesh{name="Layered individual garden leaves"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
        void Soft(string name,Vector3 p,Vector3 size,Material m,float r=.12f){CampusGeometry.SoftBox(name,p,size,r,m,root,false);}
        public void Build()
        {
            grass=Mat("Garden lawn",new Color(.45f,.59f,.39f),.08f);stone=Mat("Warm courtyard stone",new Color(.77f,.73f,.63f),.2f);
            bark=Mat("Tree bark",new Color(.36f,.29f,.2f),.1f);leaf=Mat("Courtyard leaf",new Color(.28f,.45f,.3f),.12f);leafLight=Mat("Sunlit leaf",new Color(.42f,.58f,.33f),.14f);
            cream=Mat("Warm residential plaster",new Color(.76f,.78f,.72f),.2f);roof=Mat("Terracotta and sage trim",new Color(.53f,.6f,.55f),.23f);window=Mat("Distant blue glazing",new Color(.32f,.5f,.57f),.5f);
            Form(PrimitiveType.Cube,"Green school courtyard",new Vector3(-29,-.4f,0),new Vector3(46,.3f,90),grass);
            Soft("Window garden path",new Vector3(-8.2f,-.22f,0),new Vector3(3.7f,.08f,72),stone,.3f);
            Soft("Cross courtyard path",new Vector3(-24,-.2f,9),new Vector3(29,.06f,2.2f),stone,.4f);
            for(int i=0;i<6;i++)
            {
                float z=-24+i*9;
                Soft("Raised garden border",new Vector3(-12,-.04f,z),new Vector3(2.2f,.42f,3.2f),stone,.15f);
                Soft("Planting bed",new Vector3(-12,.19f,z),new Vector3(1.9f,.06f,2.9f),grass,.2f);
                Tree(new Vector3(-12,.2f,z),.85f+i%2*.15f,i);
                Tree(new Vector3(-25,-.2f,z+3.5f),1.2f,i+7);
                Bench(new Vector3(-16,.02f,z+2.2f));
            }
            for(int i=0;i<4;i++)Building(new Vector3(-39-i%2*8,-.25f,-25+i*17),14+i%3*3,i);
            for(int i=0;i<5;i++)
            {
                var hill=Form(PrimitiveType.Sphere,"Distant green ridge",new Vector3(-72-i%2*16,-3,-55+i*29),new Vector3(45,25+i%3*8,43),Mat("Hazy ridge "+i,new Color(.42f+i*.015f,.56f+i*.012f,.51f+i*.015f),.05f));
                hill.transform.localRotation=Quaternion.Euler(0,i*29,0);
            }
            // Clouds are blended into the sky shader rather than built from visible spheres.
        }
        void Tree(Vector3 p,float scale,int seed)
        {
            Vector3 top=p+Vector3.up*3.8f*scale;
            CampusGeometry.Tube("Tree trunk",p,top,.11f*scale,bark,root);
            for(int i=0;i<6;i++)
            {
                float a=(i*137.5f+seed*24)*Mathf.Deg2Rad;
                Vector3 branch=p+new Vector3(Mathf.Cos(a)*1.1f,2.9f+i%3*.4f,Mathf.Sin(a)*1.1f)*scale;
                CampusGeometry.Tube("Natural tree branch",p+Vector3.up*(2.1f+i%2*.5f)*scale,branch,.045f*scale,bark,root);
                var crown=Form(PrimitiveType.Sphere,"Rounded tree canopy",branch+Vector3.up*.5f*scale,new Vector3(2.6f,2.2f,2.4f)*scale,i%3==0?leafLight:leaf);crown.transform.localRotation=Quaternion.Euler(i*21,seed*31+i*51,0);
            }
        }
        void Bench(Vector3 p)
        {
            var timber=Mat("Outdoor honey timber",new Color(.6f,.46f,.3f),.27f);
            for(int i=0;i<4;i++)Soft("Garden bench slat",p+new Vector3(0,.43f,-.23f+i*.15f),new Vector3(1.6f,.06f,.12f),timber,.035f);
            for(int x=-1;x<=1;x+=2)Soft("Bench foot",p+new Vector3(x*.58f,.2f,0),new Vector3(.08f,.4f,.6f),roof,.03f);
        }
        void Building(Vector3 p,float height,int index)
        {
            Vector3 size=new Vector3(7,height,10);
            Soft("Layered campus neighbourhood",p+Vector3.up*height*.5f,size,cream,.18f);
            Soft("Residential roofline",p+Vector3.up*(height+.12f),new Vector3(7.25f,.26f,10.3f),roof,.08f);
            for(int floor=1;floor<height-1;floor+=3)for(int k=-3;k<=3;k+=2)
            {
                Soft("Glazed window bay",p+new Vector3(3.55f,floor+.6f,k),new Vector3(.09f,1.6f,1.2f),window,.025f);
                if((floor+k+index)%3==0)
                {
                    Soft("Shallow balcony",p+new Vector3(3.9f,floor-.25f,k),new Vector3(.8f,.1f,1.5f),stone,.05f);
                    Soft("Balcony railing",p+new Vector3(4.24f,floor+.2f,k),new Vector3(.04f,.8f,1.5f),roof,.02f);
                }
            }
            for(int k=-3;k<=3;k+=3)Soft("Vertical facade trim",p+new Vector3(3.57f,height*.5f,k),new Vector3(.1f,height,.12f),stone,.025f);
        }
    }
}
