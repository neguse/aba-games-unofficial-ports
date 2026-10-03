import argparse
import ast
import json
import re
import struct
import xml.etree.ElementTree as ET
from pathlib import Path


def expression(text, parameters, precise=False):
    text = re.sub(r"\$(\w+)", r"var_\1", text.strip())

    def emit(node):
        if isinstance(node, ast.Constant) and type(node.value) in (int, float):
            if precise:
                hi = struct.unpack('f', struct.pack('f', node.value))[0]
                lo = node.value - hi
                return f"new PatternNumber({hi!r}f, {lo!r}f)"
            return str(node.value) + "f"
        if isinstance(node, ast.Name):
            if node.id == "var_rank":
                return "new PatternNumber(b.Rank, 0)" if precise else "b.Rank"
            if node.id == "var_rand":
                return "new PatternNumber(w.RandomValue(), 0)" if precise else "w.RandomValue()"
            if node.id.startswith("var_") and node.id[4:].isdigit():
                value = parameters[int(node.id[4:]) - 1]
                return f"new PatternNumber({value}, 0)" if precise else value
        if isinstance(node, ast.UnaryOp) and isinstance(node.op, (ast.USub, ast.UAdd)):
            if precise:
                value = emit(node.operand)
                return f"PatternNumber.Negate({value})" if isinstance(node.op, ast.USub) else value
            return "(" + ("-" if isinstance(node.op, ast.USub) else "+") + emit(node.operand) + ")"
        if isinstance(node, ast.BinOp):
            operators = {ast.Add: "+", ast.Sub: "-", ast.Mult: "*", ast.Div: "/"}
            if type(node.op) in operators:
                if precise:
                    methods = {ast.Add: "Add", ast.Sub: "Subtract", ast.Mult: "Multiply", ast.Div: "Divide"}
                    return f"PatternNumber.{methods[type(node.op)]}({emit(node.left)}, {emit(node.right)})"
                return f"({emit(node.left)} {operators[type(node.op)]} {emit(node.right)})"
        raise ValueError(f"Unsupported expression: {text}")

    return emit(ast.parse(text, mode="eval").body)


class Program:
    def __init__(self, compiler, actions):
        self.compiler = compiler
        self.states = []
        self.slots = 0
        self.loops = 0
        self.actions = actions

    def state(self, code, gated=True):
        self.states.append((code, gated))
        return len(self.states) - 1

    def slot(self):
        index = self.slots
        self.slots += 1
        return f"s.Values[{index}]"

    def expr(self, node, env):
        return expression(node.text, env)

    def integer(self, node, env):
        return f"PatternNumber.Integer({expression(node.text, env, precise=True)})"

    def reference(self, node, env, chain):
        key = (node.tag[:-3], node.attrib["label"])
        if key in chain:
            raise ValueError(f"Recursive reference: {key}")
        target = self.compiler.labels[key]
        values = [self.expr(p, env) for p in node if p.tag == "param"]
        new_env = [self.slot() for _ in values]
        self.state([f"{dst} = {src};" for dst, src in zip(new_env, values)])
        return target, new_env, chain + [key]

    def direction(self, node, env, method):
        modes = {"aim": 0, "absolute": 1, "relative": 2, "sequence": 3}
        return f"s.{method}(b, {self.expr(node, env)}, {modes[node.get('type', 'aim')]});"

    def speed(self, node, env, method):
        modes = {"absolute": 1, "relative": 2, "sequence": 3}
        return f"s.{method}(b, {self.expr(node, env)}, {modes[node.get('type', 'absolute')]});"

    def compile_node(self, node, env, chain):
        tag = node.tag
        if tag in ("actionRef", "fireRef"):
            target, new_env, chain = self.reference(node, env, chain)
            self.compile_node(target, new_env, chain)
        elif tag == "action":
            for child in node:
                self.compile_node(child, env, chain)
        elif tag == "repeat":
            loop = self.loops
            self.loops += 1
            self.state([f"s.Limits[{loop}] = {self.integer(node.find('times'), env)};", f"s.Counts[{loop}] = 0;"])
            start = len(self.states)
            child = next(e for e in node if e.tag in ("action", "actionRef"))
            self.compile_node(child, env, chain)
            end = len(self.states)
            self.state([f"s.Counts[{loop}]++;", f"s.Pc = s.Counts[{loop}] < s.Limits[{loop}] ? {start} : {end + 1};", "continue;"], gated=False)
        elif tag == "wait":
            self.state([f"s.Wake += Math.Max(0, {self.integer(node, env)});"])
        elif tag == "vanish":
            self.state(["w.Vanish(b);"])
        elif tag == "fire":
            self.state(["s.BeginShot();"])
            for kind, method in (("speed", "ShotSpeed"), ("direction", "ShotDirection")):
                value = node.find(kind)
                if value is not None:
                    self.state([getattr(self, kind)(value, env, method)])
            bullet = next(e for e in node if e.tag in ("bullet", "bulletRef"))
            bullet_env = env
            if bullet.tag == "bulletRef":
                bullet, bullet_env, chain = self.reference(bullet, env, chain)
            for kind, method in (("speed", "ShotSpeed"), ("direction", "ShotDirection")):
                value = bullet.find(kind)
                if value is not None:
                    self.state([getattr(self, kind)(value, bullet_env, method)])
            actions = [e for e in bullet if e.tag == "action"] + [e for e in bullet if e.tag == "actionRef"]
            child = self.compiler.program(actions) if actions else -1
            args = "new float[] { " + ", ".join(bullet_env) + " }" if actions and bullet_env else f"new float[{self.compiler.parameter_count}]"
            self.state([f"s.Fire(w, b, {child}, {args});"])
        elif tag in ("changeDirection", "changeSpeed", "accel"):
            self.state([f"s.Term = {self.integer(node.find('term'), env)};"])
            if tag == "changeDirection":
                self.state([self.direction(node.find("direction"), env, "ChangeDirection")])
            elif tag == "changeSpeed":
                self.state([self.speed(node.find("speed"), env, "ChangeSpeed")])
            else:
                for axis, kind in enumerate(("horizontal", "vertical")):
                    value = node.find(kind)
                    if value is not None:
                        mode = {"absolute": 1, "relative": 2, "sequence": 3}[value.get("type", "absolute")]
                        self.state([f"s.Accelerate(b, {axis}, {self.expr(value, env)}, {mode});"])
        else:
            raise ValueError(f"Unsupported command {tag}")

    def compile(self):
        for i, action in enumerate(self.actions):
            if i:
                self.state([f"s.Pc = {len(self.states) + 1};", "s.Resume = w.Turn + 1;", "return;"], gated=False)
            self.compile_node(action, [f"s.Args[{i}]" for i in range(self.compiler.parameter_count)], [])
        self.state(["s.Pc = -1;", "return;"], gated=False)

    def emit(self, index):
        lines = [f"static void Run{index}(PatternState s, PatternBody b, PatternWorld w)", "{", "while (true)", "{", "switch (s.Pc)", "{"]
        for pc, (code, gated) in enumerate(self.states):
            lines.append(f"case {pc}:")
            if gated:
                lines.append("if (s.Wake > w.Turn) return;")
            lines += code
            if not code or code[-1] not in ("return;", "continue;"):
                lines += [f"s.Pc = {pc + 1};", "break;"]
        return "\n".join(lines + ["default: return;", "}", "}", "}"])


