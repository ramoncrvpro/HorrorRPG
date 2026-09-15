# RIC RetroVision

RIC RetroVision is a full-screen VHS and analog signal effect for the Universal Render Pipeline, published by Rat in a Cup.

## Included

- Render Graph-compatible `VHSRendererFeature` runtime feature.
- `RICRetroVisionPreset` ScriptableObject presets.
- Editor tools for applying presets and restoring defaults.
- Shader `Hidden/RatInACup/RICRetroVision`.
- Installation, API, parameter, and licensing documentation.

The web demonstration, scene, UI controller, sprites, fonts, renderer data, and project settings are not included in this installable asset.

## Requirements

The initial validation target is Unity `6000.3.17f1` with URP `17.3.0` and Render Graph enabled. A Universal Render Pipeline project is required.

## Quick start

1. Import this folder into the project.
2. Open your own `Universal Renderer Data` asset.
3. Add `VHSRendererFeature` to its Renderer Features list.
4. Assign `Runtime/Shaders/RICRetroVision.shader` to the feature's Shader field if Unity did not resolve it automatically.
5. Enter Play Mode with a Game camera, or inspect the effect in a Scene View.

Use the included presets from the feature Inspector. The feature changes the parameters of its existing material; applying a preset does not create a new material or renderer.

## Compatibility scope

The effect is prepared for Unity 6 and URP Render Graph. The original validation scene uses the 2D Renderer. Forward/Universal Renderer and additional Unity versions require validation in the target project before being advertised as supported.
