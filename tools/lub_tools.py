import os
from pathlib import Path
import subprocess


def tcs(lub):
    """The command that starts tcs: LUB_TCS if set (lub reads the same variable), otherwise lub's .NET tcs."""
    if os.environ.get('LUB_TCS'):
        return os.environ['LUB_TCS'].split()
    subprocess.run(['dotnet', 'build', str(lub / 'src/tcs'), '-c', 'Release', '-nologo', '-v', 'q',
                    '-o', str(lub / 'build-tcs')], check=True)
    return ['dotnet', str(lub / 'build-tcs/tcs.dll')]


def lub_gen(lub):
    """The command that starts lub-gen."""
    subprocess.run(['dotnet', 'build', str(lub / 'tools/lub-gen'), '-c', 'Release', '-nologo', '-v', 'q',
                    '-o', str(lub / 'build-lub-gen')], check=True)
    return ['dotnet', str(lub / 'build-lub-gen/lub-gen.dll')]


def to_c(lub, sources, output, refs=()):
    """Write game.c (tcs c) and binding.c (lub-gen tcs) for the C# `sources` into `output`.

    `refs` are stubs of native APIs of the game's own, which the game calls but tcs does not compile.
    """
    lub, output = Path(lub).resolve(), Path(output)
    output.mkdir(parents=True, exist_ok=True)
    stub = lub / 'cs-lib/lub_stub.cs'
    subprocess.run([*tcs(lub), 'c', '--lib', '--ref', str(stub), *[arg for path in refs for arg in ['--ref', str(path)]],
                    *map(str, sources), '-o', str(output / 'game.c')], check=True)
    # lub-gen takes a single stub, so it reads the game's own stubs as sources.
    subprocess.run([*lub_gen(lub), 'tcs', '--stub', str(stub), '--entry', 'Standalone',
                    *[arg for path in [*sources, *refs] for arg in ['--source', str(path)]],
                    '-o', str(output / 'binding.c')], check=True)
