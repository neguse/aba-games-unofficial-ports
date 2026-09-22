// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Spark : LuminousActor
{
    public static GunroarRand rand = new GunroarRand();
    public Vector pos, ppos;
    public Vector vel;
    public float r, g, b;
    public int cnt;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Spark()
    {
        pos = new Vector();
        ppos = new Vector();
        vel = new Vector();
        {
            b = 0;
            g = b;
            r = g;
        }

        cnt = 0;
    }

    public override void init(List<object> args)
    {
    }

    public void set(Vector p, float vx, float vy, float r, float g, float b, int c)
    {
        {
            pos.x = p.x;
            ppos.x = pos.x;
        }

        {
            pos.y = p.y;
            ppos.y = pos.y;
        }

        vel.x = vx;
        vel.y = vy;
        this.r = r;
        this.g = g;
        this.b = b;
        cnt = c;
        exists = true;
    }

    public override void move()
    {
        cnt--;
        if ((cnt <= 0) || (vel.dist_2() < 0.005f))
        {
            exists = false;
            return;
        }

        ppos.x = pos.x;
        ppos.y = pos.y;
        pos.opAddAssign(vel);
        vel.opMulAssign(0.96f);
    }

    public override void draw()
    {
        float ox = vel.x;
        float oy = vel.y;
        GrScreen.setColor(r, g, b, 1);
        ox = ox * (2);
        oy = oy * (2);
        glVertex3f(pos.x - ox, pos.y - oy, 0);
        ox = ox * (0.5f);
        oy = oy * (0.5f);
        GrScreen.setColor(r * 0.5f, g * 0.5f, b * 0.5f, 0);
        glVertex3f(pos.x - oy, pos.y + ox, 0);
        glVertex3f(pos.x + oy, pos.y - ox, 0);
    }

    public override void drawLuminous()
    {
        float ox = vel.x;
        float oy = vel.y;
        GrScreen.setColor(r, g, b, 1);
        ox = ox * (2);
        oy = oy * (2);
        glVertex3f(pos.x - ox, pos.y - oy, 0);
        ox = ox * (0.5f);
        oy = oy * (0.5f);
        GrScreen.setColor(r * 0.5f, g * 0.5f, b * 0.5f, 0);
        glVertex3f(pos.x - oy, pos.y + ox, 0);
        glVertex3f(pos.x + oy, pos.y - ox, 0);
    }
}

public class SparkPool : LuminousActorPool<Spark>
{
    public SparkPool(int n, List<object> args) : base(n, args, () => new Spark())
    {
    }
}

public class Smoke : LuminousActor
{
    public static GunroarRand rand = new GunroarRand();
    public static Vector3 windVel = new Vector3(0.04f, 0.04f, 0.02f);
    public static Vector wakePos = new Vector();
    public Field field;
    public WakePool wakes;
    public Vector3 pos;
    public Vector3 vel;
    public int type;
    public int cnt, startCnt;
    public float size;
    public float r, g, b, a;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Smoke()
    {
        pos = new Vector3();
        vel = new Vector3();
        type = 0;
        cnt = 0;
        startCnt = 1;
        size = 1;
        {
            a = 0;
            b = a;
            g = b;
            r = g;
        }
    }

    public override void init(List<object> args)
    {
        field = (Field)args[0];
        wakes = (WakePool)args[1];
    }

    public void set_7(Vector p, float mx, float my, float mz, int t, int c = 60, float sz = 2)
    {
        set_8(p.x, p.y, mx, my, mz, t, c, sz);
    }

    public void set3(Vector3 p, float mx, float my, float mz, int t, int c = 60, float sz = 2)
    {
        set_8(p.x, p.y, mx, my, mz, t, c, sz);
        pos.z = p.z;
    }

