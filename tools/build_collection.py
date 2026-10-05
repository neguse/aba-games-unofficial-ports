"""Build all thirteen unchanged game sources as isolated native plug-ins.

First prepare original data and web assets with tools/build.py. No install,
Steam library mutation, publication or deployment is performed by this tool.
"""
import argparse
import json
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parent.parent


def run(*args, **kw):
    subprocess.run([str(x) for x in args], cwd=ROOT, check=True, **kw)


def project(game, lub, directory):
    directory.mkdir(parents=True, exist_ok=True)
    manifest = directory / 'sources.json'
    args = [sys.executable, 'tools/compile_game.py', '--lub', lub,
            '--game', game['id'], '--sources-json', manifest]
    if game['id'] == 'torus-trooper':
        args.append('--frame')
    run(*args)
    sources = [Path(x) for x in json.loads(manifest.read_text())]
    # compile_game's web shader file is scratch space shared by all games.
    # Each managed project needs its own immutable copy during the whole build.
    for i, source in enumerate(sources):
        if source.name == 'Shaders.cs':
            copy = directory / 'GameShaders.cs'
            shutil.copy2(source, copy)
            sources[i] = copy
    if game['id'] == 'mu-cade':
        sources.append(ROOT / 'native/MucadePhysics.cs')
    root = ET.Element('Project', Sdk='Microsoft.NET.Sdk')
    props = ET.SubElement(root, 'PropertyGroup')
    for k, v in {'TargetFramework':'net10.0', 'AssemblyName':'AbaGame',
                 'EnableDefaultCompileItems':'false', 'ImplicitUsings':'enable',
                 'InvariantGlobalization':'true', 'GenerateDependencyFile':'true'}.items():
        ET.SubElement(props, k).text = v
    refs = ET.SubElement(root, 'ItemGroup')
    for path in [lub / 'dotnet/Lub/Lub.csproj', lub / 'third_party/tcs/TinySystem/TinySystem.csproj']:
        ET.SubElement(refs, 'ProjectReference', Include=str(path))
    for path in sources:
        if not path.exists():
            raise FileNotFoundError(f'{path}: prepare original game data first')
        ET.SubElement(refs, 'Compile', Include=str(path))
    path = directory / 'Game.csproj'
    ET.indent(root)
    ET.ElementTree(root).write(path, encoding='unicode')
    return path


