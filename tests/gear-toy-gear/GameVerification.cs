using System;
using System.Collections.Generic;
public static class GtgVerification
{
    static int failures, reported;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static void Near(float a, float b, float tolerance, string label) { Check(Math.Abs(a - b) <= tolerance, label + ": " + a.ToString() + " != " + b.ToString()); }
    static void Section(string name) { Console.WriteLine((failures == reported ? "PASS " : "FAILED ") + name); reported = failures; }
    static void Tick(GtgFrame f, int input, int n = 1)
    {
        Pad.input = input;
        for (int i = 0; i < n; i++) f.Update();
    }
    static int Input(int i) { return (i % 240 < 120 ? 1 : 8) | (i % 600 < 400 ? 512 : 256); }
    static float[] Snapshot(GtgFrame f, int i)
    {
        ActorPools a = f.actors; Player p = a.player;
        return new float[] { i, p.Pos.X, p.Pos.Y, Stage.GameSpeed, Stage.Rank, a.gameState.score, a.gameState.left, a.stage.stageCount, a.enemies.Count, a.middleEnemies.Count, a.bullets.Count };
    }
    // Original C# 0.1, seed 1, alternating movement, acceleration and braking.
    static readonly float[][] reference = new float[][] {
        new float[] { 100, 0, 90, 1.6585149f, 0.55f, 1005, 2, 1, 10, 0, 0 },
        new float[] { 200, 89.13616f, 12.439598f, 2.1826792f, 0.55f, 34059, 2, 1, 1, 0, 3 },
        new float[] { 300, 22.80962f, 87.06159f, 2.5009437f, 0.55f, 38439, 2, 1, 11, 0, 0 },
        new float[] { 400, 12.260886f, 89.16092f, 1.029773f, 0.595f, 42116, 1, 2, 10, 0, 1 },
        new float[] { 500, 60.402454f, 66.71989f, 1.0000565f, 0.595f, 43216, 1, 2, 0, 0, 0 },
        new float[] { 600, 5.684682f, 89.82028f, 1.0004987f, 0.595f, 43316, 1, 2, 0, 0, 0 },
        new float[] { 700, 89.77985f, 6.291274f, 1.6651163f, 0.6355f, 44342, 1, 3, 0, 0, 0 },
        new float[] { 800, 58.028004f, 68.79499f, 1.1353887f, 0.6355f, 44955, 0, 3, 0, 0, 0 },
        new float[] { 900, 87.86851f, 19.471127f, 1.8378619f, 0.6355f, 46604, 0, 3, 0, 0, 0 },
        new float[] { 1000, 42.36875f, 79.40333f, 2.2779148f, 0.67195f, 69812, 0, 4, 5, 1, 1 },
        new float[] { 1100, 56.26398f, 70.24503f, 1.0092169f, 0.67195f, 77807, 0, 4, 2, 0, 1 },
        new float[] { 1200, 89.77636f, 6.340746f, 1.0005523f, 0.67195f, 78107, 0, 4, 4, 0, 0 },
        new float[] { 1300, 17.12627f, 88.35548f, 1.0170465f, 0.704755f, 82166, -1, 5, 5, 0, 0 },
        new float[] { 1400, 17.12627f, 88.35548f, 1.0000005f, 0.704755f, 82166, -1, 5, 11, 0, 0 },
        new float[] { 1500, 17.12627f, 88.35548f, 1.0000005f, 0.704755f, 82166, -1, 5, 2, 0, 1 },
        new float[] { 1600, 0, 66, 1.1566789f, 0.55f, 79, 2, 1, 6, 0, 0 },
        new float[] { 1700, 36.61907f, 82.2134f, 1.8550198f, 0.55f, 1795, 2, 1, 10, 0, 0 },
        new float[] { 1800, 89.89363f, 4.3743625f, 2.3023174f, 0.55f, 35347, 2, 1, 7, 0, 0 },
        new float[] { 1900, 8.101899f, 89.63458f, 2.5734198f, 0.595f, 40031, 2, 2, 10, 0, 10 },
        new float[] { 2000, 41.726597f, 79.74265f, 1.0018531f, 0.595f, 42148, 1, 2, 10, 0, 0 },
        new float[] { 2100, 23.81449f, 86.79211f, 1.000011f, 0.595f, 43248, 1, 2, 0, 0, 0 },
        new float[] { 2200, 72.71958f, 53.02699f, 1.1640145f, 0.6355f, 43400, 1, 3, 0, 0, 0 },
        new float[] { 2300, 89.89972f, 4.2475452f, 1.0902082f, 0.6355f, 44837, 0, 3, 0, 0, 0 },
        new float[] { 2400, 22.624159f, 87.10997f, 1.3787861f, 0.6355f, 45228, 0, 3, 0, 0, 0 },
        new float[] { 2500, 89.73595f, 6.8890357f, 2.0084202f, 0.67195f, 47563, 0, 4, 3, 1, 0 },
        new float[] { 2600, 15.631888f, 88.63207f, 1.3013709f, 0.67195f, 74286, 0, 4, 4, 1, 2 },
        new float[] { 2700, 85.07693f, 29.358427f, 1.0017856f, 0.67195f, 77939, 0, 4, 2, 0, 0 },
        new float[] { 2800, 52.71753f, 72.944244f, 1.1640251f, 0.704755f, 78191, 0, 5, 8, 0, 0 },
        new float[] { 2900, 17.12627f, 88.35548f, 1.0005853f, 0.704755f, 82166, -1, 5, 7, 0, 1 },
        new float[] { 3000, 17.12627f, 88.35548f, 1.0000005f, 0.704755f, 82166, -1, 5, 9, 0, 0 },
    };
    public static void Main()
    {
        // Saving waits until the record has loaded (FrameHost.Begin reads it).
        FrameHost.loaded = true;
        var f = new GtgFrame(); f.LoadContent(); f.Seed(1); f.StartGame();
        for (int i = 1; i <= 3000; i++)
        {
            Tick(f, Input(i));
            if (i % 100 == 0)
            {
                float[] actual = Snapshot(f, i);
                float[] expected = reference[i / 100 - 1];
                for (int j = 0; j < expected.Length; j++) Near(actual[j], expected[j], j >= 1 && j <= 4 ? .00001f : 0, "game " + i.ToString() + " field " + (j + 1).ToString());
            }
        }
        Section("original C# game: 3000 updates");

        f.Seed(1); f.StartGame();
        var recording = new List<float[]>();
        for (int i = 1; i <= 600; i++) { Tick(f, Input(i)); recording.Add(Snapshot(f, i)); }
        Check(f.replay.data.Count > 0 && f.replay.data.Count <= 600, "replay length");
        f.StartTitle(); Check(f.replay.IsAvailable, "replay available");
        for (int i = 1; i <= 600; i++)
        {
            Tick(f, 0);
            float[] actual = Snapshot(f, i);
            float[] expected = recording[i - 1];
            for (int j = 0; j < expected.Length; j++) Near(actual[j], expected[j], .00001f, "replay " + i.ToString() + " field " + (j + 1).ToString());
        }
        Section("600-update replay including acceleration and braking");

        f.StartGame(); ActorPools a = f.actors; Player p = a.player; GameState g = a.gameState; Stage stage = a.stage;
        int bosses = 0; bool oldBoss = false;
        for (int i = 1; i <= 6500; i++)
        {
            // Player.Destroy returns at once while invincibleTicks >= 0, and Update only freezes the player above 180: the player cannot be destroyed and moves as before.
            p.invincibleTicks = 100;
            Tick(f, (i % 240 < 120 ? 1 : 8) | 512);
            if (stage.isBossStage && stage.stageTicks == 420) { Check(a.middleEnemies.Count >= 3, "bosses at stage " + stage.stageCount.ToString()); bosses++; }
            if (oldBoss && !stage.isBossStage) Check(stage.stageTicks == 120, "boss stage " + stage.stageCount.ToString() + " ends with 120 ticks");
            oldBoss = stage.isBossStage;
            if (stage.stageCount >= 15) break;
        }
        Check(stage.stageCount >= 15 && bosses == 2, "two boss stages by stage 15");
        for (int kind = 0; kind <= 8; kind++)
        {
            a.pillars.Clear(); stage.pillarType = (StagePillarType)kind; stage.AddPillars(); Check(a.pillars.pool.Count > 0, "pillar " + kind.ToString());
        }
        f.StartGame(); Tick(f, 0, 2); g = a.gameState;
        Check(g.left == 2 && g.extendScore == 1000000, "initial lives and extend");
        int score = g.score; g.Multiplier = 10; g.AddScoreint(100); Check(g.score == score + 1000, "multiplier");
        g.Multiplier = 1; g.score = 999999; g.AddScoreint(1); Check(g.left == 3 && g.extendScore == 3000000, "first extend");
        g.AddScoreint(2000000); Check(g.left == 4 && g.extendScore == 6000000, "second extend");
        g.left = 9; g.AddScoreint(3000000); Check(g.left == 9 && g.extendScore == 10000000, "extend at nine lives");
        float ticks = stage.ticks;
        Tick(f, 16384); Check(f.storedPauseTicks >= 0, "pause");
        Tick(f, 8, 10); Check(stage.ticks == ticks, "pause freezes the stage");
        Tick(f, 16384); Check(f.storedPauseTicks < 0, "resume");
        Tick(f, 0); g.score = 7654321; g.left = 0; p.invincibleTicks = -1; p.Destroy();
        Check(g.IsInGameOver && g.left == -1, "game over");
        Tick(f, 0, 300); Check(f.state == FrameGameState.Title && a.player.isInReplay && f.record.Scores[0] == 7654321, "title replay after game over records the score");
        // The fake lub API saves nothing: check the text GtgPreference.Save sends to FrameHost's save path, built from the record it saves.
        string saved = "";
        for (int i = 0; i < Record.RankingNum; i++) saved += (i > 0 ? "," : "") + f.record.Scores[i].ToString();
        Check(FrameHost.savePath == "save/gear-toy-gear/scores.txt" && saved.StartsWith("7654321,"), "saved scores");
        var record = new Record(); record.Load();
        GtgPreference.Load(record, "7654321,90000,80000,70000,60000,50000,40000,30000,20000,10000");
        Check(record.Scores[0] == 7654321 && record.Scores[9] == 10000, "scores load");
        GtgPreference.Load(record, "1,2,3,4,5,6,7,8,9,10"); Check(record.Scores[0] == 7654321, "ascending scores are rejected");
        Section("two boss stages, nine pillar types, multipliers, extends, pause, game over and scores");

        var pool = new ActorPool<MiddleEnemy>(3, () => new MiddleEnemy(), e => true, e => { }, i => { });
        var enemy = new MiddleEnemy(); enemy.Pos.X = 10; enemy.TurretIndexes[0] = 7;
        pool.AddT(enemy); enemy.Pos.X = 20; enemy.TurretIndexes[0] = 8; pool.AddT(enemy);
        var copy = pool.Get(0); copy.Pos.X = 99; copy.TurretIndexes[0] = 99;
        Check(pool.Get(0).Pos.X == 10 && pool.Get(0).TurretIndexes[0] == 7, "actor value copies");
        pool.Remove(0); Check(pool.Count == 1 && pool.Get(0).Pos.X == 20 && pool.Get(0).TurretIndexes[0] == 8, "compaction");
        Letter.bars.Clear(); Letter.AddstringVector3floatQuaternionfloat("GEAR TOY GEAR", new Vector3(), 1, Quaternion.Identity, 1);
        Check(Letter.bars.pool.Count > 20, "alphabet glyphs");
        var sound = new Sound(); var cue = sound.GetCue("HomingLaser");
        var listener = new AudioListener(); listener.Position = new Vector3(1, 2, 3);
        var emitter = new AudioEmitter(); emitter.Position = new Vector3(4, 6, 8);
        cue.Apply3D(listener, emitter); cue.Play();
        if (FrameHost.loops.TryGetValue(cue.id, out var loop))
        {
            Check(loop.Sound == 2, "homing laser loop");
            Near(loop.Pan, 3 / (float)Math.Sqrt(34), .00001f, "loop pan");
            emitter.Position.X = 9; cue.Apply3D(listener, emitter); Near(loop.Pan, 8 / (float)Math.Sqrt(89), .00001f, "moved loop pan");
        }
        else Check(false, "homing laser loop");
        cue.Stop(AudioStopOptions.Immediate); Check(!FrameHost.loops.ContainsKey(cue.id), "stopped loop");
        Section("actor value copies, fixed buffers, compaction, alphabet glyphs and spatial cue lifecycle");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
