using System;
public static class TtVerification
{
    static int failures;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static string Snapshot(GameManager g) {
        string s = g.ship.pos.x.ToString() + "," + g.ship.pos.y.ToString() + "," + g.ship.speed.ToString() + "," + g.stageManager.level.ToString() + "," + g.inGameState.score.ToString();
        foreach (BulletActor a in g.bullets.actor) if (a.exists) s += ";" + a.bullet.pos.x.ToString() + "," + a.bullet.pos.y.ToString();
        return s;
    }
    static void Input(GameManager g, int frame) {
        g.pad.directions = PadDir.UP | (frame % 240 < 120 ? PadDir.LEFT : PadDir.RIGHT);
        g.pad.buttons = PadButton.A | (frame % 120 < 40 ? PadButton.B : 0);
    }
    static void Rules(GameManager g) {
        g.startInGame();
        var state = g.inGameState;
        Check(state.time == 120000, "initial two minutes");
        state.time = 60000; g.ship.cnt = 1; g.ship.destroyed();
        Check(state.time == 45000 && g.ship.cnt < 0, "death costs fifteen seconds");
        state.score = state.nextExtend; state.addScore(0);
        Check(state.time == 45000, "extend threshold is strict");
        state.addScore(1); Check(state.time == 60000, "extend adds fifteen seconds");
        g.stageManager._middleBossZone = true; state.gotoNextZone();
        Check(state.time == 90000, "middle boss adds thirty seconds");
        g.stageManager._middleBossZone = false; state.gotoNextZone();
        Check(state.time == 120000, "level boss adds forty-five seconds and caps time");
        g.shots.clear(); g.enemies.clear(); g.bullets.clear();
        var shot = g.shots.getInstance(); shot.set_3(true);
        for (int i = 0; i < 22; i++) shot.move();
        shot.release(); Check(!shot.exists, "short charge cancels");
        shot.set_3(true);
        for (int i = 0; i < 90; i++) shot.move();
        shot.release(); Check(shot.exists && !shot.inCharge && shot.range == 47 && Math.Abs(shot.trgSize - 13.6f) < 0.00001f, "full charge range and size");
        int score = state.score;
        shot.addScore(100, new Vector()); shot.addScore(100, new Vector());
        Check(state.score == score + 300 && shot.multiplier == 3, "piercing multiplier increases per kill");
        g.pad.pause = true; g.move(); float time = state.time;
        for (int i = 0; i < 10; i++) g.move();
        Check(state.time == time && state.pauseCnt > 0, "pause freezes time");
        g.pad.pause = false; g.move(); g.pad.pause = true; g.move(); g.pad.pause = false;
        Check(state.pauseCnt == 0, "pause release and resume");
    }
    static void Zones(GameManager g, int grade) {
        g.prefManager.prefData.selectedGrade = grade; g.startInGame();
        for (int zone = 0; zone < 12; zone++) {
            g.ship.rank = g.ship.bossAppRank - 1; g.ship.rankUp(false);
            Check(g.ship.inBossMode, "boss rank threshold");
            g.stageManager.move(); g.stageManager.nextBossAppDist = 0; g.stageManager.move();
            Check(g.stageManager.bossSpecIdx == 1, "boss spawn");
            int bosses = g.ship.bossAppNum;
            for (int i = 0; i < bosses; i++) g.ship.rankUp(true);
            g.enemies.clear();
            for (int i = 0; i < 61; i++) g.stageManager.move();
        }
        Check(g.stageManager.level == 7, "twelve half-level zones");
        g.draw();
    }
    public static void Main() {
        var random = new Rand(); random.setSeed(5489);
        Check(random.nextBits() == -795755684 && random.nextBits() == 581869302 && random.nextBits() == -404620562, "MT19937 reference words");
        random.setSeed(5489); Check(Math.Abs(random.nextFloat(1) - 0.8147237f) < 0.000001f, "unsigned float random");
        var replay = new ReplayData();
        Check(replay.decode("1|-2147483648|1|2|3/16;") && replay.seed == -2147483648, "signed replay seed");
        Check(!replay.decode("1|bad|1|2|3/16;") && !replay.decode("1|2147483648|1|2|3/16;") && !replay.decode("1|1|1|2|3/16"), "invalid replay rejected");
        var g = new GameManager(); g.init_0(); g.start(); g.draw();
        Rules(g);
        for (int grade = 0; grade < 3; grade++) {
            g.prefManager.prefData.selectedGrade = grade; g.prefManager.prefData.selectedLevel = 1;
            g.rand.setSeed(12345); g.startInGame();
            for (int i = 0; i < 600; i++) { Input(g, i); g.move(); }
            string expected = Snapshot(g);
            g.saveLastReplay(); var decoded = new ReplayData(); Check(decoded.decode(Game.savedReplay), "replay serialization");
            g.inGameState._replayData = decoded; g.pad.buttons = 0; g.pad.directions = 0; g.startTitle();
            for (int i = 0; i < 600; i++) g.move();
            Check(expected == Snapshot(g), "replay state grade " + grade.ToString());
            g.draw();
            Zones(g, grade);
            Console.WriteLine("GRADE " + grade.ToString());
        }
        g.prefManager.load("7,7654321,1,7,2,100,1,2,3,200,1,3,0,7");
        Check(g.prefManager.prefData.gradeData[0].hiScore == 7654321 && g.prefManager.prefData.selectedLevel == 7, "scores and unlocked level load");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
