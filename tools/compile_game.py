import argparse
import json
from pathlib import Path
import subprocess


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--entry', default='Game')
parser.add_argument('--game', choices=['tumiki', 'parsec47'], default='tumiki')
parser.add_argument('--test', type=Path)
parser.add_argument('--output', type=Path, default=Path('build/game.lua'))
args = parser.parse_args()
first = ['Core.cs', 'Rand.cs', 'Drawing.cs', 'PatternNumber.cs', 'Pattern.cs']
sources = [Path('game') / name for name in first]
if args.game == 'tumiki':
    sources += [p for p in sorted(Path('game').glob('*.cs')) if p.name not in first]
    sources += [Path('build/GameData.cs'), Path('build/BarrageCode.cs')]
else:
    sources.append(Path('games/parsec47/P47Rand.cs'))
    sources += [p for p in sorted(Path('games/parsec47').glob('*.cs')) if p.name != 'P47Rand.cs']
    sources += sorted(Path('build/parsec47').glob('*.cs'))
shader_source = 'public static class GameShaders {\n'
for name, stage in [('vertex', 'vs'), ('fragment', 'fs')]:
    shader_source += f'public static string {name} = {json.dumps(Path(f"shaders/game.{stage}.slang").read_text())};\n'
shader_source += '}\n'
Path('build/Shaders.cs').write_text(shader_source)
sources.append(Path('build/Shaders.cs'))
if args.test:
    sources.append(args.test)
compiler = args.lub / 'third_party/tcs/Transpiler/bin/Release/net10.0/Transpiler.dll'
command = ['dotnet', str(compiler), *map(str, sources), '--ref', str(args.lub / 'cs-lib/lub_stub.cs'), '--no-naming-check']
subprocess.run(command[:2] + ['check'] + command[2:], check=True)
args.output.parent.mkdir(parents=True, exist_ok=True)
subprocess.run(command + ['--entry', args.entry, '-o', str(args.output)], check=True)
