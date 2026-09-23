// Copyright 2002 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static NrCore;
using static NrAttract;
using static NrShip;
using static NrShot;
using static NrFrag;
using static NrBonus;
using static NrBackground;
using static NrFoe;
using static NrBarrage;
using static NrLetter;
using static NrConstants;
using static NrArrays;
using static NrRandom;
using static NrScreen;
using static NrSound;
using static NrPreference;
using static NrAngles;
using static NrVector;

public static class NrBarrage
{
    public static Barrage[][] barragePattern = Make(BARRAGE_TYPE_NUM, () => Make(BARRAGE_PATTERN_MAX, () => new Barrage()));
    public static Barrage[][] barrageQueue = Make(BARRAGE_TYPE_NUM, () => Make(BARRAGE_PATTERN_MAX, () => (Barrage)null));
    public static int[] barragePatternNum = Make(BARRAGE_TYPE_NUM, () => 0);
    public static Barrage[] barrage = Make(BARRAGE_MAX, () => (Barrage)null);
    public static int rnd;
    public static int scene;
    public static int endless, insane;
    public static int sceneCnt;
    public static float level, levelInc;
    public static void initBarrages(int seed, float startLevel, float li)
    {
        int n1 = 0, n2 = 0, rn = 0;
        for (int i = 0; i < BARRAGE_TYPE_NUM; i++)
        {
            for (int j = 0; j < barragePatternNum[(i)]; j++)
            {
                barrageQueue[(i)][(j)] = (barragePattern[(i)][(j)]);
            }
        }

        processSpeedDownBulletsNum = DEFAULT_SPEED_DOWN_BULLETS_NUM;
        if (seed >= 0)
        {
            rnd = seed;
            endless = 0;
            insane = 0;
        }
        else
        {
            rnd = NrRandom.visualSeed;
            endless = 1;
            if (seed == -2)
                insane = 1;
            else
                insane = 0;
            if (seed == -3)
                processSpeedDownBulletsNum = EASY_SPEED_DOWN_BULLETS_NUM;
            else if (seed == -4)
                processSpeedDownBulletsNum = HARD_SPEED_DOWN_BULLETS_NUM;
        }

        for (int i = 0; i < BARRAGE_TYPE_NUM; i++)
        {
            int bn = barragePatternNum[(i)];
            rn = 60 + NrBarrage.nextMod(4);
            for (int j = 0; j < rn; j++)
            {
                n1 = NrBarrage.nextMod(bn);
                n2 = NrBarrage.nextMod(bn);
                Barrage tb = barrageQueue[(i)][(n1)];
                barrageQueue[(i)][(n1)] = barrageQueue[(i)][(n2)];
                barrageQueue[(i)][(n2)] = tb;
            }

            for (int j = 0; j < bn; j++)
            {
                barrageQueue[(i)][(j)].maxRank = new PatternNumber((float)(NrBarrage.nextMod(70)) / 100 + 0.3f,0);
                barrageQueue[(i)][(j)].frq = 1;
            }
        }

        scene = -1;
        sceneCnt = 0;
        level = startLevel;
        levelInc = li;
    }

    public static void rollBarragePattern(Barrage[] br, int brNum)
    {
        Barrage tbr = null;
        int n = GameMath.integer(((float)brNum / ((float)(NrBarrage.nextMod(32)) / 32 + 1) + 0.5f));
        if (n == 0)
            return;
        if (n > brNum)
            n = brNum;
        tbr = br[(0)];
        for (int i = 0; i < n - 1; i++)
        {
            br[(i)] = br[(i + 1)];
        }

        br[(n - 1)] = tbr;
        br[(0)].maxRank = PatternNumber.Add(br[(0)].maxRank,br[(0)].maxRank);
        while (below(1,br[(0)].maxRank))
            br[(0)].maxRank = PatternNumber.Subtract(br[(0)].maxRank,new PatternNumber(0.7f,0));
    }

