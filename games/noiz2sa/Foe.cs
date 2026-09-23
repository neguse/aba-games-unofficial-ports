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

public static class NrFoe
{
    public static Foe[] foe = Make(FOE_MAX, () => new Foe());
    public static int foeCnt;
    public static int[] enNum = Make(FOE_TYPE_MAX, () => 0);
    public static void removeFoeForcedNoDeleteCmd(Foe fe)
    {
        if (fe.spc == FOE)
        {
            foeCnt--;
            enNum[(fe.type)]--;
        }

        fe.spc = NOT_EXIST;
    }

    public static void removeFoeForced(Foe fe)
    {
        removeFoeForcedNoDeleteCmd(fe);
        if (((fe.cmd) != null))
        {
            fe.cmd = null;
        }
    }

    public static void removeFoe(Foe fe)
    {
        if (fe.type == BOSS_TYPE)
            return;
        removeFoeForcedNoDeleteCmd(fe);
    }

    public static void initFoes()
    {
        int i = 0, j = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                removeFoeForced((foe[(i)]));
            }
        }

        foeCnt = 0;
        {
            i = 0;
            for (; i < FOE_TYPE_MAX; i++)
            {
                enNum[(i)] = 0;
            }
        }
    }

    public static void closeFoes()
    {
        int i = 0, j = 0;
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
                if (foe[(i)].spc == NOT_EXIST)
                    break;
            }
        }

        if (i >= FOE_MAX)
            return null;
        return (foe[(i)]);
    }

    public static Foe addFoe(int x, int y, float rank, int d, int spd, int type, int shield, int parser)
    {
        int i = 0;
        Foe fe = getNextFoe();
        if (((!(((fe) != null)))))
            return null;
        fe.parser = parser;
        fe.cmd = new FoeCommand(parser, fe);
        fe.pos.x = x;
        fe.pos.y = y;
        {
            fe.ppos = new Vector
            {
                x = (fe.pos).x,
                y = (fe.pos).y
            };
            fe.spos = new Vector
            {
                x = (fe.ppos).x,
                y = (fe.ppos).y
            };
        }

        {
            fe.vel.y = 0;
            fe.vel.x = fe.vel.y;
        }

        fe.rank = rank;
        fe.d = d;
        fe.spd = spd;
        fe.spc = FOE;
        fe.type = type;
        fe.shield = shield;
        fe.cnt = 0;
        fe.color = 0;
        fe.hit = 0;
        foeCnt++;
        enNum[(type)]++;
        return fe;
    }

    public static Foe addFoeBossActiveBullet(int x, int y, float rank, int d, int spd, int parser)
    {
        Foe fe = addFoe(x, y, rank, d, spd, BOSS_TYPE, 0, parser);
        if (((!(((fe) != null)))))
            return null;
        foeCnt--;
        enNum[(BOSS_TYPE)]--;
        fe.spc = BOSS_ACTIVE_BULLET;
        return fe;
    }

    public static void addFoeActiveBullet(Vector pos, float rank, int d, int spd, int color, PatternState state)
    {
        Foe fe = getNextFoe();
        if (((!(((fe) != null)))))
            return;
        fe.cmd = new FoeCommand(-1, fe, state);
        {
            fe.pos = new Vector
            {
                x = (pos).x,
                y = (pos).y
            };
            fe.ppos = new Vector
            {
                x = (fe.pos).x,
                y = (fe.pos).y
            };
            fe.spos = new Vector
            {
                x = (fe.ppos).x,
                y = (fe.ppos).y
            };
        }

        {
            fe.vel.y = 0;
            fe.vel.x = fe.vel.y;
        }

        fe.rank = rank;
        fe.d = d;
        fe.spd = spd;
        fe.spc = ACTIVE_BULLET;
        fe.type = 0;
        fe.cnt = 0;
        fe.color = color;
    }

    public static void addFoeNormalBullet(Vector pos, float rank, int d, int spd, int color)
    {
        Foe fe = getNextFoe();
        if (((!(((fe) != null)))))
            return;
        fe.cmd = null;
        {
            fe.pos = new Vector
            {
                x = (pos).x,
                y = (pos).y
            };
            fe.ppos = new Vector
            {
                x = (fe.pos).x,
                y = (fe.pos).y
            };
            fe.spos = new Vector
            {
                x = (fe.ppos).x,
                y = (fe.ppos).y
            };
        }

        {
            fe.vel.y = 0;
            fe.vel.x = fe.vel.y;
        }

        fe.rank = rank;
        fe.d = d;
        fe.spd = spd;
        fe.spc = BULLET;
        fe.type = 0;
        fe.cnt = 0;
        fe.color = color;
    }

    public static void wipeBullets(Vector pos, int width)
    {
        int i = 0;
        Foe fe = null;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if ((foe[(i)].spc != ACTIVE_BULLET) && (foe[(i)].spc != BULLET))
                    continue;
                fe = (foe[(i)]);
                if (vctDist(pos, (fe.pos)) < width)
                {
                    addBonus((fe.pos), (fe.mv));
                    removeFoeForced(fe);
                }
            }
        }
    }

    public static int[] foeSize = new int[]
    {
        30,
        40,
        56,
        96
    };
    public static int[] foeScanSize = new int[]
    {
        foeSize[(0)] * 256 * SCAN_WIDTH / LAYER_WIDTH / 4 * 3,
        foeSize[(1)] * 256 * SCAN_WIDTH / LAYER_WIDTH / 4 * 3,
        foeSize[(2)] * 256 * SCAN_WIDTH / LAYER_WIDTH / 4 * 3,
        foeSize[(3)] * 256 * SCAN_WIDTH / LAYER_WIDTH / 4 * 3,
    };
    public static int[] enemyScore = new int[]
    {
        500,
        1000,
        5000,
        50000
    };
    public static int processSpeedDownBulletsNum = DEFAULT_SPEED_DOWN_BULLETS_NUM;
    public static int nowait = 0;
    public static void moveFoes()
    {
        int i = 0, j = 0;
        Foe fe = null;
        int foeNum = 0;
        int mx = 0, my = 0;
        int wl = 0;
        Vector bmv = new Vector(), sofs = new Vector();
        float ht = 0, hd = 0, inab = 0, inaa = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (foe[(i)].spc == NOT_EXIST)
                    continue;
                fe = (foe[(i)]);
                if (((fe.cmd) != null))
                {
                    if (fe.type == BOSS_TYPE)
                    {
                        if (fe.cmd.isEnd())
                        {
                            fe.cmd = null;
                            fe.cmd = new FoeCommand(fe.parser, fe);
                        }
                    }

                    fe.cmd.run();
                    if (fe.spc == NOT_EXIST)
                    {
                        if (((fe.cmd) != null))
                        {
                            fe.cmd = null;
                            fe.cmd = null;
                        }

                        continue;
                    }
                }

                mx = (GameMath.signedShift((sctbl[(fe.d)] * fe.spd), 8)) + fe.vel.x;
                my = -(GameMath.signedShift((sctbl[(fe.d + 256)] * fe.spd), 8)) + fe.vel.y;
                fe.pos.x = fe.pos.x + (mx);
                fe.pos.y = fe.pos.y + (my);
                fe.mv.x = mx;
                fe.mv.y = my;
                wl = 2;
                if (fe.cnt < 4)
                    wl = 0;
                else if (fe.cnt < 8)
                    wl = 1;
                fe.ppos.x = fe.pos.x - (mx << wl);
                fe.ppos.y = fe.pos.y - (my << wl);
                fe.cnt++;
                if (fe.spc == FOE)
                {
                    fe.hit = 0;
                    {
                        j = 0;
                        for (; j < SHOT_MAX; j++)
                        {
                            if (shot[(j)].cnt != NOT_EXIST)
                            {
                                if ((absN(fe.pos.x - shot[(j)].pos.x) < foeScanSize[(fe.type)]) && (absN(fe.pos.y - shot[(j)].pos.y) < foeScanSize[(fe.type)] + SHOT_SCAN_HEIGHT))
                                {
                                    shot[(j)].cnt = NOT_EXIST;
                                    fe.shield--;
                                    fe.hit = 1;
                                    addShotFrag(shot[(j)].pos);
                                    if (fe.shield <= 0)
                                    {
                                        addScore(enemyScore[(fe.type)]);
                                        wipeBullets((fe.pos), BULLET_WIPE_WIDTH * (fe.type + 1));
                                        addEnemyFrag((fe.pos), mx, my, fe.type);
                                        if (fe.type == BOSS_TYPE)
                                        {
                                            bossDestroied();
                                            playChunk(3);
                                        }
                                        else
                                        {
                                            playChunk(2);
                                        }

                                        removeFoeForced(fe);
                                        continue;
                                    }

                                    playChunk(1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    bmv = new Vector
                    {
                        x = (fe.pos).x,
                        y = (fe.pos).y
                    };
                    vctSub(bmv, (fe.ppos));
                    inaa = vctInnerProduct(bmv, bmv);
                    if (inaa > 1.0f)
                    {
                        sofs = new Vector
                        {
                            x = (ship.pos).x,
                            y = (ship.pos).y
                        };
                        vctSub(sofs, (fe.ppos));
                        inab = vctInnerProduct(bmv, sofs);
                        ht = inab / inaa;
                        if ((ht > 0.0f) && (ht < 1.0f))
                        {
                            hd = vctInnerProduct(sofs, sofs) - inab * inab / inaa / inaa;
                            if ((hd >= 0) && (hd < SHIP_HIT_WIDTH))
                            {
                                destroyShip();
                            }
                        }
                    }
                }

                if ((((fe.ppos.x < 0) || (fe.ppos.x >= SCAN_WIDTH_8)) || (fe.ppos.y < 0)) || (fe.ppos.y >= SCAN_HEIGHT_8))
                {
                    removeFoeForced(fe);
                    continue;
                }

                foeNum++;
            }
        }

        interval = INTERVAL_BASE;
        if (((((((((!(((insane) != 0))))) && (((!(((nowait) != 0)))))))) && (foeNum > processSpeedDownBulletsNum))))
        {
            interval = interval + ((foeNum - processSpeedDownBulletsNum) * INTERVAL_BASE / processSpeedDownBulletsNum);
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
                addClearFrag((fe.pos), (fe.mv));
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
                if (((foe[(i)].spc == NOT_EXIST) || (foe[(i)].type == BOSS_TYPE)) || (foe[(i)].spc == BOSS_ACTIVE_BULLET))
                    continue;
                fe = (foe[(i)]);
                addClearFrag((fe.pos), (fe.mv));
                removeFoeForced(fe);
            }
        }
    }

    public static void drawBulletsWake()
    {
        int i = 0;
        Foe fe = null;
        int x = 0, y = 0, sx = 0, sy = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == FOE)) || (foe[(i)].cnt >= 64))
                    continue;
                fe = (foe[(i)]);
                x = GameMath.signedShift((fe.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                y = GameMath.signedShift((fe.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                sx = GameMath.signedShift((fe.spos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                sy = GameMath.signedShift((fe.spos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                drawLine(x, y, sx, sy, 13 - fe.cnt / 5, 1, l1buf);
            }
        }
    }

    public static int[][] foeColor = new int[][]
    {
        new int[]
        {
            16 * 4 - 1,
            16 * 10 - 7
        },
        new int[]
        {
            16 * 2 - 1,
            16 * 8 - 7
        },
        new int[]
        {
            16 * 6 - 1,
            16 * 12 - 7
        },
        new int[]
        {
            16 * 1 - 11,
            16 * 1 - 4
        }
    };
    public static void drawFoes()
    {
        int i = 0, j = 0;
        Foe fe = null;
        int x = 0, y = 0, px = 0, py = 0;
        int sz = 0, cl1 = 0, cl2 = 0;
        int d = 0, md = 0, di = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if (foe[(i)].spc != FOE)
                    continue;
                fe = (foe[(i)]);
                x = GameMath.signedShift((fe.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                y = GameMath.signedShift((fe.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                if (fe.cnt < 16)
                {
                    sz = GameMath.signedShift((foeSize[(fe.type)] * fe.cnt), 4);
                }
                else
                {
                    sz = foeSize[(fe.type)];
                }

                cl1 = foeColor[(fe.type)][(0)];
                cl2 = foeColor[(fe.type)][(1)];
                if (((fe.hit) != 0))
                {
                    cl1 = FOE_HIT_COLOR;
                }

                drawBox(x, y, sz, sz, cl1, cl2, l1buf);
                d = (fe.cnt * 8) << 4;
                md = (DIV << 4) / fe.shield;
                sz = sz / (3);
                {
                    j = 0;
                    for (; j < fe.shield; j++, d = d + (md))
                    {
                        di = (GameMath.signedShift(d, 4)) & (DIV - 1);
                        drawBox(x + (GameMath.signedShift((sctbl[(di)] * sz), 7)), y + (GameMath.signedShift((sctbl[(di + DIV / 4)] * sz), 7)), sz, sz, cl1, cl2, l1buf);
                    }
                }
            }
        }
    }

    public static int[][] bulletColor = new int[][]
    {
        new int[]
        {
            16 * 14 - 1,
            16 * 2 - 1
        },
        new int[]
        {
            16 * 16 - 1,
            16 * 4 - 1
        },
        new int[]
        {
            16 * 12 - 1,
            16 * 6 - 1
        },
    };
    public static void drawBullets()
    {
        int i = 0;
        Foe fe = null;
        int x = 0, y = 0, px = 0, py = 0;
        int bc = 0;
        {
            i = 0;
            for (; i < FOE_MAX; i++)
            {
                if ((foe[(i)].spc == NOT_EXIST) || (foe[(i)].spc == FOE))
                    continue;
                fe = (foe[(i)]);
                x = GameMath.signedShift((fe.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                y = GameMath.signedShift((fe.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                px = GameMath.signedShift((fe.ppos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                py = GameMath.signedShift((fe.ppos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                bc = fe.color % BULLET_COLOR_NUM;
                drawThickLine(x, y, px, py, bulletColor[(bc)][(0)], bulletColor[(bc)][(1)], BULLET_WIDTH);
            }
        }
    }
}
