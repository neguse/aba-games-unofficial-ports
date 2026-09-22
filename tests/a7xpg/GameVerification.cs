using System;
public static class A7xVerification
{
    static int failures;
    static void check(bool value, string message) { if (!value) { failures++; Console.WriteLine("FAIL " + message); } }
    public static void Main()
    {
        Rand.setSeed(1234); Rand.index = 5678; var rand = new Rand();
        int[] expected = new int[] { -1946056501, -1319034868, -57652539, 654499913, -1761236701, 471047561, 879455519, -121422286, -610275466, -1451242689 };
        for (int i = 0; i < expected.Length; i++) check(rand.nextBits() == expected[i], "Phobos random " + i.ToString());
        Rand.setSeed(1234);
        float[] small = new float[] { 0.008762520738f, 0.004202578682f, 0.0002763710218f, 0.00450108014f, 0.008169221692f, 0.001831445494f, 0.009473296814f, 0.004576541483f, 0.001127034193f, 0.0028995662f, 0.0002518402471f, 0.0006354987854f, 0.004562656395f, 0.007859470323f, 0.006024090573f, 0.001746507245f };
        for (int i = 0; i < small.Length; i++) check(Math.Abs(rand.nextFloat(0.01f) - small[i]) < 0.00000001f, "D1 small float " + i.ToString());
        Drawing.BeginFrame(); Drawing.recordBlend = true; Drawing.premultiplyAdditive = true;
        var game = new A7xGameManager(); game.init(); game.start(); game.startInGame();
        float[][] trace = new float[][] {
            new float[] { 0, -3.00000453f, 0.200000003f, 0 },
            new float[] { 4.000000477f, -1.000004053f, 0.200000003f, 0 },
            new float[] { 7.999996662f, -3.00000453f, 0.200000003f, 0 },
            new float[] { 7.999996662f, -15.8758688f, 0.5247833133f, 25.53833008f },
            new float[] { -10.07413101f, -15.8758688f, 0.6749994755f, 61.93825531f },
            new float[] { -17.09912872f, -3.767046452f, 0.431439966f, 80.64022064f },
            new float[] { -12.07180691f, 5.006109715f, 0.5250698328f, 105.0471802f },
            new float[] { 6.003468037f, 5.006109715f, 0.6749998927f, 141.4471588f },
            new float[] { 6.003468037f, -14.12771034f, 0.431439966f, 160.149353f },
            new float[] { 2.003469467f, -17, 0.200000003f, 158.9495544f }
        };
        Rand.setSeed(1234);
        for (int i = 0; i < 300; i++) {
            game.input.directions = i % 160 < 40 ? 1 : i % 160 < 80 ? 8 : i % 160 < 120 ? 2 : 4;
            game.input.buttons = i % 90 < 72 ? 16 : 0;
            if (i % 30 == 29) game.ship.addGauge();
            game.ship.move();
            if (i % 30 == 29) {
                float[] row = trace[GameMath.integer(i / 30)];
                check(Math.Abs(game.ship.pos.x - row[0]) < 0.001f && Math.Abs(game.ship.pos.y - row[1]) < 0.001f &&
                    Math.Abs(game.ship.speed - row[2]) < 0.00001f && Math.Abs(game.ship.gauge - row[3]) < 0.001f, "D1 ship trajectory " + i.ToString());
            }
        }
        game.startInGame(); game.input.buttons = 0;
        check(game.stage == 0 && game.left == 2 && game.leftGold == 10, "start state");
        game.input.directions = Input.PAD_UP;
        game.ship.move(); game.input.buttons = Input.PAD_BUTTON1;
        float before = game.ship.pos.y;
        for (int i = 0; i < 30; i++) game.ship.move();
        check(game.ship.speed > 0.5f && game.ship.pos.y > before, "boost acceleration");
        game.ship.pos.x = 0; game.ship.pos.y = 0; game.ship.gauge = 201; game.ship.move();
        check(game.ship.invincible && game.ship.enemyDstCnt == 0, "full gauge invulnerability");
        int score = game.score; game.ship.destroyEnemy(); game.ship.destroyEnemy();
        check(game.score == score + 300, "enemy bonus ladder");
        game.ship.gauge = 0.1f; game.ship.move(); check(!game.ship.invincible, "invulnerability expiry");
        game.score = 20000; game.addScore(0); check(game.left == 2, "strict extend threshold");
        game.addScore(1); check(game.left == 3 && game.extendScore == 50000, "first extend");
        game.score = 100001; game.startGameover(); check(game.continueEnable && game.prefManager.hiScore == 100001, "continue and high score");
        game.stage = 30; game.startGameover(); check(!game.continueEnable, "no continue beyond first loop");
        for (int stage = 0; stage < 31; stage++) {
            game.state = A7xGameManager.IN_GAME; game.stage = stage; game.startStage(false);
            for (int f = 0; f < 1800; f++) {
                game.ship.restart = true; game.ship.cnt = -2;
                game.input.buttons = f % 90 < 72 ? 16 : 0;
                game.input.directions = (f / 120) % 4 == 0 ? 1 : (f / 120) % 4 == 1 ? 8 : (f / 120) % 4 == 2 ? 2 : 4;
                game.inGameMove();
            }
            Drawing.BeginFrame(); game.inGameDraw();
            check(Drawing.batches.Count > 0, "stage render " + stage.ToString());
            game.leftGold = 1; game.stageTimer = 1200; game.getGold();
            check(game.state == A7xGameManager.STAGE_CLEAR && game.timeBonus == 2040, "stage clear bonus");
            for (int f = 0; f < 302; f++) { game.cnt++; game.stageClearMove(); if (game.state == A7xGameManager.IN_GAME) break; }
            check(game.stage == stage + 1, "next stage " + stage.ToString());
            Console.WriteLine("STAGE " + stage.ToString());
        }
        game.stage = 0; game.startInGame(); game.ship.cnt = 0; game.stageTimer = 1; game.stageMove();
        check(game.state == A7xGameManager.STAGE_CLEAR, "time over transition");
        game.cnt = 301; game.stageClearMove(); check(game.left == 1 && game.stage == 1, "time over loses one ship");
        game.pPrsd = false; game.input.pause = true; game.inGameMove(); check(game.state == A7xGameManager.PAUSE, "pause");
        game.input.pause = false; game.pauseMove(); game.input.pause = true; game.pauseMove(); check(game.state == A7xGameManager.IN_GAME, "resume");
        var enemy = (Enemy)game.enemies.getInstance(); enemy.set(0, 1, 0.2f);
        game.ship.invincible = false; game.ship.restart = false; game.left = 2;
        enemy.hitShip(); check(game.left == 1 && game.ship.cnt == -Ship.RESTART_CNT && !enemy.isExist, "collision death resets enemies");
        enemy = (Enemy)game.enemies.getInstance(); enemy.set(0, 1, 0.2f); game.ship.invincible = true;
        score = game.score; enemy.hitShip(); check(game.score > score && enemy.cnt == -Enemy.DESTROYED_CNT && enemy.isExist, "invincible collision respawns enemy");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
