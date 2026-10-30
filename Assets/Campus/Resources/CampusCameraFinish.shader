Shader "Hidden/CampusCameraFinish"
{
    Properties{_MainTex("Scene",2D)="white"{}}
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;float4 _MainTex_TexelSize;
            sampler2D _CameraDepthNormalsTexture;
            float depth(float2 uv){float d;float3 n;DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv),d,n);return d;}
            fixed4 frag(v2f_img i):SV_Target
            {
                float4 c=tex2D(_MainTex,i.uv);float center=depth(i.uv)*_ProjectionParams.z;
                float occlusion=0;
                const float2 directions[8]={float2(1,0),float2(-1,0),float2(0,1),float2(0,-1),float2(.7,.7),float2(-.7,.7),float2(.7,-.7),float2(-.7,-.7)};
                // Small-radius contact shading, intentionally subtle and disabled with low graphics.
                for(int k=0;k<8;k++)
                {
                    float sampleDepth=depth(i.uv+directions[k]*_MainTex_TexelSize.xy*5)*_ProjectionParams.z;
                    float delta=center-sampleDepth;occlusion+=smoothstep(.015,.09,delta)*(1-smoothstep(.15,.4,delta));
                }
                c.rgb*=1-occlusion*.018;
                // Soft highlight roll-off keeps cream materials warm rather than glowing white.
                float3 bloom=0;
                for(int b=0;b<8;b++){float3 v=tex2D(_MainTex,i.uv+directions[b]*_MainTex_TexelSize.xy*9).rgb;bloom+=max(v-.85,0);}
                c.rgb+=bloom*.009;c.rgb*=1.03;
                c.rgb=c.rgb/(1+c.rgb*.23);
                float l=dot(c.rgb,float3(.2126,.7152,.0722));c.rgb=lerp(l.xxx,c.rgb,1.07);
                float2 v=i.uv-.5;c.rgb*=1-dot(v,v)*.12;
                return c;
            }
            ENDCG
        }
    }
}
