// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class BaseShape : DrawableShape
{
    public const int POINT_NUM = 16;
    public static GunroarRand rand = new GunroarRand();
    public static Vector wakePos = new Vector();
    public float size, distRatio, spinyRatio;
    public int type;
    public float r, g, b;
    public const int PILLAR_POINT_NUM = 8;
    public List<Vector> pillarPos = new List<Vector>();
    public List<Vector> _pointPos = new List<Vector>();
    public List<float> _pointDeg = new List<float>();
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public BaseShape(float size, float distRatio, float spinyRatio, int type, float r, float g, float b)
    {
        this.size = size;
        this.distRatio = distRatio;
        this.spinyRatio = spinyRatio;
        this.type = type;
        this.r = r;
        this.g = g;
        this.b = b;
        initializeShape();
    }

    public override void createDisplayList()
    {
        float height = size * 0.5f;
        float z = 0;
        float sz = 1;
        if (type == BaseShapeShapeType.BRIDGE)
            z = z + (height);
        if (type != BaseShapeShapeType.SHIP_DESTROYED)
            GrScreen.setColor(r, g, b);
        glBegin(GL_LINE_LOOP);
        if (type != BaseShapeShapeType.BRIDGE)
            createLoop(sz, z, false, true);
        else
            createSquareLoop(sz, z, false, 1);
        glEnd();
        if ((((type != BaseShapeShapeType.SHIP_SHADOW) && (type != BaseShapeShapeType.SHIP_DESTROYED)) && (type != BaseShapeShapeType.PLATFORM_DESTROYED)) && (type != BaseShapeShapeType.TURRET_DESTROYED))
        {
            GrScreen.setColor(r * 0.4f, g * 0.4f, b * 0.4f);
            glBegin(GL_TRIANGLE_FAN);
            createLoop(sz, z, true);
            glEnd();
        }

        switch (type)
        {
            case BaseShapeShapeType.SHIP:
            case BaseShapeShapeType.SHIP_ROUNDTAIL:
            case BaseShapeShapeType.SHIP_SHADOW:
            case BaseShapeShapeType.SHIP_DAMAGED:
            case BaseShapeShapeType.SHIP_DESTROYED:
            {
                if (type != BaseShapeShapeType.SHIP_DESTROYED)
                    GrScreen.setColor(r * 0.4f, g * 0.4f, b * 0.4f);
                for (int i = 0; i < 3; i++)
                {
                    z = z - (height / 4);
                    sz = sz - (0.2f);
                    glBegin(GL_LINE_LOOP);
                    createLoop(sz, z);
                    glEnd();
                }

                break;
            }

            case BaseShapeShapeType.PLATFORM:
            case BaseShapeShapeType.PLATFORM_DAMAGED:
            case BaseShapeShapeType.PLATFORM_DESTROYED:
            {
                GrScreen.setColor(r * 0.4f, g * 0.4f, b * 0.4f);
                for (int i = 0; i < 3; i++)
                {
                    z = z - (height / 3);
                    foreach (Vector pp in pillarPos)
                    {
                        glBegin(GL_LINE_LOOP);
                        createPillar(pp, size * 0.2f, z);
                        glEnd();
                    }
                }

                break;
            }

            case BaseShapeShapeType.BRIDGE:
            case BaseShapeShapeType.TURRET:
            case BaseShapeShapeType.TURRET_DAMAGED:
            {
                GrScreen.setColor(r * 0.6f, g * 0.6f, b * 0.6f);
                z = z + (height);
                sz = sz - (0.33f);
                glBegin(GL_LINE_LOOP);
                if (type == BaseShapeShapeType.BRIDGE)
                    createSquareLoop(sz, z);
                else
                    createSquareLoop(sz, z / 2, false, 3);
                glEnd();
                GrScreen.setColor(r * 0.25f, g * 0.25f, b * 0.25f);
                glBegin(GL_TRIANGLE_FAN);
                if (type == BaseShapeShapeType.BRIDGE)
                    createSquareLoop(sz, z, true);
                else
                    createSquareLoop(sz, z / 2, true, 3);
                glEnd();
                break;
            }

            case BaseShapeShapeType.TURRET_DESTROYED:
            {
                break;
            }
        }
    }

    public void createLoop(float s, float z, bool backToFirst = false, bool record = false)
    {
        float d = 0;
        int pn = 0;
        bool firstPoint = true;
        float fpx = 0, fpy = 0;
        for (int i = 0; i < POINT_NUM; i++)
        {
            if (((((type != BaseShapeShapeType.SHIP) && (type != BaseShapeShapeType.SHIP_DESTROYED)) && (type != BaseShapeShapeType.SHIP_DAMAGED)) && (i > POINT_NUM * 2 / 5)) && (i <= POINT_NUM * 3 / 5))
                continue;
            if ((((type == BaseShapeShapeType.TURRET) || (type == BaseShapeShapeType.TURRET_DAMAGED)) || (type == BaseShapeShapeType.TURRET_DESTROYED)) && ((i <= POINT_NUM / 5) || (i > POINT_NUM * 4 / 5)))
                continue;
            d = PI * 2 * i / POINT_NUM;
            float cx = sin(d) * size * s * (1 - distRatio);
            float cy = cos(d) * size * s;
            float sx = 0, sy = 0;
            if ((i == POINT_NUM / 4) || (i == POINT_NUM / 4 * 3))
                sy = 0;
            else
                sy = 1 / (1 + fabs(tan(d)));
            sx = 1 - sy;
            if (i >= POINT_NUM / 2)
                sx = sx * (-1);
            if ((i >= POINT_NUM / 4) && (i <= POINT_NUM / 4 * 3))
                sy = sy * (-1);
            sx = sx * (size * s * (1 - distRatio));
            sy = sy * (size * s);
            float px = cx * (1 - spinyRatio) + sx * spinyRatio;
            float py = cy * (1 - spinyRatio) + sy * spinyRatio;
            glVertex3f(px, py, z);
            if (backToFirst && firstPoint)
            {
                fpx = px;
                fpy = py;
                firstPoint = false;
            }

            if (record)
            {
                if ((((i == POINT_NUM / 8) || (i == POINT_NUM / 8 * 3)) || (i == POINT_NUM / 8 * 5)) || (i == POINT_NUM / 8 * 7))
                    pillarPos.Add(new Vector(px * 0.8f, py * 0.8f));
                _pointPos.Add(new Vector(px, py));
                _pointDeg.Add(d);
            }
        }

        if (backToFirst)
            glVertex3f(fpx, fpy, z);
    }

    public void createSquareLoop(float s, float z, bool backToFirst = false, float yRatio = 1)
    {
        float d = 0;
        int pn = 0;
        if (backToFirst)
            pn = 4;
        else
            pn = 3;
        for (int i = 0; i <= pn; i++)
        {
            d = PI * 2 * i / 4 + PI / 4;
            float px = sin(d) * size * s;
            float py = cos(d) * size * s;
            if (py > 0)
                py = py * (yRatio);
            glVertex3f(px, py, z);
        }
    }

    public void createPillar(Vector p, float s, float z)
    {
        float d = 0;
        for (int i = 0; i < PILLAR_POINT_NUM; i++)
        {
            d = PI * 2 * i / PILLAR_POINT_NUM;
            glVertex3f(sin(d) * s + p.x, cos(d) * s + p.y, z);
        }
    }

    public void addWake(WakePool wakes, Vector pos, float deg, float spd, float sr = 1)
    {
        float sp = spd;
        if (sp > 0.1f)
            sp = 0.1f;
        float sz = size;
        if (sz > 10)
            sz = 10;
        wakePos.x = pos.x + sin(deg + PI / 2 + 0.7f) * size * 0.5f * sr;
        wakePos.y = pos.y + cos(deg + PI / 2 + 0.7f) * size * 0.5f * sr;
        Wake w = wakes.getInstanceForced();
        w.set(wakePos, deg + PI - 0.2f + rand.nextSignedFloat(0.1f), sp, 40, sz * 32 * sr);
        wakePos.x = pos.x + sin(deg - PI / 2 - 0.7f) * size * 0.5f * sr;
        wakePos.y = pos.y + cos(deg - PI / 2 - 0.7f) * size * 0.5f * sr;
        w = wakes.getInstanceForced();
        w.set(wakePos, deg + PI + 0.2f + rand.nextSignedFloat(0.1f), sp, 40, sz * 32 * sr);
    }

    public List<Vector> pointPos
    {
        get
        {
            return _pointPos;
        }

        set
        {
            _pointPos = value;
        }
    }

    public List<float> pointDeg
    {
        get
        {
            return _pointDeg;
        }

        set
        {
            _pointDeg = value;
        }
    }

    public bool checkShipCollision(float x, float y, float deg, float sr = 1)
    {
        float cs = size * (1 - distRatio) * 1.1f * sr;
        if (dist(x, y, 0, 0) < cs)
            return true;
        float ofs = 0;
        for (;;)
        {
            ofs = ofs + (cs);
            cs = cs * (distRatio);
            if (cs < 0.2f)
                return false;
            if ((dist(x, y, sin(deg) * ofs, cos(deg) * ofs) < cs) || (dist(x, y, -sin(deg) * ofs, -cos(deg) * ofs) < cs))
                return true;
        }
    }

    public float dist(float x, float y, float px, float py)
    {
        float ax = fabs(x - px);
        float ay = fabs(y - py);
        if (ax > ay)
            return ax + ay / 2;
        else
            return ay + ax / 2;
    }
}

