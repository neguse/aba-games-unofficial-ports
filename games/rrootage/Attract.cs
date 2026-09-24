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

public static class RrAttract
{
    public static int score;
    public static int nextExtend, neAdd;
    public static int left, stage, scene;
    public static float rank;
    public static int seed;
    public static int bombUsed, shipUsed;
    public static int mode;
    public static HiScore hiScore = new HiScore();
    public static void initHiScore()
    {
        int i = 0, j = 0;
        {
            j = 0;
            for (; j < MODE_NUM; j++)
            {
                {
                    i = 0;
                    for (; i < STAGE_NUM; i++)
                    {
                        hiScore.score[(j)][(i)] = DEFAULT_HISCORE;
                        hiScore.cleard[(j)][(i)] = 0;
                    }
                }
            }
        }

        {
            hiScore.mode = 0;
            hiScore.stage = hiScore.mode;
        }
    }

    public static float[][] bgColor = new float[][]
    {
        new float[]
        {
            0.0f,
            0.0f,
            0.0f
        },
        new float[]
        {
            0.1f,
            0.1f,
            0.3f
        },
        new float[]
        {
            0.3f,
            0.1f,
            0.3f
        },
        new float[]
        {
            0.1f,
            0.3f,
            0.1f
        },
    };
    public static void setMode(int m)
    {
        mode = m;
        RrScreen.clearColor = new float[] { bgColor[m][0], bgColor[m][1], bgColor[m][2], 1 };
    }

    public static void gotoNextScene()
    {
        scene++;
        seed = seed * 8513 + 179;
        createBoss(seed, rank, scene);
        {
            shipUsed = 0;
            bombUsed = shipUsed;
        }
    }

    public static void initStageState(int stg)
    {
        float rb = 0;
        int rn = 0, sn = 0;
        int i = 0;
        scene = -1;
        rn = GameMath.integer(stg / SAME_RANK_STAGE_NUM);
        sn = stg % SAME_RANK_STAGE_NUM;
        if (sn == SAME_RANK_STAGE_NUM - 1)
        {
            seed = rand();
        }
        else
        {
            seed = stg * 10357 + 5449 + mode * 947;
        }

        rank = 0.1f;
        rb = 0;
        {
            i = 0;
            for (; i < rn; i++)
            {
                rb = rb + (RANK_UP_BASE);
                rank = rank + (rb);
            }
        }

        if ((stg >= STAGE_NUM) || (stg < 0))
            rank = 0;
        gotoNextScene();
    }

    public static string stageStr = "1A";
    public static void makeStageStr(int stg)
    {
        stageStr = "1234567890".Substring(GameMath.integer(GameMath.integer(stg / 4)), 1) + "ABCR".Substring(stg % 4, 1);
    }

    public static void initGameStateFirst()
    {
        score = 0;
        nextExtend = 200000;
        neAdd = 300000;
        left = 2;
        stage = 0;
    }

    public static void initGameState(int stg)
    {
        initGameStateFirst();
        {
            stage = stg;
            hiScore.stage = stage;
        }

        hiScore.mode = mode;
        makeStageStr(stg);
        initStageState(stg);
    }

    public static void addScore(int s)
    {
        score = score + (s);
        if (score >= nextExtend)
        {
            nextExtend = nextExtend + (neAdd);
            neAdd = 500000;
            if (((extendShip()) != 0))
            {
                playChunk(8);
            }
        }
    }

    public static int extendShip()
    {
        if (left > 8)
            return 0;
        left++;
        return 1;
    }

    public static int decrementShip()
    {
        left--;
        if (left < 0)
            return 1;
        return 0;
    }

    public static void addLeftBonus()
    {
        nextExtend = 999999999;
        addScore(left * 100000);
    }

    public static void setHiScore(int cleard)
    {
        if (score > hiScore.score[(mode)][(stage)])
        {
            hiScore.score[(mode)][(stage)] = score;
        }

        if (((cleard) != 0))
        {
            hiScore.cleard[(mode)][(stage)] = 1;
        }

        savePreference();
    }

    public static void drawScore(float[] model, Gfx.Blend blend, string key)
    {
        drawNum(model, blend, key + "-9", score, 118, 24, 28, 200, 200, 222);
        drawNum(model, blend, key + "-61", GameMath.integer(bonusScore / 10) * 10, 24, 14, 16, 200, 200, 222);
    }

