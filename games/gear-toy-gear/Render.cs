using System.Collections.Generic;
using static Lub;

public static class GtgRender
{
    static ShaderRef shader;
    static TextureRef edge, bloom, blank;
    static int version, drawIndex;
    public static bool Begin()
    {
        version++;
        drawIndex = 0;
        shader = Gfx.UseShader("gear-toy-gear", GameShaders.vertex, GameShaders.fragment, 1);
        edge = Gfx.UseTexture("edge", 640, 480, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        bloom = Gfx.UseTexture("bloom", 640, 480, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        blank = Gfx.UseTexture("blank", 1, 1, Gfx.PixelFormat.Rgba8, new List<int> { 0, 0, 0, 0 }, 1);
        return shader != null && edge != null && bloom != null && blank != null;
    }

    public static void BeginMain()
    {
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { Stage.BackgroundR, Stage.BackgroundG, Stage.BackgroundB, 1 } });
    }

    public static void BeginEdge()
    {
        Gfx.BeginPass(new PassOpts { Target = edge, ClearColor = new float[] { 0, 0, 0, 0 } });
    }

    public static void BeginBloom()
    {
        Gfx.BeginPass(new PassOpts { Target = bloom, ClearColor = new float[] { 0, 0, 0, 0 } });
    }

    public static void EndPass()
    {
        Gfx.EndPass();
    }

    static void Add(List<float> data, Vector4 v)
    {
        data.Add(v.X);
        data.Add(v.Y);
        data.Add(v.Z);
        data.Add(v.W);
    }

    static List<float> Params(GtgFrame frame, int mode)
    {
        var data = new List<float>();
        foreach (float v in frame.ViewMatrix.M)
            data.Add(v);
        foreach (float v in frame.ProjMatrix.M)
            data.Add(v);
        Add(data, new Vector4(frame.ShadowDepthOffset, mode, 0, 0));
        foreach (Vector3 v in frame.LightColors)
            Add(data, Vector4.FromVector3(v, 0));
        return data;
    }

    static void Draw(GtgFrame frame, int count, int instances, List<float> data, BufferRef mesh, TextureRef surface)
    {
        var parameters = Gfx.UseBuffer("draw" + drawIndex.ToString(), Gfx.BufferType.Storage, data, version);
        drawIndex++;
        if (mesh == null || parameters == null)
            return;
        Gfx.Draw(count, new Dictionary<string, object> { ["vertices"] = mesh, ["parameters"] = parameters, ["surface"] = surface }, new DrawOpts { Shader = shader, InstanceCount = instances, Depth = frame.DepthEnabled, DepthWrite = frame.DepthEnabled, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha });
    }

    public static void Submit(Shape shape, GtgFrame frame, VertexPositionColor[] verts, int[] indices, int count, int primitives, Vector4[] a, Vector4[] b, Vector4[] c, int instances)
    {
        if (shader == null)
            return;
        if (shape.Geometry.Count == 0)
            for (int i = 0; i < primitives * 3; i++)
            {
                var v = verts[indices[i]];
                Add(shape.Geometry, Vector4.FromVector3(v.Position, 1));
                Add(shape.Geometry, new Vector4(v.Color.R / 255f, v.Color.G / 255f, v.Color.B / 255f, v.Color.A / 255f));
            }

        int mode = frame.Technique == "DepthTech" ? 0 : frame.Technique == "BloomTech" ? 1 : frame.Technique == "EdgeTech" ? 2 : frame.Technique == "ParticleTech" ? 3 : frame.Technique == "ParticleBloomTech" ? 4 : 5;
        var data = Params(frame, mode);
        for (int i = 0; i < instances; i++)
        {
            Add(data, a[i]);
            Add(data, b[i]);
            Add(data, c[i]);
        }

        var mesh = Gfx.UseBuffer("mesh" + shape.Id.ToString(), Gfx.BufferType.Storage, shape.Geometry, 1);
        Draw(frame, primitives * 3, instances, data, mesh, blank);
    }

    public static void Composite(GtgFrame frame)
    {
        var mesh = Gfx.UseBuffer("fullscreen", Gfx.BufferType.Storage, new List<float> { 0, 0, 0, 0, 0, 0, 0, 0 }, 1);
        Draw(frame, 3, 1, Params(frame, 6), mesh, edge);
        Draw(frame, 3, 1, Params(frame, 7), mesh, bloom);
    }
}
