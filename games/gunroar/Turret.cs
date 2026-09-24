// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Turret
{
    public string meshKey;
    public static GunroarRand rand;
    public static Vector damagedPos;
    public Field field;
    public BulletPool bullets;
    public Ship ship;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public TurretSpec spec;
    public Vector pos;
    public float deg, baseDeg;
    public int cnt;
    public int appCnt;
    public int startCnt;
    public int shield;
    public bool damaged;
    public int destroyedCnt;
    public int damagedCnt;
    public float bulletSpeed;
    public int burstCnt;
    public Enemy parent;
    public static void init()
    {
        rand = new GunroarRand();
        damagedPos = new Vector();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Turret(Field field, BulletPool bullets, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, Enemy parent, string meshKey)
    {
        this.meshKey = meshKey;
        this.field = field;
        this.bullets = bullets;
        this.ship = ship;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.parent = parent;
        pos = new Vector();
        {
            baseDeg = 0;
            deg = baseDeg;
        }

        bulletSpeed = 1;
    }

    public void start(TurretSpec spec)
    {
        this.spec = spec;
        shield = spec.shield;
        {
            startCnt = 0;
            cnt = startCnt;
            appCnt = cnt;
        }

        {
            baseDeg = 0;
            deg = baseDeg;
        }

        damaged = false;
        damagedCnt = 0;
        destroyedCnt = -1;
        bulletSpeed = 1;
        burstCnt = 0;
    }

    public bool move(float x, float y, float d, float bulletFireSpeed = 0, float bulletFireDeg = -99999)
    {
        pos.x = x;
        pos.y = y;
        baseDeg = d;
        if (destroyedCnt >= 0)
        {
            destroyedCnt++;
            int itv = 5 + destroyedCnt / 12;
            if ((itv < 60) && (destroyedCnt % itv == 0))
            {
                Smoke s = smokes.getInstance();
                if (s != null)
                    s.set_7(pos, 0, 0, 0.01f + rand.nextFloat(0.01f), SmokeSmokeType.FIRE, 90 + rand.nextInt(30), spec.size);
            }

            return false;
        }

        float td = baseDeg + deg;
        Vector shipPos = ship.nearPos(pos);
        Vector shipVel = ship.nearVel(pos);
        float ax = shipPos.x - pos.x;
        float ay = shipPos.y - pos.y;
        if (spec.lookAheadRatio != 0)
        {
            float rd = pos.dist_1(shipPos) / spec.speed * 1.2f;
            ax = ax + (shipVel.x * spec.lookAheadRatio * rd);
            ay = ay + (shipVel.y * spec.lookAheadRatio * rd);
        }

        float ad = 0;
        if (fabs(ax) + fabs(ay) < 0.1f)
            ad = 0;
        else
            ad = atan2(ax, ay);
        float od = td - ad;
        od = normalizeDeg(od);
        float ts = 0;
        if (cnt >= 0)
            ts = spec.turnSpeed;
        else
            ts = spec.turnSpeed * spec.burstTurnRatio;
        if (fabs(od) <= ts)
            deg = ad - baseDeg;
        else if (od > 0)
            deg = deg - (ts);
        else
            deg = deg + (ts);
        deg = normalizeDeg(deg);
        if (deg > spec.turnRange)
            deg = spec.turnRange;
        else if (deg < -spec.turnRange)
            deg = -spec.turnRange;
        cnt++;
        if (field.checkInField_1(pos) || (parent.isBoss() && (cnt % 4 == 0)))
            appCnt++;
        if (cnt >= spec.interval)
        {
            if (spec.blind || (((fabs(od) <= spec.turnSpeed) && (pos.dist_1(shipPos) < spec.maxRange * 1.1f)) && (pos.dist_1(shipPos) > spec.minRange)))
            {
                cnt = -(spec.burstNum - 1) * spec.burstInterval;
                bulletSpeed = spec.speed;
                burstCnt = 0;
            }
        }

        if ((((cnt <= 0) && (-cnt % spec.burstInterval == 0)) && (((spec.invisible && field.checkInField_1(pos)) || ((spec.invisible && parent.isBoss()) && field.checkInOuterField_1(pos))) || ((!spec.invisible) && field.checkInFieldExceptTop(pos)))) && (pos.dist_1(shipPos) > spec.minRange))
        {
            float bd = baseDeg + deg;
            Smoke s = smokes.getInstance();
            if (s != null)
                s.set_7(pos, sin(bd) * bulletSpeed, cos(bd) * bulletSpeed, 0, SmokeSmokeType.SPARK, 20, spec.size * 2);
            int nw = spec.nway;
            if (spec.nwayChange && (burstCnt % 2 == 1))
                nw--;
            bd = bd - (spec.nwayAngle * (nw - 1) / 2);
            for (int i = 0; i < nw; i++)
            {
                Bullet b = bullets.getInstance();
                if (!(b != null))
                    break;
                b.set(parent.index(), pos, bd, bulletSpeed, spec.size * 3, spec.bulletShape, spec.maxRange, bulletFireSpeed, bulletFireDeg, spec.bulletDestructive);
                bd = bd + (spec.nwayAngle);
            }

            bulletSpeed = bulletSpeed + (spec.speedAccel);
            burstCnt++;
        }

        damaged = false;
        if (damagedCnt > 0)
            damagedCnt--;
        startCnt++;
        return true;
    }

    public void draw(float[] model)
    {
        float[] color = null;
        if (spec.invisible)
            return;
        float[] parent1 = model;
        if ((destroyedCnt < 0) && (damagedCnt > 0))
        {
            damagedPos.x = pos.x + rand.nextSignedFloat(damagedCnt * 0.015f);
            damagedPos.y = pos.y + rand.nextSignedFloat(damagedCnt * 0.015f);
            model = Transform.Translate(model, damagedPos.x, damagedPos.y, 0);
        }
        else
        {
            model = Transform.Translate(model, pos.x, pos.y, 0);
        }

        model = Transform.Rotate(model, -(baseDeg + deg) * 180 / PI, 0, 0, 1);
        if (destroyedCnt >= 0)
            spec.destroyedShape.draw(model);
        else if (!damaged)
            spec.shape.draw(model);
        else
            spec.damagedShape.draw(model);
        model = parent1;
        if (destroyedCnt >= 0)
            return;
        if (appCnt > 120)
            return;
        float a = 1 - (float)appCnt / 120;
        if (startCnt < 12)
            a = (float)startCnt / 12;
        float td = baseDeg + deg;
        if (spec.nway <= 1)
        {
            var geometry1 = new Mesh(meshKey + "-1");
            int first1 = geometry1.vertexCount;
            color = new float[] { 0.9f, 0.1f, 0.1f, a };
            geometry1.Vertex(pos.x + sin(td) * spec.minRange, pos.y + cos(td) * spec.minRange, 0, color);
            color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.5f };
            geometry1.Vertex(pos.x + sin(td) * spec.maxRange, pos.y + cos(td) * spec.maxRange, 0, color);
            geometry1.LineStrip(first1, geometry1.vertexCount - first1);
            Gfx.Draw(geometry1.count, geometry1.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        }
        else
        {
            td = td - (spec.nwayAngle * (spec.nway - 1) / 2);
            var geometry2 = new Mesh(meshKey + "-2");
            int first2 = geometry2.vertexCount;
            color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.75f };
            geometry2.Vertex(pos.x + sin(td) * spec.minRange, pos.y + cos(td) * spec.minRange, 0, color);
            color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.25f };
            geometry2.Vertex(pos.x + sin(td) * spec.maxRange, pos.y + cos(td) * spec.maxRange, 0, color);
            geometry2.LineStrip(first2, geometry2.vertexCount - first2);
            Gfx.Draw(geometry2.count, geometry2.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
            var geometry3 = new Mesh(meshKey + "-3");
            int first3 = geometry3.vertexCount;
            for (int i = 0; i < spec.nway - 1; i++)
            {
                color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.3f };
                geometry3.Vertex(pos.x + sin(td) * spec.minRange, pos.y + cos(td) * spec.minRange, 0, color);
                color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.05f };
                geometry3.Vertex(pos.x + sin(td) * spec.maxRange, pos.y + cos(td) * spec.maxRange, 0, color);
                td = td + (spec.nwayAngle);
                geometry3.Vertex(pos.x + sin(td) * spec.maxRange, pos.y + cos(td) * spec.maxRange, 0, color);
                color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.3f };
                geometry3.Vertex(pos.x + sin(td) * spec.minRange, pos.y + cos(td) * spec.minRange, 0, color);
            }

            geometry3.Quads(first3, geometry3.vertexCount - first3);
            Gfx.Draw(geometry3.count, geometry3.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
            var geometry4 = new Mesh(meshKey + "-4");
            int first4 = geometry4.vertexCount;
            color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.75f };
            geometry4.Vertex(pos.x + sin(td) * spec.minRange, pos.y + cos(td) * spec.minRange, 0, color);
            color = new float[] { 0.9f, 0.1f, 0.1f, a * 0.25f };
            geometry4.Vertex(pos.x + sin(td) * spec.maxRange, pos.y + cos(td) * spec.maxRange, 0, color);
            geometry4.LineStrip(first4, geometry4.vertexCount - first4);
            Gfx.Draw(geometry4.count, geometry4.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        }
    }

    public bool checkCollision(float x, float y, Collidable c, Shot shot)
    {
        if ((destroyedCnt >= 0) || spec.invisible)
            return false;
        float ox = fabs(pos.x - x), oy = fabs(pos.y - y);
        if (spec.shape.checkCollision(ox, oy, c))
        {
            addDamage(shot.damage);
            return true;
        }

        return false;
    }

    public void addDamage(int n)
    {
        shield = shield - (n);
        if (shield <= 0)
            destroyed();
        damaged = true;
        damagedCnt = 10;
    }

    public void destroyed()
    {
        SoundManager.playSe("turret_destroyed.wav");
        destroyedCnt = 0;
        for (int i = 0; i < 6; i++)
        {
            Smoke s = smokes.getInstanceForced();
            s.set_7(pos, rand.nextSignedFloat(0.1f), rand.nextSignedFloat(0.1f), rand.nextFloat(0.04f), SmokeSmokeType.EXPLOSION, 30 + rand.nextInt(20), spec.size * 1.5f);
        }

        for (int i = 0; i < 32; i++)
        {
            Spark sp = sparks.getInstanceForced();
            sp.set(pos, rand.nextSignedFloat(0.5f), rand.nextSignedFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0, 30 + rand.nextInt(30));
        }

        for (int i = 0; i < 7; i++)
        {
            Fragment f = fragments.getInstanceForced();
            f.set(pos, rand.nextSignedFloat(0.25f), rand.nextSignedFloat(0.25f), 0.05f + rand.nextFloat(0.05f), spec.size * (0.5f + rand.nextFloat(0.5f)));
        }

        switch (spec.type)
        {
            case TurretSpecTurretType.MAIN:
            {
                parent.increaseMultiplier(2);
                parent.addScore(40);
                break;
            }

            case TurretSpecTurretType.SUB:
            case TurretSpecTurretType.SUB_DESTRUCTIVE:
            {
                parent.increaseMultiplier(1);
                parent.addScore(20);
                break;
            }
        }
    }

    public void remove()
    {
        if (destroyedCnt < 0)
            destroyedCnt = 999;
    }
}

