// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrInput;
using static RrPreference;
using static RrCore;
using static RrAttract;
using static RrShip;
using static RrLaser;
using static RrShot;
using static RrFrag;
using static RrBackground;
using static RrBoss;
using static RrFoe;
using static RrScreen;
using static RrLetter;
using static RrAngles;
using static RrVector;

public static class RrBoss
{
    public static Boss boss = new Boss();
    public static BossShape bossShape = new BossShape();
    public static int bossTimer;
    public static void initBoss()
    {
        boss.x = BOSS_INITIAL_X;
        boss.y = BOSS_INITIAL_Y;
        boss.d = 0;
        boss.cnt = 0;
    }

    public static Vector bossPos = new Vector();
    public static Vector getBossPos()
    {
        bossPos.x = boss.x;
        bossPos.y = boss.y;
        return bossPos;
    }

    public static void setBatteryGroupPos(BatteryGroup right, BatteryGroup left, int cx, int cy)
    {
        int r = 0, i = 0, x = 0, y = 0, bn = 0;
        int d = 0, ox = 0, oy = 0;
        bn = right.batteryNum;
        if (randN(4) == 0)
        {
            r = randN(5000) + 5000;
            d = randN(1024);
            {
                i = 0;
                for (; i < bn; i++, d = d + (GameMath.integer(1024 / bn)))
                {
                    d = d & (1023);
                    x = cx + (GameMath.signedShift((sctbl[(d)] * r), 8));
                    y = cy + (GameMath.signedShift((sctbl[(d + 256)] * r), 8));
                    right.battery[(i)].x = x;
                    right.battery[(i)].y = y;
                    left.battery[(i)].x = -x;
                    left.battery[(i)].y = y;
                }
            }
        }
        else
        {
            r = randN(4000) + 4000;
            d = randN(1024);
            ox = (GameMath.signedShift((sctbl[(d)] * r), 8));
            oy = (GameMath.signedShift((sctbl[(d + 256)] * r), 8));
            x = cx - GameMath.integer((bn - 1) * ox / 2);
            y = cy - GameMath.integer((bn - 1) * oy / 2);
            {
                i = 0;
                for (; i < bn; i++)
                {
                    x = x + (ox);
                    y = y + (oy);
                    right.battery[(i)].x = x;
                    right.battery[(i)].y = y;
                    left.battery[(i)].x = -x;
                    left.battery[(i)].y = y;
                }
            }
        }
    }

    public static float[] baseSize = new float[]
    {
        0.4f,
        0.36f,
        0.3f
    };
    public static int[][] shapePtn = new int[][]
    {
        new int[]
        {
            3,
            2,
            4,
            6
        },
        new int[]
        {
            5,
            0,
            2,
            3,
            4,
            5
        },
        new int[]
        {
            4,
            0,
            1,
            4,
            5
        }
    };
    public static void setBatteryShape(BatteryShape shape)
    {
        int i = 0;
        if (mode == IKA_MODE)
        {
            shape.color = randN(2);
        }
        else
        {
            shape.color = randN(BULLET_COLOR_NUM);
        }

        {
            i = 0;
            for (; i < BULLET_TYPE_NUM; i++)
            {
                if (mode == IKA_MODE)
                {
                    shape.bulletShape[(i)] = randN(2);
                    shape.bulletSize[(i)] = baseSize[(i)] * ((float)(256 + randN(72)) / 256.0f);
                }
                else
                {
                    shape.bulletShape[(i)] = shapePtn[(i)][(randN(shapePtn[(i)][(0)] + 1))];
                    shape.bulletSize[(i)] = baseSize[(i)] * ((float)(224 + randN(96)) / 256.0f);
                }
            }
        }
    }

    public static void setAttackIndex(Attack at, int center)
    {
        int i = 0;
        if (((center) != 0))
        {
            switch (randN(3))
            {
                case 0:
                case 1:
                    at.barrageType = NORMAL_BARRAGE;
                    break;
                case 2:
                    at.barrageType = SIMPLE_BARRAGE;
                    break;
            }
        }
        else
        {
            switch (randN(2))
            {
                case 0:
                    at.barrageType = NORMAL_BARRAGE;
                    break;
                case 1:
                    at.barrageType = REVERSIBLE_BARRAGE;
                    break;
            }
        }

        at.xReverse = randN(2) * 2 - 1;
        if ((at.barrageType == REVERSIBLE_BARRAGE) && (randN(4) == 0))
        {
            at.xrAlter = 1;
        }
        else
        {
            at.xrAlter = 0;
        }

        at.barrageIdx = randN(barragePatternNum[(at.barrageType)]);
        {
            i = 0;
            for (; i < MORPH_PATTERN_MAX; i++)
            {
                at.morphIdx[(i)] = randN(barragePatternNum[(MORPH_BARRAGE)]);
            }
        }

        if ((at.barrageType != SIMPLE_BARRAGE) && (randN(4) == 0))
            at.morphHalf = 1;
        else
            at.morphHalf = 0;
        if (mode == IKA_MODE)
        {
            int ir = randN(8), it = 0;
            if (((center) != 0))
            {
                if (ir < 4)
                    it = IKA_ALTERNATE;
                else if (ir < 7)
                    it = IKA_ALTERNATE_SHOT;
                else
                    it = IKA_HALF;
            }
            else
            {
                if (ir < 3)
                    it = IKA_FIX;
                else if (ir < 6)
                    it = IKA_ALTERNATE;
                else
                    it = IKA_ALTERNATE_SHOT;
            }

            at.ikaType = it;
        }
    }

