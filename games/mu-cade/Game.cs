using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static GameManager manager;
    public static float elapsed;
    public static ShaderRef shader;
    static string shaderSource;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
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

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("mu-cade", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw();
        Gfx.EndPass();
    }

    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
