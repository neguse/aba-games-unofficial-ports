using static Lub;

static class RenderingReadback
{
    public static byte[] Read(TextureRef texture, string key)
    {
        var readback = Gfx.Readback(key);
        Gfx.ReadTexture(readback, texture, 1, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        Gfx.ReadTexture(readback, texture, null, out var status, out var bytes, out _, out _, out _, out _, out _, out _, out var error);
        if (status != Gfx.ReadbackStatus.Ready) throw new Exception("GPU readback: " + status + " " + error);
        return bytes.ToArray();
    }
    public static void Pixel(byte[] pixels, float[] expected, string name)
    {
        for (int c = 0; c < expected.Length; c++)
            if (System.Math.Abs(pixels[(8 * 16 + 8) * 4 + c] - expected[c] * 255) > 1.5f)
                throw new Exception($"{name} channel {c}: {pixels[(8 * 16 + 8) * 4 + c]} != {expected[c] * 255}");
    }
    public static void Output(TextureRef scene)
    {
        var source = File.ReadAllText("scene.output.slang");
        var shader = Gfx.UseShader("output-test", source, source, 1);
        if (shader == null) throw new Exception("XR output shader must compile");
        var linear = Gfx.UseTexture("linear-test", 16, 16, Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true });
        Gfx.BeginPass(new PassOpts { Target = linear });
        Gfx.Draw(3, new Dictionary<string, object> { ["scene"] = scene },
            new DrawOpts { Shader = shader, Blend = Gfx.Blend.None, Depth = false, Cull = Gfx.Cull.None });
        Gfx.EndPass();
        var encoded = Read(scene, "encoded-test");
        var expected = new float[] { 0, 0, 0, 1 };
        for (int c = 0; c < 3; c++)
        {
            float value = encoded[(8 * 16 + 8) * 4 + c] / 255f;
            expected[c] = value <= .04045f ? value / 12.92f : MathF.Pow((value + .055f) / 1.055f, 2.4f);
        }
        Pixel(Read(linear, "linear-read"), expected, "Completed image to XR linear output");
    }
}