public class TurretSpec
{
    public int type;
    public int interval;
    public float speed;
    public float speedAccel;
    public float minRange, maxRange;
    public float turnSpeed, turnRange;
    public int burstNum, burstInterval;
    public float burstTurnRatio;
    public bool blind;
    public float lookAheadRatio;
    public int nway;
    public float nwayAngle;
    public bool nwayChange;
    public int bulletShape;
    public bool bulletDestructive;
    public int shield;
    public bool invisible;
    public TurretShape shape, damagedShape, destroyedShape;
    public float _size;
    public TurretSpec()
    {
        shape = new TurretShape(TurretShapeTurretShapeType.NORMAL);
        damagedShape = new TurretShape(TurretShapeTurretShapeType.DAMAGED);
        destroyedShape = new TurretShape(TurretShapeTurretShapeType.DESTROYED);
        init();
    }

    public void init()
    {
        type = 0;
        interval = 99999;
        speed = 1;
        speedAccel = 0;
        minRange = 0;
        maxRange = 99999;
        turnSpeed = 99999;
        turnRange = 99999;
        burstNum = 1;
        burstInterval = 99999;
        burstTurnRatio = 0;
        blind = false;
        lookAheadRatio = 0;
        nway = 1;
        nwayAngle = 0;
        nwayChange = false;
        bulletShape = BulletShapeBulletShapeType.NORMAL;
        bulletDestructive = false;
        shield = 99999;
        invisible = false;
        _size = 1;
    }

