using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using Aba;
using Lub;

// Integration over the real shared Lub runtime with SDL's offscreen video driver:
// real GPU backend, IO, audio, physics and saves, without a window or display.
// This does NOT compare rendered pixels or validate OpenXR or hardware.
static class Tests
{
    static int checks, sessions;
    static IntPtr host, library;
    static string package = "", fixtures = "", dataRoot = "";
    static readonly List<string> completed = new();
    static readonly List<object> gameRuns = new();
    static bool canvasValidated;
    static readonly JsonSerializerOptions json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    static readonly Type runtime = typeof(Lub.App).Assembly.GetType("Lub.LubRuntime", true)!;
    static readonly FieldInfo contextField = runtime.GetField("Ctx", BindingFlags.Static | BindingFlags.NonPublic)!;
    static readonly FieldInfo gameContextField = typeof(GameSession).GetField("context", BindingFlags.Instance | BindingFlags.NonPublic)!;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate IntPtr Create(IntPtr opts);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int Start(IntPtr ctx);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate byte Begin(IntPtr ctx, out float dt);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void ContextCall(IntPtr ctx);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void FrameEnd(IntPtr ctx, byte frameOk);
    static Begin begin = null!;
    static FrameEnd end = null!;
    static ContextCall destroy = null!;
    static T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
    static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    static unsafe void SetContext(IntPtr pointer) => contextField.SetValue(null, Pointer.Box(pointer.ToPointer(), contextField.FieldType));
    // Runs one native frame; a frame the host refuses (no drawable yet) is retried.
    static float Frame(Action<float> body)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (begin(host, out float dt) == 0) { Thread.Sleep(5); continue; }
            try { body(dt); }
            finally { end(host, 1); }
            return dt;
        }
        throw new InvalidOperationException("native host refused 100 frames in a row");
    }
    static void Collected(WeakReference reference, string label)
    {
        for (int attempt = 0; attempt < 20 && reference.IsAlive; attempt++)
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Thread.Sleep(10);
        }
        Check(!reference.IsAlive, label + ": collectible game assembly is still alive after unload");
    }
    static Assembly AssemblyOf(GameSession session)
    {
        var context = (AssemblyLoadContext)gameContextField.GetValue(session)!;
        Check(context.IsCollectible, session.Info.Id + ": context is not collectible");
        Check(ReferenceEquals(context.LoadFromAssemblyName(typeof(Lub.App).Assembly.GetName()), typeof(Lub.App).Assembly),
            session.Info.Id + ": Lub must be shared with the host");
        return context.Assemblies.Single(a => a.GetName().Name == "AbaGame");
    }
    static string SavePath(string id) => Path.Combine(dataRoot, id, "scores.txt");
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference Probe(string mode)
    {
        Environment.SetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE", mode);
        if (File.Exists(SavePath("lifecycle-fixture"))) File.Delete(SavePath("lifecycle-fixture"));
        var session = new GameSession(new GameInfo("valid", "Test fixture", false), fixtures);
        var reference = session.Unloaded;
        var probes = (List<string>)AssemblyOf(session).GetType("Game", true)!.GetField("Probes")!.GetValue(null)!;
        bool initFailed = false, quitFailed = false;
        try
        {
            try { session.Initialize(); }
            catch (InvalidOperationException ex) when (ex.Message == "fixture init failure") { initFailed = true; }
            Check(initFailed == mode.Contains("init"), mode + ": init exception behavior");
            Check(probes.Count(x => x == "init 1") == 1, mode + ": static initialization was not fresh");
            if (!initFailed)
            {
                for (int i = 0; i < 3; i++)
                {
                    Check(!session.Ended, mode + ": Scene.End was seen before the game called it");
                    Frame(dt => session.Frame(dt, Gfx.MainTex!));
                }
                session.Event(new EventData { Kind = EventKind.WindowResize });
                Check(probes.Contains("frame 3"), mode + ": frame callback missing");
                Check(session.Ended, mode + ": Scene.End did not reach the host");
                Check(probes.Contains("event 8"), mode + ": event callback missing");
            }
            try { session.Dispose(); }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("fixture ") && ex.Message.EndsWith(" failure")) { quitFailed = true; }
            Check(quitFailed == (mode.Contains("quit") || mode.Contains("shutdown")), mode + ": teardown exception behavior");
            session.Dispose(); // Teardown is idempotent, even after failure.
            Check(probes.Count(x => x.StartsWith("quit ")) == 1, mode + ": Quit did not run exactly once");
            Check(probes.Count(x => x == "shutdown") == 1, mode + ": Shutdown did not run exactly once");
            Check(probes.FindIndex(x => x.StartsWith("quit ")) < probes.IndexOf("shutdown"), mode + ": wrong teardown order");
            Check(File.Exists(SavePath("lifecycle-fixture")) && File.ReadAllText(SavePath("lifecycle-fixture")) == "fixture-final-save",
                mode + ": the final OnQuit save did not reach the save directory");
        }
        finally { session.Dispose(); }
        sessions++;
        return reference;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference[] Invalid(string id)
    {
        var seen = new List<WeakReference>();
        AssemblyLoadEventHandler handler = (_, e) =>
        {
            if (e.LoadedAssembly.GetName().Name == "AbaGame")
                seen.Add(new WeakReference(AssemblyLoadContext.GetLoadContext(e.LoadedAssembly)));
        };
        AppDomain.CurrentDomain.AssemblyLoad += handler;
        try
        {
            bool failed = false;
            try { using var session = new GameSession(new GameInfo(id, id, false), fixtures); }
            catch (Exception ex) when (ex is TypeLoadException or InvalidDataException or FileNotFoundException or InvalidOperationException) { failed = true; }
            Check(failed, id + ": invalid plugin unexpectedly loaded");
        }
        finally { AppDomain.CurrentDomain.AssemblyLoad -= handler; }
        Check(seen.Count == 1, id + ": invalid fixture was not loaded into an observable context");
        return seen.ToArray();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference Play(GameInfo info, int frameCount, int round)
    {
        // Masashikun's original title starts drawing after 32 ticks at 30 Hz.
        if (info.Id == "masashikun-hi") frameCount = Math.Max(frameCount, 72);
        string previous = Directory.GetCurrentDirectory();
        var session = new GameSession(info, package);
        var reference = session.Unloaded;
        var assembly = AssemblyOf(session);
        PropertyInfo? physicsCount = assembly.GetType("McdPhysics")?.GetProperty("LiveHandleCount");
        var savesBefore = Directory.GetFiles(dataRoot, "*.txt", SearchOption.AllDirectories)
            .Where(p => Path.GetFileName(Path.GetDirectoryName(p)) != info.Id)
            .ToDictionary(p => p, File.ReadAllText);
        try
        {
            Directory.SetCurrentDirectory(session.DirectoryPath);
            session.Initialize();
            for (int i = 0; i < frameCount; i++)
            {
                Frame(dt => session.Frame(dt, Gfx.MainTex!));
                Check(!session.Ended, info.Id + ": game called Scene.End (often a missing/undecodable asset)");
            }
            if (physicsCount != null) Check((int)physicsCount.GetValue(null)! > 0, info.Id + ": native ODE was not exercised");
            session.Dispose();
            if (physicsCount != null) Check((int)physicsCount.GetValue(null)! == 0, info.Id + ": McdPhysics.Shutdown leaked native handles");
            Check(File.Exists(SavePath(info.Id)) && new FileInfo(SavePath(info.Id)).Length > 0, info.Id + ": final OnQuit score save is missing/empty");
            foreach (var saved in savesBefore) Check(File.ReadAllText(saved.Key) == saved.Value, info.Id + ": overwrote another game's save");
            completed.Add(info.Id);
            gameRuns.Add(new { game = info.Id, round, frames = frameCount });
            sessions++;
            Console.WriteLine($"PASS session {sessions}: {info.Id}, round {round}, {frameCount} frames, OnQuit/save/physics cleanup");
        }
        finally
        {
            try { session.Dispose(); }
            finally { Directory.SetCurrentDirectory(previous); }
        }
        return reference;
    }
    static void CanvasSmoke()
    {
        var canvas = new Canvas();
        for (int frame = 0; frame < 3; frame++)
            Frame(_ =>
            {
                var target = canvas.Target("test-collection-menu", Canvas.Width, Canvas.Height);
                var image = canvas.Target("test-collection-game", 640, 480);
                canvas.Begin(image); canvas.Rect(0, 0, 640, 480, 0x123456ff); canvas.End();
                canvas.Begin(target);
                canvas.Rect(22, 20, 916, 64, 0x16496aff);
                canvas.Text("ABA GAMES / 13 games", 38, 26, 38);
                canvas.Image(image, 32, 104, 640, 480);
                canvas.End();
                canvas.Present(target, Canvas.Width, Canvas.Height, false);
            });
        canvasValidated = true;
        Console.WriteLine("PASS production Canvas: shader/font/Text/Rect/Image/desktop Present");
    }
    public static int Main(string[] args)
    {
        string? report = null;
        string previousDirectory = Directory.GetCurrentDirectory();
        string? previousMode = Environment.GetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE");
        int frames = 12, cycles = 2;
        string native = Environment.GetEnvironmentVariable("LUB_NATIVE_LIB") ?? "";
        string? only = null;
        bool fixturesOnly = false;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--package": package = Path.GetFullPath(args[++i]); break;
                case "--fixtures": fixtures = Path.GetFullPath(args[++i]); break;
                case "--native": native = Path.GetFullPath(args[++i]); break;
                case "--frames": frames = int.Parse(args[++i]); break;
                case "--cycles": cycles = int.Parse(args[++i]); break;
                case "--report": report = Path.GetFullPath(args[++i]); break;
                case "--game": only = args[++i]; break;
                case "--fixtures-only": fixturesOnly = true; break;
                default: throw new ArgumentException("Unknown argument: " + args[i]);
            }
        }
        // .NET does not pass its environment changes to native code, so SDL's drivers and save directory come from the caller.
        dataRoot = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? "";
        string? failure = null;
        try
        {
            Check(frames >= 2 && cycles >= 1, "Require at least two frames and one cycle");
            Check(Environment.GetEnvironmentVariable("SDL_VIDEO_DRIVER") == "offscreen" && Environment.GetEnvironmentVariable("SDL_AUDIODRIVER") == "dummy",
                "Run with SDL_VIDEO_DRIVER=offscreen and SDL_AUDIODRIVER=dummy");
            Check(Directory.Exists(dataRoot) && !Directory.EnumerateFileSystemEntries(dataRoot).Any(), "Run with XDG_DATA_HOME set to an empty temporary directory");
            Check(File.Exists(native), "Provide --native path to liblub.so");
            Environment.SetEnvironmentVariable("LUB_NATIVE_LIB", native);
            runtime.GetMethod("EnsureNative", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null);
            library = NativeLibrary.Load(native);
            begin = Export<Begin>("lub_host_frame_begin"); end = Export<FrameEnd>("lub_host_frame_end"); destroy = Export<ContextCall>("lub_host_destroy");
            host = Export<Create>("lub_host_create")(IntPtr.Zero);
            Check(host != IntPtr.Zero, "offscreen host creation failed");
            SetContext(host);
            Check(Export<Start>("lub_host_start")(host) == 0, "offscreen host backend did not start");
            Console.WriteLine("LIFECYCLE TEST: real shared Lub runtime on SDL offscreen; no pixel comparison, OpenXR or hardware validation");
            foreach (string mode in new[] { "normal", "normal", "init", "quit", "init-quit", "shutdown" })
                Collected(Probe(mode), "fixture " + mode);
            foreach (string id in new[] { "missing-game", "missing-frame" })
                foreach (var reference in Invalid(id)) Collected(reference, id);
            Environment.SetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE", null);
            CanvasSmoke();
            if (!fixturesOnly)
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aba.Catalog.json")!;
                var catalog = JsonSerializer.Deserialize<GameInfo[]>(stream, json)!;
                Check(catalog.Length == 13, "Production catalog must contain all 13 games");
                if (only != null) { catalog = catalog.Where(g => g.Id == only).ToArray(); Check(catalog.Length == 1, "Unknown game: " + only); }
                for (int round = 1; round <= cycles; round++)
                {
                    // Reverse on alternate rounds to switch from each game to
                    // a different neighbor; the boundary also reopens one game.
                    foreach (var info in round % 2 == 1 ? catalog : catalog.Reverse())
                        Collected(Play(info, frames, round), info.Id + " round " + round);
                }
            }
            Console.WriteLine($"PASS {checks} checks, {sessions} sessions, {completed.Distinct().Count()} actual games, all collectible contexts unloaded");
        }
        catch (Exception ex) { failure = ex.ToString(); Console.Error.WriteLine(ex); }
        finally
        {
            if (host != IntPtr.Zero) { SetContext(IntPtr.Zero); destroy(host); host = IntPtr.Zero; }
            Environment.SetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE", previousMode);
            Directory.SetCurrentDirectory(previousDirectory);
            if (report != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(report)!);
                File.WriteAllText(report, JsonSerializer.Serialize(new { passed = failure == null, checks, sessions, frames, cycles,
                    games = completed.Distinct().ToArray(), gameSessions = completed, gameRuns, canvasValidated, renderer = "offscreen", realNativeDispatch = true,
                    openXrValidated = false, hardwareValidated = false, failure }, json));
            }
        }
        return failure == null ? 0 : 1;
    }
}
