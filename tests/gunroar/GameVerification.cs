using System;
public static class GrVerification
{
    static int failures;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static int Count<T>(ActorPool<T> pool) where T : Actor
    {
        int count = 0;
        foreach (T item in pool.actor) if (item.exists) count++;
        return count;
    }
    static void Input(GameManager game, int mode, int frame)
    {
        game.pad.state.dir = frame % 240 < 120 ? PadStateDir.LEFT : PadStateDir.RIGHT;
        game.pad.state.button = PadStateButton.A | (frame % 90 == 0 ? PadStateButton.B : 0);
        game.twinStick.state.left.x = frame % 240 < 120 ? -1 : 1;
        game.twinStick.state.left.y = 0;
        game.twinStick.state.right.x = mode == 2 ? -game.twinStick.state.left.x : 0;
        game.twinStick.state.right.y = mode == 2 ? 0 : 1;
        game.mouse.state.x = frame % 120 / 10f - 6;
        game.mouse.state.y = 8;
        game.mouse.state.button = frame % 160 < 80 ? MouseStateButton.LEFT : MouseStateButton.RIGHT;
    }
    static string Snapshot(GameManager game)
    {
        string value = game.ship.boat[0].pos.x.ToString() + "," + game.ship.boat[0].pos.y.ToString() + "," + game.stageManager.rank.ToString() + "," + game.scoreReel.actualScore.ToString();
        foreach (Bullet bullet in game.bullets.actor) if (bullet.exists) value += ";" + bullet.pos.x.ToString() + "," + bullet.pos.y.ToString();
        return value;
    }
    public static void Main()
    {
        var random = new GunroarRand(); random.setSeed(5489);
        Check(random.nextBits() == -795755684 && random.nextBits() == 581869302 && random.nextBits() == -404620562, "MT19937 reference words");
        random.setSeed(5489);
        Check(Math.Abs(random.nextFloat(1) - 0.8147237f) < 0.000001f, "unsigned MT real conversion");
        bool minus = false, plus = false;
        for (int i = 0; i < 10000; i++)
        {
            float value = random.nextFloat(1); Check(value >= 0 && value <= 1, "random real range");
            int signed = random.nextSignedInt(1); if (signed == -1) minus = true; if (signed == 1) plus = true;
        }
        Check(minus && plus && random.nextInt(0) == 0, "inclusive signed random bounds");
        Drawing.BeginFrame(); Drawing.recordBlend = true; Drawing.premultiplyAdditive = true;
        var game = new GameManager(); game.init(); game.start();
        game.draw(); Check(Drawing.batches.Count > 0 && Drawing.batches[0].vertices.Count > 1000, "title geometry");
        for (int mode = 0; mode < 4; mode++)
        {
            game.inGameState.rand.setSeed(900 + mode); game.startInGame(mode);
            Check(game.ship.boatNum == (mode == 2 ? 2 : 1), "mode boat count");
            int maxShots = 0;
            for (int frame = 0; frame < 600; frame++)
            {
                Input(game, mode, frame); game.move();
                maxShots = Math.Max(maxShots, Count(game.shots));
                if (frame % 150 == 0) game.draw();
            }
            Check(maxShots > 0, "mode fires shots");
            string snapshot = Snapshot(game);
            ReplayData replay = game.inGameState.replayData;
            game.startTitle();
            for (int frame = 0; frame < 600; frame++)
            {
                game.pad.state.clear(); game.mouse.state.clear(); game.move();
                if (frame % 150 == 0) game.draw();
            }
            Check(Snapshot(game) == snapshot, "replay reproduces position rank score and bullets");
            Check(game.inGameState.replayData == replay && game.ship.replayMode(), "title retains replay");
            game.startInGame(mode);
            game.pad.pause = true; game.move(); game.pad.pause = false;
            float pausedY = game.field.screenY; game.move();
            Check(game.inGameState.pauseCnt > 0 && game.field.screenY == pausedY, "pause freezes simulation");
            game.pad.pause = true; game.move(); game.pad.pause = false;
            bool boss = false; int maxBullets = 0;
            for (int frame = 0; frame < 7200; frame++)
            {
                Input(game, mode, frame);
                for (int b = 0; b < game.ship.boatNum; b++) game.ship.boat[b].cnt = -1;
                game.move(); if (game.enemies.hasBoss()) boss = true;
                maxBullets = Math.Max(maxBullets, Count(game.bullets));
                if (frame % 500 == 0) game.draw();
            }
            Check(boss && maxBullets > 0 && game.stageManager.rank > 1, "progress to boss and rank growth");
            Console.WriteLine("MODE " + mode.ToString() + " BULLETS " + maxBullets.ToString() + " RANK " + game.stageManager.rank.ToString());
            game.scoreReel.actualScore = 12345 + mode;
            game.inGameState.left = 0; game.inGameState.shipDestroyed();
            Check(game.inGameState.isGameOver && game.prefManager.prefData.highScore(mode) == 12345 + mode, "game over saves mode score");
        }
        var pref = new PrefManager(); pref.load("11,22,33,44,3");
        Check(pref.prefData.highScore(2) == 33 && pref.prefData.gameMode == 3, "saved preferences load");
        pref.load("11,22,33,44,4"); Check(pref.prefData.gameMode == 3, "invalid preference ignored");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
