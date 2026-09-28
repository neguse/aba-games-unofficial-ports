import fcntl
import hashlib
import io
import json
import os
from pathlib import Path, PurePosixPath
import platform
import re
import shutil
import stat
import struct
import subprocess
import tempfile
import time
import urllib.request
from zipfile import ZipFile

REPO = 'neguse/aba-games-unofficial-ports'
GAMES = {'GearToyGear': 'GearToyGear', 'TorusTrooper': 'Torus Trooper', 'MazerMayhem': 'Mazer Mayhem'}


def read_shortcuts(path):
    if not path.exists():
        return []
    stream = io.BytesIO(path.read_bytes())

    def string():
        value = bytearray()
        while True:
            char = stream.read(1)
            if not char:
                raise ValueError('Steamの登録情報を読み取れませんでした。')
            if char == b'\0':
                return value.decode('utf-8')
            value.extend(char)

    def object_():
        result = {}
        while True:
            kind = stream.read(1)
            if kind == b'\x08':
                return result
            if kind not in (b'\0', b'\x01', b'\x02'):
                raise ValueError('Steamの登録情報を読み取れませんでした。')
            key = string().lower()
            result[key] = object_() if kind == b'\0' else string() if kind == b'\x01' else struct.unpack('<I', stream.read(4))[0]

    return list(object_().get('shortcuts', {}).values())


def shortcuts_path():
    for steam in (Path.home() / '.steam/steam', Path.home() / '.local/share/Steam'):
        login = steam / 'config/loginusers.vdf'
        if not login.exists():
            continue
        users = []
        for user, block in re.findall(r'"(\d{17})"\s*\{([^{}]*)\}', login.read_text()):
            timestamp = re.search(r'"timestamp"\s*"(\d+)"', block, re.I)
            recent = bool(re.search(r'"MostRecent"\s*"1"', block, re.I))
            users.append((recent, int(timestamp[1]) if timestamp else 0, int(user) & 0xffffffff))
        if users:
            return steam / 'userdata' / str(max(users)[2]) / 'config/shortcuts.vdf'
    raise RuntimeError('Steamにログインしてから、もう一度実行してください。')


def registered(shortcuts, command):
    return any(entry.get('exe', '').strip('"') == str(command) for entry in read_shortcuts(shortcuts))


def extract(archive, destination, game):
    with ZipFile(archive) as zip_:
        names = set()
        for entry in zip_.infolist():
            name = PurePosixPath(entry.filename)
            mode = entry.external_attr >> 16
            if name.is_absolute() or '..' in name.parts or not name.parts or name.parts[0] != game or stat.S_ISLNK(mode) or entry.filename in names:
                raise ValueError('配布ZIPに不正なパスがあります。')
            names.add(entry.filename)
            target = destination.joinpath(*name.parts)
            if entry.is_dir():
                target.mkdir(parents=True, exist_ok=True)
            else:
                target.parent.mkdir(parents=True, exist_ok=True)
                with zip_.open(entry) as source, target.open('wb') as output:
                    shutil.copyfileobj(source, output)
                target.chmod(mode & 0o777)
    for name in (game, game + '.dll', 'run.sh', 'Lub.dll', 'native/lub', 'native/liblub.so', 'LICENSE.txt'):
        if not (destination / game / name).is_file():
            raise ValueError('配布ZIPに必要なファイルがありません: ' + name)
    for name in (game, 'run.sh', 'native/lub'):
        if not os.access(destination / game / name, os.X_OK):
            raise ValueError('配布ZIPの実行権限がありません: ' + name)


def download(asset, destination, progress, start, span):
    digest = hashlib.sha256()
    received = 0
    with urllib.request.urlopen(asset['browser_download_url'], timeout=30) as source, destination.open('wb') as output:
        while data := source.read(1024 * 1024):
            output.write(data)
            digest.update(data)
            received += len(data)
            progress(start + span * min(1, received / max(1, asset['size'])))
    if received != asset['size'] or 'sha256:' + digest.hexdigest() != asset['digest']:
        raise ValueError('ダウンロードの検証に失敗しました。もう一度実行してください。')


def desktop(command, name):
    escaped = str(command).replace('\\', '\\\\\\\\').replace('"', '\\\\"').replace('$', '\\\\$').replace('`', '\\\\`').replace('%', '%%')
    return f'[Desktop Entry]\nType=Application\nName={name}\nExec="{escaped}"\nTerminal=false\nIcon=applications-games\nCategories=Game;\n'


