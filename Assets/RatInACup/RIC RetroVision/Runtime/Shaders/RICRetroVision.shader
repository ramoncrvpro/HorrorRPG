// RIC RetroVision full-screen shader.

Shader "Hidden/RatInACup/RICRetroVision"
{
    Properties
    {
        [HideInInspector] _VHSTimeScale ("Time Scale", Float) = 1
        [HideInInspector] _VHSRandomSeed ("Random Seed", Float) = 13.37
        [HideInInspector] _VHSChromaAmount ("Chroma Amount", Float) = 0.003
        [HideInInspector] _VHSRedBleedBoost ("Red Bleed Boost", Float) = 1.25
        [HideInInspector] _VHSNoiseIntensity ("Noise Intensity", Float) = 0.12821707
        [HideInInspector] _VHSNoiseBandFrequency ("Noise Band Frequency", Float) = 180
        [HideInInspector] _VHSNoiseSpeed ("Noise Speed", Float) = 7
        [HideInInspector] _VHSNoiseBandCoverage ("Noise Band Coverage", Float) = 0.22
        [HideInInspector] _VHSScanlineDensity ("Scanline Density", Float) = 720
        [HideInInspector] _VHSScanlineIntensity ("Scanline Intensity", Float) = 0.18
        [HideInInspector] _VHSScanlineFlickerSpeed ("Scanline Flicker Speed", Float) = 8
        [HideInInspector] _VHSScanlineFlickerAmount ("Scanline Flicker Amount", Float) = 0.08
        [HideInInspector] _VHSJitterAmount ("Jitter Amount", Float) = 0.004
        [HideInInspector] _VHSJitterLineGroupSize ("Jitter Line Group Size", Float) = 6
        [HideInInspector] _VHSJitterSpeed ("Jitter Speed", Float) = 12
        [HideInInspector] _VHSTwitchProbability ("Twitch Probability", Float) = 0.03
        [HideInInspector] _VHSTwitchAmount ("Twitch Amount", Float) = 0.01
        [HideInInspector] _VHSTwitchSpeed ("Twitch Speed", Float) = 3
        [HideInInspector] _VHSVerticalJitterAmount ("Vertical Jitter Amount", Float) = 0.003
        [HideInInspector] _VHSVerticalJitterSpeed ("Vertical Jitter Speed", Float) = 2
        [HideInInspector] _VHSDesaturation ("Desaturation", Float) = 0.15
        [HideInInspector] _VHSContrastCrush ("Contrast Crush", Float) = 0.05
        [HideInInspector] _VHSLimitColorPalette ("Limit Color Palette", Float) = 0
        [HideInInspector] _VHSColorLevelsPerChannel ("Color Levels Per Channel", Float) = 16
        [HideInInspector] _VHSPixelation ("Pixelation", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "RIC RetroVision Full Screen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _VHSTimeScale;
                float _VHSRandomSeed;
                float _VHSChromaAmount;
                float _VHSRedBleedBoost;
                float _VHSNoiseIntensity;
                float _VHSNoiseBandFrequency;
                float _VHSNoiseSpeed;
                float _VHSNoiseBandCoverage;
                float _VHSScanlineDensity;
                float _VHSScanlineIntensity;
                float _VHSScanlineFlickerSpeed;
                float _VHSScanlineFlickerAmount;
                float _VHSJitterAmount;
                float _VHSJitterLineGroupSize;
                float _VHSJitterSpeed;
                float _VHSTwitchProbability;
                float _VHSTwitchAmount;
                float _VHSTwitchSpeed;
                float _VHSVerticalJitterAmount;
                float _VHSVerticalJitterSpeed;
                float _VHSDesaturation;
                float _VHSContrastCrush;
                float _VHSLimitColorPalette;
                float _VHSColorLevelsPerChannel;
                float _VHSPixelation;
            CBUFFER_END

            float VHSHash(float2 value)
            {
                value = frac(value * float2(123.34, 456.21));
                value += dot(value, value + 45.32);
                return frac(value.x * value.y);
            }

            float VHSHash1(float value)
            {
                return VHSHash(float2(value, value + 17.31));
            }

            half4 VHSSample(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, saturate(uv), _BlitMipLevel);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float time = _TimeParameters.y * _VHSTimeScale + _VHSRandomSeed;
                float screenWidth = max(_ScreenParams.x, 1.0);
                float screenHeight = max(_ScreenParams.y, 1.0);

                float verticalCycle = floor(time * max(_VHSVerticalJitterSpeed, 0.0));
                float verticalOffset = (VHSHash1(verticalCycle + 1.7) * 2.0 - 1.0) * _VHSVerticalJitterAmount;
                uv.y += verticalOffset;

                float twitchCycle = floor(time * max(_VHSTwitchSpeed, 0.0));
                float twitchRoll = VHSHash1(twitchCycle + 92.1);
                float twitchOffset = twitchRoll < _VHSTwitchProbability
                    ? (VHSHash1(twitchCycle + 21.4) * 2.0 - 1.0) * _VHSTwitchAmount
                    : 0.0;
                uv.x += twitchOffset;

                float rowValue = floor(uv.y * screenHeight / max(_VHSJitterLineGroupSize, 1.0));
                float jitterCycle = floor(time * max(_VHSJitterSpeed, 0.0));
                float jitterOffsetValue = (VHSHash(float2(rowValue, jitterCycle) + _VHSRandomSeed) * 2.0 - 1.0) * _VHSJitterAmount;
                uv.x += jitterOffsetValue;

                if (_VHSPixelation > 1.0)
                {
                    float2 pixelGrid = float2(screenWidth, screenHeight) / _VHSPixelation;
                    uv = (floor(uv * pixelGrid) + 0.5) / pixelGrid;
                }

                float chroma = _VHSChromaAmount;
                half red = VHSSample(uv + float2(chroma * _VHSRedBleedBoost, 0.0)).r;
                half green = VHSSample(uv).g;
                half blue = VHSSample(uv - float2(chroma, 0.0)).b;
                half3 color = half3(red, green, blue);

                float noiseBand = floor(uv.y * max(_VHSNoiseBandFrequency, 1.0));
                float noiseCycle = floor(time * max(_VHSNoiseSpeed, 0.0));
                float bandMask = step(1.0 - _VHSNoiseBandCoverage, VHSHash(float2(noiseBand, noiseCycle) + _VHSRandomSeed));
                float staticNoise = VHSHash(float2(uv.x * screenHeight, uv.y * screenHeight) + noiseCycle) * 2.0 - 1.0;
                color += staticNoise * bandMask * _VHSNoiseIntensity;

                float scanline = sin(uv.y * _VHSScanlineDensity * 6.2831853 + time * _VHSScanlineFlickerSpeed);
                float flicker = 1.0 + scanline * _VHSScanlineFlickerAmount;
                float scanlineMask = 1.0 - ((scanline * 0.5 + 0.5) * _VHSScanlineIntensity);
                color *= scanlineMask * flicker;

                half luminance = dot(color, half3(0.2126, 0.7152, 0.0722));
                color = lerp(color, luminance.xxx, saturate(_VHSDesaturation));
                color = lerp(color, 0.5.xxx, saturate(_VHSContrastCrush));

                if (_VHSLimitColorPalette > 0.5)
                {
                    float levelScale = max(_VHSColorLevelsPerChannel - 1.0, 1.0);
                    color = floor(saturate(color) * levelScale + 0.5) / levelScale;
                }

                return half4(saturate(color), 1.0);
            }
            ENDHLSL
        }
    }
}