    public void setParam_1(TurretSpec ts)
    {
        type = ts.type;
        interval = ts.interval;
        speed = ts.speed;
        speedAccel = ts.speedAccel;
        minRange = ts.minRange;
        maxRange = ts.maxRange;
        turnSpeed = ts.turnSpeed;
        turnRange = ts.turnRange;
        burstNum = ts.burstNum;
        burstInterval = ts.burstInterval;
        burstTurnRatio = ts.burstTurnRatio;
        blind = ts.blind;
        lookAheadRatio = ts.lookAheadRatio;
        nway = ts.nway;
        nwayAngle = ts.nwayAngle;
        nwayChange = ts.nwayChange;
        bulletShape = ts.bulletShape;
        bulletDestructive = ts.bulletDestructive;
        shield = ts.shield;
        invisible = ts.invisible;
        size = ts.size;
    }

    public void setParam_3(float rank, int type, GunroarRand rand)
    {
        init();
        this.type = type;
        if (type == TurretSpecTurretType.DUMMY)
        {
            invisible = true;
            return;
        }

        float rk = rank;
        switch (type)
        {
            case TurretSpecTurretType.SMALL:
            {
                minRange = 8;
                bulletShape = BulletShapeBulletShapeType.SMALL;
                blind = true;
                invisible = true;
                break;
            }

            case TurretSpecTurretType.MOVING:
            {
                minRange = 6;
                bulletShape = BulletShapeBulletShapeType.MOVING_TURRET;
                blind = true;
                invisible = true;
                turnSpeed = 0;
                maxRange = 9 + rand.nextFloat(12);
                rk = rk * ((10.0f / sqrt(maxRange)));
                break;
            }

            default:
            {
                maxRange = 9 + rand.nextFloat(16);
                minRange = maxRange / (4 + rand.nextFloat(0.5f));
                if ((type == TurretSpecTurretType.SUB) || (type == TurretSpecTurretType.SUB_DESTRUCTIVE))
                {
                    maxRange = maxRange * (0.72f);
                    minRange = minRange * (0.9f);
                }

                rk = rk * ((10.0f / sqrt(maxRange)));
                if (rand.nextInt(4) == 0)
                {
                    float lar = rank * 0.1f;
                    if (lar > 1)
                        lar = 1;
                    lookAheadRatio = rand.nextFloat(lar / 2) + lar / 2;
                    rk = rk / ((1 + lookAheadRatio * 0.3f));
                }

                if ((rand.nextInt(3) == 0) && (lookAheadRatio == 0))
                {
                    blind = false;
                    rk = rk * (1.5f);
                }
                else
                {
                    blind = true;
                }

                turnRange = PI / 4 + rand.nextFloat(PI / 4);
                turnSpeed = 0.005f + rand.nextFloat(0.015f);
                if (type == TurretSpecTurretType.MAIN)
                    turnRange = turnRange * (1.2f);
                if (rand.nextInt(4) == 0)
                    burstTurnRatio = rand.nextFloat(0.66f) + 0.33f;
                break;
            }
        }

        burstInterval = 6 + rand.nextInt(8);
        switch (type)
        {
            case TurretSpecTurretType.MAIN:
            {
                size = 0.42f + rand.nextFloat(0.05f);
                float br = (rk * 0.3f) * (1 + rand.nextSignedFloat(0.2f));
                float nr = (rk * 0.33f) * rand.nextFloat(1);
                float ir = (rk * 0.1f) * (1 + rand.nextSignedFloat(0.2f));
                burstNum = GameMath.integer(br) + 1;
                nway = GameMath.integer((nr * 0.66f + 1));
                interval = GameMath.integer((120.0f / (ir * 2 + 1))) + 1;
                float sr = rk - burstNum + 1 - (nway - 1) / 0.66f - ir;
                if (sr < 0)
                    sr = 0;
                speed = sqrt(sr * 0.6f);
                speed = speed * (0.12f);
                shield = 20;
                break;
            }

            case TurretSpecTurretType.SUB:
            {
                size = 0.36f + rand.nextFloat(0.025f);
                float br = (rk * 0.4f) * (1 + rand.nextSignedFloat(0.2f));
                float nr = (rk * 0.2f) * rand.nextFloat(1);
                float ir = (rk * 0.2f) * (1 + rand.nextSignedFloat(0.2f));
                burstNum = GameMath.integer(br) + 1;
                nway = GameMath.integer((nr * 0.66f + 1));
                interval = GameMath.integer((120.0f / (ir * 2 + 1))) + 1;
                float sr = rk - burstNum + 1 - (nway - 1) / 0.66f - ir;
                if (sr < 0)
                    sr = 0;
                speed = sqrt(sr * 0.7f);
                speed = speed * (0.2f);
                shield = 12;
                break;
            }

            case TurretSpecTurretType.SUB_DESTRUCTIVE:
            {
                size = 0.36f + rand.nextFloat(0.025f);
                float br = (rk * 0.4f) * (1 + rand.nextSignedFloat(0.2f));
                float nr = (rk * 0.2f) * rand.nextFloat(1);
                float ir = (rk * 0.2f) * (1 + rand.nextSignedFloat(0.2f));
                burstNum = GameMath.integer(br) * 2 + 1;
                nway = GameMath.integer((nr * 0.66f + 1));
                interval = GameMath.integer((60.0f / (ir * 2 + 1))) + 1;
                burstInterval = GameMath.integer(burstInterval * 0.88f);
                bulletShape = BulletShapeBulletShapeType.DESTRUCTIVE;
                bulletDestructive = true;
                float sr = rk - (burstNum - 1) / 2 - (nway - 1) / 0.66f - ir;
                if (sr < 0)
                    sr = 0;
                speed = sqrt(sr * 0.7f);
                speed = speed * (0.33f);
                shield = 12;
                break;
            }

            case TurretSpecTurretType.SMALL:
            {
                size = 0.33f;
                float br = (rk * 0.33f) * (1 + rand.nextSignedFloat(0.2f));
                float ir = (rk * 0.2f) * (1 + rand.nextSignedFloat(0.2f));
                burstNum = GameMath.integer(br) + 1;
                nway = 1;
                interval = GameMath.integer((120.0f / (ir * 2 + 1))) + 1;
                float sr = rk - burstNum + 1 - ir;
                if (sr < 0)
                    sr = 0;
                speed = sqrt(sr);
                speed = speed * (0.24f);
                break;
            }

            case TurretSpecTurretType.MOVING:
            {
                size = 0.36f;
                float br = (rk * 0.3f) * (1 + rand.nextSignedFloat(0.2f));
                float nr = (rk * 0.1f) * rand.nextFloat(1);
                float ir = (rk * 0.33f) * (1 + rand.nextSignedFloat(0.2f));
                burstNum = GameMath.integer(br) + 1;
                nway = GameMath.integer((nr * 0.66f + 1));
                interval = GameMath.integer((120.0f / (ir * 2 + 1))) + 1;
                float sr = rk - burstNum + 1 - (nway - 1) / 0.66f - ir;
                if (sr < 0)
                    sr = 0;
                speed = sqrt(sr * 0.7f);
                speed = speed * (0.2f);
                break;
            }
        }

        if (speed < 0.1f)
            speed = 0.1f;
        else
            speed = sqrt(speed * 10) / 10;
        if (burstNum > 2)
        {
            if (rand.nextInt(4) == 0)
            {
                speed = speed * (0.8f);
                burstInterval = GameMath.integer(burstInterval * 0.7f);
                speedAccel = (speed * (0.4f + rand.nextFloat(0.3f))) / burstNum;
                if (rand.nextInt(2) == 0)
                    speedAccel = speedAccel * (-1);
                speed = speed - (speedAccel * burstNum / 2);
            }

            if (rand.nextInt(5) == 0)
            {
                if (nway > 1)
                    nwayChange = true;
            }
        }

        nwayAngle = (0.1f + rand.nextFloat(0.33f)) / (1 + nway * 0.1f);
    }

