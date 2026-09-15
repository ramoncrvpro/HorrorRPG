# Installation

## Requirements

- Unity `6000.3.17f1` or a later Unity 6 version pending project validation.
- Universal Render Pipeline `17.3.0` or a compatible URP version pending validation.
- Render Graph enabled.

## Setup

1. Import the `RIC RetroVision` folder into `Assets`.
2. Select the project's own `Universal Renderer Data` asset.
3. Add `VHSRendererFeature` to the Renderer Features list.
4. Keep the serialized shader reference assigned to `Runtime/Shaders/RICRetroVision.shader`.
5. If the reference is empty, assign the shader manually. The runtime fallback is `Hidden/RatInACup/RICRetroVision`.
6. Save the Renderer Data asset.

The pass runs at `AfterRenderingPostProcessing` and affects only `Game` and `SceneView` cameras. It skips frames whose active color target is the back buffer or whose source texture is invalid.

## Shader stripping

Keep the shader assigned explicitly on the feature when using a player build. Shader stripping can remove a shader that is only loaded through `Shader.Find`; the serialized reference is the recommended path.

## Scope

This asset contains no scene, prefab, runtime UI, EventSystem, sprites, fonts, demo controller, `ProjectSettings`, URP asset, or Renderer Data asset. Configure those in the consuming project.
