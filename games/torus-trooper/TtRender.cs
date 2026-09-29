using System;
using System.Collections.Generic;
using static Lub;

public static class TtRender
{
    static readonly List<TtDrawCommand> commands = new();
    static readonly List<TtDrawBatch> batches = new();
    static readonly List<float> vertices = new(), faces = new();
    static readonly List<float>[] eyeData = new List<float>[] { new(), new() };
    static readonly List<int> white = new() { 255, 255, 255, 255 };
    static readonly TextureOpts textureOptions = new() { Filter = Gfx.Filter.Linear, Wrap = Gfx.Wrap.Repeat };
    static readonly PassOpts pass = new() { ClearColor = new float[] { 0, 0, 0, 1 } };
    static readonly TextureOpts sceneOptions = new() { Target = true, Filter = Gfx.Filter.Nearest };
    static readonly DrawOpts outputOptions = new() { Depth = false, Blend = Gfx.Blend.None, Cull = Gfx.Cull.None };
    static readonly Dictionary<string, object> outputBindings = new();
    static string outputSource;
    static int used, batchCount, version;
    static bool anchored;
    static readonly float[] anchor = new float[16], world = new float[16], projection = new float[16];
    static readonly float[] eyeView = new float[16], modelView = new float[16], modelProjection = new float[16];
    static readonly float[] from = new float[3], to = new float[3], surface = new float[3], up = new float[3];
    static readonly float[] hudMatrix = new float[] { 1.2f,0,0,0, 0,.9f,0,0, 0,0,0,0, 0,0,-2,1 };
    public static bool FirstPerson;
    public static bool Hud;
    public static void Begin()
    {
        used = 0; batchCount = 0; Hud = false; version++;
        vertices.Clear(); faces.Clear();
    }
    static void Add(List<float> data, float[] values)
    {
        foreach (float value in values) data.Add(value);
    }
    public static void Draw(Mesh mesh, float[] model, float[] tint, float width, Gfx.Blend blend, Gfx.Cull cull, DrawImage image = null)
    {
        if (mesh.count == 0) return;
        if (used == commands.Count) commands.Add(new TtDrawCommand());
        var command = commands[used];
        for (int i = 0; i < 16; i++) command.Model[i] = model[i];
        command.Hud = Hud;
        for (int i = 0; i < 4; i++) command.Tint[i] = tint == null ? 1 : tint[i];
        command.Options[0] = width;
        command.Options[1] = blend == Gfx.Blend.Additive ? 1 : 0;
        command.Options[3] = image == null ? 0 : 1;
        command.ImageInfo[0] = image == null ? 1 : image.width;
        command.ImageInfo[1] = image == null ? 1 : image.height;
        command.ImageInfo[2] = image == null ? 0 : image.levels - 1;
        TtDrawBatch batch = batchCount == 0 ? null : batches[batchCount - 1];
        if (batch == null || batch.Image != image || batch.Options.Blend != blend || batch.Options.Cull != cull)
        {
            if (batchCount == batches.Count) batches.Add(new TtDrawBatch());
            batch = batches[batchCount];
            batchCount++;
            batch.First = faces.Count / 4; batch.Count = 0; batch.Image = image;
            batch.Options.Blend = blend; batch.Options.Cull = cull;
        }
        int offset = vertices.Count / 8;
        foreach (float value in mesh.vertices) vertices.Add(value);
        for (int i = 0; i < mesh.faces.Count; i += 4)
        {
            faces.Add(mesh.faces[i] + offset); faces.Add(mesh.faces[i + 1] + offset);
            faces.Add(mesh.faces[i + 2] + (mesh.faces[i + 3] == 1 ? 0 : offset));
            faces.Add(mesh.faces[i + 3] + used * 2);
        }
        batch.Count += mesh.count;
        used++;
    }
    static float[] WorldView(Ship ship)
    {
        float angle = FirstPerson ? ship._relPos.x : ship._eyePos.x;
        float y = FirstPerson ? ship._relPos.y : ship._relPos.y * .3f - 3;
        float height = FirstPerson ? 1.5f : 8;
        TrackPosition(from, ship.tunnel, angle, y, height);
        TrackPosition(to, ship.tunnel, angle, y + 6 + (FirstPerson ? 0 : ship._relPos.y * .3f), FirstPerson ? height : 5);
        TrackPosition(surface, ship.tunnel, angle, y, 0);
        float length = 0;
        for (int i = 0; i < 3; i++) { up[i] = from[i] - surface[i]; length += up[i] * up[i]; }
        length = (float)Math.Sqrt(length);
        for (int i = 0; i < 3; i++) up[i] /= length;
        FrameMath.LookAt(world, from, to, up, .05f);
        return world;
    }
    static void TrackPosition(float[] result, Tunnel tunnel, float angle, float y, float height)
    {
        var index = tunnel.calcIndex(y);
        var p = tunnel.getPos_4_Single_Single_Int32_Single(angle, index.y, GameMath.integer(index.x),
            1 - height / tunnel.getRadius(y));
        result[0] = p.x; result[1] = p.y; result[2] = p.z;
    }
    public static void Recenter() { anchored = false; }
    static void UpdateAnchor(XrView left, XrView right, bool focused)
    {
        if (!anchored)
        {
            FrameMath.Anchor(anchor, left, right);
            anchored = focused;
        }
    }
    public static void Present(Ship ship, XrView left, XrView right)
    {
        bool immersive = right != null;
        if (immersive)
        {
            if (outputSource == null)
            {
                Io.LoadText("mesh.output.slang", out var source, out _, out _, out _);
                outputSource = source;
            }
            if (outputSource == null) return;
            outputOptions.Shader = Gfx.UseShader("tt-output", outputSource, outputSource, 1);
            if (outputOptions.Shader == null) return;
            UpdateAnchor(left, right, Xr.Focused());
            WorldView(ship);
        }
        var vertexBuffer = Gfx.UseBuffer("tt-vertices", Gfx.BufferType.Storage, vertices, version);
        var faceBuffer = Gfx.UseBuffer("tt-faces", Gfx.BufferType.Storage, faces, version);
        var blank = Gfx.UseTexture("tt-white", 1, 1, Gfx.PixelFormat.Rgba8, white, 1);
        for (int eye = 0; eye < (immersive ? 2 : 1); eye++)
        {
            var view = eye == 0 ? left : right;
            if (immersive)
            {
                FrameMath.Transpose(eyeView, view.ViewProjection);
                FrameMath.Multiply(projection, anchor, eyeView);
            }
            var data = eyeData[eye]; data.Clear();
            for (int i = 0; i < used; i++)
            {
                var command = commands[i];
                if (immersive)
                {
                    FrameMath.Multiply(modelView, command.Model, command.Hud ? hudMatrix : world);
                    FrameMath.Multiply(modelProjection, modelView, projection);
                }
                Add(data, immersive ? modelProjection : command.Model);
                Add(data, command.Tint);
                data.Add((float)Math.Max(command.Options[0], view.Width / 640f));
                for (int j = 1; j < 4; j++) data.Add(command.Options[j]);
                Add(data, command.ImageInfo);
            }
            var drawBuffer = Gfx.UseBuffer(eye == 0 ? "tt-left-draws" : "tt-right-draws", Gfx.BufferType.Storage, data, version);
            if (immersive) Xr.SelectEye(eye);
            var scene = immersive ? Gfx.UseTexture(eye == 0 ? "tt-left-scene" : "tt-right-scene", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, sceneOptions) : Gfx.MainTex;
            pass.Target = scene;
            Gfx.BeginPass(pass);
            for (int i = 0; i < batchCount; i++)
            {
                var batch = batches[i];
                batch.Options.Shader = Game.shader;
                batch.Bindings["verts"] = vertexBuffer; batch.Bindings["faces"] = faceBuffer; batch.Bindings["draws"] = drawBuffer;
                var texture = batch.Image;
                batch.Bindings["image"] = texture == null ? blank : Gfx.UseTexture(texture.key, texture.width, texture.atlasHeight, Gfx.PixelFormat.Rgba8, texture.pixels, 1, textureOptions);
                batch.Range[0] = batch.First;
                batch.Viewport[0] = view.Width; batch.Viewport[1] = view.Height;
                Gfx.Draw(batch.Count, batch.Bindings, batch.Options);
            }
            Gfx.EndPass();
            if (immersive)
            {
                pass.Target = Gfx.MainTex;
                Gfx.BeginPass(pass);
                outputBindings["scene"] = scene;
                Gfx.Draw(3, outputBindings, outputOptions);
                Gfx.EndPass();
            }
        }
    }
}

sealed class TtDrawCommand
{
    public bool Hud;
    public readonly float[] Model = new float[16];
    public readonly float[] Tint = new float[4], Options = new float[] { 0, 0, 0, 0 }, ImageInfo = new float[] { 0, 0, 0, 0 };
}
sealed class TtDrawBatch
{
    public int First, Count;
    public DrawImage Image;
    public readonly DrawOpts Options = new() { Depth = false };
    public readonly float[] Range = new float[] { 0, 0, 0, 0 };
    public readonly float[] Viewport = new float[] { 0, 0, 0, 0 };
    public readonly Dictionary<string, object> Bindings;
    public TtDrawBatch()
    {
        Bindings = new() { ["uniforms"] = new Dictionary<string, object> { ["batch"] = Range, ["viewport"] = Viewport } };
    }
}
