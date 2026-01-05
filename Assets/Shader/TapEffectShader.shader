Shader "Unlit/TapEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Float) = 0.01
        _CircleRadius ("CircleRadius", Float) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Thickness;
            float _CircleRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                const float distancePerPixel = fwidth(i.uv.x);

                const bool isLessThanOnePixel = _Thickness <= distancePerPixel;
                const float thickness = isLessThanOnePixel ? distancePerPixel : _Thickness;

                fixed4 color = _Color;

                color.a *= (isLessThanOnePixel ? _Thickness / distancePerPixel : 1) *
                saturate(0.5 - ((abs(length(i.uv - float2(0.5, 0.5)) - (_CircleRadius - thickness)) - thickness) / distancePerPixel));
                return color;
            }
            ENDCG
        }
    }
}