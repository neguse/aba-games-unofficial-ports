// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public interface Drawable
{
    public void draw();
}

public interface Collidable
{
    public Vector collision();

    public bool checkCollision(float ax, float ay, Collidable shape = null, float speed = 1);
}

public class ShipShape : Collidable, Drawable
{
    public bool checkCollision(float ax, float ay, Collidable shape = null, float speed = 1)
    {
        float cx = 0, cy = 0;
        if ((shape != null))
        {
            cx = collision().x + shape.collision().x;
            cy = collision().y + shape.collision().y;
        }
        else
        {
            cx = collision().x;
            cy = collision().y;
        }

        cy = cy * (speed);
        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public static Rand rand = new Rand();
    public List<Structure> structure = new List<Structure>();
    public Vector _collision;
    public DisplayList displayList;
    public List<float> rocketX = new List<float>();
    public Vector rocketPos, fragmentPos;
    public int color;
    public ShipShape(int randSeed)
    {
        rand.setSeed(randSeed);
    }

    public void close()
    {
        displayList.close();
    }

    public void setSeed(int n)
    {
        rand.setSeed(n);
    }

    public void create(int type, bool damaged = false)
    {
        switch (type)
        {
            case ShipShapeType.SMALL:
                createSmallType(damaged);
                break;
            case ShipShapeType.MIDDLE:
                createMiddleType(damaged);
                break;
            case ShipShapeType.LARGE:
                createLargeType(damaged);
                break;
        }

        createDisplayList();
        rocketPos = new Vector();
        fragmentPos = new Vector();
    }

    public void createDisplayList()
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        foreach (Structure st in structure)
        {
            st.createDisplayList();
        }

        displayList.endNewList();
    }

