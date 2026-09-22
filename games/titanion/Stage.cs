// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Stage
{
    public bool randomized;
    public const int PHASE_RESULT_SHOW_CNT = 150;
    public const int PHASE_START_SHOW_CNT = 90;
    public Field field;
    public EnemyPool enemies;
    public BulletPool bullets;
    public Player player;
    public ParticlePool particles, bonusParticles;
    public PillarPool pillars;
    public GameState gameState;
    public TitanionRand rand;
    public int appCnt;
    public EnemySpec middleEnemySpec, smallEnemy1Spec, smallEnemy2Spec;
    public EnemyShape enemy1Shape, enemy2Shape, enemy3Shape;
    public EnemyShape enemy1TrailShape, enemy2TrailShape, enemy3TrailShape;
    public BulletSpec bulletSpec, middleBulletSpec, counterBulletSpec;
    public BulletShapeBase bulletShape, bulletLineShape, middleBulletShape, middleBulletLineShape;
    public RollBulletShapeBase counterBulletShape, counterBulletLineShape;
    public PillarSpec pillarSpec;
    public List<PillarShape> pillarShapes = new List<PillarShape>();
    public PillarShape outsidePillarShape;
    public int smallEnemyNum;
    public int smallEnemyFormationNum;
    public float rank;
    public int phaseTime;
    public bool stageStarted, waitNextFormationPhase;
    public int middleEnemyAppInterval;
    public int _attackSmallEnemyNum;
    public float goingDownBeforeStandByRatio;
    public int appCntInterval;
    public int formationIdx;
    public int cnt;
    public float rankTrg;
    public int phaseNum;
    public int shotFiredNum, shotHitNum;
    public int shotFiredNumRsl, shotHitNumRsl;
    public int shotFiredNumTotal, shotHitNumTotal;
    public float hitRatio;
    public int hitRatioBonus;
    public bool counterBulletEnabled;
    public Stage(Field field, EnemyPool enemies, BulletPool bullets, Player player, ParticlePool particles, ParticlePool bonusParticles, PillarPool pillars, GameState gameState)
    {
        this.field = field;
        this.enemies = enemies;
        this.bullets = bullets;
        this.player = player;
        this.particles = particles;
        this.bonusParticles = bonusParticles;
        this.pillars = pillars;
        this.gameState = gameState;
        rand = new TitanionRand();
        enemy1Shape = new Enemy1Shape();
        enemy2Shape = new Enemy2Shape();
        enemy3Shape = new Enemy3Shape();
        enemy1TrailShape = new Enemy1TrailShape();
        enemy2TrailShape = new Enemy2TrailShape();
        enemy3TrailShape = new Enemy3TrailShape();
        bulletShape = new BulletShape();
        bulletLineShape = new BulletLineShape();
        middleBulletShape = new MiddleBulletShape();
        middleBulletLineShape = new MiddleBulletLineShape();
        counterBulletShape = new CounterBulletShape();
        counterBulletLineShape = new CounterBulletLineShape();
        bulletSpec = new BulletSpec(field, player, enemies, particles, bulletShape, bulletLineShape, gameState);
        middleBulletSpec = new BulletSpec(field, player, enemies, particles, middleBulletShape, middleBulletLineShape, gameState);
        counterBulletSpec = new BulletSpec(field, player, enemies, particles, counterBulletShape, counterBulletLineShape, gameState);
        pillarSpec = new PillarSpec(field);
        pillarShapes.Add(new Pillar1Shape());
        pillarShapes.Add(new Pillar2Shape());
        pillarShapes.Add(new Pillar3Shape());
        pillarShapes.Add(new Pillar4Shape());
        outsidePillarShape = new OutsidePillarShape();
    }

    public virtual void close()
    {
        enemy1Shape.close();
        enemy2Shape.close();
        enemy3Shape.close();
        enemy1TrailShape.close();
        enemy2TrailShape.close();
        enemy3TrailShape.close();
        bulletShape.close();
        bulletLineShape.close();
        middleBulletShape.close();
        middleBulletLineShape.close();
        counterBulletShape.close();
        counterBulletLineShape.close();
        foreach (PillarShape ps in pillarShapes)
            ps.close();
        outsidePillarShape.close();
    }

    public virtual void start_1(int randSeed)
    {
        clear();
        rand.setSeed(randSeed);
        EnemySpec.setRandSeed(randSeed);
        TurretSpec.setRandSeed(randSeed);
        PlayerSpec.setRandSeed(randSeed);
        ParticleSpec.setRandSeed(randSeed);
        Sound.setRandSeed(randSeed);
        {
            rankTrg = 0;
            rank = rankTrg;
        }

        phaseNum = 0;
        cnt = 0;
        {
            shotHitNumTotal = 0;
            shotFiredNumTotal = shotHitNumTotal;
        }

        for (int i = 0; i < 1000; i++)
        {
            cnt++;
            moveOutsidePillars();
            pillars.move_0();
        }

        startPhase();
    }

    public virtual void clear()
    {
        {
            smallEnemyFormationNum = 0;
            smallEnemyNum = smallEnemyFormationNum;
        }

        rank = 0;
        phaseTime = 0;
        {
            waitNextFormationPhase = false;
            stageStarted = waitNextFormationPhase;
        }

        middleEnemyAppInterval = 0;
        _attackSmallEnemyNum = 0;
        goingDownBeforeStandByRatio = 0;
        appCntInterval = 0;
        formationIdx = 0;
        cnt = 0;
        rankTrg = 0;
        phaseNum = 0;
        {
            shotHitNum = 0;
            shotFiredNum = shotHitNum;
        }

        {
            shotHitNumRsl = 0;
            shotFiredNumRsl = shotHitNumRsl;
        }

        {
            shotHitNumTotal = 0;
            shotFiredNumTotal = shotHitNumTotal;
        }

        hitRatio = 0;
        hitRatioBonus = 0;
        counterBulletEnabled = false;
    }

    public virtual void startPhase()
    {
        phaseTime = 0;
        phaseNum++;
        if (phaseNum > 1)
            calcHitRatioBonus();
        if (phaseNum % 10 == 0)
            Sound.fadeBgm();
        setEnemySpecs();
        initPillars();
    }

    public virtual void calcHitRatioBonus()
    {
        shotFiredNumRsl = shotFiredNum;
        shotHitNumRsl = shotHitNum;
        {
            shotHitNum = 0;
            shotFiredNum = shotHitNum;
        }

        if (shotFiredNumRsl <= 0)
        {
            hitRatio = 0;
        }
        else
        {
            hitRatio = (float)shotHitNumRsl / shotFiredNumRsl;
        }

        float r = (float)(GameMath.integer((hitRatio * 100))) / 100;
        if (r > 1)
            r = 1;
        hitRatioBonus = GameMath.integer((10000.0f * r * r * r * r));
        if ((gameState.mode_0() == GameStateMode.MODERN))
            return;
        if ((gameState.mode_0() == GameStateMode.BASIC))
            hitRatioBonus = hitRatioBonus * (10);
        gameState.addScore_2(hitRatioBonus, true);
    }

    public virtual void setEnemySpecs()
    {
        rankTrg = rankTrg + (1.25f);
        rank = rank + ((rankTrg - rank) * 0.33f);
        if (phaseNum % 10 == 0)
            rank = rank * (0.1f);
        if (!((randomized)))
        {
            int rs = phaseNum;
            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                    rs = rs * (2);
                    break;
                case GameStateMode.BASIC:
                    break;
                case GameStateMode.MODERN:
                    rs = rs * (3);
                    break;
            }

            rand.setSeed(rs);
            EnemySpec.setRandSeed(rs);
            TurretSpec.setRandSeed(rs);
        }

        counterBulletEnabled = false;
        int en = 0;
        switch (gameState.mode_0())
        {
            case GameStateMode.CLASSIC:
                en = 24 + GameMath.integer(((50 + rand.nextInt(10)) * sqrt(rank) * 0.2f));
                smallEnemyNum = 4 + rand.nextInt(2);
                if (rank > 10)
                    counterBulletEnabled = true;
                middleEnemyAppInterval = 6 + rand.nextInt(2);
                break;
            case GameStateMode.BASIC:
                en = 32 + GameMath.integer(((50 + rand.nextInt(10)) * sqrt(rank) * 0.33f));
                smallEnemyNum = 7 + rand.nextInt(4);
                middleEnemyAppInterval = 5 + rand.nextInt(2);
                break;
            case GameStateMode.MODERN:
                en = 24 + GameMath.integer(((50 + rand.nextInt(10)) * sqrt(rank) * 0.5f));
                smallEnemyNum = 4 + rand.nextInt(2);
                middleEnemyAppInterval = 7 + rand.nextInt(3);
                break;
        }

        smallEnemyFormationNum = (int)(GameMath.integer(en / smallEnemyNum)) + 1;
        middleEnemySpec = new MiddleEnemySpec(field, bullets, player, particles, bonusParticles, enemies, this, enemy3Shape, enemy3TrailShape, middleBulletSpec, counterBulletSpec, gameState);
        middleEnemySpec.setRank(rank * 0.15f);
        smallEnemy1Spec = new SE1Spec(field, bullets, player, particles, bonusParticles, enemies, this, enemy1Shape, enemy1TrailShape, bulletSpec, counterBulletSpec, gameState);
        ((smallEnemy1Spec is SE1Spec ? (SE1Spec)smallEnemy1Spec : null)).setRank(rank * 0.22f);
        smallEnemy2Spec = new SE2Spec(field, bullets, player, particles, bonusParticles, enemies, this, enemy2Shape, enemy2TrailShape, bulletSpec, counterBulletSpec, gameState);
        ((smallEnemy2Spec is SE2Spec ? (SE2Spec)smallEnemy2Spec : null)).setRank(rank * 0.22f);
        _attackSmallEnemyNum = GameMath.integer(sqrt(rank + 2));
        goingDownBeforeStandByRatio = 0;
        if (rand.nextFloat(rank + 1) > 2)
            goingDownBeforeStandByRatio = rand.nextFloat(0.2f) + 0.1f;
        appCntInterval = 48 + rand.nextSignedInt(10);
        appCntInterval = GameMath.integer(appCntInterval * ((0.5f + 0.5f / sqrt(rank))));
        if ((gameState.mode_0() == GameStateMode.MODERN))
        {
            appCntInterval = GameMath.integer(appCntInterval * (0.75f));
            _attackSmallEnemyNum = _attackSmallEnemyNum * (2);
        }

        appCnt = 0;
        formationIdx = 0;
        stageStarted = false;
        waitNextFormationPhase = false;
    }

    public virtual void initPillars()
    {
        pillars.setEnd();
        Pillar pp = null;
        int pln = 0;
        int pn = phaseNum;
        List<int> pshapes = new List<int>();
        for (;;)
        {
            if (pn <= 0)
                break;
            if (pn >= 20)
            {
                pshapes.Add(3);
                pn = pn - (20);
            }
            else if (pn >= 10)
            {
                pshapes.Add(2);
                pn = pn - (10);
            }
            else if (pn >= 5)
            {
                pshapes.Add(1);
                pn = pn - (5);
            }
            else
            {
                pshapes.Add(0);
                pn--;
            }

            pln++;
        }

        float maxY = -15 + pln * 8;
        for (int i = 0; i < pln; i++)
        {
            Pillar p = pillars.getInstance();
            if ((!(((p) != null))))
                break;
            p.set_7(pillarSpec, -80 - i * 10, maxY, pp, pillarShapes[pshapes[i]], (pln - i) * 0.03f);
            pp = p;
        }
    }

    public virtual void move_0()
    {
        if (appCnt <= 0)
        {
            if ((formationIdx % middleEnemyAppInterval) == middleEnemyAppInterval - 1)
            {
                Enemy me = enemies.getInstance();
                if ((!(((me) != null))))
                    return;
                float middleX = rand.nextFloat(field.circularDistance());
                middleX = field.normalizeX(middleX);
                float middleSP = 0.1f + rand.nextSignedFloat(0.01f);
                float middleAV = middleSP * 0.4f + rand.nextSignedFloat(0.005f);
                float middleER = rand.nextFloat(0.5f);
                float middleED = rand.nextFloat(PI * 2);
                me.set_5(middleEnemySpec, middleX, field.size().y * Field.PIT_SIZE_Y_RATIO, PI, middleSP);
                me.setMiddleEnemyState(middleSP, middleAV, middleER, middleED);
            }

            float x = rand.nextFloat(field.circularDistance());
            x = field.normalizeX(x);
            float sp = 0.15f + rand.nextSignedFloat(0.01f);
            float av = sp * 0.5f + rand.nextSignedFloat(0.005f);
            float dst = sp * 6.0f;
            float er = rand.nextFloat(0.8f);
            float ed = rand.nextFloat(PI * 2);
            Enemy fe = null;
            float fir = 0;
            for (int i = 0; i < smallEnemyNum; i++)
            {
                Enemy e = enemies.getInstance();
                if ((!(((e) != null))))
                    break;
                SmallEnemySpec ses = null;
                int appPattern = formationIdx % 2;
                switch (formationIdx % 3)
                {
                    case 0:
                    case 1:
                        ses = (smallEnemy1Spec is SmallEnemySpec ? (SmallEnemySpec)smallEnemy1Spec : null);
                        break;
                    case 2:
                        ses = (smallEnemy2Spec is SmallEnemySpec ? (SmallEnemySpec)smallEnemy2Spec : null);
                        break;
                }

                e.set_5(ses, x, field.size().y * Field.PIT_SIZE_Y_RATIO + i * dst, PI, sp);
                bool gd = false;
                if (rand.nextFloat(1) < goingDownBeforeStandByRatio)
                    gd = true;
                if ((!(((fe) != null))))
                {
                    e.setSmallEnemyState(sp, av, GameMath.integer((i * (dst / sp))), appPattern, er, ed, gd);
                    fe = e;
                }
                else
                {
                    e.setSmallEnemyState(sp, av, GameMath.integer((i * (dst / sp))), appPattern, er, ed, gd, fir, fe);
                }

                fir = fir + ((1.0f / smallEnemyNum));
            }

            smallEnemyFormationNum--;
            formationIdx++;
            if (smallEnemyFormationNum <= 0)
            {
                stageStarted = true;
                appCnt = 9999999;
            }
            else
            {
                appCnt = appCnt + (appCntInterval * (1 - GameMath.integer(1 / (enemies.num() + 1))));
            }
        }

        appCnt--;
        phaseTime++;
        if ((((((((phaseNum >= 10)) && ((phaseNum % 10 == 0)))) && ((phaseTime == 120)))) && ((gameState.isInGameAndNotGameOver()))))
            Sound.nextBgm();
        cnt++;
        moveOutsidePillars();
        if (enemies.numInScreen() > 0)
            gameState.mulMultiplier(0.999f);
        if ((((stageStarted)) && (((enemies.num() <= 0)))))
            startPhase();
    }

    public virtual void moveOutsidePillars()
    {
        if (cnt % 120 == 0)
        {
            Pillar p = pillars.getInstance();
            if ((p) != null)
                p.set_7(pillarSpec, 180, 0, null, outsidePillarShape, ((GameMath.integer((int)cnt / 120)) % 2 * 2 - 1) * 0.003f, true);
        }
    }

    public virtual void countShotFired()
    {
        if (phaseTime >= PHASE_RESULT_SHOW_CNT)
        {
            shotFiredNum++;
            shotFiredNumTotal++;
        }
    }

    public virtual void countShotHit()
    {
        if (phaseTime >= PHASE_RESULT_SHOW_CNT)
        {
            shotHitNum++;
            shotHitNumTotal++;
        }
    }

    public virtual void draw_0()
    {
        if ((((((((gameState.mode_0() != GameStateMode.MODERN))) && ((phaseTime < PHASE_RESULT_SHOW_CNT))))) && ((phaseNum > 1))))
        {
            Letter.drawString("SHOTS FIRED", 152, 250, 6, LetterDirection.TO_RIGHT, false, 0, 1, 1, 0.33f);
            Letter.drawNum(shotFiredNumRsl, 480, 250, 6);
            Letter.drawString("NUMBER OF HITS", 152, 280, 6, LetterDirection.TO_RIGHT, false, 0, 1, 1, 0.33f);
            Letter.drawNum(shotHitNumRsl, 480, 280, 6);
            Letter.drawString("HIT-MISS RATIO", 152, 310, 6);
            Letter.drawNum(GameMath.integer((hitRatio * 10000)), 480, 310, 6, 3, -1, 2);
            Letter.drawString("BONUS", 200, 350, 6, LetterDirection.TO_RIGHT, false, 0, 1, 0.33f, 0.33f);
            Letter.drawNum(hitRatioBonus, 440, 350, 6);
        }
        else if (phaseTime < PHASE_RESULT_SHOW_CNT + PHASE_START_SHOW_CNT)
        {
            Letter.drawNum(phaseNum, 392, 200, 10);
            Letter.drawString("PHASE", 232, 200, 10);
        }
    }

    public virtual void drawPhaseNum()
    {
        Letter.drawNum(phaseNum, 622, 448, 10);
    }

    public virtual void drawGameover()
    {
        float hr = 0;
        if (shotFiredNumTotal > 0)
            hr = (float)shotHitNumTotal / shotFiredNumTotal;
        Letter.drawString("SHOTS FIRED", 152, 250, 6, LetterDirection.TO_RIGHT, false, 0, 1, 1, 0.33f);
        Letter.drawNum(shotFiredNumTotal, 480, 250, 6);
        Letter.drawString("NUMBER OF HITS", 152, 280, 6, LetterDirection.TO_RIGHT, false, 0, 1, 1, 0.33f);
        Letter.drawNum(shotHitNumTotal, 480, 280, 6);
        Letter.drawString("HIT-MISS RATIO", 152, 310, 6);
        Letter.drawNum(GameMath.integer((hr * 10000)), 480, 310, 6, 3, -1, 2);
    }

    public virtual int attackSmallEnemyNum()
    {
        return _attackSmallEnemyNum;
    }

    public virtual bool existsCounterBullet()
    {
        return (((((counterBulletEnabled)) && ((stageStarted)))) && (((enemies.numBeforeAlign() <= 0))));
    }
}
