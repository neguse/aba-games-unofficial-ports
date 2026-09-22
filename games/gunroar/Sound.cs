// Copyright 2005 Kenta Cho. Some rights reserved.
using static Lub;

public static class SoundManager
{
    public static string[] names = new string[]
    {
        "shot.wav",
        "lance.wav",
        "hit.wav",
        "turret_destroyed.wav",
        "destroyed.wav",
        "small_destroyed.wav",
        "explode.wav",
        "ship_destroyed.wav",
        "ship_shield_lost.wav",
        "score_up.wav"
    };
    public static bool[] marked = new bool[]
    {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
    };
    public static GunroarRand rand = new GunroarRand();
    public static bool bgmDisabled, seDisabled;
    public static int currentBgm, prevBgmIdx, nextIdxMv;
    public static void loadSounds()
    {
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void playNamedBgm(string name)
    {
        currentBgm = "0123".IndexOf(name.Substring(2, 1));
        if ((!bgmDisabled) && Host.Available())
            Host.Send("music.loop", currentBgm.ToString());
    }

    public static void playBgm()
    {
        int index = rand.nextInt(3) + 1;
        nextIdxMv = rand.nextInt(2) * 2 - 1;
        prevBgmIdx = index;
        playNamedBgm("gr" + index.ToString() + ".ogg");
    }

    public static void nextBgm()
    {
        int index = prevBgmIdx + nextIdxMv;
        if (index < 1)
            index = 3;
        else if (index >= 4)
            index = 1;
        prevBgmIdx = index;
        playNamedBgm("gr" + index.ToString() + ".ogg");
    }

    public static void playCurrentBgm()
    {
        playNamedBgm("gr" + currentBgm.ToString() + ".ogg");
    }

    public static void fadeBgm()
    {
        if (Host.Available())
            Host.Send("music.fade", "");
    }

    public static void haltBgm()
    {
        if (Host.Available())
            Host.Send("music.stop", "");
    }

    public static void playSe(string name)
    {
        if (seDisabled)
            return;
        for (int i = 0; i < names.Length; i++)
            if (names[i] == name)
                marked[i] = true;
    }

    public static void playMarkedSe()
    {
        for (int i = 0; i < names.Length; i++)
            if (marked[i])
            {
                if (Host.Available())
                    Host.Send("sound.play", i.ToString());
                marked[i] = false;
            }
    }

    public static void disableSe()
    {
        seDisabled = true;
    }

    public static void enableSe()
    {
        seDisabled = false;
    }

    public static void disableBgm()
    {
        bgmDisabled = true;
    }

    public static void enableBgm()
    {
        bgmDisabled = false;
    }
}
