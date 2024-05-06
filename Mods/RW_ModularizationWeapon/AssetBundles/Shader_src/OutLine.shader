Shader "Unlit/OutLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeSize ("EdgeSize", Range(0,64)) = 0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _EdgeSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float a = 0;
                int s = ceil(_EdgeSize);
                for(int x = -s; x <= s; x++)
                {
                    for(int y = -s; y <= s; y++)
                    {
                        float2 offset = float2(x, y);
                        float len = length(offset);
                        if(len < _EdgeSize)
                        {
                            a += tex2D(_MainTex, i.uv + offset * _MainTex_TexelSize.xy).a * clamp(_EdgeSize - len, 0, 1);
                        }
                    }
                }
                a = clamp(a,0,1);
                float4 col = tex2D(_MainTex, i.uv);
                col.rgb = col.a * col.rgb + (1-col.a)*float3(0,0,0);
                col.a = a;
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
