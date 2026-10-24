using Lub;

// ゲームを 1 本だけ載せるアプリ (web と Frame)。
// window は 640x480 で、ゲームは window に描く。
// ゲームが Scene.End を呼んだら、page があれば一覧に戻るよう頼み (quit)、無ければ終わる。
public static class Standalone
{
    static bool ending;

    public static void OnInit()
    {
        App.Config(new ConfigOpts { Width = 640, Height = 480 });
        Game.OnInit();
    }

    public static void OnFrame(float dt)
    {
        Scene.Target = Gfx.MainTex;
        Game.OnFrame(dt);
        EndIfRequested();
    }

    public static void EndIfRequested()
    {
        if (!Scene.Ended || ending)
            return;
        ending = true;
        if (Lub.Host.Available())
            Lub.Host.Send("quit", "");
        else
            App.Quit();
    }

    public static void OnQuit()
    {
        Game.OnQuit();
    }
}
