using static Lub;

public static class SoundManager
{
    static string[] names = new string[]
    {
        "shot.wav",
        "hit.wav",
        "bullethit.wav",
        "destroyed.wav",
        "addtail.wav",
        "breaktail.wav",
        "enhancedshot.wav",
        "shipdestroyed.wav",
        "extend.wav"
    };
    static bool[] marked = McdArrays.Make(9, () => false);
    static Rand rand = new Rand();
    static int current, step;
    static bool bgmDisabled, seDisabled;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void loadSounds()
    {
    }

    public static void playBgm()
    {
        current = rand.nextInt(4);
        step = rand.nextInt(2) * 2 - 1;
        playCurrentBgm();
    }

    public static void nextBgm()
    {
        current = (current + step + 4) % 4;
        playCurrentBgm();
    }

    public static void playCurrentBgm()
    {
        if (!(bgmDisabled) && Host.Available())
            Host.Send("music.loop", current.ToString());
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
        for (int i = 0; i < marked.Length; i++)
            if (marked[i])
            {
                if (Host.Available())
                    Host.Send("sound.play", i.ToString());
                marked[i] = false;
            }
    }

    public static void clearMarkedSes()
    {
        for (int i = 0; i < marked.Length; i++)
            marked[i] = false;
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
