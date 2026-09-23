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

public static class WkCore
{
    public static int aimScore;
    public static int musicChangeScore;
    public static int ballCnt, generatorCnt;
    public static int status;
    public static int missCnt;
    public static void initGame()
    {
        initBalls();
        initPan();
        initBoards();
        initGenerators();
        status = IN_GAME;
        {
            aimScore = 0;
            score = aimScore;
        }

        musicChangeScore = MUSIC_CHANGE_SCORE;
        rank = RANK_BASE;
        ballCnt = 16;
        generatorCnt = 0;
        missCnt = 0;
        playMusic(0);
    }

    public static void addScore(int amount)
    {
        aimScore = aimScore + (amount);
        if (aimScore > SCORE_MAX)
            aimScore = SCORE_MAX;
        if (aimScore > musicChangeScore)
        {
            nextMusic();
            while (aimScore > musicChangeScore)
            {
                musicChangeScore = musicChangeScore + (MUSIC_CHANGE_SCORE);
            }
        }
    }

    public static void moveScore()
    {
        if (score < aimScore)
        {
            score++;
            score = GameMath.integer(score + ((aimScore - score) * 0.05f));
            playChunk(1);
        }
    }

    public static int[] generatorCntSub = new int[]
    {
        4800,
        5600,
        6400,
        6000,
        6500,
        7200,
    };
    public static void addBalls()
    {
        if (ballCnt <= 0)
        {
            addBall(randN(3), randN(3), randN(SCREEN_WIDTH / 2), -randN(16) - 32, randNS(8) * 0.1f, randN(10) * 0.1f);
            ballCnt = RANK_BASE * 48 / rank;
        }

        ballCnt--;
        generatorCnt = generatorCnt + ((rank / (RANK_BASE / 5)));
        if (randN(generatorCnt) > 7200)
        {
            int spc = randN(6);
            addGenerator(spc);
            generatorCnt = generatorCnt - (generatorCntSub[(spc)]);
        }
    }

    public static void move()
    {
        switch (status)
        {
            case TITLE:
                rank = RANK_BASE;
                generatorCnt = 0;
                addBalls();
                moveBalls();
                moveTitle();
                break;
            case IN_GAME:
                addBalls();
                moveBoards();
                moveBalls();
                movePan();
                moveGenerators();
                moveScore();
                break;
            case MISS:
                moveBalls();
                moveScore();
                missCnt++;
                if (missCnt > 120)
                {
                    score = aimScore;
                    if (score > hiScore)
                        hiScore = score;
                    WkPreference.Save();
                    initOver();
                }

                break;
            case GAMEOVER:
                moveOver();
                break;
        }
    }

    public static void draw()
    {
        switch (status)
        {
            case TITLE:
                drawBalls();
                drawTitle();
                break;
            case IN_GAME:
                drawThrownZone();
                drawBoards();
                drawPan();
                drawNum(score, 0, 0);
                drawBalls();
                drawGenerators();
                break;
            case MISS:
                drawThrownZone();
                drawNum(score, 0, 0);
                drawBalls();
                break;
            case GAMEOVER:
                drawOver();
                break;
        }
    }

    public static int rank, score, hiScore = 1000000;
    public static void quitWok()
    {
        WkPreference.Save();
        if (Lub.Host.Available())
            Lub.Host.Send("quit", "");
    }
}
