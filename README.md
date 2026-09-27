# ABA Games Unofficial Ports

Kenta Cho / ABA Games の非公式 Steam Frame 移植。
GearToyGear と Torus Trooper を Steam Frame 本体で実行する。

## ダウンロード

[Releases](https://github.com/neguse/tsumiki/releases) から遊びたいゲームの ZIP を選ぶ。
1 件の Release に両ゲームの ZIP を添付する。

- `GearToyGear-steam-frame-arm64.zip`
- `TorusTrooper-steam-frame-arm64.zip`

## 導入

1. Steam Frame 上で ZIP をダウンロードし、ゲームごとに別のフォルダへ展開する。
2. ファイルマネージャーで展開先の `run.sh` を右クリックし、「Steam に追加」を選ぶ。
3. Steam ライブラリから登録したゲームを起動する。

ZIP には ARM64 の .NET ランタイムと必要なライブラリを含む。
保存先は `~/.local/share/gear-toy-gear/` と `~/.local/share/torus-trooper/`。
`XDG_DATA_HOME` を設定している場合は、その配下に保存する。
ゲームを終了して展開先を更新しても、これらの保存データは維持される。

## 操作

| 操作 | GearToyGear | Torus Trooper |
| --- | --- | --- |
| 左スティック | 移動 | 左右で旋回、上下で速度調整 |
| 右トリガー | 加速 | 通常ショット |
| 左トリガー | 通常速度まで減速 | チャージ・減速、離すと発射 |
| A | 開始・リトライ | 開始・決定 |
| Menu | ポーズ・再開 | ポーズ・再開 |
| B | ポーズ中にタイトルへ戻る | ポーズ中にタイトルへ戻る、タイトルでリプレイ切替 |
| 右スティック押し込み | 割り当てなし | 三人称／一人称の切替 |

GearToyGear のショットは自動。
Torus Trooper のタイトルでは、左スティックの左右で難易度、上下で開始レベルを選ぶ。
頭の動きは視点に反映され、機体の操作はスティックで行う。

## ソースコード

配布版のコードは、利用する Release のタグから参照する。
Frame 版の開発ブランチは
[`release/frame`](https://github.com/neguse/tsumiki/tree/release/frame)。
ゲームのソースは [GearToyGear](games/gear-toy-gear/) と
[Torus Trooper](games/torus-trooper/) にあり、各 `frame/` が Frame 用の起動・描画・入力を担当する。

## 依存バージョン

| 依存 | 使用コミット／バージョン |
| --- | --- |
| Lub | [`81d28ec2be7b056b8532b160e2a0c343db9c488e`](https://github.com/neguse/lub/tree/81d28ec2be7b056b8532b160e2a0c343db9c488e) |
| tcs | [`3b41b79e7bc9b753e52ca57fbed55aa61d6dfd53`](https://github.com/neguse/tcs/tree/3b41b79e7bc9b753e52ca57fbed55aa61d6dfd53)（Lub のサブモジュール） |
| .NET SDK | 10 |
| Slang | 2026.8.1 Linux ARM64 公式バイナリ |

Frame のビルドには上記の Lub コミットを使用する。
依存リポジトリの既定ブランチや PR のマージ状態には依存しない。

## ビルド

Linux ARM64、Git、.NET SDK 10、Python 3、FFmpeg、C/C++ コンパイラ、
CMake、Ninja、Vulkan 開発ライブラリ、zip を使用する。
利用する Release のタグをチェックアウトし、以下はリポジトリルートから実行する。

```sh
git clone https://github.com/neguse/lub.git ../lub-frame
git -C ../lub-frame checkout --detach 81d28ec2be7b056b8532b160e2a0c343db9c488e
git -C ../lub-frame submodule update --init --recursive
dotnet build ../lub-frame/third_party/tcs/Transpiler/Transpiler.csproj -c Release
(
  cd ../lub-frame
  timeout 7200 bash scripts/build-release.sh --target lub
  timeout 7200 bash scripts/build-release.sh --target lub_shared --no-configure
)

mkdir -p build/frame/native
cp -L ../lub-frame/build-release-linux/lub build/frame/native/
cp -L ../lub-frame/build-release-linux/liblub.so build/frame/native/
cp -L ../lub-frame/build-release-linux/third_party/SDL/libSDL3.so.0 build/frame/native/
cp -L ../lub-frame/build-release-linux/third_party/openxr/src/loader/libopenxr_loader.so.1 build/frame/native/
cp -L ../lub-frame/third_party/slang/lib/libslang-compiler.so.0.2026.8.1 build/frame/native/
```

[GearToyGear 0.1 の原作ソース](https://www.asahi-net.or.jp/~cs8k-cyu/xna/gtg/index_e.html)と
[Torus Trooper 0.22 の原作](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tt_e.html)を取得・展開する。
GTG の `--original` は `Content` を含むディレクトリ、
TT は `sounds`・`barrage`・`readme_e.txt` を含むディレクトリを指定する。

両ビルダーの出力先は `build/frame/publish` なので、1 作品ずつ別フォルダへ移す。
`publish`・`GearToyGear`・`TorusTrooper` が存在しない状態から実行する。

```sh
python3 tools/build_gtg_frame.py --lub ../lub-frame \
  --original /path/to/GearToyGear/GearToyGear --native build/frame/native
mv build/frame/publish build/frame/GearToyGear

python3 tools/build_tt_frame.py --lub ../lub-frame \
  --original /path/to/tt --native build/frame/native
mv build/frame/publish build/frame/TorusTrooper

(
  cd build/frame
  zip -r GearToyGear-steam-frame-arm64.zip GearToyGear
  zip -r TorusTrooper-steam-frame-arm64.zip TorusTrooper
)
```

## 原作とライセンス

- [GearToyGear](https://www.asahi-net.or.jp/~cs8k-cyu/xna/gtg/index_e.html)：Kenta Cho、MIT。
- [Torus Trooper](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tt_e.html)：Kenta Cho、BSD。

原作・ランタイム・依存ライブラリの許諾文と著作権表示を各 ZIP に同梱する。
