import math
from pathlib import Path
import struct
import sys

source = Path(sys.argv[1])
target = Path('build/masashikun-hi')
target.mkdir(parents=True, exist_ok=True)
lines = ['public static class MasData { public static void Load(){']
for filename, field, count, slots in [('dlch.ldt', 'MasMain.obd', 32, 14), ('man.ldt', 'MasMain.md', 64, 32), ('hira.ldt', 'MasHira.hd', 32, 61)]:
    data = list(struct.iter_unpack('<ii', (source / filename).read_bytes()))
    shapes = []
    for i in range(slots):
        points = data[i*count:(i+1)*count] or [(-32768, 0)]
        end = next(j for j, point in enumerate(points) if point[0] == -32768)
        points = points[:end+1]
        shapes.append('new MasShape(new MasPoint[]{' + ','.join(f'new MasPoint({x},{y})' for x, y in points) + '})')
    lines.append(field + '=new MasShape[]{' + ','.join(shapes) + '};')
for name, function in [('dsin', math.sin), ('dcos', math.cos)]:
    values = [round(function(r * 0.006136) * 256) for r in range(1024)]
    for r in [0, 256, 512, 768]:
        values[r] = round(function(r * math.pi / 512) * 256)
    lines.append(f'MasMain.{name}=new int[]{{' + ','.join(map(str, values)) + '};')
d = 0
values = []
for r in range(256):
    while round(math.sin(d * 0.006136) / math.cos(d * 0.006136) * 256) < r:
        d += 1
    values.append(d)
lines.append('MasMain.tantab=new int[]{' + ','.join(map(str, values)) + '};')
lines.append('}}')
(target / 'MasData.cs').write_text('\n'.join(lines))
