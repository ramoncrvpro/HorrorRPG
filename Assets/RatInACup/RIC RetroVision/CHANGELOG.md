# Changelog

## 1.0.0

- Prepared the RIC RetroVision runtime for Unity 6 and URP Render Graph.
- Added namespace `RatInACup.RICRetroVision` while preserving the public type name `VHSRendererFeature`.
- Added serializable settings normalization and runtime preset application.
- Added four commercial presets: Clean VHS, Old Tape, Damaged Signal, and Monochrome Retro.
- Added the Editor preset workflow and installation/API documentation.
- Kept the demo scene and `VHSControlPanel` outside the commercial asset root.

## Compatibility notes

Initial validation targets Unity `6000.3.17f1` and URP `17.3.0`. The source demonstration uses the 2D Renderer. Other renderers and Unity/URP versions require validation in their target projects and are not promised by this release.
