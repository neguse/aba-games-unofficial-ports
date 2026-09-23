import argparse
from pathlib import Path
import math
import struct
from compile_barrage import Compiler

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
output = Path('build/rrootage')
output.mkdir(parents=True, exist_ok=True)
compiler = Compiler()
compiler.load(args.original)
if len(compiler.patterns) != 68:
    raise ValueError('Expected 68 rRootage patterns')
(output / 'BarrageCode.cs').write_text(compiler.emit())
patterns = []
for directory in ['normal', 'reversible', 'morph', 'simple', 'morph_heavy', 'psy']:
    indices = [str(i) for i, (name, _) in enumerate(compiler.patterns) if name.startswith(directory + '/')]
    patterns.append('new int[] {' + ','.join(indices) + '}')
sine = [int(math.sin(i * (6.28 / 1024)) * 256) for i in range(1280)]
d = 0
tangent = []
for i in range(1024):
    while int(math.sin(d * (6.28 / 1024)) / math.cos(d * (6.28 / 1024)) * 1024) < i:
        d += 1
    tangent.append(d)
tangent += [128, 128]
images = []
for name in ['star', 'smoke', 'title']:
    data = (args.original / 'images' / (name + '.bmp')).read_bytes()
    offset = struct.unpack_from('<I', data, 10)[0]
    width, height, planes, bits, compression = struct.unpack_from('<iiHHI', data, 18)
    if data[:2] != b'BM' or bits != 24 or compression or width < 1 or height < 1:
        raise ValueError('Unexpected rRootage bitmap format')
    stride = (width * 3 + 3) & ~3
    pixels = []
    for y in range(height):
        row = data[offset + (height - 1 - y) * stride:offset + (height - y) * stride]
        pixels.append([row[x * 3] | row[x * 3 + 1] << 8 | row[x * 3 + 2] << 16 for x in range(width)])
    images.append((width, height, pixels))
atlas_width = max(w for w, _, _ in images) + 2
atlas_height = sum(h + 2 for _, h, _ in images)
pixels = []
ys = []
y = 0
for width, height, rows in images:
    ys.append(y)
    for j in range(-1, height + 1):
        row = rows[max(0, min(height - 1, j))]
        pixels.extend(row[max(0, min(width - 1, i))] for i in range(-1, atlas_width - 1))
    y += height + 2
runs = []
for color in pixels:
    if runs and runs[-1] == color:
        runs[-2] += 1
    else:
        runs.extend([1, color])
def array(values):
    return 'new int[] {' + ','.join(map(str, values)) + '}'
source = 'using System.Collections.Generic;\npublic static class RrData {\n'
source += 'public static int[][] barrages = new int[][] {' + ','.join(patterns) + '};\n'
source += 'public static int[] sine=' + array(sine) + ', tangent=' + array(tangent) + ';\n'
source += f'public const int atlasWidth={atlas_width},atlasHeight={atlas_height};\n'
source += 'public static int[] textureWidth=' + array([w for w, _, _ in images]) + ',textureHeight=' + array([h for _, h, _ in images]) + ',textureY=' + array(ys) + ';\n'
source += 'static int[] runs=' + array(runs) + ';\n'
source += '''public static List<int> pixels() {
var bytes=new List<int>();
for(int i=0;i<runs.Length;i+=2)for(int j=0;j<runs[i];j++){
int rgb=runs[i+1];bytes.Add(rgb&255);bytes.Add((rgb>>8)&255);bytes.Add((rgb>>16)&255);bytes.Add(255);
}
return bytes;
}
}
'''
(output / 'Data.cs').write_text(source)
print('Compiled 68 rRootage patterns, lookup tables and texture atlas')
