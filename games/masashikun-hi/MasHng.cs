// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasHng
{
    public const int xcent = 230;
    public const int ycent = 300;
    public static int myp = 0;
    public static int myx = 0;
    public static int myy = 0;
    public static int myd = 0;
    public static int mymx = 0;
    public static int mymy = 0;
    public static int mytd = 0;
    public static int speed = 0;
    public static int tmpsp = 0;
    public static int mypt = 0;
    public static int dist = 0;
    public static int flx = 0;
    public static int fly = 0;
    public static int flmx = 0;
    public static int flmy = 0;
    public static int zm = 0;
    public static int hngcou = 0;
    public static int hngdemocou = 0;
    public static int dmmbtcou = 0;
    public static int hngtry = 0;
    public static int[] hngdist = MasArrays.Make((3 + 1), () => 0);
    public static Hscdat[] hnghsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static void clearhnghsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                hnghsc[i].rec = -(99999);
                hnghsc[i].name = "---";
            }
        }
    }

    public static void sethnghiscore(int rec)
    {
        int i = 0;
        int n = 0;
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec > hnghsc[i].rec))
                {
                    n = i;
                    break;
                }
        }

        if ((n == 0))
            return;
        {
            i = 3;
            for (; i >= (n + 1); i--)
                hnghsc[i] = hnghsc[(i - 1)].Copy();
        }

        hnghsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((hnghsc[i].rec).ToString() + "mm"), hnghsc[i].name);
        }

        MasScores.settitle("Hitonage");
        hnghsc[n].name = MasScores.entername(n, ((rec).ToString() + "mm"));
    }

    public static void puthnghiscore()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((hnghsc[i].rec).ToString() + "mm"), hnghsc[i].name);
        }

        MasScores.settitle("Hitonage");
        MasScores.show();
    }

    public static void makehngmountdata()
    {
        int r = 0;
        // The 1.11e executable reads the following string address at cliff index 32.
        int[] gakedat = new int[]
        {
            300,
            295,
            293,
            290,
            292,
            290,
            290,
            288,
            280,
            270,
            275,
            277,
            276,
            280,
            271,
            272,
            278,
            271,
            260,
            266,
            270,
            275,
            270,
            269,
            268,
            256,
            250,
            248,
            252,
            262,
            265,
            256,
            4498156
        };
        {
            r = 0;
            for (; r <= (MasPut.moulen - 1); r++)
            {
                if ((((r > 48)) && ((r < (48 + 33)))))
                {
                    MasPut.mountdata[r].d = gakedat[(r - 48)];
                    MasPut.mountdata[r].obj = -(1);
                }
                else
                {
                    MasPut.mountdata[r].d = 0;
                    MasPut.mountdata[r].obj = -(1);
                    if ((r < 48))
                    {
                        if ((MasMath.Random(8) == 0))
                            MasPut.mountdata[r].obj = 4;
                    }
                    else if ((MasMath.Random(20) == 0))
                        MasPut.mountdata[r].obj = 2;
                }
            }
        }
    }

    public static void inithng()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        makehngmountdata();
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 512;
        speed = 0;
        tmpsp = 0;
        mytd = 0;
        mypt = 0;
        zm = 256;
        flx = -(32768);
        hngcou = 0;
        hngdemocou = 0;
        dist = (-(17) * 32);
        MasMain.setmainloopspe(4);
        MasMain.lcolor = 255;
        hngtry = 1;
    }

    public static void inithng2()
    {
        MasHira.clearhira();
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 512;
        speed = 0;
        tmpsp = 0;
        mytd = 0;
        mypt = 0;
        zm = 256;
        flx = -(32768);
        hngcou = 0;
        hngdemocou = 0;
        dist = (-(17) * 32);
        MasMain.lcolor = 255;
    }

    public static void putrollman(int xc, int yc, int d, int p)
    {
        int x = 0;
        int y = 0;
        int i = 0;
        i = 0;
        while ((MasMain.md[p].pd[i].x != -(32768)))
        {
            if ((MasMain.md[p].pd[i].x == -(32700)))
            {
                i = i + (1);
                while ((MasMain.md[p].pd[i].x != -(32760)))
                {
                    x = x + (MasMain.md[p].pd[i].x);
                    y = y + (MasMain.md[p].pd[i].y);
                    MasPut.putthickline(x, y);
                    i = i + (1);
                }

                i = i + (1);
            }
            else
            {
                x = (xc + MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dcos[d]), 256));
                y = ((yc + MasMain.md[p].pd[i].y) + MasMath.Div(MasMain.dsin[d], 64));
                MasPut.setthicklinefirst(x, y);
                i = i + (1);
                while ((MasMain.md[p].pd[i].x != -(32760)))
                {
                    x = x + (MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dcos[d]), 256));
                    y = y + (MasMain.md[p].pd[i].y);
                    MasPut.putthickline(x, y);
                    i = i + (1);
                }

                i = i + (1);
            }
        }
    }

    public static void inithngtitle()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        MasPut.setzoom(0, 0, 256);
        hngcou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(5);
    }

    public static void movehngtitle()
    {
        int mmv = 0;
        int mbt = 0;
        hngcou = hngcou + (1);
        MasHira.sethira2("ﾋﾄﾅｹﾞ", 200, 200, 50, 1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((hngcou > 165))))
            inithngex();
    }

    public static void puthngtitle()
    {
        int x = 0;
        x = (hngcou * 5);
        putrollman(x, (ycent + 128), (((x * 16)) & 1023), 17);
    }

    public static void inithngex()
    {
        MasHira.clearhira();
        makehngmountdata();
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 512;
        speed = 0;
        tmpsp = 0;
        mytd = 0;
        mypt = 0;
        zm = 256;
        flx = -(32768);
        hngcou = 0;
        dist = (-(17) * 32);
        MasMain.setmainloopspe(6);
        hngdemocou = 1;
    }

    public static void movehngex()
    {
        int mmv = 0;
        int mbt = 0;
        MasHira.sethira2("ｷｮｳｷﾞｾﾂﾒｲ", 0, 0, 48, 1);
        movehng();
        hngdemocou = hngdemocou + (1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((hngdemocou > 360))))
            inithng();
    }

    public static void puthngex()
    {
        if ((hngcou == 0))
            hngcou = 48;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        if ((hngcou <= 49))
        {
            MasPut.putmount(myp, (xcent - myx), (ycent - myy));
            putrollman(xcent, ycent, myd, 17);
            if ((hngcou == 49))
            {
                MasPut.setthicklinefirst(xcent, ycent);
                MasPut.putthickline((xcent + MasMath.Div(MasMain.dcos[mytd], 4)), (ycent - MasMath.Div(MasMain.dsin[mytd], 4)));
            }
        }
        else
        {
            MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
            MasPut.putman(xcent, ycent, MasMain.getdeg(mymx, mymy), (8 + mypt));
            if ((flx > -(32768)))
                MasPut.putman(((xcent - myx) + flx), ((ycent - myy) + fly), (((MasMain.getdeg(flmx, flmy) - 256)) & 1023), 19);
            else
                putrollman((xcent - myx), (ycent - myy), myd, 18);
        }
    }

    public static void inithngdemo()
    {
        MasHira.clearhira();
        makehngmountdata();
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 512;
        speed = 0;
        tmpsp = 0;
        mytd = 0;
        mypt = 0;
        zm = 256;
        flx = -(32768);
        hngcou = 0;
        dist = (-(17) * 32);
        MasHira.sethira2("ﾋﾄﾅｹﾞ", 0, 0, 24, 640);
        MasHira.sethira2("ｴﾌ2ｦｵｼﾃﾈ", 120, 400, 40, 640);
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(-(3));
        hngdemocou = -(1);
        dmmbtcou = 0;
        hngtry = 1;
    }

    public static void movehng()
    {
        string[] rolmes = new string[]
        {
            "ﾏﾜﾙｰ",
            "ﾔﾒﾛｰ",
            "ｸﾞﾙｸﾞﾙ",
            "ｱｳｱｳ",
            "ｼﾇｰ",
            "ｶﾞｰ",
            "ﾋｲ",
            "ﾜｰ"
        };
        string[] falmes = new string[]
        {
            "ｵﾁﾙｰ",
            "ﾀｽｹﾃｰ",
            "ｳﾜｰ",
            "ﾋｲｰ",
            "ｳｷﾞｰ",
            "ｱｰ",
            "ﾋﾄﾃﾞﾅｼｰ",
            "ｳｳｰ"
        };
        string[] tchmes = new string[]
        {
            "ｸﾞﾁｬ",
            "ﾍﾞﾁ",
            "ｸﾞｻ",
            "ﾍﾞｼｬ",
            "ｶﾞﾝ",
            "ｺﾞﾂ",
            "ｶﾞﾁ",
            "ﾄﾞﾝ"
        };
        int mmv = 0;
        int mbt = 0;
        int tp = 0;
        int d = 0;
        string s = "";
        int i = 0;
        int n = 0;
        int sc = 0;
        if ((hngdemocou == 0))
        {
            mmv = MasForm.mousemv;
            mbt = MasForm.mousebt;
            speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
        }
        else if ((hngdemocou > 0))
        {
            speed = speed + (1);
            mbt = 0;
            {
                int choice1 = hngdemocou;
                if (choice1 == 8)
                    MasHira.sethira2("ﾏｳｽｸﾞﾘｸﾞﾘﾃﾞﾋﾄｦﾏﾜｼﾏｽ", 32, 80, 30, 48);
                else if (choice1 == 64)
                    MasHira.sethira2("ｶﾞｹﾉﾊｼｷﾞﾘｷﾞﾘﾏﾃﾞﾈﾊﾞｯﾃ", 32, 80, 30, 48);
                else if (choice1 == 115)
                    MasHira.sethira2("ｱﾀﾏｶﾞｶﾞｹｶﾞﾜﾆﾑｲﾀﾄｷﾆ", 32, 80, 30, 110);
                else if (choice1 == 120)
                    mbt = 1;
                else if ((choice1 >= 121 && choice1 <= 290))
                {
                    mbt = 2;
                    if ((hngdemocou == 121))
                        MasHira.sethira2("ﾎﾞﾀﾝﾃﾞﾅｹﾞﾛ", 320, 120, 30, 105);
                    if ((hngdemocou == 230))
                        MasHira.sethira2("ﾎﾞﾀﾝｦｵｽﾅｶﾞｻﾃﾞ", 32, 80, 30, 100);
                    if ((hngdemocou == 235))
                        MasHira.sethira2("ｶｸﾄﾞｶﾞﾁｮｳｾｲﾃﾞｷﾙ", 180, 120, 30, 95);
                }
            }

            {
                int choice2 = hngdemocou;
                if ((choice2 >= 121 && choice2 <= 230))
                    return;
                else if ((choice2 >= 235 && choice2 <= 280))
                {
                    d = MasMath.Div((mytd * 360), 1024);
                    s = (d).ToString();
                    s = (s + "ﾄﾞ");
                    MasHira.sethira2(s, 350, 200, 64, 1);
                    return;
                }
            }
        }
        else
        {
            speed = speed + (1);
            mbt = 0;
            if ((dmmbtcou > 0))
            {
                dmmbtcou = dmmbtcou + (1);
                if ((((dmmbtcou > 10)) && ((MasMath.Random(5) == 0))))
                    mbt = 0;
                else
                    mbt = 2;
            }
            else
            {
                if (((hngdemocou < -(140))))
                {
                    if ((((myd < 200)) || ((myd > 800))))
                    {
                        if (((MasMath.Random(14) == 0)))
                            mbt = 1;
                    }
                    else
                    {
                        if (((MasMath.Random(56) == 0)))
                            mbt = 1;
                    }
                }

                if ((mbt == 1))
                    dmmbtcou = 1;
            }

            hngdemocou = hngdemocou - (1);
            if ((hngcou > 170))
                MasTitle.inittitle();
        }

        {
            int choice3 = hngcou;
            if (choice3 == 48)
            {
                if ((speed < 64))
                    myd = myd + (speed);
                else
                {
                    if ((MasMath.Random(16) == 0))
                    {
                        int textZoom = (36 + MasMath.Random(24));
                        int textMotion = (MasMath.Random(15) - 7);
                        MasHira.sethira(rolmes[MasMath.Random(8)], 240, 240, textZoom, textMotion);
                    }

                    myd = myd + (64);
                    speed = speed - (MasMath.Div(((speed - 64)), 32));
                }

                myd = (myd & 1023);
                myx = myx + (4);
                dist = dist + (4);
                if ((myx > 31))
                {
                    myx = myx - (32);
                    myp = myp + (1);
                    if ((myp > 48))
                    {
                        myp = myp - (1);
                        myx = myx + (32);
                    }
                }

                zm = zm + (MasMath.Div(((((128 + speed)) - zm)), 8));
                if ((mbt == 1))
                {
                    if ((myd > 512))
                        d = (1024 - myd);
                    else
                        d = myd;
                    d = MasMath.Div((d * 360), 1024);
                    s = (d).ToString();
                    s = (s + "ﾄﾞ");
                    MasHira.sethira2(s, 100, 128, 48, 64);
                    hngcou = 49;
                }

                if ((dist >= 0))
                {
                    MasHira.sethira2("ﾘﾝｸﾞｱｳﾄ", 128, 200, 64, 100);
                    hngcou = 50;
                    mymx = 8;
                    mymy = -(4);
                    flx = myx;
                    fly = myy;
                    flmx = 4;
                    flmy = -(8);
                }
            }
            else if (choice3 == 49)
            {
                if ((((myd > 256)) && ((myd < 768))))
                {
                    mymx = -(16);
                    mymy = -(10);
                    hngcou = 50;
                }

                mytd = mytd + (8);
                d = MasMath.Div((mytd * 360), 1024);
                s = (d).ToString();
                s = (s + "ﾄﾞ");
                MasHira.sethira2(s, 350, 200, 64, 1);
                if ((((mbt == 0)) || ((mytd > 250))))
                {
                    if ((mytd > 250))
                        mytd = 250;
                    tp = (speed * MasMain.dcos[myd]);
                    mymx = MasMath.Div((MasMain.dcos[mytd] * tp), 80000);
                    mymy = MasMath.Div((-(MasMain.dsin[mytd]) * tp), 80000);
                    hngcou = 50;
                    MasHira.sethira2(s, 350, 200, 64, 32);
                }
            }
            else if (choice3 == 50)
            {
                myx = myx + (mymx);
                dist = dist + (mymx);
                myy = myy + (mymy);
                mymy = mymy + (1);
                if ((MasMath.Random(12) == 0))
                {
                    int textZoom = (148 + MasMath.Random(100));
                    int textMotion = (MasMath.Random(25) - 12);
                    MasHira.sethira(falmes[MasMath.Random(8)], 240, 240, textZoom, textMotion);
                }

                if ((((dist < 0)) && ((myy > 0))))
                {
                    MasHira.sethira2("ｹｲｵｳ", 150, 200, 80, 80);
                    hngcou = 52;
                }

                if ((myy > 920))
                    hngcou = 51;
                if ((myx > 64))
                    zm = (MasMath.Div(20000, myx) + 32);
                mypt = (((mypt + 1)) & 7);
            }
            else if (choice3 == 51)
            {
                MasHira.sethira(tchmes[MasMath.Random(8)], 240, 240, 64, 0);
                myy = 920;
                mymx = 0;
                hngcou = 53;
            }
            else if (choice3 == 52)
            {
                myy = 0;
                mymx = 0;
                hngcou = 53;
            }
            else
            {
                if ((hngcou > 52))
                {
                    zm = zm + (MasMath.Div(((256 - zm)), 64));
                    hngcou = hngcou + (1);
                    if ((hngcou > 128))
                    {
                        MasMain.lcolor = MasMain.lcolor - (4);
                        if ((MasMain.lcolor < 0))
                            MasMain.lcolor = 0;
                    }

                    {
                        int choice4 = hngcou;
                        if (choice4 == 128)
                            MasHira.sethira2("ｷﾛｸ", 32, 100, 64, 170);
                        else if (choice4 == 140)
                        {
                            if ((dist >= 0))
                                s = ((dist * 13)).ToString();
                            else
                            {
                                s = ((-(dist) * 13)).ToString();
                                MasHira.sethira2("ﾏｲﾅｽ", ((400 - 128) - (s.Length * 50)), 220, 32, 160);
                            }

                            MasHira.sethira2(s, (400 - (s.Length * 50)), 200, 50, 160);
                        }
                        else if (choice4 == 150)
                        {
                            MasHira.sethira2("ﾐﾘ", 420, 220, 32, 150);
                            hngdist[hngtry] = dist;
                            if ((hngtry == 3))
                            {
                                dist = -(999999);
                                {
                                    i = 1;
                                    for (; i <= 3; i++)
                                    {
                                        if ((hngdist[i] > dist))
                                        {
                                            dist = hngdist[i];
                                            n = i;
                                        }
                                    }
                                }

                                if ((hngdist[3] >= 0))
                                    s = ((hngdist[3] * 13)).ToString();
                                else
                                {
                                    s = ((-(hngdist[3]) * 13)).ToString();
                                    MasHira.sethira2("ﾏｲﾅｽ", ((640 - 128) - (s.Length * 32)), ((3 * 32) - 32), 32, 960);
                                }

                                MasHira.sethira2(s, (640 - (s.Length * 32)), ((3 * 32) - 32), 32, 960);
                                MasHira.sethira2("ﾕｱﾍﾞｽﾄ", 256, ((n * 32) - 32), 32, 960);
                                if (MasMain.sucf)
                                {
                                    MasHira.sethira2("ﾋﾄﾅｹﾞ", 0, 428, 42, 250);
                                    sc = MasMath.Div(800000000, ((260000 - (dist * 13))));
                                    if ((sc > 20000))
                                        sc = 20000;
                                    else if ((sc < 0))
                                        sc = 20000;
                                    if (((dist * 13) < 5000))
                                        sc = 0;
                                    MasMain.score[3] = sc;
                                    s = (sc).ToString();
                                    MasHira.sethira2(s, (540 - (s.Length * 50)), 420, 50, 250);
                                    MasHira.sethira2("ﾃﾝ", 540, 428, 42, 250);
                                }

                                sethnghiscore((dist * 13));
                            }
                        }
                    }

                    if ((((((((hngcou > 150)) && ((mbt == 1)))) || ((hngcou > 400)))) && !(MasScores.hscsf)))
                    {
                        hngtry = hngtry + (1);
                        if ((hngtry > 3))
                        {
                            if (MasMain.sucf)
                                MasGfi.initgfititle();
                            else
                                MasTitle.inittitle();
                        }
                        else
                            inithng2();
                    }
                }
                else if ((hngcou > 0))
                {
                    if ((hngcou == 1))
                    {
                        {
                            i = 1;
                            for (; i <= (hngtry - 1); i++)
                            {
                                if ((hngdist[i] >= 0))
                                    s = ((hngdist[i] * 13)).ToString();
                                else
                                {
                                    s = ((-(hngdist[i]) * 13)).ToString();
                                    MasHira.sethira2("ﾏｲﾅｽ", ((640 - 128) - (s.Length * 32)), ((i * 32) - 32), 32, 960);
                                }

                                MasHira.sethira2(s, (640 - (s.Length * 32)), ((i * 32) - 32), 32, 960);
                            }
                        }
                    }

                    {
                        int choice5 = hngtry;
                        if (choice5 == 2)
                            MasHira.sethira2("ﾆｶｲﾒ", (hngcou * 16), 100, 64, 1);
                        else if (choice5 == 3)
                            MasHira.sethira2("ｻﾝｶｲﾒ", (hngcou * 16), 100, 64, 1);
                        else
                        {
                            MasHira.sethira2("ｲｯｶｲﾒ", (hngcou * 16), 100, 64, 1);
                        }
                    }

                    hngcou = hngcou + (1);
                    if ((hngcou == 48))
                        MasHira.sethira("ﾊｼﾞﾒ", 240, 240, 100, 0);
                }
            }
        }

        if ((flx > -(32768)))
        {
            flx = flx + (flmx);
            fly = fly + (flmy);
            flmy = flmy + (1);
            if ((fly > 980))
            {
                fly = 980;
                flmx = 0;
            }
        }

        if ((((hngcou <= 48)) && ((hngdemocou == 0))))
        {
            MasHira.sethira2("ﾏｲﾋﾞｮｳ", 256, 360, 32, 1);
            tmpsp = tmpsp + (MasMath.Div((((((speed * 10)) - tmpsp) - 7)), 8));
            s = (tmpsp).ToString();
            MasHira.sethira2(s, (520 - (s.Length * 64)), 400, 64, 1);
            MasHira.sethira2("ﾄﾞ", 540, 420, 50, 1);
        }

        if ((((hngcou == 50)) && ((hngdemocou == 0))))
        {
            if ((dist >= 0))
                s = ((dist * 13)).ToString();
            else
            {
                s = ((-(dist) * 13)).ToString();
                MasHira.sethira2("ﾏｲﾅｽ", ((528 - 128) - (s.Length * 50)), 440, 32, 1);
            }

            MasHira.sethira2(s, (528 - (s.Length * 50)), 420, 50, 1);
            MasHira.sethira2("ﾐﾘ", 540, 440, 32, 1);
        }

        MasPut.movemount(myp);
    }

    public static void puthng()
    {
        if ((hngcou == 0))
            hngcou = 1;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        if ((hngcou <= 49))
        {
            MasPut.putmount(myp, (xcent - myx), (ycent - myy));
            putrollman(xcent, ycent, myd, 17);
            if ((hngcou == 49))
            {
                MasPut.setthicklinefirst(xcent, ycent);
                MasPut.putthickline((xcent + MasMath.Div(MasMain.dcos[mytd], 4)), (ycent - MasMath.Div(MasMain.dsin[mytd], 4)));
            }
        }
        else
        {
            MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
            MasPut.putman(xcent, ycent, MasMain.getdeg(mymx, mymy), (8 + mypt));
            if ((flx > -(32768)))
                MasPut.putman(((xcent - myx) + flx), ((ycent - myy) + fly), (((MasMain.getdeg(flmx, flmy) - 256)) & 1023), 19);
            else
                putrollman((xcent - myx), (ycent - myy), myd, 18);
        }

        MasPut.setlcolor(255);
    }
}
