import argparse
import hashlib
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import urllib.request
import zipfile


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--original', type=Path)
args = parser.parse_args()
original = args.original
if original is None:
    archive = Path('.cache/mas1_11e.zip')
    archive.parent.mkdir(exist_ok=True)
    if not archive.exists():
        urllib.request.urlretrieve('https://www.asahi-net.or.jp/~cs8k-cyu/free/mas1_11e.zip', archive)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != 'e2ab82d97560a81b94516009869de0ea46bb60aae18b1e1176ed0b5082f39258':
        raise ValueError('Masashikun Hi! archive checksum mismatch')
    with zipfile.ZipFile(archive) as source:
        source.extractall('.cache/original')
    original = Path('.cache/original/mas')
Path('build').mkdir(exist_ok=True)
target = Path('dist/masashikun-hi')
target.mkdir(parents=True, exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_masashikun.py', str(original)], check=True)
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub), '--game', 'masashikun-hi',
                '--output', str(target / 'game.lua')], check=True)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'games/masashikun-hi/game'], check=True)
shutil.copy2('games/masashikun-hi/index.html', target / 'index.html')
for name in ['main.js', 'style.css']:
    shutil.copy2(Path('web') / name, Path('dist') / name)
wasm = Path('dist/wasm')
wasm.mkdir(exist_ok=True)
for name in ['lub.js', 'lub.wasm', 'lub.data']:
    shutil.copy2(args.lub / 'build/wasm' / name, wasm / name)
licenses = Path('games/masashikun-hi/LICENSE.txt').read_text()
for path in [args.lub / 'LICENSE', args.lub / 'THIRD_PARTY_LICENSES.md', args.lub / 'third_party/tcs/LICENSE']:
    licenses += '\n\n' + path.read_text()
(target / 'LICENSE.txt').write_text(licenses)
# Package only this game's corresponding source, never the private repository or its history.
with tarfile.open(target / 'source.tar.gz', 'w:gz') as bundle:
    for directory in ['games/masashikun-hi', 'tests/masashikun-hi']:
        for path in sorted(Path(directory).iterdir()):
            if path.is_file():
                bundle.add(path, arcname='masashikun-hi-source/' + str(path))
    for name in ['build_masashikun.py', 'compile_masashikun.py', 'compile_game.py', 'compile_shaders.mjs', 'setup_lub.sh']:
        bundle.add(Path('tools') / name, arcname='masashikun-hi-source/tools/' + name)
    for name in ['main.js', 'style.css']:
        bundle.add(Path('web') / name, arcname='masashikun-hi-source/web/' + name)
    bundle.add('tests/check_game.py', arcname='masashikun-hi-source/tests/check_game.py')
    bundle.add('games/masashikun-hi/README.md', arcname='masashikun-hi-source/README.md')
    bundle.add('games/masashikun-hi/LICENSE.txt', arcname='masashikun-hi-source/LICENSE.txt')
    for path in sorted(original.rglob('*')):
        relative = path.relative_to(original)
        if path.is_file() and (path.suffix == '.ldt' or path.name in ['readme.txt', 'license.txt']):
            bundle.add(path, arcname='masashikun-hi-source/original/' + str(relative))
    for path in sorted((args.lub / 'third_party/tcs/runtime').glob('*.lua')):
        bundle.add(path, arcname='masashikun-hi-source/tcs-runtime/' + path.name)
    bundle.add(args.lub / 'third_party/tcs/LICENSE', arcname='masashikun-hi-source/tcs-runtime/LICENSE')
print('Built dist/masashikun-hi/')