    public void set_8(float x, float y, float mx, float my, float mz, int t, int c = 60, float sz = 2)
    {
        if (!field.checkInOuterField_2(x, y))
            return;
        pos.x = x;
        pos.y = y;
        pos.z = 0;
        vel.x = mx;
        vel.y = my;
        vel.z = mz;
        type = t;
        {
            cnt = c;
            startCnt = cnt;
        }

        size = sz;
        switch (type)
        {
            case SmokeSmokeType.FIRE:
            {
                r = rand.nextFloat(0.1f) + 0.9f;
                g = rand.nextFloat(0.2f) + 0.2f;
                b = 0;
                a = 1;
                break;
            }

            case SmokeSmokeType.EXPLOSION:
            {
                r = rand.nextFloat(0.3f) + 0.7f;
                g = rand.nextFloat(0.3f) + 0.3f;
                b = 0;
                a = 1;
                break;
            }

            case SmokeSmokeType.SAND:
            {
                r = 0.8f;
                g = 0.8f;
                b = 0.6f;
                a = 0.6f;
                break;
            }

            case SmokeSmokeType.SPARK:
            {
                r = rand.nextFloat(0.3f) + 0.7f;
                g = rand.nextFloat(0.5f) + 0.5f;
                b = 0;
                a = 1;
                break;
            }

            case SmokeSmokeType.WAKE:
            {
                r = 0.6f;
                g = 0.6f;
                b = 0.8f;
                a = 0.6f;
                break;
            }

            case SmokeSmokeType.SMOKE:
            {
                r = rand.nextFloat(0.1f) + 0.1f;
                g = rand.nextFloat(0.1f) + 0.1f;
                b = 0.1f;
                a = 0.5f;
                break;
            }

            case SmokeSmokeType.LANCE_SPARK:
            {
                r = 0.4f;
                g = rand.nextFloat(0.2f) + 0.7f;
                b = rand.nextFloat(0.2f) + 0.7f;
                a = 1;
                break;
            }
        }

        exists = true;
    }

    public override void move()
    {
        cnt--;
        if ((cnt <= 0) || (!field.checkInOuterField_2(pos.x, pos.y)))
        {
            exists = false;
            return;
        }

        if (type != SmokeSmokeType.WAKE)
        {
            vel.x = vel.x + ((windVel.x - vel.x) * 0.01f);
            vel.y = vel.y + ((windVel.y - vel.y) * 0.01f);
            vel.z = vel.z + ((windVel.z - vel.z) * 0.01f);
        }

        pos.opAddAssign(vel);
        pos.y = pos.y - (field.lastScrollY);
        switch (type)
        {
            case SmokeSmokeType.FIRE:
            case SmokeSmokeType.EXPLOSION:
            case SmokeSmokeType.SMOKE:
            {
                if (cnt < startCnt / 2)
                {
                    r = r * (0.95f);
                    g = g * (0.95f);
                    b = b * (0.95f);
                }
                else
                {
                    a = a * (0.97f);
                }

                size = size * (1.01f);
                break;
            }

            case SmokeSmokeType.SAND:
            {
                r = r * (0.98f);
                g = g * (0.98f);
                b = b * (0.98f);
                a = a * (0.98f);
                break;
            }

            case SmokeSmokeType.SPARK:
            {
                r = r * (0.92f);
                g = g * (0.92f);
                a = a * (0.95f);
                vel.opMulAssign(0.9f);
                break;
            }

            case SmokeSmokeType.WAKE:
            {
                a = a * (0.98f);
                size = size * (1.005f);
                break;
            }

            case SmokeSmokeType.LANCE_SPARK:
            {
                a = a * (0.95f);
                size = size * (0.97f);
                break;
            }
        }

        if (size > 5)
            size = 5;
        if ((type == SmokeSmokeType.EXPLOSION) && (pos.z < 0.01f))
        {
            int bl = field.getBlock_2(pos.x, pos.y);
            if (bl >= 1)
                vel.opMulAssign(0.8f);
            if ((cnt % 3 == 0) && (bl < -1))
            {
                float sp = sqrt(vel.x * vel.x + vel.y * vel.y);
                if (sp > 0.3f)
                {
                    float d = atan2(vel.x, vel.y);
                    wakePos.x = pos.x + sin(d + PI / 2) * size * 0.25f;
                    wakePos.y = pos.y + cos(d + PI / 2) * size * 0.25f;
                    Wake w = wakes.getInstanceForced();
                    w.set(wakePos, d + PI - 0.2f + rand.nextSignedFloat(0.1f), sp * 0.33f, 20 + rand.nextInt(12), size * (7.0f + rand.nextFloat(3)));
                    wakePos.x = pos.x + sin(d - PI / 2) * size * 0.25f;
                    wakePos.y = pos.y + cos(d - PI / 2) * size * 0.25f;
                    w = wakes.getInstanceForced();
                    w.set(wakePos, d + PI + 0.2f + rand.nextSignedFloat(0.1f), sp * 0.33f, 20 + rand.nextInt(12), size * (7.0f + rand.nextFloat(3)));
                }
            }
        }
    }

