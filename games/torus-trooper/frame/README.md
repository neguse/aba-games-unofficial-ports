# Torus Trooper on Steam Frame

Standalone Linux ARM64, CoreCLR .NET 10, OpenXR and Vulkan. Requires lub's
`vr/frame` branch with the eye-pose and `XrInput.StickClick` APIs
(commit `e54ef44` or later).

## Build

Build lub on Linux ARM64:

```sh
timeout 7200 bash scripts/build-release.sh --target lub_shared
```

Collect `liblub.so`, `libopenxr_loader.so.1`, `libSDL3.so.0` and
`libslang-compiler.so.0.2026.8.1` from that build into one directory.
From the tsumiki root, with .NET 10, Python and ffmpeg installed:

```sh
python3 tools/build_tt_frame.py --lub /path/to/lub \
  --original /path/to/tt --native /path/to/arm64-libraries
```

`--original` is the original Torus Trooper source directory containing
`sounds`, `readme_e.txt` and the barrage data. Upload `build/frame/publish`
with SteamOS Devkit Client, title ID `tt_frame`, command `run.sh`, and
Steam Linux Runtime 4 ARM64. Set `LUB_PROFILE=1` to log frame pacing.
Scores and replay live in `$XDG_DATA_HOME/torus-trooper`, defaulting to
`~/.local/share`. Replacing the payload with an earlier build rolls back
the game without removing these saves.

Native verification requires a desktop Vulkan device and a host build of lub:

```sh
LUB_NATIVE_LIB=/path/to/host/liblub.so dotnet run \
  --project tests/torus-trooper/frame/FrameTests.csproj \
  -c Release -p:LubRoot=/path/to/lub
```

## Controls

| Action | Frame input |
|---|---|
| Turn | Left stick left/right |
| Speed adjustment | Left stick up/down |
| Normal fire | Hold right trigger |
| Charge and slow down | Hold left trigger; release to fire |
| Start / confirm | A |
| Pause / resume | Menu |
| Return to title | B while paused |
| Toggle replay at title | B |
| Switch third / first person | Right stick click |

The title uses the left stick for difficulty and level selection. Head
movement controls only the view. Right stick tilt, grip and D-pad have no
gameplay assignment. Initial head position and horizontal facing place
the scene in the room. Third person is the default: the camera sits behind
and inward from the ship, with lateral tracking lag to show dodges. First
person sits near the ship and follows its circumferential position directly.
Both views use the track's inward normal as up, preserving steering direction
around the tunnel. Ship banking, screen shake and replay zoom do not alter
the view. Switching changes only the camera; it preserves simulation,
controls and head tracking. Simulation advances in 16 ms steps; geometry is
built once per display frame and submitted to both eyes.