def copy_assets(game, dist, output):
    source = dist if game['id'] == 'tumiki' else dist / game['id']
    for name in ['audio', 'images']:
        directory = source / name
        if directory.is_dir():
            shutil.copytree(directory, output / name, dirs_exist_ok=True)
    license_file = source / 'LICENSE.txt'
    if not license_file.exists():
        raise FileNotFoundError(f'{license_file}: full web build/assets required')
    shutil.copy2(license_file, output / 'LICENSE.txt')
    for name in ['source.tar.gz', 'SOURCE-README.txt']:
        artifact = source / name
        if artifact.exists():
            shutil.copy2(artifact, output / name)
        elif game['id'] == 'masashikun-hi':
            raise FileNotFoundError(f'{artifact}: native corresponding-source package required')
    # Native audio decoder receives PCM WAV, preserving the original gains.
    for source_audio in (output / 'audio').glob('*'):
        if source_audio.suffix.lower() in ('.ogg', '.mp3'):
            wav = source_audio.with_suffix('.wav')
            if not wav.exists():
                run('ffmpeg', '-v', 'error', '-y', '-i', source_audio, '-c:a', 'pcm_s16le', wav)
    if game['id'] in ('gear-toy-gear', 'mazer-mayhem'):
        for name in ['game.vs.slang', 'game.fs.slang']:
            shutil.copy2(ROOT / 'games' / game['id'] / name, output / name)
        shutil.copy2(ROOT / 'games/frame/scene.output.slang', output / 'scene.output.slang')
    elif game['id'] == 'torus-trooper':
        for source_shader in (ROOT / 'games/torus-trooper/frame').glob('mesh.*.slang'):
            shutil.copy2(source_shader, output / source_shader.name)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--dist', type=Path, default=ROOT / 'dist')
    parser.add_argument('--output', type=Path, default=ROOT / 'build/collection/publish')
    parser.add_argument('--native', type=Path, help='directory containing liblub.so and its native shared dependencies')
    parser.add_argument('--physics', type=Path, help='prebuilt matching-architecture libmucade_physics.so (with adjacent ODE licenses)')
    parser.add_argument('--rid', choices=['linux-x64','linux-arm64'], default='linux-arm64' if platform.machine() in ('aarch64','arm64') else 'linux-x64')
    parser.add_argument('--managed-only', action='store_true', help='compile managed assemblies without claiming a playable package')
    parser.add_argument('--no-build', action='store_true', help='stage assets/native files beside already compiled assemblies')
    args=parser.parse_args()
    lub=args.lub.resolve(); out=args.output.resolve(); dist=args.dist.resolve()
    catalog=json.loads((ROOT/'native/catalog.json').read_text())
    out.mkdir(parents=True,exist_ok=True)
    if not args.no_build:
        run('dotnet','publish','native/Collection.csproj','-c','Release','-r',args.rid,
            '--self-contained','true',f'-p:LubRoot={lub}','-o',out,
            '--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
    elif not (out/'AbaCollection.dll').is_file():
        parser.error('--no-build requires an already published collection')
    for game in catalog:
        target=out/'games'/game['id']
        if not args.no_build:
            proj=project(game,lub,ROOT/'build/collection/projects'/game['id'])
            run('dotnet','build',proj,'-c','Release','-o',target,
                '--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
        elif not (target/'AbaGame.dll').is_file():
            parser.error(f'--no-build missing {target}/AbaGame.dll')
        if not args.managed_only:
            copy_assets(game,dist,target)
    if args.managed_only:
        print('Managed compilation passed; native libraries/assets not validated: '+str(out))
        return
    if args.native is None:
        parser.error('--native is required for a playable package')
    native=out/'native'; native.mkdir(exist_ok=True)
    expected=62 if args.rid=='linux-x64' else 183
    for name in ['liblub.so','libSDL3.so.0','libopenxr_loader.so.1','libslang-compiler.so.0.2026.8.1']:
        source=args.native/name
        header=source.read_bytes()[:20]
        if header[:4]!=b'\x7fELF' or int.from_bytes(header[18:20],'little')!=expected:
            raise ValueError(f'{source} is not {args.rid}')
        shutil.copy2(source,native/name)
    if args.physics:
        shutil.copy2(args.physics,native/'libmucade_physics.so')
        for name in ['ODE-LICENSE.TXT','ODE-LICENSE-BSD.TXT']:
            shutil.copy2(args.physics.parent/name,native/name)
    else:
        run(sys.executable,'tools/build_mucade_native.py','--output',native/'libmucade_physics.so')
    header=(native/'libmucade_physics.so').read_bytes()[:20]
    if int.from_bytes(header[18:20],'little')!=expected:
        raise ValueError('Mu-cade physics architecture mismatch; build on matching architecture')
    for source in [lub/'LICENSE',lub/'THIRD_PARTY_LICENSES.md']:
        shutil.copy2(source,native/('LUB-'+source.name))
    run(sys.executable,lub/'tools/package-frame-licenses.py',native,'--collection')
    for name,path in [('SDL','third_party/SDL/LICENSE.txt'),('OpenXR','third_party/openxr/LICENSE'),('Slang','third_party/slang/LICENSE')]:
        shutil.copy2(lub/path,native/f'LICENSE-{name}.txt')
    # The runtime-pack licenses are part of the self-contained distribution.
    assets=json.loads((ROOT/'native/obj/project.assets.json').read_text())
    package_name='microsoft.netcore.app.runtime.'+args.rid
    for folder in assets['packageFolders']:
        for package in (Path(folder)/package_name).glob('*'):
            for name in ['LICENSE.TXT','THIRD-PARTY-NOTICES.TXT']:
                if (package/name).exists(): shutil.copy2(package/name,out/('DOTNET-'+name))
    shutil.copy2(ROOT/'docs/collection.md',out/'README.md')
    (out/'run.sh').chmod(0o755)
    (out/'package.json').write_text(json.dumps({'runtime':args.rid,'games':catalog},indent=2))
    print(out)

if __name__=='__main__':
    main()
