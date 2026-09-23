// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class StageManager
{
    public const int MAX_BLOCKS_NUM = 16;
    public Field field;
    public BulletPool bullets;
    public EnemyPool enemies;
    public Rand rand;
    public float rank, trgRank;
    public Appearance[] appearances = McdArrays.Make<Appearance>(32, () => default);
    public int appearanceIdx, appearanceNextIdx;
    public float appearanceCnt;
    public EnemySpec _blockSpec;
    public int cnt;
    public int rankDownCnt;
    public StageManager(Field field, Ship ship, BulletPool bullets, World world, EnemyPool enemies)
    {
        this.field = field;
        this.bullets = bullets;
        this.enemies = enemies;
        rand = new Rand();
        for (int idx_a = 0; idx_a < 32; idx_a++)
            appearances[idx_a] = new Appearance(field, ship, bullets, world, this);
        _blockSpec = new Block(field, ship, bullets, world);
    }

    public virtual void start_1(int randSeed)
    {
        initRank();
        rand.setSeed(randSeed);
        Enemy.setRandSeed(randSeed);
        EnemySpec.setRandSeed(randSeed);
        SimpleBullet.setRandSeed(randSeed);
        Ship.setRandSeed(randSeed);
        ShipTail.setRandSeed(randSeed);
        Shot.setRandSeed(randSeed);
        EnhancedShot.setRandSeed(randSeed);
        Particle.setRandSeed(randSeed);
        ConnectedParticle.setRandSeed(randSeed);
        TailParticle.setRandSeed(randSeed);
        Field.setRandSeed(randSeed);
        SoundManager.setRandSeed(randSeed);
        McdPhysics.Seed(randSeed);
        clearAppearances();
        cnt = 0;
        rankDownCnt = 0;
    }

    public virtual void initRank()
    {
        rank = 0;
        trgRank = 30;
    }

    public virtual void clearAppearances()
    {
        appearanceNextIdx = 0;
        appearanceIdx = appearanceNextIdx;
        appearanceCnt = 0;
    }

    public virtual void move_0()
    {
        cnt++;
        float cntInc = 1.0f + rank * 0.04f;
        int cn = enemies.countCentipedes();
        if (cn <= 0)
        {
            appearanceCnt = -1;
        }
        else
        {
            cntInc *= 1 + GameMath.integer(1 / cn);
        }

        appearanceCnt -= cntInc;
        if (appearanceCnt < 0)
        {
            int atypeMax = GameMath.integer((rank * 2.4f));
            if (atypeMax > 16)
                atypeMax = 16;
            int atype = rand.nextInt(atypeMax);
            switch (atype)
            {
                case 0:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 0, 3, 500);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 6, 250);
                    appearanceCnt += 1500;
                    break;
                case 1:
                    addAppearance(rank, AppearanceEnemyType.TO_AND_FROM, 0, 3, 500);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 6, 250);
                    appearanceCnt += 1500;
                    break;
                case 2:
                    addAppearance(rank, AppearanceEnemyType.ROLL, 0, 3, 30);
                    appearanceCnt += 1200;
                    break;
                case 3:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 0, 5, 20);
                    appearanceCnt += 1600;
                    break;
                case 4:
                    addAppearance(rank * 1.1f, AppearanceEnemyType.CHASE, 1, 1, 1);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 10, 20);
                    appearanceCnt += 800;
                    break;
                case 5:
                    addAppearance(rank * 0.9f, AppearanceEnemyType.CHASE, 0, 3, 500);
                    addAppearance(rank * 0.9f, AppearanceEnemyType.TO_AND_FROM, 0, 3, 500);
                    appearanceCnt += 1800;
                    break;
                case 6:
                    addAppearance(rank * 1.1f, AppearanceEnemyType.TO_AND_FROM, 1, 2, 400);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 9, 120);
                    appearanceCnt += 1000;
                    break;
                case 7:
                    addAppearance(rank * 0.9f, AppearanceEnemyType.CHASE, 0, 3, 500);
                    addAppearance(rank * 0.9f, AppearanceEnemyType.ROLL, 0, 3, 500);
                    appearanceCnt += 1800;
                    break;
                case 8:
                    addAppearance(rank * 1.1f, AppearanceEnemyType.ROLL, 1, 1, 1);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 10, 20);
                    appearanceCnt += 800;
                    break;
                case 9:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 0, 7, 15);
                    appearanceCnt += 2000;
                    break;
                case 10:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 1, 1, 1);
                    addAppearance(rank * 0.8f, AppearanceEnemyType.TO_AND_FROM, 1, 3, 400);
                    appearanceCnt += 1200;
                    break;
                case 11:
                    addAppearance(rank * 1.1f, AppearanceEnemyType.TO_AND_FROM, 2, 1, 1);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 8, 60);
                    appearanceCnt += 1000;
                    break;
                case 12:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 0, 5, 300);
                    addAppearance(rank, AppearanceEnemyType.TO_AND_FROM, 2, 2, 400);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 6, 350);
                    appearanceCnt += 2000;
                    break;
                case 13:
                    addAppearance(rank, AppearanceEnemyType.CHASE, 2, 1, 1);
                    addAppearance(rank, AppearanceEnemyType.CHASE, 0, 3, 20);
                    appearanceCnt += 1200;
                    break;
                case 14:
                    addAppearance(rank * 1.1f, AppearanceEnemyType.ROLL, 2, 1, 1);
                    addAppearance(0, AppearanceEnemyType.BLOCK, 0, 10, 60);
                    appearanceCnt += 1000;
                    break;
                case 15:
                    addAppearance(rank, AppearanceEnemyType.TO_AND_FROM, 1, 2, 250);
                    addAppearance(rank, AppearanceEnemyType.CHASE, 1, 2, 250);
                    appearanceCnt += 2000;
                    break;
            }
        }

        bool forced = false;
        if (cn <= 0)
            forced = true;
        int idx = appearanceIdx;
        for (int i = 0; i < appearances.Length; i++)
        {
            if (idx == appearanceNextIdx)
                break;
            if (!(appearances[idx].move_3(rand, cntInc, forced)))
            {
                if (idx == appearanceIdx)
                {
                    appearanceIdx++;
                    if (appearanceIdx >= appearances.Length)
                        appearanceIdx = 0;
                }
            }

            idx++;
            if (idx >= appearances.Length)
                idx = 0;
        }

        if (rank > trgRank * 0.9f)
            rank = trgRank * 0.9f;
        rank += trgRank / sqrt(trgRank - rank) * 0.0006f;
    }

    public virtual void downRank()
    {
        if (rankDownCnt % 2 == 0)
        {
            rank *= 0.5f;
        }
        else
        {
            rank *= 0.1f;
            enemies.slowdown();
            bullets.slowdown();
            clearAppearances();
            appearanceCnt = 120;
        }

        trgRank += 20 / sqrt(trgRank / 30);
        rankDownCnt++;
    }

    public virtual void addAppearance(float rank, int type, int size, int num, int interval)
    {
        Appearance a = appearances[appearanceNextIdx];
        appearanceNextIdx++;
        if (appearanceNextIdx >= appearances.Length)
            appearanceNextIdx = 0;
        a.set_5_Single_Int32_Int32_Int32_Int32(rank, type, size, num, interval);
    }

    public virtual Enemy set_9_EnemySpec_Single_Single_Single_Single_Single_Single_Single_Single(EnemySpec es, float x, float y, float z, float deg, float sx = 1, float sy = 1, float sz = 1, float massScale = 1)
    {
        Enemy e = default(Enemy);
        if ((((es is Block ? (Block)es : null))) != null && enemies.countBlocks() > MAX_BLOCKS_NUM)
            return null;
        if ((((es is CentHead ? (JointedEnemySpec)es : null))) != null)
        {
            e = (((es is CentHead ? (JointedEnemySpec)es : null))).setJointedEnemies_5(enemies, x, y, z, deg);
        }
        else
        {
            e = enemies.getInstance();
            if (!((e) != null))
                return null;
            if (!(e.set_10(es, x, y, z, deg, sx, sy, sz, massScale)))
                return null;
        }

        return e;
    }

    public virtual EnemySpec blockSpec()
    {
        return _blockSpec;
    }
}

