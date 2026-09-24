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

public static class RrShip
{
    public static Ship ship = new Ship();
    public static int bonusScore;
    public static int bomb;
    public static void resetPlayer()
    {
        bonusScore = 10;
        bomb = 3;
        {
            ship.grzf = 0;
            ship.rollingCnt = ship.grzf;
            ship.grzInvCnt = ship.rollingCnt;
        }

        ship.grzWdt = GRZ_WIDTH;
        ship.absEng = 0;
        {
            ship.rfMtrDec = 0;
            ship.rfCnt = ship.rfMtrDec;
        }

        ship.rfMtr = RF_METER_MAX;
        ship.reflects = 0;
    }

    public static int btn2f;
    public static void initShip()
    {
        ship.pos.x = 0;
        ship.pos.y = GameMath.integer(FIELD_HEIGHT_8 / 5) * 2;
        ship.cnt = 0;
        ship.laserCnt = 0;
        ship.invCnt = SHIP_INVINCIBLE_CNT_BASE;
        ship.bombCnt = 0;
        ship.d = 0;
        ship.grzCnt = 0;
        {
            ship.colorChgCnt = 0;
            ship.color = ship.colorChgCnt;
        }

        ship.fldWdt = FLD_WIDTH;
        btn2f = 0;
        switch (mode)
        {
            case NORMAL_MODE:
                ship.speed = SHIP_SPEED;
                break;
            case PSY_MODE:
                ship.speed = SHIP_SLOW_SPEED;
                break;
            case IKA_MODE:
            case GW_MODE:
                ship.speed = SHIP_IKAGW_SPEED;
                break;
        }

        resetPlayer();
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
        Vector bossPos = new Vector();
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
            if (status == IN_GAME)
            {
                addLaser();
                if (ship.laserCnt == 0)
                {
                    playChunk(0);
                }
                else
                {
                    if ((ship.laserCnt & 15) == 1)
                        playChunk(1);
                }

                ship.laserCnt++;
            }

            switch (mode)
            {
                case NORMAL_MODE:
                    if (ship.speed > SHIP_SLOW_SPEED)
                    {
                        ship.speed = ship.speed - (SHIP_SLOW_DOWN);
                    }

                    break;
            }
        }
        else
        {
            switch (mode)
            {
                case NORMAL_MODE:
                    if (ship.speed < SHIP_SPEED)
                    {
                        ship.speed = ship.speed + (SHIP_SLOW_DOWN);
                    }

                    break;
            }

            if (ship.laserCnt > 0)
            {
                haltChunk(1);
                ship.laserCnt = 0;
            }
        }

        if (((((btn & PAD_BUTTON2)) != 0)) && (status == IN_GAME))
        {
            switch (mode)
            {
                case NORMAL_MODE:
                    if (((bomb > 0) && (((ship.bombCnt <= 0) || (ship.bombCnt > BOMB_NEXT_READY_CNT)))) && (ship.invCnt < SHIP_INVINCIBLE_CNT_BASE - 30))
                    {
                        bomb--;
                        bombUsed++;
                        ship.bombCnt = 1;
                        ship.bombWdt = 0;
                        ship.bombPos = new Vector
                        {
                            x = ship.pos.x,
                            y = ship.pos.y
                        };
                        ship.invCnt = GameMath.integer(SHIP_INVINCIBLE_CNT_BASE / 2);
                        playChunk(3);
                    }

                    break;
                case PSY_MODE:
                    ship.rollingCnt = ROLLING_CNT_BASE;
                    if (ship.speed > SHIP_ROLLING_SPEED)
                    {
                        ship.speed = ship.speed - (SHIP_SLOW_DOWN);
                    }

                    break;
                case IKA_MODE:
                    if ((((btn2f) != 0)) && (ship.colorChgCnt <= 0))
                    {
                        ship.colorChgCnt = COLOR_CHG_CNT;
                        btn2f = 0;
                        playChunk(12);
                    }

                    break;
                case GW_MODE:
                    if (ship.rfMtr >= RF_METER_MAX)
                    {
                        ship.rfMtrDec = ship.rfMtrDec + (RF_METER_DEC);
                        if (ship.rfMtrDec >= RF_METER_MAX)
                        {
                            {
                                ship.rfMtr = 0;
                                ship.rfMtrDec = ship.rfMtr;
                            }

                            ship.rfCnt = 120;
                            ship.invCnt = 150;
                            playChunk(13);
                        }
                    }

                    break;
            }
        }
        else
        {
            switch (mode)
            {
                case PSY_MODE:
                    if (ship.rollingCnt > 0)
                    {
                        ship.rollingCnt--;
                        if (ship.speed < SHIP_SPEED)
                        {
                            ship.speed = ship.speed + (SHIP_SLOW_DOWN);
                        }
                    }
                    else
                    {
                        if (ship.speed < SHIP_SLOW_SPEED)
                        {
                            ship.speed = ship.speed + (SHIP_SLOW_DOWN);
                        }
                        else if (ship.speed > SHIP_SLOW_SPEED)
                        {
                            ship.speed = ship.speed - (SHIP_SLOW_DOWN);
                        }
                    }

                    break;
                case IKA_MODE:
                    btn2f = 1;
                    break;
                case GW_MODE:
                    ship.rfMtrDec = 0;
                    break;
            }
        }

