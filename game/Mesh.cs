using System.Collections.Generic;
using Lub;

public class DrawImage
{
    public string key, path;
    public int width, height, atlasHeight, levels;
    public List<int> pixels;
    public TextureRef texture;

    // A file-backed image stays null until its PNG is available.
    public TextureRef Use(TextureOpts options)
    {
        if (path == null)
            return texture = Gfx.UseTexture(key, width, atlasHeight, Gfx.PixelFormat.Rgba8, pixels,
                texture == null ? (int?)null : texture.Version, options);
        Png.Load(path, out var bytes, out _, out _, out _, out _, out int version, out _, out _);
        if (bytes != null)
            texture = Gfx.UseTextureBytes(key, width, atlasHeight, Gfx.PixelFormat.Rgba8, bytes, version, options);
        return texture;
    }
}

// 原作の glBlendFunc(src, dst) の式。
// GL は同じ係数を alpha にも掛ける (alpha では DST_COLOR は dst の alpha になる)。
public static class GlBlend
{
    public static readonly Blend SrcAlphaOne = new Blend
    {
        Src = Gfx.BlendFactor.SrcAlpha, Dst = Gfx.BlendFactor.One, Op = Gfx.BlendOp.Add,
        SrcAlpha = Gfx.BlendFactor.SrcAlpha, DstAlpha = Gfx.BlendFactor.One, OpAlpha = Gfx.BlendOp.Add,
    };
    public static readonly Blend SrcAlphaOneMinusSrcAlpha = new Blend
    {
        Src = Gfx.BlendFactor.SrcAlpha, Dst = Gfx.BlendFactor.OneMinusSrcAlpha, Op = Gfx.BlendOp.Add,
        SrcAlpha = Gfx.BlendFactor.SrcAlpha, DstAlpha = Gfx.BlendFactor.OneMinusSrcAlpha, OpAlpha = Gfx.BlendOp.Add,
    };
    public static readonly Blend OneOne = new Blend
    {
        Src = Gfx.BlendFactor.One, Dst = Gfx.BlendFactor.One, Op = Gfx.BlendOp.Add,
        SrcAlpha = Gfx.BlendFactor.One, DstAlpha = Gfx.BlendFactor.One, OpAlpha = Gfx.BlendOp.Add,
    };
    public static readonly Blend DstColorZero = new Blend
    {
        Src = Gfx.BlendFactor.Dst, Dst = Gfx.BlendFactor.Zero, Op = Gfx.BlendOp.Add,
        SrcAlpha = Gfx.BlendFactor.DstAlpha, DstAlpha = Gfx.BlendFactor.Zero, OpAlpha = Gfx.BlendOp.Add,
    };
}

// 原作の display list の一続きの区間。
// hasBlend の区間は、記録したときの glBlendFunc (blend、null は blend なし) で描く。
// 無い区間は描く側の blend を使う。
// material はゲームが区間ごとに持つ値 (cull など)。
public class MeshRange
{
    public int first, count, material;
    public bool hasBlend;
    public Blend blend;
}

