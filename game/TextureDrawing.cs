using System.Collections.Generic;
using static Lub;

public class DrawImage
{
    public string key;
    public int width, height, atlasHeight, levels;
    public List<int> pixels;
}

public static class TextureDrawing
{
    static DrawImage white = new DrawImage { key = "title-white", width = 1, height = 1,
        atlasHeight = 1, levels = 1, pixels = new List<int> { 255, 255, 255, 255 } };
    public static Dictionary<string, object> Bindings(BufferRef vertices, DrawBatch batch, int index, int version)
    {
        DrawImage image = batch == null || batch.image == null ? white : batch.image;
        float[] tint = batch == null || batch.image == null ? new float[] { 1, 1, 1, 1 } : batch.tint;
        var texture = Gfx.UseTexture(image.key, image.width, image.atlasHeight, Gfx.PixelFormat.Rgba8,
            Gfx.LookupTexture(image.key) == null ? image.pixels : null, 1,
            new TextureOpts { Filter = Gfx.Filter.Linear, Wrap = Gfx.Wrap.Repeat });
        var parameters = Gfx.UseBuffer("title-parameters" + index.ToString(), Gfx.BufferType.Storage,
            new List<float> { tint[0], tint[1], tint[2], tint[3], image.width, image.height, image.levels - 1, Drawing.premultiplyAdditive && batch != null && batch.blend && !batch.alphaBlend && !batch.multiply ? 1 : 0 }, version);
        return new Dictionary<string, object> { ["verts"] = vertices, ["titleImage"] = texture, ["titleParameters"] = parameters };
    }
}
