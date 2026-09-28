using System.Reflection;

static T Field<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
static void Near(float actual, float expected, float tolerance, string name)
{
    if (!float.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
        throw new Exception($"{name}: {actual} != {expected} ±{tolerance}");
}
static IEnumerable<float> Schedule(int hz, double duration)
{
    double elapsed = 0;
    int i = 0;
    double[] jitter = [1d / 144, 1d / 90, .045, 1d / 120, .004];
    while (elapsed < duration - 1e-8)
    {
        double dt = Math.Min(hz == 0 ? jitter[i++ % jitter.Length] : 1d / hz, duration - elapsed);
        yield return (float)dt;
        elapsed += dt;
    }
}
static float[] Snapshot(MmFrame frame)
{
    var player = Field<Player>(frame, "player");
    return [player.Pos.X, player.Pos.Y, player.Pos.Z, Field<float>(player, "deg"), Field<float>(player, "baseRank"),
        Field<int>(player, "score"), Field<int>(player, "left"), Field<int>(player, "shotCnt"), Field<ShotPool>(frame, "shots").Length(), Field<BallPool>(frame, "balls").Length(), Field<BulletPool>(frame, "bullets").Length()];
}
var saveDirectory = Path.Combine(Path.GetTempPath(), "mm-test-" + Guid.NewGuid());
Environment.SetEnvironmentVariable("XDG_DATA_HOME", saveDirectory);
bool passed = false;
int runtime = Lub.Run(null, null, _ =>
{
try
{
    var pad = new Pad();
    Pad.Read(new XrInput { Active = true, StickX = .1f }, null);
    Near(pad.ThumbStickLeft.Length(), 0, 0, "deadzone");
    Pad.Read(new XrInput { Active = true, StickX = 1, StickY = 1, Trigger = .4f }, new XrInput { Active = true, Primary = true, Secondary = true, Trigger = .7f });
    Near(pad.ThumbStickLeft.Length(), 1, .00001f, "stick limit");
    Near(pad.LeftTrigger, .4f, 0, "left trigger"); Near(pad.RightTrigger, .7f, 0, "right trigger");
    if (!pad.ButtonA || pad.ButtonBack) throw new Exception("fire/back mapping");
    Pad.Read(null, new XrInput { Active = true, Secondary = true, Menu = true }, true);
    if (!pad.ButtonBack || !pad.ButtonStart || pad.ButtonA) throw new Exception("pause mapping");
    Pad.Read(null, null);
    Near(pad.ThumbStickLeft.Length(), 0, 0, "disconnected stick");
    var particleFrame = new MmFrame(); particleFrame.LoadContent();
    var particlePool = Field<ParticlePool>(particleFrame, "particles");
    var particle = new Particle { Pos = new Vector3(2, 3, 4), Vel = new Vector3(1, 0, 0),
        Dir = Quaternion.Identity, Cnt = 32, Size = .5f, R = 100, G = 150, B = 200 };
    particlePool.Add(particle); particlePool.Update(); particlePool.ClearAll();
    for (int i = 0; i < 256; i++) particlePool.Add(particle);
    long particleBytes = GC.GetAllocatedBytesForCurrentThread();
    particlePool.Update();
    particleBytes = GC.GetAllocatedBytesForCurrentThread() - particleBytes;
    if (particleBytes > 400 * 1024) throw new Exception($"particle update allocated {particleBytes} bytes");
    var vertices = Field<TriangleListShape>(particlePool, "shape").Verts;
    Near(vertices[0].Position.X, 3.45f, .00001f, "particle first vertex x");
    Near(vertices[0].Position.Z, 3.95f, .00001f, "particle first vertex z");
    Near(vertices[1].Position.X, 2.95f, .00001f, "particle rotated vertex x");
    Near(vertices[1].Position.Z, 3.45f, .00001f, "particle rotated vertex z");
    Near(vertices[2].Position.X, -6.55f, .00001f, "particle trail x");
    Near(vertices[2].Position.Z, 4.45f, .00001f, "particle trail z");
    if (vertices[0].Color.R != 100 || vertices[0].Color.A != 80 || vertices[2].Color.A != 0)
        throw new Exception("particle vertex colors");
    Console.WriteLine($"PASS particle geometry and allocation: {particleBytes} bytes for 256 particles");
    foreach (int hz in new[] { 60, 90, 120, 144, 0 })
    {
        var clock = new SimulationClock();
        var physics = new CircleParticle(); physics.Clear(); physics.Velocity = new Vector3(.5f, -.25f, .1f);
        var reference = physics.Copy();
        for (int i = 0; i < 120; i++) { reference.AddMasslessForce(new Vector3(.2f, .1f, 0)); reference.Update(); }
        foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, 1f / 60, () => { physics.AddMasslessForce(new Vector3(.2f, .1f, 0)); physics.Update(); });
        Near(physics.Pos.X, reference.Pos.X, .005f, $"physics x {hz}");
        Near(physics.Pos.Y, reference.Pos.Y, .005f, $"physics y {hz}");
        Near(physics.Velocity.X, reference.Velocity.X, .0001f, $"velocity {hz}");
        var frame = new MmFrame(); frame.LoadContent(); frame.Seed(1); frame.StartInGame();
        var player = Field<Player>(frame, "player");
        Set(player, "restartCnt", -1f); Set(player, "storedPos", new Vector3(0, 0, 1)); Set(player, "storedPos2", new Vector2());
        Pad.Read(new XrInput { Active = true, StickX = .3f }, null);
        float startX = player.Pos.X;
        foreach (float dt in Schedule(hz, 1)) clock.Advance(dt, 1f / 60, player.Update);
        Near(player.Pos.X - startX, pad.ThumbStickLeft.X * .5f * 60, .001f, $"movement {hz}");
        Pad.Read(null, new XrInput { Active = true, Trigger = .8f });
        foreach (float dt in Schedule(hz, 1)) clock.Advance(dt, 1f / 60, player.Update);
        Near(Field<float>(player, "deg"), .025f * .8f * 60, .0001f, $"turn {hz}");
        Set(player, "fireCnt", 0); Set(player, "aPressed", true);
        var shots = Field<ShotPool>(frame, "shots"); shots.Clear();
        Pad.Read(null, new XrInput { Active = true, Primary = true });
        int fired = 0;
        foreach (float dt in Schedule(hz, 1)) clock.Advance(dt, 1f / 60, () => { player.Update(); fired += shots.Length(); shots.Clear(); });
        Near(fired, 62, 0, $"fire cadence {hz}");
        var bullet = new Bullet { IsActivated = true, IsTop = true, Cnt = 30, Pos = new Vector3(0, 0, 1), Vel = new Vector3(.5f, 0, 0), Speed = 1, Dir = Quaternion.Identity };
        var bullets = Field<BulletPool>(frame, "bullets");
        foreach (float dt in Schedule(hz, 1)) clock.Advance(dt, 1f / 60, () => bullets.UpdateT(bullet));
        Near(bullet.Pos.X, 30, .001f, $"bullet travel {hz}");
        float ticks = 0;
        foreach (float dt in Schedule(hz, 2)) clock.Advance(dt, 2f / 60, () => ticks += SimulationTime.Step);
        Near(ticks, 60, .001f, $"intentional slowdown {hz}");
        frame.Seed(3); frame.StartInGame(); Pad.Read(new XrInput { Active = true, StickX = .2f, StickY = .2f }, null);
        foreach (float dt in Schedule(hz, 4)) frame.Advance(dt);
        var saved = Snapshot(frame);
        frame.StartTitle(); Pad.Read(null, null);
        foreach (float dt in Schedule(120, 4)) frame.Advance(dt);
        var actual = Snapshot(frame);
        for (int i = 0; i < saved.Length; i++) Near(actual[i], saved[i], .00001f, $"replay {hz} field {i}");
        frame.Seed(4); frame.StartInGame();
        Pad.Read(new XrInput { Active = true, StickY = .5f }, new XrInput { Active = true, Primary = true, Trigger = .6f });
        var field = Field<Field>(frame, "field");
        foreach (float dt in Schedule(hz, 15)) { frame.Advance(dt); field.SetEyePosition(); }
        saved = Snapshot(frame);
        frame.StartTitle(); Pad.Read(null, null);
        foreach (float dt in Schedule(90, 15)) { frame.Advance(dt); field.SetEyePosition(); }
        actual = Snapshot(frame);
        for (int i = 0; i < saved.Length; i++) Near(actual[i], saved[i], .00001f, $"combat replay {hz} field {i}");
        frame.Seed(5); frame.StartInGame();
        Pad.Read(new XrInput { Active = true, StickY = .2f }, new XrInput { Active = true, Primary = true });
        foreach (float dt in Schedule(hz, 1)) frame.Advance(dt);
        Pad.Read(null, new XrInput { Active = true, Menu = true }); frame.Advance(1f / 144);
        Pad.Read(null, null);
        foreach (float dt in Schedule(hz, 1.123)) frame.Advance(dt);
        Pad.Read(null, new XrInput { Active = true, Menu = true }); frame.Advance(1f / 144);
        Pad.Read(new XrInput { Active = true, StickY = .2f }, new XrInput { Active = true, Primary = true });
        foreach (float dt in Schedule(hz, 1)) frame.Advance(dt);
        saved = Snapshot(frame);
        double recordedSeconds = Field<Replay>(frame, "replay").data.Sum(value => (double)value.Seconds);
        frame.StartTitle(); Pad.Read(null, null);
        foreach (float dt in Schedule(120, recordedSeconds)) frame.Advance(dt);
        actual = Snapshot(frame);
        for (int i = 0; i < saved.Length; i++) Near(actual[i], saved[i], .00001f, $"paused replay {hz} field {i}");
        frame.StartInGame(); Pad.Read(null, null); frame.Advance(1f / 60);
        var beforePause = Snapshot(frame);
        Pad.Read(null, new XrInput { Active = true, Menu = true }); frame.Advance(1f / 60);
        if (frame.PauseCnt < 0) throw new Exception("pause did not engage");
        Pad.Read(new XrInput { Active = true, StickX = 1 }, null);
        foreach (float dt in Schedule(hz, 1)) frame.Advance(dt);
        actual = Snapshot(frame);
        for (int i = 0; i < beforePause.Length; i++) Near(actual[i], beforePause[i], 0, $"paused {hz} field {i}");
        Pad.Read(null, new XrInput { Active = true, Menu = true }); frame.Advance(1f / 60);
        if (frame.PauseCnt >= 0) throw new Exception("resume failed");
        Pad.Read(null, null); frame.Advance(1f / 60);
        Pad.Read(new XrInput { Active = true, StickY = .5f }, new XrInput { Active = true, Primary = true }); frame.Advance(1f / 144);
        if (Field<GrenadePool>(frame, "grenades").Length() == 0 || Field<float>(player, "dashCnt") <= 0) throw new Exception("A dash/grenade failed");
        for (int i = 0; i < 110; i++) player.GetBonus(player.Pos);
        Pad.Read(new XrInput { Active = true, Trigger = 1 }, new XrInput { Active = true, Trigger = 1 });
        foreach (float dt in Schedule(hz, .1)) frame.Advance(dt);
        if (!player.IsInHyper) throw new Exception("both-trigger hyper failed");
        Set(player, "left", 0); typeof(Player).GetMethod("Destroy", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(player, null); Pad.Read(null, null);
        foreach (float dt in Schedule(hz, 11)) frame.Advance(dt);
        if (frame.IsInGame) throw new Exception("gameover did not return to title");
        Pad.Read(null, new XrInput { Active = true, Primary = true }); frame.Advance(1f / 60);
        if (!frame.IsInGame || player.IsInGameover) throw new Exception("replay/restart failed");
        Console.WriteLine($"PASS MM variable time {hz}: physics, movement, turn, fire, bullet, slowdown, replay, pause, dash, hyper, gameover/restart");
    }
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    RenderingTests.Run();
    passed = true;
}
finally
{
    Lub.Quit();
    if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
}

}, null, ["--backend", "vulkan"]);
if (runtime != 0 || !passed) throw new Exception("MM Frame tests failed");
