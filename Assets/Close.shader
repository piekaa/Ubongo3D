Shader "Piekoszek/Close"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cut ("Cut", Range(0.0, 1)) = 0.5
    }
    SubShader
    {
        Cull off ZWrite Off ZTest Always

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

            sampler2D _MainTex;
            float _Cut;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 s = _MainTex_TexelSize;
                const sampler2D m = _MainTex;
                const float2 uv = i.uv;

                fixed4 sum = tex2D(m, uv + float2(-s.x, s.y)) + tex2D(m, uv + float2(0, s.y)) + tex2D(
                        m, uv + float2(s.x, s.y))
                    + tex2D(m, uv + float2(-s.x, 0)) + tex2D(m, uv + float2(0, 0)) + tex2D(m, uv + float2(s.x, 0))
                    + tex2D(m, uv + float2(-s.x, -s.y)) + tex2D(m, uv + float2(0, -s.y)) + tex2D(
                        m, uv + float2(s.x, -s.y));

                // sample the texture
                float st = step(3, sum.r);
                return fixed4(st, 1 - st, 0, 1);
            }
            ENDCG
        }
    }
}