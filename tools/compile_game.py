import argparse
import json
from pathlib import Path
import subprocess


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--entry', default='Game')
parser.add_argument('--game', choices=['tumiki', 'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper', 'rrootage', 'noiz2sa', 'wok', 'mazer-mayhem'], default='tumiki')
parser.add_argument('--test', type=Path)
parser.add_argument('--output', type=Path, default=Path('build/game.lua'))
args = parser.parse_args()
first = ['Core.cs', 'Rand.cs', 'Drawing.cs', 'PatternNumber.cs', 'Pattern.cs']
sources = [Path('game') / name for name in first]
if args.game == 'tumiki':
    sources += [p for p in sorted(Path('game').glob('*.cs')) if p.name not in first]
    sources += [Path('build/GameData.cs'), Path('build/BarrageCode.cs')]
elif args.game == 'parsec47':
    sources.append(Path('games/parsec47/P47Rand.cs'))
    sources += [p for p in sorted(Path('games/parsec47').glob('*.cs')) if p.name != 'P47Rand.cs']
    sources += sorted(Path('build/parsec47').glob('*.cs'))
if args.game == 'gunroar':
    names = ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Input.cs', 'DisplayList.cs', 'ShapeBase.cs']
    sources = [Path('games/gunroar') / name for name in names] + [Path('game/Drawing.cs')]
    sources += [p for p in sorted(Path('games/gunroar').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/gunroar').glob('*.cs'))
if args.game == 'titanion':
    names = ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Input.cs', 'DisplayList.cs', 'Token.cs']
    sources = [Path('games/titanion') / name for name in names]
    sources.append(Path('game/Drawing.cs'))
    sources += [f for f in sorted(Path('games/titanion').glob('*.cs')) if f.name not in names]
    sources += sorted(Path('build/titanion').glob('*.cs'))
if args.game == 'a7xpg':
    sources = [Path('game/Core.cs'), Path('game/Drawing.cs'), Path('game/PatternNumber.cs')]
    sources += sorted(Path('games/a7xpg').glob('*.cs'))
    sources += sorted(Path('build/a7xpg').glob('*.cs'))
if args.game == 'torus-trooper':
    sources = [Path('games/torus-trooper') / name for name in ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs']]
    sources += [Path('game/Drawing.cs'), Path('game/PatternNumber.cs'), Path('game/Pattern.cs')]
    sources += [p for p in sorted(Path('games/torus-trooper').glob('*.cs')) if p.name not in ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs']]
    sources += sorted(Path('build/torus-trooper').glob('*.cs'))
if args.game == 'rrootage':
    sources = [Path('games/rrootage') / name for name in ['GameMath.cs', 'Rand.cs', 'Arrays.cs', 'Constants.cs', 'Models.cs']]
    sources += [Path('game/Drawing.cs'), Path('game/PatternNumber.cs'), Path('game/Pattern.cs')]
    sources += [p for p in sorted(Path('games/rrootage').glob('*.cs')) if p.name not in ['GameMath.cs', 'Rand.cs', 'Arrays.cs', 'Constants.cs', 'Models.cs']]
    sources += sorted(Path('build/rrootage').glob('*.cs'))
if args.game == 'noiz2sa':
    names = ['GameMath.cs', 'Arrays.cs', 'Constants.cs', 'Models.cs', 'PixelLayer.cs']
    sources = [Path('games/noiz2sa') / name for name in names]
    sources += [Path('game/PatternNumber.cs'), Path('game/Pattern.cs')]
    sources += [p for p in sorted(Path('games/noiz2sa').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/noiz2sa').glob('*.cs'))
if args.game == 'wok':
    names = ['GameMath.cs', 'Arrays.cs', 'Constants.cs', 'Models.cs']
    sources = [Path('games/wok') / name for name in names]
    sources += [p for p in sorted(Path('games/wok').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/wok').glob('*.cs'))
if args.game == 'mazer-mayhem':
    names = ['Arrays.cs', 'Math.cs', 'GameMath.cs', 'Random.cs', 'Actor.cs', 'PhysicsActor.cs', 'PrimitiveShape.cs', 'Shape.cs']
    sources = [Path('game/PatternNumber.cs')] + [Path('games/mazer-mayhem') / name for name in names]
    sources += [p for p in sorted(Path('games/mazer-mayhem').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/mazer-mayhem').glob('*.cs'))
shader_source = 'public static class GameShaders {\n'
for name, stage in [('vertex', 'vs'), ('fragment', 'fs')]:
    shader_source += f'public static string {name} = {json.dumps(Path(f"games/{args.game}/game.{stage}.slang" if args.game in ["a7xpg", "rrootage", "noiz2sa", "wok", "mazer-mayhem"] else f"shaders/game.{stage}.slang").read_text())};\n'
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
