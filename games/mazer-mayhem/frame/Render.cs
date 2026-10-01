using System.Collections.Generic;
using static Lub;

public static class MmRender
{
    static readonly List<MmDrawCommand> commands = new();
    static readonly List<float>[] eyes = new List<float>[] { new(), new() };
    static readonly Vector3 zeroNormal = new();
    static readonly Vector2 zeroUv = new();
    static readonly Color white = new(255, 255, 255, 255);
    static readonly TextureOpts target = new() { Target = true };
    static readonly PassOpts pass = new() { ClearColor = new float[] { 210 / 255f, 210 / 255f, 210 / 255f, 1 } };
    static readonly XrAnchor anchor = new();
    static readonly float[] matrix = new float[16];
    static readonly DrawOpts outputOptions = new() { Depth = false, Blend = Gfx.Blend.None, Cull = Gfx.Cull.None };
    static readonly PassOpts outputPass = new();
    static readonly Dictionary<string, object> outputBindings = new();
    static string outputSource;
    static ShaderRef shader;
    static string vertexSource, fragment;
    static int version, used;
    static bool anchored;
    public static void Begin(MmFrame frame)
    {
        version++;
        used = 0;
        foreach (var eye in eyes) while (eye.Count < 16) eye.Add(0);
        if (vertexSource == null) { Io.LoadText("game.vs.slang", out var source, out _, out _, out _); vertexSource = source; }
        if (fragment == null) { Io.LoadText("game.fs.slang", out var source, out _, out _, out _); fragment = source; }
        if (vertexSource != null && fragment != null) shader = Gfx.UseShader("mm-xr", vertexSource, fragment, 1);
    }
    public static void End() { }

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
                Vertex(shape.data, v.Position, zeroNormal, v.Color, zeroUv);
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
                Vertex(shape.data, v.Position, v.Normal, white, v.TextureCoordinate);
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
        if (used == commands.Count) commands.Add(new MmDrawCommand(used));
        var command = commands[used];
        used++;
        var paramsData = command.Data;
        paramsData.Clear();
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
        AddVector(paramsData, new Vector4(frame.storedBlurThickness, mode, frame.Hud ? 1 : 0, 0));
        command.MeshKey = "mesh" + shape.id;
        command.Geometry = shape.data;
        command.MeshVersion = meshVersion;
        command.Count = count;
        command.Options.Shader = shader;
        command.Options.Depth = frame.DepthEnabled;
        command.Options.DepthWrite = frame.DepthEnabled;
        command.Options.InstanceCount = 1;
        if (used < 2) return;
        var previous = commands[used - 2];
        if (previous.Geometry != command.Geometry || previous.MeshVersion != meshVersion ||
            previous.Count != count || previous.Options.Depth != command.Options.Depth) return;
        foreach (float value in paramsData) previous.Data.Add(value);
        previous.Options.InstanceCount++;
        used--;
    }
    public static void Present(XrView left, XrView right)
    {
        if (outputSource == null) { Io.LoadText("scene.output.slang", out var source, out _, out _, out _); outputSource = source; }
        if (outputSource == null) return;
        outputOptions.Shader = Gfx.UseShader("scene-output", outputSource, outputSource, 1);
        if (outputOptions.Shader == null) return;
        if (!anchored) { anchor.Recenter(left, right); anchored = true; }
        for (int i = 0; i < used; i++)
        {
            var command = commands[i];
            command.Bindings["vertices"] = Gfx.UseBuffer(command.MeshKey, Gfx.BufferType.Storage, command.Geometry, command.MeshVersion);
            command.Bindings["parameters"] = Gfx.UseBuffer(command.Key, Gfx.BufferType.Storage, command.Data, version);
        }
        for (int eye = 0; eye < 2; eye++)
        {
            var view = eye == 0 ? left : right;
            anchor.ViewProjection(view, matrix);
            for (int i = 0; i < 16; i++) eyes[eye][i] = matrix[i];
            var eyeBuffer = Gfx.UseBuffer(eye == 0 ? "left" : "right", Gfx.BufferType.Storage, eyes[eye], version);
            var scene = Gfx.UseTexture(eye == 0 ? "sceneL" : "sceneR", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, target);
            var depth = Gfx.UseTexture(eye == 0 ? "depthL" : "depthR", view.Width, view.Height, Gfx.PixelFormat.Depth24Stencil8, null, 1, target);
            pass.Target = scene;
            pass.DepthTarget = depth;
            pass.ClearDepth = 1;
            Gfx.BeginPass(pass);
            for (int i = 0; i < used; i++)
            {
                var command = commands[i];
                command.Bindings["eye"] = eyeBuffer;
                Gfx.Draw(command.Count, command.Bindings, command.Options);
            }
            Gfx.EndPass();
            outputPass.Target = view.Target;
            Gfx.BeginPass(outputPass);
            outputBindings["scene"] = scene;
            Gfx.Draw(3, outputBindings, outputOptions);
            Gfx.EndPass();
        }
    }
}

sealed class MmDrawCommand
{
    public readonly string Key;
    public readonly List<float> Data = new();
    public readonly Dictionary<string, object> Bindings = new();
    public readonly DrawOpts Options = new() { Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha };
    public List<float> Geometry;
    public string MeshKey;
    public int Count, MeshVersion;
    public MmDrawCommand(int index) { Key = "draw" + index; }
}
