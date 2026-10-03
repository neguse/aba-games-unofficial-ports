using System;
using System.Collections.Generic;
using static Lub;

public static class FrameHost
{
    static readonly string[] names = new string[] { "Bonus", "BurstBig", "BurstSmall", "Dash", "Extend", "Grenade", "Hit", "HyperStart", "PlayerDestroyed", "Shot", "ShotHyper", "Mm1", "Mm2", "Mm3" };
    static readonly int[] sounds = new int[14];
    static readonly string savePath = (Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
        Environment.GetEnvironmentVariable("HOME") + "/.local/share") + "/mazer-mayhem/scores.txt";
    static int music = -1;
    static float musicVolume = 1;
    static readonly List<float> noSamples = new();
    static readonly VoiceOpts voice = new() { Loop = true };
    static bool audioStarted;

    public static bool Available() => true;
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
            if (status == Io.Status.Error) { Console.WriteLine(error); Lub.Quit(); return; }
            if (encoded == null) continue;
            Audio.Decode(encoded, out var pcm, out int channels, out int rate);
            if (pcm == null) { Console.WriteLine("Cannot decode " + names[i]); Lub.Quit(); return; }
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
    }
    public static void Send(string topic, string payload)
    {
        switch (topic)
        {
            case "ready":
                Game.frame.Seed(TinySystem.Random.Shared.Next());
                break;
            case "scores.load":
                Io.LoadText(savePath, out var scores, out _, out _, out _);
                if (scores != null) Game.frame.LoadScores(scores);
                break;
            case "scores.save":
                Io.SaveText(savePath, payload);
                break;
            case "quit": Lub.Quit(); break;
            case "music.loop": music = 11 + int.Parse(payload); musicVolume = 1; break;
            case "music.stop": music = -1; break;
            case "music.volume": musicVolume = float.Parse(payload); break;
            case "sound.play":
                int sound = int.Parse(payload);
                if (sounds[sound] != 0) Audio.Play(sounds[sound]);
                break;
        }
    }
}
