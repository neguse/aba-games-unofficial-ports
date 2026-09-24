// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class Enemy : Actor
{
    public EnemySpec spec;
    public EnemyState _state;
    public override void init(List<object> args)
    {
        _state = new EnemyState((Field)args[0], (GrScreen)args[1], (BulletPool)args[2], (Ship)args[3], (SparkPool)args[4], (SmokePool)args[5], (FragmentPool)args[6], (SparkFragmentPool)args[7], (NumIndicatorPool)args[8], (ScoreReel)args[9]);
    }

    public void setEnemyPool(EnemyPool enemies)
    {
        _state.setEnemyAndPool(this, enemies);
    }

    public void setStageManager(StageManager stageManager)
    {
        _state.setStageManager(stageManager);
    }

    public void set(EnemySpec spec)
    {
        this.spec = spec;
        exists = true;
    }

    public override void move()
    {
        if (!spec.move(state))
            remove();
    }

    public void checkShotHit(Vector p, Collidable shape, Shot shot)
    {
        if (_state.destroyedCnt >= 0)
            return;
        if (spec.checkCollision(_state, p.x, p.y, shape, shot))
        {
            if (shot != null)
                shot.removeHitToEnemy(spec.isSmallEnemy());
        }
    }

    public bool checkHitShip(float x, float y, bool largeOnly = false)
    {
        return spec.checkShipCollision(_state, x, y, largeOnly);
    }

    public void addDamage(int n)
    {
        _state.addDamage(n);
    }

    public void increaseMultiplier(float m)
    {
        _state.increaseMultiplier(m);
    }

    public void addScore(int s)
    {
        _state.addScore(s);
    }

    public void remove()
    {
        _state.removeTurrets();
        exists = false;
    }

    public override void draw(float[] model, Mesh particles = null)
    {
        spec.draw(model, _state);
    }

    public EnemyState state
    {
        get
        {
            return _state;
        }

        set
        {
            _state = value;
        }
    }

    public Vector pos()
    {
        return _state.pos;
    }

    public float size()
    {
        return spec.size;
    }

    public int index()
    {
        return _state.idx;
    }

    public bool isBoss()
    {
        return spec.isBoss();
    }
}

public class EnemyState
{
    public const int TURRET_GROUP_MAX = 10;
    public const int MOVING_TURRET_GROUP_MAX = 4;
    public const float MULTIPLIER_DECREASE_RATIO = 0.005f;
    public int appType;
    public Vector pos;
    public Vector ppos;
    public int shield;
    public float deg;
    public float velDeg;
    public float speed;
    public float turnWay;
    public float trgDeg;
    public int turnCnt;
    public int state;
    public int cnt;
    public Vector vel;
    public TurretGroup[] turretGroup = new TurretGroup[TURRET_GROUP_MAX];
    public MovingTurretGroup[] movingTurretGroup = new MovingTurretGroup[MOVING_TURRET_GROUP_MAX];
    public bool damaged;
    public int damagedCnt;
    public int destroyedCnt;
    public int explodeCnt, explodeItv;
    public int idx;
    public float multiplier;
    public EnemySpec spec;
    public static GunroarRand rand = new GunroarRand();
    public static Vector edgePos = new Vector(), explodeVel = new Vector(), damagedPos = new Vector();
    public static int idxCount = 0;
    public Field field;
    public GrScreen screen;
    public BulletPool bullets;
    public Ship ship;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public SparkFragmentPool sparkFragments;
    public NumIndicatorPool numIndicators;
    public Enemy enemy;
    public EnemyPool enemies;
    public StageManager stageManager;
    public ScoreReel scoreReel;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public EnemyState(Field field, GrScreen screen, BulletPool bullets, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, SparkFragmentPool sparkFragments, NumIndicatorPool numIndicators, ScoreReel scoreReel)
    {
        idx = idxCount;
        idxCount++;
        this.field = field;
        this.screen = screen;
        this.bullets = bullets;
        this.ship = ship;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.sparkFragments = sparkFragments;
        this.numIndicators = numIndicators;
        this.scoreReel = scoreReel;
        pos = new Vector();
        ppos = new Vector();
        vel = new Vector();
        {
            speed = 0;
            velDeg = speed;
            deg = velDeg;
        }

        turnWay = 1;
        explodeItv = 1;
        multiplier = 1;
        trgDeg = 0;
        turnCnt = 0;
    }

    public void setEnemyAndPool(Enemy enemy, EnemyPool enemies)
    {
        this.enemy = enemy;
        this.enemies = enemies;
        for (int index0 = 0; index0 < TURRET_GROUP_MAX; index0++)
        {
            turretGroup[index0] = new TurretGroup(field, bullets, ship, sparks, smokes, fragments, enemy, "turret-" + enemy.poolIndex.ToString() + "-" + index0.ToString());
        }

        for (int index1 = 0; index1 < MOVING_TURRET_GROUP_MAX; index1++)
        {
            movingTurretGroup[index1] = new MovingTurretGroup(field, bullets, ship, sparks, smokes, fragments, enemy, "moving-turret-" + enemy.poolIndex.ToString() + "-" + index1.ToString());
        }
    }

    public void setStageManager(StageManager stageManager)
    {
        this.stageManager = stageManager;
    }

    public void setSpec(EnemySpec spec)
    {
        this.spec = spec;
        shield = spec.shield;
        for (int i = 0; i < spec.turretGroupNum; i++)
            turretGroup[i].set(spec.turretGroupSpec[i]);
        for (int i = 0; i < spec.movingTurretGroupNum; i++)
            movingTurretGroup[i].set(spec.movingTurretGroupSpec[i]);
        cnt = 0;
        damaged = false;
        damagedCnt = 0;
        destroyedCnt = -1;
        explodeCnt = 0;
        explodeItv = 1;
        multiplier = 1;
    }

    public bool setAppearancePos(Field field, Ship ship, GunroarRand rand, int appType = EnemyStateAppearanceType.TOP)
    {
        this.appType = appType;
        for (int i = 0; i < 8; i++)
        {
            switch (appType)
            {
                case EnemyStateAppearanceType.TOP:
                {
                    pos.x = rand.nextSignedFloat(field.size.x);
                    pos.y = field.outerSize.y * 0.99f + spec.size;
                    if (pos.x < 0)
                    {
                        deg = PI - rand.nextFloat(0.5f);
                        velDeg = deg;
                    }
                    else
                    {
                        deg = PI + rand.nextFloat(0.5f);
                        velDeg = deg;
                    }

                    break;
                }

                case EnemyStateAppearanceType.SIDE:
                {
                    if (rand.nextInt(2) == 0)
                    {
                        pos.x = -field.outerSize.x * 0.99f;
                        {
                            deg = PI / 2 + rand.nextFloat(0.66f);
                            velDeg = deg;
                        }
                    }
                    else
                    {
                        pos.x = field.outerSize.x * 0.99f;
                        {
                            deg = -PI / 2 - rand.nextFloat(0.66f);
                            velDeg = deg;
                        }
                    }

                    pos.y = field.size.y + rand.nextFloat(field.size.y) + spec.size;
                    break;
                }

                case EnemyStateAppearanceType.CENTER:
                {
                    pos.x = 0;
                    pos.y = field.outerSize.y * 0.99f + spec.size;
                    {
                        deg = 0;
                        velDeg = deg;
                    }

                    break;
                }
            }

            ppos.x = pos.x;
            ppos.y = pos.y;
            {
                vel.y = 0;
                vel.x = vel.y;
            }

            speed = 0;
            if ((appType == EnemyStateAppearanceType.CENTER) || checkFrontClear(true))
                return true;
        }

        return false;
    }

