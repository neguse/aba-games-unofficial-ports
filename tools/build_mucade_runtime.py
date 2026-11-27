import argparse
import hashlib
from pathlib import Path
import subprocess
import tarfile
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--native', action='store_true', help='build ODE as a native library instead of the web CMake file')
args = parser.parse_args()
archive = Path('.cache/ode-0.5.0.tar.gz')
archive.parent.mkdir(exist_ok=True)
revision = '7bac210f051b3ffcfaf9a168db3d7c302f7a49a4'
if not archive.exists():
    urllib.request.urlretrieve(f'https://bitbucket.org/odedevs/ode/get/{revision}.tar.gz', archive)
if hashlib.sha256(archive.read_bytes()).hexdigest() != '538f050c5882d6b03afa5d54dd894eb7df62df4d635ef459ce2f02de059cd4c9':
    raise ValueError('ODE archive checksum mismatch')
with tarfile.open(archive) as source:
    source.extractall('.cache/ode', filter='data')
ode = Path('.cache/ode/odedevs-ode-7bac210f051b').resolve()
(ode / 'include/ode/config.h').write_text('''#ifndef _ODE_CONFIG_H_
#define _ODE_CONFIG_H_
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>
#include <stdarg.h>
#include <malloc.h>
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
''')
makefile = (ode / 'Makefile').read_text()
names = []
for start, end in [('ODE_SRC =', 'ifdef OPCODE_DIRECTORY'), ('ODE_PREGEN_SRC =', 'ifeq ($(WINDOWS),1)')]:
    text = makefile.split(start, 1)[1].split(end, 1)[0]
    names += [word for word in text.split() if word.endswith(('.cpp', '.c'))]
sources = [str(ode / name) for name in names]
output = Path('build/mu-cade')
output.mkdir(parents=True, exist_ok=True)
if args.native:
    # ODE as a native library, which the game's C calls through games/mu-cade/ode_host.c (tests/check_game.py).
    native = output / 'native'
    native.mkdir(exist_ok=True)
    objects = []
    for index, source in enumerate(sources):
        objects.append(native / f'{index}-{Path(source).stem}.o')
        subprocess.run(['c++', '-O2', '-w', '-DdNODEBUG', '-I' + str(ode / 'include'), '-c', source,
                        '-o', str(objects[-1])], check=True)
    (native / 'libode.a').unlink(missing_ok=True)
    subprocess.run(['ar', 'rcs', str(native / 'libode.a'), *map(str, objects)], check=True)
    raise SystemExit(0)
# Included by lub's project(): the deferred call runs once lub's own targets exist.
(output / 'ode.cmake').write_text('function(mcd_ode)\nadd_library(mcd_ode STATIC\n' + '\n'.join('"' + source + '"' for source in sources) + f'''
)
target_include_directories(mcd_ode PUBLIC "{ode / "include"}")
target_compile_definitions(mcd_ode PRIVATE dNODEBUG)
target_link_libraries(lub PRIVATE mcd_ode)
endfunction()
cmake_language(DEFER CALL mcd_ode)
''')
