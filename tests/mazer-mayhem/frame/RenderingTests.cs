using static Lub;

static class RenderingTests
{
    public static void Run()
    {
        var shader = Gfx.UseShader("mm-render-test", File.ReadAllText("game.vs.slang"), File.ReadAllText("game.fs.slang"), 1);
        if (shader == null) throw new Exception("MM shaders must compile");
        var scene = Gfx.UseTexture("mm-scene-test", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        var depth = Gfx.UseTexture("mm-depth-test", 16, 16, Gfx.PixelFormat.Depth24Stencil8, null, 1, new TextureOpts { Target = true });
        var data = new List<float>(new float[88]);
        foreach (int offset in new[] { 0,16,32,48 }) for (int i = 0; i < 4; i++) data[offset+i*5] = 1;
        var bindings = new Dictionary<string, object> {
            ["parameters"] = Gfx.UseBuffer("mm-test-parameters", Gfx.BufferType.Storage, data, 1),
            ["eye"] = Gfx.UseBuffer("mm-test-eye", Gfx.BufferType.Storage, new List<float>(Matrix.Identity.M), 1)
        };
        var options = new DrawOpts { Shader = shader, Depth = true, DepthWrite = true, Blend = Gfx.Blend.Alpha, Cull = Gfx.Cull.None };
        int version = 0;
        void Draw(float z, float[] color)
        {
            var vertices = new List<float>();
            foreach (var p in new float[][] { [-1,-1], [3,-1], [-1,3] })
            {
                vertices.AddRange([p[0]/.06f,p[1]/.06f,z/.06f,1, 0,0,1,1]);
                vertices.AddRange(color); vertices.AddRange([0,0,0,0]);
            }
            bindings["vertices"] = Gfx.UseBuffer("mm-test-vertices", Gfx.BufferType.Storage, vertices, ++version);
            Gfx.Draw(3, bindings, options);
        }
        float background = 210/255f;
        Gfx.BeginPass(new PassOpts { Target = scene, DepthTarget = depth, ClearDepth = 1, ClearColor = [background,background,background,1] });
        Draw(.2f, [.6f,.2f,.1f,.25f]);
        Draw(.8f, [0,1,0,1]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(scene, "mm-depth-read"), [.6f*.25f+background*.75f,.2f*.25f+background*.75f,.1f*.25f+background*.75f], "MM original alpha over gray and rear occlusion");
        RenderingReadback.Output(scene);
        Gfx.BeginPass(new PassOpts { Target = scene, DepthTarget = depth, ClearDepth = 1 });
        Draw(.8f, [0,1,0,1]);
        options.Depth = false; options.DepthWrite = false;
        Draw(.9f, [1,0,0,.5f]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(scene, "mm-hud-read"), [.5f,.5f,0], "MM depth reset and HUD draw order");
        Console.WriteLine("PASS MM GPU original alpha, depth, HUD order and XR output transfer");
    }
}
