// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class TitleManager
{
    public const float INITIAL_SCROLL_SPEED = 0.025f;
    public PrefManager prefManager;
    public RecordablePad pad;
    public RecordableMouse mouse;
    public Field field;
    public GameManager gameManager;
    public DisplayList displayList;
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
        displayList = new DisplayList(1);
        displayList.beginNewList();
        GunroarTitleImage.draw();
        GrScreen.lineWidth(3);
        glBegin(GL_LINE_STRIP);
        glVertex2f(-80, -7);
        glVertex2f(-20, -7);
        glVertex2f(10, -70);
        glEnd();
        glBegin(GL_LINE_STRIP);
        glVertex2f(45, -2);
        glVertex2f(-15, -2);
        glVertex2f(-45, 61);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        GrScreen.setColor(1, 1, 1);
        glVertex2f(-19, -6);
        GrScreen.setColor(0, 0, 0);
        glVertex2f(-79, -6);
        glVertex2f(11, -69);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        GrScreen.setColor(1, 1, 1);
        glVertex2f(-16, -3);
        GrScreen.setColor(0, 0, 0);
        glVertex2f(44, -3);
        glVertex2f(-46, 60);
        glEnd();
        GrScreen.lineWidth(1);
        displayList.endNewList();
        gameMode = prefManager.prefData.gameMode;
    }

    public void close()
    {
        displayList.close();
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

    public void draw()
    {
        if (gameMode < 0)
        {
            Letter.drawString("REPLAY", 3, 400, 5);
            return;
        }

        float ts = 1;
        if (cnt > 120)
        {
            ts = ts - ((cnt - 120) * 0.015f);
            if (ts < 0.5f)
                ts = 0.5f;
        }

        glPushMatrix();
        glTranslatef(80 * ts, 240, 0);
        glScalef(ts, ts, 0);
        displayList.call();
        glPopMatrix();
        if (cnt > 150)
        {
            Letter.drawString("HIGH", 3, 305, 4, LetterDirection.TO_RIGHT, 1);
            Letter.drawNum(prefManager.prefData.highScore(gameMode), 80, 320, 4, 0, 9);
        }

        if (cnt > 200)
        {
            Letter.drawString("LAST", 3, 345, 4, LetterDirection.TO_RIGHT, 1);
            int ls = 0;
            if (_replayData != null)
                ls = _replayData.score;
            Letter.drawNum(ls, 80, 360, 4, 0, 9);
        }

        Letter.drawString(InGameState.gameModeText[gameMode], 3, 400, 5);
    }
}
