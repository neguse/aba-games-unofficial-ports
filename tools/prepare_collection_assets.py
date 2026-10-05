#!/usr/bin/env python3
"""Prepare native collection assets from checksum-pinned cached originals.

No downloads, transpilation, WASM build, publication, or changes to dist/. Run
this after the data/image generators, then pass --dist build/collection-assets
to build_collection.py. Audio transformations match the existing native ports.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import io
import json
from pathlib import Path
import re
import shutil
import subprocess
import tarfile
import wave

ROOT = Path(__file__).resolve().parents[1]
# Keep these pins identical to build.py and build_masashikun.py.
ORIGINALS = {
    'tumiki': ('tf', 'tf0_21.zip', '774cbeab652c128e57cab6d4c03e262c28ab128241984ae755daa31c989b5e36'),
    'parsec47': ('p47', 'p47_0_21.zip', '408926dfb368fe87f655a123798fcaf5c9c1956aeb63f3d59d0aede2c52635cb'),
    'gunroar': ('gr', 'gr0_15.zip', '6ec5cf6f0a28cba738f51020629b2e1c4b6a7298712caa1764f0189c2dec508f'),
    'titanion': ('ttn', 'ttn0_3.zip', '7d1d1cb9f8ba754f3df303fc28697fbf16df409ccc98cc65619dd03462da8df3'),
    'a7xpg': ('a7xpg', 'a7xpg0_11.zip', '128c0485794732262e1685e2afaf7521f2dd9468ac8587f0afb166b4e5cf4b12'),
    'torus-trooper': ('tt', 'tt0_22.zip', '6fcbb3de9ac5cfce38253f71257143631a978c625b041c8527d18ddb5c8813fa'),
    'rrootage': ('rr', 'rr0_24.zip', 'd8bb5124d996fab4c56fa2784e660c4a2d642301658a4d55bcad0684deaf8ef8'),
    'noiz2sa': ('noiz2sa', 'noiz2sa0_52.zip', '959759140a80b3cc718946a118b6825fd5218f3186138a51a7819c5fb938dffc'),
    'wok': ('wok', 'wok_src1_0.tar.gz', 'c8a7571c9d3e28dae691f8dc896fc6fed7c220b0d631ac766c46808a896aa60f'),
    'mazer-mayhem': ('Mm/Mm', 'Mm0_14.zip', '286e6081a0d887c1f75069298174ab9bb43d43daa8d0f5288a744d9f9d58d533'),
    'gear-toy-gear': ('GearToyGear/GearToyGear', 'GearToyGear0_1.zip', 'b068c6b1dd5a7bfcc65830ba6fe946dbc6182ac720c58d6b3518546b5f22f04a'),
    'mu-cade': ('mcd', 'mcd0_11.zip', 'e5acd67e06d765c63ea7dc7df6488ca3edd3410a05bd995b3842fe8e3b78451e'),
    'masashikun-hi': ('mas', 'mas1_11e.zip', 'e2ab82d97560a81b94516009869de0ea46bb60aae18b1e1176ed0b5082f39258'),
}
IMAGE_GAMES = {'parsec47', 'gunroar', 'titanion', 'a7xpg', 'torus-trooper', 'rrootage', 'noiz2sa', 'wok', 'mu-cade'}


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def text(path):
    # Original Japanese documentation is Shift-JIS; English notices are ASCII.
    data = path.read_bytes()
    try:
        return data.decode('utf-8')
    except UnicodeDecodeError:
        return data.decode('cp932')


def licenses(game, original, lub, tcs):
    notices = []
    for name in ['readme_e.txt', 'LICENSE.txt', 'license.txt']:
        path = original / name
        if path.is_file():
            notices.append((f'Original {game}: {name}', path))
    local = ROOT / 'games' / game / 'LICENSE.txt'
    if local.exists():
        notices.append((f'{game} adaptation', local))
    if game == 'a7xpg':
        notices.append(('Phobos', ROOT / 'games/a7xpg/PHOBOS-LICENSE.txt'))
    notices += [('Shared port sources', ROOT / 'LICENSE'), ('lub', lub / 'LICENSE'),
                ('lub third-party notices', lub / 'THIRD_PARTY_LICENSES.md'), ('TinySystem', tcs / 'LICENSE')]
    return '\n\n'.join(f'{label}\n{"=" * len(label)}\n{text(path)}' for label, path in notices) + '\n'


def audio_jobs(game, original, output):
    directory = original / ('Content/Audio' if game in {'gear-toy-gear', 'mazer-mayhem'} else 'sounds')
    paths = sorted(p for p in directory.rglob('*') if p.suffix.lower() in {'.wav', '.ogg', '.mp3'})
    if game != 'masashikun-hi' and not paths:
        raise FileNotFoundError(f'{directory}: original audio is missing')
    volumes = {}
    if game == 'gear-toy-gear':
        volumes = dict(re.findall(r'Sound\s*\{\s*Name = (\w+);\s*Volume = ([-\d]+)', text(directory / 'Gtg.xap')))
    jobs, used = [], set()
    for path in paths:
        name = path.stem + '.wav'
        if name in used:
            raise ValueError(f'{game}: flattened audio filename collision: {name}')
        used.add(name)
        filters = []
        # These are intentionally identical to build_gtg_frame/build_mm_frame.
        if game == 'gear-toy-gear':
            gain = .5 * 10 ** (int(volumes[path.stem]) / 2000)
            filters = ['-af', f'volume={gain}']
        elif game == 'mazer-mayhem':
            gain = '-7dB' if path.stem in ['Mm1', 'Mm2', 'Mm3'] else '-12dB'
            filters = ['-af', f'volume={gain}']
        command = ['ffmpeg', '-v', 'error', '-nostdin', '-y']
        if game == 'wok' and path.suffix.lower() == '.ogg':
            command += ['-c:a', 'libvorbis']
        command += ['-i', str(path), *filters, '-c:a', 'pcm_s16le', str(output / name)]
        jobs.append(command)
    return jobs


def validate_wave(path):
    with wave.open(str(path), 'rb') as wav:
        if wav.getcomptype() != 'NONE' or wav.getsampwidth() != 2 or wav.getnframes() == 0:
            raise ValueError(f'{path}: expected nonempty signed 16-bit PCM WAV')


def tracked_sources(directory):
    """Include current source bytes, including local fixes, but never Git history."""
    names = subprocess.check_output(['git', '-C', str(directory), 'ls-files', '--recurse-submodules', '-z']).decode().split('\0')
    for name in names:
        if not name:
            continue
        path = directory / name
        if path.is_file() and not path.is_symlink():
            yield path
    # New files from this collection integration are not committed yet. Keep
    # this explicit so unrelated untracked files can never enter the bundle.
    for name in ['dotnet/Lub/LubSession.cs', 'tests/c/session_gamepad_smoke.c',
                 'tests/c/session_test_host.c']:
        if name not in names and (directory / name).is_file():
            yield directory / name


def source_bundle(target, original, lub, tcs):
    """Ship the GPL game's editable source and a standalone native rebuild path."""
    prefix = 'masashikun-hi-source/'
    included = {}
    def add(bundle, path, name):
        data = path.read_bytes()
        put(bundle, name, data, path.stat().st_mode & 0o777)
    def put(bundle, name, data, mode=0o644):
        if name in included:
            raise ValueError(f'Duplicate source bundle entry: {name}')
        entry = tarfile.TarInfo(prefix + name)
        entry.size = len(data); entry.mode = mode; entry.mtime = 0
        bundle.addfile(entry, io.BytesIO(data))
        included[name] = hashlib.sha256(data).hexdigest()

    readme = '''Masashikun Hi! native corresponding source
=========================================
The game and original data are GPL-2.0-or-later (see LICENSE.txt). Its C#
adaptation was modified 2026-09-24; this native integration and source
packaging were added 2026-10-05. Original Pascal sources and editable data
are included. The native host and its rebuild script are included with the
work; dependency files retain their own notices and licenses.

This archive contains only this game, its shared native host, build support,
and dependency source files. It contains no private repository history,
credentials, or unrelated games. SOURCE-MANIFEST.json gives SHA-256 for each
included file. DEPENDENCIES.json records the dependency revisions.

Requirements: Python 3, .NET 10 SDK, and (to build the native graphics engine)
CMake 3.22+, a C/C++ compiler and the Linux development packages required by
SDL3/OpenXR. To rebuild the managed game and host without network access to
the original repository:

    python3 build-native.py --rid linux-x64

Use linux-arm64 on an ARM64 builder. The script regenerates game data and
shader wrappers, then builds only Masashikun and the host. The build may
retrieve .NET SDK runtime packs from the configured NuGet source.

To build native libraries from the supplied lub/ source:

    cmake -S lub -B native-build -DCMAKE_BUILD_TYPE=Release
    cmake --build native-build --target lub_shared --parallel 4

CMake retrieves the pinned Slang compiler release if absent. Slang's source
is available from https://github.com/shader-slang/slang/tree/v2026.8.1 and is
Apache-2.0 WITH LLVM-exception. .NET runtime source is available from
https://github.com/dotnet/runtime and carries its own MIT license/notices.
Existing separately supplied native libraries can be used instead:

    python3 build-native.py --rid linux-x64 --native /path/to/native-libraries
    ./publish/run.sh

The native directory must provide matching-architecture liblub.so,
libSDL3.so.0, libopenxr_loader.so.1 and libslang-compiler.so.0.2026.8.1, with
all their license/notice files. No original audio is required by this game.
Keep source.tar.gz and SOURCE-README.txt alongside the native game binary.
Do not distribute unrelated proprietary material or repository history.
'''
    builder = r'''#!/usr/bin/env python3
"""Rebuild just the GPL game and its native host from this source bundle."""
import argparse, json, pathlib, shutil, subprocess, sys
import xml.etree.ElementTree as E
root = pathlib.Path(__file__).resolve().parent
p = argparse.ArgumentParser(); p.add_argument('--rid', default='linux-x64', choices=['linux-x64','linux-arm64']); p.add_argument('--native', type=pathlib.Path); a=p.parse_args()
def run(*args): subprocess.run([str(x) for x in args], cwd=root, check=True)
run(sys.executable, 'tools/compile_masashikun.py', 'original')
shader=root/'build/shaders/masashikun-hi/Shaders.cs'; shader.parent.mkdir(parents=True,exist_ok=True)
shader.write_text('public static class GameShaders {\n'+''.join('public static string '+n+' = '+json.dumps((root/('games/masashikun-hi/game.'+s+'.slang')).read_text())+';\n' for n,s in [('vertex','vs'),('fragment','fs')])+'}\n')
project=E.Element('Project',Sdk='Microsoft.NET.Sdk'); props=E.SubElement(project,'PropertyGroup')
for k,v in {'TargetFramework':'net10.0','AssemblyName':'AbaGame','EnableDefaultCompileItems':'false','ImplicitUsings':'enable','InvariantGlobalization':'true','GenerateDependencyFile':'true'}.items(): E.SubElement(props,k).text=v
refs=E.SubElement(project,'ItemGroup')
for source in [root/'lub/dotnet/Lub/Lub.csproj',root/'lub/third_party/tcs/TinySystem/TinySystem.csproj']: E.SubElement(refs,'ProjectReference',Include=str(source))
for source in [*sorted((root/'games/masashikun-hi').glob('*.cs')),*sorted((root/'build/masashikun-hi').glob('*.cs')),shader]: E.SubElement(refs,'Compile',Include=str(source))
proj=root/'build/Masashikun.csproj'; E.ElementTree(project).write(proj,encoding='unicode')
out=root/'publish'; target=out/'games/masashikun-hi'
run('dotnet','publish','native/Collection.csproj','-c','Release','-r',a.rid,'--self-contained','true','-p:LubRoot='+str(root/'lub'),'-o',out,'--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
run('dotnet','build',proj,'-c','Release','-o',target,'--disable-build-servers','-m:1','-p:UseSharedCompilation=false')
shutil.copy2(root/'DISTRIBUTION-LICENSE.txt',target/'LICENSE.txt'); shutil.copy2(root/'README.txt',target/'SOURCE-README.txt')
(out/'run.sh').chmod(0o755)
if a.native:
    shutil.copytree(a.native.resolve(),out/'native',dirs_exist_ok=True)
print(out)
'''
    # Another build may copy assets concurrently; never expose a partial tar.
    temporary = target / 'source.tar.gz.tmp'
    with tarfile.open(temporary, 'w:gz') as bundle:
        for directory in ['games/masashikun-hi', 'tests/masashikun-hi']:
            for path in sorted((ROOT / directory).rglob('*')):
                if path.is_file() and not any(x in {'bin','obj','__pycache__'} for x in path.relative_to(ROOT / directory).parts):
                    add(bundle, path, path.relative_to(ROOT).as_posix())
        for path in sorted(original.rglob('*')):
            if path.is_file() and (path.suffix.lower() in {'.pas','.dpr','.dfm','.ldt'} or path.name.lower() in {'readme.txt','license.txt'}):
                add(bundle, path, 'original/' + path.relative_to(original).as_posix())
        for path in sorted((ROOT / 'native').iterdir()):
            if path.is_file() and path.name not in {'catalog.json','MucadePhysics.cs','mucade_physics.c'}:
                add(bundle, path, 'native/' + path.name)
        put(bundle, 'native/catalog.json', b'[{"id":"masashikun-hi","title":"Masashikun Hi!","vr":false}]\n')
        for name in ['compile_masashikun.py', 'compile_game.py', 'build_collection.py',
                     'prepare_collection_assets.py', 'apply_collection_lub.py', 'setup_lub.sh']:
            add(bundle, ROOT / 'tools' / name, 'tools/' + name)
        for path in sorted((ROOT / 'build/masashikun-hi').glob('*.cs')):
            add(bundle, path, 'build/masashikun-hi/' + path.name)
        add(bundle, ROOT / 'games/masashikun-hi/LICENSE.txt', 'LICENSE.txt')
        add(bundle, target / 'LICENSE.txt', 'DISTRIBUTION-LICENSE.txt')
        add(bundle, ROOT / 'LICENSE', 'SHARED-PORT-LICENSE.txt')
        # The tracked public engine source includes vendored static dependencies,
        # managed facade, TinySystem, headers and its original build scripts.
        # Exclude examples/unrelated assets, history, and generated binaries.
        allowed = {'src','include','cs-lib','dotnet','third_party','tools','scripts','tests'}
        for path in tracked_sources(lub):
            rel = path.relative_to(lub)
            if len(rel.parts) == 1 or rel.parts[0] in allowed:
                if rel.parts[0].startswith('.') or path.name in {'AGENTS.md','CLAUDE.md','tasks.md'}:
                    continue
                if path.suffix.lower() in {'.dll','.exe','.so','.a','.dylib','.pdb','.wasm'}:
                    continue
                add(bundle, path, 'lub/' + rel.as_posix())
        # Native host's font is explicitly redistributable under OFL.
        for name in ['MPLUS1p-subset.ttf','MPLUS1p-OFL.txt']:
            add(bundle, lub / 'samples/data/fonts' / name, 'lub/samples/data/fonts/' + name)
        revisions = {}
        for name, directory in [('lub',lub),('TinySystem',tcs)]:
            revisions[name] = subprocess.check_output(['git','-C',str(directory),'rev-parse','HEAD'],text=True).strip()
        revisions['note'] = 'Source bytes include current local integration fixes; hashes are in SOURCE-MANIFEST.json.'
        put(bundle, 'DEPENDENCIES.json', json.dumps(revisions,indent=2).encode())
        put(bundle, 'README.txt', readme.encode())
        put(bundle, 'build-native.py', builder.encode(), 0o755)
        put(bundle, 'SOURCE-MANIFEST.json', json.dumps(included,indent=2,sort_keys=True).encode())
    temporary.replace(target / 'source.tar.gz')
    (target / 'SOURCE-README.txt').write_text(readme, encoding='utf-8')
    return len(included)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--tcs', type=Path)
    parser.add_argument('--cache', type=Path, default=ROOT / '.cache')
    parser.add_argument('--output', type=Path, default=ROOT / 'build/collection-assets')
    parser.add_argument('--jobs', type=int, default=4)
    parser.add_argument('--source-only', action='store_true', help='refresh the source bundle after native host changes, without reconverting audio')
    args = parser.parse_args()
    lub = args.lub.resolve(); tcs = (args.tcs or lub / 'third_party/tcs').resolve()
    cache = args.cache.resolve(); output = args.output.resolve()
    if output == ROOT / 'dist' or ROOT / 'dist' in output.parents or output == ROOT:
        parser.error('--output must be a separate native asset directory, not dist/ or the repository root')
    if args.jobs < 1:
        parser.error('--jobs must be positive')
    if not args.source_only and not shutil.which('ffmpeg'):
        parser.error('ffmpeg is required')
    catalog = json.loads((ROOT / 'native/catalog.json').read_text())
    if {game['id'] for game in catalog} != set(ORIGINALS):
        raise ValueError('Native catalog and asset definitions differ')
    verified = {}
    for game, (directory, filename, expected) in ORIGINALS.items():
        archive = cache / filename
        if digest(archive) != expected:
            raise ValueError(f'{archive}: original archive checksum mismatch')
        if not (cache / 'original' / directory).is_dir():
            raise FileNotFoundError(f'{directory}: extract the verified original archive first')
        verified[filename] = expected
    output.mkdir(parents=True,exist_ok=True)
    report = {'archives':verified,'games':{}}
    if args.source_only:
        target = output / 'masashikun-hi'
        target.mkdir(parents=True, exist_ok=True)
        count = source_bundle(target, cache / 'original/mas', lub, tcs)
        report_path = output / 'collection-assets.json'
        if report_path.exists():
            report = json.loads(report_path.read_text())
            report['games']['masashikun-hi']['source_files'] = count
            report['games']['masashikun-hi']['source_sha256'] = digest(target / 'source.tar.gz')
            report_path.write_text(json.dumps(report, indent=2) + '\n')
        print(f'Refreshed {count} source-bundle files: {target}', flush=True)
        return
    for game in ORIGINALS:
        original = cache / 'original' / ORIGINALS[game][0]
        target = output if game == 'tumiki' else output / game
        target.mkdir(parents=True,exist_ok=True)
        audio = target / 'audio'; audio.mkdir(exist_ok=True)
        jobs = audio_jobs(game, original, audio)
        with ThreadPoolExecutor(max_workers=args.jobs) as workers:
            list(workers.map(lambda command: subprocess.run(command,check=True), jobs))
        for path in audio.glob('*.wav'):
            validate_wave(path)
        image_files = sorted((ROOT / 'build' / game / 'images').glob('*.png'))
        if game in IMAGE_GAMES and not image_files:
            raise FileNotFoundError(f'build/{game}/images: run the original data/image generator first')
        for path in image_files:
            (target / 'images').mkdir(exist_ok=True)
            shutil.copy2(path,target / 'images' / path.name)
        shaders = []
        if game in {'gear-toy-gear','mazer-mayhem'}:
            shaders = [ROOT / 'games' / game / name for name in ['game.vs.slang','game.fs.slang']]
            shaders += [ROOT / 'games/frame/scene.output.slang']
        elif game == 'torus-trooper':
            shaders = sorted((ROOT / 'games/torus-trooper/frame').glob('mesh.*.slang'))
        for path in shaders:
            shutil.copy2(path,target / path.name)
        (target / 'LICENSE.txt').write_text(licenses(game,original,lub,tcs),encoding='utf-8')
        files = [str(Path('audio') / Path(job[-1]).name) for job in jobs]
        files += ['images/' + path.name for path in image_files] + [path.name for path in shaders]
        (target / 'assets.json').write_text(json.dumps(files,indent=2)+'\n')
        report['games'][game] = {'audio':len(jobs),'images':len(image_files),'shaders':len(shaders)}
        print(f'{game}: {len(jobs)} PCM WAV, {len(image_files)} images, license ready',flush=True)
    count = source_bundle(output / 'masashikun-hi', cache / 'original/mas', lub, tcs)
    report['games']['masashikun-hi']['source_files'] = count
    report['games']['masashikun-hi']['source_sha256'] = digest(output / 'masashikun-hi/source.tar.gz')
    (output / 'collection-assets.json').write_text(json.dumps(report,indent=2)+'\n')
    print(f'Prepared {len(ORIGINALS)} games and {count} source-bundle files: {output}',flush=True)


if __name__ == '__main__':
    main()
