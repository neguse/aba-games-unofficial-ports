using static Lub;

static class RenderingTests
{
    public static void Run()
    {
        var shader = Gfx.UseShader("gtg-render-test", File.ReadAllText("game.vs.slang"), File.ReadAllText("game.fs.slang"), 1);
        var layerShader = Gfx.UseShader("gtg-layer-test", File.ReadAllText("game.vs.slang"), "#define GTG_ALPHA_TARGET\n" + File.ReadAllText("game.fs.slang"), 1);
        if (shader == null || layerShader == null) throw new Exception("GTG shaders must compile");
        var target = Gfx.UseTexture("gtg-scene-test", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        var alpha = Gfx.UseTexture("gtg-alpha-test", 16, 16, Gfx.PixelFormat.R8, null, 1, new TextureOpts { Target = true });
        var depth = Gfx.UseTexture("gtg-depth-test", 16, 16, Gfx.PixelFormat.Depth24Stencil8, null, 1, new TextureOpts { Target = true });
        var vertices = new List<float>();
        foreach (var p in new float[][] { [-1,-1], [3,-1], [-1,3] }) vertices.AddRange([p[0],p[1],0,1, 1,1,1,1]);
        var data = new List<float>(new float[144]);
        foreach (int offset in new[] { 0, 16 }) for (int i = 0; i < 4; i++) data[offset+i*5] = 1;
        data[33] = 5; data[135] = 1; data[139] = 1;
        var bindings = new Dictionary<string, object> {
            ["vertices"] = Gfx.UseBuffer("gtg-test-vertices", Gfx.BufferType.Storage, vertices, 1),
            ["eye"] = Gfx.UseBuffer("gtg-test-eye", Gfx.BufferType.Storage, new List<float>(Matrix.Identity.M), 1),
            ["surface"] = Gfx.UseTexture("gtg-test-source", 1, 1, Gfx.PixelFormat.Rgba8, [32,48,64,80], 1),
            ["surfaceAlpha"] = Gfx.UseTexture("gtg-test-alpha-source", 1, 1, Gfx.PixelFormat.R8, [40], 1)
        };
        var options = new DrawOpts { Shader = layerShader, Depth = true, DepthWrite = true, Blend = Gfx.Blend.Alpha, Cull = Gfx.Cull.None };
        int version = 0;
        void Draw(float z, float[] color)
        {
            data[134] = z;
            for (int c = 0; c < 4; c++) data[140+c] = color[c];
            bindings["parameters"] = Gfx.UseBuffer("gtg-test-parameters", Gfx.BufferType.Storage, data, ++version);
            Gfx.Draw(3, bindings, options);
        }
        Gfx.BeginPass(new PassOpts { Targets = [target, alpha], DepthTarget = depth, ClearDepth = 1, ClearColor = [0,0,0,0] });
        Draw(.1f, [1,1,1,0]);
        Draw(.2f, [.6f,.2f,.1f,.25f]);
        Draw(.8f, [0,1,0,1]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-depth-read"), [.15f,.05f,.025f,.25f], "GTG rear occlusion and transparent fragment discard");
        RenderingReadback.Pixel(RenderingReadback.Read(alpha, "gtg-alpha-read"), [.0625f], "GTG legacy alpha in R8 target");
        RenderingReadback.Output(target);
        options.Depth = false; options.DepthWrite = false;
        Gfx.BeginPass(new PassOpts { Targets = [target, alpha], ClearColor = [0,0,0,0] });
        Draw(0, [.6f,.2f,.1f,.25f]);
        Draw(0, [.2f,.8f,.4f,.5f]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-overlap-read"), [.175f,.425f,.2125f,.625f], "GTG overlapping RGB");
        RenderingReadback.Pixel(RenderingReadback.Read(alpha, "gtg-overlap-alpha-read"), [.28125f], "GTG overlapping legacy alpha");
        options.Shader = shader;
        options.Depth = true; options.DepthWrite = true;
        Gfx.BeginPass(new PassOpts { Target = target, DepthTarget = depth, ClearDepth = 1, ClearColor = [0,0,0,0] });
        Draw(.8f, [0,1,0,1]);
        options.Depth = false; options.DepthWrite = false;
        Draw(.9f, [1,0,0,.5f]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-hud-read"), [.5f,.5f,0], "GTG depth reset and HUD draw order");
        data[33] = 6;
        Gfx.BeginPass(new PassOpts { Target = target, ClearColor = [.1f,.2f,.3f,1] });
        Draw(0, [0,0,0,0]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-composite-read"), [32/255f*40/255f+.1f*(1-40/255f),48/255f*40/255f+.2f*(1-40/255f),64/255f*40/255f+.3f*(1-40/255f)], "GTG edge composition uses separate alpha");
        options.Blend = Gfx.Blend.None;
        data[33] = 7;
        Gfx.BeginPass(new PassOpts { Target = target });
        Draw(0, [0,0,0,0]);
        Gfx.EndPass();
        RenderingReadback.Pixel(RenderingReadback.Read(target, "gtg-bloom-read"), [32/255f*2.6f,48/255f*2.6f,64/255f*2.6f,40/255f*2.6f], "GTG original 13 bloom samples divided by 5");
        Console.WriteLine("PASS GTG GPU MRT alpha, depth, HUD order, bloom gain and XR output transfer");
    }
}
