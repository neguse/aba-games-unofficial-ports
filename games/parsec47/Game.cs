using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static P47GameManager manager;
    static float elapsed;
    public static ShaderRef shader;
    static string shaderSource;

    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
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
        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("parsec47", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        if (shader == null) return;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 0, 1 } });
        manager.draw();
        Gfx.EndPass();
    }
    public static void OnQuit() { manager.prefManager.save(); manager.close(); }
}
