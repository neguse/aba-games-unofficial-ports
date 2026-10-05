#!/usr/bin/env python3
"""Inspect a staged collection without launching games or changing save data."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import wave

ROOT=Path(__file__).resolve().parent.parent

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory',type=Path)
    parser.add_argument('--rid',required=True,choices=['linux-x64','linux-arm64'])
    parser.add_argument('--report',type=Path)
    args=parser.parse_args();root=args.directory.resolve()
    catalog=json.loads((ROOT/'native/catalog.json').read_text())
    manifest=json.loads((root/'package.json').read_text())
    assert manifest['runtime']==args.rid
    assert manifest['games']==catalog and len(catalog)==13
    machine=62 if args.rid=='linux-x64' else 183
    required=['AbaCollection','AbaCollection.dll','AbaCollection.deps.json','AbaCollection.runtimeconfig.json','Lub.dll','run.sh','collection.slang','font.ttf','FONT-LICENSE.txt','DOTNET-LICENSE.TXT','DOTNET-THIRD-PARTY-NOTICES.TXT','README.md']
    required += ['native/'+x for x in ['liblub.so','libSDL3.so.0','libopenxr_loader.so.1','libslang-compiler.so.0.2026.8.1','libmucade_physics.so','THIRD_PARTY_NOTICES.txt','ODE-LICENSE.TXT','ODE-LICENSE-BSD.TXT']]
    for name in required:
        p=root/name
        assert p.is_file() and p.stat().st_size>0, name
    assert (root/'run.sh').stat().st_mode&0o111
    audio_count=image_count=0
    for game in catalog:
        directory=root/'games'/game['id']
        for name in ['AbaGame.dll','AbaGame.deps.json','TinySystem.dll','LICENSE.txt']:
            assert (directory/name).is_file(),f'{game["id"]}: {name}'
        assert (directory/'Lub.dll').read_bytes()==(root/'Lub.dll').read_bytes(),f'{game["id"]}: shared Lub mismatch'
        for path in (directory/'audio').glob('*.wav'):
            with wave.open(str(path),'rb') as stream:
                assert stream.getsampwidth()==2 and stream.getnframes()>0,path
            audio_count+=1
        for path in (directory/'images').glob('*.png'):
            assert path.read_bytes().startswith(b'\x89PNG\r\n\x1a\n'),path
            image_count+=1
        if game['id']=='masashikun-hi':
            assert (directory/'source.tar.gz').stat().st_size>0
            assert (directory/'SOURCE-README.txt').stat().st_size>0
    libraries=[]
    for path in sorted(root.rglob('*')):
        if not path.is_file(): continue
        assert not path.is_symlink(),path
        with path.open('rb') as stream:header=stream.read(20)
        if not header.startswith(b'\x7fELF'):continue
        assert int.from_bytes(header[18:20],'little')==machine,path
        dynamic=subprocess.check_output(['readelf','-d',path],text=True,stderr=subprocess.DEVNULL)
        versions=subprocess.check_output(['readelf','--version-info',path],text=True,stderr=subprocess.DEVNULL)
        libraries.append({'path':str(path.relative_to(root)),
            'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
            'needed':re.findall(r'Shared library: \[([^]]+)\]',dynamic),
            'glibc':sorted(set(re.findall(r'\bGLIBC_[0-9.]+',versions))),
            'glibcxx':sorted(set(re.findall(r'\bGLIBCXX_[0-9.]+',versions)))})
    report={'status':'passed','rid':args.rid,'games':len(catalog),'pcm_wav_files':audio_count,'png_files':image_count,'elf_files':libraries,
            'verification_scope':'File/architecture/asset/license/dependency metadata only; no rendered pixels or physical device validation'}
    if args.report:
        args.report.parent.mkdir(parents=True,exist_ok=True);args.report.write_text(json.dumps(report,indent=2)+'\n')
    print(f"Package check passed: {args.rid}, {len(catalog)} games, {len(libraries)} ELF files, {audio_count} WAVs, {image_count} PNGs")
if __name__=='__main__':main()