    static bool below(float value,PatternNumber rank){
        var difference=PatternNumber.Subtract(new PatternNumber(value,0),rank);
        return difference.High<0||(difference.High==0&&difference.Low<0);
    }

    public static int barrageNum;
    public static int bossMode;
    public static int pax;
    public static int pay;
    public static int quickAppType;
    public static void setBarrages(float level, int bm, int midMode)
    {
        int bpn = 0, bn = 0, i = 0;
        int barrageMax = 0, addFrqLoop = 0;
        barrageNum = 0;
        barrageMax = NrBarrage.nextMod(3) + 4;
        bossMode = bm;
        if (((!(((midMode) != 0)))))
        {
            bpn = 0;
        }
        else
        {
            bpn = 1;
        }

        quickAppType = bpn;
        {
            bn = 0;
            for (;; bn++)
            {
                if ((bn == 0) && (level < 0))
                    break;
                if (((bossMode) != 0))
                {
                    if (bn == 0)
                        bpn = 0;
                    else
                        bpn = 2;
                    if (bn >= BARRAGE_MAX)
                        break;
                }
                else
                {
                    if (bn >= barrageMax)
                    {
                        bn = 0;
                        addFrqLoop = 1;
                    }
                }

                if (((addFrqLoop) != 0))
                {
                    barrage[(bn)].frq++;
                    level = PatternNumber.Subtract(new PatternNumber(level,0),PatternNumber.Add(new PatternNumber(1,0),barrage[(bn)].exactRank)).High;
                    if (level < 0)
                        break;
                }
                else
                {
                    barrageNum++;
                    rollBarragePattern(barrageQueue[(bpn)], barragePatternNum[(bpn)]);
                    barrage[(bn)] = barrageQueue[(bpn)][(0)];
                    barrage[(bn)].frq = 1;
                    if (below(level,barrageQueue[(bpn)][(0)].maxRank))
                    {
                        if (level < 0)
                            level = 0;
                        barrage[(bn)].rank = level;
                        barrage[(bn)].exactRank=new PatternNumber(level,0);
                        if ((((((!(((bossMode) != 0))))) || (bn > 0))))
                            break;
                    }

                    barrage[(bn)].exactRank = barrageQueue[(bpn)][(0)].maxRank;
                    barrage[(bn)].rank = barrage[(bn)].exactRank.High;
                    if (((!(((bossMode) != 0)))))
                    {
                        level = PatternNumber.Subtract(new PatternNumber(level,0),PatternNumber.Add(new PatternNumber(1,0),barrageQueue[(bpn)][(0)].maxRank)).High;
                    }
                    else
                    {
                        if (bn > 0)
                            level = PatternNumber.Subtract(new PatternNumber(level,0),PatternNumber.Add(new PatternNumber(4,0),PatternNumber.Multiply(barrageQueue[(bpn)][(0)].maxRank,new PatternNumber(6,0)))).High;
                    }

                    bpn++;
                    if (bpn >= BARRAGE_TYPE_NUM)
                    {
                        if (((!(((midMode) != 0)))))
                        {
                            bpn = 0;
                        }
                        else
                        {
                            bpn = 1;
                        }
                    }
                }
            }
        }

        pax = (NrBarrage.nextMod((SCAN_WIDTH_8 * 2 / 3)) + (SCAN_WIDTH_8 / 6));
        pay = (NrBarrage.nextMod((SCAN_HEIGHT_8 / 6)) + (SCAN_HEIGHT_8 / 10));
        scene++;
    }

