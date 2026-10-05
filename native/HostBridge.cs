// Desktop counterpart of web/main.js. Game sources and their Host protocol stay unchanged.
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static Lub;

namespace Aba.Native;

public sealed class HostProfile
{
    public string Id { get; }
    public bool Direct { get; }
    public bool Seed { get; }
    public bool Pointer { get; }
    public bool Replay { get; }
    public string MusicExtension { get; }
    public string[] Music { get; }
    public string[] Sounds { get; }
    public int[] Channels { get; }
    public (string Key, int Mask)[] Controls { get; }

    public HostProfile(string id, JsonElement data)
    {
        Id = id;
        Direct = data.TryGetProperty("saves", out _);
        Seed = data.TryGetProperty("seed", out var seed) && seed.GetBoolean();
        Pointer = id == "wok" || data.TryGetProperty("pointer", out var pointer) && pointer.GetBoolean();
        Replay = data.TryGetProperty("replay", out _);
        MusicExtension = data.TryGetProperty("musicExtension", out var ext) ? ext.GetString()! : "ogg";
        Music = Strings(data, "music"); Sounds = Strings(data, "sounds");
        var channels = new List<int>();
        if (data.TryGetProperty("channels", out var chan))
            foreach (var value in chan.EnumerateArray()) channels.Add(value.GetInt32());
        else for (int i = 0; i < Sounds.Length; i++) channels.Add(i);
        Channels = channels.ToArray();
        if (Channels.Length != Sounds.Length) throw new InvalidDataException($"Invalid sound channels: {id}");
        var keys = new List<(string, int)>();
        foreach (var key in data.GetProperty("controls").EnumerateArray())
            keys.Add((NativeKey(key[0].GetString()!), key[1].GetInt32()));
        Controls = keys.ToArray();
    }

    static string[] Strings(JsonElement data, string key)
    {
        var result = new List<string>();
        if (data.TryGetProperty(key, out var values))
            foreach (var value in values.EnumerateArray()) result.Add(value.GetString()!);
        return result.ToArray();
    }

    public static string NativeKey(string browserCode) => browserCode switch
    {
        "ArrowUp" => "up", "ArrowDown" => "down", "ArrowLeft" => "left", "ArrowRight" => "right",
        "ControlLeft" => "left ctrl", "ControlRight" => "right ctrl",
        "ShiftLeft" => "left shift", "ShiftRight" => "right shift", "AltLeft" => "left alt", "AltRight" => "right alt",
        "Period" => ".", "Comma" => ",", "Slash" => "/", "Space" => "space",
        _ when browserCode.StartsWith("Key", StringComparison.Ordinal) && browserCode.Length == 4 => browserCode[3..].ToLowerInvariant(),
        _ when browserCode.StartsWith("Numpad", StringComparison.Ordinal) => "Keypad " + browserCode[6..],
        _ => browserCode.ToLowerInvariant()
    };

    public static HostProfile Load(string id)
    {
        if (id == "tumiki-fighters") id = "tumiki";
        using Stream stream = typeof(HostProfile).Assembly.GetManifestResourceStream("Aba.HostProfiles.json")
            ?? File.OpenRead(Path.Combine(AppContext.BaseDirectory, "HostProfiles.json"));
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty(id, out var profile)) throw new ArgumentException($"Unknown game: {id}");
        return new HostProfile(id, profile);
    }

    public int KeyboardMask(Func<string, bool> down)
    {
        int result = 0;
        foreach (var (key, mask) in Controls) if (down(key)) result |= mask;
        return result;
    }
}

