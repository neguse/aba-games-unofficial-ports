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

public static class NrShip
{
    public static Ship ship = new Ship();
    public static void initShip()
    {
        ship.pos.x = (SCAN_WIDTH / 2) << 8;
        ship.pos.y = (SCAN_HEIGHT / 5 * 4) << 8;
        ship.cnt = 0;
        ship.shotCnt = -1;
        ship.speed = SHIP_SPEED;
        ship.invCnt = SHIP_INVINCIBLE_CNT_BASE * (100 - scene) / 100;
        if (ship.invCnt < 0)
            ship.invCnt = 0;
    }

    public static int[][] shipMv = new int[][]
    {
        new int[]
        {
            0,
            -256
        },
        new int[]
        {
            181,
            -181
        },
        new int[]
        {
            256,
            0
        },
        new int[]
        {
            181,
            181
        },
        new int[]
        {
            0,
            256
        },
        new int[]
        {
            -181,
            181
        },
        new int[]
        {
            -256,
            0
        },
        new int[]
        {
            -181,
            -181
        },
    };
    public static void moveShip()
    {
        int pad = getPadState();
        int btn = getButtonState();
        int sd = -1;
        if (((pad & PAD_RIGHT) != 0))
        {
            sd = 2;
        }

        if (((pad & PAD_LEFT) != 0))
        {
            sd = 6;
        }

        if (((pad & PAD_DOWN) != 0))
        {
            switch (sd)
            {
                case 2:
                    sd = 3;
                    break;
                case 6:
                    sd = 5;
                    break;
                default:
                    sd = 4;
                    break;
            }
        }

        if (((pad & PAD_UP) != 0))
        {
            switch (sd)
            {
                case 2:
                    sd = 1;
                    break;
                case 6:
                    sd = 7;
                    break;
                default:
                    sd = 0;
                    break;
            }
        }

        if (((btn & PAD_BUTTON1) != 0))
        {
            if ((ship.shotCnt < 0) && (status == IN_GAME))
            {
                addShot((ship.pos));
                ship.shotCnt = SHOT_INTERVAL;
            }
        }

        ship.shotCnt--;
        if (((btn & PAD_BUTTON2) != 0))
        {
            if (ship.speed > SHIP_SLOW_SPEED)
            {
                ship.speed = ship.speed - (SHIP_SLOW_DOWN);
            }
        }
        else
        {
            if (ship.speed < SHIP_SPEED)
            {
                ship.speed = ship.speed + (SHIP_SLOW_DOWN);
            }
        }

        if (sd >= 0)
        {
            ship.pos.x = ship.pos.x + (GameMath.signedShift((ship.speed * shipMv[(sd)][(0)]), 8));
            ship.pos.y = ship.pos.y + (GameMath.signedShift((ship.speed * shipMv[(sd)][(1)]), 8));
            if (ship.pos.x < SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.x = SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
            else if (ship.pos.x > SCAN_WIDTH_8 - SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.x = SCAN_WIDTH_8 - SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }

            if (ship.pos.y < SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.y = SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
            else if (ship.pos.y > SCAN_HEIGHT_8 - SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.y = SCAN_HEIGHT_8 - SHIP_SCAN_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
        }

        ship.cnt++;
        if (ship.invCnt > 0)
            ship.invCnt--;
    }

    public static void drawShip()
    {
        int x = 0, y = 0, d = 0;
        int i = 0;
        int ic = 0;
        x = GameMath.signedShift((ship.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        y = GameMath.signedShift((ship.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        d = (ship.cnt * 8) & (DIV / 8 - 1);
        d = d - (DIV / 4);
        ic = ship.invCnt & 31;
        if ((ic > 0) && (ic < 16))
        {
            drawBox(x, y, SHIP_DRAW_WIDTH, SHIP_DRAW_WIDTH, 16 * 2 - 1, 16 * 4 - 5, buf);
            return;
        }

        {
            i = 0;
            for (; i < 4; i++)
            {
                d = d & ((DIV - 1));
                drawBox(x + (GameMath.signedShift((sctbl[(d)] * SHIP_DRUM_WIDTH), 8)), y - (GameMath.signedShift((sctbl[(d + DIV / 4)] * SHIP_DRUM_WIDTH), 10)), SHIP_DRUM_SIZE, SHIP_DRUM_WIDTH * 2, 16 * 3 - 10, 16 * 3 - 12, buf);
                d = d + (DIV / 8);
            }
        }

        drawBox(x, y, SHIP_DRAW_WIDTH, SHIP_DRAW_WIDTH, 16 * 2 - 1, 16 * 4 - 5, buf);
        {
            i = 0;
            for (; i < 4; i++)
            {
                d = d & ((DIV - 1));
                drawBox(x + (GameMath.signedShift((sctbl[(d)] * SHIP_DRUM_WIDTH), 8)), y - (GameMath.signedShift((sctbl[(d + DIV / 4)] * SHIP_DRUM_WIDTH), 10)), SHIP_DRUM_SIZE, SHIP_DRUM_WIDTH * 2, 16 * 3 - 7, 16 * 4 - 11, buf);
                d = d + (DIV / 8);
            }
        }
    }

    public static void destroyShip()
    {
        if ((status != IN_GAME) || (ship.invCnt > 0))
            return;
        addShipFrag((ship.pos));
        playChunk(4);
        resetBonusScore();
        if (((decrementShip()) != 0))
        {
            initGameover();
        }
        else
        {
            ship.invCnt = SHIP_INVINCIBLE_CNT_BASE * (100 - scene) / 100;
            if (ship.invCnt < 0)
                ship.invCnt = 0;
            clearFoesZako();
        }
    }

    public static int getPlayerDeg(int x, int y)
    {
        return getDeg(ship.pos.x - x, ship.pos.y - y);
    }
}
