using static Lub;

public static class FrameHost
{
    static readonly string[] names = [.. SoundManager.names.Select(Path.GetFileNameWithoutExtension), "tt1", "tt2", "tt3", "tt4"];
    static readonly int[] sounds = new int[names.Length];
    static readonly int[] channels = [0, 1, 1, 2, 3, 4, 4, 5, 6, 7];
    static readonly int[] playing = Enumerable.Repeat(-1, 8).ToArray();
    static readonly string[] soundKeys = new string[8];
    static int generation;
    static readonly List<float> noSamples = new();
    static readonly VoiceOpts voice = new();
    static readonly string saveRoot = Path.Combine(Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share"), "torus-trooper");
    static int music = -1;
    static float fade = -1;
    static bool audioStarted;
    static string savedReplay = "";
    public static bool Available() => true;
    public static void Load()
    {
        string scores = Path.Combine(saveRoot, "scores.txt");
        if (File.Exists(scores))
        {
            Game.manager.prefManager.load(File.ReadAllText(scores));
            Game.manager.titleManager.start();
        }
        string replay = Path.Combine(saveRoot, "replay.txt");
        if (File.Exists(replay))
        {
            var data = new ReplayData();
            string text = File.ReadAllText(replay);
            if (data.decode(text))
            {
                Game.savedReplay = savedReplay = text;
                Game.manager.inGameState._replayData = data;
                Game.manager.startTitle();
            }
        }
    }
    static void Save(string name, string text)
    {
        Directory.CreateDirectory(saveRoot);
        string path = Path.Combine(saveRoot, name + ".txt");
        File.WriteAllText(path + ".tmp", text);
        File.Move(path + ".tmp", path, true);
    }
    public static void SaveReplay()
    {
        if (Game.savedReplay == savedReplay) return;
        Save("replay", Game.savedReplay);
        savedReplay = Game.savedReplay;
    }
    public static void Begin()
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (sounds[i] != 0) { Audio.Snd(names[i], noSamples, 1, 48000, 1); continue; }
            Io.LoadBytes("audio/" + names[i] + ".wav", out var encoded, out _, out var status, out var error);
            if (status == Io.Status.Error) throw new IOException(error);
            if (encoded == null) continue;
            Audio.Decode(encoded, out var pcm, out int count, out int rate);
            if (pcm == null) throw new IOException("Cannot decode " + names[i]);
            sounds[i] = Audio.SndBytes(names[i], pcm, count, rate, 1);
        }
        if (!audioStarted && sounds[0] != 0)
        {
            Audio.Play(sounds[0], new PlayOpts { Volume = 0 });
            audioStarted = true;
        }
    }
    public static void End(bool focused, float dt)
    {
        SaveReplay();
        if (!focused) return;
        if (music >= 0 && sounds[music] != 0)
        {
            voice.Loop = true;
            voice.Volume = fade < 0 ? 1 : MathF.Max(0, fade / 2);
            Audio.Voice("music", sounds[music], voice);
            if (fade >= 0) { fade -= dt; if (fade <= 0) music = -1; }
        }
        voice.Loop = false; voice.Volume = 1;
        for (int channel = 0; channel < playing.Length; channel++)
        {
            int index = playing[channel];
            if (index >= 0 && sounds[index] != 0) Audio.Voice(soundKeys[channel], sounds[index], voice);
        }
    }
    public static void Send(string topic, string payload)
    {
        switch (topic)
        {
            case "scores.save": Save("scores", payload); break;
            case "music.loop": music = 10 + int.Parse(payload); fade = -1; break;
            case "music.stop": music = -1; break;
            case "music.fade": fade = 2; break;
            case "sound.play":
                int index = int.Parse(payload);
                playing[channels[index]] = index;
                soundKeys[channels[index]] = "se" + channels[index] + "-" + generation++;
                break;
        }
    }
}
