// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class BulletActor : Actor
{
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
    public override void init_1_(object[] args)
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

    public virtual void set_5__Single_Single_Single_Single(PatternState[] runner, float x, float y, float deg, float speed)
    {
        bullet.set_6(runner, x, y, deg, speed, 0);
        start_0();
    }

    public virtual void start_0()
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

    public virtual void setInvisible()
    {
        isVisible = false;
    }

    public virtual void setTop()
    {
        isAimTop = true;
        isTop = isAimTop;
        setInvisible();
    }

    public virtual void unsetTop()
    {
        isAimTop = false;
        isTop = isAimTop;
    }

    public virtual void unsetAimTop()
    {
        isAimTop = false;
    }

    public virtual void setWait(int prvw, int pstw)
    {
        isWait = true;
        waitCnt = prvw;
        postWait = pstw;
    }

    public virtual void setMorphSeed()
    {
        isMorphSeed = true;
    }

    public virtual void rewind()
    {
        bullet.resetParser();
        PatternState[] runner = Bullet.createRunner(bullet.getParser());
        bullet.setRunner(runner);
    }

    public virtual void remove_0()
    {
        shouldBeRemoved = true;
    }

    public virtual void removeForced()
    {
        bullet.remove_0();
        exists = false;
    }

    public override void move_0()
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

    public override void draw()
    {
    }

    public virtual void slowdown()
    {
        bullet.slowdown();
        postWait *= 2;
    }
}

public class SimpleBullet : OdeActor
{
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
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1_(object[] args)
    {
        base.init_1_Boolean();
        field = (Field)args[0];
        ship = ((args[1] is Ship ? (Ship)args[1] : null));
        gameManager = (GameManager)args[2];
        particles = (ParticlePool)args[3];
        pos = new Vector();
        deg = 0;
        speed = 1;
        firstForce = new Vector();
        shape = new ShapeGroup();
        shape.addShape(new Square(world, MASS, 0, 0, SIZE * 0.5f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(1, 0.25f, 0.5f, 0.2f);
    }

    public virtual void set_4_Single_Single_Single_Single(float x, float y, float d, float sp)
    {
        base.set_1();
        pos.x = x;
        pos.y = y;
        McdPhysics.BodyPosition(_bodyId, x, y, 0);
        deg = d;
        setDeg(d);
        speed = sp;
        shape.setMass_3(this);
        shape.setGeom(this, null);
        firstForce.x = -sin(d) * sp * FORCE;
        firstForce.y = cos(d) * sp * FORCE;
        addForce(firstForce.x, firstForce.y);
        removeCnt = 0;
        cnt = 0;
        linePoint.init_0();
    }

    public override void move_0()
    {
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        pos.x = p[0];
        pos.y = p[1];
        McdPhysics.BodyPosition(_bodyId, pos.x, pos.y, 0);
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
        doCollide();
        cnt++;
        if (cnt > 300)
        {
            remove_0();
            return;
        }

        float[] lv = getLinearVel();
        if (fabs(lv[0]) + fabs(lv[1]) < 1)
        {
            remove_0();
            return;
        }
    }

    public override void collide_2(OdeActor actor, Collision flags)
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

    public virtual void recordLinePoints_0()
    {
        glPushMatrix();
        Screen.glTranslate_1_Vector(pos);
        glRotatef(deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord();
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        glPopMatrix();
    }

    public virtual void collapseIntoParticle()
    {
        Particle p = particles.getInstanceForced();
        float d = deg;
        float v = speed;
        p.set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(pos, -sin(d) * v, cos(d) * v, 0.2f, 0.9f, 0.6f, 0.3f);
        remove_0();
        if (!(ship.inRestartBulletDisap()))
            gameManager.addScore_1(10);
    }

    public virtual void drawSpectrum()
    {
        if (removeCnt > 0)
            return;
        linePoint.drawSpectrum();
    }

    public virtual void drawShadow_0()
    {
        if (removeCnt > 0)
            return;
        shape.drawShadow_1(linePoint);
    }

    public override void draw()
    {
        if (removeCnt > 0)
            return;
        linePoint.draw();
    }

    public virtual void slowdown()
    {
        if (removeCnt > 0)
            return;
        addForce(-firstForce.x / 2, -firstForce.y / 2);
    }
}
