import argparse
import json
from pathlib import Path


class Tokens:
    def __init__(self, path):
        self.values = [v.strip() for line in path.read_text().splitlines() for v in line.split(',') if v.strip()]
        self.index = 0

    def take(self):
        value = self.values[self.index]
        self.index += 1
        return value

    def __bool__(self):
        return self.index < len(self.values)


def number(value):
    return str(float(value)) + 'f'


def array(kind, values):
    return f'new {kind}[] {{ ' + ', '.join(values) + ' }'


def obj(kind, **fields):
    return f'new {kind} {{ ' + ', '.join(f'{key} = {value}' for key, value in fields.items()) + ' }'


class Compiler:
    def __init__(self, source):
        self.source = source
        self.methods = []
        self.names = {}

    def files(self, directory, suffix):
        paths = sorted((self.source / directory).rglob('*.' + suffix))
        self.names[directory] = {p.relative_to(self.source / directory).as_posix(): i for i, p in enumerate(paths)}
        return paths

    def reference(self, kind, name):
        return f'{kind}[{self.names[kind][name]}]'

    def method(self, kind, index, expression):
        name = {'tumiki': 'TumikiSet', 'enemy': 'EnemySpec', 'stage': 'StagePattern', 'field': 'FieldPattern'}[kind]
        self.methods.append(f'static {name} Build_{kind}_{index}() {{ return {expression}; }}')

    def tumiki(self, path):
        t = Tokens(path)
        ratio = number(t.take())
        score, fire, interval = (t.take() for _ in range(3))
        shapes = []
        while t:
            shape = dict(zip(['s', 'ul', 'ur', 'dr', 'dl', 'u', 'r', 'd', 'l', 'pu', 'pdr', 'pr', 'pur', 'pd', 'pf'], range(15))).get(t.take(), 0)
            color = dict(zip(['r', 'g', 'b', 'y', 'p', 'a', 'w', 'gr'], range(8))).get(t.take(), 0)
            coords = [number(t.take()) for _ in range(4)]
            barrages = []
            while (value := t.take()) != 'e':
                if value == 's':
                    barrages.append('new Barrage()')
                    continue
                bullet_shape = ['b', 'a', 'r'].index(value)
                bullet_color = {'r': 0, 'a': 1, 'p': 2}.get(t.take(), 0)
                size, reverse = number(t.take()), number(t.take())
                before, after = t.take(), t.take()
                patterns, ranks, speeds = [], [], []
                while (value := t.take()) != 'e':
                    if not (self.source / 'barrage' / value).is_file():
                        raise ValueError(f'Missing barrage: {value}')
                    patterns.append(f'BarrageCode.Find({json.dumps(value)})')
                    ranks.append(number(t.take()))
                    speeds.append(number(t.take()) + ' * 1.2f')
                barrages.append(obj('Barrage', shape=bullet_shape, color=bullet_color, size=size, yReverse=reverse,
                                    prevWait=before, postWait=after, parser=array('int', patterns),
                                    rank=array('float', ranks), speed=array('float', speeds)))
            shapes.append(f'new Tumiki({shape}, {color}, ' + ', '.join(coords + [ratio]) + ') { barrage = ' + array('Barrage', barrages) + ' }')
        return f'new TumikiSet({score}, {fire}, {interval}, {array("Tumiki", shapes)})'

    def enemy(self, path):
        t = Tokens(path)
        main = self.reference('tumiki', t.take())
        forms = []
        offset = 0
        shield = None
        while (value := t.take()) != 'e':
            if shield is None:
                shield = number(value)
            attack, rest = [], []
            while (period := t.take()) != 'e':
                attack.append(period)
                rest.append(t.take())
            forms.append(obj('AttackForm', shield=number(value), barragePtnStartIdx=offset,
                             attackPeriod=array('int', attack), breakPeriod=array('int', rest)))
            offset += len(attack)
        parts = [obj('EnemyPartSpec', tumikiSet=main, ofs='new Vector()', shield=shield,
                     destroyedFormIdx=99999, damageToMainBody='0f')]
        while t:
            part = self.reference('tumiki', t.take())
            x, y, armor = (number(t.take()) for _ in range(3))
            destroy, damage = t.take(), number(t.take())
            parts.append(obj('EnemyPartSpec', tumikiSet=part, ofs=f'new Vector({x}, {y})', shield=armor,
                             destroyedFormIdx=destroy, damageToMainBody=damage))
        return f'new EnemySpec({array("EnemyPartSpec", parts)}, {array("AttackForm", forms)})'

    def stage(self, path):
        t = Tokens(path)
        seed, warning = t.take(), t.take()
        appearances = []
        while t:
            start, duration, interval = (t.take() for _ in range(3))
            side = ['f', 'u'].index(t.take())
            position, width = number(t.take()), number(t.take())
            wait = 'true' if t.take() == 'y' else 'false'
            enemy = self.reference('enemy', t.take())
            movement = t.take()
            direction = '4.712389f' if side == 0 else '3.1415927f'
            if movement == 'p':
                withdraw = t.take()
                routes = []
                index = '-1'
                while True:
                    speed = number(t.take())
                    points = []
                    while (x := t.take()) != 'e':
                        points.append(f'new Vector({number(x)}, {number(t.take())})')
                    routes.append(obj('MoveRoute', index=index, speed=speed, point=array('Vector', points)))
                    index = t.take()
                    if index == 'e':
                        break
                move = obj('PointsMovePattern', deg=direction, withdrawCnt=withdraw, routes=array('MoveRoute', routes))
            else:
                if not (self.source / 'barrage' / movement).is_file():
                    raise ValueError(f'Missing movement: {movement}')
                move = obj('BulletMLMovePattern', deg=direction, parser=f'BarrageCode.Find({json.dumps(movement)})', speed=number(t.take()))
            appearances.append(obj('EnemyAppearancePattern', startTime=start, duration=duration, interval=interval,
                                   posType=side, pos=position, width=width, waitTillEnemiesDestroyed=wait, spec=enemy, move=move))
        return obj('StagePattern', randSeed=seed, warningCnt=warning, pattern=array('EnemyAppearancePattern', appearances))

    def field(self, path):
        t = Tokens(path)
        seed, speed = t.take(), number(t.take())
        colors = dict(zip(['br', 'bg', 'bb', 'gr', 'gg', 'gb', 'mtr', 'mtg', 'mtb'], [number(t.take()) for _ in range(9)]))
        lines = []
        while t:
            z = float(t.take())
            intervals, shapes = [], []
            while (value := t.take()) != 'e':
                intervals.append(value)
            while (value := t.take()) != 'e':
                shapes.append(self.reference('tumiki', value))
            lines.append(obj('FieldLinePattern', z=number(-abs(z)), onGround='true' if z > 0 else 'false',
                             interval=array('int', intervals), tumikiSet=array('TumikiSet', shapes)))
        return obj('FieldPattern', randSeed=seed, scrollSpeed=speed, **colors, line=array('FieldLinePattern', lines))

    def compile(self):
        init, fields, lookup = [], [], []
        for kind, suffix, model in [('tumiki', 'tmk', 'TumikiSet'), ('enemy', 'enm', 'EnemySpec'), ('stage', 'stg', 'StagePattern'), ('field', 'fld', 'FieldPattern')]:
            paths = self.files(kind, suffix)
            fields.append(f'public static {model}[] {kind};')
            init.append(f'{kind} = new {model}[{len(paths)}];')
            for i, path in enumerate(paths):
                self.method(kind, i, getattr(self, kind)(path))
                init.append(f'{kind}[{i}] = Build_{kind}_{i}();')
            if kind in ('tumiki', 'enemy'):
                lookup.append(f'public static {model} Find_{kind}(string name) {{ switch(name) {{')
                for name, index in self.names[kind].items():
                    lookup.append(f'case {json.dumps(name)}: return {kind}[{index}];')
                lookup.append('default: return null; } }')
        return '\n'.join(['// Copyright 2004 Kenta Cho. All rights reserved.', 'public static class GameData', '{', *fields,
                          'public static void Initialize() {', *init, '}', *lookup, *self.methods, '}', ''])


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('input', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    compiler = Compiler(args.input)
    source = compiler.compile()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(source)
    print(', '.join(f'{len(names)} {kind}' for kind, names in compiler.names.items()))
