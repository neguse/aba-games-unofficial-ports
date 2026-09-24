using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static Frame manager;
    static float elapsed, interval = 16;
    static int inputMask;
    public static ShaderRef shader;
    static string shaderSource;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        manager = new Frame();
        manager.init_0();
        manager.start_0();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("replay.load", "");
            Host.Send("ready", "");
        }
    }

    public static void applyInput()
    {
        manager.pad.state.dir = (inputMask & 15) | ((inputMask >> 8) & 15) | ((inputMask >> 12) & 15);
        manager.pad.state.button = inputMask & 48;
        manager.pad.pause = (inputMask & 64) != 0;
        manager.pad.escape = (inputMask & 128) != 0;
    }

    public static void OnFrame(float dt)
    {
        while (Host.Available())
        {
            Host.Poll(out string topic, out string payload);
            if (topic == null)
                break;
            if (topic == "input")
            {
                int value = GameMath.parseNonnegative(payload);
                if (value >= 0 && value <= 65535)
                    inputMask = value;
            }

            if (topic == "seed")
            {
                int seed = GameMath.parseNonnegative(payload);
                if (seed >= 0)
                    manager.rand.setSeed(seed);
            }

            if (topic == "scores")
            {
                manager.preference.load_1(payload);
                if (manager.gameState.isTitle())
                    manager.title.setMode(manager.preference.lastMode);
            }

            if (topic == "replay" && payload.Length > 0 && manager.gameState.isTitle())
            {
                manager.savedReplay = payload;
                manager.loadLastReplay();
                manager.startTitle();
            }
        }

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= interval / 1000)
        {
            elapsed = elapsed - (interval / 1000);
            applyInput();
            manager.slowdownRatio = 0;
            manager.move_0();
            if (manager.slowdownRatio > 1)
                interval = interval + ((Math.Min(manager.slowdownRatio, 1.5f) * 16 - interval) * 0.1f);
            else
                interval = interval + ((16 - interval) * 0.08f);
        }

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("titanion", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw_0();
        Gfx.EndPass();
    }

    public static void OnQuit()
    {
        manager.preference.save();
        manager.quit();
    }
}
