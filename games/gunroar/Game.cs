using System;
using System.Collections.Generic;
using static Lub;

public static class Game
{
    public static GameManager manager;
    static float elapsed;
    static int version, inputMask, mouseX = 320, mouseY = 240, mouseButtons;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        Drawing.BeginFrame();
        Drawing.recordBlend = true;
        Drawing.premultiplyAdditive = true;
        manager = new GameManager();
        manager.init();
        manager.start();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("ready", "");
        }
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

            if (topic == "pointer")
            {
                string[] values = payload.Split(",");
                if (values.Length == 3)
                {
                    int x = GameMath.parseNonnegative(values[0]), y = GameMath.parseNonnegative(values[1]), buttons = GameMath.parseNonnegative(values[2]);
                    if (x >= 0 && x <= 640 && y >= 0 && y <= 480 && buttons >= 0 && buttons <= 3)
                    {
                        mouseX = x;
                        mouseY = y;
                        mouseButtons = buttons;
                    }
                }
            }

            if (topic == "seed")
            {
                int seed = GameMath.parseNonnegative(payload);
                if (seed >= 0)
                    manager.inGameState.rand.setSeed(seed);
            }

            if (topic == "scores")
            {
                manager.prefManager.load(payload);
                if (manager.state == manager.titleState)
                    manager.titleManager.gameMode = manager.prefManager.prefData.gameMode;
            }

            if (topic == "replay" && payload.Length > 0 && manager.state == manager.titleState)
            {
                ReplayData replay = new ReplayData();
                if (replay.loadData(payload))
                {
                    manager.inGameState.replayData = replay;
                    manager.startTitle();
                }
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

        manager.draw();
        var shader = Gfx.UseShader("gunroar", GameShaders.vertex, GameShaders.fragment, 1);
        if (shader == null)
            return;
        version++;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = Drawing.clearColor });
        int index = 0;
        foreach (var batch in Drawing.batches)
        {
            if (batch.vertices.Count == 0)
                continue;
            var buffer = Gfx.UseBuffer("geometry" + index.ToString(), Gfx.BufferType.Storage, batch.vertices, version);
            if (buffer != null)
                Gfx.Draw(GameMath.integer(batch.vertices.Count / 8), TextureDrawing.Bindings(buffer, batch, index, version), new DrawOpts { Shader = shader, Depth = batch.depth, DepthWrite = batch.depth, Cull = batch.cull ? Gfx.Cull.Front : Gfx.Cull.None, Blend = batch.multiply ? Gfx.Blend.Multiply : batch.blend ? (batch.alphaBlend ? Gfx.Blend.Alpha : Gfx.Blend.Additive) : Gfx.Blend.None });
            index++;
        }

        Gfx.EndPass();
    }

    public static void OnQuit()
    {
        manager.prefManager.save();
        manager.close();
    }
}
