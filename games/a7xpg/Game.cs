using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static A7xGameManager manager;
    public static TextureRef glow;
    public static ShaderRef shader;
    static string shaderSource;
    static float elapsed;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
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

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("a7xpg", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        glow = Gfx.UseTexture("glow", 128, 128, Gfx.PixelFormat.Rgba8, null,
            glow == null ? (int?)null : glow.Version, new TextureOpts { Target = true });
        if (shader == null || glow == null) return;
        manager.draw();
    }

    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
