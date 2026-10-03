using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.scoreReel.actualScore = 7654321;
            g.inGameState.left = 0;
            g.inGameState.shipDestroyed();
            g.startTitle(true);
        }
    }

    public static void Report()
    {
        var g = Game.manager;
        int shots = 0;
        int lance = 0;
        for (int i = 0; i < g.shots.actor.Length; i++)
        {
            Shot s = g.shots.actor[i];
            if (s.exists)
            {
                shots++;
                if (s.lance) lance++;
            }
        }
        Host.Send("test.state", (g.state == g.inGameState ? 1 : 0) + "," + g.titleManager.gameMode + "," + g.inGameState.gameMode + "," + g.ship.boatNum + ","
            + g.ship.boat[0].pos.x + "," + g.ship.boat[1].pos.x + "," + shots + "," + g.inGameState.pauseCnt + "," + g.inGameState.time + ","
            + g.ship.boat[0].fireSprDeg + "," + lance + "," + (g.ship.replayMode() ? 1 : 0) + "," + g.prefManager.prefData.highScore(3));
    }
}
