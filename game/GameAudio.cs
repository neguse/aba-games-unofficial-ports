using System;
using System.Collections.Generic;
using Lub;
using Lubx;

// BGM と効果音。
// 音のファイルは audio/<名前>.<拡張子> で、名前は GameProfile.Music / Sounds、BGM の拡張子は GameProfile.MusicExtension (効果音は wav)。
// 効果音は GameProfile.Channels の channel ごとに 1 つ鳴り、同じ channel の次の音は前の音を止める。
// Update を毎フレーム呼んで、鳴らしている音を宣言し続ける (宣言が途切れた音は止まる)。
public static class GameAudio
{
    const float FadeSeconds = 1.28f;

    static int music = -1, generation;
    static bool musicLoop;
    static float fade = -1;
    static string musicKey = "";
    // channel → (効果音の番号, 声の key)
    static readonly Dictionary<int, int> channelSounds = new Dictionary<int, int>();
    static readonly Dictionary<int, string> channelKeys = new Dictionary<int, string>();

    public static void PlayMusic(int track, bool loop)
    {
        if (track < 0 || track >= GameProfile.Music.Length)
            return;
        music = track;
        musicLoop = loop;
        fade = -1;
        musicKey = "aba.music." + (++generation);
    }

    public static void StopMusic()
    {
        music = -1;
        fade = -1;
    }

    public static void FadeMusic()
    {
        if (music >= 0)
            fade = FadeSeconds;
    }

    public static void Play(int sound)
    {
        if (sound < 0 || sound >= GameProfile.Sounds.Length)
            return;
        int channel = GameProfile.Channels[sound];
        channelSounds[channel] = sound;
        channelKeys[channel] = "aba.se." + (++generation);
    }

    public static void Stop(int sound)
    {
        if (sound < 0 || sound >= GameProfile.Sounds.Length)
            return;
        int channel = GameProfile.Channels[sound];
        channelSounds.Remove(channel);
        channelKeys.Remove(channel);
    }

    static int? Clip(string name, string extension)
    {
        string path = "audio/" + name + "." + extension;
        return extension == "ogg" ? Lubx.Assets.Ogg("aba.clip." + name, path) : Lubx.Assets.Wav("aba.clip." + name, path);
    }

    public static void Update(float dt)
    {
        if (music >= 0)
        {
            var clip = Clip(GameProfile.Music[music], GameProfile.MusicExtension);
            if (clip != null)
                Lub.Audio.Voice(musicKey, clip.Value, new Lub.VoiceOpts { Loop = musicLoop, Volume = fade < 0 ? 1 : Math.Clamp(fade / FadeSeconds, 0, 1) });
            if (fade >= 0)
            {
                fade -= Math.Max(0, dt);
                if (fade <= 0)
                {
                    music = -1;
                    fade = -1;
                }
            }
        }
        foreach (var pair in channelSounds)
        {
            var clip = Clip(GameProfile.Sounds[pair.Value], "wav");
            if (clip != null)
                Lub.Audio.Voice(channelKeys[pair.Key], clip.Value, new Lub.VoiceOpts { Loop = false, Volume = 1 });
        }
    }
}
