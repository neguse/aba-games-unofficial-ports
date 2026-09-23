using System;
using System.Collections.Generic;

public class DrawVertex
{
    public float x, y, z, w = 1, r, g, b, a;
    public bool inheritedColor;
}
public class DrawPart
{
    public int mode;
    public bool savedBlend, useSavedBlend;
    public bool savedAlphaBlend, useSavedAlphaBlend;
    public List<DrawVertex> vertices = new List<DrawVertex>();
}
public class DrawMesh
{
    public List<DrawPart> parts = new List<DrawPart>();
    public bool changesColor;
    public bool changesBlend, finalBlend;
    public bool changesAlphaBlend, finalAlphaBlend;
    public float r, g, b, a;
}
public class DrawBatch
{
    public bool depth, blend, cull;
    public bool alphaBlend;
    public List<float> vertices = new List<float>();
}

public static class Drawing
{
    public const int GL_QUADS = 0, GL_TRIANGLES = 1, GL_TRIANGLE_STRIP = 2, GL_LINES = 3, GL_LINE_STRIP = 4;
    public const int GL_DEPTH_TEST = 5, GL_BLEND = 6, GL_CULL_FACE = 7, GL_COMPILE = 8;
    public const int GL_TRIANGLE_FAN = 9, GL_LINE_LOOP = 10, GL_SRC_ALPHA = 11, GL_ONE = 12, GL_ONE_MINUS_SRC_ALPHA = 13;
    public static List<DrawBatch> batches = new List<DrawBatch>();
    public static float[] clearColor = new float[] { 0, 0, 0, 1 };
    public static bool ortho;
    public static float farPlane = 1000, projectionScale = 1, viewportRatio = 1;
    public static bool recordBlend;
    public static bool premultiplyAdditive;
    static bool alphaBlend;
    static bool depth = true, blend, cull = true;
    static float lineWidth = 1;
    public static float viewportWidth = 640, viewportHeight = 480;
    static float red = 1, green = 1, blue = 1, alpha = 1;
    static float savedRed, savedGreen, savedBlue, savedAlpha;
    static bool savedBlend, savedAlphaBlend;
    static float[] matrix;
    static List<float[]> stack = new List<float[]>();
    static List<DrawMesh> meshes = new List<DrawMesh>();
    static DrawMesh recording;
    static DrawPart part;
    static List<DrawVertex> projected = new List<DrawVertex>();

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
    public static void glMultMatrix(float[] values) { Multiply(values); }
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
    public static void Color(float r, float g, float b, float a)
    {
        red = r; green = g; blue = b; alpha = a;
        if (recording != null && recordBlend)
        {
            recording.changesColor = true;
            recording.r = r; recording.g = g; recording.b = b; recording.a = a;
        }
    }
    public static int glGenLists(int count)
    {
        int start = meshes.Count;
        for (int i = 0; i < count; i++) meshes.Add(new DrawMesh());
        return start;
    }
    public static void glNewList(int index, int mode)
    {
        recording = meshes[index]; recording.parts.Clear(); glPushMatrix(); LoadIdentity();
        recording.changesColor = false;
        recording.changesBlend = false;
        recording.changesAlphaBlend = false;
        savedAlphaBlend = alphaBlend;
        savedBlend = blend;
        savedRed = red; savedGreen = green; savedBlue = blue; savedAlpha = alpha;
    }
    public static void glEndList()
    {
        recording = null; glPopMatrix();
        if (recordBlend) { Color(savedRed, savedGreen, savedBlue, savedAlpha); blend = savedBlend; alphaBlend = savedAlphaBlend; }
    }
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
            r = v.r, g = v.g, b = v.b, a = v.a, inheritedColor = v.inheritedColor };
    }
    public static void glCallList(int index)
    {
        foreach (var p in meshes[index].parts)
        {
            if (recording != null)
            {
                var transformed = new DrawPart { mode = p.mode, savedAlphaBlend = p.savedAlphaBlend, useSavedAlphaBlend = p.useSavedAlphaBlend };
                foreach (var v in p.vertices) transformed.vertices.Add(Transform(v));
                recording.parts.Add(transformed);
            }
            else Emit(p, true);
        }
        var mesh = meshes[index];
        if (recordBlend && mesh.changesColor) Color(mesh.r, mesh.g, mesh.b, mesh.a);
        if (recordBlend && mesh.changesBlend) glEnableBlend(mesh.finalBlend);
        if (recordBlend && mesh.changesAlphaBlend) glBlendFunc(GL_SRC_ALPHA, mesh.finalAlphaBlend ? GL_ONE_MINUS_SRC_ALPHA : GL_ONE);
    }
    public static void glBegin(int mode) { part = new DrawPart { mode = mode, savedBlend = blend, savedAlphaBlend = alphaBlend,
        useSavedAlphaBlend = recording != null && recordBlend && recording.changesAlphaBlend,
        useSavedBlend = recording != null && recordBlend && recording.changesBlend }; }
    public static void glBlendFunc(int source, int destination)
    {
        alphaBlend = destination == GL_ONE_MINUS_SRC_ALPHA;
        if (recording != null && recordBlend) { recording.changesAlphaBlend = true; recording.finalAlphaBlend = alphaBlend; }
    }
    public static void glVertex3f(float x, float y, float z)
    {
        part.vertices.Add(Transform(new DrawVertex { x = x, y = y, z = z, r = red, g = green, b = blue, a = alpha,
            inheritedColor = recordBlend && recording != null && !recording.changesColor }));
    }
    public static void glVertex2f(float x, float y) { glVertex3f(x, y, 0); }
    public static void glEnd()
    {
        if (recording != null) recording.parts.Add(part);
        else Emit(part, false);
        part = null;
    }
    public static void glEnable(int state) { Set(state, true); }
    public static void glDisable(int state) { Set(state, false); }
    static void Set(int state, bool enabled)
    {
        if (state == GL_DEPTH_TEST) depth = enabled;
        if (state == GL_BLEND) glEnableBlend(enabled);
        if (state == GL_CULL_FACE) cull = enabled;
    }
    static void glEnableBlend(bool enabled)
    {
        blend = enabled;
        if (recording != null && recordBlend) { recording.changesBlend = true; recording.finalBlend = enabled; }
    }
    public static void glLineWidth(float width) { lineWidth = width; }
    static void Project(DrawVertex v, DrawVertex result, bool transform)
    {
        float x = v.x, y = v.y, z = v.z;
        if (transform)
        {
            x = matrix[0]*v.x + matrix[4]*v.y + matrix[8]*v.z + matrix[12];
            y = matrix[1]*v.x + matrix[5]*v.y + matrix[9]*v.z + matrix[13];
            z = matrix[2]*v.x + matrix[6]*v.y + matrix[10]*v.z + matrix[14];
        }
        result.x = ortho ? x / 320 - 1 : x * projectionScale;
        result.y = ortho ? 1 - y / 240 : y * (4f / 3) * projectionScale;
        result.z = ortho ? (1 - z) / 2 : -z * (farPlane / (farPlane - 0.1f)) - (farPlane * 0.1f) / (farPlane - 0.1f);
        result.w = ortho ? 1 : -z;
        result.x = result.x * viewportRatio + (viewportRatio - 1) * result.w;
        result.r = v.inheritedColor ? red : v.r; result.g = v.inheritedColor ? green : v.g;
        result.b = v.inheritedColor ? blue : v.b; result.a = v.inheritedColor ? alpha : v.a;
    }
    static List<float> Batch(bool useCull)
    {
        DrawBatch batch = batches.Count == 0 ? null : batches[batches.Count - 1];
        if (batch == null || batch.depth != depth || batch.blend != blend || batch.cull != useCull || batch.alphaBlend != alphaBlend)
        {
            batch = new DrawBatch { depth = depth, blend = blend, cull = useCull, alphaBlend = alphaBlend }; batches.Add(batch);
        }
        return batch.vertices;
    }
    static void Vertex(List<float> buffer, DrawVertex v)
    {
        buffer.Add(v.x); buffer.Add(v.y); buffer.Add(v.z); buffer.Add(v.w);
        float opacity = premultiplyAdditive && blend && !alphaBlend ? v.a : 1;
        buffer.Add(v.r * opacity); buffer.Add(v.g * opacity); buffer.Add(v.b * opacity); buffer.Add(v.a);
    }
    static void Triangle(DrawVertex a, DrawVertex b, DrawVertex c)
    {
        var dst = Batch(cull);
        Vertex(dst, a); Vertex(dst, b); Vertex(dst, c);
    }
    static void Line(DrawVertex a, DrawVertex b)
    {
        if (a.w <= 0 || b.w <= 0) return;
        float dx = (b.x / b.w - a.x / a.w) * viewportWidth / 2, dy = (b.y / b.w - a.y / a.w) * viewportHeight / 2;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length == 0) return;
        float ox = -dy / length * lineWidth / viewportWidth, oy = dx / length * lineWidth / viewportHeight;
        var dst = Batch(false);
        Offset(dst, a, ox, oy); Offset(dst, a, -ox, -oy); Offset(dst, b, ox, oy);
        Offset(dst, b, ox, oy); Offset(dst, a, -ox, -oy); Offset(dst, b, -ox, -oy);
    }
    static void Offset(List<float> buffer, DrawVertex v, float x, float y)
    {
        buffer.Add(v.x + x * v.w); buffer.Add(v.y + y * v.w); buffer.Add(v.z); buffer.Add(v.w);
        float opacity = premultiplyAdditive && blend && !alphaBlend ? v.a : 1;
        buffer.Add(v.r * opacity); buffer.Add(v.g * opacity); buffer.Add(v.b * opacity); buffer.Add(v.a);
    }
    static void Emit(DrawPart p, bool transform)
    {
        bool previousBlend = blend, previousAlphaBlend = alphaBlend;
        if (p.useSavedAlphaBlend) alphaBlend = p.savedAlphaBlend;
        if (p.useSavedBlend) blend = p.savedBlend;
        int n = p.vertices.Count;
        while (projected.Count < n) projected.Add(new DrawVertex());
        for (int i = 0; i < n; i++) Project(p.vertices[i], projected[i], transform);
        var vertices = projected;
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
        else if (p.mode == GL_LINE_LOOP && n > 1)
            for (int i = 0; i < n; i++) Line(vertices[i], vertices[(i+1)%n]);
        else if (p.mode == GL_TRIANGLE_FAN)
            for (int i = 1; i + 1 < n; i++) Triangle(vertices[0], vertices[i], vertices[i+1]);
        blend = previousBlend; alphaBlend = previousAlphaBlend;
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
