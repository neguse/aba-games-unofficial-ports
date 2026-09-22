import argparse
from pathlib import Path
import struct

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
bitmap = (args.original / 'images/title.bmp').read_bytes()
offset = struct.unpack_from('<I', bitmap, 10)[0]
width, height, planes, bits, compression = struct.unpack_from('<iiHHI', bitmap, 18)
if bitmap[:2] != b'BM' or (width, height, bits, compression) != (64, 64, 24, 0):
    raise ValueError('Unexpected A7x title bitmap format')
spans = []
stride = (width * 3 + 3) & ~3
for y in range(height):
    row = bitmap[offset + (height - 1 - y) * stride:offset + (height - 1 - y) * stride + width * 3]
    x = 0
    while x < width:
        color = row[x * 3:x * 3 + 3]
        end = x + 1
        while end < width and row[end * 3:end * 3 + 3] == color:
            end += 1
        if color != b'\0\0\0':
            spans.extend([x, y, end - x, *reversed(color)])
        x = end
output = Path('build/a7xpg')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''using static Drawing;
public static class A7xData {
static int[] spans = new int[] {''' + ','.join(map(str, spans)) + '''};
public static void drawTitle() {
for (int i = 0; i < spans.Length; i += 6) {
Color(spans[i+3]/255f, spans[i+4]/255f, spans[i+5]/255f, 1);
float x = 80 + spans[i] * 100f / 64, y = 50 + spans[i+1] * 100f / 64;
float width = spans[i+2] * 100f / 64, height = 100f / 64;
glBegin(GL_QUADS);
glVertex2f(x, y); glVertex2f(x+width, y); glVertex2f(x+width, y+height); glVertex2f(x, y+height);
glEnd();
}
}
}
''')
