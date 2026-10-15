// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public static class GearShape
{
    private static int[] quadIndices = new int[]
    {
        0,
        1,
        2,
        1,
        2,
        9,
        7,
        2,
        9,
        0,
        1,
        3,
        1,
        3,
        4,
        1,
        9,
        4,
        9,
        4,
        12,
        2,
        7,
        5,
        5,
        7,
        10,
        2,
        9,
        5,
        5,
        9,
        12
    };
    public static Shape Create(GtgFrame frame, float radius, float innerRadius, float thickness, Color frontColor, Color backColor, bool hasCenter)
    {
        var s = new Shape(frame, 56, 0);
        float a = 0;
        for (int i = 0; i < 8; i++)
        {
            s.Add((float)Math.Sin(a - 0.1f) * radius, (float)Math.Cos(a - 0.1f) * radius, thickness, frontColor.R, frontColor.G, frontColor.B, frontColor.A);
            s.Add((float)Math.Sin(a + 0.1f) * radius, (float)Math.Cos(a + 0.1f) * radius, thickness, frontColor.R, frontColor.G, frontColor.B, frontColor.A);
            s.Add((float)Math.Sin(a) * innerRadius, (float)Math.Cos(a) * innerRadius, thickness, frontColor.R, frontColor.G, frontColor.B, frontColor.A);
            s.Add((float)Math.Sin(a - 0.1f) * radius, (float)Math.Cos(a - 0.1f) * radius, -thickness, backColor.R, backColor.G, backColor.B, backColor.A);
            s.Add((float)Math.Sin(a + 0.1f) * radius, (float)Math.Cos(a + 0.1f) * radius, -thickness, backColor.R, backColor.G, backColor.B, backColor.A);
            s.Add((float)Math.Sin(a) * innerRadius, (float)Math.Cos(a) * innerRadius, -thickness, backColor.R, backColor.G, backColor.B, backColor.A);
            s.Add((float)Math.Sin(a) * innerRadius, (float)Math.Cos(a) * innerRadius, 0, 0, 0, 0, 0);
            a += (float)Math.PI / 4;
        }

        s.EndAdd();
        int indexCount;
        if (hasCenter)
            indexCount = 282;
        else
            indexCount = 264;
        s.indices = GtgArrays.Make(indexCount * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 33; k++)
                {
                    int idx = quadIndices[(k)] + j * 7;
                    if (idx >= 56)
                        idx -= 56;
                    s.indices[(i * indexCount + j * 33 + k)] = (int)(i * 56 + idx);
                }
            }

            if (hasCenter)
            {
                for (int j = 0; j < 6; j++)
                {
                    s.indices[(i * indexCount + 264 + j * 3)] = (int)(i * 56 + 6);
                    s.indices[(i * indexCount + 264 + j * 3 + 1)] = (int)(i * 56 + 7 + j * 7);
                    s.indices[(i * indexCount + 264 + j * 3 + 2)] = (int)(i * 56 + 14 + j * 7);
                }
            }
        }

        s.primitiveCount = indexCount / 3;
        s.CreateIndexBuffer();

        return s;
    }
}

public static class GearEdgeShape
{
    private static int[] quadIndices = new int[]
    {
        0,
        1,
        3,
        0,
        2,
        3
    };
    public static Shape Create(GtgFrame frame, float radius, float innerRadius, float thickness, float edgeThickness)
    {
        var s = new Shape(frame, 48, 48);
        float a = 0;
        float ir = (radius + innerRadius) * 0.5f;
        for (int i = 0; i < 8; i++)
        {
            s.Add((float)Math.Sin(a - 0.1f) * radius, (float)Math.Cos(a - 0.1f) * radius, thickness, 250, 250, 250, 240);
            s.Add((float)Math.Sin(a - 0.1f) * (radius - edgeThickness), (float)Math.Cos(a - 0.1f) * (radius - edgeThickness), thickness, 250, 250, 250, 180);
            s.Add((float)Math.Sin(a + 0.1f) * radius, (float)Math.Cos(a + 0.1f) * radius, thickness, 250, 250, 250, 240);
            s.Add((float)Math.Sin(a + 0.1f) * (radius - edgeThickness), (float)Math.Cos(a + 0.1f) * (radius - edgeThickness), thickness, 250, 250, 250, 180);
            s.Add((float)Math.Sin(a + (float)Math.PI / 8) * ir, (float)Math.Cos(a + (float)Math.PI / 8) * ir, thickness, 250, 250, 250, 240);
            s.Add((float)Math.Sin(a + (float)Math.PI / 8) * (ir - edgeThickness), (float)Math.Cos(a + (float)Math.PI / 8) * (ir - edgeThickness), thickness, 250, 250, 250, 240);
            a += (float)Math.PI / 4;
        }

        s.EndAdd();
        int indexCount = 144;
        s.indices = GtgArrays.Make(indexCount * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < 24; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    int idx = j * 2 + quadIndices[(k)];
                    if (idx >= 48)
                        idx -= 48;
                    s.indices[(i * indexCount + j * 6 + k)] = (int)(i * 48 + idx);
                }
            }
        }

        s.CreateIndexBuffer();

        return s;
    }
}

