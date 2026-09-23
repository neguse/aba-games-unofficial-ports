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

public static class NrBackground
{
    public static Board[] board = Make(BOARD_MAX, () => new Board());
    public static void initBackground()
    {
        int i = 0;
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                board[(i)].width = NOT_EXIST;
            }
        }
    }

    public static int bdIdx;
    public static int boardMx, boardMy;
    public static int boardRepx, boardRepy;
    public static int boardRepXn, boardRepYn;
    public static void addBoard(int x, int y, int z, int width, int height)
    {
        if (bdIdx >= BOARD_MAX)
            return;
        board[(bdIdx)].x = x;
        board[(bdIdx)].y = y;
        board[(bdIdx)].z = z;
        board[(bdIdx)].width = width / z;
        board[(bdIdx)].height = height / z;
        bdIdx++;
    }

    public static void setStageBackground(int bn)
    {
        int i = 0, j = 0, k = 0;
        bdIdx = 0;
        switch (bn)
        {
            case 0:
            case 6:
                addBoard(9000, 9000, 500, 25000, 25000);
            {
                i = 0;
                for (; i < 4; i++)
                {
                    {
                        j = 0;
                        for (; j < 4; j++)
                        {
                            if ((i > 1) || (j > 1))
                            {
                                addBoard(i * 16384, j * 16384, 500, 10000 + (i * 12345) % 3000, 10000 + (j * 54321) % 3000);
                            }
                        }
                    }
                }
            }

            {
                j = 0;
                for (; j < 8; j++)
                {
                    {
                        i = 0;
                        for (; i < 4; i++)
                        {
                            addBoard(0, i * 16384, 500 - j * 50, 20000 - j * 1000, 12000 - j * 500);
                        }
                    }
                }
            }

            {
                i = 0;
                for (; i < 8; i++)
                {
                    addBoard(0, i * 8192, 100, 20000, 6400);
                }
            }

                if (bn == 0)
                {
                    boardMx = 40;
                    boardMy = 300;
                }
                else
                {
                    boardMx = -40;
                    boardMy = 480;
                }

            {
                boardRepy = 65536;
                boardRepx = boardRepy;
            }

            {
                boardRepYn = 4;
                boardRepXn = boardRepYn;
            }

                break;
            case 1:
                addBoard(12000, 12000, 400, 48000, 48000);
                addBoard(12000, 44000, 400, 48000, 8000);
                addBoard(44000, 12000, 400, 8000, 48000);
            {
                i = 0;
                for (; i < 16; i++)
                {
                    addBoard(0, 0, 400 - i * 10, 16000, 16000);
                    if (i < 6)
                    {
                        addBoard(9600, 16000, 400 - i * 10, 40000, 16000);
                    }
                }
            }

                boardMx = 128;
                boardMy = 512;
            {
                boardRepy = 65536;
                boardRepx = boardRepy;
            }

            {
                boardRepYn = 4;
                boardRepXn = boardRepYn;
            }

                break;
            case 2:
            {
                i = 0;
                for (; i < 16; i++)
                {
                    addBoard(7000 + i * 3000, 0, 1600 - i * 100, 24000, 5000);
                    addBoard(7000 + i * 3000, 50000, 1600 - i * 100, 4000, 10000);
                    addBoard(-7000 - i * 3000, 0, 1600 - i * 100, 24000, 5000);
                    addBoard(-7000 - i * 3000, 50000, 1600 - i * 100, 4000, 10000);
                }
            }

                boardMx = 0;
                boardMy = 1200;
                boardRepx = 0;
                boardRepy = 65536;
                boardRepXn = 1;
                boardRepYn = 10;
                break;
            case 3:
                addBoard(9000, 9000, 500, 30000, 30000);
            {
                i = 0;
                for (; i < 4; i++)
                {
                    {
                        j = 0;
                        for (; j < 4; j++)
                        {
                            if ((i > 1) || (j > 1))
                            {
                                addBoard(i * 16384, j * 16384, 500, 12000 + (i * 12345) % 3000, 12000 + (j * 54321) % 3000);
                            }
                        }
                    }
                }
            }

            {
                i = 0;
                for (; i < 4; i++)
                {
                    {
                        j = 0;
                        for (; j < 4; j++)
                        {
                            if ((((i > 1) || (j > 1))) && ((i + j) % 3 == 0))
                            {
                                addBoard(i * 16384, j * 16384, 480, 9000 + (i * 12345) % 3000, 9000 + (j * 54321) % 3000);
                            }
                        }
                    }
                }
            }

                addBoard(9000, 9000, 480, 20000, 20000);
                addBoard(9000, 9000, 450, 20000, 20000);
                addBoard(32768, 40000, 420, 65536, 5000);
                addBoard(30000, 32768, 370, 4800, 65536);
                addBoard(32768, 0, 8, 65536, 10000);
                boardMx = 10;
                boardMy = 100;
            {
                boardRepy = 65536;
                boardRepx = boardRepy;
            }

            {
                boardRepYn = 4;
                boardRepXn = boardRepYn;
            }

                break;
            case 4:
                addBoard(32000, 12000, 160, 48000, 48000);
                addBoard(32000, 44000, 160, 48000, 8000);
                addBoard(64000, 12000, 160, 8000, 48000);
            {
                i = 0;
                for (; i < 16; i++)
                {
                    addBoard(20000, 0, 160 - i * 10, 16000, 16000);
                    if (i < 6)
                    {
                        addBoard(29600, 16000, 160 - i * 10, 40000, 16000);
                    }
                }
            }

                boardMx = 0;
                boardMy = 128;
            {
                boardRepy = 65536;
                boardRepx = boardRepy;
            }

                boardRepXn = 2;
                boardRepYn = 2;
                break;
            case 5:
            {
                k = 0;
                for (; k < 5; k++)
                {
                    j = 0;
                    {
                        i = 0;
                        for (; i < 16; i++)
                        {
                            addBoard(j, i * 4096, 200 - k * 10, 16000, 4096);
                            addBoard(j + 16000 - j * 2, i * 4096, 200 - k * 10, 16000, 4096);
                            if (i < 4)
                                j = j + (2000);
                            else if (i < 6)
                                j = j - (3500);
                            else if (i < 12)
                                j = j + (1500);
                            else
                                j = j - (2000);
                        }
                    }
                }
            }

                boardMx = -10;
                boardMy = 25;
            {
                boardRepy = 65536;
                boardRepx = boardRepy;
            }

            {
                boardRepYn = 2;
                boardRepXn = boardRepYn;
            }

                break;
        }
    }

    public static void moveBackground()
    {
        int i = 0;
        Board bd = null;
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                if (board[(i)].width == NOT_EXIST)
                    continue;
                bd = (board[(i)]);
                bd.x = bd.x + (boardMx);
                bd.y = bd.y + (boardMy);
                bd.x = bd.x & ((boardRepx - 1));
                bd.y = bd.y & ((boardRepy - 1));
            }
        }
    }

    public static void drawBackground()
    {
        int i = 0;
        Board bd = null;
        int ox = 0, oy = 0, osx = 0, osy = 0, rx = 0, ry = 0;
        osx = -boardRepx * (boardRepXn / 2);
        osy = -boardRepy * (boardRepYn / 2);
        {
            i = 0;
            for (; i < BOARD_MAX; i++)
            {
                if (board[(i)].width == NOT_EXIST)
                    continue;
                bd = (board[(i)]);
                ox = osx;
                {
                    rx = 0;
                    for (; rx < boardRepXn; rx++, ox = ox + (boardRepx))
                    {
                        oy = osy;
                        {
                            ry = 0;
                            for (; ry < boardRepYn; ry++, oy = oy + (boardRepy))
                            {
                                drawBox((bd.x + ox) / bd.z + LAYER_WIDTH / 2, (bd.y + oy) / bd.z + LAYER_HEIGHT / 2, bd.width, bd.height, 1, 3, l1buf);
                            }
                        }
                    }
                }
            }
        }
    }
}