public class CollidableBaseShape : BaseShape, Collidable
{
    public Vector getCollision()
    {
        return collisionBounds;
    }

    public bool checkCollision(float ax, float ay, Collidable shape = null)
    {
        float cx = 0, cy = 0;
        if (shape != null)
        {
            cx = collisionBounds.x + shape.getCollision().x;
            cy = collisionBounds.y + shape.getCollision().y;
        }
        else
        {
            cx = collisionBounds.x;
            cy = collisionBounds.y;
        }

        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public Vector _collision;
    public CollidableBaseShape(float size, float distRatio, float spinyRatio, int type, float r, float g, float b) : base(size, distRatio, spinyRatio, type, r, g, b)
    {
        _collision = new Vector(size / 2, size / 2);
    }

    public Vector collisionBounds
    {
        get
        {
            return _collision;
        }

        set
        {
            _collision = value;
        }
    }
}

public class TurretShape : ResizableDrawable
{
    public static List<BaseShape> shapes = new List<BaseShape>();
    public static void init()
    {
        shapes.Add(new CollidableBaseShape(1, 0, 0, BaseShapeShapeType.TURRET, 1, 0.8f, 0.8f));
        shapes.Add(new BaseShape(1, 0, 0, BaseShapeShapeType.TURRET_DAMAGED, 0.9f, 0.9f, 1));
        shapes.Add(new BaseShape(1, 0, 0, BaseShapeShapeType.TURRET_DESTROYED, 0.8f, 0.33f, 0.66f));
    }