    public void createSmallType(bool damaged = false)
    {
        _collision = new Vector();
        int shaftNum = 1 + rand.nextInt(2);
        float sx = 0.25f + rand.nextFloat(0.1f);
        float so = 0.5f + rand.nextFloat(0.3f);
        float sl = 0.7f + rand.nextFloat(0.9f);
        float sw = 1.5f + rand.nextFloat(0.7f);
        sx = sx * (1.5f);
        so = so * (1.5f);
        sl = sl * (1.5f);
        sw = sw * (1.5f);
        float sd1 = rand.nextFloat(1) * PI / 3 + PI / 4;
        float sd2 = rand.nextFloat(1) * PI / 10;
        int cl = rand.nextInt(Structure.COLOR_RGB.Length - 2) + 2;
        color = cl;
        int shp = rand.nextInt(StructureShape.ROCKET);
        switch (shaftNum)
        {
            case 1:
                foreach (Structure part in createShaft(0, 0, so, sd1, sl, 2, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                _collision.x = so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(0);
                break;
            case 2:
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                _collision.x = sx + so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(sx * 0.05f);
                rocketX.Add(-sx * 0.05f);
                break;
        }

        _collision.x = _collision.x * (0.1f);
        _collision.y = _collision.y * (1.2f);
    }

    public void createMiddleType(bool damaged = false)
    {
        _collision = new Vector();
        int shaftNum = 3 + rand.nextInt(2);
        float sx = 1.0f + rand.nextFloat(0.7f);
        float so = 0.9f + rand.nextFloat(0.6f);
        float sl = 1.5f + rand.nextFloat(2.0f);
        float sw = 2.5f + rand.nextFloat(1.4f);
        sx = sx * (1.6f);
        so = so * (1.6f);
        sl = sl * (1.6f);
        sw = sw * (1.6f);
        float sd1 = rand.nextFloat(1) * PI / 3 + PI / 4;
        float sd2 = rand.nextFloat(1) * PI / 10;
        int cl = rand.nextInt(Structure.COLOR_RGB.Length - 2) + 2;
        color = cl;
        int shp = rand.nextInt(StructureShape.ROCKET);
        switch (shaftNum)
        {
            case 3:
                int cshp = rand.nextInt(StructureShape.ROCKET);
                foreach (Structure part in createShaft(0, 0, so * 0.5f, sd1, sl, 2, sw, sd1, sd2, cl, cshp, 8, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl * 0.8f, 1, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl * 0.8f, 1, sw, sd1 / 2, sd2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                _collision.x = sx + so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(0);
                rocketX.Add(sx * 0.05f);
                rocketX.Add(-sx * 0.05f);
                break;
            case 4:
                foreach (Structure part in createShaft(sx / 3, -sx / 2, so, sd1, sl * 0.7f, 1, sw * 0.6f, sd1 / 3, sd2 / 2, cl, shp, 5, 1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx / 3, -sx / 2, so, sd1, sl * 0.7f, 1, sw * 0.6f, sd1 / 3, sd2 / 2, cl, shp, 5, -1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                _collision.x = sx + so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(sx * 0.025f);
                rocketX.Add(-sx * 0.025f);
                rocketX.Add(sx * 0.05f);
                rocketX.Add(-sx * 0.05f);
                break;
        }

        _collision.x = _collision.x * (0.1f);
        _collision.y = _collision.y * (1.2f);
    }

    public void createLargeType(bool damaged = false)
    {
        _collision = new Vector();
        int shaftNum = 5 + rand.nextInt(2);
        float sx = 3.0f + rand.nextFloat(2.2f);
        float so = 1.5f + rand.nextFloat(1.0f);
        float sl = 3.0f + rand.nextFloat(4.0f);
        float sw = 5.0f + rand.nextFloat(2.5f);
        sx = sx * (1.6f);
        so = so * (1.6f);
        sl = sl * (1.6f);
        sw = sw * (1.6f);
        float sd1 = rand.nextFloat(1) * PI / 3 + PI / 4;
        float sd2 = rand.nextFloat(1) * PI / 10;
        int cl = rand.nextInt(Structure.COLOR_RGB.Length - 2) + 2;
        color = cl;
        int shp = rand.nextInt(StructureShape.ROCKET);
        switch (shaftNum)
        {
            case 5:
                int cshp = rand.nextInt(StructureShape.ROCKET);
                foreach (Structure part in createShaft(0, 0, so * 0.5f, sd1, sl, 2, sw, sd1, sd2, cl, cshp, 8, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx * 0.6f, 0, so, sd1, sl * 0.6f, 1, sw, sd1 / 3, sd2 / 2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx * 0.6f, 0, so, sd1, sl * 0.6f, 1, sw, sd1 / 3, sd2 / 2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl * 0.9f, 1, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl * 0.9f, 1, sw, sd1 / 2, sd2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                _collision.x = sx + so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(0);
                rocketX.Add(sx * 0.03f);
                rocketX.Add(-sx * 0.03f);
                rocketX.Add(sx * 0.05f);
                rocketX.Add(-sx * 0.05f);
                break;
            case 6:
                foreach (Structure part in createShaft(sx / 4, -sx / 2, so, sd1, sl * 0.6f, 1, sw * 0.6f, sd1 / 3, sd2 / 2, cl, shp, 5, 1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx / 4, -sx / 2, so, sd1, sl * 0.6f, 1, sw * 0.6f, sd1 / 3, sd2 / 2, cl, shp, 5, -1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx / 2, -sx / 3 * 2, so, sd1, sl * 0.8f, 1, sw * 0.8f, sd1 / 3, sd2 / 3 * 2, cl, shp, 5, 1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx / 2, -sx / 3 * 2, so, sd1, sl * 0.8f, 1, sw * 0.8f, sd1 / 3, sd2 / 3 * 2, cl, shp, 5, -1))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, 1, damaged))
                    structure.Add(part);
                foreach (Structure part in createShaft(sx, 0, so, sd1, sl, 1, sw, sd1 / 2, sd2, cl, shp, 5, -1, damaged))
                    structure.Add(part);
                _collision.x = sx + so / 2 + sw;
                _collision.y = sl / 2;
                rocketX.Add(sx * 0.0125f);
                rocketX.Add(-sx * 0.0125f);
                rocketX.Add(sx * 0.025f);
                rocketX.Add(-sx * 0.025f);
                rocketX.Add(sx * 0.05f);
                rocketX.Add(-sx * 0.05f);
                break;
        }

        _collision.x = _collision.x * (0.1f);
        _collision.y = _collision.y * (1.2f);
    }

    public List<Structure> createShaft(float ox, float oy, float offset, float od1, float rocketLength, int wingNum, float wingWidth, float wingD1, float wingD2, int color, int shp, int divNum, int rev, bool damaged = false)
    {
        List<Structure> sts = new List<Structure>();
        Structure st = new Structure();
        st.pos.x = ox;
        st.pos.y = oy;
        {
            st.d2 = 0;
            st.d1 = st.d2;
        }

        st.width = rocketLength * 0.15f;
        st.height = rocketLength;
        st.shape = StructureShape.ROCKET;
        st.shapeXReverse = 1;
        if (!(damaged))
            st.color = 1;
        else
            st.color = 0;
        if (rev == -1)
            st.pos.x = st.pos.x * (-1);
        sts.Add(st);
        float wofs = offset;
        float whgt = rocketLength * (rand.nextFloat(0.5f) + 1.5f);
        for (int i = 0; i < wingNum; i++)
        {
            Structure wing = new Structure();
            wing.d1 = wingD1 * 180 / PI;
            wing.d2 = wingD2 * 180 / PI;
            wing.pos.x = ox + sin(od1) * wofs;
            wing.pos.y = oy + cos(od1) * wofs;
            wing.width = wingWidth;
            wing.height = whgt;
            wing.shape = shp;
            wing.divNum = divNum;
            wing.shapeXReverse = 1;
            if (!(damaged))
                wing.color = color;
            else
                wing.color = 0;
            if ((((i % 2) * 2) - 1) * rev == 1)
            {
                wing.pos.x = wing.pos.x * (-1);
                wing.d1 = wing.d1 * (-1);
                wing.shapeXReverse = wing.shapeXReverse * (-1);
            }

            sts.Add(wing);
        }

        return sts;
    }

    public void addParticles(Vector pos, ParticlePool particles)
    {
        foreach (float rx in rocketX)
        {
            Particle pt = particles.getInstance();
            if (!((pt != null)))
                break;
            rocketPos.x = pos.x + rx;
            rocketPos.y = pos.y - 0.15f;
            pt.set_12(rocketPos, 1, PI, 0, 0.2f, 0.3f, 0.4f, 1.0f, 16, ParticlePType.JET);
        }
    }

    public void addFragments(Vector pos, ParticlePool particles)
    {
        if (collision().x < 0.5f)
            return;
        for (int i = 0; i < collision().x * 40; i++)
        {
            Particle pt = particles.getInstance();
            if (!((pt != null)))
                break;
            fragmentPos.x = pos.x;
            fragmentPos.y = pos.y;
            float wb = collision().x;
            float hb = collision().y;
            pt.set_12(fragmentPos, 1, rand.nextSignedFloat(0.1f), 1 + rand.nextSignedFloat(1), 0.2f + rand.nextFloat(0.2f), Structure.COLOR_RGB[color][0], Structure.COLOR_RGB[color][1], Structure.COLOR_RGB[color][2], 32 + rand.nextInt(16), ParticlePType.FRAGMENT, wb + rand.nextFloat(wb), hb + rand.nextFloat(hb));
        }
    }

    public void draw()
    {
        displayList.call(0);
    }

    public Vector collision() {
            return _collision;
        }
}

public class Structure
{
    public static float[][] COLOR_RGB = new float[][]
    {
        new float[] { 1, 1, 1 },
        new float[] { 0.6f, 0.6f, 0.6f },
        new float[] { 0.9f, 0.5f, 0.5f },
        new float[] { 0.5f, 0.9f, 0.5f },
        new float[] { 0.5f, 0.5f, 0.9f },
        new float[] { 0.7f, 0.7f, 0.5f },
        new float[] { 0.7f, 0.5f, 0.7f },
        new float[] { 0.5f, 0.7f, 0.7f },
    };
    public Vector pos;
    public float d1, d2;
    public float width, height;
    public int shape;
    public float shapeXReverse;
    public int color;
    public int divNum;
    public Structure()
    {
        pos = new Vector();
    }

    public void createDisplayList()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0);
        glRotatef(-d2, 1, 0, 0);
        glRotatef(d1, 0, 0, 1);
        if (shape == StructureShape.ROCKET)
            glScalef(width, width, height);
        else
            glScalef(width, height, 1);
        glScalef(shapeXReverse, 1, 1);
        float alp = 0.5f;
        if (color == 0)
            alp = 1;
        TtScreen.setColor(COLOR_RGB[color][0], COLOR_RGB[color][1], COLOR_RGB[color][2]);
        switch (shape)
        {
            case StructureShape.SQUARE:
                for (int i = 0; i < divNum; i++)
                {
                    float x11 = -0.5f + (1.0f / divNum) * i;
                    float x12 = x11 + (1.0f / divNum) * 0.8f;
                    float x21 = -0.5f + (0.8f / divNum) * i;
                    float x22 = x21 + (0.8f / divNum) * 0.8f;
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x21, 0, -0.5f);
                    glVertex3f(x22, 0, -0.5f);
                    glVertex3f(x12, 0, 0.5f);
                    glVertex3f(x11, 0, 0.5f);
                    glEnd();
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x21, 0.1f, -0.5f);
                    glVertex3f(x22, 0.1f, -0.5f);
                    glVertex3f(x12, 0.1f, 0.5f);
                    glVertex3f(x11, 0.1f, 0.5f);
                    glEnd();
                    TtScreen.setColor(COLOR_RGB[color][0], COLOR_RGB[color][1], COLOR_RGB[color][2], alp);
                    glBegin(GL_TRIANGLE_FAN);
                    glVertex3f(x21, 0, -0.5f);
                    glVertex3f(x22, 0, -0.5f);
                    glVertex3f(x12, 0, 0.5f);
                    glVertex3f(x11, 0, 0.5f);
                    glEnd();
                }

                break;
            case StructureShape.WING:
                for (int i = 0; i < divNum; i++)
                {
                    float x1 = -0.5f + (1.0f / divNum) * i;
                    float x2 = x1 + (1.0f / divNum) * 0.8f;
                    float y1 = x1;
                    float y2 = x2;
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x1, 0, y1);
                    glVertex3f(x2, 0, y2);
                    glVertex3f(x2, 0, 0.5f);
                    glVertex3f(x1, 0, 0.5f);
                    glEnd();
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x1, 0.1f, y1);
                    glVertex3f(x2, 0.1f, y2);
                    glVertex3f(x2, 0.1f, 0.5f);
                    glVertex3f(x1, 0.1f, 0.5f);
                    glEnd();
                    TtScreen.setColor(COLOR_RGB[color][0], COLOR_RGB[color][1], COLOR_RGB[color][2], alp);
                    glBegin(GL_TRIANGLE_FAN);
                    glVertex3f(x1, 0, y1);
                    glVertex3f(x2, 0, y2);
                    glVertex3f(x2, 0, 0.5f);
                    glVertex3f(x1, 0, 0.5f);
                    glEnd();
                }

                break;
            case StructureShape.TRIANGLE:
                for (int i = 0; i < divNum; i++)
                {
                    float x1 = -0.5f + (1.0f / divNum) * i;
                    float x2 = x1 + (1.0f / divNum) * 0.8f;
                    float y1 = -0.5f + (1.0f / divNum) * fabs(i - GameMath.integer(divNum / 2)) * 2;
                    float y2 = -0.5f + (1.0f / divNum) * fabs(i + 0.8f - GameMath.integer(divNum / 2)) * 2;
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x1, 0, y1);
                    glVertex3f(x2, 0, y2);
                    glVertex3f(x2, 0, 0.5f);
                    glVertex3f(x1, 0, 0.5f);
                    glEnd();
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(x1, 0.1f, y1);
                    glVertex3f(x2, 0.1f, y2);
                    glVertex3f(x2, 0.1f, 0.5f);
                    glVertex3f(x1, 0.1f, 0.5f);
                    glEnd();
                    TtScreen.setColor(COLOR_RGB[color][0], COLOR_RGB[color][1], COLOR_RGB[color][2], alp);
                    glBegin(GL_TRIANGLE_FAN);
                    glVertex3f(x1, 0, y1);
                    glVertex3f(x2, 0, y2);
                    glVertex3f(x2, 0, 0.5f);
                    glVertex3f(x1, 0, 0.5f);
                    glEnd();
                }

                break;
            case StructureShape.ROCKET:
                for (int i = 0; i < 4; i++)
                {
                    float d = i * PI / 2 + PI / 4;
                    glBegin(GL_LINE_LOOP);
                    glVertex3f(sin(d - 0.3f), cos(d - 0.3f), -0.5f);
                    glVertex3f(sin(d + 0.3f), cos(d + 0.3f), -0.5f);
                    glVertex3f(sin(d + 0.3f), cos(d + 0.3f), 0.5f);
                    glVertex3f(sin(d - 0.3f), cos(d - 0.3f), 0.5f);
                    glEnd();
                    TtScreen.setColor(COLOR_RGB[color][0], COLOR_RGB[color][1], COLOR_RGB[color][2], alp);
                    glBegin(GL_TRIANGLE_FAN);
                    glVertex3f(sin(d - 0.3f), cos(d - 0.3f), -0.5f);
                    glVertex3f(sin(d + 0.3f), cos(d + 0.3f), -0.5f);
                    glVertex3f(sin(d + 0.3f), cos(d + 0.3f), 0.5f);
                    glVertex3f(sin(d - 0.3f), cos(d - 0.3f), 0.5f);
                    glEnd();
                }

                break;
        }

