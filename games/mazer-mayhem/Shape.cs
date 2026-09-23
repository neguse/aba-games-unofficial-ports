// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class BlurNormalTextureCubeListShape : Shape
{
    public VertexPositionNormalTexture[] Verts;
    private int index;
    public int version;
    private int primitiveCnt;
    private int[] indices;
    private Quaternion dir = new Quaternion();
    public BlurNormalTextureCubeListShape(MmFrame frame) : base(frame)
    {
    }

    public void BeginAdd(int nn)
    {
        int n = nn * 24;
        Verts = MmArrays.Make(n, () => new VertexPositionNormalTexture());
        for (int i = 0; i < n; i++)
            Verts[(i)] = new VertexPositionNormalTexture((Vector3.Zero).Copy(), (Vector3.Up).Copy(), (Vector2.Zero).Copy());
        indices = MmArrays.Make(n * 180, () => 0);
        Clear();
    }

    public void Clear()
    {
        index = 0;
        primitiveCnt = 0;
    }

    static int[] quadIndices = new int[]
    {
        0,
        1,
        3,
        1,
        2,
        3
    };
    static int[] cubeIndices = new int[]
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
    static float[][] cubeOffsets = new float[][]
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
    public void AddVector3Vector3float(Vector3 p, Vector3 cp, float size)
    {
        AddVector3Vector3floatQuaternion((p).Copy(), (cp).Copy(), size, (Quaternion.Identity).Copy());
    }

    public void AddVector3Vector3floatQuaternion(Vector3 p, Vector3 cp, float size, Quaternion dir)
    {
        for (int i = 0; i < 8; i++)
            AddVertex((p).Copy(), (cp).Copy(), size * cubeOffsets[(i)][(0)], size * cubeOffsets[(i)][(1)], size * cubeOffsets[(i)][(2)], (dir).Copy());
        int ci = (int)(index - 24);
        int ii = GameMath.integer((ci / 24)) * 180;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                indices[(ii)] = (int)(ci + cubeIndices[(i * 4 + quadIndices[(j)])] * 3);
                ii++;
            }

            primitiveCnt += 2;
            for (int k = 0; k < 4; k++)
            {
                int[] sideCubeIndices = new int[]
                {
                    cubeIndices[(i * 4 + k)] * 3 + 1,
                    cubeIndices[(i * 4 + (k + 1) % 4)] * 3 + 1,
                    cubeIndices[(i * 4 + (k + 1) % 4)] * 3 + 2,
                    cubeIndices[(i * 4 + k)] * 3 + 2
                };
                for (int j = 0; j < 6; j++)
                {
                    indices[(ii)] = (int)(ci + sideCubeIndices[(quadIndices[(j)])]);
                    ii++;
                }

                primitiveCnt += 2;
            }
        }
    }

    private void AddVertex(Vector3 p, Vector3 cp, float ox, float oy, float oz, Quaternion dir)
    {
        Vector3 o = new Vector3(ox, oy, oz);
        Vector3 n = (cp - p).Copy();
        o = (Vector3.TransformVector3Quaternion((o).Copy(), (dir).Copy())).Copy();
        p += o;
        Verts[(index)].Position = (p).Copy();
        Verts[(index)].Normal = (n).Copy();
        Verts[(index)].TextureCoordinate = new Vector2(0, 0);
        index++;
        Verts[(index)].Position = (p).Copy();
        Verts[(index)].Normal = (n).Copy();
        Verts[(index)].TextureCoordinate = new Vector2(0, 1);
        index++;
        Verts[(index)].Position = (p).Copy();
        Verts[(index)].Normal = (n).Copy();
        Verts[(index)].TextureCoordinate = new Vector2(1, 1);
        index++;
    }

    public void EndAdd()
    {
        version++;
    }

    public override void Draw()
    {
        MmRender.DrawShapeMmFrameVertexPositionNormalTextureArrayintArrayintint(this, frame, Verts, indices, primitiveCnt * 3, version);
    }

    public void DrawDeg(Vector3 pos, float d)
    {
        dir = (Quaternion.CreateFromYawPitchRoll(0, 0, -d)).Copy();
        DrawVector3Quaternion((pos).Copy(), (dir).Copy());
    }
}