        switch (mode)
        {
            case NORMAL_MODE:
                if (ship.bombCnt > 0)
                {
                    ship.bombCnt++;
                    ship.bombPos.y = ship.bombPos.y - (BOMB_UP_SPEED);
                    if (ship.bombCnt < 20)
                    {
                        ship.bombWdt = ship.bombWdt + (BOMB_SPEED);
                    }
                    else if (ship.bombCnt > BOMB_CNT - 20)
                    {
                        ship.bombWdt = ship.bombWdt - (BOMB_SPEED);
                    }

                    wipeBullets((ship.bombPos), ship.bombWdt);
                    if (ship.bombCnt > BOMB_CNT)
                        ship.bombCnt = 0;
                }

                ship.d = ship.d + (2.8f);
                break;
            case PSY_MODE:
                if ((ship.rollingCnt > 0) && (ship.grzWdt == GRZ_WIDTH))
                {
                    ship.grzWdt = GRZ_WIDTH * 2;
                }
                else
                {
                    ship.grzWdt = GRZ_WIDTH;
                }

                if (ship.grzCnt >= INVINCIBLE_GRZ_NUM)
                {
                    int i = 0;
                    ship.grzCnt = ship.grzCnt - (INVINCIBLE_GRZ_NUM);
                    ship.grzInvCnt = INVINCIBLE_GRZ_CNT;
                    {
                        i = 0;
                        for (; i < 32; i++)
                        {
                            addGrazeFrag(ship.pos.x, ship.pos.y, randNS(30000), randNS(30000));
                        }
                    }

                    ship.invCnt = 0;
                    playChunk(10);
                }

                if (ship.grzInvCnt > 0)
                {
                    ship.grzInvCnt--;
                }

                ship.d = ship.d + (ship.rollingCnt * 0.3f);
                if (((ship.grzf) != 0))
                {
                    playChunk(9);
                    ship.grzf = 0;
                }

                break;
            case IKA_MODE:
                if (ship.colorChgCnt > 0)
                {
                    ship.colorChgCnt--;
                    if (ship.colorChgCnt == GameMath.integer(COLOR_CHG_CNT / 2))
                    {
                        ship.color = ship.color ^ (1);
                    }

                    if (ship.colorChgCnt < GameMath.integer(COLOR_CHG_CNT / 2))
                    {
                        ship.fldWdt = GameMath.integer((GameMath.integer(COLOR_CHG_CNT / 2) - ship.colorChgCnt) * FLD_WIDTH / (GameMath.integer(COLOR_CHG_CNT / 2)));
                    }
                    else
                    {
                        ship.fldWdt = GameMath.integer((ship.colorChgCnt - GameMath.integer(COLOR_CHG_CNT / 2)) * FLD_WIDTH / (GameMath.integer(COLOR_CHG_CNT / 2)));
                    }
                }

                if (ship.absEng >= ABSENG_CNT)
                {
                    ship.absEng = ship.absEng - (ABSENG_CNT);
                    bossPos = getBossPos();
                    addShot(ship.pos.x, ship.pos.y, bossPos.x - ship.pos.x, bossPos.y - ship.pos.y, ship.color);
                    playChunk(11);
                }

                ship.d = ship.d + (1.5f);
                break;
            case GW_MODE:
                if (ship.rfCnt > 0)
                {
                    if (ship.rfCnt > 20)
                    {
                        ship.rfWdt = GameMath.integer(RF_WIDTH * (ship.rfCnt - 20) / 200) + GameMath.integer(RF_WIDTH / 2);
                    }
                    else
                    {
                        ship.rfWdt = GameMath.integer(RF_WIDTH * (20 - ship.rfCnt) / 8) + GameMath.integer(RF_WIDTH / 2);
                        if (ship.rfCnt == 20)
                            playChunk(14);
                    }

                    ship.rfCnt--;
                }
                else if (ship.rfMtr < RF_METER_MAX)
                {
                    ship.rfMtr = ship.rfMtr + (RF_METER_INC);
                    if (ship.rfMtr >= RF_METER_MAX)
                    {
                        ship.rfMtr = RF_METER_MAX;
                        playChunk(15);
                    }
                }

                if (((ship.reflects) != 0))
                {
                    ship.reflects = 0;
                    playChunk(11);
                }

                ship.d = ship.d + (3.2f);
                break;
        }

