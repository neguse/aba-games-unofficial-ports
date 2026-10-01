# 統合リリース計画

全13作品のWeb版と、GearToyGear・Torus Trooper・Mazer MayhemのWebXR／OpenXR版を、
同じC#ソースと同じlub固定から`tools/build.py`1本でビルドし、
`main`へのマージと`v*`タグ1本で単一のワークフローが配布する状態を目指す。

## ゴール

- 開発ブランチは`main`のみ。`release/frame`は廃止する。
- Webの実行方式は全作品tcs2c（C#→C→wasm）。Lua経路は廃止する。
- VR 3作品は描画・入力・ホストをWebXRとOpenXRで共有し、`frame/`にはcsprojと起動スクリプトだけを置く。
- lubとtcsの固定は`tools/lub.lock`1箇所。ワークフロー・セットアップ・READMEはそこを参照する。
- ワークフローは`.github/workflows/release.yml`1本。PRではビルドと検証、`v*`タグでWebをCloudflare Workersへ配布し、
  Frame ZIP・installer・Web配布物をGitHub Releaseに添付する。
- バージョンはWebとFrameで共通の`v*`タグ。`frame-v*`は使わない。
- 最初の統合リリース`v0.3.0`の条件は、全13作品がtcs2cで動き、上記の全項目が揃っていること。

## 現状（2026-10-01）

- 既定ブランチは`release/frame`。`main`はその祖先で41コミット遅れ、独自コミットはない。
- ワークフローは`frame-release.yml`のみ。`release/frame`向けPRと`frame-v*`タグでFrame ZIP 3本を作る。
  WebはCIがなく、手元から`wrangler deploy`している。
- lubの固定が二重。Webは`tools/setup_lub.sh`で`58e0072`、FrameはワークフローとREADMEで`81d28ec`。
  未マージの`codex/torus-webxr-c`は両方を`5471ced`（tcs `2d551b8`）に揃えている。
- その`5471ced`はlubの`codex/shared-webxr`、tcs `2d551b8`はtcsの`codex/shared-xr-runtime`にしかなく、
  どちらもmaster未マージ（2026-10-02時点）。OpenXR・Frame ARM64・`tcs_host.c`・`web/xr.mjs`・tcs2cのゲーム対応は
  全部この側枝にある。lub masterはその後21コミット（SDL 3.4.16、WebGPU修正）、tcs masterは9コミット進んでいる。
  ローカルで試したマージの衝突はlubが`scripts/native-gate.sh`と`src/backend_webgpu.c`の3箇所、
  tcsが`Transpiler/IlExport.cs`の2箇所で、いずれも小さい。
- ゲームロジックは共有済み。差分はGTG・MMの`frame/Render.cs`・`frame/Pad.cs`、
  TTの`frame/Game.cs`・`frame/Render.cs`・`frame/Controls.cs`のみ。
  `codex/torus-webxr-c`はTTについてこれを解消し、WebXRをlubの`web/xr.mjs`へ寄せ、Web版TTをtcs2c wasmで作る。
- Webの実行方式は3系統。12作品はLua on wasm、TT（codex branch）はtcs2c wasm、FrameはCoreCLRとtcs2cネイティブ。
- Lua経路のホストは`web/main.js`がtopic（`scores.*`・`replay.*`・`music.*`・`sound.*`・`spatial.*`）を中継する。
  tcs2c経路はlubの`Audio`・`Io`を直接使い、保存は`web/compiled.js`がemscripten FSとlocalStorageを同期する。
- Mu-cadeはlubの`lua_api.c`にパッチを当てた別のlub wasmをビルドしている。tcs2cなら`OdeApi.cs`経由でC同士がつながる。
- ブラウザテストは`page.route('**/game.lua')`でLuaに観測コードを注入する。
  tcs2c経路は`tests/torus-trooper/web-host.c`のようにCのテストホストで観測する。
- `tests/*/frame`はVulkanデバイス前提でCI未実行。Webの検証はPlaywright（swiftshader）でCI実行できる。
- PR #20〜#26は取り込み済みのブランチをbaseにしたまま開いている。

## 到達形

### リポジトリ

