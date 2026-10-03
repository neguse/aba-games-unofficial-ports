import argparse
from pathlib import Path
import re
import struct
import zlib
from compile_title import png as write_png

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
output = Path('build/wok')
output.mkdir(parents=True, exist_ok=True)

def png(path):
    data = path.read_bytes()
    if data[:8] != b'\x89PNG\r\n\x1a\n':
        raise ValueError('Expected PNG image')
    offset, compressed = 8, b''
    while offset < len(data):
        length = struct.unpack_from('>I', data, offset)[0]
        kind, chunk = data[offset+4:offset+8], data[offset+8:offset+8+length]
        if kind == b'IHDR':
            width, height, depth, color, _, _, interlace = struct.unpack('>IIBBBBB', chunk)
            if (depth, color, interlace) != (8, 2, 0):
                raise ValueError('Expected non-interlaced RGB8 Wok sprite')
        if kind == b'IDAT':
            compressed += chunk
        offset += length + 12
    raw = zlib.decompress(compressed)
    stride = width * 3
    previous = [0] * stride
    rows = []
    for y in range(height):
        mode = raw[y*(stride+1)]
        row = list(raw[y*(stride+1)+1:(y+1)*(stride+1)])
        for x in range(stride):
            a, b, c = row[x-3] if x >= 3 else 0, previous[x], previous[x-3] if x >= 3 else 0
            p = a + b - c
            pa, pb, pc = abs(p-a), abs(p-b), abs(p-c)
            if mode > 4:
                raise ValueError('Invalid PNG row filter')
            predictor = [0, a, b, (a+b)//2, a if pa <= pb and pa <= pc else b if pb <= pc else c][mode]
            row[x] = (row[x] + predictor) & 255
        rows.append([tuple(row[x:x+3]) for x in range(0,stride,3)])
        previous = row
    return width, height, rows

text = (args.original / 'screen.c').read_text()
names = re.findall(r'"([^"]+\.png)"', text)
if len(names) != 48:
    raise ValueError('Expected 48 Wok sprites')
base = [(0,0,0),(255,0,0),(0,255,0),(0,0,255),(255,255,0),(0,255,255),(255,0,255),(255,127,0),(255,0,127),(127,255,0),(0,255,127),(127,0,255),(0,127,255)]
palette = [tuple(255-((255-c)//16)*j for c in color) for color in base for j in range(16)] + [(0,0,0)]*48
images, widths, heights, ys = [], [], [], []
y = 0
for index, name in enumerate(names):
    width, height, rows = png(args.original / 'images' / name)
    palette[0] = (255,0,0) if index < 3 else (0,0,255)
    mapped = {}
    def pixel(rgb):
        if rgb not in mapped:
            match = min(range(256), key=lambda i: sum((a-b)**2 for a,b in zip(rgb,palette[i])))
            mapped[rgb] = 0 if match == 0 else palette[match][0] | palette[match][1]<<8 | palette[match][2]<<16 | 255<<24
        return mapped[rgb]
    images.append([[pixel(rgb) for rgb in row] for row in rows])
    widths.append(width); heights.append(height); ys.append(y); y += height
atlas_width, atlas_height = max(widths), y
pixels = [color for width, rows in zip(widths,images) for row in rows for color in row+[0]*(atlas_width-width)]
write_png(output/'images/atlas.png', atlas_width, atlas_height, [color>>shift&255 for color in pixels for shift in (0,8,16,24)])
zone = min(palette[1:], key=lambda rgb: sum((a-b)**2 for a,b in zip(rgb,(240,240,128))))
def duration(path):
    data = path.read_bytes()
    rate = struct.unpack_from('<I', data, data.index(b'\x01vorbis')+12)[0]
    offset, granule = 0, 0
    while offset < len(data):
        if data[offset:offset+4] != b'OggS':
            raise ValueError('Invalid Ogg page')
        position = struct.unpack_from('<Q',data,offset+6)[0]
        if position != 2**64-1:
            granule = position
        count = data[offset+26]
        offset += 27 + count + sum(data[offset+27:offset+27+count])
    return granule / rate

def array(values):
    return 'new int[]{'+','.join(map(str,values))+'}'
source = 'using System.Collections.Generic;\npublic static class WkData {\n'
source += f'public const int atlasWidth={atlas_width},atlasHeight={atlas_height};\n'
for name, values in [('widths',widths),('heights',heights),('ys',ys),('zoneColor',zone)]:
    source += f'public static int[] {name}='+array(values)+';\n'
source += 'public static float[] musicDuration=new float[]{'+','.join(str(duration(args.original/'sounds'/f'wok{i}.ogg'))+'f' for i in [1,2])+'};\n'
source += '}\n'
(output/'Data.cs').write_text(source)
print(f'Compiled 48 Wok sprites into {atlas_width}x{atlas_height} atlas')
