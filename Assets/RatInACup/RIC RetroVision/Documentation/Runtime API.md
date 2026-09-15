# Runtime API

Namespace: `RatInACup.RICRetroVision`

## `VHSRendererFeature`

`VHSRendererFeature` derives from `ScriptableRendererFeature` and records a full-screen Render Graph pass.

- `RetroVisionSettings Settings`: serialized effect parameters.
- `static VHSRendererFeature ActiveInstance`: currently active feature instance.
- `bool IsEffectEnabled`: current pass state.
- `void SetEffectEnabled(bool enabled)`: enables or disables the pass.
- `void ResetSettingsToDefaults()`: restores defaults and enables the effect.
- `void ApplyPreset(RICRetroVisionPreset preset)`: copies preset values without rebuilding the material.

The feature preserves the public type name `VHSRendererFeature` for the first commercial release. Use the serialized shader reference as the primary shader loading path.

## `RetroVisionSettings`

`RetroVisionSettings` is a serializable parameter container.

- `void ResetToDefaults()` restores the built-in defaults.
- `void CopyFrom(RetroVisionSettings source)` copies and normalizes values.
- `void Normalize()` clamps values to shader-supported ranges.

## `RICRetroVisionPreset`

Create presets through `Assets > Create > Rat in a Cup > RIC RetroVision > Preset`.

- `RetroVisionSettings Settings` returns the preset's settings.
- `void ApplyTo(VHSRendererFeature feature)` applies the preset safely to a feature.

Null features and null presets are rejected without a partial application.
