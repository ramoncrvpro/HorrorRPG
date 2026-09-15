using System;
using UnityEngine;

namespace RatInACup.RICRetroVision
{
    /// <summary>Stores the visual parameters used by the RIC RetroVision full-screen effect.</summary>
    [Serializable]
    public sealed class RetroVisionSettings
    {
        private const float DefaultTimeScale = 1f;
        private const float DefaultRandomSeed = 13.37f;
        private const float DefaultChromaAmount = 0.003f;
        private const float DefaultRedBleedBoost = 1.25f;
        private const float DefaultNoiseIntensity = 0.12821707f;
        private const float DefaultNoiseBandFrequency = 180f;
        private const float DefaultNoiseSpeed = 7f;
        private const float DefaultNoiseBandCoverage = 0.22f;
        private const float DefaultScanlineDensity = 720f;
        private const float DefaultScanlineIntensity = 0.18f;
        private const float DefaultScanlineFlickerSpeed = 8f;
        private const float DefaultScanlineFlickerAmount = 0.08f;
        private const float DefaultJitterAmount = 0.004f;
        private const float DefaultJitterLineGroupSize = 6f;
        private const float DefaultJitterSpeed = 12f;
        private const float DefaultTwitchProbability = 0.03f;
        private const float DefaultTwitchAmount = 0.01f;
        private const float DefaultTwitchSpeed = 3f;
        private const float DefaultVerticalJitterAmount = 0.003f;
        private const float DefaultVerticalJitterSpeed = 2f;
        private const float DefaultDesaturation = 0.15f;
        private const float DefaultContrastCrush = 0.05f;
        private const float DefaultColorLevelsPerChannel = 16f;
        private const float DefaultPixelation = 1f;

        [Header("Time And Randomness")]
        [Min(0f)] public float timeScale = DefaultTimeScale;
        public float randomSeed = DefaultRandomSeed;

        [Header("Chromatic Bleeding")]
        [Range(0f, 0.05f)] public float chromaAmount = DefaultChromaAmount;
        [Min(0f)] public float redBleedBoost = DefaultRedBleedBoost;

        [Header("Tape Noise")]
        [Range(0f, 1f)] public float noiseIntensity = DefaultNoiseIntensity;
        [Min(1f)] public float noiseBandFrequency = DefaultNoiseBandFrequency;
        [Min(0f)] public float noiseSpeed = DefaultNoiseSpeed;
        [Range(0f, 1f)] public float noiseBandCoverage = DefaultNoiseBandCoverage;

        [Header("Scanlines")]
        [Min(0f)] public float scanlineDensity = DefaultScanlineDensity;
        [Range(0f, 1f)] public float scanlineIntensity = DefaultScanlineIntensity;
        [Min(0f)] public float scanlineFlickerSpeed = DefaultScanlineFlickerSpeed;
        [Range(0f, 1f)] public float scanlineFlickerAmount = DefaultScanlineFlickerAmount;

        [Header("Line Jitter")]
        [Range(0f, 0.05f)] public float jitterAmount = DefaultJitterAmount;
        [Min(1f)] public float jitterLineGroupSize = DefaultJitterLineGroupSize;
        [Min(0f)] public float jitterSpeed = DefaultJitterSpeed;

        [Header("Frame Twitch")]
        [Range(0f, 1f)] public float twitchProbability = DefaultTwitchProbability;
        [Range(0f, 0.05f)] public float twitchAmount = DefaultTwitchAmount;
        [Min(0f)] public float twitchSpeed = DefaultTwitchSpeed;

        [Header("Vertical Instability")]
        [Range(0f, 0.05f)] public float verticalJitterAmount = DefaultVerticalJitterAmount;
        [Min(0f)] public float verticalJitterSpeed = DefaultVerticalJitterSpeed;

        [Header("Color Grading")]
        [Range(0f, 1f)] public float desaturation = DefaultDesaturation;
        [Range(0f, 1f)] public float contrastCrush = DefaultContrastCrush;

        [Header("Color Palette")]
        public bool limitColorPalette;
        [Range(2f, 256f)] public float colorLevelsPerChannel = DefaultColorLevelsPerChannel;

        [Header("Pixelation")]
        [Range(1f, 64f)] public float pixelation = DefaultPixelation;

