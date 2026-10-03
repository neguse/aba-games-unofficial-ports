using static Lub;

public static class BrowserHooks
{
    static Cue cue;

    public static void Command(int command, float value)
    {
        var f = Game.frame;
        var a = f.actors;
        if (command == 0)
        {
            a.stage.stageCount = 6;
            a.stage.stageTicks = 0;
            a.stage.GoToNextStage();
            a.stage.stageTicks = 421;
        }
        if (command == 1)
        {
            a.gameState.score = 7654321;
            a.gameState.left = 0;
            a.player.invincibleTicks = -1;
            a.player.Destroy();
            a.gameState.gameOverTicks = 2;
        }
        if (command == 2)
        {
            cue = f.sound.GetCue("HomingLaser");
            cue.Apply3D(new AudioListener(), new AudioEmitter());
            cue.Play();
        }
        if (command == 3)
            cue.Stop(AudioStopOptions.Immediate);
    }

    public static void Report()
    {
        var f = Game.frame;
        var a = f.actors;
        var p = a.player;
        Lub.Host.Send("test.state", (int)f.state + "," + p.Pos.X + "," + p.Pos.Y + "," + Stage.GameSpeed + "," + a.shots.Count + ","
            + f.storedPauseTicks + "," + a.stage.ticks + "," + f.record.Scores[0] + "," + a.middleEnemies.Count + ","
            + (p.isInReplay ? 1 : 0) + "," + Pad.input + "," + a.playerHomingLasers.Count + ","
            + FrameHost.loops.Count + "," + (Xr.Active() ? 1 : 0) + "," + (Xr.Focused() ? 1 : 0));
    }
}
