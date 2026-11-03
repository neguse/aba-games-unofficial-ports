"""Data the VR games (GearToyGear, Torus Trooper, Mazer Mayhem) ship next to their code.

The web build (compile_frame_web.py) and the Quest build (build_quest.py) use the same audio and license text.
"""
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parent.parent


def convert(source, target, *filters):
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', str(source), *filters,
                    '-c:a', 'pcm_s16le', str(target)], check=True)


def torus_audio(original, output):
    paths = [path for path in (original / 'sounds').rglob('*') if path.suffix in ['.wav', '.ogg']]
    for path in paths:
        convert(path, output / (path.stem + '.wav'))
    return [path.stem + '.wav' for path in paths]


def gear_audio(original, output):
    volumes = dict(re.findall(r'Sound\s*\{\s*Name = (\w+);\s*Volume = ([-\d]+)',
                              (original / 'Content/Audio/Gtg.xap').read_text()))
    paths = list((original / 'Content/Audio').glob('*.wav'))
    for path in paths:
        gain = .5 * 10 ** (int(volumes[path.stem]) / 2000)
        convert(path, output / path.name, '-af', f'volume={gain}')
    return [path.name for path in paths]


def mazer_audio(original, output):
    paths = list((original / 'Content/Audio').glob('*.wav'))
    for path in paths:
        # Mm.xap: each cue is -12 dB, with +5 dB for the Music category.
        gain = '-7dB' if path.stem in ['Mm1', 'Mm2', 'Mm3'] else '-12dB'
        convert(path, output / path.name, '-af', f'volume={gain}')
    return [path.name for path in paths]


def license_text(game, original, lub, tcs):
    """The LICENSE.txt of a package that holds `game` linked with lub and the tcs2c runtime."""
    own = {'torus-trooper': lambda: (original / 'readme_e.txt').read_text(),
           'gear-toy-gear': lambda: (root / 'games/gear-toy-gear/LICENSE.txt').read_text(),
           'mazer-mayhem': lambda: (root / 'games/mazer-mayhem/LICENSE.txt').read_text()}[game]()
    return (own + '\n\n' + (root / 'LICENSE').read_text() + '\n\nlub\n---\n' + (lub / 'LICENSE').read_text()
            + '\n\n' + (lub / 'THIRD_PARTY_LICENSES.md').read_text()
            + '\n\ntcs2c runtime\n---\n' + (tcs / 'LICENSE').read_text()
            + '\n\n' + (tcs / 'THIRD_PARTY_LICENSES.md').read_text())
