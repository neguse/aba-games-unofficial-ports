using System;
using System.Collections.Generic;
using static Lub;

public static class FrameHost
{
    static readonly string[] names = new string[] { Sound.names[0], Sound.names[1], Sound.names[2], Sound.names[3], Sound.names[4], Sound.names[5], Sound.names[6], Sound.names[7], "Gtg1", "Gtg2", "Gtg3" };
    static readonly int[] sounds = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    internal static readonly Dictionary<int, FrameSoundLoop> loops = new();
    static readonly string savePath = (Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
        Environment.GetEnvironmentVariable("HOME") + "/.local/share") + "/gear-toy-gear/scores.txt";
    static int music = -1;
    static float musicVolume = 1;
    static readonly List<float> noSamples = new();
    static readonly VoiceOpts voice = new() { Loop = true };
    static readonly PlayOpts oneShot = new();
    static bool audioStarted;

    public static bool Available() => true;
    public static void Load()
    {
        Game.frame.Seed(TinySystem.Random.Shared.Next());
        Io.LoadText(savePath, out var scores, out _, out _, out _);
        if (scores != null) Game.frame.LoadScores(scores);
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
        foreach (var pair in loops)
        {
            int id = pair.Key;
            var loop = pair.Value;
            if (sounds[loop.Sound] == 0) continue;
            voice.Pan = loop.Pan;
            voice.Volume = 1;
            Audio.Voice("cue" + id, sounds[loop.Sound], voice);
        }
    }
    static float Pan(string[] parts, int start)
    {
        float x = float.Parse(parts[start]);
        float z = float.Parse(parts[start + 2]);
        return x / Math.Max(1, (float)Math.Sqrt(x * x + z * z));
    }
    public static void Send(string topic, string payload)
    {
        switch (topic)
        {
            case "scores.save":
                Io.SaveText(savePath, payload);
                break;
            case "music.loop": music = 8 + int.Parse(payload); musicVolume = 1; break;
            case "music.stop": music = -1; break;
            case "music.volume": musicVolume = float.Parse(payload); break;
            case "sound.play":
                int sound = int.Parse(payload);
                if (sounds[sound] != 0) Audio.Play(sounds[sound]);
                break;
            case "spatial.play":
                var parts = payload.Split(",");
                int id = int.Parse(parts[0]), index = int.Parse(parts[1]);
                float pan = Pan(parts, 3);
                if (parts[2] == "1") loops[id] = new FrameSoundLoop { Sound = index, Pan = pan };
                else if (sounds[index] != 0)
                {
                    oneShot.Pan = pan;
                    Audio.Play(sounds[index], oneShot);
                }
                break;
            case "spatial.update":
                var update = payload.Split(",");
                int key = int.Parse(update[0]);
                if (loops.TryGetValue(key, out var loop)) loop.Pan = Pan(update, 1);
                break;
            case "spatial.stop": loops.Remove(int.Parse(payload)); break;
        }
    }
}

sealed class FrameSoundLoop { public int Sound; public float Pan; }
