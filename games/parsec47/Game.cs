using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    static P47GameManager manager;
    static float elapsed;
    static int version;

    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        Drawing.BeginFrame(); Drawing.recordBlend = true;
        Drawing.premultiplyAdditive = true;
        Drawing.glDisable(Drawing.GL_DEPTH_TEST); Drawing.glDisable(Drawing.GL_CULL_FACE); Drawing.glEnable(Drawing.GL_BLEND);
        manager = new P47GameManager();
        manager.init(); manager.start();
        if (Host.Available()) Host.Send("ready", "");
    }
    public static void OnFrame(float dt)
    {
        while (Host.Available())
        {
            Host.Poll(out string topic, out string payload);
            if (topic == null) break;
            if (topic == "scores")
            {
                manager.prefManager.load(payload);
                if (manager.state == P47GameManager.TITLE_STATE)
                {
                    manager.difficulty = manager.prefManager.selectedDifficulty;
                    manager.parsecSlot = manager.prefManager.selectedParsecSlot;
                    manager.mode = manager.prefManager.selectedMode;
                    manager.startTitle();
                }
            }
            if (topic == "input")
            {
                int input = GameMath.parseNonnegative(payload);
                if (input < 0 || input > 255) continue;
                manager.pad.directions = input & 15;
                manager.pad.buttons = input & 48;
                manager.pad.pause = (input & 64) != 0;
                manager.pad.escape = (input & 128) != 0;
            }
        }
        elapsed += Math.Min(dt, 0.1f);
        while (elapsed >= manager.interval / 1000) { manager.move(); elapsed -= manager.interval / 1000; }
        manager.draw();
        var shader = Gfx.UseShader("parsec47", GameShaders.vertex, GameShaders.fragment, 1);
        if (shader == null) return;
        version++;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = Drawing.clearColor });
        int index = 0;
        foreach (var batch in Drawing.batches)
        {
            if (batch.vertices.Count == 0) continue;
            var buffer = Gfx.UseBuffer("geometry" + index.ToString(), Gfx.BufferType.Storage, batch.vertices, version);
            if (buffer != null)
                Gfx.Draw(GameMath.integer(batch.vertices.Count / 8), new Dictionary<string, object> { ["verts"] = buffer }, new DrawOpts {
                    Shader = shader, Depth = batch.depth, DepthWrite = batch.depth,
                    Cull = batch.cull ? Gfx.Cull.Front : Gfx.Cull.None,
                    Blend = batch.blend ? (batch.alphaBlend ? Gfx.Blend.Alpha : Gfx.Blend.Additive) : Gfx.Blend.None });
            index++;
        }
        Gfx.EndPass();
    }
    public static void OnQuit() { manager.prefManager.save(); manager.close(); }
}
