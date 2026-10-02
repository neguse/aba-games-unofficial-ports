# ABA Games ブラウザ移植

ABA GamesのWindows作品をlubとTinyC#でブラウザへ移植する。
対象作品と公開条件は[移植方針](../PORTING_PLAN.md)を参照。
同じ非公開リポジトリで管理し、作品ごとに検証して配布する。

| ゲーム | 配布パス | ソース |
| --- | --- | --- |
| TUMIKI Fighters | `/` | `game/` |
| PARSEC47 | `/parsec47/` | `games/parsec47/` |
| Gunroar | `/gunroar/` | `games/gunroar/` |
| Titanion | `/titanion/` | `games/titanion/` |
| A7Xpg | `/a7xpg/` | `games/a7xpg/` |
| Torus Trooper | `/torus-trooper/` | `games/torus-trooper/` |
| rRootage | `/rrootage/` | `games/rrootage/` |
| Noiz2sa | `/noiz2sa/` | `games/noiz2sa/` |
| Wok | `/wok/` | `games/wok/` |
| Mazer Mayhem | `/mazer-mayhem/` | `games/mazer-mayhem/` |
| GearToyGear | `/gear-toy-gear/` | `games/gear-toy-gear/` |
| Mu-cade | `/mu-cade/` | `games/mu-cade/` |
| まさしくんハイ！ | `/masashikun-hi/` | `games/masashikun-hi/` |

## TUMIKI Fighters

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

## PARSEC47

