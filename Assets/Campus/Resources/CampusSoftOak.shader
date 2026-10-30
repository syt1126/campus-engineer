Shader "Campus/Soft Oak"
{
    Properties
    {
        _Color ("Warm oak", Color) = (.76,.62,.43,1)
        _MainTex ("Natural grain", 2D) = "gray" {}
        _BumpMap ("Fine grain normal", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = .32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex,_BumpMap;fixed4 _Color;half _Glossiness;
        struct Input {float2 uv_MainTex;float2 uv_BumpMap;};
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            fixed3 grain=tex2D(_MainTex,IN.uv_MainTex).rgb;
            half variation=dot(grain,half3(.299,.587,.114))-.5;
            o.Albedo=_Color.rgb*(.95+variation*.2);
            half3 n=UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap));n.xy*=.12;o.Normal=normalize(n);
            o.Smoothness=_Glossiness;o.Metallic=0;o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Standard"
}
