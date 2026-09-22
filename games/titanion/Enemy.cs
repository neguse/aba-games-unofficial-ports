// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class EnemyPool : ActorPool<Enemy>
{
    public EnemyPool(int n) : base(n, null, () => new Enemy())
    {
    }

    public static bool trailEffect = false;
    public Field _field;
    public virtual Enemy getNearestEnemy(Vector p)
    {
        float dst = 99999;
        Enemy ne = null;
        foreach (Enemy e in actors)
        {
            if ((((e.exists)) && (((!((e.isBeingCaptured_0())))))))
                if (_field.calcCircularDist_2(e.pos(), p) < dst)
                {
                    dst = _field.calcCircularDist_2(e.pos(), p);
                    ne = e;
                }
        }

        return ne;
    }

    public virtual Enemy getNearestMiddleEnemy(Vector p)
    {
        float dst = 99999;
        Enemy ne = null;
        foreach (Enemy e in actors)
        {
            if ((((e.exists)) && (((!((e.isBeingCaptured_0())))))))
                if (((e.spec is MiddleEnemySpec ? (MiddleEnemySpec)e.spec : null)) != null)
                    if (_field.calcCircularDist_2(e.pos(), p) < dst)
                    {
                        dst = _field.calcCircularDist_2(e.pos(), p);
                        ne = e;
                    }
        }

        return ne;
    }

    public virtual bool checkShotHit(Vector p, float deg, float widthRatio = 1.0f)
    {
        Enemy e = getNearestEnemy(p);
        if ((e) != null)
        {
            float ox = _field.normalizeX(e.pos().x - p.x);
            float oy = e.pos().y - p.y;
            if (((fabs(ox) < 1.0f * e.state.size.x)) && ((fabs(oy) < 1.0f * e.state.size.y * widthRatio)))
            {
                e.hitShot_1(deg);
                return true;
            }
        }

        return false;
    }

    public virtual bool checkBulletHit(Vector p, Vector pp)
    {
        bool hitf = false;
        foreach (Enemy e in actors)
        {
            if ((((e.exists)) && ((e.isCaptured_0()))))
                if (_field.checkHitDist_4(e.pos(), p, pp, EnemySpec.BULLET_HIT_WIDTH))
                {
                    e.hitCaptured_0();
                    hitf = true;
                }
        }

        return hitf;
    }

    public virtual bool checkEnemyHit_2(Vector p, Vector size)
    {
        bool hitf = false;
        foreach (Enemy e in actors)
        {
            if ((((e.exists)) && ((e.isCaptured_0()))))
            {
                float ox = _field.normalizeX(e.pos().x - p.x);
                float oy = e.pos().y - p.y;
                if (((fabs(ox) < 0.5f * (e.state.size.x + size.x))) && ((fabs(oy) < 0.5f * (e.state.size.y + size.y))))
                {
                    e.hitCaptured_0();
                    hitf = true;
                }
            }
        }

        return hitf;
    }

    public virtual bool checkMiddleEnemyExists(float x, float px)
    {
        foreach (Enemy e in actors)
        {
            if ((((e.exists)) && (((!((e.isBeingCaptured_0())))))))
                if (((e.spec is MiddleEnemySpec ? (MiddleEnemySpec)e.spec : null)) != null)
                    if ((e.pos().x - x) * (e.pos().x - px) < 0)
                        return true;
        }

        return false;
    }

    public virtual int num()
    {
        int n = 0;
        foreach (Enemy e in actors)
            if ((((e.exists)) && (((!((e.isCaptured_0())))))))
                n++;
        return n;
    }

    public virtual int numInAttack()
    {
        int n = 0;
        foreach (Enemy e in actors)
            if ((((e.exists)) && ((e.isInAttack_0()))))
                n++;
        return n;
    }

    public virtual int numInScreen()
    {
        int n = 0;
        foreach (Enemy e in actors)
            if ((((e.exists)) && ((e.isInScreen_0()))))
                n++;
        return n;
    }

    public virtual int numBeforeAlign()
    {
        int n = 0;
        foreach (Enemy e in actors)
            if ((((e.exists)) && ((e.beforeAlign_0()))))
                n++;
        return n;
    }

    public virtual void drawFront()
    {
        if (trailEffect)
            foreach (Enemy a in actors)
                if (((a.exists)) && ((a.state.pos.y <= _field.size().y * 1.5f)))
                    a.drawTrails_0();
        foreach (Enemy a in actors)
            if (((a.exists)) && ((a.state.pos.y <= _field.size().y * 1.5f)))
                a.draw_0();
    }

    public virtual void drawBack()
    {
        if (trailEffect)
            foreach (Enemy a in actors)
                if (((((a.exists)) && ((a.state.pos.y > _field.size().y * 1.5f)))) && (((((a.state.pos.x <= _field.circularDistance() / 4)) && ((a.state.pos.x >= -_field.circularDistance() / 4))))))
                    a.drawTrails_0();
        foreach (Enemy a in actors)
            if (((((a.exists)) && ((a.state.pos.y > _field.size().y * 1.5f)))) && (((((a.state.pos.x <= _field.circularDistance() / 4)) && ((a.state.pos.x >= -_field.circularDistance() / 4))))))
                a.draw_0();
    }

    public virtual void drawPillarBack()
    {
        if (trailEffect)
            foreach (Enemy a in actors)
                if (((((a.exists)) && ((a.state.pos.y > _field.size().y * 1.5f)))) && (((((a.state.pos.x > _field.circularDistance() / 4)) || ((a.state.pos.x < -_field.circularDistance() / 4))))))
                    a.drawTrails_0();
        foreach (Enemy a in actors)
            if (((((a.exists)) && ((a.state.pos.y > _field.size().y * 1.5f)))) && (((((a.state.pos.x > _field.circularDistance() / 4)) || ((a.state.pos.x < -_field.circularDistance() / 4))))))
                a.draw_0();
    }

    public virtual Field field(Field v)
    {
        _field = v;
        return _field;
    }
}

public class Enemy : Token<EnemyState, EnemySpec>
{
    public override void init_1(List<object> args)
    {
        state = new EnemyState();
        state.enemy = this;
    }

    public virtual void setSmallEnemyState(float baseSpeed, float angVel, int waitCnt, int appPattern, float er = 0, float ed = 0, bool gd = false, float fireIntervalRatio = 0, Enemy firstEnemy = null)
    {
        {
            state.baseSpeed = baseSpeed;
            state.baseBaseSpeed = state.baseSpeed;
        }

        {
            state.angVel = angVel;
            state.baseAngVel = state.angVel;
        }

        state.waitCnt = waitCnt;
        state.ellipseRatio = er;
        state.ellipseDeg = ed;
        state.isGoingDownBeforeStandBy = gd;
        switch (appPattern)
        {
            case 0:
                state.phase = -200;
                break;
            case 1:
                state.phase = -100;
                break;
        }

        if ((firstEnemy) != null)
        {
            ((spec is SmallEnemySpec ? (SmallEnemySpec)spec : null)).init_2(state, firstEnemy.state);
            state.isFirstEnemy = false;
        }
        else
        {
            spec.init_1(state);
            state.isFirstEnemy = true;
        }
    }

    public virtual void setMiddleEnemyState(float baseSpeed, float angVel, float er = 0, float ed = 0)
    {
        {
            state.baseSpeed = baseSpeed;
            state.baseBaseSpeed = state.baseSpeed;
        }

        {
            state.angVel = angVel;
            state.baseAngVel = state.angVel;
        }

        state.ellipseRatio = er;
        state.ellipseDeg = ed;
        spec.init_1(state);
    }

    public virtual void setGhostEnemyState(float x, float y, float deg, int cnt)
    {
        state.pos.x = x;
        state.pos.y = y;
        state.deg = deg;
        state.cnt = cnt;
    }

    public virtual void hitShot_1(float deg = 0)
    {
        if (spec.hitShot_2(state, deg))
            remove();
    }

    public virtual void hitCaptured_0()
    {
        SmallEnemySpec ses = (spec is SmallEnemySpec ? (SmallEnemySpec)spec : null);
        if ((ses) != null)
            ses.hitCaptured_1(state);
    }

    public virtual void destroyed_0()
    {
        spec.destroyed_2(state);
        exists = false;
    }

