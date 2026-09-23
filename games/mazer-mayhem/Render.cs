using System.Collections.Generic;
using static Lub;

public static class MmRender
{
    static ShaderRef shader;
    static int version, drawIndex;
    public static void Begin(MmFrame frame)
    {
        version++;
        drawIndex = 0;
        shader = Gfx.UseShader("mazer-mayhem", GameShaders.vertex, GameShaders.fragment, 1);
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 210 / 255f, 210 / 255f, 210 / 255f, 1 } });
    }

    public static void End()
    {
        Gfx.EndPass();
    }

    static void Vertex(List<float> data, Vector3 p, Vector3 n, Color c, Vector2 uv)
    {
        data.Add(p.X);
        data.Add(p.Y);
        data.Add(p.Z);
        data.Add(1);
        data.Add(n.X);
        data.Add(n.Y);
        data.Add(n.Z);
        // Direct3D 9 supplies w = 1 when FLOAT3 is read as float4.
        data.Add(1);
        data.Add(c.R / 255f);
        data.Add(c.G / 255f);
        data.Add(c.B / 255f);
        data.Add(c.A / 255f);
        data.Add(uv.X);
        data.Add(uv.Y);
        data.Add(0);
        data.Add(0);
    }

    public static void DrawShapeMmFrameVertexPositionColorArrayintArrayintint(Shape shape, MmFrame frame, VertexPositionColor[] vertices, int[] indices, int count, int meshVersion)
    {
        if (shape.meshVersion != meshVersion)
        {
            shape.data.Clear();
            for (int i = 0; i < count; i++)
            {
                var v = vertices[indices[i]];
                Vertex(shape.data, v.Position, Vector3.Zero, v.Color, Vector2.Zero);
            }

            shape.meshVersion = meshVersion;
        }

        Submit(shape, frame, count, meshVersion);
    }

    public static void DrawShapeMmFrameVertexPositionNormalTextureArrayintArrayintint(Shape shape, MmFrame frame, VertexPositionNormalTexture[] vertices, int[] indices, int count, int meshVersion)
    {
        if (shape.meshVersion != meshVersion)
        {
            shape.data.Clear();
            for (int i = 0; i < count; i++)
            {
                var v = vertices[indices[i]];
                Vertex(shape.data, v.Position, v.Normal, new Color(255, 255, 255, 255), v.TextureCoordinate);
            }

            shape.meshVersion = meshVersion;
        }

        Submit(shape, frame, count, meshVersion);
    }

    static void AddVector(List<float> data, Vector4 v)
    {
        data.Add(v.X);
        data.Add(v.Y);
        data.Add(v.Z);
        data.Add(v.W);
    }

    static void Submit(Shape shape, MmFrame frame, int count, int meshVersion)
    {
        if (count <= 0 || shader == null)
            return;
        var paramsData = new List<float>();
        foreach (float x in frame.storedWorldMatrix.M)
            paramsData.Add(x);
        foreach (float x in frame.storedViewMatrix.M)
            paramsData.Add(x);
        foreach (float x in frame.storedProjMatrix.M)
            paramsData.Add(x);
        float[] m = frame.storedWorldMatrix.M;
        float a = m[0], b = m[1], c = m[2], d = m[4], e = m[5], f = m[6], g = m[8], h = m[9], i = m[10];
        float determinant = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
        float[] normal = new float[]
        {
            (e * i - f * h) / determinant,
            (f * g - d * i) / determinant,
            (d * h - e * g) / determinant,
            0,
            (c * h - b * i) / determinant,
            (a * i - c * g) / determinant,
            (b * g - a * h) / determinant,
            0,
            (b * f - c * e) / determinant,
            (c * d - a * f) / determinant,
            (a * e - b * d) / determinant,
            0,
            0,
            0,
            0,
            1
        };
        foreach (float x in normal)
            paramsData.Add(x);
        AddVector(paramsData, frame.LightVector);
        AddVector(paramsData, frame.storedEffectColor);
        AddVector(paramsData, frame.storedBlurColor);
        AddVector(paramsData, new Vector4(210 / 255f, 210 / 255f, 210 / 255f, 1));
        AddVector(paramsData, frame.storedVelocity);
        int mode = frame.Technique == "SimpleTech" ? 0 : frame.Technique == "SimpleFogTech" ? 1 : frame.Technique == "BoardTech" ? 2 : frame.Technique == "BlurLightingTech" ? 3 : frame.Technique == "BlurAlphaLightingTech" ? 4 : 5;
        AddVector(paramsData, new Vector4(frame.storedBlurThickness, mode, 0, 0));
        var vertices = Gfx.UseBuffer("mesh" + shape.id.ToString(), Gfx.BufferType.Storage, shape.data, meshVersion);
        var parameters = Gfx.UseBuffer("draw" + drawIndex.ToString(), Gfx.BufferType.Storage, paramsData, version);
        drawIndex++;
        if (vertices == null || parameters == null)
            return;
        Gfx.Draw(count, new Dictionary<string, object> { ["vertices"] = vertices, ["parameters"] = parameters }, new DrawOpts { Shader = shader, Depth = frame.DepthEnabled, DepthWrite = frame.DepthEnabled, Blend = Gfx.Blend.Alpha, Cull = Gfx.Cull.None });
    }
}
