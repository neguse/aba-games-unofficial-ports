import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import urllib.request
import zipfile
from link_web import link, mucade


parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--tcs', type=Path, required=True)
parser.add_argument('--emsdk', type=Path, required=True)
parser.add_argument('--original', type=Path)
args = parser.parse_args()


def compiled(game, target, **native):
    source = Path('build/web-c') / game
    subprocess.run([sys.executable, 'tools/compile_game.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                    '--game', game, '--c', str(source)], check=True)
    link(args.lub, args.emsdk, source, target / 'wasm', **native)
    files = []
    for path in sorted((Path('build') / game / 'images').glob('*.png')):
        (target / 'images').mkdir(exist_ok=True)
        shutil.copy2(path, target / 'images' / path.name)
        files.append('images/' + path.name)
    (target / 'assets.json').write_text(json.dumps(files), encoding='utf-8')


original = args.original
if original is None:
    cache = Path('.cache')
    cache.mkdir(exist_ok=True)
    archive = cache / 'tf0_21.zip'
    if not archive.exists():
        urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/tf0_21.zip', archive)
    digest = hashlib.sha256(archive.read_bytes()).hexdigest()
    if digest != '774cbeab652c128e57cab6d4c03e262c28ab128241984ae755daa31c989b5e36':
        raise ValueError('Original archive checksum mismatch')
    with zipfile.ZipFile(archive) as source:
        source.extractall(cache / 'original')
    original = cache / 'original/tf'
dist = Path('dist')
dist.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_barrage.py', str(original / 'barrage'), 'build/BarrageCode.cs'], check=True)
subprocess.run([sys.executable, 'tools/compile_data.py', str(original), 'build/GameData.cs'], check=True)
compiled('tumiki', dist)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), 'dist/shaders.json', 'shaders/mesh'], check=True)
for path in Path('web').iterdir():
    shutil.copy2(path, dist / path.name)
