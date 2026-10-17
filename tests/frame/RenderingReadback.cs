using Lub;

// The tests run with --readback-fixed-latency: a readback requested in frame N is Ready exactly in frame N+2.
// Each step draws in its own frame, and its checks run when the readbacks it requested arrive.
static class RenderingReadback
{
    const int Latency = 2;
    static readonly Queue<Action> steps = new();
    static readonly Queue<(int Frame, Readback Readback, TextureRef Texture, Action<byte[]> Check)> reads = new();
    static int frame;

    public static void Step(Action draw) => steps.Enqueue(draw);
    // Checks run in request order, so a check may use pixels captured by an earlier one.
    public static void Read(TextureRef texture, string key, Action<byte[]> check)
    {
        var readback = Gfx.Readback(key)!;
        Gfx.ReadTexture(readback, texture, frame, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        reads.Enqueue((frame, readback, texture, check));
    }
    // Call once in every frame; true once every step has drawn and passed its checks.
    public static bool Frame()
    {
        while (reads.Count > 0 && reads.Peek().Frame + Latency <= frame)
        {
            var read = reads.Dequeue();
            Gfx.ReadTexture(read.Readback, read.Texture, null, out var status, out var bytes, out _, out _, out _, out _, out int id, out _, out var error);
            if (status != Gfx.ReadbackStatus.Ready || id != read.Frame)
                throw new Exception($"GPU readback {read.Frame} in frame {frame}: {status} {id} {error}");
            read.Check(bytes!.ToArray());
        }
        if (steps.Count > 0) steps.Dequeue()();
        frame++;
        return steps.Count == 0 && reads.Count == 0;
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
        byte[] encoded = [];
        Read(scene, "encoded-test", pixels => encoded = pixels);
        Read(linear, "linear-read", pixels =>
        {
            var expected = new float[] { 0, 0, 0, 1 };
            for (int c = 0; c < 3; c++)
            {
                float value = encoded[(8 * 16 + 8) * 4 + c] / 255f;
                expected[c] = value <= .04045f ? value / 12.92f : MathF.Pow((value + .055f) / 1.055f, 2.4f);
            }
            Pixel(pixels, expected, "Completed image to XR linear output");
        });
    }
}
