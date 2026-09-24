import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (128, 64, 24))
output = Path('build/torus-trooper')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''public static class TtData {
public const int musicCount = 4;
public static DrawImage title = ''' + image('torus-trooper', pixels, 128, 64, False) + ''';
}
''')

from compile_barrage import Compiler
import json
compiler = Compiler()
compiler.load(args.original / 'barrage')
if len(compiler.patterns) != 28:
    raise ValueError('Expected 28 Torus Trooper patterns')
(output / 'BarrageCode.cs').write_text(compiler.emit())
lines = ['public static class BarrageManager {', 'public static void load() {}', 'public static void unload() {}', 'public static int getInstance(string directory, string file) {', 'string name = directory + "/" + file;']
for i, (name, _) in enumerate(compiler.patterns):
    lines.append(f'if (name == {json.dumps(name)}) return {i};')
lines += ['return -1;', '}', 'public static int[] getInstanceList(string directory) {']
for directory in sorted({name.split('/')[0] for name, _ in compiler.patterns}):
    indices = ','.join(str(i) for i, (name, _) in enumerate(compiler.patterns) if name.startswith(directory + '/'))
    lines.append(f'if (directory == {json.dumps(directory)}) return new int[] {{' + indices + '};')
lines += ['return new int[0];', '}', '}']
(output / 'BarrageManager.cs').write_text('\n'.join(lines) + '\n')
