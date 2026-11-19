Shader "FurnishAR/PlaneStencil"
{
    Properties
    {
        _Color ("Fill Color", Color) = (0.1, 0.65, 1, 0.18)
        _LineColor ("Grid Line Color", Color) = (0.2, 0.8, 1, 0.75)
        _CellSize ("Grid Cell Size (m)", Float) = 0.25
        _LineWidth ("Grid Line Width (m)", Float) = 0.012
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        ZWrite Off
        ZTest LEqual
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _LineColor;
            float _CellSize;
            float _LineWidth;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
            };

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.world = mul(unity_ObjectToWorld, vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 g = i.world.xz / _CellSize;
                float2 d = abs(frac(g - 0.5) - 0.5) * _CellSize;
                float2 aa = fwidth(i.world.xz) * 1.5;
                float2 w = max(_LineWidth * 0.5, aa);
                float2 l = 1.0 - smoothstep(w - aa * 0.5, w + aa * 0.5, d);
                float lineMask = max(l.x, l.y);
                lineMask *= saturate(_LineWidth / max(max(aa.x, aa.y), 1e-5));
                return lerp(_Color, _LineColor, lineMask);
            }
            ENDCG
        }
    }
}
