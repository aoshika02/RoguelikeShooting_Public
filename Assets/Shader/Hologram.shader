Shader "Universal Render Pipeline/UI/HologramAlphaCut"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _ScanSpeed ("Scanline Speed", Range(0.05, 1)) = 0.05
        _ScanDensity ("Scanline Density", Range(10, 300)) = 200
        _Flicker("Flicker",Range(0,100)) = 5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "HologramUI"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float _ScanSpeed;
            float _ScanDensity;
            float _Flicker;

            struct appdata 
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata  v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                if (col.a < 0.01) discard;
                float flickerRatio = _Flicker/100;
                col.rgb *= (sin((i.uv.y + _Time.y * _ScanSpeed) * _ScanDensity) * flickerRatio + 1 - flickerRatio);
                return col;
            }
            ENDHLSL
        }
    }
}