// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Title
{
    public Preference preference;
    public RecordablePad pad;
    public Frame frame;
    public int cnt;
    public bool aPressed, udPressed;
    public Vector titlePos;
    public float titleSize;
    public int cursorIdx;
    public Title(Preference preference, Pad pad, Frame frame)
    {
        this.preference = preference;
        this.pad = (pad is RecordablePad ? (RecordablePad)pad : null);
        this.frame = frame;
        titlePos = new Vector();
        cursorIdx = 0;
    }

    public virtual void init_0()
    {
    }

    public virtual void close()
    {
    }

    public virtual void setMode(int mode)
    {
        cursorIdx = mode;
    }

    public virtual void start_0()
    {
        cnt = 0;
        aPressed = true;
        udPressed = true;
        titlePos.x = 150;
        titlePos.y = 150;
        titleSize = 1.0f;
    }

    public virtual void move_0()
    {
        PadState input = null;
        input = pad.getState(false);
        if ((input.button & PadStateButton.A) != 0)
        {
            if (!((aPressed)))
            {
                aPressed = true;
                frame.startInGame(cursorIdx);
            }
        }
        else
        {
            aPressed = false;
        }

        if ((input.dir & (PadStateDir.UP | PadStateDir.DOWN)) != 0)
        {
            if (!((udPressed)))
            {
                udPressed = true;
                if ((input.dir & PadStateDir.UP) != 0)
                    cursorIdx--;
                else if ((input.dir & PadStateDir.DOWN) != 0)
                    cursorIdx++;
                if (cursorIdx < 0)
                    cursorIdx = GameState.MODE_NUM - 1;
                else if (cursorIdx > GameState.MODE_NUM - 1)
                    cursorIdx = 0;
            }
        }
        else
        {
            udPressed = false;
        }

        if (((cnt > 180)) && ((cnt < 235)))
            titlePos.y = titlePos.y - (2);
        if (((cnt > 600)) && ((cnt < 675)))
        {
            titlePos.x = titlePos.x - (2);
            titlePos.y++;
            titleSize = titleSize - (0.007f);
        }

        cnt++;
    }

    public virtual void draw_0()
    {
        TtnScreen.setColor(1, 1, 1);
        drawBoard(titlePos.x, titlePos.y, 280 * titleSize, 64 * titleSize);
        if ((cnt % 120) < 60)
        {
            float x = 175, sz = 6;
            if (cnt >= 600)
            {
                int c = cnt - 600;
                if (c > 75)
                    c = 75;
                x = x + (c * 4.33f);
                sz = sz - (c * 0.045f);
            }

            Letter.drawString("PUSH SHOT BUTTON TO START", x, 440, sz);
        }

        if (cnt >= 240)
        {
            drawRanking();
        }

        if ((cnt % 60) < 30)
        {
            drawTriangle(575, 398, 180);
            drawTriangle(575, 417, 0);
        }

        Letter.drawString(GameState.MODE_NAME[cursorIdx], 540, 400, 5);
    }

    public virtual void drawBoard(float x, float y, float w, float h)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(w / 280, h / 64, 1);
        TitanionTitleImage.draw();
        glPopMatrix();
    }

    public virtual void drawTriangle(float x, float y, float d)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glRotatef(d, 0, 0, 1);
        glScalef(5, 5, 1);
        glBegin(GL_TRIANGLE_FAN);
        TtnScreen.setColor(1, 1, 1, 0.5f);
        glVertex3f(0, 1.7f, 0);
        glVertex3f(1, 0, 0);
        glVertex3f(-1, 0, 0);
        glEnd();
        glBegin(GL_LINE_LOOP);
        TtnScreen.setColor(1, 1, 1, 1);
        glVertex3f(0, 1.7f, 0);
        glVertex3f(1, 0, 0);
        glVertex3f(-1, 0, 0);
        glEnd();
        glPopMatrix();
    }

    public virtual void drawRanking()
    {
        int rn = GameMath.integer((cnt - 240) / 30);
        if (rn > Preference.RANKING_NUM)
            rn = Preference.RANKING_NUM;
        float y = 140;
        for (int i = 0; i < rn; i++)
        {
            if (cnt < 600)
            {
                string rstr = null;
                switch (i)
                {
                    case 0:
                        rstr = "1ST";
                        break;
                    case 1:
                        rstr = "2ND";
                        break;
                    case 2:
                        rstr = "3RD";
                        break;
                    default:
                        rstr = (i + 1).ToString() + "TH";
                        break;
                }

                if (i < 9)
                    Letter.drawString(rstr, 180, y, 7);
                else
                    Letter.drawString(rstr, 166, y, 7);
            }

            float sx = 450, sy = y, sz = 6;
            if (cnt >= 600)
            {
                int c = cnt - 600;
                if (c > 75)
                    c = 75;
                sx = sx + (GameMath.integer((c * 2.35f)));
                sz = sz - (c * 0.03f);
            }

            Letter.drawNum(preference.highScore[cursorIdx][i], sx, sy, sz);
            y = y + (24);
        }
    }
}
