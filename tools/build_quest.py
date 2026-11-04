"""Build the Meta Quest APK of a VR game (GearToyGear, Torus Trooper, Mazer Mayhem).

The game is the same C# as the web and Steam Frame builds, compiled to C by tcs2c and linked
with lub's Android player (lub's scripts/build-android.sh and docs/android.md).
The Android player has no Slang, so this compiles the game's shaders into lub's shader cache
with `lub export android`, run by a Linux build of the same lub checkout.
"""
import argparse
from pathlib import Path
import shutil
import subprocess
import sys
from compile_frame_c import compile_game
from frame_assets import gear_audio, license_text, mazer_audio, torus_audio

root = Path(__file__).resolve().parent.parent

# files: data path in the APK -> source in this repository.
# shaders: the (vertex file, fragment file, fragment prefix) of each Gfx.UseShader call of the game.
# Each pair is one shader cache entry, keyed by the exact text the game passes.
games = {
    'gear-toy-gear': dict(
        name='GearToyGear', package='dev.neguse.aba.geartoygear', data='tools/compile_gear.py',
        files={'game.vs.slang': 'games/gear-toy-gear/game.vs.slang',
               'game.fs.slang': 'games/gear-toy-gear/game.fs.slang',
               'scene.output.slang': 'games/frame/scene.output.slang'},
        shaders=[('game.vs.slang', 'game.fs.slang', ''),
                 ('scene.output.slang', 'scene.output.slang', '')],
        audio=gear_audio),
    'torus-trooper': dict(
        name='TorusTrooper', package='dev.neguse.aba.torustrooper', data='tools/compile_torus.py',
        files={'mesh.vs.slang': 'games/torus-trooper/frame/mesh.vs.slang',
               'mesh.fs.slang': 'games/torus-trooper/frame/mesh.fs.slang',
               'mesh.output.slang': 'games/torus-trooper/frame/mesh.output.slang'},
        shaders=[('mesh.vs.slang', 'mesh.fs.slang', ''),
                 ('mesh.output.slang', 'mesh.output.slang', '')],
        audio=torus_audio),
    'mazer-mayhem': dict(
        name='MazerMayhem', package='dev.neguse.aba.mazermayhem', data='tools/compile_mazer.py',
        files={'game.vs.slang': 'games/mazer-mayhem/game.vs.slang',
               'game.fs.slang': 'games/mazer-mayhem/game.fs.slang',
               'scene.output.slang': 'games/frame/scene.output.slang'},
        shaders=[('game.vs.slang', 'game.fs.slang', ''),
                 ('scene.output.slang', 'scene.output.slang', '')],
        audio=mazer_audio),
}


def export_source(shaders):
    """The C# entry that `lub export android` runs to compile `shaders` into the cache."""
    calls = ''.join(f'        Use({json_string(vertex)}, {json_string(fragment)}, {json_string(prefix)});\n'
                    for vertex, fragment, prefix in shaders)
    return ('using Lub;\n\n'
            '// lub export android で、ゲームが Gfx.UseShader に渡す (vertex, fragment) の組を shader cache に入れる。\n'
            'public static class ShaderExport\n{\n'
            '    public static void OnInit()\n    {\n    }\n\n'
            '    public static void OnFrame(float dt)\n    {\n    }\n\n'
            '    public static void OnExport()\n    {\n' + calls + '    }\n\n'
            '    static void Use(string vertexPath, string fragmentPath, string fragmentPrefix)\n    {\n'
            '        Io.LoadText(vertexPath, out var vertex, out _, out _, out _);\n'
            '        Io.LoadText(fragmentPath, out var fragment, out _, out _, out _);\n'
            '        if (vertex == null || fragment == null)\n        {\n            App.Quit(1);\n            return;\n        }\n'
            '        Gfx.UseShader(vertexPath + fragmentPrefix, vertex, fragmentPrefix + fragment, 1);\n'
            '    }\n}\n')


def json_string(text):
    return '"' + text.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n') + '"'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--game', choices=sorted(games), required=True)
    parser.add_argument('--original', type=Path, required=True)
    parser.add_argument('--lub-native', type=Path,
                        help='Linux lub of the same checkout, for lub export (default: <lub>/build-release-linux/lub)')
    parser.add_argument('--version-code', default='1')
    parser.add_argument('--version-name', default='0.1.0')
    parser.add_argument('--keystore', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    game = games[args.game]
    lub, original = args.lub.resolve(), args.original.resolve()
    tcs = lub / 'third_party/tcs'
    native = (args.lub_native or lub / 'build-release-linux/lub').resolve()
    work = root / 'build/quest' / args.game
    output = (args.output or root / 'build/quest' / f'{game["name"]}-quest.apk').resolve()

    subprocess.run([sys.executable, game['data'], str(original)], cwd=root, check=True)
    compile_game(lub, tcs, args.game, work / 'c')

    assets = work / 'assets'
    shutil.rmtree(assets, ignore_errors=True)
    (assets / 'audio').mkdir(parents=True)
    for name, source in game['files'].items():
        shutil.copy2(root / source, assets / name)
    game['audio'](original, assets / 'audio')
    (assets / 'LICENSE.txt').write_text(license_text(args.game, original, lub, tcs), encoding='utf-8')

    export = work / 'export'
    shutil.rmtree(export, ignore_errors=True)
    export.mkdir(parents=True)
    (export / 'ShaderExport.cs').write_text(export_source(game['shaders']), encoding='utf-8')
    subprocess.run(['dotnet', str(tcs / 'Transpiler/bin/Release/net10.0/Transpiler.dll'),
                    str(export / 'ShaderExport.cs'), '--ref', str(lub / 'cs-lib/lub_stub.cs'),
                    '--entry', 'ShaderExport', '-o', str(export / 'ShaderExport.lua')], check=True)
    # The entry reads the shaders by the same relative paths as the game, so it runs in the data directory.
    subprocess.run([str(native), 'export', 'android', str(export / 'ShaderExport.lua'), '-o', str(export / 'out')],
                   cwd=assets, check=True)
    cache = sorted((export / 'out/cache/shader').glob('*.lubshader'))
    if len(cache) != len(set(game['shaders'])):
        sys.exit(f'lub export wrote {len(cache)} shaders, expected {len(set(game["shaders"]))}')
    (assets / 'cache/shader').mkdir(parents=True)
    for path in cache:
        shutil.copy2(path, assets / 'cache/shader' / path.name)

    command = ['bash', str(lub / 'scripts/build-android.sh'), '--game', str(work / 'c'), '--assets', str(assets),
               '--package', game['package'], '--label', game['name'],
               '--version-code', args.version_code, '--version-name', args.version_name,
               '--build-dir', str(root / 'build/quest/android'), '-o', str(output)]
    if args.keystore:
        command += ['--keystore', str(args.keystore.resolve())]
    subprocess.run(command, check=True)
    print(output)


if __name__ == '__main__':
    main()