    public void setBossSpec()
    {
        minRange = 0;
        maxRange = maxRange * (1.5f);
        shield = GameMath.integer(shield * 2.1f);
    }

    public float size
    {
        get
        {
            return _size;
        }

        set
        {
            _size = value;
            {
                destroyedShape.size = _size;
                damagedShape.size = destroyedShape.size;
                shape.size = damagedShape.size;
            }
        }
    }
}

public class TurretGroup
{
    public const int MAX_NUM = 16;
    public Ship ship;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public TurretGroupSpec spec;
    public Vector centerPos;
    public Turret[] turret = new Turret[MAX_NUM];
    public int cnt;
    public TurretGroup(Field field, BulletPool bullets, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, Enemy parent, string meshKey)
    {
        this.ship = ship;
        centerPos = new Vector();
        for (int index0 = 0; index0 < MAX_NUM; index0++)
        {
            turret[index0] = new Turret(field, bullets, ship, sparks, smokes, fragments, parent, meshKey + "-" + index0.ToString());
        }
    }

    public void set(TurretGroupSpec spec)
    {
        this.spec = spec;
        for (int i = 0; i < spec.num; i++)
            turret[i].start(spec.turretSpec);
        cnt = 0;
    }

    public bool move(Vector p, float deg)
    {
        bool alive = false;
        centerPos.x = p.x;
        centerPos.y = p.y;
        float d = 0, md = 0, y = 0, my = 0;
        switch (spec.alignType)
        {
            case TurretGroupSpecAlignType.ROUND:
            {
                d = spec.alignDeg;
                if (spec.num > 1)
                {
                    md = spec.alignWidth / (spec.num - 1);
                    d = d - (spec.alignWidth / 2);
                }
                else
                {
                    md = 0;
                }

                break;
            }

            case TurretGroupSpecAlignType.STRAIGHT:
            {
                y = 0;
                my = spec.offset.y / (spec.num + 1);
                break;
            }
        }

        for (int i = 0; i < spec.num; i++)
        {
            float tbx = 0, tby = 0;
            switch (spec.alignType)
            {
                case TurretGroupSpecAlignType.ROUND:
                {
                    tbx = sin(d) * spec.radius;
                    tby = cos(d) * spec.radius;
                    break;
                }

                case TurretGroupSpecAlignType.STRAIGHT:
                {
                    y = y + (my);
                    tbx = spec.offset.x;
                    tby = y;
                    d = atan2(tbx, tby);
                    break;
                }
            }

            tbx = tbx * ((1 - spec.distRatio));
            float bx = tbx * cos(-deg) - tby * sin(-deg);
            float by = tbx * sin(-deg) + tby * cos(-deg);
            if (turret[i].move(centerPos.x + bx, centerPos.y + by, d + deg))
                alive = true;
            if (spec.alignType == TurretGroupSpecAlignType.ROUND)
                d = d + (md);
        }

        cnt++;
        return alive;
    }

