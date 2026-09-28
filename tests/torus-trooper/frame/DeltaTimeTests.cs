static class DeltaTimeTests
{
    static void Near(float actual, float expected, float tolerance, string name)
    {
        if (!float.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
            throw new Exception($"{name}: {actual} != {expected} ±{tolerance}");
    }
    static IEnumerable<float> Schedule(int hz, double duration)
    {
        double elapsed = 0;
        int index = 0;
        double[] jitter = [1d / 144, 1d / 90, .045, 1d / 120, .004];
        while (elapsed < duration - 1e-8)
        {
            double dt = Math.Min(hz == 0 ? jitter[index++ % jitter.Length] : 1d / hz, duration - elapsed);
            yield return (float)dt;
            elapsed += dt;
        }
    }
    static float[] Snapshot(GameManager g) => [g.ship.pos.x, g.ship.pos.y, g.ship.speed, g.stageManager.level,
        g.inGameState.score, g.bullets.actor.Count(a => a.exists), g.ship.fireShotCnt];
    public static void Run()
    {
        float[] reference = null;
        float? steering = null;
        foreach (int hz in new[] { 60, 90, 120, 144, 0 })
        {
            var g = new GameManager(); g.init_0(); g.start(); g.rand.setSeed(12345); g.startInGame();
            var clock = new SimulationClock();
            g.pad.buttons = PadButton.A;
            g.pad.directions = PadDir.UP;
            g.ship.btnPressed = false;
            foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, .016f, () => { g.ship.move(); g.shots.move(); g.particles.move(); });
            reference ??= [g.ship.pos.y, g.ship.speed, g.ship.relPos.y];
            Near(g.ship.pos.y, reference[0], .5f, $"distance {hz}");
            Near(g.ship.speed, reference[1], .01f, $"acceleration {hz}");
            Near(g.ship.relPos.y, reference[2], .11f, $"forward movement {hz}");
            Near(g.ship.fireShotCnt, 63, 0, $"fire rate {hz}");
            g.shots.clear();
            var shot = g.shots.getInstance(); shot.set_3(true);
            foreach (float dt in Schedule(hz, 1.44)) clock.Advance(dt, .016f, shot.move);
            shot.release();
            Near(shot.chargeCnt, 90, .001f, $"charge {hz}");
            Near(shot.range, 47, .001f, $"charge range {hz}");
            Near(shot.trgSize, 13.6f, .001f, $"charge power {hz}");
            var tween = new PatternTween(); tween.Set(0, 20, 1, 3);
            Near(tween.Value(.5f), 1.05f, .00001f, "fractional barrage tween");

            g.rand.setSeed(12345); g.startInGame();
            g.pad.directions = PadDir.LEFT; g.pad.buttons = 0;
            foreach (float dt in Schedule(hz, .5)) clock.Advance(dt, .016f, g.ship.move);
            steering ??= g.ship.pos.x;
            Near(g.ship.pos.x, steering.Value, .015f, $"steering {hz}");
            g.ship.cnt = 1;
            var center = g.ship.relPos;
            if (!g.ship.checkBulletHit(new Vector(center.x, center.y - 1), new Vector(center.x, center.y + 1)))
                throw new Exception("swept bullet collision");

            g.rand.setSeed(12345); g.startInGame();
            g.pad.buttons = PadButton.A; g.pad.directions = PadDir.UP;
            foreach (float dt in Schedule(hz, 4)) g.Advance(dt);
            var expected = Snapshot(g);
            g.saveLastReplay();
            var replay = new ReplayData();
            if (!replay.decode(Game.savedReplay)) throw new Exception("timed replay decode");
            g.inGameState._replayData = replay;
            g.pad.buttons = 0; g.pad.directions = 0; g.startTitle();
            foreach (float dt in Schedule(120, 4)) g.Advance(dt);
            var actual = Snapshot(g);
            for (int i = 0; i < expected.Length; i++) Near(actual[i], expected[i], .00001f, $"replay {hz} field {i}");
            if (replay.padRecord.hasNext()) throw new Exception($"replay duration {hz}");
            Console.WriteLine($"PASS TT variable time {hz}: distance, acceleration, fire, charge, replay");
            g.close();
        }
        var invalid = new ReplayData();
        foreach (string step in new[] { "0", "-1", "NaN", "Infinity", "1.1", "1E-", "1..2" })
            if (invalid.decode("2|1|1|0|1/16/" + step + ";")) throw new Exception("invalid replay step " + step);
        var longRecord = new PadRecord();
        for (int i = 0; i < 200000; i++)
        {
            SimulationTime.Step = i % 2 == 0 ? .4340278f : .4341f;
            longRecord.add(PadButton.A);
        }
        SimulationTime.Step = 1;
        var loaded = new PadRecord();
        if (!loaded.decode(longRecord.encode()) || loaded.steps.Count != 200000)
            throw new Exception("long variable-time replay");
    }
}
