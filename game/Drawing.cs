using System;
using System.Collections.Generic;

public class DrawVertex
{
    public float x, y, z, w = 1, r, g, b, a;
}
public class DrawPart
{
    public int mode;
    public List<DrawVertex> vertices = new List<DrawVertex>();
}
public class DrawMesh { public List<DrawPart> parts = new List<DrawPart>(); }
public class DrawBatch
{
    public bool depth, blend, cull;
    public List<float> vertices = new List<float>();
}

public static class Drawing
{
    public const int GL_QUADS = 0, GL_TRIANGLES = 1, GL_TRIANGLE_STRIP = 2, GL_LINES = 3, GL_LINE_STRIP = 4;
    public const int GL_DEPTH_TEST = 5, GL_BLEND = 6, GL_CULL_FACE = 7, GL_COMPILE = 8;
    public static List<DrawBatch> batches = new List<DrawBatch>();
    public static float[] clearColor = new float[] { 0, 0, 0, 1 };
    public static bool ortho;
    static bool depth = true, blend, cull = true;
    static float lineWidth = 1;
    static float red = 1, green = 1, blue = 1, alpha = 1;
    static float[] matrix;
    static List<float[]> stack = new List<float[]>();
    static List<DrawMesh> meshes = new List<DrawMesh>();
    static DrawMesh recording;
    static DrawPart part;

