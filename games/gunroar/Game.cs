using System;
using System.Collections.Generic;
using Lub;

public static class Game
{
    public static GameManager manager;
    public static ShaderRef shader;
    static string shaderSource;
    static float elapsed;
    static int inputMask, mouseX = 320, mouseY = 240, mouseButtons;
    static bool scoresLoaded;
    // GameManager.loadLastReplay が立て、記録を読み終えたら下ろす。
    public static bool replayRequested;
    public static void OnInit()
    {
        manager = new GameManager();
        manager.init();
        manager.start();
        manager.inGameState.rand.setSeed(TinySystem.Random.Shared.Next());
    }

    // マウスを動かすかクリックしたときだけ、pointer をマウスの位置に合わせる。
    // ボタンはマウスと pad の両方から取り、フレームの間に押して離したクリックも押したことにする。
    static void readPointer()
    {
        if (!GameInput.Focused())
        {
            mouseButtons = 0;
            return;
        }
        Lub.Input.MouseDelta(out float dx, out float dy);
        if (dx != 0 || dy != 0 || Lub.Input.MousePressed(1) || Lub.Input.MousePressed(3))
        {
            mouseX = (int)Math.Clamp(GameInput.PointerX(), 0, 640);
            mouseY = (int)Math.Clamp(GameInput.PointerY(), 0, 480);
        }
        var pad = GameInput.Pad();
        mouseButtons = (Lub.Input.MouseDown(1) || Lub.Input.MousePressed(1) || pad.A ? 1 : 0) | (Lub.Input.MouseDown(3) || pad.Secondary ? 2 : 0);
    }

    public static void applyInput()
    {
        manager.pad.state.dir = (inputMask & 15) | ((inputMask >> 8) & 15) | ((inputMask >> 12) & 15);
        manager.pad.state.button = inputMask & 48;
        manager.pad.pause = (inputMask & 64) != 0;
        manager.pad.escape = (inputMask & 128) != 0;
        manager.twinStick.state.left.x = ((inputMask & 2048) != 0 ? 1 : 0) - ((inputMask & 1024) != 0 ? 1 : 0);
        manager.twinStick.state.left.y = ((inputMask & 256) != 0 ? 1 : 0) - ((inputMask & 512) != 0 ? 1 : 0);
        manager.twinStick.state.right.x = ((inputMask & 32768) != 0 ? 1 : 0) - ((inputMask & 16384) != 0 ? 1 : 0);
        manager.twinStick.state.right.y = ((inputMask & 4096) != 0 ? 1 : 0) - ((inputMask & 8192) != 0 ? 1 : 0);
        manager.mouse.state.x = (mouseX - 320) * 26f / 640;
        manager.mouse.state.y = -(mouseY - 240) * 19.5f / 480;
        manager.mouse.state.button = mouseButtons;
    }

    public static void OnFrame(float dt)
    {
        inputMask = GameInput.Mask();
        readPointer();
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
            {
                manager.prefManager.load(scores);
                if (manager.state == manager.titleState)
                    manager.titleManager.gameMode = manager.prefManager.prefData.gameMode;
            }
        }

        if (replayRequested && GameSave.Ready("replay"))
        {
            replayRequested = false;
            string? text = GameSave.Load("replay");
            ReplayData replay = new ReplayData();
            if (text != null && text.Length > 0 && manager.state == manager.titleState && replay.loadData(text))
            {
                manager.inGameState.replayData = replay;
                manager.startTitle();
            }
        }

        elapsed = elapsed + (Math.Min(dt, 0.1f));
        while (elapsed >= manager.interval / 1000)
        {
            elapsed = elapsed - (manager.interval / 1000);
            applyInput();
            manager.slowdownRatio = 0;
            manager.move();
            if (manager.slowdownRatio > 1)
                manager.interval = manager.interval + ((Math.Min(manager.slowdownRatio, 1.75f) * 16 - manager.interval) * 0.1f);
            else
                manager.interval = manager.interval + ((16 - manager.interval) * 0.08f);
        }

        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("gunroar", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        GameAudio.Update(dt);
        if (shader == null)
            return;
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
