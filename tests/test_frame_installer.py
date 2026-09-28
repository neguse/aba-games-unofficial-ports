import hashlib
import importlib.util
import io
import json
import os
from pathlib import Path
import struct
import tempfile
import unittest
from unittest.mock import patch
from zipfile import ZipFile, ZipInfo

spec = importlib.util.spec_from_file_location('installer', Path(__file__).parents[1] / 'tools/install_frame.py')
installer = importlib.util.module_from_spec(spec)
spec.loader.exec_module(installer)


def shortcuts(entries):
    data = b'\0Shortcuts\0'
    for index, command in enumerate(entries):
        data += b'\0' + str(index).encode() + b'\0'
        data += b'\x02appid\0' + struct.pack('<I', 0x80000000 + index)
        data += b'\x01AppName\0Game\0\x01Exe\0"' + str(command).encode() + b'"\0\0tags\0\x08\x08'
    return data + b'\x08\x08'


def archive(game):
    output = io.BytesIO()
    with ZipFile(output, 'w') as zip_:
        for name in [game, game + '.dll', 'run.sh', 'Lub.dll', 'native/lub', 'native/liblub.so', 'LICENSE.txt']:
            info = ZipInfo(game + '/' + name)
            info.external_attr = 0o100755 << 16
            zip_.writestr(info, 'payload')
    return output.getvalue()


class InstallerTests(unittest.TestCase):
    def test_steam_registration_case_and_unsigned_id(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'shortcuts.vdf'
            command = Path(directory) / 'game folder/run.sh'
            path.write_bytes(shortcuts([command]))
            self.assertTrue(installer.registered(path, command))
            self.assertFalse(installer.registered(path, command.parent / 'other.sh'))
            self.assertEqual(installer.read_shortcuts(path)[0]['appid'], 0x80000000)
            path.write_bytes(b'\0shortcuts\0\x01broken')
            with self.assertRaises(ValueError):
                installer.read_shortcuts(path)

    def test_zip_permissions_and_path_escape(self):
        with tempfile.TemporaryDirectory() as directory:
            stage = Path(directory)
            installer.extract(io.BytesIO(archive('GearToyGear')), stage, 'GearToyGear')
            self.assertTrue(os.access(stage / 'GearToyGear/run.sh', os.X_OK))
            for name, mode in [('GearToyGear/../../escape', 0o100644), ('/escape', 0o100644), ('GearToyGear/link', 0o120777)]:
                output = io.BytesIO()
                with ZipFile(output, 'w') as zip_:
                    info = ZipInfo(name)
                    info.external_attr = mode << 16
                    zip_.writestr(info, 'bad')
                with self.assertRaises(ValueError):
                    installer.extract(io.BytesIO(output.getvalue()), stage, 'GearToyGear')
            self.assertFalse((stage.parent / 'escape').exists())

    def test_install_update_and_failed_download(self):
        with tempfile.TemporaryDirectory() as directory:
            data = Path(directory)
            path = data / 'shortcuts.vdf'
            path.write_bytes(shortcuts([]))
            saves = data / 'gear-toy-gear/scores.txt'
            saves.parent.mkdir()
            saves.write_text('keep score')
            payloads = {game: archive(game) for game in installer.GAMES}
            assets = [{'name': game + '-steam-frame-arm64.zip', 'browser_download_url': game,
                       'size': len(payload), 'digest': 'sha256:' + hashlib.sha256(payload).hexdigest()}
                      for game, payload in payloads.items()]
            release = {'draft': False, 'prerelease': False, 'assets': assets}
            registered = []

            def open_url(url, **kwargs):
                return io.BytesIO(json.dumps(release).encode() if not isinstance(url, str) else payloads[url])

            def register(command, **kwargs):
                self.assertEqual(command[0], 'steamos-add-to-steam')
                launcher = Path(command[1])
                game = launcher.stem.removeprefix('aba-')
                registered.append(data / 'aba-games-unofficial-ports' / game / 'run.sh')
                path.write_bytes(shortcuts(registered))

            with patch.object(installer.urllib.request, 'urlopen', side_effect=open_url), patch.object(installer.subprocess, 'run', side_effect=register):
                installer.install(data, path, lambda *args: None)
                self.assertEqual(len(registered), 3)
                target = data / 'aba-games-unofficial-ports/GearToyGear/run.sh'
                target.write_text('old version')
                installer.install(data, path, lambda *args: None)
                self.assertEqual(len(registered), 3)
                self.assertEqual(target.read_text(), 'payload')
                self.assertEqual((data / 'aba-games-unofficial-ports/.previous/GearToyGear/run.sh').read_text(), 'old version')
                target.write_text('keep installed')
                payloads['MazerMayhem'] = b'broken download'
                with self.assertRaises(ValueError):
                    installer.install(data, path, lambda *args: None)
                self.assertEqual(target.read_text(), 'keep installed')
                self.assertEqual(saves.read_text(), 'keep score')

    def test_current_steam_account(self):
        with tempfile.TemporaryDirectory() as directory:
            home = Path(directory)
            config = home / '.local/share/Steam/config'
            config.mkdir(parents=True)
            (config / 'loginusers.vdf').write_text('"users" { "76561197960265729" { "timestamp" "10" } "76561197960265730" { "timestamp" "20" } }')
            with patch.object(installer.Path, 'home', return_value=home):
                self.assertEqual(installer.shortcuts_path().parent.parent.name, '2')


if __name__ == '__main__':
    unittest.main()