    public virtual bool isInAttack_0()
    {
        if (spec.isBeingCaptured_1(state))
            return false;
        return spec.isInAttack_1(state);
    }

    public virtual bool isInScreen_0()
    {
        if (spec.isBeingCaptured_1(state))
            return false;
        return spec.isInScreen_1(state);
    }

    public virtual bool isBeingCaptured_0()
    {
        return spec.isBeingCaptured_1(state);
    }

    public virtual bool isCaptured_0()
    {
        GhostEnemySpec ges = (spec is GhostEnemySpec ? (GhostEnemySpec)spec : null);
        if ((ges) != null)
            return true;
        SmallEnemySpec ses = (spec is SmallEnemySpec ? (SmallEnemySpec)spec : null);
        if ((!(((ses) != null))))
            return false;
        return ses.isCaptured_1(state);
    }

    public virtual bool beforeAlign_0()
    {
        if (spec.isBeingCaptured_1(state))
            return false;
        return spec.beforeAlign_1(state);
    }

    public virtual void drawTrails_0()
    {
        spec.drawTrails_1(state);
    }

    public override Vector pos()
    {
        return state.pos;
    }
}

public class EnemyState : TokenState
{
    public const int TRAIL_NUM = 64;
    public const int TRAIL_INTERVAL = 8;
    public const int TURRET_MAX_NUM = 3;
    public TurretState[] turretStates = new TurretState[TURRET_MAX_NUM];
    public Enemy enemy;
    public Vector vel;
    public Vector centerPos, centerVel, standByPos;
    public float baseBaseSpeed, baseSpeed;
    public float baseAngVel, angVel;
    public int waitCnt;
    public int cnt;
    public float ellipseRatio, ellipseDeg;
    public float shield;
    public int phase;
    public int phaseCnt, nextPhaseCnt;
    public int captureState, captureIdx;
    public bool isGoingDownBeforeStandBy;
    public Vector size, targetSize, sizeVel;
    public Trail[] trails;
    public int trailIdx;
    public bool trailLooped;
    public bool isFirstEnemy;
    public float anger;
    public EnemyState() : base()
    {
        for (int i = 0; i < TURRET_MAX_NUM; i++)
            turretStates[i] = new TurretState();
        vel = new Vector();
        centerPos = new Vector();
        centerVel = new Vector();
        standByPos = new Vector();
        size = new Vector();
        targetSize = new Vector();
        sizeVel = new Vector();
        trails = new Trail[TRAIL_NUM];
        for (int i = 0; i < TRAIL_NUM; i++)
            trails[i] = new Trail();
    }

    public override void clear()
    {
        foreach (TurretState ts in turretStates)
            ts.clear();
        {
            vel.y = 0;
            vel.x = vel.y;
        }

        {
            centerPos.y = 0;
            centerPos.x = centerPos.y;
        }

        {
            centerVel.y = 0;
            centerVel.x = centerVel.y;
        }

        {
            standByPos.y = 0;
            standByPos.x = standByPos.y;
        }

        {
            baseSpeed = 0;
            baseBaseSpeed = baseSpeed;
        }

        {
            angVel = 0;
            baseAngVel = angVel;
        }

        waitCnt = 0;
        cnt = 0;
        ellipseRatio = 0;
        ellipseDeg = 0;
        shield = 0;
        phase = 0;
        {
            nextPhaseCnt = 0;
            phaseCnt = nextPhaseCnt;
        }

        captureState = 0;
        captureIdx = 0;
        isGoingDownBeforeStandBy = false;
        {
            size.y = 1;
            size.x = size.y;
        }

        {
            targetSize.y = 1;
            targetSize.x = targetSize.y;
        }

        {
            sizeVel.y = 0;
            sizeVel.x = sizeVel.y;
        }

        trailIdx = 0;
        trailLooped = false;
        isFirstEnemy = false;
        anger = 0;
        base.clear();
    }

    public virtual void move_0()
    {
        cnt++;
        anger = anger * (0.9995f);
    }

    public virtual void recordTrail()
    {
        trails[trailIdx].set_4(pos.x, pos.y, deg, cnt);
        trailIdx++;
        if (trailIdx >= TRAIL_NUM)
        {
            trailIdx = 0;
            trailLooped = true;
        }
    }

    public virtual void drawTrails_6(EnemyShape s, float r, float g, float b, Vector size, Field field)
    {
        int ti = trailIdx;
        float a = 1.0f;
        for (int i = 0; i < GameMath.integer(TRAIL_NUM / TRAIL_INTERVAL); i++)
        {
            ti = ti - (TRAIL_INTERVAL);
            if (ti < 0)
            {
                if (trailLooped)
                    ti = ti + (TRAIL_NUM);
                else
                    break;
            }

            Trail t = trails[ti];
            TtnScreen.setColor(r * a, g * a, b * a, a * 0.66f);
            Vector3 p = field.calcCircularPos_1(t.pos);
            float cd = field.calcCircularDeg(t.pos.x);
            s.draw_5(p, cd, t.deg, t.cnt, size);
            a = a * (0.7f);
        }
    }
}

public abstract class EnemySpec : TokenSpec<EnemyState>
{
    public static TitanionRand rand = new TitanionRand();
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public const float BULLET_HIT_WIDTH = 0.8f;
    public const float NEXT_PHASE_DIST = 5;
    public const int TURRET_MAX_NUM = 3;
    public BulletPool bullets;
    public Player player;
    public ParticlePool particles, bonusParticles;
    public EnemyPool enemies;
    public Stage stage;
    public EnemyShape trailShape;
    public BulletSpec bulletSpec, counterBulletSpec;
    public TurretSpec[] turretSpecs = new TurretSpec[TURRET_MAX_NUM];
    public int turretNum;
    public float turretWidth = 0;
    public GameState gameState;
    public float shield = 1;
    public float rank = 0;
    public bool capturable;
    public int score;
    public string explosionSeName;
    public bool removeBullets;
    public EnemySpec(Field field = null, BulletPool bullets = null, Player player = null, ParticlePool particles = null, ParticlePool bonusParticles = null, EnemyPool enemies = null, Stage stage = null, Shape shape = null, EnemyShape trailShape = null, BulletSpec bulletSpec = null, BulletSpec counterBulletSpec = null, GameState gameState = null)
    {
        this.field = field;
        this.bullets = bullets;
        this.player = player;
        this.particles = particles;
        this.bonusParticles = bonusParticles;
        this.enemies = enemies;
        this.stage = stage;
        this.shape = shape;
        this.trailShape = trailShape;
        this.bulletSpec = bulletSpec;
        this.counterBulletSpec = counterBulletSpec;
        this.gameState = gameState;
    }

    public override void set_1(EnemyState es)
    {
        es.shield = shield;
        for (int i = 0; i < turretNum; i++)
            turretSpecs[i].set_1(es.turretStates[i]);
    }

