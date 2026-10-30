Shader "Campus/CompanionFur"
{
 Properties{_FurNoise("Short fur density",2D)="white"{} _FurLength("Coat length",Range(0,.02))=.006}
 CGINCLUDE
 #include "UnityCG.cginc"
 #include "Lighting.cginc"
 #include "AutoLight.cginc"
 sampler2D _FurNoise;float _FurLength;
 struct app{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct vary{float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;float4 color:COLOR;SHADOW_COORDS(2)};
 vary makeVert(app v,float layer){vary o;v.vertex.xyz+=v.normal*_FurLength*layer;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;o.color=v.color;TRANSFER_SHADOW(o);return o;}
 vary baseVert(app v){return makeVert(v,0);}vary shell1(app v){return makeVert(v,.20);}vary shell2(app v){return makeVert(v,.4);}vary shell3(app v){return makeVert(v,.6);}vary shell4(app v){return makeVert(v,.8);}vary shell5(app v){return makeVert(v,1);}
 fixed4 shade(vary i,float layer)
 {
   float density=tex2D(_FurNoise,i.uv*float2(2,4)).r;
   if(layer>0)clip(density-.18-layer*.65);
   float3 n=normalize(i.normal);float diffuse=smoothstep(-.25,.9,dot(n,normalize(_WorldSpaceLightPos0.xyz)));
   float shadow=SHADOW_ATTENUATION(i);float3 ambient=max(float3(.26,.27,.28),ShadeSH9(float4(n,1)));
   float3 color=GammaToLinearSpace(i.color.rgb);
   color*=ambient+_LightColor0.rgb*diffuse*shadow*.65;
   color*=lerp(.92,1.05,density)*lerp(.96,1.08,layer);
   return fixed4(color,layer==0?1:.86);
 }
 fixed4 baseFrag(vary i):SV_Target{return shade(i,0);}fixed4 f1(vary i):SV_Target{return shade(i,.2);}fixed4 f2(vary i):SV_Target{return shade(i,.4);}fixed4 f3(vary i):SV_Target{return shade(i,.6);}fixed4 f4(vary i):SV_Target{return shade(i,.8);}fixed4 f5(vary i):SV_Target{return shade(i,1);}
 ENDCG
 SubShader
 {
  Tags{"RenderType"="Opaque" "Queue"="Geometry"}
  Pass{Tags{"LightMode"="ForwardBase"} ZWrite On CGPROGRAM
   #pragma vertex baseVert
   #pragma fragment baseFrag
   #pragma multi_compile_fwdbase
  ENDCG}
  Pass{Tags{"LightMode"="Always"} ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
   #pragma vertex shell1
   #pragma fragment f1
   #pragma multi_compile_fwdbase
  ENDCG}
  Pass{Tags{"LightMode"="Always"} ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
   #pragma vertex shell2
   #pragma fragment f2
   #pragma multi_compile_fwdbase
  ENDCG}
  Pass{Tags{"LightMode"="Always"} ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
   #pragma vertex shell3
   #pragma fragment f3
   #pragma multi_compile_fwdbase
  ENDCG}
  Pass{Tags{"LightMode"="Always"} ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
   #pragma vertex shell4
   #pragma fragment f4
   #pragma multi_compile_fwdbase
  ENDCG}
  Pass{Tags{"LightMode"="Always"} ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
   #pragma vertex shell5
   #pragma fragment f5
   #pragma multi_compile_fwdbase
  ENDCG}
  UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 }
 FallBack "Diffuse"
}
