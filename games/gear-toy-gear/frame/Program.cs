using System.Diagnostics;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
long report = Stopwatch.GetTimestamp();
int frames = 0;
float leftTrigger = 0, rightTrigger = 0;
bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
return Lub.Run(FrameApp.OnInit, null, dt =>
{
    var left = Lub.Xr.Input(0);
    var right = Lub.Xr.Input(1);
    FrameApp.DrawInput(dt, left, right, Lub.Xr.Focused());
    if (!profile) return;
    leftTrigger = MathF.Max(leftTrigger, left?.Trigger ?? 0);
    rightTrigger = MathF.Max(rightTrigger, right?.Trigger ?? 0);
    frames++;
    double seconds = Stopwatch.GetElapsedTime(report).TotalSeconds;
    if (seconds >= 2)
    {
        Console.WriteLine($"GTG fps={frames / seconds:F2} focused={Lub.Xr.Focused()} game={Game.frame.IsInGame} pause={Game.frame.PauseTicks} lt_max={leftTrigger:F3} rt_max={rightTrigger:F3} speed={Stage.GameSpeed:F3} allocated={GC.GetTotalAllocatedBytes()} gc={GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)} audio={Lub.Audio.Info().Device}");
        frames = 0;
        leftTrigger = rightTrigger = 0;
        report = Stopwatch.GetTimestamp();
    }
}, FrameApp.OnQuit, args);
