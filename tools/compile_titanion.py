import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (280, 64, 24))
output = Path('build/titanion')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''using static Drawing;
public static class TitanionTitleImage {
static DrawImage title = ''' + image('titanion', pixels, 280, 64, True) + ''';
public static void draw() {
Color(1, 1, 1, 1);
Image(title, 0, 0, 280, 64, 1, 1, 1, 1);
}
}
''')
