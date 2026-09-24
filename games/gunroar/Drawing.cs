using System;
using System.Collections.Generic;
using static Lub;

public class DrawGeometry
{
    public List<float> vertices;
    public int first, count, epoch;
}
public class DrawPart
{
    public DrawGeometry geometry;
    public int mode;
    public bool savedBlend, useSavedBlend, savedAlphaBlend, useSavedAlphaBlend;
    public DrawImage image;
    public bool multiply;
    public float[] tint, inheritedColor;
}
public class DrawMesh
{
    public List<DrawPart> parts = new List<DrawPart>();
    public bool changesColor, changesBlend, changesAlphaBlend, finalBlend, finalAlphaBlend;
    public float r, g, b, a;
}
public class DrawCommand
{
    public DrawPart part;
    public float[] model, color;
    public bool ortho;
    public float scale, far, ratio, width, height, lineWidth;
}
public class DrawBatch
{
    public List<DrawCommand> commands = new List<DrawCommand>();
    public int count;
    public bool depth, blend, cull, alphaBlend, multiply;
    public DrawImage image;
    public float[] tint;
}

public static class Drawing
{
    public const int GL_QUADS = 0, GL_TRIANGLES = 1, GL_TRIANGLE_STRIP = 2, GL_LINES = 3, GL_LINE_STRIP = 4;
    public const int GL_DEPTH_TEST = 5, GL_BLEND = 6, GL_CULL_FACE = 7, GL_COMPILE = 8;
    public const int GL_TRIANGLE_FAN = 9, GL_LINE_LOOP = 10, GL_SRC_ALPHA = 11, GL_ONE = 12, GL_ONE_MINUS_SRC_ALPHA = 13;
    public static List<DrawBatch> batches = new List<DrawBatch>();
    public static float[] clearColor = new float[] { 0, 0, 0, 1 };
    public static bool ortho, recordBlend, premultiplyAdditive;
    public static float farPlane = 1000, projectionScale = 1, viewportRatio = 1;
    public static float viewportWidth = 640, viewportHeight = 480;
    static bool alphaBlend, depth = true, blend, cull = true;
    static float lineWidth = 1, red = 1, green = 1, blue = 1, alpha = 1;
    static float savedRed, savedGreen, savedBlue, savedAlpha;
    static bool savedBlend, savedAlphaBlend;
    static float[] matrix;
    static List<float[]> stack = new List<float[]>();
    static List<DrawMesh> meshes = new List<DrawMesh>();
    static DrawMesh recording;
    static DrawPart part;
    static List<float> dynamicVertices = new List<float>();
    static int dynamicVertexCount;
    static List<float> staticVertices = new List<float>();
    static bool staticDirty = true;
    static int staticEpoch, version;
    static BufferRef staticBuffer;
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
        batches.Clear(); dynamicVertexCount = 0; matrix = Identity(); stack.Clear();
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
        staticDirty = true; recording = meshes[index]; recording.parts.Clear(); glPushMatrix(); LoadIdentity();
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
        staticDirty = true;
        for (int i = first; i < first + count; i++) meshes[i].parts.Clear();
    }
    // Bake list-local transforms once; per-frame placement is evaluated by the GPU.
    static DrawGeometry TransformGeometry(DrawGeometry source)
    {
        var vertices = new List<float>();
        for (int i = 0; i < source.vertices.Count; i += 8)
        {
            float x = source.vertices[i], y = source.vertices[i + 1], z = source.vertices[i + 2];
            vertices.Add(matrix[0] * x + matrix[4] * y + matrix[8] * z + matrix[12]);
            vertices.Add(matrix[1] * x + matrix[5] * y + matrix[9] * z + matrix[13]);
            vertices.Add(matrix[2] * x + matrix[6] * y + matrix[10] * z + matrix[14]);
            for (int j = 3; j < 8; j++) vertices.Add(source.vertices[i + j]);
        }
        return new DrawGeometry { vertices = vertices, count = source.count };
    }
    public static void glCallList(int index)
    {
        DrawMesh mesh = meshes[index];
        float[] model = Copy(matrix);
        foreach (DrawPart p in mesh.parts)
        {
            if (recording != null)
            {
                var copy = new DrawPart { geometry = TransformGeometry(p.geometry), mode = p.mode,
                    savedBlend = p.savedBlend, useSavedBlend = p.useSavedBlend,
                    savedAlphaBlend = p.savedAlphaBlend, useSavedAlphaBlend = p.useSavedAlphaBlend,
                    image = p.image, multiply = p.multiply, tint = p.tint, inheritedColor = p.inheritedColor };
                if (!copy.useSavedBlend && recording.changesBlend) { copy.useSavedBlend = true; copy.savedBlend = blend; }
                if (!copy.useSavedAlphaBlend && recording.changesAlphaBlend) { copy.useSavedAlphaBlend = true; copy.savedAlphaBlend = alphaBlend; }
                if (copy.inheritedColor == null && recording.changesColor)
                    copy.inheritedColor = new float[] { red, green, blue, alpha };
                recording.parts.Add(copy);
            }
            else Submit(p, model);
        }
        if (recordBlend && mesh.changesColor) Color(mesh.r, mesh.g, mesh.b, mesh.a);
        if (recordBlend && mesh.changesBlend) glEnableBlend(mesh.finalBlend);
        if (recordBlend && mesh.changesAlphaBlend) glBlendFunc(GL_SRC_ALPHA, mesh.finalAlphaBlend ? GL_ONE_MINUS_SRC_ALPHA : GL_ONE);
    }
    public static void glBegin(int mode)
    {
        var geometry = new DrawGeometry();
        if (recording != null) geometry.vertices = new List<float>();
        else geometry.first = dynamicVertexCount;
        part = new DrawPart { geometry = geometry, mode = mode, savedBlend = blend, savedAlphaBlend = alphaBlend,
            useSavedBlend = recording != null && recordBlend && recording.changesBlend,
            useSavedAlphaBlend = recording != null && recordBlend && recording.changesAlphaBlend };
    }
    public static void glVertex3f(float x, float y, float z)
    {
        float inherit = 0;
        if (recordBlend && recording != null && !recording.changesColor) inherit = 1;
        if (recording != null)
        {
            var vertices = part.geometry.vertices;
            vertices.Add(x); vertices.Add(y); vertices.Add(z); vertices.Add(inherit);
            vertices.Add(red); vertices.Add(green); vertices.Add(blue); vertices.Add(alpha);
        }
        else
        {
            int offset = dynamicVertexCount * 8;
            if (offset == dynamicVertices.Count)
                for (int i = 0; i < 8; i++) dynamicVertices.Add(0);
            dynamicVertices[offset] = x; dynamicVertices[offset + 1] = y; dynamicVertices[offset + 2] = z;
            dynamicVertices[offset + 3] = 0;
            dynamicVertices[offset + 4] = red; dynamicVertices[offset + 5] = green;
            dynamicVertices[offset + 6] = blue; dynamicVertices[offset + 7] = alpha;
            dynamicVertexCount++;
        }
        part.geometry.count++;
    }
    public static void glVertex2f(float x, float y) { glVertex3f(x, y, 0); }
    public static void glEnd()
    {
        if (recording != null)
        {
            part.geometry = TransformGeometry(part.geometry);
            recording.parts.Add(part);
        }
        else Submit(part, Copy(matrix));
        part = null;
    }
    public static void glBlendFunc(int source, int destination)
    {
        alphaBlend = destination == GL_ONE_MINUS_SRC_ALPHA;
        if (recording != null && recordBlend) { recording.changesAlphaBlend = true; recording.finalAlphaBlend = alphaBlend; }
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
    static int VertexCount(DrawPart p)
    {
        int n = p.geometry.count;
        if (p.mode == GL_QUADS) return n / 4 * 6;
        if (p.mode == GL_TRIANGLES) return n / 3 * 3;
        if (p.mode == GL_TRIANGLE_STRIP || p.mode == GL_TRIANGLE_FAN) return Math.Max(0, n - 2) * 3;
        if (p.mode == GL_LINES) return n / 2 * 6;
        if (p.mode == GL_LINE_STRIP) return Math.Max(0, n - 1) * 6;
        if (p.mode == GL_LINE_LOOP && n > 1) return n * 6;
        return 0;
    }
    static void Submit(DrawPart p, float[] model)
    {
        int count = VertexCount(p);
        if (count == 0) return;
        bool useBlend = blend, useAlpha = alphaBlend, useCull = cull;
        if (p.useSavedBlend) useBlend = p.savedBlend;
        if (p.useSavedAlphaBlend) useAlpha = p.savedAlphaBlend;
        if (p.mode == GL_LINES || p.mode == GL_LINE_STRIP || p.mode == GL_LINE_LOOP) useCull = false;
        DrawBatch batch = null;
        if (batches.Count > 0) batch = batches[batches.Count - 1];
        if (batch == null || p.image != null || batch.image != null || batch.depth != depth ||
            batch.blend != useBlend || batch.alphaBlend != useAlpha || batch.cull != useCull)
        {
            batch = new DrawBatch { depth = depth, blend = useBlend, alphaBlend = useAlpha, cull = useCull,
                image = p.image, multiply = p.multiply, tint = p.tint };
            batches.Add(batch);
        }
        float[] color = p.inheritedColor;
        if (color == null) color = new float[] { red, green, blue, alpha };
        batch.commands.Add(new DrawCommand { part = p, model = model, color = color, ortho = ortho,
            scale = projectionScale, far = farPlane, ratio = viewportRatio,
            width = viewportWidth, height = viewportHeight, lineWidth = lineWidth });
        batch.count += count;
    }
    public static void Image(DrawImage image, float x, float y, float width, float height,
        float r, float g, float b, float a, bool multiply = false)
    {
        glBegin(GL_QUADS);
        part.image = image; part.multiply = multiply; part.tint = new float[] { r, g, b, a };
        glVertex2f(x, y); glVertex2f(x + width, y);
        glVertex2f(x + width, y + height); glVertex2f(x, y + height);
        glEnd();
    }
    static void Retain(DrawGeometry geometry)
    {
        if (geometry.vertices == null || geometry.epoch == staticEpoch) return;
        geometry.epoch = staticEpoch;
        geometry.first = staticVertices.Count / 8;
        foreach (float value in geometry.vertices) staticVertices.Add(value);
    }
    public static void Render(ShaderRef shader)
    {
        version++;
        if (staticDirty)
        {
            staticEpoch++;
            staticVertices.Clear();
            foreach (DrawMesh mesh in meshes)
                foreach (DrawPart p in mesh.parts) Retain(p.geometry);
            foreach (DrawBatch batch in batches)
                foreach (DrawCommand command in batch.commands) Retain(command.part.geometry);
            if (staticVertices.Count == 0) for (int i = 0; i < 8; i++) staticVertices.Add(0);
            staticBuffer = Gfx.UseBuffer("static-geometry", Gfx.BufferType.Storage, staticVertices, staticEpoch);
            staticDirty = false;
        }
        else staticBuffer = Gfx.UseBufferEmpty("static-geometry", Gfx.BufferType.Storage, staticVertices.Count, staticEpoch);
        if (dynamicVertices.Count == 0) for (int i = 0; i < 8; i++) dynamicVertices.Add(0);
        var dynamicBuffer = Gfx.UseBuffer("dynamic-geometry", Gfx.BufferType.Storage, dynamicVertices, version);
        int index = 0;
        foreach (DrawBatch batch in batches)
        {
            var commands = new List<float>();
            int first = 0;
            foreach (DrawCommand command in batch.commands)
            {
                DrawPart p = command.part;
                commands.Add(first); commands.Add(p.geometry.first); commands.Add(p.geometry.count); commands.Add(p.mode);
                commands.Add(p.geometry.vertices != null ? 1 : 0); commands.Add(command.lineWidth);
                commands.Add(premultiplyAdditive && batch.blend && !batch.alphaBlend ? 1 : 0); commands.Add(p.image != null ? 1 : 0);
                foreach (float value in command.model) commands.Add(value);
                commands.Add(command.scale); commands.Add(command.far); commands.Add(command.ortho ? 1 : 0); commands.Add(command.ratio);
                commands.Add(command.width); commands.Add(command.height); commands.Add(0); commands.Add(0);
                foreach (float value in command.color) commands.Add(value);
                first += VertexCount(p);
            }
            var commandBuffer = Gfx.UseBuffer("draw-commands" + index.ToString(), Gfx.BufferType.Storage, commands, version);
            var bindings = TextureDrawing.Bindings(dynamicBuffer, batch, index, version, "dynamicVertices");
            bindings["staticVertices"] = staticBuffer;
            bindings["draws"] = commandBuffer;
            Gfx.Draw(batch.count, bindings,
                new DrawOpts { Shader = shader, Depth = batch.depth, DepthWrite = batch.depth,
                    Cull = batch.cull ? Gfx.Cull.Front : Gfx.Cull.None,
                    Blend = batch.multiply ? Gfx.Blend.Multiply : batch.blend ? (batch.alphaBlend ? Gfx.Blend.Alpha : Gfx.Blend.Additive) : Gfx.Blend.None });
            index++;
        }
    }
}
