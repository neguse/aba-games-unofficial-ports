using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using Aba;
using Aba.Native;
using static Lub;

// Cloud-only integration: the real native API/runtime dispatch with a mock
// Renderer. This does NOT validate rendered pixels, GPU drivers or OpenXR.
static class Tests
{
    static int checks, sessions;
    static IntPtr host, library;
    static string package = "", fixtures = "", dataRoot = "";
    static readonly List<string> completed = new();
    static readonly List<object> gameRuns = new();
    static bool canvasValidated;
    static readonly JsonSerializerOptions json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    static readonly Type runtime = typeof(Lub).Assembly.GetType("LubRuntime", true)!;
    static readonly FieldInfo contextField = runtime.GetField("Ctx", BindingFlags.Static | BindingFlags.NonPublic)!;
    static readonly FieldInfo gameContextField = typeof(GameSession).GetField("context", BindingFlags.Instance | BindingFlags.NonPublic)!;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate IntPtr Create();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate byte Begin(IntPtr ctx, out float dt);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate void ContextCall(IntPtr ctx);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int Count();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int ContextCount(IntPtr ctx);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate ulong DrawCount();
    static Begin begin = null!;
    static ContextCall end = null!, destroy = null!;
    static Count live = null!;
    static ContextCount allocations = null!;
    static DrawCount draws = null!;
    static T Export<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
    static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    static unsafe void SetContext(IntPtr pointer) => contextField.SetValue(null, Pointer.Box(pointer.ToPointer(), contextField.FieldType));
    static void Reset()
    {
        Session.Reset();
        Check(Session.Configuring == null && Session.Quitting == null && Session.HostSending == null && Session.HostPolling == null,
            "Reset retained a game delegate");
        Check(Session.MainTarget == null && Session.MainSize == null && Session.XrActiveOverride == null, "Reset retained a game rendering override");
        Check(live() == 0, $"Reset retained {live()} mock backend resources");
        Check(allocations(host) == 0, $"Reset retained {allocations(host)} native session allocations");
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
        Check(ReferenceEquals(context.LoadFromAssemblyName(typeof(Lub).Assembly.GetName()), typeof(Lub).Assembly),
            session.Info.Id + ": Lub must be shared with the host");
        return context.Assemblies.Single(a => a.GetName().Name == "AbaGame");
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference Probe(string mode)
    {
        Environment.SetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE", mode);
        var messages = new List<(string topic, string value)>();
        var storage = new NativeGameStorage("lifecycle-fixture", dataRoot);
        int configured = 0;
        Session.Configuring = _ => configured++;
        Session.HostSending = (topic, value) => { messages.Add((topic, value)); if (topic == "scores.save") storage.Save("scores", value); };
        Session.HostPolling = () => (null, null);
        var session = new GameSession(new GameInfo("valid", "Test fixture", false), fixtures);
        var reference = session.Unloaded;
        _ = AssemblyOf(session);
        bool initFailed = false, quitFailed = false;
        try
        {
            try { session.Initialize(); }
            catch (InvalidOperationException ex) when (ex.Message == "fixture init failure") { initFailed = true; }
            Check(initFailed == mode.Contains("init"), mode + ": init exception behavior");
            Check(configured == 1, mode + ": Config was not routed to the host");
            Check(messages.Count(x => x == ("probe.init", "1")) == 1, mode + ": static initialization was not fresh");
            if (!initFailed)
            {
                for (int i = 0; i < 3; i++) session.Frame(1f / 60);
                session.Event(new EventData { Kind = EventKind.WindowResize });
                Check(messages.Contains(("probe.frame", "3")), mode + ": frame callback missing");
                Check(messages.Contains(("probe.event", "8")), mode + ": event callback missing");
            }
            try { session.Dispose(); }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("fixture ") && ex.Message.EndsWith(" failure")) { quitFailed = true; }
            Check(quitFailed == (mode.Contains("quit") || mode.Contains("shutdown")), mode + ": teardown exception behavior");
            session.Dispose(); // Teardown is idempotent, even after failure.
            Check(messages.Count(x => x.topic == "probe.quit") == 1, mode + ": Quit did not run exactly once");
            Check(messages.Count(x => x.topic == "probe.shutdown") == 1, mode + ": Shutdown did not run exactly once");
            Check(messages.FindIndex(x => x.topic == "probe.quit") < messages.FindIndex(x => x.topic == "probe.shutdown"), mode + ": wrong teardown order");
            Check(storage.Load("scores") == "fixture-final-save", mode + ": final save did not reach the host before teardown");
        }
        finally
        {
            try { session.Dispose(); }
            finally { Reset(); }
        }
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
        // Preserve that startup behavior and prove real draws after its delay.
        if (info.Id == "masashikun-hi") frameCount = Math.Max(frameCount, 72);
        string previous = Directory.GetCurrentDirectory();
        var session = new GameSession(info, package);
        var reference = session.Unloaded;
        HostBridge? bridge = null;
        bool quitRequested = false;
        int configured = 0;
        var assembly = AssemblyOf(session);
        PropertyInfo? physicsCount = assembly.GetType("McdPhysics")?.GetProperty("LiveHandleCount");
        ulong before = draws();
        var savesBefore = Directory.GetFiles(dataRoot, "*.txt", SearchOption.AllDirectories)
            .Where(p => Path.GetFileName(Path.GetDirectoryName(p)) != info.Id)
            .ToDictionary(p => p, File.ReadAllText);
        try
        {
            Directory.SetCurrentDirectory(session.DirectoryPath);
            Session.Configuring = options => { configured++; Session.MainSize = (options.Width ?? 640, options.Height ?? 480); };
            Session.Quitting = () => quitRequested = true;
            if (!info.Vr) { bridge = new HostBridge(info.Id, session.DirectoryPath, dataRoot); bridge.Install(); }
            session.Initialize();
            Check(configured == 1, info.Id + ": initialization did not call Config exactly once");
            if (bridge != null) Check(bridge.Ready, info.Id + ": no Host ready handshake");
            for (int i = 0; i < frameCount; i++)
            {
                Check(begin(host, out float dt) != 0, info.Id + ": native host refused a frame");
                try
                {
                    bridge?.BeforeFrame(dt);
                    session.Frame(dt);
                    bridge?.AfterFrame(dt);
                }
                finally { end(host); }
                Check(!quitRequested, info.Id + ": game requested quit (often a missing/undecodable asset)");
                Check(bridge?.LastError == null, info.Id + ": " + bridge?.LastError);
            }
            Check(draws() > before, info.Id + ": no draw dispatch reached the mock renderer");
            if (physicsCount != null) Check((int)physicsCount.GetValue(null)! > 0, info.Id + ": native ODE was not exercised");
            session.Dispose();
            if (physicsCount != null) Check((int)physicsCount.GetValue(null)! == 0, info.Id + ": McdPhysics.Shutdown leaked native handles");
            string scores = Path.Combine(dataRoot, info.Id, "scores.txt");
            Check(File.Exists(scores) && new FileInfo(scores).Length > 0, info.Id + ": final OnQuit score save is missing/empty");
            foreach (var saved in savesBefore) Check(File.ReadAllText(saved.Key) == saved.Value, info.Id + ": overwrote another game's save");
            completed.Add(info.Id);
            gameRuns.Add(new { game = info.Id, round, frames = frameCount, drawDispatches = draws() - before });
            sessions++;
            Console.WriteLine($"PASS session {sessions}: {info.Id}, round {round}, {frameCount} frames, OnQuit/save/physics cleanup");
        }
        finally
        {
            try { session.Dispose(); }
            finally
            {
                try { bridge?.Dispose(); }
                finally { Directory.SetCurrentDirectory(previous); Reset(); }
            }
        }
        return reference;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void CanvasSmoke()
    {
        var canvas = new Canvas();
        var messages = new List<(string topic, string value)>();
        var panel = new MasashikunPanel((topic, value) => messages.Add((topic, value)));
        ulong textDraws = 0, panelDraws = 0;
        try
        {
            for (int frame = 0; frame < 3; frame++)
            {
                Check(begin(host, out _) != 0, "Canvas: native frame begin");
                try
                {
                    var target = canvas.Target("test-collection-menu", Canvas.Width, Canvas.Height);
                    var image = canvas.Target("test-collection-game", 640, 480);
                    canvas.Begin(image); canvas.Rect(0, 0, 640, 480, 0x123456ff); canvas.End();
                    canvas.Begin(target);
                    canvas.Rect(22, 20, 916, 64, 0x16496aff);
                    ulong beforeText = draws();
                    canvas.Text("ABA GAMES / 13 games", 38, 26, 38);
                    textDraws += draws() - beforeText;
                    canvas.Image(image, 32, 104, 640, 480);
                    ulong beforePanel = draws();
                    if (frame == 1) panel.OnMessage("ranking", "Overall\n1\n100\tAAA\n90\tBBB\n80\tCCC\n");
                    if (frame == 2) { panel.CloseRanking(); panel.ShowMenu(); }
                    panel.Draw(canvas);
                    panelDraws += draws() - beforePanel;
                    canvas.End();
                    ulong beforePresent = draws();
                    canvas.Present(target, Canvas.Width, Canvas.Height, false);
                    Check(draws() > beforePresent, "Canvas: desktop presentation did not draw");
                }
                finally { end(host); }
            }
            Check(textDraws > 0, "Canvas: production font produced no draw dispatch");
            Check(panelDraws > 0, "Canvas: Masashikun menu/ranking produced no draw dispatch");
            canvasValidated = true;
            Console.WriteLine("PASS production Canvas: shader/font/Text/Rect/Image/desktop Present and Masashikun menu/ranking dispatch (mock renderer, no pixels)");
        }
        finally { Reset(); }
    }
    public static int Main(string[] args)
    {
        string? report = null;
        string previousDirectory = Directory.GetCurrentDirectory();
        string? previousData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
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
        dataRoot = Path.Combine(Path.GetTempPath(), "aba-collection-lifecycle-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataRoot);
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", dataRoot);
        Environment.SetEnvironmentVariable("SDL_AUDIODRIVER", "dummy");
        string? failure = null;
        try
        {
            Check(frames >= 2 && cycles >= 1, "Require at least two frames and one cycle");
            Check(File.Exists(native), "Provide --native path to liblub_session_test_host.so");
            Environment.SetEnvironmentVariable("LUB_NATIVE_LIB", native);
            runtime.GetMethod("EnsureNative", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, null);
            library = NativeLibrary.Load(native);
            begin = Export<Begin>("lub_host_frame_begin"); end = Export<ContextCall>("lub_host_frame_end"); destroy = Export<ContextCall>("lub_host_destroy");
            live = Export<Count>("lub_test_live_resources"); allocations = Export<ContextCount>("lub_test_session_allocations"); draws = Export<DrawCount>("lub_test_draw_calls");
            host = Export<Create>("lub_test_host_create")();
            Check(host != IntPtr.Zero, "SDL dummy test host creation failed");
            SetContext(host);
            Console.WriteLine("CLOUD LIFECYCLE TEST: mock rendering, real native dispatch/IO/audio/physics/Slang metadata; no GPU, pixels, OpenXR, or hardware validation");
            Action afterFrame = () => { };
            Session.AfterFrame = afterFrame;
            Reset();
            Check(Session.AfterFrame == afterFrame, "Reset cleared the host's AfterFrame callback");
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
            Session.ClearHooks(); Session.AfterFrame = null;
            if (host != IntPtr.Zero) { SetContext(IntPtr.Zero); destroy(host); host = IntPtr.Zero; }
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", previousData);
            Environment.SetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE", previousMode);
            Directory.SetCurrentDirectory(previousDirectory);
            if (report != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(report)!);
                File.WriteAllText(report, JsonSerializer.Serialize(new { passed = failure == null, checks, sessions, frames, cycles,
                    games = completed.Distinct().ToArray(), gameSessions = completed, gameRuns, canvasValidated, renderer = "mock-no-pixels", realNativeDispatch = true,
                    openXrValidated = false, hardwareValidated = false, failure }, json));
            }
            Directory.Delete(dataRoot, true);
        }
        return failure == null ? 0 : 1;
    }
}
