// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Ship : OdeActor, BulletTarget
{
    public const int RESTART_CNT_CONST = 72;
    public const int FIRE_INTERVAL_CONST = 2;
    public const int FIRE_INTERVAL_MAX = 4;
    public const int SHOT_MAX_NUM = 15;
    public const int ENHANCED_SHOT_MAX_NUM = 24;
    public const int TAIL_MAX_NUM = 63;
    public const float SLIDE_FORCE_BASE = 10;
    public const float ANGULAR_FORCE_BASE = 10;
    public const float SIZE = 1.0f;
    public const float MASS = 3;
    public const float TURN_RATIO_BASE = 0.2f;
    public const float TURN_CHANGE_RATIO = 0.5f;
    public const float FIX_CHANGE_RATIO = 0.2f;
    public const float SLOW_VELOCITY_RATIO = 0.01f;
    public const float SLOW_ANGULAR_RATIO = 0.1f;
    public static Rand rand;
    public RecordableTwinStickPad pad;
    public Field field;
    public Screen screen;
    public ParticlePool particles;
    public ConnectedParticlePool connectedParticles;
    public GameManager gameManager;
    public BulletPool bullets;
    public EnhancedShotPool enhancedShots;
    public Vector3 _pos;
    public Vector trgPos;
    public Vector slideVel;
    public ShotPool shots;
    public float deg;
    public float trgDeg;
    public float[] rot = McdArrays.Make<float>(16, () => 0);
    public int restartCnt;
    public int fireCnt;
    public float fireInterval;
    public ShipTail[] tails = McdArrays.Make<ShipTail>(TAIL_MAX_NUM, () => default);
    public int tailNum;
    public int enhancedShotCnt;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public Drawable subShape;
    public bool aPressed, bPressed, gsaPressed;
    public int bulletDisapCnt;
    public int restartBulletDisapCnt;
    public int titleCnt;
    public bool _replayMode;
    public static void init_0()
    {
        rand = new Rand();
        ShipTail.init_0();
        Shot.init_0();
        EnhancedShot.init_0();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Ship(World world, TwinStickPad pad, Field field, Screen screen, ParticlePool particles, ConnectedParticlePool connectedParticles, GameManager gameManager)
    {
        setWorld(world);
        this.pad = (RecordableTwinStickPad)pad;
        this.field = field;
        this.screen = screen;
        this.particles = particles;
        this.connectedParticles = connectedParticles;
        this.gameManager = gameManager;
        _pos = new Vector3();
        trgPos = new Vector();
        slideVel = new Vector();
        deg = 0;
        trgDeg = 0;
        object[] sargs = default(object[]);
        sargs = McdArrays.Append(sargs, field);
        sargs = McdArrays.Append(sargs, particles);
        shots = new ShotPool(SHOT_MAX_NUM, sargs);
        shots.init_1_World(world);
        enhancedShots = new EnhancedShotPool(ENHANCED_SHOT_MAX_NUM, sargs);
        enhancedShots.init_1_World(world);
        shape = new ShapeGroup();
        shape.addShape(new Triangle(world, MASS, 0, 0, SIZE * 0.5f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(0, 1.0f, 0, 1.0f);
        subShape = new CenterShape();
        for (int idx_t = 0; idx_t < TAIL_MAX_NUM; idx_t++)
            tails[idx_t] = new ShipTail(world, field, this, particles, connectedParticles);
        base.init_1_Boolean();
    }

    public virtual void setBullets(BulletPool bullets)
    {
        this.bullets = bullets;
    }

    public override void init_1_(object[] args)
    {
    }

    public virtual void initMassAndGeom()
    {
        base.set_1();
        shape.setMass_3(this);
        shape.setGeom(this, world.space);
    }

    public virtual void start_0()
    {
        initMassAndGeom();
        restart();
        _pos.z = 2;
        McdPhysics.BodyPosition(_bodyId, _pos.x, _pos.y, _pos.z);
        restartCnt = 0;
        bulletDisapCnt = 0;
        restartBulletDisapCnt = bulletDisapCnt;
        titleCnt = 0;
        bPressed = true;
        aPressed = bPressed;
    }

    public virtual void restart()
    {
        fireCnt = 99999;
        fireInterval = 99999;
        tailNum = 0;
        enhancedShotCnt = 0;
        bulletDisapCnt = 200;
        restartBulletDisapCnt = bulletDisapCnt + 1;
        trgDeg = 0;
        trgPos.y = 0;
        trgPos.x = trgPos.y;
        slideVel.y = 0;
        slideVel.x = slideVel.y;
        reset();
        _pos.x = 0;
        _pos.y = 0;
        _pos.z = 5;
        deg = 0;
        McdPhysics.BodyPosition(_bodyId, _pos.x, _pos.y, _pos.z);
        setDeg(deg);
    }

    public virtual void clear()
    {
        shots.clear();
        enhancedShots.clear();
        removeAllTailsWithoutParticles();
        remove_0();
    }

    public override void move_0()
    {
        TwinStickPadState input = default(TwinStickPadState);
        if (!(_replayMode))
        {
            input = pad.getState_1();
        }
        else
        {
            input = pad.replay();
            if (input == null)
            {
                gameManager.restartTitle();
                return;
            }
        }

        if (gameManager.isGameOver_0())
        {
            if ((input.button & TwinStickPadStateButton.A) != 0)
            {
                if (!(aPressed))
                    gameManager.backToTitle();
                aPressed = true;
            }
            else
            {
                aPressed = false;
            }

            return;
        }

        restartCnt--;
        if (restartCnt > 0)
            return;
        if (restartCnt == 0)
            restart();
        restartBulletDisapCnt--;
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        _pos.x = p[0];
        _pos.y = p[1];
        _pos.z = p[2];
        if ((input.button & TwinStickPadStateButton.B) != 0)
        {
            if (tailNum > 0 && !(bPressed))
            {
                slowLinearVel(SLOW_VELOCITY_RATIO * 10);
                bulletDisapCnt = getMultiplier() * 5;
                enhancedShotCnt = bulletDisapCnt;
                restartBulletDisapCnt = 0;
                removeAllTails();
                SoundManager.playSe("breaktail.wav");
            }

            bPressed = true;
        }
        else
        {
            bPressed = false;
        }

        if (_pos.z < -1)
            input.clear();
        slideVel.x = input.left.x;
        slideVel.y = input.left.y;
        if (slideVel.vctSize() > 1)
            slideVel /= slideVel.vctSize();
        slideVel *= SLIDE_FORCE_BASE;
        addForce(slideVel.x, slideVel.y);
        deg = getDeg();
        getRot(rot);
        deg = normalizeDeg(deg);
        float ad = default(float);
        bool adjustDeg = false;
        if (((input.button & TwinStickPadStateButton.A)) != 0 || (input.right.x != 0 || input.right.y != 0))
        {
            fireInterval = FIRE_INTERVAL_CONST;
            if (!(aPressed))
            {
                fireCnt = 0;
                aPressed = true;
                trgDeg = deg;
            }

            if (input.right.x != 0 || input.right.y != 0)
                trgDeg = atan2(-input.right.x, input.right.y);
            ad = trgDeg;
            adjustDeg = true;
        }
        else
        {
            aPressed = false;
            fireInterval *= 1.033f;
            if (fireInterval > FIRE_INTERVAL_MAX)
                fireInterval = 99999;
            if (slideVel.x != 0 || slideVel.y != 0)
            {
                ad = atan2(-slideVel.x, slideVel.y);
                ad = normalizeDeg(ad);
                adjustDeg = true;
            }
        }

        if (adjustDeg)
        {
            ad -= deg;
            ad = normalizeDeg(ad);
            float sf = fabs(ad) * 2;
            if (sf > 1)
                sf = 1;
            if (ad > 0.001f)
                addRelForceAtRelPos(0, SIZE * 0.5f, 0, ANGULAR_FORCE_BASE * sf, 0, 0);
            else if (ad < -0.001f)
                addRelForceAtRelPos(0, SIZE * 0.5f, 0, -ANGULAR_FORCE_BASE * sf, 0, 0);
            deg += ad * 0.05f;
        }

        if (field.checkInField_1_Vector3(_pos))
        {
            setDeg(deg);
            linePoint.enableSpectrumColor(true);
        }
        else
        {
            linePoint.enableSpectrumColor(false);
        }

        addForce(0, 0, -Field.GRAVITY);
        slowLinearVel(SLOW_VELOCITY_RATIO);
        slowAngularVel(SLOW_ANGULAR_RATIO);
        if (_pos.z < -10)
            destroyed_0();
        if (fireCnt <= 0)
        {
            if (fabs(_pos.z) < 1)
                fireShot(deg);
            fireCnt = GameMath.integer(fireInterval);
        }

        fireCnt--;
        shots.move_0();
        enhancedShots.move_0();
        for (int i = 0; i < tailNum; i++)
            tails[i].move_0();
        if (tailNum <= 0)
            TailParticle.setTarget(_pos, deg);
        else
            tails[tailNum - 1].setTailParticleTarget();
        recordLinePoints_0();
        if (bulletDisapCnt > 0)
        {
            bulletDisapCnt--;
            bullets.collapseIntoParticle();
        }

        if (enhancedShotCnt > 0)
            enhancedShotCnt--;
    }

    public virtual void fireShot(float deg)
    {
        if (enhancedShotCnt > 0)
        {
            EnhancedShot es = enhancedShots.getInstance();
            if (!((es) != null))
                return;
            es.set_2(_pos, deg);
            SoundManager.playSe("enhancedshot.wav");
        }
        else
        {
            Shot s = shots.getInstance();
            if (!((s) != null))
                return;
            s.set_2(_pos, deg);
            SoundManager.playSe("shot.wav");
        }
    }

    public override void clearContactJoint()
    {
        base.clearContactJoint();
        foreach (ShipTail ss in tails)
            ss.clearContactJoint();
    }

    public virtual void addTail_1(float size)
    {
        if (restartCnt > 0 || !(field.checkInField_1_Vector3(_pos)))
            return;
        if (tailNum <= 0)
        {
            float id = (SIZE + size) * ShipTail.TAIL_INTERVAL;
            tails[0].set_6(_pos.x + sin(deg) * id, _pos.y - cos(deg) * id, _pos.z, deg, size, _bodyId);
            tailNum++;
            SoundManager.playSe("addtail.wav");
        }
        else if (tailNum < TAIL_MAX_NUM)
        {
            if (tails[tailNum - 1].addTail_2(tails[tailNum], size))
            {
                tailNum++;
                SoundManager.playSe("addtail.wav");
            }
        }
    }

    public override void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        if ((((actor is Wall ? (Wall)actor : null))) != null)
            flags.hit = true;
    }

    public virtual void destroyed_0()
    {
        if (restartCnt > 0)
            return;
        gameManager.shipDestroyed();
        for (int i_0 = 0; i_0 < 16; i_0++)
            addConnectedParticles_3_Vector3_Single_Single(_pos, rand.nextSignedFloat(PI), 0.25f + rand.nextFloat(3));
        for (int i_1 = 0; i_1 < 64; i_1++)
        {
            Particle p = particles.getInstanceForced();
            float d = rand.nextSignedFloat(PI);
            float v = 0.1f + rand.nextFloat(0.3f);
            p.set_8_Vector3_Single_Single_Single_Single_Single_Single_Int32(_pos, -sin(d) * v, cos(d) * v, 0.4f + rand.nextFloat(0.4f), 0.25f + rand.nextFloat(0.25f), 0.75f + rand.nextFloat(0.25f), 0.25f + rand.nextFloat(0.25f));
        }

        removeAllTails();
        restartCnt = RESTART_CNT_CONST;
        SoundManager.playSe("shipdestroyed.wav");
    }

    public virtual void removeAllTails()
    {
        for (int i = 0; i < tailNum; i++)
            tails[i].remove_0();
        tailNum = 0;
    }

    public virtual void removeAllTailsWithoutParticles()
    {
        for (int i = 0; i < tailNum; i++)
            tails[i].removeWithoutParticles();
        tailNum = 0;
    }

    public virtual void moveInTitle()
    {
        titleCnt++;
        float tcr = (float)(titleCnt % 600) / 600;
        if (tcr < 0.3f)
        {
            _pos.x = (tcr - 0.15f) / 0.15f * field.size().x;
            _pos.y = -field.size().y;
        }
        else if (tcr < 0.5f)
        {
            _pos.x = field.size().x;
            _pos.y = (tcr - 0.4f) / 0.1f * field.size().y;
        }
        else if (tcr < 0.8f)
        {
            _pos.x = -(tcr - 0.65f) / 0.15f * field.size().x;
            _pos.y = field.size().y;
        }
        else
        {
            _pos.x = -field.size().x;
            _pos.y = -(tcr - 0.9f) / 0.1f * field.size().y;
        }

        _pos.z = 5;
        McdPhysics.BodyPosition(_bodyId, _pos.x, _pos.y, _pos.z);
        TailParticle.setTarget(_pos, deg);
        restartCnt = RESTART_CNT_CONST;
    }

    public virtual void addConnectedParticles_3_Vector3_Single_Single(Vector3 p, float deg, float speed = 1)
    {
        ConnectedParticle[] cps = connectedParticles.getMultipleInstances(9);
        if (!((cps) != null))
            return;
        float d = deg - PI / 8;
        ConnectedParticle pcp = null;
        int c = 60 + rand.nextInt(120);
        foreach (ConnectedParticle cp in cps)
        {
            cp.set_12(p.x, p.y, p.z, d, (0.2f + rand.nextSignedFloat(0.08f)) * speed, 0.25f + rand.nextFloat(0.25f), 0.75f + rand.nextFloat(0.25f), 0.25f + rand.nextFloat(0.25f), c, rand.nextFloat(2), pcp, true);
            d += PI / 4 / 8 + rand.nextSignedFloat(PI / 4 / 8 / 4);
            pcp = cp;
        }
    }

    public virtual int getMultiplier()
    {
        return tailNum + 1;
    }

    public virtual bool inRestartBulletDisap()
    {
        return (restartBulletDisapCnt > 0);
    }

    public virtual void recordLinePoints_0()
    {
        glPushMatrix();
        Screen.glTranslate_1_Vector3(_pos);
        glMultMatrix(rot);
        linePoint.beginRecord();
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        glPopMatrix();
    }

    public override void draw()
    {
        shots.draw();
        enhancedShots.draw();
        if (restartCnt > 0)
            return;
        for (int i = 0; i < tailNum; i++)
            tails[i].draw();
        linePoint.drawSpectrum();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        shape.drawShadow_1(linePoint);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        linePoint.draw();
        glPushMatrix();
        Screen.glTranslate_1_Vector3(_pos);
        glMultMatrix(rot);
        subShape.draw();
        glPopMatrix();
    }

    public virtual void drawLeft(float x, float y)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(15, 15, 15);
        glRotatef(180, 0, 0, 1);
        linePoint.beginRecord();
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        shape.drawShadow_1(linePoint);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        linePoint.draw();
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(15, 15, 15);
        glRotatef(180, 0, 0, 1);
        subShape.draw();
        glPopMatrix();
    }

    public virtual Vector3 pos()
    {
        return _pos;
    }

    public virtual Vector getTargetPos()
    {
        trgPos.x = _pos.x;
        trgPos.y = _pos.y;
        return trgPos;
    }

    public virtual bool replayMode_1(bool v)
    {
        _replayMode = v;
        return _replayMode;
    }

    public virtual bool replayMode_0()
    {
        return _replayMode;
    }
}

public class ShipTail : OdeActor
{
    public const float WIDTH = 0.25f;
    public const float COLOR_R = 0.1f;
    public const float COLOR_G = 0.4f;
    public const float COLOR_B = 0.2f;
    public const float TAIL_INTERVAL = 0.57f;
    public const float SIZE_CONST = 1.0f;
    public const float MASS = 0.1f;
    public static Rand rand;
    public Vector3 _pos;
    public float deg;
    public float[] rot = McdArrays.Make<float>(16, () => 0);
    public Vector3 size;
    public Field field;
    public Ship ship;
    public ParticlePool particles;
    public ConnectedParticlePool connectedParticles;
    public OdeMass m;
    public Shape shape;
    public LinePoint linePoint;
    public OdeHandle joint;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public ShipTail(World world, Field field, Ship ship, ParticlePool particles, ConnectedParticlePool connectedParticles)
    {
        setWorld(world);
        this.field = field;
        this.ship = ship;
        this.particles = particles;
        this.connectedParticles = connectedParticles;
        _pos = new Vector3();
        size = new Vector3();
        deg = 0;
        shape = new Square(world, MASS, 0, 0, SIZE_CONST * WIDTH, SIZE_CONST);
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(COLOR_R, COLOR_G, COLOR_B, 0.6f);
        linePoint.alpha(0.5f);
        base.init_1_Boolean();
    }

    public override void init_1_(object[] args)
    {
    }

    public virtual void initMassAndGeom()
    {
        base.set_1();
        m = McdPhysics.Mass();
        shape.addMass(m, size);
        setMass_1(m);
        shape.addGeom_3(this, world.space, size);
    }

    public virtual void set_6(float x, float y, float z, float deg, float sz, OdeHandle jointedBodyId)
    {
        size.y = sz;
        size.x = size.y;
        size.z = 1;
        initMassAndGeom();
        _pos.x = x;
        _pos.y = y;
        _pos.z = z;
        McdPhysics.BodyPosition(_bodyId, _pos.x, _pos.y, _pos.z);
        this.deg = deg;
        setDeg(deg);
        joint = McdPhysics.Hinge(World.world);
        McdPhysics.HingeLimit(joint, 0, -1);
        McdPhysics.HingeLimit(joint, 1, 1);
        McdPhysics.JointAttach(joint, _bodyId, jointedBodyId);
        McdPhysics.HingeAnchor(joint, x - sin(deg) * SIZE_CONST / 2, y + cos(deg) * SIZE_CONST / 2, 0);
        McdPhysics.HingeAxis(joint, 0, 0, 1);
        linePoint.init_0();
    }

    public override void move_0()
    {
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        _pos.x = p[0];
        _pos.y = p[1];
        _pos.z = p[2];
        deg = getDeg();
        getRot(rot);
        addForce(0, 0, -Field.GRAVITY * 0.1f);
        if (field.checkInField_1_Vector3(_pos))
        {
            setDeg(deg);
            linePoint.enableSpectrumColor(true);
        }
        else
        {
            linePoint.enableSpectrumColor(false);
        }

        recordLinePoints_0();
    }

    public override void remove_0()
    {
        McdPhysics.JointDestroy(joint);
        base.remove_0();
        ship.addConnectedParticles_3_Vector3_Single_Single(_pos, deg + PI / 2);
        ship.addConnectedParticles_3_Vector3_Single_Single(_pos, deg - PI / 2);
    }

    public virtual void removeWithoutParticles()
    {
        McdPhysics.JointDestroy(joint);
        base.remove_0();
    }

    public override void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        if ((((actor is Wall ? (Wall)actor : null))) != null || (((actor is ShipTail ? (ShipTail)actor : null))) != null || (((actor is Ship ? (Ship)actor : null))) != null)
            flags.hit = true;
    }

    public virtual void setTailParticleTarget()
    {
        TailParticle.setTarget(_pos, deg);
    }

    public virtual bool addTail_2(ShipTail tail, float sz)
    {
        if (fabs(_pos.z) >= 1)
            return false;
        float id = (size.x + sz) * TAIL_INTERVAL;
        tail.set_6(_pos.x + sin(deg) * id, _pos.y - cos(deg) * id, _pos.z, deg, sz, _bodyId);
        return true;
    }

    public virtual void recordLinePoints_0()
    {
        glPushMatrix();
        Screen.glTranslate_1_Vector3(_pos);
        glMultMatrix(rot);
        glScalef(size.x, size.y, size.z);
        linePoint.beginRecord();
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        glPopMatrix();
    }

    public override void draw()
    {
        linePoint.drawSpectrum();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        shape.drawShadow_1(linePoint);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        linePoint.draw();
    }

    public virtual Vector3 pos()
    {
        return _pos;
    }
}
