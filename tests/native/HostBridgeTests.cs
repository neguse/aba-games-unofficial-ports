using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aba.Native;

static class Test
{
    static int checks;
    static void Equal<T>(T expected, T actual, string name)
    {
        checks++;
        if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"{name}: expected {expected}, got {actual}");
    }
    public static void Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "aba-host-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            string[] ids = { "tumiki", "parsec47", "gunroar", "titanion", "a7xpg", "torus-trooper", "rrootage", "noiz2sa", "wok", "mazer-mayhem", "gear-toy-gear", "mu-cade", "masashikun-hi" };
            foreach (string id in ids)
            {
                var profile = HostProfile.Load(id);
                Equal(id, profile.Id, "profile " + id);
                Equal(profile.Sounds.Length, profile.Channels.Length, "channel count " + id);
                foreach (var (key, mask) in profile.Controls)
                    Equal(mask, profile.KeyboardMask(k => k == key), $"binding {id} {key}");
            }
            Equal(1, HostProfile.Load("tumiki").KeyboardMask(k => k == "w"), "standard WASD");
            Equal(256, HostProfile.Load("gunroar").KeyboardMask(k => k == "w"), "Gunroar left twin-stick");
            Equal(4096, HostProfile.Load("gunroar").KeyboardMask(k => k == "i"), "Gunroar right twin-stick");
            Equal(16, HostProfile.Load("mu-cade").KeyboardMask(k => k == "i"), "Mu-cade right twin-stick");
            Equal(256, HostProfile.Load("mu-cade").KeyboardMask(k => k == "z"), "Mu-cade primary");
            Equal("Keypad 8", HostProfile.NativeKey("Numpad8"), "keypad alias");
            Equal("left ctrl", HostProfile.NativeKey("ControlLeft"), "control alias");
            Equal(0, NativePadState.DirectionMask(.34f, -.34f), "deadzone");
            Equal(9, NativePadState.DirectionMask(1, 1), "up right");
            var pad = new NativePadState { LeftX = -1, LeftY = 1, RightX = 1, RightY = -1, Primary = true, Secondary = true, Start = true, Back = true };
            Equal(5 | 10 << 4 | 256 | 512 | 1024 | 2048, pad.MaskFor("mu-cade"), "Mu-cade pad protocol");
            Equal(5 | 5 << 8 | 10 << 12 | 16 | 32 | 64 | 128, pad.MaskFor("gunroar"), "Gunroar pad protocol");
            Equal(5 | 16 | 32 | 64 | 128, pad.MaskFor("tumiki"), "standard pad protocol");
            Equal(128, pad.MaskFor("wok"), "Wok has pointer actions");
            var a = new NativeGameStorage("tumiki", root); var b = new NativeGameStorage("parsec47", root);
            Equal("", a.Load("scores"), "missing score");
            a.Save("scores", "100\tAlice\n"); b.Save("scores", "200\tBob\n"); a.Save("replay", "version:1\nframes");
            Equal("100\tAlice\n", a.Load("scores"), "score round trip");
            Equal("200\tBob\n", b.Load("scores"), "pergame isolation");
            Equal("version:1\nframes", a.Load("replay"), "replay round trip");
            a.Save("scores", "new"); Equal("new", a.Load("scores"), "atomic overwrite");
            Equal(0, Directory.GetFiles(a.DirectoryPath, "*.tmp").Length, "no temp leak");
            foreach (string id in new[] { "torus-trooper", "mazer-mayhem", "gear-toy-gear" })
                Equal(Path.Combine(root, id), new NativeGameStorage(id, root).DirectoryPath, "legacy native save path " + id);
            bool blocked = false; try { _ = new NativeGameStorage("../escape", root); } catch (ArgumentException) { blocked = true; }
            Equal(true, blocked, "path traversal blocked");
            using (var bridge = new HostBridge("gunroar", root, root))
            {
                bridge.Install(); Equal(true, Lub.Host.Available(), "managed Host available");
                bridge.Send("scores.save", "native score"); bridge.Send("scores.load", "");
                Equal(("scores", "native score"), bridge.Poll(), "Host storage protocol");
                bridge.Send("replay.save", "test replay"); bridge.Send("replay.load", "");
                Equal(("replay", "test replay"), bridge.Poll(), "Host replay protocol");
                bridge.Send("ready", ""); var seed = bridge.Poll();
                Equal("seed", seed.topic, "seed topic"); Equal(true, int.TryParse(seed.payload, out int n) && n >= 0, "seed range");
                bridge.Send("sound.play", "999999"); bridge.Send("music.loop", "-1"); bridge.Send("sound.stop", "bad");
                Equal(null, bridge.LastError, "invalid audio commands safely ignored");
                bridge.Send("quit", ""); Equal(true, bridge.QuitRequested, "quit returns to selector");
            }
            Equal(null, Lub.Session.HostSending, "Host send callback detached"); Equal(null, Lub.Session.HostPolling, "Host poll callback detached");
            var messages = new List<(string, string)>();
            var panel = new MasashikunPanel((topic, payload) => messages.Add((topic, payload)));
            for (int n = 0; n <= 6; n++) { panel.Start(n); Equal(("start", n.ToString()), messages[^1], "Mas event " + n); }
            panel.OnMessage("state", "2,1"); Equal(true, panel.CaptureMouse, "Mas capture while playing");
            panel.ShowMenu(); Equal(("pause", ""), messages[^1], "opening Mas controls uses game pause"); Equal(false, panel.CaptureMouse, "menu releases mouse");
            panel.OnMessage("ranking", "Super Tobibako\n2\n100\tAAA\n90\tBBB\n80\tCCC\n");
            Equal(true, panel.RankingOpen, "ranking shown"); Equal(2, panel.PendingRank, "pending rank");
            panel.SetName("Test\tName\n"); panel.CloseRanking(); Equal(("name", "Test Name "), messages[^1], "native name accepted via game protocol");
            panel.OnMessage("ranking", "Overall\n0\n1\tA\n2\tB\n3\tC\n");
            panel.ShowRanking(5); Equal(("rank", "5"), messages[^1], "category request");
            panel.CloseRanking(); Equal(false, panel.RankingOpen, "ranking closes");
            panel.ShowMenu(); var canvas = new Canvas(); panel.Draw(canvas); Equal(true, canvas.Draws > 10, "panel renders");
            Equal(true, panel.HandlePointer(140, 164, true), "native event button hit"); Equal(("start", "0"), messages[^1], "native event click");
            Console.WriteLine($"PASS: {checks} Host bridge, input, save isolation, and Masashikun panel checks");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    sealed class Canvas : INativeCanvas
    {
        public int Draws;
        public void Text(string text, float x, float y, float size, uint rgba) => Draws++;
        public void Rect(float x, float y, float w, float h, uint rgba) => Draws++;
    }
}
