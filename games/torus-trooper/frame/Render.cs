using System.Numerics;
using static Lub;
using NVector3 = System.Numerics.Vector3;

public static class TtRender
{
    sealed class Command
    {
        public bool Hud;
        public Matrix4x4 Model;
        public Vector4 Tint, Options, ImageInfo;
    }
    sealed class Batch
    {
        public int First, Count;
        public DrawImage Image;
        public readonly DrawOpts Options = new() { Depth = false };
        public readonly float[] Range = new float[4];
        public readonly float[] Viewport = new float[4];
        public readonly Dictionary<string, object> Bindings;
        public Batch()
        {
            Bindings = new() { ["uniforms"] = new Dictionary<string, object> { ["batch"] = Range, ["viewport"] = Viewport } };
        }
    }
    static readonly List<Command> commands = new();
    static readonly List<Batch> batches = new();
    static readonly List<float> vertices = new(), faces = new();
    static readonly List<float>[] eyeData = [new(), new()];
    static readonly List<int> white = [255, 255, 255, 255];
    static readonly TextureOpts textureOptions = new() { Filter = Gfx.Filter.Linear, Wrap = Gfx.Wrap.Repeat };
    static readonly PassOpts pass = new() { ClearColor = [0, 0, 0, 1] };
    static int used, batchCount, version;
    static bool anchored;
    static Matrix4x4 anchor;
    public static bool FirstPerson;
    public static bool Hud;
    public static void Begin()
    {
        used = batchCount = 0; Hud = false; version++;
        vertices.Clear(); faces.Clear();
    }
    static Matrix4x4 From(float[] m) => new(m[0],m[1],m[2],m[3],m[4],m[5],m[6],m[7],m[8],m[9],m[10],m[11],m[12],m[13],m[14],m[15]);
    static void Add(List<float> data, Matrix4x4 m)
    {
        data.Add(m.M11); data.Add(m.M12); data.Add(m.M13); data.Add(m.M14);
        data.Add(m.M21); data.Add(m.M22); data.Add(m.M23); data.Add(m.M24);
        data.Add(m.M31); data.Add(m.M32); data.Add(m.M33); data.Add(m.M34);
        data.Add(m.M41); data.Add(m.M42); data.Add(m.M43); data.Add(m.M44);
    }
    static void Add(List<float> data, Vector4 v) { data.Add(v.X); data.Add(v.Y); data.Add(v.Z); data.Add(v.W); }
    public static void Draw(Mesh mesh, float[] model, float[] tint, float width, Gfx.Blend blend, Gfx.Cull cull, DrawImage image = null)
    {
        if (mesh.count == 0) return;
        if (used == commands.Count) commands.Add(new Command());
        var command = commands[used];
        command.Model = From(model); command.Hud = Hud;
        command.Tint = tint == null ? Vector4.One : new(tint[0], tint[1], tint[2], tint[3]);
        command.Options = new(width, blend == Gfx.Blend.Additive ? 1 : 0, 0, image == null ? 0 : 1);
        command.ImageInfo = image == null ? new(1, 1, 0, 0) : new(image.width, image.height, image.levels - 1, 0);
        Batch batch = batchCount == 0 ? null : batches[batchCount - 1];
        if (batch == null || batch.Image != image || batch.Options.Blend != blend || batch.Options.Cull != cull)
        {
            if (batchCount == batches.Count) batches.Add(new Batch());
            batch = batches[batchCount++];
            batch.First = faces.Count / 4; batch.Count = 0; batch.Image = image;
            batch.Options.Blend = blend; batch.Options.Cull = cull;
        }
        int offset = vertices.Count / 8;
        vertices.AddRange(mesh.vertices);
        for (int i = 0; i < mesh.faces.Count; i += 4)
        {
            faces.Add(mesh.faces[i] + offset); faces.Add(mesh.faces[i + 1] + offset);
            faces.Add(mesh.faces[i + 2] + (mesh.faces[i + 3] == 1 ? 0 : offset));
            faces.Add(mesh.faces[i + 3] + used * 2);
        }
        batch.Count += mesh.count;
        used++;
    }
    static Matrix4x4 WorldView(Ship ship)
    {
        float angle = FirstPerson ? ship._relPos.x : ship._eyePos.x;
        float y = ship._relPos.y - (FirstPerson ? 0 : 3);
        float height = FirstPerson ? 1.5f : 8;
        var from = TrackPosition(ship.tunnel, angle, y, height);
        var to = TrackPosition(ship.tunnel, angle, y + 6, FirstPerson ? height : 5);
        var surface = TrackPosition(ship.tunnel, angle, y, 0);
        var up = NVector3.Normalize(from - surface);
        return Matrix4x4.CreateLookAt(from, to, up) * Matrix4x4.CreateScale(.05f);
    }
    static NVector3 TrackPosition(Tunnel tunnel, float angle, float y, float height)
    {
        var index = tunnel.calcIndex(y);
        var p = tunnel.getPos_4_Single_Single_Int32_Single(angle, index.y, GameMath.integer(index.x),
            1 - height / tunnel.getRadius(y));
        return new NVector3(p.x, p.y, p.z);
    }
    public static void Recenter() { anchored = false; }
    static void UpdateAnchor(XrView left, XrView right, bool focused)
    {
        if (!anchored)
        {
            var q = left.Orientation;
            var forward = NVector3.Transform(-NVector3.UnitZ, new Quaternion(q[0],q[1],q[2],q[3]));
            anchor = Matrix4x4.CreateRotationY(MathF.Atan2(-forward.X, -forward.Z)) * Matrix4x4.CreateTranslation(
                (left.Position[0] + right.Position[0]) * .5f,
                (left.Position[1] + right.Position[1]) * .5f,
                (left.Position[2] + right.Position[2]) * .5f);
            anchored = focused;
        }
    }
    public static void Present(Ship ship, XrView left, XrView right)
    {
        UpdateAnchor(left, right, Xr.Focused());
        var world = WorldView(ship);
        var hud = Matrix4x4.CreateScale(1.2f, .9f, 0) * Matrix4x4.CreateTranslation(0, 0, -2);
        var vertexBuffer = Gfx.UseBuffer("tt-vertices", Gfx.BufferType.Storage, vertices, version);
        var faceBuffer = Gfx.UseBuffer("tt-faces", Gfx.BufferType.Storage, faces, version);
        var blank = Gfx.UseTexture("tt-white", 1, 1, Gfx.PixelFormat.Rgba8, white, 1);
        for (int eye = 0; eye < 2; eye++)
        {
            var view = eye == 0 ? left : right;
            var projection = anchor * Matrix4x4.Transpose(From(view.ViewProjection));
            var data = eyeData[eye]; data.Clear();
            for (int i = 0; i < used; i++)
            {
                var command = commands[i];
                Add(data, command.Model * (command.Hud ? hud : world) * projection);
                Add(data, command.Tint);
                var options = command.Options; options.X = MathF.Max(options.X, view.Width / 640f);
                Add(data, options); Add(data, command.ImageInfo);
            }
            var drawBuffer = Gfx.UseBuffer(eye == 0 ? "tt-left-draws" : "tt-right-draws", Gfx.BufferType.Storage, data, version);
            Xr.SelectEye(eye);
            pass.Target = Gfx.MainTex;
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
        }
    }
}
