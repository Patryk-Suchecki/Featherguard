Shader "Featherguard/Toon Flame"
{
    Properties
    {
        [HDR] _CoreColor("Core", Color) = (1.25, 0.95, 0.3, 1)
        [HDR] _MiddleColor("Middle", Color) = (1.15, 0.42, 0.05, 1)
        [HDR] _EdgeColor("Edge", Color) = (0.75, 0.12, 0.02, 1)
        _OutlineColor("Outline", Color) = (0.22, 0.04, 0.02, 1)
        _Core("Core Band", Range(0, 1)) = 0.62
        _Middle("Middle Band", Range(0, 1)) = 0.3
        _Outline("Outline Band", Range(0, 0.3)) = 0.08
        _Speed("Speed", Float) = 2.2
        _Sway("Sway", Range(0, 1)) = 0.35
        _Forward("Towards Camera", Range(0, 0.5)) = 0.07
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ToonFlame"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _MiddleColor;
                half4 _EdgeColor;
                half4 _OutlineColor;
                half _Core;
                half _Middle;
                half _Outline;
                float _Speed;
                half _Sway;
                float _Forward;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float seed : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), u.x), lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), u.x), u.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 centre = TransformObjectToWorld(float3(0, 0, 0));
                float width = length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x));
                float height = length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y));
                float3 toCamera = _WorldSpaceCameraPos - centre;
                toCamera.y = 0;
                float3 right = normalize(cross(float3(0, 1, 0), normalize(toCamera + float3(1e-4, 0, 0))));
                float3 positionWS = centre + right * input.positionOS.x * width + float3(0, 1, 0) * input.positionOS.y * height;
                positionWS += normalize(_WorldSpaceCameraPos - positionWS) * _Forward;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.seed = Hash(centre.xz) * 37.0;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed + input.seed;
                float y = input.uv.y;
                float x = (input.uv.x - 0.5) * 2.0;
                float rising = Noise(float2(x * 2.5, y * 3.0 - t * 1.6));
                float sway = sin(t * 1.3 + y * 4.0) * 0.5 + (Noise(float2(t * 0.7, y * 2.0)) - 0.5);
                x += sway * _Sway * y * y + (rising - 0.5) * 0.35 * y;
                float width = sqrt(saturate(y * 3.0)) * pow(saturate(1.0 - y), 1.25) * 1.35;
                float flame = 1.0 - abs(x) / max(width, 1e-3);
                flame -= (rising * 0.55 + 0.15) * y * y;
                clip(flame);
                half3 color = flame < _Outline ? _OutlineColor.rgb
                    : flame < _Middle ? _EdgeColor.rgb
                    : flame < _Core ? _MiddleColor.rgb
                    : _CoreColor.rgb;
                return half4(MixFog(color, input.fogFactor), 1);
            }
            ENDHLSL
        }
    }
}
