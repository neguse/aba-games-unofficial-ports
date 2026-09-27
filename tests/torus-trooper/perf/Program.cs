using System.Diagnostics;
using static Lub;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
int seconds = args.Length > 0 ? int.Parse(args[0]) : 60;
int grade = args.Length > 1 ? int.Parse(args[1]) : 2;
int level = args.Length > 2 ? int.Parse(args[2]) : 10;
using var samples = new StreamWriter("frames.csv");
samples.WriteLine("frame,seconds,cpuMs,allocated,gen0,gen1,gen2,game,charge,first");
var left = new XrInput { Active = true };
var right = new XrInput { Active = true };
int frame = 0, playingFrames = 0;
long start = 0;
int result = 1;
int runtime = Lub.Run(() =>
{
    Game.OnInit();
    Game.manager.rand.setSeed(1234);
    Game.manager.prefManager.prefData.selectedGrade = grade;
    Game.manager.prefManager.prefData.selectedLevel = level;
    Game.manager.titleManager.start();
}, null, dt =>
{
    try
    {
        if (Xr.GetView(0, .05f, 500) == null) return;
        if (start == 0)
        {
            start = Stopwatch.GetTimestamp();
            Console.WriteLine($"BENCH clock={(double)start / Stopwatch.Frequency:F6}");
        }
        double elapsed = Stopwatch.GetElapsedTime(start).TotalSeconds;
        var game = Game.manager;
        bool playing = game.state == game.inGameState && !game.ship.isGameOver;
        if (playing) playingFrames++;
        int phase = frame % 1440;
        left.StickX = playing ? (phase / 240 % 2 == 0 ? -.8f : .8f) : 0;
        left.StickY = playing ? .8f : 0;
        left.Trigger = playing && phase >= 480 && phase < 720 ? .9f : 0;
        right.Trigger = playing && playingFrames % 360 >= 30 ? .9f : 0;
        right.Primary = !playing && frame % 90 < 30;
        right.StickClick = phase == 960 || phase == 1200;
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        long before = Stopwatch.GetTimestamp();
        Game.OnFrame(dt, left, right, true);
        double cpu = Stopwatch.GetElapsedTime(before).TotalMilliseconds;
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
        samples.WriteLine($"{frame},{elapsed:F6},{cpu:F6},{allocated},{GC.CollectionCount(0)},{GC.CollectionCount(1)},{GC.CollectionCount(2)},{playing},{game.ship.chargingShot != null},{TtRender.FirstPerson}");
        if (frame++ % 1440 == 0)
        {
            samples.Flush();
            Console.WriteLine($"BENCH frame={frame} seconds={elapsed:F2} playing={playing} grade={grade} level={level}");
        }
        if (elapsed >= seconds)
        {
            result = playingFrames > frame / 2 ? 0 : 1;
            Console.WriteLine($"BENCH {(result == 0 ? "PASS" : "FAIL")} frames={frame} playing={playingFrames}");
            Quit();
        }
    }
    catch (Exception e) { Console.Error.WriteLine(e); result = 1; Quit(); }
}, Game.OnQuit, []);
return runtime == 0 ? result : runtime;