    public bool checkFrontClear(bool checkCurrentPos = false)
    {
        int si = 1;
        if (checkCurrentPos)
            si = 0;
        for (int i = si; i < 5; i++)
        {
            float cx = pos.x + sin(deg) * i * spec.size;
            float cy = pos.y + cos(deg) * i * spec.size;
            if (field.getBlock_2(cx, cy) >= 0)
                return false;
            if (enemies.checkHitShip(cx, cy, enemy, true) != null)
                return false;
        }

        return true;
    }

    public bool move()
    {
        ppos.x = pos.x;
        ppos.y = pos.y;
        multiplier = multiplier - (MULTIPLIER_DECREASE_RATIO);
        if (multiplier < 1)
            multiplier = 1;
        if (destroyedCnt >= 0)
        {
            destroyedCnt++;
            explodeCnt--;
            if (explodeCnt < 0)
            {
                explodeItv = explodeItv + (2);
                explodeItv = GameMath.integer(((float)explodeItv * (1.2f + rand.nextFloat(1))));
                explodeCnt = explodeItv;
                destroyedEdge(GameMath.integer((sqrt(spec.size) * 27.0f / (explodeItv * 0.1f + 1))));
            }
        }

        damaged = false;
        if (damagedCnt > 0)
            damagedCnt--;
        bool alive = false;
        for (int i = 0; i < spec.turretGroupNum; i++)
            if (turretGroup[i].move(pos, deg))
                alive = true;
        for (int i = 0; i < spec.movingTurretGroupNum; i++)
            movingTurretGroup[i].move(pos, deg);
        if ((destroyedCnt < 0) && (!alive))
            return destroyed();
        return true;
    }

    public bool checkCollision(float x, float y, Collidable c, Shot shot)
    {
        float ox = fabs(pos.x - x), oy = fabs(pos.y - y);
        if (ox + oy > spec.size * 2)
            return false;
        for (int i = 0; i < spec.turretGroupNum; i++)
            if (turretGroup[i].checkCollision(x, y, c, shot))
                return true;
        if (spec.bridgeShape.checkCollision(ox, oy, c))
        {
            addDamage(shot.damage, shot);
            return true;
        }

        return false;
    }

    public void increaseMultiplier(float m)
    {
        multiplier = multiplier + (m);
    }

    public void addScore(int s)
    {
        setScoreIndicator(s, 1);
    }

    public void addDamage(int n, Shot shot = null)
    {
        shield = shield - (n);
        if (shield <= 0)
        {
            destroyed(shot);
        }
        else
        {
            damaged = true;
            damagedCnt = 7;
        }
    }

    public bool destroyed(Shot shot = null)
    {
        float vz = 0;
        if (shot != null)
        {
            explodeVel.x = Shot.SPEED * sin(shot.deg) / 2;
            explodeVel.y = Shot.SPEED * cos(shot.deg) / 2;
            vz = 0;
        }
        else
        {
            {
                explodeVel.y = 0;
                explodeVel.x = explodeVel.y;
            }

            vz = 0.05f;
        }

        float ss = spec.size * 1.5f;
        if (ss > 2)
            ss = 2;
        float sn = 0;
        if (spec.size < 1)
            sn = spec.size;
        else
            sn = sqrt(spec.size);
        if (sn > 3)
            sn = 3;
        for (int i = 0; i < sn * 8; i++)
        {
            Smoke s = smokes.getInstanceForced();
            s.set_7(pos, rand.nextSignedFloat(0.1f) + explodeVel.x, rand.nextSignedFloat(0.1f) + explodeVel.y, rand.nextFloat(vz), SmokeSmokeType.EXPLOSION, 32 + rand.nextInt(30), ss);
        }

        for (int i = 0; i < sn * 36; i++)
        {
            Spark sp = sparks.getInstanceForced();
            sp.set(pos, rand.nextSignedFloat(0.8f) + explodeVel.x, rand.nextSignedFloat(0.8f) + explodeVel.y, 0.5f + rand.nextFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0, 30 + rand.nextInt(30));
        }

        for (int i = 0; i < sn * 12; i++)
        {
            Fragment f = fragments.getInstanceForced();
            f.set(pos, rand.nextSignedFloat(0.33f) + explodeVel.x, rand.nextSignedFloat(0.33f) + explodeVel.y, 0.05f + rand.nextFloat(0.1f), 0.2f + rand.nextFloat(0.33f));
        }

        removeTurrets();
        int sc = spec.score();
        bool r = false;
        if (spec.type == EnemySpecEnemyType.SMALL)
        {
            SoundManager.playSe("small_destroyed.wav");
            r = false;
        }
        else
        {
            SoundManager.playSe("destroyed.wav");
            int bn = bullets.removeIndexedBullets(idx);
            destroyedCnt = 0;
            explodeCnt = 1;
            explodeItv = 3;
            sc = sc + (bn * 10);
            r = true;
            if (spec.isBoss())
                screen.setScreenShake(45, 0.04f);
        }

        setScoreIndicator(sc, multiplier);
        return r;
    }

