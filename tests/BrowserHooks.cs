using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.stage = (int)value;
            g.startInGame();
            for (int i = 0; i < 600; i++)
            {
                g.ship.cnt = -30;
                g.move();
            }
            g.ship.cnt = 1;
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
        Host.Send("test.state", g.state + "," + g.stage + "," + g.score + "," + g.ship.pos.x + "," + g.ship.pos.y + ","
            + g.ship.stuckEnemies.pullInCnt + "," + g.cnt + "," + g.prefManager.ranking[0].score);
    }
}
