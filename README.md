# TUMIKI Fighters

[TUMIKI Fighters 0.21](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tf_e.html)
のブラウザ移植。ゲーム本体はTinyC#、実行環境は[lub](https://github.com/neguse/lub)。
BulletMLと原作データをビルド時にTCSへ変換する。

## 遊び方

WebGPU対応のPCブラウザとキーボードを使用する。

| 操作 | キー |
| --- | --- |
| 移動 | 矢印 / WASD |
| ショット・開始・決定 | Z / 左Ctrl / . |
| 低速移動・部品の収納 | X / 左Alt / Shift / / |
| ポーズ・再開 | P |
| タイトルへ戻る | Esc |

敵を破壊した破片を拾うと自機に接続され、反撃と定期的な得点が発生する。
収納中は部品への被弾を防げるが、反撃が止まり、部品の得点が5分の1になる。
ランキングはブラウザのローカルストレージに保存する。

## ビルド

Linux、Git、Python 3.12以降、.NET SDK 10、Node.js 26、C/C++コンパイラ、
CMake、Ninja、curl、unzip、Emscripten SDK 5.0.2が必要。
emsdkの`emsdk_env.sh`を読み込み、リポジトリのルートで実行する。

```sh
bash tools/setup_lub.sh
python3 tools/build.py --lub .cache/lub
python3 -m http.server 8765 --directory dist --bind 127.0.0.1
```

`http://127.0.0.1:8765`を開く。WebGPUにはlocalhostまたはHTTPSが必要。
依存物と原作アーカイブは`.cache/`、生成コードは`build/`、配布物は`dist/`に置く。
原作アーカイブのSHA-256はビルド時に照合する。
展開済みの原作を使う場合は`--original /path/to/tf`を指定する。

lubは`e30ea534847fb61fd37c732e0b73cabe06006065`に固定し、
TCSとLuaはそのサブモジュール、Slangは`v2026.8.1`を使用する。
シェーダーもビルド時に変換するため、ブラウザにはSlangやBulletMLの解析器を配布しない。

## 検証

```sh
python3 tests/check_patterns.py
python3 tests/check_game.py --lub .cache/lub
node tests/audio.test.mjs
```

弾幕の検証はlibBulletML 0.0.6を比較用に取得し、SHA-256を照合する。
原作の69パターンを5段階の難易度で各1200フレーム実行し、発射・消滅時刻と軌道を比較する。
ゲームの検証は部品の取得・接続・収納・反撃・切断・得点・難易度変化、敵の部分破壊、コンティニューと、
自動操作・無敵状態での全5面からエンディングへの進行を確認する。

ブラウザ検証にはPlaywrightを使用する。ローカル配信を起動した状態で実行する。

```sh
npm install --prefix .cache/browser --no-save playwright@1.60.0
.cache/browser/node_modules/.bin/playwright install chromium
node tests/browser.mjs
```

開始・移動・収納・ポーズ、各面の描画、音源のデコードと再生、ランキングの再読込を確認する。
テスト内でLuaに観測処理と面・ゲームオーバーへの遷移を挿入する。
画面は`build/screenshots/`へ出力する。公開先も同じ検証を実行できる。

```sh
node tests/browser.mjs https://公開先のホスト名
```

## 公開と取り消し

PRを人間がマージした後、そのコミットをビルド・検証し、
Cloudflare Workers Static Assetsへ配布する。

```sh
npx wrangler@4.135.0 deploy --dry-run
npx wrangler@4.135.0 deploy
npx wrangler@4.135.0 deployments list
```

公開URLでも上記のブラウザ検証を実行する。更新前のVersion IDを控え、
不具合時は`npx wrangler@4.135.0 rollback <Version ID>`で戻す。
初回公開を取り下げる場合は`npx wrangler@4.135.0 delete --name tsumiki`を使用する。

## ライセンス

原作のコード・データ・音源はKenta ChoのBSDライセンス。
[LICENSE](LICENSE)に原作および乱数生成器のライセンスを収録する。
配布物の`LICENSE.txt`にはlubと依存ライブラリのライセンスも含む。