def install(data_home, shortcuts, progress):
    root = data_home / 'aba-games-unofficial-ports'
    root.mkdir(parents=True, exist_ok=True)
    with (root / '.install.lock').open('w') as lock:
        try:
            fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        except BlockingIOError:
            raise RuntimeError('別のインストーラが実行中です。') from None
        for proc in Path('/proc').iterdir():
            try:
                if (proc / 'comm').read_text().strip() in GAMES:
                    raise RuntimeError('ゲームを終了してから、もう一度実行してください。')
            except (OSError, PermissionError):
                pass
        read_shortcuts(shortcuts)
        request = urllib.request.Request(f'https://api.github.com/repos/{REPO}/releases/latest', headers={'User-Agent': 'ABA-Games-Installer'})
        with urllib.request.urlopen(request, timeout=30) as response:
            release = json.load(response)
        if release['draft'] or release['prerelease']:
            raise ValueError('正式な配布版を取得できませんでした。')
        assets = {asset['name']: asset for asset in release['assets']}
        with tempfile.TemporaryDirectory(dir=root) as temporary:
            stage = Path(temporary)
            for index, (game, name) in enumerate(GAMES.items()):
                progress(index * 30, name + ' をダウンロードしています')
                asset = assets[game + '-steam-frame-arm64.zip']
                archive = stage / (game + '.zip')
                download(asset, archive, progress, index * 30, 28)
                extract(archive, stage, game)
            previous = root / '.previous'
            previous.mkdir(exist_ok=True)
            updated = []
            try:
                for game in GAMES:
                    target, old = root / game, previous / game
                    if old.exists():
                        shutil.rmtree(old)
                    existed = target.exists()
                    if existed:
                        target.rename(old)
                    try:
                        (stage / game).rename(target)
                    except BaseException:
                        if existed:
                            old.rename(target)
                        raise
                    updated.append((game, existed))
            except BaseException:
                for game, existed in reversed(updated):
                    shutil.rmtree(root / game)
                    if existed:
                        (previous / game).rename(root / game)
                raise
        applications = data_home / 'applications'
        applications.mkdir(parents=True, exist_ok=True)
        for index, (game, name) in enumerate(GAMES.items()):
            progress(91 + index * 3, name + ' をSteamに登録しています')
            command = root / game / 'run.sh'
            launcher = applications / ('aba-' + game + '.desktop')
            launcher.write_text(desktop(command, name))
            launcher.chmod(0o755)
            if registered(shortcuts, command):
                continue
            subprocess.run(['steamos-add-to-steam', str(launcher)], check=True, capture_output=True)
            deadline = time.monotonic() + 15
            while not registered(shortcuts, command):
                if time.monotonic() >= deadline:
                    raise RuntimeError(name + ' のSteam登録を確認できませんでした。Steamを開いて再実行してください。')
                time.sleep(.2)
        progress(100)


def main():
    window = None
    try:
        if platform.machine() != 'aarch64' or os.geteuid() == 0:
            raise RuntimeError('Steam Frame本体で、通常のユーザーとして実行してください。')
        if not shutil.which('steamos-add-to-steam') or subprocess.run(['pgrep', '-x', 'steam'], stdout=subprocess.DEVNULL).returncode:
            raise RuntimeError('Steamを起動してログインしてから実行してください。')
        shortcuts = shortcuts_path()
        window = subprocess.Popen(['zenity', '--progress', '--title=ABA Games', '--text=3ゲームを準備しています', '--percentage=0', '--auto-close', '--width=480'], stdin=subprocess.PIPE, text=True)

        def progress(percent, text=None):
            if window.poll() is not None:
                raise InterruptedError()
            if text:
                window.stdin.write('# ' + text + '\n')
            window.stdin.write(str(int(percent)) + '\n')
            window.stdin.flush()

        install(Path(os.environ.get('XDG_DATA_HOME', str(Path.home() / '.local/share'))), shortcuts, progress)
        window.stdin.close()
        window.wait(timeout=10)
    except (InterruptedError, BrokenPipeError):
        return
    except Exception as error:
        if window and window.poll() is None:
            window.terminate()
            window.wait()
        subprocess.run(['zenity', '--error', '--title=ABA Games', '--text=' + str(error), '--width=480'])
        return
    subprocess.run(['zenity', '--info', '--title=ABA Games', '--text=インストールが完了しました。Steamライブラリから3ゲームを起動できます。\n更新するときも、この導入ファイルを開いてください。', '--width=480'])


if __name__ == '__main__':
    main()
