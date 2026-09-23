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

public static class NrCore
{
    public static int status;
    public static float[][] stagePrm = new float[][]
    {
        new float[]
        {
            13,
            0.5f,
            0.12f
        },
        new float[]
        {
            2,
            1.8f,
            0.15f
        },
        new float[]
        {
            3,
            3.2f,
            0.1f
        },
        new float[]
        {
            90,
            6.0f,
            0.3f
        },
        new float[]
        {
            5,
            5.0f,
            0.6f
        },
        new float[]
        {
            6,
            10.0f,
            0.6f
        },
        new float[]
        {
            7,
            5.0f,
            2.2f
        },
        new float[]
        {
            98,
            12.0f,
            1.5f
        },
        new float[]
        {
            9,
            10.0f,
            2.0f
        },
        new float[]
        {
            79,
            21.0f,
            1.5f
        },
        new float[]
        {
            -3,
            5.0f,
            0.7f
        },
        new float[]
        {
            -1,
            10.0f,
            1.2f
        },
        new float[]
        {
            -4,
            15.0f,
            1.8f
        },
        new float[]
        {
            -2,
            16.0f,
            1.8f
        },
        new float[]
        {
            0,
            -1.0f,
            0.0f
        },
    };
    public static void initTitleStage(int stg)
    {
        initFoes();
        initBarrages(GameMath.integer(stagePrm[(stg)][(0)]), stagePrm[(stg)][(1)], stagePrm[(stg)][(2)]);
    }

    public static void initTitle()
    {
        int stg = 0;
        status = TITLE;
        stg = initTitleAtr();
        initShip();
        initShots();
        initFrags();
        initBonuses();
        initBackground();
        setStageBackground(1);
        initTitleStage(stg);
    }

    public static void initGame(int stg)
    {
        status = IN_GAME;
        initShip();
        initShots();
        initFoes();
        initFrags();
        initBonuses();
        initBackground();
        initBarrages(GameMath.integer(stagePrm[(stg)][(0)]), stagePrm[(stg)][(1)], stagePrm[(stg)][(2)]);
        initGameState(stg);
        if (stg < STAGE_NUM)
        {
            setStageBackground(stg % 5 + 1);
            playMusic(stg % 5 + 1);
        }
        else
        {
            if (((!(((insane) != 0)))))
            {
                setStageBackground(0);
                playMusic(0);
            }
            else
            {
                setStageBackground(6);
                playMusic(6);
            }
        }
    }

    public static void initGameover()
    {
        status = GAMEOVER;
        initGameoverAtr();
    }

    public static void initStageClear()
    {
        status = STAGE_CLEAR;
        initStageClearAtr();
    }

    public static void move()
    {
        switch (status)
        {
            case TITLE:
                moveTitleMenu();
                moveBackground();
                addBullets();
                moveFoes();
                break;
            case IN_GAME:
                moveBackground();
                addBullets();
                moveShots();
                moveShip();
                moveFoes();
                moveFrags();
                moveBonuses();
                break;
            case GAMEOVER:
                moveGameover();
                moveBackground();
                addBullets();
                moveShots();
                moveFoes();
                moveFrags();
                break;
            case STAGE_CLEAR:
                moveStageClear();
                moveBackground();
                moveShots();
                moveShip();
                moveFrags();
                moveBonuses();
                break;
            case PAUSE:
                movePause();
                break;
        }
    }

    public static void draw()
    {
        switch (status)
        {
            case TITLE:
                drawBackground();
                drawFoes();
                drawBulletsWake();
                blendScreen();
                drawBullets();
                drawScore();
                drawTitleMenu();
                break;
            case IN_GAME:
                drawBackground();
                drawBonuses();
                drawFoes();
                drawBulletsWake();
                drawFrags();
                blendScreen();
                drawShots();
                drawShip();
                drawBullets();
                drawScore();
                break;
            case GAMEOVER:
                drawBackground();
                drawFoes();
                drawBulletsWake();
                drawFrags();
                blendScreen();
                drawShots();
                drawBullets();
                drawScore();
                drawGameover();
                break;
            case STAGE_CLEAR:
                drawBackground();
                drawBonuses();
                drawFrags();
                blendScreen();
                drawShots();
                drawShip();
                drawScore();
                drawStageClear();
                break;
            case PAUSE:
                drawBackground();
                drawBonuses();
                drawFoes();
                drawBulletsWake();
                drawFrags();
                blendScreen();
                drawShots();
                drawShip();
                drawBullets();
                drawScore();
                drawPause();
                break;
        }
    }

    public static int interval = INTERVAL_BASE;
    public static int tick = 0;
}
