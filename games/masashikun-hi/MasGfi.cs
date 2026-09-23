// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasGfi
{
    public const int xcent = 230;
    public const int ycent = 300;
    public const int maxspeed = 30;
    public static int myp = 0;
    public static int myx = 0;
    public static int maxmymy = 0;
    public static int myy = 0;
    public static int tpy = 0;
    public static int myd = 0;
    public static int mymy = 0;
    public static int myc = 0;
    public static int speed = 0;
    public static int tmpsp = 0;
    public static int dist = 0;
    public static int mvsum = 0;
    public static int mychar = 0;
    public static int flx = 0;
    public static int fly = 0;
    public static int kkx = 0;
    public static int zm = 0;
    public static int gficou = 0;
    public static int gfidemocou = 0;
    public static Hscdat[] gfihsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static void cleargfihsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                gfihsc[i].rec = 0;
                gfihsc[i].name = "---";
            }
        }
    }

    public static void setgfihiscore(int rec)
    {
        int i = 0;
        int n = 0;
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec > gfihsc[i].rec))
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
                gfihsc[i] = gfihsc[(i - 1)].Copy();
        }

        gfihsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((gfihsc[i].rec).ToString() + "mm"), gfihsc[i].name);
        }

        MasScores.settitle("Gyaku Fire");
        gfihsc[n].name = MasScores.entername(n, ((rec).ToString() + "mm"));
    }

    public static void putgfihiscore()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((gfihsc[i].rec).ToString() + "mm"), gfihsc[i].name);
        }

        MasScores.settitle("Gyaku Fire");
        MasScores.show();
    }

    public static void initgfidemo()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                if ((((i > 128)) && ((i < 256))))
                    MasPut.mountdata[i].d = 256;
                else
                    MasPut.mountdata[i].d = 0;
                if ((((i > 256)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = 3;
                else if ((((i < 128)) && ((MasMath.Random(12) == 0))))
                    MasPut.mountdata[i].obj = 12;
                else
                    MasPut.mountdata[i].obj = -(1);
            }
        }

        MasPut.mountdata[128].obj = 13;
        myp = 32;
        myx = 0;
        myy = 0;
        speed = 0;
        mvsum = 0;
        tmpsp = 0;
        myc = 0;
        myd = 0;
        kkx = -(1024);
        flx = 0;
        fly = 0;
        zm = 256;
        gficou = 0;
        gfidemocou = -(1);
        dist = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(-(6));
        MasHira.sethira2("ｷﾞｬｸﾌｱｲﾔｰ", 0, 0, 24, 640);
        MasHira.sethira2("ｴﾌ2ｦｵｼﾃﾈ", 120, 400, 40, 640);
    }

    public static void initgfi()
    {
        int i = 0;
        MasForm.hidemousecursor();
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                if ((((i > 128)) && ((i < 256))))
                    MasPut.mountdata[i].d = 256;
                else
                    MasPut.mountdata[i].d = 0;
                if ((((i > 256)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = 3;
                else if ((((i < 128)) && ((MasMath.Random(12) == 0))))
                    MasPut.mountdata[i].obj = 12;
                else
                    MasPut.mountdata[i].obj = -(1);
            }
        }

        MasPut.mountdata[128].obj = 13;
        myp = 32;
        myx = 0;
        myy = 0;
        speed = 0;
        mvsum = 0;
        tmpsp = 0;
        myc = 0;
        myd = 0;
        kkx = -(1024);
        flx = 0;
        fly = 0;
        zm = 256;
        gficou = 0;
        gfidemocou = 0;
        dist = 0;
        MasMain.setmainloopspe(13);
        MasMain.lcolor = 255;
    }

    public static void initgfititle()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        MasPut.setzoom(0, 0, 256);
        gficou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(14);
    }

    public static void movegfititle()
    {
        int mmv = 0;
        int mbt = 0;
        gficou = gficou + (1);
        MasHira.sethira2("ｷﾞｬｸﾌｱｲﾔｰ", 128, 200, 50, 1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((gficou > 165))))
            initgfiex();
    }

    public static void putgfititle()
    {
        MasPut.puthngman(((gficou * 5) - 64), (ycent + 128), (((3 - ((gficou & 7)))) * 3), 23);
        MasPut.puthngman(((gficou * 5) + 128), (ycent + 128), (((3 - ((gficou & 7)))) * 3), 28);
        MasPut.setthicklinefirst(((gficou * 5) - 32), ((ycent + 128) - 52));
        MasPut.putthickline(((gficou * 5) + 96), ((ycent + 128) - 52));
    }

    public static void initgfiex()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                if ((((i > 128)) && ((i < 256))))
                    MasPut.mountdata[i].d = 256;
                else
                    MasPut.mountdata[i].d = 0;
                if ((((i > 256)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = 3;
                else if ((((i < 128)) && ((MasMath.Random(12) == 0))))
                    MasPut.mountdata[i].obj = 12;
                else
                    MasPut.mountdata[i].obj = -(1);
            }
        }

        MasPut.mountdata[128].obj = 13;
        myp = 32;
        myx = 0;
        myy = 0;
        speed = 0;
        mvsum = 0;
        tmpsp = 0;
        myc = 0;
        myd = 0;
        kkx = -(1024);
        flx = 0;
        fly = 0;
        zm = 256;
        gficou = 0;
        gfidemocou = 0;
        dist = 0;
        MasMain.setmainloopspe(15);
        MasMain.lcolor = 255;
    }

    public static void movegfi()
    {
        string[] dshmes = new string[]
        {
            "ﾀﾞｯｼｭﾂ",
            "ﾆｹﾞﾀ",
            "ﾔｯﾀｰ",
            "ﾀﾞｲﾌﾞ",
            "ﾄﾋﾞﾀﾃ",
            "ｺﾞｰ",
            "ｵｵｿﾞﾗﾍ",
            "ﾊﾊﾞﾀｸ"
        };
        string[] ochmes = new string[]
        {
            "ｵｳﾜｰ",
            "ﾀｽｹﾃｰ",
            "ﾋｰ",
            "ﾄﾝﾃﾞﾙ",
            "ｶﾞｰ",
            "ｳｵ",
            "ﾀｶｲ",
            "ｵﾁﾙ"
        };
        int mmv = 0;
        int mbt = 0;
        string s = "";
        int sc = 0;
        if ((gfidemocou == 0))
        {
            mmv = MasForm.mousemv;
            mbt = MasForm.mousebt;
        }
        else if ((gfidemocou > 0))
        {
            speed = speed + (1);
            mmv = 0;
            mbt = 0;
            {
                int choice1 = gfidemocou;
                if (choice1 == 8)
                    MasHira.sethira2("ﾏｳｽｦｸﾞﾘｸﾞﾘｼﾃﾊｼﾚ", 32, 80, 30, 64);
                else if (choice1 == 80)
                    MasHira.sethira2("ﾀﾞｯｼｭﾂｼﾃｶﾗﾓﾏｳｽｦﾔｽﾒﾙﾅ", 32, 80, 30, 48);
                else if (choice1 == 175)
                    MasHira.sethira2("ｷｭｳｼｭﾂﾀｲﾉﾏｯﾄｦﾖｸﾐﾃ", 32, 80, 30, 50);
                else if (choice1 == 240)
                    MasHira.sethira2("ｲﾁﾊﾞﾝﾀﾜﾝﾀﾞﾄｺﾛﾃﾞﾎﾞﾀﾝｦｵｾ", 32, 80, 30, 64);
                else if (choice1 == 281)
                    mbt = 1;
                else if (choice1 == 350)
                    MasHira.sethira2("ｼﾞｬﾝﾌﾟﾆｼｯﾊﾟｲｽﾙﾄ", 32, 80, 30, 64);
                else if (choice1 == 420)
                    MasHira.sethira2("ｷｭｳｷｭｳｼｬﾆﾀｽｹﾗﾚﾃｵﾜﾘ", 32, 80, 30, 64);
            }

            {
                int choice2 = gfidemocou;
                if ((choice2 >= 175 && choice2 <= 225))
                    return;
                else if ((choice2 >= 240 && choice2 <= 280))
                    return;
            }
        }
        else
        {
            speed = speed + (1);
            mmv = 0;
            mbt = 0;
            if (((((gficou == 50)) && ((mymy < 16))) && ((MasMath.Random(8) == 0))))
                mbt = 1;
            if ((gficou > 170))
                MasTitle.inittitle();
        }

        {
            int choice3 = gficou;
            if (choice3 == 48)
            {
                speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
                if ((speed > maxspeed))
                {
                    myd = (maxspeed * 4);
                    speed = speed - (MasMath.Div(((speed - maxspeed)), 32));
                }
                else
                {
                    myd = (speed * 4);
                }

                myx = myx + (speed);
                zm = zm + (MasMath.Div(((128 - zm)), 16));
                mvsum = mvsum + (speed);
                while ((myx > 31))
                {
                    myx = myx - (32);
                    myp = myp + (1);
                }

                if ((mvsum > 7))
                {
                    mvsum = 0;
                    mychar = mychar + (1);
                }

                if (((myp >= 128)))
                {
                    {
                        int textZoom = 200;
                        int textMotion = MasMath.Random(9);
                        MasHira.sethira(dshmes[MasMath.Random(8)], 280, 240, textZoom, textMotion);
                    }

                    myx = 0;
                    myp = 128;
                    gficou = 49;
                    mymy = 0;
                }

                myc = ((mychar & 7));
            }
            else if (choice3 == 49)
            {
                speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
                speed = speed - (MasMath.Div(speed, 32));
                zm = zm + (MasMath.Div(((MasMath.Div(20000, ((4000 - myy))) - zm)), 16));
                myx = myx + (MasMath.Div(speed, 4));
                dist = dist + (MasMath.Div(speed, 4));
                myy = myy + (mymy);
                mymy = mymy + (1);
                mychar = mychar + (1);
                if ((myy > 3920))
                {
                    myy = 3920;
                    gficou = 50;
                    maxmymy = mymy;
                    tpy = 0;
                }

                myc = (((mychar & 7)) + 8);
                if ((MasMath.Random(16) == 0))
                {
                    int textZoom = (100 + MasMath.Random(100));
                    int textMotion = (10 - MasMath.Random(21));
                    MasHira.sethira(ochmes[MasMath.Random(8)], xcent, ycent, textZoom, textMotion);
                }
            }
            else if (choice3 == 50)
            {
                zm = zm + (MasMath.Div(((320 - zm)), 16));
                mymy = mymy - (4);
                tpy = tpy + (MasMath.Div(mymy, 10));
                if ((((mbt == 1)) || ((mymy < -(maxmymy)))))
                {
                    MasHira.sethira("ｼﾞｬﾝﾌﾟ", xcent, ycent, 160, 0);
                    mymy = MasMath.Div((((Math.Abs(mymy) - maxmymy)) * 4), 5);
                    if ((mymy < -(16)))
                        gficou = 49;
                    else
                    {
                        myy = 3920;
                        gficou = 51;
                        mymy = -(32);
                    }
                }

                if ((MasMath.Random(6) == 0))
                {
                    int textZoom = 32;
                    int textMotion = (5 - MasMath.Random(9));
                    MasHira.sethira("ﾀﾒﾃ", (xcent - 64), ((((ycent + 4064) - myy) - tpy) - 64), textZoom, textMotion);
                }

                if ((MasMath.Random(6) == 0))
                {
                    int textZoom = 32;
                    int textMotion = (5 - MasMath.Random(9));
                    MasHira.sethira("ﾀﾒﾃ", (xcent + 128), ((((ycent + 4064) - myy) - tpy) - 64), textZoom, textMotion);
                }
            }
            else if (choice3 == 51)
            {
                zm = zm + (MasMath.Div(((80 - zm)), 8));
                myy = myy + (mymy);
                mymy = mymy + (1);
                mychar = mychar + (1);
                if ((myy > 3920))
                    gficou = 52;
                myc = (((mychar & 7)) + 8);
                kkx = kkx + (64);
                if ((kkx == -(128)))
                {
                    int textZoom = 120;
                    int textMotion = (7 + MasMath.Random(15));
                    MasHira.sethira("ｶﾞﾝ", xcent, ((ycent + 4096) - myy), textZoom, textMotion);
                }

                if ((kkx >= 0))
                {
                    kkx = 0;
                    flx = flx + (48);
                    fly = fly - (32);
                }
            }
            else if ((choice3 >= 52 && choice3 <= 1000))
            {
                gficou = gficou + (1);
                zm = zm + (MasMath.Div(((200 - zm)), 16));
                if ((gficou > 118))
                {
                    MasMain.lcolor = MasMain.lcolor - (4);
                    if ((MasMain.lcolor < 0))
                        MasMain.lcolor = 0;
                }

                {
                    int choice4 = gficou;
                    if (choice4 == 118)
                        MasHira.sethira2("ｷﾛｸ", 32, 100, 64, 170);
                    else if (choice4 == 130)
                    {
                        s = ((dist * 13)).ToString();
                        MasHira.sethira2(s, (400 - (s.Length * 50)), 200, 50, 160);
                    }
                    else if (choice4 == 140)
                    {
                        MasHira.sethira2("ﾐﾘ", 420, 220, 32, 150);
                        if ((MasMain.sucf && ((gfidemocou == 0))))
                        {
                            MasHira.sethira2("ﾌｱｲﾔｰ", 0, 428, 42, 250);
                            sc = MasMath.Div(700000000, ((170000 - (dist * 13))));
                            if ((sc > 20000))
                                sc = 20000;
                            else if ((sc < 0))
                                sc = 20000;
                            MasMain.score[4] = sc;
                            s = (sc).ToString();
                            MasHira.sethira2(s, (540 - (s.Length * 50)), 420, 50, 250);
                            MasHira.sethira2("ﾃﾝ", 540, 428, 42, 250);
                        }

                        if ((gfidemocou == 0))
                            setgfihiscore((dist * 13));
                    }
                }

                if ((((((((gficou > 140)) && ((mbt == 1)))) || ((gficou > 390)))) && !(MasScores.hscsf)))
                    if (MasMain.sucf)
                        MasResult.initresult();
                    else
                        MasTitle.inittitle();
            }
            else
            {
                {
                    if ((gficou > 0))
                    {
                        MasHira.sethira2("ｶｼﾞﾀﾞ", (gficou * 16), 100, 64, 1);
                        gficou = gficou + (1);
                        if ((gficou == 48))
                            MasHira.sethira("ﾆｹﾞﾛ", 280, 240, 100, 0);
                    }
                }
            }
        }

        if ((((gficou < 49)) && ((gfidemocou == 0))))
        {
            MasHira.sethira2("ﾏｲﾆﾁ", 256, 360, 32, 1);
            tmpsp = tmpsp + (MasMath.Div((((((speed * 14)) - tmpsp) - 7)), 8));
            s = (tmpsp).ToString();
            MasHira.sethira2(s, (520 - (s.Length * 64)), 400, 64, 1);
            MasHira.sethira2("ｷﾛ", 540, 420, 50, 1);
            if ((((myp < 110)) && ((MasMath.Random(4) == 0))))
            {
                int textZoom = (32 + MasMath.Random(32));
                int textMotion = (11 - MasMath.Random(23));
                MasHira.sethira("ﾒﾗ", MasMath.Random(640), ycent, textZoom, textMotion);
            }
        }

        if (((((gficou > 48)) && ((gficou < 51))) && ((gfidemocou == 0))))
        {
            s = ((dist * 13)).ToString();
            MasHira.sethira2(s, (528 - (s.Length * 50)), 420, 50, 1);
            MasHira.sethira2("ﾐﾘ", 540, 440, 32, 1);
        }

        MasPut.movemount(myp);
    }

    public static void putobject(int x, int y, int spe)
    {
        int i = 0;
        MasPut.setthicklinefirst(x, y);
        i = 0;
        while ((MasMain.obd[spe].pd[i].x != -(32768)))
        {
            x = x + (MasMain.obd[spe].pd[i].x);
            y = y + (MasMain.obd[spe].pd[i].y);
            MasPut.putthickline(x, y);
            i = i + (1);
        }
    }

    public static void movegfiex()
    {
        int mmv = 0;
        int mbt = 0;
        MasHira.sethira2("ｷｮｳｷﾞｾﾂﾒｲ", 0, 0, 48, 1);
        movegfi();
        gfidemocou = gfidemocou + (1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((gfidemocou > 520))))
            initgfi();
    }

    public static void putgfiex()
    {
        if ((gficou == 0))
            gficou = 48;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        {
            int choice5 = gficou;
            if ((choice5 >= 0 && choice5 <= 48))
            {
                MasPut.putmount(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, myd, myc);
            }
            else if (choice5 == 49)
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, mymy), myc);
                MasPut.puthngman((xcent - 64), ((ycent + 4064) - myy), 0, 23);
                MasPut.puthngman((xcent + 128), ((ycent + 4064) - myy), 0, 28);
                MasPut.setthicklinefirst((xcent - 32), (((ycent + 4064) - 52) - myy));
                MasPut.putthickline((xcent + 96), (((ycent + 4064) - 52) - myy));
            }
            else if (choice5 == 50)
            {
                MasPut.putmount2(myp, (xcent - myx), ((ycent - myy) - tpy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, maxmymy), myc);
                MasPut.puthngman((xcent - 64), (((ycent + 4064) - myy) - tpy), 0, 23);
                MasPut.puthngman((xcent + 128), (((ycent + 4064) - myy) - tpy), 0, 28);
                MasPut.setthicklinefirst((xcent - 32), ((((ycent + 4064) - 52) - myy) - tpy));
                MasPut.putthickline((xcent + 32), (((ycent + 4064) - 50) - myy));
                MasPut.putthickline((xcent + 96), ((((ycent + 4064) - 52) - myy) - tpy));
            }
            else if (choice5 == 51)
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, mymy), myc);
                if ((flx == 0))
                {
                    MasPut.puthngman((xcent - 64), ((ycent + 4064) - myy), 0, 23);
                    MasPut.puthngman((xcent + 128), ((ycent + 4064) - myy), 0, 28);
                    MasPut.setthicklinefirst((xcent - 32), (((ycent + 4064) - 52) - myy));
                    MasPut.putthickline((xcent + 96), (((ycent + 4064) - 52) - myy));
                }
                else
                {
                    MasPut.puthngman(((xcent - 64) + flx), (((ycent + 4064) - myy) + fly), MasMath.Div(-(flx), 8), 23);
                    MasPut.puthngman(((xcent + 128) + flx), (((ycent + 4064) - myy) + fly), MasMath.Div(-(flx), 8), 28);
                }

                putobject(((xcent - 64) + kkx), ((ycent + 4064) - myy), 11);
            }
            else if ((choice5 >= 52 && choice5 <= 1000))
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                putobject(((xcent - 64) + kkx), ((ycent + 4064) - myy), 11);
            }
        }

        MasPut.setlcolor(255);
    }

    public static void putgfi()
    {
        if ((gficou == 0))
            gficou = 1;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        {
            int choice6 = gficou;
            if ((choice6 >= 0 && choice6 <= 48))
            {
                MasPut.putmount(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, myd, myc);
            }
            else if (choice6 == 49)
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, mymy), myc);
                MasPut.puthngman((xcent - 64), ((ycent + 4064) - myy), 0, 23);
                MasPut.puthngman((xcent + 128), ((ycent + 4064) - myy), 0, 28);
                MasPut.setthicklinefirst((xcent - 32), (((ycent + 4064) - 52) - myy));
                MasPut.putthickline((xcent + 96), (((ycent + 4064) - 52) - myy));
            }
            else if (choice6 == 50)
            {
                MasPut.putmount2(myp, (xcent - myx), ((ycent - myy) - tpy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, maxmymy), myc);
                MasPut.puthngman((xcent - 64), (((ycent + 4064) - myy) - tpy), 0, 23);
                MasPut.puthngman((xcent + 128), (((ycent + 4064) - myy) - tpy), 0, 28);
                MasPut.setthicklinefirst((xcent - 32), ((((ycent + 4064) - 52) - myy) - tpy));
                MasPut.putthickline((xcent + 32), (((ycent + 4064) - 50) - myy));
                MasPut.putthickline((xcent + 96), ((((ycent + 4064) - 52) - myy) - tpy));
            }
            else if (choice6 == 51)
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                MasPut.putman(xcent, ycent, MasMain.getdeg(speed, mymy), myc);
                if ((flx == 0))
                {
                    MasPut.puthngman((xcent - 64), ((ycent + 4064) - myy), 0, 23);
                    MasPut.puthngman((xcent + 128), ((ycent + 4064) - myy), 0, 28);
                    MasPut.setthicklinefirst((xcent - 32), (((ycent + 4064) - 52) - myy));
                    MasPut.putthickline((xcent + 96), (((ycent + 4064) - 52) - myy));
                }
                else
                {
                    MasPut.puthngman(((xcent - 64) + flx), (((ycent + 4064) - myy) + fly), MasMath.Div(-(flx), 8), 23);
                    MasPut.puthngman(((xcent + 128) + flx), (((ycent + 4064) - myy) + fly), MasMath.Div(-(flx), 8), 28);
                }

                putobject(((xcent - 64) + kkx), ((ycent + 4064) - myy), 11);
            }
            else if ((choice6 >= 52 && choice6 <= 1000))
            {
                MasPut.putmount2(myp, (xcent - myx), (ycent - myy));
                putobject(((xcent - 64) + kkx), ((ycent + 4064) - myy), 11);
            }
        }

        MasPut.setlcolor(255);
    }
}
