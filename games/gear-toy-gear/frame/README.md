# GearToyGear on Steam Frame

Standalone Linux ARM64, CoreCLR .NET 10, OpenXR and Vulkan. Requires the lub
revision pinned in the repository README (`Xr.View` / `Xr.Input` / `Xr.Active`
and `XrView.Target`).

## Build

Build lub on Linux ARM64 with its release script:

```sh
timeout 7200 bash scripts/build-release.sh --target lub_shared
```

Collect `liblub.so`, `libopenxr_loader.so.1`, `libSDL3.so.0` and
`libslang-compiler.so.0.2026.8.1` from that build into one directory.
From the tsumiki root, with .NET 10, Python and ffmpeg installed:

```sh
python3 tools/build_gtg_frame.py --lub /path/to/lub \
  --original /path/to/GearToyGear/GearToyGear --native /path/to/arm64-libraries
dotnet run --project tests/gear-toy-gear/frame/FrameTests.csproj \
  -c Release -p:LubRoot=/path/to/lub
```

`--original` is the directory containing `Content` in the original
GearToyGear 0.1 source distribution. The game, rendering, input and audio
C# in `games/gear-toy-gear/` and its shaders are the ones the browser build
compiles to WebAssembly; this directory holds only the native launcher and
build settings. Without an XR session the same code draws a flat 640x480
view and reads the keyboard.

Upload `build/frame/publish` with SteamOS Devkit Client. Use the title ID
`gtg_frame`, command `run.sh`, and Steam Linux Runtime 4 ARM64. For profiling,
set `LUB_PROFILE=1` in the title's environment. Scores live in
`$XDG_DATA_HOME/gear-toy-gear/scores.txt`, defaulting to `~/.local/share`.
Replacing the uploaded directory with an earlier payload rolls back the
game without removing scores.

## Controls

| Action | Frame input |
|---|---|
| Move | Left stick, analog XY |
| Accelerate | Hold right trigger |
| Brake to normal speed | Hold left trigger |
| Start / retry | A |
| Pause / resume | Menu or X |
| Return to title | B while paused |

Weapons fire automatically. Head movement controls only the view. Right
stick, grip and D-pad have no gameplay assignment. The initial head position
and horizontal facing direction place the scene in the room. Game simulation
runs at 60 Hz; one draw list is submitted to both eyes at the headset rate.
