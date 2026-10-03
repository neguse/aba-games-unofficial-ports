import argparse
import json
from pathlib import Path
import shutil
import subprocess
import sys

parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--original', type=Path, required=True)
parser.add_argument('--native', type=Path, required=True)
parser.add_argument('--runtime', choices=['coreclr', 'tcs2c', 'both'], default='coreclr')
parser.add_argument('--tcs', type=Path)
parser.add_argument('--cc', default='cc')
args = parser.parse_args()
if args.runtime != 'coreclr' and args.tcs is None:
    parser.error('--tcs is required for tcs2c')
lub, original, native = args.lub.resolve(), args.original.resolve(), args.native.resolve()
root = Path(__file__).resolve().parent.parent
output = root / 'build/frame/publish'
for name in ['lub', 'liblub.so', 'libopenxr_loader.so.1', 'libSDL3.so.0', 'libslang-compiler.so.0.2026.8.1']:
    header = (native / name).read_bytes()[:20]
    if header[:4] != b'\x7fELF' or int.from_bytes(header[18:20], 'little') != 183:
        parser.error(f'{name} must be a Linux ARM64 library')
subprocess.run([sys.executable, 'tools/compile_torus.py', str(original)], cwd=root, check=True)
output.mkdir(parents=True, exist_ok=True)
if args.runtime in ['coreclr', 'both']:
    subprocess.run(['dotnet', 'publish', 'games/torus-trooper/frame/TorusTrooper.csproj',
                    '-c', 'Release', '-r', 'linux-arm64', '--self-contained', 'true',
                    f'-p:LubRoot={lub}', '-o', str(output)], cwd=root, check=True)
if args.runtime in ['tcs2c', 'both']:
    subprocess.run([sys.executable, 'tools/compile_frame_c.py', '--lub', str(lub),
                    '--tcs', str(args.tcs.resolve()), '--game', 'torus-trooper', '--native', str(native / 'liblub.so'),
                    '--cc', args.cc, '--executable', str(output / 'TorusTrooper-c')], cwd=root, check=True)
    shutil.copy2(args.tcs / 'LICENSE', output / 'LICENSE-tcs.txt')
for path in (root / 'games/torus-trooper/frame').glob('mesh.*.slang'):
    shutil.copy2(path, output / path.name)
launcher = (root / 'games/torus-trooper/frame/run.sh').read_text()
if args.runtime == 'tcs2c':
    launcher = launcher.replace('${LUB_RUNTIME:-coreclr}', '${LUB_RUNTIME:-tcs2c}')
(output / 'run.sh').write_text(launcher)
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(lub),
                '--game', 'torus-trooper', '--frame', '--output', str(output / 'game.lua')], cwd=root, check=True)
(output / 'native').mkdir(exist_ok=True)
shutil.copy2(native / 'lub', output / 'native/lub')
for source in ['samples/boot.lua', 'third_party/lume/lume.lua']:
    destination = output / source
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(lub / source, destination)
for name in ['liblub.so', 'libopenxr_loader.so.1', 'libSDL3.so.0', 'libslang-compiler.so.0.2026.8.1']:
    shutil.copy2(native / name, output / 'native' / name)
(output / 'audio').mkdir(exist_ok=True)
for path in (original / 'sounds').rglob('*'):
    if path.suffix in ['.wav', '.ogg']:
        subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', str(path),
                        '-c:a', 'pcm_s16le', str(output / 'audio' / (path.stem + '.wav'))], check=True)
shutil.copytree(root / 'build/torus-trooper/images', output / 'images', dirs_exist_ok=True)
shutil.copy2(original / 'readme_e.txt', output / 'README-original.txt')
shutil.copy2(root / 'LICENSE', output / 'LICENSE.txt')
shutil.copy2(lub / 'LICENSE', output / 'native/LICENSE-lub.txt')
subprocess.run([sys.executable, str(lub / 'tools/package-frame-licenses.py'),
                str(output / 'native')], check=True)
for component, source in [('SDL', 'third_party/SDL/LICENSE.txt'), ('OpenXR', 'third_party/openxr/LICENSE'),
                           ('Slang', 'third_party/slang/LICENSE')]:
    shutil.copy2(lub / source, output / 'native' / f'LICENSE-{component}.txt')
if args.runtime in ['coreclr', 'both']:
    assets = json.loads((root / 'games/torus-trooper/frame/obj/project.assets.json').read_text())
    runtime = next(d for d in assets['project']['frameworks']['net10.0']['downloadDependencies']
                   if d['name'] == 'Microsoft.NETCore.App.Runtime.linux-arm64')
    version = runtime['version'].strip('[]').split(',')[0].strip()
    package = next(Path(folder) / runtime['name'].lower() / version for folder in assets['packageFolders']
                   if (Path(folder) / runtime['name'].lower() / version).is_dir())
    for name in ['LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT']:
        shutil.copy2(package / name, output / ('DOTNET-' + name))
(output / 'run.sh').chmod(0o755)
print(output)
