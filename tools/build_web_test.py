import argparse
from pathlib import Path
import shutil
import subprocess
import sys
from link_web import link

# Builds a game with its browser-test hooks into build/web-test/, next to a copy of dist/.
parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--tcs', type=Path, required=True)
parser.add_argument('--emsdk', type=Path, required=True)
parser.add_argument('--game', required=True)
args = parser.parse_args()
root = Path(__file__).resolve().parent.parent
tumiki = args.game == 'tumiki'
hooks = root / ('tests/BrowserHooks.cs' if tumiki else f'tests/{args.game}/BrowserHooks.cs')
source = root / 'build/web-c-test' / args.game
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                '--game', args.game, '--test', str(hooks), '--c', str(source)], cwd=root, check=True)
site = root / 'build/web-test'
site.mkdir(parents=True, exist_ok=True)
for name in ['main.js', 'compiled.js', 'style.css']:
    shutil.copy2(root / 'dist' / name, site / name)
target = site if tumiki else site / args.game
if tumiki:
    for path in (root / 'dist').iterdir():
        if path.is_file():
            shutil.copy2(path, site / path.name)
    shutil.copytree(root / 'dist/audio', site / 'audio', dirs_exist_ok=True)
else:
    shutil.copytree(root / 'dist' / args.game, target, dirs_exist_ok=True)
link(args.lub, args.emsdk, source, target / 'wasm', root / 'tests/web-host.c')
