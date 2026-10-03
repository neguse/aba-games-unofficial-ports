import argparse
from pathlib import Path
import shutil
import subprocess

# link() arguments that add ODE and its tcs2c host to Mu-cade (see build_mucade_runtime.py).
mucade = dict(build=Path('build/web-c-mu-cade'), bridge=Path('games/mu-cade/ode_host.c'),
              project=Path('build/mu-cade/ode.cmake'))


def link(lub, emsdk, source, output, host=None, build=None, bridge=None, project=None):
    """Link the game.c and binding.c in `source` with lub into `output`/lub.{js,wasm,data}.

    A game with native code of its own passes a private `build` tree, a `bridge` C file
    included after the host, and a `project` CMake file included by lub's project().
    """
    root = Path(__file__).resolve().parent.parent
    lub, source = lub.resolve(), source.resolve()
    # One build tree serves every game without a `build` of its own: only the host object depends on the game.
    build = build.resolve() if build else root / 'build/web-c'
    current = build / 'current'
    current.mkdir(parents=True, exist_ok=True)
    for name in ['game.c', 'binding.c']:
        shutil.copyfile(source / name, current / name)
    base = lub / 'src/tcs_host.c'
    entry = current / 'host.c'
    text = (f'#define LUB_TCS_BASE_HOST "{base.as_posix()}"\n#include "{host.resolve().as_posix()}"\n'
            if host else f'#include "{base.as_posix()}"\n')
    if bridge:
        text += f'#include "{bridge.resolve().as_posix()}"\n'
    entry.write_text(text, encoding='utf-8')
    subprocess.run(['cmake', '-S', str(lub), '-B', str(build / 'wasm'), '-G', 'Ninja',
                    f'-DCMAKE_TOOLCHAIN_FILE={emsdk.resolve().as_posix()}/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake',
                    '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_C_FLAGS_RELEASE=-O2 -DNDEBUG -fwrapv -ffp-contract=off',
                    '-DCMAKE_CXX_FLAGS_RELEASE=-O2 -DNDEBUG',
                    f'-DLUB_TCS_HOST={entry.as_posix()}',
                    f'-DLUB_TCS_GAME={current.as_posix()}/game.c',
                    f'-DLUB_TCS_BINDING={current.as_posix()}/binding.c',
                    *([f'-DCMAKE_PROJECT_lub_INCLUDE={project.resolve().as_posix()}'] if project else [])], check=True)
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
