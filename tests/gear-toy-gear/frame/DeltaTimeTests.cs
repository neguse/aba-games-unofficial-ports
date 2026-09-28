using System.Reflection;

static class DeltaTimeTests
{
    static T Field<T>(object value, string name) => (T)value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
    static void Set(object value, string name, object data) => value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(value, data);
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
    public static void Run()
    {
        float? acceleration = null;
        foreach (int hz in new[] { 60, 90, 120, 144, 0 })
        {
            var frame = new GtgFrame(); frame.LoadContent(); frame.Seed(1); frame.StartGame();
            var actors = Field<ActorPools>(frame, "actors");
            var player = Field<Player>(actors, "player");
            var shots = Field<ShotPool>(actors, "shots");
            var clock = new SimulationClock();
            Pad.Read(new XrInput { Active = true, StickX = .3f }, null);
            float startX = player.Pos.X;
            int shotCount = 0;
            foreach (float dt in Schedule(hz, 1))
                clock.Advance(dt, 1f / 60, () => { player.Update(); shotCount += shots.Count; shots.Clear(); });
            Near(player.Pos.X - startX, new Pad().ThumbStickLeft.X * 3 * 60, .001f, $"movement {hz}");
            Near(shotCount, 150, 0, $"autofire {hz}");
            player.IncrementAccel();
            Pad.Read(null, new XrInput { Active = true, Trigger = 1 });
            foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, 1f / 60, player.Update);
            acceleration ??= Stage.GameSpeed;
            Near(Stage.GameSpeed, acceleration.Value, .01f, $"acceleration {hz}");
            Pad.Read(new XrInput { Active = true, Trigger = 1 }, null);
            foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, 1f / 60, player.Update);
            Near(Stage.GameSpeed, 1, .02f, $"braking {hz}");

            var bullet = new Bullet { Pos = new Vector3(0, 0, -100), Vel = new Vector3(0, 0, 2), Scale = 1, Orientation = Quaternion.Identity };
            var pool = Field<BulletPool>(actors, "bullets");
            Stage.GameSpeed = 1;
            foreach (float dt in Schedule(hz, .5)) clock.Advance(dt, 1f / 60, () => pool.UpdateT(bullet));
            Near(bullet.Pos.Z, -40, .001f, $"bullet speed {hz}");
            Near(bullet.Orientation.Z, MathF.Sin(1.5f), .0001f, $"bullet rotation {hz}");

            player.Pos = new Vector3();
            var state = Field<GameState>(actors, "gameState");
            int lives = Field<int>(state, "left");
            var crossing = new Bullet { Pos = new Vector3(0, 0, -2), Vel = new Vector3(0, 0, 4), Scale = 8, Orientation = Quaternion.Identity };
            bool alive = true;
            clock.Advance(.075f, 1f / 60, () => { if (alive) alive = pool.UpdateT(crossing); });
            Near(Field<int>(state, "left"), lives - 1, 0, $"collision during stall {hz}");
            var stage = Field<Stage>(actors, "stage");
            Set(stage, "isBossStage", true); Set(stage, "stageTicks", 421.1f);
            for (int i = 0; i < 3; i++) clock.Advance(1f / 120, 1f / 60, stage.Update);
            if (Field<MiddleEnemyPool>(actors, "middleEnemies").Count == 0) throw new Exception("fractional boss threshold skipped");

            stage.Start(1); state.Initialize(); clock.Reset();
            foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, 1f / 60, stage.Update);
            Near(Field<int>(state, "score"), 120, 0, $"time score {hz}");

            frame.Seed(3); frame.StartGame();
            Pad.Read(new XrInput { Active = true, StickX = .2f, StickY = .2f }, null);
            var times = Schedule(hz, 4).ToArray();
            foreach (float dt in times) frame.Advance(dt);
            var replay = Field<Replay>(frame, "replay");
            var saved = new[] { player.Pos.X, player.Pos.Y, Stage.GameSpeed, Stage.Rank,
                Field<GameState>(actors, "gameState").IsInGameOver ? 1 : 0, Field<EnemyPool>(actors, "enemies").Count };
            frame.StartTitle(); Pad.Read(null, null);
            foreach (float dt in Schedule(120, 4)) frame.Advance(dt);
            var actual = new[] { player.Pos.X, player.Pos.Y, Stage.GameSpeed, Stage.Rank,
                Field<GameState>(actors, "gameState").IsInGameOver ? 1 : 0, Field<EnemyPool>(actors, "enemies").Count };
            for (int i = 0; i < saved.Length; i++) Near(actual[i], saved[i], .00001f, $"replay {hz} field {i}");
            if (replay.HasNext()) throw new Exception($"replay duration {hz}");
            Console.WriteLine($"PASS GTG variable time {hz}: movement, fire, acceleration, brake, bullet, replay");
        }
        var c = new SimulationClock();
        float total = 0, largest = 0;
        c.Advance(.075f, 1f / 60, () => { total += SimulationTime.Step; largest = Math.Max(largest, SimulationTime.Step); });
        Near(total, 4.5f, .00001f, "stall catch-up"); Near(largest, 1, 0, "bounded collision step");
        c.Advance(0, 1f / 60, () => throw new Exception("zero dt update"));
    }
}
