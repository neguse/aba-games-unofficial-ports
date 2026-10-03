import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (256, 64, 8))
output = Path('build/gunroar')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('public static class GunroarTitleImage {\npublic static DrawImage title = '
    + image(output, 'gunroar', pixels, 256, 64, True) + ';\n}\n')
