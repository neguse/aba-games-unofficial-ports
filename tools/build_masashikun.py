import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tarfile
import urllib.request
import zipfile
from link_web import link


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--emsdk', type=Path, required=True)
parser.add_argument('--original', type=Path)
parser.add_argument('--source-only', action='store_true', help='refresh corresponding source beside an existing web build')
args = parser.parse_args()


def embedded_runtime(lub):
    """The runtime files tcs ships and embeds into the generated C (src/tcs/shipped.txt): (shipped path, source)."""
    for line in (lub / 'src/tcs/shipped.txt').read_text(encoding='utf-8').splitlines():
        if line.strip() and not line.startswith('#'):
            destination, source = line.split()
            yield destination, lub / source


def lubx_sprite_shaders(lub, directory):
    """Write the shaders lubx SpriteBatch passes to Gfx.UseShader as files.

    The page looks up precompiled shaders by their source text, so these must match the C# strings exactly.
    """
    text = (lub / 'cs-lib/lubx/SpriteBatch.cs').read_text(encoding='utf-8')
    directory.mkdir(parents=True, exist_ok=True)
    paths = []
    for name, stage in [('instancedVs', 'vs'), ('fs', 'fs')]:
        expression = re.search(r'private static string ' + name + r' = (.*?);\n', text, re.S).group(1)
        source = ''.join(json.loads('"' + piece + '"') for piece in re.findall(r'"((?:[^"\\]|\\.)*)"', expression))
        path = directory / f'sprite.{stage}.slang'
        path.write_text(source, encoding='utf-8')
        paths.append(path)
    return paths


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
if not args.source_only:
    subprocess.run([sys.executable, 'tools/compile_masashikun.py', str(original)], check=True)
    source = Path('build/web-c/masashikun-hi')
    subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub),
                    '--game', 'masashikun-hi', '--output', str(source)], check=True)
    link(args.lub, args.emsdk, source, target / 'wasm')
    # MasPanel draws its text with this font (the SIL OFL license travels with it).
    (target / 'fonts').mkdir(exist_ok=True)
    fonts = args.lub / 'samples/data/fonts'
    shutil.copy2(fonts / 'MPLUS1p-subset.ttf', target / 'fonts/panel.ttf')
    shutil.copy2(fonts / 'MPLUS1p-OFL.txt', target / 'fonts/OFL.txt')
    (target / 'assets.json').write_text(json.dumps(['fonts/panel.ttf']), encoding='utf-8')
    sprite = lubx_sprite_shaders(args.lub, Path('build/masashikun-hi'))
    subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'games/masashikun-hi/game', *map(str, sprite)], check=True)
    shutil.copy2('games/masashikun-hi/index.html', target / 'index.html')
    for name in ['main.js', 'compiled.js', 'style.css']:
        shutil.copy2(Path('web') / name, Path('dist') / name)
licenses = Path('games/masashikun-hi/LICENSE.txt').read_text()
for path in [args.lub / 'LICENSE', args.lub / 'THIRD_PARTY_LICENSES.md']:
    licenses += '\n\n' + path.read_text()
(target / 'LICENSE.txt').write_text(licenses)
# Package only this game's corresponding source, never the private repository or its history.
with tarfile.open(target / 'source.tar.gz', 'w:gz') as bundle:
    for directory in ['games/masashikun-hi', 'tests/masashikun-hi']:
        for path in sorted(Path(directory).iterdir()):
            if path.is_file():
                bundle.add(path, arcname='masashikun-hi-source/' + str(path))
    for name in ['Scene.cs', 'Standalone.cs', 'GameInput.cs', 'GameAudio.cs', 'GameSave.cs']:
        bundle.add(Path('game') / name, arcname='masashikun-hi-source/game/' + name)
    for name in ['build_masashikun.py', 'compile_masashikun.py', 'compile_game.py', 'lub_tools.py', 'compile_shaders.mjs', 'link_web.py', 'setup_lub.sh']:
        bundle.add(Path('tools') / name, arcname='masashikun-hi-source/tools/' + name)
    for name in ['main.js', 'compiled.js', 'style.css']:
        bundle.add(Path('web') / name, arcname='masashikun-hi-source/web/' + name)
    for name in ['check_game.py', 'null_lub.py', 'NullLub.cs']:
        bundle.add('tests/' + name, arcname='masashikun-hi-source/tests/' + name)
    bundle.add('games/masashikun-hi/README.md', arcname='masashikun-hi-source/README.md')
    bundle.add('games/masashikun-hi/LICENSE.txt', arcname='masashikun-hi-source/LICENSE.txt')
    for path in sorted(original.rglob('*')):
        relative = path.relative_to(original)
        if path.is_file() and (path.suffix == '.ldt' or path.name in ['readme.txt', 'license.txt']):
            bundle.add(path, arcname='masashikun-hi-source/original/' + str(relative))
    # tcs writes its C runtime into the game; these files are that runtime's source (part of lub).
    for destination, path in embedded_runtime(args.lub.resolve()):
        bundle.add(path, arcname='masashikun-hi-source/tcs-runtime/' + destination)
    for name in ['LICENSE', 'THIRD_PARTY_LICENSES.md']:
        bundle.add(args.lub / name, arcname='masashikun-hi-source/tcs-runtime/' + name)
print('Built dist/masashikun-hi/')
