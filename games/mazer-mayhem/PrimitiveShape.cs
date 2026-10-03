// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public abstract class Shape
{
    static int nextId;
    public int id, meshVersion = -1;
    public List<float> data = new List<float>();
    protected MmFrame frame;
    private Matrix m = new Matrix();
    public Shape(MmFrame frame)
    {
        id = nextId;
        nextId++;
        this.frame = frame;
    }

    public abstract void Draw();
    public void DrawVector3(Vector3 pos)
    {
        m = Matrix.CreateTranslationVector3(pos);
        frame.WorldMatrix = m;
        Draw();
    }

    public void DrawVector3Quaternion(Vector3 pos, Quaternion dir)
    {
        m = Matrix.CreateFromQuaternion(dir);
        m *= Matrix.CreateTranslationVector3(pos);
        frame.WorldMatrix = m;
        Draw();
    }

    public void DrawVector3float(Vector3 pos, float size)
    {
        m = Matrix.CreateScalefloat(size);
        m *= Matrix.CreateTranslationVector3(pos);
        frame.WorldMatrix = m;
        Draw();
    }

    public void DrawVector3Quaternionfloat(Vector3 pos, Quaternion dir, float size)
    {
        m = Matrix.CreateScalefloat(size);
        m *= Matrix.CreateFromQuaternion(dir);
        m *= Matrix.CreateTranslationVector3(pos);
        frame.WorldMatrix = m;
        Draw();
    }

    public void DrawVector3Quaternionfloatfloatfloat(Vector3 pos, Quaternion dir, float xs, float ys, float zs)
    {
        m = Matrix.CreateScalefloatfloatfloat(xs, ys, zs);
        m *= Matrix.CreateFromQuaternion(dir);
        m *= Matrix.CreateTranslationVector3(pos);
        frame.WorldMatrix = m;
        Draw();
    }
}

public abstract class PrimitiveShape : Shape
{
    public VertexPositionColor[] Verts;
    public PrimitiveShape(MmFrame frame) : base(frame)
    {
    }

    public void Initializeint(int n)
    {
        Initializeintbytebytebytebyte(n, 255, 255, 255, 255);
    }

    public virtual void Initializeintbytebytebytebyte(int n, int r, int g, int b, int a)
    {
        Verts = MmArrays.Make(n, () => new VertexPositionColor());
        for (int i = 0; i < n; i++)
            Verts[(i)] = new VertexPositionColor((Vector3.Zero).Copy(), new Color(r, g, b, a));
    }
}

public abstract class PrimitiveListShape : PrimitiveShape
{
    protected int index;
    public int version;
    protected int[] indices;
    public PrimitiveListShape(MmFrame frame) : base(frame)
    {
    }

    public override void Initializeintbytebytebytebyte(int n, int r, int g, int b, int a)
    {
        base.Initializeintbytebytebytebyte(n, r, g, b, a);
        BeginAdd();
    }

    public void Clear()
    {
        index = 0;
    }

    public void BeginAdd()
    {
        index = 0;
    }

    public void AddVector3bytebytebytebyte(Vector3 p, int r, int g, int b, int a)
    {
        Verts[(index)].Position.X = p.X;
        Verts[(index)].Position.Y = p.Y;
        Verts[(index)].Position.Z = p.Z;
        Verts[(index)].Color.R = r;
        Verts[(index)].Color.G = g;
        Verts[(index)].Color.B = b;
        Verts[(index)].Color.A = a;
        index++;
    }

    public void Addfloatfloatfloat(float x, float y, float z)
    {
        Verts[(index)].Position.X = x;
        Verts[(index)].Position.Y = y;
        Verts[(index)].Position.Z = z;
        index++;
    }

    public void Addfloatfloatfloatfloat(float x, float y, float z, float a)
    {
        Verts[(index)].Position.X = x;
        Verts[(index)].Position.Y = y;
        Verts[(index)].Position.Z = z;
        Verts[(index)].Color = new Color(Verts[(index)].Color.R, Verts[(index)].Color.G, Verts[(index)].Color.B, (int)(a * 255));
        index++;
    }

    public void EndAdd()
    {
        if (index <= 0)
            return;
        version++;
    }
}

public class TriangleListShape : PrimitiveListShape
{
    public TriangleListShape(MmFrame frame) : base(frame)
    {
    }

    public override void Initializeintbytebytebytebyte(int n, int r, int g, int b, int a)
    {
        indices = MmArrays.Make(n * 3, () => 0);
        for (int i = 0; i < n * 3; i++)
            indices[(i)] = (int)i;
        base.Initializeintbytebytebytebyte(n * 3, r, g, b, a);
    }

    public override void Draw()
    {
        if (index <= 0)
            return;
        MmRender.DrawShapeMmFrameVertexPositionColorArrayintArrayintint(this, frame, Verts, indices, index, version);
    }
}

public class QuadListShape : PrimitiveListShape
{
    private static int[] QuadIndices = new int[]
    {
        0,
        1,
        3,
        1,
        2,
        3
    };
    public QuadListShape(MmFrame frame) : base(frame)
    {
    }

    public override void Initializeintbytebytebytebyte(int n, int r, int g, int b, int a)
    {
        indices = MmArrays.Make(n * 6, () => 0);
        for (int i = 0; i < n; i++)
            for (int j = 0; j < 6; j++)
                indices[(i * 6 + j)] = (int)(i * 4 + QuadIndices[(j)]);
        base.Initializeintbytebytebytebyte(n * 4, r, g, b, a);
    }

    public override void Draw()
    {
        if (index <= 0)
            return;
        MmRender.DrawShapeMmFrameVertexPositionColorArrayintArrayintint(this, frame, Verts, indices, index / 4 * 6, version);
    }
}