    public override void draw()
    {
        float quadSize = size / 2;
        GrScreen.setColor(r, g, b, a);
        glVertex3f(pos.x - quadSize, pos.y - quadSize, pos.z);
        glVertex3f(pos.x + quadSize, pos.y - quadSize, pos.z);
        glVertex3f(pos.x + quadSize, pos.y + quadSize, pos.z);
        glVertex3f(pos.x - quadSize, pos.y + quadSize, pos.z);
    }

    public override void drawLuminous()
    {
        if ((r + g > 0.8f) && (b < 0.5f))
        {
            float quadSize = size / 2;
            GrScreen.setColor(r, g, b, a);
            glVertex3f(pos.x - quadSize, pos.y - quadSize, pos.z);
            glVertex3f(pos.x + quadSize, pos.y - quadSize, pos.z);
            glVertex3f(pos.x + quadSize, pos.y + quadSize, pos.z);
            glVertex3f(pos.x - quadSize, pos.y + quadSize, pos.z);
        }
    }
}

public class SmokePool : LuminousActorPool<Smoke>
{
    public SmokePool(int n, List<object> args) : base(n, args, () => new Smoke())
    {
    }
}

public class Fragment : Actor
{
    public static DisplayList displayList;
    public static GunroarRand rand;
    public Field field;
    public SmokePool smokes;
    public Vector3 pos;
    public Vector3 vel;
    public float size;
    public float d2, md2;
    public static void init_0()
    {
        rand = new GunroarRand();
        displayList = new DisplayList(1);
        displayList.beginNewList();
        GrScreen.setColor(0.7f, 0.5f, 0.5f, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex2f(-0.5f, -0.25f);
        glVertex2f(0.5f, -0.25f);
        glVertex2f(0.5f, 0.25f);
        glVertex2f(-0.5f, 0.25f);
        glEnd();
        GrScreen.setColor(0.7f, 0.5f, 0.5f, 0.9f);
        glBegin(GL_LINE_LOOP);
        glVertex2f(-0.5f, -0.25f);
        glVertex2f(0.5f, -0.25f);
        glVertex2f(0.5f, 0.25f);
        glVertex2f(-0.5f, 0.25f);
        glEnd();
        displayList.endNewList();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void close()
    {
        displayList.close();
    }

    public Fragment()
    {
        pos = new Vector3();
        vel = new Vector3();
        size = 1;
        {
            md2 = 0;
            d2 = md2;
        }
    }

    public override void init(List<object> args)
    {
        field = (Field)args[0];
        smokes = (SmokePool)args[1];
    }

    public void set(Vector p, float mx, float my, float mz, float sz = 1)
    {
        if (!field.checkInOuterField_2(p.x, p.y))
            return;
        pos.x = p.x;
        pos.y = p.y;
        pos.z = 0;
        vel.x = mx;
        vel.y = my;
        vel.z = mz;
        size = sz;
        if (size > 5)
            size = 5;
        d2 = rand.nextFloat(360);
        md2 = rand.nextSignedFloat(20);
        exists = true;
    }

    public override void move()
    {
        if (!field.checkInOuterField_2(pos.x, pos.y))
        {
            exists = false;
            return;
        }

        vel.x = vel.x * (0.96f);
        vel.y = vel.y * (0.96f);
        vel.z = vel.z + ((-0.04f - vel.z) * 0.01f);
        pos.opAddAssign(vel);
        if (pos.z < 0)
        {
            Smoke s = smokes.getInstanceForced();
            if (field.getBlock_2(pos.x, pos.y) < 0)
                s.set_8(pos.x, pos.y, 0, 0, 0, SmokeSmokeType.WAKE, 60, size * 0.66f);
            else
                s.set_8(pos.x, pos.y, 0, 0, 0, SmokeSmokeType.SAND, 60, size * 0.75f);
            exists = false;
            return;
        }

        pos.y = pos.y - (field.lastScrollY);
        d2 = d2 + (md2);
    }

    public override void draw()
    {
        glPushMatrix();
        GrScreen.glTranslate3(pos);
        glRotatef(d2, 1, 0, 0);
        glScalef(size, size, 1);
        displayList.call(0);
        glPopMatrix();
    }
}

public class FragmentPool : ActorPool<Fragment>
{
    public FragmentPool(int n, List<object> args) : base(n, args, () => new Fragment())
    {
    }
}

public class SparkFragment : LuminousActor
{
    public static DisplayList displayList;
    public static GunroarRand rand;
    public Field field;
    public SmokePool smokes;
    public Vector3 pos;
    public Vector3 vel;
    public float size;
    public float d2, md2;
    public int cnt;
    public bool hasSmoke;
    public static void init_0()
    {
        rand = new GunroarRand();
        displayList = new DisplayList(1);
        displayList.beginNewList();
        glBegin(GL_TRIANGLE_FAN);
        glVertex2f(-0.25f, -0.25f);
        glVertex2f(0.25f, -0.25f);
        glVertex2f(0.25f, 0.25f);
        glVertex2f(-0.25f, 0.25f);
        glEnd();
        displayList.endNewList();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void close()
    {
        displayList.close();
    }

    public SparkFragment()
    {
        pos = new Vector3();
        vel = new Vector3();
        size = 1;
        {
            md2 = 0;
            d2 = md2;
        }

        cnt = 0;
    }

    public override void init(List<object> args)
    {
        field = (Field)args[0];
        smokes = (SmokePool)args[1];
    }

    public void set(Vector p, float mx, float my, float mz, float sz = 1)
    {
        if (!field.checkInOuterField_2(p.x, p.y))
            return;
        pos.x = p.x;
        pos.y = p.y;
        pos.z = 0;
        vel.x = mx;
        vel.y = my;
        vel.z = mz;
        size = sz;
        if (size > 5)
            size = 5;
        d2 = rand.nextFloat(360);
        md2 = rand.nextSignedFloat(15);
        if (rand.nextInt(4) == 0)
            hasSmoke = true;
        else
            hasSmoke = false;
        cnt = 0;
        exists = true;
    }

    public override void move()
    {
        if (!field.checkInOuterField_2(pos.x, pos.y))
        {
            exists = false;
            return;
        }

        vel.x = vel.x * (0.99f);
        vel.y = vel.y * (0.99f);
        vel.z = vel.z + ((-0.08f - vel.z) * 0.01f);
        pos.opAddAssign(vel);
        if (pos.z < 0)
        {
            Smoke s = smokes.getInstanceForced();
            if (field.getBlock_2(pos.x, pos.y) < 0)
                s.set_8(pos.x, pos.y, 0, 0, 0, SmokeSmokeType.WAKE, 60, size * 0.66f);
            else
                s.set_8(pos.x, pos.y, 0, 0, 0, SmokeSmokeType.SAND, 60, size * 0.75f);
            exists = false;
            return;
        }

        pos.y = pos.y - (field.lastScrollY);
        d2 = d2 + (md2);
        cnt++;
        if (hasSmoke && (cnt % 5 == 0))
        {
            Smoke s = smokes.getInstance();
            if (s != null)
                s.set3(pos, 0, 0, 0, SmokeSmokeType.SMOKE, 90 + rand.nextInt(60), size * 0.5f);
        }
    }

    public override void draw()
    {
        glPushMatrix();
        GrScreen.setColor(1, rand.nextFloat(1), 0, 0.8f);
        GrScreen.glTranslate3(pos);
        glRotatef(d2, 1, 0, 0);
        glScalef(size, size, 1);
        displayList.call(0);
        glPopMatrix();
    }

    public override void drawLuminous()
    {
        glPushMatrix();
        GrScreen.setColor(1, rand.nextFloat(1), 0, 0.8f);
        GrScreen.glTranslate3(pos);
        glRotatef(d2, 1, 0, 0);
        glScalef(size, size, 1);
        displayList.call(0);
        glPopMatrix();
    }
}

public class SparkFragmentPool : LuminousActorPool<SparkFragment>
{
    public SparkFragmentPool(int n, List<object> args) : base(n, args, () => new SparkFragment())
    {
    }
}

public class Wake : Actor
{
    public Field field;
    public Vector pos;
    public Vector vel;
    public float deg;
    public float speed;
    public float size;
    public int cnt;
    public bool revShape;
    public Wake()
    {
        pos = new Vector();
        vel = new Vector();
        size = 1;
        deg = 0;
        speed = 0;
        cnt = 0;
    }

    public override void init(List<object> args)
    {
        field = (Field)args[0];
    }

    public void set(Vector p, float deg, float speed, int c = 60, float sz = 1, bool rs = false)
    {
        if (!field.checkInOuterField_2(p.x, p.y))
            return;
        pos.x = p.x;
        pos.y = p.y;
        this.deg = deg;
        this.speed = speed;
        vel.x = sin(deg) * speed;
        vel.y = cos(deg) * speed;
        cnt = c;
        size = sz;
        revShape = rs;
        exists = true;
    }

    public override void move()
    {
        cnt--;
        if (((cnt <= 0) || (vel.dist_2() < 0.005f)) || (!field.checkInOuterField_2(pos.x, pos.y)))
        {
            exists = false;
            return;
        }

        pos.opAddAssign(vel);
        pos.y = pos.y - (field.lastScrollY);
        vel.opMulAssign(0.96f);
        size = size * (1.02f);
    }

    public override void draw()
    {
        float ox = vel.x;
        float oy = vel.y;
        GrScreen.setColor(0.33f, 0.33f, 1);
        ox = ox * (size);
        oy = oy * (size);
        if (revShape)
            glVertex3f(pos.x + ox, pos.y + oy, 0);
        else
            glVertex3f(pos.x - ox, pos.y - oy, 0);
        ox = ox * (0.2f);
        oy = oy * (0.2f);
        GrScreen.setColor(0.2f, 0.2f, 0.6f, 0.5f);
        glVertex3f(pos.x - oy, pos.y + ox, 0);
        glVertex3f(pos.x + oy, pos.y - ox, 0);
    }
}

public class WakePool : ActorPool<Wake>
{
    public WakePool(int n, List<object> args) : base(n, args, () => new Wake())
    {
    }
}

public static class SmokeSmokeType
{
    public const int FIRE = 0;
    public const int EXPLOSION = 1;
    public const int SAND = 2;
    public const int SPARK = 3;
    public const int WAKE = 4;
    public const int SMOKE = 5;
    public const int LANCE_SPARK = 6;
}
