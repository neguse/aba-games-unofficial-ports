using System.Numerics;
using static Lub;
using NVector3 = System.Numerics.Vector3;

public static class TtRender
{
    sealed class Command
    {
        public int Count;
        public bool Hud;
        public Dictionary<string, object> Bindings;
        public DrawOpts Options;
        public Matrix4x4 Model;
        public readonly float[] Matrix = new float[16];
        public readonly float[] Viewport = new float[4];
    }
    static readonly List<Command> commands = new();
    static readonly PassOpts pass = new() { ClearColor = [0, 0, 0, 1] };
    static int used;
    static bool anchored;
    static Matrix4x4 anchor;
    public static bool FirstPerson;
    public static bool Hud;
    public static void Begin() { used = 0; Hud = false; }
    static Matrix4x4 From(float[] m) => new(m[0],m[1],m[2],m[3],m[4],m[5],m[6],m[7],m[8],m[9],m[10],m[11],m[12],m[13],m[14],m[15]);
    static void Store(float[] v, Matrix4x4 m)
    {
        v[0]=m.M11; v[1]=m.M12; v[2]=m.M13; v[3]=m.M14;
        v[4]=m.M21; v[5]=m.M22; v[6]=m.M23; v[7]=m.M24;
        v[8]=m.M31; v[9]=m.M32; v[10]=m.M33; v[11]=m.M34;
        v[12]=m.M41; v[13]=m.M42; v[14]=m.M43; v[15]=m.M44;
    }
    public static void Draw(int count, Dictionary<string, object> bindings, DrawOpts options)
    {
        if (used == commands.Count) commands.Add(new Command());
        var command = commands[used++];
        command.Count = count; command.Bindings = bindings; command.Options = options; command.Hud = Hud;
        var uniforms = (Dictionary<string, object>)bindings["uniforms"];
        command.Model = From((float[])uniforms["model"]);
        uniforms["model"] = command.Matrix;
        uniforms["viewport"] = command.Viewport;
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
        for (int eye = 0; eye < 2; eye++)
        {
            var view = eye == 0 ? left : right;
            var projection = anchor * Matrix4x4.Transpose(From(view.ViewProjection));
            Xr.SelectEye(eye);
            pass.Target = Gfx.MainTex;
            Gfx.BeginPass(pass);
            for (int i = 0; i < used; i++)
            {
                var command = commands[i];
                Store(command.Matrix, command.Model * (command.Hud ? hud : world) * projection);
                command.Viewport[0] = view.Width; command.Viewport[1] = view.Height;
                var uniforms = (Dictionary<string, object>)command.Bindings["uniforms"];
                var options = (float[])uniforms["options"];
                options[0] = MathF.Max(options[0], view.Width / 640f);
                Gfx.Draw(command.Count, command.Bindings, command.Options);
            }
            Gfx.EndPass();
        }
    }
}
