using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        var f = Game.frame;
        var p = f.player;
        if (command == 0)
        {
            for (int i = 0; i < 100; i++)
                p.GetBonus(p.storedPos);
        }
        if (command == 1)
        {
            f.stage.appearanceWaitCnt = 0;
            f.stage.appearanceCntDec = 31;
        }
        if (command == 2)
        {
            p.score = 7654321;
            p.left = 0;
            p.Destroy();
            p.gameoverCnt = 599;
        }
    }

    public static void Report()
    {
        var f = Game.frame;
        var p = f.player;
        int boss = 0;
        for (int i = 0; i < f.balls.Length(); i++)
        {
            if (f.balls.actors[i].BaseRadius > 5)
                boss++;
        }

        Lub.Host.Send("test.state", (int)f.state + "," + p.storedPos.X + "," + p.storedPos.Y + "," + p.deg + "," + p.shotCnt + "," + p.dashCnt + ","
            + (p.storedIsInHyper ? 1 : 0) + "," + f.storedPauseCnt + "," + p.cnt + "," + f.record.Scores[0] + "," + boss + ","
            + (p.isInReplay ? 1 : 0) + "," + Pad.input + "," + f.grenades.Length() + ","
            + (Xr.Active() ? 1 : 0) + "," + (Xr.Focused() ? 1 : 0));
    }
}
