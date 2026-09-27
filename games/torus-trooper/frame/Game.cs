global using Host = FrameHost;
using System.Diagnostics;
using static Lub;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.Run(Game.OnInit, null, Game.OnFrame, Game.OnQuit, args);

public static class Game
{
    public static GameManager manager;
    public static string savedReplay = "";
    public static ShaderRef shader;
    static float elapsed;
    static readonly string vertex = File.ReadAllText("mesh.vs.slang");
    static readonly string fragment = File.ReadAllText("mesh.fs.slang");
    static long report = Stopwatch.GetTimestamp();
    static int frames;
    static readonly bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        manager = new GameManager();
        manager.init_0(); manager.start();
        manager.rand.setSeed(System.Random.Shared.Next());
        FrameHost.Load();
    }
    public static void OnFrame(float dt)
        => OnFrame(dt, Xr.GetInput(0), Xr.GetInput(1), Xr.Focused());
    public static void OnFrame(float dt, XrInput leftInput, XrInput rightInput, bool focused)
    {
        if (profile) Profiler.BeginScope("tt.audio.begin");
        FrameHost.Begin();
        if (profile) Profiler.EndScope("tt.audio.begin");
        var left = Xr.GetView(0, .05f, 500);
        var right = Xr.GetView(1, .05f, 500);
        if (left == null || right == null)
        {
            report = Stopwatch.GetTimestamp(); frames = 0;
            return;
        }
        FrameControls.Read(manager, leftInput, rightInput);
        bool wasTitle = manager.state == manager.titleState;
        if (profile) Profiler.BeginScope("tt.update");
        if (focused)
        {
            elapsed += Math.Min(dt, .1f);
            while (elapsed >= .016f)
            {
                manager.move();
                elapsed -= .016f;
            }
        }
        if (wasTitle && manager.state == manager.inGameState) TtRender.Recenter();
        if (profile) Profiler.EndScope("tt.update");
        if (profile) Profiler.BeginScope("tt.shader");
        shader = Gfx.UseShader("tt-xr", vertex, fragment, 1);
        if (profile) Profiler.EndScope("tt.shader");
        if (shader == null) return;
        if (profile) Profiler.BeginScope("tt.geometry");
        TtRender.Begin();
        manager.state.draw(Transform.Identity(), null, Gfx.Blend.Additive, Gfx.Cull.None, 1);
        TtRender.Hud = true;
        manager.state.drawFront(Transform.Ortho(), null, Gfx.Blend.Additive, Gfx.Cull.None, 1);
        if (profile) Profiler.EndScope("tt.geometry");
        if (profile) Profiler.BeginScope("tt.submit");
        TtRender.Present(manager.ship, left, right);
        if (profile) Profiler.EndScope("tt.submit");
        if (profile) Profiler.BeginScope("tt.audio.end");
        FrameHost.End(focused, dt);
        if (profile) Profiler.EndScope("tt.audio.end");
        if (!profile) return;
        frames++;
        double seconds = Stopwatch.GetElapsedTime(report).TotalSeconds;
        if (seconds >= 5)
        {
            Console.WriteLine($"TT fps={frames / seconds:F2} focused={Xr.Focused()} game={manager.state == manager.inGameState} speed={manager.ship._speed:F3} charge={manager.ship.chargingShot != null} allocated={GC.GetTotalAllocatedBytes()} gc={GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)} audio={Audio.Info().Device}");
            frames = 0; report = Stopwatch.GetTimestamp();
        }
    }
    public static void OnQuit()
    {
        manager.prefManager.save(); manager.close();
        FrameHost.SaveReplay();
    }
}
