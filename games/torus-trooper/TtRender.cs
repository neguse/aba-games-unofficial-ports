using System;
using System.Collections.Generic;
using static Lub;

public static class TtRender
{
    public static bool Active, Focused;
    static bool hud, firstPerson;
    static int eye, width = 640, height = 480, input;
    static readonly float[][] projection = new float[][] { new float[16], new float[16] };
    static float[] world;
    static readonly float[] hudMatrix = new float[] { 1.2f,0,0,0, 0,.9f,0,0, 0,0,0,0, 0,0,-2,1 };
    static Mesh quad;

    public static void Receive(string topic, string payload)
    {
        if (topic == "xr.end")
        {
            Active = false; Focused = false;
            Game.manager.pad.directions = 0; Game.manager.pad.buttons = 0;
            Game.manager.pad.pause = false; Game.manager.pad.escape = false;
        }
        if (topic == "xr.wait") { Active = true; Focused = false; }
        if (topic != "xr.frame") return;
        string[] values = payload.Split(",");
        if (values.Length != 36) return;
        width = GameMath.parseNonnegative(values[0]); height = GameMath.parseNonnegative(values[1]);
        input = GameMath.parseNonnegative(values[2]); firstPerson = values[3] == "1";
        for (int e = 0; e < 2; e++)
            for (int i = 0; i < 16; i++) projection[e][i] = float.Parse(values[4 + e * 16 + i]);
        Active = true; Focused = true;
    }

    public static void ApplyInput(GameManager manager)
    {
        bool title = manager.state == manager.titleState;
        manager.pad.directions = input & 15;
        manager.pad.buttons = title || manager.ship.isGameOver
            ? ((input & 256) != 0 ? PadButton.A : 0) | (title && (input & 128) != 0 ? PadButton.B : 0)
            : input & 48;
        manager.pad.pause = (input & 64) != 0;
        manager.pad.escape = !title && manager.inGameState.pauseCnt > 0 && (input & 128) != 0;
    }

    static Vector3 TrackPosition(Ship ship, float angle, float y, float height)
    {
        Vector index = ship.tunnel.calcIndex(y);
        Vector3 position = ship.tunnel.getPos_4_Single_Single_Int32_Single(angle, index.y, GameMath.integer(index.x),
            1 - height / ship.tunnel.getRadius(y));
        return new Vector3(position.x, position.y, position.z);
    }

    static float[] WorldView(Ship ship)
    {
        float angle = firstPerson ? ship._relPos.x : ship._eyePos.x;
        float y = firstPerson ? ship._relPos.y : ship._relPos.y * .3f - 3;
        float elevation = firstPerson ? 1.5f : 8;
        Vector3 from = TrackPosition(ship, angle, y, elevation);
        Vector3 to = TrackPosition(ship, angle, y + 6 + (firstPerson ? 0 : ship._relPos.y * .3f), firstPerson ? elevation : 5);
        Vector3 surface = TrackPosition(ship, angle, y, 0);
        return Transform.LookAt(Transform.Scale(Transform.Identity(), .05f, .05f, .05f),
            from.x, from.y, from.z, to.x, to.y, to.z, from.x - surface.x, from.y - surface.y, from.z - surface.z);
    }

    public static void Present(GameManager manager)
    {
        world = WorldView(manager.ship);
        var scenes = new TextureRef[2];
        for (int e = 0; e < 2; e++)
        {
            eye = e;
            scenes[eye] = Gfx.UseTexture(eye == 0 ? "xr-left" : "xr-right", width, height, Gfx.PixelFormat.Rgba8,
                null, 1, new TextureOpts { Target = true });
            Gfx.BeginPass(new PassOpts { Target = scenes[eye], ClearColor = new float[] { 0, 0, 0, 1 } });
            hud = false;
            manager.state.draw(Transform.Identity(), null, Gfx.Blend.Additive, Gfx.Cull.None, 1);
            hud = true;
            manager.state.drawFront(Transform.Ortho(), null, Gfx.Blend.Additive, Gfx.Cull.None, 1);
            Gfx.EndPass();
        }
        if (quad == null)
        {
            quad = new Mesh("xr-output");
            quad.Vertex(-1, -1, 0, new float[] { 0, 1, 0, 1 });
            quad.Vertex(1, -1, 0, new float[] { 1, 1, 0, 1 });
            quad.Vertex(1, 1, 0, new float[] { 1, 0, 0, 1 });
            quad.Vertex(-1, 1, 0, new float[] { 0, 0, 0, 1 });
            quad.Quads(0, 4);
        }
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 0, 1 } });
        for (int e = 0; e < 2; e++)
        {
            var model = Transform.Scale(Transform.Translate(Transform.Identity(), e == 0 ? -.5f : .5f, 0, 0), .5f, 1, 1);
            var bindings = quad.Bindings(model, null, 1, false);
            bindings["image"] = scenes[e];
            var uniforms = (Dictionary<string, object>)bindings["uniforms"];
            uniforms["options"] = new float[] { 1, 0, 0, 2 };
            Gfx.Draw(quad.count, bindings, new DrawOpts { Shader = Game.shader, Depth = false, Blend = Gfx.Blend.None, Cull = Gfx.Cull.None });
        }
        Gfx.EndPass();
        Host.Send("xr.present", "");
    }

    public static void Draw(Mesh mesh, float[] model, float[] tint, float width, Lub.Gfx.Blend blend, Lub.Gfx.Cull cull, DrawImage image = null)
    {
        if (Active) model = Transform.Multiply(projection[eye], Transform.Multiply(hud ? hudMatrix : world, model));
        Lub.Gfx.Draw(mesh.count, mesh.Bindings(model, tint, Active ? Math.Max(width, TtRender.width / 640f) : width,
            blend == Lub.Gfx.Blend.Additive, 0, image, Active ? TtRender.width : 640, Active ? height : 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = cull, Blend = blend });
    }
}