[PARSEC47 0.21](https://www.asahi-net.or.jp/~cs8k-cyu/windows/p47_e.html)の
ROLL／LOCKと4段階の難易度に対応する。矢印・テンキー・WASDで移動、Z・左Ctrlで
ショット、X・左Alt・左Shiftで低速移動と特殊攻撃、Pでポーズ。
タイトル画面のXでモードを切り替え、矢印で難易度と開始PARSECを選び、Zで開始する。
ROLLはXを離すと発射、LOCKは押している間に正面の敵を狙う。
最高得点、到達PARSECと選択したモード・難易度をTUMIKIとは別に保存する。

弾幕85種をビルド時にTCSへ変換し、Bulletsmorphによる弾幕の切り替えと
弾数・速度に応じた原作の意図的な処理落ちを再現する。
通常描画を使用し、原作のオプションで有効にする発光処理は対象外。

## Gunroar

[Gunroar 0.15](https://www.asahi-net.or.jp/~cs8k-cyu/windows/gr_e.html)の
NORMAL／TWIN STICK／DOUBLE PLAY／MOUSEに対応する。
タイトルの上下・Xでモードを選び、Zで開始する。Pでポーズ、Escでタイトルへ戻る。

| モード | 操作 |
| --- | --- |
| NORMAL | 矢印・WASDで移動、Zで砲撃・向き固定、Xでランス |
| TWIN STICK | WASDで移動、IJKLで照準・砲撃 |
| DOUBLE PLAY | WASDとIJKLで2隻を操作 |
| MOUSE | 矢印・WASDで移動、マウスで照準、左ボタンで集中砲撃、右ボタンで拡散砲撃 |

前進速度による難易度と得点倍率の上昇、砲台の破壊、ボス出現を原作の処理で行う。
各モードの最高得点と選択モード、ゲームオーバー後の直前のリプレイを
ブラウザに保存する。リプレイはタイトルのデモとREPLAYで再生する。
通常描画を使用し、原作の任意の発光処理とゲームパッドは対象外。

## Titanion

[Titanion 0.3](https://www.asahi-net.or.jp/~cs8k-cyu/windows/ttn_e.html)の
CLASSIC／BASIC／MODERNに対応する。上下でモードを選び、Zで開始する。
矢印・テンキー・WASD・IJKLで移動、Zでショット、Xで捕獲・挑発ビーム、Pでポーズ。
CLASSICはエネルギー満タンで捕獲ビームを発射し、その間は無敵になる。
BASICはいつでも捕獲でき、近距離で倒すと最大16倍の倍率が付く。
MODERNはXだけで挑発し、Z＋Xでは低速移動と集中ショットになる。
モード別ランキングと直前のリプレイを保存し、タイトルでリプレイを再生する。
原作の通常描画と固定ステージ構成を使用し、任意の残像表示・ランダム化・ゲームパッドは対象外。

## A7Xpg

[A7Xpg 0.11](https://www.asahi-net.or.jp/~cs8k-cyu/windows/a7xpg_e.html)の
全30ステージと周回、6種類の敵、ブースト・金塊回収・無敵状態を再現する。
矢印・テンキーで移動、Z・X・Ctrl・Altで開始とブースト、Pでポーズ。
金塊を高速で回収してゲージを満たし、無敵中の体当たりで連続ボーナスを狙う。
原作の標準発光処理と15音源を使い、ハイスコアはブラウザ内に保存する。

## Torus Trooper

[Torus Trooper 0.22](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tt_e.html)の3難易度と立体コース、
加減速・チャージショット・弾消し・連続撃破倍率・時間制の進行を移植する。
28本のBulletMLはビルド時に変換し、敵の弾幕変形もゲーム側で実行する。
タイトルで左右が難易度、上下が開始レベル、Zが開始、Xが最終プレイのリプレイ。
プレイ中は矢印／WASDで移動、Zで通常弾、Xを押してチャージし、離すと発射する。
ハイスコア・到達レベル・リプレイをブラウザ内に保存する。
描画は原作の既定設定を使用し、コマンドラインの発光・画面回転設定とジョイパッドは対象外。

## rRootage

4モード×40ステージで、各5体のボスを倒す。矢印・WASDで移動、Zでレーザー、
Xで特殊操作、Pでポーズ、Escでタイトルへ戻る。
タイトルでは矢印でステージを選び、Xでモードを切り替える。
NORMALのボム、PSYのかすりと回転、IKAの属性吸収、GWの反射を原作のルールで扱う。
最高得点とクリア状況をモード・ステージごとにブラウザへ保存する。
68弾幕と画像をビルド時に変換し、原作の19音源を使用する。
原作のコマンドライン設定とゲームパッドは対象外。

## Noiz2sa

通常10ステージと4種類のエンドレスに対応する。上下でステージを選び、Zで開始する。
矢印・WASD・テンキーで移動、Zでショット、Xで低速移動、Pでポーズ、Escでタイトルへ戻る。
緑のボーナスを連続して拾うと得点が増え、取り逃すと半減する。
73弾幕と色の合成表・タイトル画像をビルド時に変換し、原作の残像と14音源を使用する。
ステージ別・シーン別の最高得点と選択ステージをブラウザへ保存する。
原作のコマンドライン設定とゲームパッドは対象外。

## Wok

原作1.0の球の衝突・鍋の傾き・6種類の発生装置・連続得点を移植する。
マウスでSTARTをクリックし、鍋で球を受け止めて右側へ投げる。
プレイ中はポインターロックで相対移動を受け取り、原作の全画面時の感度を使う。
Escでマウスを解放してタイトルへ戻る。球を1個でも下へ落とすと終了する。
48画像をビルド時に変換し、2曲・3効果音とブラウザ内のハイスコア保存を使う。
原作の古いVorbis音源は、ブラウザで再生できるPCMへビルド時に変換する。
コマンドラインのマウス感度設定は対象外。

## Mazer Mayhem

原作0.14の戦車・球の物理、迷路、ボス、ハイパーと得点倍率を移植する。
矢印/WASDで移動、Xで開始・射撃、Z/Cで旋回する。
Xを押すとダッシュとグレネード、ZとCを同時に押し続けると収集アイテムでハイパーを発動する。
F1でポーズ、Escでタイトルへ戻る。
44文字の形状と迷路XMLをビルド時に変換し、原作の立体・残像・反射と14音源を使う。
ランキング10件をブラウザへ保存し、タイトルでは直前のプレイを再生する。
ゲームパッドと右スティックによる視点変更は対象外。

## GearToyGear

原作0.1の筒状コース、加減速、自動ショットと誘導レーザー、ボスを移植する。
矢印/WASDで移動、X/V/カンマ/スラッシュで開始・加速、Z/C/M/ピリオドで減速する。
Eでアクセル、Qでブレーキ、F1/Pでポーズ、Escでタイトルへ戻る。
加速するとゲーム速度と得点倍率が上がる。6区間ごとにボスが出現する。
44文字の形状をビルド時に変換し、原作の立体・発光描画と11音源を使う。
効果音は敵と自機の相対位置に応じて定位し、レーザー音は飛行中にループする。
ランキング10件をブラウザに保存し、直前のプレイをタイトルで再生する。
ゲームパッドは対象外。

## Mu-cade

[Mu-cade 0.11](https://www.asahi-net.or.jp/~cs8k-cyu/windows/mcd_e.html)の
移動は矢印・WASD、開始・ショットはZ・Ctrl、IJKLは照準とショット。
ショット中は向きを固定する。X・Shift・Spaceで尾を切り、敵弾を消してショットを強化する。
敵を場外へ押し出すと得点し、尾が長いほど倍率が上がる。Pでポーズ、Escでタイトルへ戻る。
得点と経過時間のランキングを保存する。

弾幕13種と文字・タイトル画像をビルド時に変換する。連結・衝突・反力は
ODE 0.5.0（`7bac210f051b3ffcfaf9a168db3d7c302f7a49a4`）を倍精度で実行し、
TinyC#との接続だけを`ode.cpp`に持つ。Mu-cade専用ランタイムは`mu-cade/wasm/`へ配置する。

## まさしくんハイ！

5競技・競技ごとの説明・総合記録に対応する。F2で5種競技、F5〜F9で各競技を開始し、
マウスを回して加速、クリックでアクション。F3でポーズ、ShiftとSpaceで代替操作。
上位3件の記録と名前を競技別に保存する。
GPLの対応ソースを配布物に含める。単体ビルドと検証は[手順](../games/masashikun-hi/README.md)を参照。

## ビルド

Linux、Git、Python 3.12以降、.NET SDK 10、Node.js 26、C/C++コンパイラ、
CMake、Ninja、curl、unzip、libvorbisデコーダー付きFFmpeg、Emscripten SDK 5.0.2が必要。
emsdkの`emsdk_env.sh`を読み込み、リポジトリのルートで実行する。

```sh
bash tools/setup_lub.sh
python3 tools/build.py --lub .cache/lub --tcs .cache/lub/third_party/tcs --emsdk /path/to/emsdk
python3 -m http.server 8765 --directory dist --bind 127.0.0.1
```

`http://127.0.0.1:8765`を開く。WebGPUにはlocalhostまたはHTTPSが必要。
PARSEC47は`http://127.0.0.1:8765/parsec47/`、Gunroarは`http://127.0.0.1:8765/gunroar/`を開く。
Titanionは`http://127.0.0.1:8765/titanion/`を開く。同じビルドで上記の12作品を生成する。
依存物と原作アーカイブは`.cache/`、生成コードは`build/`、配布物は`dist/`に置く。
原作アーカイブのSHA-256はビルド時に照合する。
展開済みの原作を使う場合は`--original /path/to/tf`を指定する。

lubは`d28a095bdd5fbb09044faecf92fc42469af6c6f3`に固定し、
Torus Trooperはネイティブ版と同じC#・描画・操作・シェーダーを使う。
`TorusTrooper.csproj`のソース一覧をtcs2cでCへ変換し、Wasmを`torus-trooper/wasm/`へ配置する。
OpenXRとWebXRの接続の差はLubが扱う。
ほかのゲームのTCSとLuaはlubのサブモジュール、Slangは`v2026.8.1`を使用する。
シェーダーもビルド時に変換するため、ブラウザにはSlangやBulletMLの解析器を配布しない。

## 検証

```sh
python3 tests/check_patterns.py
python3 tests/check_game.py --lub .cache/lub
node tests/audio.test.mjs
python3 tests/check_patterns.py --patterns .cache/original/p47 --expected-cases 425
python3 tests/check_game.py --lub .cache/lub --game parsec47
python3 tests/check_game.py --lub .cache/lub --game gunroar
python3 tests/check_game.py --lub .cache/lub --game titanion
python3 tests/check_game.py --lub .cache/lub --game a7xpg
python3 tests/check_game.py --lub .cache/lub --game torus-trooper
python3 tests/check_patterns.py --patterns .cache/original/tt/barrage --expected-cases 140
python3 tests/check_game.py --lub .cache/lub --game rrootage
python3 tests/check_patterns.py --patterns .cache/original/rr --expected-cases 340
python3 tests/check_game.py --lub .cache/lub --game noiz2sa
python3 tests/check_patterns.py --patterns .cache/original/noiz2sa --expected-cases 365
python3 tests/check_game.py --lub .cache/lub --game wok
python3 tests/check_game.py --lub .cache/lub --game mazer-mayhem
python3 tests/check_game.py --lub .cache/lub --game gear-toy-gear
python3 tests/check_game.py --lub .cache/lub --game mu-cade
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
node tests/parsec47/browser.mjs
node tests/gunroar/browser.mjs
node tests/titanion/browser.mjs
node tests/a7xpg/browser.mjs
node tests/torus-trooper/browser.mjs
node tests/rrootage/browser.mjs
node tests/noiz2sa/browser.mjs
node tests/wok/browser.mjs
node tests/mazer-mayhem/browser.mjs
node tests/gear-toy-gear/browser.mjs
node tests/mu-cade/browser.mjs
```

開始・移動・収納・ポーズ、各面の描画、音源のデコードと再生、ランキングの再読込を確認する。
テスト内でLuaに観測処理と面・ゲームオーバーへの遷移を挿入する。
Torus Trooperのブラウザテストは、`tools/compile_torus_web.py`に同じ依存引数と
`--original .cache/original/tt --test --output build/web-test/torus-trooper`を渡してビルドし、
`build/web-test/`で実行する。観測用ホストは通常ビルドに含めない。
PARSEC47はROLL／LOCKの操作・進行・描画、15音源とモード別保存を検証する。
ゲーム進行テストは両モード×4難易度でボスを経てPARSEC 12まで進め、
特殊攻撃・得点・被弾・ポーズ・意図的な処理落ちも確認する。
Gunroarは4モードのボス出現、MT19937の参照値と値域、リプレイによる位置・難易度・得点・弾の一致、
ブラウザのキー・マウス操作、14音源、スコアとリプレイの再読込を検証する。
Titanionは3モードの通常進行とPHASE 12までの遷移、捕獲・挑発・接触時の挙動、
連鎖による弾消去と得点、リプレイ再現、
ブラウザ操作、12音源、ランキングとリプレイの再読込を検証する。
A7Xpgは全30面から2周目への遷移、加速・無敵・時間切れ・コンティニュー、
Phobos乱数の参照値、D1で実行した原作の移動軌道、発光描画、15音源と保存を検証する。
Torus Trooperは3難易度で12区間の進行、時間の加減算、チャージと倍率、
600フレームのリプレイ一致、28弾幕×5難易度の原作比較、14音源と保存を検証する。
rRootageは全160ステージの5体のボス進行、原作Cの自機軌道、各モードの防御、
68弾幕×5難易度、19音源とスコア・クリア状況の再読込を検証する。
Noiz2saは通常10面のボス・クリアと4種のエンドレス、原作Cの移動・描画・敵出現順、
73弾幕×5難易度、ボーナス・残機、パレット、14音源とスコアの再読込を検証する。
Wokは原作Cとの球・鍋の軌道と6種の発生装置の比較、連続得点・ミス・音楽切替、
ポインターロック・再開、5音源とハイスコアの再読込を検証する。
Mazer Mayhemは原作C#の3,000更新のゲーム進行と1,200更新の物理、
ハイパー・ボス・リプレイ・ポーズ、14音源とランキングの再読込を検証する。
GearToyGearは原作C#の3,000更新の進行、2回のボス区間・9種の障害物、
加減速・リプレイ・ポーズ、位置音・11音源とランキングの再読込を検証する。
Mu-cadeは移動・照準固定・尾の連結と切断、3種×3サイズの敵、残機と倍率、
原作ODE DLLの衝突・反力・落下軌道、13音源とランキングの再読込を検証する。
画面は`build/screenshots/`へ出力する。公開先も同じ検証を実行できる。

```sh
node tests/browser.mjs https://公開先のホスト名
node tests/parsec47/browser.mjs https://公開先のホスト名/parsec47/
node tests/gunroar/browser.mjs https://公開先のホスト名/gunroar/
node tests/titanion/browser.mjs https://公開先のホスト名/titanion/
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
[LICENSE](../LICENSE)に原作および乱数生成器のライセンスを収録する。
配布物の`LICENSE.txt`にはlubと依存ライブラリのライセンスも含む。
PARSEC47の原作ライセンスも`parsec47/LICENSE.txt`に収録する。
Gunroarの原作ライセンスは`gunroar/LICENSE.txt`に収録する。
Titanionの原作ライセンスは`titanion/LICENSE.txt`に収録する。
A7XpgとPhobos乱数のライセンスは`a7xpg/LICENSE.txt`に収録する。
Torus Trooperの原作ライセンスは`torus-trooper/LICENSE.txt`に収録する。
rRootageの原作ライセンスは`rrootage/LICENSE.txt`に収録する。
Noiz2saの原作ライセンスは`noiz2sa/LICENSE.txt`に収録する。
Wokの原作ライセンスは`wok/LICENSE.txt`に収録する。
Mazer Mayhemの原作ライセンスは`mazer-mayhem/LICENSE.txt`に収録する。
GearToyGearの原作MITライセンスは`gear-toy-gear/LICENSE.txt`に収録する。
Mu-cadeとODEのBSDライセンスは`mu-cade/LICENSE.txt`に収録する。
