// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;

public static class MasMain
{
    public static int[] dsin, dcos, tantab;
    public static int mlspe, lcolor;
    public static int mousesense = 128;
    public static MasShape[] obd, md;
    public static bool sucf;
    public static int[] score = MasArrays.Make(5, () => 0);
    public static void initall()
    {
        MasData.Load();
        MasHira.inithira();
        MasScores.Clear();
        MasPut.lwidth = 4;
        MasPut.setzoom(0, 0, 256);
        MasPut.setlcolor(255);
        MasTitle.inittitle();
    }

    public static int setmainloopspe(int spe)
    {
        int result = 0;
        result = mlspe;
        mlspe = spe;
        return result;
    }

    public static int getdeg(int x, int y)
    {
        int result = 0;
        int tx = 0;
        int ty = 0;
        int f = 0;
        int od = 0;
        int tn = 0;
        if ((((x == 0)) && ((y == 0))))
        {
            result = 0;
            return result;
        }

        if ((x < 0))
        {
            tx = -(x);
            if ((y < 0))
            {
                ty = -(y);
                if ((tx > ty))
                {
                    f = 1;
                    od = (896 - 128);
                    tn = MasMath.Div((ty * 256), tx);
                }
                else
                {
                    f = -(1);
                    od = (896 + 128);
                    tn = MasMath.Div((tx * 256), ty);
                }
            }
            else
            {
                ty = y;
                if ((tx > ty))
                {
                    f = -(1);
                    od = (640 + 128);
                    tn = MasMath.Div((ty * 256), tx);
                }
                else
                {
                    f = 1;
                    od = (640 - 128);
                    tn = MasMath.Div((tx * 256), ty);
                }
            }
        }
        else
        {
            tx = x;
            if ((y < 0))
            {
                ty = -(y);
                if ((tx > ty))
                {
                    f = -(1);
                    od = (128 + 128);
                    tn = MasMath.Div((ty * 256), tx);
                }
                else
                {
                    f = 1;
                    od = (128 - 128);
                    tn = MasMath.Div((tx * 256), ty);
                }
            }
            else
            {
                ty = y;
                if ((tx > ty))
                {
                    f = 1;
                    od = (384 - 128);
                    tn = MasMath.Div((ty * 256), tx);
                }
                else
                {
                    f = -(1);
                    od = (384 + 128);
                    tn = MasMath.Div((tx * 256), ty);
                }
            }
        }

        // At index 256 the 1.11e executable reads the adjacent game state.
        result = (((od + ((tn == 256 ? mlspe : tantab[tn]) * f))) & 1023);
        return result;
    }

    public static void moveall()
    {
        MasHira.movehira();
        {
            int choice1 = mlspe;
            if (choice1 == 0)
                MasTitle.movetitle();
            else if (choice1 == 1 || choice1 == -(2))
                MasKak.movekak();
            else if (choice1 == 2)
                MasKak.movekaktitle();
            else if (choice1 == 3)
                MasKak.movekakex();
            else if (choice1 == 4 || choice1 == -(3))
                MasHng.movehng();
            else if (choice1 == 5)
                MasHng.movehngtitle();
            else if (choice1 == 6)
                MasHng.movehngex();
            else if (choice1 == 7 || choice1 == -(4))
                MasTob.movetob();
            else if (choice1 == 8)
                MasTob.movetobtitle();
            else if (choice1 == 9)
                MasTob.movetobex();
            else if (choice1 == 10 || choice1 == -(5))
                MasOok.moveook();
            else if (choice1 == 11)
                MasOok.moveooktitle();
            else if (choice1 == 12)
                MasOok.moveookex();
            else if (choice1 == 13 || choice1 == -(6))
                MasGfi.movegfi();
            else if (choice1 == 14)
                MasGfi.movegfititle();
            else if (choice1 == 15)
                MasGfi.movegfiex();
            else if (choice1 == 16)
                MasResult.moveresult();
        }
    }

    public static void putall()
    {
        {
            int choice2 = mlspe;
            if (choice2 == 1 || choice2 == -(2))
                MasKak.putkak();
            else if (choice2 == 2)
                MasKak.putkaktitle();
            else if (choice2 == 3)
                MasKak.putkakex();
            else if (choice2 == 4 || choice2 == -(3))
                MasHng.puthng();
            else if (choice2 == 5)
                MasHng.puthngtitle();
            else if (choice2 == 6)
                MasHng.puthngex();
            else if (choice2 == 7 || choice2 == -(4))
                MasTob.puttob();
            else if (choice2 == 8)
                MasTob.puttobtitle();
            else if (choice2 == 9)
                MasTob.puttobex();
            else if (choice2 == 10 || choice2 == -(5))
                MasOok.putook();
            else if (choice2 == 11)
                MasOok.putooktitle();
            else if (choice2 == 12)
                MasOok.putookex();
            else if (choice2 == 13 || choice2 == -(6))
                MasGfi.putgfi();
            else if (choice2 == 14)
                MasGfi.putgfititle();
            else if (choice2 == 15)
                MasGfi.putgfiex();
            else if (choice2 == 16)
                MasResult.putresult();
        }

        MasHira.puthira();
    }
}