| 項目 | 到達形 |
| --- | --- |
| ブランチ | `main`のみ。作業ブランチからPR、CI greenでマージ |
| lub固定 | `tools/lub.lock`（lubとtcsのリビジョン）。`setup_lub.sh`・`release.yml`・READMEが参照 |
| 作品一覧 | `games.json`。作品ごとに原作アーカイブとSHA-256、ソース構成、シェーダー、VRの有無を持つ |
| ビルド | `tools/build.py --target web\|frame\|all`。`build_*_frame.py`と手書きの作品別区間は廃止 |
| Web実行方式 | 全作品tcs2c wasm。lub wasmは1回静的ライブラリとしてビルドし、作品ごとに`game.c`とリンク |
| Frame実行方式 | tcs2cネイティブ（`LUB_RUNTIME=tcs2c`）とCoreCLRの両方を同梱。既定はcodex branchのまま |
| VR 3作品 | `games/<game>/`に共有の`Render.cs`・`Controls.cs`・`Host.cs`。`frame/`はcsproj・`run.sh`・slangのみ |
| 検証 | 弾幕比較・ゲーム進行はtcs2cネイティブのテストホストで実行。ブラウザ検証はPlaywright。Frameの描画テストはlavapipe（CI）と実機 |
| 文書 | README 1本。依存・ビルド・検証・配布をtarget別の節にする |

### release.yml

| job | ランナー | 内容 |
| --- | --- | --- |
| `web` | ubuntu-24.04 | emsdk・.NET・Node。lub wasmを`lub.lock`キーでキャッシュ。`build.py --target web`。弾幕比較・ゲーム進行・Playwright。`dist`をartifact |
| `frame` | ubuntu-24.04-arm | 現行`frame-release.yml`のbuild job。`build.py --target frame`。ZIP検査。ZIPとinstallerをartifact |
| `deploy` | ubuntu-24.04 | `v*`タグのみ。`dist`を`wrangler deploy`。公開URLへPlaywrightのスモーク。失敗時`wrangler rollback` |
| `release` | ubuntu-24.04 | `v*`タグのみ。`web`と`frame`の後。Frame ZIP 3本・`Install-ABA-Games.desktop`・`install_frame.py`・`web-dist.zip`を添付したReleaseを作成 |

PRと`main`へのpushでは`web`と`frame`だけ動く。Secretsは`CLOUDFLARE_API_TOKEN`と`CLOUDFLARE_ACCOUNT_ID`。
installerが参照する`releases/latest`は変えない。

## 手順

各段を1 PRにし、CI greenとFrame実機確認を通してから次へ進む。

1. **ブランチ整理**
   `main`を`release/frame`へfast-forwardし、既定ブランチを`main`にする。
   `frame-release.yml`のPRトリガーを`main`に変える。#20〜#26を取り込み済みとしてcloseし、
   マージ済みの`ci/*`・`vr/*`・`perf/*`・`fix/*`・`port/*`を削除する。
   `release/frame`は`v0.3.0`まで残し、その後削除する。READMEの開発ブランチの記述を直す。
   完了条件: `main`が既定で、PRのCIが`main`向けに動く。

2. **lub固定の一本化とTTのtcs2c化**
   先にtcsの`codex/shared-xr-runtime`をtcs masterへ、lubの`codex/shared-webxr`をlub masterへマージし、
   lubのサブモジュールをマージ後のtcsへ進める。以後の`lub.lock`はmaster上のコミットだけを指す。
   `codex/torus-webxr-c`はそのlubに合わせて仕上げてマージする。`docs/web.md`の`b1bd350`を揃える。
   `tools/lub.lock`を作り、`setup_lub.sh`・ワークフロー・READMEをそこから読む。
   lubの`audio.c`にstb_vorbisを足してoggを復号できるようにし、TTのwav変換を外す。
   lubが`58e0072`から`5471ced`へ進むので、13作品の`check_game.py`・`check_patterns.py`・ブラウザテストを全部回す。
   完了条件: lubの固定が1箇所。TTがWeb・Frameとも同じC#から動く。全作品の既存検証が通る。

