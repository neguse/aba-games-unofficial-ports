using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static GameManager manager;
    public static string savedReplay = "";
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
        manager.start();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("replay.load", "");
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
                manager.prefManager.load(payload);
                if (manager.state == manager.titleState)
                    manager.titleManager.start();
            }

            if (topic == "seed")
            {
                int seed = GameMath.parseNonnegative(payload);
                if (seed >= 0)
                    manager.rand.setSeed(seed);
            }

            if ((topic == "replay") && (payload.Length > 0))
            {
                var replay = new ReplayData();
                if (replay.decode(payload))
                {
                    savedReplay = payload;
                    manager.inGameState._replayData = replay;
                    manager.startTitle();
                }
            }

            if (topic == "input")
            {
                int input = GameMath.parseNonnegative(payload);
                if ((input < 0) || (input > 255))
                    continue;
                manager.pad.directions = input & 15;
                manager.pad.buttons = input & 48;
                manager.pad.pause = (input & 64) != 0;
                manager.pad.escape = (input & 128) != 0;
            }
        }

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= 0.016f)
        {
            manager.move();
            elapsed = elapsed - (0.016f);
        }

        manager.draw();
        var shader = Gfx.UseShader("torus-trooper", GameShaders.vertex, GameShaders.fragment, 1);
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
                Gfx.Draw(GameMath.integer(GameMath.integer(batch.vertices.Count / 8)), TextureDrawing.Bindings(buffer, batch, index, version), new DrawOpts { Shader = shader, Depth = batch.depth, DepthWrite = batch.depth, Cull = batch.cull ? Gfx.Cull.Front : Gfx.Cull.None, Blend = batch.multiply ? Gfx.Blend.Multiply : batch.blend ? (batch.alphaBlend ? Gfx.Blend.Alpha : Gfx.Blend.Additive) : Gfx.Blend.None });
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
