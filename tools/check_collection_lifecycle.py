#!/usr/bin/env python3
"""Run cloud-only all-13 GameSession lifecycle integration tests.

Uses actual game assemblies, production GameSession/HostBridge, and Lub's
SDL-dummy test fixture with mocked rendering. This is not a GPU/OpenXR or
rendered-output test. All saves are isolated in a temporary XDG data directory.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parent.parent
BUILD = ROOT / 'build/collection-lifecycle'
BUILD_FLAGS = ['--disable-build-servers', '-m:1', '-p:UseSharedCompilation=false']


def run(*args, **kwargs):
    print('+ ' + ' '.join(map(str, args)), flush=True)
    subprocess.run([str(x) for x in args], cwd=ROOT, check=True, **kwargs)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--package', type=Path, default=ROOT / 'build/collection/publish')
    parser.add_argument('--native', type=Path, help='liblub_session_test_host.so, defaults to Lub build-release-linux')
    parser.add_argument('--physics', type=Path, default=ROOT / 'build/native-physics/libmucade_physics.so')
    parser.add_argument('--dist', type=Path, help='stage full web assets beside managed-only game outputs without changing the package')
    parser.add_argument('--frames', type=int, default=12)
    parser.add_argument('--cycles', type=int, default=2)
    parser.add_argument('--game', help='diagnostic subset; omit for all-13 validation')
    parser.add_argument('--fixtures-only', action='store_true', help='run synthetic fault/reload/unload fixtures without actual games')
    parser.add_argument('--skip-build', action='store_true')
    parser.add_argument('--report', type=Path, default=BUILD / 'report.json')
    args = parser.parse_args()
    # Never leave an older passing report behind if preparation or a native
    # process fails before the managed harness can write its own report.
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps({'passed': False, 'stage': 'preparing',
        'renderer': 'mock-no-pixels', 'hardwareValidated': False,
        'failure': 'Preparation/build/run has not completed; inspect command output.'}, indent=2))
    lub = args.lub.resolve()
    native = (args.native or lub / 'build-release-linux/liblub_session_test_host.so').resolve()
    package = args.package.resolve()
    if args.frames < 2 or args.cycles < 1:
        parser.error('--frames must be at least 2 and --cycles at least 1')
    if not native.is_file():
        parser.error(f'{native} is missing; build Lub target lub_session_test_host first')
    if not args.fixtures_only and not args.physics.is_file():
        parser.error(f'{args.physics} is missing; run tools/build_mucade_native.py first')
    if args.game and args.fixtures_only:
        parser.error('--game and --fixtures-only are mutually exclusive')
    catalog = json.loads((ROOT / 'native/catalog.json').read_text())
    games = [g for g in catalog if not args.game or g['id'] == args.game]
    if not games:
        parser.error('Unknown game: ' + args.game)
    if not args.fixtures_only:
        for game in games:
            path = package / 'games' / game['id'] / 'AbaGame.dll'
            if not path.is_file():
                parser.error(f'{path} is missing; run tools/build_collection.py --managed-only first')
        if args.dist:
            # Reuse production packaging rules for PCM conversion and shaders;
            # keep the actual publish tree untouched while it may be rebuilding.
            from build_collection import copy_assets
            staged = BUILD / 'package'
            for game in games:
                target = staged / 'games' / game['id']
                target.mkdir(parents=True, exist_ok=True)
                for source in (package / 'games' / game['id']).iterdir():
                    if source.is_file() and source.suffix in ('.dll', '.json', '.pdb'):
                        shutil.copy2(source, target / source.name)
                copy_assets(game, args.dist.resolve(), target)
            package = staged.resolve()
    fixtures = BUILD / 'fixtures'
    harness = BUILD / 'harness'
    if not args.skip_build:
        for game_id, mode in [('valid', 'VALID'), ('missing-game', 'MISSING_GAME'), ('missing-frame', 'MISSING_FRAME')]:
            run('dotnet', 'build', ROOT / 'tests/collection-lifecycle/fixture/Fixture.csproj',
                '-c', 'Release', '-o', fixtures / 'games' / game_id,
                f'-p:LubRoot={lub}', f'-p:FixtureMode={mode}', *BUILD_FLAGS)
        run('dotnet', 'build', ROOT / 'tests/collection-lifecycle/CollectionLifecycle.csproj',
            '-c', 'Release', '-o', harness, f'-p:LubRoot={lub}', *BUILD_FLAGS)
    env = os.environ.copy()
    directories = [str(native.parent), str(args.physics.resolve().parent)]
    if env.get('LD_LIBRARY_PATH'):
        directories.append(env['LD_LIBRARY_PATH'])
    env['LD_LIBRARY_PATH'] = ':'.join(directories)
    env['LUB_NATIVE_LIB'] = str(native)
    env['SDL_AUDIODRIVER'] = 'dummy'
    command = ['dotnet', harness / 'CollectionLifecycle.dll', '--package', package,
               '--fixtures', fixtures, '--native', native, '--frames', args.frames,
               '--cycles', args.cycles, '--report', args.report.resolve()]
    if args.game:
        command += ['--game', args.game]
    if args.fixtures_only:
        command += ['--fixtures-only']
    run(*command, env=env)
    report = json.loads(args.report.read_text())
    if not report['passed']:
        raise RuntimeError('Lifecycle report did not pass')
    expected = 0 if args.fixtures_only else len(games)
    if len(report['games']) != expected:
        raise RuntimeError(f'Expected {expected} actual games in report, got {report["games"]}')
    print(f'Lifecycle report: {args.report.resolve()}')
    print('Scope: mocked rendering only; no GPU pixels, OpenXR runtime or physical device validation.')


if __name__ == '__main__':
    main()
