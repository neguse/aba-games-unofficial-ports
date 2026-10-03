using static Lub;

public static class BrowserHooks
{
    static int renderFrames;

    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.ship.cnt = -30;
            for (int i = 0; i < 900; i++)
            {
                g.ship.cnt = -30;
                g.move();
            }
        }
        if (command == 1)
        {
            g.score = 1234567;
            g.startGameover();
        }
    }

    public static void Report()
    {
        var g = Game.manager;
        renderFrames++;
        Host.Send("test.state", g.state + "," + g.mode + "," + g.title.mode + "," + g.stageManager.parsec + "," + g.ship.pos.x + ","
            + g.ship.rollLockCnt + "," + g.cnt + "," + g.prefManager.hiScore[1][1][0] + "," + renderFrames);
    }
}
