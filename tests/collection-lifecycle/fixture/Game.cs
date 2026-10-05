using static Lub;

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
        Config(new ConfigOpts { Width = 640, Height = 480 });
        Host.Send("probe.init", (++starts).ToString());
        // Deliberately leave a delegate from this collectible assembly in Lub.
        // Reset must clear it before the assembly can actually be collected.
        Session.Quitting = OnRequestedQuit;
        if (Mode.Contains("init")) throw new InvalidOperationException("fixture init failure");
    }
    static void OnRequestedQuit() => Host.Send("probe.requested-quit", "");
#if !MISSING_FRAME
    public static void OnFrame(float dt)
    {
        if (dt <= 0) throw new InvalidOperationException("positive frame delta required");
        Host.Send("probe.frame", (++frames).ToString());
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
