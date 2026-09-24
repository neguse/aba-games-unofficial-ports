// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Letter
{
    public static Mesh[] meshes;
    public const float LETTER_WIDTH = 2.1f;
    public const float LETTER_HEIGHT = 3.0f;
    public const int LETTER_NUM = 44;
    public const int DISPLAY_LIST_NUM = LETTER_NUM;
    public static void init_0()
    {
        meshes = new Mesh[DISPLAY_LIST_NUM];
        float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null;
        for (int i = 0; i < LETTER_NUM; i++)
        {
            mesh = new Mesh("letter-" + i.ToString()); meshes[i] = mesh;
            appendLetter(mesh, model, tint, i);

        }
    }

    public static void close()
    {
        meshes = null;
    }

    public static float getWidth(int n, float s)
    {
        return n * s * LETTER_WIDTH;
    }

    public static float getWidthNum(int num, float s)
    {
        int dg = 1;
        int n = num;
        int c = 1;
        for (;;)
        {
            if (n < 10)
                break;
            n = integer(n / 10);
            c++;
        }

        return c * s * LETTER_WIDTH;
    }

    public static float getHeight(float s)
    {
        return s * LETTER_HEIGHT;
    }

    public static void drawLetter_1(float[] model, float[] tint, Gfx.Blend blend, string key, int n)
    {
        { Mesh shape1 = meshes[n]; if (shape1.count > 0) Gfx.Draw(shape1.count, shape1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
    }

    public static void drawLetter_5(float[] model, float[] tint, Gfx.Blend blend, string key, int n, float x, float y, float s, float d)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, s, s);
        model = Transform.Rotate(model, d, 0, 0, 1);
        { Mesh shape2 = meshes[n]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public static void drawLetterRev(float[] model, float[] tint, Gfx.Blend blend, string key, int n, float x, float y, float s, float d)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, -s, s);
        model = Transform.Rotate(model, d, 0, 0, 1);
        { Mesh shape2 = meshes[n]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public static int convertCharToInt(string c){
        int idx="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+??!/".IndexOf(c);
        if(idx>=0)return idx;
        idx="abcdefghijklmnopqrstuvwxyz".IndexOf(c);
        return idx>=0?idx+10:0;
    }

    public static void drawString(float[] model, float[] tint, Gfx.Blend blend, string key, string str, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, bool rev = false, float od = 0)
    {
        lx += LETTER_WIDTH * s / 2;
        y += LETTER_HEIGHT * s / 2;
        float x = lx;
        int idx = default(int);
        float ld = default(float);
        switch (d)
        {
            case LetterDirection.TO_RIGHT:
                ld = 0;
                break;
            case LetterDirection.TO_DOWN:
                ld = 90;
                break;
            case LetterDirection.TO_LEFT:
                ld = 180;
                break;
            case LetterDirection.TO_UP:
                ld = 270;
                break;
        }

        ld += od;
        for(int character=0;character<str.Length;character++)
        {
            string c=str.Substring(character,1);
            if (c != " ")
            {
                idx = convertCharToInt(c);
                if (rev)
                    drawLetterRev(model, tint, blend, key + "-drawLetterRev-1" + "-" + character.ToString(), idx, x, y, s, ld);
                else
                    drawLetter_5(model, tint, blend, key + "-drawLetter_5-1" + "-" + character.ToString(), idx, x, y, s, ld);
            }

            if (od == 0)
            {
                switch (d)
                {
                    case LetterDirection.TO_RIGHT:
                        x += s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_DOWN:
                        y += s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_LEFT:
                        x -= s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_UP:
                        y -= s * LETTER_WIDTH;
                        break;
                }
            }
            else
            {
                x += cos(ld * PI / 180) * s * LETTER_WIDTH;
                y += sin(ld * PI / 180) * s * LETTER_WIDTH;
            }
        }
    }

    public static void drawNum(float[] model, float[] tint, Gfx.Blend blend, string key, int num, float lx, float y, float s, int dg = 0, int headChar = -1, int floatDigit = -1)
    {
        lx += LETTER_WIDTH * s / 2;
        y += LETTER_HEIGHT * s / 2;
        int n = num;
        float x = lx;
        float ld = 0;
        int digit = dg;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetter_5(model, tint, blend, key + "-drawLetter_5-1", n % 10, x, y, s, ld);
                x -= s * LETTER_WIDTH;
            }
            else
            {
                drawLetter_5(model, tint, blend, key + "-drawLetter_5-2", n % 10, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x -= s * LETTER_WIDTH * 0.5f;
            }

            n = integer(n / 10);
            digit--;
            fd--;
            if (n <= 0 && digit <= 0 && fd < 0)
                break;
            if (fd == 0)
            {
                drawLetter_5(model, tint, blend, key + "-drawLetter_5-3", 36, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x -= s * LETTER_WIDTH * 0.5f;
            }
        }

        if (headChar >= 0)
            drawLetter_5(model, tint, blend, key + "-drawLetter_5-4", headChar, x + s * LETTER_WIDTH * 0.2f, y + s * LETTER_WIDTH * 0.2f, s * 0.6f, ld);
    }

    public static void drawNumSign(float[] model, float[] tint, Gfx.Blend blend, string key, int num, float lx, float ly, float s, int headChar = -1, int floatDigit = -1)
    {
        float x = lx;
        float y = ly;
        int n = num;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetterRev(model, tint, blend, key + "-drawLetterRev-1", n % 10, x, y, s, 0);
                x -= s * LETTER_WIDTH;
            }
            else
            {
                drawLetterRev(model, tint, blend, key + "-drawLetterRev-2", n % 10, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x -= s * LETTER_WIDTH * 0.5f;
            }

            n = integer(n / 10);
            if (n <= 0)
                break;
            fd--;
            if (fd == 0)
            {
                drawLetterRev(model, tint, blend, key + "-drawLetterRev-3", 36, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x -= s * LETTER_WIDTH * 0.5f;
            }
        }

        if (headChar >= 0)
            drawLetterRev(model, tint, blend, key + "-drawLetterRev-4", headChar, x + s * LETTER_WIDTH * 0.2f, y - s * LETTER_WIDTH * 0.2f, s * 0.6f, 0);
    }

    public static void drawTime(float[] model, float[] tint, Gfx.Blend blend, string key, int time, float lx, float y, float s)
    {
        int n = time;
        if (n < 0)
            n = 0;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter_5(model, tint, blend, key + "-drawLetter_5-1" + "-" + i.ToString(), n % 10, x, y, s, LetterDirection.TO_RIGHT);
                n = integer(n / 10);
            }
            else
            {
                drawLetter_5(model, tint, blend, key + "-drawLetter_5-2" + "-" + i.ToString(), n % 6, x, y, s, LetterDirection.TO_RIGHT);
                n = integer(n / 6);
            }

            if ((i & 1) == 1 || i == 0)
            {
                switch (i)
                {
                    case 3:
                        drawLetter_5(model, tint, blend, key + "-drawLetter_5-3" + "-" + i.ToString(), 41, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
                        break;
                    case 5:
                        drawLetter_5(model, tint, blend, key + "-drawLetter_5-4" + "-" + i.ToString(), 40, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
                        break;
                    default:
                        break;
                }

                x -= s * LETTER_WIDTH;
            }
            else
            {
                x -= s * LETTER_WIDTH * 1.3f;
            }

            if (n <= 0)
                break;
        }
    }

    public static void appendLetter(Mesh mesh, float[] model, float[] tint, int idx)
    {
        float x = default(float), y = default(float), length = default(float), size = default(float), t = default(float);
        float deg = default(float);
        for (int i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            y *= 0.9f;
            size *= 1.4f;
            length *= 1.05f;
            x = -x;
            y = y;
            deg %= 180;
            appendSegment(mesh, model, tint, x, y, size, length, deg);
        }
    }

    public static void appendSegment(Mesh mesh, float[] model, float[] tint, float x, float y, float width, float height, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x - width / 2, y, 0);
        model = Transform.Rotate(model, deg, 0, 0, 1);
        tint = new float[] { 1, 1, 1, 0.5f };
        int part2 = mesh.vertexCount;
        appendSegmentPart(mesh, model, tint, width, height);
        mesh.Fan(part2, mesh.vertexCount - part2);
        tint = new float[] { 1, 1, 1, 1 };
        int part3 = mesh.vertexCount;
        appendSegmentPart(mesh, model, tint, width, height);
        mesh.LineStrip(part3, mesh.vertexCount - part3, true);
        model = parent1;
    }

    public static void appendSegmentPart(Mesh mesh, float[] model, float[] tint, float width, float height)
    {
        mesh.Vertex(-width / 2, 0, 0, tint, model);
        mesh.Vertex(-width / 3 * 1, -height / 2, 0, tint, model);
        mesh.Vertex(width / 3 * 1, -height / 2, 0, tint, model);
        mesh.Vertex(width / 2, 0, 0, tint, model);
        mesh.Vertex(width / 3 * 1, height / 2, 0, tint, model);
        mesh.Vertex(-width / 3 * 1, height / 2, 0, tint, model);
    }

    public static float[][][] spData = McdData.Letters();
}

public static class LetterDirection
{
    public const int TO_RIGHT = 0;
    public const int TO_DOWN = 1;
    public const int TO_LEFT = 2;
    public const int TO_UP = 3;
}
