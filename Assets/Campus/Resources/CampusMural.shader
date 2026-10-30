Shader "Campus/Mural"
{
 Properties{_MainTex("Original artwork",2D)="white"{}}
 SubShader
 {
  Tags{"RenderType"="Opaque"} Cull Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;
   fixed4 frag(v2f_img i):SV_Target{return tex2D(_MainTex,i.uv);}
   ENDCG
  }
 }
}
