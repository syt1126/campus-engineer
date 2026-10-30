using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CampusEngineer.Editor
{
    public static class CampusBuild
    {
        [InitializeOnLoadMethod]
        static void PrepareOnFirstOpen()
        {
            if(Application.isBatchMode || File.Exists("Assets/Campus/Scenes/SmartClassroom.unity"))return;
            EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode)Prepare();};
        }
        [MenuItem("Campus Engineer/Prepare first chapter")]
        public static void Prepare()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            string path="Assets/Campus/Scenes/SmartClassroom.unity";
            EditorSceneManager.SaveScene(scene,path);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};
            PlayerSettings.companyName="Campus Engineer";PlayerSettings.productName="Campus Engineer - Smart Classroom";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
            PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            AssetDatabase.SaveAssets();Debug.Log("CAMPUS_PREPARE_OK");
        }
        [MenuItem("Campus Engineer/Build Windows prototype")]
        public static void BuildWindows()
        {
            Prepare();Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Builds/Windows/CampusEngineer.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Campus build failed: "+report.summary.result);
            Directory.CreateDirectory("Builds/Windows/ThirdPartyNotices");
            foreach(var name in new[]{"Microsoft-Rocketbox-LICENSE.md","Quaternius-Animation-LICENSE.txt","ASSET_SOURCES.json","THIRD_PARTY_NOTICES.md","VRoid-Model-Terms.md"})
            {string source="Documentation/"+name;if(File.Exists(source))File.Copy(source,"Builds/Windows/ThirdPartyNotices/"+name,true);}
            Debug.Log("CAMPUS_BUILD_OK");
        }
    }

    public class CampusTextureImport:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("Campus/Resources/") || !assetPath.Contains("Textures/"))return;
            var importer=(TextureImporter)assetImporter;importer.maxTextureSize=1024;importer.mipmapEnabled=true;
            if(assetPath.Contains("_normal"))importer.textureType=TextureImporterType.NormalMap;
        }
    }

    public class CampusCharacterImport:AssetPostprocessor
    {
        bool IsMentor { get { return assetPath.EndsWith("Campus/Resources/Characters/Mentor.fbx"); } }
        void OnPreprocessModel()
        {
            if(!IsMentor)return;
            var importer=(ModelImporter)assetImporter;
            importer.animationType=ModelImporterAnimationType.Generic;
            importer.importBlendShapes=true;
        }
        void OnPostprocessMaterial(Material material)
        {
            if(!IsMentor)return;
            material.color=Color.white;
        }
        void OnPostprocessModel(GameObject model)
        {
            if(!IsMentor)return;
            foreach(var transform in model.GetComponentsInChildren<Transform>(true))
            {string n=transform.name.ToLowerInvariant();if(n.Contains("poly") && !n.Contains("hipoly"))transform.gameObject.SetActive(false);}
        }
    }
}
