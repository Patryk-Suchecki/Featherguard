Shader "Featherguard/Flame Billboard"
{
    Properties
    {
        [MainTexture] _BaseMap("Flipbook", 2D) = "white" {}
        [HDR] _Tint("Tint", Color) = (1.6, 1.1, 0.8, 1)
        _Columns("Columns", Float) = 8
        _Rows("Rows", Float) = 8
        _FramesPerSecond("Frames Per Second", Float) = 18
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Flame"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ColorMask RGB
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Tint;
                float _Columns;
                float _Rows;
                float _FramesPerSecond;
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
                half fogFactor : TEXCOORD1;
            };

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
                output.positionCS = TransformWorldToHClip(positionWS);

                float frames = _Columns * _Rows;
                float phase = frac(sin(dot(centre.xz, float2(12.9898, 78.233))) * 43758.5453) * frames;
                float frame = fmod(floor(_Time.y * _FramesPerSecond + phase), frames);
                float column = fmod(frame, _Columns);
                float row = _Rows - 1 - floor(frame / _Columns);
                output.uv = (input.uv + float2(column, row)) / float2(_Columns, _Rows);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 flame = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 color = min(flame.rgb * _Tint.rgb * saturate(flame.a * _Tint.a), 8.0h);
                return half4(MixFogColor(color, half3(0, 0, 0), input.fogFactor), 0);
            }
            ENDHLSL
        }
    }
}
