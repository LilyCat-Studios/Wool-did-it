
Shader "Custom/TerrainHeightVisualization"
{
    Properties
    {
        _MinHeight ("Minimum Height", Float) = 0
        _MaxHeight ("Maximum Height", Float) = 100

        _NegativeColour ("Negative", Color) = (0.0, 0.0, 0.0, 1)
        _LowColor ("Low", Color) = (0.05, 0.15, 0.9, 1)
        _LowMidColor ("Low-Medium", Color) = (0.05, 0.8, 0.3, 1)
        _MidColor ("Medium", Color) = (1, 0.9, 0.05, 1)
        _HighMidColor ("High", Color) = (1, 0.35, 0.02, 1)
        _HighColor ("Highest", Color) = (0.8, 0.02, 0.02, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "TerrainCompatible" = "True"
        }

        Pass
        {
            Name "TerrainHeight"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _MinHeight;
                float _MaxHeight;
                float4 _LowColor;
                float4 _LowMidColor;
                float4 _MidColor;
                float4 _HighMidColor;
                float4 _HighColor;
                float4 _NegativeColour;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float heightWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(
                    IN.positionOS.xyz
                );

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.heightWS = positionWS.y;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float range = max(_MaxHeight - _MinHeight, 0.001);

                float h = saturate(
                    (IN.heightWS - _MinHeight) / range
                );

                float3 color;

                if (h < 0.25)
                {
                    color = lerp(_LowColor.rgb, _LowMidColor.rgb, h / 0.25);
                }
                else if (h < 0.5)
                {
                    color = lerp(_LowMidColor.rgb, _MidColor.rgb, (h - 0.25) / 0.25);
                }
                else if (h < 0.75)
                {
                    color = lerp(_MidColor.rgb, _HighMidColor.rgb, (h - 0.5) / 0.25);
                }
                else
                {
                    color = lerp(_HighMidColor.rgb, _HighColor.rgb, (h - 0.75) / 0.25);
                }

                return half4(color, 1);
            }

            ENDHLSL
        }
    }
}
