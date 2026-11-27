import argparse
import json
from pathlib import Path
from lub_tools import to_c


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--game', choices=['tumiki', 'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper', 'rrootage', 'noiz2sa', 'wok', 'mazer-mayhem', 'gear-toy-gear', 'mu-cade', 'masashikun-hi'], default='tumiki')
parser.add_argument('--test', type=Path)
parser.add_argument('--ref', type=Path, action='append', default=[], help='a stub of native functions the C# calls')
parser.add_argument('--frame', action='store_true')
parser.add_argument('--output', type=Path, help='write game.c and binding.c here')
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
# Scene.cs is the boundary to the app that hosts the game; Standalone.cs is the app for a single game.
# AsciiText.cs reads the games' ASCII text by byte position.
for name in ['Scene.cs', 'Standalone.cs', 'AsciiText.cs']:
    if Path('game') / name not in sources:
        sources.append(Path('game') / name)
# The flat games read input, play sound and save through lub directly (the VR games have their own Host.cs).
if args.game not in ['gear-toy-gear', 'mazer-mayhem', 'torus-trooper']:
    for name in ['GameInput.cs', 'GameAudio.cs', 'GameSave.cs']:
        if Path('game') / name not in sources:
            sources.append(Path('game') / name)
    sources.append(args.lub / 'cs-lib/lubx/Assets.cs')
# Masashikun draws its menu and ranking (MasPanel.cs) with lubx text.
if args.game == 'masashikun-hi':
    sources += [args.lub / 'cs-lib/lubx' / name for name in ['Atlas.cs', 'Color.cs', 'Rect.cs', 'SpriteBatch.cs', 'Text.cs']]

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
# lub's own C# (cs-lib/lub: Bindings.Set, the Vec/Mat types, the Blend presets).
sources += sorted((args.lub / 'cs-lib/lub').glob('*.cs'))
if args.test:
    sources.append(args.test)
if args.sources_json:
    args.sources_json.parent.mkdir(parents=True, exist_ok=True)
    args.sources_json.write_text(json.dumps([str(path.resolve()) for path in sources]), encoding='utf-8')
    raise SystemExit(0)
if args.output is None:
    parser.error('--output or --sources-json is required')
to_c(args.lub, sources, args.output, [Path('games/mu-cade/OdeApi.cs')] * (args.game == 'mu-cade') + args.ref)
