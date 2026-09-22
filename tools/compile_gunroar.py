import argparse
from pathlib import Path
import struct

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
bitmap = (args.original / 'images/title.bmp').read_bytes()
offset = struct.unpack_from('<I', bitmap, 10)[0]
width, height, planes, bits, compression = struct.unpack_from('<iiHHI', bitmap, 18)
if bitmap[:2] != b'BM' or (width, height, bits, compression) != (256, 64, 8, 0):
    raise ValueError('Unexpected Gunroar title bitmap format')
palette = [bitmap[54+i*4:57+i*4] for i in range(256)]
spans = []
for y in range(height):
    indices = bitmap[offset + (height - 1 - y) * width:offset + (height - y) * width]
    row = b"".join(palette[i] for i in indices)
    x = 0
    while x < width:
        color = row[x * 3:x * 3 + 3]
        end = x + 1
        while end < width and row[end * 3:end * 3 + 3] == color:
            end += 1
        if color != b'\0\0\0':
            spans.extend([x, y, end - x, *reversed(color)])
        x = end
output = Path('build/gunroar')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''using static Drawing;
public static class GunroarTitleImage {
static int[] spans = new int[] {''' + ','.join(map(str, spans)) + '''};
public static void draw() {
for (int i = 0; i < spans.Length; i += 6) {
Color(spans[i+3]/255f, spans[i+4]/255f, spans[i+5]/255f, 1);
float x = spans[i] * 255f / 256, y = -63 + spans[i+1] * 63f / 64;
float width = spans[i+2] * 255f / 256, height = 63f / 64;
glBegin(GL_QUADS);
glVertex2f(x, y); glVertex2f(x+width, y); glVertex2f(x+width, y+height); glVertex2f(x, y+height);
glEnd();
}
}
}
''')
