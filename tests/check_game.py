import argparse
import os
from pathlib import Path
import subprocess
import sys
from null_lub import null_api

# The class whose static Main runs the game's checks and ends with the line "RESULT <failures>".
entries = {'tumiki': 'GameVerification', 'parsec47': 'P47Verification', 'gunroar': 'GrVerification',
           'titanion': 'TtnVerification', 'a7xpg': 'A7xVerification', 'torus-trooper': 'TtVerification',
           'rrootage': 'RrVerification', 'noiz2sa': 'NrVerification', 'wok': 'WkVerification',
           'mazer-mayhem': 'MmVerification', 'gear-toy-gear': 'GtgVerification', 'mu-cade': 'McdVerification',
           'masashikun-hi': 'MasVerification'}
parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--game', choices=list(entries), default='tumiki')
parser.add_argument('--cc', default=os.environ.get('CC', 'cc'))
args = parser.parse_args()
lub = args.lub.resolve()
build = Path('build/tests') / args.game
test = Path('tests/GameVerification.cs') if args.game == 'tumiki' else Path(f'tests/{args.game}/GameVerification.cs')
subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(lub), '--game', args.game,
                '--test', str(test), '--ref', 'tests/NullLub.cs', '--output', str(build)], check=True)
# The game's C runs alone: lub is the API that draws, plays and loads nothing.
(build / 'null_lub.c').write_text(null_api(lub), encoding='utf-8')
host, native = '', []
if args.game == 'mu-cade':
    # Mu-cade's physics is ODE, which its C calls through games/mu-cade/ode_host.c.
    subprocess.run([sys.executable, 'tools/build_mucade_runtime.py', '--native'], check=True)
    host = f'#include "{Path("games/mu-cade/ode_host.c").resolve().as_posix()}"\n'
    native = ['-I', '.cache/ode/odedevs-ode-7bac210f051b/include', 'build/mu-cade/native/libode.a', '-lstdc++']
(build / 'test.c').write_text('#include "game.c"\n#include "binding.c"\n#include "null_lub.c"\n' + host +
                              f'int main(void) {{ tcs_lib_init(); tcs_entry_{entries[args.game]}_Main(); return 0; }}\n',
                              encoding='utf-8')
flags = subprocess.run(['bash', str(lub / 'src/tcs/cflags.sh')], check=True, capture_output=True, text=True).stdout.split()
subprocess.run([args.cc, '-std=gnu11', *flags, '-w', '-I', str(lub / 'include'), str(build / 'test.c'), *native,
                '-lm', '-o', str(build / 'test')], check=True)
result = subprocess.run([str(build.resolve() / 'test')], text=True, stdout=subprocess.PIPE)
print(result.stdout, end='')
sys.exit(0 if result.returncode == 0 and result.stdout.rstrip().endswith('RESULT 0') else 1)
