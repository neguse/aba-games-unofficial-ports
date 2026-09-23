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

public static class WkBoard
{
    public static Board[] board = Make(BOARD_MAX, () => new Board());
    public static void initBoards()
    {
        int i = 0;
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                board[(i)].cnt = 0;
            }
        }
    }

    public static int boardIdx = BOARD_MAX;
    public static void addBoard(float x, float y, float mx, float my, int sc, int mp, int cnt)
    {
        boardIdx--;
        if (boardIdx < 0)
            boardIdx = BOARD_MAX - 1;
        board[(boardIdx)].x = x;
        board[(boardIdx)].y = y;
        board[(boardIdx)].mx = mx;
        board[(boardIdx)].my = my;
        board[(boardIdx)].sc = sc;
        board[(boardIdx)].mp = mp;
        board[(boardIdx)].cnt = cnt;
        board[(boardIdx)].apCnt = 0;
    }

    public static void moveBoards()
    {
        int i = 0;
        Board bd = null;
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                if (board[(i)].cnt <= 0)
                    continue;
                bd = (board[(i)]);
                bd.x = bd.x + (bd.mx);
                bd.y = bd.y + (bd.my);
                if (bd.cnt == 1)
                {
                    bd.apCnt = bd.apCnt - (2);
                    if (bd.apCnt <= 0)
                        bd.cnt = 0;
                }
                else
                {
                    if (bd.apCnt >= 16)
                    {
                        bd.cnt--;
                    }
                    else
                    {
                        bd.apCnt++;
                    }
                }
            }
        }
    }

    public static void drawBoards()
    {
        int i = 0, x = 0, pc = 0;
        Board bd = null;
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                if (board[(i)].cnt <= 0)
                    continue;
                bd = (board[(i)]);
                if (bd.apCnt >= 16)
                {
                    x = drawNum(bd.sc, GameMath.integer(bd.x), GameMath.integer(bd.y));
                    drawSprite(NUM_SPRITE_IDX + 10, x, GameMath.integer(bd.y));
                    x = x + (50);
                    drawNum(bd.mp, x, GameMath.integer(bd.y));
                }
                else
                {
                    pc = bd.apCnt / 2;
                    x = drawNumPaper(1, GameMath.integer(bd.x), GameMath.integer(bd.y), pc);
                    pc--;
                    if (pc < 0)
                        pc = 0;
                    x = drawNumPaper(1, x, GameMath.integer(bd.y), pc);
                    pc--;
                    if (pc < 0)
                        pc = 0;
                    drawNumPaper(bd.mp, x, GameMath.integer(bd.y), pc);
                }
            }
        }
    }
}