    public override bool move_1(EnemyState es)
    {
        {
            es.move_0();
            if (((isInScreen_1(es))) && ((es.isFirstEnemy)))
            {
                Sound.playSe("flying_down.wav");
                es.isFirstEnemy = false;
            }

            if (es.captureState > 0)
            {
                moveCaptured(es);
                return true;
            }

            if (player.enemiesHasCollision())
            {
                if (enemies.checkEnemyHit_2(es.pos, es.size))
                {
                    destroyed_2(es);
                    return false;
                }
            }

            if (player.checkEnemyHit_3(es.pos, es.vel, es.size))
            {
                destroyed_2(es);
                return false;
            }

            if (capturable)
                checkCaptured(es);
            float er = (1 - es.ellipseRatio) + fabs(sin(es.deg + es.ellipseDeg)) * es.ellipseRatio * 2;
            float rk = rank;
            es.vel.x = es.vel.x - (sin(es.deg) * es.speed * er * 0.1f * rk);
            es.vel.y = es.vel.y + (cos(es.deg) * es.speed * er * 0.1f * rk);
            es.vel.opMulAssign(0.9f);
            es.pos.opAddAssign(es.vel);
            if (isInScreen_1(es))
                field.addSlowdownRatio(es.speed * 0.04f * rk);
            es.pos.x = field.normalizeX(es.pos.x);
            es.recordTrail();
            if (((((es.phase >= -50)) && ((es.phase < 0)))) && ((!((field.containsIncludingPit(es.pos))))))
                return false;
            if (es.waitCnt > 0)
            {
                es.waitCnt--;
            }
            else
            {
                Vector cp = es.centerPos;
                es.centerPos.x = field.normalizeX(es.centerPos.x);
                es.phaseCnt++;
                if (field.calcCircularDist_2(es.centerPos, es.pos) < NEXT_PHASE_DIST)
                {
                    es.nextPhaseCnt--;
                    if (es.nextPhaseCnt <= 0)
                    {
                        es.phase++;
                        if (!((gotoNextPhase(es))))
                            return false;
                    }
                }

                cp.x = field.normalizeX(cp.x);
                float dst = field.calcCircularDist_2(cp, es.pos);
                es.speed = es.speed + (((es.baseSpeed * (1 + dst * 0.1f)) - es.speed) * 0.05f);
                float av = es.angVel * rk;
                float td = atan2(field.normalizeX(-(cp.x - es.pos.x)), cp.y - es.pos.y);
                float ad = GameMath.normalizeDeg(td - es.deg);
                av = av * ((2.5f - er));
                if (((ad > av)) || ((ad < -PI * 0.8f)))
                    es.deg = es.deg + (av);
                else if (ad < -av)
                    es.deg = es.deg - (av);
                else
                    es.deg = td;
                for (int i = 0; i < turretNum; i++)
                {
                    TurretState ts = es.turretStates[i];
                    float tx = es.pos.x;
                    float ty = es.pos.y;
                    switch (i)
                    {
                        case 0:
                            break;
                        case 1:
                            tx = tx - (turretWidth);
                            break;
                        case 2:
                            tx = tx + (turretWidth);
                            break;
                    }

                    float turretDeg = atan2(field.normalizeX(-(player.pos().x - tx)), player.pos().y - ty);
                    switch (gameState.mode_0())
                    {
                        case GameStateMode.CLASSIC:
                            if (((turretDeg >= 0)) && ((turretDeg < PI - PI / 6)))
                                turretDeg = PI - PI / 6;
                            else if (((turretDeg < 0)) && ((turretDeg > -PI + PI / 6)))
                                turretDeg = -PI + PI / 6;
                            turretDeg = GameMath.integer(((turretDeg + PI / 64) / (PI / 32))) * (PI / 32);
                            break;
                        case GameStateMode.BASIC:
                            if (((turretDeg >= 0)) && ((turretDeg < PI - PI / 4)))
                                turretDeg = PI - PI / 4;
                            else if (((turretDeg < 0)) && ((turretDeg > -PI + PI / 4)))
                                turretDeg = -PI + PI / 4;
                            break;
                        case GameStateMode.MODERN:
                            break;
                    }

                    ts.update(tx, ty, turretDeg);
                }

                movePhase(es);
                es.sizeVel.x = es.sizeVel.x + ((es.targetSize.x - es.size.x) * 0.2f);
                es.sizeVel.y = es.sizeVel.y + ((es.targetSize.y - es.size.y) * 0.2f);
                es.size.opAddAssign(es.sizeVel);
                es.sizeVel.opMulAssign(0.95f);
            }

            return true;
        }
    }

    public virtual void moveCaptured(EnemyState es)
    {
        {
            switch (es.captureState)
            {
                case 1:
                    es.vel.x = es.vel.x + ((player.pos().x - es.pos.x) * 0.03f);
                    es.vel.y = es.vel.y + ((player.pos().y - es.pos.y) * 0.03f);
                    es.pos.x = es.pos.x + ((player.pos().x - es.pos.x) * 0.03f);
                    es.pos.y = es.pos.y + ((player.pos().y - es.pos.y) * 0.03f);
                    es.deg = es.deg * (0.95f);
                    if (player.pos().dist_1(es.pos) < 1)
                        es.captureState = 2;
                    break;
                case 2:
                    float cx = calcCapturePosX(es.captureIdx);
                    es.vel.x = es.vel.x + ((player.pos().x + cx - es.pos.x) * 0.03f);
                    es.pos.x = es.pos.x + ((player.pos().x + cx - es.pos.x) * 0.1f);
                    es.pos.y = es.pos.y + ((player.pos().y - es.pos.y) * 0.33f);
                    es.vel.y = es.vel.y * (0.6f);
                    es.deg = es.deg * (0.95f);
                    if (fabs(player.pos().x + cx - es.pos.x) < 0.2f)
                        es.captureState = 3;
                    break;
                case 3:
                    float cx3 = calcCapturePosX(es.captureIdx);
                    es.pos.x = player.pos().x + cx3;
                    es.pos.y = player.pos().y;
                    es.deg = player.deg();
                    break;
            }

            es.vel.opMulAssign(0.9f);
            es.pos.opAddAssign(es.vel);
        }
    }

    public virtual float calcCapturePosX(int idx)
    {
        if (idx % 2 == 0)
            return ((GameMath.integer(idx / 2)) + 0.5f) * PlayerSpec.CAPTURED_ENEMIES_INTERVAL_LENGTH * player.capturedEnemyWidth();
        else
            return -((GameMath.integer(idx / 2)) + 0.5f) * PlayerSpec.CAPTURED_ENEMIES_INTERVAL_LENGTH * player.capturedEnemyWidth();
    }

    public virtual void checkCaptured(EnemyState es)
    {
        {
            if (player.isInTractorBeam(es.pos))
            {
                if ((gameState.mode_0() != GameStateMode.MODERN))
                {
                    int idx = player.addCapturedEnemy(es.enemy);
                    if (idx >= 0)
                    {
                        es.captureState = 1;
                        es.captureIdx = idx;
                    }
                }
                else
                {
                    provacated(es);
                }
            }
        }
    }

    public virtual void hitCaptured_1(EnemyState es)
    {
        player.destroyCapturedEnemies(es.captureIdx);
    }

    public virtual bool isBeingCaptured_1(EnemyState es)
    {
        return (es.captureState > 0);
    }

    public virtual bool isCaptured_1(EnemyState es)
    {
        return (es.captureState == 3);
    }

    public virtual bool beforeAlign_1(EnemyState es)
    {
        return (es.phase < -10);
    }

