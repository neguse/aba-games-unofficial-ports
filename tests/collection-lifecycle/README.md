# Collection lifecycle integration

This suite compiles the production `native/GameSession.cs` and `native/Canvas.cs`
into a small .NET harness. It loads the actual published assemblies in fresh
collectible `AssemblyLoadContext`s, sharing only Lub, and drives them on the
shared Lub runtime (`liblub.so`) with SDL's `offscreen` video and `dummy` audio.

Coverage:

- Production Canvas shader/font/Text/Rect/Image/desktop presentation runs
- All 13 catalog games: `OnInit`, repeated `OnFrame`, `OnQuit`, final score saves
- Two default passes, reversing the second pass to exercise different switches
  and consecutive re-entry into the final game
- Mu-cade real ODE handles are live during play and zero after `Shutdown`
- Actual native IO/audio/physics/resource and GPU backend dispatch
- Weak-reference checks prove the collectible game contexts are collected
- Synthetic repeated-load, event routing, `Scene.Target` and `Scene.End`,
  init/quit/shutdown exception, idempotent dispose, missing Game, and missing
  OnFrame tests
- Final saves reach the save directory before teardown, different game saves stay
  isolated, and all saves use a temporary XDG data directory removed afterward

Rendering goes through the real backend into an offscreen window; pixels are not
compared. This suite makes no claim about visual correctness, controller hardware,
OpenXR, or Steam Frame. Those require the separate renderer/device acceptance workflow.

## Run

Prepare the actual managed games and collection assets using the collection build
instructions. Build Lub's `lub_shared` CMake target and Mu-cade's native physics
bridge. Then, with the .NET 10 toolchain on PATH:

```sh
python3 tools/check_collection_lifecycle.py \
  --lub ../lub \
  --package build/collection/publish \
  --dist build/collection-assets \
  --physics build/native-physics/libmucade_physics.so
```

`--dist` stages assets into `build/collection-lifecycle/package`, using production
packaging rules and preserving the supplied publish tree. Omit it for a package
that already contains assets. `--native /path/to/liblub.so` selects another Lub
build. `--frames 12 --cycles 2` are the defaults; increase them for a longer smoke
test. Masashikun always runs at least 72 frames to cross its original title's
intentional 32-tick no-geometry startup delay. Use `--skip-build` after compiling
the harness/fixtures to repeat a run. Do not use it after changing test source.

`--game <id>` and `--fixtures-only` are diagnostic subsets and do not count as
all-13 validation. The JSON report records the actual games/cycles executed and
the no-pixel/no-hardware limitations. It defaults to
`build/collection-lifecycle/report.json`.

The harness fails rather than silently accepting a game that ends itself early
(`Scene.End`, often a missing or undecodable asset), uncollected ALCs, or a
missing final save.
