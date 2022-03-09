Shader "Custom/PaintShader" 
    {
    Properties
        {
        _MainTex("Base (RGB)", 2D) = "" {}
        _Mask   ("mask (RGB)", 2D) = "" {}
        _Noise  ("noise (RGB)", 2D) = "" {}
        _KoeffSpot( "KoeffSpot", Range(0, 1) ) = 0
        _Scale  ("Scale", Float)   = 1

        _Color    ("Position",   Color) = (1,1,1,1)
        _Color_B  ("Position_2",   Color) = (1,1,1,1)
        _ColorFarb("ColorPaint", Color) = (1,1,1,1)       
        }
    SubShader
        {
        ZTest Always Cull Off ZWrite Off Fog { Mode Off }
        Pass 
            {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _Mask;
            uniform sampler2D _Noise;

            uniform fixed4    _Color;
            uniform fixed4     _Color_B;
            uniform fixed4    _ColorFarb;
            uniform float     _Scale;
            uniform float     _Epasseur;
            uniform float     _KoeffSpot;

            float customDist(float4 aPoint, float4 bPoint, float a, float b, float c);
            float distLine(float3 a, float3 b, float3 c);

            float4 frag(v2f_img i) : COLOR 
                {
                fixed4 maskS      = tex2D(_Mask,    i.uv);
                fixed4 albedoS    = tex2D(_MainTex, i.uv);
                fixed4 noiseColor = tex2D(_Noise,   i.uv * 4);

                float koeff = 1.0f; // (0.0f + maskS.r + maskS.g + maskS.b) / 3;
                float prix = 1.5f;

                float rasstX = abs(_Color.r - maskS.r); // (sqrt(abs(_Color.r)));
                float rasstY = abs(_Color.g - maskS.g); // (sqrt(abs(_Color.g)));
                float rasstZ = abs(_Color.b - maskS.b); // (sqrt(abs(_Color.b)));

                float rasstX_B = abs(_Color_B.r - maskS.r); // (sqrt(abs(_Color.r)));
                float rasstY_B = abs(_Color_B.g - maskS.g); // (sqrt(abs(_Color.g)));
                float rasstZ_B = abs(_Color_B.b - maskS.b); // (sqrt(abs(_Color.b)));
                 
                //float dis_1 = length( float3(rasstX, rasstY, rasstZ)       );
                //float dis_2 = length( float3(rasstX_B, rasstY_B, rasstZ_B) );

                float d = distLine( _Color.rgb, _Color_B.rgb, maskS.rgb );

                float dist = d * lerp(1.0f,  noiseColor.r, _KoeffSpot);

                float x = step(0.05f * koeff * _Epasseur / _Scale, dist );

                albedoS = lerp(_ColorFarb, albedoS, x);// +fixed4(0.01f, 0.01f, 0.01f, 0.01f);

                albedoS.a = lerp(_ColorFarb.a, albedoS.a, x);

                return albedoS;
                }
            

            float customDist(float4 aPoint, float4 bPoint, float a, float b, float c)
                {
                float dX = (aPoint.x - bPoint.x);
                float dY = (aPoint.y - bPoint.y);
                float dZ = (aPoint.z - bPoint.z);

                float res = (dX * dX) * a + (dY*dY) * b + (dZ * dZ) * c;
                return sqrt(res);
                }

            float distLine(float3 a, float3 b, float3 c)
                {
                float3 v = b - a;
                float3 w = c - a;

                float c1 = dot(w, v);
                float c2 = dot(v, v);

                if (c1 <= 0)
                    {
                    return distance(c, a);
                    }

                if (c2 <= c1)
                    {
                    return distance(c, b);
                    }

                float bd = c1 / c2;
                float3 Pb = a + bd * v;

                return distance(c, Pb);
                }

            ENDCG
            }

        }
            
    }

