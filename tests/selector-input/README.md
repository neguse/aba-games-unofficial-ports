# Collection transition input regressions

Run from the repository root with .NET 10:

```sh
dotnet run --project tests/selector-input/SelectorInputTests.csproj
```

This compiles the production `SelectionGate` and verifies release-before-rearm
for select, return, exit and mixed held-button transitions. It reproduces the
menu's exit edge detection after an in-game Escape quit and checks that only a
new Escape press can exit the collection. This is deterministic input-state
coverage, not physical keyboard, controller, desktop rendering or XR validation.

The shader ABI check uses the actual patched Lub shader compiler and parses its
SPIR-V to verify stage-specific descriptor sets and the combined sampled-image
resource expected by Vulkan/SDLGPU:

```sh
python3 tests/selector-input/test_shader_bindings.py --library /path/to/lub/build-release-linux/liblub.so
```

This catches unmapped separate sampler declarations even if a mocked renderer
accepts the draw. It does not render or validate image pixels.
