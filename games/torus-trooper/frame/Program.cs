using System.Diagnostics;
using static Lub;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
long report = Stopwatch.GetTimestamp();
int frames = 0;
bool profile = Environment.GetEnvironmentVariable("LUB_PROFILE") == "1";
return Lub.Run(Game.OnInit, null, dt =>
{
    Game.OnFrame(dt);
    if (!profile) return;
    frames++;
    double seconds = Stopwatch.GetElapsedTime(report).TotalSeconds;
    if (seconds >= 5)
    {
        Console.WriteLine($"TT fps={frames / seconds:F2} focused={Xr.Focused()} game={Game.manager.state == Game.manager.inGameState} speed={Game.manager.ship._speed:F3} charge={Game.manager.ship.chargingShot != null} allocated={GC.GetTotalAllocatedBytes()} gc={GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)} audio={Audio.Info().Device}");
        frames = 0; report = Stopwatch.GetTimestamp();
    }
}, Game.OnQuit, args);