shutil.copytree(original / 'sounds', dist / 'audio', dirs_exist_ok=True)
licenses = Path('LICENSE').read_text() + '\n\nlub\n---\n' + (args.lub / 'LICENSE').read_text()
licenses += '\n\n' + (args.lub / 'THIRD_PARTY_LICENSES.md').read_text()
licenses += '\n\ntcs2c runtime\n---\n' + (args.tcs / 'LICENSE').read_text()
(dist / 'LICENSE.txt').write_text(licenses)
archive = Path('.cache/p47_0_21.zip')
if not archive.exists():
    archive.parent.mkdir(exist_ok=True)
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/p47_0_21.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '408926dfb368fe87f655a123798fcaf5c9c1956aeb63f3d59d0aede2c52635cb':
    raise ValueError('PARSEC47 archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
p47 = Path('.cache/original/p47')
target = dist / 'parsec47'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_parsec47.py', str(p47)], check=True)
compiled('parsec47', target)
shutil.copy2('games/parsec47/index.html', target / 'index.html')
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'shaders/mesh'], check=True)
shutil.copytree(p47 / 'sounds', target / 'audio', dirs_exist_ok=True)
(target / 'LICENSE.txt').write_text((p47 / 'readme_e.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/gr0_15.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/gr0_15.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '6ec5cf6f0a28cba738f51020629b2e1c4b6a7298712caa1764f0189c2dec508f':
    raise ValueError('Gunroar archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
gr = Path('.cache/original/gr')
target = dist / 'gunroar'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_gunroar.py', str(gr)], check=True)
compiled('gunroar', target)
shutil.copy2('games/gunroar/index.html', target / 'index.html')
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'games/gunroar/game'], check=True)
(target / 'audio').mkdir(exist_ok=True)
for directory in ['chunks', 'musics']:
    for audio in (gr / 'sounds' / directory).iterdir():
        shutil.copy2(audio, target / 'audio' / audio.name)
(target / 'LICENSE.txt').write_text((gr / 'readme_e.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/ttn0_3.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/ttn0_3.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '7d1d1cb9f8ba754f3df303fc28697fbf16df409ccc98cc65619dd03462da8df3':
    raise ValueError('Titanion archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
ttn = Path('.cache/original/ttn')
target = dist / 'titanion'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_titanion.py', str(ttn)], check=True)
compiled('titanion', target)
shutil.copy2('games/titanion/index.html', target / 'index.html')
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'shaders/mesh'], check=True)
(target / 'audio').mkdir(exist_ok=True)
for directory in ['chunks', 'musics']:
    for audio in (ttn / 'sounds' / directory).iterdir():
        shutil.copy2(audio, target / 'audio' / audio.name)
(target / 'LICENSE.txt').write_text((ttn / 'readme_e.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/a7xpg0_11.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/a7xpg0_11.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '128c0485794732262e1685e2afaf7521f2dd9468ac8587f0afb166b4e5cf4b12':
    raise ValueError('A7Xpg archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
a7x = Path('.cache/original/a7xpg')
target = dist / 'a7xpg'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_a7xpg.py', str(a7x)], check=True)
compiled('a7xpg', target)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'shaders/mesh'], check=True)
shutil.copy2('games/a7xpg/index.html', target / 'index.html')
shutil.copytree(a7x / 'sounds', target / 'audio', dirs_exist_ok=True)
(target / 'LICENSE.txt').write_text((a7x / 'readme_e.txt').read_text() + '\n\n' + Path('games/a7xpg/PHOBOS-LICENSE.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/tt0_22.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/tt0_22.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '6fcbb3de9ac5cfce38253f71257143631a978c625b041c8527d18ddb5c8813fa':
    raise ValueError('Torus Trooper archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
tt = Path('.cache/original/tt')
target = dist / 'torus-trooper'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_torus.py', str(tt)], check=True)
subprocess.run([sys.executable, 'tools/compile_frame_web.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                '--emsdk', str(args.emsdk), '--game', 'torus-trooper', '--original', str(tt), '--output', str(target)], check=True)
archive = Path('.cache/rr0_24.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/rr0_24.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != 'd8bb5124d996fab4c56fa2784e660c4a2d642301658a4d55bcad0684deaf8ef8':
    raise ValueError('rRootage archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
rr = Path('.cache/original/rr')
target = dist / 'rrootage'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_rrootage.py', str(rr)], check=True)
compiled('rrootage', target)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'shaders/mesh'], check=True)
shutil.copy2('games/rrootage/index.html', target / 'index.html')
shutil.copytree(rr / 'sounds', target / 'audio', dirs_exist_ok=True)
(target / 'LICENSE.txt').write_text((rr / 'LICENSE.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/noiz2sa0_52.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/noiz2sa0_52.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '959759140a80b3cc718946a118b6825fd5218f3186138a51a7819c5fb938dffc':
    raise ValueError('Noiz2sa archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
nr = Path('.cache/original/noiz2sa')
target = dist / 'noiz2sa'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_noiz2sa.py', str(nr)], check=True)
compiled('noiz2sa', target)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'games/noiz2sa/game'], check=True)
shutil.copy2('games/noiz2sa/index.html', target / 'index.html')
shutil.copytree(nr / 'sounds', target / 'audio', dirs_exist_ok=True)
(target / 'LICENSE.txt').write_text((nr / 'readme_e.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/wok_src1_0.tar.gz')
if not archive.exists():
    urllib.request.urlretrieve('https://www.asahi-net.or.jp/~cs8k-cyu/linux/wok_src1_0.tar.gz', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != 'c8a7571c9d3e28dae691f8dc896fc6fed7c220b0d631ac766c46808a896aa60f':
    raise ValueError('Wok archive checksum mismatch')
with tarfile.open(archive) as source:
    source.extractall('.cache/original', filter='data')
wok = Path('.cache/original/wok')
target = dist / 'wok'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_wok.py', str(wok)], check=True)
compiled('wok', target)
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'games/wok/game'], check=True)
shutil.copy2('games/wok/index.html', target / 'index.html')
(target / 'audio').mkdir(exist_ok=True)
for path in (wok / 'sounds').glob('*.wav'):
    shutil.copy2(path, target / 'audio' / path.name)
for path in (wok / 'sounds').glob('*.ogg'):
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-c:a', 'libvorbis', '-i', str(path),
                    '-c:a', 'pcm_s16le', str(target / 'audio' / (path.stem + '.wav'))], check=True)
(target / 'LICENSE.txt').write_text(Path('games/wok/LICENSE.txt').read_text() + '\n\n' + licenses)
archive = Path('.cache/Mm0_14.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/xna/mm/Mm0_14.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '286e6081a0d887c1f75069298174ab9bb43d43daa8d0f5288a744d9f9d58d533':
    raise ValueError('Mazer Mayhem archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
mm = Path('.cache/original/Mm/Mm')
target = dist / 'mazer-mayhem'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_mazer.py', str(mm)], check=True)
subprocess.run([sys.executable, 'tools/compile_frame_web.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                '--emsdk', str(args.emsdk), '--game', 'mazer-mayhem', '--original', str(mm), '--output', str(target)], check=True)
archive = Path('.cache/GearToyGear0_1.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/xna/gtg/GearToyGear0_1.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != 'b068c6b1dd5a7bfcc65830ba6fe946dbc6182ac720c58d6b3518546b5f22f04a':
    raise ValueError('GearToyGear archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
gtg = Path('.cache/original/GearToyGear/GearToyGear')
target = dist / 'gear-toy-gear'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/compile_gear.py', str(gtg)], check=True)
subprocess.run([sys.executable, 'tools/compile_frame_web.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                '--emsdk', str(args.emsdk), '--game', 'gear-toy-gear', '--original', str(gtg), '--output', str(target)], check=True)
archive = Path('.cache/mcd0_11.zip')
if not archive.exists():
    urllib.request.urlretrieve('https://abagames.sakura.ne.jp/windows/mcd0_11.zip', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != 'e5acd67e06d765c63ea7dc7df6488ca3edd3410a05bd995b3842fe8e3b78451e':
    raise ValueError('Mu-cade archive checksum mismatch')
with zipfile.ZipFile(archive) as source:
    source.extractall('.cache/original')
mcd = Path('.cache/original/mcd')
target = dist / 'mu-cade'
target.mkdir(exist_ok=True)
subprocess.run([sys.executable, 'tools/build_mucade_runtime.py', '--lub', str(args.lub)], check=True)
subprocess.run([sys.executable, 'tools/compile_mucade.py', str(mcd)], check=True)
compiled('mu-cade', target, **mucade)
shutil.copy2('games/mu-cade/index.html', target / 'index.html')
subprocess.run(['node', 'tools/compile_shaders.mjs', str(args.lub), str(target / 'shaders.json'), 'shaders/mesh'], check=True)
(target / 'audio').mkdir(exist_ok=True)
for path in (mcd / 'sounds').rglob('*'):
    if path.is_file():
        shutil.copy2(path, target / 'audio' / path.name)
(target / 'LICENSE.txt').write_text(Path('games/mu-cade/LICENSE.txt').read_text() + '\n\n' + licenses)
subprocess.run([sys.executable, 'tools/build_masashikun.py', '--lub', str(args.lub), '--tcs', str(args.tcs),
                '--emsdk', str(args.emsdk)], check=True)
print('Built dist/')
