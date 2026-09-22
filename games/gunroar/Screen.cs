// Copyright 2005 Kenta Cho. Some rights reserved.
using static Drawing;

public class GrScreen
{
    public static GunroarRand rand = new GunroarRand();
    public int screenShakeCnt;
    public float screenShakeIntense;
    public GrScreen()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glEnable(GL_BLEND);
        glDisable(GL_CULL_FACE);
        glDisable(GL_DEPTH_TEST);
        clearColor = new float[] { 0, 0, 0, 1 };
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void setColor(float r, float g, float b, float a = 1)
    {
        Color(r, g, b, a);
    }

    public static void setColorForced(float r, float g, float b, float a = 1)
    {
        Color(r, g, b, a);
    }

    public static void glTranslate(Vector v)
    {
        glTranslatef(v.x, v.y, 0);
    }

    public static void glTranslate3(Vector3 v)
    {
        glTranslatef(v.x, v.y, v.z);
    }

    public static void glVertex(Vector v)
    {
        glVertex3f(v.x, v.y, 0);
    }

    public static void glVertex3(Vector3 v)
    {
        glVertex3f(v.x, v.y, v.z);
    }

    public static void lineWidth(int width)
    {
        glLineWidth(width);
    }

    public void clear()
    {
        BeginFrame();
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

    public void setEyepos()
    {
        float x = 0, y = 0;
        if (screenShakeCnt > 0)
        {
            x = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 4));
            y = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 4));
        }

        glTranslatef(-x, -y, -13);
    }

    public void setScreenShake(int count, float intensity)
    {
        screenShakeCnt = count;
        screenShakeIntense = intensity;
    }

    public void move()
    {
        if (screenShakeCnt > 0)
            screenShakeCnt--;
    }
}
