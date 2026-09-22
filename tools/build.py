import argparse
import hashlib
from pathlib import Path
import shutil
import subprocess
import sys
import urllib.request
import zipfile


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--original', type=Path)
args = parser.parse_args()
original = args.original
if original is None:
    cache = Path('.cache')
    cache.mkdir(exist_ok=True)
    archive = cache / 'tf0_21.zip'
    if not archive.exists():
        urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/tf0_21.zip', archive)
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    if digest != '774cbeab652c128e57cab6d4c03e262c28ab128241984ae755daa31c989b5e36':
        raise ValueError('Original archive checksum mismatch')
    with zipfile.ZipFile(archive) as source:
        source.extractall(cache / 'original')
    original = cache / 'original/tf'
dist = Path('dist')
dist.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_barrage.py', str(original / 'barrage'), 'build/BarrageCode.cs'], check=True)
subprocess.run([sys.executable, 'tools/compile_data.py', str(original), 'build/GameData.cs'], check=True)
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub), '--output', 'dist/game.lua'], check=True)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), 'dist/shaders.json'], check=True)
for path in Path('web').iterdir():
    shutil.copy2(path, dist / path.name)
shutil.copytree(original / 'sounds', dist / 'audio', dirs_exist_ok=True)
(dist / 'wasm').mkdir(exist_ok=True)
for name in ['lub.js', 'lub.wasm', 'lub.data']:
    shutil.copy2(args.lub / 'build/wasm' / name, dist / 'wasm' / name)
licenses = Path('LICENSE').read_text() + '\n\nlub\n---\n' + (args.lub / 'LICENSE').read_text()
licenses += '\n\n' + (args.lub / 'THIRD_PARTY_LICENSES.md').read_text()
(dist / 'LICENSE.txt').write_text(licenses)
print('Built dist/')