        if (sd >= 0)
        {
            ship.pos.x = ship.pos.x + (GameMath.signedShift((ship.speed * shipMv[(sd)][(0)]), 8));
            ship.pos.y = ship.pos.y + (GameMath.signedShift((ship.speed * shipMv[(sd)][(1)]), 8));
            if (ship.pos.x < GameMath.integer(-FIELD_WIDTH_8 / 2) + SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.x = GameMath.integer(-FIELD_WIDTH_8 / 2) + SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
            else if (ship.pos.x > GameMath.integer(FIELD_WIDTH_8 / 2) - SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.x = GameMath.integer(FIELD_WIDTH_8 / 2) - SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }

            if (ship.pos.y < GameMath.integer(-FIELD_HEIGHT_8 / 2) + SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.y = GameMath.integer(-FIELD_HEIGHT_8 / 2) + SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
            else if (ship.pos.y > GameMath.integer(FIELD_HEIGHT_8 / 2) - SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH)
            {
                ship.pos.y = GameMath.integer(FIELD_HEIGHT_8 / 2) - SHIP_FIELD_WIDTH * SHIP_SCREEN_EDGE_WIDTH;
            }
        }

        ship.cnt++;
        if (ship.invCnt > 0)
            ship.invCnt--;
    }

    public static void drawShip(float[] model, Gfx.Blend blend, string key)
    {
        float x = 0, y = 0, bx = 0, by = 0;
        int inv = 0, ic = 0;
        x = (float)ship.pos.x / FIELD_SCREEN_RATIO;
        y = -(float)ship.pos.y / FIELD_SCREEN_RATIO;
        switch (mode)
        {
            case NORMAL_MODE:
                if (ship.bombCnt > 0)
                {
                    bx = (float)ship.bombPos.x / FIELD_SCREEN_RATIO;
                    by = -(float)ship.bombPos.y / FIELD_SCREEN_RATIO;
                    drawBomb(model, blend, key + "-456", bx, by, (float)ship.bombWdt / FIELD_SCREEN_RATIO, ship.bombCnt);
                }

                break;
            case PSY_MODE:
                if (ship.grzInvCnt > 0)
                {
                    drawCircle(model, blend, key + "-677", x, y, 0.01f * ship.grzInvCnt, ship.grzInvCnt, 150, 180, 240, 220, 220, 230);
                }

                break;
            case IKA_MODE:
                if (ship.color == 0)
                {
                    drawCircle(model, blend, key + "-909", x, y, (float)ship.fldWdt / FIELD_SCREEN_RATIO, ship.cnt, 120, 120, 150, 255, 255, 255);
                }
                else
                {
                    drawCircle(model, blend, key + "-1085", x, y, (float)ship.fldWdt / FIELD_SCREEN_RATIO, ship.cnt, 200, 0, 0, 100, 0, 0);
                }

                break;
            case GW_MODE:
                if (ship.rfCnt > 0)
                {
                    drawCircle(model, blend, key + "-1318", x, y, (float)ship.rfWdt / FIELD_SCREEN_RATIO, ship.cnt, 200, 250, 200, 100, 200, 100);
                }

                break;
        }

        ic = ship.invCnt & 31;
        if ((ic > 0) && (ic < 16))
            inv = 1;
        drawShipShape(model, blend, key + "-1564", x, y, ship.d, inv);
    }

    public static void destroyShip()
    {
        float x = 0, y = 0;
        if (((status != IN_GAME) || (ship.invCnt > 0)) || (ship.grzInvCnt > 0))
            return;
        x = (float)ship.pos.x / FIELD_SCREEN_RATIO;
        y = -(float)ship.pos.y / FIELD_SCREEN_RATIO;
        addShipFrag(x, y);
        playChunk(7);
        shipUsed++;
        resetPlayer();
        if (((decrementShip()) != 0))
        {
            initGameover();
        }
        else
        {
            ship.invCnt = SHIP_INVINCIBLE_CNT_BASE;
            clearFoesZako();
        }
    }

    public static int getPlayerDeg(int x, int y)
    {
        return getDeg(ship.pos.x - x, ship.pos.y - y);
    }
}
