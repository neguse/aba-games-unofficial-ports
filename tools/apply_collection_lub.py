"""Apply the reviewed collection runtime delta to the pinned Lub checkout.

Does not fetch, publish, commit or change unrelated files. Repeated setup is
idempotent. A partially applied/conflicting patch stops with Git's diagnostics.
"""
import argparse
from pathlib import Path
import subprocess

PIN='362510eb1f596f69d983501d930e4dfbb57581c3'

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('lub',type=Path)
    args=parser.parse_args()
    root=args.lub.resolve()
    patch=Path(__file__).resolve().parent/'patches/lub-collection.patch'
    head=subprocess.check_output(['git','-C',str(root),'rev-parse','HEAD'],text=True).strip()
    if head!=PIN: parser.error(f'Expected Lub {PIN}, got {head}')
    command=['git','-C',str(root),'apply']
    if subprocess.run(command+['--reverse','--check',str(patch)],capture_output=True).returncode==0:
        print('Collection runtime patch is already applied')
        return
    subprocess.run(command+['--check',str(patch)],check=True)
    subprocess.run(command+[str(patch)],check=True)
    print('Applied collection runtime patch')

if __name__=='__main__':main()
