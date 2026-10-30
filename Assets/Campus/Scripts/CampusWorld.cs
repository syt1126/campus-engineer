using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CampusEngineer
{
    public enum CampusAction { Inspect, Sensor, Tablet, Terminal, Robot, Mentor, Door, Drawer, Window, Fan, Light, Evidence, Play }
    public class CampusInteractable : MonoBehaviour
    {
        public CampusAction action;
        public string title;
        public Transform animatedPart;
        public bool opened;
        Vector3 startPosition;
        Quaternion startRotation;
        public string playId;
        public bool IsAnimating {get;private set;}
        float progress;
        void Start(){if(animatedPart!=null){startPosition=animatedPart.localPosition;startRotation=animatedPart.localRotation;}}
        public void Toggle()
        {
            opened=!opened;
            if(animatedPart==null)return;
            IsAnimating=true;
        }
        void Update()
        {
            if(animatedPart==null || !IsAnimating)return;
            progress=Mathf.MoveTowards(progress,opened?1:0,Time.deltaTime/ .85f);
            float eased=progress*progress*(3-2*progress);
            if(action==CampusAction.Drawer)animatedPart.localPosition=startPosition+Vector3.back*(.4f*eased);
            else animatedPart.localRotation=startRotation*Quaternion.Euler(0,-80*eased,0);
            IsAnimating=Mathf.Abs(progress-(opened?1:0))>.001f;
        }
    }

    public class CampusWorld
    {
        public Transform fanRotor,robot;
        public List<Light> classroomLights=new List<Light>();
        public List<Renderer> lampPanels=new List<Renderer>();
        public Material wood,metal,wall,floor,navy,white,green,glass;
        Material sage,linen,oak,chairPadding;
        readonly CampusGame game;
        readonly Transform root;
        Font font;
        public CampusWorld(CampusGame game)
        {
            this.game=game;root=new GameObject("Campus / First chapter").transform;
            font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft JhengHei","Microsoft YaHei","Arial"},40);
        }

        Material Mat(string name,Color color,float smooth=.25f,float metallic=0)
        {
            var m=new Material(Shader.Find("Standard")){name=name,color=color};
            m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",metallic);return m;
        }
        Material Surface(string key,Color tint,Vector2 tiling,float smooth)
        {
            var m=Mat(key,tint,smooth);var texture=Resources.Load<Texture2D>("Textures/"+key+"_diffuse");
            if(texture!=null){m.mainTexture=texture;m.mainTextureScale=tiling;}
            var normal=Resources.Load<Texture2D>("Textures/"+key+"_normal");
            if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");m.SetTextureScale("_BumpMap",tiling);}
            return m;
        }
        GameObject Primitive(PrimitiveType type,string name,Vector3 position,Vector3 size,Material material,Transform parent=null,bool collider=true)
        {
            var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(parent??root,false);
            obj.transform.localPosition=position;obj.transform.localScale=size;
            obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!collider)UnityEngine.Object.Destroy(obj.GetComponent<Collider>());
            return obj;
        }
        GameObject Box(string name,Vector3 p,Vector3 s,Material m,Transform parent=null,bool collider=true)
        {
            if(name=="Control console" || name=="Sensor 0" || name=="Sensor 1" || name=="Sensor 2" || name=="Data tablet" || name=="Tablet glass" || name=="Terminal screen" || name=="Evidence board" || name=="Tools drawer")
                return CampusGeometry.SoftBox(name,p,s,.055f,m,parent??root,collider);
            return Primitive(PrimitiveType.Cube,name,p,s,m,parent,collider);
        }
        void Label(string text,Vector3 p,float size,Color color,Vector3 rotation)
        {
            var go=new GameObject(text);go.transform.SetParent(root);go.transform.position=p;go.transform.eulerAngles=rotation;
            var mesh=go.AddComponent<TextMesh>();mesh.font=font;mesh.text=text;mesh.fontSize=70;mesh.characterSize=size;mesh.anchor=TextAnchor.MiddleCenter;mesh.alignment=TextAlignment.Center;mesh.color=color;
            go.GetComponent<Renderer>().sharedMaterial=new Material(Resources.Load<Shader>("CampusWorldText")){mainTexture=font.material.mainTexture};
            go.AddComponent<CampusWorldText>().source=text;
        }
        void Interact(GameObject go,CampusAction action,string title,Transform part=null)
        {var item=go.AddComponent<CampusInteractable>();item.action=action;item.title=title;item.animatedPart=part;}

        public void Build()
        {
            white=Mat("Warm enamel",new Color(.96f,.95f,.9f),.42f);
            metal=Mat("Satin sage steel",new Color(.48f,.57f,.53f),.42f,.2f);
            navy=Mat("Soft ocean teal",new Color(.19f,.38f,.4f),.42f);
            green=Mat("Sage board",new Color(.18f,.3f,.27f),.2f);
            wood=Surface("wood",new Color(1.3f,1.25f,1.15f),new Vector2(1,1),.35f);
            var oakShader=Resources.Load<Shader>("CampusSoftOak");if(oakShader!=null)wood.shader=oakShader;wood.color=new Color(.79f,.65f,.46f);
            oak=Mat("Honey oak",new Color(.73f,.57f,.37f),.4f);
            sage=Mat("Soft sage",new Color(.55f,.69f,.62f),.35f);
            linen=Mat("Ivory linen",new Color(.89f,.85f,.74f),.06f);
            chairPadding=Mat("Muted eucalyptus upholstery",new Color(.33f,.51f,.45f),.13f);
            wall=Surface("plaster",new Color(.98f,.96f,.9f),new Vector2(3,1),.16f);
            floor=Mat("Clean warm terrazzo",Color.white,.38f);floor.mainTexture=CampusGeometry.Terrazzo();floor.mainTextureScale=new Vector2(6,7);
            glass=Mat("Window glass",new Color(.4f,.65f,.74f,.15f),.94f,.12f);
            glass.SetFloat("_Mode",3);glass.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);glass.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);glass.SetInt("_ZWrite",0);glass.EnableKeyword("_ALPHAPREMULTIPLY_ON");glass.renderQueue=3000;

            Box("Floor slab",new Vector3(0,-.12f,0),new Vector3(12.3f,.24f,14.3f),floor);
            Box("Corridor floor",new Vector3(0,-.12f,-9),new Vector3(12.3f,.24f,4),floor);
            Box("Front teaching wall",new Vector3(0,1.9f,7),new Vector3(12.3f,3.8f,.2f),wall);
            Box("Storage wall",new Vector3(6,1.9f,0),new Vector3(.2f,3.8f,14),wall);
            Box("Entrance wall left",new Vector3(-1.9f,1.9f,-7),new Vector3(8.4f,3.8f,.2f),wall);
            Box("Entrance wall right",new Vector3(5.65f,1.9f,-7),new Vector3(.8f,3.8f,.2f),wall);
            Box("Door lintel",new Vector3(3.8f,3.35f,-7),new Vector3(3,.9f,.2f),wall);
            Box("Ceiling",new Vector3(0,3.85f,0),new Vector3(12.2f,.12f,14.2f),white);
            Box("Window sill wall",new Vector3(-6,.5f,0),new Vector3(.2f,1,14),wall);
            Box("Window upper wall",new Vector3(-6,3.35f,0),new Vector3(.2f,1,14),wall);
            for(int z=-6;z<=6;z+=3)
            {
                Box("Window pier",new Vector3(-6,1.95f,z),new Vector3(.2f,1.9f,.14f),white);
                if(z<6)
                {
                    var pane=Box("Window",new Vector3(-6,1.95f,z+1.5f),new Vector3(.04f,1.8f,2.8f),glass);
                    Interact(pane,CampusAction.Window,"檢查窗邊光線");
                    Box("Window crossbar",new Vector3(-5.97f,1.65f,z+1.5f),new Vector3(.08f,.07f,2.8f),metal,null,false);
                }
            }
            var grout=Mat("Quiet grout",new Color(.61f,.65f,.61f),.1f);
            for(int z=-7;z<=7;z+=2)
                Box("Floor grout line",new Vector3(0,.003f,z),new Vector3(12,.002f,.007f),grout,null,false);
            for(int x=-6;x<=6;x+=2)
                Box("Floor grout line",new Vector3(x,.003f,0),new Vector3(.007f,.002f,14),grout,null,false);
            Box("Lower wall trim",new Vector3(5.87f,.36f,0),new Vector3(.035f,.055f,14),navy,null,false);

            var doorHinge=new GameObject("Door hinge").transform;doorHinge.SetParent(root);doorHinge.position=new Vector3(2.3f,0,-6.98f);
            var door=Box("Classroom door",new Vector3(1.38f,1.35f,0),new Vector3(2.7f,2.7f,.09f),wood,doorHinge);
            Box("Door view panel",new Vector3(1.38f,1.94f,-.065f),new Vector3(1.25f,.8f,.035f),glass,doorHinge,false);
            Box("Door handle",new Vector3(2.5f,1.1f,-.12f),new Vector3(.2f,.04f,.08f),metal,doorHinge,false);
            Interact(door,CampusAction.Door,"推開課室門",doorHinge);
            Label("203   智慧課室",new Vector3(3.8f,3.25f,-7.15f),.027f,Color.white,Vector3.zero);
            Label("CAMPUS ENGINEER",new Vector3(-1.3f,2.8f,-7.15f),.025f,new Color(.15f,.27f,.3f),Vector3.zero);
            Box("Entrance sign",new Vector3(3.8f,3.25f,-7.1f),new Vector3(2.4f,.38f,.04f),navy,null,false);

            Box("Board frame",new Vector3(-1,2.1f,6.82f),new Vector3(7.4f,1.8f,.1f),metal);
            var board=Box("Classroom board",new Vector3(-1,2.1f,6.73f),new Vector3(7.2f,1.6f,.08f),green);
            Interact(board,CampusAction.Inspect,"查看工程任務");
            Label("校園開放日工程隊\n觀察  設計  測試  改進",new Vector3(-1,2.1f,6.65f),.065f,new Color(.92f,.96f,.87f),Vector3.zero);
            Box("Board tray",new Vector3(-1,1.25f,6.65f),new Vector3(7.3f,.07f,.18f),metal,null,false);
            for(int row=0;row<4;row++)for(int col=0;col<4;col++)Desk(new Vector3(-3.8f+col*2.1f,0,-3.9f+row*2.05f));
            Desk(new Vector3(-2,0,5.1f),true);

            var console=Box("Control console",new Vector3(5.1f,1.2f,3.6f),new Vector3(1.25f,1.3f,.24f),navy);
            console.transform.rotation=Quaternion.Euler(0,-90,0);
            Box("Terminal screen",new Vector3(4.94f,1.25f,3.6f),new Vector3(.05f,.8f,.95f),Mat("Screen glow",new Color(.14f,.55f,.57f),.65f),null,false);
            Interact(console,CampusAction.Terminal,"打開控制終端");
            Label("控制終端\nCONTROL",new Vector3(4.88f,2.06f,3.6f),.027f,new Color(.1f,.2f,.25f),new Vector3(0,90,0));

            for(int i=0;i<3;i++)
            {
                var sensor=Box("Sensor "+i,new Vector3(5.82f,1.5f,-3+i*1.25f),new Vector3(.17f,.28f,.45f),white);
                Interact(sensor,CampusAction.Sensor,new[]{"檢查溫度感測器","檢查有人感測器","檢查光線感測器"}[i]);
                Primitive(PrimitiveType.Sphere,"Sensor status indicator",new Vector3(5.72f,1.51f,-3+i*1.25f),new Vector3(.018f,.06f,.06f),sage,null,false);
                Label(new[]{"TEMP","OCC","LUX"}[i],new Vector3(5.7f,1.77f,-3+i*1.25f),.012f,Color.black,new Vector3(0,90,0));
            }

            var tabletRoot=new GameObject("Portable data tablet").transform;tabletRoot.SetParent(root);tabletRoot.position=new Vector3(-2,.9f,5);
            var tablet=Box("Data tablet",Vector3.zero,new Vector3(.46f,.035f,.34f),navy,tabletRoot);
            Box("Tablet glass",new Vector3(0,.024f,0),new Vector3(.4f,.005f,.28f),Mat("Tablet display",new Color(.35f,.7f,.73f),.8f),tabletRoot,false);
            Interact(tablet,CampusAction.Tablet,"拿取數據板",tabletRoot);
            var evidence=Box("Evidence board",new Vector3(5.84f,2.1f,5.6f),new Vector3(.1f,1.2f,1.8f),wood);
            Interact(evidence,CampusAction.Evidence,"查看工程記錄");
            Label("工程記錄\nEVIDENCE",new Vector3(5.74f,2.1f,5.6f),.027f,Color.white,new Vector3(0,90,0));

            BuildFan();BuildLighting();BuildCity();BuildSoftDetails();BuildSchoolProps();BuildRobot();BuildMentor();
            new CampusAdventureBuilder(game,root).Build();
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.55f,.66f,.77f);
            RenderSettings.ambientEquatorColor=new Color(.46f,.49f,.45f);
            RenderSettings.ambientGroundColor=new Color(.27f,.29f,.26f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.72f,.8f,.79f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0045f;
            var skyShader=Shader.Find("Skybox/Procedural");
            if(skyShader!=null){var sky=new Material(skyShader);sky.SetColor("_SkyTint",new Color(.52f,.69f,.84f));sky.SetColor("_GroundColor",new Color(.65f,.72f,.65f));sky.SetFloat("_AtmosphereThickness",.85f);sky.SetFloat("_Exposure",1.2f);RenderSettings.skybox=sky;}
            var softSky=Resources.Load<Shader>("CampusSoftSky");if(softSky!=null)RenderSettings.skybox=new Material(softSky);
            DynamicGI.UpdateEnvironment();
            QualitySettings.shadowDistance=45;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.antiAliasing=4;
        }

        void Desk(Vector3 p,bool teacher=false)
        {
            var desk=new GameObject(teacher?"Teacher workstation":"Student desk").transform;desk.SetParent(root);desk.position=p;
            Vector3 top=new Vector3(teacher ? 1.7f : 1.05f,.07f,teacher ? .85f : .66f);
            CampusGeometry.SoftBox("Rounded oak desktop",new Vector3(0,.76f,0),top,.055f,oak,desk);
            CampusGeometry.SoftBox("Thin oak grain inset",new Vector3(0,.798f,0),new Vector3(top.x-.065f,.009f,top.z-.065f),.055f,wood,desk,false);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {
                var foot=new Vector3(x*(top.x*.43f),.035f,z*(top.z*.43f));
                CampusGeometry.Tube("Tapered desk support",foot,new Vector3(x*(top.x*.39f),.73f,z*(top.z*.35f)),.021f,metal,desk);
                Primitive(PrimitiveType.Sphere,"Quiet rubber foot",foot,new Vector3(.052f,.035f,.052f),navy,desk,false);
            }
            CampusGeometry.SoftBox("Soft edged lower shelf",new Vector3(0,.55f,0),new Vector3(top.x*.85f,.035f,top.z*.7f),.015f,sage,desk,false);
            if(!teacher)
            {
                var chair=new GameObject("Comfortable student chair").transform;chair.SetParent(desk,false);chair.localPosition=new Vector3(0,0,-.75f);
                int index=Mathf.RoundToInt((p.x+4)*7+(p.z+4)*3);chair.localRotation=Quaternion.Euler(0,(index%5-2)*2.5f,0);
                CampusGeometry.SoftBox("Rounded plywood seat",new Vector3(0,.44f,0),new Vector3(.51f,.045f,.48f),.06f,oak,chair);
                CampusGeometry.SoftBox("Soft seat pad",new Vector3(0,.47f,0),new Vector3(.46f,.05f,.43f),.06f,chairPadding,chair);
                CampusGeometry.CurvedChairBack(new Vector3(0,.78f,-.23f),sage,chair);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                    CampusGeometry.Tube("Tubular chair leg",new Vector3(x*.22f,.025f,z*.21f),new Vector3(x*.18f,.43f,z*.17f),.016f,metal,chair);
                for(int x=-1;x<=1;x+=2)CampusGeometry.Tube("Curved back support",new Vector3(x*.18f,.43f,-.16f),new Vector3(x*.18f,.83f,-.23f),.014f,metal,chair);
                var notebook=CampusGeometry.SoftBox("Notebook",new Vector3(.13f,.818f,.03f),new Vector3(.22f,.018f,.29f),.009f,index%3==0?sage:white,desk,false);
                notebook.transform.localRotation=Quaternion.Euler(0,(index%7-3)*3,0);
                CampusGeometry.Tube("Pencil",new Vector3(-.22f,.823f,.05f),new Vector3(-.13f,.823f,.17f),.004f,oak,desk);
            }
            else
            {
                var drawerRoot=new GameObject("Sliding drawer").transform;drawerRoot.SetParent(desk);drawerRoot.localPosition=new Vector3(.57f,.6f,-.05f);
                var drawer=Box("Tools drawer",Vector3.zero,new Vector3(.48f,.2f,.6f),wood,drawerRoot);
                Box("Handle",new Vector3(0,0,-.33f),new Vector3(.2f,.03f,.035f),metal,drawerRoot,false);
                Interact(drawer,CampusAction.Drawer,"打開工具抽屜",drawerRoot);
            }
        }
        void BuildFan()
        {
            Primitive(PrimitiveType.Cylinder,"Fan ceiling mount",new Vector3(.4f,3.6f,0),new Vector3(.23f,.12f,.23f),white,null,false);
            Primitive(PrimitiveType.Cylinder,"Fan shaft",new Vector3(.4f,3.34f,0),new Vector3(.05f,.22f,.05f),metal,null,false);
            fanRotor=new GameObject("Fan rotor").transform;fanRotor.SetParent(root);fanRotor.position=new Vector3(.4f,3.11f,0);
            var hub=Primitive(PrimitiveType.Cylinder,"Fan hub",Vector3.zero,new Vector3(.36f,.08f,.36f),white,fanRotor);
            Interact(hub,CampusAction.Fan,"檢查風扇狀態");
            for(int i=0;i<3;i++)
            {
                var pivot=new GameObject("Blade pivot").transform;pivot.SetParent(fanRotor,false);pivot.localRotation=Quaternion.Euler(0,i*120,0);
                Box("Fan blade",new Vector3(.55f,0,0),new Vector3(.92f,.035f,.2f),white,pivot,false);
            }
        }
        void BuildLighting()
        {
            var sun=new GameObject("Hong Kong afternoon sunlight").AddComponent<Light>();sun.transform.SetParent(root);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(26,75,0);sun.intensity=1.35f;sun.color=new Color(1,.83f,.62f);sun.shadows=LightShadows.Soft;sun.shadowStrength=.72f;sun.shadowBias=.035f;
            var emission=Mat("Diffused fluorescent panel",Color.white,.42f);emission.EnableKeyword("_EMISSION");emission.SetColor("_EmissionColor",new Color(.7f,.8f,.83f)*1.3f);
            for(int x=-3;x<=3;x+=6)for(int z=-4;z<=4;z+=4)
            {
                Box("Ceiling fitting",new Vector3(x,3.72f,z),new Vector3(.5f,.12f,1.3f),metal,null,false);
                var panel=Box("Ceiling light",new Vector3(x,3.64f,z),new Vector3(.42f,.035f,1.2f),emission);
                Interact(panel,CampusAction.Light,"檢查燈光狀態");
                lampPanels.Add(panel.GetComponent<Renderer>());
                var go=new GameObject("Soft classroom fill");go.transform.SetParent(root);go.transform.position=new Vector3(x,3.25f,z);
                var light=go.AddComponent<Light>();light.type=LightType.Point;light.range=10;light.intensity=1.1f;light.color=new Color(.92f,.96f,1);classroomLights.Add(light);
            }
        }
        void BuildCity()
        {
            new CampusScenery(root).Build();
        }
        void BuildSoftDetails()
        {
            for(int z=-6;z<6;z+=3)
            {
                CampusGeometry.Tube("Curtain rail",new Vector3(-5.76f,2.91f,z+.16f),new Vector3(-5.76f,2.91f,z+2.84f),.018f,metal,root);
                foreach(float offset in new[]{.38f,2.62f})
                {
                    var curtain=new GameObject("Soft linen window drape");curtain.transform.SetParent(root,false);curtain.transform.localPosition=new Vector3(-5.77f,1.91f,z+offset);curtain.transform.localRotation=Quaternion.Euler(0,90,0);
                    curtain.AddComponent<MeshFilter>().sharedMesh=CampusGeometry.Curtain(.55f,1.94f);curtain.AddComponent<MeshRenderer>().sharedMaterial=linen;
                    curtain.AddComponent<CampusCurtainMotion>();
                }
            }
            Plant(new Vector3(-5.05f,0,5.8f),1);
            Plant(new Vector3(5.1f,0,6.3f),.82f);
            Plant(new Vector3(-5.1f,0,-5.9f),.9f);
            var pot=new GameObject("Desk plant").transform;pot.SetParent(root,false);pot.localPosition=new Vector3(-2.6f,.81f,5.23f);pot.localScale=Vector3.one*.28f;Plant(Vector3.zero,1,pot);
            CampusGeometry.SoftBox("Notice pinboard",new Vector3(5.83f,2.65f,-3.9f),new Vector3(.08f,.68f,1.15f),.06f,oak,root,false);
            Label("工程隊提示\n先觀察，再測試",new Vector3(5.77f,2.65f,-3.9f),.012f,new Color(.96f,.95f,.85f),new Vector3(0,90,0));
            for(int i=0;i<3;i++)
            {
                var card=CampusGeometry.SoftBox("Project inspiration card",new Vector3(5.86f,2.65f,-.9f+i*.78f),new Vector3(.025f,.65f,.56f),.025f,i==1?sage:white,root,false);
                Label(new[]{"提出\n假設","比較\n證據","分享\n發現"}[i],new Vector3(5.82f,2.65f,-.9f+i*.78f),.013f,new Color(.18f,.34f,.32f),new Vector3(0,90,0));
            }
            // A small welcoming reading corner; kept clear of the first-person route.
            CampusGeometry.SoftBox("Reading bench",new Vector3(-4.65f,.4f,4.5f),new Vector3(1.4f,.16f,.65f),.09f,oak,root);
            CampusGeometry.SoftBox("Bench cushion",new Vector3(-4.65f,.51f,4.5f),new Vector3(1.33f,.12f,.6f),.1f,chairPadding,root);
            for(int side=-1;side<=1;side+=2)CampusGeometry.SoftBox("Bench support",new Vector3(-4.65f+side*.52f,.19f,4.5f),new Vector3(.1f,.38f,.5f),.035f,sage,root);
            var rug=Mat("Warm woven rug",new Color(.7f,.62f,.47f),.04f);
            CampusGeometry.SoftBox("Reading corner rug",new Vector3(-4.65f,.008f,4.35f),new Vector3(2.2f,.012f,1.55f),.2f,rug,root,false);
            var shelf=new GameObject("Open classroom bookshelf").transform;shelf.SetParent(root,false);shelf.localPosition=new Vector3(-4.7f,0,6.4f);
            foreach(float h in new[]{.12f,.51f,.94f})CampusGeometry.SoftBox("Rounded bookshelf board",new Vector3(0,h,0),new Vector3(1.65f,.06f,.4f),.04f,oak,shelf);
            foreach(float x in new[]{-.79f,.79f})CampusGeometry.SoftBox("Bookshelf side",new Vector3(x,.52f,0),new Vector3(.06f,.95f,.4f),.035f,oak,shelf);
            var peach=Mat("Peach paper",new Color(.86f,.65f,.48f),.12f);var blue=Mat("Sky paper",new Color(.56f,.7f,.75f),.1f);
            for(int i=0;i<15;i++)
            {
                int row=i/8,col=i%8;float h=.23f+(i%3)*.025f;
                var book=CampusGeometry.SoftBox("Classroom library book",new Vector3(-.67f+col*.17f,.17f+row*.39f+h*.5f,0),new Vector3(.11f,h,.25f),.009f,new[]{sage,blue,peach,white}[i%4],shelf,false);
                book.transform.localRotation=Quaternion.Euler(0,0,(i%5==0)?-8:0);
            }
            CampusGeometry.SoftBox("Discovery pinboard frame",new Vector3(4.25f,2.32f,6.8f),new Vector3(1.8f,1.4f,.1f),.07f,oak,root,false);
            CampusGeometry.SoftBox("Discovery cork",new Vector3(4.25f,2.32f,6.73f),new Vector3(1.67f,1.27f,.035f),.04f,rug,root,false);
            Label("我們的發現",new Vector3(4.25f,2.79f,6.69f),.028f,Color.white,Vector3.zero);
            for(int i=0;i<3;i++)
            {
                var paper=CampusGeometry.SoftBox("Pinned idea note",new Vector3(3.75f+i*.5f,2.27f,6.69f),new Vector3(.4f,.55f,.012f),.018f,new[]{white,sage,peach}[i],root,false);
                paper.transform.localRotation=Quaternion.Euler(0,0,(i-1)*5);
                Label(new[]{"想一想\n為甚麼？","試一試\n有證據嗎？","分享\n新發現"}[i],new Vector3(3.75f+i*.5f,2.27f,6.674f),.012f,new Color(.17f,.32f,.3f),Vector3.zero);
                Primitive(PrimitiveType.Sphere,"Notice pin",new Vector3(3.75f+i*.5f,2.49f,6.67f),Vector3.one*.035f,navy,null,false);
            }
        }
        void Plant(Vector3 position,float scale,Transform parent=null)
        {
            var plant=new GameObject("Living classroom plant").transform;plant.SetParent(parent??root,false);plant.localPosition=position;plant.localScale=Vector3.one*scale;
            var clay=Mat("Matte warm ceramic",new Color(.7f,.53f,.4f),.2f);var leaf=Mat("Leaf green",new Color(.24f,.42f,.29f),.28f);var leafLight=Mat("Fresh leaf green",new Color(.36f,.52f,.34f),.3f);
            var planter=Primitive(PrimitiveType.Cylinder,"Rounded ceramic pot",new Vector3(0,.16f,0),new Vector3(.4f,.16f,.4f),clay,plant,false);
            Primitive(PrimitiveType.Cylinder,"Dark soil",new Vector3(0,.323f,0),new Vector3(.35f,.006f,.35f),Mat("Plant soil",new Color(.16f,.13f,.1f),.05f),plant,false);
            CampusGeometry.Tube("Plant stem",new Vector3(0,.32f,0),new Vector3(.015f,1.13f,.01f),.012f,leaf,plant);
            for(int i=0;i<9;i++)
            {
                float angle=i*137.5f*Mathf.Deg2Rad,h=.42f+i*.075f;
                Vector3 start=new Vector3(0,h,0),end=start+new Vector3(Mathf.Cos(angle)*.18f,.08f,Mathf.Sin(angle)*.18f);
                CampusGeometry.Tube("Leaf petiole",start,end,.004f,leaf,plant);
                var blade=new GameObject("Natural curved leaf");blade.transform.SetParent(plant,false);blade.transform.localPosition=end;
                blade.transform.localRotation=Quaternion.LookRotation(new Vector3(Mathf.Cos(angle),.36f,Mathf.Sin(angle)));
                blade.AddComponent<MeshFilter>().sharedMesh=CampusGeometry.Leaf(.17f,.3f-i*.008f);blade.AddComponent<MeshRenderer>().sharedMaterial=i%3==0?leafLight:leaf;
            }
        }
        void BuildRobot()
        {
            robot=new GameObject("小芯 / Teaching companion").transform;robot.SetParent(root);robot.position=new Vector3(2.65f,1.3f,4.95f);
            Primitive(PrimitiveType.Sphere,"Companion shell",Vector3.zero,new Vector3(.59f,.5f,.46f),white,robot,false);
            var face=CampusGeometry.SoftBox("Rounded face display",new Vector3(0,.025f,-.217f),new Vector3(.43f,.28f,.07f),.075f,navy,robot);
            Interact(face,CampusAction.Robot,"與小芯討論  預設教學提示");
            var eyeMat=Mat("Gentle mint display",new Color(.48f,.9f,.76f),.5f);eyeMat.EnableKeyword("_EMISSION");eyeMat.SetColor("_EmissionColor",new Color(.22f,.55f,.4f));
            for(int i=-1;i<=1;i+=2)
            {
                Primitive(PrimitiveType.Sphere,"Expressive eye",new Vector3(i*.105f,.055f,-.258f),new Vector3(.055f,.082f,.02f),eyeMat,robot,false);
                var arm=Primitive(PrimitiveType.Capsule,"Companion rounded arm",new Vector3(i*.315f,-.04f,0),new Vector3(.07f,.11f,.075f),white,robot,false);arm.transform.localRotation=Quaternion.Euler(0,0,i*28);
                Primitive(PrimitiveType.Sphere,"Ear accent",new Vector3(i*.26f,.12f,0),new Vector3(.08f,.14f,.14f),sage,robot,false);
            }
            Vector3 previous=new Vector3(-.055f,-.04f,-.259f);
            for(int i=1;i<=10;i++){float x=-.055f+i*.011f;Vector3 next=new Vector3(x,-.063f+Mathf.Pow(x/.055f,2)*.023f,-.259f);CampusGeometry.Tube("Friendly smile",previous,next,.005f,eyeMat,robot);previous=next;}
            CampusGeometry.Tube("Antenna",new Vector3(0,.22f,0),new Vector3(0,.32f,.01f),.012f,sage,robot);
            Primitive(PrimitiveType.Sphere,"Antenna light",new Vector3(0,.33f,.01f),Vector3.one*.05f,eyeMat,robot,false);
            Primitive(PrimitiveType.Sphere,"Hover base",new Vector3(0,-.26f,0),new Vector3(.29f,.07f,.29f),sage,robot,false);
            robot.gameObject.AddComponent<CompanionMotion>().target=game.player;
            robot.gameObject.AddComponent<CompanionExpression>();
        }
        void BuildMentor()
        {
            CampusMascot.Create(root,new Vector3(3.7f,0,5.45f),game.player);
        }
        GameObject SchoolProp(string name,Vector3 position,float height,float angle=0)
        {
            var prefab=Resources.Load<GameObject>("SchoolAssets/Prefabs/props/"+name);if(prefab==null)return null;
            var go=UnityEngine.Object.Instantiate(prefab,root);go.name="School asset / "+name;go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.Euler(0,angle,0);
            var renderers=go.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return go;
            Bounds b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            if(b.size.y>.001f)go.transform.localScale*=height/b.size.y;
            b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            go.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);
            foreach(var c in go.GetComponentsInChildren<Collider>())UnityEngine.Object.Destroy(c);
            return go;
        }
        void BuildSchoolProps()
        {
            // Licensed Asset Store props supplement the original classroom; the walking aisle stays clear.
            for(int i=0;i<4;i++)SchoolProp("locker_1",new Vector3(5.48f,0,-5.55f+i*.7f),1.35f,-90);
            SchoolProp("projector",new Vector3(-1,3.35f,2.8f),.18f,180);
            SchoolProp("computer1",new Vector3(-2.65f,.81f,5.35f),.4f,180);
            SchoolProp("rack1",new Vector3(-5.22f,0,-4.6f),1.1f,90);
            SchoolProp("a laptop",new Vector3(.45f,.81f,.4f),.25f,165);
            for(int i=0;i<4;i++)SchoolProp("book"+(i+1),new Vector3(1.7f,.81f,-3.9f+i*2.05f),.035f,12+i*8);
            SchoolProp("speaker",new Vector3(-4.8f,3.05f,6.78f),.27f,180);
            SchoolProp("speaker",new Vector3(4.8f,3.05f,6.78f),.27f,180);
            Label("課後科學社",new Vector3(-3.7f,2.85f,-6.84f),.03f,new Color(.19f,.35f,.33f),new Vector3(0,180,0));
        }
    }

    public class CampusCurtainMotion:MonoBehaviour
    {
        Mesh mesh;Vector3[] rest,moving;float phase;
        void Start(){mesh=GetComponent<MeshFilter>().mesh;rest=mesh.vertices;moving=new Vector3[rest.Length];phase=transform.position.z*.7f;}
        void Update(){for(int i=0;i<rest.Length;i++){var v=rest[i];float freedom=Mathf.Clamp01((.97f-v.y)/1.94f);moving[i]=v+Vector3.forward*(Mathf.Sin(Time.time*.7f+phase+v.x*3)*.012f*freedom);}mesh.vertices=moving;}
    }

    // Dynamic OS fonts can repack their shared atlas after UI glyphs are requested.
    // Refresh TextMesh geometry on the next frame, outside the font rebuild callback.
    public class CampusWorldText:MonoBehaviour
    {
        TextMesh label;bool refresh=true;
        public string source;int languageRevision=-1;float baseSize;
        void OnEnable(){label=GetComponent<TextMesh>();baseSize=label.characterSize;Font.textureRebuilt+=OnFontRebuilt;}
        void OnDisable(){Font.textureRebuilt-=OnFontRebuilt;}
        void OnFontRebuilt(Font font){if(label!=null && font==label.font)refresh=true;}
        void LateUpdate()
        {
            if(languageRevision!=CampusLocalization.Revision && label!=null)
            {
                languageRevision=CampusLocalization.Revision;
                if(source!=null)
                {
                    label.text=CampusLocalization.T(source);int original=1,translated=1;
                    foreach(var line in source.Split('\n'))original=Mathf.Max(original,line.Length);
                    foreach(var line in label.text.Split('\n'))translated=Mathf.Max(translated,line.Length);
                    label.characterSize=baseSize*(CampusLocalization.Language==2?Mathf.Min(1,original*1.45f/translated):1);
                }
                refresh=true;
            }
            if(!refresh || label==null || label.font==null)return;
            refresh=false;string value=label.text;
            label.font.RequestCharactersInTexture(value,label.fontSize,label.fontStyle);
            label.text="";label.text=value;GetComponent<Renderer>().sharedMaterial.mainTexture=label.font.material.mainTexture;
        }
    }

    public class CompanionMotion:MonoBehaviour
    {
        public Transform target;Vector3 home;float phase;
        void Start(){home=transform.position;phase=UnityEngine.Random.value*6;}
        void Update(){transform.position=home+Vector3.up*Mathf.Sin(Time.time*1.35f+phase)*.035f;if(target!=null){var direction=target.position-transform.position;direction.y=0;if(direction.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(-direction)*Quaternion.Euler(Mathf.Sin(Time.time*.7f)*2,0,Mathf.Sin(Time.time*.55f)*2),Time.deltaTime*2);}}
    }
    public class CompanionExpression:MonoBehaviour
    {
        readonly List<Transform> eyes=new List<Transform>();
        void Start(){foreach(var t in GetComponentsInChildren<Transform>())if(t.name=="Expressive eye")eyes.Add(t);}
        void Update(){float t=Time.time%5.3f,blink=t<.2f?1-Mathf.Sin(t/.2f*Mathf.PI)*.88f:1;foreach(var eye in eyes)eye.localScale=new Vector3(.055f,.082f*blink,.02f);}
    }
    public class MentorMotion:MonoBehaviour
    {
        public Transform target;
        Transform head,rightArm,leftArm,rightForearm,leftForearm,spine,pelvis;
        Quaternion original,rightRest,leftRest,rightForeRest,leftForeRest,spineRest,pelvisRest;Vector3 hipPosition;float start;
        readonly List<SkinnedMeshRenderer> faces=new List<SkinnedMeshRenderer>();
        readonly List<int> blinkIndices=new List<int>();
        void Start()
        {
            foreach(var t in GetComponentsInChildren<Transform>())
            {
                string n=t.name.ToLowerInvariant();
                if(head==null && n.EndsWith(" head"))head=t;
                if(n.EndsWith("r upperarm"))rightArm=t;
                if(n.EndsWith("l upperarm"))leftArm=t;
                if(n.EndsWith("r forearm"))rightForearm=t;
                if(n.EndsWith("l forearm"))leftForearm=t;
                if(spine==null && n.EndsWith(" spine"))spine=t;
                if(n.EndsWith(" pelvis"))pelvis=t;
            }
            if(head!=null)original=head.localRotation;
            RelaxArm(rightArm,.12f);RelaxArm(leftArm,-.12f);
            RelaxArm(rightForearm,.16f);RelaxArm(leftForearm,-.16f);
            if(rightArm!=null)rightRest=rightArm.localRotation;if(leftArm!=null)leftRest=leftArm.localRotation;
            if(rightForearm!=null)rightForeRest=rightForearm.localRotation;if(leftForearm!=null)leftForeRest=leftForearm.localRotation;
            if(spine!=null)spineRest=spine.localRotation;if(pelvis!=null){pelvisRest=pelvis.localRotation;hipPosition=pelvis.localPosition;}
            foreach(var mesh in GetComponentsInChildren<SkinnedMeshRenderer>())
                if(mesh.sharedMesh!=null)for(int i=0;i<mesh.sharedMesh.blendShapeCount;i++)
                {string n=mesh.sharedMesh.GetBlendShapeName(i).ToLowerInvariant();if(n.Contains("blink") || n.Contains("au45")){faces.Add(mesh);blinkIndices.Add(i);}}
            start=Time.time;
        }
        void RelaxArm(Transform arm,float side)
        {
            if(arm==null || arm.childCount==0)return;
            Vector3 direction=arm.GetChild(0).position-arm.position;
            if(direction.sqrMagnitude>.0001f)arm.rotation=Quaternion.FromToRotation(direction.normalized,transform.TransformDirection(new Vector3(side,-1,.08f)).normalized)*arm.rotation;
        }
        void LateUpdate()
        {
            float t=Time.time-start;
            float attention=0;if(target!=null){var d=transform.InverseTransformDirection(target.position-transform.position);attention=Mathf.Clamp(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,-18,18);}
            float gestureTime=t%18,gesture=gestureTime>4 && gestureTime<7?Mathf.Sin((gestureTime-4)/3*Mathf.PI):0;
            if(head!=null)head.localRotation=Quaternion.Slerp(head.localRotation,original*Quaternion.Euler(Mathf.Sin(t*.8f)*1.1f,attention*.55f+Mathf.Sin(t*.37f)*2,Mathf.Sin(t*.43f)*1.1f),Time.deltaTime*2);
            if(spine!=null)spine.localRotation=spineRest*Quaternion.Euler(Mathf.Sin(t*1.35f)*.65f,Mathf.Sin(t*.3f)*1.1f,Mathf.Sin(t*.4f)*.6f);
            if(pelvis!=null){pelvis.localRotation=pelvisRest*Quaternion.Euler(0,Mathf.Sin(t*.24f)*.7f,Mathf.Sin(t*.4f)*.5f);pelvis.localPosition=hipPosition+Vector3.up*(Mathf.Sin(t*1.35f)*.0004f);}
            if(rightArm!=null)rightArm.localRotation=rightRest*Quaternion.Euler(Mathf.Sin(t*.8f)*1.7f,gesture*9,Mathf.Sin(t*.5f)*1.2f+gesture*13);
            if(leftArm!=null)leftArm.localRotation=leftRest*Quaternion.Euler(Mathf.Sin(t*.7f)*1.4f,0,-Mathf.Sin(t*.5f)*1.1f);
            if(rightForearm!=null)rightForearm.localRotation=rightForeRest*Quaternion.Euler(gesture*18,0,Mathf.Sin(t*.6f)*1.5f);
            if(leftForearm!=null)leftForearm.localRotation=leftForeRest*Quaternion.Euler(Mathf.Sin(t*.5f)*1.5f,0,0);
            float blink=(t%4.7f)<.16f?Mathf.Sin((t%4.7f)/.16f*Mathf.PI)*100:0;
            for(int i=0;i<faces.Count;i++)faces[i].SetBlendShapeWeight(blinkIndices[i],blink);
        }
    }
}
