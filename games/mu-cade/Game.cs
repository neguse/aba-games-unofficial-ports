using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static GameManager manager;
    public static float elapsed;
    public static int version;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        Drawing.BeginFrame();
        Drawing.recordBlend = true;
        Drawing.premultiplyAdditive = true;
        Drawing.glDisable(Drawing.GL_DEPTH_TEST);
        Drawing.glDisable(Drawing.GL_CULL_FACE);
        Drawing.glEnable(Drawing.GL_BLEND);
        manager = new GameManager();
        manager.init_0();
        manager.start_0();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("ready", "");
        }
    }

    public static void OnFrame(float dt)
    {
        while (Host.Available())
        {
            Host.Poll(out string topic, out string payload);
            if (topic == null)
                break;
            if (topic == "scores")
            {
                manager.prefManager.loadText(payload);
                if (manager.state == GameManagerGameState.TITLE)
                    manager.titleManager.start_0();
            }

            if (topic == "seed")
            {
                int seed = GameMath.parseNonnegative(payload);
                if (seed >= 0)
                    manager.rand.setSeed(seed);
            }

            if (topic == "input")
            {
                int input = GameMath.parseNonnegative(payload);
                if ((input < 0) || (input > 4095))
                    continue;
                TwinStickPad.input = input;
            }
        }

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= 0.016f)
        {
            manager.move_0();
            elapsed = elapsed - (0.016f);
        }

        manager.screen.clear();
        manager.draw();
        var shader = Gfx.UseShader("mu-cade", GameShaders.vertex, GameShaders.fragment, 1);
        if (shader == null)
            return;
        version++;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = Drawing.clearColor });
        int index = 0;
        foreach (var batch in Drawing.batches)
        {
            if (batch.vertices.Count == 0)
                continue;
            var buffer = Gfx.UseBuffer("geometry" + index.ToString(), Gfx.BufferType.Storage, batch.vertices, version);
            if (buffer != null)
                Gfx.Draw(GameMath.integer(GameMath.integer(GameMath.integer(batch.vertices.Count / 8))), TextureDrawing.Bindings(buffer, batch, index, version), new DrawOpts { Shader = shader, Depth = batch.depth, DepthWrite = batch.depth, Cull = batch.cull ? Gfx.Cull.Front : Gfx.Cull.None, Blend = batch.multiply ? Gfx.Blend.Multiply : batch.blend ? (batch.alphaBlend ? Gfx.Blend.Alpha : Gfx.Blend.Additive) : Gfx.Blend.None });
            index++;
        }

        Gfx.EndPass();
    }

    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