    public static string lStr = "LEFT", bStr = "BOMB", okStr = "OK";
    public static void drawRPanel(float[] model, Gfx.Blend blend, string key)
    {
        int y = 0;
        int ml = 0;
        if (left >= 0)
        {
            drawString(model, blend, key + "-85", lStr, 40 + 480, 280, 18, 1, 200, 200, 222);
            drawLetter(model, blend, key + "-179", left, 40 + 480, 420, 18, 1, 230, 180, 150);
            switch (mode)
            {
                case NORMAL_MODE:
                    drawString(model, blend, key + "-301", bStr, 90 + 480, 280, 18, 1, 200, 200, 222);
                    drawLetter(model, blend, key + "-431", bomb, 90 + 480, 420, 18, 1, 230, 180, 150);
                    break;
                case PSY_MODE:
                    ml = GameMath.integer(ship.grzCnt / 40);
                    drawBox(model, blend, key + "-681", 550, 460, 50, 8, 120, 120, 120);
                    drawBox(model, blend, key + "-742", 500 + ml, 460, ml, 8, 210, 210, 240);
                    break;
                case GW_MODE:
                    ml = GameMath.integer((ship.rfMtr - ship.rfMtrDec) / 40);
                    drawBox(model, blend, key + "-943", 550, 460, 50, 8, 120, 120, 120);
                    drawBox(model, blend, key + "-1004", 500 + ml, 460, ml, 8, 210, 240, 210);
                    if (ml >= 50)
                    {
                        drawString(model, blend, key + "-1019", okStr, 540, 460, 10, 0, 230, 240, 230);
                    }

                    break;
            }
        }

        y = 24;
        drawString(model, blend, key + "-1169", stageStr, 124 + 480, y, 24, 1, 200, 200, 222);
        y = GameMath.integer(y + (24 * 1.7f * 2));
        drawLetter(model, blend, key + "-1399", 38, 124 + 480, y, 24, 1, 200, 200, 222);
        y = GameMath.integer(y + (24 * 1.7f));
        drawNumRight(model, blend, key + "-1393", scene + 1, 124 + 480, y, 24, 200, 200, 222);
    }

    public static int[] stageX = Make(STG_BOX_NUM, () => 0), stageY = Make(STG_BOX_NUM, () => 0);
    public static void initAttractManager()
    {
        int i = 0, j = 0, x = 0, y = 0, s = 0;
        y = 172;
        s = 0;
        {
            i = 0;
            for (; i < 12; i++, y = GameMath.integer(y + (STG_BOX_SIZE * 1.2f)))
            {
                x = 180;
                if (i < 11)
                {
                    {
                        j = 0;
                        for (; j < SAME_RANK_STAGE_NUM; j++, s++, x = GameMath.integer(x + (STG_BOX_SIZE * 1.2f)))
                        {
                            stageX[(s)] = x;
                            stageY[(s)] = y;
                        }
                    }
                }
                else
                {
                    stageX[(s)] = x;
                    stageY[(s)] = y;
                }
            }
        }
    }

    public static int titleCnt;
    public static int slcStg;
    public static int mnp;
    public static int initTitleAtr()
    {
        stopMusic();
        titleCnt = 0;
        slcStg = hiScore.stage;
        setMode(hiScore.mode);
        mnp = 0;
        return slcStg;
    }

    public static void moveTitleMenu()
    {
        int pad = getPadState();
        int btn = getButtonState();
        int bs = slcStg;
        if (((pad & PAD_DOWN) != 0))
        {
            if (((mnp) != 0))
            {
                if (slcStg < STAGE_NUM - SAME_RANK_STAGE_NUM)
                    slcStg = slcStg + (SAME_RANK_STAGE_NUM);
                else if (slcStg == QUIT_STAGE_NUM)
                    slcStg = -MODE_NUM;
                else
                    slcStg = QUIT_STAGE_NUM;
            }
        }
        else if (((pad & PAD_UP) != 0))
        {
            if (((mnp) != 0))
            {
                if (slcStg >= 0)
                    slcStg = slcStg - (SAME_RANK_STAGE_NUM);
                else
                    slcStg = QUIT_STAGE_NUM;
            }
        }
        else if (((pad & PAD_RIGHT) != 0))
        {
            if (((mnp) != 0))
            {
                if (slcStg >= 0)
                {
                    if (((slcStg % SAME_RANK_STAGE_NUM) < SAME_RANK_STAGE_NUM - 1) && (slcStg != QUIT_STAGE_NUM))
                    {
                        slcStg++;
                    }
                }
                else if (slcStg < -1)
                {
                    slcStg++;
                }
            }
        }
        else if (((pad & PAD_LEFT) != 0))
        {
            if (((mnp) != 0))
            {
                if (slcStg >= 0)
                {
                    if (((slcStg % SAME_RANK_STAGE_NUM) > 0) && (slcStg != QUIT_STAGE_NUM))
                    {
                        slcStg--;
                    }
                }
                else if (slcStg > -4)
                {
                    slcStg--;
                }
            }
        }
        else if (btn == 0)
        {
            mnp = 1;
        }

        if (slcStg != bs)
        {
            mnp = 0;
            initTitleStage(slcStg);
            titleCnt = 0;
        }

        if ((((mnp) != 0)) && ((((btn & PAD_BUTTON1)) != 0)))
        {
            if (slcStg == QUIT_STAGE_NUM)
            {
                quitLast();
            }
            else if (slcStg < 0)
            {
                mnp = 0;
                setMode(MODE_NUM + slcStg);
            }
            else
            {
                hiScore.stage = slcStg;
                initGame(slcStg);
            }
        }

        if ((((mnp) != 0)) && ((((btn & PAD_BUTTON2)) != 0)))
        {
            mnp = 0;
            setMode((mode + 1) % MODE_NUM);
            initTitleStage(slcStg);
            titleCnt = 0;
        }

        titleCnt++;
    }

