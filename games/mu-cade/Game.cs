using System;
using System.Collections.Generic;
using Lub;

public static class Game
{
    public static GameManager manager;
    public static float elapsed;
    static bool scoresLoaded;
    public static ShaderRef shader;
    static string shaderSource;
    public static void OnInit()
    {
        manager = new GameManager();
        manager.init_0();
        manager.start_0();
        manager.rand.setSeed(TinySystem.Random.Shared.Next());
    }

    public static void OnFrame(float dt)
    {
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
            {
                manager.prefManager.loadText(scores);
                if (manager.state == GameManagerGameState.TITLE)
                    manager.titleManager.start_0();
            }
        }

        TwinStickPad.input = GameInput.Mask();

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
        GameAudio.Update(dt);
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Scene.Target, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw();
        Gfx.EndPass();
    }

    public static void OnEvent(EventData e) { }
    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
