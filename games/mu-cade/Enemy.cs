// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Enemy : OdeActor
{
    public static Rand rand = new Rand();
    public Field field;
    public ParticlePool particles;
    public ConnectedParticlePool connectedParticles;
    public TailParticlePool tailParticles;
    public NumIndicatorPool numIndicators;
    public Ship ship;
    public GameManager gameManager;
    public EnemySpec spec;
    public EnemyState state;
    public Vector3 lastForce;
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
        base.init_1_Boolean(true);
        field = (Field)args[0];
        particles = (ParticlePool)args[1];
        connectedParticles = (ConnectedParticlePool)args[2];
        tailParticles = (TailParticlePool)args[3];
        numIndicators = (NumIndicatorPool)args[4];
        ship = ((args[5] is Ship ? (Ship)args[5] : null));
        gameManager = (GameManager)args[6];
        state = new EnemyState(args);
        lastForce = new Vector3();
    }

    public virtual bool set_8_EnemySpec_Single_Single_Single_Single_Vector3_Single_Int32(EnemySpec spec, float x, float y, float z, float deg, Vector3 sizeScale = null, float massScale = 1, int type = 0)
    {
        if ((sizeScale) != null)
            return set_10(spec, x, y, z, deg, sizeScale.x, sizeScale.y, sizeScale.z, massScale, type);
        else
            return set_10(spec, x, y, z, deg, 1, 1, 1, massScale, type);
    }

    public virtual bool set_10(EnemySpec spec, float x, float y, float z, float deg, float sx, float sy, float sz, float massScale = 1, int type = 0)
    {
        base.set_1();
        this.spec = spec;
        state.clear();
        state.pos.x = x;
        state.pos.y = y;
        state.pos.z = z;
        state.deg = deg;
        state.sizeScale.x = sx;
        state.sizeScale.y = sy;
        state.sizeScale.z = sz;
        state.massScale = massScale;
        state.type = type;
        lastForce.z = 0;
        lastForce.y = lastForce.z;
        lastForce.x = lastForce.y;
        McdPhysics.BodyPosition(_bodyId, x, y, z);
        if (spec.rotate2d())
            setDeg(deg);
        spec.initState(this, state);
        state.linePoint.alpha(0);
        state.linePoint.alphaTrg(1);
        return true;
    }

    public virtual void setJointedEnemies_1(Enemy[] enemies)
    {
        state.jointedEnemies = enemies;
    }

    public virtual void setJointedEnemiesPrevNext(Enemy pe, Enemy ne)
    {
        state.prevJointedEnemy = pe;
        state.nextJointedEnemy = ne;
    }

    public virtual void setJoints(OdeHandle[] joints)
    {
        state.joints = joints;
    }

    public override void move_0()
    {
        if (checkDestroyed())
            return;
        updateState();
        if (!(spec.move_2(this, state)))
        {
            remove_0();
            return;
        }

        Vector3 f = getForce();
        lastForce.x = f.x;
        lastForce.y = f.y;
        lastForce.z = f.z;
        if (spec.rotate2d() && field.checkInField_1_Vector3(state.pos))
            setDeg(state.deg);
        spec.recordLinePoints_2(state, state.linePoint);
    }

    public override void remove_0()
    {
        removeCleaning();
        base.remove_0();
    }

    public virtual void removeAsTail(int idx)
    {
        TailParticle tp = tailParticles.getInstance();
        if ((tp) != null)
            tp.set_8_Single_Single_Single_Single_Single_Single_Single_Int32(state.pos.x, state.pos.y, state.pos.z, state.sizeScale.x, spec.colorR(), spec.colorG(), spec.colorB(), GameMath.integer((30 + 30.0f / (idx + 1))));
        (((ConnectedParticlesBodyAddable)spec)).addConnectedParticlesBody(this, state);
        remove_0();
    }

    public virtual void removeCleaning()
    {
        state.removeAllJoints();
        if ((state.topBullet) != null)
        {
            state.topBullet.removeForced();
            state.topBullet = null;
        }
    }

    public virtual void updateState()
    {
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        state.pos.x = p[0];
        state.pos.y = p[1];
        state.pos.z = p[2];
        state.deg = getDeg();
        getRot(state.rot);
        float[] lv = getLinearVel();
        float[] av = getAngularVel();
        for (int i = 0; i < 3; i++)
        {
            state.linearVel[i] = lv[i];
            state.angularVel[i] = av[i];
        }

        if ((state.topBullet) != null)
        {
            state.topBullet.bullet.pos.x = state.pos.x;
            state.topBullet.bullet.pos.y = state.pos.y;
            if (state.setTopBulletDirection)
                state.topBullet.bullet.deg = state.deg;
            if (state.setPlumbDirection)
            {
                float td = atan2(-ship.pos().x + state.pos.x, ship.pos().y - state.pos.y);
                td = normalizeDeg(td);
                if (td < -PI * 3 / 4)
                    td = PI;
                else if (td < -PI / 4)
                    td = -PI / 2;
                else if (td < PI / 4)
                    td = 0;
                else if (td < PI * 3 / 4)
                    td = PI / 2;
                else
                    td = PI;
                state.topBullet.bullet.deg = td;
            }
        }
    }

    public override void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        if (!(exists))
            return;
        Enemy ce = ((actor is Enemy ? (Enemy)actor : null));
        if ((ce) != null)
            if (ce == state.prevJointedEnemy || ce == state.nextJointedEnemy)
                return;
        if ((((actor is SimpleBullet ? (SimpleBullet)actor : null))) != null)
            return;
        flags.hit = true;
        flags.feedback = true;
        spec.collide_3(this, state, actor);
    }

    public override void checkFeedbackForce()
    {
        if (contactJointNum <= 0)
            return;
        getFeedbackForce();
        for (int i = 0; i < contactJointNum; i++)
        {
            ContactJoint cj = contactJoint[i];
            Vector3 ff = cj.feedbackForce;
            ff.x += lastForce.x * 0.9f;
            ff.y += lastForce.y * 0.9f;
            int pn = GameMath.integer(((fabs(ff.x) + fabs(ff.y)) * 0.01f));
            float bv = pn * 0.1f;
            if (pn <= 0)
                continue;
            if (pn > 3)
                pn = 3;
            float pd = atan2(-ff.x, ff.y) + PI;
            for (int j = 0; j < pn; j++)
            {
                Particle p = particles.getInstanceForced();
                float d = pd + PI + rand.nextSignedFloat(1.0f);
                float v = bv * (1 + rand.nextSignedFloat(0.3f));
                p.set_8_Vector3_Single_Single_Single_Single_Single_Single_Int32(cj.pos, -sin(d) * v, cos(d) * v, 0.3f + rand.nextFloat(0.3f), 0.3f, 0.3f + rand.nextFloat(0.3f), 0.4f + rand.nextFloat(0.4f), 30 + rand.nextInt(10));
            }
        }
    }

    public virtual bool checkDestroyed()
    {
        if (state.destroyable && state.pos.z < -10)
        {
            removeCleaning();
            removeBodyAndGeom();
            spec.destroyed_2(this, state);
            removeExistence();
            SoundManager.playSe("destroyed.wav");
            return true;
        }

        return false;
    }

    public virtual void addScore_2(int sc, float sz = 0.5f)
    {
        gameManager.addScore_1(sc * ship.getMultiplier());
        NumIndicator ni = numIndicators.getInstance();
        if (!((ni) != null))
            return;
        float vx = 0, vy = 0;
        if (state.pos.x < -field.size().x)
            vx = 1;
        else if (state.pos.x > field.size().x)
            vx = -1;
        if (state.pos.y < -field.size().y)
            vy = 1;
        else if (state.pos.y > field.size().y)
            vy = -1;
        if (vx != 0 && vy != 0)
        {
            vx *= 0.8f;
            vy *= 0.6f;
        }

        ni.set_8_Int32_Int32_Single_Single_Single_Single_Single_Int32(sc, ship.getMultiplier(), state.pos.x, state.pos.y, vx * 0.3f, vy * 0.3f, sz);
    }

    public virtual void addConnectedParticles_3_Single_Single_Boolean(float deg, float speed = 1, bool rot = true)
    {
        ConnectedParticle[] cps = connectedParticles.getMultipleInstances(9);
        if (!((cps) != null))
            return;
        float d = deg - PI / 8;
        ConnectedParticle pcp = null;
        int c = 60 + rand.nextInt(60);
        foreach (ConnectedParticle cp in cps)
        {
            cp.set_12(state.pos.x, state.pos.y, state.pos.z, d, (0.2f + rand.nextSignedFloat(0.08f)) * speed, 0.75f + rand.nextFloat(0.25f), 0.25f + rand.nextFloat(0.75f), 0, c, 1, pcp, true);
            if (rot)
                cp.setRot(state.rot);
            d += PI / 4 / 8 + rand.nextSignedFloat(PI / 4 / 8 / 4);
            pcp = cp;
        }
    }

    public virtual Vector3 pos()
    {
        return state.pos;
    }

    public virtual void drawSpectrum()
    {
        state.linePoint.drawSpectrum();
    }

    public virtual bool collideBullet()
    {
        return spec.collideBullet();
    }

    public virtual void drawShadow_0()
    {
        spec.drawShadow_1(state.linePoint);
    }

    public override void draw()
    {
        state.linePoint.draw();
        spec.drawSubShape(state);
    }

    public virtual EnemyState getState_0()
    {
        return state;
    }

    public virtual bool isCentipedeHead()
    {
        if ((((spec is CentHead ? (CentHead)spec : null))) != null)
            return true;
        else
            return false;
    }

    public virtual bool isBlock()
    {
        if ((((spec is Block ? (Block)spec : null))) != null)
            return true;
        else
            return false;
    }

    public virtual void slowdown()
    {
        state.slowdown();
    }
}