    public static void drawTitle(float[] model, Gfx.Blend blend, string key)
    {
        int i = 0;
        int r = 0, g = 0, b = 0;
        int sx = 0, sy = 0;
        string stgChr = "STAGE";
        string quitChr = "QUIT";
        string[] mdChr = new string[]
        {
            "NORMAL MODE",
            "PSY MODE",
            "IKA MODE",
            "GW MODE"
        };
        int[] mdChrX = new int[]
        {
            270,
            330,
            330,
            350
        };
        char[] mdIni = new char[]
        {
            'N',
            'P',
            'I',
            'G'
        };
        drawTitleBoard(model, blend, key + "-554" + "-" + i.ToString());
        {
            i = -MODE_NUM;
            for (; i < STAGE_NUM + 1; i++)
            {
                if (i < 0)
                {
                    if (4 + i == mode)
                    {
                        r = 100;
                        g = 100;
                        b = 240;
                    }
                    else
                    {
                        r = 150;
                        g = 150;
                        b = 200;
                    }
                }
                else if ((i < QUIT_STAGE_NUM) && (((hiScore.cleard[(mode)][(i)]) != 0)))
                {
                    r = 240;
                    g = 180;
                    b = 180;
                }
                else
                {
                    r = 210;
                    g = 210;
                    b = 240;
                }

                sx = stageX[(i + MODE_NUM)];
                sy = stageY[(i + MODE_NUM)];
                if (i == slcStg)
                {
                    int sz = GameMath.integer(STG_BOX_SIZE * 3 / 2);
                    if (titleCnt < 16)
                        sz = GameMath.integer(sz * titleCnt / 16);
                    drawBox(model, blend, key + "-1821" + "-" + i.ToString(), sx, sy, sz, sz, r, g, b);
                    sz = GameMath.integer(sz * 3 / 5);
                    if (i < 0)
                    {
                        int md = MODE_NUM + i;
                        drawString(model, blend, key + "-2034" + "-" + i.ToString(), mdChr[(md)], mdChrX[(md)], 133, 12, 0, 150, 150, 200);
                        drawLetter(model, blend, key + "-2174" + "-" + i.ToString(), mdIni[(md)] - 'A' + 10, sx, sy, sz, 0, 150, 150, 240);
                    }
                    else if (i < QUIT_STAGE_NUM)
                    {
                        makeStageStr(i);
                        drawString(model, blend, key + "-2348" + "-" + i.ToString(), stageStr, sx - sz, sy, sz, 0, 210, 210, 240);
                        drawString(model, blend, key + "-2429" + "-" + i.ToString(), stgChr, 330, 133, 12, 0, 210, 210, 240);
                        drawString(model, blend, key + "-2505" + "-" + i.ToString(), stageStr, 445, 133, 12, 0, 210, 210, 240);
                        drawNumCenter(model, blend, key + "-2583" + "-" + i.ToString(), hiScore.score[(mode)][(i)], 466, 168, 12, 210, 210, 240);
                    }
                    else
                    {
                        drawLetter(model, blend, key + "-2998" + "-" + i.ToString(), 'Q' - 'A' + 10, sx, sy, sz, 0, 210, 210, 240);
                        drawString(model, blend, key + "-2880" + "-" + i.ToString(), quitChr, 410, 133, 12, 0, 210, 210, 240);
                    }
                }
                else
                {
                    drawBox(model, blend, key + "-3382" + "-" + i.ToString(), sx, sy, GameMath.integer(STG_BOX_SIZE / 2), GameMath.integer(STG_BOX_SIZE / 2), GameMath.integer(r * 2 / 3), GameMath.integer(g * 2 / 3), GameMath.integer(b * 2 / 3));
                }
            }
        }

        drawString(model, blend, key + "-3259" + "-" + i.ToString(), mdChr[(mode)], mdChrX[(mode)], 455, 12, 0, 150, 150, 200);
    }

