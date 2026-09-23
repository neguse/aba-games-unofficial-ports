import argparse
import json
import math
from pathlib import Path
import struct
import xml.etree.ElementTree as ET

parser=argparse.ArgumentParser()
parser.add_argument('original',type=Path)
args=parser.parse_args()
out=Path('build/mazer-mayhem');out.mkdir(parents=True,exist_ok=True)
def f(value):return struct.unpack('<f',struct.pack('<f',value))[0]
letters=[]
for item in ET.parse(args.original/'Content/Xmls/Letters.xml').getroot().find('Asset'):
    points=[]
    for bar in item:
        x,y,w,h,d=map(lambda n:f(float(n)),bar.text.split(','))
        w=f(w*f(1.05));h=f(h*0.5);d=f(f(-d*f(math.pi))/180)
        ox1=f(math.cos(d)*w-math.sin(d)*h);oy1=f(math.sin(d)*w+math.cos(d)*h)
        ox2=f(math.cos(d)*-w-math.sin(d)*h);oy2=f(math.sin(d)*-w+math.cos(d)*h)
        points.extend(map(f,[x-ox1,y-oy1,x-ox2,y-oy2,x+ox1,y+oy1,x+ox2,y+oy2]))
    letters.append('new LetterData(new float[]{'+','.join(str(n)+'f' for n in points)+'})')
stages=['new List<string>{'+','.join(json.dumps(row.text) for row in item)+'}' for item in ET.parse(args.original/'Content/Xmls/Stages.xml').getroot().find('Asset')]
(out/'Data.cs').write_text('using System.Collections.Generic;\npublic static class MmData {\npublic static LetterData[] Letters(){return new LetterData[]{'+','.join(letters)+'};}\npublic static List<List<string>> Stages(){return new List<List<string>>{'+','.join(stages)+'};}\n}\n')
print(f'Compiled {len(letters)} Mazer Mayhem glyphs and {len(stages)} maze layouts')
