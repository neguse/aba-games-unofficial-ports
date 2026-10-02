using System;
using System.Collections.Generic;
using static Lub;

public static class FrameHost
{
    static readonly string[] names = new string[] { SoundManager.names[0].Replace(".wav", ""), SoundManager.names[1].Replace(".wav", ""), SoundManager.names[2].Replace(".wav", ""), SoundManager.names[3].Replace(".wav", ""), SoundManager.names[4].Replace(".wav", ""), SoundManager.names[5].Replace(".wav", ""), SoundManager.names[6].Replace(".wav", ""), SoundManager.names[7].Replace(".wav", ""), SoundManager.names[8].Replace(".wav", ""), SoundManager.names[9].Replace(".wav", ""), "tt1", "tt2", "tt3", "tt4" };
    static readonly int[] sounds = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly int[] channels = new int[] { 0, 1, 1, 2, 3, 4, 4, 5, 6, 7 };
    static readonly int[] playing = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
    static readonly string[] soundKeys = new string[8];
    static int generation;
    static readonly List<float> noSamples = new();
    static readonly VoiceOpts voice = new();
    static readonly string saveRoot = (Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
        Environment.GetEnvironmentVariable("HOME") + "/.local/share") + "/torus-trooper";
    static int music = -1;
    static float fade = -1;
    static bool audioStarted;
    static string savedReplay = "";
    public static bool Available() => true;
    public static void Load()
    {
        Io.LoadText(saveRoot + "/scores.txt", out var scores, out _, out _, out _);
        if (scores != null)
        {
            Game.manager.prefManager.load(scores);
            Game.manager.titleManager.start();
        }
        Io.LoadText(saveRoot + "/replay.txt", out var text, out _, out _, out _);
        if (text != null)
        {
            var data = new ReplayData();
            if (data.decode(text))
            {
                Game.savedReplay = text;
                savedReplay = text;
                Game.manager.inGameState._replayData = data;
                Game.manager.startTitle();
            }
        }
    }
    static void Save(string name, string text)
    {
        Io.SaveText(saveRoot + "/" + name + ".txt", text);
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
            if (status == Io.Status.Error) { Console.WriteLine(error); Lub.Quit(); return; }
            if (encoded == null) continue;
            Audio.Decode(encoded, out var pcm, out int count, out int rate);
            if (pcm == null) { Console.WriteLine("Cannot decode " + names[i]); Lub.Quit(); return; }
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
            voice.Volume = fade < 0 ? 1 : Math.Max(0, fade / 2);
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
                soundKeys[channels[index]] = "se" + channels[index] + "-" + generation;
                generation++;
                break;
        }
    }
}
