using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

static T Field<T>(object value, string name) => (T)value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
static void Near(double actual, double expected, double tolerance = .00001)
{
    if (Math.Abs(actual - expected) > tolerance) throw new Exception($"{actual} != {expected}");
}
static double[] Snapshot(GtgFrame frame, int tick)
{
    var actors = Field<ActorPools>(frame, "actors");
    var player = Field<Player>(actors, "player");
    var state = Field<GameState>(actors, "gameState");
    return [tick, player.Pos.X, player.Pos.Y, Stage.GameSpeed, Stage.Rank,
        Field<int>(state, "score"), Field<int>(state, "left"), Field<int>(Field<Stage>(actors, "stage"), "stageCount"),
        Field<EnemyPool>(actors, "enemies").Count, Field<MiddleEnemyPool>(actors, "middleEnemies").Count, Field<BulletPool>(actors, "bullets").Count];
}
var saveDirectory = Path.Combine(Path.GetTempPath(), "gtg-test-" + Guid.NewGuid());
Environment.SetEnvironmentVariable("XDG_DATA_HOME", saveDirectory);
try
{
    var pad = new Pad();
    Pad.Read(new XrInput { Active = true, StickX = .1f }, null);
    Near(pad.ThumbStickLeft.Length(), 0);
    Pad.Read(new XrInput { Active = true, StickX = 1, StickY = 1, Trigger = .4f }, new XrInput { Active = true, Trigger = .7f });
    Near(pad.ThumbStickLeft.Length(), 1);
    Near(pad.LeftTrigger, .4f); Near(pad.RightTrigger, .7f);
    Pad.Read(null, null);
    Near(pad.ThumbStickLeft.Length(), 0); Near(pad.LeftTrigger, 0);
    Pad.Read(new XrInput { Active = true, Trigger = 1 }, new XrInput { Active = true, Trigger = 1, Menu = true });
    Near(pad.CheckGameStartPressed(), -1);
    Pad.Read(null, new XrInput { Active = true, Primary = true, Secondary = true, StickX = 1 });
    Near(pad.CheckGameStartPressed(), 4);
    Near(pad.ThumbStickRight.Length(), 0);
    if (pad.ButtonA || pad.ButtonB || pad.ButtonBack) throw new Exception("Menu buttons affect gameplay");
    Pad.Read(null, new XrInput { Active = true, Secondary = true }, true);
    if (!pad.ButtonBack) throw new Exception("Cannot leave paused game");
    var reference = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "reference.lua"));
    reference = reference.Split("local reference={")[1].Split("}\nlocal f=")[0];
    var expected = Regex.Matches(reference, @"\{([^}]+)\}")
        .Select(m => m.Groups[1].Value.Split(',').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray()).ToArray();
    var frame = new GtgFrame();
    frame.LoadContent(); frame.Seed(1); frame.StartGame();
    for (int tick = 1; tick <= 300; tick++)
    {
        Pad.Read(new XrInput { Active = true, StickX = tick % 240 < 120 ? 0 : 1, StickY = tick % 240 < 120 ? 1 : 0 },
            new XrInput { Active = true, Trigger = 1 });
        frame.Update();
        if (tick % 100 != 0) continue;
        var actual = Snapshot(frame, tick);
        for (int i = 0; i < actual.Length; i++)
        {
            try { Near(actual[i], expected[tick / 100 - 1][i], i >= 1 && i <= 4 ? .00002 : 0); }
            catch (Exception e) { throw new Exception($"tick={tick} field={i}: {e.Message}"); }
        }
    }
    frame.Seed(1); frame.StartGame();
    var recording = new List<double[]>();
    for (int tick = 1; tick <= 600; tick++)
    {
        Pad.Read(new XrInput { Active = true, StickX = .5f, StickY = .3f, Trigger = tick > 400 ? .8f : 0 },
            new XrInput { Active = true, Trigger = tick <= 400 ? .65f : 0 });
        frame.Update(); recording.Add(Snapshot(frame, tick));
    }
    frame.StartTitle(); Pad.Read(null, null);
    for (int tick = 1; tick <= 600; tick++)
    {
        frame.Update(); var actual = Snapshot(frame, tick);
        for (int i = 0; i < actual.Length; i++) Near(actual[i], recording[tick - 1][i]);
    }
    Console.WriteLine("PASS Frame input, 300 original-game updates, 600 analog replay updates");
}
finally
{
    if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
}

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var parameters = new List<float>(new float[144]); parameters[33] = 6;
var vertex = File.ReadAllText("game.vs.slang");
var fragment = File.ReadAllText("game.fs.slang");
var readback = Lub.Gfx.Readback("sampler-test");
int rendered = 0;
bool verified = false;
int runtime = Lub.Run(null, null, dt =>
{
    var shader = Lub.Gfx.UseShader("gtg-sampler-test", vertex, fragment, 1);
    var source = Lub.Gfx.UseTexture("source", 1, 1, Lub.Gfx.PixelFormat.Rgba8, new List<int> { 64, 128, 192, 255 }, 1);
    var target = Lub.Gfx.UseTexture("target", 4, 4, Lub.Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
    var bindings = new Dictionary<string, object>
    {
        ["surface"] = source,
        ["vertices"] = Lub.Gfx.UseBuffer("vertices", Lub.Gfx.BufferType.Storage, new List<float>(new float[24]), 1),
        ["parameters"] = Lub.Gfx.UseBuffer("parameters", Lub.Gfx.BufferType.Storage, parameters, 1),
        ["eye"] = Lub.Gfx.UseBuffer("eye", Lub.Gfx.BufferType.Storage, new List<float>(new float[16]), 1)
    };
    Lub.Gfx.BeginPass(new PassOpts { Target = target, ClearColor = [0, 0, 0, 0] });
    Lub.Gfx.Draw(3, bindings, new DrawOpts { Shader = shader, Depth = false, Cull = Lub.Gfx.Cull.None, Blend = Lub.Gfx.Blend.None });
    Lub.Gfx.EndPass();
    Lub.Gfx.ReadTexture(readback, target, rendered++ == 0 ? 1 : null, out var status, out var bytes,
        out int width, out int height, out _, out _, out _, out _, out var error);
    if (error != null) throw new Exception(error);
    if (status == Lub.Gfx.ReadbackStatus.Ready)
    {
        if (width != 4 || height != 4) throw new Exception("Sampler readback size");
        var pixel = bytes.AsSpan();
        Near(pixel[0], 64, 1); Near(pixel[1], 128, 1); Near(pixel[2], 192, 1); Near(pixel[3], 255, 1);
        verified = true;
        Lub.Quit();
    }
    if (rendered >= 120) Lub.Quit();
}, null, ["--backend", "vulkan"]);
if (runtime != 0 || !verified) throw new Exception("Native texture sampler did not render");
Console.WriteLine("PASS native texture sampler readback");
