# まさしくんハイ！

[原作1.11e](https://www.asahi-net.or.jp/~cs8k-cyu/free/mas.html)の5競技を
TinyC# / lubで実行する。原作の33ms更新、地形、線画、文字、記録計算を使う。
F2で5種競技、F5〜F9で各競技、F3でポーズ。マウスの移動量で加速し、
クリックでジャンプ・引っ張り・投擲を行う。ShiftとSpaceでも操作できる。
各競技と総合の上位3件を、名前とともにブラウザー内に保存する。

ゲームと原作データはGPL-2.0-or-later。著作権表示と全文は
[LICENSE.txt](LICENSE.txt)を参照。ブラウザー用の変更日は2026-09-24。
原作のWindowsメニュー・ファイル保存・カーソル固定をHTMLとHostメッセージへ置き換え、
640×480の整数線分をWebGPUで描画する。音源は原作に含まれない。

## ビルド

Python 3、Node.js、.NET 10、CMake、C/C++コンパイラ、Git、Emscriptenが必要。
Emscriptenの環境を読み込んだシェルで、リポジトリまたはソース配布のルートから実行する。

```sh
bash tools/setup_lub.sh
python3 tools/build_masashikun.py --lub .cache/lub
python3 tests/check_game.py --lub .cache/lub --game masashikun-hi
python3 -m http.server 8765 --directory dist
```

`http://localhost:8765/masashikun-hi/`をWebGPU対応ブラウザーで開く。
`source.tar.gz`から展開した場合は、同梱の原作データを指定できる。

```sh
python3 tools/build_masashikun.py --lub .cache/lub --tcs .cache/lub/third_party/tcs --emsdk /path/to/emsdk --original original
```

lubとそのサブモジュールTinyC#は、`tools/setup_lub.sh`が固定するコミットを使う。
同スクリプトがこのソースと依存ライブラリを取得して実行環境を構築する。
ゲームはtcs2cでCへ変換し、lubとリンクしてWasmにする。
原作アーカイブのSHA-256はビルド時に照合する。
配布先の`source.tar.gz`には、このゲームの編集用C#、線画データ、
シェーダー、Webページ、ビルド・検証手順、およびゲームのWasmに含まれるtcs2cランタイムのソースを収録する。

原作のDelphi引数評価順と32bit乱数を保持する。
人投げの崖配列の33番目と角度表の257番目は原作EXEが隣接データを読むため、
1.11eの値を明示的に使用する。比較テストはこの条件で原作Pascalを実行した
3,600更新の状態と30画面のピクセルハッシュ、および5競技を通す進行を検証する。

ブラウザー検証にはPlaywrightの`chromium`を使用する。
必要に応じて`PLAYWRIGHT_MODULE`と`CHROMIUM_PATH`でインストール先を指定する。

```sh
node tests/masashikun-hi/browser.mjs http://localhost:8765/masashikun-hi/
```
