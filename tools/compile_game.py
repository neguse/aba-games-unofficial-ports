import argparse
import json
from pathlib import Path
import subprocess


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--entry', default='Game')
parser.add_argument('--game', choices=['tumiki', 'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper', 'rrootage', 'noiz2sa', 'wok', 'mazer-mayhem', 'gear-toy-gear', 'mu-cade', 'masashikun-hi'], default='tumiki')
parser.add_argument('--test', type=Path)
parser.add_argument('--frame', action='store_true')
parser.add_argument('--output', type=Path, default=Path('build/game.lua'))
parser.add_argument('--c', type=Path, help='write game.c and binding.c here instead of Lua')
parser.add_argument('--tcs', type=Path)
parser.add_argument('--sources-json', type=Path, help='write the resolved C# source list without compiling')
args = parser.parse_args()
first = ['Core.cs', 'Rand.cs', 'PatternNumber.cs', 'Pattern.cs']
sources = [Path('game') / name for name in first]
if args.game == 'tumiki':
    sources += [p for p in sorted(Path('game').glob('*.cs')) if p.name not in first]
    sources += [Path('build/GameData.cs'), Path('build/BarrageCode.cs')]
elif args.game == 'parsec47':
    sources += [Path('game/Mesh.cs'), Path('game/Transform.cs')]
    sources.append(Path('games/parsec47/P47Rand.cs'))
    sources += [p for p in sorted(Path('games/parsec47').glob('*.cs')) if p.name != 'P47Rand.cs']
    sources += sorted(Path('build/parsec47').glob('*.cs'))
