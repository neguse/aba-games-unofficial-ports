// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Title
{
    public Mesh logo = new Mesh("title-logo");
    Mesh marker = new Mesh("title-marker");
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
        logo.Vertex(0, 0, 0, new float[] { 0, 0, 0, 1 });
        logo.Vertex(280, 0, 0, new float[] { 1, 0, 0, 1 });
        logo.Vertex(280, 64, 0, new float[] { 1, 1, 0, 1 });
        logo.Vertex(0, 64, 0, new float[] { 0, 1, 0, 1 });
        logo.Quads(0, 4);
        marker.Vertex(0, 1.7f, 0, new float[] { 1, 1, 1, 0.5f });
        marker.Vertex(1, 0, 0, new float[] { 1, 1, 1, 0.5f });
        marker.Vertex(-1, 0, 0, new float[] { 1, 1, 1, 0.5f });
        marker.Fan(0, 3);
        marker.Vertex(0, 1.7f, 0, new float[] { 1, 1, 1, 1 });
        marker.Vertex(1, 0, 0, new float[] { 1, 1, 1, 1 });
        marker.Vertex(-1, 0, 0, new float[] { 1, 1, 1, 1 });
        marker.LineStrip(3, 3, true);
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

    public virtual void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        color = new float[] { 1, 1, 1, 1 };
        drawBoard(model, color, blend, titlePos.x, titlePos.y, 280 * titleSize, 64 * titleSize);
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

            Letter.drawString(model, color, blend, "PUSH SHOT BUTTON TO START", x, 440, sz);
        }

        if (cnt >= 240)
        {
            drawRanking(model, color, blend);
        }

        if ((cnt % 60) < 30)
        {
            drawTriangle(model, color, blend, 575, 398, 180);
            drawTriangle(model, color, blend, 575, 417, 0);
        }

        Letter.drawString(model, color, blend, GameState.MODE_NAME[cursorIdx], 540, 400, 5);
    }

    public virtual void drawBoard(float[] model, float[] color, Gfx.Blend blend, float x, float y, float w, float h)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, w / 280, h / 64, 1);
        Gfx.Draw(logo.count, logo.Bindings(model, new float[] { 1, 1, 1, 1 }, 1, blend == Gfx.Blend.Additive, 0, TitanionTitleImage.title),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public virtual void drawTriangle(float[] model, float[] color, Gfx.Blend blend, float x, float y, float d)
    {
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Rotate(model, d, 0, 0, 1);
        model = Transform.Scale(model, 5, 5, 1);
        Gfx.Draw(marker.count, marker.Bindings(model, null, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public virtual void drawRanking(float[] model, float[] color, Gfx.Blend blend)
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
                    Letter.drawString(model, color, blend, rstr, 180, y, 7);
                else
                    Letter.drawString(model, color, blend, rstr, 166, y, 7);
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

            Letter.drawNum(model, color, blend, preference.highScore[cursorIdx][i], sx, sy, sz);
            y = y + (24);
        }
    }
}
