using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        if (command == 0)
        {
            RrAttract.score = 7654321;
            RrAttract.setHiScore(1);
            RrCore.initTitle();
        }
    }

    public static void Report()
    {
        var s = RrShip.ship;
        Host.Send("test.state", RrCore.status + "," + RrAttract.mode + "," + RrAttract.slcStg + "," + s.pos.x + "," + s.laserCnt + ","
            + RrShip.bomb + "," + s.rollingCnt + "," + s.color + "," + s.rfCnt + "," + s.cnt + "," + RrAttract.score + ","
            + RrAttract.hiScore.score[3][1] + "," + RrAttract.hiScore.cleard[3][1] + "," + RrAttract.scene + "," + s.rfMtrDec);
    }
}
