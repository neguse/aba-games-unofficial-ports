using Lub;

#if MISSING_GAME
public static class NotGame
#else
public static class Game
#endif
{
    // The harness reads what happened through reflection.
    public static readonly List<string> Probes = new();
    static int starts, quits;
#if !MISSING_FRAME
    static int frames;
#endif
    static string Mode => Environment.GetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE") ?? "normal";
    public static void OnInit()
    {
        Probes.Add("init " + ++starts);
        if (Mode.Contains("init")) throw new InvalidOperationException("fixture init failure");
    }
#if !MISSING_FRAME
    public static void OnFrame(float dt)
    {
        if (dt <= 0) throw new InvalidOperationException("positive frame delta required");
        if (Scene.Target == null) throw new InvalidOperationException("the host must set Scene.Target");
        Probes.Add("frame " + ++frames);
        if (frames == 3) Scene.End();
    }
#endif
    public static void OnEvent(EventData e) => Probes.Add("event " + (int)e.Kind);
    public static void OnQuit()
    {
        Probes.Add("quit " + ++quits);
        Io.SaveText("save/lifecycle-fixture/scores.txt", "fixture-final-save");
        if (Mode.Contains("quit")) throw new InvalidOperationException("fixture quit failure");
    }
}

public static class McdPhysics
{
    public static void Shutdown()
    {
#if !MISSING_GAME
        Game.Probes.Add("shutdown");
#endif
        if ((Environment.GetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE") ?? "").Contains("shutdown"))
            throw new InvalidOperationException("fixture shutdown failure");
    }
}
