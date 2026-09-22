// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;

public class Field
{
    public Vector size;
    public float eyeZ;
    public float eyeZa;
    public float alpha;
    public const float HEIGHT = 1;
    public const float HEIGHT_OFFSET = 8;
    public float z;
    public float r, g, b;
    public float lr, lg, lb;
    public void init()
    {
        size = new Vector();
        eyeZ = 0;
        alpha = 1;
    }

    public static float[][] COLOR = new float[][]
    {
        new float[] { 0.4f, 0.8f, 1 },
        new float[] { 0.4f, 1, 0.8f },
        new float[] { 1, 0.8f, 0.4f }
    };
    public static float[][] LUMINOUS_COLOR = new float[][]
    {
        new float[] { 0.2f, 0.2f, 1 },
        new float[] { 0.2f, 0.6f, 0.7f },
        new float[] { 0.6f, 0.2f, 0.7f }
    };
    public void start(int colorType)
    {
        if (size.x > size.y)
            eyeZa = size.x * 1.3f;
        else
            eyeZa = size.y * 1.3f / 480 * 640;
        z = 0;
        r = COLOR[colorType % 3][0];
        g = COLOR[colorType % 3][1];
        b = COLOR[colorType % 3][2];
        lr = LUMINOUS_COLOR[colorType % 3][0];
        lg = LUMINOUS_COLOR[colorType % 3][1];
        lb = LUMINOUS_COLOR[colorType % 3][2];
    }

    public void addSpeed(float s)
    {
        z = z - (s);
        if (z < 0)
            z = z + (HEIGHT_OFFSET);
    }

    public void move()
    {
        eyeZ = eyeZ + ((eyeZa - eyeZ) * 0.06f);
    }

    public void draw()
    {
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(r, g, b, 0.4f);
        glVertex3f(-size.x, -size.y, 0);
        A7xScreen.setColor(r, g, b, 0.8f);
        glVertex3f(-size.x, -size.y, HEIGHT);
        A7xScreen.setColor(r, g, b, 0.4f);
        glVertex3f(size.x, -size.y, 0);
        A7xScreen.setColor(r, g, b, 0.8f);
        glVertex3f(size.x, -size.y, HEIGHT);
        A7xScreen.setColor(r, g, b, 0.4f);
        glVertex3f(size.x, size.y, 0);
        A7xScreen.setColor(r, g, b, 0.8f);
        glVertex3f(size.x, size.y, HEIGHT);
        A7xScreen.setColor(r, g, b, 0.4f);
        glVertex3f(-size.x, size.y, 0);
        A7xScreen.setColor(r, g, b, 0.8f);
        glVertex3f(-size.x, size.y, HEIGHT);
        A7xScreen.setColor(r, g, b, 0.4f);
        glVertex3f(-size.x, -size.y, 0);
        A7xScreen.setColor(r, g, b, 0.8f);
        glVertex3f(-size.x, -size.y, HEIGHT);
        glEnd();
    }

    public void drawLuminous()
    {
        A7xScreen.setColor(lr, lg, lb, 0.9f * alpha);
        glBegin(GL_LINE_STRIP);
        glVertex3f(-size.x, -size.y, HEIGHT);
        glVertex3f(size.x, -size.y, HEIGHT);
        glVertex3f(size.x, size.y, HEIGHT);
        glVertex3f(-size.x, size.y, HEIGHT);
        glVertex3f(-size.x, -size.y, HEIGHT);
        glEnd();
        float hz = HEIGHT_OFFSET - z;
        for (int i = 0; i < 8; i++)
        {
            A7xScreen.setColor(lr, lg, lb, (0.8f - i * 0.05f) * alpha);
            glBegin(GL_LINE_STRIP);
            glVertex3f(-size.x, -size.y, hz);
            glVertex3f(size.x, -size.y, hz);
            glVertex3f(size.x, size.y, hz);
            glVertex3f(-size.x, size.y, hz);
            glVertex3f(-size.x, -size.y, hz);
            glEnd();
            hz = hz - (HEIGHT_OFFSET);
        }
    }
}
