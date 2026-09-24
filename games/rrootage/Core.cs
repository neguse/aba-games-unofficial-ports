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

    public static void draw(float[] model, Gfx.Blend blend, string key)
    {
        switch (status)
        {
            case TITLE:
                drawBackground(model, blend, key + "-75");
                drawBoss(model, blend, key + "-136");
                drawBulletsWake(model, blend, key + "-137");
                drawBullets(model, blend, key + "-227");
                model = Transform.Ortho();
                drawSideBoards(model, blend, key + "-301");
                drawTitle(model, blend, key + "-391");

                break;
            case IN_GAME:
            case STAGE_CLEAR:
                drawBackground(model, blend, key + "-433");
                drawBoss(model, blend, key + "-606");
                drawLasers(model, blend, key + "-606");
                drawShots(model, blend, key + "-692");
                drawBulletsWake(model, blend, key + "-526");
                drawFrags(model, blend, key + "-784");
                drawShip(model, blend, key + "-925");
                drawBullets(model, blend, key + "-757");
                model = Transform.Ortho();
                drawSideBoards(model, blend, key + "-803");
                drawBossState(model, blend, key + "-893");

                break;
            case GAMEOVER:
                drawBackground(model, blend, key + "-882");
                drawBoss(model, blend, key + "-1251");
                drawBulletsWake(model, blend, key + "-888");
                drawFrags(model, blend, key + "-1286");
                drawBullets(model, blend, key + "-1203");
                model = Transform.Ortho();
                drawSideBoards(model, blend, key + "-1193");
                drawGameover(model, blend, key + "-1340");

                break;
            case PAUSE:
                drawBackground(model, blend, key + "-1240");
                drawBoss(model, blend, key + "-1754");
                drawLasers(model, blend, key + "-1641");
                drawShots(model, blend, key + "-1813");
                drawBulletsWake(model, blend, key + "-1277");
                drawFrags(model, blend, key + "-1849");
                drawShip(model, blend, key + "-2135");
                drawBullets(model, blend, key + "-1736");
                model = Transform.Ortho();
                drawSideBoards(model, blend, key + "-1668");
                drawBossState(model, blend, key + "-1816");
                drawPause(model, blend, key + "-2106");

                break;
        }
    }

    public static int interval = INTERVAL_BASE;
    public static int tick = 0;
}
