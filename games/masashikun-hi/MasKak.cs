// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasKak
{
    public const int xcent = 230;
    public const int ycent = 300;
    public const int maxspeed = 30;
    public static int myx = 0;
    public static int myy = 0;
    public static int myp = 0;
    public static int myd = 0;
    public static int myc = 0;
    public static int mychar = 0;
    public static int myvy = 0;
    public static int speed = 0;
    public static int tmpsp = 0;
    public static int mvsum = 0;
    public static int zm = 0;
    public static int kakcou = 0;
    public static int kakdemocou = 0;
    public static int time = 0;
    public static Hscdat[] kakhsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static void clearkakhsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                kakhsc[i].rec = 99999;
                kakhsc[i].name = "---";
            }
        }
    }

    public static void setkakhiscore(int rec)
    {
        int i = 0;
        int n = 0;
        string s = "";
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec < kakhsc[i].rec))
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
                kakhsc[i] = kakhsc[(i - 1)].Copy();
        }

        kakhsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                s = (((kakhsc[i].rec % 1000) + 1000)).ToString();
                s = s.Substring(1);
                s = (((MasMath.Div(kakhsc[i].rec, 1000)).ToString() + "sec") + s);
                MasScores.sethiscore(i, s, kakhsc[i].name);
            }
        }

        MasScores.settitle("Kakenukero Dougenzaka");
        s = (((rec % 1000) + 1000)).ToString();
        s = s.Substring(1);
        s = (((MasMath.Div(rec, 1000)).ToString() + "sec") + s);
        kakhsc[n].name = MasScores.entername(n, s);
    }

    public static void putkakhiscore()
    {
        int i = 0;
        string s = "";
        {
            i = 1;
            for (; i <= 3; i++)
            {
                s = (((kakhsc[i].rec % 1000) + 1000)).ToString();
                s = s.Substring(1);
                s = (((MasMath.Div(kakhsc[i].rec, 1000)).ToString() + "sec") + s);
                MasScores.sethiscore(i, s, kakhsc[i].name);
            }
        }

        MasScores.settitle("Kakenukero Dougenzaka");
        MasScores.show();
    }

    public static void makemountdata()
    {
        int r = 0;
        int d = 0;
        int md = 0;
        d = 0;
        {
            r = 0;
            for (; r <= (MasPut.moulen - 1); r++)
            {
                if ((d > 250))
                    d = 250;
                else if ((d < -(250)))
                    d = -(250);
                MasPut.mountdata[r].d = d;
                if ((((r > 48)) && ((r < (MasPut.moulen - 248)))))
                {
                    md = md + ((MasMath.Random(3) - 1));
                    if ((md > 16))
                        md = 0;
                    else if ((md < -(16)))
                        md = 0;
                    if ((d > 192))
                        md = -(4);
                    else if ((d < -(192)))
                        md = 4;
                    d = d + (md);
                    if ((MasMath.Random(32) == 0))
                    {
                        md = (MasMath.Random(40) - 20);
                        d = d + ((MasMath.Random(256) - 128));
                    }

                    if ((MasMath.Random(16) == 0))
                    {
                        int choice1 = MasMath.Random(10);
                        if ((choice1 >= 0 && choice1 <= 3))
                            MasPut.mountdata[r].obj = 3;
                        else if ((choice1 >= 4 && choice1 <= 5))
                            MasPut.mountdata[r].obj = 1;
                        else if (choice1 == 6)
                            MasPut.mountdata[r].obj = 2;
                        else if ((choice1 >= 7 && choice1 <= 9))
                            MasPut.mountdata[r].obj = 4;
                    }
                    else
                        MasPut.mountdata[r].obj = -(1);
                }
                else
                    MasPut.mountdata[r].obj = -(1);
                {
                    int choice2 = r;
                    if (choice2 == 38)
                        MasPut.mountdata[r].obj = 0;
                    else if (choice2 == (MasPut.moulen - 238))
                        MasPut.mountdata[r].obj = 5;
                    else if (choice2 == (MasPut.moulen - 228))
                        MasPut.mountdata[r].obj = 5;
                    else if (choice2 == (MasPut.moulen - 258))
                        MasPut.mountdata[r].obj = 0;
                }
            }
        }
    }

    public static void initkak()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        makemountdata();
        MasPut.scd = 0;
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        myvy = -(32768);
        zm = 128;
        kakcou = 0;
        time = 0;
        kakdemocou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(1);
    }

    public static void initkakdemo()
    {
        MasHira.clearhira();
        makemountdata();
        MasPut.scd = 0;
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        myvy = -(32768);
        zm = 128;
        kakcou = 0;
        time = 0;
        kakdemocou = -(1);
        MasHira.sethira2("ｶｹﾇｹﾛﾄﾞｳｹﾞﾝｻﾞｶ", 0, 0, 24, 640);
        MasHira.sethira2("ｴﾌ2ｦｵｼﾃﾈ", 120, 400, 40, 640);
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(-(2));
    }

    public static void initkaktitle()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        MasPut.setzoom(0, 0, 256);
        kakcou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(2);
    }

    public static void movekaktitle()
    {
        int mmv = 0;
        int mbt = 0;
        kakcou = kakcou + (1);
        MasHira.sethira2("ｶｹﾇｹﾛﾄﾞｳｹﾞﾝｻﾞｶ", 50, 200, 50, 1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((kakcou > 165))))
            initkakex();
    }

    public static void putkaktitle()
    {
        int[] manpat = new int[]
        {
            3,
            4,
            5,
            4,
            3,
            4,
            5,
            6,
            7,
            0,
            1,
            0,
            7,
            0,
            1,
            2
        };
        int i = 0;
        int x = 0;
        x = (kakcou * 5);
        {
            i = 0;
            for (; i <= 4; i++)
            {
                MasPut.putman(x, (ycent + 128), 960, manpat[(kakcou & 15)]);
                x = x - (32);
            }
        }
    }

    public static void initkakex()
    {
        int i = 0;
        makemountdata();
        {
            i = 48;
            for (; i <= 127; i++)
                MasPut.mountdata[i].d = 0;
        }

        {
            i = 128;
            for (; i <= 209; i++)
                MasPut.mountdata[i].d = 80;
        }

        {
            i = 210;
            for (; i <= 280; i++)
                MasPut.mountdata[i].d = -(120);
        }

        MasPut.scd = 0;
        myp = 32;
        myx = 0;
        myy = 0;
        myd = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        myvy = -(32768);
        zm = 128;
        kakcou = 0;
        time = 0;
        kakdemocou = 1;
        MasMain.setmainloopspe(3);
    }

    public static void movekakex()
    {
        int mmv = 0;
        int mbt = 0;
        MasHira.sethira2("ｷｮｳｷﾞｾﾂﾒｲ", 0, 0, 48, 1);
        movekak();
        kakdemocou = kakdemocou + (1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((kakdemocou > 520))))
            initkak();
    }

    public static void putkakex()
    {
        if ((kakcou == 0))
            kakcou = 48;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.putmount(myp, (xcent - myx), (ycent + myy));
        MasPut.putman(xcent, ycent, ((((myd - MasPut.scd) + (speed * 4))) & 1023), myc);
    }

    public static void movekak()
    {
        string[] maxmes = new string[]
        {
            "ﾋｲ",
            "ｺﾗ",
            "ｾﾞｲ",
            "ﾊｱ",
            "ｳｶﾞｰ",
            "ｳｲ",
            "ｷﾂｲｯｽ",
            "ｱｶﾞ"
        };
        string[] kokmes = new string[]
        {
            "ｳﾜｯﾄ",
            "ｵｳ",
            "ｸﾞﾜｯ",
            "ｯﾂ",
            "ｺｹﾙｺｹﾙ",
            "ｲﾃｯ",
            "ｶﾞﾂ",
            "ｱｳ"
        };
        string[] jmpmes = new string[]
        {
            "ｻｲｷｮｰ",
            "ﾋｬｯﾎｰ",
            "ﾎﾚ",
            "ﾄｰ",
            "ﾀｰ",
            "ｿｲﾔ",
            "ﾄﾋﾞﾏｽ",
            "ｳﾘｬ"
        };
        int mmv = 0;
        int mbt = 0;
        bool sdf = false;
        int bmc = 0;
        string s = "";
        int sc = 0;
        bmc = mychar;
        sdf = false;
        if ((kakdemocou == 0))
        {
            mmv = MasForm.mousemv;
            mbt = MasForm.mousebt;
            speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
        }
        else if ((kakdemocou > 0))
        {
            speed = 20;
            mbt = 0;
            {
                int choice3 = kakdemocou;
                if (choice3 == 16)
                    MasHira.sethira2("ﾏｳｽｦｸﾞﾘｸﾞﾘｼﾃﾊｼﾘﾏｼｮｳ", 32, 80, 30, 64);
                else if (choice3 == 110)
                    MasHira.sethira2("ﾀﾞﾝｻｶﾞﾐｴﾀﾗﾎﾞﾀﾝﾃﾞｼﾞｬﾝﾌﾟ", 32, 80, 30, 80);
                else if (choice3 == 150)
                    mbt = 1;
                else if (choice3 == 220)
                    MasHira.sethira2("ﾀﾞﾝｻﾆｿﾉﾏﾏﾂｯｺﾑﾄｵｿｸﾅﾙ", 32, 80, 30, 100);
                else if (choice3 == 400)
                    MasHira.sethira2("ｿﾝﾅﾄｺﾛﾃﾞｽ", 32, 80, 30, 80);
            }
        }
        else
        {
            speed = speed + (1);
            if ((MasMath.Random(32) == 0))
                mbt = 1;
            else
                mbt = 0;
            kakdemocou = kakdemocou - (1);
            if ((kakdemocou < -(640)))
                MasTitle.inittitle();
        }

        if ((speed >= maxspeed))
        {
            speed = maxspeed;
            if ((((MasMath.Random(32) == 0)) && ((kakcou == 0))))
            {
                int textZoom = (8 + MasMath.Random(32));
                int textMotion = (MasMath.Random(5) - 2);
                MasHira.sethira(maxmes[MasMath.Random(8)], 300, 240, textZoom, textMotion);
            }
        }

        if ((kakcou >= 48))
        {
            myx = myx + (speed);
            time = time + (33);
            zm = zm + (MasMath.Div((((128 + MasMath.Div((speed * speed), 3)) - zm)), 8));
            if ((myp > (MasPut.moulen - 228)))
                kakcou = -(1);
        }
        else
        {
            if ((kakcou > 0))
            {
                MasHira.sethira2("ﾖｰｲ", (kakcou * 16), 100, 64, 1);
                kakcou = kakcou + (1);
                if ((kakcou == 48))
                    MasHira.sethira("ﾄﾞﾝ", 280, 240, 100, 0);
            }
            else if ((kakcou < 0))
            {
                MasHira.sethira2("ｺﾞｰﾙｲﾝ", (640 + (kakcou * 10)), 150, 72, 1);
                if ((((((kakcou & 15)) == 0)) && ((kakcou > -(200)))))
                {
                    int textZoom = (MasMath.Random(80) + 20);
                    int textMotion = (MasMath.Random(15) - 7);
                    MasHira.sethira("ｺﾞｰﾙ", 280, 240, textZoom, textMotion);
                }

                kakcou = kakcou - (1);
                if ((myp < (MasPut.moulen - 128)))
                    myx = myx + (speed);
                zm = zm + (MasMath.Div(((MasMath.Div(256, 3) - zm)), 8));
                if ((kakcou < -(200)))
                {
                    MasMain.lcolor = MasMain.lcolor - (4);
                    if ((MasMain.lcolor < 0))
                        MasMain.lcolor = 0;
                }

                {
                    int choice4 = kakcou;
                    if (choice4 == -(128))
                        MasHira.sethira2("ｷﾛｸ", 32, 100, 64, 170);
                    else if (choice4 == -(140))
                    {
                        s = (MasMath.Div(time, 1000)).ToString();
                        MasHira.sethira2(s, (220 - (s.Length * 64)), 200, 64, 160);
                    }
                    else if (choice4 == -(150))
                        MasHira.sethira2("ﾋﾞｮｳ", 240, 210, 52, 150);
                    else if (choice4 == -(160))
                    {
                        s = (((time % 1000) + 1000)).ToString();
                        s = s.Substring(1);
                        MasHira.sethira2(s, 400, 200, 64, 140);
                        if (MasMain.sucf)
                        {
                            MasHira.sethira2("ｶｹﾇｹﾛ", 0, 428, 42, 250);
                            sc = MasMath.Div(20000000, ((time - 25400)));
                            if ((sc > 20000))
                                sc = 20000;
                            else if ((sc < 0))
                                sc = 20000;
                            MasMain.score[0] = sc;
                            s = (sc).ToString();
                            MasHira.sethira2(s, (540 - (s.Length * 50)), 420, 50, 250);
                            MasHira.sethira2("ﾃﾝ", 540, 428, 42, 250);
                        }

                        if ((kakdemocou == 0))
                            setkakhiscore(time);
                    }
                }

                if ((((((((kakcou < -(160))) && ((mbt == 1)))) || ((kakcou < -(410))))) && !(MasScores.hscsf)))
                    if (MasMain.sucf)
                        MasTob.inittobtitle();
                    else
                        MasTitle.inittitle();
            }
        }

        mvsum = mvsum + (speed);
        if ((myx > 31))
        {
            myx = myx - (32);
            myp = myp + (1);
        }
        else if ((myx > 63))
        {
            myx = myx - (64);
            myp = myp + (2);
        }

        if ((mvsum > 7))
        {
            mvsum = 0;
            mychar = mychar + (1);
        }

        if (((myd - MasPut.scd) > 8))
        {
            myd = myd - (9);
            sdf = true;
        }
        else if (((myd - MasPut.scd) < -(8)))
        {
            myd = myd + (9);
            sdf = true;
        }
        else
            myd = MasPut.scd;
        if ((myy != 0))
            sdf = false;
        if (sdf)
        {
            if ((speed > 2))
                speed = speed - (3);
            else
                speed = 0;
            mychar = (bmc + 1);
            if ((MasMath.Random(5) == 0))
            {
                int textZoom = (24 + MasMath.Random(32));
                int textMotion = (MasMath.Random(15) - 7);
                MasHira.sethira(kokmes[MasMath.Random(8)], 280, 240, textZoom, textMotion);
            }
        }

        if ((((mbt == 1)) && ((myy == 0))))
        {
            myvy = (12 + (MasMath.Div(speed, 16)));
            {
                int textZoom = 50;
                int textMotion = (MasMath.Random(9) - 5);
                MasHira.sethira(jmpmes[MasMath.Random(8)], 280, 240, textZoom, textMotion);
            }
        }

        if ((myvy != -(32768)))
        {
            myy = myy + (myvy);
            myvy = myvy - (2);
            if ((myy <= 0))
            {
                myy = 0;
                myvy = -(32768);
            }
        }

        if ((myvy > 0))
            myc = 16;
        else
        {
            if (sdf)
                myc = (((mychar & 7)) + 8);
            else
                myc = ((mychar & 7));
        }

        if ((((kakdemocou == 0)) && ((kakcou >= 0))))
        {
            MasHira.sethira2("ﾏｲﾆﾁ", 256, 360, 32, 1);
            tmpsp = tmpsp + (MasMath.Div((((((speed * 14)) - tmpsp) - 7)), 8));
            s = (tmpsp).ToString();
            MasHira.sethira2(s, (520 - (s.Length * 64)), 400, 64, 1);
            MasHira.sethira2("ｷﾛ", 540, 420, 50, 1);
            s = (((time % 1000) + 1000)).ToString();
            s = s.Substring(1);
            MasHira.sethira2(s, 500, 0, 48, 1);
            s = (MasMath.Div(time, 1000)).ToString();
            MasHira.sethira2(s, (410 - (s.Length * 48)), 0, 48, 1);
            MasHira.sethira2("ﾋﾞｮｳ", 420, 24, 24, 1);
        }

        MasPut.movemount(myp);
    }

    public static void putkak()
    {
        if ((kakcou == 0))
            kakcou = 1;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        MasPut.putmount(myp, (xcent - myx), (ycent + myy));
        MasPut.putman(xcent, ycent, ((((myd - MasPut.scd) + (speed * 4))) & 1023), myc);
        MasPut.setlcolor(255);
    }
}