    public void draw(float[] model)
    {
        for (int i = 0; i < spec.num; i++)
            turret[i].draw(model);
    }

    public void remove()
    {
        for (int i = 0; i < spec.num; i++)
            turret[i].remove();
    }

    public bool checkCollision(float x, float y, Collidable c, Shot shot)
    {
        bool col = false;
        for (int i = 0; i < spec.num; i++)
            if (turret[i].checkCollision(x, y, c, shot))
                col = true;
        return col;
    }
}

public class TurretGroupSpec
{
    public TurretSpec turretSpec;
    public int num;
    public int alignType;
    public float alignDeg;
    public float alignWidth;
    public float radius;
    public float distRatio;
    public Vector offset;
    public TurretGroupSpec()
    {
        turretSpec = new TurretSpec();
        offset = new Vector();
        num = 1;
        {
            alignWidth = 0;
            alignDeg = alignWidth;
        }

        radius = 0;
        distRatio = 0;
    }

    public void init()
    {
        num = 1;
        alignType = TurretGroupSpecAlignType.ROUND;
        {
            distRatio = 0;
            radius = distRatio;
            alignWidth = radius;
            alignDeg = alignWidth;
        }

        {
            offset.y = 0;
            offset.x = offset.y;
        }
    }
}

public class MovingTurretGroup
{
    public const int MAX_NUM = 16;
    public Ship ship;
    public MovingTurretGroupSpec spec;
    public float radius;
    public float radiusAmpCnt;
    public float deg;
    public float rollAmpCnt;
    public float swingAmpCnt;
    public float swingAmpDeg;
    public float swingFixDeg;
    public float alignAmpCnt;
    public float distDeg;
    public float distAmpCnt;
    public int cnt;
    public Vector centerPos;
    public Turret[] turret = new Turret[MAX_NUM];
    public MovingTurretGroup(Field field, BulletPool bullets, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, Enemy parent, string meshKey)
    {
        this.ship = ship;
        centerPos = new Vector();
        for (int index1 = 0; index1 < MAX_NUM; index1++)
        {
            turret[index1] = new Turret(field, bullets, ship, sparks, smokes, fragments, parent, meshKey + "-" + index1.ToString());
        }

        {
            radiusAmpCnt = 0;
            radius = radiusAmpCnt;
        }

        deg = 0;
        {
            alignAmpCnt = 0;
            swingFixDeg = alignAmpCnt;
            swingAmpDeg = swingFixDeg;
            swingAmpCnt = swingAmpDeg;
            rollAmpCnt = swingAmpCnt;
        }

        {
            distAmpCnt = 0;
            distDeg = distAmpCnt;
        }
    }

