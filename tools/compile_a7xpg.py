import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (64, 64, 24))
output = Path('build/a7xpg')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('public static class A7xData {\npublic static DrawImage title = ' + image('a7xpg', pixels, 64, 64, False) + ';\n}\n')
