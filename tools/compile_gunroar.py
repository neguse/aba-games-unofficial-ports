import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (256, 64, 8))
output = Path('build/gunroar')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''using static Drawing;
public static class GunroarTitleImage {
static DrawImage title = ''' + image('gunroar', pixels, 256, 64, True) + ''';
public static void draw() {
Color(1, 1, 1, 1);
Image(title, 0, -63, 255, 63, 1, 1, 1, 1);
}
}
''')
