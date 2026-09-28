global using Host = FrameHost;

public static class FrameApp
{
    public static void OnInit() { Game.VariableTime = true; Game.OnInit(); }
    public static void OnQuit() => Game.OnQuit();
    public static void OnFrame(float dt) => Draw(dt);
    public static void Draw(float dt)
        => DrawInput(dt, Lub.Xr.GetInput(0), Lub.Xr.GetInput(1), Lub.Xr.Focused());
    public static void DrawInput(float dt, XrInput leftHand, XrInput rightHand, bool focused)
    {
        FrameHost.Begin();
        var left = Lub.Xr.GetView(0, .05f, 100);
        var right = Lub.Xr.GetView(1, .05f, 100);
        if (left == null || right == null) return;
        Pad.Read(leftHand, rightHand, Game.frame.IsInGame && Game.frame.PauseTicks >= 0);
        if (focused) Game.OnFrame(dt);
        else Game.frame.Draw();
        GtgRender.Present(left, right);
        FrameHost.End(focused);
    }
}
