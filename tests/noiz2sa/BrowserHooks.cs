using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        if (command == 0)
        {
            NrAttract.hiScore.stage = (int)value;
            NrCore.initTitle();
        }
        if (command == 1)
        {
            NrAttract.score = 123456;
            NrBarrage.scene = 0;
            NrAttract.setClearScore();
            NrPreference.savePreference();
        }
        if (command == 2)
        {
            NrAttract.score = 7654321;
            NrCore.initGameover();
            NrAttract.goCnt = 901;
        }
        if (command == 3)
        {
            NrRender.overlay = Palette;
        }
    }

    static void Palette()
    {
        int[] colors = new int[] { 0, 1, 16, 31, 32, 47, 48, 63 };
        for (int i = 0; i < 8; i++)
        {
            NrScreen.buf.Rect(16 + i * 36, 220, 32, 32, colors[i], 0, 0, 0, 0);
        }
    }

    public static void Report()
    {
        var s = NrShip.ship;
        int shots = 0;
        for (int i = 0; i < NrShot.shot.Length; i++)
        {
            if (NrShot.shot[i].cnt != -999999)
            {
                shots++;
            }
        }
        Host.Send("test.state", NrCore.status + "," + NrAttract.slcStg + "," + NrAttract.stage + "," + s.pos.x + "," + s.speed + "," + s.cnt + ","
            + shots + "," + NrBarrage.endless + "," + NrBarrage.insane + "," + NrAttract.hiScore.stageScore[13] + ","
            + NrAttract.hiScore.sceneScore[1][0] + "," + NrBarrage.scene);
    }
}