    public static void close()
    {
        foreach (BaseShape s in shapes)
            s.close();
    }

    public TurretShape(int t)
    {
        shape = shapes[t];
    }
}

public class EnemyShape : ResizableDrawable
{
    public const float MIDDLE_COLOR_R = 1, MIDDLE_COLOR_G = 0.6f, MIDDLE_COLOR_B = 0.5f;
    public static List<BaseShape> shapes = new List<BaseShape>();
    public static void init()
    {
        shapes.Add(new BaseShape(1, 0.5f, 0.1f, BaseShapeShapeType.SHIP, 0.9f, 0.7f, 0.5f));
        shapes.Add(new BaseShape(1, 0.5f, 0.1f, BaseShapeShapeType.SHIP_DAMAGED, 0.5f, 0.5f, 0.9f));
        shapes.Add(new CollidableBaseShape(0.66f, 0, 0, BaseShapeShapeType.BRIDGE, 1, 0.2f, 0.3f));
        shapes.Add(new BaseShape(1, 0.7f, 0.33f, BaseShapeShapeType.SHIP, MIDDLE_COLOR_R, MIDDLE_COLOR_G, MIDDLE_COLOR_B));
        shapes.Add(new BaseShape(1, 0.7f, 0.33f, BaseShapeShapeType.SHIP_DAMAGED, 0.5f, 0.5f, 0.9f));
        shapes.Add(new BaseShape(1, 0.7f, 0.33f, BaseShapeShapeType.SHIP_DESTROYED, 0, 0, 0));
        shapes.Add(new CollidableBaseShape(0.66f, 0, 0, BaseShapeShapeType.BRIDGE, 1, 0.2f, 0.3f));
        shapes.Add(new BaseShape(1, 0, 0, BaseShapeShapeType.PLATFORM, 1, 0.6f, 0.7f));
        shapes.Add(new BaseShape(1, 0, 0, BaseShapeShapeType.PLATFORM_DAMAGED, 0.5f, 0.5f, 0.9f));
        shapes.Add(new BaseShape(1, 0, 0, BaseShapeShapeType.PLATFORM_DESTROYED, 1, 0.6f, 0.7f));
        shapes.Add(new CollidableBaseShape(0.5f, 0, 0, BaseShapeShapeType.BRIDGE, 1, 0.2f, 0.3f));
    }