    public static void setAttackRank(Attack at, float rank)
    {
        if (rank <= 0.3f)
        {
            at.rank = rank;
            at.morphCnt = 0;
            at.speedRank = 1;
        }
        else
        {
            at.rank = rank * (90 + randN(38)) / 256.0f;
            if (at.rank > 0.8f)
            {
                at.rank = 0.2f * (randN(8) + 1) / 8.0f + 0.8f;
            }

            rank = rank / ((at.rank + 2));
            if (mode == IKA_MODE)
            {
                at.speedRank = sqrt(rank) * (randN(80) + 256) / 256;
            }
            else if (mode == GW_MODE)
            {
                at.speedRank = sqrt(rank) * (randN(92) + 236) / 256;
            }
            else
            {
                at.speedRank = sqrt(rank) * (randN(128) + 192) / 256;
            }

            if (at.speedRank < 0.8f)
                at.speedRank = 0.8f;
            at.morphRank = rank / at.speedRank;
            at.morphCnt = 0;
            while (at.morphRank > 1)
            {
                at.morphCnt++;
                at.morphRank = at.morphRank / (3);
            }
        }

        at.morphType = -1;
        if (at.barrageType == SIMPLE_BARRAGE)
        {
            if (at.morphCnt == 0)
                at.morphCnt++;
            at.morphType = MORPH_HEAVY_BARRAGE;
            at.morphIdx[((at.morphCnt - 1) & (MORPH_PATTERN_MAX - 1))] = randN(barragePatternNum[(MORPH_HEAVY_BARRAGE)]);
        }

        if (at.morphCnt == 0)
        {
            switch (mode)
            {
                case PSY_MODE:
                    at.morphCnt = 1;
                    at.morphType = PSY_MORPH_BARRAGE;
                    at.morphIdx[(0)] = randN(barragePatternNum[(PSY_MORPH_BARRAGE)]);
                    break;
            }

            at.morphRank = 0.5f + randN(6) * 0.1f;
        }

        switch (mode)
        {
            case NORMAL_MODE:
                at.morphRank = at.morphRank * (0.7f);
                at.speedRank = at.speedRank * (0.8f);
                break;
            case PSY_MODE:
                at.speedRank = at.speedRank * (0.72f);
                break;
            case IKA_MODE:
                at.morphRank = at.morphRank * (0.8f);
                at.speedRank = at.speedRank * (0.77f);
                break;
            case GW_MODE:
                at.speedRank = at.speedRank * (0.8f);
                break;
        }
    }

    public static void setAttack(Attack at, float rank, int center)
    {
        setAttackIndex(at, center);
        setAttackRank(at, rank);
    }

    public static void setFoeBattery(Boss bs, Battery bt, Attack at, BatteryShape sp, Limiter lt, int idx)
    {
        int[] mrp = Make(MORPH_PATTERN_MAX, () => 0);
        int i = 0, mi = 0;
        int xr = 0;
        if (at.barrageType == NOT_EXIST)
        {
            bt.foe = null;
            return;
        }

        {
            i = 0;
            for (; i < MORPH_PATTERN_MAX; i++)
            {
                mrp[(i)] = barragePattern[(MORPH_BARRAGE)][(at.morphIdx[(i)])].bulletml;
            }
        }

        if (at.morphType >= 0)
        {
            switch (at.morphType)
            {
                case PSY_MORPH_BARRAGE:
                    mrp[(0)] = barragePattern[(at.morphType)][(at.morphIdx[(0)])].bulletml;
                    break;
                default:
                    mi = (at.morphCnt - 1) & (MORPH_PATTERN_MAX - 1);
                    mrp[(mi)] = barragePattern[(at.morphType)][(at.morphIdx[(mi)])].bulletml;
                    break;
            }
        }

        xr = at.xReverse;
        if ((((at.xrAlter) != 0)) && ((idx & 1) == 1))
            xr = -xr;
        bt.foe = addFoeBattery(bs.x + bt.x, bs.y + bt.y, at.rank, 512, 0, xr, mrp, at.morphCnt, at.morphHalf, at.morphRank, at.speedRank, sp.color, sp.bulletShape, sp.bulletSize, lt, at.ikaType, barragePattern[(at.barrageType)][(at.barrageIdx)].bulletml);
    }

    public static void setBossWing(BossWing lw, BossWing rw, int size, int num)
    {
        int i = 0, j = 0;
        {
            rw.wingNum = num;
            lw.wingNum = rw.wingNum;
        }

        {
            rw.size = 0;
            lw.size = rw.size;
        }

        {
            i = 0;
            for (; i < num; i++)
            {
                {
                    j = 0;
                    for (; j < 2; j++)
                    {
                        lw.x[(i)][(j)] = ((float)(randN(GameMath.integer(size / 2)) + GameMath.integer(size / 2)) / FIELD_SCREEN_RATIO) * (randN(2) * 2 - 1);
                        lw.y[(i)][(j)] = ((float)(randN(GameMath.integer(size / 2)) + GameMath.integer(size / 2)) / FIELD_SCREEN_RATIO) * (randN(2) * 2 - 1);
                        lw.z[(i)][(j)] = ((float)(randN(GameMath.integer(size / 2)) + GameMath.integer(size / 2)) / FIELD_SCREEN_RATIO) * (randN(2) * 2 - 1);
                        rw.x[(i)][(j)] = -lw.x[(i)][(j)];
                        rw.y[(i)][(j)] = lw.y[(i)][(j)];
                        rw.z[(i)][(j)] = lw.z[(i)][(j)];
                    }
                }
            }
        }
    }

