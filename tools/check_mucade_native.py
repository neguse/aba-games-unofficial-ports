#!/usr/bin/env python3
"""Check the native bridge against the existing web physics golden fixtures.

Builds a temporary SDK project under build/ with no packages or engine. The
fixture body and reference numbers come directly from BrowserHooks.cs, so web
and .NET exercise precisely the same ODE scenarios and tolerance. No game
source is edited or replaced, apart from choosing the native API implementation.
"""
import argparse
import os
import re
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--dotnet', default=os.environ.get('DOTNET', 'dotnet'))
    parser.add_argument('--skip-native-build', action='store_true')
    args = parser.parse_args()
    pattern = r'public static ([\w<>\[\], ]+) (\w+)\(([^)]*)\)'
    def api(path):
        return {name: (result, [p.strip().rsplit(' ', 1)[0] for p in arguments.split(',')] if arguments else [])
                for result, name, arguments in re.findall(pattern, path.read_text())}
    required = api(ROOT / 'games/mu-cade/OdeApi.cs')
    actual = api(ROOT / 'native/MucadePhysics.cs')
    for name, signature in required.items():
        if actual.get(name) != signature:
            raise AssertionError(f'Native ODE API mismatch: {name}')
    print(f'PASS: native bridge retains all {len(required)} OdeApi methods', flush=True)
    output = ROOT / 'build/native-physics'
    output.mkdir(parents=True, exist_ok=True)
    suffix = 'dylib' if sys.platform == 'darwin' else 'so'
    library = output / ('libmucade_physics.' + suffix)
    if not args.skip_native_build:
        subprocess.run([sys.executable, str(ROOT / 'tools/build_mucade_native.py'), '--output', str(library)], check=True)
    source = (ROOT / 'tests/mu-cade/BrowserHooks.cs').read_text()
    references = source.split('    static string failure;', 1)[1].split('    public static void Command(', 1)[0]
    methods = source[source.index('    static void Near('):]
    methods = methods.replace('static void RunPhysics(GameManager g)', 'static void RunPhysics()')
    methods = methods.replace('        g.clearAll();\n', '').replace('        g.startTitle();\n', '')
    fixture = ('using System;\npublic static class ReferencePhysics\n{\n    static string failure;\n'
               + references + '\n    public static void Run() { failure = null; RunPhysics(); if (failure != null) throw new Exception(failure); }\n' + methods)
    (output / 'ReferencePhysics.cs').write_text(fixture)
    project = output / 'PhysicsTests.csproj'
    project.write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <Nullable>disable</Nullable><CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="{ROOT / 'native/MucadePhysics.cs'}" />
    <Compile Include="{ROOT / 'tests/native-physics/MucadePhysicsTests.cs'}" />
    <Compile Include="ReferencePhysics.cs" />
    <None Include="{library}" Link="{library.name}" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
</Project>
''')
    env = os.environ.copy()
    env.setdefault('DOTNET_CLI_HOME', str(ROOT / '.cache/dotnet-home'))
    env.setdefault('NUGET_PACKAGES', str(ROOT / '.cache/nuget'))
    env.setdefault('DOTNET_SKIP_FIRST_TIME_EXPERIENCE', '1')
    env.setdefault('DOTNET_CLI_TELEMETRY_OPTOUT', '1')
    env.setdefault('DOTNET_GENERATE_ASPNET_CERTIFICATE', 'false')
    subprocess.run([args.dotnet, 'run', '--project', str(project), '--configuration', 'Release'], check=True, env=env)


if __name__ == '__main__':
    main()