    public static void close()
    {
        foreach (BaseShape s in shapes)
            s.close();
    }

    public EnemyShape(int t)
    {
        shape = shapes[t];
    }

    public void addWake(WakePool wakes, Vector pos, float deg, float sp)
    {
        ((BaseShape)shape).addWake(wakes, pos, deg, sp, size);
    }

    public bool checkShipCollision(float x, float y, float deg)
    {
        return ((BaseShape)shape).checkShipCollision(x, y, deg, size);
    }
}

public class BulletShape : ResizableDrawable
{
    public static List<DrawableShape> shapes = new List<DrawableShape>();
    public static void init()
    {
        shapes.Add(new NormalBulletShape());
        shapes.Add(new SmallBulletShape());
        shapes.Add(new MovingTurretBulletShape());
        shapes.Add(new DestructiveBulletShape());
    }

    public static void close()
    {
        foreach (DrawableShape s in shapes)
            s.close();
    }

    public void set(int t)
    {
        shape = shapes[t];
    }
}

public class NormalBulletShape : DrawableShape
{
    public NormalBulletShape()
    {
        initializeShape();
    }

    public override void createDisplayList()
    {
        glDisable(GL_BLEND);
        GrScreen.setColor(1, 1, 0.3f);
        glBegin(GL_LINE_STRIP);
        glVertex3f(0.2f, -0.25f, 0.2f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(-0.2f, -0.25f, -0.2f);
        glEnd();
        glBegin(GL_LINE_STRIP);
        glVertex3f(-0.2f, -0.25f, 0.2f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.2f, -0.25f, -0.2f);
        glEnd();
        glEnable(GL_BLEND);
        GrScreen.setColor(0.5f, 0.2f, 0.1f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.2f, -0.25f, 0.2f);
        glVertex3f(-0.2f, -0.25f, 0.2f);
        glVertex3f(-0.2f, -0.25f, -0.2f);
        glVertex3f(0.2f, -0.25f, -0.2f);
        glVertex3f(0.2f, -0.25f, 0.2f);
        glEnd();
    }
}

public class SmallBulletShape : DrawableShape
{
    public SmallBulletShape()
    {
        initializeShape();
    }

    public override void createDisplayList()
    {
        glDisable(GL_BLEND);
        GrScreen.setColor(0.6f, 0.9f, 0.3f);
        glBegin(GL_LINE_STRIP);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(-0.25f, -0.25f, -0.25f);
        glEnd();
        glBegin(GL_LINE_STRIP);
        glVertex3f(-0.25f, -0.25f, 0.25f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.25f, -0.25f, -0.25f);
        glEnd();
        glEnable(GL_BLEND);
        GrScreen.setColor(0.2f, 0.4f, 0.1f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glVertex3f(-0.25f, -0.25f, 0.25f);
        glVertex3f(-0.25f, -0.25f, -0.25f);
        glVertex3f(0.25f, -0.25f, -0.25f);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glEnd();
    }
}

public class MovingTurretBulletShape : DrawableShape
{
    public MovingTurretBulletShape()
    {
        initializeShape();
    }

    public override void createDisplayList()
    {
        glDisable(GL_BLEND);
        GrScreen.setColor(0.7f, 0.5f, 0.9f);
        glBegin(GL_LINE_STRIP);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(-0.25f, -0.25f, -0.25f);
        glEnd();
        glBegin(GL_LINE_STRIP);
        glVertex3f(-0.25f, -0.25f, 0.25f);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.25f, -0.25f, -0.25f);
        glEnd();
        glEnable(GL_BLEND);
        GrScreen.setColor(0.2f, 0.2f, 0.3f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0.33f, 0);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glVertex3f(-0.25f, -0.25f, 0.25f);
        glVertex3f(-0.25f, -0.25f, -0.25f);
        glVertex3f(0.25f, -0.25f, -0.25f);
        glVertex3f(0.25f, -0.25f, 0.25f);
        glEnd();
    }
}

public class DestructiveBulletShape : DrawableShape, Collidable
{
    public DestructiveBulletShape()
    {
        initializeShape();
    }