    public static void setBossTree(BatteryGroup bg, BossTree left, BossTree right)
    {
        int cx = 0, cy = 0, tn = 0, bn = 0, x = 0, y = 0;
        int i = 0;
        {
            right.diffuse = 0;
            left.diffuse = right.diffuse;
        }

        bn = bg.batteryNum;
        {
            right.epNum = bn;
            left.epNum = right.epNum;
        }

        {
            cy = 0;
            cx = cy;
        }

        {
            i = 0;
            for (; i < bn; i++)
            {
                cx = cx + (bg.battery[(i)].x);
                cy = cy + (bg.battery[(i)].y);
                left.ex[(i)] = (float)bg.battery[(i)].x / FIELD_SCREEN_RATIO;
                left.ey[(i)] = (float)bg.battery[(i)].y / FIELD_SCREEN_RATIO;
                left.ez[(i)] = 0;
                right.ex[(i)] = -(float)bg.battery[(i)].x / FIELD_SCREEN_RATIO;
                right.ey[(i)] = (float)bg.battery[(i)].y / FIELD_SCREEN_RATIO;
                right.ez[(i)] = 0;
                setBossWing((left.eWing[(i)]), (right.eWing[(i)]), 5000, 1);
            }
        }

        cx = GameMath.integer(cx / (bn));
        cy = GameMath.integer(cy / (bn));
        cx = GameMath.integer(cx / (2));
        cy = GameMath.integer(cy / (2));
        tn = 2 + randN(TREE_MAX_LENGTH - 2);
        {
            right.posNum = tn + 1;
            left.posNum = right.posNum;
        }

        {
            y = 0;
            x = y;
        }

        cx = GameMath.integer(cx / (tn));
        cy = GameMath.integer(cy / (tn));
        {
            i = 0;
            for (; i <= tn; i++)
            {
                if (i == 0)
                {
                    {
                        left.z[(i)] = 0;
                        left.y[(i)] = left.z[(i)];
                        left.x[(i)] = left.y[(i)];
                    }
                }
                else
                {
                    left.x[(i)] = (float)(x + randNS(2000)) / FIELD_SCREEN_RATIO;
                    left.y[(i)] = (float)(y + randNS(2000)) / FIELD_SCREEN_RATIO;
                    left.z[(i)] = (float)randNS(3000) / FIELD_SCREEN_RATIO;
                }

                right.x[(i)] = -left.x[(i)];
                right.y[(i)] = left.y[(i)];
                right.z[(i)] = left.z[(i)];
                x = x + (cx);
                y = y + (cy);
                setBossWing((left.wing[(i)]), (right.wing[(i)]), 10000, 2);
            }
        }
    }