/// <summary>Sanitized, atomic, per-game storage, using the existing three native save locations.</summary>
public sealed class NativeGameStorage
{
    public string DirectoryPath { get; }
    public NativeGameStorage(string gameId, string? dataRoot = null)
    {
        if (string.IsNullOrWhiteSpace(gameId) || gameId.IndexOfAny(new[] { '/', '\\', '.' }) >= 0)
            throw new ArgumentException("A game ID must be a single safe directory name", nameof(gameId));
        string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home)) home = Environment.GetEnvironmentVariable("HOME") ?? AppContext.BaseDirectory;
        dataRoot ??= !string.IsNullOrWhiteSpace(xdg) && Path.IsPathRooted(xdg) ? xdg : Path.Combine(home, ".local", "share");
        DirectoryPath = Path.Combine(dataRoot, gameId);
    }
    string PathFor(string name) => name is "scores" or "replay"
        ? Path.Combine(DirectoryPath, name + ".txt") : throw new ArgumentException("Unknown save kind", nameof(name));
    public string Load(string name)
    {
        string path = PathFor(name);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : "";
    }
    public void Save(string name, string text)
    {
        string path = PathFor(name);
        Directory.CreateDirectory(DirectoryPath);
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, text, new UTF8Encoding(false));
            File.Move(temp, path, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}

/// <summary>Aggregated standard gamepad + XR actions. Y is positive upwards.</summary>
public sealed class NativePadState
{
    public float LeftX, LeftY, RightX, RightY;
    public bool Up, Down, Left, Right, Primary, Secondary, West, North, Start, Back, LeftShoulder, RightShoulder;
    public float LeftTrigger, RightTrigger;
    public static int DirectionMask(float x, float y, bool up = false, bool down = false, bool left = false, bool right = false)
        => (up || y > .35f ? 1 : 0) | (down || y < -.35f ? 2 : 0) | (left || x < -.35f ? 4 : 0) | (right || x > .35f ? 8 : 0);
    public int LeftMask => DirectionMask(LeftX, LeftY, Up, Down, Left, Right);
    public int RightMask => DirectionMask(RightX, RightY);
    static float Stronger(float a, float b) => Math.Abs(a) >= Math.Abs(b) ? a : b;
    public static NativePadState Read()
    {
        var state = new NativePadState();
        for (int player = 0; player < 4; player++)
        {
            if (!Input.GamepadConnected(player)) continue;
            state.LeftX = Stronger(state.LeftX, Input.GamepadAxis(player, Input.PadAxis.LeftX));
            state.LeftY = Stronger(state.LeftY, -Input.GamepadAxis(player, Input.PadAxis.LeftY));
            state.RightX = Stronger(state.RightX, Input.GamepadAxis(player, Input.PadAxis.RightX));
            state.RightY = Stronger(state.RightY, -Input.GamepadAxis(player, Input.PadAxis.RightY));
            state.LeftTrigger = Math.Max(state.LeftTrigger, Input.GamepadAxis(player, Input.PadAxis.LeftTrigger));
            state.RightTrigger = Math.Max(state.RightTrigger, Input.GamepadAxis(player, Input.PadAxis.RightTrigger));
            state.Up |= Input.GamepadDown(player, Input.PadButton.DpadUp); state.Down |= Input.GamepadDown(player, Input.PadButton.DpadDown);
            state.Left |= Input.GamepadDown(player, Input.PadButton.DpadLeft); state.Right |= Input.GamepadDown(player, Input.PadButton.DpadRight);
            state.Primary |= Input.GamepadDown(player, Input.PadButton.South); state.Secondary |= Input.GamepadDown(player, Input.PadButton.East);
            state.West |= Input.GamepadDown(player, Input.PadButton.West); state.North |= Input.GamepadDown(player, Input.PadButton.North);
            state.Start |= Input.GamepadDown(player, Input.PadButton.Start); state.Back |= Input.GamepadDown(player, Input.PadButton.Back);
            state.LeftShoulder |= Input.GamepadDown(player, Input.PadButton.LeftShoulder); state.RightShoulder |= Input.GamepadDown(player, Input.PadButton.RightShoulder);
        }
        // Xr.Input still reports real controllers while the launcher redirects Xr.Active for a flat game.
        var l = Xr.Input(0); var r = Xr.Input(1);
        if (l?.Active == true)
        {
            state.LeftX = Stronger(state.LeftX, l.StickX); state.LeftY = Stronger(state.LeftY, l.StickY);
            state.LeftTrigger = Math.Max(state.LeftTrigger, l.Trigger); state.Start |= l.Menu || l.Primary;
            state.Back |= l.Secondary;
        }
        if (r?.Active == true)
        {
            state.RightX = Stronger(state.RightX, r.StickX); state.RightY = Stronger(state.RightY, r.StickY);
            state.RightTrigger = Math.Max(state.RightTrigger, r.Trigger); state.Primary |= r.Primary;
            state.Secondary |= r.Secondary; state.Start |= r.Menu;
        }
        return state;
    }
    public int MaskFor(string gameId)
    {
        bool a = Primary || RightTrigger > .35f, b = Secondary || LeftTrigger > .35f;
        if (gameId == "wok" || gameId == "masashikun-hi") return Back ? 128 : 0;
        if (gameId == "mu-cade") return LeftMask | RightMask << 4 | (a ? 256 : 0) | (b ? 512 : 0) | (Start ? 1024 : 0) | (Back ? 2048 : 0);
        if (gameId is "gunroar" or "titanion") return LeftMask | LeftMask << 8 | RightMask << 12 | (a ? 16 : 0) | (b ? 32 : 0) | (Start ? 64 : 0) | (Back ? 128 : 0);
        return LeftMask | (a ? 16 : 0) | (b ? 32 : 0) | (Start ? 64 : 0) | (Back ? 128 : 0);
    }
}

