using System.Diagnostics;
using System.Reflection;
using static Lub;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
int seconds = args.Length > 0 ? int.Parse(args[0]) : 60;
float rank = args.Length > 1 ? float.Parse(args[1]) : 8;
static T Field<T>(object value, string name) => (T)value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value);
GameState state = null;
Stage stage = null;
BulletPool bullets = null;
ParticlePool particles = null;
bool wasPlaying = false;
var targetRank = typeof(Stage).GetField("targetRank", BindingFlags.Instance | BindingFlags.NonPublic);
var samples = new Sample[checked((seconds + 30) * 160)];
var left = new XrInput { Active = true };
var right = new XrInput { Active = true };
int frame = 0, playingFrames = 0;
long start = 0;
int result = 1;
int runtime = Lub.Run(() =>
{
    Game.OnInit();
    while (FrameHost.Available())
    {
        Lub.Host.Poll(out string topic, out _);
        if (topic == null) break;
    }
    Game.frame.Seed(1234);
    var actors = Field<ActorPools>(Game.frame, "actors");
    state = Field<GameState>(actors, "gameState");
    stage = Field<Stage>(actors, "stage");
    bullets = Field<BulletPool>(actors, "bullets");
    particles = Field<ParticlePool>(actors, "particles");
}, null, dt =>
{
    try
    {
        if (Xr.View(0, .05f, 100) == null) return;
        if (start == 0) start = Stopwatch.GetTimestamp();
        double elapsed = Stopwatch.GetElapsedTime(start).TotalSeconds;
        bool playing = Game.frame.IsInGame && !state.IsInGameOver;
        if (playing && !wasPlaying && rank > 0)
        {
            Stage.Rank = rank;
            targetRank.SetValue(stage, rank);
        }
        wasPlaying = playing;
        if (playing) playingFrames++;
        float phase = (float)(elapsed % 12);
        left.StickX = MathF.Sin(phase * MathF.PI / 3) * .8f;
        left.StickY = MathF.Cos(phase * MathF.PI / 3) * .8f;
        left.Trigger = phase >= 6 ? .9f : 0;
        right.Trigger = phase < 6 ? .9f : 0;
        right.Primary = frame % 144 < 30;
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        long before = Stopwatch.GetTimestamp();
        FrameApp.DrawInput(dt, left, right, true);
        double cpu = Stopwatch.GetElapsedTime(before).TotalMilliseconds;
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
        samples[frame] = new(elapsed, cpu, allocated, GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2), playing, Stage.Rank, Stage.GameSpeed, bullets.Count, particles.Count);
        if (frame++ % 1440 == 0)
        {
            Console.WriteLine($"BENCH frame={frame} seconds={elapsed:F2} playing={playing} rank={Stage.Rank:F3} bullets={bullets.Count} particles={particles.Count}");
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
using (var output = new StreamWriter("frames.csv"))
{
    output.WriteLine("frame,seconds,cpuMs,allocated,gen0,gen1,gen2,game,rank,speed,bullets,particles");
    for (int i = 0; i < frame; i++)
    {
        var s = samples[i];
        output.WriteLine($"{i},{s.Seconds:F6},{s.Cpu:F6},{s.Allocated},{s.Gen0},{s.Gen1},{s.Gen2},{s.Playing},{s.Rank:F4},{s.Speed:F4},{s.Bullets},{s.Particles}");
    }
}
return runtime == 0 ? result : runtime;

readonly record struct Sample(double Seconds, double Cpu, long Allocated, int Gen0, int Gen1, int Gen2, bool Playing, float Rank, float Speed, int Bullets, int Particles);
