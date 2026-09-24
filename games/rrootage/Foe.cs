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

public static class RrFoe
{
    public static Foe[] foe = Make(FOE_MAX, () => new Foe());
    public static void removeFoeCommand(Foe fe)
    {
        if (fe.spc == BATTERY)
            return;
        fe.spc = NOT_EXIST_TMP;
    }

    public static void removeFoeForced(Foe fe)
    {
        fe.spc = NOT_EXIST;
        if (((fe.cmd) != null))
        {
            fe.cmd = null;
        }
    }

    public static void removeFoe(Foe fe)
    {
        if (fe.spc == BATTERY)
            return;
        removeFoeForced(fe);
    }

    public static void initFoes()
    {
        int i = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                removeFoeForced((foe[(i)]));
            }
        }
    }

    public static void closeFoes()
    {
        int i = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (((foe[(i)].cmd) != null))
                    foe[(i)].cmd = null;
            }
        }
    }

    public static int foeIdx = FOE_MAX;
    public static Foe getNextFoe()
    {
        int i = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                foeIdx--;
                if (foeIdx < 0)
                    foeIdx = FOE_MAX - 1;
                if (foe[(foeIdx)].spc == NOT_EXIST)
                    break;
            }
        }

        if (i >= FOE_MAX)
            return null;
        return (foe[(foeIdx)]);
    }

    public static Foe addFoe(Foe foe, int d, int spd, int color)
    {
        Foe fe = null;
        if ((((foe.limiter.on) != 0)) && (foe.spc == BATTERY))
            return null;
        fe = getNextFoe();
        if (!(((fe) != null)))
            return null;
        fe.copyFrom(foe);
        fe.spos = new Vector
        {
            x = fe.pos.x,
            y = fe.pos.y
        };
        fe.ppos = new Vector
        {
            x = fe.pos.x,
            y = fe.pos.y
        };
        {
            fe.vel.y = 0;
            fe.vel.x = fe.vel.y;
        }

        {
            fe.mv.y = 0;
            fe.mv.x = fe.mv.y;
        }

        fe.cnt = 0;
        fe.slowMvCnt = 0;
        fe.d = d;
        fe.spd = spd;
        fe.grzRng = 0;
        fe.color = color;
        fe.shapeType = 2;
        if (mode == IKA_MODE)
        {
            switch (foe.ikaType)
            {
                case IKA_FIX:
                case IKA_ALTERNATE:
                    fe.color--;
                    break;
                case IKA_HALF:
                    if (((fe.fireCnt & 1) != 0))
                        fe.color--;
                    break;
                case IKA_ALTERNATE_SHOT:
                    break;
            }

            fe.color = fe.color & (1);
        }

        fe.limiter.cnt++;
        fe.spc = BULLET;
        return fe;
    }

    public static Foe addFoeBattery(int x, int y, float rank, int d, int spd, int xReverse, int[] morphParser, int morphCnt, int morphHalf, float morphRank, float speedRank, int color, int[] bulletShape, float[] bulletSize, Limiter limiter, int ikaType, int parser)
    {
        Foe foe = new Foe();
        int i = 0;
        foe.pos.x = x;
        foe.pos.y = y;
        foe.rank = rank;
        foe.xReverse = xReverse;
        {
            i = 0;
            for (; i < MORPH_PATTERN_MAX; i++)
            {
                foe.morphParser[(i)] = morphParser[(i)];
            }
        }

        foe.morphCnt = morphCnt;
        foe.morphHalf = morphHalf;
        foe.morphRank = morphRank;
        foe.speedRank = speedRank;
        {
            i = 0;
            for (; i < BULLET_TYPE_NUM; i++)
            {
                foe.bulletShape[(i)] = bulletShape[(i)];
                foe.bulletSize[(i)] = bulletSize[(i)];
            }
        }

        foe.limiter = limiter;
        foe.ikaType = ikaType;
        foe.cntTotal = 0;
        Foe fe = addFoe(foe, d, spd, color);
        if (!(((fe) != null)))
            return null;
        fe.cmd = new FoeCommand(parser, fe);
        fe.spc = BATTERY;
        fe.parser = parser;
        fe.fireCnt = randN(2);
        return fe;
    }

    public static void addFoeActiveBullet(Foe foe, int d, int spd, int color, PatternState state)
    {
        Foe fe = null;
        fe = addFoe(foe, d, spd, color);
        if (!(((fe) != null)))
            return;
        fe.cmd = new FoeCommand(-1, fe, state);
        fe.spc = ACTIVE_BULLET;
        if (fe.morphCnt > 0)
            fe.shapeType = 0;
        else
            fe.shapeType = 1;
    }

    public static void addFoeNormalBullet(Foe foe, int d, int spd, int color)
    {
        Foe fe = null;
        if ((foe.morphCnt > 0) && (((!(((foe.morphHalf) != 0))) || ((((foe.fireCnt & 1)) != 0)))))
        {
            fe = addFoe(foe, d, spd, color);
            if (!(((fe) != null)))
                return;
            fe.morphCnt--;
            fe.morphRank = fe.morphRank * (0.77f);
            fe.cmd = new FoeCommand(foe.morphParser[(fe.morphCnt & (MORPH_PATTERN_MAX - 1))], fe);
            fe.spc = ACTIVE_BULLET;
            if (fe.morphCnt > 0)
                fe.shapeType = 0;
            else
                fe.shapeType = 1;
            return;
        }

        fe = addFoe(foe, d, spd, color);
        if (!(((fe) != null)))
            return;
        fe.cmd = null;
        fe.spc = BULLET;
        fe.shapeType = 2;
    }

    public static void clearFoeShape(Foe fe, int shape)
    {
        float x = 0, y = 0;
        int d = 0;
        if (fe.spc == BATTERY)
            return;
        x = (float)fe.pos.x / FIELD_SCREEN_RATIO;
        y = -(float)fe.pos.y / FIELD_SCREEN_RATIO;
        d = (fe.d * fe.xReverse) & 1023;
        if (((shape) != 0))
        {
            addShapeFrag(x, y, fe.bulletSize[(fe.shapeType)], d, fe.cnt, fe.bulletShape[(fe.shapeType)], fe.mv.x, fe.mv.y);
        }
        else
        {
            addShapeFrag(x, y, fe.bulletSize[(fe.shapeType)], d, fe.cnt, -1, fe.mv.x, fe.mv.y);
        }
    }

    public static int processSpeedDownBulletsNum = DEFAULT_SPEED_DOWN_BULLETS_NUM;
    public static int nowait = 0;
    public static void moveFoes()
    {
        int i = 0;
        Foe fe = null;
        int foeNum = 0;
        int mx = 0, my = 0;
        Vector bmv = new Vector(), sofs = new Vector();
        int hd = 0, inab = 0, inaa = 0;
        int sd = 0, sdx = 0, sdy = 0;
        Vector bossPos = getBossPos();
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (foe[(i)].spc == NOT_EXIST)
                    continue;
                fe = (foe[(i)]);
                if (((fe.cmd) != null))
                {
                    if (fe.spc == BATTERY)
                    {
                        if (fe.cmd.isEnd())
                        {
                            fe.cmd = null;
                            fe.cmd = new FoeCommand(fe.parser, fe);
                            if (mode == IKA_MODE)
                            {
                                if ((fe.ikaType == IKA_ALTERNATE) || (fe.ikaType == IKA_ALTERNATE_SHOT))
                                {
                                    fe.color = fe.color ^ (1);
                                }
                            }
                        }
                    }

                    fe.cmd.run();
                    if (fe.spc == NOT_EXIST_TMP)
                    {
                        removeFoeForced(fe);
                        continue;
                    }

                    if (((fe.cntTotal < LIMITER_VANISH_CNT) && (((fe.spc == ACTIVE_BULLET) || (fe.spc == BULLET)))) && (((fe.limiter.on) != 0)))
                    {
                        clearFoeShape(fe, 0);
                        removeFoe(fe);
                        continue;
                    }
                }

                fe.cnt++;
                fe.cntTotal++;
                fe.ppos = new Vector
                {
                    x = fe.pos.x,
                    y = fe.pos.y
                };
                mx = GameMath.integer((((GameMath.signedShift((sctbl[(fe.d)] * fe.spd), 8)) + fe.vel.x) * fe.speedRank));
                my = GameMath.integer(((-(GameMath.signedShift((sctbl[(fe.d + 256)] * fe.spd), 8)) + fe.vel.y) * fe.speedRank));
                fe.mv.x = (mx * fe.xReverse);
                fe.mv.y = my;
                fe.pos.x = fe.pos.x + (fe.mv.x);
                fe.pos.y = fe.pos.y + (my);
                if (fe.spc != BATTERY)
                {
                    if (absN(mx) + absN(my) < SLOW_MOVE)
                    {
                        fe.slowMvCnt++;
                        if (fe.slowMvCnt > SLOW_MOVE_VANISH_CNT)
                        {
                            removeFoe(fe);
                            continue;
                        }
                    }
                    else
                    {
                        fe.slowMvCnt = 0;
                    }
                }

                if ((fe.spc != BATTERY) && (status == IN_GAME))
                {
                    bmv = new Vector
                    {
                        x = fe.pos.x,
                        y = fe.pos.y
                    };
                    bmv.x = bmv.x - (fe.ppos.x);
                    bmv.y = bmv.y - (fe.ppos.y);
                    bmv.x = GameMath.signedShift(bmv.x, (2));
                    bmv.y = GameMath.signedShift(bmv.y, (2));
                    inaa = bmv.x * bmv.x + bmv.y * bmv.y;
                    if (inaa > 1)
                    {
                        sofs = new Vector
                        {
                            x = ship.pos.x,
                            y = ship.pos.y
                        };
                        sofs.x = sofs.x - (fe.ppos.x);
                        sofs.y = sofs.y - (fe.ppos.y);
                        sofs.x = GameMath.signedShift(sofs.x, (2));
                        sofs.y = GameMath.signedShift(sofs.y, (2));
                        inab = bmv.x * sofs.x + bmv.y * sofs.y;
                        if ((inab > 0) && (inab < inaa))
                        {
                            hd = sofs.x * sofs.x + sofs.y * sofs.y - GameMath.integer(GameMath.integer(inab * inab / inaa) / inaa);
                            if ((hd >= 0) && (hd < SHIP_HIT_WIDTH))
                            {
                                destroyShip();
                                removeFoe(fe);
                                continue;
                            }
                        }
                    }

                    switch (mode)
                    {
                        case PSY_MODE:
                        case IKA_MODE:
                            if ((mode == PSY_MODE) && (ship.invCnt <= 0))
                            {
                                sdx = fe.pos.x - ship.pos.x;
                                sdy = fe.pos.y - ship.pos.y;
                                sd = getDistance(sdx, sdy);
                                if (fe.grzRng > 0)
                                {
                                    fe.grzRng--;
                                }
                                else if (sd < ship.grzWdt)
                                {
                                    fe.grzRng = -1;
                                }
                                else if (fe.grzRng == -1)
                                {
                                    addGrazeFrag(ship.pos.x, ship.pos.y, sdx, sdy);
                                    if (ship.rollingCnt > 0)
                                    {
                                        ship.grzCnt = ship.grzCnt + (GRZ_METER_UP_ROLLING);
                                    }
                                    else
                                    {
                                        ship.grzCnt = ship.grzCnt + (GRZ_METER_UP);
                                    }

                                    ship.grzf = 1;
                                    fe.grzRng = 24;
                                    addScore(50);
                                }
                            }
                            else
                            {
                                sdx = fe.pos.x - ship.pos.x;
                                sdy = fe.pos.y - ship.pos.y;
                                sd = getDistance(sdx, sdy);
                                if ((sd < ship.fldWdt) && (fe.color == ship.color))
                                {
                                    addScore(100);
                                    ship.absEng++;
                                    removeFoe(fe);
                                    continue;
                                }
                            }

                            break;
                        case GW_MODE:
                            if (ship.rfCnt > 0)
                            {
                                sdx = fe.pos.x - ship.pos.x;
                                sdy = fe.pos.y - ship.pos.y;
                                sd = getDistance(sdx, sdy);
                                if (sd < ship.rfWdt)
                                {
                                    addScore(100);
                                    ship.reflects = 1;
                                    addShot(fe.pos.x, fe.pos.y, bossPos.x - fe.pos.x, bossPos.y - fe.pos.y, 2);
                                    removeFoe(fe);
                                    continue;
                                }
                            }

                            break;
                    }
                }

                if ((((fe.ppos.x < GameMath.integer(-FIELD_WIDTH_8 / 2)) || (fe.ppos.x >= GameMath.integer(FIELD_WIDTH_8 / 2))) || (fe.ppos.y < GameMath.integer(-FIELD_HEIGHT_8 / 2))) || (fe.ppos.y >= GameMath.integer(FIELD_HEIGHT_8 / 2)))
                {
                    removeFoe(fe);
                    continue;
                }

                foeNum++;
            }
        }

        interval = INTERVAL_BASE;
        if ((!(((nowait) != 0))) && (foeNum > processSpeedDownBulletsNum))
        {
            interval = interval + (GameMath.integer((foeNum - processSpeedDownBulletsNum) * INTERVAL_BASE / processSpeedDownBulletsNum));
            if (interval > INTERVAL_BASE * 2)
                interval = INTERVAL_BASE * 2;
        }
    }

    public static void clearFoes()
    {
        int i = 0;
        Foe fe = null;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (foe[(i)].spc == NOT_EXIST)
                    continue;
                fe = (foe[(i)]);
                clearFoeShape(fe, 0);
                removeFoeForced(fe);
            }
        }
    }

    public static void clearFoesZako()
    {
        int i = 0;
        Foe fe = null;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if ((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == BATTERY))
                    continue;
                fe = (foe[(i)]);
                clearFoeShape(fe, 0);
                removeFoeForced(fe);
            }
        }
    }

    public static void wipeBullets(Vector pos, int width)
    {
        int i = 0;
        Foe fe = null;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if ((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == BATTERY))
                    continue;
                fe = (foe[(i)]);
                if (vctDist(pos, (fe.pos)) < width)
                {
                    clearFoeShape(fe, 1);
                    addScore(10);
                    removeFoeForced(fe);
                }
            }
        }
    }

    public static void drawBulletsWake(float[] model, Gfx.Blend blend, string key)
    {
        int i = 0;
        Foe fe = null;
        float x = 0, y = 0, sx = 0, sy = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == BATTERY)) || (foe[(i)].cnt >= 64))
                    continue;
                fe = (foe[(i)]);
                x = (float)fe.pos.x / FIELD_SCREEN_RATIO;
                y = -(float)fe.pos.y / FIELD_SCREEN_RATIO;
                sx = (float)fe.spos.x / FIELD_SCREEN_RATIO;
                sy = -(float)fe.spos.y / FIELD_SCREEN_RATIO;
                drawLine(model, blend, key + "-588" + "-" + i.ToString(), x, y, 0, sx, sy, 0, 150, 180, 90, (63 - fe.cnt) * 3);
            }
        }
    }

    public static int[][] bulletColor = new int[][]
    {
        new int[]
        {
            180,
            100,
            50
        },
        new int[]
        {
            100,
            100,
            140
        },
        new int[]
        {
            150,
            100,
            120
        },
        new int[]
        {
            100,
            120,
            150
        },
    };
    public static void drawBullets(float[] model, Gfx.Blend blend, string key)
    {
        int i = 0;
        Foe fe = null;
        float x = 0, y = 0;
        int bc = 0;
        int d = 0, bt = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if ((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == BATTERY))
                    continue;
                fe = (foe[(i)]);
                x = (float)fe.pos.x / FIELD_SCREEN_RATIO;
                y = -(float)fe.pos.y / FIELD_SCREEN_RATIO;
                d = 1023 - getDeg(fe.pos.x - fe.ppos.x, fe.pos.y - fe.ppos.y);
                bt = fe.shapeType;
                if (mode == IKA_MODE)
                {
                    drawShapeIka(model, blend, key + "-646" + "-" + i.ToString(), x, y, fe.bulletSize[(bt)], d, fe.cnt & 1, fe.bulletShape[(bt)], fe.color);
                }
                else
                {
                    bc = fe.color % BULLET_COLOR_NUM;
                    drawShape(model, blend, key + "-914" + "-" + i.ToString(), x, y, fe.bulletSize[(bt)], d, fe.cnt, fe.bulletShape[(bt)], bulletColor[(bc)][(0)], bulletColor[(bc)][(1)], bulletColor[(bc)][(2)]);
                }
            }
        }
    }
}
