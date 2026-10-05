# ABA Games collection

The collection contains all thirteen games. `./run.sh` opens the Linux desktop
selector; `./run.sh --vr` opens the selector in OpenXR. Select a game with the
arrow keys, controller stick or D-pad, then press Enter, A or the right VR
trigger. Mouse selection is also available on desktop.

Return to the list with F12, controller Back + Start, or both VR stick clicks.
This ends the current game session and saves its normal persistent data. It does
not add a pause state or change the game's rules. Escape and the original pause
buttons keep their original in-game meanings. From the list, Escape or B exits.

GearToyGear, Torus Trooper and Mazer Mayhem retain their existing VR views.
The other ten games appear on an anchored 2D screen. R recenters the selector.
The runtime, desktop window and OpenXR session remain alive while switching.
Each game receives a fresh managed assembly and its resources/audio are released
before the next game starts.

The three existing native games keep their existing XDG save directories.
Other games use independent game-specific directories managed by the collection.
Do not delete these directories when updating the game files.

## Build

Use .NET SDK 10 and the pinned Lub checkout, with the collection's Lub changes.
Apply the bundled runtime patch with `python3 tools/apply_collection_lub.py ../lub`.
The patch stays on the pinned commit and is applied automatically by `tools/setup_lub.sh`.
Prepare assets using `tools/build.py`, then build the Lub shared runtime using
its `scripts/build-release.sh`. Stage its native libraries in one directory.

```
python3 tools/build_collection.py --lub ../lub --native build/native --rid linux-x64
```

Use `linux-arm64` when building ARM64 native libraries on that architecture.
The builder checks ELF architectures and does not install or register anything
in Steam. `--managed-only` is a compiler check, not a playable release package.

The self-contained output is `build/collection/publish/`. Keep the native
libraries, per-game data, fonts and all license files together.

The existing Click-to-Install entry and release workflow still describe and
package the three previously released games. They do not install this new
collection or register it in Steam. The collection is currently a separately
built review candidate; its release jobs and easy-install path need updating
before an RC is published. Building the candidate never changes a Steam library.

Native assets can be prepared independently of the WebAssembly build:

```
python3 tools/prepare_collection_assets.py --lub ../lub
python3 tools/build_collection.py --lub ../lub --dist build/collection-assets --native build/native --rid linux-x64
```

The verified cloud build uses .NET 10.0.401, GCC 14.2, CMake 4.4.4 and Slang
2026.8.1. Both native candidates require GLIBC 2.38, GLIBCXX 3.4.32 and
CXXABI 1.3.15 or newer. SDL, Lub and the OpenXR loader require GLIBC 2.38;
the OpenXR loader requires the listed C++ runtime versions. These requirements
come from inspecting the binaries, not from testing a particular SteamOS image.
An older Linux baseline needs the native libraries rebuilt against its sysroot
and C++ toolchain; changing a user's system libraries is not part of installation.

Desktop builds use Vulkan and X11, including XWayland where supplied by the OS.
This candidate does not include SDL's native Wayland driver. VR needs an active
OpenXR runtime and uses the existing dummy-window OpenXR path. Neither a Steam
Deck nor a Steam Frame was used for these cloud builds.

ARM cross builds can select a separate Slang tree with CMake's `SLANG_ROOT`
cache variable. Never replace a host x64 Slang tree with ARM libraries. Use
`--physics` to package a prebuilt, matching-architecture Mu-cade bridge with
its adjacent ODE license files. `tools/check_collection_package.py` verifies
the final file set, ELF architectures, audio/images and dependency metadata.

## Web

The web root is the thirteen-game list. Each game is launched in its own frame.
Use the visible return button or Alt+Escape to leave the game; browser Back also
returns to the list. Each return shuts the game down before discarding its
frame. Original save keys are retained.