3. **Web CIと単一ワークフロー**
   `release.yml`を作り、`frame-release.yml`を置き換える。`web` jobはこの時点ではLuaとtcs2cの混在をそのままビルドする。
   lub wasmのキャッシュ、Playwrightの実行、`dist` artifactまで入れる。
   `deploy`・`release` jobも書くが、`v*`タグは手順7まで打たない。
   完了条件: PRでWebとFrameの両方がビルド・検証される。

4. **lub wasmの作品別リンク**
   `compile_torus_web.py`が作品ごとにlub全体をcmakeしている構成を、lubを1回ビルドして`game.c`・`binding.c`・テストホストだけを作品ごとにリンクする形にする。
   lub側に手を入れる場合は`lub.lock`を進める。
   完了条件: 13作品分のwasmビルドがCIの時間制限に収まる。

5. **GTG・MMの統一**
   TTで確立した構成を適用する。`frame/Render.cs`・`frame/Pad.cs`をWebと共有の`Render.cs`・`Controls.cs`・`Host.cs`にまとめ、
   Web版GTG・MMにVRボタンを付ける。Web版はtcs2cにする。GTGの定位音はlubの`Audio`で再現できるか確認し、
   できなければlub側に足す。
   完了条件: 3作品ともWebXRとOpenXRが同じC#・同じslangで動き、`frame/`にC#が残らない。

6. **tcs2cの制限解消と残り10作品のtcs2c化**
   先にtcs側でジェネリックメソッド・nullable引数・オーバーロード・IL本体が空になる文の形を直し、
   13作品の検査が通ってから作品ごとに進める。1作品1 PR。各PRで次を行う。
   - `Host.Send`のtopic中継をlubの`Audio`・`Io`に置き換える（TTの`Host.cs`が雛形）。
   - `index.html`の設定を`compiled`と`saves`に変える。
   - ブラウザテストのLua注入をCのテストホストに置き換え、`check_game.py`もそのホストで実行する。
   - 弾幕比較を回し、原作との一致を確認する。
   - その作品のLua経路（`compile_game.py`の分岐・`build.py`の区間）を削除する。

   順番はMu-cade（別lubビルドが消える）、rRootage、Noiz2sa、TUMIKI Fighters、PARSEC47、Gunroar、Titanion、A7Xpg、Wok、まさしくんハイ！。
   当たりやすい箇所はまさしくんハイ！（Pascal由来の構文）、Wok（ポインターロック）、Noiz2sa（`PixelLayer`）。
   tcs2cの未対応構文はtcs側で直し、`lub.lock`を進める。
   完了条件: 13作品のWeb版が全部tcs2cで動き、既存の検証が全部通る。

7. **Lua経路の削除と`v0.3.0`**
   `compile_game.py`のLua出力、`setup_lub.sh`のlua32ビルド、`main.js`のLua分岐、`web/torus-webxr.js`の残骸を消す。
   `games.json`と`build.py --target`に統合し、`build_*_frame.py`を削除する。
   `v0.3.0`を打ち、`deploy`と`release`が動くのを確認し、公開URLとFrame実機で確認する。
   完了条件: `v0.3.0`のReleaseにFrame ZIP・installer・`web-dist.zip`が揃い、Cloudflareの配布がそのタグのビルドになっている。

8. **文書統合**
   `README.md`と`docs/web.md`を1本にし、依存表・ビルド・検証・配布をtarget別の節にする。
   `PORTING_PLAN.md`の「非公開リポジトリ」「手元で配布」など古い前提を直す。
   完了条件: 手順書どおりに手元で`build.py --target all`が通る。

9. **FrameネイティブテストのCI化**
   `frame` jobに`mesa-vulkan-drivers`と`xvfb`を入れ、`tests/*/frame`の描画テストを`xvfb-run`下でlavapipe上で実行する。
   x86_64で3作品とも通ることは確認済み。arm64ランナーで通らない項目があれば、その項目だけ実機確認に残す。

## 確認結果（2026-10-01、lub `5471ced` / tcs `2d551b8`、x86_64で実測）

### 定位音: Panで足りる