    public static int bossDstBaseTime;
    public static void createBoss(int seed, float rank, int round)
    {
        int bn = 0, bgn = 0, lbn = 0, bgni = 0;
        int i = 0, j = 0;
        int wx = 0, wy = 0, cx = 0, cy = 0;
        float tr = 0, sr = 0, sra = 0;
        int bx = 0, by = 0;
        int idx1 = 0, idx2 = 0;
        int maxX = -999999, maxXY = 0;
        int vbgn = 0, vn = 0;
        int[] vbg = Make(GameMath.integer(BATTERY_GROUP_MAX / 2), () => 0);
        {
            vbgn = 0;
            maxXY = vbgn;
            cx = maxXY;
        }

        setSeed(seed);
        wx = 18000 + randN(round * 2000 + 5000);
        wy = 12000 + randN(round * 1500 + 3000);
        bn = GameMath.integer(round / 2) + 4 + randN(GameMath.integer(round / 2) + 2);
        for (; (bn > 0) && (bgn < GameMath.integer(BATTERY_GROUP_MAX / 2)); bgni = bgni + (2))
        {
            bgn++;
            lbn = randN(3) + 1;
            if (lbn >= bn)
            {
                boss.batteryGroup[(bgni)].batteryNum = bn;
                boss.batteryGroup[(bgni + 1)].batteryNum = bn;
                break;
            }

            bn = bn - (lbn);
            boss.batteryGroup[(bgni)].batteryNum = lbn;
            boss.batteryGroup[(bgni + 1)].batteryNum = lbn;
        }

        boss.batteryGroupNum = bgn * 2;
        cy = -wy;
        {
            i = 0;
            for (; i < bgn; i++)
            {
                cx = randN(GameMath.integer(wx * 2 / 3)) + GameMath.integer(wx / 3);
                cy = cy + (GameMath.integer(wy * 3 / (bgn + 1)));
                setBatteryGroupPos((boss.batteryGroup[(i * 2)]), (boss.batteryGroup[(i * 2 + 1)]), cx, cy);
                if (cx > maxX)
                {
                    maxX = cx;
                    maxXY = cy;
                }

                if (i == 0)
                {
                    boss.collisionYUp = cy;
                }
            }
        }

        boss.collisionX[(0)] = GameMath.integer(-maxX * 4 / 3);
        boss.collisionY[(0)] = maxXY;
        boss.collisionX[(4)] = GameMath.integer(maxX * 4 / 3);
        boss.collisionY[(4)] = maxXY;
        boss.collisionX[(1)] = GameMath.integer(-cx * 4 / 3);
        boss.collisionY[(1)] = cy;
        boss.collisionX[(3)] = GameMath.integer(cx * 4 / 3);
        boss.collisionY[(3)] = cy;
        boss.collisionX[(2)] = 0;
        boss.collisionY[(2)] = GameMath.integer(cy / 2);
        if (boss.collisionYUp > maxXY - 10000)
        {
            boss.collisionYUp = maxXY - 10000;
        }

        boss.shield = BOSS_SHIELD + GameMath.integer(round * (BOSS_SHIELD_MAX - BOSS_SHIELD) / 4);
        boss.patternChangeShield = GameMath.integer(boss.shield * (70 + randN(8)) / 256);
        {
            boss.damageCnt = 0;
            boss.damaged = boss.damageCnt;
        }

        bossDstBaseTime = (GameMath.integer(boss.shield / 40)) * BOSS_TIMER_COUNT_UP;
        setBatteryShape((boss.shape));
        {
            i = 0;
            for (; i < bgn; i++)
            {
                setBatteryShape((boss.batteryGroup[(i * 2)].shape));
                boss.batteryGroup[(i * 2 + 1)].shape.copyFrom(boss.batteryGroup[(i * 2)].shape);
                if (mode == IKA_MODE)
                {
                    boss.batteryGroup[(i * 2 + 1)].shape.color = boss.batteryGroup[(i * 2 + 1)].shape.color ^ (1);
                }
            }
        }

        boss.patternNum = 4 + randN(3);
        {
            j = 0;
            for (; j < boss.patternNum; j++)
            {
                {
                    i = 0;
                    for (; i < bgn; i++)
                    {
                        setAttackIndex((boss.batteryGroup[(i * 2)].attack[(j)]), 0);
                    }
                }
            }
        }

        {
            j = 0;
            for (; j < boss.patternNum; j++)
            {
                tr = rank;
                if (j == 0)
                {
                    tr = tr * (1.2f);
                    vbgn = 1 + GameMath.integer(bgn / 2);
                }
                else
                {
                    switch (randN(6))
                    {
                        case 0:
                            vbgn = 0;
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            vbgn = 1;
                            break;
                        case 5:
                            vbgn = 2;
                            break;
                    }
                }

                if (vbgn > bgn)
                    vbgn = bgn;
                {
                    i = 0;
                    for (; i < bgn; i++)
                        vbg[(i)] = 0;
                }

                {
                    i = 0;
                    for (; i < vbgn; i++)
                    {
                        vn = randN(bgn);
                        while (((vbg[(vn)]) != 0))
                        {
                            vn--;
                            if (vn < 0)
                                vn = vn + (bgn);
                        }

                        vbg[(vn)] = 1;
                    }
                }

                sra = tr / vbgn;
                {
                    i = 0;
                    for (; i < bgn; i++)
                    {
                        if ((tr > 0) && (((vbg[(i)]) != 0)))
                        {
                            sr = ((float)randN((GameMath.integer((sra * 256 + 1))))) / 256 + sra / 2;
                            if (sr > tr)
                                sr = tr;
                            bn = boss.batteryGroup[(i * 2)].batteryNum;
                            tr = tr - (sr);
                            sr = sr / ((bn * bn));
                            setAttackRank((boss.batteryGroup[(i * 2)].attack[(j)]), sr / bn);
                            boss.batteryGroup[(i * 2 + 1)].attack[(j)].copyFrom(boss.batteryGroup[(i * 2)].attack[(j)]);
                            boss.batteryGroup[(i * 2 + 1)].attack[(j)].xReverse = boss.batteryGroup[(i * 2 + 1)].attack[(j)].xReverse * (-1);
                            {
                                boss.batteryGroup[(i * 2 + 1)].limiter.max = GameMath.integer((rank * 5)) + 1;
                                boss.batteryGroup[(i * 2)].limiter.max = boss.batteryGroup[(i * 2 + 1)].limiter.max;
                            }
                        }
                        else
                        {
                            boss.batteryGroup[(i * 2)].attack[(j)].barrageType = NOT_EXIST;
                            boss.batteryGroup[(i * 2 + 1)].attack[(j)].barrageType = NOT_EXIST;
                        }
                    }
                }

                if (tr > 0)
                {
                    setAttack((boss.topAttack[(j)]), tr, 1);
                    boss.topLimiter.max = GameMath.integer((rank * 12)) + 2;
                }
                else
                {
                    boss.topAttack[(j)].barrageType = NOT_EXIST;
                }
            }
        }

        boss.patternLgt = 420 + randN(90);
        boss.patternIdx = 1;
        boss.patternCnt = BOSS_PATTERN_CHANGE_CNT;
        boss.state = CREATING;
        boss.stateCnt = BOSS_PATTERN_CHANGE_CNT;
        wx = 10000 - round * 2000 + randN(4001 - round * 1000);
        wy = 4500 - round * 800 + randN(2001 - round * 500);
        boss.mpNum = randN(3) + 2;
        {
            i = 0;
            for (; i < GameMath.integer(boss.mpNum / 2); i++)
            {
                boss.mpx[(i * 2)] = randN(GameMath.integer(wx / 2)) + GameMath.integer(wx / 2);
                boss.mpx[(i * 2 + 1)] = -boss.mpx[(i * 2)];
                {
                    boss.mpy[(i * 2 + 1)] = randN(wy * 2) - wy + boss.y;
                    boss.mpy[(i * 2)] = boss.mpy[(i * 2 + 1)];
                }
            }
        }

        if (boss.mpNum == 3)
        {
            boss.mpx[(2)] = 0;
            boss.mpy[(2)] = randN(wy * 2) - wy + boss.y;
        }

        {
            i = 0;
            for (; i < 8; i++)
            {
                idx1 = randN(boss.mpNum);
                idx2 = randN(boss.mpNum);
                if (idx1 == idx2)
                {
                    idx2++;
                    if (idx2 >= boss.mpNum)
                        idx2 = 0;
                }

                bx = boss.mpx[(idx1)];
                by = boss.mpy[(idx2)];
                boss.mpx[(idx1)] = boss.mpx[(idx2)];
                boss.mpy[(idx1)] = boss.mpy[(idx2)];
                boss.mpx[(idx2)] = bx;
                boss.mpy[(idx2)] = by;
            }
        }

        boss.speed = randN(48) + 64 - 8 * round;
        boss.md = 2 + randN(3);
        boss.mpIdx = 0;
        boss.onRoute = 0;
        bossShape.r = 240;
        bossShape.g = 240;
        bossShape.b = 120;
        {
            i = 0;
            for (; i < boss.batteryGroupNum; i = i + (2))
            {
                BatteryGroup bg = null;
                bg = (boss.batteryGroup[(i)]);
                setBossTree(bg, (bossShape.tree[(i)]), (bossShape.tree[(i + 1)]));
            }
        }

        bossTimer = 0;
    }

    public static void addBossTreeFrag(BossTree bt)
    {
        int i = 0, dst = 0, deg = 0;
        int ox = 0, oy = 0;
        float x = 0, y = 0;
        int bpn = 0;
        x = (float)boss.x / FIELD_SCREEN_RATIO;
        y = -(float)boss.y / FIELD_SCREEN_RATIO;
        bpn = bt.posNum - 1;
        {
            i = 0;
            for (; i < bpn; i++)
            {
                ox = GameMath.integer(((bt.x[(i + 1)] - bt.x[(i)]) * 256));
                oy = -GameMath.integer(((bt.y[(i + 1)] - bt.y[(i)]) * 256));
                dst = getDistance(ox, oy);
                deg = getDeg(ox, oy);
                addBossFrag((bt.x[(i + 1)] + bt.x[(i)]) / 2 + x, -(bt.y[(i + 1)] + bt.y[(i)]) / 2 + y, (bt.z[(i + 1)] + bt.z[(i)]) / 2, (float)dst / 512, deg);
            }
        }

        {
            i = 0;
            for (; i < bt.epNum; i++)
            {
                ox = GameMath.integer(((bt.ex[(i)] - bt.x[(bpn)]) * 256));
                oy = -GameMath.integer(((bt.ey[(i)] - bt.y[(bpn)]) * 256));
                dst = getDistance(ox, oy);
                deg = getDeg(ox, oy);
                addBossFrag((bt.ex[(i)] + bt.x[(bpn)]) / 2 + x, -(bt.ey[(i)] + bt.y[(bpn)]) / 2 + y, (bt.ez[(i)] + bt.z[(bpn)]) / 2, (float)dst / 512, deg);
            }
        }
    }

