// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using Lub;

public class BulletActor : Actor
{
    public string meshKey;
    public bool exists;
    public BulletImpl bullet;
    public bool activated;
    public const float SIZE = 0.8f;
    public static int idCnt = 0;
    public Field field;
    public Ship ship;
    public GameManager gameManager;
    public int cnt;
    public bool isTop;
    public bool isAimTop;
    public bool isVisible;
    public bool shouldBeRemoved;
    public bool isWait;
    public int postWait;
    public int waitCnt;
    public bool isMorphSeed;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public BulletActor()
    {
        meshKey = ActorMesh.nextKey();
    }

    public bool getExists()
    {
        return exists;
    }

    public void setExists(bool exists)
    {
        this.exists = exists;
    }

    public string getMeshKey()
    {
        return meshKey;
    }

    public void init_1_(object[] args)
    {
        field = (Field)args[0];
        ship = ((args[1] is Ship ? (Ship)args[1] : null));
        gameManager = (GameManager)args[2];
        cnt = 0;
        bullet = new BulletImpl(idCnt);
        idCnt++;
        shape = new ShapeGroup();
        shape.addShape(new Square(null, 0, 0, 0, SIZE * 0.5f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(1, 0.25f, 0.5f, 0.2f);
    }

    public void set_5__Single_Single_Single_Single(PatternState[] runner, float x, float y, float deg, float speed)
    {
        bullet.set_6(runner, x, y, deg, speed, 0);
        start_0();
    }

    public void start_0()
    {
        isAimTop = false;
        isTop = isAimTop;
        isWait = false;
        isVisible = true;
        isMorphSeed = false;
        activated = true;
        cnt = 0;
        shouldBeRemoved = false;
        linePoint.init_0();
        exists = true;
    }

    public void setInvisible()
    {
        isVisible = false;
    }

    public void setTop()
    {
        isAimTop = true;
        isTop = isAimTop;
        setInvisible();
    }

    public void unsetTop()
    {
        isAimTop = false;
        isTop = isAimTop;
    }

    public void unsetAimTop()
    {
        isAimTop = false;
    }

    public void setWait(int prvw, int pstw)
    {
        isWait = true;
        waitCnt = prvw;
        postWait = pstw;
    }

    public void setMorphSeed()
    {
        isMorphSeed = true;
    }

    public void rewind()
    {
        bullet.resetParser();
        PatternState[] runner = Bullet.createRunner(bullet.getParser());
        bullet.setRunner(runner);
    }

    public void remove_0()
    {
        shouldBeRemoved = true;
    }

    public void removeForced()
    {
        bullet.remove_0();
        exists = false;
    }

    public void move_0()
    {
        Vector tpos = bullet.target.getTargetPos();
        Bullet.target.x = tpos.x;
        Bullet.target.y = tpos.y;
        if (isAimTop)
        {
            float ox = tpos.x - bullet.pos.x;
            bullet.deg = (atan2(-ox, tpos.y - bullet.pos.y) * bullet.xReverse + PI / 2) * bullet.yReverse - PI / 2;
        }

        if (isWait && waitCnt > 0)
        {
            waitCnt--;
            if (shouldBeRemoved)
                removeForced();
            return;
        }

        bullet.move_0();
        if (bullet.isEnd())
        {
            if (isTop)
            {
                rewind();
                if (isWait)
                {
                    waitCnt = postWait;
                    return;
                }
            }
            else if (isMorphSeed)
            {
                removeForced();
                return;
            }
        }

        if (shouldBeRemoved)
        {
            removeForced();
            return;
        }

        float mx = (-sin(bullet.deg) * bullet.velocity + bullet.acc.x) * bullet.getSpeedRank() * bullet.xReverse;
        float my = (cos(bullet.deg) * bullet.velocity - bullet.acc.y) * bullet.getSpeedRank() * bullet.yReverse;
        bullet.pos.x += mx;
        bullet.pos.y += my;
        if (isVisible)
        {
            if (!(field.checkInField_1_Vector(bullet.pos)))
                removeForced();
        }

        cnt++;
        if (!(isTop) && cnt > 600)
            removeForced();
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
    }

    public void slowdown()
    {
        bullet.slowdown();
        postWait *= 2;
    }
}

public class SimpleBullet : Actor, OdeActor
{
    public string meshKey;
    public bool exists;
    public OdeBody ode;
    public const float SIZE = 0.8f;
    public const float MASS = 500;
    public const float FORCE = 200000;
    public static Rand rand;
    public Field field;
    public Ship ship;
    public GameManager gameManager;
    public ParticlePool particles;
    public Vector pos;
    public Vector firstForce;
    public float deg;
    public float speed;
    public int removeCnt;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public int cnt;
    public SimpleBullet()
    {
        meshKey = ActorMesh.nextKey();
        ode = new OdeBody(this);
    }

    public OdeBody getOde()
    {
        return ode;
    }

    public bool getExists()
    {
        return exists;
    }

    public void setExists(bool exists)
    {
        this.exists = exists;
    }

    public string getMeshKey()
    {
        return meshKey;
    }

    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public void init_1_(object[] args)
    {
        ode.init_1_Boolean();
        field = (Field)args[0];
        ship = ((args[1] is Ship ? (Ship)args[1] : null));
        gameManager = (GameManager)args[2];
        particles = (ParticlePool)args[3];
        pos = new Vector();
        deg = 0;
        speed = 1;
        firstForce = new Vector();
        shape = new ShapeGroup();
        shape.addShape(new Square(ode.world, MASS, 0, 0, SIZE * 0.5f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(1, 0.25f, 0.5f, 0.2f);
    }

    public void set_4_Single_Single_Single_Single(float x, float y, float d, float sp)
    {
        ode.set_1();
        pos.x = x;
        pos.y = y;
        McdPhysics.BodyPosition(ode._bodyId, x, y, 0);
        deg = d;
        ode.setDeg(d);
        speed = sp;
        shape.setMass_3(this);
        shape.setGeom(this, null);
        firstForce.x = -sin(d) * sp * FORCE;
        firstForce.y = cos(d) * sp * FORCE;
        ode.addForce(firstForce.x, firstForce.y);
        removeCnt = 0;
        cnt = 0;
        linePoint.init_0();
    }

    public void move_0()
    {
        float[] p = McdPhysics.BodyVector(ode._bodyId, 0);
        pos.x = p[0];
        pos.y = p[1];
        McdPhysics.BodyPosition(ode._bodyId, pos.x, pos.y, 0);
        if (removeCnt > 0)
        {
            removeCnt++;
            if (removeCnt > 5)
            {
                remove_0();
                return;
            }
        }

        if (!(field.checkInField_1_Vector(pos)))
        {
            remove_0();
            return;
        }

        recordLinePoints_0();
        ode.doCollide();
        cnt++;
        if (cnt > 300)
        {
            remove_0();
            return;
        }

        float[] lv = ode.getLinearVel();
        if (fabs(lv[0]) + fabs(lv[1]) < 1)
        {
            remove_0();
            return;
        }
    }

    public void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        Enemy e = ((actor is Enemy ? (Enemy)actor : null));
        if ((((actor is Ship ? (Ship)actor : null))) != null || (((actor is ShipTail ? (ShipTail)actor : null))) != null || ((e) != null && e.collideBullet()))
        {
            if (removeCnt <= 0)
            {
                removeCnt = 1;
                for (int i = 0; i < 5; i++)
                {
                    Particle p = particles.getInstanceForced();
                    float d = deg + PI + rand.nextSignedFloat(0.5f);
                    float v = speed * (0.5f + rand.nextFloat(0.5f));
                    p.set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(pos, -sin(d) * v, cos(d) * v, 0.2f + rand.nextFloat(0.2f), 1, 0.5f, 0.5f);
                }

                if ((((actor is Ship ? (Ship)actor : null))) != null || (((actor is ShipTail ? (ShipTail)actor : null))) != null)
                {
                    gameManager.addScore_1(10);
                    SoundManager.playSe("bullethit.wav");
                }
            }

            flags.hit = true;
            return;
        }
    }

    public void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord(model);
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        model = parent1;
    }

    public void collapseIntoParticle()
    {
        Particle p = particles.getInstanceForced();
        float d = deg;
        float v = speed;
        p.set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(pos, -sin(d) * v, cos(d) * v, 0.2f, 0.9f, 0.6f, 0.3f);
        remove_0();
        if (!(ship.inRestartBulletDisap()))
            gameManager.addScore_1(10);
    }

    public void drawSpectrum(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        if (removeCnt > 0)
            return;
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
    }

    public void drawShadow_0(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        if (removeCnt > 0)
            return;
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", linePoint);
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (removeCnt > 0)
            return;
        linePoint.draw(model, tint, blend, key + "-draw-1");
    }

    public void slowdown()
    {
        if (removeCnt > 0)
            return;
        ode.addForce(-firstForce.x / 2, -firstForce.y / 2);
    }

    public void clearContactJoint()
    {
        ode.clearContactJoint();
    }

    public void checkFeedbackForce()
    {
    }

    public void remove_0()
    {
        ode.remove_0();
    }
}