public class EnemyPool : OdeActorPool<Enemy>
{
    public EnemyPool(int n, object[] args) : base(n, args, () => new Enemy())
    {
    }

    public virtual void drawShadow_0()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        foreach (Enemy e in actor)
            if (e.exists)
                e.drawShadow_0();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }

    public virtual void drawSpectrum()
    {
        foreach (Enemy e in actor)
            if (e.exists)
                e.drawSpectrum();
    }

    public virtual bool exists()
    {
        foreach (Enemy e in actor)
            if (e.exists)
                return true;
        return false;
    }

    public virtual int countCentipedes()
    {
        int c = 0;
        foreach (Enemy e in actor)
            if (e.exists && e.isCentipedeHead())
                c++;
        return c;
    }

    public virtual int countBlocks()
    {
        int c = 0;
        foreach (Enemy e in actor)
            if (e.exists && e.isBlock())
                c++;
        return c;
    }

    public virtual void slowdown()
    {
        foreach (Enemy e in actor)
            if (e.exists)
                e.slowdown();
    }
}

public class EnemyState
{
    public Vector3 pos;
    public float deg;
    public float[] rot = McdArrays.Make<float>(16, () => 0);
    public float[] linearVel = McdArrays.Make<float>(3, () => 0);
    public float[] angularVel = McdArrays.Make<float>(3, () => 0);
    public Vector3 sizeScale;
    public float massScale;
    public bool destroyable;
    public int cnt;
    public int trgDeg;
    public int turnCnt;
    public int moveFlag;
    public bool isHead;
    public int type;
    public float forwardForceScale;
    public float slowVelocityRatio;
    public Enemy[] jointedEnemies;
    public Enemy prevJointedEnemy;
    public Enemy nextJointedEnemy;
    public OdeHandle[] joints;
    public Barrage barrage;
    public BulletActor topBullet;
    public bool setTopBulletDirection;
    public bool setPlumbDirection;
    public LinePoint linePoint;
    public const int MAX_LINE_POINT_NUM = 24;
    public Field field;
    public EnemyState(object[] args)
    {
        field = (Field)args[0];
        pos = new Vector3();
        sizeScale = new Vector3();
        linePoint = new LinePoint(field, MAX_LINE_POINT_NUM);
        barrage = new Barrage();
        clearState();
    }