public class Appearance
{
    public Field field;
    public Ship ship;
    public BulletPool bullets;
    public World world;
    public StageManager stageManager;
    public EnemySpec spec;
    public Vector blockAppPos;
    public float cnt;
    public int num;
    public int interval;
    public int appType;
    public Appearance(Field field, Ship ship, BulletPool bullets, World world, StageManager stageManager)
    {
        this.field = field;
        this.ship = ship;
        this.bullets = bullets;
        this.world = world;
        this.stageManager = stageManager;
        blockAppPos = new Vector();
        num = 0;
    }

    public virtual void set_5_Single_Int32_Int32_Int32_Int32(float rank, int type, int size, int num, int interval)
    {
        appType = AppearanceAppearanceType.NORMAL;
        switch (type)
        {
            case AppearanceEnemyType.TO_AND_FROM:
                spec = new CentHeadToAndFrom(field, ship, bullets, world, rank, size);
                break;
            case AppearanceEnemyType.CHASE:
                spec = new CentHeadChase(field, ship, bullets, world, rank, size);
                break;
            case AppearanceEnemyType.ROLL:
                spec = new CentHeadRoll(field, ship, bullets, world, rank, size);
                break;
            case AppearanceEnemyType.BLOCK:
                spec = stageManager.blockSpec();
                appType = AppearanceAppearanceType.BLOCK;
                blockAppPos.x = field.size().x * 2;
                blockAppPos.y = field.size().y * 2;
                break;
        }

        this.num = num;
        this.interval = interval;
        cnt = 0;
    }

