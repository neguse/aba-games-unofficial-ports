"""A lub C API that draws, plays and loads nothing, for running a game's C without a window.

Every function of lub_api.h succeeds (LUB_OK, false, 0) and fills its out arguments with zeros:
a resource is handle 0 and a file stays pending (LubIoStatus 0), as before the first frame that has it.
Bindings keep only their scale, so the generated glue's grow-and-retry path still works.
It counts the vertices drawn, which a test reads through tests/NullLub.cs.
"""
import re
from pathlib import Path

# Functions whose result the generated glue depends on.
special = {
    'lub_last_error': 'return "";',
    'lub_bindings_size': 'return sizeof(LubBindings);',
    'lub_bindings_init': 'b->scale = scale;',
    'lub_bindings_scale': 'return b->scale;',
    'lub_bindings_grow_into': 'dst->scale = src->scale;',
    'lub_gfx_draw': 'null_lub_drawn_vertices += count; return LUB_OK;',
}


def null_api(lub):
    """C source of the API; include it after game.c and binding.c."""
    text = re.sub(r'//[^\n]*', '', (Path(lub) / 'include/lub/lub_api.h').read_text(encoding='utf-8'))
    lines = ['struct LubBindings { int32_t scale; };', 'static int32_t null_lub_drawn_vertices;',
             'int32_t tcs_host_nulllub_drawn_vertices(void) { return null_lub_drawn_vertices; }']
    for declaration in re.findall(r'LUB_API\s+([^;#]*?\))\s*;', text):
        declaration = ' '.join(declaration.split())
        result, name, parameters = re.match(r'(.*?)\b(lubx?_[a-z0-9_]+)\s*\((.*)\)$', declaration).groups()
        result = result.strip()
        body = []
        if name in special:
            body.append(special[name])
        else:
            for parameter in parameters.split(','):
                parameter = parameter.strip()
                # A non-const pointer other than the context is an out argument.
                match = re.match(r'(?!const\b)(\w+)\s*\*\s*(\w+)$', parameter)
                if match and match.group(1) not in ['LubContext', 'void']:
                    body.append(f'if ({match.group(2)}) memset({match.group(2)}, 0, sizeof *{match.group(2)});')
            if result != 'void':
                body.append(f'{result} result; memset(&result, 0, sizeof result); return result;')
        unused = ''.join(f'(void){p.split("*")[-1].split()[-1]}; ' for p in parameters.split(',')
                         if p.strip() not in ['', 'void'])
        lines.append(f'{declaration} {{ {unused}{" ".join(body)} }}')
    return '\n'.join(lines) + '\n'