lubの`Audio`は`PlayOpts.Pan`だけで、3D定位はない。
Web版GTGのWebAudio pannerは`rolloffFactor = 0`で距離減衰なし、equalpowerの左右定位のみ。
Frame版の`games/gear-toy-gear/frame/Host.cs`が既に`spatial.*`を`Pan`へ写しているので、
Web版もその`Host.cs`を共有すれば終わる。lub側の変更は不要。

### ogg: lubは復号できない。lubにstb_vorbisを足す

`src/audio.c`はminiaudioを`MA_NO_*`付きで組み込み、`stb_vorbis`を含めていないため
`Audio.Decode`はwav・flac・mp3のみ。codex branchのTTがoggをwavへ変換して同梱しているのはこのため。
原作の音楽は44.1kHzモノラルで、wav化するとoggの約10倍になる。

| 作品 | ogg合計 | wav換算（モノラル） |
| --- | --- | --- |
| TUMIKI | 3.8MB | 24MB |
| Noiz2sa | 8.1MB | 54MB |
| その他8作品 | 2.4〜4.9MB | 16〜23MB |

13作品で約250MBの配布増になるので、lubの`audio.c`で`stb_vorbis.c`をminiaudioの前に含めて
`MA_HAS_VORBIS`を有効にする（miniaudioが用意している経路）。これは手順2のlub固定更新に含める。

### tcs2c: TT以外の12作品は最初のエラーで止まる。原因は4種で、tcs側の対応が先

13作品の`compile_game.py`のソース一覧（VR 3作品は`--frame`）に`tcs2c --lib`を掛けた結果。
tcs2cは最初のエラーで止まるので、各作品の先頭の障害だけが分かっている。

| 作品 | 結果 | 先頭の障害 |
| --- | --- | --- |
| Torus Trooper | 成功（6.5MBのC、gccで通る） | |
| TUMIKI、PARSEC47 | 失敗 | `game/Core.cs`の`Vector(float? x = null, float? y = null)`。nullable引数をf32へ代入できない |
| Gunroar | 失敗 | `Boat.setReplayMode`のオーバーロード。tcs2cはクラス内の同名メソッドを拒否する |
| Titanion、A7Xpg | 失敗 | `EnemySpec.gotoNextPhaseInAppearing`・`Bonus.set`のIL本体が空。IL exportが落とす文の形がある |
| rRootage、Noiz2sa、Wok、MM、GTG、Mu-cade、まさしくんハイ！ | 失敗 | `Arrays.Make<T>(int, Func<T>)`（Mu-cadeは`Append<T>`も）。ジェネリックメソッド未対応 |

4種とも作品固有ではなくtcs2cの制限なので、作品ごとに回避するより先にtcs側で対応する。
ジェネリックメソッド（7作品）、nullable引数（2作品）、オーバーロードの名前付け（Lua経路の`_1`接尾辞と同じ規則）、
IL本体が空になる文の形（`for (dig = 0; …)`の既存変数初期化、`case`直下のローカル宣言のいずれか）。
対応後に同じ検査を回し、次の障害を潰す。手順6はこのtcs作業の後に始める。
検査は`tcs2c --lib --ref cs-lib/lub_stub.cs <sources> -o game.c`と、
`src/tcs_host.c`を`LUB_TCS_GAME`付きで`gcc -c`することで、lubのネイティブビルドなしに行える。

### lavapipe: Frameの描画テストは3作品ともCPU Vulkanで通る

`mesa-vulkan-drivers`と`xvfb`を入れ、`VK_ICD_FILENAMES`でlavapipeを指定し、
`LUB_NATIVE_LIB`にx86_64の`liblub.so`を渡して`xvfb-run`下で
`tests/{torus-trooper,gear-toy-gear,mazer-mayhem}/frame/FrameTests.csproj`を実行した。
GPU readbackを含む全項目がPASS。lubのCIも同じ構成でVulkanバックエンドの検証をしている。
手順9は任意ではなく`frame` jobに入れる。arm64ランナーでも`mesa-vulkan-drivers`はlavapipeを含むが、
実測はx86_64のみ。lubのネイティブビルドは依存のapt導入込みで約25分なので、
`lub.lock`をキーにビルド成果物をキャッシュする。
