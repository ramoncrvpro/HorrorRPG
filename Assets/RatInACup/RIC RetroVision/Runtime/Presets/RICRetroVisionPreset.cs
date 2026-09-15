using System;
using UnityEngine;

namespace RatInACup.RICRetroVision
{
    /// <summary>Reusable visual configuration for the RIC RetroVision effect.</summary>
    [CreateAssetMenu(menuName = "Rat in a Cup/RIC RetroVision/Preset", fileName = "RIC RetroVision Preset")]
    public sealed class RICRetroVisionPreset : ScriptableObject
    {
        [SerializeField] private RetroVisionSettings settings = new RetroVisionSettings();

        /// <summary>Gets the serializable settings stored by this preset.</summary>
        public RetroVisionSettings Settings
        {
            get
            {
                if (settings == null)
                    settings = new RetroVisionSettings();
                settings.Normalize();
                return settings;
            }
        }

        /// <summary>Copies this preset into a renderer feature without rebuilding its material.</summary>
        /// <param name="feature">The feature that receives this preset.</param>
        public void ApplyTo(VHSRendererFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature), "RIC RetroVision cannot apply a preset to a null feature.");

            feature.ApplyPreset(this);
        }

        private void OnEnable()
        {
            if (settings == null)
                settings = new RetroVisionSettings();
            settings.Normalize();
        }
    }
}
