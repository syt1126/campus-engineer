Shader "Campus/Soft Sky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct input {float4 vertex:POSITION;};
            struct varying {float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
            varying vert(input v){varying o;o.vertex=UnityObjectToClipPos(v.vertex);o.direction=v.vertex.xyz;return o;}
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
            fixed4 frag(varying i):SV_Target
            {
                float3 d=normalize(i.direction);float h=saturate(d.y);
                float3 color=lerp(float3(.76,.85,.85),float3(.32,.56,.79),pow(h,.5));
                float2 p=d.xz/max(.08,d.y)*2.3+float2(_Time.y*.002,0);
                float n=noise(p)*.57+noise(p*2.03)*.28+noise(p*4.1)*.15;
                float cloud=smoothstep(.56,.72,n)*smoothstep(.07,.23,h)*(1-smoothstep(.82,1,h));
                color=lerp(color,float3(.98,.97,.92),cloud*.88);
                color=lerp(float3(.66,.72,.64),color,smoothstep(-.08,.03,d.y));return fixed4(color,1);
            }
            ENDCG
        }
    }
}
