using System.Reflection;

public static class FrameTests
{
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static int Main()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        string saves = Path.Combine(Path.GetTempPath(), "tt-test-" + Guid.NewGuid());
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", saves);
        int result = 1;
        try
        {
            int runtime = Lub.Run(null, null, _ =>
            {
                try
                {
                    TtRender.Begin();
                    TtVerification.Main();
                    result = (int)typeof(TtVerification).GetField("failures", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    var game = new GameManager(); game.init_0(); game.start();
                    FrameControls.Read(game, new XrInput { Active = true, Trigger = 1 }, new XrInput { Active = true, Trigger = 1 });
                    Check(game.pad.buttons == 0, "Triggers must not start a game or replay");
                    FrameControls.Read(game, null, new XrInput { Active = true, Primary = true });
                    Check(game.pad.buttons == PadButton.A, "A starts game");
                    game.startInGame();
                    var world = (System.Numerics.Matrix4x4)typeof(TtRender).GetMethod("WorldView", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [game.ship]);
                    var point = System.Numerics.Vector3.Transform(System.Numerics.Vector3.Zero, world);
                    Check(float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z), "Camera coordinates must survive the tunnel's shared return vector");
                    Check(System.Numerics.Matrix4x4.Invert(world, out var inverse), "World camera must be invertible");
                    Console.WriteLine("PASS finite world camera transform");
                    FrameControls.Read(game, new XrInput { Active = true, StickX = -.5f, StickY = .5f, Trigger = .8f }, new XrInput { Active = true, Trigger = .8f, Secondary = true });
                    Check(game.pad.directions == (PadDir.LEFT | PadDir.UP), "Steering and acceleration");
                    Check(game.pad.buttons == PadButton.ANY && !game.pad.escape, "Charge and fire with no accidental exit");
                    FrameControls.Read(game, new XrInput { Active = true, Trigger = .3f }, new XrInput { Active = true, Trigger = .3f });
                    Check(game.pad.buttons == PadButton.ANY, "Trigger hysteresis");
                    FrameControls.Read(game, new XrInput { Active = true, Trigger = .1f }, new XrInput { Active = true, Trigger = .8f });
                    Check(game.pad.buttons == PadButton.A, "Release charge while retaining normal fire");
                    game.inGameState.pauseCnt = 1;
                    FrameControls.Read(game, null, new XrInput { Active = true, Secondary = true });
                    Check(game.pad.escape, "B leaves paused game");
                    Console.WriteLine("PASS Frame trigger semantics and menu mapping");
                }
                catch (Exception e) { Console.Error.WriteLine(e); result = 1; }
                finally { Lub.Quit(); }
            }, null, ["--backend", "vulkan"]);
            return runtime == 0 ? result : runtime;
        }
        finally { if (Directory.Exists(saves)) Directory.Delete(saves, true); }
    }
}