    public virtual void clearState()
    {
        pos.z = 0;
        pos.y = pos.z;
        pos.x = pos.y;
        deg = 0;
        for (int i_0 = 0; i_0 < 16; i_0++)
            rot[i_0] = 0;
        rot[15] = 1;
        rot[10] = rot[15];
        rot[5] = rot[10];
        rot[0] = rot[5];
        for (int i_1 = 0; i_1 < 3; i_1++)
        {
            angularVel[i_1] = 0;
            linearVel[i_1] = angularVel[i_1];
        }

        sizeScale.z = 1;
        sizeScale.y = sizeScale.z;
        sizeScale.x = sizeScale.y;
        massScale = 1;
        destroyable = true;
        cnt = 0;
        trgDeg = 0;
        turnCnt = 0;
        moveFlag = 0;
        isHead = false;
        type = 0;
        forwardForceScale = 1;
        slowVelocityRatio = 1;
        jointedEnemies = null;
        nextJointedEnemy = null;
        prevJointedEnemy = nextJointedEnemy;
        joints = null;
        barrage.clear();
        topBullet = null;
        setTopBulletDirection = false;
        setPlumbDirection = false;
        linePoint.init_0();
    }

    public virtual void clear()
    {
        clearState();
    }

    public virtual void removeAllJoints()
    {
        if ((joints) != null)
            foreach (OdeHandle j in joints)
                McdPhysics.JointDestroy(j);
        joints = null;
    }

