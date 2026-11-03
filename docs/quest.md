# Meta Quest 版

GearToyGear・Torus Trooper・Mazer Mayhemを、Meta Quest（Quest 2以降）のネイティブアプリとして動かす。
ゲーム・描画・操作のC#とシェーダーはweb版・Steam Frame版と同じものを使う。
C#をtcs2cでCへ変換し、lubのAndroidのplayer（OpenXRとVulkan）とリンクしたAPKを作品ごとに作る。

lubのAndroid対応は[lubのdocs/android.md](https://github.com/neguse/lub/blob/master/docs/android.md)を参照。
Quest版はリリース条件（web版とSteam Frame版）に含まれない。

## 操作

Steam Frame版と同じ。Questのコントローラー（Touch）の左右のボタンに次のように対応する。

| Steam Frame | Quest |
| --- | --- |
| A | A（右） |
| B | B（右） |
| X | X（左） |
| Menu | メニュー（左） |

右のメニューボタン（Metaボタン）はシステムが使う。

## ビルド

Linux x64、.NET SDK 10、Python 3、FFmpeg、CMake、Ninja、zip、JDK、
Android SDK（build-tools 36とplatform 35）とNDK（r27で確認）を使用する。
`ANDROID_HOME`にAndroid SDKを指定する。
shaderのcacheを作るため、同じlubのLinux版（`scripts/build-release.sh`の`build-release-linux/lub`）とtcsのTranspilerも先にビルドしておく。

```sh
python3 tools/build_quest.py --lub ../lub --game gear-toy-gear \
  --original .cache/original/GearToyGear/GearToyGear
python3 tools/build_quest.py --lub ../lub --game torus-trooper --original .cache/original/tt
python3 tools/build_quest.py --lub ../lub --game mazer-mayhem --original .cache/original/Mm/Mm
```

出力は`build/quest/<作品>-quest.apk`。
原作の展開先はweb版と同じ（[docs/web.md](web.md)）。

`tools/build_quest.py`は次の順に処理する。

1. 原作のデータを変換し、`tools/compile_frame_c.py`と同じ手順でC#をCにする。
2. シェーダー、音（web版と同じ変換）、`LICENSE.txt`をAPKのデータにする。
3. Androidのplayerはシェーダーのコンパイラを持たないので、各作品の`Gfx.UseShader`に渡すシェーダーの組を列挙した小さなC#を作り、`lub export android`でshaderのcacheにする。
   組は`tools/build_quest.py`の`games`にある。ゲームのシェーダーの使い方を変えたら合わせて直す。
4. lubの`scripts/build-android.sh`でAPKを組み、署名する。署名の鍵は`--keystore`で指定でき、無ければデバッグ用の鍵を使う。

## Questへの導入

1. Questを開発者モードにする（Meta Horizonアプリの設定）。
2. PCとUSBでつなぎ、`adb install -r build/quest/GearToyGear-quest.apk`のように入れる。
3. Questのアプリ一覧の「提供元不明」から起動する。

記録は`save/<作品>/`（アプリの内部ストレージ）に保存し、アプリを更新しても残る。
アプリを削除すると消える。
動作の記録は`adb logcat -s SDL SDL/APP`で見る。
