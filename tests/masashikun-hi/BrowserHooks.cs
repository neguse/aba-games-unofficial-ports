using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        if (command == 0)
            MasKak.setkakhiscore(12345);
    }

    static int lastMotion, presses, holds;

    static void Observe()
    {
        if (MasForm.mousemv > 0) lastMotion = MasForm.mousemv;
        if (MasForm.mousebt == 1) presses++;
        if (MasForm.mousebt == 2) holds++;
    }

    public static void Report()
    {
        Game.beforeMove = Observe;
        Host.Send("test.state", MasMain.mlspe + "," + MasKak.kakcou + "," + MasKak.myx + "," + MasKak.myp + "," + MasKak.speed + ","
            + lastMotion + "," + presses + "," + holds + "," + (MasScores.hscsf ? 1 : 0) + "," + MasKak.kakhsc[1].rec);
    }
}
