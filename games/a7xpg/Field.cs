// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class Field
{
    public Vector size;
    public float eyeZ;
    public float eyeZa;
    public float alpha;
    public const float HEIGHT = 1;
    public const float HEIGHT_OFFSET = 8;
    public float z;
    public float r, g, b;
    public float lr, lg, lb;
    public void init()
    {
        size = new Vector();
        eyeZ = 0;
        alpha = 1;
    }

    public static float[][] COLOR = new float[][]
    {
        new float[] { 0.4f, 0.8f, 1 },
        new float[] { 0.4f, 1, 0.8f },
        new float[] { 1, 0.8f, 0.4f }
    };
    public static float[][] LUMINOUS_COLOR = new float[][]
    {
        new float[] { 0.2f, 0.2f, 1 },
        new float[] { 0.2f, 0.6f, 0.7f },
        new float[] { 0.6f, 0.2f, 0.7f }
    };
    public void start(int colorType)
    {
        if (size.x > size.y)
            eyeZa = size.x * 1.3f;
        else
            eyeZa = size.y * 1.3f / 480 * 640;
        z = 0;
        r = COLOR[colorType % 3][0];
        g = COLOR[colorType % 3][1];
        b = COLOR[colorType % 3][2];
        lr = LUMINOUS_COLOR[colorType % 3][0];
        lg = LUMINOUS_COLOR[colorType % 3][1];
        lb = LUMINOUS_COLOR[colorType % 3][2];
    }

    public void addSpeed(float s)
    {
        z = z - (s);
        if (z < 0)
            z = z + (HEIGHT_OFFSET);
    }

    public void move()
    {
        eyeZ = eyeZ + ((eyeZa - eyeZ) * 0.06f);
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend)
    {
        var mesh = new Mesh("Field-draw");
        int part1 = mesh.vertexCount;
        tint = new float[] { r, g, b, 0.4f };
        mesh.Vertex(-size.x, -size.y, 0, tint);
        tint = new float[] { r, g, b, 0.8f };
        mesh.Vertex(-size.x, -size.y, HEIGHT, tint);
        tint = new float[] { r, g, b, 0.4f };
        mesh.Vertex(size.x, -size.y, 0, tint);
        tint = new float[] { r, g, b, 0.8f };
        mesh.Vertex(size.x, -size.y, HEIGHT, tint);
        tint = new float[] { r, g, b, 0.4f };
        mesh.Vertex(size.x, size.y, 0, tint);
        tint = new float[] { r, g, b, 0.8f };
        mesh.Vertex(size.x, size.y, HEIGHT, tint);
        tint = new float[] { r, g, b, 0.4f };
        mesh.Vertex(-size.x, size.y, 0, tint);
        tint = new float[] { r, g, b, 0.8f };
        mesh.Vertex(-size.x, size.y, HEIGHT, tint);
        tint = new float[] { r, g, b, 0.4f };
        mesh.Vertex(-size.x, -size.y, 0, tint);
        tint = new float[] { r, g, b, 0.8f };
        mesh.Vertex(-size.x, -size.y, HEIGHT, tint);
        for (int vi = part1; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part1) % 2), vi + 1 - ((vi - part1) % 2), vi + 2);

        if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public void drawLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        var mesh = new Mesh("Field-drawLuminous");
        tint = new float[] { lr, lg, lb, 0.9f * alpha };
        int part1 = mesh.vertexCount;
        mesh.Vertex(-size.x, -size.y, HEIGHT, tint);
        mesh.Vertex(size.x, -size.y, HEIGHT, tint);
        mesh.Vertex(size.x, size.y, HEIGHT, tint);
        mesh.Vertex(-size.x, size.y, HEIGHT, tint);
        mesh.Vertex(-size.x, -size.y, HEIGHT, tint);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
        float hz = HEIGHT_OFFSET - z;
        for (int i = 0; i < 8; i++)
        {
            tint = new float[] { lr, lg, lb, (0.8f - i * 0.05f) * alpha };
            int part2 = mesh.vertexCount;
            mesh.Vertex(-size.x, -size.y, hz, tint);
            mesh.Vertex(size.x, -size.y, hz, tint);
            mesh.Vertex(size.x, size.y, hz, tint);
            mesh.Vertex(-size.x, size.y, hz, tint);
            mesh.Vertex(-size.x, -size.y, hz, tint);
            mesh.LineStrip(part2, mesh.vertexCount - part2);
            hz = hz - (HEIGHT_OFFSET);
        }

        if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
}