    public static int goCnt;
    public static void initGameoverAtr()
    {
        goCnt = 0;
        mnp = 0;
        fadeMusic();
    }

    public static void moveGameover()
    {
        int btn = getButtonState();
        if ((goCnt > 900) || ((((goCnt > 128) && (((mnp) != 0))) && ((((btn & PAD_BUTTON1)) != 0)))))
        {
            setHiScore(0);
            initTitle();
            return;
        }

        if (btn == 0)
        {
            mnp = 1;
        }

        goCnt++;
    }

    public static void drawGameover(float[] model, Gfx.Blend blend, string key)
    {
        string goChr = "GAME OVER";
        int y = 0;
        if (goCnt < 128)
        {
            y = GameMath.integer(GameMath.integer(LAYER_HEIGHT / 3) * goCnt / 128);
        }
        else
        {
            y = GameMath.integer(LAYER_HEIGHT / 3);
        }

        drawString(model, blend, key + "-279", goChr, 184, y, 20, 0, 180, 180, 220);
    }

    public static int psCnt = 0;
    public static void movePause()
    {
        psCnt++;
    }

    public static void drawPause(float[] model, Gfx.Blend blend, string key)
    {
        string psChr = "PAUSE";
        if ((psCnt & 63) < 32)
        {
            drawString(model, blend, key + "-86", psChr, 252, GameMath.integer(LAYER_HEIGHT / 3), 20, 0, 200, 200, 180);
        }
    }

    public static int bsCnt, bossScore, bsAdd;
    public static void initBossScoreAtr(int bs)
    {
        bsCnt = 0;
        mnp = 0;
        bossScore = bs;
        bsAdd = GameMath.integer(GameMath.integer(bs / (1 + shipUsed + bombUsed)) / 10) * 10;
        if (scene > 3)
        {
            status = STAGE_CLEAR;
            fadeMusic();
        }
    }

    public static void moveBossScoreAtr()
    {
        int btn = getButtonState();
        if (bsCnt == 128)
            addScore(bsAdd);
        if ((bsCnt > 600) || ((((bsCnt > 160) && (((mnp) != 0))) && ((((btn & PAD_BUTTON1)) != 0)))))
        {
            if (status != STAGE_CLEAR)
            {
                gotoNextScene();
            }
            else
            {
                setHiScore(1);
                initTitle();
            }

            return;
        }

        if (btn == 0)
        {
            mnp = 1;
        }

        bsCnt++;
    }

    public static void drawBossScoreAtr(float[] model, Gfx.Blend blend, string key)
    {
        if (bsCnt < 32)
            return;
        drawNumCenter(model, blend, key + "-53", bossScore, 450, 240, 16, 200, 200, 220);
        if (bsCnt < 64)
            return;
        drawBox(model, blend, key + "-187", 320, 272, 150, 4, 200, 200, 220);
        if (bsCnt < 96)
            return;
        drawNumCenter(model, blend, key + "-254", 1, 230, 306, 16, 200, 200, 220);
        drawLetter(model, blend, key + "-364", 39, 260, 306, 16, 0, 200, 200, 220);
        drawNumCenter(model, blend, key + "-365", shipUsed, 340, 306, 16, 200, 200, 220);
        if (mode == NORMAL_MODE)
        {
            drawLetter(model, blend, key + "-557", 39, 380, 306, 16, 0, 200, 200, 220);
            drawNumCenter(model, blend, key + "-534", bombUsed, 450, 306, 16, 200, 200, 220);
        }

        if (bsCnt < 128)
            return;
        drawNumCenter(model, blend, key + "-652", bsAdd, 450, 380, 19, 200, 220, 200);
        if (status == STAGE_CLEAR)
        {
            string scChr = "STAGE CLEAR";
            drawString(model, blend, key + "-941", scChr, 190, 440, 15, 0, 180, 220, 180);
        }
    }
}
