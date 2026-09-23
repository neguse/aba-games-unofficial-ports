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

public static class RrBackground
{
    public static Plane[] plane = Make(PLANE_MAX, () => new Plane());
    public static int planeNum;
    public static void initBackground(int s)
    {
        int i = 0;
        Plane pl = null;
        switch (s)
        {
            case 0:
                planeNum = 3;
            {
                i = 0;
                for (; i < planeNum; i++)
                {
                    pl = (plane[(i)]);
                    pl.xn = 8;
                    pl.yn = 16;
                    pl.width = 3.0f;
                    pl.height = 2.0f;
                    pl.x = -pl.width * pl.xn / 2;
                    pl.y = -pl.height * pl.yn / 2;
                    pl.z = -10.0f;
                    {
                        pl.oy = 0;
                        pl.ox = pl.oy;
                    }

                    switch (i)
                    {
                        case 0:
                            pl.r = 250;
                            pl.g = 200;
                            pl.b = 20;
                            pl.a = 100;
                            pl.d1 = 0;
                            pl.mx = 0;
                            pl.my = 0.05f;
                            break;
                        case 1:
                            pl.r = 200;
                            pl.g = 150;
                            pl.b = 20;
                            pl.a = 50;
                            pl.d1 = 32;
                            pl.mx = 0.04f;
                            pl.my = 0.05f;
                            break;
                        case 2:
                            pl.r = 200;
                            pl.g = 150;
                            pl.b = 20;
                            pl.a = 50;
                            pl.d1 = -32 & 1023;
                            pl.mx = -0.04f;
                            pl.my = 0.05f;
                            break;
                    }
                }
            }

                break;
            case 1:
                planeNum = 2;
            {
                i = 0;
                for (; i < planeNum; i++)
                {
                    pl = (plane[(i)]);
                    pl.xn = 8;
                    pl.yn = 8;
                    pl.width = 3.0f;
                    pl.height = 3.0f;
                    pl.x = -pl.width * pl.xn / 2;
                    pl.y = -pl.height * pl.yn / 2;
                    pl.z = -10.0f;
                    {
                        pl.oy = 0;
                        pl.ox = pl.oy;
                    }

                    switch (i)
                    {
                        case 0:
                            pl.r = 200;
                            pl.g = 100;
                            pl.b = 200;
                            pl.a = 150;
                            pl.d1 = 4;
                            pl.mx = 0;
                            pl.my = 0.12f;
                            break;
                        case 1:
                            pl.r = 120;
                            pl.g = 100;
                            pl.b = 120;
                            pl.a = 120;
                            pl.d1 = 4;
                            pl.mx = 0;
                            pl.my = -0.05f;
                            break;
                    }
                }
            }

                break;
            case 2:
                planeNum = 3;
            {
                i = 0;
                for (; i < planeNum; i++)
                {
                    pl = (plane[(i)]);
                    switch (i)
                    {
                        case 0:
                            pl.xn = 6;
                            pl.yn = 6;
                            pl.width = 5.0f;
                            pl.height = 5.0f;
                            break;
                        case 1:
                        case 2:
                            pl.xn = 16;
                            pl.yn = 16;
                            pl.width = 1.5f;
                            pl.height = 1.5f;
                            break;
                    }

                    pl.x = -pl.width * pl.xn / 2;
                    pl.y = -pl.height * pl.yn / 2;
                    pl.z = -10.0f;
                    {
                        pl.oy = 0;
                        pl.ox = pl.oy;
                    }

                    switch (i)
                    {
                        case 0:
                            pl.r = 150;
                            pl.g = 200;
                            pl.b = 150;
                            pl.a = 125;
                            pl.d1 = 0;
                            pl.mx = 0;
                            pl.my = 0.04f;
                            break;
                        case 1:
                            pl.r = 170;
                            pl.g = 200;
                            pl.b = 170;
                            pl.a = 60;
                            pl.d1 = 0;
                            pl.mx = 0.01f;
                            pl.my = 0.01f;
                            break;
                        case 2:
                            pl.r = 170;
                            pl.g = 200;
                            pl.b = 170;
                            pl.a = 60;
                            pl.d1 = 0;
                            pl.mx = -0.01f;
                            pl.my = 0.01f;
                            break;
                    }
                }
            }

                break;
            case 3:
                planeNum = 4;
            {
                i = 0;
                for (; i < planeNum; i++)
                {
                    pl = (plane[(i)]);
                    pl.xn = 8;
                    pl.yn = 16;
                    pl.width = 4.0f;
                    pl.height = 2.5f;
                    pl.x = -pl.width * pl.xn / 2;
                    pl.y = -pl.height * pl.yn / 2;
                    pl.z = -10.0f;
                    {
                        pl.oy = 0;
                        pl.ox = pl.oy;
                    }

                    pl.r = 200;
                    pl.g = 200;
                    pl.b = 100;
                    switch (i)
                    {
                        case 0:
                            pl.a = 72;
                            pl.d1 = 0;
                            pl.mx = -0.05f;
                            pl.my = 0.1f;
                            break;
                        case 1:
                            pl.a = 40;
                            pl.d1 = 10;
                            pl.mx = -0.025f;
                            pl.my = 0;
                            break;
                        case 2:
                            pl.a = 40;
                            pl.d1 = -10;
                            pl.mx = 0.025f;
                            pl.my = 0;
                            break;
                        case 3:
                            pl.a = 72;
                            pl.d1 = 0;
                            pl.mx = -0.025f;
                            pl.my = 0.1f;
                            break;
                    }
                }
            }

                break;
        }
    }

    public static void moveBackground()
    {
        int i = 0;
        Plane pl = null;
        {
            i = 0;
            for (; i < planeNum; i++)
            {
                pl = (plane[(i)]);
                pl.ox = pl.ox - (pl.mx);
                if (pl.ox < 0)
                    pl.ox = pl.ox + (pl.width);
                if (pl.ox >= pl.width)
                    pl.ox = pl.ox - (pl.width);
                pl.oy = pl.oy - (pl.my);
                if (pl.oy < 0)
                    pl.oy = pl.oy + (pl.height);
                if (pl.oy >= pl.height)
                    pl.oy = pl.oy - (pl.height);
            }
        }
    }

    public static void drawBackground()
    {
        int lx = 0, ly = 0, i = 0;
        float x = 0, y = 0;
        Plane pl = null;
        {
            i = 0;
            for (; i < planeNum; i++)
            {
                pl = (plane[(i)]);
                x = pl.x + pl.ox;
                {
                    lx = 0;
                    for (; lx < pl.xn; lx++, x = x + (pl.width))
                    {
                        drawRollLineAbs(x, pl.y + pl.oy, pl.z, x, pl.y + pl.oy + pl.height * pl.yn, pl.z, pl.r, pl.g, pl.b, pl.a, pl.d1);
                    }
                }

                y = pl.y + pl.oy;
                {
                    ly = 0;
                    for (; ly < pl.yn; ly++, y = y + (pl.height))
                    {
                        drawRollLineAbs(pl.x + pl.ox, y, pl.z, pl.x + pl.ox + pl.width * pl.xn, y, pl.z, pl.r, pl.g, pl.b, pl.a, pl.d1);
                    }
                }
            }
        }
    }
}
