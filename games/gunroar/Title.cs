// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class TitleManager
{
    public const float INITIAL_SCROLL_SPEED = 0.025f;
    public PrefManager prefManager;
    public RecordablePad pad;
    public RecordableMouse mouse;
    public Field field;
    public GameManager gameManager;
    public Mesh logo, decoration;
    public int cnt;
    public ReplayData _replayData;
    public ReplayData replayData
    {
        set
        {
            _replayData = value;
        }
    }

    public int btnPressedCnt;
    public int gameMode;
    public TitleManager(PrefManager prefManager, Pad pad, Mouse mouse, Field field, GameManager gameManager)
    {
        this.prefManager = prefManager;
        this.pad = (RecordablePad)pad;
        this.mouse = (RecordableMouse)mouse;
        this.field = field;
        this.gameManager = gameManager;
        init();
    }

    public void init()
    {
        logo = new Mesh("title-logo");
        logo.Vertex(0, -63, 0, new float[] { 0, 0, 0, 1 });
        logo.Vertex(255, -63, 0, new float[] { 1, 0, 0, 1 });
        logo.Vertex(255, 0, 0, new float[] { 1, 1, 0, 1 });
        logo.Vertex(0, 0, 0, new float[] { 0, 1, 0, 1 });
        logo.Quads(0, 4);
        decoration = new Mesh("title-decoration");
        float[] color = new float[] { 1, 1, 1, 1 };
        int first1 = decoration.vertexCount;
        decoration.Vertex(-80, -7, 0, color);
        decoration.Vertex(-20, -7, 0, color);
        decoration.Vertex(10, -70, 0, color);
        decoration.LineStrip(first1, decoration.vertexCount - first1);
        int first2 = decoration.vertexCount;
        decoration.Vertex(45, -2, 0, color);
        decoration.Vertex(-15, -2, 0, color);
        decoration.Vertex(-45, 61, 0, color);
        decoration.LineStrip(first2, decoration.vertexCount - first2);
        int first3 = decoration.vertexCount;
        color = new float[] { 1, 1, 1, 1 };
        decoration.Vertex(-19, -6, 0, color);
        color = new float[] { 0, 0, 0, 1 };
        decoration.Vertex(-79, -6, 0, color);
        decoration.Vertex(11, -69, 0, color);
        decoration.Fan(first3, decoration.vertexCount - first3);
        int first4 = decoration.vertexCount;
        color = new float[] { 1, 1, 1, 1 };
        decoration.Vertex(-16, -3, 0, color);
        color = new float[] { 0, 0, 0, 1 };
        decoration.Vertex(44, -3, 0, color);
        decoration.Vertex(-46, 60, 0, color);
        decoration.Fan(first4, decoration.vertexCount - first4);
        gameMode = prefManager.prefData.gameMode;
    }

    public void close()
    {
        logo = null; decoration = null;
    }

    public void start()
    {
        cnt = 0;
        field.start();
        btnPressedCnt = 1;
    }

    public void move()
    {
        if (!(_replayData != null))
        {
            field.move();
            field.scroll(INITIAL_SCROLL_SPEED, true);
        }

        PadState input = pad.getState(false);
        MouseState mouseInput = mouse.getState(false);
        if (btnPressedCnt <= 0)
        {
            if ((((input.button & PadStateButton.A) != 0) || ((gameMode == InGameStateGameMode.MOUSE) && ((mouseInput.button & MouseStateButton.LEFT) != 0))) && (gameMode >= 0))
                gameManager.startInGame(gameMode);
            int gmc = 0;
            if (((input.button & PadStateButton.B) != 0) || ((input.dir & PadStateDir.DOWN) != 0))
                gmc = 1;
            else if ((input.dir & PadStateDir.UP) != 0)
                gmc = -1;
            if (gmc != 0)
            {
                gameMode = gameMode + (gmc);
                if (gameMode >= InGameState.GAME_MODE_NUM)
                    gameMode = -1;
                else if (gameMode < -1)
                    gameMode = InGameState.GAME_MODE_NUM - 1;
                if ((gameMode == -1) && (_replayData != null))
                {
                    SoundManager.enableBgm();
                    SoundManager.enableSe();
                    SoundManager.playCurrentBgm();
                }
                else
                {
                    SoundManager.fadeBgm();
                    SoundManager.disableBgm();
                    SoundManager.disableSe();
                }
            }
        }

        if ((((input.button & (PadStateButton.A | PadStateButton.B)) != 0) || ((input.dir & (PadStateDir.UP | PadStateDir.DOWN)) != 0)) || ((mouseInput.button & MouseStateButton.LEFT) != 0))
            btnPressedCnt = 6;
        else
            btnPressedCnt--;
        cnt++;
    }

    public void draw(float[] model)
    {
        if (gameMode < 0)
        {
            Letter.drawString(model, "REPLAY", 3, 400, 5);
            return;
        }

        float ts = 1;
        if (cnt > 120)
        {
            ts = ts - ((cnt - 120) * 0.015f);
            if (ts < 0.5f)
                ts = 0.5f;
        }

        float[] parent1 = model;
        model = Transform.Translate(model, 80 * ts, 240, 0);
        model = Transform.Scale(model, ts, ts, 0);
        Gfx.Draw(logo.count, logo.Bindings(model, null, 1, true, 0, GunroarTitleImage.title),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        Gfx.Draw(decoration.count, decoration.Bindings(model, null, 3, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        model = parent1;
        if (cnt > 150)
        {
            Letter.drawString(model, "HIGH", 3, 305, 4, LetterDirection.TO_RIGHT, 1);
            Letter.drawNum(model, prefManager.prefData.highScore(gameMode), 80, 320, 4, 0, 9);
        }

        if (cnt > 200)
        {
            Letter.drawString(model, "LAST", 3, 345, 4, LetterDirection.TO_RIGHT, 1);
            int ls = 0;
            if (_replayData != null)
                ls = _replayData.score;
            Letter.drawNum(model, ls, 80, 360, 4, 0, 9);
        }

        Letter.drawString(model, InGameState.gameModeText[gameMode], 3, 400, 5);
    }
}
