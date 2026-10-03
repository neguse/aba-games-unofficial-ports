using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        if (command == 0)
        {
            WkBall.initBalls();
            WkGenerator.initGenerators();
            WkGenerator.addGenerator((int)value);
            WkCore.ballCnt = 100000;
        }
        if (command == 1)
        {
            WkCore.aimScore = 7654321;
            WkCore.status = 2;
            WkCore.missCnt = 120;
        }
        if (command == 2)
        {
            WkCore.addScore(1000001);
            WkSound.Frame(WkData.musicDuration[0]);
        }
    }

    public static void Report()
    {
        int active = 0;
        for (int i = 0; i < WkGenerator.generator.Length; i++)
        {
            if (WkGenerator.generator[i].cnt > 0)
            {
                active++;
            }
        }
        Host.Send("test.state", WkCore.status + "," + WkPan.pan.pos.x + "," + WkPan.pan.pos.y + "," + WkPan.pan.deg + "," + WkCore.score + ","
            + WkCore.hiScore + "," + active + "," + WkSound.playingMusicIdx);
    }
}