public sealed class HostBridge : IDisposable
{
    readonly Queue<(string? topic, string? payload)> queue = new();
    readonly string assetDirectory;
    readonly Dictionary<string, Clip> clips = new();
    readonly Dictionary<int, SoundVoice> voices = new();
    readonly HashSet<string> failedClips = new();
    static readonly List<float> NoSamples = new();
    readonly Action<string, string> sending;
    readonly Func<(string? topic, string? payload)> polling;
    int music = -1, generation, inputMask = -1, state;
    bool musicLoop, wasFocused = true, capture, disposed;
    float fade = -1, pointerX = 320, pointerY = 240;
    string musicKey = "";
    string? previousPointer;
    public HostProfile Profile { get; }
    public NativeGameStorage Storage { get; }
    public MasashikunPanel? Masashikun { get; }
    public bool QuitRequested { get; private set; }
    public bool Ready { get; private set; }
    public string? LastError { get; private set; }
    public event Action<string>? Error;
    public bool MouseCaptureRequested => Profile.Id == "wok" ? state != 0 : Masashikun?.CaptureMouse == true;

    public HostBridge(string gameId, string assetDirectory, string? dataRoot = null)
    {
        Profile = HostProfile.Load(gameId); Storage = new NativeGameStorage(Profile.Id, dataRoot);
        this.assetDirectory = Path.GetFullPath(assetDirectory);
        sending = Send; polling = Poll;
        if (Profile.Id == "masashikun-hi") Masashikun = new MasashikunPanel(Enqueue);
    }
    public void Install()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Session.HostSending = sending; Session.HostPolling = polling;
    }
    public void Enqueue(string topic, string payload = "")
    {
        if (!disposed && !Profile.Direct) queue.Enqueue((topic, payload));
    }
    public (string? topic, string? payload) Poll() => queue.Count > 0 ? queue.Dequeue() : (null, null);
    void Report(string message) { LastError = message; Error?.Invoke(message); Console.Error.WriteLine(message); }
    static bool Index(string text, int count, out int index) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out index) && index >= 0 && index < count;
    public void Send(string topic, string payload)
    {
        if (disposed) return;
        try
        {
            switch (topic)
            {
                case "ready": Ready = true; if (Profile.Seed) Enqueue("seed", RandomNumberGenerator.GetInt32(int.MaxValue).ToString(CultureInfo.InvariantCulture)); break;
                case "scores.load": Enqueue("scores", Storage.Load("scores")); break;
                case "scores.save": Storage.Save("scores", payload); break;
                case "replay.load": if (Profile.Replay) Enqueue("replay", Storage.Load("replay")); break;
                case "replay.save": if (Profile.Replay) Storage.Save("replay", payload); break;
                case "quit": QuitRequested = true; break;
                case "state":
                    if (Profile.Id == "wok") int.TryParse(payload, out state);
                    Masashikun?.OnMessage(topic, payload); break;
                case "ranking": Masashikun?.OnMessage(topic, payload); break;
                case "music.loop": case "music.once":
                    if (Index(payload, Profile.Music.Length, out int track)) { music = track; musicLoop = topic == "music.loop"; fade = -1; musicKey = "aba.music." + ++generation; }
                    break;
                case "music.stop": music = -1; fade = -1; break;
                case "music.fade": if (music >= 0) fade = 1.28f; break;
                case "sound.play":
                    if (Index(payload, Profile.Sounds.Length, out int sound)) voices[Profile.Channels[sound]] = new SoundVoice(sound, "aba.se." + ++generation);
                    break;
                case "sound.stop": if (Index(payload, Profile.Sounds.Length, out int stopped)) voices.Remove(Profile.Channels[stopped]); break;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Report($"{Profile.Id}: {topic} failed: {e.Message}");
            if (topic is "scores.load" or "replay.load") Enqueue(topic[..topic.IndexOf('.')], "");
        }
    }

    /// <summary>Call before Game.OnFrame. Viewport uses the same pixel coordinates as Input.MousePos.</summary>
    public void BeforeFrame(float dt, bool focused = true, float viewportX = 0, float viewportY = 0, float viewportWidth = 640, float viewportHeight = 480)
    {
        if (Profile.Direct || disposed) return;
        dt = Math.Clamp(dt, 0, .25f);
        NativePadState pad = focused ? NativePadState.Read() : new NativePadState();
        if (wasFocused && !focused)
        {
            if (Profile.Id == "masashikun-hi") { Enqueue("button", "0"); Enqueue("blur"); }
            if (Profile.Id == "wok" && state != 0) Enqueue("leave");
        }
        wasFocused = focused;
        if (Masashikun != null) Masashikun.Update(dt, focused, pad);
        else
        {
            int mask = focused ? Profile.KeyboardMask(Input.KeyDown) | pad.MaskFor(Profile.Id) : 0;
            if (mask != inputMask) { inputMask = mask; Enqueue("input", mask.ToString(CultureInfo.InvariantCulture)); }
        }
        if (Profile.Pointer)
        {
            Input.MousePos(out float mx, out float my); Input.MouseDelta(out float dx, out float dy);
            if (focused)
            {
                if (Profile.Id == "wok" && state != 0) { pointerX += dx * 3; pointerY += dy * 3; }
                else if (dx != 0 || dy != 0 || Input.MousePressed(1) || Input.MousePressed(3))
                { pointerX = (mx - viewportX) * 640 / Math.Max(1, viewportWidth); pointerY = (my - viewportY) * 480 / Math.Max(1, viewportHeight); }
                if (Profile.Id == "wok")
                {
                    pointerX += (pad.RightX != 0 ? pad.RightX : pad.LeftX) * 600 * dt;
                    pointerY -= (pad.RightY != 0 ? pad.RightY : pad.LeftY) * 600 * dt;
                    if (pad.Left) pointerX -= 600 * dt; if (pad.Right) pointerX += 600 * dt;
                    if (pad.Up) pointerY -= 600 * dt; if (pad.Down) pointerY += 600 * dt;
                }
            }
            pointerX = Math.Clamp(pointerX, 0, Profile.Id == "wok" ? 639 : 640);
            pointerY = Math.Clamp(pointerY, 0, Profile.Id == "wok" ? 479 : 480);
            int buttons = focused ? (Input.MouseDown(1) || pad.Primary || pad.RightTrigger > .35f ? 1 : 0) | (Input.MouseDown(3) || pad.Secondary ? 2 : 0) : 0;
            // Preserve a click completed between two render frames, as the browser event queue does.
            if (focused && Input.MousePressed(1) && !Input.MouseDown(1))
                Enqueue("pointer", $"{(int)pointerX},{(int)pointerY},{buttons | 1}");
            string pointer = $"{(int)pointerX},{(int)pointerY},{buttons}";
            if (pointer != previousPointer || focused && Input.MousePressed(1))
            { Enqueue("pointer", pointer); previousPointer = pointer; }
        }
        bool nextCapture = focused && MouseCaptureRequested;
        if (capture != nextCapture) { capture = Input.CaptureMouse(nextCapture) && nextCapture; }
        RefreshClips();
    }

    void RefreshClips()
    {
        foreach (var clip in clips.Values) Audio.Snd(clip.Key, NoSamples, 1, 48000, 1);
        // Decode on demand; pending requests remain live until data is ready or are superseded/stopped.
        if (music >= 0) LoadClip(Profile.Music[music], Profile.MusicExtension);
        foreach (var voice in voices.Values) LoadClip(Profile.Sounds[voice.Index], "wav");
    }
    string ClipId(string name, string extension) => name + "." + extension;
    Clip? LoadClip(string name, string extension)
    {
        string id = ClipId(name, extension);
        if (clips.TryGetValue(id, out var clip)) return clip;
        if (failedClips.Contains(id)) return null;
        string wav = Path.Combine(assetDirectory, "audio", name + ".wav");
        string path = File.Exists(wav) ? wav : Path.Combine(assetDirectory, "audio", id);
        Io.LoadBytes(path, out var encoded, out _, out var status, out var error);
        if (status == Io.Status.Error) { failedClips.Add(id); Report($"{Profile.Id}: audio {id}: {error}"); return null; }
        if (encoded == null) return null;
        Audio.Decode(encoded, out var pcm, out int channels, out int rate);
        if (pcm == null) { failedClips.Add(id); Report($"{Profile.Id}: cannot decode audio {id}"); return null; }
        string key = "aba.clip." + id;
        clip = new Clip(key, Audio.SndBytes(key, pcm, channels, rate, 1));
        clips.Add(id, clip); return clip;
    }
    /// <summary>Call after Game.OnFrame so this frame's Host commands take effect without a frame of stale audio.</summary>
    public void AfterFrame(float dt, bool focused = true)
    {
        if (Profile.Direct || disposed) return;
        // Keep browser audio semantics: focus loss clears input, but is not an unsolicited game pause.
        if (music >= 0)
        {
            var clip = LoadClip(Profile.Music[music], Profile.MusicExtension);
            if (clip != null) Audio.Voice(musicKey, clip.Handle, new VoiceOpts { Loop = musicLoop, Volume = fade < 0 ? 1 : Math.Clamp(fade / 1.28f, 0, 1) });
            if (fade >= 0) { fade -= Math.Max(0, dt); if (fade <= 0) { music = -1; fade = -1; } }
        }
        foreach (var voice in voices.Values)
        {
            var clip = LoadClip(Profile.Sounds[voice.Index], "wav");
            if (clip != null) Audio.Voice(voice.Key, clip.Handle, new VoiceOpts { Loop = false, Volume = 1 });
        }
    }
    public void Dispose()
    {
        if (disposed) return;
        if (capture) Input.CaptureMouse(false);
        if (Session.HostSending == sending) Session.HostSending = null;
        if (Session.HostPolling == polling) Session.HostPolling = null;
        queue.Clear(); voices.Clear(); clips.Clear(); disposed = true;
        // The launcher calls Session.Reset at its safe frame boundary to stop voices and release native handles.
    }
    sealed record Clip(string Key, int Handle);
    sealed record SoundVoice(int Index, string Key);
}
