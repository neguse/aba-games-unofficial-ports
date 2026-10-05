"""The desktop Host mappings must exactly match each game's browser protocol."""
import ast
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]


class HostProfiles(unittest.TestCase):
    def test_browser_profile_parity(self):
        profiles = json.loads((ROOT / 'native/HostProfiles.json').read_text())
        self.assertEqual(13, len(profiles))
        for page in ROOT.glob('games/*/index.html'):
            match = re.search(r'<script id="game-config" type="application/json">(.*?)</script>', page.read_text(), re.S)
            if not match:
                continue
            browser = json.loads(match[1])
            expected = {k: v for k, v in browser.items() if k not in ('wasm', 'webxr')}
            self.assertEqual(expected, profiles[page.parent.name], page.parent.name)

    def test_tumiki_defaults_match_browser(self):
        profile = json.loads((ROOT / 'native/HostProfiles.json').read_text())['tumiki']
        source = (ROOT / 'web/main.js').read_text()
        for field, variable in [('music', 'musicNames'), ('sounds', 'soundNames'), ('channels', 'soundChannels')]:
            match = re.search(r'const ' + variable + r' = config\.' + field + r' \|\| (\[.*?\]);', source, re.S)
            self.assertIsNotNone(match, variable)
            self.assertEqual(ast.literal_eval(match[1]), profile[field], field)
        controls = re.search(r'const controls = new Map\(config.controls \|\| (\[.*?\])\);', source, re.S)
        self.assertIsNotNone(controls)
        self.assertEqual(ast.literal_eval(controls[1]), profile['controls'])

    def test_no_native_gameplay_changes(self):
        """Only native desktop input paths mention the additional hardware APIs."""
        for path in ['games/gear-toy-gear/Pad.cs', 'games/mazer-mayhem/Pad.cs', 'games/torus-trooper/Controls.cs']:
            source = (ROOT / path).read_text()
            self.assertIn('public static void ReadDesktop', source)
            self.assertIn('Lub.Input.GamepadConnected', source)


if __name__ == '__main__':
    unittest.main()
