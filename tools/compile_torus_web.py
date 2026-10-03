import argparse
import json
from pathlib import Path
import shutil
import subprocess
from compile_torus_c import compile_game

parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--tcs', type=Path, required=True)
parser.add_argument('--emsdk', type=Path, required=True)
parser.add_argument('--test', action='store_true')
parser.add_argument('--original', type=Path, required=True)
parser.add_argument('--output', type=Path, default=Path('dist/torus-trooper'))
args = parser.parse_args()
root = Path(__file__).resolve().parent.parent
if args.test and args.output == Path('dist/torus-trooper'):
    parser.error('--test requires a separate --output directory')
build = root / ('build/torus-web-test' if args.test else 'build/torus-web')
lub = args.lub.resolve()
compile_game(lub, args.tcs.resolve(), build)
host = lub / 'src/tcs_host.c'
if args.test:
    host = build / 'host.c'
    host.write_text(f'#define LUB_TCS_BASE_HOST "{lub.as_posix()}/src/tcs_host.c"\n'
                    f'#include "{root.as_posix()}/tests/torus-trooper/web-host.c"\n', encoding='utf-8')
subprocess.run(['cmake', '-S', str(lub), '-B', str(build / 'wasm'), '-G', 'Ninja',
                f'-DCMAKE_TOOLCHAIN_FILE={args.emsdk.resolve().as_posix()}/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake',
                '-DCMAKE_BUILD_TYPE=Release', '-DCMAKE_C_FLAGS_RELEASE=-O2 -DNDEBUG -fwrapv -ffp-contract=off',
                '-DCMAKE_CXX_FLAGS_RELEASE=-O2 -DNDEBUG',
                f'-DLUB_TCS_HOST={host.as_posix()}',
                f'-DLUB_TCS_GAME={build.as_posix()}/game.c',
                f'-DLUB_TCS_BINDING={build.as_posix()}/binding.c'], check=True)
subprocess.run(['cmake', '--build', str(build / 'wasm'), '--target', 'lub', '-j', '4'], check=True)
output = args.output.resolve()
(output / 'wasm').mkdir(parents=True, exist_ok=True)
(output / 'audio').mkdir(exist_ok=True)
for name in ['lub.js', 'lub.wasm', 'lub.data']:
    shutil.copy2(build / 'wasm' / name, output / 'wasm' / name)
subprocess.run(['node', str(root / 'tools/compile_shaders.mjs'), str(lub),
                str(output / 'shaders.json'), str(root / 'games/torus-trooper/frame/mesh'),
                str(root / 'games/torus-trooper/frame/mesh.output.slang')], check=True)
files = []
for path in (root / 'games/torus-trooper/frame').glob('mesh.*.slang'):
    shutil.copy2(path, output / path.name)
    files.append(path.name)
for path in (args.original / 'sounds').rglob('*'):
    if path.suffix in ['.wav', '.ogg']:
        name = 'audio/' + path.stem + '.wav'
        subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', str(path),
                        '-c:a', 'pcm_s16le', str(output / name)], check=True)
        files.append(name)
(output / 'images').mkdir(exist_ok=True)
for path in (root / 'build/torus-trooper/images').glob('*.png'):
    shutil.copy2(path, output / 'images' / path.name)
    files.append('images/' + path.name)
(output / 'assets.json').write_text(json.dumps(files), encoding='utf-8')
shutil.copy2(lub / 'web/xr.mjs', output / 'xr.js')
shutil.copy2(root / 'games/torus-trooper/index.html', output / 'index.html')
(output / 'LICENSE.txt').write_text((args.original / 'readme_e.txt').read_text()
    + '\n\n' + (root / 'LICENSE').read_text() + '\n\nlub\n---\n' + (lub / 'LICENSE').read_text()
    + '\n\n' + (lub / 'THIRD_PARTY_LICENSES.md').read_text()
    + '\n\ntcs2c runtime\n---\n' + (args.tcs / 'LICENSE').read_text(), encoding='utf-8')
for name in ['main.js', 'compiled.js', 'style.css']:
    shutil.copy2(root / 'web' / name, output.parent / name)
