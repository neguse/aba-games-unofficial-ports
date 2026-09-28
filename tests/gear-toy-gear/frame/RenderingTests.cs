using static Lub;

static class RenderingTests
{
    public static void Run()
    {
        var shader = Gfx.UseShader("gtg-render-test", File.ReadAllText("game.vs.slang"), File.ReadAllText("game.fs.slang"), 1);
        if (shader == null) throw new Exception("GTG shaders must compile");
        var target = Gfx.UseTexture("gtg-scene-test", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        var depth = Gfx.UseTexture("gtg-depth-test", 16, 16, Gfx.PixelFormat.Depth24Stencil8, null, 1, new TextureOpts { Target = true });
        var vertices = new List<float>();
        foreach (var p in new float[][] { [-1,-1], [3,-1], [-1,3] }) vertices.AddRange([p[0],p[1],0,1, 1,1,1,1]);
        var data = new List<float>(new float[144]);
        foreach (int offset in new[] { 0, 16 }) for (int i = 0; i < 4; i++) data[offset+i*5] = 1;
        data[33] = 5; data[135] = 1; data[139] = 1;
        var bindings = new Dictionary<string, object> {
            ["vertices"] = Gfx.UseBuffer("gtg-test-vertices", Gfx.BufferType.Storage, vertices, 1),
            ["eye"] = Gfx.UseBuffer("gtg-test-eye", Gfx.BufferType.Storage, new List<float>(Matrix.Identity.M), 1),
            ["surface"] = Gfx.UseTexture("gtg-test-source", 1, 1, Gfx.PixelFormat.Rgba8, [32,48,64,80], 1)
        };
        var options = new DrawOpts { Shader = shader, Depth = true, DepthWrite = true, Blend = Gfx.Blend.AlphaRgba, Cull = Gfx.Cull.None };
        int version = 0;
        void Draw(float z, float[] color)
        {
            data[134] = z;
            for (int c = 0; c < 4; c++) data[140+c] = color[c];
            bindings["parameters"] = Gfx.UseBuffer("gtg-test-parameters", Gfx.BufferType.Storage, data, ++version);
            Gfx.Draw(3, bindings, options);
        }
        Gfx.BeginPass(new PassOpts { Target = target, DepthTarget = depth, ClearDepth = 1, ClearColor = [.1f,.2f,.3f,0] });
        Draw(.1f, [1,1,1,0]);
        Draw(.2f, [.6f,.2f,.1f,.25f]);
        Draw(.8f, [0,1,0,1]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-depth-read"), [.225f,.2f,.25f,.0625f], "GTG original alpha and rear occlusion");
        RenderingReadback.Output(target);
        Gfx.BeginPass(new PassOpts { Target = target, DepthTarget = depth, ClearDepth = 1, ClearColor = [0,0,0,0] });
        Draw(.8f, [0,1,0,1]);
        options.Depth = false; options.DepthWrite = false;
        Draw(.9f, [1,0,0,.5f]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-hud-read"), [.5f,.5f,0], "GTG depth reset and HUD draw order");
        options.Blend = Gfx.Blend.None;
        data[33] = 7;
        Gfx.BeginPass(new PassOpts { Target = target });
        Draw(0, [0,0,0,0]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-bloom-read"), [32/255f*2.6f,48/255f*2.6f,64/255f*2.6f,80/255f*2.6f], "GTG original 13 bloom samples divided by 5");
        Console.WriteLine("PASS GTG GPU alpha, depth, HUD order, bloom gain and XR output transfer");
    }
}
