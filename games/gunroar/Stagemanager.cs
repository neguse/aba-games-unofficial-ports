// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class StageManager
{
    public const float RANK_INC_BASE = 0.0018f;
    public const int BLOCK_DENSITY_MIN = 0;
    public const int BLOCK_DENSITY_MAX = 3;
    public Field field;
    public EnemyPool enemies;
    public Ship ship;
    public BulletPool bullets;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public WakePool wakes;
    public GunroarRand rand;
    public float rank, baseRank, addRank, rankVel, rankInc;
    public EnemyAppearance[] enemyApp;
    public int _blockDensity;
    public int batteryNum;
    public PlatformEnemySpec platformEnemySpec;
    public bool _bossMode;
    public int bossAppCnt;
    public int bossAppTime, bossAppTimeBase;
    public int bgmStartCnt;
    public StageManager(Field field, EnemyPool enemies, Ship ship, BulletPool bullets, SparkPool sparks, SmokePool smokes, FragmentPool fragments, WakePool wakes)
    {
        this.field = field;
        this.enemies = enemies;
        this.ship = ship;
        this.bullets = bullets;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.wakes = wakes;
        rand = new GunroarRand();
        enemyApp = new EnemyAppearance[3];
        for (int index0 = 0; index0 < 3; index0++)
        {
            enemyApp[index0] = new EnemyAppearance();
        }

        PlatformEnemySpec platformEnemySpec = new PlatformEnemySpec(field, ship, sparks, smokes, fragments, wakes);
        {
            baseRank = 1;
            rank = baseRank;
        }

        {
            rankInc = 0;
            rankVel = rankInc;
            addRank = rankVel;
        }

        _blockDensity = 2;
    }

    public void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public void start(float rankIncRatio)
    {
        {
            baseRank = 1;
            rank = baseRank;
        }

        {
            rankVel = 0;
            addRank = rankVel;
        }

        rankInc = RANK_INC_BASE * rankIncRatio;
        _blockDensity = rand.nextInt(BLOCK_DENSITY_MAX - BLOCK_DENSITY_MIN + 1) + BLOCK_DENSITY_MIN;
        _bossMode = false;
        bossAppTimeBase = 60 * 1000;
        resetBossMode();
        gotoNextBlockArea();
        bgmStartCnt = -1;
    }

    public void startBossMode()
    {
        _bossMode = true;
        bossAppCnt = 2;
        SoundManager.fadeBgm();
        bgmStartCnt = 120;
        rankVel = 0;
    }

    public void resetBossMode()
    {
        if (_bossMode)
        {
            _bossMode = false;
            SoundManager.fadeBgm();
            bgmStartCnt = 120;
            bossAppTimeBase = bossAppTimeBase + (30 * 1000);
        }

        bossAppTime = bossAppTimeBase;
    }

    public void move()
    {
        bgmStartCnt--;
        if (bgmStartCnt == 0)
        {
            if (_bossMode)
                SoundManager.playNamedBgm("gr0.ogg");
            else
                SoundManager.nextBgm();
        }

        if (_bossMode)
        {
            addRank = addRank * (0.999f);
            if ((!enemies.hasBoss()) && (bossAppCnt <= 0))
                resetBossMode();
        }
        else
        {
            float rv = field.lastScrollY / ship.scrollSpeedBase - 2;
            bossAppTime = bossAppTime - (17);
            if (bossAppTime <= 0)
            {
                bossAppTime = 0;
                startBossMode();
            }

            if (rv > 0)
            {
                rankVel = rankVel + (rv * rv * 0.0004f * baseRank);
            }
            else
            {
                rankVel = rankVel + (rv * baseRank);
                if (rankVel < 0)
                    rankVel = 0;
            }

            addRank = addRank + (rankInc * (rankVel + 1));
            addRank = addRank * (0.999f);
            baseRank = baseRank + (rankInc + addRank * 0.0001f);
        }

        rank = baseRank + addRank;
        foreach (EnemyAppearance ea in enemyApp)
            ea.move(enemies, field);
    }

    public void shipDestroyed()
    {
        rankVel = 0;
        if (!_bossMode)
            addRank = 0;
        else
            addRank = addRank / (2);
    }

    public void gotoNextBlockArea()
    {
        if (_bossMode)
        {
            bossAppCnt--;
            if (bossAppCnt == 0)
            {
                ShipEnemySpec ses = new ShipEnemySpec(field, ship, sparks, smokes, fragments, wakes);
                ses.setParam(rank, ShipEnemySpecShipClass.BOSS, rand);
                Enemy en = enemies.getInstance();
                if (en != null)
                {
                    if (((HasAppearType)ses).setFirstState(en.state, EnemyStateAppearanceType.CENTER))
                        en.set(ses);
                }
                else
                {
                    resetBossMode();
                }
            }

            foreach (EnemyAppearance ea in enemyApp)
                ea.unset();
            return;
        }

        bool noSmallShip = false;
        if ((_blockDensity < BLOCK_DENSITY_MAX) && (rand.nextInt(2) == 0))
            noSmallShip = true;
        else
            noSmallShip = false;
        _blockDensity = _blockDensity + (rand.nextSignedInt(1));
        if (_blockDensity < BLOCK_DENSITY_MIN)
            _blockDensity = BLOCK_DENSITY_MIN;
        else if (_blockDensity > BLOCK_DENSITY_MAX)
            _blockDensity = BLOCK_DENSITY_MAX;
        batteryNum = GameMath.integer(((_blockDensity + rand.nextSignedFloat(1)) * 0.75f));
        float tr = rank;
        int largeShipNum = GameMath.integer(((2 - _blockDensity + rand.nextSignedFloat(1)) * 0.5f));
        if (noSmallShip)
            largeShipNum = GameMath.integer(largeShipNum * 1.5f);
        else
            largeShipNum = GameMath.integer(largeShipNum * 0.5f);
        int appType = rand.nextInt(2);
        if (largeShipNum > 0)
        {
            float lr = tr * (0.25f + rand.nextFloat(0.15f));
            if (noSmallShip)
                lr = lr * (1.5f);
            tr = tr - (lr);
            ShipEnemySpec ses = new ShipEnemySpec(field, ship, sparks, smokes, fragments, wakes);
            ses.setParam(lr / largeShipNum, ShipEnemySpecShipClass.LARGE, rand);
            enemyApp[0].set(ses, largeShipNum, appType, rand);
        }
        else
        {
            enemyApp[0].unset();
        }

        if (batteryNum > 0)
        {
            platformEnemySpec = new PlatformEnemySpec(field, ship, sparks, smokes, fragments, wakes);
            float pr = tr * (0.3f + rand.nextFloat(0.1f));
            platformEnemySpec.setParam(pr / batteryNum, rand);
        }

        appType = (appType + 1) % 2;
        int middleShipNum = GameMath.integer(((4 - _blockDensity + rand.nextSignedFloat(1)) * 0.66f));
        if (noSmallShip)
            middleShipNum = middleShipNum * (2);
        if (middleShipNum > 0)
        {
            float mr = 0;
            if (noSmallShip)
                mr = tr;
            else
                mr = tr * (0.33f + rand.nextFloat(0.33f));
            tr = tr - (mr);
            ShipEnemySpec ses = new ShipEnemySpec(field, ship, sparks, smokes, fragments, wakes);
            ses.setParam(mr / middleShipNum, ShipEnemySpecShipClass.MIDDLE, rand);
            enemyApp[1].set(ses, middleShipNum, appType, rand);
        }
        else
        {
            enemyApp[1].unset();
        }

        if (!noSmallShip)
        {
            appType = EnemyStateAppearanceType.TOP;
            int smallShipNum = GameMath.integer((sqrt(3 + tr) * (1 + rand.nextSignedFloat(0.5f)) * 2)) + 1;
            if (smallShipNum > 256)
                smallShipNum = 256;
            SmallShipEnemySpec sses = new SmallShipEnemySpec(field, ship, sparks, smokes, fragments, wakes);
            sses.setParam(tr / smallShipNum, rand);
            enemyApp[2].set(sses, smallShipNum, appType, rand);
        }
        else
        {
            enemyApp[2].unset();
        }
    }

    public void addBatteries(PlatformPos[] platformPos, int platformPosNum)
    {
        int ppn = platformPosNum;
        int bn = batteryNum;
        for (int i = 0; i < 100; i++)
        {
            if ((ppn <= 0) || (bn <= 0))
                break;
            int ppi = rand.nextInt(platformPosNum);
            for (int j = 0; j < platformPosNum; j++)
            {
                if (!platformPos[ppi].used)
                    break;
                ppi++;
                if (ppi >= platformPosNum)
                    ppi = 0;
            }

            if (platformPos[ppi].used)
                break;
            Enemy en = enemies.getInstance();
            if (!(en != null))
                break;
            platformPos[ppi].used = true;
            ppn--;
            Vector p = field.convertToScreenPos(GameMath.integer(platformPos[ppi].pos.x), GameMath.integer(platformPos[ppi].pos.y));
            if (!platformEnemySpec.setFirstState(en.state, p.x, p.y, platformPos[ppi].deg))
                continue;
            for (int nearIndex = 0; nearIndex < platformPosNum; nearIndex++)
            {
                if (((fabs(platformPos[ppi].pos.x - platformPos[nearIndex].pos.x) <= 1) && (fabs(platformPos[ppi].pos.y - platformPos[nearIndex].pos.y) <= 1)) && (!platformPos[nearIndex].used))
                {
                    platformPos[nearIndex].used = true;
                    ppn--;
                }
            }

            en.set(platformEnemySpec);
            bn--;
        }
    }

    public int blockDensity
    {
        get
        {
            return _blockDensity;
        }

        set
        {
            _blockDensity = value;
        }
    }

    public void draw()
    {
        Letter.drawNum(GameMath.integer((rank * 1000)), 620, 10, 10, 0, 0, 33, 3);
        Letter.drawTime(bossAppTime, 120, 20, 7);
    }

    public float rankMultiplier()
    {
        return rank;
    }

    public bool bossMode
    {
        get
        {
            return _bossMode;
        }

        set
        {
            _bossMode = value;
        }
    }
}

public class EnemyAppearance
{
    public EnemySpec spec;
    public float nextAppDist, nextAppDistInterval;
    public int appType;
    public EnemyAppearance()
    {
        nextAppDist = 0;
        nextAppDistInterval = 1;
    }

    public void set(EnemySpec s, int num, int appType, GunroarRand rand)
    {
        spec = s;
        nextAppDistInterval = ((float)Field.NEXT_BLOCK_AREA_SIZE) / num;
        nextAppDist = rand.nextFloat(nextAppDistInterval);
        this.appType = appType;
    }

    public void unset()
    {
        spec = null;
    }

    public void move(EnemyPool enemies, Field field)
    {
        if (!(spec != null))
            return;
        nextAppDist = nextAppDist - (field.lastScrollY);
        if (nextAppDist <= 0)
        {
            nextAppDist = nextAppDist + (nextAppDistInterval);
            appear(enemies);
        }
    }

    public void appear(EnemyPool enemies)
    {
        Enemy en = enemies.getInstance();
        if (en != null)
        {
            if (((HasAppearType)spec).setFirstState(en.state, appType))
                en.set(spec);
        }
    }
}
