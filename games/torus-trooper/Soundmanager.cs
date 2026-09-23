// Copyright 2004 Kenta Cho. Some rights reserved.
using static Lub;

public static class SoundManager
{
    public static Rand rand = new Rand();
    public static int prevBgmIdx = -1, nextIdxMv;
    public static bool seDisabled;
    public static string[] names = new string[]
    {
        "shot.wav",
        "charge.wav",
        "charge_shot.wav",
        "hit.wav",
        "small_dest.wav",
        "middle_dest.wav",
        "boss_dest.wav",
        "myship_dest.wav",
        "extend.wav",
        "timeup_beep.wav"
    };
    public static void loadSounds()
    {
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void playBgm()
    {
        int i = rand.nextInt(TtData.musicCount);
        nextIdxMv = rand.nextInt(2) * 2 - 1;
        if (i == prevBgmIdx)
            i = (i + 1) % TtData.musicCount;
        prevBgmIdx = i;
        if (Host.Available())
            Host.Send("music.loop", i.ToString());
    }

    public static void nextBgm()
    {
        int i = prevBgmIdx + nextIdxMv;
        if (i < 0)
            i = TtData.musicCount - 1;
        else if (i >= TtData.musicCount)
            i = 0;
        prevBgmIdx = i;
        if (Host.Available())
            Host.Send("music.loop", i.ToString());
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
            if ((names[i] == name) && (Host.Available()))
                Host.Send("sound.play", i.ToString());
    }

    public static void disableSe()
    {
        seDisabled = true;
    }

    public static void enableSe()
    {
        seDisabled = false;
    }
}
