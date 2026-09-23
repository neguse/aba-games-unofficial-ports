import argparse
import math
from pathlib import Path
import struct
import xml.etree.ElementTree as ET

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
def f(value):
    return struct.unpack('<f', struct.pack('<f', value))[0]
def cs(value):
    return repr(value) + 'f'
letters = []
for item in ET.parse(args.original / 'Content/Xmls/Letters.xml').getroot().find('Asset'):
    bars = []
    for bar in item:
        x, y, width, height, angle = map(lambda v: f(float(v)), bar.text.split(','))
        angle = f(f(-angle * f(math.pi)) / 180)
        z, w = f(math.sin(f(angle * .5))), f(math.cos(f(angle * .5)))
        bars.append('new BarData {Offset=new Vector3(' + cs(x) + ',' + cs(y) + ',0),Scale=' + cs(f(width / f(.65))) + ',Orientation=new Quaternion(0,0,' + cs(z) + ',' + cs(w) + ')}')
    letters.append('new LetterData(new BarData[]{' + ','.join(bars) + '})')
out = Path('build/gear-toy-gear')
out.mkdir(parents=True, exist_ok=True)
(out / 'Data.cs').write_text('public static class GtgData {public static LetterData[] Letters(){return new LetterData[]{' + ','.join(letters) + '};}}\n')
print(f'Compiled {len(letters)} GearToyGear glyphs')
