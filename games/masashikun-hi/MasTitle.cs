// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasTitle
{
    public static int bfmlspe = 0;
    public static int titlecou = 0;
    public static int demospe = 0;
    public static int td = 0;
    public static int tr = 0;
    public static int stn = 0;
    public static void startpause()
    {
        MasForm.showmousecursor();
        MasHira.clearhira();
        bfmlspe = MasMain.setmainloopspe(-(1));
        MasHira.sethira2("ﾎﾟｰｽﾞ", 200, 150, 64, 10000000);
        MasHira.sethira2("ｴﾌ3ｦｵｽﾄﾂﾂﾞｹﾗﾚﾙ", 100, 300, 32, 10000000);
    }

    public static void endpause()
    {
        MasHira.clearhira();
        MasMain.setmainloopspe(bfmlspe);
        if ((bfmlspe > 0))
            MasForm.hidemousecursor();
    }

    public static void inittitle()
    {
        MasMain.sucf = false;
        MasForm.showmousecursor();
        MasHira.clearhira();
        MasMain.setmainloopspe(0);
        titlecou = 0;
        td = 0;
        tr = 0;
        stn = MasMath.Random(32);
    }

    public static void movetitle()
    {
        string[] subtit = new string[]
        {
            "ｼﾗﾚｻﾞﾙ5ｼｭｷｮｳｷﾞ",
            "ﾎﾝｶｸﾌｳｱｸｼｮﾝｹﾞｰﾑ",
            "ﾋｼﾞﾝﾄﾞｳﾃｷ",
            "ｺﾚﾃﾞｲｲﾉｶ",
            "ｷｷｼﾆﾏｻﾙ",
            "ｼｬﾆﾑﾆｽｽﾒ",
            "ﾔﾏﾄﾀﾞﾏｼｲ",
            "ﾑｹﾞﾝｼﾞｺﾞｸ",
            "ﾑｺﾞｲﾑｺﾞｽｷﾞﾙ",
            "ｿﾚﾊﾑﾘﾀﾞ",
            "ｶﾅｼｷｾﾝｼﾀﾁ",
            "ｷﾛｸｿﾚﾊﾊｶﾅｲ",
            "ｱｼﾀﾆﾑｶｯﾃ",
            "ﾊｼﾞﾒﾉｲｯﾎﾟ",
            "ﾏｴｦｼｯｶﾘﾐﾛ",
            "ﾏﾀﾞﾏﾀﾞｱﾏｲﾈ",
            "ｷﾞﾈｽﾆﾁｮｳｾﾝ",
            "ｹﾞﾝｶｲｦｺｴﾛ",
            "ｵﾚﾀﾁﾆｱｽﾊﾅｲ",
            "ｿﾚﾅﾘﾆ",
            "ｱｽｦﾐｽｴﾙ",
            "ｻﾗﾅﾙﾀｶﾐ",
            "ｺﾝﾅﾓﾝｼﾞｬﾅｲ",
            "ﾑﾘｦｼｮｳﾁﾃﾞ",
            "ｲｯｼﾐﾀﾞﾚﾇ",
            "ﾔﾎﾞｳﾉﾊﾃ",
            "ﾑﾀﾞﾅﾄﾞﾘｮｸ",
            "ｻｲﾊﾃﾉﾁ",
            "ｼﾞﾀﾞｲﾉｶｶﾞﾐ",
            "ｺｴﾃﾐｾﾙ",
            "ﾐﾁﾉｾｶｲ",
            "ﾏｴﾑｷｼｺｳ"
        };
        if ((titlecou > 32))
            MasHira.sethira2("ﾏｻｼｸﾝ", (64 + MasMath.Div((MasMain.dcos[td] * tr), 256)), (160 + MasMath.Div((MasMain.dsin[td] * tr), 256)), 64, 1);
        if ((titlecou > 128))
            MasHira.sethira2((("ｰ" + subtit[stn]) + "ｰ"), ((320 - 32) - (MasText.Codes(subtit[stn]).Length * 16)), 300, 32, 1);
        {
            int choice1 = titlecou;
            if ((choice1 >= 64 && choice1 <= 72))
                MasHira.sethira2("h", 380, 140, ((((72 - titlecou)) * 24) + 92), 1);
            else if (choice1 == 73)
            {
                td = MasMath.Random(1023);
                tr = (48 + MasMath.Random(16));
            }
            else if ((choice1 >= 74 && choice1 <= 80))
            {
                MasHira.sethira2("h", 400, 140, 92, 1);
                MasHira.sethira2("i", 480, 140, ((((80 - titlecou)) * 24) + 92), 1);
            }
            else if (choice1 == 81)
            {
                td = MasMath.Random(1023);
                tr = (48 + MasMath.Random(16));
            }
            else if ((choice1 >= 82 && choice1 <= 300))
            {
                MasHira.sethira2("h", 400, 140, 92, 1);
                MasHira.sethira2("i", 480, 140, 92, 1);
            }
        }

        td = ((((td + 400) + MasMath.Random(224))) & 1023);
        tr = tr - (MasMath.Div(tr, 6));
        if ((tr < 6))
            tr = 0;
        titlecou = titlecou + (1);
        if ((titlecou > 300))
        {
            demospe = demospe + (1);
            {
                int choice2 = ((demospe % 5));
                if (choice2 == 0)
                    MasKak.initkakdemo();
                else if (choice2 == 1)
                    MasTob.inittobdemo();
                else if (choice2 == 2)
                    MasOok.initookdemo();
                else if (choice2 == 3)
                    MasHng.inithngdemo();
                else if (choice2 == 4)
                    MasGfi.initgfidemo();
            }
        }
    }
}
