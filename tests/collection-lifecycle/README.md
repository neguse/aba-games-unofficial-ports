# Collection lifecycle integration (cloud-only)

This suite compiles the production `native/GameSession.cs`, `HostBridge.cs`, and
`MasashikunPanel.cs` into a small .NET harness. It loads the actual published
assemblies in fresh collectible `AssemblyLoadContext`s, sharing only Lub.

Coverage:

- Production Canvas shader/font/Text/Rect/Image/desktop presentation, plus the
  Masashikun menu and ranking UI, reach actual runtime draw dispatch
- All 13 catalog games: `OnInit`, repeated `OnFrame`, `OnQuit`, final score saves
- Two default passes, reversing the second pass to exercise different switches
  and consecutive re-entry into the final game
- Mu-cade real ODE handles are live during play and zero after `Shutdown`
- Actual native IO/audio/physics/resource dispatch, shader metadata compilation
- Native session resource reset and managed-hook detachment after each game
- Weak-reference checks prove the collectible game contexts are collected
- Synthetic repeated-load, event routing, init/quit/shutdown exception,
  idempotent dispose, missing Game, and missing OnFrame tests
- Final saves reach the host before teardown, different game saves stay isolated,
  and all saves use a temporary XDG data directory removed afterward

The Lub test fixture replaces the Renderer with a no-pixel backend and uses SDL
`dummy` video/audio. Draw calls are counted, not rendered. This suite makes no
claim about visual correctness, GPU drivers, controller hardware, OpenXR, or
Steam Frame. Those require the separate renderer/device acceptance workflow.

## Run

Prepare the actual managed games and full web assets using the collection build
instructions. Build Lub's test-only `lub_session_test_host` CMake target and
Mu-cade's native physics bridge. Then, with the .NET 10 toolchain on PATH:

```sh
python3 tools/check_collection_lifecycle.py \
  --lub ../lub \
  --package build/collection/publish \
  --dist dist \
  --physics build/native-physics/libmucade_physics.so
```

`--dist` stages assets into `build/collection-lifecycle/package`, using production
packaging rules and preserving the supplied publish tree. Omit it for a package
that already contains assets. `--native /path/to/liblub_session_test_host.so`
selects another test fixture build. `--frames 12 --cycles 2` are the defaults;
increase them for a longer smoke test. Masashikun always runs at least 72 frames
to cross its original title's intentional 32-tick no-geometry startup delay; the
report records each game's actual frame and draw-dispatch counts. Use `--skip-build` after compiling the
harness/fixtures to repeat a run. Do not use it after changing test source.

`--game <id>` and `--fixtures-only` are diagnostic subsets and do not count as
all-13 validation. The JSON report records the actual games/cycles executed and
explicit mock-rendering/no-hardware limitations. It defaults to
`build/collection-lifecycle/report.json`.

The harness fails rather than silently accepting missing assets, audio failures,
missing render dispatch, uncollected ALCs, surviving native allocations, or an
early game-requested quit.