        glPopMatrix();
    }
}

public class BitShape : Drawable
{
    public static float[] COLOR_RGB = new float[] { 1, 0.9f, 0.5f };
    public DisplayList displayList;
    public void create()
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        for (int i = 0; i < 4; i++)
        {
            float d = i * PI / 2 + PI / 4;
            TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
            glBegin(GL_LINE_LOOP);
            glVertex3f(sin(d - 0.3f), -0.8f, cos(d - 0.3f));
            glVertex3f(sin(d + 0.3f), -0.8f, cos(d + 0.3f));
            glVertex3f(sin(d + 0.3f), 0.8f, cos(d + 0.3f));
            glVertex3f(sin(d - 0.3f), 0.8f, cos(d - 0.3f));
            glEnd();
            d = d + (PI / 4);
            glBegin(GL_LINE_LOOP);
            glVertex3f(sin(d - 0.3f) * 2, -0.2f, cos(d - 0.3f) * 2);
            glVertex3f(sin(d + 0.3f) * 2, -0.2f, cos(d + 0.3f) * 2);
            glVertex3f(sin(d + 0.3f) * 2, 0.2f, cos(d + 0.3f) * 2);
            glVertex3f(sin(d - 0.3f) * 2, 0.2f, cos(d - 0.3f) * 2);
            glEnd();
            d = d - (PI / 4);
            TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2], 0.5f);
            glBegin(GL_TRIANGLE_FAN);
            glVertex3f(sin(d - 0.3f), -0.8f, cos(d - 0.3f));
            glVertex3f(sin(d + 0.3f), -0.8f, cos(d + 0.3f));
            glVertex3f(sin(d + 0.3f), 0.8f, cos(d + 0.3f));
            glVertex3f(sin(d - 0.3f), 0.8f, cos(d - 0.3f));
            glEnd();
            d = d + (PI / 4);
            glBegin(GL_TRIANGLE_FAN);
            glVertex3f(sin(d - 0.3f) * 2, -0.2f, cos(d - 0.3f) * 2);
            glVertex3f(sin(d + 0.3f) * 2, -0.2f, cos(d + 0.3f) * 2);
            glVertex3f(sin(d + 0.3f) * 2, 0.2f, cos(d + 0.3f) * 2);
            glVertex3f(sin(d - 0.3f) * 2, 0.2f, cos(d - 0.3f) * 2);
            glEnd();
        }

        displayList.endNewList();
    }

    public void close()
    {
        displayList.close();
    }

    public void draw()
    {
        displayList.call(0);
    }
}

