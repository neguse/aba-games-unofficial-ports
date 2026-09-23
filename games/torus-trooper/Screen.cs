using static Drawing;
using static GameMath;

public class TtScreen
{
    public const float nearPlane = 0.1f, farPlane = 10000;
    public const int width = 640, height = 480;
    public static void setColor(float r, float g, float b, float a = 1)
    {
        Color(r, g, b, a);
    }

    public static void glTranslate(Vector3 v)
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

    public bool startRenderToLuminousScreen()
    {
        return false;
    }

    public void endRenderToLuminousScreen()
    {
    }

    public void drawLuminous()
    {
    }

    public void clear()
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