    public virtual bool hitShot_2(EnemyState es, float dd = 0)
    {
        {
            es.shield--;
            float r = 0.5f + rand.nextFloat(0.5f);
            float g = 0.1f + rand.nextFloat(0.5f);
            float b = 0.5f + rand.nextFloat(0.5f);
            for (int i = 0; i < 10; i++)
            {
                Particle p = null;
                float d = 0;
                p = particles.getInstanceForced();
                d = dd + rand.nextSignedFloat(PI / 4);
                p.set_13(ParticleShape.LINE, es.pos.x, es.pos.y, d, 0.1f + rand.nextFloat(0.5f), 1, r, g, b, 30 + rand.nextInt(30));
                p = particles.getInstanceForced();
                d = dd + PI + rand.nextSignedFloat(PI / 4);
                p.set_13(ParticleShape.LINE, es.pos.x, es.pos.y, d, 0.1f + rand.nextFloat(0.5f), 1, r, g, b, 30 + rand.nextInt(30));
            }

            if (es.shield <= 0)
            {
                destroyed_2(es, dd);
                return true;
            }

            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                    es.targetSize.x = es.targetSize.x * (1.3f);
                    es.targetSize.y = es.targetSize.y * (1.3f);
                    break;
                case GameStateMode.BASIC:
                    es.targetSize.x = es.targetSize.x * (1.2f);
                    es.targetSize.y = es.targetSize.y * (1.2f);
                    break;
                case GameStateMode.MODERN:
                    es.targetSize.x = es.targetSize.x * (1.01f);
                    es.targetSize.y = es.targetSize.y * (1.01f);
                    break;
            }

            es.sizeVel.x = 0.3f;
            es.sizeVel.y = 0.3f;
            return false;
        }
    }

    public virtual void destroyed_2(EnemyState es, float dd = 0)
    {
        {
            float r = 0.5f + rand.nextFloat(0.5f);
            float g = 0.1f + rand.nextFloat(0.5f);
            float b = 0.5f + rand.nextFloat(0.5f);
            float sz = (es.targetSize.x + es.targetSize.y) / 2;
            sz = (sz - 1) * 2 + 1;
            int n = 3 + rand.nextInt(2);
            n = GameMath.integer(n * (sz));
            for (int i = 0; i < n; i++)
            {
                Particle p = particles.getInstanceForced();
                float d = dd + rand.nextSignedFloat(PI / 5);
                p.set_13(ParticleShape.TRIANGLE, es.pos.x, es.pos.y, d, 0.5f, (2 + rand.nextFloat(0.5f)) * sz, r, g, b, 50 + rand.nextInt(100));
            }

            for (int i = 0; i < n; i++)
            {
                Particle p = particles.getInstanceForced();
                float d = rand.nextFloat(PI * 2);
                p.set_13(ParticleShape.QUAD, es.pos.x, es.pos.y, d, 0.1f + rand.nextFloat(0.1f), (1 + rand.nextFloat(0.5f)) * sz, r, g, b, 50 + rand.nextInt(100));
            }

            if (!((isBeingCaptured_1(es))))
            {
                if (removeBullets)
                {
                    int bonusCount = 1;
                    bonusCount = bullets.removeAround(bonusCount, es.pos, particles, bonusParticles, player);
                    Particle p = bonusParticles.getInstanceForced();
                    int wc = 0;
                    if (bonusCount <= 50)
                        wc = bonusCount;
                    else
                        wc = 50 + GameMath.integer(sqrt((float)(bonusCount - 50)));
                    p.set_13(ParticleShape.BONUS, es.pos.x, es.pos.y, 0, 0.1f, 1.0f + (float)wc / 75, 1, 1, 1, 120, false, bonusCount, wc);
                    player.addScore_1(score * bonusCount);
                }
                else
                {
                    if ((gameState.mode_0() == GameStateMode.BASIC))
                    {
                        float oy = es.pos.y - player.pos().y;
                        int pm = GameMath.integer((18 - oy));
                        if (pm > 16)
                            pm = 16;
                        else if (pm < 1)
                            pm = 1;
                        player.addScore_1(score * pm);
                        Particle p = bonusParticles.getInstanceForced();
                        p.set_13(ParticleShape.BONUS, es.pos.x, es.pos.y, 0, 0.1f, 0.5f, 1, 1, 1, 60, false, pm);
                        gameState.setProximityMultiplier(pm);
                    }
                    else
                    {
                        player.addScore_1(score);
                    }
                }

                player.addMultiplier(0.1f);
                if (stage.existsCounterBullet())
                {
                    Bullet blt = bullets.getInstance();
                    if ((blt) != null)
                        blt.setAt(counterBulletSpec, es.pos, es.turretStates[0].deg, turretSpecs[0].speed * TurretSpec.SPEED_RATIO);
                }
            }

            Sound.playSe(explosionSeName);
        }
    }

    public virtual void provacated(EnemyState es)
    {
        {
            es.anger = es.anger + ((1 - es.anger) * 0.05f);
            if (es.sizeVel.dist_2() < 0.1f)
            {
                es.sizeVel.x = 0.2f;
                es.sizeVel.y = 0.2f;
            }

            Particle p = null;
            p = particles.getInstanceForced();
            p.set_13(ParticleShape.LINE, es.pos.x, es.pos.y, PI / 2 + rand.nextSignedFloat(PI / 4), 0.1f + rand.nextFloat(0.2f), 1, 1, 0.5f, 0.5f, 30 + rand.nextInt(30));
            p = particles.getInstanceForced();
            p.set_13(ParticleShape.LINE, es.pos.x, es.pos.y, -PI / 2 + rand.nextSignedFloat(PI / 4), 0.1f + rand.nextFloat(0.2f), 1, 1, 0.5f, 0.5f, 30 + rand.nextInt(30));
            if (removeBullets)
                player.midEnemyProvacated();
        }
    }

    public virtual bool gotoNextPhaseInAppearing(EnemyState es)
    {
        {
            switch (es.phase)
            {
                case -300:
                    float cpw = 0;
                    switch (gameState.mode_0())
                    {
                        case GameStateMode.CLASSIC:
                        case GameStateMode.BASIC:
                            cpw = 0.2f;
                            break;
                        case GameStateMode.MODERN:
                            cpw = 0.4f;
                            break;
                    }

                    es.centerPos.x = rand.nextSignedFloat(field.size().x * cpw);
                    es.centerPos.y = field.size().y * 2.0f;
                    es.standByPos.x = rand.nextSignedFloat(field.size().x * cpw);
                    es.standByPos.y = field.size().y * (0.7f + rand.nextFloat(0.1f));
                    es.nextPhaseCnt = 15;
                    es.baseSpeed = es.baseBaseSpeed * 1.5f;
                    es.angVel = es.baseAngVel * 1.5f;
                    es.phase = -50;
                    break;
                case -200:
                    es.centerPos.x = rand.nextSignedFloat(field.size().x * 0.1f);
                    es.centerPos.y = field.size().y * 1.6f;
                    if (es.centerPos.x < 0)
                        es.standByPos.x = field.size().x * (rand.nextSignedFloat(0.4f) + 0.4f);
                    else
                        es.standByPos.x = field.size().x * (rand.nextSignedFloat(0.4f) - 0.4f);
                    es.standByPos.y = field.size().y * (0.5f + rand.nextFloat(0.3f));
                    es.nextPhaseCnt = 60;
                    es.baseSpeed = es.baseBaseSpeed * 1.0f;
                    es.angVel = es.baseAngVel * 1.0f;
                    break;
                case -199:
                    if (es.standByPos.x < 0)
                        es.centerPos.x = field.size().x * 0.75f;
                    else
                        es.centerPos.x = -field.size().x * 0.75f;
                    es.centerPos.y = 0;
                    if (es.isGoingDownBeforeStandBy)
                        es.nextPhaseCnt = 20;
                    else
                        es.nextPhaseCnt = 60;
                    es.baseSpeed = es.baseBaseSpeed * 2;
                    es.angVel = es.baseAngVel * 2;
                    es.phase = -50;
                    break;
                case -100:
                    es.centerPos.x = field.size().x * 4.0f;
                    if (rand.nextInt(2) == 0)
                        es.centerPos.x = es.centerPos.x * (-1);
                    es.centerPos.y = field.size().y * 1.6f;
                    if (es.centerPos.x < 0)
                        es.standByPos.x = field.size().x * (rand.nextSignedFloat(0.4f) + 0.4f);
                    else
                        es.standByPos.x = field.size().x * (rand.nextSignedFloat(0.4f) - 0.4f);
                    es.standByPos.y = field.size().y * (0.5f + rand.nextFloat(0.3f));
                    es.nextPhaseCnt = 20;
                    es.baseSpeed = es.baseBaseSpeed * 2.0f;
                    es.angVel = es.baseAngVel * 2.0f;
                    break;
                case -99:
                    if (es.centerPos.x > 0)
                        es.centerPos.x = field.size().x * 2.0f;
                    else
                        es.centerPos.x = -field.size().x * 2.0f;
                    es.centerPos.y = -field.size().y * 1.2f;
                    es.nextPhaseCnt = 20;
                    es.baseSpeed = es.baseBaseSpeed * 2;
                    es.angVel = es.baseAngVel * 2;
                    break;
                case -98:
                    if (es.centerPos.x > 0)
                        es.centerPos.x = field.size().x * 0.5f;
                    else
                        es.centerPos.x = -field.size().x * 0.5f;
                    es.centerPos.y = 0;
                    es.nextPhaseCnt = 30;
                    es.phase = -50;
                    break;
                case -49:
                    if (es.isGoingDownBeforeStandBy)
                    {
                        es.centerPos.x = es.centerPos.x / 2;
                        es.centerPos.y = 0;
                        es.phase = -30;
                        es.nextPhaseCnt = 10;
                        break;
                    }

                    es.centerPos.x = es.standByPos.x;
                    es.centerPos.y = es.standByPos.y;
                    es.nextPhaseCnt = calcStandByTime(es);
                    es.baseSpeed = es.baseBaseSpeed;
                    es.angVel = es.baseAngVel;
                    es.phase = -10;
                    break;
                case -29:
                    es.centerPos.x = (es.centerPos.x + player.pos().x * 2) / 3;
                    es.centerPos.y = -field.size().y * 1.2f;
                    es.baseSpeed = es.baseBaseSpeed * 1.2f;
                    es.angVel = es.baseAngVel * 1.2f;
                    es.nextPhaseCnt = 5;
                    break;
                case -28:
                    es.centerPos.y = -field.size().y * 1.5f;
                    es.nextPhaseCnt = 10;
                    break;
                case -9:
                    es.phase = 0;
                    break;
                default:
                    return false;
            }

            es.nextPhaseCnt = GameMath.integer(es.nextPhaseCnt / (rank));
            es.phaseCnt = 0;
        }

        return true;
    }

    public virtual void movePhase(EnemyState es)
    {
        {
            switch (es.phase)
            {
                case -200:
                case -100:
                    if (es.pos.y < field.size().y * 1.5f)
                        es.pos.y = field.size().y * 1.5f;
                    break;
                case -99:
                    if (((es.centerPos.x < 0)) && ((es.pos.x > -field.size().x)))
                        es.pos.x = es.pos.x + ((-field.size().x - es.pos.x) * 0.2f);
                    else if (((es.centerPos.x > 0)) && ((es.pos.x < field.size().x)))
                        es.pos.x = es.pos.x + ((field.size().x - es.pos.x) * 0.2f);
                    break;
                case -50:
                case -49:
                case -10:
                    if (es.pos.y < -field.size().y * 0.5f)
                        es.pos.y = es.pos.y + ((-field.size().y * 0.5f - es.pos.y) * 0.2f);
                    break;
                default:
                    break;
            }

            if (isInAttack_1(es))
                if ((((((((gameState.mode_0() == GameStateMode.MODERN))) || ((es.phase >= 0))))) || ((rand.nextInt(5) == 0))))
                    for (int i = 0; i < turretNum; i++)
                        turretSpecs[i].move_3(es.turretStates[i], rank, es.anger);
        }
    }

    public virtual bool isInScreen_1(EnemyState es)
    {
        return (field.size().contains_2(es.pos));
    }

    public abstract void setRank(float rank);
    public abstract void init_1(EnemyState es);
    public abstract bool gotoNextPhase(EnemyState es);
    public abstract bool isInAttack_1(EnemyState es);
    public abstract int calcStandByTime(EnemyState es);
    public override void draw_1(EnemyState es)
    {
        Vector3 p = field.calcCircularPos_1(es.pos);
        float cd = field.calcCircularDeg(es.pos.x);
        ((shape is EnemyShape ? (EnemyShape)shape : null)).draw_5(p, cd, es.deg, es.cnt, es.size);
        for (int i = 1; i < turretNum; i++)
        {
            float x = es.pos.x;
            switch (i)
            {
                case 1:
                    x = x - (turretWidth);
                    break;
                case 2:
                    x = x + (turretWidth);
                    break;
            }

            p = field.calcCircularPos_2(x, es.pos.y);
            cd = field.calcCircularDeg(x);
            TtnScreen.setColor(0.5f, 0.5f, 1);
            ((EnemyShape)trailShape).draw_6(p, cd, es.deg, es.cnt, es.size.x * 0.5f, es.size.y * 0.5f);
        }
    }

    public virtual void drawTrails_1(EnemyState es)
    {
        if (es.captureState > 0)
            return;
        es.drawTrails_6(trailShape, 0.2f, 0.2f, 0.8f, es.size, field);
    }
}

