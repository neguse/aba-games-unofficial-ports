using System;
using System.Collections.Generic;
using static Lub;

public static class GtgRender
{
    static readonly List<GtgDrawCommand> commands = new();
    static readonly Dictionary<int, string> meshKeys = new();
    static readonly List<float> fullscreen = new() { 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly List<float>[] eyes = new List<float>[] { new(), new() };
    static readonly List<int> transparent = new() { 0, 0, 0, 0 };
    static readonly TextureOpts target = new() { Target = true };
    static readonly PassOpts passOpts = new() { ClearColor = new float[4] };
    static readonly DrawOpts outputOptions = new() { Depth = false, Blend = Gfx.Blend.None, Cull = Gfx.Cull.None };
    static readonly PassOpts outputPass = new();
    static readonly Dictionary<string, object> outputBindings = new();
    static string outputSource;
    static readonly List<TextureRef> layerTargets = new();
    static ShaderRef shader, layerShader;
    static string vertex, fragment, layerFragment;
    static TextureRef blank;
    static int version, used, pass;
    static bool anchored;
    static readonly float[] anchor = new float[16], eyeView = new float[16], projection = new float[16], matrix = new float[16];

    public static bool Begin()
    {
        foreach (var eye in eyes)
            while (eye.Count < 16) eye.Add(0);
        used = 0;
        version++;
        if (vertex == null)
        {
            Io.LoadText("game.vs.slang", out var source, out _, out _, out _);
            vertex = source;
        }
        if (fragment == null)
        {
            Io.LoadText("game.fs.slang", out var source, out _, out _, out _);
            fragment = source;
            layerFragment = "#define GTG_ALPHA_TARGET\n" + source;
        }
        if (vertex == null || fragment == null) return false;
        shader = Gfx.UseShader("gtg-xr", vertex, fragment, 1);
        layerShader = Gfx.UseShader("gtg-xr-layer", vertex, layerFragment, 1);
        blank = Gfx.UseTexture("blank", 1, 1, Gfx.PixelFormat.Rgba8, transparent, 1);
        if (blank == null) return false;
        if (layerTargets.Count == 0) { layerTargets.Add(blank); layerTargets.Add(blank); }
        return shader != null && layerShader != null;
    }
    public static void BeginMain() { pass = 2; }
    public static void BeginEdge() { pass = 0; }
    public static void BeginBloom() { pass = 1; }
    public static void EndPass() { }

    static void Add(List<float> data, Vector4 v)
    {
        data.Add(v.X); data.Add(v.Y); data.Add(v.Z); data.Add(v.W);
    }
    static GtgDrawCommand Record(GtgFrame frame, int mode)
    {
        if (used == commands.Count) commands.Add(new GtgDrawCommand(used));
        var command = commands[used];
        used++;
        command.Pass = pass;
        command.Mode = mode;
        command.Options.Shader = pass < 2 ? layerShader : shader;
        command.Options.Depth = mode < 6 && frame.DepthEnabled;
        command.Options.DepthWrite = mode < 6 && frame.DepthEnabled;
        var data = command.Data;
        data.Clear();
        foreach (float value in frame.ViewMatrix.M) data.Add(value);
        var m = frame.ViewMatrix.M;
        const float scale = .05f;
        for (int i = 0; i < 12; i++) data.Add(i == 0 || i == 5 || i == 10 ? scale : 0);
        data.Add((m[12]*m[0]+m[13]*m[1]+m[14]*m[2])*scale);
        data.Add((m[12]*m[4]+m[13]*m[5]+m[14]*m[6])*scale);
        data.Add((m[12]*m[8]+m[13]*m[9]+m[14]*m[10])*scale);
        data.Add(1);
        Add(data, new Vector4(frame.ShadowDepthOffset, mode, 0, 0));
        foreach (var light in frame.LightColors)
        {
            data.Add(light.X); data.Add(light.Y); data.Add(light.Z); data.Add(0);
        }
        return command;
    }
    public static void Submit(Shape shape, GtgFrame frame, VertexPositionColor[] verts, int[] indices, int count, int primitives, Vector4[] a, Vector4[] b, Vector4[] c, int instances)
    {
        if (shape.Geometry.Count == 0)
            for (int i = 0; i < primitives * 3; i++)
            {
                var v = verts[indices[i]];
                Add(shape.Geometry, Vector4.FromVector3(v.Position, 1));
                Add(shape.Geometry, new Vector4(v.Color.R / 255f, v.Color.G / 255f, v.Color.B / 255f, v.Color.A / 255f));
            }
        int mode = frame.Technique == "DepthTech" ? 0 : frame.Technique == "BloomTech" ? 1 : frame.Technique == "EdgeTech" ? 2 : frame.Technique == "ParticleTech" ? 3 : frame.Technique == "ParticleBloomTech" ? 4 : 5;
        var command = Record(frame, mode);
        for (int i = 0; i < instances; i++) { Add(command.Data, a[i]); Add(command.Data, b[i]); Add(command.Data, c[i]); }
        if (!meshKeys.TryGetValue(shape.Id, out var key))
        {
            key = "mesh" + shape.Id;
            meshKeys[shape.Id] = key;
        }
        command.MeshKey = key;
        command.Geometry = shape.Geometry;
        command.Count = primitives * 3;
        command.Options.InstanceCount = instances;
        if (used < 2) return;
        var previous = commands[used - 2];
        if (previous.Pass != command.Pass || previous.Geometry != command.Geometry || previous.Count != command.Count ||
            previous.Options.Depth != command.Options.Depth) return;
        for (int i = 0; i < 132; i++) if (previous.Data[i] != command.Data[i]) return;
        for (int i = 132; i < command.Data.Count; i++) previous.Data.Add(command.Data[i]);
        previous.Options.InstanceCount += instances;
        used--;
    }
    public static void Composite(GtgFrame frame)
    {
        for (int mode = 6; mode <= 7; mode++)
        {
            var command = Record(frame, mode);
            command.MeshKey = "fullscreen";
            command.Geometry = fullscreen;
            command.Count = 3;
            command.Options.InstanceCount = 1;
        }
    }
    public static void Present(XrView left, XrView right)
    {
        if (outputSource == null) { Io.LoadText("scene.output.slang", out var source, out _, out _, out _); outputSource = source; }
        if (outputSource == null) return;
        outputOptions.Shader = Gfx.UseShader("scene-output", outputSource, outputSource, 1);
        if (outputOptions.Shader == null) return;
        if (!anchored)
        {
            FrameMath.Anchor(anchor, left, right);
            anchored = true;
        }
        for (int i = 0; i < used; i++)
        {
            var command = commands[i];
            command.Bindings["vertices"] = Gfx.UseBuffer(command.MeshKey, Gfx.BufferType.Storage, command.Geometry, 1);
            command.Bindings["parameters"] = Gfx.UseBuffer(command.Key, Gfx.BufferType.Storage, command.Data, version);
        }
        for (int eye = 0; eye < 2; eye++)
        {
            var view = eye == 0 ? left : right;
            FrameMath.Transpose(eyeView, view.ViewProjection);
            FrameMath.Multiply(projection, anchor, eyeView);
            FrameMath.Transpose(matrix, projection);
            var values = eyes[eye];
            for (int i = 0; i < 16; i++) values[i] = matrix[i];
            var eyeBuffer = Gfx.UseBuffer(eye == 0 ? "left" : "right", Gfx.BufferType.Storage, values, version);
            var edge = Gfx.UseTexture(eye == 0 ? "edgeL" : "edgeR", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, target);
            var bloom = Gfx.UseTexture(eye == 0 ? "bloomL" : "bloomR", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, target);
            var edgeAlpha = Gfx.UseTexture(eye == 0 ? "edgeAlphaL" : "edgeAlphaR", view.Width, view.Height, Gfx.PixelFormat.R8, null, 1, target);
            var bloomAlpha = Gfx.UseTexture(eye == 0 ? "bloomAlphaL" : "bloomAlphaR", view.Width, view.Height, Gfx.PixelFormat.R8, null, 1, target);
            var scene = Gfx.UseTexture(eye == 0 ? "sceneL" : "sceneR", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, target);
            var depth = Gfx.UseTexture(eye == 0 ? "depthL" : "depthR", view.Width, view.Height, Gfx.PixelFormat.Depth24Stencil8, null, 1, target);
            Xr.SelectEye(eye);
            for (int layer = 0; layer < 3; layer++)
            {
                layerTargets[0] = layer == 0 ? edge : bloom;
                layerTargets[1] = layer == 0 ? edgeAlpha : bloomAlpha;
                passOpts.Targets = layer < 2 ? layerTargets : null;
                passOpts.Target = layer == 2 ? scene : null;
                passOpts.DepthTarget = depth;
                passOpts.ClearDepth = 1;
                passOpts.ClearColor[0] = layer == 2 ? Stage.BackgroundR : 0;
                passOpts.ClearColor[1] = layer == 2 ? Stage.BackgroundG : 0;
                passOpts.ClearColor[2] = layer == 2 ? Stage.BackgroundB : 0;
                passOpts.ClearColor[3] = layer == 2 ? 1 : 0;
                Gfx.BeginPass(passOpts);
                for (int i = 0; i < used; i++)
                {
                    var command = commands[i];
                    if (command.Pass != layer) continue;
                    command.Bindings["eye"] = eyeBuffer;
                    command.Bindings["surface"] = command.Mode == 6 ? edge : command.Mode == 7 ? bloom : blank;
                    command.Bindings["surfaceAlpha"] = command.Mode == 6 ? edgeAlpha : command.Mode == 7 ? bloomAlpha : blank;
                    Gfx.Draw(command.Count, command.Bindings, command.Options);
                }
                Gfx.EndPass();
            }
            outputPass.Target = Gfx.MainTex;
            Gfx.BeginPass(outputPass);
            outputBindings["scene"] = scene;
            Gfx.Draw(3, outputBindings, outputOptions);
            Gfx.EndPass();
        }
    }
}

sealed class GtgDrawCommand
{
    public readonly string Key;
    public readonly List<float> Data = new();
    public readonly Dictionary<string, object> Bindings = new();
    public readonly DrawOpts Options = new() { Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha };
    public List<float> Geometry;
    public string MeshKey;
    public int Count, Pass, Mode;
    public GtgDrawCommand(int index) { Key = "draw" + index; }
}
