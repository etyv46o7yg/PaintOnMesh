// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/UnwrapShader"
    {
    Properties
        {
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleParam("Scale_of_center", Range(0.0, 1.0)) = 0.5
        }
    SubShader
        {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off

        Pass
            {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
                {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                };

            struct v2f
                {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color  : COLOR_0;
                };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float scale, _ScaleParam;
            float4 bias;

            v2f vert (appdata v)
                {
                v2f o;
                //v.vertex = float4(v.uv.xy * 2 - float2(1, 1), 0.0, 1.0);
                o.vertex = float4(  ( float2(1, 1) - v.uv.xy * 2), 0.0, 1.0);
                o.vertex.x = -o.vertex.x;
                o.color = (v.vertex - bias) / scale;
                return o;
                }

            fixed4 frag (v2f i) : SV_Target
                {
                
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return i.color;
                }
            ENDCG
            }
        }
    }
