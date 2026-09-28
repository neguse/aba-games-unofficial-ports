using static Lub;

public class Sound
{
    public static string[] names = new string[]
    {
        "Bonus",
        "BurstBig",
        "BurstSmall",
        "Dash",
        "Extend",
        "Grenade",
        "Hit",
        "HyperStart",
        "PlayerDestroyed",
        "Shot",
        "ShotHyper"
    };
    float fadeoutCnt = -1;
    public void Initialize()
    {
    }

    public void Update()
    {
        if (fadeoutCnt > 0)
        {
            fadeoutCnt = System.Math.Max(0, fadeoutCnt - SimulationTime.Step);
            if (Host.Available())
                Host.Send("music.volume", (fadeoutCnt / 120f).ToString());
            if (fadeoutCnt == 0)
                StopBgm();
        }
    }

    public void PlayBgm(string name)
    {
        StopBgm();
        fadeoutCnt = -1;
        int index = name == "Mm1" ? 0 : name == "Mm2" ? 1 : 2;
        if (Host.Available())
            Host.Send("music.loop", index.ToString());
    }

    public void StopBgm()
    {
        if (Host.Available())
            Host.Send("music.stop", "");
    }

    public void FadeoutBgm()
    {
        fadeoutCnt = 120;
    }

    public void PlaySe(string name)
    {
        for (int i = 0; i < names.Length; i++)
            if (names[i] == name && Host.Available())
                Host.Send("sound.play", i.ToString());
    }
}