public static class EllipseShape
{
    public static Shape Create(GtgFrame frame, float radius, float ratio)
    {
        var s = new Shape(frame, 16, 14);
        float a = 0;
        for (int i = 0; i < 16; i++)
        {
            s.Add((float)Math.Sin(a) * radius * ratio, (float)Math.Cos(a) * radius, 0, 0, 0, 0, 0);
            a += (float)Math.PI / 8;
        }

        s.EndAdd();
        int indexCount = 42;
        s.indices = GtgArrays.Make(indexCount * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < 14; j++)
            {
                s.indices[(i * indexCount + j * 3)] = (int)(i * 16);
                s.indices[(i * indexCount + j * 3 + 1)] = (int)(i * 16 + 1 + j);
                s.indices[(i * indexCount + j * 3 + 2)] = (int)(i * 16 + 2 + j);
            }
        }

        s.CreateIndexBuffer();

        return s;
    }
}

public static class EllipseEdgeShape
{
    private static int[] quadIndices = new int[]
    {
        0,
        1,
        3,
        0,
        2,
        3
    };
    public static Shape Create(GtgFrame frame, float radius, float ratio, float innerRadius, int vc)
    {
        var s = new Shape(frame, vc * 2, vc * 2);
        float a = 0;
        for (int i = 0; i < vc; i++)
        {
            s.Add((float)Math.Sin(a) * radius * ratio, (float)Math.Cos(a) * radius, 0, 250, 250, 250, 240);
            s.Add((float)Math.Sin(a) * innerRadius * ratio, (float)Math.Cos(a) * innerRadius, 0, 250, 250, 250, 180);
            a += (float)Math.PI * 2 / vc;
        }

        s.EndAdd();
        int indexCount = vc * 6;
        s.indices = GtgArrays.Make(indexCount * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < vc; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    int idx = j * 2 + quadIndices[(k)];
                    if (idx >= vc * 2)
                        idx -= vc * 2;
                    s.indices[(i * indexCount + j * 6 + k)] = (int)(i * vc * 2 + idx);
                }
            }
        }

        s.CreateIndexBuffer();

        return s;
    }
}

public static class CubeEdgeShape
{
    private static int[] quadIndices = new int[]
    {
        0,
        1,
        3,
        1,
        2,
        3
    };
    private static int[] xyEdgeIndices = new int[]
    {
        1,
        2,
        5,
        3
    };
    private static int[] zEdgeIndices = new int[]
    {
        0,
        1,
        13,
        12
    };
    private static float[][] cubeOffsets = new float[][]
    {
        new float[]
        {
            -1,
            -1,
            -1
        },
        new float[]
        {
            1,
            -1,
            -1
        },
        new float[]
        {
            1,
            1,
            -1
        },
        new float[]
        {
            -1,
            1,
            -1
        },
        new float[]
        {
            -1,
            -1,
            1
        },
        new float[]
        {
            1,
            -1,
            1
        },
        new float[]
        {
            1,
            1,
            1
        },
        new float[]
        {
            -1,
            1,
            1
        }
    };
    public static Shape Create(GtgFrame frame, float x, float y, float z)
    {
        var s = new Shape(frame, 24, 24);
        for (int i = 0; i < 8; i++)
        {
            s.Add(cubeOffsets[(i)][(0)] * x * 1.1f, cubeOffsets[(i)][(1)] * y * 0.9f, cubeOffsets[(i)][(2)] * z * 0.9f, 250, 250, 250, 240);
            s.Add(cubeOffsets[(i)][(0)] * x * 0.9f, cubeOffsets[(i)][(1)] * y * 1.1f, cubeOffsets[(i)][(2)] * z * 0.9f, 250, 250, 250, 240);
            s.Add(cubeOffsets[(i)][(0)] * x * 0.9f, cubeOffsets[(i)][(1)] * y * 0.9f, cubeOffsets[(i)][(2)] * z * 1.1f, 250, 250, 250, 240);
        }

        s.EndAdd();
        s.indices = GtgArrays.Make(24 * 3 * Shape.MaxInstanceCount, () => 0);
        int[] edgeIndices = GtgArrays.Make(4, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (j < 8)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        int idx = (int)(xyEdgeIndices[(k)] + (j % 4) * 3);
                        if (idx >= 12)
                            idx -= 12;
                        if (j >= 4)
                            idx += 12;
                        edgeIndices[(k)] = idx;
                    }
                }
                else
                {
                    for (int k = 0; k < 4; k++)
                        edgeIndices[(k)] = (int)(zEdgeIndices[(k)] + (j - 8) * 3);
                }

                for (int k = 0; k < 6; k++)
                    s.indices[(i * 24 * 3 + j * 6 + k)] = (int)(i * 24 + edgeIndices[(quadIndices[(k)])]);
            }
        }

        s.CreateIndexBuffer();

        return s;
    }
}