public class BallShape
{
    public const int DotNum = 18;
    private static Vector3[] dotPoss = MmArrays.Make(DotNum, () => new Vector3());
    private static int dotIdx;
    public static void Init()
    {
        dotIdx = 0;
        AddDotPos((Quaternion.Identity).Copy());
        for (int i = 0; i < 4; i++)
            AddDotPos((Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 2, (float)Math.PI / 4, 0)).Copy());
        for (int i = 0; i < 8; i++)
            AddDotPos((Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 4, (float)Math.PI / 2, 0)).Copy());
        for (int i = 0; i < 4; i++)
            AddDotPos((Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 2, (float)Math.PI / 4 * 3, 0)).Copy());
        AddDotPos((Quaternion.CreateFromYawPitchRoll(0, (float)Math.PI, 0)).Copy());
    }

    private static void AddDotPos(Quaternion dir)
    {
        Vector3 r = new Vector3(0, 0.8f, 0);
        dotPoss[(dotIdx)] = (Vector3.TransformVector3Quaternion((r).Copy(), (dir).Copy())).Copy();
        dotIdx++;
    }

    public static void AddShadow(MmFrame frame, QuadListShape shape, Vector3 p, Quaternion dir, float size)
    {
        Vector3 p3 = new Vector3();
        Vector2 p2 = new Vector2();
        Vector4 lv = (frame.LightVector).Copy();
        float lr = p.Z / lv.Z;
        float wr = 1 + lr * 0.1f;
        for (int i = 0; i < DotNum; i++)
        {
            p3 = (Vector3.TransformVector3Quaternion((dotPoss[(i)]).Copy(), (dir).Copy()) * size / wr + p).Copy();
            float w = size * 0.6f / wr;
            p2.X = p3.X - lv.X * lr;
            p2.Y = p3.Y - lv.Y * lr;
            shape.Addfloatfloatfloat(p2.X - w, p2.Y - w, 0);
            shape.Addfloatfloatfloat(p2.X - w, p2.Y + w, 0);
            shape.Addfloatfloatfloat(p2.X + w, p2.Y + w, 0);
            shape.Addfloatfloatfloat(p2.X + w, p2.Y - w, 0);
        }
    }

    public static BlurNormalTextureCubeListShape CreateShape(MmFrame frame)
    {
        BlurNormalTextureCubeListShape shape = new BlurNormalTextureCubeListShape(frame);
        shape.BeginAdd(DotNum);
        AddDot(shape, (Quaternion.Identity).Copy());
        for (int i = 0; i < 4; i++)
            AddDot(shape, (Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 2, (float)Math.PI / 4, 0)).Copy());
        for (int i = 0; i < 8; i++)
            AddDot(shape, (Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 4, (float)Math.PI / 2, 0)).Copy());
        for (int i = 0; i < 4; i++)
            AddDot(shape, (Quaternion.CreateFromYawPitchRoll(i * (float)Math.PI / 2, (float)Math.PI / 4 * 3, 0)).Copy());
        AddDot(shape, (Quaternion.CreateFromYawPitchRoll(0, (float)Math.PI, 0)).Copy());
        shape.EndAdd();
        return shape;
    }

    private static void AddDot(BlurNormalTextureCubeListShape shape, Quaternion dir)
    {
        Vector3 c = new Vector3(0, 0, 0);
        Vector3 r = new Vector3(0, 0.8f, 0);
        r = (Vector3.TransformVector3Quaternion((r).Copy(), (dir).Copy())).Copy();
        shape.AddVector3Vector3floatQuaternion((r).Copy(), (c).Copy(), 0.3f, (dir).Copy());
    }
}

public class ShadowShape
{
    public static void AddShadow(QuadListShape shadowShape, Vector3 p3, float size, MmFrame frame)
    {
        Vector2 p = new Vector2();
        Vector4 lv = (frame.LightVector).Copy();
        float lr = p3.Z / lv.Z;
        p.X = p3.X - lv.X * lr;
        p.Y = p3.Y - lv.Y * lr;
        float w = size / (1 + lr * 0.1f);
        shadowShape.Addfloatfloatfloat(p.X - w, p.Y - w, 0);
        shadowShape.Addfloatfloatfloat(p.X - w, p.Y + w, 0);
        shadowShape.Addfloatfloatfloat(p.X + w, p.Y + w, 0);
        shadowShape.Addfloatfloatfloat(p.X + w, p.Y - w, 0);
    }
}