if args.game == 'gunroar':
    names = ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Input.cs', 'ShapeBase.cs']
    sources = [Path('game/Mesh.cs'), Path('game/Transform.cs')] + [Path('games/gunroar') / name for name in names]
    sources += [p for p in sorted(Path('games/gunroar').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/gunroar').glob('*.cs'))
if args.game == 'titanion':
    names = ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Input.cs', 'Token.cs']
    sources = [Path('games/titanion') / name for name in names]
    sources += [Path('game/Mesh.cs'), Path('game/Transform.cs')]
    sources += [f for f in sorted(Path('games/titanion').glob('*.cs')) if f.name not in names]
    sources += sorted(Path('build/titanion').glob('*.cs'))
if args.game == 'a7xpg':
    sources = [Path('game/Core.cs'), Path('game/Mesh.cs'), Path('game/Transform.cs'), Path('game/PatternNumber.cs')]
    sources += sorted(Path('games/a7xpg').glob('*.cs'))
    sources += sorted(Path('build/a7xpg').glob('*.cs'))
if args.game == 'torus-trooper':
    sources = [Path('games/torus-trooper') / name for name in ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Soundmanager.cs']]
    sources += [Path('game/Mesh.cs'), Path('game/Transform.cs'), Path('game/PatternNumber.cs'), Path('game/Pattern.cs')]
    sources += [p for p in sorted(Path('games/torus-trooper').glob('*.cs')) if p.name not in ['GameMath.cs', 'Rand.cs', 'Vector.cs', 'Actor.cs', 'Soundmanager.cs']]
    sources += sorted(Path('build/torus-trooper').glob('*.cs'))
    if not args.frame: sources.append(Path('games/frame/FrameMath.cs'))
if args.game == 'rrootage':
    sources = [Path('games/rrootage') / name for name in ['GameMath.cs', 'Rand.cs', 'Arrays.cs', 'Constants.cs', 'Models.cs']]
    sources += [Path('game/Mesh.cs'), Path('game/Transform.cs'), Path('game/PatternNumber.cs'), Path('game/Pattern.cs')]
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
    sources.append(Path('game/SimulationTime.cs'))
if args.game == 'gear-toy-gear':
    names = ['Arrays.cs', 'Math.cs', 'GameMath.cs', 'Random.cs', 'Actor.cs', 'PrimitiveShape.cs', 'Sound.cs']
    sources = [Path('game/PatternNumber.cs')] + [Path('games/gear-toy-gear') / name for name in names]
    sources += [p for p in sorted(Path('games/gear-toy-gear').glob('*.cs')) if p.name not in names]
    sources += sorted(Path('build/gear-toy-gear').glob('*.cs'))
if args.game == 'mu-cade':
    names = ['GameMath.cs', 'Rand.cs', 'Arrays.cs', 'Vector.cs', 'Actor.cs', 'OdeActor.cs', 'Shape.cs', 'Bullet.cs', 'Bulletimpl.cs', 'Spec.cs']
    sources = [Path('game/Mesh.cs'), Path('game/Transform.cs'), Path('game/PatternNumber.cs'), Path('game/Pattern.cs')] + [Path('games/mu-cade') / name for name in names]
    sources += sorted(Path('build/mu-cade').glob('*.cs'))
    sources += [p for p in sorted(Path('games/mu-cade').glob('*.cs')) if p.name not in names + ['OdeApi.cs']]
if args.game == 'masashikun-hi':
    sources = [Path('games/masashikun-hi/Models.cs')]
    sources += [p for p in sorted(Path('games/masashikun-hi').glob('*.cs')) if p.name != 'Models.cs']
    sources += sorted(Path('build/masashikun-hi').glob('*.cs'))
if args.game in ['gear-toy-gear', 'torus-trooper']:
    sources.append(Path('game/SimulationTime.cs'))

if args.frame:
    if args.game != 'torus-trooper':
        parser.error('--frame requires torus-trooper')
    sources.append(Path('games/frame/FrameMath.cs'))
elif args.game not in ['gear-toy-gear', 'mazer-mayhem']:
    shader_source = 'public static class GameShaders {\n'
    for name, stage in [('vertex', 'vs'), ('fragment', 'fs')]:
        prefix = 'shaders/mesh' if args.game in ['titanion', 'parsec47', 'tumiki', 'torus-trooper', 'a7xpg', 'rrootage', 'mu-cade'] else f'games/{args.game}/game'
        shader_source += f'public static string {name} = {json.dumps(Path(f"{prefix}.{stage}.slang").read_text())};\n'
    shader_source += '}\n'
    shader_path = Path('build/shaders') / args.game / 'Shaders.cs'
    shader_path.parent.mkdir(parents=True, exist_ok=True)
    shader_path.write_text(shader_source)
    sources.append(shader_path)
if args.game in ['torus-trooper', 'gear-toy-gear', 'mazer-mayhem']:
    sources.insert(0, args.lub / 'cs-lib/lubx/XrAnchor.cs')
if args.test:
    sources.append(args.test)
if args.sources_json:
    args.sources_json.parent.mkdir(parents=True, exist_ok=True)
    args.sources_json.write_text(json.dumps([str(path.resolve()) for path in sources]), encoding='utf-8')
    raise SystemExit(0)
if args.c:
    stubs = [Path('games/mu-cade/OdeApi.cs')] if args.game == 'mu-cade' else []
    args.c.mkdir(parents=True, exist_ok=True)
    subprocess.run(['dotnet', str(args.tcs / 'tcs2c/bin/Release/net10.0/tcs2c.dll'), '--lib',
                    '--ref', str(args.lub / 'cs-lib/lub_stub.cs'), *[arg for path in stubs for arg in ['--ref', str(path)]],
                    *map(str, sources), '-o', str(args.c / 'game.c')], check=True)
    subprocess.run(['dotnet', str(args.lub / 'tools/lub-gen/bin/Release/net10.0/lub-gen.dll'), 'tcs',
                    '--stub', str(args.lub / 'cs-lib/lub_stub.cs'),
                    # lub-gen takes a single stub, so it reads the game's own as source.
                    *[arg for path in sources + stubs for arg in ['--source', str(path)]],
                    '-o', str(args.c / 'binding.c')], check=True)
    raise SystemExit(0)
compiler = args.lub / 'third_party/tcs/Transpiler/bin/Release/net10.0/Transpiler.dll'
command = ['dotnet', str(compiler), *map(str, sources), '--ref', str(args.lub / 'cs-lib/lub_stub.cs'), '--no-naming-check']
if args.game == 'mu-cade':
    command += ['--ref', 'games/mu-cade/OdeApi.cs']
subprocess.run(command[:2] + ['check'] + command[2:], check=True)
args.output.parent.mkdir(parents=True, exist_ok=True)
subprocess.run(command + ['--entry', args.entry, '-o', str(args.output)], check=True)