    public void set(MovingTurretGroupSpec spec)
    {
        this.spec = spec;
        radius = spec.radiusBase;
        radiusAmpCnt = 0;
        deg = 0;
        {
            alignAmpCnt = 0;
            swingAmpDeg = alignAmpCnt;
            swingAmpCnt = swingAmpDeg;
            rollAmpCnt = swingAmpCnt;
        }

        {
            distAmpCnt = 0;
            distDeg = distAmpCnt;
        }

        swingFixDeg = PI;
        for (int i = 0; i < spec.num; i++)
            turret[i].start(spec.turretSpec);
        cnt = 0;
    }

    public void move(Vector p, float ed)
    {
        if (spec.moveType == MovingTurretGroupSpecMoveType.SWING_FIX)
            swingFixDeg = ed;
        centerPos.x = p.x;
        centerPos.y = p.y;
        if (spec.radiusAmp > 0)
        {
            radiusAmpCnt = radiusAmpCnt + (spec.radiusAmpVel);
            float av = sin(radiusAmpCnt);
            radius = spec.radiusBase + spec.radiusAmp * av;
        }

        if (spec.moveType == MovingTurretGroupSpecMoveType.ROLL)
        {
            if (spec.rollAmp != 0)
            {
                rollAmpCnt = rollAmpCnt + (spec.rollAmpVel);
                float av = sin(rollAmpCnt);
                deg = deg + (spec.rollDegVel + spec.rollAmp * av);
            }
            else
            {
                deg = deg + (spec.rollDegVel);
            }
        }
        else
        {
            swingAmpCnt = swingAmpCnt + (spec.swingAmpVel);
            if (cos(swingAmpCnt) > 0)
            {
                swingAmpDeg = swingAmpDeg + (spec.swingDegVel);
            }
            else
            {
                swingAmpDeg = swingAmpDeg - (spec.swingDegVel);
            }

            if (spec.moveType == MovingTurretGroupSpecMoveType.SWING_AIM)
            {
                float od = 0;
                Vector shipPos = ship.nearPos(centerPos);
                if (shipPos.dist_1(centerPos) < 0.1f)
                    od = 0;
                else
                    od = atan2(shipPos.x - centerPos.x, shipPos.y - centerPos.y);
                od = od + (swingAmpDeg - deg);
                od = normalizeDeg(od);
                deg = deg + (od * 0.1f);
            }
            else
            {
                float od = swingFixDeg + swingAmpDeg - deg;
                od = normalizeDeg(od);
                deg = deg + (od * 0.1f);
            }
        }

        float d = 0, ad = 0, md = 0;
        alignAmpCnt = alignAmpCnt + (spec.alignAmpVel);
        ad = spec.alignDeg * (1 + sin(alignAmpCnt) * spec.alignAmp);
        if (spec.num > 1)
        {
            if (spec.moveType == MovingTurretGroupSpecMoveType.ROLL)
                md = ad / spec.num;
            else
                md = ad / (spec.num - 1);
        }
        else
        {
            md = 0;
        }

        d = deg - md - ad / 2;
        for (int i = 0; i < spec.num; i++)
        {
            d = d + (md);
            float bx = sin(d) * radius * spec.xReverse;
            float by = cos(d) * radius * (1 - spec.distRatio);
            float fs = 0, fd = 0;
            if (fabs(bx) + fabs(by) < 0.1f)
            {
                fs = radius;
                fd = d;
            }
            else
            {
                fs = sqrt(bx * bx + by * by);
                fd = atan2(bx, by);
            }

            fs = fs * (0.06f);
            turret[i].move(centerPos.x, centerPos.y, d, fs, fd);
        }

        cnt++;
    }

