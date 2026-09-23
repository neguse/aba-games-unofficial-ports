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

public static class RrLaser
{
    public static Laser[] laser = Make(LASER_MAX, () => new Laser());
    public static int laserWidth, laserCnt;
    public static void initLasers()
    {
        int i = 0;
        {
            i = 0;
            for (; i < LASER_MAX; i++)
            {
                laser[(i)].cnt = NOT_EXIST;
            }
        }

        {
            laserCnt = 0;
            laserWidth = laserCnt;
        }
    }

    public static int laserIdx = LASER_MAX;
    public static int laserColor = 0;
    public static int laserAdded = 0;
    public static void addLaser()
    {
        int i = 0;
        {
            i = 0;
            for (; i < LASER_MAX; i++)
            {
                laserIdx--;
                if (laserIdx < 0)
                    laserIdx = LASER_MAX - 1;
                if (laser[(laserIdx)].cnt == NOT_EXIST)
                    break;
            }
        }

        if (i >= LASER_MAX)
            return;
        laser[(laserIdx)].y = LASER_SPEED;
        laser[(laserIdx)].color = laserColor;
        laserColor = laserColor - (LASER_COLOR_SPEED);
        laserColor = laserColor & (255);
        laser[(laserIdx)].cnt = 0;
        laserWidth = laserWidth + (LASER_WIDTH_ADD);
        if (laserWidth > LASER_WIDTH)
            laserWidth = LASER_WIDTH;
        laserAdded = 1;
    }

    public static void moveLasers()
    {
        int i = 0;
        Laser ls = null;
        int ry = 0;
        int huy = 0, hdy = 0;
        if (!(((laserAdded) != 0)))
        {
            laserWidth = laserWidth - (LASER_WIDTH_ADD);
            if (laserWidth < 0)
            {
                laserWidth = 0;
                {
                    i = 0;
                    for (; i < LASER_MAX; i++)
                    {
                        laser[(i)].cnt = NOT_EXIST;
                    }
                }

                return;
            }
        }
        else
        {
            laserAdded = 0;
        }

        hdy = checkHitDownside(ship.pos.x);
        huy = checkHitUpside();
        {
            i = 0;
            for (; i < LASER_MAX; i++)
            {
                if (laser[(i)].cnt == NOT_EXIST)
                    continue;
                else if (laser[(i)].cnt == -1)
                {
                    laser[(i)].cnt = NOT_EXIST;
                    continue;
                }

                ls = (laser[(i)]);
                ls.y = ls.y - (LASER_SPEED);
                ry = ship.pos.y + ls.y;
                if ((huy < ry) && (ry < hdy))
                {
                    damageBossLaser(ls.cnt);
                    if ((laserCnt & 3) == 0)
                    {
                        addLaserFrag(ship.pos.x, -ship.pos.y - ls.y, LASER_WIDTH);
                    }

                    ls.cnt = -1;
                    continue;
                }

                if (ry < GameMath.integer(-FIELD_HEIGHT_8 / 2))
                {
                    ls.cnt = NOT_EXIST;
                    continue;
                }

                ls.cnt++;
            }
        }

        laserCnt++;
    }

    public static void drawLasers()
    {
        float x = 0, y = 0;
        int i = 0;
        Laser ls = null;
        int t = 0;
        {
            i = 0;
            for (; i < LASER_MAX; i++)
            {
                if (laser[(i)].cnt == NOT_EXIST)
                    continue;
                ls = (laser[(i)]);
                x = (float)ship.pos.x / FIELD_SCREEN_RATIO;
                y = -(float)(ship.pos.y + ls.y) / FIELD_SCREEN_RATIO;
                if (ls.cnt > 1)
                    t = 1;
                else if (ls.cnt == 1)
                    t = 0;
                else
                    t = 2;
                drawLaser(x, y, (float)laserWidth / FIELD_SCREEN_RATIO, LASER_SCREEN_HEIGHT, ls.color, (ls.color + LASER_COLOR_SPEED) & 255, (ls.color + LASER_COLOR_SPEED * 2) & 255, (ls.color + LASER_COLOR_SPEED * 3) & 255, laserCnt, t);
            }
        }
    }
}
