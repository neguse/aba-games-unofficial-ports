import argparse
import ast
import json
from pathlib import Path
import re
import struct
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
bitmap = (args.original / 'images/title.bmp').read_bytes()
offset = struct.unpack_from('<I', bitmap, 10)[0]
width, height, planes, bits, compression = struct.unpack_from('<iiHHI', bitmap, 18)
if (width, height, bits, compression) != (320, 64, 24, 0):
    raise ValueError('Unexpected Mu-cade title bitmap')
spans = []
for letter in range(5):
    for y in range(height):
        x = 0
        while x < 64:
            start = offset + (height - 1 - y) * width * 3 + (letter * 64 + x) * 3
            color = bitmap[start:start + 3]
            end = x + 1
            while end < 64 and bitmap[start + (end - x) * 3:start + (end - x + 1) * 3] == color:
                end += 1
            if color != b'\0\0\0':
                spans.extend([letter, x, y, end - x, *reversed(color)])
            x = end
(output / 'Data.cs').write_text('''using static Drawing;
public static class McdData {
public static float[][][] Letters(){return ''' + emit(letters, 3) + ''';}
static int[] spans=new int[]{''' + ','.join(map(str, spans)) + '''};
public static void DrawGlyph(int letter,float cx,float cy,float size,bool mask){
glBlendFunc(GL_SRC_ALPHA,mask?GL_ONE_MINUS_SRC_ALPHA:GL_ONE);
for(int i=0;i<spans.Length;i+=7){if(spans[i]!=letter)continue;
Color(mask?0:spans[i+4]/255f*Screen.red,mask?0:spans[i+5]/255f*Screen.green,mask?0:spans[i+6]/255f*Screen.blue,1);
float x=cx-size/2+spans[i+1]*size/64,y=cy-size/2+spans[i+2]*size/64,w=spans[i+3]*size/64,h=size/64;
glBegin(GL_QUADS);glVertex2f(x,y);glVertex2f(x+w,y);glVertex2f(x+w,y+h);glVertex2f(x,y+h);glEnd();
}
glBlendFunc(GL_SRC_ALPHA,GL_ONE);
}
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
