using System;
using System.Collections.Generic;
using Lub;

public static class Game
{
    public static Frame manager;
    static float elapsed, interval = 16;
    static bool scoresLoaded, replayLoaded;
    static int inputMask;
    public static ShaderRef shader;
    static string shaderSource;
    public static void OnInit()
    {
        manager = new Frame();
        manager.init_0();
        manager.start_0();
        manager.rand.setSeed(TinySystem.Random.Shared.Next());
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
        inputMask = GameInput.Mask();
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
            {
                manager.preference.load_1(scores);
                if (manager.gameState.isTitle())
                    manager.title.setMode(manager.preference.lastMode);
            }
        }

        if (!replayLoaded && GameSave.Ready("replay"))
        {
            replayLoaded = true;
            string? replay = GameSave.Load("replay");
            if (replay != null && replay.Length > 0 && manager.gameState.isTitle())
            {
                manager.savedReplay = replay;
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
        GameAudio.Update(dt);
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Scene.Target, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw_0();
        Gfx.EndPass();
    }

    public static void OnEvent(EventData e) { }
    public static void OnQuit()
    {
        manager.preference.save();
        manager.quit();
    }
}
