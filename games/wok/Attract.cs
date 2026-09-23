// Copyright 2001 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static WkCore;
using static WkAttract;
using static WkBall;
using static WkBoard;
using static WkGenerator;
using static WkPan;
using static WkVector;
using static WkConstants;
using static WkArrays;
using static WkRandom;
using static WkScreen;
using static WkSound;

public static class WkAttract
{
    public static int mvTpIdx;
    public static int startMv, quitMv;
    public static void initTitle()
    {
        stopMusic();
        initBalls();
        initMouse();
        status = TITLE;
        mvTpIdx = -MVTPIDX_OFS;
        {
            quitMv = 0;
            startMv = quitMv;
        }
    }

    public static int[] titlePtn = new int[]
    {
        2,
        0,
        0,
        2,
        0,
        30,
        3,
        12,
        60,
        1,
        36,
        60,
        2,
        48,
        0,
        2,
        48,
        30,
        3,
        60,
        60,
        1,
        84,
        60,
        2,
        96,
        0,
        2,
        96,
        30,
        1,
        140,
        0,
        2,
        132,
        30,
        3,
        140,
        60,
        0,
        164,
        80,
        1,
        188,
        60,
        2,
        204,
        30,
        3,
        188,
        0,
        0,
        164,
        0,
        2,
        250,
        0,
        2,
        250,
        30,
        2,
        250,
        60,
        1,
        278,
        4,
        1,
        260,
        30,
        3,
        270,
        40,
        3,
        288,
        60,
    };
    public static int mx, my, mb;
    public static void moveTitle()
    {
        mx=WkScreen.mx;my=WkScreen.my;mb=WkScreen.buttons;
        {
            quitMv = 0;
            startMv = quitMv;
        }

        if ((mx > 456) && (mx < 586))
        {
            if ((my > 300) && (my < 332))
                startMv = 1;
            else if ((my > 350) && (my < 382))
                quitMv = 1;
        }

        mvTpIdx++;
        if (mvTpIdx > TITLE_PTN_NUM + MVTPIDX_OFS)
            mvTpIdx = -MVTPIDX_OFS;
    }

    public static void drawTitle()
    {
        int i = 0, x = 0, y = 0;
        fillRect(0, TITLE_Y, 40, 92);
        fillRect(400, TITLE_Y, 240, 92);
        {
            i = 0;
            for (; i < TITLE_PTN_NUM; i = i + (3))
            {
                if ((i > mvTpIdx) && (i < mvTpIdx + MVTPIDX_OFS))
                {
                    x = randNS(4) + randNS(4) + randNS(4) + TITLE_X;
                    y = randNS(4) + randNS(4) + randNS(4) + TITLE_Y;
                }
                else
                {
                    x = TITLE_X;
                    y = TITLE_Y;
                }

                drawSprite(BAR_SPRITE_IDX + titlePtn[(i)], titlePtn[(i + 1)] + x, titlePtn[(i + 2)] + y);
            }
        }

        fillRect(450, 282, 190, 120);
        if (((startMv) != 0))
        {
            drawSprite(TITLE_SPRITE_IDX, 480 + randNS(4) + randNS(4), 300 + randNS(4) + randNS(4));
            if ((((mb & 1)) != 0))
            {
                initGame();
            }
        }
        else
        {
            drawSprite(TITLE_SPRITE_IDX, 480, 300);
        }

        if (((quitMv) != 0))
        {
            drawSprite(TITLE_SPRITE_IDX + 1, 480 + randNS(4) + randNS(4), 350 + randNS(4) + randNS(4));
            if ((((mb & 1)) != 0))
            {
                quitWok();
            }
        }
        else
        {
            drawSprite(TITLE_SPRITE_IDX + 1, 480, 350);
        }

        drawSprite(TITLE_SPRITE_IDX + 2, 20, 360);
        drawNum(hiScore, 10, 410);
        if (score > 0)
            drawNum(score, 0, 0);
        drawSprite(TITLE_SPRITE_IDX + 3, mx, my);
    }

    public static int overCnt;
    public static void initOver()
    {
        status = GAMEOVER;
        overCnt = 0;
    }

    public static int[] overPtn = new int[]
    {
        0,
        24,
        0,
        1,
        0,
        0,
        3,
        0,
        25,
        0,
        24,
        45,
        2,
        42,
        25,
        0,
        24,
        25,
        2,
        70,
        25,
        1,
        70,
        0,
        3,
        94,
        0,
        2,
        114,
        25,
        0,
        82,
        28,
        2,
        140,
        0,
        2,
        140,
        25,
        3,
        145,
        0,
        1,
        166,
        0,
        2,
        188,
        0,
        2,
        188,
        25,
        2,
        210,
        0,
        2,
        210,
        25,
        0,
        212,
        0,
        0,
        231,
        0,
        0,
        212,
        22,
        0,
        212,
        48,
        0,
        231,
        48,
        1,
        160,
        80,
        3,
        160,
        108,
        1,
        185,
        108,
        3,
        185,
        80,
        2,
        230,
        80,
        3,
        230,
        108,
        1,
        254,
        108,
        2,
        264,
        80,
        2,
        295,
        80,
        2,
        295,
        105,
        0,
        297,
        80,
        0,
        315,
        80,
        0,
        297,
        102,
        0,
        297,
        128,
        0,
        315,
        128,
        2,
        360,
        80,
        2,
        360,
        112,
        0,
        370,
        80,
        0,
        370,
        105,
        2,
        390,
        80,
        3,
        375,
        115,
    };
    public static void moveOver()
    {
        mx=WkScreen.mx;my=WkScreen.my;mb=WkScreen.buttons;
        overCnt++;
        if ((overCnt > 360) || ((((mb & SDL_BUTTON_LEFT)) != 0)))
            initTitle();
    }

    public static void drawOver()
    {
        int i = 0, x = 0, y = 0;
        drawNum(score, 0, 0);
        {
            i = 0;
            for (; i < OVER_PTN_NUM; i = i + (3))
            {
                if (overCnt < 30)
                {
                    x = SCREEN_WIDTH / 2 - (SCREEN_WIDTH / 2 - (OVER_X + overPtn[(i + 1)])) * overCnt / 30 + randNS(4) + randNS(4) + randNS(4);
                    y = SCREEN_HEIGHT - (SCREEN_HEIGHT - (OVER_Y + overPtn[(i + 2)])) * overCnt / 30 + randNS(4) + randNS(4) + randNS(4);
                }
                else
                {
                    x = OVER_X + overPtn[(i + 1)] + randNS(2);
                    y = OVER_Y + overPtn[(i + 2)] + randNS(2);
                }

                drawSprite(BAR_SPRITE_IDX + overPtn[(i)], x, y);
            }
        }
    }
}
