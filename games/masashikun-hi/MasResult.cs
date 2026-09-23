// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasResult
{
    public static int resultcou = 0;
    public static Hscdat[] ttlhsc = MasArrays.Make((3 + 1), () => new Hscdat());
    public static void clearttlhsc()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
            {
                ttlhsc[i].rec = 0;
                ttlhsc[i].name = "---";
            }
        }
    }

    public static void setttlhiscore(int rec)
    {
        int i = 0;
        int n = 0;
        n = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                if ((rec > ttlhsc[i].rec))
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
                ttlhsc[i] = ttlhsc[(i - 1)].Copy();
        }

        ttlhsc[n].rec = rec;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((ttlhsc[i].rec).ToString() + "point"), ttlhsc[i].name);
        }

        MasScores.settitle("Best of Masashi");
        ttlhsc[n].name = MasScores.entername(n, ((rec).ToString() + "point"));
    }

    public static void putttlhiscore()
    {
        int i = 0;
        {
            i = 1;
            for (; i <= 3; i++)
                MasScores.sethiscore(i, ((ttlhsc[i].rec).ToString() + "point"), ttlhsc[i].name);
        }

        MasScores.settitle("Best of Masashi");
        MasScores.show();
    }

    public static void initresult()
    {
        MasHira.clearhira();
        resultcou = 0;
        MasMain.setmainloopspe(16);
    }

    public static void moveresult()
    {
        string[] kyonum = new string[]
        {
            "ｶｹﾇｹﾛ",
            "ﾄﾋﾞﾊﾞｺ",
            "ｵｵｵｶ",
            "ﾋﾄﾅｹﾞ",
            "ﾌｱｲﾔｰ"
        };
        int i = 0;
        int y = 0;
        int sc = 0;
        int mmv = 0;
        int mbt = 0;
        string s = "";
        int c = 0;
        resultcou = resultcou + (1);
        MasHira.sethira2("ｻｲｼｭｳｹｯｶ", 100, 0, 40, 1);
        y = 64;
        c = MasMath.Div(((resultcou - 16)), 16);
        if ((c > 4))
            c = 4;
        if ((resultcou >= 16))
        {
            i = 0;
            for (; i <= c; i++)
            {
                MasHira.sethira2(kyonum[i], 0, y, 42, 1);
                s = (MasMain.score[i]).ToString();
                MasHira.sethira2(s, (540 - (s.Length * 50)), (y - 8), 50, 1);
                MasHira.sethira2("ﾃﾝ", 540, y, 42, 1);
                y = y + (64);
            }
        }

        if ((resultcou == 112))
        {
            sc = 0;
            {
                i = 0;
                for (; i <= 4; i++)
                    sc = sc + (MasMain.score[i]);
            }

            s = (sc).ToString();
            MasHira.sethira2(s, (540 - (s.Length * 50)), (380 - 8), 50, 9999);
            MasHira.sethira2("ﾃﾝ", 540, 380, 42, 9999);
            setttlhiscore(sc);
        }

        mmv = MasForm.mousemv;
        mbt = MasForm.mousebt;
        if ((((((((resultcou > 112)) && ((mbt == 1)))) || ((resultcou > 350)))) && !(MasScores.hscsf)))
            MasTitle.inittitle();
    }

    public static void putresult()
    {
        {
            int choice1 = resultcou;
            if ((choice1 >= 96 && choice1 <= 350))
            {
                MasPut.setthicklinefirst2(8, 365);
                MasPut.putthickline2(632, 365);
            }
        }
    }
}