    public void setScoreIndicator(int sc, float mp)
    {
        float ty = NumIndicator.getTargetY();
        if (mp > 1)
        {
            NumIndicator ni = numIndicators.getInstanceForced();
            ni.set_4(sc, NumIndicatorIndicatorType.SCORE, 0.5f, pos);
            ni.addTarget(8, ty, NumIndicatorFlyingToType.RIGHT, 1, 0.5f, sc, 40);
            ni.addTarget(11, ty, NumIndicatorFlyingToType.RIGHT, 0.5f, 0.75f, GameMath.integer((sc * mp)), 30);
            ni.addTarget(13, ty, NumIndicatorFlyingToType.RIGHT, 0.25f, 1, GameMath.integer((sc * mp * stageManager.rankMultiplier())), 20);
            ni.addTarget(12, -8, NumIndicatorFlyingToType.BOTTOM, 0.5f, 0.1f, GameMath.integer((sc * mp * stageManager.rankMultiplier())), 40);
            ni.gotoNextTarget();
            ni = numIndicators.getInstanceForced();
            int mn = GameMath.integer((mp * 1000));
            ni.set_4(mn, NumIndicatorIndicatorType.MULTIPLIER, 0.7f, pos);
            ni.addTarget(10.5f, ty, NumIndicatorFlyingToType.RIGHT, 0.5f, 0.2f, mn, 70);
            ni.gotoNextTarget();
            ni = numIndicators.getInstanceForced();
            int rn = GameMath.integer((stageManager.rankMultiplier() * 1000));
            ni.set_5(rn, NumIndicatorIndicatorType.MULTIPLIER, 0.4f, 11, 8);
            ni.addTarget(13, ty, NumIndicatorFlyingToType.RIGHT, 0.5f, 0.2f, rn, 40);
            ni.gotoNextTarget();
            scoreReel.addActualScore(GameMath.integer((sc * mp * stageManager.rankMultiplier())));
        }
        else
        {
            NumIndicator ni = numIndicators.getInstanceForced();
            ni.set_4(sc, NumIndicatorIndicatorType.SCORE, 0.3f, pos);
            ni.addTarget(11, ty, NumIndicatorFlyingToType.RIGHT, 1.5f, 0.2f, sc, 40);
            ni.addTarget(13, ty, NumIndicatorFlyingToType.RIGHT, 0.25f, 0.25f, GameMath.integer((sc * stageManager.rankMultiplier())), 20);
            ni.addTarget(12, -8, NumIndicatorFlyingToType.BOTTOM, 0.5f, 0.1f, GameMath.integer((sc * stageManager.rankMultiplier())), 40);
            ni.gotoNextTarget();
            ni = numIndicators.getInstanceForced();
            int rn = GameMath.integer((stageManager.rankMultiplier() * 1000));
            ni.set_5(rn, NumIndicatorIndicatorType.MULTIPLIER, 0.4f, 11, 8);
            ni.addTarget(13, ty, NumIndicatorFlyingToType.RIGHT, 0.5f, 0.2f, rn, 40);
            ni.gotoNextTarget();
            scoreReel.addActualScore(GameMath.integer((sc * stageManager.rankMultiplier())));
        }
    }

    public void destroyedEdge(int n)
    {
        SoundManager.playSe("explode.wav");
        int sn = n;
        if (sn > 48)
            sn = 48;
        List<Vector> spp = ((BaseShape)spec.shape.shape).pointPos;
        List<float> spd = ((BaseShape)spec.shape.shape).pointDeg;
        int si = rand.nextInt(spp.Count);
        edgePos.x = spp[si].x * spec.size + pos.x;
        edgePos.y = spp[si].y * spec.size + pos.y;
        float ss = spec.size * 0.5f;
        if (ss > 1)
            ss = 1;
        for (int i = 0; i < sn; i++)
        {
            Smoke s = smokes.getInstanceForced();
            float sr = rand.nextFloat(0.5f);
            float sd = spd[si] + rand.nextSignedFloat(0.2f);
            s.set_7(edgePos, sin(sd) * sr, cos(sd) * sr, -0.004f, SmokeSmokeType.EXPLOSION, 75 + rand.nextInt(25), ss);
            for (int j = 0; j < 2; j++)
            {
                Spark sp = sparks.getInstanceForced();
                sp.set(edgePos, sin(sd) * sr * 2, cos(sd) * sr * 2, 0.5f + rand.nextFloat(0.5f), 0.5f + rand.nextFloat(0.5f), 0, 30 + rand.nextInt(30));
            }

            if (i % 2 == 0)
            {
                SparkFragment sf = sparkFragments.getInstanceForced();
                sf.set(edgePos, sin(sd) * sr * 0.5f, cos(sd) * sr * 0.5f, 0.06f + rand.nextFloat(0.07f), (0.2f + rand.nextFloat(0.1f)));
            }
        }
    }

    public void removeTurrets()
    {
        for (int i = 0; i < spec.turretGroupNum; i++)
            turretGroup[i].remove();
        for (int i = 0; i < spec.movingTurretGroupNum; i++)
            movingTurretGroup[i].remove();
    }

    public void draw(float[] model, float[] color = null)
    {
        float[] parent1 = model;
        if ((destroyedCnt < 0) && (damagedCnt > 0))
        {
            damagedPos.x = pos.x + rand.nextSignedFloat(damagedCnt * 0.01f);
            damagedPos.y = pos.y + rand.nextSignedFloat(damagedCnt * 0.01f);
            model = Transform.Translate(model, damagedPos.x, damagedPos.y, 0);
        }
        else
        {
            model = Transform.Translate(model, pos.x, pos.y, 0);
        }

        model = Transform.Rotate(model, -deg * 180 / PI, 0, 0, 1);
        if (destroyedCnt >= 0)
            spec.destroyedShape.draw(model, color);
        else if (!damaged)
            spec.shape.draw(model);
        else
            spec.damagedShape.draw(model);
        if (destroyedCnt < 0)
            spec.bridgeShape.draw(Transform.Scale(model, spec.shape.size, spec.shape.size, spec.shape.size));
        model = parent1;
        if (destroyedCnt >= 0)
            return;
        for (int i = 0; i < spec.turretGroupNum; i++)
            turretGroup[i].draw(model);
        if (multiplier > 1)
        {
            float ox = 0, oy = 0;
            if (multiplier < 10)
                ox = 2.1f;
            else
                ox = 1.4f;
            oy = 1.25f;
            if (spec.isBoss())
            {
                ox = ox + (4);
                oy = oy - (1.25f);
            }

            Letter.drawNumSign(model, GameMath.integer((multiplier * 1000)), pos.x + ox, pos.y + oy, 0.33f, 1, 33, 3);
        }
    }
}

public abstract class EnemySpec
{
    public static GunroarRand rand = new GunroarRand();
    public Field field;
    public Ship ship;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public WakePool wakes;
    public int shield;
    public float _size;
    public float distRatio;
    public TurretGroupSpec[] turretGroupSpec = new TurretGroupSpec[EnemyState.TURRET_GROUP_MAX];
    public int turretGroupNum;
    public MovingTurretGroupSpec[] movingTurretGroupSpec = new MovingTurretGroupSpec[EnemyState.MOVING_TURRET_GROUP_MAX];
    public int movingTurretGroupNum;
    public EnemyShape shape, damagedShape, destroyedShape, bridgeShape;
    public int type;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public EnemySpec(Field field, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes)
    {
        this.field = field;
        this.ship = ship;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.wakes = wakes;
        for (int index2 = 0; index2 < EnemyState.TURRET_GROUP_MAX; index2++)
        {
            turretGroupSpec[index2] = new TurretGroupSpec();
        }

        for (int index3 = 0; index3 < EnemyState.MOVING_TURRET_GROUP_MAX; index3++)
        {
            movingTurretGroupSpec[index3] = new MovingTurretGroupSpec();
        }

        distRatio = 0;
        shield = 1;
        _size = 1;
    }

