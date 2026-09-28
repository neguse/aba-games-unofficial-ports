using static Lub;

static class RenderingTests
{
    static byte[] Read(TextureRef texture, string key)
    {
        var readback = Gfx.Readback(key);
        Gfx.ReadTexture(readback, texture, 1, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        Gfx.ReadTexture(readback, texture, null, out var status, out var bytes, out _, out _, out _, out _, out _, out _, out var error);
        if (status != Gfx.ReadbackStatus.Ready) throw new Exception("GPU readback: " + status + " " + error);
        return bytes.ToArray();
    }
    static void Pixel(byte[] pixels, int offset, float[] expected, string name)
    {
        for (int c = 0; c < expected.Length; c++)
            if (Math.Abs(pixels[offset + c] - expected[c] * 255) > 1.5f)
                throw new Exception($"{name} channel {c}: {pixels[offset + c]} != {expected[c] * 255}");
    }
    public static void Run()
    {
        var shader = Gfx.UseShader("tt-render-test", File.ReadAllText("mesh.vs.slang"), File.ReadAllText("mesh.fs.slang"), 1);
        var source = File.ReadAllText("mesh.output.slang");
        var output = Gfx.UseShader("tt-output-test", source, source, 1);
        if (shader == null || output == null) throw new Exception("TT rendering shaders must compile");
        var scene = Gfx.UseTexture("tt-test-scene", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        var linear = Gfx.UseTexture("tt-test-linear", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        var image = Gfx.UseTexture("tt-test-white", 1, 1, Gfx.PixelFormat.Rgba8, [255, 255, 255, 255], 1);
        var mesh = new Mesh("tt-test-quad");
        foreach (var p in new float[][] { [-1,-1], [1,-1], [1,1], [-1,1] })
            mesh.Vertex(p[0], p[1], 0, [.6f, .2f, .1f, .25f]);
        mesh.Quads(0, 4);
        var bindings = new Dictionary<string, object> {
            ["verts"] = Gfx.UseBuffer("tt-test-verts", Gfx.BufferType.Storage, mesh.vertices, 1),
            ["faces"] = Gfx.UseBuffer("tt-test-faces", Gfx.BufferType.Storage, mesh.faces, 1),
            ["image"] = image,
            ["uniforms"] = new Dictionary<string, object> { ["viewport"] = new float[] {16,16,0,0}, ["batch"] = new float[4] }
        };
        float[][] expected = [[.6f,.2f,.1f], [.225f,.2f,.25f], [.25f,.25f,.325f], [.06f,.04f,.03f]];
        for (int mode = 0; mode < 4; mode++)
        {
            var blend = (Gfx.Blend)(mode + 1);
            var data = new List<float>(Transform.Identity());
            data.AddRange([.7f,.6f,.5f,.4f, 1,blend == Gfx.Blend.Additive ? 1 : 0,0,0, 1,1,0,0]);
            bindings["draws"] = Gfx.UseBuffer("tt-test-draws", Gfx.BufferType.Storage, data, mode + 1);
            Gfx.BeginPass(new PassOpts { Target = scene, ClearColor = [.1f,.2f,.3f,1] });
            Gfx.Draw(mesh.count, bindings, new DrawOpts { Shader = shader, Blend = blend, Depth = false, Cull = Gfx.Cull.None });
            Gfx.EndPass();
            var pixels = Read(scene, "tt-test-read-" + mode);
            Pixel(pixels, (8 * 16 + 8) * 4, expected[mode], "Original RGB blend " + blend);
            Gfx.BeginPass(new PassOpts { Target = linear });
            Gfx.Draw(3, new Dictionary<string, object> { ["scene"] = scene },
                new DrawOpts { Shader = output, Blend = Gfx.Blend.None, Depth = false, Cull = Gfx.Cull.None });
            Gfx.EndPass();
            var decoded = Read(linear, "tt-test-output-" + mode);
            var expectedLinear = new float[4];
            for (int c = 0; c < 3; c++)
            {
                float value = pixels[(8 * 16 + 8) * 4 + c] / 255f;
                expectedLinear[c] = value <= .04045f ? value / 12.92f : MathF.Pow((value + .055f) / 1.055f, 2.4f);
            }
            expectedLinear[3] = 1;
            Pixel(decoded, (8 * 16 + 8) * 4, expectedLinear, "XR output " + blend);
        }
        var line = new Mesh("tt-test-line");
        line.Vertex(-.75f, 0, 0, [1,1,1,1]); line.Vertex(.75f, 0, 0, [1,1,1,1]); line.Line(0,1);
        bindings["verts"] = Gfx.UseBuffer("tt-test-line-verts", Gfx.BufferType.Storage, line.vertices, 1);
        bindings["faces"] = Gfx.UseBuffer("tt-test-line-faces", Gfx.BufferType.Storage, line.faces, 1);
        var lineData = new List<float>(Transform.Identity());
        lineData.AddRange([1,1,1,1, 1,1,0,0, 1,1,0,0]);
        bindings["draws"] = Gfx.UseBuffer("tt-test-line-draws", Gfx.BufferType.Storage, lineData, 1);
        Gfx.BeginPass(new PassOpts { Target = scene, ClearColor = [0,0,0,1] });
        Gfx.Draw(line.count, bindings, new DrawOpts { Shader = shader, Blend = Gfx.Blend.Additive, Depth = false, Cull = Gfx.Cull.None });
        Gfx.EndPass();
        var linePixels = Read(scene, "tt-test-line-read");
        Pixel(linePixels, (7 * 16 + 8) * 4, [.5f,.5f,.5f], "Upper half-pixel line coverage");
        Pixel(linePixels, (8 * 16 + 8) * 4, [.5f,.5f,.5f], "Lower half-pixel line coverage");
        Pixel(linePixels, (6 * 16 + 8) * 4, [0,0,0], "Outside line coverage");
        bindings["verts"] = Gfx.UseBuffer("tt-test-verts", Gfx.BufferType.Storage, mesh.vertices, 1);
        bindings["faces"] = Gfx.UseBuffer("tt-test-faces", Gfx.BufferType.Storage, mesh.faces, 1);
        bindings["image"] = Gfx.UseTexture("tt-test-title", 1, 1, Gfx.PixelFormat.Rgba8, [128,64,192,128], 1);
        var titleData = new List<float>(Transform.Identity());
        titleData.AddRange([.5f,.75f,.25f,.5f, 1,1,0,1, 1,1,0,0]);
        bindings["draws"] = Gfx.UseBuffer("tt-test-title-draws", Gfx.BufferType.Storage, titleData, 1);
        Gfx.BeginPass(new PassOpts { Target = scene, ClearColor = [0,0,0,1] });
        Gfx.Draw(mesh.count, bindings, new DrawOpts { Shader = shader, Blend = Gfx.Blend.Additive, Depth = false, Cull = Gfx.Cull.None });
        Gfx.EndPass();
        float alpha = 128f / 255 * .5f;
        Pixel(Read(scene, "tt-test-title-read"), (8 * 16 + 8) * 4,
            [128f/255*.5f*alpha, 64f/255*.75f*alpha, 192f/255*.25f*alpha], "Texture modulation and one alpha multiplication");
        Console.WriteLine("PASS GPU original RGB replacement/alpha/additive/multiply and XR output transfer");
        Console.WriteLine("PASS GPU antialiased line coverage and texture color/alpha modulation");
    }
}