public class Trail
{
    public Vector pos;
    public float deg;
    public int cnt;
    public Trail()
    {
        pos = new Vector();
        deg = 0;
    }

    public virtual void set_4(float x, float y, float d, int c)
    {
        pos.x = x;
        pos.y = y;
        deg = d;
        cnt = c;
    }
}

public class GhostEnemySpec : EnemySpec
{
    public GhostEnemySpec(Field field, Shape shape)
    {
        this.field = field;
        this.shape = shape;
    }

    public override void draw_1(EnemyState es)
    {
        {
            Vector3 p = field.calcCircularPos_1(es.pos);
            float cd = field.calcCircularDeg(es.pos.x);
            TtnScreen.setColor(0.5f, 0.5f, 1, 0.8f);
            ((shape is EnemyShape ? (EnemyShape)shape : null)).draw_5(p, cd, es.deg, es.cnt, es.size);
        }
    }

    public override void set_1(EnemyState es)
    {
    }

    public override bool move_1(EnemyState es)
    {
        return true;
    }

    public override void destroyed_2(EnemyState es, float dd = 0)
    {
    }

    public override void setRank(float rank)
    {
    }

    public override void init_1(EnemyState es)
    {
    }

    public override bool gotoNextPhase(EnemyState es)
    {
        return false;
    }

    public override bool isInAttack_1(EnemyState es)
    {
        return false;
    }

    public override int calcStandByTime(EnemyState es)
    {
        return 0;
    }

    public override bool isBeingCaptured_1(EnemyState es)
    {
        return true;
    }

    public override bool isCaptured_1(EnemyState es)
    {
        return true;
    }
}

public class MiddleEnemySpec : EnemySpec
{
    public MiddleEnemySpec(Field field, BulletPool bullets, Player player, ParticlePool particles, ParticlePool bonusParticles, EnemyPool enemies, Stage stage, Shape shape, EnemyShape trailShape, BulletSpec bulletSpec, BulletSpec counterBulletSpec, GameState gameState) : base(field, bullets, player, particles, bonusParticles, enemies, stage, shape, trailShape, bulletSpec, counterBulletSpec, gameState)
    {
        for (int i = 0; i < TURRET_MAX_NUM; i++)
            turretSpecs[i] = new TurretSpec(field, bullets, player, enemies, particles, stage, bulletSpec, gameState);
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                shield = 2;
                capturable = false;
                removeBullets = false;
                break;
            case GameStateMode.BASIC:
                shield = 3;
                capturable = false;
                removeBullets = false;
                break;
            case GameStateMode.MODERN:
                shield = 32;
                capturable = true;
                removeBullets = true;
                break;
        }

