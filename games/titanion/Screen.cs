using static Drawing;
using static GameMath;

public class TtnScreen
{
    public TtnScreen()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glEnable(GL_BLEND);
        glDisable(GL_DEPTH_TEST);
        glDisable(GL_CULL_FACE);
        clearColor = new float[]
        {
            0,
            0,
            0,
            1
        };
    }

    public static void setColor(float r, float g, float b, float a = 1)
    {
        Color(r, g, b, a);
    }

    public static void glTranslate(Vector3 p)
    {
        glTranslatef(p.x, p.y, p.z);
    }

    public static void glVertex(Vector3 p)
    {
        glVertex3f(p.x, p.y, p.z);
    }

    public static void glRotate(float d)
    {
        glRotatef(d * 180 / PI, 0, 0, 1);
    }
}
