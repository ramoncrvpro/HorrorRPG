using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RatInACup.RICRetroVision
{
    /// <summary>Applies the RIC RetroVision VHS-style effect as a URP full-screen render pass.</summary>
    public sealed class VHSRendererFeature : ScriptableRendererFeature
    {
        private const string DefaultShaderName = "Hidden/RatInACup/RICRetroVision";
        private const string DestinationTextureName = "RIC RetroVision Camera Color";
        private const string RenderPassName = "RIC RetroVision Full Screen Pass";

        [SerializeField] private Shader shader;
        [SerializeField] public RetroVisionSettings Settings = new RetroVisionSettings();
        [SerializeField] private RICRetroVisionPreset preset;

        /// <summary>Gets the feature instance currently used by the active renderer.</summary>
        public static VHSRendererFeature ActiveInstance { get; private set; }

        /// <summary>Gets whether the full-screen RIC RetroVision pass is enabled.</summary>
        public bool IsEffectEnabled { get; private set; } = true;

        /// <summary>Gets whether the serialized shader or its fallback can be resolved.</summary>
        public bool HasValidShader => shader != null || Shader.Find(DefaultShaderName) != null;

        private Material material;
        private VHSRenderPass renderPass;

        /// <summary>Creates the RIC RetroVision material and Render Graph pass.</summary>
        public override void Create()
        {
            ActiveInstance = this;
            IsEffectEnabled = true;
            EnsureSettings();

            if (shader == null)
                shader = Shader.Find(DefaultShaderName);

            CoreUtils.Destroy(material);
            material = shader != null ? CoreUtils.CreateEngineMaterial(shader) : null;
            if (material == null)
                Debug.LogError("RIC RetroVision: o shader não pôde ser resolvido. Atribua o shader comercial no campo Shader da feature.", this);

            renderPass = new VHSRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing,
                requiresIntermediateTexture = true
            };
        }

        /// <summary>Adds the pass to eligible Game and Scene View cameras.</summary>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!IsEffectEnabled || material == null || renderPass == null)
                return;

            CameraType cameraType = renderingData.cameraData.camera.cameraType;
            if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
                return;

            EnsureSettings();
            Settings.Normalize();
            ApplySettingsToMaterial();
            renderPass.Setup(material);
            renderer.EnqueuePass(renderPass);
        }

        /// <summary>Enables or disables the complete RIC RetroVision render pass.</summary>
        /// <param name="enabled">Whether the effect should run.</param>
        public void SetEffectEnabled(bool enabled)
        {
            IsEffectEnabled = enabled;
        }

        /// <summary>Restores all settings to their defaults and re-enables the effect.</summary>
        public void ResetSettingsToDefaults()
        {
            EnsureSettings();
            Settings.ResetToDefaults();
            IsEffectEnabled = true;
        }

        /// <summary>Copies a preset into this feature without recreating its material.</summary>
        /// <param name="sourcePreset">The preset to apply.</param>
        public void ApplyPreset(RICRetroVisionPreset sourcePreset)
        {
            if (sourcePreset == null)
                throw new ArgumentNullException(nameof(sourcePreset), "RIC RetroVision não pode aplicar um preset nulo.");

            EnsureSettings();
            Settings.CopyFrom(sourcePreset.Settings);
        }

        /// <summary>Releases the internally created material.</summary>
        protected override void Dispose(bool disposing)
        {
            if (ActiveInstance == this)
                ActiveInstance = null;

            CoreUtils.Destroy(material);
            material = null;
            renderPass = null;
        }

        private void EnsureSettings()
        {
            if (Settings == null)
                Settings = new RetroVisionSettings();
        }

        private void ApplySettingsToMaterial()
        {
            material.SetFloat("_VHSTimeScale", Settings.timeScale);
            material.SetFloat("_VHSRandomSeed", Settings.randomSeed);
            material.SetFloat("_VHSChromaAmount", Settings.chromaAmount);
            material.SetFloat("_VHSRedBleedBoost", Settings.redBleedBoost);
            material.SetFloat("_VHSNoiseIntensity", Settings.noiseIntensity);
            material.SetFloat("_VHSNoiseBandFrequency", Settings.noiseBandFrequency);
            material.SetFloat("_VHSNoiseSpeed", Settings.noiseSpeed);
            material.SetFloat("_VHSNoiseBandCoverage", Settings.noiseBandCoverage);
            material.SetFloat("_VHSScanlineDensity", Settings.scanlineDensity);
            material.SetFloat("_VHSScanlineIntensity", Settings.scanlineIntensity);
            material.SetFloat("_VHSScanlineFlickerSpeed", Settings.scanlineFlickerSpeed);
            material.SetFloat("_VHSScanlineFlickerAmount", Settings.scanlineFlickerAmount);
            material.SetFloat("_VHSJitterAmount", Settings.jitterAmount);
            material.SetFloat("_VHSJitterLineGroupSize", Settings.jitterLineGroupSize);
            material.SetFloat("_VHSJitterSpeed", Settings.jitterSpeed);
            material.SetFloat("_VHSTwitchProbability", Settings.twitchProbability);
            material.SetFloat("_VHSTwitchAmount", Settings.twitchAmount);
            material.SetFloat("_VHSTwitchSpeed", Settings.twitchSpeed);
            material.SetFloat("_VHSVerticalJitterAmount", Settings.verticalJitterAmount);
            material.SetFloat("_VHSVerticalJitterSpeed", Settings.verticalJitterSpeed);
            material.SetFloat("_VHSDesaturation", Settings.desaturation);
            material.SetFloat("_VHSContrastCrush", Settings.contrastCrush);
            material.SetFloat("_VHSLimitColorPalette", Settings.limitColorPalette ? 1f : 0f);
            material.SetFloat("_VHSColorLevelsPerChannel", Settings.colorLevelsPerChannel);
            material.SetFloat("_VHSPixelation", Settings.pixelation);
        }

        private sealed class VHSRenderPass : ScriptableRenderPass
        {
            private Material material;

            public void Setup(Material sourceMaterial)
            {
                material = sourceMaterial;
            }

            private sealed class PassData
            {
                public TextureHandle source;
                public Material material;
            }

            private static void ExecutePass(PassData data, RasterGraphContext context)
            {
                Blitter.BlitTexture(context.cmd, data.source, Vector2.one, data.material, 0);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                if (resourceData.isActiveTargetBackBuffer || material == null)
                    return;

                CameraType cameraType = cameraData.camera.cameraType;
                if (cameraType != CameraType.Game && cameraType != CameraType.SceneView)
                    return;

                TextureHandle source = resourceData.activeColorTexture;
                if (!source.IsValid())
                    return;

                TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = DestinationTextureName;
                destinationDesc.clearBuffer = false;
                destinationDesc.depthBufferBits = 0;
                destinationDesc.msaaSamples = MSAASamples.None;
                TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(RenderPassName, out PassData passData))
                {
                    passData.source = source;
                    passData.material = material;
                    builder.UseTexture(source, AccessFlags.Read);
                    builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }

                resourceData.cameraColor = destination;
            }
        }
    }
}
