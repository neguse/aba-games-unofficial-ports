// Copyright 2005 Kenta Cho. Some rights reserved.
using static Lub;

public static class Sound
{
    public static string[] names = new string[]
    {
        "shot.wav",
        "explosion1.wav",
        "explosion2.wav",
        "explosion3.wav",
        "tractor.wav",
        "flying_down.wav",
        "player_explosion.wav",
        "flick.wav",
        "extend.wav"
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
        false
    };
    public static void clearMarkedSes()
    {
        for (int i = 0; i < marked.Length; i++)
            marked[i] = false;
    }

    public static TitanionRand rand = new TitanionRand();
    public static bool bgmDisabled, seDisabled;
    public static int currentBgm, prevBgmIdx, nextIdxMv;
    public static void load_0()
    {
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void playNamedBgm(string name)
    {
        currentBgm = "123".IndexOf(name.Substring(3, 1));
        if ((((!((bgmDisabled))))) && ((Host.Available())))
            Host.Send("music.loop", currentBgm.ToString());
    }

    public static void playBgm()
    {
        int index = rand.nextInt(3);
        nextIdxMv = rand.nextInt(2) * 2 - 1;
        prevBgmIdx = index;
        playNamedBgm("ttn" + (index + 1).ToString() + ".ogg");
    }

    public static void nextBgm()
    {
        int index = prevBgmIdx + nextIdxMv;
        if (index < 0)
            index = 2;
        else if (index >= 3)
            index = 0;
        prevBgmIdx = index;
        playNamedBgm("ttn" + (index + 1).ToString() + ".ogg");
    }

    public static void playCurrentBgm()
    {
        playNamedBgm("ttn" + (currentBgm + 1).ToString() + ".ogg");
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

    public static void playMarkedSes()
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