    public static void addBossTreeFragPart()
    {
        addBossTreeFrag((bossShape.tree[(randN(boss.batteryGroupNum))]));
    }

    public static void destroyBoss()
    {
        boss.state = DESTROIED;
        boss.patternCnt = 999999;
        boss.stateCnt = BOSS_PATTERN_CHANGE_CNT * 2;
        clearFoes();
        playChunk(5);
        setScreenShake(0, BOSS_PATTERN_CHANGE_CNT * 2);
        ship.absEng = 0;
    }

    public static int handleLimiter(Limiter lt, int allLmtOn)
    {
        if (!(((lt.on) != 0)))
        {
            if (lt.cnt > lt.max)
            {
                lt.on = 1;
                lt.cnt = GameMath.integer(lt.cnt / (3));
                lt.cnt = lt.cnt + (32);
            }
        }

        if (((boss.cnt & 3) == 0) && (lt.cnt > 0))
        {
            lt.cnt--;
            if (((allLmtOn) != 0))
                lt.cnt = lt.cnt - (2);
            if ((((lt.on) != 0)) && (lt.cnt <= 0))
            {
                lt.on = 0;
                lt.cnt = 0;
            }
        }

        return lt.on;
    }

    public static int allLmtOn = 0;
    public static void moveBoss()
    {
        int bpi = 0;
        int i = 0, j = 0;
        int ax = 0, ay = 0, d = 0, od = 0, aod = 0, emd = 0;
        int dfsChg = 0;
        int lmtOn = 0;
        boss.patternCnt--;
        if (boss.patternCnt < 0)
        {
            if (boss.state == LAST_ATTACK)
            {
                boss.patternCnt = 999999;
            }
            else
            {
                boss.patternCnt = boss.patternLgt;
            }

            bpi = boss.patternIdx;
            boss.topLimiter.cnt = 0;
            boss.topLimiter.on = 0;
            setFoeBattery(boss, (boss.topBattery), (boss.topAttack[(bpi)]), (boss.shape), (boss.topLimiter), 0);
            {
                i = 0;
                for (; i < boss.batteryGroupNum; i++)
                {
                    BatteryGroup bg = (boss.batteryGroup[(i)]);
                    bg.limiter.cnt = 0;
                    bg.limiter.on = 0;
                    {
                        j = 0;
                        for (; j < bg.batteryNum; j++)
                        {
                            setFoeBattery(boss, (bg.battery[(j)]), (bg.attack[(bpi)]), (bg.shape), (bg.limiter), j);
                        }
                    }
                }
            }
        }
        else if (boss.patternCnt == BOSS_PATTERN_CHANGE_CNT)
        {
            if (((boss.topBattery.foe) != null))
                removeFoeForced(boss.topBattery.foe);
            {
                i = 0;
                for (; i < boss.batteryGroupNum; i++)
                {
                    BatteryGroup bg = (boss.batteryGroup[(i)]);
                    {
                        j = 0;
                        for (; j < bg.batteryNum; j++)
                        {
                            if (((bg.battery[(j)].foe) != null))
                                removeFoeForced(bg.battery[(j)].foe);
                        }
                    }
                }
            }
        }
        else if (boss.patternCnt < BOSS_PATTERN_CHANGE_CNT)
        {
            if (boss.patternCnt <= GameMath.integer(BOSS_PATTERN_CHANGE_CNT / 2))
            {
                if ((boss.patternCnt == GameMath.integer(BOSS_PATTERN_CHANGE_CNT / 2)) && (boss.state == ATTACKING))
                {
                    boss.patternIdx++;
                    if (boss.patternIdx >= boss.patternNum)
                        boss.patternIdx = 1;
                }

                dfsChg = 6;
            }
            else
            {
                dfsChg = -6;
            }

            if (boss.topAttack[(boss.patternIdx)].barrageType != NOT_EXIST)
            {
                bossShape.diffuse = bossShape.diffuse + (dfsChg);
                if (bossShape.diffuse < 0)
                    bossShape.diffuse = 0;
                else if (bossShape.diffuse > 255)
                    bossShape.diffuse = 255;
            }

            {
                i = 0;
                for (; i < boss.batteryGroupNum; i++)
                {
                    BatteryGroup bg = (boss.batteryGroup[(i)]);
                    if (bg.attack[(boss.patternIdx)].barrageType != NOT_EXIST)
                    {
                        bossShape.tree[(i)].diffuse = bossShape.tree[(i)].diffuse + (dfsChg);
                        if (bossShape.tree[(i)].diffuse < 0)
                            bossShape.tree[(i)].diffuse = 0;
                        else if (bossShape.tree[(i)].diffuse > 255)
                            bossShape.tree[(i)].diffuse = 255;
                    }
                }
            }
        }

        if (boss.state >= DESTROIED_END)
        {
            boss.d = 0;
            boss.x = boss.x + (GameMath.signedShift((BOSS_INITIAL_X - boss.x), 6));
            boss.y = boss.y + (GameMath.signedShift((BOSS_INITIAL_Y - boss.y), 6));
        }
        else
        {
            ax = boss.mpx[(boss.mpIdx)];
            ay = boss.mpy[(boss.mpIdx)];
            d = getDeg(ax - boss.x, ay - boss.y);
            od = d - boss.d;
            if (od > 512)
                od = od - (1024);
            if (od < -512)
                od = od + (1024);
            aod = absN(od);
            if (!(((boss.onRoute) != 0)))
            {
                if (aod < 256)
                {
                    boss.onRoute = 1;
                }
            }
            else
            {
                if (aod > 256)
                {
                    boss.onRoute = 0;
                    boss.mpIdx++;
                    if (boss.mpIdx >= boss.mpNum)
                        boss.mpIdx = 0;
                }
            }

            emd = boss.md;
            if (aod < emd)
            {
                boss.d = d;
            }
            else if (od > 0)
            {
                boss.d = boss.d + (emd);
            }
            else
            {
                boss.d = boss.d - (emd);
            }

            boss.d = boss.d & (1023);
            boss.x = boss.x + (GameMath.signedShift((sctbl[(boss.d)] * boss.speed), 8));
            boss.y = boss.y - (GameMath.signedShift((sctbl[(boss.d + 256)] * boss.speed), 8));
        }

        if (boss.y < GameMath.integer(-FIELD_HEIGHT_8 / 2))
        {
            boss.y = GameMath.integer(-FIELD_HEIGHT_8 / 2);
        }

        lmtOn = 1;
        if (((boss.topBattery.foe) != null))
        {
            boss.topBattery.foe.pos.x = boss.x;
            boss.topBattery.foe.pos.y = boss.y;
        }

        if (boss.topAttack[(boss.patternIdx)].barrageType != NOT_EXIST)
        {
            lmtOn = lmtOn & (handleLimiter((boss.topLimiter), allLmtOn));
        }

        {
            i = 0;
            for (; i < boss.batteryGroupNum; i++)
            {
                BatteryGroup bg = (boss.batteryGroup[(i)]);
                if (bg.attack[(boss.patternIdx)].barrageType != NOT_EXIST)
                {
                    lmtOn = lmtOn & (handleLimiter((bg.limiter), allLmtOn));
                }

                {
                    j = 0;
                    for (; j < bg.batteryNum; j++)
                    {
                        Battery bt = (bg.battery[(j)]);
                        if (((bt.foe) != null))
                        {
                            bt.foe.pos.x = boss.x + bt.x;
                            bt.foe.pos.y = boss.y + bt.y;
                        }
                    }
                }
            }
        }

        if (((lmtOn) != 0))
        {
            allLmtOn = 1;
        }
        else
        {
            allLmtOn = 0;
        }

        boss.r = bossShape.r;
        boss.g = bossShape.g;
        boss.b = bossShape.b;
        if (((boss.damaged) != 0))
        {
            if ((boss.damageCnt & 1) == 0)
            {
                boss.r = GameMath.integer(bossShape.r / 2);
                boss.g = 255;
                boss.b = GameMath.integer(bossShape.b / 2);
                if ((boss.damageCnt & 31) == 0)
                {
                    playChunk(2);
                }
            }

            boss.damageCnt++;
            switch (mode)
            {
                case NORMAL_MODE:
                    if ((boss.cnt & 7) == 0)
                    {
                        addScore(GameMath.integer(bonusScore / 10) * 10);
                    }

                    break;
                case PSY_MODE:
                case IKA_MODE:
                case GW_MODE:
                    if ((boss.cnt & 15) == 0)
                    {
                        addScore(GameMath.integer(bonusScore / 10) * 10);
                    }

                    break;
            }
        }
        else
        {
            if (boss.damageCnt > 0)
            {
                boss.damageCnt = 0;
                haltChunk(2);
            }

            bonusScore = bonusScore - (10);
            if (bonusScore < 10)
                bonusScore = 10;
        }

        boss.damaged = 0;
        boss.cnt++;
        boss.stateCnt--;
        switch (boss.state)
        {
            case CREATING:
                if (boss.stateCnt <= 0)
                {
                    boss.state = ATTACKING;
                }

                break;
            case CHANGE:
                if (boss.stateCnt <= 0)
                {
                    boss.state = LAST_ATTACK;
                }

                break;
            case DESTROIED:
                if (randN(7) == 0)
                    addBossTreeFragPart();
                if (randN(15) == 0)
                    playChunk(6);
            {
                i = 0;
                for (; i < boss.batteryGroupNum; i++)
                {
                    BossTree bt = (bossShape.tree[(i)]);
                    {
                        j = 0;
                        for (; j < bt.posNum; j++)
                        {
                            bt.wing[(j)].size = bt.wing[(j)].size * (0.99f);
                        }
                    }

                    {
                        j = 0;
                        for (; j < boss.batteryGroup[(i)].batteryNum; j++)
                        {
                            bt.eWing[(j)].size = bt.eWing[(j)].size * (0.985f);
                        }
                    }
                }
            }

                if (boss.stateCnt <= 0)
                {
                    int bs = 0;
                    {
                        i = 0;
                        for (; i < boss.batteryGroupNum; i++)
                        {
                            BossTree bt = (bossShape.tree[(i)]);
                            {
                                j = 0;
                                for (; j < 16; j++)
                                    addBossTreeFrag(bt);
                            }
                        }
                    }

                    boss.stateCnt = 999999;
                    boss.state = DESTROIED_END;
                    playChunk(4);
                    setScreenShake(1, BOSS_PATTERN_CHANGE_CNT);
                    switch (mode)
                    {
                        case NORMAL_MODE:
                            bs = 7000;
                            break;
                        case PSY_MODE:
                        case IKA_MODE:
                        case GW_MODE:
                            bs = 5000;
                            break;
                    }

                    bs = GameMath.integer((bossDstBaseTime * 3 - bossTimer) * 5000 / (bossDstBaseTime * 2));
                    if ((bs < 0) || (bossTimer >= BOSS_TIME_UP))
                        bs = 0;
                    bs = bs * (100);
                    initBossScoreAtr(bs);
                }

                break;
            case DESTROIED_END:
                moveBossScoreAtr();
                break;
        }

        if ((boss.state == ATTACKING) || (boss.state == LAST_ATTACK))
        {
            bossTimer = bossTimer + (BOSS_TIMER_COUNT_UP);
            if ((bossTimer >= BOSS_TIME_UP) && (status == IN_GAME))
            {
                bossTimer = BOSS_TIME_UP;
                destroyBoss();
            }
        }
    }

