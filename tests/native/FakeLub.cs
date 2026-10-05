// Deterministic host-only double: no native window/audio device is needed for protocol regression tests.
using System;
using System.Collections.Generic;
public sealed class XrInput { public bool Active, Primary, Secondary, Menu, StickClick; public float StickX, StickY, Trigger, Grip; }
public sealed class Bytes { public string Path = ""; }
public class PlayOpts { public float? Volume, Pitch, Pan; }
public class VoiceOpts : PlayOpts { public bool? Loop; }
public static class Lub
{
    public static class Session
    {
        public static Action<string, string>? HostSending;
        public static Func<(string? topic, string? payload)>? HostPolling;
    }
    public static class Host { public static bool Available() => Session.HostSending != null; }
    public static class Xr { public static XrInput? Input(int hand) => null; }
    public static class Input
    {
        public enum PadAxis { LeftX, LeftY, RightX, RightY, LeftTrigger, RightTrigger }
        public enum PadButton { South, East, West, North, Back, Guide, Start, LeftStick, RightStick, LeftShoulder, RightShoulder, DpadUp, DpadDown, DpadLeft, DpadRight }
        public static readonly HashSet<string> Keys = new(), Pressed = new();
        public static readonly HashSet<int> Mouse = new(), Clicks = new();
        public static float X, Y, Dx, Dy;
        public static bool Captured;
        public static bool GamepadConnected(int player) => false;
        public static float GamepadAxis(int player, PadAxis axis) => 0;
        public static bool GamepadDown(int player, PadButton button) => false;
        public static bool KeyDown(string key) => Keys.Contains(key);
        public static bool KeyPressed(string key) => Pressed.Contains(key);
        public static bool MouseDown(int button = 1) => Mouse.Contains(button);
        public static bool MousePressed(int button = 1) => Clicks.Contains(button);
        public static void MousePos(out float x, out float y) { x = X; y = Y; }
        public static void MouseDelta(out float x, out float y) { x = Dx; y = Dy; }
        public static bool CaptureMouse(bool captured) { Captured = captured; return true; }
        public static void Clear() { Keys.Clear(); Pressed.Clear(); Mouse.Clear(); Clicks.Clear(); X = Y = Dx = Dy = 0; Captured = false; }
    }
    public static class Io
    {
        public enum Status { Pending, Ready, Error }
        public static readonly HashSet<string> ReadyFiles = new();
        public static void LoadBytes(string path, out Bytes? bytes, out int version, out Status status, out string? error)
        { bool ready = ReadyFiles.Contains(System.IO.Path.GetFileName(path)); bytes = ready ? new Bytes { Path = path } : null; version = 1; status = ready ? Status.Ready : Status.Pending; error = null; }
    }
    public static class Audio
    {
        public sealed record Played(string Key, string Sound, bool Loop, float Volume);
        static readonly Dictionary<string, int> handles = new();
        static readonly Dictionary<int, string> names = new();
        public static readonly List<Played> Voices = new();
        public static int Snd(string key, List<float> samples, int channels, int rate, int? version = null)
        { if (!handles.TryGetValue(key, out int id)) { id = handles.Count + 1; handles[key] = id; names[id] = key; } return id; }
        public static int SndBytes(string key, Bytes bytes, int channels, int rate, int? version = null) => Snd(key, new(), channels, rate, version);
        public static void Decode(Bytes encoded, out Bytes? pcm, out int channels, out int rate) { pcm = encoded; channels = 2; rate = 48000; }
        public static bool Voice(string key, int sound, VoiceOpts? options = null)
        { Voices.Add(new Played(key, names[sound], options?.Loop ?? false, options?.Volume ?? 1)); return true; }
    }
}