public class BulletShape : Drawable
{
    public const int NUM = 6;
    public static float[] COLOR_RGB = new float[] { 1, 0.7f, 0.8f };
    public DisplayList displayList;
    public void create(int type)
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        switch (type)
        {
            case 0:
                createTriangleShape(false);
                break;
            case 1:
                createTriangleShape(true);
                break;
            case 2:
                createSquareShape(false);
                break;
            case 3:
                createSquareShape(true);
                break;
            case 4:
                createBarShape(false);
                break;
            case 5:
                createBarShape(true);
                break;
        }

        displayList.endNewList();
    }

    public void close()
    {
        displayList.close();
    }

    public void createTriangleShape(bool wireShape)
    {
        Vector3 cp = new Vector3();
        Vector3 p1 = new Vector3();
        Vector3 p2 = new Vector3();
        Vector3 p3 = new Vector3();
        Vector3 np1 = new Vector3();
        Vector3 np2 = new Vector3();
        Vector3 np3 = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            float d = PI * 2 / 3 * i;
            {
                p1.y = 0;
                p1.x = p1.y;
            }

            p1.z = 2.5f;
            p2.x = sin(d) * 1.8f;
            p2.y = cos(d) * 1.8f;
            p2.z = -1.2f;
            p3.x = sin(d + PI * 2 / 3) * 1.2f;
            p3.y = cos(d + PI * 2 / 3) * 1.2f;
            p3.z = -1.2f;
            {
                cp.z = 0;
                cp.y = cp.z;
                cp.x = cp.y;
            }

            cp.opAddAssign(p1);
            cp.opAddAssign(p2);
            cp.opAddAssign(p3);
            cp.opDivAssign(3);
            np1.blend(p1, cp, 0.6f);
            np2.blend(p2, cp, 0.6f);
            np3.blend(p3, cp, 0.6f);
            if (!(wireShape))
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
            else
                TtScreen.setColor(COLOR_RGB[0] * 0.6f, COLOR_RGB[1], COLOR_RGB[2]);
            glBegin(GL_LINE_LOOP);
            TtScreen.glVertex(np1);
            TtScreen.glVertex(np2);
            TtScreen.glVertex(np3);
            glEnd();
            if (!(wireShape))
            {
                glBegin(GL_TRIANGLE_FAN);
                TtScreen.setColor(COLOR_RGB[0] * 0.7f, COLOR_RGB[1] * 0.7f, COLOR_RGB[2] * 0.7f);
                TtScreen.glVertex(np1);
                TtScreen.setColor(COLOR_RGB[0] * 0.4f, COLOR_RGB[1] * 0.4f, COLOR_RGB[2] * 0.4f);
                TtScreen.glVertex(np2);
                TtScreen.glVertex(np3);
                glEnd();
            }
        }
    }

    public void createSquareShape(bool wireShape)
    {
        Vector3 cp = new Vector3();
        Vector3[] p = new Vector3[4];
        Vector3[] np = new Vector3[4];
        float[][][] POINT_DAT = new float[][][]
        {
            new float[][]
            {
                new float[] { -1, -1, 1 },
                new float[] { 1, -1, 1 },
                new float[] { 1, 1, 1 },
                new float[] { -1, 1, 1 },
            },
            new float[][]
            {
                new float[] { -1, -1, -1 },
                new float[] { 1, -1, -1 },
                new float[] { 1, 1, -1 },
                new float[] { -1, 1, -1 },
            },
            new float[][]
            {
                new float[] { -1, 1, -1 },
                new float[] { 1, 1, -1 },
                new float[] { 1, 1, 1 },
                new float[] { -1, 1, 1 },
            },
            new float[][]
            {
                new float[] { -1, -1, -1 },
                new float[] { 1, -1, -1 },
                new float[] { 1, -1, 1 },
                new float[] { -1, -1, 1 },
            },
            new float[][]
            {
                new float[] { 1, -1, -1 },
                new float[] { 1, -1, 1 },
                new float[] { 1, 1, 1 },
                new float[] { 1, 1, -1 },
            },
            new float[][]
            {
                new float[] { -1, -1, -1 },
                new float[] { -1, -1, 1 },
                new float[] { -1, 1, 1 },
                new float[] { -1, 1, -1 },
            },
        };
        for (int i = 0; i < 4; i++)
            p[i] = new Vector3();
        for (int i = 0; i < 4; i++)
            np[i] = new Vector3();
        for (int i = 0; i < 6; i++)
        {
            {
                cp.z = 0;
                cp.y = cp.z;
                cp.x = cp.y;
            }

            for (int j = 0; j < 4; j++)
            {
                p[j].x = POINT_DAT[i][j][0];
                p[j].y = POINT_DAT[i][j][1];
                p[j].z = POINT_DAT[i][j][2];
                cp.opAddAssign(p[j]);
            }

            cp.opDivAssign(4);
            for (int j = 0; j < 4; j++)
                np[j].blend(p[j], cp, 0.6f);
            if (!(wireShape))
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
            else
                TtScreen.setColor(COLOR_RGB[0] * 0.6f, COLOR_RGB[1], COLOR_RGB[2]);
            glBegin(GL_LINE_LOOP);
            for (int j = 0; j < 4; j++)
                TtScreen.glVertex(np[j]);
            glEnd();
            if (!(wireShape))
            {
                glBegin(GL_TRIANGLE_FAN);
                TtScreen.setColor(COLOR_RGB[0] * 0.7f, COLOR_RGB[1] * 0.7f, COLOR_RGB[2] * 0.7f);
                for (int j = 0; j < 4; j++)
                    TtScreen.glVertex(np[j]);
                glEnd();
            }
        }
    }

    public void createBarShape(bool wireShape)
    {
        Vector3 cp = new Vector3();
        Vector3[] p = new Vector3[4];
        Vector3[] np = new Vector3[4];
        float[][][] POINT_DAT = new float[][][]
        {
            new float[][]
            {
                new float[] { -1, -1, 1 },
                new float[] { 1, -1, 1 },
                new float[] { 1, 1, 1 },
                new float[] { -1, 1, 1 },
            },
            new float[][]
            {
                new float[] { -1, 1, -1 },
                new float[] { 1, 1, -1 },
                new float[] { 1, 1, 1 },
                new float[] { -1, 1, 1 },
            },
            new float[][]
            {
                new float[] { -1, -1, -1 },
                new float[] { 1, -1, -1 },
                new float[] { 1, -1, 1 },
                new float[] { -1, -1, 1 },
            },
            new float[][]
            {
                new float[] { 1, -1, -1 },
                new float[] { 1, -1, 1 },
                new float[] { 1, 1, 1 },
                new float[] { 1, 1, -1 },
            },
            new float[][]
            {
                new float[] { -1, -1, -1 },
                new float[] { -1, -1, 1 },
                new float[] { -1, 1, 1 },
                new float[] { -1, 1, -1 },
            },
        };
        for (int i = 0; i < 4; i++)
            p[i] = new Vector3();
        for (int i = 0; i < 4; i++)
            np[i] = new Vector3();
        for (int i = 0; i < 5; i++)
        {
            {
                cp.z = 0;
                cp.y = cp.z;
                cp.x = cp.y;
            }

            for (int j = 0; j < 4; j++)
            {
                p[j].x = POINT_DAT[i][j][0] * 0.7f;
                p[j].y = POINT_DAT[i][j][1] * 0.7f;
                p[j].z = POINT_DAT[i][j][2] * 1.75f;
                cp.opAddAssign(p[j]);
            }

            cp.opDivAssign(4);
            for (int j = 0; j < 4; j++)
                np[j].blend(p[j], cp, 0.6f);
            if (!(wireShape))
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
            else
                TtScreen.setColor(COLOR_RGB[0] * 0.6f, COLOR_RGB[1], COLOR_RGB[2]);
            glBegin(GL_LINE_LOOP);
            for (int j = 0; j < 4; j++)
                TtScreen.glVertex(np[j]);
            glEnd();
            if (!(wireShape))
            {
                glBegin(GL_TRIANGLE_FAN);
                TtScreen.setColor(COLOR_RGB[0] * 0.7f, COLOR_RGB[1] * 0.7f, COLOR_RGB[2] * 0.7f);
                for (int j = 0; j < 4; j++)
                    TtScreen.glVertex(np[j]);
                glEnd();
            }
        }
    }

    public void draw()
    {
        displayList.call(0);
    }
}

