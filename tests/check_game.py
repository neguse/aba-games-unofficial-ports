import argparse
from pathlib import Path
import subprocess
import sys


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
args = parser.parse_args()
output = Path('build/tests/game.lua')
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub),
                '--entry', 'GameVerification', '--test', 'tests/GameVerification.cs',
                '--output', str(output)], check=True)
runner = output.with_name('run-game.lua')
runner.write_text('lub = {host = {available = function() return false end}}\nlocal game = dofile(arg[1]); game.main()\n')
result = subprocess.run([str(args.lub / 'third_party/tcs/deps/lua/lua32'), str(runner), str(output)],
                        text=True, stdout=subprocess.PIPE, check=True)
print(result.stdout, end='')
sys.exit(0 if result.stdout.rstrip().endswith('RESULT 0') else 1)
