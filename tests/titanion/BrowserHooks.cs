using static Lub;

public static class BrowserHooks
{
    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.gameState.score = 7654321;
            g.gameState.left = 0;
            g.gameState.destroyedPlayer();
            g.saveLastReplay();
            g.startTitle();
        }
    }

    public static void Report()
    {
        var g = Game.manager;
        int shots = 0;
        for (int i = 0; i < g.playerSpec.shots.actors.Length; i++)
        {
            if (g.playerSpec.shots.actors[i].exists) shots++;
        }
        Host.Send("test.state", (g.gameState.isInGame() ? 1 : 0) + "," + g.title.cursorIdx + "," + g.gameState.mode_0() + "," + g.player.state.pos.x + ","
            + shots + "," + (g.gameState.paused() ? 1 : 0) + "," + g.stage.phaseTime + "," + g.playerSpec.tractorBeam.length + ","
            + (g.player.state.isInvincible ? 1 : 0) + "," + (g.player.state.replayMode ? 1 : 0) + "," + g.preference.highScore[2][0] + ","
            + g.player.state.capturedEnemyWidth);
    }
}
