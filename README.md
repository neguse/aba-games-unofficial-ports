# ABA Games Unofficial Ports

Kenta Cho / ABA Games の非公式 Steam Frame 移植。
GearToyGear・Torus Trooper・Mazer Mayhem を Steam Frame 本体で実行する。

## ダウンロード

[![Click to Install](https://img.shields.io/badge/Click_to_Install-Steam_Frame-1b2838?style=for-the-badge&logo=steam)](https://github.com/neguse/aba-games-unofficial-ports/releases/latest/download/Install-ABA-Games.desktop)

手動で展開する場合は、[Releases](https://github.com/neguse/aba-games-unofficial-ports/releases) から遊びたいゲームの ZIP を選ぶ。
1 件の Release に各ゲームの ZIP を添付する。

- `GearToyGear-steam-frame-arm64.zip`
- `TorusTrooper-steam-frame-arm64.zip`
- `MazerMayhem-steam-frame-arm64.zip`

## 導入

1. Steam Frame のデスクトップで上のボタンを押し、`Install-ABA-Games.desktop` をダウンロードする。
2. ダウンロードしたファイルを開き、実行を許可する。
3. 完了画面が出たら、Steam ライブラリからゲームを起動する。

3ゲームを `~/.local/share/aba-games-unofficial-ports/` に配置し、ゲーム名でSteamに登録する。
更新時もゲームを終了して同じ導入ファイルを開く。セーブデータと既存のSteam登録は維持する。
以前ZIPから手動で登録したゲームがある場合は、Steamに別の項目が追加される。
`XDG_DATA_HOME` を設定している場合は、その配下に配置する。

ZIP には ARM64 の .NET ランタイムと必要なライブラリを含む。
保存先は `~/.local/share/gear-toy-gear/`・`~/.local/share/torus-trooper/`・`~/.local/share/mazer-mayhem/`。
`XDG_DATA_HOME` を設定している場合は、その配下に保存する。
ゲームを終了して展開先を更新しても、これらの保存データは維持される。

## 操作

| 操作 | GearToyGear | Torus Trooper | Mazer Mayhem |
| --- | --- | --- | --- |
| 左スティック | 移動 | 左右で旋回、上下で速度調整 | 移動 |
| 右トリガー | 加速 | 通常ショット | 右旋回 |
| 左トリガー | 通常速度まで減速 | チャージ・減速、離すと発射 | 左旋回 |
| A | 開始・リトライ | 開始・決定 | 開始、押すとダッシュ＋グレネード、保持でショット |
| Menu | ポーズ・再開 | ポーズ・再開 | ポーズ・再開 |
| B | ポーズ中にタイトルへ戻る | ポーズ中にタイトルへ戻る、タイトルでリプレイ切替 | ポーズ中にタイトルへ戻る |
| 右スティック押し込み | 割り当てなし | 三人称／一人称の切替 | 割り当てなし |

GearToyGear のショットは自動。
Torus Trooper のタイトルでは、左スティックの左右で難易度、上下で開始レベルを選ぶ。
Mazer Mayhem は倍率をためて両トリガーを同時に引くとハイパーを発動する。
頭の動きは視点に反映され、機体の操作はスティックで行う。

## ソースコード

配布版のコードは、利用する Release のタグから参照する。
Frame 版の開発ブランチは
[`release/frame`](https://github.com/neguse/aba-games-unofficial-ports/tree/release/frame)。
ゲームのソースは [GearToyGear](games/gear-toy-gear/)・
[Torus Trooper](games/torus-trooper/)・[Mazer Mayhem](games/mazer-mayhem/) にある。Torus Trooperの描画・入力はWebXRと共通で、`frame/`にはネイティブ起動とビルド設定を置く。

## 依存バージョン

| 依存 | 使用コミット／バージョン |
| --- | --- |
| Lub | [`0f6732c444cb8f14ad8851b4ef2f79dc5b8c249f`](https://github.com/neguse/lub/tree/0f6732c444cb8f14ad8851b4ef2f79dc5b8c249f) |
| tcs | [`2d551b8f3b14d0224317b58118db21904a602c88`](https://github.com/neguse/tcs/tree/2d551b8f3b14d0224317b58118db21904a602c88)（Lub のサブモジュール） |
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
git -C ../lub-frame checkout --detach 0f6732c444cb8f14ad8851b4ef2f79dc5b8c249f
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
[Torus Trooper 0.22 の原作](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tt_e.html)、
[Mazer Mayhem 0.14 の原作](https://www.asahi-net.or.jp/~cs8k-cyu/xna/mm/index_e.html)を取得・展開する。
GTG・MM の `--original` は `Content` を含むディレクトリ、
TT は `sounds`・`barrage`・`readme_e.txt` を含むディレクトリを指定する。

GTG・TT ビルダーの出力先は `build/frame/publish` なので、1 作品ずつ別フォルダへ移す。
`publish`・`GearToyGear`・`TorusTrooper`・`MazerMayhem` が存在しない状態から実行する。

```sh
python3 tools/build_gtg_frame.py --lub ../lub-frame \
  --original /path/to/GearToyGear/GearToyGear --native build/frame/native
mv build/frame/publish build/frame/GearToyGear

python3 tools/build_tt_frame.py --lub ../lub-frame \
  --original /path/to/tt --native build/frame/native
mv build/frame/publish build/frame/TorusTrooper

python3 tools/build_mm_frame.py --lub ../lub-frame \
  --original /path/to/Mm/Mm --native build/frame/native
mv build/mm-frame/publish build/frame/MazerMayhem

(
  cd build/frame
  zip -r GearToyGear-steam-frame-arm64.zip GearToyGear
  zip -r TorusTrooper-steam-frame-arm64.zip TorusTrooper
  zip -r MazerMayhem-steam-frame-arm64.zip MazerMayhem
)
```

Torus Trooperは`--runtime coreclr`（既定）、`--runtime tcs2c`、`--runtime both`で
実行方式を選ぶ。tcs2cを含む場合は`--tcs ../lub-frame/third_party/tcs`を指定し、
GCCまたはClangでCをビルドする（`--cc`でコンパイラーを指定できる）。
両方を含む配布物では`LUB_RUNTIME=coreclr ./run.sh`または
`LUB_RUNTIME=tcs2c ./run.sh`で起動する。ゲーム・描画・操作のC#とシェーダーは、
WebXR版も同じものを使う。


## 原作とライセンス

- [GearToyGear](https://www.asahi-net.or.jp/~cs8k-cyu/xna/gtg/index_e.html)：Kenta Cho、MIT。
- [Torus Trooper](https://www.asahi-net.or.jp/~cs8k-cyu/windows/tt_e.html)：Kenta Cho、BSD。
- [Mazer Mayhem](https://www.asahi-net.or.jp/~cs8k-cyu/xna/mm/index_e.html)：Kenta Cho、BSD。物理エンジン APE の MIT ライセンス表示を含む。

原作・ランタイム・依存ライブラリの許諾文と著作権表示を各 ZIP に同梱する。