    public void draw(float[] model)
    {
        for (int i = 0; i < spec.num; i++)
            turret[i].draw(model);
    }

    public void remove()
    {
        for (int i = 0; i < spec.num; i++)
            turret[i].remove();
    }
}

public class MovingTurretGroupSpec
{
    public TurretSpec turretSpec;
    public int num;
    public float alignDeg;
    public float alignAmp;
    public float alignAmpVel;
    public float radiusBase;
    public float radiusAmp;
    public float radiusAmpVel;
    public int moveType;
    public float rollDegVel;
    public float rollAmp;
    public float rollAmpVel;
    public float swingDegVel;
    public float swingAmpVel;
    public float distRatio;
    public float xReverse;
    public MovingTurretGroupSpec()
    {
        turretSpec = new TurretSpec();
        num = 1;
        initParam();
    }

    public void initParam()
    {
        num = 1;
        alignDeg = PI * 2;
        {
            alignAmpVel = 0;
            alignAmp = alignAmpVel;
        }

        radiusBase = 1;
        {
            radiusAmpVel = 0;
            radiusAmp = radiusAmpVel;
        }

        moveType = MovingTurretGroupSpecMoveType.SWING_FIX;
        {
            rollAmpVel = 0;
            rollAmp = rollAmpVel;
            rollDegVel = rollAmp;
        }

        {
            swingAmpVel = 0;
            swingDegVel = swingAmpVel;
        }

        distRatio = 0;
        xReverse = 1;
    }