    public Vector getCollision()
    {
        return collisionBounds;
    }

    public bool checkCollision(float ax, float ay, Collidable shape = null)
    {
        float cx = 0, cy = 0;
        if (shape != null)
        {
            cx = collisionBounds.x + shape.getCollision().x;
            cy = collisionBounds.y + shape.getCollision().y;
        }
        else
        {
            cx = collisionBounds.x;
            cy = collisionBounds.y;
        }

        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public Vector _collision;
    public override void createDisplayList()
    {
        glDisable(GL_BLEND);
        GrScreen.setColor(0.9f, 0.9f, 0.6f);
        glBegin(GL_LINE_LOOP);
        glVertex3f(0.2f, 0, 0);
        glVertex3f(0, 0.4f, 0);
        glVertex3f(-0.2f, 0, 0);
        glVertex3f(0, -0.4f, 0);
        glEnd();
        glEnable(GL_BLEND);
        GrScreen.setColor(0.7f, 0.5f, 0.4f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0.2f, 0, 0);
        glVertex3f(0, 0.4f, 0);
        glVertex3f(-0.2f, 0, 0);
        glVertex3f(0, -0.4f, 0);
        glEnd();
        _collision = new Vector(0.4f, 0.4f);
    }

    public Vector collisionBounds
    {
        get
        {
            return _collision;
        }

        set
        {
            _collision = value;
        }
    }
}

public class CrystalShape : DrawableShape
{
    public CrystalShape()
    {
        initializeShape();
    }

    public override void createDisplayList()
    {
        GrScreen.setColor(0.6f, 1, 0.7f);
        glBegin(GL_LINE_LOOP);
        glVertex3f(-0.2f, 0.2f, 0);
        glVertex3f(0.2f, 0.2f, 0);
        glVertex3f(0.2f, -0.2f, 0);
        glVertex3f(-0.2f, -0.2f, 0);
        glEnd();
    }
}

public class ShieldShape : DrawableShape
{
    public ShieldShape()
    {
        initializeShape();
    }

    public override void createDisplayList()
    {
        GrScreen.setColor(0.5f, 0.5f, 0.7f);
        glBegin(GL_LINE_LOOP);
        float d = 0;
        for (int i = 0; i < 8; i++)
        {
            glVertex3f(sin(d), cos(d), 0);
            d = d + (PI / 4);
        }

        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        GrScreen.setColor(0, 0, 0);
        glVertex3f(0, 0, 0);
        d = 0;
        GrScreen.setColor(0.3f, 0.3f, 0.5f);
        for (int i = 0; i < 9; i++)
        {
            glVertex3f(sin(d), cos(d), 0);
            d = d + (PI / 4);
        }

        glEnd();
    }
}

public static class BulletShapeBulletShapeType
{
    public const int NORMAL = 0;
    public const int SMALL = 1;
    public const int MOVING_TURRET = 2;
    public const int DESTRUCTIVE = 3;
}

public static class EnemyShapeEnemyShapeType
{
    public const int SMALL = 0;
    public const int SMALL_DAMAGED = 1;
    public const int SMALL_BRIDGE = 2;
    public const int MIDDLE = 3;
    public const int MIDDLE_DAMAGED = 4;
    public const int MIDDLE_DESTROYED = 5;
    public const int MIDDLE_BRIDGE = 6;
    public const int PLATFORM = 7;
    public const int PLATFORM_DAMAGED = 8;
    public const int PLATFORM_DESTROYED = 9;
    public const int PLATFORM_BRIDGE = 10;
}

public static class TurretShapeTurretShapeType
{
    public const int NORMAL = 0;
    public const int DAMAGED = 1;
    public const int DESTROYED = 2;
}

public static class BaseShapeShapeType
{
    public const int SHIP = 0;
    public const int SHIP_ROUNDTAIL = 1;
    public const int SHIP_SHADOW = 2;
    public const int PLATFORM = 3;
    public const int TURRET = 4;
    public const int BRIDGE = 5;
    public const int SHIP_DAMAGED = 6;
    public const int SHIP_DESTROYED = 7;
    public const int PLATFORM_DAMAGED = 8;
    public const int PLATFORM_DESTROYED = 9;
    public const int TURRET_DAMAGED = 10;
    public const int TURRET_DESTROYED = 11;
}
