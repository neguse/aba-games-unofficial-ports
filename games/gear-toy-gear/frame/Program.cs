global using Host = FrameHost;
using System.Diagnostics;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.Run(Game.OnInit, null, FrameApp.Draw, Game.OnQuit, args);

public static class FrameApp
{
    static long report = Stopwatch.GetTimestamp();
    static int frames;
    static float leftTrigger, rightTrigger;
    static readonly bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
    public static void Draw(float dt)
        => Draw(dt, Lub.Xr.GetInput(0), Lub.Xr.GetInput(1), Lub.Xr.Focused());
    public static void Draw(float dt, XrInput leftHand, XrInput rightHand, bool focused)
    {
        FrameHost.Begin();
        var left = Lub.Xr.GetView(0, .05f, 100);
        var right = Lub.Xr.GetView(1, .05f, 100);
        if (left == null || right == null)
        {
            report = Stopwatch.GetTimestamp();
            frames = 0;
            return;
        }
        Pad.Read(leftHand, rightHand, Game.frame.IsInGame && Game.frame.PauseTicks >= 0);
        leftTrigger = MathF.Max(leftTrigger, leftHand?.Trigger ?? 0);
        rightTrigger = MathF.Max(rightTrigger, rightHand?.Trigger ?? 0);
        if (focused) Game.OnFrame(dt);
        else Game.frame.Draw();
        GtgRender.Present(left, right);
        FrameHost.End(focused);
        if (!profile) return;
        frames++;
        double seconds = Stopwatch.GetElapsedTime(report).TotalSeconds;
        if (seconds >= 2)
        {
            Console.WriteLine($"GTG fps={frames / seconds:F2} focused={Lub.Xr.Focused()} game={Game.frame.IsInGame} pause={Game.frame.PauseTicks} lt_max={leftTrigger:F3} rt_max={rightTrigger:F3} speed={Stage.GameSpeed:F3} allocated={GC.GetTotalAllocatedBytes()} gc={GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)} audio={Lub.Audio.Info().Device}");
            frames = 0;
            leftTrigger = rightTrigger = 0;
            report = Stopwatch.GetTimestamp();
        }
    }
}
