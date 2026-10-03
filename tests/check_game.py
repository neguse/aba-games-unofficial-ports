import argparse
from pathlib import Path
import subprocess
import sys


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--game', choices=['tumiki', 'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper', 'rrootage', 'noiz2sa', 'wok', 'mazer-mayhem', 'gear-toy-gear', 'mu-cade', 'masashikun-hi'], default='tumiki')
args = parser.parse_args()
output = Path(f'build/tests/{args.game}.lua')
if args.game in ['mazer-mayhem', 'gear-toy-gear', 'mu-cade', 'masashikun-hi']:
    subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub),
                    '--game', args.game, '--output', str(output)], check=True)
    runner = Path('build/mu-cade/lua-ode') if args.game == 'mu-cade' else args.lub / 'third_party/tcs/deps/lua/lua32'
    subprocess.run([str(runner),
                    f'tests/{args.game}/game.lua', str(output)], check=True)
    sys.exit(0)
entry = {'tumiki': 'GameVerification', 'parsec47': 'P47Verification', 'gunroar': 'GrVerification', 'titanion': 'TtnVerification', 'a7xpg': 'A7xVerification', 'torus-trooper': 'TtVerification', 'rrootage': 'RrVerification', 'noiz2sa': 'NrVerification', 'wok': 'WkVerification'}[args.game]
test = 'tests/GameVerification.cs' if args.game == 'tumiki' else f'tests/{args.game}/GameVerification.cs'
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub),
                '--game', args.game, '--entry', entry, '--test', test,
                '--output', str(output)], check=True)
runner = output.with_name('run-game.lua')
runner.write_text('lub = {host = {available = function() return false end}}\n' + (
    'lub.png = {load = function() return nil, 0, 0, 0, 0, 0, "pending" end}\n'
    'lub.gfx = {use_buffer = function(key, kind, data) assert(#data > 0, key); return {version=1} end, '
    'use_texture = function() return {version=1} end, draw = function() end}\n'
    if args.game in ['gunroar', 'titanion', 'parsec47', 'tumiki', 'torus-trooper', 'a7xpg', 'rrootage'] else '') + 'local game = dofile(arg[1]); game.main()\n')
result = subprocess.run([str(args.lub / 'third_party/tcs/deps/lua/lua32'), str(runner), str(output)],
                        text=True, stdout=subprocess.PIPE, check=True)
print(result.stdout, end='')
sys.exit(0 if result.stdout.rstrip().endswith('RESULT 0') else 1)
