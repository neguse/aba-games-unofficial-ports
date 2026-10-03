import argparse
from pathlib import Path
import shutil
import subprocess


def link(lub, emsdk, source, output, host=None):
    """Link the game.c and binding.c in `source` with lub into `output`/lub.{js,wasm,data}."""
    root = Path(__file__).resolve().parent.parent
    lub, source = lub.resolve(), source.resolve()
    # One build tree serves every game: only the host object depends on the game.
    build = root / 'build/web-c'
    current = build / 'current'
    current.mkdir(parents=True, exist_ok=True)
    for name in ['game.c', 'binding.c']:
        shutil.copyfile(source / name, current / name)
    base = lub / 'src/tcs_host.c'
    entry = current / 'host.c'
    entry.write_text(f'#define LUB_TCS_BASE_HOST "{base.as_posix()}"\n#include "{host.resolve().as_posix()}"\n'
                     if host else f'#include "{base.as_posix()}"\n', encoding='utf-8')
    subprocess.run(['cmake', '-S', str(lub), '-B', str(build / 'wasm'), '-G', 'Ninja',
                    f'-DCMAKE_TOOLCHAIN_FILE={emsdk.resolve().as_posix()}/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake',
                    '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_C_FLAGS_RELEASE=-O2 -DNDEBUG -fwrapv -ffp-contract=off',
                    '-DCMAKE_CXX_FLAGS_RELEASE=-O2 -DNDEBUG',
                    f'-DLUB_TCS_HOST={entry.as_posix()}',
                    f'-DLUB_TCS_GAME={current.as_posix()}/game.c',
                    f'-DLUB_TCS_BINDING={current.as_posix()}/binding.c'], check=True)
    subprocess.run(['cmake', '--build', str(build / 'wasm'), '--target', 'lub', '-j', '4'], check=True)
    output.mkdir(parents=True, exist_ok=True)
    for name in ['lub.js', 'lub.wasm', 'lub.data']:
        shutil.copy2(build / 'wasm' / name, output / name)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--emsdk', type=Path, required=True)
    parser.add_argument('--c', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--host', type=Path)
    args = parser.parse_args()
    link(args.lub, args.emsdk, args.c, args.output, args.host)
