using System;
using System.Collections.Generic;
using Lub;

public static class Game
{
    public static A7xGameManager manager;
    public static TextureRef glow;
    public static ShaderRef shader;
    static string shaderSource;
    static float elapsed;
    static bool scoresLoaded;
    public static void OnInit()
    {
        manager = new A7xGameManager();
        manager.init();
        manager.start();
        Rand.setSeed(TinySystem.Random.Shared.Next());
    }

    public static void OnFrame(float dt)
    {
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
                manager.prefManager.load(scores);
        }

        int input = GameInput.Mask();
        manager.input.directions = input & 15;
        manager.input.buttons = input & 48;
        manager.input.pause = (input & 64) != 0;
        manager.input.escape = (input & 128) != 0;

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= 0.016f)
        {
            manager.move();
            elapsed = elapsed - (0.016f);
        }

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("a7xpg", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        glow = Gfx.UseTexture("glow", 128, 128, Gfx.PixelFormat.Rgba8, null,
            glow == null ? (int?)null : glow.Version, new TextureOpts { Target = true });
        GameAudio.Update(dt);
        if (shader == null || glow == null) return;
        manager.draw();
    }

    public static void OnEvent(EventData e) { }
    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
