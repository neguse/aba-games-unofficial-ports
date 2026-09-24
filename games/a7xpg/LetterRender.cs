// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class LetterRender
{
    public static Mesh[] meshes;
    public static void appendBoxSolid(Mesh mesh, float[] model, float[] tint, float x, float y, float width, float height)
    {
        int part1 = mesh.vertexCount;
        mesh.Vertex(x, y, 0, tint, model);
        mesh.Vertex(x + width, y, 0, tint, model);
        mesh.Vertex(x + width, y + height, 0, tint, model);
        mesh.Vertex(x, y + height, 0, tint, model);
        mesh.Quads(part1, mesh.vertexCount - part1);
    }

    public static void appendBoxLine(Mesh mesh, float[] model, float[] tint, float x, float y, float width, float height)
    {
        int part1 = mesh.vertexCount;
        mesh.Vertex(x, y, 0, tint, model);
        mesh.Vertex(x + width, y, 0, tint, model);
        mesh.Vertex(x + width, y + height, 0, tint, model);
        mesh.Vertex(x, y + height, 0, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true);
    }

    public static void drawLetter(float[] model, float[] tint, Gfx.Blend blend, int n, float x, float y, float s)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, s, s);
        { Mesh shape2 = meshes[n]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public static void drawLetterReverse(float[] model, float[] tint, Gfx.Blend blend, int n, float x, float y, float s)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Scale(model, s, -s, s);
        { Mesh shape2 = meshes[n]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public static void drawString(float[] model, float[] tint, Gfx.Blend blend, string str, float lx, float y, float s)
    {
        float x = lx;
        for (int i = 0; i < str.Length; i++)
        {
            string c = str.Substring(i, 1);
            if (c != " ")
            {
                int idx = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+".IndexOf(c);
                if (idx < 0)
                {
                    idx = "abcdefghijklmnopqrstuvwxyz".IndexOf(c);
                    idx = idx < 0 ? 37 : idx + 10;
                }

                drawLetter(model, tint, blend, idx, x, y, s);
            }

            x = x + (s * 1.7f);
        }
    }

    public static void drawNum(float[] model, float[] tint, Gfx.Blend blend, int num, float lx, float y, float s)
    {
        int n = num;
        float x = lx;
        for (;;)
        {
            drawLetter(model, tint, blend, n % 10, x, y, s);
            x = x - (s * 1.7f);
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }
    }

    public static void drawNumReverse(float[] model, float[] tint, Gfx.Blend blend, int num, float lx, float y, float s)
    {
        int n = num;
        float x = lx;
        for (;;)
        {
            drawLetterReverse(model, tint, blend, n % 10, x, y, s);
            x = x - (s * 1.7f);
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }
    }

    public static void drawTime(float[] model, float[] tint, Gfx.Blend blend, int time, float lx, float y, float s)
    {
        int n = time;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter(model, tint, blend, n % 10, x, y, s);
                n = GameMath.integer(n / (10));
            }
            else
            {
                drawLetter(model, tint, blend, n % 6, x, y, s);
                n = GameMath.integer(n / (6));
            }

            if ((i & 1) == 1 || i == 0)
            {
                switch (i)
                {
                    case 3:
                        drawLetter(model, tint, blend, 41, x + s * 1.16f, y, s);
                        break;
                    case 5:
                        drawLetter(model, tint, blend, 40, x + s * 1.16f, y, s);
                        break;
                    default:
                        break;
                }

                x = x - (s * 1.7f);
            }
            else
            {
                x = x - (s * 2.2f);
            }

            if (n <= 0)
                break;
        }
    }

    public static void appendBox(Mesh mesh, float[] model, float[] tint, float x, float y, float width, float height)
    {
        tint = new float[] { 1, 1, 1, 0.5f };
        appendBoxSolid(mesh, model, tint, x - width, y - height, width * 2, height * 2);
        tint = new float[] { 1, 1, 1, 1 };
        appendBoxLine(mesh, model, tint, x - width, y - height, width * 2, height * 2);
    }

    public static void appendLetter(Mesh mesh, float[] model, float[] tint, int idx)
    {
        int i = 0;
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        int deg = 0;
        for (i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            size = size * (0.66f);
            length = length * (0.6f);
            x = -x;
            y = y;
            deg = deg % (180);
            if (deg <= 45 || deg > 135)
                appendBox(mesh, model, tint, x, y, size, length);
            else
                appendBox(mesh, model, tint, x, y, length, size);
        }
    }

    public static void createMeshes()
    {
        meshes = new Mesh[42]; Mesh mesh = null; float[] model = Transform.Identity(); float[] tint = null;
        for (int i = 0; i < 42; i++)
        {
            mesh = new Mesh("LetterRender-" + (i).ToString()); meshes[i] = mesh;
            appendLetter(mesh, model, tint, i);

        }
    }

    public static void deleteMeshes() { meshes = null; }

    public static float[][][] spData = new float[][][]
    {
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.6f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.6f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.6f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.1f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.1f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.4f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.25f, 0, 0.25f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.75f, 0.25f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.3f, 1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0.3f, 1.15f, 0.25f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.2f, -0.6f, 0.45f, 0.3f, 360 - 300 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0.4f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.5f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.3f, -1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0.3f, -1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 360 - 120 },
            new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 360 - 60 },
            new float[] { -0.4f, -0.6f, 0.85f, 0.3f, 360 - 240 },
            new float[] { 0.4f, -0.6f, 0.85f, 0.3f, 360 - 300 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 360 - 120 },
            new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 360 - 60 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.35f, 0.5f, 0.65f, 0.3f, 360 - 60 },
            new float[] { -0.35f, -0.5f, 0.65f, 0.3f, 360 - 240 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.05f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0.4f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.19f, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0.2f, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        }
    };
}