    public static void damageBoss(int dmg)
    {
        int tn = 0, pn = 0, bn = 0;
        int i = 0, j = 0;
        BossTree bt = null;
        tn = randN(TREE_MAX_LENGTH);
        if (tn < boss.batteryGroupNum)
        {
            bt = (bossShape.tree[(tn)]);
            pn = randN(bt.posNum);
            bt.wing[(pn)].size = bt.wing[(pn)].size * (0.996f);
            bn = randN(boss.batteryGroup[(tn)].batteryNum);
            bt.eWing[(bn)].size = bt.eWing[(bn)].size * (0.996f);
        }

        boss.shield = boss.shield - (dmg);
        boss.damaged = 1;
        switch (boss.state)
        {
            case ATTACKING:
                if (boss.shield <= boss.patternChangeShield)
                {
                    boss.shield = boss.patternChangeShield;
                    {
                        i = 0;
                        for (; i < boss.batteryGroupNum; i++)
                        {
                            BossTree brokenTree = (bossShape.tree[(i)]);
                            {
                                j = 0;
                                for (; j < 4; j++)
                                    addBossTreeFrag(brokenTree);
                            }

                            {
                                j = 0;
                                for (; j < brokenTree.posNum; j++)
                                {
                                    brokenTree.wing[(j)].size = 0;
                                }
                            }

                            {
                                j = 0;
                                for (; j < boss.batteryGroup[(i)].batteryNum; j++)
                                {
                                    brokenTree.eWing[(j)].size = 0;
                                }
                            }
                        }
                    }

                    boss.state = CHANGE;
                    boss.patternCnt = BOSS_PATTERN_CHANGE_CNT + 1;
                    boss.stateCnt = BOSS_PATTERN_CHANGE_CNT;
                    boss.patternIdx = 0;
                    clearFoes();
                    playChunk(5);
                }

                break;
            case LAST_ATTACK:
                if (boss.shield <= 0)
                {
                    {
                        i = 0;
                        for (; i < boss.batteryGroupNum; i++)
                        {
                            BossTree brokenTree = (bossShape.tree[(i)]);
                            {
                                j = 0;
                                for (; j < 2; j++)
                                    addBossTreeFrag(brokenTree);
                            }
                        }
                    }

                    destroyBoss();
                }

                break;
        }
    }