    public void init()
    {
        initParam();
    }

    public void setAlignAmp(float a, float v)
    {
        alignAmp = a;
        alignAmpVel = v;
    }

    public void setRadiusAmp(float a, float v)
    {
        radiusAmp = a;
        radiusAmpVel = v;
    }

    public void setRoll(float dv, float a, float v)
    {
        moveType = MovingTurretGroupSpecMoveType.ROLL;
        rollDegVel = dv;
        rollAmp = a;
        rollAmpVel = v;
    }

    public void setSwing(float dv, float a, bool aim = false)
    {
        if (aim)
            moveType = MovingTurretGroupSpecMoveType.SWING_AIM;
        else
            moveType = MovingTurretGroupSpecMoveType.SWING_FIX;
        swingDegVel = dv;
        swingAmpVel = a;
    }

    public void setXReverse(float xr)
    {
        xReverse = xr;
    }
}

public static class MovingTurretGroupSpecMoveType
{
    public const int ROLL = 0;
    public const int SWING_FIX = 1;
    public const int SWING_AIM = 2;
}

public static class TurretGroupSpecAlignType
{
    public const int ROUND = 0;
    public const int STRAIGHT = 1;
}

public static class TurretSpecTurretType
{
    public const int MAIN = 0;
    public const int SUB = 1;
    public const int SUB_DESTRUCTIVE = 2;
    public const int SMALL = 3;
    public const int MOVING = 4;
    public const int DUMMY = 5;
}