        score = 100;
        explosionSeName = "explosion3.wav";
    }

    public override void init_1(EnemyState es)
    {
        {
            {
                es.size.y = 1.33f;
                es.size.x = es.size.y;
            }

            es.phase = -300;
            gotoNextPhaseInAppearing(es);
        }
    }

    public override void setRank(float r)
    {
        rank = sqrt(r);
        float tr = 0;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                rank = sqrt(rank);
                tr = r * 2;
                break;
            case GameStateMode.BASIC:
                tr = r * 3;
                break;
            case GameStateMode.MODERN:
                rank = 1;
                tr = r * 15;
                break;
        }

        if (rank < 1.5f)
            rank = 1.5f;
        turretSpecs[0].setRankMiddle(tr);
        turretNum = 1;
        if ((gameState.mode_0() == GameStateMode.MODERN))
        {
            TurretSpec ts = turretSpecs[0];
            int ptn = rand.nextInt(6);
            switch (ptn)
            {
                case 0:
                    break;
                case 1:
                case 2:
                case 4:
                    turretSpecs[1].copy(turretSpecs[0]);
                    turretSpecs[2].copy(turretSpecs[0]);
                    if (((ts.nway > 1)) && ((rand.nextInt(2) == 0)))
                    {
                        float nsa = (ts.speed * (0.2f + ts.nway * 0.05f + rand.nextFloat(0.1f))) / (ts.nway - 1);
                        if (rand.nextInt(2) == 0)
                            nsa = nsa * (-1);
                        turretSpecs[1].nwaySpeedAccel = nsa;
                        turretSpecs[2].nwaySpeedAccel = -nsa;
                    }

                    turretWidth = 1.0f + rand.nextFloat(1.0f);
                    turretNum = 3;
                    if (ptn == 4)
                    {
                        turretSpecs[0].setRankMiddle(tr * 2);
                        turretSpecs[1].interval = turretSpecs[1].interval * (4);
                        turretSpecs[2].interval = turretSpecs[2].interval * (4);
                        turretSpecs[0].interval = turretSpecs[1].interval;
                        turretSpecs[2].fireIntervalRatio = 0.25f;
                        turretSpecs[0].fireIntervalRatio = 0.5f;
                    }
                    else
                    {
                        turretSpecs[0].disabled(true);
                        turretSpecs[1].interval = turretSpecs[1].interval * (2);
                        turretSpecs[2].interval = turretSpecs[2].interval * (2);
                        if (rand.nextInt(2) == 0)
                            turretSpecs[2].fireIntervalRatio = 0.5f;
                    }

                    break;
                case 3:
                case 5:
                    turretSpecs[0].interval = turretSpecs[0].interval * (2);
                    if (rand.nextInt(3) == 0)
                        turretSpecs[0].nwayAngle = turretSpecs[0].nwayAngle * (0.1f);
                    turretSpecs[1].setRankMiddle(tr);
                    turretSpecs[1].interval = turretSpecs[1].interval * (2);
                    turretSpecs[2].copy(turretSpecs[1]);
                    if (((ts.nway > 1)) && ((rand.nextInt(2) == 0)))
                    {
                        float nsa = (ts.speed * (0.2f + ts.nway * 0.05f + rand.nextFloat(0.1f))) / (ts.nway - 1);
                        if (rand.nextInt(2) == 0)
                            nsa = nsa * (-1);
                        turretSpecs[1].nwaySpeedAccel = nsa;
                        turretSpecs[2].nwaySpeedAccel = -nsa;
                    }

                    turretSpecs[1].nwayBaseDeg = -PI / 8 - rand.nextFloat(PI / 12);
                    if (turretSpecs[1].nway > 1)
                        turretSpecs[1].nwayBaseDeg = turretSpecs[1].nwayBaseDeg - (turretSpecs[1].nwayAngle * (turretSpecs[1].nway - 1) / 2);
                    turretSpecs[2].nwayBaseDeg = -turretSpecs[1].nwayBaseDeg;
                    turretWidth = 1.5f + rand.nextFloat(0.5f);
                    turretNum = 3;
                    break;
            }
        }
    }

    public override bool gotoNextPhase(EnemyState es)
    {
        {
            if (es.phase < 0)
                return gotoNextPhaseInAppearing(es);
            switch (es.phase)
            {
                case 1:
                    if (((((gameState.mode_0() != GameStateMode.MODERN))) && (((!((player.hasCollision())))))))
                    {
                        es.phase = 0;
                        es.nextPhaseCnt = calcStandByTime(es);
                        break;
                    }

                    Sound.playSe("flying_down.wav");
                    if ((gameState.mode_0() != GameStateMode.MODERN))
                    {
                        es.centerPos.x = field.size().x * (0.6f + rand.nextSignedFloat(0.1f));
                        if (rand.nextInt(2) == 0)
                            es.centerPos.x = es.centerPos.x * (-1);
                        es.centerPos.y = field.size().y * (0.2f + rand.nextFloat(0.2f));
                        es.nextPhaseCnt = 60;
                    }
                    else
                    {
                        es.centerPos.x = es.standByPos.x;
                        es.centerPos.y = field.size().y * 0.95f;
                        es.baseSpeed = es.baseBaseSpeed * 0.3f;
                        es.nextPhaseCnt = 60;
                    }

                    break;
                case 2:
                    if ((gameState.mode_0() != GameStateMode.MODERN))
                    {
                        es.centerPos.x = es.centerPos.x * (-0.9f);
                        es.centerPos.y = field.size().y * (0.2f + rand.nextFloat(0.2f));
                        es.nextPhaseCnt = 60;
                    }
                    else
                    {
                        es.centerPos.x = es.standByPos.x;
                        es.centerPos.y = 0;
                        es.baseSpeed = es.baseBaseSpeed * 0.1f;
                        es.nextPhaseCnt = 10;
                    }

                    break;
                case 3:
                    if ((gameState.mode_0() != GameStateMode.MODERN))
                    {
                        es.centerPos.x = es.standByPos.x;
                        es.centerPos.y = es.standByPos.y;
                        es.phase = 0;
                        es.nextPhaseCnt = calcStandByTime(es);
                    }
                    else
                    {
                        es.centerPos.x = es.standByPos.x;
                        es.centerPos.y = -field.size().y * 1.5f;
                        es.baseSpeed = es.baseBaseSpeed * 0.5f;
                        es.nextPhaseCnt = 10;
                    }

                    break;
                default:
                    return false;
            }

            es.nextPhaseCnt = GameMath.integer(es.nextPhaseCnt / (rank));
            es.phaseCnt = 0;
        }

        return true;
    }

    public override bool isInAttack_1(EnemyState es)
    {
        return (((es.phase == 1)) || ((es.phase == 2)));
    }

    public override int calcStandByTime(EnemyState es)
    {
        if ((((es.phase < 0)) || (((gameState.mode_0() == GameStateMode.MODERN)))))
            return 30 + rand.nextInt(30);
        else
            return 200 + rand.nextInt(150);
    }
}

public abstract class SmallEnemySpec : EnemySpec
{
    public SmallEnemySpec(Field field, BulletPool bullets, Player player, ParticlePool particles, ParticlePool bonusParticles, EnemyPool enemies, Stage stage, Shape shape, EnemyShape trailShape, BulletSpec bulletSpec, BulletSpec counterBulletSpec, GameState gameState) : base(field, bullets, player, particles, bonusParticles, enemies, stage, shape, trailShape, bulletSpec, counterBulletSpec, gameState)
    {
        turretSpecs[0] = new TurretSpec(field, bullets, player, enemies, particles, stage, bulletSpec, gameState);
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
            case GameStateMode.BASIC:
                shield = 1;
                break;
            case GameStateMode.MODERN:
                shield = 2;
                break;
        }

        capturable = true;
        score = 10;
        removeBullets = false;
    }

    public override void init_1(EnemyState es)
    {
        gotoNextPhaseInAppearing(es);
    }

    public virtual void init_2(EnemyState es, EnemyState fes)
    {
        {
            es.centerPos.x = fes.centerPos.x;
            es.centerPos.y = fes.centerPos.y;
            es.standByPos.x = fes.standByPos.x;
            es.standByPos.y = fes.standByPos.y;
            es.nextPhaseCnt = fes.nextPhaseCnt;
            es.baseSpeed = fes.baseSpeed;
            es.angVel = fes.angVel;
            es.phase = fes.phase;
            {
                es.size.y = 1.25f;
                es.size.x = es.size.y;
            }
        }
    }

    public override void setRank(float r)
    {
        rank = sqrt(r * 0.5f);
        float tr = 0;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                rank = sqrt(rank);
                tr = r;
                break;
            case GameStateMode.BASIC:
                tr = r * 2;
                break;
            case GameStateMode.MODERN:
                rank = 1;
                tr = r;
                break;
        }

        if (rank < 1)
            rank = 1;
        turretSpecs[0].setRankNormal(tr);
        turretNum = 1;
    }

    public override int calcStandByTime(EnemyState es)
    {
        return 60 + rand.nextInt(120);
    }
}

public class SE1Spec : SmallEnemySpec
{
    public SE1Spec(Field field, BulletPool bullets, Player player, ParticlePool particles, ParticlePool bonusParticles, EnemyPool enemies, Stage stage, Shape shape, EnemyShape trailShape, BulletSpec bulletSpec, BulletSpec counterBulletSpec, GameState gameState) : base(field, bullets, player, particles, bonusParticles, enemies, stage, shape, trailShape, bulletSpec, counterBulletSpec, gameState)
    {
        explosionSeName = "explosion1.wav";
    }

    public override bool gotoNextPhase(EnemyState es)
    {
        {
            if (es.phase < 0)
                return gotoNextPhaseInAppearing(es);
            switch (es.phase)
            {
                case 1:
                    if (((((!((player.hasCollision()))))) || (((enemies.numInAttack() > stage.attackSmallEnemyNum())))))
                    {
                        es.phase = 0;
                        es.nextPhaseCnt = calcStandByTime(es);
                        break;
                    }

                    Sound.playSe("flying_down.wav");
                    es.centerPos.y = 0;
                    es.centerPos.x = (es.standByPos.x + player.pos().x) / 2;
                    es.nextPhaseCnt = 60;
                    break;
                case 2:
                    es.centerPos.y = -field.size().y * 0.7f;
                    es.centerPos.x = player.pos().x;
                    es.nextPhaseCnt = 30;
                    break;
                case 3:
                    es.centerPos.x = es.standByPos.x;
                    es.centerPos.y = es.standByPos.y;
                    es.phase = 0;
                    es.nextPhaseCnt = calcStandByTime(es);
                    break;
            }

            es.nextPhaseCnt = GameMath.integer(es.nextPhaseCnt / (rank));
            es.phaseCnt = 0;
        }

        return true;
    }

