# Parameters

All values are normalized before they are sent to the shader. The effect runs in this order: vertical instability, frame twitch, line jitter, RGB separation, banded tape noise, scanlines and flicker, desaturation, contrast crush, optional palette quantization, and final saturation.

## Time and randomness

- `Time Scale`: speed multiplier for animated defects. Minimum `0`.
- `Random Seed`: deterministic offset for the procedural noise and instability patterns.

## Chromatic bleeding

- `Chroma Amount`: horizontal RGB separation. Range `0`–`0.05`.
- `Red Bleed Boost`: multiplier for the red-channel offset. Minimum `0`.

## Tape noise

- `Noise Intensity`: strength of procedural static. Range `0`–`1`.
- `Noise Band Frequency`: number of horizontal noise bands. Minimum `1`.
- `Noise Speed`: animation speed of the noise bands. Minimum `0`.
- `Noise Band Coverage`: probability that a band contains static. Range `0`–`1`.

## Scanlines

- `Scanline Density`: scanline frequency in screen space. Minimum `0`.
- `Scanline Intensity`: darkening strength. Range `0`–`1`.
- `Scanline Flicker Speed`: animation speed of the scanline modulation. Minimum `0`.
- `Scanline Flicker Amount`: brightness modulation amount. Range `0`–`1`.

## Line and frame instability

- `Jitter Amount`: horizontal displacement of line groups. Range `0`–`0.05`.
- `Jitter Line Group Size`: number of pixels per jitter group. Minimum `1`.
- `Jitter Speed`: horizontal jitter animation speed. Minimum `0`.
- `Twitch Probability`: chance of a frame twitch each cycle. Range `0`–`1`.
- `Twitch Amount`: horizontal twitch displacement. Range `0`–`0.05`.
- `Twitch Speed`: twitch cycle speed. Minimum `0`.
- `Vertical Jitter Amount`: vertical frame displacement. Range `0`–`0.05`.
- `Vertical Jitter Speed`: vertical instability cycle speed. Minimum `0`.

## Color grading

- `Desaturation`: interpolation toward luminance. Range `0`–`1`.
- `Contrast Crush`: interpolation toward middle gray. Range `0`–`1`.

## Palette and pixelation

- `Limit Color Palette`: enables per-channel color quantization.
- `Color Levels Per Channel`: quantization levels per channel, clamped to `2`–`256`. Total possible colors are approximately `levels³`.
- `Pixelation`: screen-space pixel grid divisor, clamped to `1`–`64`; `1` disables pixelation.

The effect uses `_BlitTexture`, `_BlitMipLevel`, `SAMPLE_TEXTURE2D_X_LOD`, stereo setup, and `_ScreenParams` from the official URP/Core RP shader includes.