public class ShotShape : Collidable, Drawable
{
    public bool checkCollision(float ax, float ay, Collidable shape = null, float speed = 1)
    {
        float cx = 0, cy = 0;
        if ((shape != null))
        {
            cx = collision().x + shape.collision().x;
            cy = collision().y + shape.collision().y;
        }
        else
        {
            cx = collision().x;
            cy = collision().y;
        }

        cy = cy * (speed);
        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public static float[] COLOR_RGB = new float[] { 0.8f, 1, 0.7f };
    public DisplayList displayList;
    public Vector _collision;
    public void create(bool charge)
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        if (charge)
        {
            for (int i = 0; i < 8; i++)
            {
                float d = i * PI / 4;
                glBegin(GL_TRIANGLES);
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
                glVertex3f(sin(d) * 0.1f, cos(d) * 0.1f, 0.2f);
                glVertex3f(sin(d) * 0.5f, cos(d) * 0.5f, 0.5f);
                TtScreen.setColor(COLOR_RGB[0] * 0.2f, COLOR_RGB[1] * 0.2f, COLOR_RGB[2] * 0.2f);
                glVertex3f(sin(d) * 1.0f, cos(d) * 1.0f, -0.7f);
                glEnd();
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
                glBegin(GL_LINE_LOOP);
                glVertex3f(sin(d) * 0.1f, cos(d) * 0.1f, 0.2f);
                glVertex3f(sin(d) * 0.5f, cos(d) * 0.5f, 0.5f);
                glVertex3f(sin(d) * 1.0f, cos(d) * 1.0f, -0.7f);
                glEnd();
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                float d = i * PI / 2;
                glBegin(GL_TRIANGLES);
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
                glVertex3f(sin(d) * 0.1f, cos(d) * 0.1f, 0.4f);
                glVertex3f(sin(d) * 0.3f, cos(d) * 0.3f, 1.0f);
                TtScreen.setColor(COLOR_RGB[0] * 0.2f, COLOR_RGB[1] * 0.2f, COLOR_RGB[2] * 0.2f);
                glVertex3f(sin(d) * 0.5f, cos(d) * 0.5f, -1.4f);
                glEnd();
                TtScreen.setColor(COLOR_RGB[0], COLOR_RGB[1], COLOR_RGB[2]);
                glBegin(GL_LINE_LOOP);
                glVertex3f(sin(d) * 0.1f, cos(d) * 0.1f, 0.4f);
                glVertex3f(sin(d) * 0.3f, cos(d) * 0.3f, 1.0f);
                glVertex3f(sin(d) * 0.5f, cos(d) * 0.5f, -1.4f);
                glEnd();
            }
        }

        displayList.endNewList();
        _collision = new Vector(0.15f, 0.3f);
    }

