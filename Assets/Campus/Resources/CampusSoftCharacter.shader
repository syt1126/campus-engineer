Shader "Campus/SoftCharacter"
{
    Properties { _MainTex("Painted color",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Cutoff("Alpha cutoff",Range(0,1))=.03 _Cull("Cull",Float)=2 }
    SubShader
    {
        Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest"}
        Cull [_Cull]
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            sampler2D _MainTex;float4 _MainTex_ST,_Color;float _Cutoff;
            struct V{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
            struct F{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;};
            F vert(V v){F o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.normal=UnityObjectToWorldNormal(v.normal);return o;}
            fixed4 frag(F i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*_Color;clip(c.a-_Cutoff);
                float diffuse=smoothstep(-.3,.65,dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)));
                // Painted eyes/face stay legible in interior light. No shiny skin or deep eye sockets.
                c.rgb*=lerp(float3(.77,.8,.86),float3(1.02,1.01,.98),diffuse);return c;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/Transparent/Cutout/VertexLit/SHADOWCASTER"
    }
    FallBack "Transparent/Cutout/Diffuse"
}