        /// <summary>Restores every setting to the RIC RetroVision default.</summary>
        public void ResetToDefaults()
        {
            timeScale = DefaultTimeScale;
            randomSeed = DefaultRandomSeed;
            chromaAmount = DefaultChromaAmount;
            redBleedBoost = DefaultRedBleedBoost;
            noiseIntensity = DefaultNoiseIntensity;
            noiseBandFrequency = DefaultNoiseBandFrequency;
            noiseSpeed = DefaultNoiseSpeed;
            noiseBandCoverage = DefaultNoiseBandCoverage;
            scanlineDensity = DefaultScanlineDensity;
            scanlineIntensity = DefaultScanlineIntensity;
            scanlineFlickerSpeed = DefaultScanlineFlickerSpeed;
            scanlineFlickerAmount = DefaultScanlineFlickerAmount;
            jitterAmount = DefaultJitterAmount;
            jitterLineGroupSize = DefaultJitterLineGroupSize;
            jitterSpeed = DefaultJitterSpeed;
            twitchProbability = DefaultTwitchProbability;
            twitchAmount = DefaultTwitchAmount;
            twitchSpeed = DefaultTwitchSpeed;
            verticalJitterAmount = DefaultVerticalJitterAmount;
            verticalJitterSpeed = DefaultVerticalJitterSpeed;
            desaturation = DefaultDesaturation;
            contrastCrush = DefaultContrastCrush;
            limitColorPalette = false;
            colorLevelsPerChannel = DefaultColorLevelsPerChannel;
            pixelation = DefaultPixelation;
        }

        /// <summary>Copies and normalizes values from another settings instance.</summary>
        /// <param name="source">The settings to copy.</param>
        public void CopyFrom(RetroVisionSettings source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "RIC RetroVision settings cannot be null.");

            timeScale = source.timeScale;
            randomSeed = source.randomSeed;
            chromaAmount = source.chromaAmount;
            redBleedBoost = source.redBleedBoost;
            noiseIntensity = source.noiseIntensity;
            noiseBandFrequency = source.noiseBandFrequency;
            noiseSpeed = source.noiseSpeed;
            noiseBandCoverage = source.noiseBandCoverage;
            scanlineDensity = source.scanlineDensity;
            scanlineIntensity = source.scanlineIntensity;
            scanlineFlickerSpeed = source.scanlineFlickerSpeed;
            scanlineFlickerAmount = source.scanlineFlickerAmount;
            jitterAmount = source.jitterAmount;
            jitterLineGroupSize = source.jitterLineGroupSize;
            jitterSpeed = source.jitterSpeed;
            twitchProbability = source.twitchProbability;
            twitchAmount = source.twitchAmount;
            twitchSpeed = source.twitchSpeed;
            verticalJitterAmount = source.verticalJitterAmount;
            verticalJitterSpeed = source.verticalJitterSpeed;
            desaturation = source.desaturation;
            contrastCrush = source.contrastCrush;
            limitColorPalette = source.limitColorPalette;
            colorLevelsPerChannel = source.colorLevelsPerChannel;
            pixelation = source.pixelation;
            Normalize();
        }

        /// <summary>Clamps values to the ranges supported by the shader.</summary>
        public void Normalize()
        {
            timeScale = Mathf.Max(0f, timeScale);
            chromaAmount = Mathf.Clamp(chromaAmount, 0f, 0.05f);
            redBleedBoost = Mathf.Max(0f, redBleedBoost);
            noiseIntensity = Mathf.Clamp01(noiseIntensity);
            noiseBandFrequency = Mathf.Max(1f, noiseBandFrequency);
            noiseSpeed = Mathf.Max(0f, noiseSpeed);
            noiseBandCoverage = Mathf.Clamp01(noiseBandCoverage);
            scanlineDensity = Mathf.Max(0f, scanlineDensity);
            scanlineIntensity = Mathf.Clamp01(scanlineIntensity);
            scanlineFlickerSpeed = Mathf.Max(0f, scanlineFlickerSpeed);
            scanlineFlickerAmount = Mathf.Clamp01(scanlineFlickerAmount);
            jitterAmount = Mathf.Clamp(jitterAmount, 0f, 0.05f);
            jitterLineGroupSize = Mathf.Max(1f, jitterLineGroupSize);
            jitterSpeed = Mathf.Max(0f, jitterSpeed);
            twitchProbability = Mathf.Clamp01(twitchProbability);
            twitchAmount = Mathf.Clamp(twitchAmount, 0f, 0.05f);
            twitchSpeed = Mathf.Max(0f, twitchSpeed);
            verticalJitterAmount = Mathf.Clamp(verticalJitterAmount, 0f, 0.05f);
            verticalJitterSpeed = Mathf.Max(0f, verticalJitterSpeed);
            desaturation = Mathf.Clamp01(desaturation);
            contrastCrush = Mathf.Clamp01(contrastCrush);
            colorLevelsPerChannel = Mathf.Clamp(colorLevelsPerChannel, 2f, 256f);
            pixelation = Mathf.Clamp(pixelation, 1f, 64f);
        }
    }
}
