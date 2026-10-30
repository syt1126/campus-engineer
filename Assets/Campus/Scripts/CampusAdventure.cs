using System;
using System.Collections;
using UnityEngine;

namespace CampusEngineer
{
    public sealed class CampusAdventure:MonoBehaviour
    {
        public CampusGame game;
        public Transform plane,wateringCan,flower;
        public Transform[] bars=new Transform[8];
        public bool PlaneFlying {get;private set;}
        public bool Watering {get;private set;}
        public int NotesPlayed {get;private set;}
        public int Completed {get{return game.save.discoveries==null?0:game.save.discoveries.Count;}}
        public void Remember(string id)
        {
            if(game.save.discoveries==null)game.save.discoveries=new System.Collections.Generic.List<string>();
            if(game.save.discoveries.Contains(id))return;
            game.save.discoveries.Add(id);game.Persist();game.Audio.Success();
        }
        public void Play(string id,Transform source)
        {
            if(id=="plane")
            {if(!PlaneFlying)StartCoroutine(Fly());}
            else if(id=="water")
            {if(!Watering)StartCoroutine(Water());}
            else if(id.StartsWith("note"))
            {
                int n=int.Parse(id.Substring(4));game.Audio.Note(n);StartCoroutine(Bounce(bars[n]));
                NotesPlayed|=1<<n;Spark(source.position,new Color(.94f,.61f,.32f),6);
                if((NotesPlayed&(NotesPlayed-1))!=0 && BitCount(NotesPlayed)>=4){Remember("music");game.Notify("你的旋律被記住了。試試用不同次序，創作自己的下課鈴！",5);}
                else game.Notify("叮！每根琴鍵都有不同的聲音。試著彈出自己的旋律。",3);
            }
            else if(id=="bell")
            {game.Audio.Note(7);game.Audio.Chirp();StartCoroutine(Swing(source));Spark(source.position,new Color(.9f,.75f,.4f),9);game.Notify("風鈴響了，窗外的小鳥也回應了你。",4);Remember("chime");}
            else if(id.StartsWith("star"))
            {Remember(id);Spark(source.position,new Color(1,.67f,.24f),16);source.gameObject.SetActive(false);game.Notify("找到一顆紙星星！它會留在你的發現手帳裡。",4);}
        }
        static int BitCount(int n){int c=0;while(n!=0){c+=n&1;n>>=1;}return c;}
        IEnumerator Bounce(Transform t)
        {var p=t.localPosition;for(float f=0;f<.32f;f+=Time.deltaTime){t.localPosition=p+Vector3.down*Mathf.Sin(f/.32f*Mathf.PI)*.035f;yield return null;}t.localPosition=p;}
        IEnumerator Swing(Transform t)
        {var q=t.localRotation;for(float f=0;f<2;f+=Time.deltaTime){t.localRotation=q*Quaternion.Euler(Mathf.Sin(f*15)*18*Mathf.Exp(-f*2),0,0);yield return null;}t.localRotation=q;}
        IEnumerator Fly()
        {
            PlaneFlying=true;var p=plane.position;var q=plane.rotation;var scale=plane.localScale;
            game.Notify("紙飛機出發！它會繞過桌椅，再飛回你的窗邊。",6);game.Audio.Whoosh();
            // A continuous closed Bezier flight, starting and ending at its real resting position.
            for(float t=0;t<1;t+=Time.deltaTime/5f)
            {
                Vector3 next=FlightPoint(p,Mathf.Min(1,t+.006f));Vector3 current=FlightPoint(p,t);
                plane.position=current;if((next-current).sqrMagnitude>.00001f)plane.rotation=Quaternion.LookRotation(next-current)*Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI*4)*18);
                yield return null;
            }
            plane.position=p;plane.rotation=q;plane.localScale=scale;PlaneFlying=false;Remember("plane");Spark(p,new Color(.9f,.75f,.4f),14);
            game.Notify("完美降落！紙飛機飛行章已加入發現手帳。",4);
        }
        Vector3 FlightPoint(Vector3 p,float t)
        {
            Vector3 a=p,b=new Vector3(-3.5f,2.7f,-3.8f),c=new Vector3(3.4f,2.7f,1.7f),d=p;
            float s=1-t;return s*s*s*a+3*s*s*t*b+3*s*t*t*c+t*t*t*d+Vector3.up*Mathf.Sin(t*Mathf.PI)*.7f;
        }
        IEnumerator Water()
        {
            Watering=true;var p=wateringCan.position;var q=wateringCan.rotation;var grown=flower.localScale;
            game.Notify("慢慢倒一點水……看看花苞有甚麼變化。",6);game.Audio.Water();
            for(float t=0;t<2.6f;t+=Time.deltaTime)
            {
                float k=Mathf.SmoothStep(0,1,Mathf.Min(t/.65f,(2.6f-t)/.6f));wateringCan.position=p+new Vector3(-.20f,.65f,.50f)*k;wateringCan.rotation=q*Quaternion.Euler(0,0,-32*k);
                if(t>.55f && t<2 && UnityEngine.Random.value<.7f)StartCoroutine(Drop(wateringCan.position+new Vector3(.30f,.06f,0),flower.position-Vector3.up*.30f));
                if(t>1)flower.localScale=Vector3.Lerp(grown,Vector3.one,Mathf.SmoothStep(0,1,(t-1)/1.2f));
                yield return null;
            }
            flower.localScale=Vector3.one;wateringCan.position=p;wateringCan.rotation=q;Watering=false;Remember("garden");Spark(flower.position+Vector3.up*.3f,new Color(.95f,.53f,.48f),18);
            game.Notify("花開了！窗邊多了一點顏色，謝謝你的照顧。",5);
        }
        IEnumerator Drop(Vector3 from,Vector3 to)
        {
            var drop=CampusAdventureBuilder.Ellipsoid("Water droplet",transform,from,new Vector3(.018f,.045f,.018f),CampusAdventureBuilder.Material("Clear water",new Color(.3f,.7f,.8f)));
            drop.transform.position=from;for(float t=0;t<1;t+=Time.deltaTime*2.4f){drop.transform.position=Vector3.Lerp(from,to,t);yield return null;}Destroy(drop.GetComponent<Renderer>().sharedMaterial);Destroy(drop);
        }
        public void Spark(Vector3 p,Color color,int amount)
        {StartCoroutine(Sparks(p,color,amount));}
        IEnumerator Sparks(Vector3 p,Color color,int count)
        {
            var mat=CampusAdventureBuilder.Material("Discovery sparkle",color);var objects=new Transform[count];var velocity=new Vector3[count];
            for(int i=0;i<count;i++){var g=CampusAdventureBuilder.Ellipsoid("Discovery sparkle",transform,Vector3.zero,Vector3.one*.035f,mat);objects[i]=g.transform;objects[i].position=p;velocity[i]=UnityEngine.Random.onUnitSphere*.6f+Vector3.up*.6f;}
            for(float t=0;t<1.1f;t+=Time.deltaTime){for(int i=0;i<count;i++){objects[i].position+=velocity[i]*Time.deltaTime;velocity[i]+=Vector3.down*.6f*Time.deltaTime;objects[i].localScale=Vector3.one*.035f*(1-t/1.1f);}yield return null;}
            foreach(var o in objects)Destroy(o.gameObject);Destroy(mat);
        }
    }

    public sealed class CampusAdventureBuilder
    {
        readonly CampusGame game;readonly Transform root;CampusAdventure play;
        public CampusAdventureBuilder(CampusGame game,Transform root){this.game=game;this.root=root;}
        public static Material Material(string name,Color color)
        {var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Glossiness",.2f);return m;}
        public static GameObject Ellipsoid(string name,Transform parent,Vector3 p,Vector3 scale,Material m)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.Destroy(g.GetComponent<Collider>());return g;}
        Transform Group(string name,Vector3 position){var t=new GameObject(name).transform;t.SetParent(root,false);t.localPosition=position;return t;}
        void Interaction(GameObject g,string id,string text,Vector3 size)
        {var c=g.AddComponent<BoxCollider>();c.size=size;var i=g.AddComponent<CampusInteractable>();i.action=CampusAction.Play;i.playId=id;i.title=text;}
        public void Build()
        {
            play=game.gameObject.AddComponent<CampusAdventure>();play.game=game;
            Plane();Music();Garden();Chime();Stars();Atmosphere();
            var mural=Resources.Load<Texture2D>("CampusGardenMural");
            if(mural!=null)
            {
                var frame=Material("Campus mural / honey oak frame",new Color(.58f,.38f,.21f));
                CampusGeometry.SoftBox("Original illustrated campus mural frame",new Vector3(-.7f,2.3f,-6.84f),new Vector3(3.9f,2.25f,.08f),.06f,frame,root,false);
                var art=GameObject.CreatePrimitive(PrimitiveType.Quad);art.name="Original campus garden illustration";art.transform.SetParent(root,false);art.transform.localPosition=new Vector3(-.7f,2.3f,-6.78f);art.transform.localRotation=Quaternion.Euler(0,180,0);art.transform.localScale=new Vector3(3.72f,2.09f,1);
                var muralShader=Resources.Load<Shader>("CampusMural");var m=new Material(muralShader!=null?muralShader:Shader.Find("Standard")){mainTexture=mural};art.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.Destroy(art.GetComponent<Collider>());
            }
            // Rejected primitive hand removed. Only a properly skinned hand may replace it.
        }
        void Plane()
        {
            var t=Group("Folded paper aeroplane",new Vector3(-3.8f,.85f,.2f));play.plane=t;
            var m=Material("Warm blue origami paper",new Color(.38f,.68f,.75f));
            var mesh=new Mesh{name="Original folded paper plane"};mesh.vertices=new[]{new Vector3(0,0,.36f),new Vector3(-.34f,0,-.23f),new Vector3(-.035f,.09f,-.13f),new Vector3(.035f,.09f,-.13f),new Vector3(.34f,0,-.23f),new Vector3(0,-.045f,-.18f)};
            int[] faces={0,1,2,0,2,5,0,5,3,0,3,4,2,1,0,5,2,0,3,5,0,4,3,0};var original=mesh.vertices;var folded=new Vector3[faces.Length];var indices=new int[faces.Length];
            for(int i=0;i<faces.Length;i++){folded[i]=original[faces[i]];indices[i]=i;}mesh.Clear();mesh.vertices=folded;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;Interaction(t.gameObject,"plane","放飛紙飛機",new Vector3(.7f,.18f,.65f));
        }
        void Music()
        {
            var t=Group("Playable classroom xylophone",new Vector3(-4.7f,.62f,4.5f));
            var wood=Material("Music box / honey wood",new Color(.54f,.32f,.16f));
            CampusGeometry.SoftBox("Xylophone resonator",Vector3.zero,new Vector3(1.05f,.12f,.55f),.045f,wood,t,false);
            Color[] colors={new Color(.76f,.33f,.28f),new Color(.88f,.49f,.29f),new Color(.9f,.7f,.30f),new Color(.48f,.66f,.37f),new Color(.28f,.6f,.56f),new Color(.31f,.55f,.7f),new Color(.43f,.44f,.65f),new Color(.66f,.43f,.61f)};
            for(int i=0;i<8;i++)
            {
                var bar=CampusGeometry.SoftBox("Playable note "+i,new Vector3(-.43f+i*.123f,.095f,0),new Vector3(.10f,.05f,.48f-i*.025f),.018f,Material("Music key "+i,colors[i]),t,false);
                play.bars[i]=bar.transform;Interaction(bar,"note"+i,"彈奏彩色木琴",new Vector3(.115f,.11f,.5f));
                for(int s=-1;s<=1;s+=2)Ellipsoid("Key mounting pin",bar.transform,new Vector3(0,.03f,s*(.17f-i*.009f)),Vector3.one*.018f,wood);
            }
            CampusGeometry.Tube("Xylophone mallet",new Vector3(-.39f,.13f,.29f),new Vector3(.20f,.13f,.36f),.012f,wood,t);
            Ellipsoid("Soft mallet head",t,new Vector3(.22f,.13f,.36f),Vector3.one*.075f,Material("Mallet felt",new Color(.78f,.69f,.51f)));
        }
        void Garden()
        {
            var t=Group("Window flower to care for",new Vector3(-5.45f,1.06f,1.55f));
            var clay=Material("Garden / rose ceramic",new Color(.69f,.38f,.3f));var leaf=Material("Garden / jade leaf",new Color(.23f,.46f,.28f));
            Ellipsoid("Rounded window planter",t,new Vector3(0,.1f,0),new Vector3(.32f,.24f,.32f),clay);
            CampusGeometry.Tube("Flower stem",new Vector3(0,.15f,0),new Vector3(0,.52f,0),.012f,leaf,t);
            play.flower=new GameObject("Blooming bauhinia flower").transform;play.flower.SetParent(t,false);play.flower.localPosition=new Vector3(0,.5f,0);play.flower.localRotation=Quaternion.Euler(0,90,0);play.flower.localScale=Vector3.one*.24f;
            var petal=Material("Bauhinia / coral pink",new Color(.85f,.36f,.48f));
            for(int k=0;k<5;k++){float a=k*Mathf.PI*2/5;var p=Ellipsoid("Soft bauhinia petal",play.flower,new Vector3(Mathf.Sin(a)*.11f,Mathf.Cos(a)*.11f,0),new Vector3(.13f,.19f,.045f),petal);p.transform.localRotation=Quaternion.Euler(-12,0,-k*72);}
            Ellipsoid("Golden flower heart",play.flower,new Vector3(0,0,-.03f),Vector3.one*.07f,Material("Pollen gold",new Color(.97f,.68f,.2f)));
            var can=Group("Animated watering can",new Vector3(-5.55f,1.24f,1.02f));play.wateringCan=can;
            var sage=Material("Watering can / pistachio",new Color(.43f,.65f,.49f));
            Ellipsoid("Watering vessel",can,Vector3.zero,new Vector3(.23f,.22f,.22f),sage);
            CampusGeometry.Tube("Watering spout",new Vector3(.08f,0,0),new Vector3(.29f,.12f,0),.025f,sage,can);
            for(int k=0;k<14;k++){float a=k/13f*Mathf.PI*1.5f;float b=(k+1)/13f*Mathf.PI*1.5f;CampusGeometry.Tube("Curved watering handle",new Vector3(-.15f-Mathf.Sin(a)*.09f,Mathf.Cos(a)*.09f,0),new Vector3(-.15f-Mathf.Sin(b)*.09f,Mathf.Cos(b)*.09f,0),.012f,sage,can);}
            Interaction(can.gameObject,"water","給窗邊的花澆水",new Vector3(.55f,.32f,.35f));
        }
        void Chime()
        {
            var t=Group("Swaying window wind chime",new Vector3(-5.6f,2.5f,-2.2f));var brass=Material("Wind chime / brushed brass",new Color(.79f,.57f,.28f));
            Ellipsoid("Wind chime dome",t,Vector3.zero,new Vector3(.18f,.12f,.18f),brass);
            for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;CampusGeometry.Tube("Wind chime tube",new Vector3(Mathf.Cos(a)*.07f,-.09f,Mathf.Sin(a)*.07f),new Vector3(Mathf.Cos(a)*.07f,-.33f-i*.025f,Mathf.Sin(a)*.07f),.01f,brass,t);}
            CampusGeometry.Tube("Chime cord",Vector3.zero,new Vector3(0,-.42f,0),.004f,brass,t);
            CampusGeometry.SoftBox("Bauhinia wish tag",new Vector3(0,-.49f,0),new Vector3(.1f,.16f,.01f),.01f,Material("Wish tag",new Color(.38f,.65f,.62f)),t,false);
            Interaction(t.gameObject,"bell","輕碰窗邊風鈴",new Vector3(.25f,1.1f,.25f));
        }
        void Stars()
        {
            Vector3[] positions={new Vector3(-4.3f,1.03f,6.35f),new Vector3(5.42f,1.46f,-4.85f),new Vector3(2.1f,.86f,-1.85f)};
            var mat=Material("Hidden origami / gold",new Color(.9f,.62f,.24f));
            for(int i=0;i<3;i++)
            {
                var t=Group("Hidden paper star "+i,positions[i]);
                for(int k=0;k<5;k++){var point=Ellipsoid("Folded star point",t,new Vector3(Mathf.Sin(k*Mathf.PI*.4f)*.05f,Mathf.Cos(k*Mathf.PI*.4f)*.05f,0),new Vector3(.045f,.12f,.025f),mat);point.transform.localRotation=Quaternion.Euler(0,0,-k*72);}
                Interaction(t.gameObject,"star"+i,"收集藏起來的紙星星",Vector3.one*.28f);t.gameObject.AddComponent<CampusFloatingKeepsake>();
            }
        }
        void Atmosphere()
        {
            var peach=Material("Bauhinia canopy / rose",new Color(.8f,.43f,.47f));var pale=Material("Bauhinia canopy / warm blush",new Color(.92f,.62f,.59f));
            for(int tree=0;tree<4;tree++)for(int k=0;k<13;k++)
            {
                float a=k*2.4f;var pos=new Vector3(-12-Mathf.Cos(a)*1.2f,3.2f+Mathf.Sin(k*3)*.6f,-9+tree*7+Mathf.Sin(a)*1.2f);
                var blossoms=Ellipsoid("Bauhinia blossom canopy",root,pos,new Vector3(1.35f,1.05f,1.3f),k%3==0?pale:peach);
                blossoms.GetComponent<MeshFilter>().sharedMesh=CampusScenery.Foliage(tree*29+k);
            }
            var motion=Group("Drifting bauhinia petals",Vector3.zero);motion.gameObject.AddComponent<CampusPetalDrift>();
            var art=Material("Classroom celebration / peach",new Color(.78f,.45f,.35f));var cream=Material("Classroom celebration / linen",new Color(.83f,.75f,.58f));
            for(int i=0;i<9;i++)
            {
                float x=-4.6f+i*.9f,y=3.35f-Mathf.Sin(i/8f*Mathf.PI)*.26f;
                CampusGeometry.Tube("Celebration string",new Vector3(x,y,6.55f),new Vector3(x+.9f,3.35f-Mathf.Sin((i+1)/8f*Mathf.PI)*.26f,6.55f),.006f,cream,root);
                var flag=CampusGeometry.SoftBox("Fabric celebration pennant",new Vector3(x,y-.14f,6.55f),new Vector3(.22f,.28f,.008f),.025f,i%2==0?art:cream,root,false);flag.transform.localRotation=Quaternion.Euler(0,0,(i%3-1)*8);
            }
        }
    }
    public sealed class CampusFloatingKeepsake:MonoBehaviour
    {Vector3 p;void Start(){p=transform.localPosition;}void Update(){transform.localPosition=p+Vector3.up*Mathf.Sin(Time.time*1.5f+p.x)*.025f;transform.Rotate(0,Time.deltaTime*18,0);}}
    public sealed class CampusPetalDrift:MonoBehaviour
    {
        Transform[] petals=new Transform[32];
        void Start(){var m=CampusAdventureBuilder.Material("Drifting bauhinia / blush",new Color(.92f,.57f,.57f));for(int i=0;i<petals.Length;i++){petals[i]=CampusAdventureBuilder.Ellipsoid("Drifting petal",transform,new Vector3(-7.5f-i%6*.75f,.5f+i*.13f,-10+i*.65f),new Vector3(.06f,.018f,.035f),m).transform;}}
        void Update(){for(int i=0;i<petals.Length;i++){var t=petals[i];t.localPosition+=new Vector3(Mathf.Sin(Time.time*.5f+i)*.025f,-.10f,.12f)*Time.deltaTime;t.Rotate(18*Time.deltaTime,23*Time.deltaTime,0);if(t.localPosition.y<-.1f)t.localPosition=new Vector3(-7.5f-i%6*.75f,4.5f,-10+i*.65f);}}
    }
    public sealed class CampusHand:MonoBehaviour
    {
        Transform hand;float action=-10;
        public void Reach(){action=Time.time;}
        void Start()
        {
            hand=new GameObject("First person reach / canvas glove").transform;hand.SetParent(transform,false);
            var sleeve=CampusAdventureBuilder.Material("Player / jade school sleeve",new Color(.16f,.4f,.36f));var glove=CampusAdventureBuilder.Material("Player / warm canvas glove",new Color(.65f,.49f,.32f));
            CampusAdventureBuilder.Ellipsoid("Sleeve",hand,new Vector3(.025f,-.12f,0),new Vector3(.12f,.27f,.13f),sleeve);
            CampusAdventureBuilder.Ellipsoid("Palm",hand,Vector3.zero,new Vector3(.115f,.15f,.055f),glove);
            for(int i=0;i<4;i++)CampusAdventureBuilder.Ellipsoid("Gloved finger",hand,new Vector3(-.041f+i*.027f,.09f,0),new Vector3(.025f,.105f-i*.005f,.035f),glove);
            var thumb=CampusAdventureBuilder.Ellipsoid("Gloved thumb",hand,new Vector3(-.07f,.01f,0),new Vector3(.035f,.095f,.04f),glove);thumb.transform.localRotation=Quaternion.Euler(0,0,-33);
        }
        void LateUpdate(){float t=Time.time-action;bool visible=t>=0 && t<.8f;hand.gameObject.SetActive(visible);if(!visible)return;float k=Mathf.Sin(t/.8f*Mathf.PI);hand.localPosition=Vector3.Lerp(new Vector3(.37f,-.65f,.48f),new Vector3(.18f,-.17f,.65f),k);hand.localRotation=Quaternion.Euler(-15,0,-12-k*18);}
    }
}
