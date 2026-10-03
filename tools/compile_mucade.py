import argparse
import ast
import json
from pathlib import Path
import re
from compile_title import bitmap, image
from compile_barrage import Compiler

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
output = Path('build/mu-cade')
output.mkdir(parents=True, exist_ok=True)
source = (args.original / 'src/abagames/mcd/letter.d').read_text()
source = re.sub(r'//[^\n]*', '', source.split('spData =', 1)[1].rsplit(';', 1)[0])
def number(node):
    if isinstance(node, ast.List):
        return [number(item) for item in node.elts]
    if isinstance(node, ast.Constant) and isinstance(node.value, (int, float)):
        return node.value
    if isinstance(node, ast.UnaryOp) and isinstance(node.op, ast.USub):
        return -number(node.operand)
    if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Sub):
        return number(node.left) - number(node.right)
    raise ValueError('Unsupported glyph expression')
letters = number(ast.parse(re.sub(r'(?<=\d)f\b', '', source.strip()), mode='eval').body)
def emit(value, depth):
    if depth == 0:
        return str(value) + 'f'
    return 'new float' + '[]' * depth + '{' + ','.join(emit(v, depth - 1) for v in value) + '}'
pixels = bitmap(args.original / 'images/title.bmp', (320, 64, 24))
images, masks = [], []
for letter in range(5):
    panel = [pixels[y * 320 + letter * 64 + x] for y in range(64) for x in range(64)]
    mask = [(255, 255, 255, 0) if p[:3] == (0, 0, 0) else (0, 0, 0, 0) for p in panel]
    images.append(image(output, f'mu-cade-{letter}', panel, 64, 64, True))
    masks.append(image(output, f'mu-cade-mask-{letter}', mask, 64, 64, True))
(output / 'Data.cs').write_text('''public static class McdData {
public static float[][][] Letters(){return ''' + emit(letters, 3) + ''';}
public static DrawImage[] images = new DrawImage[]{''' + ','.join(images) + '''};
public static DrawImage[] masks = new DrawImage[]{''' + ','.join(masks) + '''};
}
''')
compiler = Compiler()
compiler.load(args.original / 'barrage')
if len(compiler.patterns) != 13:
    raise ValueError('Expected 13 Mu-cade patterns')
(output / 'BarrageCode.cs').write_text(compiler.emit())
lines = ['public static class BarrageManager {', 'public static void load() {}', 'public static void unload() {}', 'public static int getInstance(string directory,string file){string name=directory+"/"+file;']
for i, (name, _) in enumerate(compiler.patterns):
    lines.append(f'if(name=={json.dumps(name)})return {i};')
lines += ['return -1;}', '}']
(output / 'BarrageManager.cs').write_text('\n'.join(lines) + '\n')
print(f'Compiled {len(letters)} glyphs, five title images and {len(compiler.patterns)} Mu-cade patterns')
