global using Host = FrameHost;
using System;
using static Lub;

public static class Game
{
    public static GtgFrame frame;
    static readonly XrView desktop = new();
    static readonly bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        frame = new GtgFrame();
        frame.LoadContent();
        FrameHost.Load();
    }

    public static void OnFrame(float dt)
        => DrawInput(dt, Xr.Input(0), Xr.Input(1), Xr.Focused());

    public static void DrawInput(float dt, XrInput leftHand, XrInput rightHand, bool focused)
    {
        FrameHost.Begin();
        var left = Xr.View(0, .05f, 100);
        var right = Xr.View(1, .05f, 100);
        bool immersive = Xr.Active();
        if (immersive)
        {
            if (left == null || right == null) return;
            Pad.Read(leftHand, rightHand, frame.IsInGame && frame.PauseTicks >= 0);
        }
        else
        {
            Gfx.Size(out int width, out int height);
            desktop.Width = width; desktop.Height = height;
            left = desktop; right = null; focused = true;
            Pad.ReadDesktop();
        }
        if (focused)
        {
            if (profile) Profiler.BeginScope("gtg.update");
            frame.Advance(dt);
            if (profile) Profiler.EndScope("gtg.update");
        }
        GtgRender.Immersive = immersive;
        frame.Draw();
        GtgRender.Present(left, right);
        FrameHost.End(focused);
    }

    public static void OnQuit()
    {
        frame.SaveScores();
    }
}
