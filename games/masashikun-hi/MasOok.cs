// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasOok
{
    public const int xcent = 230;
    public const int ycent = 300;
    public static Hscdat[] ookhsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static int myx = 0;
    public static int myy = 0;
    public static int myp = 0;
    public static int myc = 0;
    public static int mychar = 0;
    public static int speed = 0;
    public static int mvsum = 0;
    public static int myhng = 0;
    public static int dist = 0;
    public static int endist = 0;
    public static int enhng = 0;
    public static int enspeed = 0;
    public static int plcou = 0;
    public static int enplcou = 0;
    public static int time = 0;
    public static int zm = 0;
    public static int ookcou = 0;
    public static int ookdemocou = 0;
    public static void clearookhsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                ookhsc[i].rec = 99999;
                ookhsc[i].name = "---";
            }
        }
    }

    public static void setookhiscore(int rec)
    {
        int i = 0;
        int n = 0;
        string s = "";
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec < ookhsc[i].rec))
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
                ookhsc[i] = ookhsc[(i - 1)].Copy();
        }

        ookhsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                s = (((ookhsc[i].rec % 1000) + 1000)).ToString();
                s = s.Substring(1);
                s = (((MasMath.Div(ookhsc[i].rec, 1000)).ToString() + "sec") + s);
                MasScores.sethiscore(i, s, ookhsc[i].name);
            }
        }

        MasScores.settitle("Oooka Sabaki");
        s = (((rec % 1000) + 1000)).ToString();
        s = s.Substring(1);
        s = (((MasMath.Div(rec, 1000)).ToString() + "sec") + s);
        ookhsc[n].name = MasScores.entername(n, s);
    }

    public static void putookhiscore()
    {
        int i = 0;
        string s = "";
        {
            i = 1;
            for (; i <= 3; i++)
            {
                s = (((ookhsc[i].rec % 1000) + 1000)).ToString();
                s = s.Substring(1);
                s = (((MasMath.Div(ookhsc[i].rec, 1000)).ToString() + "sec") + s);
                MasScores.sethiscore(i, s, ookhsc[i].name);
            }
        }

        MasScores.settitle("Oooka Sabaki");
        MasScores.show();
    }

    public static void initook()
    {
        int i = 0;
        MasForm.hidemousecursor();
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if (((MasMath.Random(12) == 0)))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                    MasPut.mountdata[i].obj = -(1);
                {
                    int choice1 = i;
                    if (choice1 == 520)
                        MasPut.mountdata[i].obj = 5;
                    else if ((choice1 >= 509 && choice1 <= 519))
                        MasPut.mountdata[i].obj = -(1);
                    else if (choice1 == 508)
                        MasPut.mountdata[i].obj = 5;
                    else if (choice1 == 450)
                        MasPut.mountdata[i].obj = 6;
                }
            }
        }

        myp = 512;
        myx = 0;
        speed = 0;
        mvsum = 0;
        mychar = 0;
        myc = 23;
        myy = 0;
        zm = 128;
        dist = 0;
        myhng = 0;
        endist = 0;
        enhng = 0;
        enspeed = 0;
        plcou = 0;
        enplcou = 0;
        time = 0;
        ookcou = 0;
        ookdemocou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(10);
    }

    public static void initookdemo()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if (((MasMath.Random(12) == 0)))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                    MasPut.mountdata[i].obj = -(1);
                {
                    int choice2 = i;
                    if (choice2 == 520)
                        MasPut.mountdata[i].obj = 5;
                    else if ((choice2 >= 509 && choice2 <= 519))
                        MasPut.mountdata[i].obj = -(1);
                    else if (choice2 == 508)
                        MasPut.mountdata[i].obj = 5;
                }
            }
        }

        myp = 512;
        myx = 0;
        speed = 0;
        mvsum = 0;
        mychar = 0;
        myc = 23;
        myy = 0;
        zm = 128;
        dist = 0;
        myhng = 0;
        endist = 0;
        enhng = 0;
        enspeed = 0;
        plcou = 0;
        enplcou = 0;
        time = 0;
        ookcou = 0;
        ookdemocou = -(1);
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(-(5));
        MasHira.sethira2("ｵｵｵｶｻﾊﾞｷ", 0, 0, 24, 640);
        MasHira.sethira2("ｴﾌ2ｦｵｼﾃﾈ", 120, 400, 40, 640);
    }

    public static void initookex()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if (((MasMath.Random(12) == 0)))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                    MasPut.mountdata[i].obj = -(1);
                {
                    int choice3 = i;
                    if (choice3 == 520)
                        MasPut.mountdata[i].obj = 5;
                    else if ((choice3 >= 509 && choice3 <= 519))
                        MasPut.mountdata[i].obj = -(1);
                    else if (choice3 == 508)
                        MasPut.mountdata[i].obj = 5;
                }
            }
        }

        myp = 512;
        myx = 0;
        speed = 0;
        mvsum = 0;
        mychar = 0;
        myc = 23;
        myy = 0;
        zm = 128;
        dist = 0;
        myhng = 0;
        endist = 0;
        enhng = 0;
        enspeed = 0;
        plcou = 0;
        enplcou = 0;
        time = 0;
        ookcou = 0;
        ookdemocou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(12);
    }

    public static void initooktitle()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        MasPut.setzoom(0, 0, 256);
        ookcou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(11);
    }

    public static void moveooktitle()
    {
        int mmv = 0;
        int mbt = 0;
        ookcou = ookcou + (1);
        MasHira.sethira2("ｵｵｵｶｻﾊﾞｷ", 128, 200, 50, 1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((ookcou > 165))))
            initookex();
    }

    public static void putooktitle()
    {
        MasPut.puthngman(((-(ookcou) * 5) + 640), (ycent + 128), 0, (23 + ((ookcou & 3))));
        MasPut.setthicklinefirst2((((-(ookcou) * 5) + 640) + 30), ((ycent + 128) - 52));
        MasPut.putthickline2(640, ((ycent + 128) - 52));
    }

    public static void moveook()
    {
        string[] hikmes = new string[]
        {
            "ｴｽ",
            "ｵｳ",
            "ﾖｲｼｮ",
            "ｿｲﾔ",
            "ﾋｹ",
            "ｿﾘｬ",
            "ｳﾘ",
            "ｵﾗ"
        };
        string[] itames = new string[]
        {
            "ｲﾃ",
            "ｳｷﾞｬ",
            "ﾔﾒﾛｰ",
            "ﾉﾋﾞﾙｰ",
            "ｸﾞﾜｰ",
            "ﾋｴｰ",
            "ｳｶﾞｶﾞ",
            "ｱｳｰ"
        };
        string[] lstmes = new string[]
        {
            "ｲﾃｲﾝﾀﾞﾖ",
            "ｲｲｶｹﾞﾝｲｼﾅｻｲﾖ",
            "ｲﾃﾏｳｿﾞｺﾗ",
            "ﾅﾆｽﾝﾀﾞ",
            "ｼﾇｶﾄｵﾓｯﾀ",
            "ﾉﾊﾞｼｽｷﾞ",
            "ﾊｲｼｭｳﾘｮｳ",
            "ｿｺﾏﾃﾞ"
        };
        int mmv = 0;
        int mbt = 0;
        string s = "";
        int sc = 0;
        int d = 0;
        if ((ookdemocou == 0))
        {
            mmv = MasForm.mousemv;
            mbt = MasForm.mousebt;
        }
        else if ((ookdemocou > 0))
        {
            speed = speed + (1);
            mmv = 0;
            mbt = 0;
            {
                int choice4 = ookdemocou;
                if (choice4 == 8)
                    MasHira.sethira2("ﾏｳｽｦｸﾞﾘｸﾞﾘｼﾃｶﾗﾀﾞｦｶﾀﾑｹ", 32, 80, 30, 52);
                else if (choice4 == 70)
                    MasHira.sethira2("ｱﾙﾃｲﾄﾞｶﾀﾑｲﾀﾗ", 32, 80, 30, 64);
                else if (choice4 == 141)
                    MasHira.sethira2("ﾎﾞﾀﾝﾃﾞﾋｷﾖｾﾛ", 32, 80, 30, 80);
                else if (choice4 == 142)
                    mbt = 1;
                else if (choice4 == 241)
                    MasHira.sethira2("ｺﾚｦｸﾘｶｴｼﾃﾄﾞﾝﾄﾞﾝﾋｯﾊﾟﾚ", 32, 80, 30, 80);
                else if (choice4 == 250)
                    mbt = 1;
                else if (choice4 == 360)
                    MasHira.sethira2("ﾃｷﾓﾋｯﾊﾟﾙﾉﾃﾞﾕﾀﾞﾝｽﾙﾅ", 32, 80, 30, 72);
                else if (choice4 == 300)
                    mbt = 1;
                else if (choice4 == 350)
                    mbt = 1;
                else if (choice4 == 400)
                    mbt = 1;
            }

            {
                int choice5 = ookdemocou;
                if ((choice5 >= 70 && choice5 <= 140))
                    return;
                else if ((choice5 >= 155 && choice5 <= 240))
                    return;
            }
        }
        else
        {
            speed = speed + (1);
            mbt = 0;
            mmv = 0;
            if (((MasMath.Random(20) == 0)))
                mbt = 1;
            ookdemocou = ookdemocou - (1);
            if ((ookdemocou < -(640)))
                MasTitle.inittitle();
        }

        {
            int choice6 = ookcou;
            if (choice6 == 48)
            {
                time = time + (33);
                if ((plcou == 0))
                {
                    speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
                    speed = speed - (MasMath.Div((speed * dist), 2000));
                    myx = myx - (speed);
                    dist = dist + (speed);
                    zm = zm + (MasMath.Div(((180 - zm)), 16));
                    mvsum = mvsum + (speed);
                    myhng = -(dist);
                }
                else
                {
                    myhng = myhng - (MasMath.Div(myhng, 6));
                    dist = dist - (MasMath.Div(((dist + myhng)), 6));
                    plcou = plcou - (1);
                    if ((plcou == 0))
                        dist = -(myhng);
                    zm = zm + (MasMath.Div(((320 - zm)), 8));
                }

                if ((enplcou == 0))
                {
                    if ((((((ookdemocou <= 0)) || ((ookdemocou > 250)))) && ((time < 20000))))
                        enspeed = enspeed + (1);
                    enspeed = enspeed - (MasMath.Div((enspeed * endist), 2000));
                    endist = endist + (enspeed);
                    enhng = endist;
                    if ((MasMath.Random(16) == 0))
                    {
                        enplcou = 16;
                        enspeed = 0;
                    }
                }
                else
                {
                    enhng = enhng - (MasMath.Div(enhng, 6));
                    myx = myx + (MasMath.Div(((endist - enhng)), 6));
                    endist = endist - (MasMath.Div(((endist - enhng)), 6));
                    enplcou = enplcou - (1);
                    if ((enplcou == 0))
                        endist = enhng;
                }

                d = ((dist - MasMath.Div(((((dist + myhng) + endist) - enhng)), 2)));
                dist = dist - (MasMath.Div(d, 64));
                endist = endist + (MasMath.Div(d, 64));
                d = MasMath.Div(((((dist + myhng) + endist) - enhng)), 128);
                dist = dist - (d);
                myx = myx - (d);
                endist = endist - (d);
                if ((myx < -(31)))
                {
                    myx = myx + (32);
                    myp = myp - (1);
                }
                else if ((myx < -(63)))
                {
                    myx = myx + (64);
                    myp = myp - (2);
                }
                else if ((myx > 31))
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

                myc = (((mychar & 3)) + 23);
                if (((((((plcou > 0)) || ((enplcou > 0)))) && ((MasMath.Random(16) == 0))) && ((ookdemocou <= 0))))
                {
                    int textZoom = 80;
                    int textMotion = (5 - MasMath.Random(9));
                    MasHira.sethira(itames[MasMath.Random(8)], (((xcent + dist) + myhng) + 64), (ycent - 40), textZoom, textMotion);
                }

                if ((plcou == 0))
                {
                    if ((mbt == 1))
                    {
                        {
                            int textZoom = (32 + MasMath.Random(32));
                            int textMotion = -(MasMath.Random(9));
                            MasHira.sethira(hikmes[MasMath.Random(8)], xcent, (ycent - 40), textZoom, textMotion);
                        }

                        plcou = 16;
                        speed = 0;
                    }
                }

                if ((((((((((512 - myp)) * 32) - myx) - dist) - 64) > 1984)) && ((ookdemocou == 0))))
                    ookcou = 65;
            }
            else if ((choice6 >= 65 && choice6 <= 1000))
            {
                ookcou = ookcou + (1);
                dist = dist - (MasMath.Div(dist, 8));
                endist = endist - (MasMath.Div(endist, 8));
                myhng = myhng - (MasMath.Div(myhng, 8));
                enhng = enhng - (MasMath.Div(enhng, 8));
                zm = zm + (MasMath.Div(((256 - zm)), 16));
                if ((ookcou > 128))
                {
                    MasMain.lcolor = MasMain.lcolor - (4);
                    if ((MasMain.lcolor < 0))
                        MasMain.lcolor = 0;
                }

                {
                    int choice7 = ookcou;
                    if (choice7 == 72)
                        MasHira.sethira(lstmes[MasMath.Random(8)], (((xcent + dist) + myhng) + 64), (ycent - 40), 56, 0);
                    else if (choice7 == 128)
                        MasHira.sethira2("ｷﾛｸ", 32, 100, 64, 170);
                    else if (choice7 == 140)
                    {
                        s = (MasMath.Div(time, 1000)).ToString();
                        MasHira.sethira2(s, (220 - (s.Length * 64)), 200, 64, 160);
                    }
                    else if (choice7 == 150)
                        MasHira.sethira2("ﾋﾞｮｳ", 240, 210, 52, 150);
                    else if (choice7 == 160)
                    {
                        s = (((time % 1000) + 1000)).ToString();
                        s = s.Substring(1);
                        MasHira.sethira2(s, 400, 200, 64, 140);
                        if ((MasMain.sucf && ((ookdemocou == 0))))
                        {
                            MasHira.sethira2("ｵｵｵｶ", 0, 428, 42, 250);
                            sc = MasMath.Div(150000000, time);
                            if ((sc > 20000))
                                sc = 20000;
                            else if ((sc < 0))
                                sc = 20000;
                            MasMain.score[2] = sc;
                            s = (sc).ToString();
                            MasHira.sethira2(s, (520 - (s.Length * 50)), 420, 50, 250);
                            MasHira.sethira2("ﾃﾝ", 520, 428, 42, 250);
                        }

                        if ((ookdemocou == 0))
                            setookhiscore(time);
                    }
                }

                if ((((((((ookcou > 160)) && ((mbt == 1)))) || ((ookcou > 410)))) && !(MasScores.hscsf)))
                    if (MasMain.sucf)
                        MasHng.inithngtitle();
                    else
                        MasTitle.inittitle();
            }
            else
            {
                {
                    if ((ookcou > 0))
                    {
                        MasHira.sethira2("ｵｻﾊﾞｷ", (ookcou * 16), 100, 64, 1);
                        ookcou = ookcou + (1);
                        if ((ookcou == 48))
                            MasHira.sethira("ｶｲｼ", 280, 240, 100, 0);
                    }
                }
            }
        }

        if ((((ookcou < 65)) && ((ookdemocou == 0))))
        {
            d = (((1984 - (((((((512 - myp)) * 32) - myx) - dist) - 64)))) * 13);
            if ((d < 0))
                d = 0;
            s = (d).ToString();
            MasHira.sethira2("ｱﾄ", 200, 440, 32, 1);
            MasHira.sethira2(s, (528 - (s.Length * 50)), 420, 50, 1);
            MasHira.sethira2("ﾐﾘ", 540, 440, 32, 1);
            s = (((time % 1000) + 1000)).ToString();
            s = s.Substring(1);
            MasHira.sethira2(s, 500, 0, 48, 1);
            s = (MasMath.Div(time, 1000)).ToString();
            MasHira.sethira2(s, (410 - (s.Length * 48)), 0, 48, 1);
            MasHira.sethira2("ﾋﾞｮｳ", 420, 24, 24, 1);
        }

        MasPut.movemount(myp);
    }

    public static void moveookex()
    {
        int mmv = 0;
        int mbt = 0;
        MasHira.sethira2("ｷｮｳｷﾞｾﾂﾒｲ", 0, 0, 48, 1);
        moveook();
        ookdemocou = ookdemocou + (1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((ookdemocou > 460))))
            initook();
    }

    public static void putookex()
    {
        int nd = 0;
        if ((ookcou == 0))
            ookcou = 48;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        MasPut.putmount2(myp, ((xcent - myx) + myhng), (ycent - myy));
        MasPut.puthngman((xcent + myhng), ycent, myhng, myc);
        nd = (((dist + myhng) + endist) - enhng);
        MasPut.puthngman((((xcent + dist) + myhng) + 64), ycent, ((dist + myhng) - MasMath.Div(nd, 2)), 27);
        MasPut.setthicklinefirst((xcent + 32), (ycent - 52));
        MasPut.putthickline(((xcent + 32) + MasMath.Div(((nd + 64)), 2)), (ycent - 50));
        MasPut.putthickline((((xcent + 32) + nd) + 64), (ycent - 52));
        MasPut.puthngman(((((xcent + 64) + nd) + enhng) + 64), ycent, enhng, 28);
        MasPut.setlcolor(255);
    }

    public static void putook()
    {
        int nd = 0;
        if ((ookcou == 0))
            ookcou = 1;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        MasPut.putmount2(myp, ((xcent - myx) + myhng), (ycent - myy));
        MasPut.puthngman((xcent + myhng), ycent, myhng, myc);
        nd = (((dist + myhng) + endist) - enhng);
        MasPut.puthngman((((xcent + dist) + myhng) + 64), ycent, ((dist + myhng) - MasMath.Div(nd, 2)), 27);
        MasPut.setthicklinefirst((xcent + 32), (ycent - 52));
        MasPut.putthickline(((xcent + 32) + MasMath.Div(((nd + 64)), 2)), (ycent - 50));
        MasPut.putthickline((((xcent + 32) + nd) + 64), (ycent - 52));
        MasPut.puthngman(((((xcent + 64) + nd) + enhng) + 64), ycent, enhng, 28);
        MasPut.setlcolor(255);
    }
}
