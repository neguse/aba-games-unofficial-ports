using System;
public static class P47Verification
{
    static int failures;
    static void Check(bool condition, string name)
    {
        if (!condition) { failures++; Console.WriteLine("FAIL " + name); }
    }
    static int Count(ActorPool pool)
    {
        int count = 0;
        foreach (var actor in pool.actor) if (actor.isExist) count++;
        return count;
    }
    public static void Main()
    {
        var first = new P47Rand(); var second = new P47Rand(); var reference = new Rand();
        first.setSeed(123); reference.setSeed(123);
        Check(first.nextInt(10000) == reference.nextInt(10000), "MT first value");
        Check(second.nextInt(10000) == reference.nextInt(10000), "shared MT sequence");
        var game = new P47GameManager(); game.init(); game.start();
        Check(game.state == P47GameManager.TITLE_STATE, "title");
        Check(game.stageManager.smallType[0].barrage[0].morphNum == 8, "eight morph patterns");
        game.draw();
        Check(Field.meshes[0].vertices[3] == 1 && Field.meshes[0].count > 0,
              "field mesh inherits draw color");
        Check(BulletActor.meshes[1].ranges[0].material == (int)Lub.Gfx.Blend.None,
              "bullet outline is opaque");
        game.title.changeMode();
        Check(game.title.mode == 1, "title selects LOCK");
        game.title.setStatus(); game.startInGame();
        Check(game.mode == 1 && game.state == P47GameManager.IN_GAME, "LOCK starts");
        game.ship.cnt = 1; game.pad.directions = Pad.PAD_RIGHT;
        float x = game.ship.pos.x; game.ship.move();
        Check(game.ship.pos.x > x, "movement"); game.pad.directions = 0;
        game.pad.buttons = Pad.PAD_BUTTON1 | Pad.PAD_BUTTON2;
        for (int i = 0; i < 30; i++) game.ship.move();
        Check(Count(game.targetLocks) > 0 && Count(game.shots) > 0, "LOCK and shots");
        game.startPause(); int turn = game.bullets.getTurn(); game.move();
        Check(game.state == P47GameManager.PAUSE && game.bullets.getTurn() == turn, "pause freezes simulation");
        game.resumePause(); game.mode = 0; game.startInGame(); game.ship.cnt = 1;
        for (int i = 0; i < 30; i++) game.ship.move();
        Check(Count(game.rolls) > 0 && game.ship.rollCharged, "ROLL charges");
        game.pad.buttons = 0; game.ship.move();
        int released = 0;
        foreach (var actor in game.rolls.actor) if (actor.isExist && ((Roll)actor).released) released++;
        Check(released > 0 && !game.ship.rollCharged, "ROLL releases");
        Bonus.resetBonusScore();
        var bonus = (Bonus)game.bonuses.getInstance(); bonus.set(game.ship.pos, null);
        int score = game.score; bonus.getBonus();
        Check(game.score == score + 10 && Bonus.bonusScore == 20, "bonus chain");
        int lives = game.left; game.ship.cnt = 1; game.ship.destroyed();
        Check(game.left == lives - 1 && game.ship.cnt < 0, "death and invulnerability");
        for (int mode = 0; mode < 2; mode++) for (int difficulty = 0; difficulty < 4; difficulty++)
        {
            game.mode = mode; game.difficulty = difficulty; game.parsecSlot = 0; game.title.mode = mode;
            game.startInGame();
            int frame = 0, maxBullets = 0; bool boss = false;
            while (frame < 24000 && game.stageManager.parsec < 12)
            {
                game.ship.cnt = -30;
                game.pad.buttons = Pad.PAD_BUTTON1 | (frame % 180 < 100 ? Pad.PAD_BUTTON2 : 0);
                game.move();
                if (game.stageManager.bossSection) boss = true;
                int bullets = Count(game.bullets); if (bullets > maxBullets) maxBullets = bullets;
                if (frame % 600 == 0) game.draw();
                frame++;
            }
            Check(game.stageManager.parsec >= 12 && boss && maxBullets > 10, "progress through bosses");
            Check(game.interval >= 16 && game.interval <= 28.01f, "intentional slowdown bounds");
            Console.WriteLine("MODE " + mode.ToString() + " DIFFICULTY " + difficulty.ToString() + " PARSEC " + game.stageManager.parsec.ToString() + " BULLETS " + maxBullets.ToString() + " FRAME " + frame.ToString());
            game.score = 1234567; game.startGameover();
            Check(game.prefManager.hiScore[mode][difficulty][0] == 1234567 && game.prefManager.reachedParsec[mode][difficulty] >= 12, "score and reached parsec");
        }
        var pref = new P47PrefManager();
        string saved = "";
        for (int i = 0; i < 88; i++) saved += "123,";
        pref.load(saved + "2,3,1");
        Check(pref.selectedMode == 1 && pref.selectedDifficulty == 2 && pref.selectedParsecSlot == 3 && pref.hiScore[1][3][9] == 123, "saved progress loads");
        pref.load(saved + "4,3,1");
        Check(pref.selectedDifficulty == 2, "invalid save is ignored");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
