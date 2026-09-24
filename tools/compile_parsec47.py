import argparse
from pathlib import Path
from compile_title import bitmap, image

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
pixels = bitmap(args.original / 'images/title.bmp', (128, 128, 24))
(output / 'TitleImage.cs').write_text('''public static class TitleImage {
public static DrawImage title = ''' + image('parsec47', pixels, 128, 128, False) + ''';
}
''')
print(f'Compiled {len(compiler.patterns)} PARSEC47 patterns and title image')
