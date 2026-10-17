import argparse
import hashlib
from pathlib import Path
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
int luaopen_tcs_math(lua_State*);
int luaopen_tcs_rt(lua_State*);
}
int main(int argc,char** argv){
 lua_State* L=luaL_newstate();luaL_openlibs(L);luaopen_mcd_ode(L);lua_setglobal(L,"mcdphysics");
 luaL_requiref(L,"tcs_math",luaopen_tcs_math,1);lua_pop(L,1);
 luaL_requiref(L,"tcs_rt",luaopen_tcs_rt,1);lua_pop(L,1);
 lua_newtable(L);for(int i=1;i<argc;i++){lua_pushstring(L,argv[i]);lua_rawseti(L,-2,i-1);}lua_setglobal(L,"arg");
 int result=luaL_dofile(L,argv[1]);if(result)fprintf(stderr,"%s\\n",lua_tostring(L,-1));lua_close(L);return result?1:0;
}
''')
# tcs の Lua 出力は Math の超越関数で tcs_math、float の文字列化と string の順序で tcs_rt を呼ぶ
# (どちらも tcs の runtime/lua、Lua の lua32 と同じ build)
tcs = args.lub.resolve() / 'third_party/tcs'
runtime = []
for source in ['runtime/lua/tcs_math_lua.c', 'runtime/lua/tcs_rt_lua.c', 'runtime/fmt/f2s.c']:
    runtime.append(output / (Path(source).stem + '.o'))
    subprocess.run(['cc', '-O2', '-DLUA_32BITS', '-I' + str(tcs / 'deps/lua'), '-c',
                    str(tcs / source), '-o', str(runtime[-1])], check=True)
subprocess.run(['c++', '-O2', '-w', '-DLUA_32BITS', '-DdNODEBUG',
                '-I' + str(ode / 'include'), '-I' + str(tcs / 'deps/lua'),
                str(runner), str(bridge), *sources, *map(str, runtime),
                str(tcs / 'build/libtcs_math.a'), str(tcs / 'build/libliblua32.a'),
                '-lm', '-ldl', '-o', str(output / 'lua-ode')], check=True)
if args.native_only:
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
