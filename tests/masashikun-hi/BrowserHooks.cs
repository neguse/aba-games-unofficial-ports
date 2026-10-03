using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        if (command == 0)
            MasKak.setkakhiscore(12345);
    }

    public static void Report()
    {
        Host.Send("test.state", MasMain.mlspe + "," + MasKak.kakcou + "," + MasKak.myx + "," + MasKak.myp + "," + MasKak.speed + ","
            + Game.lastMotion + "," + Game.presses + "," + Game.holds + "," + (MasScores.hscsf ? 1 : 0) + "," + MasKak.kakhsc[1].rec);
    }
}