    public override bool isInAttack_1(EnemyState es)
    {
        return (((((es.phase < -10)) || ((es.phase == 1)))) || ((es.phase == 2)));
    }
}

public class SE2Spec : SmallEnemySpec
{
    public SE2Spec(Field field, BulletPool bullets, Player player, ParticlePool particles, ParticlePool bonusParticles, EnemyPool enemies, Stage stage, Shape shape, EnemyShape trailShape, BulletSpec bulletSpec, BulletSpec counterBulletSpec, GameState gameState) : base(field, bullets, player, particles, bonusParticles, enemies, stage, shape, trailShape, bulletSpec, counterBulletSpec, gameState)
    {
        explosionSeName = "explosion2.wav";
    }

    public override bool gotoNextPhase(EnemyState es)
    {
        {
            if (es.phase < 0)
                return gotoNextPhaseInAppearing(es);
            switch (es.phase)
            {
                case 1:
                    if (((((!((player.hasCollision()))))) || (((enemies.numInAttack() > stage.attackSmallEnemyNum())))))
                    {
                        es.phase = 0;
                        es.nextPhaseCnt = calcStandByTime(es);
                        break;
                    }

                    Sound.playSe("flying_down.wav");
                    es.centerPos.y = -field.size().y * 0.3f;
                    es.centerPos.x = (es.standByPos.x + player.pos().x) / 2;
                    es.baseSpeed = es.baseBaseSpeed;
                    es.angVel = es.baseAngVel;
                    es.nextPhaseCnt = 30 + rand.nextInt(60);
                    break;
                case 2:
                    es.centerPos.y = -field.size().y * 1.3f;
                    es.centerPos.x = es.centerPos.x * (-1);
                    es.nextPhaseCnt = 30;
                    break;
                case 3:
                    es.centerPos.y = -field.size().y * 1.0f;
                    if (es.centerPos.x < 0)
                        es.centerPos.x = -field.size().x * 1.5f;
                    else
                        es.centerPos.x = field.size().x * 1.5f;
                    es.baseSpeed = es.baseBaseSpeed * 1.5f;
                    es.angVel = es.baseAngVel * 1.5f;
                    es.nextPhaseCnt = 30;
                    break;
                case 4:
                    es.centerPos.x = es.standByPos.x;
                    es.centerPos.y = es.standByPos.y;
                    es.phase = 0;
                    es.nextPhaseCnt = calcStandByTime(es);
                    break;
            }

            es.nextPhaseCnt = GameMath.integer(es.nextPhaseCnt / (rank));
            es.phaseCnt = 0;
        }

        return true;
    }

    public override void movePhase(EnemyState es)
    {
        base.movePhase(es);
        {
            if (es.phase == 3)
            {
                if (es.centerPos.x < 0)
                {
                    if (es.pos.x > -field.size().x * 1.2f)
                        es.pos.x = es.pos.x + ((es.centerPos.x - es.pos.x) * 0.2f);
                }
                else
                {
                    if (es.pos.x < field.size().x * 1.2f)
                        es.pos.x = es.pos.x + ((es.centerPos.x - es.pos.x) * 0.2f);
                }
            }
        }
    }

    public override bool isInAttack_1(EnemyState es)
    {
        return (((((((es.phase < -10)) || ((es.phase == 1)))) || ((es.phase == 2)))) || ((es.phase == 3)));
    }
}

public class TurretState : TokenState
{
    public float fireCnt, burstCnt;
    public int burstNum;
    public int nwaySpeedAccelDir;
    public override void clear()
    {
        {
            burstCnt = 0;
            fireCnt = burstCnt;
        }

        burstNum = 0;
        nwaySpeedAccelDir = 1;
        base.clear();
    }

    public virtual void update(float x, float y, float d)
    {
        pos.x = x;
        pos.y = y;
        if (burstNum <= 0)
            deg = d;
    }
}

public class TurretSpec : TokenSpec<TurretState>
{
    public static TitanionRand rand = new TitanionRand();
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public const float SPEED_RATIO = 5.0f;
    public const float INTERVAL_MAX = 90.0f;
    public BulletSpec bulletSpec;
    public BulletPool bullets;
    public Player player;
    public Stage stage;
    public GameState gameState;
    public int interval;
    public float speed;
    public float speedAccel;
    public int burstNum, burstInterval;
    public int nway;
    public float nwayAngle;
    public float nwayBaseDeg;
    public float nwaySpeedAccel;
    public bool fireingAtATime;
    public float fireIntervalRatio;
    public bool _disabled;
    public float minimumFireDist;
    public TurretSpec(Field field, BulletPool bullets, Player player, EnemyPool enemies, ParticlePool particles, Stage stage, BulletSpec bulletSpec, GameState gameState)
    {
        this.bulletSpec = bulletSpec;
        this.field = field;
        this.bullets = bullets;
        this.player = player;
        this.stage = stage;
        this.gameState = gameState;
        initParam();
    }

    public virtual void initParam()
    {
        interval = 99999;
        speed = 1;
        speedAccel = 0;
        burstNum = 1;
        burstInterval = 99999;
        nway = 1;
        nwayAngle = 0;
        nwayBaseDeg = 0;
        nwaySpeedAccel = 0;
        fireingAtATime = false;
        fireIntervalRatio = 0;
        _disabled = false;
        minimumFireDist = 0;
    }

    public virtual void copy(TurretSpec ts)
    {
        interval = ts.interval;
        speed = ts.speed;
        speedAccel = ts.speedAccel;
        burstNum = ts.burstNum;
        burstInterval = ts.burstInterval;
        nway = ts.nway;
        nwayAngle = ts.nwayAngle;
        nwayBaseDeg = ts.nwayBaseDeg;
        nwaySpeedAccel = ts.nwaySpeedAccel;
        fireingAtATime = ts.fireingAtATime;
    }

    public override void set_1(TurretState ts)
    {
        setFireIntervalRatio(ts, fireIntervalRatio);
    }

    public virtual void setFireIntervalRatio(TurretState ts, float fir)
    {
        ts.fireCnt = fir * interval;
    }

    public virtual void setRankNormal(float rank, bool isWide = false)
    {
        initParam();
        float rr = rand.nextFloat(0.5f);
        float nsr = 0.5f + rand.nextSignedFloat(0.3f);
        float nr = 0, br = 0, ir = 0;
        float intervalMax = INTERVAL_MAX;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
            {
                br = 0;
                nr = br;
            }

                ir = sqrt(rank * nsr) * 2;
                burstInterval = 3 + rand.nextInt(2);
                break;
            case GameStateMode.BASIC:
                if (isWide)
                {
                    nr = (rank * nsr * rr);
                    br = 0;
                    ir = (rank * nsr * (1 - rr));
                }
                else
                {
                    nr = 0;
                    br = (rank * nsr * rr);
                    ir = (rank * nsr * (1 - rr));
                }

                burstInterval = 3 + rand.nextInt(2);
                break;
            case GameStateMode.MODERN:
                if (isWide)
                {
                    nr = (rank * nsr * rr);
                    br = 0;
                    ir = (rank * nsr * (1 - rr));
                }
                else
                {
                    nr = 0;
                    br = (rank * nsr * rr);
                    ir = (rank * nsr * (1 - rr));
                }

                intervalMax = 120;
                burstInterval = 4 + rand.nextInt(4);
                break;
        }