    public void set(int type)
    {
        this.type = type;
        _size = 1;
        distRatio = 0;
        {
            movingTurretGroupNum = 0;
            turretGroupNum = movingTurretGroupNum;
        }
    }

    public TurretGroupSpec getTurretGroupSpec()
    {
        turretGroupNum++;
        turretGroupSpec[turretGroupNum - 1].init();
        return turretGroupSpec[turretGroupNum - 1];
    }

    public MovingTurretGroupSpec getMovingTurretGroupSpec()
    {
        movingTurretGroupNum++;
        movingTurretGroupSpec[movingTurretGroupNum - 1].init();
        return movingTurretGroupSpec[movingTurretGroupNum - 1];
    }

    public void addMovingTurret(float rank, bool bossMode = false)
    {
        int mtn = GameMath.integer((rank * 0.2f));
        if (mtn > EnemyState.MOVING_TURRET_GROUP_MAX)
            mtn = EnemyState.MOVING_TURRET_GROUP_MAX;
        if (mtn >= 2)
            mtn = 1 + rand.nextInt(mtn - 1);
        else
            mtn = 1;
        float br = rank / mtn;
        int type = 0;
        if (!bossMode)
        {
            switch (rand.nextInt(4))
            {
                case 0:
                case 1:
                {
                    type = MovingTurretGroupSpecMoveType.ROLL;
                    break;
                }

                case 2:
                {
                    type = MovingTurretGroupSpecMoveType.SWING_FIX;
                    break;
                }

                case 3:
                {
                    type = MovingTurretGroupSpecMoveType.SWING_AIM;
                    break;
                }
            }
        }
        else
        {
            type = MovingTurretGroupSpecMoveType.ROLL;
        }

        float rad = 0.9f + rand.nextFloat(0.4f) - mtn * 0.1f;
        float radInc = 0.5f + rand.nextFloat(0.25f);
        float ad = PI * 2;
        float a = 0, av = 0, dv = 0, s = 0, sv = 0;
        switch (type)
        {
            case MovingTurretGroupSpecMoveType.ROLL:
            {
                a = 0.01f + rand.nextFloat(0.04f);
                av = 0.01f + rand.nextFloat(0.03f);
                dv = 0.01f + rand.nextFloat(0.04f);
                break;
            }

            case MovingTurretGroupSpecMoveType.SWING_FIX:
            {
                ad = PI / 10 + rand.nextFloat(PI / 15);
                s = 0.01f + rand.nextFloat(0.02f);
                sv = 0.01f + rand.nextFloat(0.03f);
                break;
            }

            case MovingTurretGroupSpecMoveType.SWING_AIM:
            {
                ad = PI / 10 + rand.nextFloat(PI / 15);
                if (rand.nextInt(5) == 0)
                    s = 0.01f + rand.nextFloat(0.01f);
                else
                    s = 0;
                sv = 0.01f + rand.nextFloat(0.02f);
                break;
            }
        }

        for (int i = 0; i < mtn; i++)
        {
            MovingTurretGroupSpec tgs = getMovingTurretGroupSpec();
            tgs.moveType = type;
            tgs.radiusBase = rad;
            float sr = 0;
            switch (type)
            {
                case MovingTurretGroupSpecMoveType.ROLL:
                {
                    tgs.alignDeg = ad;
                    tgs.num = 4 + rand.nextInt(6);
                    if (rand.nextInt(2) == 0)
                    {
                        if (rand.nextInt(2) == 0)
                            tgs.setRoll(dv, 0, 0);
                        else
                            tgs.setRoll(-dv, 0, 0);
                    }
                    else
                    {
                        if (rand.nextInt(2) == 0)
                            tgs.setRoll(0, a, av);
                        else
                            tgs.setRoll(0, -a, av);
                    }

                    if (rand.nextInt(3) == 0)
                        tgs.setRadiusAmp(1 + rand.nextFloat(1), 0.01f + rand.nextFloat(0.03f));
                    if (rand.nextInt(2) == 0)
                        tgs.distRatio = 0.8f + rand.nextSignedFloat(0.3f);
                    sr = br / tgs.num;
                    break;
                }

                case MovingTurretGroupSpecMoveType.SWING_FIX:
                {
                    tgs.num = 3 + rand.nextInt(5);
                    tgs.alignDeg = ad * (tgs.num * 0.1f + 0.3f);
                    if (rand.nextInt(2) == 0)
                        tgs.setSwing(s, sv);
                    else
                        tgs.setSwing(-s, sv);
                    if (rand.nextInt(6) == 0)
                        tgs.setRadiusAmp(1 + rand.nextFloat(1), 0.01f + rand.nextFloat(0.03f));
                    if (rand.nextInt(4) == 0)
                        tgs.setAlignAmp(0.25f + rand.nextFloat(0.25f), 0.01f + rand.nextFloat(0.02f));
                    sr = br / tgs.num;
                    sr = sr * (0.6f);
                    break;
                }

                case MovingTurretGroupSpecMoveType.SWING_AIM:
                {
                    tgs.num = 3 + rand.nextInt(4);
                    tgs.alignDeg = ad * (tgs.num * 0.1f + 0.3f);
                    if (rand.nextInt(2) == 0)
                        tgs.setSwing(s, sv, true);
                    else
                        tgs.setSwing(-s, sv, true);
                    if (rand.nextInt(4) == 0)
                        tgs.setRadiusAmp(1 + rand.nextFloat(1), 0.01f + rand.nextFloat(0.03f));
                    if (rand.nextInt(5) == 0)
                        tgs.setAlignAmp(0.25f + rand.nextFloat(0.25f), 0.01f + rand.nextFloat(0.02f));
                    sr = br / tgs.num;
                    sr = sr * (0.4f);
                    break;
                }
            }

            if (rand.nextInt(4) == 0)
                tgs.setXReverse(-1);
            tgs.turretSpec.setParam_3(sr, TurretSpecTurretType.MOVING, rand);
            if (bossMode)
                tgs.turretSpec.setBossSpec();
            rad = rad + (radInc);
            ad = ad * (1 + rand.nextSignedFloat(0.2f));
        }
    }

    public bool checkCollision(EnemyState es, float x, float y, Collidable c, Shot shot)
    {
        return es.checkCollision(x, y, c, shot);
    }

    public bool checkShipCollision(EnemyState es, float x, float y, bool largeOnly = false)
    {
        if ((es.destroyedCnt >= 0) || (largeOnly && (type != EnemySpecEnemyType.LARGE)))
            return false;
        return shape.checkShipCollision(x - es.pos.x, y - es.pos.y, es.deg);
    }

    public virtual bool move(EnemyState es)
    {
        return es.move();
    }

