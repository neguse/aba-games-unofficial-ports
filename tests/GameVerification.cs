using System;

public static class GameVerification
{
    static int failures;
    static void Check(bool value, string name)
    {
        if (!value) { Console.WriteLine("FAIL " + name); failures++; }
    }
    static void CheckAttachments(GameManager game)
    {
        game.startInGameFirst();
        game.ship.pos.x = 0; game.ship.pos.y = 0; game.ship.deg = 0; game.ship.cnt = 1;
        game.field.setGroundY(0);
        var pool = game.ship.stuckEnemies;
        pool.move();
        var shape = TumikiSet.getInstance("small/s1.tmk");
        var splinter = (Splinter)game.splinters.getInstance();
        int score = game.score;
        splinter.set(0.6f, 0, shape, 0, false);
        splinter.move();
        StuckEnemy attached = null;
        foreach (Actor actor in pool.actor)
            if (actor.isExist && !((StuckEnemy)actor).isMyShip) attached = (StuckEnemy)actor;
        Check(!splinter.isExist && attached != null, "collect and attach a falling part");
        if (attached == null) return;
        Check(game.score == score + 50, "collection score");
        Check(attached.topBulletNum == 1, "captured weapon");
        pool.move();
        Check(pool.checkHitWithoutMyShip(attached.pos) == attached, "attached part collision");
        Check(Math.Abs(game.stageManager.rank - shape.size * 0.0048f) < 0.00001f, "attachment rank");
        score = game.score;
        attached.cnt = shape.fireScoreInterval - 1;
        attached.move();
        Check(game.score == score + shape.fireScore * 5, "extended part periodic score");
        for (int i = 0; i < 16; i++) pool.pullIn();
        pool.move();
        Check(pool.pullInRatio == 0 && attached.pos.dist(game.ship.pos) == 0, "fully retract attached part");
        Check(attached.topBullet[0].actor.bullet.deactivated && pool.checkHitWithoutMyShip(attached.pos) == null, "retracted weapon and collision");
        score = game.score;
        attached.cnt = shape.fireScoreInterval - 1;
        attached.move();
        Check(game.score == score + shape.fireScore, "retracted part periodic score");
        for (int i = 0; i < 16; i++) pool.pushOut();
        pool.move();
        Check(!attached.topBullet[0].actor.bullet.deactivated && attached.pos.dist(game.ship.pos) > 0, "restore captured weapon");
        bool fired = false;
        for (int i = 0; i < 65; i++)
        {
            game.bullets.move();
            foreach (Actor actor in game.bullets.actor)
                if (actor.isExist && ((BulletActor)actor).isVisible && ((BulletActor)actor).bullet.type == BulletType.SHIP) fired = true;
        }
        Check(fired, "captured part fires player bullets");
        pool.removeAllEnemies();
        StuckEnemy root = null;
        foreach (Actor actor in pool.actor)
            if (actor.isExist && ((StuckEnemy)actor).isMyShip) root = (StuckEnemy)actor;
        var bridge = (StuckEnemy)pool.getInstance();
        bridge.tumikiSet = shape; bridge.setColSize();
        float spacing = root.colSize + bridge.colSize - 0.1f;
        Check(bridge.set(spacing, 0, 0, shape, 0), "bridge attachment");
        var tip = (StuckEnemy)pool.getInstance();
        Check(tip.set(spacing + bridge.colSize * 2 - 0.1f, 0, 0, shape, 0), "chain attachment");
        var sibling = (StuckEnemy)pool.getInstance();
        Check(sibling.set(-spacing, 0, 0, shape, 0), "independent attachment");
        pool.move();
        pool.removeStuckEnemy(bridge);
        Check(!bridge.isExist && !tip.isExist && sibling.isExist && root.isExist, "cut only disconnected parts");
        Check(!bridge.topBullet[0].actor.isExist && !tip.topBullet[0].actor.isExist, "remove disconnected weapons");
        pool.removeAllEnemies();
    }
    static void CheckEnemyParts(GameManager game)
    {
        game.startInGameFirst();
        var enemy = (Enemy)game.enemies.getInstance();
        enemy.set(0, 0, GameData.Find_enemy("middle/b1.enm"), new PointsMovePattern(), false);
        enemy.mv.reachFirstPointFirst = true;
        enemy.addTopBullets();
        int score = game.score;
        Check(enemy.checkHit(new Vector(-6, 3), 15), "hit upper enemy part");
        Check(enemy.isExist && enemy.parts[1].shield < 0 && enemy.parts[2].shield == 15, "partial enemy destruction");
        Check(enemy.parts[0].shield == 15 && game.score == score + 400, "part damage transferred to body and score");
        int fallen = 0;
        foreach (Actor actor in game.splinters.actor) if (actor.isExist) fallen++;
        Check(fallen == 1, "destroyed part becomes collectible");
        Check(enemy.checkHit(new Vector(-6, -3), 15) && !enemy.isExist, "remaining part destroys body");
    }
    static void CheckContinue(GameManager game)
    {
        game.startInGameFirst();
        game.stage = 2;
        game.startInGame();
        game.left = 0;
        game.ship.cnt = 1;
        game.ship.destroyed();
        Check(game.state == GameState.GAMEOVER, "last life gameover");
        game.pad.buttons = 0; game.pad.directions = 0;
        for (int i = 0; i < 66; i++) game.move();
        game.pad.directions = Pad.PAD_LEFT; game.move();
        game.pad.directions = 0;
        game.pad.buttons = Pad.PAD_BUTTON1; game.move();
        Check(game.state == GameState.IN_GAME && game.stage == 2 && game.credit == GameManager.CREDIT_NUM - 1, "continue on same stage");
        Check(game.left == GameManager.LEFT_NUM && game.score == 0, "continue resets lives and score");
        game.pad.buttons = 0;
    }
    public static void Main()
    {
        var random = new Rand();
        random.setSeed(5489);
        Check(random.nextBits() == -795755684, "MT first output");
        Check(random.nextBits() == 581869302, "MT second output");
        random.setSeed(5489);
        Check(Math.Abs(random.nextFloat(1) - 0.8147237f) < 0.000001f, "MT unsigned real output");
        bool inRange = true;
        for (int i = 0; i < 10000; i++)
        {
            float value = random.nextFloat(1);
            if (value < 0 || value > 1) inRange = false;
        }
        Check(inRange, "MT real range");
        var game = new GameManager();
        game.init(); game.start(); game.draw();
        Check(GameData.stage.Length == 5 && GameData.enemy.Length == 33 && GameData.tumiki.Length == 99, "original data");
        Check(game.state == GameState.TITLE && Tumiki.meshes[0].count > 0 && LetterRender.meshes[0].count > 0, "title");
        for (int i = 0; i < 18; i++) game.move();
        game.pad.buttons = Pad.PAD_BUTTON1; game.move();
        Check(game.state == GameState.START_GAME, "start input");
        for (int i = 0; i < 258; i++) game.move();
        Check(game.state == GameState.IN_GAME, "takeoff");
        game.pad.buttons = 0;
        game.pad.pause = true; game.move();
        Check(game.state == GameState.PAUSE, "pause");
        game.pad.pause = false; game.move(); game.pad.pause = true; game.move();
        Check(game.state == GameState.IN_GAME, "resume");
        game.pad.pause = false;
        game.ship.cnt = 1;
        int lives = game.left;
        game.ship.destroyed();
        Check(game.left == lives - 1 && game.ship.cnt == -Ship.RESTART_CNT, "death and respawn");
        game.ship.stuckEnemies.pullIn();
        Check(game.ship.stuckEnemies.pullInCnt == 1 && game.ship.stuckEnemies.pullInRatio < 1, "retract");
        game.ship.stuckEnemies.pushOut();
        Check(game.ship.stuckEnemies.pullInCnt == 0 && game.ship.stuckEnemies.pullInRatio == 1, "extend");
        CheckAttachments(game);
        CheckEnemyParts(game);
        CheckContinue(game);
        game.startInGameFirst();
        game.pad.buttons = Pad.PAD_BUTTON1;
        int previousStage = -1, frames = 0;
        while (frames < 65000 && game.state != GameState.END_GAME)
        {
            game.ship.cnt = -1;
            game.pad.directions = frames % 240 < 120 ? Pad.PAD_UP : Pad.PAD_DOWN;
            game.move();
            if (game.stage != previousStage)
            {
                game.draw();
                Console.WriteLine($"STAGE {game.stage} FRAME {frames} SCORE {game.score}");
                previousStage = game.stage;
            }
            frames++;
        }
        Check(game.state == GameState.END_GAME && game.stage == 5, "five stages and ending");
        game.draw();
        game.pad.buttons = 0;
        for (int i = 0; i < 702; i++) game.move();
        Check(game.state == GameState.GAMEOVER, "ending to gameover");
        Console.WriteLine($"RESULT {failures}");
    }
}
