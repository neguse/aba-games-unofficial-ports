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

public static class NrAttract
{
    public static int score;
    public static int nextExtend, neAdd;
    public static int dsc;
    public static int left, stage;
    public static int ssSc;
    public static int hsScene, hsScSc, hsOfs;
    public static HiScore hiScore = new HiScore();
    public static void initHiScore()
    {
        int i = 0, j = 0;
        {
            i = 0;
            for (; i < STAGE_NUM; i++)
            {
                hiScore.stageScore[(i)] = DEFAULT_HISCORE;
                {
                    j = 0;
                    for (; j < SCENE_NUM; j++)
                    {
                        hiScore.sceneScore[(i)][(j)] = DEFAULT_SCENE_HISCORE;
                    }
                }
            }
        }

        {
            i = 0;
            for (; i < ENDLESS_STAGE_NUM; i++)
            {
                hiScore.stageScore[(i + STAGE_NUM)] = DEFAULT_HISCORE;
            }
        }

        hiScore.stage = 0;
    }

    public static void initGameState(int stg)
    {
        score = 0;
        ssSc = 0;
        nextExtend = 200000;
        neAdd = 300000;
        dsc = -1;
        left = 2;
        {
            stage = stg;
            hiScore.stage = stage;
        }

        hsScene = -1;
        drawRPanel();
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
                playChunk(6);
            }
        }
    }

    public static int extendShip()
    {
        if (left > 8)
            return 0;
        left++;
        drawRPanel();
        return 1;
    }

    public static int decrementShip()
    {
        left--;
        drawRPanel();
        if (left < 0)
            return 1;
        return 0;
    }

    public static void addLeftBonus()
    {
        nextExtend = 999999999;
        addScore(left * 100000);
    }

    public static void setClearScore()
    {
        int rss = hiScore.sceneScore[(stage)][(scene)];
        int ss = score - ssSc;
        hsScene = scene;
        hsOfs = ss - rss;
        hsScSc = ss;
        if (ss > rss)
        {
            hiScore.sceneScore[(stage)][(scene)] = ss;
        }

        drawRPanel();
        ssSc = score;
    }

    public static void setHiScore()
    {
        if (score > hiScore.stageScore[(stage)])
        {
            hiScore.stageScore[(stage)] = score;
        }
        savePreference();
    }

    public static void showScore()
    {
        dsc = -1;
    }

    public static void drawScore()
    {
        if (dsc == score)
            return;
        dsc = score;
        clearLPanel();
        drawNum(score, 118, 24, 28, 16 * 1 - 12, 16 * 1 - 3);
        drawNum(bonusScore, 24, 14, 16, 16 * 1 - 12, 16 * 1 - 3);
    }

    public static void drawRPanel()
    {
        int y = 0;
        string str = "LEFT";
        clearRPanel();
        if (left >= 0)
        {
            drawString(str, 34, 272, 24, 3, 16 * 1 - 12, 16 * 1 - 3, rpbuf);
            drawLetter(left, 34, 450, 24, 3, 16 * 2 - 10, 16 * 2 - 1, rpbuf);
        }

        y = 24;
        if (((!(((endless) != 0)))))
        {
            y = drawNumRight(stage + 1, 124, y, 24, 16 * 1 - 12, 16 * 1 - 3);
            drawLetter(38, 124, y, 24, 3, 16 * 1 - 12, 16 * 1 - 3, rpbuf);
            y = GameMath.integer(y + (24 * 1.7f));
            if (scene >= 10)
            {
                drawLetter(14, 124, y, 24, 3, 16 * 1 - 12, 16 * 1 - 3, rpbuf);
                return;
            }
        }

        drawNumRight(scene + 1, 124, y, 24, 16 * 1 - 12, 16 * 1 - 3);
        if (hsScene >= 0)
        {
            y = SCENE_STAT_SIZE;
            y = drawNumRight(stage + 1, SCENE_STAT_X, y, SCENE_STAT_SIZE, 16 * 1 - 12, 16 * 1 - 3);
            drawLetter(38, SCENE_STAT_X, y, SCENE_STAT_SIZE, 3, 16 * 1 - 12, 16 * 1 - 3, rpbuf);
            y = GameMath.integer(y + (SCENE_STAT_SIZE * 1.7f));
            y = drawNumRight(hsScene + 1, SCENE_STAT_X, y, SCENE_STAT_SIZE, 16 * 1 - 12, 16 * 1 - 3);
            y = GameMath.integer(y + (SCENE_STAT_SIZE * 1.7f * 2));
            y = drawNumRight(hsScSc, SCENE_STAT_X, y, SCENE_STAT_SIZE, 16 * 1 - 12, 16 * 1 - 3);
            y = GameMath.integer(y + (SCENE_STAT_SIZE * 1.7f));
            if (hsOfs >= 0)
            {
                drawLetter(39, SCENE_STAT_X, y, SCENE_STAT_SIZE, 3, 16 * 2 - 12, 16 * 2 - 3, rpbuf);
                y = GameMath.integer(y + (SCENE_STAT_SIZE * 1.7f));
                drawNumRight(hsOfs, SCENE_STAT_X, y, SCENE_STAT_SIZE, 16 * 2 - 12, 16 * 2 - 3);
            }
            else
            {
                drawLetter(38, SCENE_STAT_X, y, SCENE_STAT_SIZE, 3, 16 * 4 - 12, 16 * 4 - 3, rpbuf);
                y = GameMath.integer(y + (SCENE_STAT_SIZE * 1.7f));
                drawNumRight(-hsOfs, SCENE_STAT_X, y, SCENE_STAT_SIZE, 16 * 4 - 12, 16 * 4 - 3);
            }
        }
    }

    public static int[] stageX = Make(STG_BOX_NUM, () => 0), stageY = Make(STG_BOX_NUM, () => 0);
    public static void initAttractManager()
    {
        int i = 0, j = 0, x = 0, y = 0, s = 0;
        y = LAYER_HEIGHT / 3 + STG_BOX_SIZE / 2;
        s = 0;
        {
            i = 0;
            for (; i < 6; i++, y = GameMath.integer(y + (STG_BOX_SIZE * 1.2f)))
            {
                x = STG_BOX_SIZE / 2 + STG_BOX_SIZE / 2;
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    {
                        j = 0;
                        for (; j <= i; j++, s++, x = GameMath.integer(x + (STG_BOX_SIZE * 1.2f)))
                        {
                            stageX[(s)] = x;
                            stageY[(s)] = y;
                        }
                    }

                        break;
                    case 4:
                    {
                        j = 0;
                        for (; j <= 2; j++, s++, x = GameMath.integer(x + (STG_BOX_SIZE * 1.2f)))
                        {
                            stageX[(s)] = x;
                            stageY[(s)] = y;
                        }
                    }

                        x = GameMath.integer(x + (STG_BOX_SIZE * 1.2f));
                        stageX[(s)] = x;
                        stageY[(s)] = y;
                        s++;
                        break;
                    case 5:
                        y = y + (STG_BOX_SIZE / 3);
                        stageX[(s)] = x;
                        stageY[(s)] = y;
                        break;
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
        mnp = 0;
        return slcStg;
    }

    public static void drawTitle()
    {
        int i = 0;
        {
            i = 0;
            for (; i < 7; i++)
            {
                drawSprite(i, 162 + i * 46, 16);
            }
        }
    }

    public static int[][] stgMv = new int[][]
    {
        new int[]
        {
            0,
            0,
            1,
            0
        },
        new int[]
        {
            -1,
            1,
            2,
            0
        },
        new int[]
        {
            0,
            0,
            2,
            -1
        },
        new int[]
        {
            -2,
            1,
            3,
            0
        },
        new int[]
        {
            -2,
            1,
            3,
            -1
        },
        new int[]
        {
            0,
            0,
            3,
            -1
        },
        new int[]
        {
            -3,
            1,
            4,
            0
        },
        new int[]
        {
            -3,
            1,
            4,
            -1
        },
        new int[]
        {
            -3,
            1,
            4,
            -1
        },
        new int[]
        {
            0,
            0,
            3,
            -1
        },
        new int[]
        {
            -4,
            1,
            4,
            0
        },
        new int[]
        {
            -4,
            1,
            3,
            -1
        },
        new int[]
        {
            -4,
            1,
            2,
            -1
        },
        new int[]
        {
            0,
            0,
            1,
            -1
        },
        new int[]
        {
            -4,
            0,
            0,
            0
        },
    };
    public static void moveTitleMenu()
    {
        int pad = getPadState();
        int btn = getButtonState();
        int p = -1;
        int sm = 0;
        if (((pad & PAD_DOWN) != 0))
        {
            p = 2;
        }
        else if (((pad & PAD_UP) != 0))
        {
            p = 0;
        }
        else if (((pad & PAD_RIGHT) != 0))
        {
            p = 1;
        }
        else if (((pad & PAD_LEFT) != 0))
        {
            p = 3;
        }
        else if (btn == 0)
        {
            mnp = 1;
        }

        if ((((((mnp) != 0)) && (p >= 0))))
        {
            mnp = 0;
            sm = stgMv[(slcStg)][(p)];
            slcStg = slcStg + (sm);
            if (sm != 0)
            {
                initTitleStage(slcStg);
            }

            titleCnt = 16;
        }

        if ((((((mnp) != 0)) && ((((btn & PAD_BUTTON1)) != 0)))))
        {
            if (slcStg == STAGE_NUM + ENDLESS_STAGE_NUM)
            {
                quitLast();
                return;
            }

            hiScore.stage = slcStg;
            initGame(slcStg);
        }

        titleCnt++;
    }

    public static void drawTitleMenu()
    {
        int i = 0;
        string stgChr = "STAGE";
        string endlessChr = "ENDLESS";
        string hardChr = "HARD";
        string extChr = "EXTREME";
        string insChr = "INSANE";
        string quitChr = "QUIT";
        {
            i = 0;
            for (; i < STG_BOX_NUM; i++)
            {
                if (i == slcStg)
                {
                    int sz = STG_BOX_SIZE + 6 + sctbl[((titleCnt * 16) & (DIV - 1))] / 24;
                    drawBox(stageX[(i)], stageY[(i)], sz, sz, 16 * 2 - 14, 16 * 2 - 3, buf);
                    if (i < STAGE_NUM)
                    {
                        drawStringBuf(stgChr, 180, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                        drawNumCenter(i + 1, 308, 80, 12, 16 * 1 - 14, 16 * 1 - 2);
                    }
                    else
                    {
                        switch (i)
                        {
                            case 10:
                                drawStringBuf(endlessChr, 188, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                break;
                            case 11:
                                drawStringBuf(endlessChr, 93, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                drawStringBuf(hardChr, 248, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                break;
                            case 12:
                                drawStringBuf(endlessChr, 36, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                drawStringBuf(extChr, 190, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                break;
                            case 13:
                                drawStringBuf(endlessChr, 56, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                drawStringBuf(insChr, 210, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                break;
                            case 14:
                                drawStringBuf(quitChr, 230, 80, 12, 2, 16 * 1 - 14, 16 * 1 - 2, buf, 0);
                                break;
                        }
                    }

                    if (i < STAGE_NUM + ENDLESS_STAGE_NUM)
                    {
                        drawNumCenter(hiScore.stageScore[(i)], 308, 112, 12, 16 * 1 - 14, 16 * 1 - 2);
                    }
                }

                drawBox(stageX[(i)], stageY[(i)], STG_BOX_SIZE, STG_BOX_SIZE, 16 * 1 - 14, 16 * 1 - 3, buf);
                if (i < 9)
                {
                    drawNumCenter(i + 1, stageX[(i)], stageY[(i)], 12, 16 * 1 - 16, 16 * 1 - 1);
                }
                else
                {
                    switch (i)
                    {
                        case 9:
                            drawNumCenter(10, stageX[(i)] + 8, stageY[(i)], 12, 16 * 1 - 16, 16 * 1 - 1);
                            break;
                        case 10:
                            drawLetterBuf(14, stageX[(i)], stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            break;
                        case 11:
                            drawLetterBuf(14, stageX[(i)] - 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            drawLetterBuf(17, stageX[(i)] + 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            break;
                        case 12:
                            drawLetterBuf(14, stageX[(i)] - 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            drawLetterBuf(14, stageX[(i)] + 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            break;
                        case 13:
                            drawLetterBuf(14, stageX[(i)] - 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            drawLetterBuf(18, stageX[(i)] + 8, stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            break;
                        case 14:
                            drawLetterBuf(26, stageX[(i)], stageY[(i)], 12, 2, 16 * 1 - 16, 16 * 1 - 1, buf, 0);
                            break;
                    }
                }
            }
        }
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
        if ((((goCnt > 900) || ((((((((goCnt > 128) && (((mnp) != 0))))) && ((((btn & PAD_BUTTON1)) != 0)))))))))
        {
            setHiScore();
            initTitle();
            return;
        }

        if (btn == 0)
        {
            mnp = 1;
        }

        goCnt++;
    }

    public static void drawGameover()
    {
        string goChr = "GAME OVER";
        int y = 0;
        if (goCnt < 128)
        {
            y = LAYER_HEIGHT / 3 * goCnt / 128;
        }
        else
        {
            y = LAYER_HEIGHT / 3;
        }

        drawStringBuf(goChr, 24, y, 20, 2, 16 * 4 - 10, 16 * 1 - 1, buf, 0);
    }

    public static int scCnt;
    public static void initStageClearAtr()
    {
        scCnt = 0;
        mnp = 0;
        fadeMusic();
    }

    public static void moveStageClear()
    {
        int btn = getButtonState();
        if ((((scCnt > 900) || ((((((((scCnt > 128) && (((mnp) != 0))))) && ((((btn & PAD_BUTTON1)) != 0)))))))))
        {
            setHiScore();
            initTitle();
            return;
        }

        if (btn == 0)
        {
            mnp = 1;
        }

        scCnt++;
    }

    public static void drawStageClear()
    {
        string scChr = "STAGE CLEAR";
        int y = 0;
        if (scCnt < 128)
        {
            y = LAYER_HEIGHT - LAYER_HEIGHT / 3 * 2 * scCnt / 128;
        }
        else
        {
            y = LAYER_HEIGHT / 3;
        }

        drawStringBuf(scChr, 24, y, 16, 2, 16 * 3 - 10, 16 * 1 - 1, buf, 0);
    }

    public static int psCnt = 0;
    public static void movePause()
    {
        psCnt++;
    }

    public static void drawPause()
    {
        string psChr = "PAUSE";
        if ((psCnt & 63) < 32)
        {
            drawStringBuf(psChr, 92, LAYER_HEIGHT / 3, 20, 2, 16 * 2 - 10, 16 * 1 - 1, buf, 0);
        }
    }
}