    public virtual void draw(float[] model, EnemyState es, float[] color = null)
    {
        es.draw(model, color);
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
            if (shape != null)
                shape.size = _size;
            if (damagedShape != null)
                damagedShape.size = _size;
            if (destroyedShape != null)
                destroyedShape.size = _size;
            if (bridgeShape != null)
            {
                float s = 0.9f;
                bridgeShape.size = s * (1 - distRatio);
            }
        }
    }

    public bool isSmallEnemy()
    {
        if (type == EnemySpecEnemyType.SMALL)
            return true;
        else
            return false;
    }

    public abstract int score();
    public abstract bool isBoss();
}

public interface HasAppearType
{
    public bool setFirstState(EnemyState es, int appType);
}

public class SmallShipEnemySpec : EnemySpec, HasAppearType
{
    public int movementType;
    public float accel, maxSpeed, staySpeed;
    public int moveDuration, stayDuration;
    public float speed, turnDeg;
    public SmallShipEnemySpec(Field field, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes) : base(field, ship, sparks, smokes, fragments, wakes)
    {
        movementType = 0;
        {
            staySpeed = 0;
            maxSpeed = staySpeed;
            accel = maxSpeed;
        }

        {
            stayDuration = 1;
            moveDuration = stayDuration;
        }

        {
            turnDeg = 0;
            speed = turnDeg;
        }
    }

    public void setParam(float rank, GunroarRand rand)
    {
        set(EnemySpecEnemyType.SMALL);
        shape = new EnemyShape(EnemyShapeEnemyShapeType.SMALL);
        damagedShape = new EnemyShape(EnemyShapeEnemyShapeType.SMALL_DAMAGED);
        bridgeShape = new EnemyShape(EnemyShapeEnemyShapeType.SMALL_BRIDGE);
        movementType = rand.nextInt(2);
        float sr = rand.nextFloat(rank * 0.8f);
        if (sr > 25)
            sr = 25;
        switch (movementType)
        {
            case SmallShipEnemySpecMoveType.STOPANDGO:
            {
                distRatio = 0.5f;
                size = 0.47f + rand.nextFloat(0.1f);
                accel = 0.5f - 0.5f / (2.0f + rand.nextFloat(rank));
                maxSpeed = 0.05f * (1.0f + sr);
                staySpeed = 0.03f;
                moveDuration = 32 + rand.nextSignedInt(12);
                stayDuration = 32 + rand.nextSignedInt(12);
                break;
            }

            case SmallShipEnemySpecMoveType.CHASE:
            {
                distRatio = 0.5f;
                size = 0.5f + rand.nextFloat(0.1f);
                speed = 0.036f * (1.0f + sr);
                turnDeg = 0.02f + rand.nextSignedFloat(0.04f);
                break;
            }
        }

        shield = 1;
        TurretGroupSpec tgs = getTurretGroupSpec();
        tgs.turretSpec.setParam_3(rank - sr * 0.5f, TurretSpecTurretType.SMALL, rand);
    }

    public bool setFirstState(EnemyState es, int appType)
    {
        es.setSpec(this);
        if (!es.setAppearancePos(field, ship, rand, appType))
            return false;
        switch (movementType)
        {
            case SmallShipEnemySpecMoveType.STOPANDGO:
            {
                es.speed = 0;
                es.state = SmallShipEnemySpecMoveState.MOVING;
                es.cnt = moveDuration;
                break;
            }

            case SmallShipEnemySpecMoveType.CHASE:
            {
                es.speed = speed;
                break;
            }
        }

        return true;
    }

    public override bool move(EnemyState es)
    {
        if (!base.move(es))
            return false;
        switch (movementType)
        {
            case SmallShipEnemySpecMoveType.STOPANDGO:
            {
                es.pos.x = es.pos.x + (sin(es.velDeg) * es.speed);
                es.pos.y = es.pos.y + (cos(es.velDeg) * es.speed);
                es.pos.y = es.pos.y - (field.lastScrollY);
                if (es.pos.y <= -field.outerSize.y)
                    return false;
                if ((field.getBlock_1(es.pos) >= 0) || (!field.checkInOuterHeightField(es.pos)))
                {
                    es.velDeg = es.velDeg + (PI);
                    es.pos.x = es.pos.x + (sin(es.velDeg) * es.speed * 2);
                    es.pos.y = es.pos.y + (cos(es.velDeg) * es.speed * 2);
                }

                switch (es.state)
                {
                    case SmallShipEnemySpecMoveState.MOVING:
                    {
                        es.speed = es.speed + ((maxSpeed - es.speed) * accel);
                        es.cnt--;
                        if (es.cnt <= 0)
                        {
                            es.velDeg = rand.nextFloat(PI * 2);
                            es.cnt = stayDuration;
                            es.state = SmallShipEnemySpecMoveState.STAYING;
                        }

                        break;
                    }

                    case SmallShipEnemySpecMoveState.STAYING:
                    {
                        es.speed = es.speed + ((staySpeed - es.speed) * accel);
                        es.cnt--;
                        if (es.cnt <= 0)
                        {
                            es.cnt = moveDuration;
                            es.state = SmallShipEnemySpecMoveState.MOVING;
                        }

                        break;
                    }
                }

                break;
            }

            case SmallShipEnemySpecMoveType.CHASE:
            {
                es.pos.x = es.pos.x + (sin(es.velDeg) * speed);
                es.pos.y = es.pos.y + (cos(es.velDeg) * speed);
                es.pos.y = es.pos.y - (field.lastScrollY);
                if (es.pos.y <= -field.outerSize.y)
                    return false;
                if ((field.getBlock_1(es.pos) >= 0) || (!field.checkInOuterHeightField(es.pos)))
                {
                    es.velDeg = es.velDeg + (PI);
                    es.pos.x = es.pos.x + (sin(es.velDeg) * es.speed * 2);
                    es.pos.y = es.pos.y + (cos(es.velDeg) * es.speed * 2);
                }

                float ad = 0;
                Vector shipPos = ship.nearPos(es.pos);
                if (shipPos.dist_1(es.pos) < 0.1f)
                    ad = 0;
                else
                    ad = atan2(shipPos.x - es.pos.x, shipPos.y - es.pos.y);
                float chaseOffset = ad - es.velDeg;
                chaseOffset = normalizeDeg(chaseOffset);
                if ((chaseOffset <= turnDeg) && (chaseOffset >= -turnDeg))
                    es.velDeg = ad;
                else if (chaseOffset < 0)
                    es.velDeg = es.velDeg - (turnDeg);
                else
                    es.velDeg = es.velDeg + (turnDeg);
                es.velDeg = normalizeDeg(es.velDeg);
                es.cnt++;
                break;
            }
        }

        float od = es.velDeg - es.deg;
        od = normalizeDeg(od);
        es.deg = es.deg + (od * 0.05f);
        es.deg = normalizeDeg(es.deg);
        if ((es.cnt % 6 == 0) && (es.speed >= 0.03f))
            shape.addWake(wakes, es.pos, es.deg, es.speed);
        return true;
    }

