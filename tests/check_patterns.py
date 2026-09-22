import argparse
import hashlib
import math
import os
from pathlib import Path
import subprocess
import sys
import tarfile
import urllib.request


def run(command, **kwargs):
    result = subprocess.run(command, text=True, capture_output=True, **kwargs)
    if result.returncode:
        sys.stderr.write(result.stdout + result.stderr)
        result.check_returncode()
    return result.stdout


parser = argparse.ArgumentParser()
parser.add_argument("--original", type=Path, default=Path('.cache/original/tf'))
parser.add_argument("--library", type=Path)
parser.add_argument("--tcs", type=Path, default=Path('.cache/lub/third_party/tcs'))
parser.add_argument("--turns", type=int, default=1200)
args = parser.parse_args()
if args.library is None:
    cache = Path('.cache')
    cache.mkdir(exist_ok=True)
    archive = cache / 'libbulletml-0.0.6.tar.bz2'
    if not archive.exists():
        urllib.request.urlretrieve('https://shinh.skr.jp/libbulletml/libbulletml-0.0.6.tar.bz2', archive)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != '7c37f3d2d52825417c5de716f89bea4b71156371e698e2579daf7921df07aa79':
        raise ValueError('libBulletML archive checksum mismatch')
    with tarfile.open(archive) as source:
        source.extractall(cache, filter='data')
    args.library = cache / 'bulletml'
build = Path("build/tests")
build.mkdir(parents=True, exist_ok=True)
source = args.library / "src"
sources = [source / (s + ".cpp") for s in ["bulletmlparser-tinyxml", "bulletmlparser", "bulletmltree", "calc", "formula-variables", "bulletmlrunner", "bulletmlrunnerimpl"]]
sources += sorted((source / "tinyxml").glob("tinyxml*.cpp"))
run(["g++", "-std=gnu++11", "-O2", "-w", "-include", "cstring", "-include", "cstdlib", "-include", "cstdio", "-I" + str(source), "tests/pattern_oracle.cpp", *map(str, sources), "-o", str(build / "oracle")])
run([sys.executable, "tools/compile_barrage.py", str(args.original / "barrage"), str(build / "BarrageCode.cs")])
compiler = args.tcs / "Transpiler/bin/Release/net10.0/Transpiler.dll"
result = run(["dotnet", str(compiler), "game/Pattern.cs", "game/PatternNumber.cs", str(build / "BarrageCode.cs"), "tests/PatternTrace.cs", "--entry", "PatternTrace", "--no-naming-check", "-o", str(build / "trace.lua")])
if result.strip(): print(result)
(build / "run.lua").write_text('local trace = dofile(arg[1]); trace.main()\n')
failures = []
cases = 0
maximum = 0.0
for path in sorted((args.original / "barrage").rglob("*.xml")):
    for rank in ["0", "0.25", "0.5", "0.9", "1"]:
        name = path.relative_to(args.original / "barrage").as_posix()
        env = dict(os.environ, PATTERN_NAME=name, PATTERN_RANK=rank, PATTERN_TURNS=str(args.turns))
        expected = run([str(build / "oracle"), str(path), rank, str(args.turns)]).splitlines()
        actual = run([str(args.tcs / "deps/lua/lua32"), str(build / "run.lua"), str(build / "trace.lua")], env=env).splitlines()
        error = None
        if len(expected) != len(actual): error = f"trace length {len(expected)} != {len(actual)}"
        for index, (a, b) in enumerate(zip(expected, actual)):
            av, bv = a.split(), b.split()
            ints = {1, 2, 3} if av[0] == "F" else {1, 2, 7} if av[0] == "B" else {1, 2}
            if len(av) != len(bv) or av[0] != bv[0]: error = f"event {index}: {a} != {b}"; break
            for col in range(1, len(av)):
                if not math.isfinite(float(av[col])) or not math.isfinite(float(bv[col])):
                    error = f"non-finite event {index}: {a} != {b}"; break
                delta = abs(float(av[col]) - float(bv[col]))
                if (av[0] == "F" and col == 4) or (av[0] == "B" and col == 3):
                    delta = abs((delta + 180) % 360 - 180)
                if col not in ints: maximum = max(maximum, delta)
                tolerance = 0 if col in ints else 0.002 + abs(float(av[col])) * 0.00002
                if delta > tolerance:
                    error = f"event {index}: {a} != {b}"; break
            if error: break
        cases += 1
        if error:
            failures.append(f"{name} rank={rank}: {error}")
            print(failures[-1], flush=True)
print(f"{cases} cases, {len(failures)} failures, largest numeric difference {maximum:g}")
sys.exit(bool(failures) or cases != 345)
