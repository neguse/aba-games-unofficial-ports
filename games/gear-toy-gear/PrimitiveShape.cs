// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public abstract class Shape
{
    static int nextId;
    public int Id;
    public List<float> Geometry = new List<float>();
    public const int MaxInstanceCount = 72;
    public const int MaxInstanceBytes = 4800;
    protected static Vector4[] params0 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    protected static Vector4[] params1 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    protected static Vector4[] params2 = GtgArrays.Make(MaxInstanceCount, () => new Vector4());
    protected static int instanceCount;
    protected GtgFrame frame;
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

    public Shape(GtgFrame frame)
    {
        this.frame = frame;
        Id = nextId;
        nextId++;
    }

    public abstract void Draw();
}

public abstract class PrimitiveShape : Shape
{
    protected VertexPositionColor[] verts;
    protected int vertCount;
    public PrimitiveShape(GtgFrame frame, int vc) : base(frame)
    {
        verts = GtgArrays.Make(vc, () => new VertexPositionColor());
        vertCount = vc;
        for (int i = 0; i < vc; i++)
            verts[(i)] = new VertexPositionColor((Vector3.Zero).Copy(), new Color(255, 255, 255, 255));
    }
}

public abstract class PrimitiveListShape : PrimitiveShape
{
    protected int index = 0;
    protected int[] indices;
    protected int primitiveCount;
    public PrimitiveListShape(GtgFrame frame, int vc, int pc = 0) : base(frame, vc)
    {
        primitiveCount = pc;
    }

    protected void CreateIndexBuffer()
    {
    }

    protected void SetIndexedPrimitives()
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

    public override void Draw()
    {
        if (instanceCount > 0)
            GtgRender.Submit(this, frame, verts, indices, vertCount, primitiveCount, params0, params1, params2, instanceCount);
    }
}

public class TriangleListShape : PrimitiveListShape
{
    public TriangleListShape(GtgFrame frame, int tc) : base(frame, tc * 3, tc)
    {
        indices = GtgArrays.Make(vertCount * MaxInstanceCount, () => 0);
        for (int i = 0; i < vertCount * MaxInstanceCount; i++)
            indices[(i)] = (int)i;
        CreateIndexBuffer();
    }
}

public class QuadListShape : PrimitiveListShape
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
    public QuadListShape(GtgFrame frame, int qc) : base(frame, qc * 4, qc * 2)
    {
        indices = GtgArrays.Make(qc * 6 * MaxInstanceCount, () => 0);
        for (int i = 0; i < qc * MaxInstanceCount; i++)
            for (int j = 0; j < 6; j++)
                indices[(i * 6 + j)] = (int)(i * 4 + quadIndices[(j)]);
        CreateIndexBuffer();
    }
}

public class CubeShape : PrimitiveListShape
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
    public CubeShape(GtgFrame frame, float x, float y, float z) : base(frame, 8, 12)
    {
        for (int i = 0; i < 8; i++)
        {
            Add(cubeOffsets[(i)][(0)] * x, cubeOffsets[(i)][(1)] * y, cubeOffsets[(i)][(2)] * z, 0, 0, 0, 0);
        }

        EndAdd();
        indices = GtgArrays.Make(36 * MaxInstanceCount, () => 0);
        for (int i = 0; i < MaxInstanceCount; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 6; k++)
                    indices[(i * 36 + j * 6 + k)] = (int)(i * 8 + cubeIndices[(quadIndices[(k)] + j * 4)]);
            }
        }

        CreateIndexBuffer();
    }
}
