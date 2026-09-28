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
args = parser.parse_args()
lub, original, native = args.lub.resolve(), args.original.resolve(), args.native.resolve()
root = Path(__file__).resolve().parent.parent
output = root / 'build/mm-frame/publish'
for name in ['lub', 'liblub.so', 'libopenxr_loader.so.1', 'libSDL3.so.0', 'libslang-compiler.so.0.2026.8.1']:
    header = (native / name).read_bytes()[:20]
    if header[:4] != b'\x7fELF' or int.from_bytes(header[18:20], 'little') != 183:
        parser.error(f'{name} must be a Linux ARM64 library')
subprocess.run([sys.executable, 'tools/compile_mazer.py', str(original)], cwd=root, check=True)
subprocess.run(['dotnet', 'publish', 'games/mazer-mayhem/frame/MazerMayhem.csproj',
                '-c', 'Release', '-m:1', '-p:NuGetAudit=false', '-r', 'linux-arm64', '--self-contained', 'true',
                f'-p:LubRoot={lub}', '-o', str(output)], cwd=root, check=True)
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(lub),
                '--game', 'mazer-mayhem', '--frame', '--output', str(output / 'game.lua')], cwd=root, check=True)
(output / 'native').mkdir(exist_ok=True)
shutil.copy2(native / 'lub', output / 'native/lub')
for source in ['samples/boot.lua', 'third_party/lume/lume.lua']:
    destination = output / source
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(lub / source, destination)
for name in ['liblub.so', 'libopenxr_loader.so.1', 'libSDL3.so.0', 'libslang-compiler.so.0.2026.8.1']:
    shutil.copy2(native / name, output / 'native' / name)
(output / 'audio').mkdir(exist_ok=True)
for path in (original / 'Content/Audio').glob('*.wav'):
    gain = '-7dB' if path.stem in ['Mm1', 'Mm2', 'Mm3'] else '-12dB'
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', str(path), '-af', f'volume={gain}',
                    '-c:a', 'pcm_s16le', str(output / 'audio' / path.name)], check=True)
shutil.copy2(lub / 'LICENSE', output / 'native/LICENSE-lub.txt')
subprocess.run([sys.executable, str(lub / 'tools/package-frame-licenses.py'),
                str(output / 'native')], check=True)
for component, source in [('SDL', 'third_party/SDL/LICENSE.txt'), ('OpenXR', 'third_party/openxr/LICENSE'),
                           ('Slang', 'third_party/slang/LICENSE')]:
    shutil.copy2(lub / source, output / 'native' / f'LICENSE-{component}.txt')
assets = json.loads((root / 'games/mazer-mayhem/frame/obj/project.assets.json').read_text())
runtime = next(d for d in assets['project']['frameworks']['net10.0']['downloadDependencies']
               if d['name'] == 'Microsoft.NETCore.App.Runtime.linux-arm64')
version = runtime['version'].strip('[]').split(',')[0].strip()
package = next(Path(folder) / runtime['name'].lower() / version for folder in assets['packageFolders']
               if (Path(folder) / runtime['name'].lower() / version).is_dir())
for name in ['LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT']:
    shutil.copy2(package / name, output / ('DOTNET-' + name))
(output / 'run.sh').chmod(0o755)
print(output)