public class Mesh
{
    static DrawImage white = new DrawImage { key = "mesh-white", width = 1, height = 1, atlasHeight = 1, levels = 1, pixels = new List<int> { 255, 255, 255, 255 } };
    public List<float> vertices = new List<float>();
    public List<float> faces = new List<float>();
    public List<MeshRange> ranges = new List<MeshRange>();
    public string key;
    BufferRef vertexBuffer, faceBuffer;
    public Mesh(string key) { this.key = key; }
    public void Clear()
    {
        vertices.Clear(); faces.Clear(); ranges.Clear();
        vertexBuffer = null; faceBuffer = null;
    }
    public int vertexCount { get { return vertices.Count / 8; } }
    public int count { get { return faces.Count / 4 * 3; } }
    public void Vertex(float x, float y, float z, float[] color, float[] transform = null, bool inheritColor = false)
    {
        if (transform != null)
        {
            float px = x, py = y, pz = z;
            x = transform[0] * px + transform[4] * py + transform[8] * pz + transform[12];
            y = transform[1] * px + transform[5] * py + transform[9] * pz + transform[13];
            z = transform[2] * px + transform[6] * py + transform[10] * pz + transform[14];
        }
        vertices.Add(x); vertices.Add(y); vertices.Add(z); vertices.Add(color == null || inheritColor ? 1 : 0);
        for (int i = 0; i < 4; i++) vertices.Add(color == null ? 1 : color[i]);
        vertexBuffer = null;
    }
    public void Append(Mesh source, float[] model)
    {
        int baseVertex = vertexCount, baseFace = count;
        for (int i = 0; i < source.vertices.Count; i += 8)
            Vertex(source.vertices[i], source.vertices[i + 1], source.vertices[i + 2],
                new float[] { source.vertices[i + 4], source.vertices[i + 5], source.vertices[i + 6], source.vertices[i + 7] }, model, source.vertices[i + 3] > 0);
        for (int i = 0; i < source.faces.Count; i += 4)
        {
            faces.Add(source.faces[i] + baseVertex); faces.Add(source.faces[i + 1] + baseVertex);
            faces.Add(source.faces[i + 2] + (source.faces[i + 3] == 1 ? 0 : baseVertex)); faces.Add(source.faces[i + 3]);
        }
        foreach (MeshRange range in source.ranges)
            ranges.Add(new MeshRange { first = baseFace + range.first, count = range.count, material = range.material, hasBlend = range.hasBlend, blend = range.blend });
        faceBuffer = null;
    }
    public void Triangle(int a, int b, int c)
    {
        faces.Add(a); faces.Add(b); faces.Add(c); faces.Add(0);
        faceBuffer = null;
    }
    public void Fan(int first, int count)
    {
        for (int i = 1; i < count - 1; i++) Triangle(first, first + i, first + i + 1);
    }
    public void Quads(int first, int count)
    {
        for (int i = first; i + 3 < first + count; i += 4)
        {
            Triangle(i, i + 1, i + 2); Triangle(i, i + 2, i + 3);
        }
    }
    public void Line(int a, int b)
    {
        faces.Add(a); faces.Add(b); faces.Add(0); faces.Add(1);
        faces.Add(a); faces.Add(b); faces.Add(3); faces.Add(1);
        faceBuffer = null;
    }
    public void LineStrip(int first, int count, bool loop = false)
    {
        for (int i = 0; i < count - 1; i++) Line(first + i, first + i + 1);
        if (loop && count > 1) Line(first + count - 1, first);
    }
    public void AddRange(int first, int material)
    {
        if (count == first) return;
        var last = ranges.Count > 0 ? ranges[ranges.Count - 1] : null;
        if (last != null && last.material == material && !last.hasBlend)
            last.count += count - first;
        else ranges.Add(new MeshRange { first = first, count = count - first, material = material });
    }
    // first からの区間を、記録したときの blend で描く区間にする。
    public void AddBlendRange(int first, Blend blend)
    {
        if (count == first) return;
        var last = ranges.Count > 0 ? ranges[ranges.Count - 1] : null;
        if (last != null && last.hasBlend && last.blend == blend)
            last.count += count - first;
        else ranges.Add(new MeshRange { first = first, count = count - first, hasBlend = true, blend = blend });
    }
    public Bindings Bindings(float[] model, float[] color = null, float width = 1, int first = 0, DrawImage image = null, float viewportWidth = 640, float viewportHeight = 480)
    {
        vertexBuffer = Gfx.UseBuffer(key + "-vertices", Gfx.BufferType.Storage, vertices,
            vertexBuffer == null ? (int?)null : vertexBuffer.Version);
        faceBuffer = Gfx.UseBuffer(key + "-faces", Gfx.BufferType.Storage, faces,
            faceBuffer == null ? (int?)null : faceBuffer.Version);
        var options = new TextureOpts { Filter = Gfx.Filter.Linear, Wrap = Gfx.Wrap.Repeat };
        var source = image == null ? white : image;
        var texture = source.Use(options);
        if (texture == null) texture = white.Use(options);
        var bindings = new Bindings();
        bindings.Set("image", texture);
        bindings.Set("verts", vertexBuffer);
        bindings.Set("faces", faceBuffer);
        bindings.Set("model", model);
        bindings.Set("tint", color == null ? new float[] { 1, 1, 1, 1 } : color);
        bindings.Set("options", new float[] { width, 0, first, image == null ? 0 : 1 });
        bindings.Set("viewport", new float[] { viewportWidth, viewportHeight, 0, 0 });
        bindings.Set("imageInfo", new float[] { source.width, source.height, source.levels - 1, 0 });
        return bindings;
    }
}
