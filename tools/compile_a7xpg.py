import argparse
from pathlib import Path
from compile_title import bitmap, image

parser = argparse.ArgumentParser()
parser.add_argument('original', type=Path)
args = parser.parse_args()
pixels = bitmap(args.original / 'images/title.bmp', (64, 64, 24))
output = Path('build/a7xpg')
output.mkdir(parents=True, exist_ok=True)
(output / 'TitleImage.cs').write_text('''using static Drawing;
public static class A7xData {
static DrawImage title = ''' + image('a7xpg', pixels, 64, 64, False) + ''';
public static void drawTitle() {
Color(1, 1, 1, 1);
Image(title, 80, 50, 100, 100, 1, 1, 1, 1);
}
}
''')
