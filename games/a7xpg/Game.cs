using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static A7xGameManager manager;
    public static TextureRef glow, blank;
    static ShaderRef shader;
    static float elapsed;
    static int version, bufferIndex;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        Drawing.BeginFrame();
        Drawing.recordBlend = true;
        Drawing.premultiplyAdditive = true;
        Drawing.glDisable(Drawing.GL_DEPTH_TEST);
        Drawing.glDisable(Drawing.GL_CULL_FACE);
        Drawing.glEnable(Drawing.GL_BLEND);
        manager = new A7xGameManager();
        manager.init();
        manager.start();
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
                manager.prefManager.load(payload);
            if (topic == "seed")
            {
                int seed = GameMath.parseNonnegative(payload);
                if (seed >= 0)
                    Rand.setSeed(seed);
            }

            if (topic == "input")
            {
                int input = GameMath.parseNonnegative(payload);
                if (input < 0 || input > 255)
                    continue;
                manager.input.directions = input & 15;
                manager.input.buttons = input & 48;
                manager.input.pause = (input & 64) != 0;
                manager.input.escape = (input & 128) != 0;
            }
        }

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= 0.016f)
        {
            manager.move();
            elapsed = elapsed - (0.016f);
        }

        shader = Gfx.UseShader("a7xpg", GameShaders.vertex, GameShaders.fragment, 1);
        glow = Gfx.UseTexture("glow", 128, 128, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        blank = Gfx.UseTexture("blank", 1, 1, Gfx.PixelFormat.Rgba8, new List<int> { 0, 0, 0, 0 }, 1);
        if (shader == null || glow == null || blank == null)
            return;
        version++;
        bufferIndex = 0;
        manager.draw();
        render(Gfx.MainTex, blank, true);
    }

    public static void render(TextureRef target, TextureRef texture, bool load)
    {
        Gfx.BeginPass(new PassOpts { Target = target, Load = load ? Gfx.LoadAction.Load : Gfx.LoadAction.Clear, ClearColor = new float[] { 0, 0, 0, 0 } });
        foreach (var batch in Drawing.batches)
            drawVertices(batch.vertices, texture, batch.alphaBlend, batch);
        Gfx.EndPass();
        Drawing.batches.Clear();
        Drawing.glLineWidth(1);
    }

    public static void drawVertices(List<float> vertices, TextureRef texture, bool alpha, DrawBatch batch = null)
    {
        if (vertices.Count == 0)
            return;
        var buffer = Gfx.UseBuffer("geometry" + bufferIndex.ToString(), Gfx.BufferType.Storage, vertices, version);
        var bindings = TextureDrawing.Bindings(buffer, batch, bufferIndex, version);
        bindings["glow"] = texture;
        bufferIndex++;
        if (buffer != null)
            Gfx.Draw(GameMath.integer(vertices.Count / 8), bindings, new DrawOpts { Shader = shader, Cull = Gfx.Cull.None, Depth = false, DepthWrite = false, Blend = alpha ? Gfx.Blend.Alpha : Gfx.Blend.Additive });
    }

    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