    public void close()
    {
        displayList.close();
    }

    public void draw()
    {
        displayList.call(0);
    }

    public Vector collision() {
            return _collision;
        }
}

public class ResizableDrawable : Collidable, Drawable
{
    public bool checkCollision(float ax, float ay, Collidable shape = null, float speed = 1)
    {
        float cx = 0, cy = 0;
        if ((shape != null))
        {
            cx = collision().x + shape.collision().x;
            cy = collision().y + shape.collision().y;
        }
        else
        {
            cx = collision().x;
            cy = collision().y;
        }

        cy = cy * (speed);
        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public Drawable _shape;
    public float _size;
    public Vector _collision;
    public void draw()
    {
        glScalef(_size, _size, _size);
        _shape.draw();
    }

    public Drawable shape
    {
        set
        {
            _collision = new Vector();
            _shape = value;
        }
    }

    public float size
    {
        set
        {
            _size = value;
        }
    }

    public Vector collision() {
            Collidable cd = (Collidable)_shape;
            if ((cd != null))
            {
                _collision.x = cd.collision().x * _size;
                _collision.y = cd.collision().y * _size;
                return _collision;
            }
            else
            {
                return null;
            }
        }
}

public static class ShipShapeType
{
    public const int SMALL = 0, MIDDLE = 1, LARGE = 2;
}

public static class StructureShape
{
    public const int SQUARE = 0, WING = 1, TRIANGLE = 2, ROCKET = 3;
}

public static class BulletShapeBSType
{
    public const int TRIANGLE = 0, TRIANGLE_WIRE = 1, SQUARE = 2, SQUARE_WIRE = 3, BAR = 4, BAR_WIRE = 5;
}