class Compiler:
    def __init__(self):
        self.programs = []
        self.labels = {}
        self.registered = {}
        self.patterns = []
        self.parameter_count = 0

    def program(self, actions):
        key = tuple(id(a) for a in actions)
        if key in self.registered:
            return self.registered[key]
        index = len(self.programs)
        program = Program(self, actions)
        self.programs.append(program)
        self.registered[key] = index
        program.compile()
        return index

    def load(self, root):
        self.parameter_count = max((int(p) for path in root.rglob("*.xml")
                                    for p in re.findall(r"\$(\d+)", path.read_text())), default=0)
        for path in sorted(root.rglob("*.xml")):
            tree = ET.parse(path).getroot()
            if tree.tag.split("}")[-1] != "bulletml":
                continue
            for element in tree.iter():
                element.tag = element.tag.split("}")[-1]
            if tree.get("type", "vertical") != "vertical":
                raise ValueError(f"Unsupported orientation in {path}")
            self.labels = {(e.tag, e.get("label")): e for e in tree if e.get("label")}
            tops = [e for e in tree if e.tag == "action" and e.get("label", "").startswith("top")]
            self.patterns.append((path.relative_to(root).as_posix(), [self.program([e]) for e in tops]))

    def emit(self):
        lines = ["using System;", "using System.Collections.Generic;", "public static class BarrageCode", "{"]
        lines += ["public static int Find(string name)", "{", "switch (name)", "{"]
        lines += [f"case {json.dumps(name)}: return {i};" for i, (name, _) in enumerate(self.patterns)]
        lines += ['default: return -1;', "}", "}"]
        lines += ["public static int[] Roots(int pattern)", "{", "switch (pattern)", "{"]
        lines += [f"case {i}: return new int[] {{ {', '.join(map(str, roots))} }};" for i, (_, roots) in enumerate(self.patterns)]
        lines += ["default: return new int[0];", "}", "}"]
        lines += ["public static PatternState Create(int program, float[] args)", "{", "var s = new PatternState();", "s.Program = program;", "s.Args = args;", "switch (program)", "{"]
        for i, p in enumerate(self.programs):
            lines += [f"case {i}: s.Values = new float[{p.slots}]; s.Counts = new int[{p.loops}]; s.Limits = new int[{p.loops}]; break;"]
        lines += ["}", "return s;", "}"]
        lines += ["public static void Run(PatternState s, PatternBody b, PatternWorld w)", "{", "switch (s.Program)", "{"]
        lines += [f"case {i}: Run{i}(s, b, w); break;" for i in range(len(self.programs))]
        lines += ["}", "}"]
        lines += [p.emit(i) for i, p in enumerate(self.programs)]
        return "\n".join(lines + ["}", ""])


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    compiler = Compiler()
    compiler.load(args.input)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(compiler.emit())
    print(f"Compiled {len(compiler.patterns)} patterns into {len(compiler.programs)} state machines")
