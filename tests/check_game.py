import argparse
from pathlib import Path
import subprocess
import sys


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--game', choices=['tumiki', 'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper'], default='tumiki')
args = parser.parse_args()
output = Path(f'build/tests/{args.game}.lua')
entry = {'tumiki': 'GameVerification', 'parsec47': 'P47Verification', 'gunroar': 'GrVerification', 'titanion': 'TtnVerification', 'a7xpg': 'A7xVerification', 'torus-trooper': 'TtVerification'}[args.game]
test = 'tests/GameVerification.cs' if args.game == 'tumiki' else f'tests/{args.game}/GameVerification.cs'
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub),
                '--game', args.game, '--entry', entry, '--test', test,
                '--output', str(output)], check=True)
runner = output.with_name('run-game.lua')
runner.write_text('lub = {host = {available = function() return false end}}\nlocal game = dofile(arg[1]); game.main()\n')
result = subprocess.run([str(args.lub / 'third_party/tcs/deps/lua/lua32'), str(runner), str(output)],
                        text=True, stdout=subprocess.PIPE, check=True)
print(result.stdout, end='')
sys.exit(0 if result.stdout.rstrip().endswith('RESULT 0') else 1)
