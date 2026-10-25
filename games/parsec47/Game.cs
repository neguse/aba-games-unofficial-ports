using System;
using System.Collections.Generic;
using Lub;

public static class Game
{
    public static P47GameManager manager;
    static float elapsed;
    static bool scoresLoaded;
    public static ShaderRef shader;
    static string shaderSource;

    public static void OnInit()
    {
        manager = new P47GameManager();
        manager.init(); manager.start();
    }
    public static void OnFrame(float dt)
    {
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
            {
                manager.prefManager.load(scores);
                if (manager.state == P47GameManager.TITLE_STATE)
                {
                    manager.difficulty = manager.prefManager.selectedDifficulty;
                    manager.parsecSlot = manager.prefManager.selectedParsecSlot;
                    manager.mode = manager.prefManager.selectedMode;
                    manager.startTitle();
                }
            }
        }
        int input = GameInput.Mask();
        manager.pad.directions = input & 15;
        manager.pad.buttons = input & 48;
        manager.pad.pause = (input & 64) != 0;
        manager.pad.escape = (input & 128) != 0;
        elapsed += Math.Min(dt, 0.1f);
        while (elapsed >= manager.interval / 1000) { manager.move(); elapsed -= manager.interval / 1000; }
        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("parsec47", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        GameAudio.Update(dt);
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Scene.Target, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw();
        Gfx.EndPass();
    }
    public static void OnEvent(EventData e) { }
    public static void OnQuit() { manager.prefManager.save(); manager.close(); }
}