    public static int zakoAppCnt;
    public static int[] appFreq = new int[]
    {
        90,
        360,
        800
    };
    public static int[] shield = new int[]
    {
        3,
        6,
        9
    };
    public static Foe bossBullet;
    public static void addBullets()
    {
        int x = 0, y = 0, i = 0;
        int type = 0, frq = 0;
        sceneCnt--;
        if (sceneCnt < 0)
        {
            if (((!(((insane) != 0)))))
                clearFoes();
            if ((((scene >= 0) && (((!(((endless) != 0))))))))
                setClearScore();
            if (scene % 10 == 8)
            {
                sceneCnt = 999999;
                zakoAppCnt = ZAKO_APP_TERM;
                setBarrages(level, (true ? 1 : 0), (false ? 1 : 0));
                addBossBullet();
            }
            else
            {
                sceneCnt = SCENE_TERM;
                if (scene % 10 == 3)
                {
                    setBarrages(level, (false ? 1 : 0), (true ? 1 : 0));
                }
                else
                {
                    setBarrages(level, (false ? 1 : 0), (false ? 1 : 0));
                }
            }

            level = level + (levelInc);
            if (status == IN_GAME)
            {
                drawRPanel();
            }
            else
            {
                sceneCnt = 999999;
            }
        }

        if (sceneCnt < SCENE_END_TERM)
            return;
        {
            i = 0;
            for (; i < barrageNum; i++)
            {
                if (((bossMode) != 0))
                {
                    if (i > 0)
                        break;
                    if (zakoAppCnt <= 0)
                        break;
                    zakoAppCnt--;
                }

                type = barrage[(i)].type;
                if ((type == quickAppType) && (enNum[(type)] == 0))
                {
                    x = pax;
                    y = pay;
                    addFoe(x, y, barrage[(i)].rank, 512, 0, type, shield[(type)], barrage[(i)].bulletml);
                }

                frq = appFreq[(type)] / barrage[(i)].frq;
                if (frq < 2)
                    frq = 2;
                if ((NrBarrage.nextMod(frq)) == 0)
                {
                    x = NrBarrage.nextMod((SCAN_WIDTH_8 * 2 / 3)) + (SCAN_WIDTH_8 / 6);
                    y = NrBarrage.nextMod((SCAN_HEIGHT_8 / 6)) + (SCAN_HEIGHT_8 / 10);
                    if (type == quickAppType)
                    {
                        pax = x;
                        pay = y;
                    }

                    addFoe(x, y, barrage[(i)].rank, 512, 0, type, shield[(type)], barrage[(i)].bulletml);
                }
            }
        }
    }

    public static void bossDestroied()
    {
        if (((!(((endless) != 0)))))
        {
            setClearScore();
            addLeftBonus();
            initStageClear();
        }

        clearFoes();
        sceneCnt = 180;
        zakoAppCnt = 0;
    }

    public static void addBossBullet()
    {
        Foe bl = null;
        bossBullet = null;
        for (int i = 0; i < barrageNum; i++)
        {
            if (barrage[(i)].type != 2)
                continue;
            if (bossBullet == null)
            {
                bl = addFoe(SCAN_WIDTH_8 / 2, SCAN_HEIGHT_8 / 5, barrage[(i)].rank, 512, 0, BOSS_TYPE, BOSS_SHIELD, barrage[(i)].bulletml);
                bossBullet = bl;
            }
            else
            {
                bl = addFoeBossActiveBullet(SCAN_WIDTH_8 / 2, SCAN_HEIGHT_8 / 5, barrage[(i)].rank, 512, 0, barrage[(i)].bulletml);
            }
        }
    }

    public static int nextRand()
    {
        rnd = rnd * 8513 + 179;
        return rnd;
    }

    public static int nextMod(int n)
    {
        int bits = nextRand();
        return ((((GameMath.signedShift(bits, 1)) & 2147483647) % n) * 2 + (bits & 1)) % n;
    }

    public static void initBarragemanager()
    {
        for (int i = 0; i < 3; i++)
        {
            barragePatternNum[(i)] = NrData.barrages[(i)].Length;
            for (int j = 0; j < barragePatternNum[(i)]; j++)
            {
                barragePattern[(i)][(j)].bulletml = NrData.barrages[(i)][(j)];
                barragePattern[(i)][(j)].type = i;
            }
        }
    }
}
