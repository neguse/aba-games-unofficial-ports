using static Drawing;
using static GameMath;

public class Screen
{
    public static float red = 1, green = 1, blue = 1;
    public virtual void setField(Field field)
    {
    }

    public static void setColorForced(float r, float g, float b, float a = 1)
    {
        red = r;
        green = g;
        blue = b;
        Color(r, g, b, a);
    }

    public static void drawLine(float x1, float y1, float z1, float x2, float y2, float z2, float a = 1)
    {
        setColor(a, a, a);
        glVertex3f(x1, y1, z1);
        setColor(a * .5f, a * .5f, a * .5f);
        glVertex3f((x1 + x2) / 2, (y1 + y2) / 2, (z1 + z2) / 2);
        glVertex3f((x1 + x2) / 2, (y1 + y2) / 2, (z1 + z2) / 2);
        setColor(a, a, a);
        glVertex3f(x2, y2, z2);
    }

    public const float nearPlane = 0.1f, farPlane = 1000;
    public const int width = 640, height = 480;
    public static void setColor(float r, float g, float b, float a = 1)
    {
        red = r;
        green = g;
        blue = b;
        Color(r, g, b, a);
    }

    public static void glTranslate_1_Vector(Vector v)
    {
        glTranslatef(v.x, v.y, 0);
    }

    public static void glTranslate_1_Vector3(Vector3 v)
    {
        glTranslatef(v.x, v.y, v.z);
    }

    public static void glVertex(Vector3 v)
    {
        glVertex3f(v.x, v.y, v.z);
    }

    public static void viewOrthoFixed()
    {
        glPushMatrix();
        LoadIdentity();
        ortho = true;
    }

    public static void viewPerspective()
    {
        glPopMatrix();
        ortho = false;
    }

    public virtual void clear()
    {
        BeginFrame();
        Drawing.farPlane = farPlane;
        Drawing.projectionScale = 1;
    }

    public static void lookAt(float ex, float ey, float ez, float lx, float ly, float lz, float ux, float uy, float uz)
    {
        float fx = lx - ex, fy = ly - ey, fz = lz - ez, l = sqrt(fx * fx + fy * fy + fz * fz);
        fx = fx / (l);
        fy = fy / (l);
        fz = fz / (l);
        float sx = fy * uz - fz * uy, sy = fz * ux - fx * uz, sz = fx * uy - fy * ux;
        l = sqrt(sx * sx + sy * sy + sz * sz);
        sx = sx / (l);
        sy = sy / (l);
        sz = sz / (l);
        float tx = sy * fz - sz * fy, ty = sz * fx - sx * fz, tz = sx * fy - sy * fx;
        glMultMatrix(new float[] { sx, tx, -fx, 0, sy, ty, -fy, 0, sz, tz, -fz, 0, 0, 0, 0, 1 });
        glTranslatef(-ex, -ey, -ez);
    }
}
