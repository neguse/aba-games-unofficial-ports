global using Host = FrameHost;
using System;
using static Lub;

public static class Game
{
    public static GameManager manager;
    public static string savedReplay = "";
    public static ShaderRef shader;
    static string vertex, fragment;
    static readonly bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        manager = new GameManager();
        manager.init_0(); manager.start();
        manager.rand.setSeed(TinySystem.Random.Next());
        FrameHost.Load();
    }
    public static void OnFrame(float dt)
        => DrawInput(dt, Xr.GetInput(0), Xr.GetInput(1), Xr.Focused());
    public static void DrawInput(float dt, XrInput leftInput, XrInput rightInput, bool focused)
    {
        if (profile) Profiler.BeginScope("tt.audio.begin");
        FrameHost.Begin();
        if (profile) Profiler.EndScope("tt.audio.begin");
        var left = Xr.GetView(0, .05f, 500);
        var right = Xr.GetView(1, .05f, 500);
        if (left == null || right == null) return;
        FrameControls.Read(manager, leftInput, rightInput);
        bool wasTitle = manager.state == manager.titleState;
        if (profile) Profiler.BeginScope("tt.update");
        if (focused)
            manager.Advance(dt);
        if (wasTitle && manager.state == manager.inGameState) TtRender.Recenter();
        if (profile) Profiler.EndScope("tt.update");
        if (profile) Profiler.BeginScope("tt.shader");
        if (vertex == null)
        {
            Io.LoadText("mesh.vs.slang", out var source, out _, out _, out _);
            vertex = source;
        }
        if (fragment == null)
        {
            Io.LoadText("mesh.fs.slang", out var source, out _, out _, out _);
            fragment = source;
        }
        if (vertex == null || fragment == null) { if (profile) Profiler.EndScope("tt.shader"); return; }
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
    }
    public static void OnQuit()
    {
        manager.prefManager.save(); manager.close();
        FrameHost.SaveReplay();
    }
}