    public override int score()
    {
        return 50;
    }

    public override bool isBoss()
    {
        return false;
    }
}

public class ShipEnemySpec : EnemySpec, HasAppearType
{
    public const int SINK_INTERVAL = 120;
    public float speed, degVel;
    public int shipClass;
    public ShipEnemySpec(Field field, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes) : base(field, ship, sparks, smokes, fragments, wakes)
    {
        {
            degVel = 0;
            speed = degVel;
        }
    }

    public void setParam(float rank, int cls, GunroarRand rand)
    {
        shipClass = cls;
        set(EnemySpecEnemyType.LARGE);
        shape = new EnemyShape(EnemyShapeEnemyShapeType.MIDDLE);
        damagedShape = new EnemyShape(EnemyShapeEnemyShapeType.MIDDLE_DAMAGED);
        destroyedShape = new EnemyShape(EnemyShapeEnemyShapeType.MIDDLE_DESTROYED);
        bridgeShape = new EnemyShape(EnemyShapeEnemyShapeType.MIDDLE_BRIDGE);
        distRatio = 0.7f;
        int mainTurretNum = 0, subTurretNum = 0;
        float movingTurretRatio = 0;
        float rk = rank;
        switch (cls)
        {
            case ShipEnemySpecShipClass.MIDDLE:
            {
                float sz = 1.5f + rank / 15 + rand.nextFloat(rank / 15);
                float ms = 2 + rand.nextFloat(0.5f);
                if (sz > ms)
                    sz = ms;
                size = sz;
                speed = 0.015f + rand.nextSignedFloat(0.005f);
                degVel = 0.005f + rand.nextSignedFloat(0.003f);
                switch (rand.nextInt(3))
                {
                    case 0:
                    {
                        mainTurretNum = GameMath.integer((size * (1 + rand.nextSignedFloat(0.25f)) + 1));
                        break;
                    }

                    case 1:
                    {
                        subTurretNum = GameMath.integer((size * 1.6f * (1 + rand.nextSignedFloat(0.5f)) + 2));
                        break;
                    }

                    case 2:
                    {
                        mainTurretNum = GameMath.integer((size * (0.5f + rand.nextSignedFloat(0.12f)) + 1));
                        movingTurretRatio = 0.5f + rand.nextFloat(0.25f);
                        rk = rank * (1 - movingTurretRatio);
                        movingTurretRatio = movingTurretRatio * (2);
                        break;
                    }
                }

                break;
            }

            case ShipEnemySpecShipClass.LARGE:
            {
                float sz = 2.5f + rank / 24 + rand.nextFloat(rank / 24);
                float ms = 3 + rand.nextFloat(1);
                if (sz > ms)
                    sz = ms;
                size = sz;
                speed = 0.01f + rand.nextSignedFloat(0.005f);
                degVel = 0.003f + rand.nextSignedFloat(0.002f);
                mainTurretNum = GameMath.integer((size * (0.7f + rand.nextSignedFloat(0.2f)) + 1));
                subTurretNum = GameMath.integer((size * 1.6f * (0.7f + rand.nextSignedFloat(0.33f)) + 2));
                movingTurretRatio = 0.25f + rand.nextFloat(0.5f);
                rk = rank * (1 - movingTurretRatio);
                movingTurretRatio = movingTurretRatio * (3);
                break;
            }

            case ShipEnemySpecShipClass.BOSS:
            {
                float sz = 5 + rank / 30 + rand.nextFloat(rank / 30);
                float ms = 9 + rand.nextFloat(3);
                if (sz > ms)
                    sz = ms;
                size = sz;
                speed = ship.scrollSpeedBase + 0.0025f + rand.nextSignedFloat(0.001f);
                degVel = 0.003f + rand.nextSignedFloat(0.002f);
                mainTurretNum = GameMath.integer((size * 0.8f * (1.5f + rand.nextSignedFloat(0.4f)) + 2));
                subTurretNum = GameMath.integer((size * 0.8f * (2.4f + rand.nextSignedFloat(0.6f)) + 2));
                movingTurretRatio = 0.2f + rand.nextFloat(0.3f);
                rk = rank * (1 - movingTurretRatio);
                movingTurretRatio = movingTurretRatio * (2.5f);
                break;
            }
        }

        shield = GameMath.integer((size * 10));
        if (cls == ShipEnemySpecShipClass.BOSS)
            shield = GameMath.integer(shield * 2.4f);
        if (mainTurretNum + subTurretNum <= 0)
        {
            TurretGroupSpec tgs = getTurretGroupSpec();
            tgs.turretSpec.setParam_3(0, TurretSpecTurretType.DUMMY, rand);
        }
        else
        {
            float subTurretRank = rk / (mainTurretNum * 3 + subTurretNum);
            float mainTurretRank = subTurretRank * 2.5f;
            if (cls != ShipEnemySpecShipClass.BOSS)
            {
                int frontMainTurretNum = GameMath.integer((mainTurretNum / 2 + 0.99f));
                int rearMainTurretNum = mainTurretNum - frontMainTurretNum;
                if (frontMainTurretNum > 0)
                {
                    TurretGroupSpec tgs = getTurretGroupSpec();
                    tgs.turretSpec.setParam_3(mainTurretRank, TurretSpecTurretType.MAIN, rand);
                    tgs.num = frontMainTurretNum;
                    tgs.alignType = TurretGroupSpecAlignType.STRAIGHT;
                    tgs.offset.y = -size * (0.9f + rand.nextSignedFloat(0.05f));
                }

                if (rearMainTurretNum > 0)
                {
                    TurretGroupSpec tgs = getTurretGroupSpec();
                    tgs.turretSpec.setParam_3(mainTurretRank, TurretSpecTurretType.MAIN, rand);
                    tgs.num = rearMainTurretNum;
                    tgs.alignType = TurretGroupSpecAlignType.STRAIGHT;
                    tgs.offset.y = size * (0.9f + rand.nextSignedFloat(0.05f));
                }

                TurretSpec pts = null;
                if (subTurretNum > 0)
                {
                    int frontSubTurretNum = (subTurretNum + 2) / 4;
                    int rearSubTurretNum = (subTurretNum - frontSubTurretNum * 2) / 2;
                    int tn = frontSubTurretNum;
                    float ad = -PI / 4;
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 2)
                            tn = rearSubTurretNum;
                        if (tn <= 0)
                            continue;
                        TurretGroupSpec tgs = getTurretGroupSpec();
                        if ((i == 0) || (i == 2))
                        {
                            if (rand.nextInt(2) == 0)
                                tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB, rand);
                            else
                                tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB_DESTRUCTIVE, rand);
                            pts = tgs.turretSpec;
                        }
                        else
                        {
                            tgs.turretSpec.setParam_1(pts);
                        }

                        tgs.num = tn;
                        tgs.alignType = TurretGroupSpecAlignType.ROUND;
                        tgs.alignDeg = ad;
                        ad = ad + (PI / 2);
                        tgs.alignWidth = PI / 6 + rand.nextFloat(PI / 8);
                        tgs.radius = size * 0.75f;
                        tgs.distRatio = distRatio;
                    }
                }
            }
            else
            {
                mainTurretRank = mainTurretRank * (2.5f);
                subTurretRank = subTurretRank * (2);
                TurretSpec pts = null;
                if (mainTurretNum > 0)
                {
                    int frontMainTurretNum = (mainTurretNum + 2) / 4;
                    int rearMainTurretNum = (mainTurretNum - frontMainTurretNum * 2) / 2;
                    int tn = frontMainTurretNum;
                    float ad = -PI / 4;
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 2)
                            tn = rearMainTurretNum;
                        if (tn <= 0)
                            continue;
                        TurretGroupSpec tgs = getTurretGroupSpec();
                        if ((i == 0) || (i == 2))
                        {
                            tgs.turretSpec.setParam_3(mainTurretRank, TurretSpecTurretType.MAIN, rand);
                            pts = tgs.turretSpec;
                            pts.setBossSpec();
                        }
                        else
                        {
                            tgs.turretSpec.setParam_1(pts);
                        }

                        tgs.num = tn;
                        tgs.alignType = TurretGroupSpecAlignType.ROUND;
                        tgs.alignDeg = ad;
                        ad = ad + (PI / 2);
                        tgs.alignWidth = PI / 6 + rand.nextFloat(PI / 8);
                        tgs.radius = size * 0.45f;
                        tgs.distRatio = distRatio;
                    }
                }

                if (subTurretNum > 0)
                {
                    int[] tn = new int[3];
                    tn[0] = (subTurretNum + 2) / 6;
                    tn[1] = (subTurretNum - tn[0] * 2) / 4;
                    tn[2] = (subTurretNum - tn[0] * 2 - tn[1] * 2) / 2;
                    float[] ad = new float[]
                    {
                        PI / 4,
                        -PI / 4,
                        PI / 2,
                        -PI / 2,
                        PI / 4 * 3,
                        -PI / 4 * 3
                    };
                    for (int i = 0; i < 6; i++)
                    {
                        int idx = i / 2;
                        if (tn[idx] <= 0)
                            continue;
                        TurretGroupSpec tgs = getTurretGroupSpec();
                        if (((i == 0) || (i == 2)) || (i == 4))
                        {
                            if (rand.nextInt(2) == 0)
                                tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB, rand);
                            else
                                tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB_DESTRUCTIVE, rand);
                            pts = tgs.turretSpec;
                            pts.setBossSpec();
                        }
                        else
                        {
                            tgs.turretSpec.setParam_1(pts);
                        }

                        tgs.num = tn[idx];
                        tgs.alignType = TurretGroupSpecAlignType.ROUND;
                        tgs.alignDeg = ad[i];
                        tgs.alignWidth = PI / 7 + rand.nextFloat(PI / 9);
                        tgs.radius = size * 0.75f;
                        tgs.distRatio = distRatio;
                    }
                }
            }
        }

        if (movingTurretRatio > 0)
        {
            if (cls == ShipEnemySpecShipClass.BOSS)
                addMovingTurret(rank * movingTurretRatio, true);
            else
                addMovingTurret(rank * movingTurretRatio);
        }
    }

    public bool setFirstState(EnemyState es, int appType)
    {
        es.setSpec(this);
        if (!es.setAppearancePos(field, ship, rand, appType))
            return false;
        es.speed = speed;
        if (es.pos.x < 0)
            es.turnWay = -1;
        else
            es.turnWay = 1;
        if (isBoss())
        {
            es.trgDeg = rand.nextFloat(0.1f) + 0.1f;
            if (rand.nextInt(2) == 0)
                es.trgDeg = es.trgDeg * (-1);
            es.turnCnt = 250 + rand.nextInt(150);
        }

        return true;
    }

    public override bool move(EnemyState es)
    {
        if (es.destroyedCnt >= SINK_INTERVAL)
            return false;
        if (!base.move(es))
            return false;
        es.pos.x = es.pos.x + (sin(es.deg) * es.speed);
        es.pos.y = es.pos.y + (cos(es.deg) * es.speed);
        es.pos.y = es.pos.y - (field.lastScrollY);
        if (((es.pos.x <= -field.outerSize.x - size) || (es.pos.x >= field.outerSize.x + size)) || (es.pos.y <= -field.outerSize.y - size))
            return false;
        if (es.pos.y > field.outerSize.y * 2.2f + size)
            es.pos.y = field.outerSize.y * 2.2f + size;
        if (isBoss())
        {
            es.turnCnt--;
            if (es.turnCnt <= 0)
            {
                es.turnCnt = 250 + rand.nextInt(150);
                es.trgDeg = rand.nextFloat(0.1f) + 0.2f;
                if (es.pos.x > 0)
                    es.trgDeg = es.trgDeg * (-1);
            }

            es.deg = es.deg + ((es.trgDeg - es.deg) * 0.0025f);
            if (ship.higherPos().y > es.pos.y)
                es.speed = es.speed + ((speed * 2 - es.speed) * 0.005f);
            else
                es.speed = es.speed + ((speed - es.speed) * 0.01f);
        }
        else
        {
            if (!es.checkFrontClear())
            {
                es.deg = es.deg + (degVel * es.turnWay);
                es.speed = es.speed * (0.98f);
            }
            else
            {
                if (es.destroyedCnt < 0)
                    es.speed = es.speed + ((speed - es.speed) * 0.01f);
                else
                    es.speed = es.speed * (0.98f);
            }
        }

        es.cnt++;
        if (((es.cnt % 6 == 0) && (es.speed >= 0.01f)) && (es.destroyedCnt < SINK_INTERVAL / 2))
            shape.addWake(wakes, es.pos, es.deg, es.speed);
        return true;
    }

    public override void draw(float[] model, EnemyState es, float[] color = null)
    {
        if (es.destroyedCnt >= 0)
            color = new float[] { EnemyShape.MIDDLE_COLOR_R * (1 - (float)es.destroyedCnt / SINK_INTERVAL) * 0.5f, EnemyShape.MIDDLE_COLOR_G * (1 - (float)es.destroyedCnt / SINK_INTERVAL) * 0.5f, EnemyShape.MIDDLE_COLOR_B * (1 - (float)es.destroyedCnt / SINK_INTERVAL) * 0.5f, 1 };
        base.draw(model, es, color);
    }

    public override int score()
    {
        switch (shipClass)
        {
            case ShipEnemySpecShipClass.MIDDLE:
            {
                return 100;
            }

            case ShipEnemySpecShipClass.LARGE:
            {
                return 300;
            }

            case ShipEnemySpecShipClass.BOSS:
            {
                return 1000;
            }
        }

        return 0;
    }

    public override bool isBoss()
    {
        if (shipClass == ShipEnemySpecShipClass.BOSS)
            return true;
        return false;
    }
}

