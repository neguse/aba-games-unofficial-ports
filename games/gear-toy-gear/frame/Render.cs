using System.Numerics;
using static Lub;
using NVector3 = System.Numerics.Vector3;
using NQuaternion = System.Numerics.Quaternion;

public static class GtgRender
{
    sealed class Command
    {
        public readonly string Key;
        public readonly List<float> Data = new(512);
        public readonly Dictionary<string, object> Bindings = new();
        public readonly DrawOpts Options = new() { Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha };
        public List<float> Geometry;
        public string MeshKey;
        public int Count, Pass, Mode;
        public Command(int index) { Key = "draw" + index; }
    }
    static readonly List<Command> commands = new();
    static readonly Dictionary<int, string> meshKeys = new();
    static readonly List<float> fullscreen = [0, 0, 0, 0, 0, 0, 0, 0];
    static readonly List<float>[] eyes = [new(new float[16]), new(new float[16])];
    static readonly List<int> transparent = [0, 0, 0, 0];
    static readonly TextureOpts target = new() { Target = true };
    static readonly PassOpts passOpts = new() { ClearColor = new float[4] };
    static readonly string vertex = File.ReadAllText("game.vs.slang");
    static readonly string fragment = File.ReadAllText("game.fs.slang");
    static ShaderRef shader;
    static TextureRef blank;
    static int version, used, pass;
    static bool anchored;
    static Matrix4x4 anchor;

    public static bool Begin()
    {
        used = 0;
        version++;
        shader = Gfx.UseShader("gtg-xr", vertex, fragment, 1);
        blank = Gfx.UseTexture("blank", 1, 1, Gfx.PixelFormat.Rgba8, transparent, 1);
        return shader != null && blank != null;
    }
    public static void BeginMain() { pass = 2; }
    public static void BeginEdge() { pass = 0; }
    public static void BeginBloom() { pass = 1; }
    public static void EndPass() { }

    static void Add(List<float> data, Vector4 v)
    {
        data.Add(v.X); data.Add(v.Y); data.Add(v.Z); data.Add(v.W);
    }
    static Command Record(GtgFrame frame, int mode)
    {
        if (used == commands.Count) commands.Add(new Command(used));
        var command = commands[used++];
        command.Pass = pass;
        command.Mode = mode;
        command.Options.Shader = shader;
        command.Options.Depth = mode < 6 && frame.DepthEnabled;
        command.Options.DepthWrite = mode < 6 && frame.DepthEnabled;
        var data = command.Data;
        data.Clear();
        data.AddRange(frame.ViewMatrix.M);
        var m = frame.ViewMatrix.M;
        var view = new Matrix4x4(m[0],m[1],m[2],m[3],m[4],m[5],m[6],m[7],m[8],m[9],m[10],m[11],m[12],m[13],m[14],m[15]);
        Matrix4x4.Invert(view, out var camera);
        const float scale = .05f;
        data.AddRange([scale,0,0,0, 0,scale,0,0, 0,0,scale,0,
            -camera.M41*scale,-camera.M42*scale,-camera.M43*scale,1]);
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
        if (!meshKeys.TryGetValue(shape.Id, out var key)) meshKeys[shape.Id] = key = "mesh" + shape.Id;
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
        if (!anchored)
        {
            var q = left.Orientation;
            var forward = NVector3.Transform(-NVector3.UnitZ, new NQuaternion(q[0],q[1],q[2],q[3]));
            float yaw = MathF.Atan2(-forward.X, -forward.Z);
            anchor = Matrix4x4.CreateRotationY(yaw) * Matrix4x4.CreateTranslation(
                (left.Position[0] + right.Position[0]) * .5f,
                (left.Position[1] + right.Position[1]) * .5f,
                (left.Position[2] + right.Position[2]) * .5f);
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
            var p = view.ViewProjection;
            var vp = new Matrix4x4(p[0],p[4],p[8],p[12],p[1],p[5],p[9],p[13],p[2],p[6],p[10],p[14],p[3],p[7],p[11],p[15]);
            var matrix = Matrix4x4.Transpose(anchor * vp);
            var values = eyes[eye];
            values[0]=matrix.M11; values[1]=matrix.M12; values[2]=matrix.M13; values[3]=matrix.M14;
            values[4]=matrix.M21; values[5]=matrix.M22; values[6]=matrix.M23; values[7]=matrix.M24;
            values[8]=matrix.M31; values[9]=matrix.M32; values[10]=matrix.M33; values[11]=matrix.M34;
            values[12]=matrix.M41; values[13]=matrix.M42; values[14]=matrix.M43; values[15]=matrix.M44;
            var eyeBuffer = Gfx.UseBuffer(eye == 0 ? "left" : "right", Gfx.BufferType.Storage, values, version);
            var edge = Gfx.UseTexture(eye == 0 ? "edgeL" : "edgeR", view.Width, view.Height, Gfx.PixelFormat.Rgba8, null, 1, target);
            var bloom = Gfx.UseTexture(eye == 0 ? "bloomL" : "bloomR", view.Width / 2, view.Height / 2, Gfx.PixelFormat.Rgba8, null, 1, target);
            Xr.SelectEye(eye);
            for (int layer = 0; layer < 3; layer++)
            {
                passOpts.Target = layer == 0 ? edge : layer == 1 ? bloom : Gfx.MainTex;
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
                    Gfx.Draw(command.Count, command.Bindings, command.Options);
                }
                Gfx.EndPass();
            }
        }
    }
}
