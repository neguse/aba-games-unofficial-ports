using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aba.Native;
using static Lub;

static class ProtocolTests
{
    static int checks;
    static void Assert(bool condition, string text) { checks++; if (!condition) throw new Exception(text); }
    static List<(string? topic, string? payload)> Drain(HostBridge bridge)
    { var result = new List<(string?, string?)>(); while (true) { var value = bridge.Poll(); if (value.topic == null) return result; result.Add(value); } }
    static void Frame(HostBridge bridge, float dt = .016f, bool focused = true)
    { Audio.Voices.Clear(); bridge.BeforeFrame(dt, focused); bridge.AfterFrame(dt, focused); }
    public static void Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "aba-protocol-" + Guid.NewGuid().ToString("N"));
        try
        {
            using (var bridge = new HostBridge("tumiki", root, root))
            {
                bridge.Send("sound.play", "0"); Frame(bridge); Assert(Audio.Voices.Count == 0, "pending audio is not played");
                bridge.Send("sound.stop", "0"); Io.ReadyFiles.Add("ship_shot.wav"); Frame(bridge); Assert(Audio.Voices.Count == 0, "stopping cancels pending audio");
                Io.ReadyFiles.Add("stuck_bonus.wav"); Io.ReadyFiles.Add("ship_destroyed.wav");
                bridge.Send("sound.play", "2"); bridge.Send("sound.play", "4"); Frame(bridge);
                Assert(Audio.Voices.Count == 1 && Audio.Voices[0].Sound.Contains("ship_destroyed"), "newest effect on shared channel wins");
                string key = Audio.Voices[0].Key; bridge.Send("sound.play", "4"); Frame(bridge);
                Assert(Audio.Voices[0].Key != key, "replaying same sound retriggers");
                bridge.Send("sound.stop", "2"); Frame(bridge); Assert(Audio.Voices.Count == 0, "stop resolves mapped shared channel");
                Io.ReadyFiles.Add("we_are_tumiki_fighters.ogg");
                bridge.Send("music.loop", "0"); Frame(bridge); Assert(Audio.Voices.Count == 1 && Audio.Voices[0].Loop, "music loops");
                bridge.Send("music.fade", ""); Frame(bridge, .64f); Frame(bridge, .64f);
                Assert(Math.Abs(Audio.Voices[0].Volume - .5f) < .001f, "browser 1.28s fade duration");
                Frame(bridge); Assert(Audio.Voices.Count == 0, "fade stops voice");
                bridge.Send("music.once", "0"); Frame(bridge); Assert(!Audio.Voices[0].Loop, "one-shot music does not loop");
                bridge.Send("music.stop", ""); Frame(bridge); Assert(Audio.Voices.Count == 0, "music stop");
                Input.Keys.Add("z"); Frame(bridge); Assert(Drain(bridge).Contains(("input", "16")), "keyboard input enters unchanged protocol");
                Frame(bridge, .016f, false); Assert(Drain(bridge).Contains(("input", "0")), "focus loss releases keyboard");
                Input.Clear();
            }
            using (var bridge = new HostBridge("gunroar", root, root))
            {
                Input.X = 480; Input.Y = 360; Input.Dx = 1; Input.Mouse.Add(1);
                bridge.BeforeFrame(.016f, true, 0, 0, 960, 720);
                Assert(Drain(bridge).Contains(("pointer", "320,240,1")), "Gunroar scales desktop pointer to original 640x480");
                Input.Clear();
            }
            using (var bridge = new HostBridge("wok", root, root))
            {
                bridge.Send("state", "1"); Input.Dx = 10; Input.Dy = -5; Frame(bridge);
                Assert(Drain(bridge).Contains(("pointer", "350,225,0")), "Wok original relative pointer sensitivity");
                Assert(Input.Captured, "Wok captures cursor during play");
                Input.Dx = Input.Dy = 0; Frame(bridge, .016f, false);
                Assert(Drain(bridge).Contains(("leave", "")), "Wok focus loss returns to title");
                Assert(!Input.Captured, "Wok focus loss releases capture");
                bridge.Send("state", "0"); Input.Clear(); Input.Clicks.Add(1); Frame(bridge);
                var clicks = Drain(bridge);
                Assert(clicks.Any(x => x.topic == "pointer" && x.payload!.EndsWith(",1")) && clicks.Any(x => x.topic == "pointer" && x.payload!.EndsWith(",0")), "Wok preserves click completed between frames");
                Input.Clear();
            }
            using (var bridge = new HostBridge("masashikun-hi", root, root))
            {
                var panel = bridge.Masashikun!; panel.Start(1); bridge.Send("state", "1,1"); Frame(bridge); Drain(bridge);
                Input.Clicks.Add(1); Frame(bridge); var quick = Drain(bridge);
                Assert(quick.Contains(("button", "1")) && quick.Contains(("button", "0")), "Mas preserves a quick click pulse"); Input.Clicks.Clear();
                Input.Keys.Add("space"); Frame(bridge); Assert(Drain(bridge).Contains(("button", "1")), "Mas Space action");
                bridge.Send("ranking", "Record\n1\n123\tName\n0\t\n0\t\n");
                Assert(Drain(bridge).Contains(("button", "0")), "automatic ranking releases held button");
                panel.CloseRanking(); Input.Keys.Clear(); Frame(bridge); Drain(bridge);
                Input.Pressed.Add("left shift"); Frame(bridge); Assert(Drain(bridge).Contains(("shift", "")), "Mas original Shift movement");
                Input.Pressed.Clear(); Frame(bridge, .016f, false); Assert(Drain(bridge).Contains(("blur", "")), "Mas focus loss uses original pause path");
                Input.Clear();
                panel.OnMessage("ranking", "Overall\n0\n0\tA\n0\tB\n0\tC\n"); Drain(bridge);
                Input.Pressed.Add("delete"); panel.Update(.016f, true, new());
                Assert(!Drain(bridge).Any(x => x.topic == "clear"), "clear scores requires confirmation");
                Input.Pressed.Clear(); Input.Pressed.Add("escape"); panel.Update(.016f, true, new());
                Assert(!Drain(bridge).Any(x => x.topic == "clear"), "clear confirmation can cancel");
                Input.Clear();
            }
            Console.WriteLine($"PASS: {checks} audio, pointer, focus, and Masashikun protocol regressions");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
