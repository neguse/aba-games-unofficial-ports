import argparse
import json
import os
from pathlib import Path
import subprocess


def compile_game(lub, tcs, output):
    root = Path(__file__).resolve().parent.parent
    project = root / 'games/torus-trooper/frame/TorusTrooper.csproj'
    result = subprocess.run(['dotnet', 'msbuild', str(project), '-nologo', '-getItem:Compile',
                             f'-p:LubRoot={lub}'], check=True, capture_output=True, text=True)
    sources = [Path(item['FullPath']) for item in json.loads(result.stdout)['Items']['Compile']]
    sources = [path for path in sources if path != project.parent / 'Program.cs']
    output.mkdir(parents=True, exist_ok=True)
    subprocess.run(['dotnet', 'build', str(tcs / 'tcs2c/tcs2c.csproj'), '-c', 'Release'], check=True)
    subprocess.run(['dotnet', 'build', str(lub / 'tools/lub-gen/lub-gen.csproj'), '-c', 'Release'], check=True)
    subprocess.run(['dotnet', str(tcs / 'tcs2c/bin/Release/net10.0/tcs2c.dll'), '--lib',
                    '--ref', str(lub / 'cs-lib/lub_stub.cs'), *map(str, sources), '-o', str(output / 'game.c')], check=True)
    subprocess.run(['dotnet', str(lub / 'tools/lub-gen/bin/Release/net10.0/lub-gen.dll'), 'tcs',
                    '--stub', str(lub / 'cs-lib/lub_stub.cs'),
                    *[arg for path in sources for arg in ['--source', str(path)]],
                    '-o', str(output / 'binding.c')], check=True)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--lub', type=Path, required=True)
    parser.add_argument('--tcs', type=Path, required=True)
    parser.add_argument('--output', type=Path, default=Path('build/torus-c'))
    parser.add_argument('--native', type=Path)
    parser.add_argument('--executable', type=Path)
    parser.add_argument('--cc', default=os.environ.get('CC', 'cc'))
    args = parser.parse_args()
    if bool(args.native) != bool(args.executable):
        parser.error('--native and --executable must be used together')
    lub, tcs, output = args.lub.resolve(), args.tcs.resolve(), args.output.resolve()
    compile_game(lub, tcs, output)
    if args.native:
        host = output / 'host.c'
        host.write_text(f'#define LUB_TCS_GAME "{output.as_posix()}/game.c"\n'
                        f'#define LUB_TCS_BINDING "{output.as_posix()}/binding.c"\n'
                        f'#include "{lub.as_posix()}/src/tcs_host.c"\n', encoding='utf-8')
        args.executable.parent.mkdir(parents=True, exist_ok=True)
        flags = [] if os.name == 'nt' else ['-lm', '-Wl,-rpath,$ORIGIN/native']
        subprocess.run([args.cc, '-std=gnu11', '-O2', '-fwrapv', '-ffp-contract=off',
                        '-fexcess-precision=standard', '-I', str(lub / 'include'), str(host),
                        str(args.native.resolve()), *flags, '-o', str(args.executable)], check=True)
