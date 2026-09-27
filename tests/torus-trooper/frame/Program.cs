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
                    var mesh = new Mesh("frame-binding-test");
                    mesh.Vertex(0, 0, 0, null); mesh.Vertex(1, 0, 0, null); mesh.Line(0, 1);
                    TtRender.Begin();
                    TtRender.Draw(mesh, Transform.Translate(Transform.Identity(), 1, 0, 0), new float[] { 1, 0, 0, 1 }, 1, Lub.Gfx.Blend.Additive, Lub.Gfx.Cull.None);
                    TtRender.Draw(mesh, Transform.Translate(Transform.Identity(), 2, 0, 0), new float[] { 0, 1, 0, 1 }, 1, Lub.Gfx.Blend.Additive, Lub.Gfx.Cull.None);
                    var commands = (System.Collections.IList)typeof(TtRender).GetField("commands", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    var commandType = commands[0].GetType();
                    var firstTint = (System.Numerics.Vector4)commandType.GetField("Tint").GetValue(commands[0]);
                    var secondTint = (System.Numerics.Vector4)commandType.GetField("Tint").GetValue(commands[1]);
                    Check(firstTint.X == 1 && firstTint.Y == 0 && secondTint.X == 0 && secondTint.Y == 1, "Batched draws must retain independent colors");
                    Check(((System.Numerics.Matrix4x4)commandType.GetField("Model").GetValue(commands[0])).M41 == 1, "Deferred model must survive a later binding call");
                    mesh.Clear();
                    Check(mesh.vertexCount == 0 && mesh.count == 0, "Dynamic mesh reuse clears geometry");
                    Check((int)typeof(TtRender).GetField("batchCount", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) == 1, "Compatible adjacent draws share one batch");
                    Console.WriteLine("PASS reusable mesh and batched draw isolation");
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
                    var updateAnchor = typeof(TtRender).GetMethod("UpdateAnchor", BindingFlags.NonPublic | BindingFlags.Static);
                    var anchorField = typeof(TtRender).GetField("anchor", BindingFlags.NonPublic | BindingFlags.Static);
                    var rearPose = new XrView { Position = [1, 2, 3], Orientation = [0, 1, 0, 0] };
                    var frontPose = new XrView { Position = [1, 2, 3], Orientation = [0, 0, 0, 1] };
                    TtRender.Recenter();
                    updateAnchor.Invoke(null, [rearPose, rearPose, false]);
                    updateAnchor.Invoke(null, [frontPose, frontPose, true]);
                    var anchor = (System.Numerics.Matrix4x4)anchorField.GetValue(null);
                    Check(System.Numerics.Vector3.TransformNormal(-System.Numerics.Vector3.UnitZ, anchor).Z < -.99f, "An unfocused startup pose must not lock the game behind the player");
                    updateAnchor.Invoke(null, [rearPose, rearPose, true]);
                    Check(anchor.Equals((System.Numerics.Matrix4x4)anchorField.GetValue(null)), "Head turns must retain the established game direction");
                    TtRender.Recenter();
                    updateAnchor.Invoke(null, [rearPose, rearPose, true]);
                    anchor = (System.Numerics.Matrix4x4)anchorField.GetValue(null);
                    Check(System.Numerics.Vector3.TransformNormal(-System.Numerics.Vector3.UnitZ, anchor).Z > .99f, "Game start recenter adopts the player's current heading");
                    Check(anchor.Translation == new System.Numerics.Vector3(1, 2, 3), "Recenter adopts the current head position");
                    Console.WriteLine("PASS focused startup and game-start recenter");
                    Check(!TtRender.FirstPerson, "Third person is the default");
                    FrameControls.Read(game, null, new XrInput { Active = true, StickClick = true });
                    Check(TtRender.FirstPerson && game.pad.buttons == 0 && !game.pad.pause && !game.pad.escape, "Right stick click changes only the view");
                    FrameControls.Read(game, null, new XrInput { Active = true, StickClick = true });
                    Check(TtRender.FirstPerson, "Holding the stick button must not repeat");
                    FrameControls.Read(game, new XrInput { Active = true, StickClick = true }, null);
                    Check(TtRender.FirstPerson, "Left stick click must not change the view");
                    FrameControls.Read(game, null, new XrInput { Active = true, StickClick = true });
                    Check(!TtRender.FirstPerson, "Second press returns to third person");
                    for (int mode = 0; mode < 2; mode++)
                    {
                        TtRender.FirstPerson = mode == 1;
                        for (int step = 0; step < 8; step++)
                        {
                            float angle = step * MathF.PI / 4;
                            game.ship._eyePos.x = game.ship._relPos.x = angle;
                            world = (System.Numerics.Matrix4x4)typeof(TtRender).GetMethod("WorldView", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [game.ship]);
                            var center = TrackPoint(game, angle, game.ship._relPos.y + 3);
                            var right = TrackPoint(game, angle - .01f, game.ship._relPos.y + 3);
                            var projected = System.Numerics.Vector3.Transform(center, world);
                            var projectedRight = System.Numerics.Vector3.Transform(right, world);
                            Check(projected.Z < 0 && projectedRight.X / -projectedRight.Z > projected.X / -projected.Z, "Right must remain screen-right around the entire tunnel in both views");
                            System.Numerics.Matrix4x4.Invert(world, out inverse);
                            var camera = System.Numerics.Vector3.Transform(System.Numerics.Vector3.Zero, inverse);
                            var at = System.Numerics.Vector3.Transform(TrackPoint(game, angle, game.ship._relPos.y), world);
                            if (mode == 0)
                            {
                                Check(at.Z < 0 && at.Y < 0 && MathF.Abs(at.Y / at.Z) < .7f, "Third-person ship stays visible below center");
                                Check(System.Numerics.Vector3.Distance(camera, TrackPoint(game, angle, game.ship._relPos.y)) > 10, "Chase camera remains behind the ship");
                            }
                            else Check(System.Numerics.Vector3.Distance(camera, TrackPoint(game, angle, game.ship._relPos.y)) < 3, "First person stays near the ship");
                        }
                    }
                    TtRender.FirstPerson = false;
                    game.ship._eyePos.x = game.ship._relPos.x = 0;
                    Console.WriteLine("PASS camera switching and full-circle steering visibility");
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
    static System.Numerics.Vector3 TrackPoint(GameManager game, float angle, float y)
    {
        var p = game.tunnel.getPos_1_Vector(new Vector(angle, y));
        return new System.Numerics.Vector3(p.x, p.y, p.z);
    }
}
