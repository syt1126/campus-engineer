using UnityEngine;

namespace CampusEngineer
{
    [RequireComponent(typeof(Camera))]
    public class CampusCameraFinish:MonoBehaviour
    {
        Material finish;
        void OnEnable(){var shader=Resources.Load<Shader>("CampusCameraFinish");if(shader!=null && shader.isSupported)finish=new Material(shader);GetComponent<Camera>().depthTextureMode|=DepthTextureMode.DepthNormals;}
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {if(finish!=null && QualitySettings.shadows!=ShadowQuality.Disable)Graphics.Blit(source,destination,finish);else Graphics.Blit(source,destination);}
        void OnDisable(){if(finish!=null)Destroy(finish);}
    }
}
