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
| 検証 | 弾幕比較・ゲーム進行はtcs2cネイティブのテストホストで実行。ブラウザ検証はPlaywright。Frameの描画テストは実機または任意でlavapipe |
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
   `codex/torus-webxr-c`を仕上げてマージする。`docs/web.md`の`b1bd350`を揃える。
   `tools/lub.lock`を作り、`setup_lub.sh`・ワークフロー・READMEをそこから読む。
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

6. **残り10作品のtcs2c化**
   1作品1 PR。各PRで次を行う。
   - `Host.Send`のtopic中継をlubの`Audio`・`Io`に置き換える（TTの`Host.cs`が雛形）。
   - `index.html`の設定を`compiled`と`saves`に変える。
   - ブラウザテストのLua注入をCのテストホストに置き換え、`check_game.py`もそのホストで実行する。
   - 弾幕比較を回し、原作との一致を確認する。
   - その作品のLua経路（`compile_game.py`の分岐・`build.py`の区間）を削除する。

   順番はMu-cade（別lubビルドが消える）、rRootage、Noiz2sa、TUMIKI Fighters、PARSEC47、Gunroar、Titanion、A7Xpg、Wok、まさしくんハイ！。
   当たりやすい箇所はまさしくんハイ！（Pascal由来の構文）、Wok（ポインターロック）、Noiz2sa（`PixelLayer`）、
   音源のogg（tcs2c経路はwavへ変換して同梱するため配布サイズが増える。lubの`Audio.Decode`がoggを扱えるなら変換を止める）。
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

9. **Frameネイティブテストの CI 化（任意）**
   arm64ランナーに`mesa-vulkan-drivers`（lavapipe）を入れ、`tests/*/frame`の描画テストを`--backend vulkan`で試す。
   通れば`frame` jobに入れ、通らなければ実機確認のままにする。

## 確認が必要な点

- lubの`Audio`に定位（GTGの`spatial.*`相当）があるか。
- lubの`Audio.Decode`がoggを扱えるか。扱えなければwav同梱で配布サイズが増える。
- tcs2cが13作品の構文をどこまで扱えるか。手順6の各PRで判明する。
- lavapipeでlubのVulkanバックエンドが動くか。手順9で判明する。
