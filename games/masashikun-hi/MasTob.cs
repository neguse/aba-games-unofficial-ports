// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasTob
{
    public const int xcent = 230;
    public const int ycent = 300;
    public const int maxspeed = 30;
    public static int myx = 0;
    public static int myy = 0;
    public static int myp = 0;
    public static int myc = 0;
    public static int mychar = 0;
    public static int mymy = 0;
    public static int mymx = 0;
    public static int speed = 0;
    public static int tmpsp = 0;
    public static int mvsum = 0;
    public static int tobtry = 0;
    public static bool ovf = false;
    public static int zm = 0;
    public static Hscdat[] tobhsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static int tobcou = 0;
    public static int tobdemocou = 0;
    public static int[] tby = MasArrays.Make((255 + 1), () => 0);
    public static int tbc = 0;
    public static int tbcc = 0;
    public static int[] tobrec = MasArrays.Make((3 + 1), () => 0);
    public static void cleartobhsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                tobhsc[i].rec = 0;
                tobhsc[i].name = "---";
            }
        }
    }

    public static void settobhiscore(int rec)
    {
        int i = 0;
        int n = 0;
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec > tobhsc[i].rec))
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
                tobhsc[i] = tobhsc[(i - 1)].Copy();
        }

        tobhsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((tobhsc[i].rec).ToString() + "steps"), tobhsc[i].name);
        }

        MasScores.settitle("Super Tobibako");
        tobhsc[n].name = MasScores.entername(n, ((rec).ToString() + "steps"));
    }

    public static void puttobhiscore()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((tobhsc[i].rec).ToString() + "steps"), tobhsc[i].name);
        }

        MasScores.settitle("Super Tobibako");
        MasScores.show();
    }

    public static void inittob()
    {
        int i = 0;
        MasForm.hidemousecursor();
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if ((((i < 96)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                {
                    int choice1 = i;
                    if (choice1 == 124)
                        MasPut.mountdata[i].obj = 7;
                    else if (choice1 == 128)
                        MasPut.mountdata[i].obj = 9;
                    else
                    {
                        MasPut.mountdata[i].obj = -(1);
                    }
                }
            }
        }

        MasPut.mountdata[26].obj = 5;
        MasPut.mountdata[30].obj = 5;
        MasPut.mountdata[34].obj = 5;
        MasPut.mountdata[38].obj = 5;
        myp = 32;
        myx = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        zm = 128;
        tbc = -(1);
        ovf = false;
        {
            i = 0;
            for (; i <= 255; i++)
                tby[i] = ((i * 64) + 512);
        }

        tobcou = 0;
        tobdemocou = 0;
        tobtry = 1;
        tbcc = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(7);
    }

    public static void inittob2()
    {
        int i = 0;
        MasHira.clearhira();
        myp = 32;
        myx = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        zm = 128;
        tbc = -(1);
        ovf = false;
        {
            i = 0;
            for (; i <= 255; i++)
                tby[i] = ((i * 64) + 512);
        }

        tobcou = 0;
        tobdemocou = 0;
        tbcc = 0;
        MasMain.lcolor = 255;
    }

    public static void inittobdemo()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if ((((i < 96)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                {
                    int choice2 = i;
                    if (choice2 == 124)
                        MasPut.mountdata[i].obj = 7;
                    else if (choice2 == 128)
                        MasPut.mountdata[i].obj = 9;
                    else
                    {
                        MasPut.mountdata[i].obj = -(1);
                    }
                }
            }
        }

        MasPut.mountdata[26].obj = 5;
        MasPut.mountdata[30].obj = 5;
        MasPut.mountdata[34].obj = 5;
        MasPut.mountdata[38].obj = 5;
        myp = 32;
        myx = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        zm = 128;
        tbc = -(1);
        ovf = false;
        {
            i = 0;
            for (; i <= 255; i++)
                tby[i] = ((i * 64) + 512);
        }

        tobcou = 0;
        tobdemocou = -(1);
        tobtry = 1;
        tbcc = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(-(4));
        MasHira.sethira2("ｽｰﾊﾟｰﾄﾋﾞﾊﾞｺ", 0, 0, 24, 640);
        MasHira.sethira2("ｴﾌ2ｦｵｼﾃﾈ", 120, 400, 40, 640);
    }

    public static void inittobtitle()
    {
        MasForm.hidemousecursor();
        MasHira.clearhira();
        MasPut.setzoom(0, 0, 256);
        tobcou = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(8);
    }

    public static void movetobtitle()
    {
        int mmv = 0;
        int mbt = 0;
        tobcou = tobcou + (1);
        MasHira.sethira2("ｽｰﾊﾟｰﾄﾋﾞﾊﾞｺ", 100, 200, 50, 1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((tobcou > 165))))
            inittobex();
    }

    public static void puttobtitle()
    {
        int y = 0;
        int i = 0;
        int c = 0;
        y = ((-(tobcou) * 5) + 256);
        {
            i = 0;
            for (; i <= 5; i++)
            {
                y = y + (100);
                if ((((tobcou & 7)) > 3))
                    c = (17 - ((tobcou & 7)));
                else
                    c = (((tobcou & 7)) + 10);
                MasPut.putman(((((((i * 300)) & 511)) - 192) + xcent), y, 0, c);
            }
        }
    }

    public static void inittobex()
    {
        int i = 0;
        MasHira.clearhira();
        {
            i = 0;
            for (; i <= (MasPut.moulen - 1); i++)
            {
                MasPut.mountdata[i].d = 0;
                if ((((i < 96)) && ((MasMath.Random(16) == 0))))
                    MasPut.mountdata[i].obj = (MasMath.Random(4) + 1);
                else
                {
                    int choice3 = i;
                    if (choice3 == 124)
                        MasPut.mountdata[i].obj = 7;
                    else if (choice3 == 128)
                        MasPut.mountdata[i].obj = 9;
                    else
                    {
                        MasPut.mountdata[i].obj = -(1);
                    }
                }
            }
        }

        MasPut.mountdata[26].obj = 5;
        MasPut.mountdata[30].obj = 5;
        MasPut.mountdata[34].obj = 5;
        MasPut.mountdata[38].obj = 5;
        myp = 32;
        myx = 0;
        speed = 0;
        tmpsp = 0;
        mvsum = 0;
        mychar = 0;
        myc = 0;
        myy = 0;
        zm = 128;
        tbc = -(1);
        ovf = false;
        {
            i = 0;
            for (; i <= 255; i++)
                tby[i] = ((i * 64) + 512);
        }

        tobcou = 0;
        tobdemocou = 0;
        tobtry = 1;
        tbcc = 0;
        MasMain.lcolor = 255;
        MasMain.setmainloopspe(9);
    }

    public static void movetobex()
    {
        int mmv = 0;
        int mbt = 0;
        MasHira.sethira2("ｷｮｳｷﾞｾﾂﾒｲ", 0, 0, 48, 1);
        movetob();
        tobdemocou = tobdemocou + (1);
        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((mbt == 1)) || ((tobdemocou > 460))))
            inittob();
    }

    public static void putobject(int ox, int oy, int spe)
    {
        int x = 0;
        int y = 0;
        int i = 0;
        x = ((((((128 - myp)) * 32) - myx) + ox) + xcent);
        y = ((myy - oy) + ycent);
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

    public static void puttobex()
    {
        int i = 0;
        if ((tobcou == 0))
            tobcou = 48;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        MasPut.putmount(myp, (xcent - myx), (ycent + myy));
        {
            int choice4 = tobcou;
            if ((choice4 >= 50 && choice4 <= 54))
                MasPut.putman(xcent, ycent, 0, myc);
            else if (choice4 == 55)
            {
                MasPut.putman(xcent, ycent, 0, myc);
                {
                    i = 0;
                    for (; i <= tbc; i++)
                        putobject(0, tby[i], 9);
                }
            }
            else if ((choice4 >= 56 && choice4 <= 57))
            {
                MasPut.putman(xcent, ycent, 0, myc);
                {
                    i = 0;
                    for (; i <= (tbc - 1); i++)
                        putobject(0, tby[i], 9);
                }

                if ((tbc >= 0))
                    putobject(0, tby[tbc], 10);
            }
            else if ((choice4 >= 58 && choice4 <= 511))
            {
                MasPut.putman(xcent, ycent, MasMain.getdeg(mymx, -(speed)), myc);
                {
                    i = 0;
                    for (; i <= (tbc - 1); i++)
                        putobject(0, tby[i], 9);
                }

                if ((tbc >= 0))
                    putobject(0, tby[tbc], 10);
            }
            else if ((choice4 >= 512 && choice4 <= 1024))
            {
                MasPut.putman(xcent, ycent, 0, myc);
            }
            else
            {
                {
                    MasPut.putman(xcent, ycent, (((speed * 4)) & 1023), myc);
                    putobject((-(speed) * 13), 0, 6);
                }
            }
        }

        MasPut.setlcolor(255);
    }

    public static void movetob()
    {
        string[] imames = new string[]
        {
            "ｲﾏﾀﾞ",
            "ｺｺﾀﾞ",
            "ﾄｳｯ",
            "ｿﾘｬ",
            "ｲｸｿﾞ",
            "ｲｸｾﾞ",
            "ﾁｮﾝﾜ",
            "ﾎｲ"
        };
        string[] atames = new string[]
        {
            "ｸﾞｴｯ",
            "ｶﾞﾝ",
            "ｲﾀｲ",
            "ｺﾞﾝ",
            "ﾍﾞﾁ",
            "ｳｶﾞｯ",
            "ｱｳ",
            "ﾊﾞﾝ"
        };
        int mmv = 0;
        int mbt = 0;
        int t = 0;
        int rec = 0;
        int i = 0;
        int n = 0;
        string s = "";
        int sc = 0;
        if ((tobdemocou == 0))
        {
            mmv = MasForm.mousemv;
            mbt = MasForm.mousebt;
        }
        else if ((tobdemocou > 0))
        {
            if ((tobcou == 48))
                speed = speed + (1);
            mbt = 0;
            mmv = 0;
            {
                int choice5 = tobdemocou;
                if (choice5 == 8)
                    MasHira.sethira2("ﾏｳｽｸﾞﾘｸﾞﾘﾃﾞｶﾙｸﾊｼﾘﾏｼｮｳ", 32, 80, 30, 64);
                else if (choice5 == 80)
                    MasHira.sethira2("ﾔｼﾞﾙｼﾉﾃﾏｴｷﾞﾘｷﾞﾘﾃﾞ", 32, 80, 30, 80);
                else if (choice5 == 86)
                    MasHira.sethira2("ﾎﾞﾀﾝﾃﾞｼﾞｬﾝﾌﾟ", 220, 120, 30, 74);
                else if (choice5 == 103)
                    mbt = 1;
                else if (choice5 == 180)
                    MasHira.sethira2("ﾏｳｽﾃﾞﾊﾊﾞﾀｹ", 32, 80, 30, 64);
                else if (choice5 == 270)
                    MasHira.sethira2("ﾓｳｲｯｶｲﾎﾞﾀﾝｦｵｽﾄ", 32, 80, 30, 140);
                else if (choice5 == 276)
                    MasHira.sethira2("ｲﾁﾊﾞﾝｳｴﾉﾀﾞﾝｶﾞﾌｯﾃｸﾙ", 180, 120, 30, 134);
                else if (choice5 == 280)
                    mbt = 1;
            }

            {
                int choice6 = tobdemocou;
                if ((choice6 >= 105 && choice6 <= 155))
                    return;
                else if ((choice6 >= 290 && choice6 <= 330))
                    return;
            }
        }
        else
        {
            if ((tobcou == 48))
                speed = speed + (1);
            mbt = 0;
            mmv = 0;
            tobdemocou = tobdemocou - (1);
            if (((((tobdemocou < -(148))) && ((MasMath.Random(2) == 0))) && ((tobcou == 48))))
                mbt = 1;
            if (((((speed < 2472)) && ((tobcou == 55))) && ((MasMath.Random(5) == 0))))
                mbt = 1;
            if ((((tobcou > 150)) && ((tobcou < 512))))
                MasTitle.inittitle();
        }

        {
            int choice7 = tobcou;
            if (choice7 == 48)
            {
                speed = speed + (MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense));
                if ((speed >= maxspeed))
                {
                    speed = maxspeed;
                }

                myx = myx + (speed);
                zm = zm + (MasMath.Div(((128 - zm)), 16));
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

                if ((((mbt == 1)) || ((myp >= 128))))
                {
                    {
                        int textZoom = 50;
                        int textMotion = (MasMath.Random(9) - 5);
                        MasHira.sethira(imames[MasMath.Random(8)], 280, 240, textZoom, textMotion);
                    }

                    tobcou = 49;
                    mymy = 12;
                }

                myc = ((mychar & 7));
            }
            else if (choice7 == 49)
            {
                myx = myx + (speed);
                zm = zm + (MasMath.Div(((256 - zm)), 8));
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

                myc = 16;
                myy = myy + (mymy);
                mymy = mymy - (2);
                if ((myp >= 128))
                {
                    MasHira.sethira2("ｼｯﾊﾟｲ", 150, 200, 80, 80);
                    {
                        int textZoom = 64;
                        int textMotion = (MasMath.Random(9) - 5);
                        MasHira.sethira(atames[MasMath.Random(8)], xcent, ycent, textZoom, textMotion);
                    }

                    tobcou = 512;
                    myx = 0;
                    myp = 128;
                }
                else if ((myy <= 0))
                {
                    tobcou = 50;
                    speed = ((speed * (((128 - (((127 - myp)) * 32)) - ((32 - myx))))) * 3);
                    if ((speed < 128))
                        speed = 128;
                    MasPut.mountdata[124].obj = 8;
                }
            }
            else if ((choice7 >= 50 && choice7 <= 54))
            {
                myc = 20;
                zm = zm + (MasMath.Div(((128 - zm)), 8));
                myy = ((((54 - tobcou)) * 4) - 8);
                if ((tobcou == 54))
                    MasPut.mountdata[124].obj = 7;
                tobcou = tobcou + (1);
            }
            else if (choice7 == 55)
            {
                t = (64 - ((MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense)) * 4));
                if ((t < 24))
                    t = 24;
                speed = speed - (t);
                if ((((speed > 3000)) && ((MasMath.Random(5) == 0))))
                {
                    int textZoom = (MasMath.Random(48) + 20);
                    int textMotion = (MasMath.Random(32) - 16);
                    MasHira.sethira("ﾊﾟﾀ", xcent, ycent, textZoom, textMotion);
                }

                myy = myy + (MasMath.Div(speed, 256));
                if (((myy - 64) > (tbc * 64)))
                    tbc = MasMath.Div(((myy - 64)), 64);
                mvsum = mvsum + (MasMath.Div(speed, 256));
                if ((mvsum > 7))
                {
                    mvsum = 0;
                    mychar = mychar + (1);
                }

                if ((((mbt == 1)) || ((speed < 0))))
                {
                    tobcou = 56;
                    tbc = tbc + (1);
                }

                if ((((mychar & 7)) > 3))
                    myc = (17 - ((mychar & 7)));
                else
                    myc = (((mychar & 7)) + 10);
            }
            else if (choice7 == 56)
            {
                t = (64 - ((MasMath.Div(((mmv + MasMath.Div(MasMain.mousesense, 2))), MasMain.mousesense)) * 4));
                if ((t < 24))
                    t = 24;
                speed = speed - (t);
                myy = myy + (MasMath.Div(speed, 256));
                myc = 8;
                if (((myy - 64) > (tbc * 64)))
                    ovf = true;
                else
                {
                    if (ovf)
                    {
                        tobcou = 57;
                        tbcc = (tbc + 2);
                    }
                    else if ((speed < 0))
                    {
                        MasHira.sethira2("ｼｯﾊﾟｲ", 150, 200, 80, 50);
                        tobcou = 58;
                        mymx = 0;
                    }
                }
            }
            else if (choice7 == 57)
            {
                if ((myp < 127))
                    myx = myx + (24);
                else
                    myx = myx + (8);
                myy = myy + (2);
                zm = zm + (MasMath.Div(((128 - zm)), 16));
                myc = 21;
                if ((myx > 31))
                {
                    myx = myx - (32);
                    myp = myp + (1);
                }

                if ((myp > 130))
                {
                    tobcou = 58;
                    speed = 1024;
                    mymx = 4;
                }
            }
            else if ((choice7 >= 58 && choice7 <= 511))
            {
                tobcou = tobcou + (1);
                myx = myx + (mymx);
                speed = speed - (64);
                myy = myy + (MasMath.Div(speed, 256));
                myc = (((mychar & 7)) + 8);
                if ((tbc > 0))
                    zm = zm + (MasMath.Div((((16 + MasMath.Div(30000, ((tbc * 64)))) - zm)), 16));
                if ((myy < 64))
                {
                    myy = 64;
                    mymx = 0;
                }
                else
                    mychar = mychar + (1);
                if ((tobcou > 100))
                {
                    MasMain.lcolor = MasMain.lcolor - (4);
                    if ((MasMain.lcolor < 0))
                        MasMain.lcolor = 0;
                }

                {
                    int choice8 = tobcou;
                    if (choice8 == 100)
                        MasHira.sethira2("ｷﾛｸ", 32, 100, 64, 170);
                    else if (choice8 == 110)
                    {
                        s = (tbcc).ToString();
                        MasHira.sethira2(s, (400 - (s.Length * 64)), 180, 64, 160);
                    }
                    else if (choice8 == 120)
                    {
                        MasHira.sethira2("ﾀﾞﾝ", 420, 220, 32, 150);
                        tobrec[tobtry] = tbcc;
                        if ((tobtry == 3))
                        {
                            rec = -(999999);
                            {
                                i = 1;
                                for (; i <= 3; i++)
                                {
                                    if ((tobrec[i] > rec))
                                    {
                                        rec = tobrec[i];
                                        n = i;
                                    }
                                }
                            }

                            s = (tobrec[3]).ToString();
                            MasHira.sethira2(s, (640 - (s.Length * 32)), ((3 * 32) - 32), 32, 960);
                            MasHira.sethira2("ﾕｱﾍﾞｽﾄ", 256, ((n * 32) - 32), 32, 960);
                            if (MasMain.sucf)
                            {
                                MasHira.sethira2("ﾄﾋﾞﾊﾞｺ", 0, 428, 42, 250);
                                sc = MasMath.Div(300000, ((87 - rec)));
                                if ((sc > 20000))
                                    sc = 20000;
                                else if ((sc < 0))
                                    sc = 20000;
                                if ((rec == 0))
                                    sc = 0;
                                MasMain.score[1] = sc;
                                s = (sc).ToString();
                                MasHira.sethira2(s, (520 - (s.Length * 50)), 420, 50, 250);
                                MasHira.sethira2("ﾃﾝ", 520, 428, 42, 250);
                            }

                            settobhiscore(rec);
                        }
                    }
                }

                if ((((((((tobcou > 120)) && ((mbt == 1)))) || ((tobcou > 370)))) && !(MasScores.hscsf)))
                {
                    tobtry = tobtry + (1);
                    if ((tobtry > 3))
                    {
                        if (MasMain.sucf)
                            MasOok.initooktitle();
                        else
                            MasTitle.inittitle();
                    }
                    else
                        inittob2();
                }
            }
            else if ((choice7 >= 512 && choice7 <= 1024))
            {
                MasMain.lcolor = MasMain.lcolor - (4);
                if ((MasMain.lcolor < 0))
                    MasMain.lcolor = 0;
                myc = 22;
                if ((myy > -(48)))
                    myy = myy - (1);
                tobcou = tobcou + (1);
                if ((tobcou > 590))
                    tobcou = 99;
            }
            else
            {
                {
                    if ((tobcou > 0))
                    {
                        if ((tobcou == 1))
                        {
                            {
                                i = 1;
                                for (; i <= (tobtry - 1); i++)
                                {
                                    s = (tobrec[i]).ToString();
                                    MasHira.sethira2(s, (640 - (s.Length * 32)), ((i * 32) - 32), 32, 960);
                                }
                            }
                        }

                        {
                            int choice9 = tobtry;
                            if (choice9 == 2)
                                MasHira.sethira2("ﾆｶｲﾒ", (tobcou * 16), 100, 64, 1);
                            else if (choice9 == 3)
                                MasHira.sethira2("ｻﾝｶｲﾒ", (tobcou * 16), 100, 64, 1);
                            else
                            {
                                MasHira.sethira2("ｲｯｶｲﾒ", (tobcou * 16), 100, 64, 1);
                            }
                        }

                        tobcou = tobcou + (1);
                        if ((tobcou == 48))
                            MasHira.sethira("ｺﾞｳ", 280, 240, 100, 0);
                    }
                }
            }
        }

        {
            int choice10 = tobcou;
            if ((choice10 >= 0 && choice10 <= 48))
            {
                if (((tobdemocou == 0)))
                {
                    MasHira.sethira2("ﾏｲﾆﾁ", 256, 360, 32, 1);
                    tmpsp = tmpsp + (MasMath.Div((((((speed * 14)) - tmpsp) - 7)), 8));
                    s = (tmpsp).ToString();
                    MasHira.sethira2(s, (520 - (s.Length * 64)), 400, 64, 1);
                    MasHira.sethira2("ｷﾛ", 540, 420, 50, 1);
                }
            }
            else if ((choice10 >= 55 && choice10 <= 99))
            {
                {
                    i = 0;
                    for (; i <= tbc; i++)
                        if ((tby[i] > ((i * 64) + 64)))
                            tby[i] = tby[i] - (64);
                }

                s = ((tbc + 2)).ToString();
                MasHira.sethira2(s, (528 - (s.Length * 64)), 400, 64, 1);
                MasHira.sethira2("ﾀﾞﾝ", 540, 440, 32, 1);
            }
        }

        MasPut.movemount(myp);
    }

    public static void puttob()
    {
        int i = 0;
        if ((tobcou == 0))
            tobcou = 1;
        MasPut.setzoom((xcent - MasMath.Div((xcent * 256), zm)), (ycent - MasMath.Div((ycent * 256), zm)), zm);
        MasPut.setlcolor(MasMain.lcolor);
        MasPut.putmount(myp, (xcent - myx), (ycent + myy));
        {
            int choice11 = tobcou;
            if ((choice11 >= 50 && choice11 <= 54))
                MasPut.putman(xcent, ycent, 0, myc);
            else if (choice11 == 55)
            {
                MasPut.putman(xcent, ycent, 0, myc);
                {
                    i = 0;
                    for (; i <= tbc; i++)
                        putobject(0, tby[i], 9);
                }
            }
            else if ((choice11 >= 56 && choice11 <= 57))
            {
                MasPut.putman(xcent, ycent, 0, myc);
                {
                    i = 0;
                    for (; i <= (tbc - 1); i++)
                        putobject(0, tby[i], 9);
                }

                if ((tbc >= 0))
                    putobject(0, tby[tbc], 10);
            }
            else if ((choice11 >= 58 && choice11 <= 511))
            {
                MasPut.putman(xcent, ycent, MasMain.getdeg(mymx, -(speed)), myc);
                {
                    i = 0;
                    for (; i <= (tbc - 1); i++)
                        putobject(0, tby[i], 9);
                }

                if ((tbc >= 0))
                    putobject(0, tby[tbc], 10);
            }
            else if ((choice11 >= 512 && choice11 <= 1024))
            {
                MasPut.putman(xcent, ycent, 0, myc);
            }
            else
            {
                {
                    MasPut.putman(xcent, ycent, (((speed * 4)) & 1023), myc);
                    putobject((-(speed) * 13), 0, 6);
                }
            }
        }

        MasPut.setlcolor(255);
    }
}
