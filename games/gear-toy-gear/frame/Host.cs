using System.Globalization;
using static Lub;

public static class FrameHost
{
    static readonly string[] names = [.. Sound.names, "Gtg1", "Gtg2", "Gtg3"];
    static readonly int[] sounds = new int[names.Length];
    static readonly Queue<(string, string)> messages = new();
    static readonly Dictionary<int, (int sound, float pan)> loops = new();
    static readonly string savePath = Path.Combine(
        Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share"),
        "gear-toy-gear", "scores.txt");
    static int music = -1;
    static float musicVolume = 1;
    static readonly List<float> noSamples = new();
    static readonly VoiceOpts voice = new() { Loop = true };
    static readonly PlayOpts oneShot = new();
    static bool audioStarted;

    public static bool Available() => true;
    public static void Poll(out string topic, out string payload)
    {
        if (messages.TryDequeue(out var message)) (topic, payload) = message;
        else (topic, payload) = (null, null);
    }
    public static void Begin()
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (sounds[i] != 0)
            {
                Audio.Snd(names[i], noSamples, 1, 48000, 1);
                continue;
            }
            Io.LoadBytes("audio/" + names[i] + ".wav", out var encoded, out _, out var status, out var error);
            if (status == Io.Status.Error) throw new IOException(error);
            if (encoded == null) continue;
            Audio.Decode(encoded, out var pcm, out int channels, out int rate);
            if (pcm == null) throw new IOException("Cannot decode " + names[i]);
            sounds[i] = Audio.SndBytes(names[i], pcm, channels, rate, 1);
        }
        if (!audioStarted && sounds[0] != 0)
        {
            Audio.Play(sounds[0], new PlayOpts { Volume = 0 });
            audioStarted = true;
        }
    }
    public static void End(bool focused)
    {
        if (!focused) return;
        if (music >= 0 && sounds[music] != 0)
        {
            voice.Pan = 0;
            voice.Volume = musicVolume;
            Audio.Voice("music", sounds[music], voice);
        }
        foreach (var (id, loop) in loops)
        {
            if (sounds[loop.sound] == 0) continue;
            voice.Pan = loop.pan;
            voice.Volume = 1;
            Audio.Voice("cue" + id, sounds[loop.sound], voice);
        }
    }
    static float Pan(string[] parts, int start)
    {
        float x = float.Parse(parts[start], CultureInfo.InvariantCulture);
        float z = float.Parse(parts[start + 2], CultureInfo.InvariantCulture);
        return x / MathF.Max(1, MathF.Sqrt(x * x + z * z));
    }
    public static void Send(string topic, string payload)
    {
        switch (topic)
        {
            case "ready":
                messages.Enqueue(("seed", System.Random.Shared.Next().ToString(CultureInfo.InvariantCulture)));
                break;
            case "scores.load":
                if (File.Exists(savePath)) messages.Enqueue(("scores", File.ReadAllText(savePath)));
                break;
            case "scores.save":
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                File.WriteAllText(savePath + ".tmp", payload);
                File.Move(savePath + ".tmp", savePath, true);
                break;
            case "quit": Lub.Quit(); break;
            case "music.loop": music = 8 + int.Parse(payload); musicVolume = 1; break;
            case "music.stop": music = -1; break;
            case "music.volume": musicVolume = float.Parse(payload, CultureInfo.InvariantCulture); break;
            case "sound.play":
                int sound = int.Parse(payload);
                if (sounds[sound] != 0) Audio.Play(sounds[sound]);
                break;
            case "spatial.play":
                var parts = payload.Split(',');
                int id = int.Parse(parts[0]), index = int.Parse(parts[1]);
                float pan = Pan(parts, 3);
                if (parts[2] == "1") loops[id] = (index, pan);
                else if (sounds[index] != 0)
                {
                    oneShot.Pan = pan;
                    Audio.Play(sounds[index], oneShot);
                }
                break;
            case "spatial.update":
                var update = payload.Split(',');
                int key = int.Parse(update[0]);
                if (loops.TryGetValue(key, out var loop)) loops[key] = (loop.sound, Pan(update, 1));
                break;
            case "spatial.stop": loops.Remove(int.Parse(payload)); break;
        }
    }
}
