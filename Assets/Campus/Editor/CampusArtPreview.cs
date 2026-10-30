using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
namespace CampusEngineer.Editor
{
    public static class CampusArtPreview
    {
        static double start;
        [MenuItem("Campus Engineer/Review companion close-up")]
        static void Begin(){if(EditorApplication.isPlaying){Focus();return;}start=EditorApplication.timeSinceStartup;EditorApplication.update+=Wait;EditorApplication.isPlaying=true;}
        static void Wait(){if(EditorApplication.timeSinceStartup-start>30){EditorApplication.update-=Wait;return;}if(!EditorApplication.isPlaying || EditorApplication.isCompiling || UnityEngine.Object.FindObjectOfType<CampusGame>()==null)return;EditorApplication.update-=Wait;EditorApplication.delayCall+=Focus;}
        static void Focus()
        {
            var game=UnityEngine.Object.FindObjectOfType<CampusGame>();if(game==null)return;
            typeof(CampusGame).GetField("panel",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,"");
            typeof(CampusGame).GetField("pointMode",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,true);
            typeof(CampusGame).GetMethod("SetViewpoint",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,new object[]{new Vector3(3.15f,.1f,4.2f),new Vector3(3.7f,1.0f,5.45f)});
            typeof(CampusGame).GetMethod("SetCursor",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,new object[]{false});
        }
        [MenuItem("Campus Engineer/Capture companion review")]
        static void Capture()
        {
            var g=UnityEngine.Object.FindObjectOfType<CampusGame>();if(g==null)return;
            Directory.CreateDirectory("Documentation/ArtReview");var rt=RenderTexture.GetTemporary(1600,900,24);var old=g.view.targetTexture;var active=RenderTexture.active;
            try{g.view.targetTexture=rt;g.view.Render();RenderTexture.active=rt;var t=new Texture2D(1600,900,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1600,900),0,0);t.Apply();File.WriteAllBytes("Documentation/ArtReview/companion.png",t.EncodeToPNG());UnityEngine.Object.Destroy(t);}
            finally{g.view.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
        }
    }
}
