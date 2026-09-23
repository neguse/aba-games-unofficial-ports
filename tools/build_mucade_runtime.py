import argparse
import hashlib
from pathlib import Path
import shutil
import subprocess
import tarfile
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--lub', type=Path, required=True)
parser.add_argument('--native-only', action='store_true')
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
bridge = Path('games/mu-cade/ode.cpp').resolve()
output = Path('build/mu-cade')
output.mkdir(parents=True, exist_ok=True)
runner = output / 'runner.cpp'
runner.write_text('''extern "C" {
#include <lua.h>
#include <lauxlib.h>
#include <lualib.h>
int luaopen_mcd_ode(lua_State*);
}
int main(int argc,char** argv){
 lua_State* L=luaL_newstate();luaL_openlibs(L);luaopen_mcd_ode(L);lua_setglobal(L,"mcdphysics");
 lua_newtable(L);for(int i=1;i<argc;i++){lua_pushstring(L,argv[i]);lua_rawseti(L,-2,i-1);}lua_setglobal(L,"arg");
 int result=luaL_dofile(L,argv[1]);if(result)fprintf(stderr,"%s\\n",lua_tostring(L,-1));lua_close(L);return result?1:0;
}
''')
subprocess.run(['c++', '-O2', '-w', '-DLUA_32BITS', '-DdNODEBUG',
                '-I' + str(ode / 'include'), '-I' + str(args.lub.resolve() / 'third_party/tcs/deps/lua'),
                str(runner), str(bridge), *sources, str(args.lub.resolve() / 'third_party/tcs/build/libliblua32.a'),
                '-lm', '-ldl', '-o', str(output / 'lua-ode')], check=True)
if args.native_only:
    raise SystemExit(0)
runtime = Path('.cache/mu-cade-lub')
shutil.copytree(args.lub, runtime, dirs_exist_ok=True,
                ignore=shutil.ignore_patterns('.git', 'build', 'third_party', 'node_modules', 'bin', 'obj'))
third_party = runtime / 'third_party'
if not third_party.exists():
    third_party.symlink_to(args.lub.resolve() / 'third_party', target_is_directory=True)
api = runtime / 'src/lua_api.c'
text = api.read_text()
text = text.replace('void lua_api_register(lua_State *L) {',
                    'extern int luaopen_mcd_ode(lua_State *L);\nvoid lua_api_register(lua_State *L) {\n  luaopen_mcd_ode(L);\n  lua_setglobal(L, "mcdphysics");')
api.write_text(text)
with (runtime / 'CMakeLists.txt').open('a') as cmake:
    cmake.write('\nadd_library(mcd_ode STATIC\n' + '\n'.join('"' + source + '"' for source in sources) + '\n)\n')
    cmake.write(f'target_include_directories(mcd_ode PUBLIC "{ode / "include"}")\n')
    cmake.write('target_compile_definitions(mcd_ode PRIVATE dNODEBUG)\n')
    cmake.write(f'target_sources(lub_objs PRIVATE "{bridge}")\n')
    cmake.write('target_link_libraries(lub_objs PUBLIC mcd_ode)\n')
subprocess.run(['emcmake', 'cmake', '--preset', 'wasm-release'], cwd=runtime, check=True)
subprocess.run(['cmake', '--build', 'build/wasm', '--target', 'lub', '--parallel', '8'], cwd=runtime, check=True)
