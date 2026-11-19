Shader "UI/FurnishDepthPreview"
{
    Properties
    {
        _MainTex ("Depth (m)", 2D) = "black" {}
        _MaxDist ("Max Distance (m)", Float) = 3
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _MaxDist;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = tex2D(_MainTex, i.uv).r;
                if (d <= 0.0) return fixed4(0, 0, 0, 1);
                float t = saturate(d / _MaxDist);
                return fixed4(1.0 - t, 0.15, t, 1);
            }
            ENDCG
        }
    }
}