    public static void damageBossLaser(int cnt)
    {
        if ((mode == NORMAL_MODE) || (mode == PSY_MODE))
        {
            damageBoss(40 - cnt);
        }
        else
        {
            damageBoss(GameMath.integer((40 - cnt) * 2 / 3));
        }

        bonusScore = bonusScore + (GameMath.integer((40 - cnt) / 8));
        if (bonusScore > 1000)
            bonusScore = 1000;
        ship.grzCnt++;
    }

    public static int checkHitDownside(int x)
    {
        int i = 0;
        int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
        if ((boss.state != ATTACKING) && (boss.state != LAST_ATTACK))
            return -999999;
        {
            i = 0;
            for (; i < 4; i++)
            {
                x1 = boss.collisionX[(i)] + boss.x;
                x2 = boss.collisionX[(i + 1)] + boss.x;
                if ((x1 <= x) && (x < x2))
                {
                    y1 = boss.collisionY[(i)] + boss.y;
                    y2 = boss.collisionY[(i + 1)] + boss.y;
                    return GameMath.integer((y2 - y1) * (x - x1) / (x2 - x1)) + y1;
                }
            }
        }

        return -999999;
    }

    public static int checkHitUpside()
    {
        return boss.y + boss.collisionYUp;
    }

    public static void drawBossWing(float[] model, Gfx.Blend blend, string key, float x1, float y1, float z1, float x2, float y2, float z2, BossWing wg)
    {
        int i = 0;
        float sz = wg.size;
        {
            i = 0;
            for (; i < wg.wingNum; i++)
            {
                drawSquare(model, blend, key + "-147" + "-" + i.ToString(), x2, y2, z2, x1, y1, z1, x1 + wg.x[(i)][(0)] * sz, y1 + wg.y[(i)][(0)] * sz, z1 + wg.z[(i)][(0)] * sz, x2 + wg.x[(i)][(1)] * sz, y2 + wg.y[(i)][(1)] * sz, z2 + wg.z[(i)][(1)] * sz, boss.r, boss.g, boss.b);
            }
        }
    }

