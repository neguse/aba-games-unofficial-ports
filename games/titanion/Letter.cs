// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Letter
{
    public static Mesh[] meshes = new Mesh[DISPLAY_LIST_NUM];
    public const float LETTER_WIDTH = 2.1f;
    public const float LETTER_HEIGHT = 3.0f;
    public const int LETTER_NUM = 44;
    public const int DISPLAY_LIST_NUM = LETTER_NUM * 3;
    public static void init_0()
    {
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < LETTER_NUM; i++)
            {
                var mesh = new Mesh("letter-" + (i + j * LETTER_NUM).ToString());
                setLetter(mesh, Transform.Identity(), null, 0, i, j);
                meshes[i + j * LETTER_NUM] = mesh;
            }
        }
    }

    public static void close()
    {
        meshes = new Mesh[DISPLAY_LIST_NUM];
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
            n = GameMath.integer(n / (10));
            c++;
        }

        return c * s * LETTER_WIDTH;
    }

    public static float getHeight(float s)
    {
        return s * LETTER_HEIGHT;
    }

    public static void drawLetter_1(float[] model, float[] color, Gfx.Blend blend, int n)
    {
        Mesh mesh = meshes[n];
        Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawLetter_5(float[] model, float[] color, Gfx.Blend blend, int n, float x, float y, float s, float d)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, s, s);
        model = Transform.Rotate(model, d, 0, 0, 1);
        Mesh mesh = meshes[n];
        Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public static void drawLetterRev(float[] model, float[] color, Gfx.Blend blend, int n, float x, float y, float s, float d)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, -s, s);
        model = Transform.Rotate(model, d, 0, 0, 1);
        Mesh mesh = meshes[n];
        Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public static int convertCharToInt(string c)
    {
        return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+  !/".IndexOf(c.ToUpper());
    }

    public static void drawString(float[] model, float[] color, Gfx.Blend blend, string str, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, bool rev = false, float od = 0, float r = 1, float g = 1, float b = 1)
    {
        lx = lx + (LETTER_WIDTH * s / 2);
        y = y + (LETTER_HEIGHT * s / 2);
        float x = lx;
        int idx = 0;
        float ld = 0;
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

        ld = ld + (od);
        for (int ci = 0; ci < str.Length; ci++)
        {
            if (str.Substring(ci, 1) != " ")
            {
                idx = convertCharToInt(str.Substring(ci, 1));
                if (((((r == 1)) && ((g == 1)))) && ((b == 1)))
                {
                    if (rev)
                        drawLetterRev(model, color, blend, idx, x, y, s, ld);
                    else
                        drawLetter_5(model, color, blend, idx, x, y, s, ld);
                }
                else
                {
                    color = new float[] { r, g, b, 0.5f };
                    if (rev)
                        drawLetterRev(model, color, blend, idx + LETTER_NUM, x, y, s, ld);
                    else
                        drawLetter_5(model, color, blend, idx + LETTER_NUM, x, y, s, ld);
                    color = new float[] { r, g, b, 1 };
                    if (rev)
                        drawLetterRev(model, color, blend, idx + LETTER_NUM * 2, x, y, s, ld);
                    else
                        drawLetter_5(model, color, blend, idx + LETTER_NUM * 2, x, y, s, ld);
                }
            }

            if (od == 0)
            {
                switch (d)
                {
                    case LetterDirection.TO_RIGHT:
                        x = x + (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_DOWN:
                        y = y + (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_LEFT:
                        x = x - (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_UP:
                        y = y - (s * LETTER_WIDTH);
                        break;
                }
            }
            else
            {
                x = x + (cos(ld * PI / 180) * s * LETTER_WIDTH);
                y = y + (sin(ld * PI / 180) * s * LETTER_WIDTH);
            }
        }
    }

    public static void drawNum(float[] model, float[] color, Gfx.Blend blend, int num, float lx, float y, float s, int dg = 0, int headChar = -1, int floatDigit = -1)
    {
        lx = lx + (LETTER_WIDTH * s / 2);
        y = y + (LETTER_HEIGHT * s / 2);
        int n = num;
        float x = lx;
        float ld = 0;
        int digit = dg;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetter_5(model, color, blend, n % 10, x, y, s, ld);
                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                drawLetter_5(model, color, blend, n % 10, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }

            n = GameMath.integer(n / (10));
            digit--;
            fd--;
            if (((((n <= 0)) && ((digit <= 0)))) && ((fd < 0)))
                break;
            if (fd == 0)
            {
                drawLetter_5(model, color, blend, 36, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }
        }

        if (headChar >= 0)
            drawLetter_5(model, color, blend, headChar, x + s * LETTER_WIDTH * 0.2f, y + s * LETTER_WIDTH * 0.2f, s * 0.6f, ld);
    }

    public static void drawNumSign(float[] model, float[] color, Gfx.Blend blend, int num, float lx, float ly, float s, int headChar = -1, int floatDigit = -1, int type = 0)
    {
        float x = lx;
        float y = ly;
        int n = num;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetterRev(model, color, blend, n % 10 + type * LETTER_NUM, x, y, s, 0);
                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                drawLetterRev(model, color, blend, n % 10 + type * LETTER_NUM, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }

            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
            fd--;
            if (fd == 0)
            {
                drawLetterRev(model, color, blend, 36 + type * LETTER_NUM, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }
        }

        if (headChar >= 0)
            drawLetterRev(model, color, blend, headChar + type * LETTER_NUM, x + s * LETTER_WIDTH * 0.2f, y - s * LETTER_WIDTH * 0.2f, s * 0.6f, 0);
    }

    public static void drawTime(float[] model, float[] color, Gfx.Blend blend, int time, float lx, float y, float s)
    {
        int n = time;
        if (n < 0)
            n = 0;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter_5(model, color, blend, n % 10, x, y, s, LetterDirection.TO_RIGHT);
                n = GameMath.integer(n / (10));
            }
            else
            {
                drawLetter_5(model, color, blend, n % 6, x, y, s, LetterDirection.TO_RIGHT);
                n = GameMath.integer(n / (6));
            }

            if ((((i & 1) == 1)) || ((i == 0)))
            {
                switch (i)
                {
                    case 3:
                        drawLetter_5(model, color, blend, 41, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
                        break;
                    case 5:
                        drawLetter_5(model, color, blend, 40, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
                        break;
                    default:
                        break;
                }

                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                x = x - (s * LETTER_WIDTH * 1.3f);
            }

            if (n <= 0)
                break;
        }
    }

    public static void setLetter(Mesh mesh, float[] model, float[] color, int material, int idx, int type = LetterShape.NORMAL)
    {
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        float deg = 0;
        for (int i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            y = y * (0.9f);
            size = size * (1.4f);
            length = length * (1.05f);
            x = -x;
            y = y;
            deg = deg % (180);
            switch (type)
            {
                case LetterShape.NORMAL:
                    appendSegment(mesh, model, color, material, x, y, size, length, deg);
                    break;
                case LetterShape.POLYGON:
                    appendSegmentPolygon(mesh, model, color, material, x, y, size, length, deg);
                    break;
                case LetterShape.LINE:
                    appendSegmentLine(mesh, model, color, material, x, y, size, length, deg);
                    break;
            }
        }
    }

    public static void appendSegment(Mesh mesh, float[] model, float[] color, int material, float x, float y, float width, float height, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x - width / 2, y, 0);
        model = Transform.Rotate(model, deg, 0, 0, 1);
        color = new float[] { 1, 1, 1, 0.5f };
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        appendSegmentPart(mesh, model, color, material, width, height);
        mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, material);
        color = new float[] { 1, 1, 1, 1 };
        int part3 = mesh.vertexCount; int face3 = mesh.count;
        appendSegmentPart(mesh, model, color, material, width, height);
        mesh.LineStrip(part3, mesh.vertexCount - part3, true); mesh.AddRange(face3, material);
        model = parent1;
    }

    public static void appendSegmentPolygon(Mesh mesh, float[] model, float[] color, int material, float x, float y, float width, float height, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x - width / 2, y, 0);
        model = Transform.Rotate(model, deg, 0, 0, 1);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        appendSegmentPart(mesh, model, color, material, width, height);
        mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, material);
        model = parent1;
    }

    public static void appendSegmentLine(Mesh mesh, float[] model, float[] color, int material, float x, float y, float width, float height, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x - width / 2, y, 0);
        model = Transform.Rotate(model, deg, 0, 0, 1);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        appendSegmentPart(mesh, model, color, material, width, height);
        mesh.LineStrip(part2, mesh.vertexCount - part2, true); mesh.AddRange(face2, material);
        model = parent1;
    }

    public static void appendSegmentPart(Mesh mesh, float[] model, float[] color, int material, float width, float height)
    {
        mesh.Vertex(-width / 2, 0, 0, color, model);
        mesh.Vertex(-width / 3 * 1, -height / 2, 0, color, model);
        mesh.Vertex(width / 3 * 1, -height / 2, 0, color, model);
        mesh.Vertex(width / 2, 0, 0, color, model);
        mesh.Vertex(width / 3 * 1, height / 2, 0, color, model);
        mesh.Vertex(-width / 3 * 1, height / 2, 0, color, model);
    }

    public static float[][][] spData = new float[][][] { 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.6f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.6f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.5f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.18f, 1.15f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.18f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.15f, 1.15f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, 0.45f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.05f, 0f, 0.3f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.7f, -0.7f, 0.3f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.4f, 0.55f, 0.65f, 0.3f, 100f }, new float[] { -0.25f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 80f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 1.15f, 0.3f, 0.3f, 0f }, new float[] { 0.1f, 1.15f, 0.3f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.05f, -0.55f, 0.45f, 0.3f, 60f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.2f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, -0.55f, 0.65f, 0.3f, 80f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 1.15f, 0.55f, 0.3f, 0f }, new float[] { 0.5f, 1.15f, 0.55f, 0.3f, 0f }, new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.1f, -1.15f, 0.45f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.5f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0.1f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 240f }, new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 300f }, new float[] { -0.4f, -0.6f, 0.85f, 0.3f, 120f }, new float[] { 0.4f, -0.6f, 0.85f, 0.3f, 60f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 240f }, new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 300f }, new float[] { -0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.3f, 0.4f, 0.65f, 0.3f, 120f }, new float[] { -0.3f, -0.4f, 0.65f, 0.3f, 120f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, -1.15f, 0.8f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 0f, 0.9f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 0f, 0.45f, 0.3f, 0f }, new float[] { 0.45f, 0f, 0.45f, 0.3f, 0f }, new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.19f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0.2f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.56f, 0.25f, 1.1f, 0.3f, 90f }, new float[] { 0f, -1.0f, 0.3f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.8f, 0f, 1.75f, 0.3f, 120f }, new float[] { 0f, 0f, 0f, 0f, 99999f } } };

}

public static class LetterDirection
{
    public const int TO_RIGHT = 0;
    public const int TO_DOWN = 1;
    public const int TO_LEFT = 2;
    public const int TO_UP = 3;
}

public static class LetterShape
{
    public const int NORMAL = 0;
    public const int POLYGON = 1;
    public const int LINE = 2;
}
