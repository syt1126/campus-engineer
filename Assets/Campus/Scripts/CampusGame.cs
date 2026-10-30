using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CampusEngineer
{
    [Serializable] public class CampusSave
    {
        public string version="0.3.0";
        public List<string> discoveries=new List<string>();
        public int phase=1;
        public bool tablet,temperature,occupancy,lux,firstDone,secondDone,counterexample,completed;
        public ControlRules rules=new ControlRules{requireOccupied=false};
        public List<string> history=new List<string>();
        public List<ExperimentResult> experiments=new List<ExperimentResult>();
    }

    public class CampusGame:MonoBehaviour
    {
        public Transform player;
        public Camera view;
        public CampusSave save=new CampusSave();
        CampusWorld world;
        CharacterController controller;
        CampusInteractable lookedAt;
        Font uiFont;
        GUIStyle titleStyle,subtitleStyle,bodyStyle,smallStyle,buttonStyle,codeStyle;
        string panel="intro",notice="",conversation="";
        float noticeUntil,pitch,verticalSpeed,replayClock;
        int minute;
        bool replay,paused,pointMode;
        float sensitivity=1.55f;
        string testedRule="";
        ExperimentResult result;
        CampusAudio sound;
        Vector2 historyScroll;
        int styleLanguage=-1;
        Texture2D menuArt;
        float interactAfter;
        public CampusAudio Audio {get{return sound;}}
        public void Notify(string message,float seconds=4){Toast(message,seconds);}
        public void Persist(){Store();}
        string SavePath { get { return Path.Combine(Application.persistentDataPath,"campus-save.json"); } }
        string[] PhaseTitles={"","無人課室仍在運轉","燈光為何反覆切換","檢驗小芯的建議"};

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if(FindObjectOfType<CampusGame>()==null)new GameObject("CampusEngineer Game").AddComponent<CampusGame>();
        }
        void Awake()
        {
            CampusLocalization.Init();
            menuArt=Resources.Load<Texture2D>("CampusGardenMural");
            foreach(var camera in FindObjectsOfType<Camera>())camera.enabled=false;
            foreach(var listener in FindObjectsOfType<AudioListener>())listener.enabled=false;
            foreach(var light in FindObjectsOfType<Light>())light.enabled=false;
            Application.targetFrameRate=60;
            QualitySettings.vSyncCount=1;
            uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft JhengHei","Microsoft YaHei","Arial"},24);
            var actor=new GameObject("Player / First person");player=actor.transform;player.position=new Vector3(3.6f,.15f,-9.4f);
            controller=actor.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.27f;controller.center=Vector3.up*.9f;controller.stepOffset=.25f;
            var cam=new GameObject("Player Camera");cam.transform.SetParent(player,false);cam.transform.localPosition=Vector3.up*1.62f;
            view=cam.AddComponent<Camera>();view.nearClipPlane=.08f;view.farClipPlane=120;view.fieldOfView=72;view.allowHDR=true;
            cam.AddComponent<CampusCameraFinish>();
            cam.AddComponent<AudioListener>();
            sound=actor.AddComponent<CampusAudio>();
            world=new CampusWorld(this);world.Build();SetCursor(false);
        }
        void Start(){SetViewpoint(new Vector3(4.4f,.1f,-3.6f),new Vector3(-1.8f,1.5f,3.4f));}
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(panel!=""){panel="";paused=false;}
                else{paused=!paused;panel=paused?"pause":"";}
                SetCursor(panel=="" && !paused && !pointMode);
            }
            if(panel=="" && !paused)
            {
                if(!pointMode)MovePlayer();LookForInteraction();
                if(Time.time>=interactAfter && (Input.GetKeyDown(KeyCode.E) || (pointMode && Input.GetMouseButtonDown(0))) && lookedAt!=null)Interact(lookedAt);
                if(Input.GetKeyDown(KeyCode.Tab)){panel="evidence";SetCursor(false);}
                if(Input.GetKeyDown(KeyCode.R))ShowHelp();
                if(Input.GetKeyDown(KeyCode.J)){panel="journal";SetCursor(false);}
            }
            else lookedAt=null;
            if(replay && !paused && result!=null)
            {
                replayClock+=Time.deltaTime;
                if(replayClock>.25f){replayClock=0;minute++;if(minute>=result.ticks.Count){minute=0;replay=false;}}
            }
            UpdateWorld();
        }
        void MovePlayer()
        {
            float mouseX=Input.GetAxis("Mouse X")*sensitivity,mouseY=Input.GetAxis("Mouse Y")*sensitivity;
            player.Rotate(0,mouseX,0);pitch=Mathf.Clamp(pitch-mouseY,-72,72);view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            var direction=player.forward*Input.GetAxisRaw("Vertical")+player.right*Input.GetAxisRaw("Horizontal");
            direction=Vector3.ClampMagnitude(direction,1)*2.6f;
            verticalSpeed=controller.isGrounded?-1:verticalSpeed-20*Time.deltaTime;
            direction.y=verticalSpeed;controller.Move(direction*Time.deltaTime);
            sound.Walk(controller.isGrounded && new Vector2(direction.x,direction.z).sqrMagnitude>.2f);
            if(player.position.y<-4)player.position=new Vector3(3.6f,.2f,-9.4f);
        }
        void LookForInteraction()
        {
            lookedAt=null;RaycastHit hit;
            if(pointMode && PointerOverHud())return;
            Ray ray=pointMode?view.ScreenPointToRay(Input.mousePosition):new Ray(view.transform.position,view.transform.forward);
            if(Physics.SphereCast(ray,.06f,out hit,pointMode?15:3.1f))lookedAt=hit.collider.GetComponentInParent<CampusInteractable>();
        }
        bool PointerOverHud()
        {
            var cursor=new Vector2(Input.mousePosition.x*1600f/Screen.width,(Screen.height-Input.mousePosition.y)*900f/Screen.height);
            return new Rect(35,30,565,165).Contains(cursor) || new Rect(650,35,900,120).Contains(cursor);
        }
        void UpdateWorld()
        {
            bool fan=true,light=true;
            if(result!=null && result.ticks.Count>0){var tick=result.ticks[Mathf.Clamp(minute,0,result.ticks.Count-1)];fan=tick.fan;light=tick.light;}
            if(world.fanRotor!=null && fan && !paused)world.fanRotor.Rotate(0,Time.deltaTime*320,0,Space.Self);
            foreach(var lamp in world.classroomLights)lamp.intensity=light ? .48f : .06f;
            foreach(var renderer in world.lampPanels)renderer.sharedMaterial.SetColor("_EmissionColor",light?new Color(.7f,.8f,.83f)*1.3f:Color.black);
        }
        void Interact(CampusInteractable target)
        {
            interactAfter=Time.time+.35f;
            var hand=view.GetComponent<CampusHand>();if(hand!=null)hand.Reach();
            sound.Interact();
            switch(target.action)
            {
                case CampusAction.Door:target.Toggle();Toast("課室門已"+(target.opened?"打開":"關閉"));break;
                case CampusAction.Drawer:target.Toggle();Toast("連接標籤：TEMP 溫度 / OCC 有人狀態 / LUX 光照。這些是輸入，風扇和燈是輸出。",7);break;
                case CampusAction.Tablet:
                    save.tablet=true;
                    EquipTablet(target);
                    Store();Toast("已取得數據板。到右側牆壁逐一檢查 TEMP、OCC 和 LUX 感測器。",7);break;
                case CampusAction.Sensor:
                    if(!save.tablet){Toast("先到講台拿取數據板，再讀取感測器。");break;}
                    if(target.title.Contains("溫度"))save.temperature=true;
                    if(target.title.Contains("有人"))save.occupancy=true;
                    if(target.title.Contains("光線"))save.lux=true;
                    GetComponent<CampusAdventure>().Spark(target.transform.position,new Color(.3f,.85f,.63f),12);
                    Toast(target.title+"：記錄已加入數據板。所有數值均為教學模擬。",6);Store();break;
                case CampusAction.Terminal:
                    if(!save.tablet){Toast("控制器需要數據板。先到講台取得工具。");break;}
                    StartCoroutine(OpenTerminal());break;
                case CampusAction.Evidence:panel="evidence";SetCursor(false);break;
                case CampusAction.Robot:ShowHelp();break;
                case CampusAction.Mentor:
                    FindObjectOfType<CampusMascot>().Greet();StartCoroutine(MentorGreeting());break;
                case CampusAction.Play:GetComponent<CampusAdventure>().Play(target.playId,target.transform);break;
                case CampusAction.Window:Toast("窗邊自然光較亮。想一想：甚麼時候應該開燈？本樣章不模擬真實熱力學。",7);break;
                case CampusAction.Fan:Toast("風扇是輸出設備。手動關閉不代表修好自動控制規則。");break;
                case CampusAction.Light:Toast("燈是輸出設備。請比較有人狀態與光照，再決定何時需要開燈。");break;
                default:Toast("任務：滿足有人時段的設備需求，並消除無人空轉。比較結果前，先預測會發生甚麼。",7);break;
            }
        }
        void EquipTablet(CampusInteractable target)
        {
            if(target==null || target.animatedPart==null)return;
            StartCoroutine(TabletPickup(target.animatedPart));
            var collider=target.GetComponent<Collider>();if(collider!=null)collider.enabled=false;
        }
        System.Collections.IEnumerator TabletPickup(Transform tablet)
        {
            tablet.SetParent(view.transform,true);var p=tablet.localPosition;var q=tablet.localRotation;var s=tablet.localScale;
            for(float t=0;t<1;t+=Time.deltaTime/.9f){float k=Mathf.SmoothStep(0,1,t);tablet.localPosition=Vector3.Lerp(p,new Vector3(.45f,-.42f,.7f),k)+Vector3.up*Mathf.Sin(k*Mathf.PI)*.15f;tablet.localRotation=Quaternion.Slerp(q,Quaternion.Euler(-65,0,-12),k);tablet.localScale=Vector3.Lerp(s,Vector3.one*.8f,k);yield return null;}
            tablet.localPosition=new Vector3(.45f,-.42f,.7f);tablet.localRotation=Quaternion.Euler(-65,0,-12);tablet.localScale=Vector3.one*.8f;
        }
        System.Collections.IEnumerator OpenTerminal(){interactAfter=Time.time+.6f;yield return new WaitForSeconds(.55f);panel="terminal";SetCursor(false);}
        System.Collections.IEnumerator MentorGreeting(){interactAfter=Time.time+1;yield return new WaitForSeconds(.95f);ShowHelp();}
        void ShowHelp()
        {
            string[] hints={"","先檢查『有人』輸入。空課室裡，單靠溫度判斷會發生甚麼？","查看光照在300附近的變化。若每次都只比較同一條界線，燈會怎樣？可用雙閾值保留前一狀態。","小芯說全部關閉最省電。先用全關方案找出反例，再恢復合理規則完成未見情境。"};
            conversation=save.completed?"你已完成三段挑戰。到成果板查看測試證據，再想想這套模型與真實課室有甚麼差別。":hints[Mathf.Clamp(save.phase,1,3)];
            save.history.Add("提示 / "+PhaseTitles[Mathf.Clamp(save.phase,1,3)]);panel="conversation";SetCursor(false);Store();
        }
        void SetCursor(bool locked){Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!locked;}
        void Toast(string text,float duration=4){notice=text;noticeUntil=Time.unscaledTime+duration;}
        void Store()
        {
            try{Directory.CreateDirectory(Application.persistentDataPath);File.WriteAllText(SavePath,JsonUtility.ToJson(save,true));}
            catch(Exception e){notice="儲存暫時失敗："+e.Message;noticeUntil=Time.unscaledTime+6;}
        }
        void ContinueGame()
        {
            try{if(File.Exists(SavePath) && new FileInfo(SavePath).Length<1000000){var loaded=JsonUtility.FromJson<CampusSave>(File.ReadAllText(SavePath));if(loaded!=null && loaded.rules!=null){save=loaded;save.phase=Mathf.Clamp(save.phase,1,3);if(save.history==null)save.history=new List<string>();}}}
            catch(Exception){Toast("存檔未能讀取，保留原檔並開始新體驗。");}
            if(save.experiments==null)save.experiments=new List<ExperimentResult>();
            if(save.discoveries==null)save.discoveries=new List<string>();
            foreach(var prop in FindObjectsOfType<CampusInteractable>())if(prop.action==CampusAction.Play && prop.playId!=null && prop.playId.StartsWith("star") && save.discoveries.Contains(prop.playId))prop.gameObject.SetActive(false);
            var adventure=GetComponent<CampusAdventure>();if(save.discoveries.Contains("garden"))adventure.flower.localScale=Vector3.one;
            if(float.IsNaN(save.rules.fanThreshold) || float.IsInfinity(save.rules.fanThreshold))save.rules.fanThreshold=28;
            if(float.IsNaN(save.rules.lightThreshold) || float.IsInfinity(save.rules.lightThreshold))save.rules.lightThreshold=300;
            save.rules.fanThreshold=Mathf.Clamp(save.rules.fanThreshold,24,32);save.rules.lightThreshold=Mathf.Clamp(save.rules.lightThreshold,200,400);
            if(save.tablet)foreach(var item in FindObjectsOfType<CampusInteractable>())if(item.action==CampusAction.Tablet)EquipTablet(item);
            panel="";SetCursor(!pointMode);
        }
        void Run(bool counterexample=false)
        {
            if(!save.temperature || !save.occupancy || !save.lux){Toast("請先完成三種感測器的檢查，確保方案有觀察依據。");return;}
            var rules=save.rules.Copy();if(counterexample)rules.forceOff=true;
            int scenario=save.phase-1;
            result=CampusSimulation.Run(rules,scenario);minute=0;replay=true;replayClock=0;
            testedRule=RuleSignature(save.rules)+(counterexample?" / all-off":"");
            save.experiments.Add(result);
            save.history.Add("第"+save.phase+"段 / "+(counterexample?"檢驗全關":"測試方案")+" / 錯誤 "+result.errors+" / "+result.kwh.ToString("0.000000")+" kWh");
            if(counterexample && !result.Passed)save.counterexample=true;
            if(result.Passed)
            {
                sound.Success();
                if(save.phase==1)save.firstDone=true;
                if(save.phase==2)save.secondDone=true;
                if(save.phase==3 && save.counterexample){save.completed=true;Toast("修復完成！到成果板交付你的工程記錄。",7);}
            }
            Store();
        }
        string RuleSignature(ControlRules r){return JsonUtility.ToJson(r);}
        void NextPhase()
        {
            if(save.phase==1 && save.firstDone){save.phase=2;result=null;save.rules.hysteresis=false;}
            else if(save.phase==2 && save.secondDone){save.phase=3;result=null;save.rules.hysteresis=false;}
            Store();
        }

        void EnsureStyles()
        {
            if(titleStyle!=null && styleLanguage==CampusLocalization.Language)return;
            styleLanguage=CampusLocalization.Language;
            GUI.skin.font=uiFont;
            titleStyle=new GUIStyle(GUI.skin.label){font=uiFont,fontSize=34,fontStyle=FontStyle.Bold,wordWrap=true};titleStyle.normal.textColor=Color.white;
            subtitleStyle=new GUIStyle(titleStyle){fontSize=23};
            bodyStyle=new GUIStyle(GUI.skin.label){font=uiFont,fontSize=20,wordWrap=true};bodyStyle.normal.textColor=new Color(.87f,.92f,.94f);
            smallStyle=new GUIStyle(bodyStyle){fontSize=15};smallStyle.normal.textColor=new Color(.62f,.74f,.8f);
            buttonStyle=new GUIStyle(GUI.skin.button){font=uiFont,fontSize=19,padding=new RectOffset(14,14,8,8)};
            buttonStyle.normal.background=ButtonTexture(new Color(.075f,.25f,.29f));buttonStyle.normal.textColor=Color.white;
            buttonStyle.hover.background=ButtonTexture(new Color(.1f,.37f,.4f));buttonStyle.hover.textColor=Color.white;
            buttonStyle.active.background=ButtonTexture(new Color(.17f,.46f,.44f));buttonStyle.active.textColor=Color.white;
            codeStyle=new GUIStyle(bodyStyle){fontSize=18};codeStyle.normal.textColor=new Color(.47f,.91f,.75f);
            if(styleLanguage==2){titleStyle.fontSize=30;subtitleStyle.fontSize=22;bodyStyle.fontSize=18;smallStyle.fontSize=14;buttonStyle.fontSize=17;}
        }
        Texture2D ButtonTexture(Color color){var texture=new Texture2D(1,1);texture.SetPixel(0,0,color);texture.Apply();return texture;}
        static void RectFill(Rect rect,Color color){Color previous=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=previous;}
        void Text(float x,float y,float w,float h,string text,GUIStyle style=null){GUI.Label(new Rect(x,y,w,h),CampusLocalization.T(text),style??bodyStyle);}
        bool Button(float x,float y,float w,string text){return GUI.Button(new Rect(x,y,w,44),CampusLocalization.T(text),buttonStyle);}
        void DrawLanguageOptions(float x,float y)
        {
            Text(x,y,540,25,"語言 / Language",smallStyle);
            string[] labels={"简体中文","繁體中文","English"};
            for(int i=0;i<3;i++)
            {
                var previous=GUI.backgroundColor;if(i==CampusLocalization.Language)GUI.backgroundColor=new Color(.45f,1,.8f);
                if(Button(x+i*180,y+30,170,(i==CampusLocalization.Language?"✓  ":"")+labels[i]))CampusLocalization.SetLanguage(i);
                GUI.backgroundColor=previous;
            }
        }
        void WindowBase(string title,string detail)
        {
            RectFill(new Rect(0,0,1600,900),new Color(.015f,.03f,.04f,.58f));
            RectFill(new Rect(170,75,1260,750),new Color(.035f,.075f,.1f,.97f));
            RectFill(new Rect(170,75,6,750),new Color(.24f,.73f,.64f));
            Text(215,105,1120,58,title,titleStyle);Text(215,163,1120,45,detail,smallStyle);
            if(panel!="intro" && Button(1275,105,105,"返回")){panel="";paused=false;SetCursor(!pointMode);}
        }
        void OnGUI()
        {
            EnsureStyles();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1600f,Screen.height/900f,1));
            if(panel=="")
            {
                RectFill(new Rect(35,30,565,100),new Color(.025f,.06f,.08f,.88f));
                Text(55,42,530,30,"CAMPUS ENGINEER  /  校園工程師",smallStyle);
                Text(55,78,530,40,save.completed?"修復完成  前往成果板":PhaseTitles[save.phase],subtitleStyle);
                string objective=!save.tablet?"下一步  到講台拿取數據板":(!save.temperature || !save.occupancy || !save.lux)?"下一步  檢查右牆 TEMP / OCC / LUX":save.completed?"下一步  到成果板交付工程記錄":"下一步  前往控制終端測試方案";
                RectFill(new Rect(35,143,565,52),new Color(.025f,.06f,.08f,.88f));Text(55,154,530,35,objective,smallStyle);
                if(!pointMode)RectFill(new Rect(798,448,4,4),new Color(.8f,.95f,.92f));
                if(pointMode)DrawViewpoints();
                if(Button(1210,100,340,"發現手帳  J  ·  "+(save.discoveries==null?0:save.discoveries.Count)+" / 7")){panel="journal";SetCursor(false);}
                if(lookedAt!=null)
                {RectFill(new Rect(440,720,720,65),new Color(.035f,.075f,.1f,.94f));Text(465,735,680,40,"E  "+lookedAt.title,subtitleStyle);}
                Text(40,845,1100,35,"WASD 移動  ·  滑鼠觀察  ·  E 互動  ·  R 提示  ·  Tab 記錄  ·  Esc 暫停",smallStyle);
            }
            else if(panel=="intro")DrawIntro();
            else if(panel=="terminal")DrawTerminal();
            else if(panel=="evidence")DrawEvidence();
            else if(panel=="conversation")DrawConversation();
            else if(panel=="pause")DrawPause();
            else if(panel=="journal")DrawJournal();
            if(Time.unscaledTime<noticeUntil)
            {RectFill(new Rect(320,785,960,54),new Color(.08f,.19f,.22f,.97f));Text(340,797,920,42,notice,smallStyle);}
        }
        void DrawIntro()
        {
            if(menuArt!=null)GUI.DrawTexture(new Rect(0,0,1600,900),menuArt,ScaleMode.ScaleAndCrop);
            RectFill(new Rect(70,60,810,780),new Color(.025f,.075f,.08f,.94f));RectFill(new Rect(70,60,5,780),new Color(.80f,.62f,.36f));
            Text(110,105,720,58,"校園工程師",titleStyle);Text(110,163,720,45,"香港校園 STEAM 第一人稱實驗樣章  /  0.3 開發版",smallStyle);
            Text(110,230,720,135,"203課室沒有人，風扇卻一直轉。\n你能找出原因，讓系統作出合理決定嗎？",styleLanguage==2?subtitleStyle:titleStyle);
            Text(110,380,720,145,"走進課室、拿取工具、檢查資料，再改進控制規則。小芯會陪你提出假設，但每個建議都要經過測試。\n目前是規則卡原型，Python 顯示為對應示例；教學機器人使用預設提示。",bodyStyle);
            if(Button(110,560,300,"開始新體驗")){save=new CampusSave();SetViewpoint(new Vector3(3.6f,.15f,-9.4f),new Vector3(3.8f,1.5f,-6.8f));panel="";SetCursor(!pointMode);}
            if(File.Exists(SavePath) && Button(430,560,300,"繼續上次記錄"))ContinueGame();
            Text(110,635,720,65,"遊戲內數據與節能比例屬教學模擬，不代表真實課室標準。\n按 Esc 可隨時暫停並解除滑鼠鎖定。",smallStyle);
            DrawLanguageOptions(110,725);
        }
        void DrawTerminal()
        {
            WindowBase("控制終端  /  "+PhaseTitles[save.phase],"觀察 → 設計 → 測試 → 比較 → 改進");
            Text(215,220,490,40,"構建你的控制規則",subtitleStyle);
            if(Button(215,275,490,(save.rules.requireOccupied?"已啟用":"未啟用")+"  有人才允許設備啟動"))save.rules.requireOccupied=!save.rules.requireOccupied;
            Text(215,330,490,35,"風扇閾值  "+save.rules.fanThreshold.ToString("0")+" °C");
            save.rules.fanThreshold=Mathf.Round(GUI.HorizontalSlider(new Rect(220,378,440,25),save.rules.fanThreshold,24,32));
            Text(215,420,490,35,"開燈閾值  "+save.rules.lightThreshold.ToString("0")+" lux");
            save.rules.lightThreshold=Mathf.Round(GUI.HorizontalSlider(new Rect(220,468,440,25),save.rules.lightThreshold,200,400)/10)*10;
            if(save.phase==2 && Button(215,515,490,(save.rules.hysteresis?"已啟用":"未啟用")+"  雙閾值與前一次狀態"))save.rules.hysteresis=!save.rules.hysteresis;
            Text(215,570,490,40,"對應 Python 示例  非自由程式執行",smallStyle);
            Text(215,610,490,100,CampusSimulation.PythonPreview(save.rules),codeStyle);
            if(Button(215,730,240,"運行測試"))Run();
            if(save.phase==3 && Button(470,730,230,"檢驗全關建議"))Run(true);
            DrawResults(760,220);
        }
        void DrawResults(float x,float y)
        {
            Text(x,y,580,42,"實驗證據",subtitleStyle);
            if(result==null){Text(x,y+65,580,150,"先運行一次測試。\n\n任務標準：有人且溫度 ≥28°C 時需要風扇；有人且光照 <300 lux 時需要燈。第二段另用280/320雙閾值。",bodyStyle);return;}
            Color status=result.Passed?new Color(.35f,.9f,.7f):new Color(1,.72f,.38f);
            RectFill(new Rect(x,y+55,580,55),new Color(status.r*.2f,status.g*.2f,status.b*.2f));
            Text(x+15,y+65,550,40,result.Passed?"全部情境符合要求":"有 "+result.errors+" 個時段需要改進",subtitleStyle);
            Text(x,y+125,580,65,"模擬電量 "+result.kwh.ToString("0.000000")+" kWh   /   空轉 "+result.emptyActiveFrames+" 分鐘\n相對全開基準減少 "+(result.Savings*100).ToString("0")+"%   /   切換 "+result.switches+" 次",smallStyle);
            if(testedRule.EndsWith(" / all-off"))Text(x,y+190,580,25,"正在回放全關方案  請比較需求與實際輸出",smallStyle);
            else if(testedRule!=RuleSignature(save.rules))Text(x,y+190,580,25,"顯示上一次實驗  規則改變後請重新測試",smallStyle);
            float barWidth=580f/result.ticks.Count;
            for(int i=0;i<result.ticks.Count;i++)RectFill(new Rect(x+i*barWidth,y+215,barWidth-2,55),result.ticks[i].Correct?new Color(.13f,.6f,.48f):new Color(.8f,.36f,.17f));
            minute=Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(x,y+287,575,25),minute,0,result.ticks.Count-1));
            var tick=result.ticks[Mathf.Clamp(minute,0,result.ticks.Count-1)];
            Text(x,y+330,580,110,"第 "+minute+" 模擬分鐘  /  "+(tick.sensor.occupied?"有人":"無人")+"\n溫度 "+tick.sensor.temperature+"°C   光照 "+tick.sensor.lux+" lux\n輸出：風扇 "+(tick.fan?"開":"關")+"   燈 "+(tick.light?"開":"關"),bodyStyle);
            if(Button(x,y+455,180,replay?"暫停回放":"播放回放"))replay=!replay;
            if(save.phase<3 && result.Passed && testedRule==RuleSignature(save.rules) && ((save.phase==1 && save.firstDone)||(save.phase==2 && save.secondDone)))
            {if(Button(x+195,y+455,260,"進入下一段挑戰"))NextPhase();}
            if(save.phase==3 && save.completed)Text(x,y+515,580,55,"已完成！到成果板查看你的工程記錄。",smallStyle);
        }
        void DrawEvidence()
        {
            WindowBase("工程記錄","以操作與測試留下學習證據  /  本地匿名記錄");
            Text(215,220,1120,50,"數據板 "+(save.tablet?"已取得":"未取得")+"   TEMP "+(save.temperature?"✓":"—")+"   OCC "+(save.occupancy?"✓":"—")+"   LUX "+(save.lux?"✓":"—"),subtitleStyle);
            Text(215,290,1120,45,save.completed?"任務完成  你用反例檢驗了建議，並完成新情境。":"進度  "+PhaseTitles[save.phase],bodyStyle);
            historyScroll=GUI.BeginScrollView(new Rect(215,355,1130,300),historyScroll,new Rect(0,0,1080,Mathf.Max(300,save.history.Count*37)));
            for(int i=0;i<save.history.Count;i++)Text(10,i*37,1050,36,save.history[i],smallStyle);
            GUI.EndScrollView();
            if(Button(215,715,300,"保存並導出本地記錄"))
            {
                Store();string path=Path.Combine(Application.persistentDataPath,"campus-evidence-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".json");
                try{File.WriteAllText(path,JsonUtility.ToJson(save,true));Toast("已導出至 "+path,10);}catch(Exception e){Toast("導出失敗："+e.Message,6);}
            }
            Text(550,715,760,60,"通關不等於獨立掌握。教師仍需查看未見情境的表現與提示使用。",smallStyle);
        }
        void DrawConversation()
        {
            WindowBase("小芯的工程筆記","預設教學提示  /  未連接生成式 AI 服務");
            Text(245,270,1060,200,conversation,subtitleStyle);
            Text(245,510,1060,90,"你可以提出自己的方案。請用實驗決定它是否合理，而不是只相信機器人的說法。",bodyStyle);
            if(Button(245,675,280,"回到現場驗證")){panel="";SetCursor(!pointMode);}
            if(Button(550,675,340,"和小嵐開心擊掌")){panel="";SetCursor(!pointMode);var mascot=FindObjectOfType<CampusMascot>();mascot.Greet();GetComponent<CampusAdventure>().Spark(mascot.transform.position+Vector3.up,new Color(.95f,.61f,.3f),20);Toast("耶！小嵐很高興和你一起探索校園。",4);}
        }
        void DrawJournal()
        {
            WindowBase("課間發現手帳","學習之外，也可以慢慢探索、玩一會兒。這些活動不影響工程任務。 ");
            string[] ids={"plane","garden","music","chime","star0","star1","star2"};
            string[] names={"紙飛機飛行章","窗邊小園丁","自己的下課旋律","聽見風的聲音","書架裡的紙星星","儲物櫃上的紙星星","同學留下的紙星星"};
            string[] hints={"找找課室左側桌上的紙飛機。","窗邊的澆水壺正在等你。","閱讀角有一台能真的彈奏的木琴。","輕碰窗邊掛著的風鈴。","慢慢看看閱讀角。","留意儲物櫃的小角落。","桌面上有個閃亮的小秘密。"};
            for(int i=0;i<ids.Length;i++)
            {
                bool done=save.discoveries!=null && save.discoveries.Contains(ids[i]);float y=225+i*68;
                RectFill(new Rect(230,y,1120,59),done?new Color(.12f,.31f,.25f):new Color(.08f,.14f,.17f));
                Text(250,y+8,450,42,(done?"✓  ":"◇  ")+names[i],subtitleStyle);Text(730,y+15,590,38,done?"已收入你的校園回憶":hints[i],smallStyle);
            }
        }
        void DrawPause()
        {
            WindowBase("暫停","遊戲進度已保留  /  可自由離開滑鼠鎖定");
            if(Button(245,255,400,"繼續探索")){panel="";paused=false;SetCursor(!pointMode);}
            if(Button(245,325,400,"保存目前進度")){Store();Toast("進度已保存。");}
            if(Button(245,395,400,"回到入口安全位置")){SetViewpoint(new Vector3(3.6f,.2f,-9.4f),new Vector3(3.8f,1.5f,-6.8f));panel="";paused=false;SetCursor(!pointMode);}
            if(Button(245,465,400,"低畫質  關閉陰影")){QualitySettings.shadows=ShadowQuality.Disable;Toast("已關閉陰影。");}
            if(Button(245,535,400,"離開遊戲")){Store();Application.Quit();}
            if(Button(245,605,400,pointMode?"切換為第一人稱自由移動":"切換為固定視點點擊模式")){pointMode=!pointMode;Toast(pointMode?"固定視點已啟用。返回場景後可點擊上方位置按鈕。":"已切換為自由移動。");}
            Text(760,275,550,175,"操作\nWASD 移動 / 滑鼠觀察\nE 互動 / Tab 記錄 / R 提示\nJ 發現手帳 / Esc 暫停\n\n預設無鏡頭搖晃、無跳躍要求。",bodyStyle);
            Text(760,500,500,30,"視角靈敏度",smallStyle);
            sensitivity=GUI.HorizontalSlider(new Rect(760,545,500,25),sensitivity,.5f,3);
            Text(760,590,500,30,"視野角度  "+view.fieldOfView.ToString("0"),smallStyle);
            view.fieldOfView=GUI.HorizontalSlider(new Rect(760,635,500,25),view.fieldOfView,60,95);
            DrawLanguageOptions(760,700);
        }
        void SetViewpoint(Vector3 position,Vector3 target)
        {
            controller.enabled=false;player.position=position;controller.enabled=true;
            Vector3 direction=target-(position+Vector3.up*1.62f);
            player.rotation=Quaternion.Euler(0,Quaternion.LookRotation(direction).eulerAngles.y,0);
            pitch=Quaternion.LookRotation(direction).eulerAngles.x;
            if(pitch>180)pitch-=360;view.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
        void DrawViewpoints()
        {
            if(Button(650,35,132,"入口"))SetViewpoint(new Vector3(3.6f,.2f,-9.4f),new Vector3(3.8f,1.5f,-6.8f));
            if(Button(795,35,132,"感測器"))SetViewpoint(new Vector3(3.6f,.1f,-2),new Vector3(5.8f,1.5f,-2));
            if(Button(940,35,132,"控制台"))SetViewpoint(new Vector3(3.4f,.1f,3.6f),new Vector3(5,1.5f,3.6f));
            if(Button(1085,35,132,"講台"))SetViewpoint(new Vector3(-2,.1f,3.7f),new Vector3(-2,.85f,5.1f));
            if(Button(1230,35,132,"窗邊花園"))SetViewpoint(new Vector3(-4.6f,.1f,1.5f),new Vector3(-22,2.8f,2.5f));
            if(Button(1375,35,175,"小芯與成果"))SetViewpoint(new Vector3(3.2f,.1f,3.4f),new Vector3(5,1.6f,5.5f));
            if(Button(650,100,165,"紙飛機"))SetViewpoint(new Vector3(-2.8f,.1f,-.7f),new Vector3(-3.8f,.85f,.2f));
            if(Button(825,100,165,"音樂角"))SetViewpoint(new Vector3(-3.75f,.1f,3.75f),new Vector3(-4.7f,.75f,4.5f));
            if(Button(1000,100,165,"窗邊小花"))SetViewpoint(new Vector3(-4.15f,.1f,.5f),new Vector3(-5.45f,1.45f,1.5f));
        }
        void OnApplicationQuit(){Store();SetCursor(false);}
    }
}