    static float[] Identity() { return new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }; }
    static float[] Copy(float[] src)
    {
        var dst = new float[16];
        for (int i = 0; i < 16; i++) dst[i] = src[i];
        return dst;
    }
    static void Multiply(float[] right)
    {
        var result = new float[16];
        for (int i = 0; i < 16; i++) result[i] = 0;
        for (int column = 0; column < 4; column++)
            for (int row = 0; row < 4; row++)
                for (int k = 0; k < 4; k++) result[column * 4 + row] += matrix[k * 4 + row] * right[column * 4 + k];
        matrix = result;
    }
    public static void BeginFrame()
    {
        batches.Clear(); matrix = Identity(); stack.Clear();
        ortho = false;
    }
    public static void glPushMatrix() { stack.Add(Copy(matrix)); }
    public static void glPopMatrix() { matrix = stack[stack.Count - 1]; stack.RemoveAt(stack.Count - 1); }
    public static void LoadIdentity() { matrix = Identity(); }
    public static void glTranslatef(float x, float y, float z)
    {
        for (int i = 0; i < 4; i++) matrix[12 + i] += matrix[i] * x + matrix[4 + i] * y + matrix[8 + i] * z;
    }
    public static void glScalef(float x, float y, float z)
    {
        for (int i = 0; i < 4; i++) { matrix[i] *= x; matrix[4 + i] *= y; matrix[8 + i] *= z; }
    }
    public static void glRotatef(float degrees, float x, float y, float z)
    {
        float length = (float)Math.Sqrt(x * x + y * y + z * z);
        if (length == 0) return;
        x /= length; y /= length; z /= length;
        float c = (float)Math.Cos(degrees * (GameMath.PI / 180)), s = (float)Math.Sin(degrees * (GameMath.PI / 180)), t = 1 - c;
        Multiply(new float[] {
            t*x*x+c, t*x*y+s*z, t*x*z-s*y, 0,
            t*x*y-s*z, t*y*y+c, t*y*z+s*x, 0,
            t*x*z+s*y, t*y*z-s*x, t*z*z+c, 0, 0,0,0,1 });
    }
    public static void Color(float r, float g, float b, float a) { red = r; green = g; blue = b; alpha = a; }
    public static int glGenLists(int count)
    {
        int start = meshes.Count;
        for (int i = 0; i < count; i++) meshes.Add(new DrawMesh());
        return start;
    }
    public static void glNewList(int index, int mode)
    {
        recording = meshes[index]; recording.parts.Clear(); glPushMatrix(); LoadIdentity();
    }
    public static void glEndList() { recording = null; glPopMatrix(); }
    public static void glDeleteLists(int first, int count)
    {
        for (int i = first; i < first + count; i++) meshes[i].parts.Clear();
    }
    static DrawVertex Transform(DrawVertex v)
    {
        return new DrawVertex {
            x = matrix[0]*v.x + matrix[4]*v.y + matrix[8]*v.z + matrix[12],
            y = matrix[1]*v.x + matrix[5]*v.y + matrix[9]*v.z + matrix[13],
            z = matrix[2]*v.x + matrix[6]*v.y + matrix[10]*v.z + matrix[14],
            r = v.r, g = v.g, b = v.b, a = v.a };
    }
    public static void glCallList(int index)
    {
        foreach (var p in meshes[index].parts)
        {
            var transformed = new DrawPart { mode = p.mode };
            foreach (var v in p.vertices) transformed.vertices.Add(Transform(v));
            if (recording != null) recording.parts.Add(transformed);
            else Emit(transformed);
        }
    }
    public static void glBegin(int mode) { part = new DrawPart { mode = mode }; }
    public static void glVertex3f(float x, float y, float z)
    {
        part.vertices.Add(Transform(new DrawVertex { x = x, y = y, z = z, r = red, g = green, b = blue, a = alpha }));
    }
    public static void glEnd()
    {
        if (recording != null) recording.parts.Add(part);
        else Emit(part);
        part = null;
    }
    public static void glEnable(int state) { Set(state, true); }
    public static void glDisable(int state) { Set(state, false); }
    static void Set(int state, bool enabled)
    {
        if (state == GL_DEPTH_TEST) depth = enabled;
        if (state == GL_BLEND) blend = enabled;
        if (state == GL_CULL_FACE) cull = enabled;
    }
    public static void glLineWidth(float width) { lineWidth = width; }
    static DrawVertex Project(DrawVertex v)
    {
        if (ortho) return new DrawVertex { x = v.x / 320 - 1, y = 1 - v.y / 240, z = (1 - v.z) / 2, r = v.r, g = v.g, b = v.b, a = v.a };
        return new DrawVertex { x = v.x, y = v.y * (4f / 3), z = -v.z * (1000f / 999.9f) - 100f / 999.9f,
            w = -v.z, r = v.r, g = v.g, b = v.b, a = v.a };
    }
    static List<float> Batch(bool useCull)
    {
        DrawBatch batch = batches.Count == 0 ? null : batches[batches.Count - 1];
        if (batch == null || batch.depth != depth || batch.blend != blend || batch.cull != useCull)
        {
            batch = new DrawBatch { depth = depth, blend = blend, cull = useCull }; batches.Add(batch);
        }
        return batch.vertices;
    }
    static void Vertex(List<float> buffer, DrawVertex v)
    {
        buffer.Add(v.x); buffer.Add(v.y); buffer.Add(v.z); buffer.Add(v.w);
        buffer.Add(v.r); buffer.Add(v.g); buffer.Add(v.b); buffer.Add(v.a);
    }
    static void Triangle(DrawVertex a, DrawVertex b, DrawVertex c)
    {
        var dst = Batch(cull);
        Vertex(dst, a); Vertex(dst, b); Vertex(dst, c);
    }
    static void Line(DrawVertex a, DrawVertex b)
    {
        if (a.w <= 0 || b.w <= 0) return;
        float dx = (b.x / b.w - a.x / a.w) * 320, dy = (b.y / b.w - a.y / a.w) * 240;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length == 0) return;
        float ox = -dy / length * lineWidth / 640, oy = dx / length * lineWidth / 480;
        var dst = Batch(false);
        var a1 = Offset(a, ox, oy); var a2 = Offset(a, -ox, -oy);
        var b1 = Offset(b, ox, oy); var b2 = Offset(b, -ox, -oy);
        Vertex(dst, a1); Vertex(dst, a2); Vertex(dst, b1);
        Vertex(dst, b1); Vertex(dst, a2); Vertex(dst, b2);
    }
    static DrawVertex Offset(DrawVertex v, float x, float y)
    {
        return new DrawVertex { x = v.x + x * v.w, y = v.y + y * v.w, z = v.z, w = v.w, r = v.r, g = v.g, b = v.b, a = v.a };
    }
    static void Emit(DrawPart p)
    {
        var vertices = new List<DrawVertex>();
        foreach (var v in p.vertices) vertices.Add(Project(v));
        int n = vertices.Count;
        if (p.mode == GL_QUADS)
            for (int i = 0; i + 3 < n; i += 4) { Triangle(vertices[i], vertices[i+1], vertices[i+2]); Triangle(vertices[i], vertices[i+2], vertices[i+3]); }
        else if (p.mode == GL_TRIANGLES)
            for (int i = 0; i + 2 < n; i += 3) Triangle(vertices[i], vertices[i+1], vertices[i+2]);
        else if (p.mode == GL_TRIANGLE_STRIP)
            for (int i = 0; i + 2 < n; i++) Triangle(vertices[i+(i%2)], vertices[i+1-(i%2)], vertices[i+2]);
        else if (p.mode == GL_LINES)
            for (int i = 0; i + 1 < n; i += 2) Line(vertices[i], vertices[i+1]);
        else if (p.mode == GL_LINE_STRIP)
            for (int i = 0; i + 1 < n; i++) Line(vertices[i], vertices[i+1]);
    }
}

public class Screen
{
    public static void setColor(float r, float g, float b) { Drawing.Color(r, g, b, 1); }
    public static void setColorAlpha(float r, float g, float b, float a) { Drawing.Color(r, g, b, a); }
    public static void setClearColor(float r, float g, float b, float a) { Drawing.clearColor = new float[] { r, g, b, a }; }
    public void clear() { Drawing.BeginFrame(); }
    public void viewOrthoFixed() { Drawing.glPushMatrix(); Drawing.LoadIdentity(); Drawing.ortho = true; }
    public void viewPerspective() { Drawing.glPopMatrix(); Drawing.ortho = false; }
}
