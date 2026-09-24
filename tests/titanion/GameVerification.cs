using System;
public static class TtnVerification
{
    static int failures;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static int Count<T>(ActorPool<T> pool) where T : Actor
    {
        int count = 0;
        foreach (T item in pool.actors) if (item.exists) count++;
        return count;
    }
    static void Input(Frame game, int frame)
    {
        game.pad.state.dir = frame % 240 < 120 ? PadStateDir.LEFT : PadStateDir.RIGHT;
        game.pad.state.button = PadStateButton.A | (frame % 120 < 40 ? PadStateButton.B : 0);
    }
    static string Snapshot(Frame game)
    {
        string value = game.player.pos().x.ToString() + "," + game.player.pos().y.ToString() + "," + game.stage.rank.ToString() + "," + game.gameState.score.ToString();
        foreach (Bullet bullet in game.bullets.actors) if (bullet.exists) value += ";" + bullet.pos().x.ToString() + "," + bullet.pos().y.ToString();
        return value;
    }
    static void Draw(Frame game) { game.draw_0(); }
    static void Rules(Frame game, int mode)
    {
        game.startInGame(mode);
        game.player.state.invincibleCnt = 0; game.player.state.isInvincible = false;
        game.pad.state.dir = 0; game.pad.state.button = PadStateButton.B;
        game.player.move_0();
        Check(game.playerSpec.tractorBeam.length > 0 && game.player.state.isInvincible == (mode == 0), "mode beam invulnerability");
        int captured = game.player.state.capturedEnemyNum;
        var enemy = game.enemies.getInstance();
        enemy.set_5(game.stage.smallEnemy1Spec, game.player.pos().x, game.player.pos().y + 1, 0, 0);
        enemy.spec.checkCaptured(enemy.state);
        Check(mode == 2 ? enemy.state.anger > 0 && game.player.state.capturedEnemyNum == captured : enemy.state.captureState == 1 && game.player.state.capturedEnemyNum == captured + 1, "capture or provocation");
        game.startInGame(mode);
        game.player.state.invincibleCnt = 0; game.player.state.isInvincible = false;
        var position = new Vector(game.player.pos().x + 0.5f, game.player.pos().y + 0.2f);
        bool hit = game.player.checkEnemyHit_3(position, new Vector(0.2f, 0.1f), new Vector(1, 1));
        Check(mode == 0 ? hit && game.gameState.left == 1 : mode == 1 ? hit && game.gameState.left == 2 && game.player.state.vel.vctSize() > 0 : !hit && game.gameState.left == 2, "mode enemy collision");
    }
    static void Phases(Frame game, int mode)
    {
        game.startInGame(mode);
        for (int frame = 0; frame < 1000 && game.stage.phaseNum < 12; frame++) {
            game.enemies.clear(); if (!game.stage.stageStarted) game.stage.appCnt = 0;
            game.player.state.isInvincible = true; game.player.state.invincibleCnt = 300;
            game.pad.state.clear(); game.move_0();
        }
        Check(game.stage.phaseNum == 12, "phase transition through rank reset");
        Draw(game);
    }
    static void Chain(Frame game)
    {
        game.startInGame(2);
        for (int i = 0; i < 3; i++) {
            Bullet bullet = game.bullets.getInstance();
            bullet.set_5(game.stage.bulletSpec, i * 1.5f, 0, 0, 0.1f);
        }
        int count = game.bullets.removeAround(1, new Vector(0, 0), game.particles, game.bonusParticles, game.player);
        Check(count == 4 && Count(game.bullets) == 0 && game.gameState.score == 6, "recursive bullet chain scoring");
        game.startInGame(2);
        for (int i = 0; i < 3; i++) {
            Bullet bullet = game.bullets.getInstance();
            bullet.set_5(game.stage.bulletSpec, i * 1.5f, 0, 0, 0.1f);
        }
        Enemy enemy = game.enemies.getInstance();
        enemy.set_5(game.stage.middleEnemySpec, 0, 0, 0, 0);
        enemy.state.cnt = 123;
        enemy.spec.destroyed_2(enemy.state);
        Check(Count(game.bullets) == 0 && game.gameState.score == 6 + game.stage.middleEnemySpec.score * 4, "middle enemy chain multiplier uses removed bullets");
    }
    public static void Main()
    {
        var random = new TitanionRand(); random.setSeed(5489);
        Check(random.nextBits() == -795755684 && random.nextBits() == 581869302 && random.nextBits() == -404620562, "MT19937 reference words");
        random.setSeed(5489);
        Check(Math.Abs(random.nextFloat(1) - 0.8147237f) < 0.000001f, "unsigned MT real conversion");
        random.setSeed(5489); random.nextSignedInt(0);
        Check(random.nextBits() == -795755684, "zero signed bound preserves RNG state");
        var game = new Frame(); game.init_0(); game.start_0();
        Draw(game);
        Check(game.title.logo.count == 6, "title uses one textured quad");
        var playerShape = new PlayerShape();
        Check(playerShape.mesh.ranges.Count > 0 && playerShape.mesh.ranges[0].material == (int)Lub.Gfx.Blend.Alpha, "player shape preserves alpha material");
        Vector3 circular = game.field.calcCircularPos_2(0, 36);
        Check(Math.Abs(circular.z - 51.2f) < 0.0001f && Math.Abs(circular.y - 24.8f) < 0.0001f, "circular field curve");
        Check(Math.Abs(game.field.normalizeX(game.field.circularDistance() + 3) - 3) < 0.0001f, "circular wrapping");
        for (int mode = 0; mode < 3; mode++)
        {
            game.rand.setSeed(900 + mode); game.startInGame(mode);
            int maxShots = 0;
            for (int frame = 0; frame < 600; frame++)
            {
                Input(game, frame); game.move_0();
                maxShots = Math.Max(maxShots, Count(game.playerSpec.shots));
                if (frame % 150 == 0) Draw(game);
            }
            Check(maxShots > 0, "mode fires shots");
            string snapshot = Snapshot(game);
            game.saveLastReplay();
            string replay = game.savedReplay;
            game.loadLastReplay(); game.startTitle();
            for (int frame = 0; frame < 600; frame++)
            {
                game.pad.state.clear(); game.move_0();
                if (frame % 150 == 0) Draw(game);
            }
            Check(Snapshot(game) == snapshot, "replay reproduces position rank score and bullets");
            Check(game.player.state.replayMode && game.savedReplay == replay, "saved replay reload");
            game.startInGame(mode);
            game.pad.pause = true; game.move_0(); game.pad.pause = false;
            int phaseTime = game.stage.phaseTime; game.move_0();
            Check(game.gameState.paused() && game.stage.phaseTime == phaseTime, "pause freezes simulation");
            game.pad.pause = true; game.move_0(); game.pad.pause = false;
            int maxBullets = 0;
            for (int frame = 0; frame < 7200; frame++)
            {
                Input(game, frame); game.player.state.invincibleCnt = 300;
                game.move_0(); maxBullets = Math.Max(maxBullets, Count(game.bullets));
                if (frame % 500 == 0) Draw(game);
            }
            Check(maxBullets > 0, "mode enemy attack");
            Console.WriteLine("MODE " + mode.ToString() + " BULLETS " + maxBullets.ToString() + " PHASE " + game.stage.phaseNum.ToString());
            game.gameState.score = 1234567 + mode;
            game.gameState.left = 0; game.gameState.destroyedPlayer();
            Check(game.gameState.isGameOver() && game.preference.highScore[mode][0] == 1234567 + mode, "game over saves mode score");
        }
        for (int mode = 0; mode < 3; mode++) { Rules(game, mode); Phases(game, mode); }
        Chain(game);
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