        burstNum = GameMath.integer(sqrt(br)) + 1;
        nway = GameMath.integer(sqrt(nr)) + 1;
        interval = GameMath.integer((intervalMax / (ir + 1))) + 1;
        float sr = rank - nway + 1 - burstNum + 1 - ir;
        if (sr < 0.01f)
            sr = 0.01f;
        speed = sqrt(sr * 0.66f);
        speed = speed * (0.2f);
        if (speed < 0.1f)
            speed = 0.1f;
        else
            speed = sqrt(speed * 10) / 10;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                speed = speed * (0.36f);
                if (speed < 0.05f)
                    speed = 0.05f;
                else
                    speed = sqrt(speed * 20) / 20;
                break;
            case GameStateMode.BASIC:
                speed = speed * (0.33f);
                break;
            case GameStateMode.MODERN:
                speed = speed * (0.25f);
                if (speed < 0.04f)
                    speed = 0.04f;
                if (speed > 0.04f)
                    speed = sqrt(speed * 25) / 25;
                break;
        }

        nwayAngle = (1.66f + rand.nextFloat(0.33f)) / (1 + nway * 0.7f) * 0.06f;
        fireingAtATime = true;
        minimumFireDist = 10;
    }

    public virtual void setRankMiddle(float rank)
    {
        initParam();
        float nr = 0, br = 0, ir = 0;
        float nwayDegRatio = 0;
        float intervalMax = INTERVAL_MAX;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
            {
                br = 0;
                nr = br;
            }

                ir = sqrt(rank * (0.5f + rand.nextSignedFloat(0.3f))) * 2;
                nwayDegRatio = 0;
                burstInterval = 3 + rand.nextInt(2);
                break;
            case GameStateMode.BASIC:
                if (rand.nextInt(3) == 0)
                {
                    nr = 0;
                    br = (rank * 0.4f) * (1.0f + rand.nextSignedFloat(0.2f));
                    ir = (rank * 0.5f) * (1.0f + rand.nextSignedFloat(0.2f));
                }
                else
                {
                    rank = rank * (0.5f);
                    nr = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                    br = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                    ir = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                }

                ir = ir * (1.5f);
                nwayDegRatio = 0.06f;
                burstInterval = 3 + rand.nextInt(2);
                break;
            case GameStateMode.MODERN:
                switch (rand.nextInt(5))
                {
                    case 0:
                        rank = rank * (1.2f);
                        nr = 0;
                        br = (rank * 0.7f) * (1.0f + rand.nextSignedFloat(0.2f));
                        ir = (rank * 0.2f) * (1.0f + rand.nextSignedFloat(0.2f));
                        break;
                    case 1:
                    case 2:
                        nr = (rank * 0.7f) * (1.0f + rand.nextSignedFloat(0.2f));
                        br = 0;
                        ir = (rank * 0.2f) * (1.0f + rand.nextSignedFloat(0.2f));
                        break;
                    case 3:
                    case 4:
                        rank = rank * (0.75f);
                        nr = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                        br = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                        ir = (rank * 0.3f) * (1.0f + rand.nextSignedFloat(0.2f));
                        break;
                }

                nwayDegRatio = 1;
                intervalMax = 120;
                burstInterval = 4 + rand.nextInt(8);
                break;
        }

        bool acf = false;
        burstNum = GameMath.integer(sqrt(br)) + 1;
        if (((burstNum > 1)) && ((rand.nextInt(3) > 0)))
        {
            acf = true;
            nr = nr * (0.9f);
            ir = ir * (0.9f);
            rank = rank * (0.9f);
        }

        nway = GameMath.integer(sqrt(nr)) + 1;
        interval = GameMath.integer((intervalMax / (sqrt(ir + 1)))) + 1;
        float sr = rank - burstNum + 1 - nway + 1 - ir;
        if (sr < 0.01f)
            sr = 0.01f;
        speed = sqrt(sr * 0.66f);
        speed = speed * (0.2f);
        if (speed < 0.1f)
            speed = 0.1f;
        else
            speed = sqrt(speed * 10) / 10;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                speed = speed * (0.36f);
                if (speed < 0.05f)
                    speed = 0.05f;
                else
                    speed = sqrt(speed * 20) / 20;
                break;
            case GameStateMode.BASIC:
                speed = speed * (0.4f);
                break;
            case GameStateMode.MODERN:
                speed = speed * (0.22f);
                if (speed < 0.04f)
                    speed = 0.04f;
                if (speed > 0.04f)
                    speed = sqrt(speed * 25) / 25;
                break;
        }

        if (acf)
        {
            speedAccel = (speed * (0.2f + burstNum * 0.05f + rand.nextFloat(0.1f))) / (burstNum - 1);
            if (rand.nextInt(2) == 0)
                speedAccel = speedAccel * (-1);
        }

        if ((((((((gameState.mode_0() == GameStateMode.BASIC))) && ((nway > 1))))) && ((rand.nextInt(3) == 0))))
        {
            speed = speed * (0.9f);
            nwaySpeedAccel = (speed * (0.2f + nway * 0.05f + rand.nextFloat(0.1f))) / (nway - 1);
            if (rand.nextInt(2) == 0)
                nwaySpeedAccel = nwaySpeedAccel * (-1);
        }

        if (nway > 1)
            nwayAngle = (1.66f + rand.nextFloat(0.33f)) / (1 + nway * 0.7f) * nwayDegRatio;
        if (rand.nextInt(3) == 0)
            fireingAtATime = true;
        minimumFireDist = 5;
    }

    public virtual bool move_3(TurretState ts, float time = 1, float anger = 0)
    {
        if (_disabled)
            return true;
        float itv = interval * ((1 - anger) * 0.99f + 0.01f);
        if (itv < 3)
            itv = 3;
        if (ts.fireCnt > itv)
            ts.fireCnt = itv;
        float spd = speed * (1 + anger * 0.2f);
        if (fireingAtATime)
        {
            ts.fireCnt = ts.fireCnt - (time);
            if (ts.fireCnt <= 0)
            {
                ts.fireCnt = itv;
                if (ts.fireCnt < 3)
                    ts.fireCnt = 3;
                if (isAbleToFire(ts.pos))
                {
                    float sp = spd - speedAccel * (burstNum - 1) / 2;
                    for (int i = 0; i < burstNum; i++)
                    {
                        float d = ts.deg - nwayAngle * (nway - 1) / 2 + nwayBaseDeg;
                        float nsp = sp - nwaySpeedAccel * ts.nwaySpeedAccelDir * (nway - 1) / 2;
                        for (int j = 0; j < nway; j++)
                        {
                            Bullet b = bullets.getInstance();
                            if ((!(((b) != null))))
                                break;
                            b.setAt(bulletSpec, ts.pos, d, nsp * SPEED_RATIO);
                            b.setWaitCnt(i * burstInterval);
                            d = d + (nwayAngle);
                            nsp = nsp + (nwaySpeedAccel * ts.nwaySpeedAccelDir);
                        }

                        sp = sp + (speedAccel);
                    }

                    ts.nwaySpeedAccelDir = ts.nwaySpeedAccelDir * (-1);
                }
            }
        }
        else
        {
            if (ts.burstNum <= 0)
            {
                ts.fireCnt = ts.fireCnt - (time);
                if (ts.fireCnt <= 0)
                {
                    ts.fireCnt = itv;
                    if (ts.fireCnt < 3)
                        ts.fireCnt = 3;
                    ts.burstNum = burstNum;
                    ts.burstCnt = 0;
                    ts.speed = spd - speedAccel * (ts.burstNum - 1) / 2;
                }
            }

            if (ts.burstNum > 0)
            {
                ts.burstCnt = ts.burstCnt - (time);
                if (ts.burstCnt <= 0)
                {
                    ts.burstCnt = burstInterval;
                    ts.burstNum--;
                    if (isAbleToFire(ts.pos))
                    {
                        float d = ts.deg - nwayAngle * (nway - 1) / 2 + nwayBaseDeg;
                        float nsp = ts.speed - nwaySpeedAccel * ts.nwaySpeedAccelDir * (nway - 1) / 2;
                        for (int i = 0; i < nway; i++)
                        {
                            Bullet b = bullets.getInstance();
                            if ((!(((b) != null))))
                                break;
                            b.setAt(bulletSpec, ts.pos, d, nsp * SPEED_RATIO);
                            d = d + (nwayAngle);
                            nsp = nsp + (nwaySpeedAccel * ts.nwaySpeedAccelDir);
                        }
                    }

                    ts.speed = ts.speed + (speedAccel);
                }
            }
        }

        return true;
    }

    public virtual bool isAbleToFire(Vector p)
    {
        if ((gameState.mode_0() != GameStateMode.MODERN))
            return (p.y > 0);
        else
            return (((p.y > 0)) && ((p.dist_1(player.pos()) > minimumFireDist)));
    }

    public virtual bool disabled(bool v)
    {
        _disabled = v;
        return _disabled;
    }
}