public class PlatformEnemySpec : EnemySpec
{
    public PlatformEnemySpec(Field field, Ship ship, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes) : base(field, ship, sparks, smokes, fragments, wakes)
    {
    }

    public void setParam(float rank, GunroarRand rand)
    {
        set(EnemySpecEnemyType.PLATFORM);
        shape = new EnemyShape(EnemyShapeEnemyShapeType.PLATFORM);
        damagedShape = new EnemyShape(EnemyShapeEnemyShapeType.PLATFORM_DAMAGED);
        destroyedShape = new EnemyShape(EnemyShapeEnemyShapeType.PLATFORM_DESTROYED);
        bridgeShape = new EnemyShape(EnemyShapeEnemyShapeType.PLATFORM_BRIDGE);
        distRatio = 0;
        size = 1 + rank / 30 + rand.nextFloat(rank / 30);
        float ms = 1 + rand.nextFloat(0.25f);
        if (size > ms)
            size = ms;
        int mainTurretNum = 0, frontTurretNum = 0, sideTurretNum = 0;
        float rk = rank;
        float movingTurretRatio = 0;
        switch (rand.nextInt(3))
        {
            case 0:
            {
                frontTurretNum = GameMath.integer((size * (2 + rand.nextSignedFloat(0.5f)) + 1));
                movingTurretRatio = 0.33f + rand.nextFloat(0.46f);
                rk = rk * ((1 - movingTurretRatio));
                movingTurretRatio = movingTurretRatio * (2.5f);
                break;
            }

            case 1:
            {
                frontTurretNum = GameMath.integer((size * (0.5f + rand.nextSignedFloat(0.2f)) + 1));
                sideTurretNum = GameMath.integer((size * (0.5f + rand.nextSignedFloat(0.2f)) + 1)) * 2;
                break;
            }

            case 2:
            {
                mainTurretNum = GameMath.integer((size * (1 + rand.nextSignedFloat(0.33f)) + 1));
                break;
            }
        }

        shield = GameMath.integer((size * 20));
        int subTurretNum = frontTurretNum + sideTurretNum;
        float subTurretRank = rk / (mainTurretNum * 3 + subTurretNum);
        float mainTurretRank = subTurretRank * 2.5f;
        if (mainTurretNum > 0)
        {
            TurretGroupSpec tgs = getTurretGroupSpec();
            tgs.turretSpec.setParam_3(mainTurretRank, TurretSpecTurretType.MAIN, rand);
            tgs.num = mainTurretNum;
            tgs.alignType = TurretGroupSpecAlignType.ROUND;
            tgs.alignDeg = 0;
            tgs.alignWidth = PI * 0.66f + rand.nextFloat(PI / 2);
            tgs.radius = size * 0.7f;
            tgs.distRatio = distRatio;
        }

        if (frontTurretNum > 0)
        {
            TurretGroupSpec tgs = getTurretGroupSpec();
            tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB, rand);
            tgs.num = frontTurretNum;
            tgs.alignType = TurretGroupSpecAlignType.ROUND;
            tgs.alignDeg = 0;
            tgs.alignWidth = PI / 5 + rand.nextFloat(PI / 6);
            tgs.radius = size * 0.8f;
            tgs.distRatio = distRatio;
        }