    public virtual bool move_3(Rand rand, float cntInc, bool forced)
    {
        if (num <= 0)
            return false;
        if (forced)
        {
            if (cnt > 5)
                cnt = 5;
        }

        cnt -= cntInc;
        if (cnt < 0)
        {
            cnt = interval;
            switch (appType)
            {
                case AppearanceAppearanceType.NORMAL:
                    stageManager.set_9_EnemySpec_Single_Single_Single_Single_Single_Single_Single_Single(spec, rand.nextSignedFloat(field.size().x * 0.5f), rand.nextSignedFloat(field.size().y * 0.5f), 10, rand.nextSignedFloat(PI));
                    break;
                case AppearanceAppearanceType.BLOCK:
                    float xs = 1 + rand.nextFloat(1);
                    float ys = 1 + rand.nextFloat(1);
                    float zs = 1 + rand.nextFloat(1);
                    if (rand.nextInt(3) == 0)
                    {
                        switch (rand.nextInt(3))
                        {
                            case 0:
                                xs *= (2 + rand.nextFloat(2));
                                break;
                            case 1:
                                ys *= (2 + rand.nextFloat(2));
                                break;
                            case 2:
                                zs *= (2 + rand.nextFloat(2));
                                break;
                        }
                    }

                    if (fabs(blockAppPos.x) >= field.size().x)
                        blockAppPos.x = rand.nextSignedFloat(field.size().x * 0.5f);
                    if (fabs(blockAppPos.y) >= field.size().y)
                        blockAppPos.y = rand.nextSignedFloat(field.size().y * 0.5f);
                    stageManager.set_9_EnemySpec_Single_Single_Single_Single_Single_Single_Single_Single(spec, blockAppPos.x, blockAppPos.y, 10, rand.nextSignedFloat(PI), xs, ys, zs, xs * ys * zs);
                    blockAppPos.x += rand.nextSignedFloat(0.5f);
                    blockAppPos.y += rand.nextSignedFloat(0.5f);
                    break;
            }

            num--;
        }

        return true;
    }
}

public static class AppearanceEnemyType
{
    public const int TO_AND_FROM = 0;
    public const int CHASE = 1;
    public const int ROLL = 2;
    public const int BLOCK = 3;
}

public static class AppearanceAppearanceType
{
    public const int NORMAL = 0;
    public const int BLOCK = 1;
}
