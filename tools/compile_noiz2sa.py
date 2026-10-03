import argparse
import ast
import math
from pathlib import Path
import re
import struct
from compile_barrage import Compiler
from compile_title import png

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
output = Path('build/noiz2sa')
output.mkdir(parents=True, exist_ok=True)
compiler = Compiler()
compiler.load(args.original)
if len(compiler.patterns) != 73:
    raise ValueError('Expected 73 Noiz2sa patterns')
(output / 'BarrageCode.cs').write_text(compiler.emit())
def c_array(path, name):
    text = path.read_text()
    text = re.sub(r'/\*.*?\*/', '', text, flags=re.S)
    text = re.sub(r'//[^\n]*', '', text)
    value = re.search(r'\b' + name + r'\[[^=]+?=\s*(\{.*?\});', text, re.S)[1]
    value = re.sub(r'(?<=\d)[fF]\b', '', value).replace('{', '[').replace('}', ']')
    value = re.sub(r"360\s*-\s*(\d+)", lambda m: str(360-int(m[1])), value)
    return ast.literal_eval(value)
def array(values, kind='int'):
    if isinstance(values[0], list):
        rank = 1
        value = values[0]
        while isinstance(value, list):
            rank += 1
            value = value[0]
        return 'new ' + kind + '[]' * rank + '{' + ','.join(array(v, kind) for v in values) + '}'
    return 'new ' + kind + '[]{' + ','.join(str(v) + ('f' if kind == 'float' else '') for v in values) + '}'
sine = [int(math.sin(i * (6.28 / 1024)) * 256) for i in range(1280)]
d = 0
tangent = []
for i in range(1024):
    while int(math.sin(d * (6.28 / 1024)) / math.cos(d * (6.28 / 1024)) * 1024) < i:
        d += 1
    tangent.append(d)
tangent += [128, 128]
color_file = args.original / 'src/clrtbl.c'
colors = [[int(c * 224 / 256) for c in row[:3]] for row in c_array(color_file, 'color')]
decay = c_array(color_file, 'colorDfs')
blend = c_array(color_file, 'colorAlp')
if len(colors) != 256 or len(decay) != 256 or len(blend) != 256 or any(len(row) != 256 for row in blend):
    raise ValueError('Unexpected Noiz2sa palette tables')
colors[0] = [100, 0, 0]
sprites = []
for name in 'noiz2sa':
    data = (args.original / 'images' / f'title_{name}.bmp').read_bytes()
    offset = struct.unpack_from('<I', data, 10)[0]
    width, height, planes, bits, compression = struct.unpack_from('<iiHHI', data, 18)
    if width != 40 or height != 40 or bits != 4 or compression:
        raise ValueError('Unexpected Noiz2sa title bitmap')
    palette = [tuple(data[54+i*4:54+i*4+3][::-1]) for i in range(16)]
    mapped = [min(range(256), key=lambda i: sum((a-b)**2 for a, b in zip(rgb, colors[i]))) for rgb in palette]
    for y in range(40):
        for x in range(40):
            byte = data[offset + (39-y)*20 + x//2]
            sprites.append(mapped[byte >> 4 if x % 2 == 0 else byte & 15])
colors[0] = [255, 255, 255]
def pack(r, g=0, b=0, a=255):
    return r | g << 8 | b << 16 | a << 24
pixels = [pack(*rgb) for rgb in colors] + [pack(n) for n in decay]
pixels += [pack(n) for row in blend for n in row]
for i in range(512):
    shift = int(sine[(i*8)&1023] / 128)
    pixels.append(pack(shift+2, shift+2))
sprite_pixels = [pack(*colors[i], 255 if i else 0) for i in sprites]
for name, width, values in [('tables', 256, pixels), ('sprites', 40, sprite_pixels)]:
    png(output / f'images/{name}.png', width, len(values) // width, [value >> shift & 255 for value in values for shift in (0, 8, 16, 24)])
patterns = [[i for i, (name, _) in enumerate(compiler.patterns) if name.startswith(directory + '/')] for directory in ['zako', 'middle', 'boss']]
source = 'using System.Collections.Generic;\npublic static class NrData {\n'
source += 'public static int[][] barrages=' + array(patterns) + ';\n'
source += 'public static int[] sine=' + array(sine) + ';\npublic static int[] tangent=' + array(tangent) + ';\n'
source += 'public static float[][][] letters=' + array(c_array(args.original / 'src/letterdata.h', 'spData'), 'float') + ';\n'
source += '}\n'
(output / 'Data.cs').write_text(source)
print('Compiled 73 Noiz2sa patterns, palette/blend tables and title sprites')
