using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.score = 7654321;
            g.startGameover();
        }
        if (command == 1)
        {
            Gold gold = (Gold)g.golds.actor[15];
            gold.isExist = true;
            gold.pos.x = g.ship.pos.x + GameMath.sin(g.ship.deg) * 0.4f;
            gold.pos.y = g.ship.pos.y + GameMath.cos(g.ship.deg) * 0.4f;
        }
        if (command == 2)
        {
            g.ship.gauge = 201;
        }
    }

    public static void Report()
    {
        var g = Game.manager;
        Host.Send("test.state", g.state + "," + g.stage + "," + g.ship.pos.x + "," + g.ship.pos.y + "," + g.ship.speed + ","
            + g.stageTimer + "," + g.leftGold + "," + (g.ship.invincible ? 1 : 0) + "," + g.prefManager.hiScore);
    }
}