    public virtual void slowdown()
    {
        forwardForceScale *= 0.5f;
        if ((topBullet) != null)
            topBullet.slowdown();
    }
}

public abstract class EnemySpec
{
    public static Rand rand = new Rand();
    public Field field;
    public Ship ship;
    public BulletPool bullets;
    public World world;
    public ShapeGroup shape;
    public Drawable subShape = null;
    public bool _rotate2d = true;
    public bool _collideBullet = false;
    public float _colorR, _colorG, _colorB;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public virtual void initState(Enemy enemy, EnemyState state)
    {
        shape.setMass_3(enemy, state.sizeScale, state.massScale);
        shape.setGeom(enemy, world.space, state.sizeScale);
    }

    public abstract bool move_2(Enemy enemy, EnemyState state);
    public virtual void destroyed_2(Enemy enemy, EnemyState state)
    {
    }

    public virtual void collide_3(Enemy enemy, EnemyState state, OdeActor actor)
    {
    }

    public virtual void recordLinePoints_2(EnemyState state, LinePoint lp)
    {
        glPushMatrix();
        Screen.glTranslate_1_Vector3(state.pos);
        glMultMatrix(state.rot);
        glScalef(state.sizeScale.x, state.sizeScale.y, state.sizeScale.z);
        lp.beginRecord();
        shape.recordLinePoints_1(lp);
        lp.endRecord();
        glPopMatrix();
    }

    public virtual void drawShadow_1(LinePoint lp)
    {
        shape.drawShadow_1(lp);
    }

    public virtual void drawSubShape(EnemyState state)
    {
    }

    public virtual bool rotate2d()
    {
        return _rotate2d;
    }

    public virtual bool collideBullet()
    {
        return _collideBullet;
    }

    public virtual float colorR()
    {
        return _colorR;
    }

    public virtual float colorG()
    {
        return _colorG;
    }

    public virtual float colorB()
    {
        return _colorB;
    }
}

public interface JointedEnemySpec
{
    public Enemy setJointedEnemies_5(EnemyPool enemies, float x, float y, float z, float deg);
}
