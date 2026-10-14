// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Shape
{
    static int nextId;
    public int Id;
    public List<float> Geometry = new List<float>();
    public const int MaxInstanceCount = 72;
    public const int MaxInstanceBytes = 4800;
    private static Vector4[] params0 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    private static Vector4[] params1 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    private static Vector4[] params2 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    private static int instanceCount;
    private GtgFrame frame;
    public static void Initialize(GtgFrame frame)
    {
    }

    public static void BeginAddInstance()
    {
        instanceCount = 0;
    }

    public static void AddInstance(Vector3 pos, float scale, Quaternion quaternion, Vector4 color)
    {
        params0[(instanceCount)].X = pos.X;
        params0[(instanceCount)].Y = pos.Y;
        params0[(instanceCount)].Z = pos.Z;
        params0[(instanceCount)].W = scale;
        params1[(instanceCount)].X = quaternion.X;
        params1[(instanceCount)].Y = quaternion.Y;
        params1[(instanceCount)].Z = quaternion.Z;
        params1[(instanceCount)].W = quaternion.W;
        params2[(instanceCount)].X = color.X;
        params2[(instanceCount)].Y = color.Y;
        params2[(instanceCount)].Z = color.Z;
        params2[(instanceCount)].W = color.W;
        instanceCount++;
    }

    public VertexPositionColor[] verts;
    public int vertCount;
    public int index = 0;
    public int[] indices;
    public int primitiveCount;
    public Shape(GtgFrame frame, int vc, int pc)
    {
        this.frame = frame;
        Id = nextId;
        nextId++;
        verts = GtgArrays.Make(vc, () => new VertexPositionColor());
        vertCount = vc;
        for (int i = 0; i < vc; i++)
            verts[(i)] = new VertexPositionColor((Vector3.Zero).Copy(), new Color(255, 255, 255, 255));
        primitiveCount = pc;
    }

    public void CreateIndexBuffer()
    {
    }

    public void SetIndexedPrimitives()
    {
    }

    public void Add(float x, float y, float z, int r, int g, int b, int a)
    {
        verts[(index)].Position.X = x;
        verts[(index)].Position.Y = y;
        verts[(index)].Position.Z = z;
        verts[(index)].Color = new Color(r, g, b, a);
        index++;
    }

    public void EndAdd()
    {
    }

    public void Draw()
    {
        if (instanceCount > 0)
            GtgRender.Submit(this, frame, verts, indices, vertCount, primitiveCount, params0, params1, params2, instanceCount);
    }
}

public static class TriangleListShape
{
    public static Shape Create(GtgFrame frame, int tc)
    {
        var s = new Shape(frame, tc * 3, tc);
        s.indices = GtgArrays.Make(s.vertCount * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < s.vertCount * Shape.MaxInstanceCount; i++)
            s.indices[(i)] = (int)i;
        s.CreateIndexBuffer();

        return s;
    }
}

public static class QuadListShape
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
    public static Shape Create(GtgFrame frame, int qc)
    {
        var s = new Shape(frame, qc * 4, qc * 2);
        s.indices = GtgArrays.Make(qc * 6 * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < qc * Shape.MaxInstanceCount; i++)
            for (int j = 0; j < 6; j++)
                s.indices[(i * 6 + j)] = (int)(i * 4 + quadIndices[(j)]);
        s.CreateIndexBuffer();

        return s;
    }
}

public static class CubeShape
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
    private static int[] cubeIndices = new int[]
    {
        0,
        1,
        2,
        3,
        1,
        0,
        4,
        5,
        2,
        1,
        5,
        6,
        3,
        2,
        6,
        7,
        0,
        3,
        7,
        4,
        7,
        6,
        5,
        4
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
        var s = new Shape(frame, 8, 12);
        for (int i = 0; i < 8; i++)
        {
            s.Add(cubeOffsets[(i)][(0)] * x, cubeOffsets[(i)][(1)] * y, cubeOffsets[(i)][(2)] * z, 0, 0, 0, 0);
        }

        s.EndAdd();
        s.indices = GtgArrays.Make(36 * Shape.MaxInstanceCount, () => 0);
        for (int i = 0; i < Shape.MaxInstanceCount; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 6; k++)
                    s.indices[(i * 36 + j * 6 + k)] = (int)(i * 8 + cubeIndices[(quadIndices[(k)] + j * 4)]);
            }
        }

        s.CreateIndexBuffer();

        return s;
    }
}
