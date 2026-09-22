import argparse
from pathlib import Path
import struct

from compile_barrage import Compiler


parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
output = Path('build/parsec47')
output.mkdir(parents=True, exist_ok=True)
compiler = Compiler()
compiler.load(args.original)
if len(compiler.patterns) != 85:
    raise ValueError('Expected 85 PARSEC47 patterns')
(output / 'BarrageCode.cs').write_text(compiler.emit())
directories = ['morph', 'small', 'smallmove', 'smallsidemove', 'middle', 'middlesub',
               'middlemove', 'middlebackmove', 'large', 'largemove', 'morph_lock', 'small_lock', 'middlesub_lock']
lines = ['public class BarrageManager {', 'public const int BARRAGE_MAX = 64;']
lines += [f'public const int {name.upper()} = {i};' for i, name in enumerate(directories)]
lines += ['public int[][] parser = new int[13][];', 'public int[] parserNum = new int[13];',
          'public void loadBulletMLs() {']
for i, directory in enumerate(directories):
    indices = [str(n) for n, (name, _) in enumerate(compiler.patterns) if name.startswith(directory + '/')]
    lines += [f'parser[{i}] = new int[] {{' + ','.join(indices) + '};', f'parserNum[{i}] = {len(indices)};']
lines += ['}', 'public void unloadBulletMLs() {}', '}']
(output / 'BarrageManager.cs').write_text('\n'.join(lines) + '\n')
bitmap = (args.original / 'images/title.bmp').read_bytes()
offset = struct.unpack_from('<I', bitmap, 10)[0]
width, height = struct.unpack_from('<ii', bitmap, 18)
bits, compression = struct.unpack_from('<HI', bitmap, 28)
if bitmap[:2] != b'BM' or (width, height, bits, compression) != (128, 128, 24, 0):
    raise ValueError('Unexpected PARSEC47 title bitmap format')
spans = []
for y in range(height):
    row = bitmap[offset + (height - 1 - y) * width * 3:offset + (height - y) * width * 3]
    x = 0
    while x < width:
        color = row[x * 3:x * 3 + 3]
        end = x + 1
        while end < width and row[end * 3:end * 3 + 3] == color:
            end += 1
        if color != b'\0\0\0':
            spans.extend([x, y, end - x, *reversed(color)])
        x = end
lines = ['using static Drawing;', 'public static class TitleImage {',
         'static int[] spans = new int[] {' + ','.join(map(str, spans)) + '};',
         'public static void Draw() {', 'for (int i = 0; i < spans.Length; i += 6) {',
         'Color(spans[i+3]/255f, spans[i+4]/255f, spans[i+5]/255f, 1);',
         'P47Screen.drawBoxSolid(180+spans[i], 20+spans[i+1], spans[i+2], 1);', '}', '}', '}']
(output / 'TitleImage.cs').write_text('\n'.join(lines) + '\n')
print(f'Compiled {len(compiler.patterns)} PARSEC47 patterns and title image')
