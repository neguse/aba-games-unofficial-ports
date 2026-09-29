using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static GameManager manager;
    public static string savedReplay = "";
    public static float elapsed;
    public static ShaderRef shader;
    static string shaderSource;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
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
        bool wasFocused = TtRender.Active && TtRender.Focused;
        while (Host.Available())
        {
            Host.Poll(out string topic, out string payload);
            if (topic == null)
                break;
            if (topic.StartsWith("xr."))
            {
                TtRender.Receive(topic, payload);
                if (topic != "xr.frame") { elapsed = 0; dt = 0; }
            }
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

        if (TtRender.Active && !TtRender.Focused) return;
        if (TtRender.Active && !wasFocused) dt = 0;
        if (TtRender.Active) TtRender.ApplyInput(manager);
        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= 0.016f)
        {
            manager.move();
            elapsed = elapsed - (0.016f);
        }

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("torus-trooper", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        if (shader == null) return;
        if (TtRender.Active)
        {
            TtRender.Present(manager);
            return;
        }
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
