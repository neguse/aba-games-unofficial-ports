#!/usr/bin/env python3
"""Build Mu-cade's .NET ODE C ABI using the same pinned solver as the web port.

No system packages, Lua, or Emscripten are needed. The resulting shared library
must be placed beside the native launcher/game assemblies. Only ODE source code
from the existing project's checksum-pinned upstream archive is downloaded.
"""
import argparse
import hashlib
import os
from pathlib import Path
import subprocess
import sys
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
REVISION = '7bac210f051b3ffcfaf9a168db3d7c302f7a49a4'
SHA256 = '538f050c5882d6b03afa5d54dd894eb7df62df4d635ef459ce2f02de059cd4c9'
CONFIG = '''#ifndef _ODE_CONFIG_H_
#define _ODE_CONFIG_H_
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>
#include <stdarg.h>
#include <alloca.h>
#include <float.h>
#include <stdint.h>
typedef char int8;
typedef unsigned char uint8;
typedef short int16;
typedef unsigned short uint16;
typedef int int32;
typedef unsigned int uint32;
typedef uintptr_t intP;
#define dDOUBLE 1
#define dInfinity DBL_MAX
#endif
'''


def build(output, cxx):
    if sys.platform not in ('linux', 'darwin'):
        raise SystemExit('This ODE helper currently supports Linux and macOS native builds.')
    archive = ROOT / '.cache/ode-0.5.0.tar.gz'
    archive.parent.mkdir(parents=True, exist_ok=True)
    if not archive.exists():
        temporary = archive.with_suffix('.download')
        urllib.request.urlretrieve(f'https://bitbucket.org/odedevs/ode/get/{REVISION}.tar.gz', temporary)
        if hashlib.sha256(temporary.read_bytes()).hexdigest() != SHA256:
            raise ValueError('ODE archive checksum mismatch')
        temporary.replace(archive)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != SHA256:
        raise ValueError('ODE archive checksum mismatch')
    # Separate extraction from the Lua/web helper: builds may run concurrently.
    cache = ROOT / '.cache/ode-dotnet'
    with tarfile.open(archive) as source:
        source.extractall(cache, filter='data')
    ode = cache / 'odedevs-ode-7bac210f051b'
    (ode / 'include/ode/config.h').write_text(CONFIG)
    makefile = (ode / 'Makefile').read_text()
    sources = []
    for start, end in [('ODE_SRC =', 'ifdef OPCODE_DIRECTORY'), ('ODE_PREGEN_SRC =', 'ifeq ($(WINDOWS),1)')]:
        text = makefile.split(start, 1)[1].split(end, 1)[0]
        sources.extend(str(ode / word) for word in text.split() if word.endswith(('.cpp', '.c')))
    output = output.resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    shared = '-dynamiclib' if sys.platform == 'darwin' else '-shared'
    subprocess.run([cxx, '-O2', '-w', '-DdNODEBUG', '-fPIC', '-fvisibility=hidden', shared,
                    '-I' + str(ode / 'include'), str(ROOT / 'native/mucade_physics.c'),
                    *sources, '-lm', '-o', str(output)], check=True)
    # Preserve the upstream licensing material next to redistributable output.
    for name in ('LICENSE.TXT', 'LICENSE-BSD.TXT', 'LICENSE-GPL.TXT'):
        source = ode / name
        if source.exists():
            (output.parent / ('ODE-' + name)).write_bytes(source.read_bytes())
    print(output)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True, help='Output libmucade_physics.so (or .dylib)')
    parser.add_argument('--cxx', default=os.environ.get('CXX', 'c++'))
    args = parser.parse_args()
    build(args.output, args.cxx)