    public static void drawBoss(float[] model, Gfx.Blend blend, string key)
    {
        float x = 0, y = 0;
        float x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;
        int i = 0, j = 0;
        int df = 0;
        int crBpn = 0, crBpl = 0;
        int bpn = 0;
        {
            crBpl = 0;
            crBpn = crBpl;
        }

        x = (float)boss.x / FIELD_SCREEN_RATIO;
        y = -(float)boss.y / FIELD_SCREEN_RATIO;
        if ((bossShape.diffuse > 0) && (boss.state < DESTROIED))
        {
            df = bossShape.diffuse;
            drawStar(model, blend, key + "-483" + "-" + i.ToString() + "-" + j.ToString(), 1, x, y, 0, df, df, df, (float)(df + 256) / 500.0f);
            drawStar(model, blend, key + "-557" + "-" + i.ToString() + "-" + j.ToString(), 1, x, y, 0, df, df, df, (float)(df + randN(256)) / 500.0f);
        }

        {
            i = 0;
            for (; i < boss.batteryGroupNum; i++)
            {
                BossTree bt = (bossShape.tree[(i)]);
                bpn = bt.posNum - 1;
                x1 = x;
                y1 = y;
                z1 = 0;
                switch (boss.state)
                {
                    case CREATING:
                    case CHANGE:
                        crBpn = GameMath.integer((bpn + 1) * (BOSS_PATTERN_CHANGE_CNT - boss.stateCnt - 1) / BOSS_PATTERN_CHANGE_CNT);
                        crBpl = 255 - GameMath.integer((boss.stateCnt % (GameMath.integer(BOSS_PATTERN_CHANGE_CNT / (bpn + 1))) * 256) / (GameMath.integer(BOSS_PATTERN_CHANGE_CNT / (bpn + 1))));
                        break;
                }

                {
                    j = 0;
                    for (; j < bpn; j++)
                    {
                        x2 = x + bt.x[(j + 1)];
                        y2 = y - bt.y[(j + 1)];
                        z2 = bt.z[(j + 1)];
                        switch (boss.state)
                        {
                            case ATTACKING:
                            case LAST_ATTACK:
                            case DESTROIED:
                                drawLine(model, blend, key + "-2018" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 240);
                                drawBossWing(model, blend, key + "-1988" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.wing[(j)]));
                                break;
                            case CREATING:
                                if (j == crBpn)
                                {
                                    drawLinePart(model, blend, key + "-2313" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 240, crBpl);
                                }
                                else if (j < crBpn)
                                {
                                    drawLine(model, blend, key + "-2769" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 240);
                                }

                                if (crBpn == bpn)
                                {
                                    bt.wing[(j)].size = (float)crBpl / 255;
                                    drawBossWing(model, blend, key + "-2796" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.wing[(j)]));
                                }

                                break;
                            case CHANGE:
                                drawLine(model, blend, key + "-3350" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 240);
                                if (crBpn == bpn)
                                {
                                    bt.wing[(j)].size = (float)crBpl / 128;
                                    drawBossWing(model, blend, key + "-3271" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.wing[(j)]));
                                }

                                break;
                        }

                        if (((bt.diffuse > 0) && (boss.state != CHANGE)) && (boss.state < DESTROIED))
                        {
                            df = bt.diffuse;
                            drawStar(model, blend, key + "-3911" + "-" + i.ToString() + "-" + j.ToString(), 0, x2, y2, z2, df, df, df, (float)(df + 256) / 900.0f);
                            drawStar(model, blend, key + "-4004" + "-" + i.ToString() + "-" + j.ToString(), 0, x2, y2, z2, df, df, df, (float)(df + randN(256)) / 900.0f);
                        }

                        x1 = x2;
                        y1 = y2;
                        z1 = z2;
                    }
                }

                x1 = x + bt.x[(bpn)];
                y1 = y - bt.y[(bpn)];
                z1 = bt.z[(bpn)];
                {
                    j = 0;
                    for (; j < bt.epNum; j++)
                    {
                        x2 = x + bt.ex[(j)];
                        y2 = y - bt.ey[(j)];
                        z2 = bt.ez[(j)];
                        switch (boss.state)
                        {
                            case ATTACKING:
                            case LAST_ATTACK:
                            case DESTROIED:
                                drawLine(model, blend, key + "-5115" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 220);
                                drawBossWing(model, blend, key + "-4659" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.eWing[(j)]));
                                break;
                            case CREATING:
                                if (crBpn == bpn)
                                {
                                    drawLinePart(model, blend, key + "-5200" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 220, crBpl);
                                    bt.eWing[(j)].size = (float)crBpl / 255;
                                    drawBossWing(model, blend, key + "-5118" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.eWing[(j)]));
                                }

                                break;
                            case CHANGE:
                                drawLine(model, blend, key + "-6099" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, bossShape.r, bossShape.g, bossShape.b, 220);
                                if (crBpn == bpn)
                                {
                                    bt.eWing[(j)].size = (float)crBpl / 128;
                                    drawBossWing(model, blend, key + "-5595" + "-" + i.ToString() + "-" + j.ToString(), x1, y1, z1, x2, y2, z2, (bt.eWing[(j)]));
                                }

                                break;
                        }

                        if (((bt.diffuse > 0) && (boss.state != CHANGE)) && (boss.state < DESTROIED))
                        {
                            df = bt.diffuse;
                            drawStar(model, blend, key + "-6520" + "-" + i.ToString() + "-" + j.ToString(), 1, x2, y2, z2, df, df, df, (float)(df + 256) / 640.0f);
                            drawStar(model, blend, key + "-6613" + "-" + i.ToString() + "-" + j.ToString(), 1, x2, y2, z2, df, df, df, (float)(df + randN(256)) / 640.0f);
                        }
                    }
                }
            }
        }

        drawCore(model, blend, key + "-6784" + "-" + i.ToString() + "-" + j.ToString(), x, y, boss.cnt, boss.r, boss.g, boss.b);
    }

    public static void drawBossState(float[] model, Gfx.Blend blend, string key)
    {
        int wd = 0, cwd = 0;
        if (boss.state >= ATTACKING)
        {
            if ((boss.state < DESTROIED) || ((boss.cnt & 31) < 16))
            {
                drawTimeCenter(model, blend, key + "-175", bossTimer, 470, 44, 10, 210, 240, 210);
            }

            if (boss.state == DESTROIED_END)
            {
                drawBossScoreAtr(model, blend, key + "-320");
            }
        }

        if (boss.state >= DESTROIED)
            return;
        if (boss.state == CREATING)
        {
            wd = GameMath.integer(GameMath.integer(boss.shield * 300 / BOSS_SHIELD_MAX) * (BOSS_PATTERN_CHANGE_CNT - boss.stateCnt) / BOSS_PATTERN_CHANGE_CNT);
        }
        else
        {
            wd = GameMath.integer(boss.shield * 300 / BOSS_SHIELD_MAX);
        }

        drawBox(model, blend, key + "-808", 180 + GameMath.integer(wd / 2), 24, GameMath.integer(wd / 2), 6, 240, 240, 210);
        drawNumCenter(model, blend, key + "-905", boss.shield, 176 + wd, 10, 6, 210, 210, 240);
        cwd = GameMath.integer(boss.patternChangeShield * 300 / BOSS_SHIELD_MAX);
        if (wd > cwd)
        {
            drawNumCenter(model, blend, key + "-1091", boss.patternChangeShield, 176 + cwd, 10, 6, 240, 210, 210);
        }
    }
}
