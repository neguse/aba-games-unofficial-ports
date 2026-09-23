// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrGl;
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

public static class RrCore
{
    public static int status;
    public static void initTitleStage(int stg)
    {
        initFoes();
        initStageState(stg);
    }

    public static void initTitle()
    {
        int stg = 0;
        status = TITLE;
        stg = initTitleAtr();
        initBoss();
        initShip();
        initLasers();
        initFrags();
        initShots();
        initBackground(0);
        initTitleStage(stg);
        left = -1;
    }

    public static void initGame(int stg)
    {
        int sn = 0;
        status = IN_GAME;
        initBoss();
        initFoes();
        initShip();
        initLasers();
        initFrags();
        initShots();
        initGameState(stg);
        sn = stg % SAME_RANK_STAGE_NUM;
        initBackground(sn);
        if (sn == SAME_RANK_STAGE_NUM - 1)
        {
            playMusic(rand() % (SAME_RANK_STAGE_NUM - 1));
        }
        else
        {
            playMusic(sn);
        }
    }

    public static void initGameover()
    {
        status = GAMEOVER;
        initGameoverAtr();
    }

    public static void move()
    {
        switch (status)
        {
            case TITLE:
                moveTitleMenu();
                moveBoss();
                moveFoes();
                moveBackground();
                break;
            case IN_GAME:
            case STAGE_CLEAR:
                moveShip();
                moveBoss();
                moveLasers();
                moveShots();
                moveFoes();
                moveFrags();
                moveBackground();
                break;
            case GAMEOVER:
                moveGameover();
                moveBoss();
                moveFoes();
                moveFrags();
                moveBackground();
                break;
            case PAUSE:
                movePause();
                break;
        }

        moveScreenShake();
    }

    public static void draw()
    {
        switch (status)
        {
            case TITLE:
                drawBackground();
                drawBoss();
                drawBulletsWake();
                drawBullets();
                startDrawBoards();
                drawSideBoards();
                drawTitle();
                endDrawBoards();
                break;
            case IN_GAME:
            case STAGE_CLEAR:
                drawBackground();
                drawBoss();
                drawLasers();
                drawShots();
                drawBulletsWake();
                drawFrags();
                drawShip();
                drawBullets();
                startDrawBoards();
                drawSideBoards();
                drawBossState();
                endDrawBoards();
                break;
            case GAMEOVER:
                drawBackground();
                drawBoss();
                drawBulletsWake();
                drawFrags();
                drawBullets();
                startDrawBoards();
                drawSideBoards();
                drawGameover();
                endDrawBoards();
                break;
            case PAUSE:
                drawBackground();
                drawBoss();
                drawLasers();
                drawShots();
                drawBulletsWake();
                drawFrags();
                drawShip();
                drawBullets();
                startDrawBoards();
                drawSideBoards();
                drawBossState();
                drawPause();
                endDrawBoards();
                break;
        }
    }

    public static int interval = INTERVAL_BASE;
    public static int tick = 0;
}
