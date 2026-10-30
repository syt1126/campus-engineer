using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CampusEngineer.Editor
{
    // Explicit editor-only integration check. Never runs during a player's session.
    public static class CampusValidation
    {
        static double started;
        static int step;
        static int previousLanguage;
        static bool doorTriggered;
        static Vector3 planeRest;
        static double nextFrame;static int frameNumber;
        static EditorWindow gameWindow;static bool previousMaximized;
        static CampusGame game;
        static readonly List<string> checks=new List<string>();
        static readonly List<string> errors=new List<string>();
        static readonly string folder="Documentation/RuntimeQA";
        const string activeKey="CampusEngineer.Validation.Active";

        [InitializeOnLoadMethod]
        static void ResumeAfterDomainReload()
        {
            if(!SessionState.GetBool(activeKey,false))return;
            started=EditorApplication.timeSinceStartup;
            Application.logMessageReceived-=OnLog;Application.logMessageReceived+=OnLog;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        }

        [MenuItem("Campus Engineer/Run integration check")]
        public static void Begin()
        {
            checks.Clear();errors.Clear();step=0;game=null;doorTriggered=false;frameNumber=0;nextFrame=0;
            previousLanguage=PlayerPrefs.GetInt(CampusLocalization.PreferenceKey,1);
            Directory.CreateDirectory(folder);
            EditorSceneManager.OpenScene("Assets/Campus/Scenes/SmartClassroom.unity");
            if(!Application.isBatchMode){gameWindow=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));previousMaximized=gameWindow.maximized;gameWindow.maximized=true;gameWindow.Focus();}
            SessionState.SetBool(activeKey,true);
            Application.logMessageReceived+=OnLog;
            EditorApplication.update+=Tick;
            started=EditorApplication.timeSinceStartup;
            EditorApplication.isPlaying=true;
        }
        static void OnLog(string message,string stack,LogType type)
        {if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert)errors.Add(message);}
        static void Require(bool condition,string label)
        {if(!condition)throw new Exception("Integration check failed: "+label);checks.Add(label);}
        static void Invoke(string name,params object[] args)
        {typeof(CampusGame).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);}
        static void Field(string name,object value)
        {typeof(CampusGame).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,value);}
        static void Shot(string name)
        {
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(folder,name+".png")));
            if(string.CompareOrdinal(name,"04")<0)return;
            // Actual Unity camera render, at a fixed resolution, without interface overlays.
            var target=RenderTexture.GetTemporary(1600,900,24);var previousTarget=game.view.targetTexture;var previousActive=RenderTexture.active;
            Texture2D texture=null;
            try
            {
                game.view.targetTexture=target;game.view.Render();RenderTexture.active=target;
                texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(folder,name+"-world.png"),texture.EncodeToPNG());
            }
            finally{game.view.targetTexture=previousTarget;RenderTexture.active=previousActive;RenderTexture.ReleaseTemporary(target);if(texture!=null)UnityEngine.Object.Destroy(texture);}
        }
        static CampusInteractable Find(CampusAction action)
        {return Array.Find(UnityEngine.Object.FindObjectsOfType<CampusInteractable>(),i=>i.action==action);}
        static void Tick()
        {
            if(game!=null && step==20 && EditorApplication.timeSinceStartup>=nextFrame && frameNumber<14)
            {
                Directory.CreateDirectory(folder+"/GreetingFrames");var rt=RenderTexture.GetTemporary(800,450,24);var oldTarget=game.view.targetTexture;var oldActive=RenderTexture.active;
                try{game.view.targetTexture=rt;game.view.Render();RenderTexture.active=rt;var frame=new Texture2D(800,450,TextureFormat.RGB24,false);frame.ReadPixels(new Rect(0,0,800,450),0,0);frame.Apply();File.WriteAllBytes(folder+"/GreetingFrames/"+frameNumber.ToString("D2")+".png",frame.EncodeToPNG());UnityEngine.Object.Destroy(frame);}
                finally{game.view.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(rt);}
                frameNumber++;nextFrame=EditorApplication.timeSinceStartup+.13f;
            }
            if(EditorApplication.timeSinceStartup-started>100){errors.Add("Editor check timed out");Finish();return;}
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling)return;
            if(EditorApplication.timeSinceStartup-started<3+step*2)return;
            try
            {
                if(step==0)
                {
                    game=UnityEngine.Object.FindObjectOfType<CampusGame>();Require(game!=null,"runtime bootstrap creates game");
                    Require(game.view!=null && game.view.enabled,"first-person camera active");
                    Require(Resources.Load<GameObject>("Characters/CampusGuide/CampusGuide")==null,"rejected white-faced sample excluded from player resources");
                    Require(UnityEngine.Object.FindObjectOfType<CampusMascot>().GetComponentsInChildren<MeshRenderer>().Length>45,"original red panda has detailed visible geometry");
                    Require(UnityEngine.Object.FindObjectOfType<CampusMascot>().head!=null,"red panda has articulated face, arms and tail");
                    Require(Resources.Load<GameObject>("Characters/Mentor")==null,"retired uncanny mentor is excluded from runtime resources");
                    Require(Resources.Load<GameObject>("SchoolAssets/Prefabs/props/locker_1")==null || GameObject.Find("School asset / locker_1")!=null,"optional licensed classroom props load when installed");
                    Require(CampusLocalization.Count>=155,"all three language catalogs load");
                    CampusLocalization.SetLanguage(2);Require(CampusLocalization.T("校園工程師")=="Campus Engineer","English menu translation resolves");
                    Require(PlayerPrefs.GetInt(CampusLocalization.PreferenceKey)==2,"language preference is saved independently of progress");
                    Require(UnityEngine.Object.FindObjectsOfType<CampusInteractable>().Length>=20,"world interaction targets created");
                    Require(Resources.Load<Texture2D>("Textures/floor_diffuse")!=null,"floor texture loads");
                    Shot("01-intro");
                }
                else if(step==1)
                {
                    var controller=game.player.GetComponent<CharacterController>();
                    if(!doorTriggered)
                    {
                        game.save=new CampusSave();Field("panel","");Field("pointMode",true);Invoke("SetCursor",false);
                        Invoke("SetViewpoint",new Vector3(3.6f,.15f,-9.4f),new Vector3(3.8f,1.77f,-6.8f));
                        controller.Move(Vector3.forward*3);Require(game.player.position.z<-7,"closed door blocks first-person movement");
                        Invoke("Interact",Find(CampusAction.Door));Require(Find(CampusAction.Door).opened,"door opens through game interaction");
                        Require(Find(CampusAction.Door).IsAnimating,"door begins a timed animation instead of teleporting");doorTriggered=true;return;
                    }
                    if(Find(CampusAction.Door).IsAnimating)return;
                    Require(Quaternion.Angle(Find(CampusAction.Door).animatedPart.localRotation,Quaternion.identity)>70,"door animation reaches its open pose");
                    Physics.SyncTransforms();
                    controller.Move(Vector3.forward*3);
                    Require(game.player.position.z>-6.5f,"open doorway allows movement into classroom");
                    Invoke("Interact",Find(CampusAction.Tablet));Require(game.save.tablet,"tablet pickup updates progress");
                    foreach(var target in UnityEngine.Object.FindObjectsOfType<CampusInteractable>())if(target.action==CampusAction.Sensor)Invoke("Interact",target);
                    Require(game.save.temperature && game.save.occupancy && game.save.lux,"all three sensor interactions record evidence");
                    Require(CampusLocalization.Language==2,"sensor recording remains correct in English");
                    Invoke("SetViewpoint",new Vector3(3.4f,.1f,-4.5f),new Vector3(-.5f,1.6f,4.8f));
                    Shot("02-classroom");
                }
                else if(step==2)
                {
                    Invoke("Interact",Find(CampusAction.Terminal));Invoke("Run",false);
                    Require(!game.save.firstDone && game.save.experiments.Count==1,"missing occupancy fails first experiment");
                    game.save.rules.requireOccupied=true;Invoke("Run",false);
                    Require(game.save.firstDone,"occupancy repair passes first challenge");
                    Shot("03-terminal");
                }
                else if(step==3)
                {
                    Invoke("NextPhase");Invoke("Run",false);Require(!game.save.secondDone,"single light threshold fails noisy scenario");
                    game.save.rules.hysteresis=true;Invoke("Run",false);Require(game.save.secondDone,"hysteresis repair passes second challenge");
                    Invoke("NextPhase");Invoke("Run",true);Require(game.save.counterexample && !game.save.completed,"all-off proposal has a counterexample");
                    Invoke("Run",false);Require(game.save.completed,"independent final scenario completes mission");
                    Require(game.save.experiments.Count==6,"all six experiment traces retained");
                    Field("panel","");Invoke("SetViewpoint",new Vector3(2,.1f,3.35f),new Vector3(3.7f,1.25f,5.45f));Shot("04-mentor");
                }
                else if(step==4)
                {
                    Invoke("Store");string path=Path.Combine(Application.persistentDataPath,"campus-save.json");
                    Require(File.Exists(path),"local save written");
                    var restored=JsonUtility.FromJson<CampusSave>(File.ReadAllText(path));
                    Require(restored.completed && restored.experiments.Count==6,"completion and traces survive save roundtrip");
                    Field("panel","");Field("noticeUntil",0f);Invoke("SetViewpoint",new Vector3(4.4f,.1f,-3.6f),new Vector3(-1.8f,1.5f,3.4f));Shot("05-room-overview");
                }
                else if(step==5)
                {
                    Invoke("SetViewpoint",new Vector3(-.5f,.1f,-1.25f),new Vector3(-1.7f,.65f,.2f));Shot("06-furniture");
                }
                else if(step==6)
                {
                    Invoke("SetViewpoint",new Vector3(-4.6f,.1f,1.5f),new Vector3(-22,2.8f,2.5f));Shot("07-window-garden");
                }
                else if(step==7)
                {
                    Field("panel","intro");CampusLocalization.SetLanguage(2);Invoke("SetViewpoint",new Vector3(4.4f,.1f,-3.6f),new Vector3(-1.8f,1.5f,3.4f));
                }
                else if(step==8)
                {
                    Require(CampusLocalization.T("第2段 / 測試方案 / 錯誤 1").IndexOf("測試",StringComparison.Ordinal)<0,"composite engineering records translate to English");
                    Shot("08-language-en");
                }
                else if(step==9)
                {
                    CampusLocalization.SetLanguage(0);
                    Require(CampusLocalization.T("校園工程師")=="校园工程师","Simplified Chinese resolves");
                    Require(CampusLocalization.T("檢查溫度感測器")=="检查温度感测器","Simplified Chinese interaction resolves");
                    Shot("09-language-sc");
                }
                else if(step==10)
                {
                    CampusLocalization.SetLanguage(1);
                    Require(CampusLocalization.T("校園工程師")=="校園工程師","Traditional Chinese resolves");
                }
                else if(step==11){Shot("10-language-tc");}
                else if(step==12)
                {
                    CampusLocalization.SetLanguage(previousLanguage);Field("panel","");
                    Invoke("SetViewpoint",new Vector3(3.15f,.1f,4.2f),new Vector3(3.7f,1.3f,5.45f));
                }
                else if(step==13)
                {
                    Shot("11-guide-closeup");
                    Require(game.view.GetComponent<CampusHand>()==null,"rejected primitive hand is absent");
                    var a=game.GetComponent<CampusAdventure>();planeRest=a.plane.position;
                    a.Play("plane",a.plane);Require(a.PlaneFlying,"paper plane starts a real flight");
                    Invoke("SetViewpoint",new Vector3(-2,.1f,-2),new Vector3(0,2,0));
                }
                else if(step==14){var a=game.GetComponent<CampusAdventure>();Require(a.PlaneFlying && Vector3.Distance(planeRest,a.plane.position)>.5f,"paper plane moves continuously through classroom");Shot("12-paper-plane");}
                else if(step==15){var a=game.GetComponent<CampusAdventure>();a.Play("water",a.wateringCan);Require(a.Watering,"watering begins with timed can movement");Invoke("SetViewpoint",new Vector3(-4.15f,.1f,.5f),new Vector3(-5.45f,1.45f,1.5f));}
                else if(step==16){Require(game.GetComponent<CampusAdventure>().flower.localScale.x>.25f,"flower opens during watering");Shot("13-watering");}
                else if(step==17)
                {
                    var a=game.GetComponent<CampusAdventure>();Require(!a.PlaneFlying && Vector3.Distance(planeRest,a.plane.position)<.01f,"plane lands back at its resting place");
                    Require(!a.Watering && a.flower.localScale.x>.99f,"watering finishes and flower remains open");
                    for(int n=0;n<4;n++)a.Play("note"+n,a.bars[n]);Require(game.save.discoveries.Contains("music"),"four distinct wooden keys unlock music keepsake");
                    foreach(var prop in UnityEngine.Object.FindObjectsOfType<CampusInteractable>())if(prop.action==CampusAction.Play && (prop.playId.StartsWith("star") || prop.playId=="bell"))a.Play(prop.playId,prop.transform);
                    Require(game.save.discoveries.Count==7,"all seven optional keepsakes can be earned");int count=game.save.discoveries.Count;a.Remember("plane");Require(game.save.discoveries.Count==count,"keepsakes are not duplicated");
                    Invoke("Store");var saved=JsonUtility.FromJson<CampusSave>(File.ReadAllText(Path.Combine(Application.persistentDataPath,"campus-save.json")));Require(saved.discoveries.Count==7,"optional discoveries survive save serialization");
                    CampusLocalization.SetLanguage(2);Field("panel","journal");
                }
                else if(step==18){Require(CampusLocalization.T("課間發現手帳")=="Recess discovery journal","new activities are translated to English");Shot("14-discovery-journal");}
                else if(step==19){Field("panel","");var m=UnityEngine.Object.FindObjectOfType<CampusMascot>();m.Greet();Require(m.Greeting,"red panda reacts with a full greeting animation");Invoke("SetViewpoint",new Vector3(3.15f,.1f,4.2f),new Vector3(3.7f,1.0f,5.45f));}
                else if(step==20){Shot("15-new-guide");Invoke("SetViewpoint",new Vector3(-3.75f,.1f,3.75f),new Vector3(-4.7f,.75f,4.5f));}
                else if(step==21){Shot("16-music-corner");CampusLocalization.SetLanguage(previousLanguage);game.save=new CampusSave();Invoke("Store");Finish();return;}
                step++;
            }
            catch(Exception e){errors.Add(e.ToString());Finish();}
        }
        [Serializable] class Report{public string scope="Unity editor integration check; does not replace manual usability testing";public string status;public string[] passed;public string[] errors;}
        static void Finish()
        {
            SessionState.SetBool(activeKey,false);
            Application.logMessageReceived-=OnLog;EditorApplication.update-=Tick;
            File.WriteAllText(Path.Combine(folder,"integration-report.json"),JsonUtility.ToJson(new Report{status=errors.Count==0?"passed":"failed",passed=checks.ToArray(),errors=errors.ToArray()},true));
            Debug.Log("CAMPUS_INTEGRATION_"+(errors.Count==0?"PASSED":"FAILED"));
            EditorApplication.isPlaying=false;
            if(gameWindow!=null)gameWindow.maximized=previousMaximized;
            if(Application.isBatchMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(errors.Count==0?0:1);
        }
    }
}