        sideTurretNum = sideTurretNum / (2);
        if (sideTurretNum > 0)
        {
            TurretSpec pts = null;
            for (int i = 0; i < 2; i++)
            {
                TurretGroupSpec tgs = getTurretGroupSpec();
                if (i == 0)
                {
                    tgs.turretSpec.setParam_3(subTurretRank, TurretSpecTurretType.SUB, rand);
                    pts = tgs.turretSpec;
                }
                else
                {
                    tgs.turretSpec.setParam_1(pts);
                }

                tgs.num = sideTurretNum;
                tgs.alignType = TurretGroupSpecAlignType.ROUND;
                tgs.alignDeg = PI / 2 - PI * i;
                tgs.alignWidth = PI / 5 + rand.nextFloat(PI / 6);
                tgs.radius = size * 0.75f;
                tgs.distRatio = distRatio;
            }
        }

        if (movingTurretRatio > 0)
        {
            addMovingTurret(rank * movingTurretRatio);
        }
    }

    public bool setFirstState(EnemyState es, float x, float y, float d)
    {
        es.setSpec(this);
        es.pos.x = x;
        es.pos.y = y;
        es.deg = d;
        es.speed = 0;
        if (!es.checkFrontClear(true))
            return false;
        return true;
    }

    public override bool move(EnemyState es)
    {
        if (!base.move(es))
            return false;
        es.pos.y = es.pos.y - (field.lastScrollY);
        if (es.pos.y <= -field.outerSize.y)
            return false;
        return true;
    }

    public override int score()
    {
        return 100;
    }

    public override bool isBoss()
    {
        return false;
    }
}

public class EnemyPool : ActorPool<Enemy>
{
    public EnemyPool(int n, List<object> args) : base(n, args, () => new Enemy())
    {
        foreach (Enemy e in actor)
            e.setEnemyPool(this);
    }

    public void setStageManager(StageManager stageManager)
    {
        foreach (Enemy e in actor)
            e.setStageManager(stageManager);
    }

    public void checkShotHit(Vector pos, Collidable shape, Shot shot = null)
    {
        foreach (Enemy e in actor)
            if (e.exists)
                e.checkShotHit(pos, shape, shot);
    }

    public Enemy checkHitShip(float x, float y, Enemy deselection = null, bool largeOnly = false)
    {
        foreach (Enemy e in actor)
            if (e.exists && (e != deselection))
                if (e.checkHitShip(x, y, largeOnly))
                    return e;
        return null;
    }

    public bool hasBoss()
    {
        foreach (Enemy e in actor)
            if (e.exists && e.isBoss())
                return true;
        return false;
    }
}

public static class ShipEnemySpecShipClass
{
    public const int MIDDLE = 0;
    public const int LARGE = 1;
    public const int BOSS = 2;
}

public static class SmallShipEnemySpecMoveState
{
    public const int STAYING = 0;
    public const int MOVING = 1;
}

public static class SmallShipEnemySpecMoveType
{
    public const int STOPANDGO = 0;
    public const int CHASE = 1;
}

public static class EnemySpecEnemyType
{
    public const int SMALL = 0;
    public const int LARGE = 1;
    public const int PLATFORM = 2;
}

public static class EnemyStateAppearanceType
{
    public const int TOP = 0;
    public const int SIDE = 1;
    public const int CENTER = 2;
}
