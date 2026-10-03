# CLAUDE.md

## 決定事項

- web 版は全作品を tcs2c（C → wasm）で実行する。TCS → Lua の web 経路は残さない。
- OpenXR 版と web 版は同じ C# ソースとシェーダーからビルドする。
  `games/<作品>/frame/` に置くのはネイティブ起動とビルド設定だけ。
- ABA Games の原作実装を尊重する。描画・合成・ルール・音で迷ったら原作ソースを正とし、
  移植側の都合（共通化など）で挙動を変えない。更新レートを上げる可変フレーム時間は、原作の尊重に反しない。
- 画像などのアセットはソースコードに埋め込まない。ファイルとして配布し、lub の API（`Png.Load` など）で読み込む。
- lub と tcs は各リポジトリの既定ブランチの最新コミットに pin する。
  tcs は lub のサブモジュール経由で pin する。

## リリース条件

- 1 回のリリースで web と OpenXR（Steam Frame）の両方をビルド・デプロイする。
- 本リリースの前に rc を出し、rc の配布物で確認してから本リリースにする。
- タグ `frame-vX.Y.Z-rcN` が rc（GitHub の prerelease と、本番へ切り替えない web のプレビュー版）、
  `frame-vX.Y.Z` が本リリース（GitHub の Release と web の本番デプロイ）。`.github/workflows/frame-release.yml` が行う。
- 受け入れ条件は同ワークフローの `build`（Frame ZIP の検査）と `web`（全作品のビルド、`check_game.py`、`check_patterns.py`）の成功。
  ブラウザテストと実機確認は rc の配布物で行う。
- 戻し方: web は `wrangler rollback` で前のバージョンへ、Frame は前の Release を案内する。
