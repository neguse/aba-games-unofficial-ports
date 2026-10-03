import argparse
import json
from pathlib import Path
import re
import shutil
import subprocess
from compile_frame_c import compile_game
from link_web import link

root = Path(__file__).resolve().parent.parent


def convert(source, target, *filters):
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', str(source), *filters,
                    '-c:a', 'pcm_s16le', str(target)], check=True)


def torus_audio(original, output):
    paths = [path for path in (original / 'sounds').rglob('*') if path.suffix in ['.wav', '.ogg']]
    for path in paths:
        convert(path, output / (path.stem + '.wav'))
    return [path.stem + '.wav' for path in paths]


def gear_audio(original, output):
    volumes = dict(re.findall(r'Sound\s*\{\s*Name = (\w+);\s*Volume = ([-\d]+)',
                              (original / 'Content/Audio/Gtg.xap').read_text()))
    paths = list((original / 'Content/Audio').glob('*.wav'))
    for path in paths:
        gain = .5 * 10 ** (int(volumes[path.stem]) / 2000)
        convert(path, output / path.name, '-af', f'volume={gain}')
    return [path.name for path in paths]


# shaders: the arguments of compile_shaders.mjs. test: the observation host and the C# it calls.
games = {
    'torus-trooper': dict(
        shaders=['games/torus-trooper/frame/mesh', 'games/torus-trooper/frame/mesh.output.slang'],
        audio=torus_audio,
        license=lambda original: (original / 'readme_e.txt').read_text(),
        test=('tests/torus-trooper/web-host.c', [])),
    'gear-toy-gear': dict(
        shaders=['games/gear-toy-gear/game', 'games/frame/scene.output.slang',
                 'GTG_ALPHA_TARGET:games/gear-toy-gear/game.fs.slang'],
        audio=gear_audio,
        license=lambda original: (root / 'games/gear-toy-gear/LICENSE.txt').read_text(),
        test=('tests/web-host.c', ['tests/gear-toy-gear/BrowserHooks.cs'])),
}

parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--tcs', type=Path, required=True)
parser.add_argument('--emsdk', type=Path, required=True)
parser.add_argument('--game', choices=sorted(games), required=True)
parser.add_argument('--test', action='store_true')
parser.add_argument('--original', type=Path, required=True)
parser.add_argument('--output', type=Path)
args = parser.parse_args()
if args.test and not args.output:
    parser.error('--test requires a separate --output directory')
game = games[args.game]
lub = args.lub.resolve()
source = root / ('build/web-c-test' if args.test else 'build/web-c') / args.game
host, hooks = game['test'] if args.test else (None, [])
compile_game(lub, args.tcs.resolve(), args.game, source, [root / path for path in hooks])
output = (args.output or Path('dist') / args.game).resolve()
link(lub, args.emsdk, source, output / 'wasm', root / host if host else None)
(output / 'audio').mkdir(exist_ok=True)
prefix, *others = game['shaders']
subprocess.run(['node', 'tools/compile_shaders.mjs', str(lub), str(output / 'shaders.json'), *game['shaders']],
               cwd=root, check=True)
files = []
for path in [f'{prefix}.vs.slang', f'{prefix}.fs.slang', *[path for path in others if ':' not in path]]:
    shutil.copy2(root / path, output / Path(path).name)
    files.append(Path(path).name)
files += ['audio/' + name for name in game['audio'](args.original.resolve(), output / 'audio')]
for path in sorted((root / 'build' / args.game / 'images').glob('*.png')):
    (output / 'images').mkdir(exist_ok=True)
    shutil.copy2(path, output / 'images' / path.name)
    files.append('images/' + path.name)
(output / 'assets.json').write_text(json.dumps(files), encoding='utf-8')
shutil.copy2(lub / 'web/xr.mjs', output / 'xr.js')
shutil.copy2(root / 'games' / args.game / 'index.html', output / 'index.html')
(output / 'LICENSE.txt').write_text(game['license'](args.original)
    + '\n\n' + (root / 'LICENSE').read_text() + '\n\nlub\n---\n' + (lub / 'LICENSE').read_text()
    + '\n\n' + (lub / 'THIRD_PARTY_LICENSES.md').read_text()
    + '\n\ntcs2c runtime\n---\n' + (args.tcs / 'LICENSE').read_text(), encoding='utf-8')
for name in ['main.js', 'compiled.js', 'style.css']:
    shutil.copy2(root / 'web' / name, output.parent / name)
