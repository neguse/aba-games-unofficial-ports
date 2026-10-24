using Lub;

#if MISSING_GAME
public static class NotGame
#else
public static class Game
#endif
{
    static int starts, quits;
#if !MISSING_FRAME
    static int frames;
#endif
    static string Mode => Environment.GetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE") ?? "normal";
    public static void OnInit()
    {
        Host.Send("probe.init", (++starts).ToString());
        if (Mode.Contains("init")) throw new InvalidOperationException("fixture init failure");
    }
#if !MISSING_FRAME
    public static void OnFrame(float dt)
    {
        if (dt <= 0) throw new InvalidOperationException("positive frame delta required");
        if (Scene.Target == null) throw new InvalidOperationException("the host must set Scene.Target");
        Host.Send("probe.frame", (++frames).ToString());
        if (frames == 3) Scene.End();
    }
#endif
    public static void OnEvent(EventData e) => Host.Send("probe.event", ((int)e.Kind).ToString());
    public static void OnQuit()
    {
        Host.Send("probe.quit", (++quits).ToString());
        Host.Send("scores.save", "fixture-final-save");
        if (Mode.Contains("quit")) throw new InvalidOperationException("fixture quit failure");
    }
}

public static class McdPhysics
{
    public static void Shutdown()
    {
        Lub.Host.Send("probe.shutdown", "");
        if ((Environment.GetEnvironmentVariable("ABA_LIFECYCLE_FIXTURE") ?? "").Contains("shutdown"))
            throw new InvalidOperationException("fixture shutdown failure");
    }
}
