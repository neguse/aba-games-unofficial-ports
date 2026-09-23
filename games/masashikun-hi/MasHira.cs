// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasHira
{
    public const int hiranum = (8 - 1);
    public const int hira2num = (32 - 1);
    public static MasShape[] hd;
    public static DHira[] hira = MasArrays.Make((hiranum + 1), () => new DHira());
    public static DHira2[] hira2 = MasArrays.Make((hira2num + 1), () => new DHira2());
    public static int hc = 0;
    public static int hc2 = 0;
    public static void inithira()
    {
        int i = 0;
        hc = 0;
        hc2 = 0;
        {
            i = 0;
            for (; i <= hiranum; i++)
                hira[i].cou = 0;
        }

        {
            i = 0;
            for (; i <= hira2num; i++)
                hira2[i].cou = 0;
        }
    }

    public static void clearhira()
    {
        int i = 0;
        {
            i = 0;
            for (; i <= hiranum; i++)
                hira[i].cou = 0;
        }

        {
            i = 0;
            for (; i <= hira2num; i++)
                hira2[i].cou = 0;
        }
    }

    public static void sethira(string mes, int x, int y, int zoom, int mx)
    {
        hira[hc].mes = MasText.Codes(mes);
        hira[hc].x = x;
        hira[hc].y = y;
        hira[hc].zoom = zoom;
        hira[hc].cou = 32;
        hira[hc].mx = mx;
        hc = (((hc + 1)) & hiranum);
    }

    public static void sethira2(string mes, int x, int y, int zoom, int cou)
    {
        int i = 0;
        {
            i = 0;
            for (; i <= hira2num; i++)
            {
                hc2 = (((hc2 + 1)) & hira2num);
                if ((hira2[hc2].cou == 0))
                {
                    hira2[hc2].mes = MasText.Codes(mes);
                    hira2[hc2].x = x;
                    hira2[hc2].y = y;
                    hira2[hc2].zoom = zoom;
                    hira2[hc2].cou = cou;
                    break;
                }
            }
        }
    }

    public static void movehira()
    {
        int r = 0;
        int zoom = 0;
        {
            r = 0;
            for (; r <= hiranum; r++)
            {
                if ((hira[r].cou > 0))
                {
                    {
                        int choice1 = hira[r].cou;
                        if ((choice1 >= 17 && choice1 <= 32))
                            zoom = MasMath.Div((hira[r].zoom * ((32 - hira[r].cou))), 16);
                        else if ((choice1 >= 9 && choice1 <= 16))
                            zoom = MasMath.Div((hira[r].zoom * hira[r].cou), 16);
                        else if ((choice1 >= 0 && choice1 <= 8))
                            zoom = MasMath.Div((hira[r].zoom * ((16 - hira[r].cou))), 16);
                    }

                    hira[r].x = hira[r].x + (hira[r].mx);
                    hira[r].y = hira[r].y - (MasMath.Div((100 * zoom), 256));
                    hira[r].cou = hira[r].cou - (1);
                }
            }
        }

        {
            r = 0;
            for (; r <= hira2num; r++)
            {
                if ((hira2[r].cou > 0))
                    hira2[r].cou = hira2[r].cou - (1);
            }
        }
    }

    public static void puthira()
    {
        int r = 0;
        int x = 0;
        int y = 0;
        int stx = 0;
        int sty = 0;
        int tsty = 0;
        int i = 0;
        int n = 0;
        int p = 0;
        int zoom = 0;
        int tzm = 0;
        int len = 0;
        {
            r = 0;
            for (; r <= hiranum; r++)
            {
                if ((hira[r].cou > 0))
                {
                    stx = hira[r].x;
                    sty = hira[r].y;
                    {
                        int choice2 = hira[r].cou;
                        if ((choice2 >= 17 && choice2 <= 32))
                            zoom = MasMath.Div((hira[r].zoom * ((32 - hira[r].cou))), 16);
                        else if ((choice2 >= 9 && choice2 <= 16))
                            zoom = MasMath.Div((hira[r].zoom * hira[r].cou), 16);
                        else if ((choice2 >= 0 && choice2 <= 8))
                            zoom = MasMath.Div((hira[r].zoom * ((16 - hira[r].cou))), 16);
                    }

                    len = hira[r].mes.Length;
                    stx = stx - (MasMath.Div((zoom * len), 2));
                    {
                        n = 1;
                        for (; n <= len; n++)
                        {
                            i = 0;
                            p = ((int)hira[r].mes[n - 1]);
                            {
                                int choice3 = p;
                                if ((choice3 >= 48 && choice3 <= 57))
                                {
                                    p = p - ((48 - 49));
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice3 == 172)
                                {
                                    p = 36;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice3 == 173)
                                {
                                    p = 37;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice3 == 174)
                                {
                                    p = 38;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice3 == 175)
                                {
                                    p = 18;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice3 == 222)
                                {
                                    p = 46;
                                    tzm = zoom;
                                    tsty = sty;
                                    stx = stx - (zoom);
                                }
                                else if (choice3 == 223)
                                {
                                    p = 47;
                                    tzm = zoom;
                                    tsty = sty;
                                    stx = stx - (zoom);
                                }
                                else if (choice3 == 166)
                                {
                                    p = 48;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice3 == 104)
                                {
                                    p = 59;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice3 == 105)
                                {
                                    p = 60;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else
                                {
                                    {
                                        p = p - (176);
                                        tzm = zoom;
                                        tsty = sty;
                                    }
                                }
                            }

                            while ((hd[p].pd[i].x != -(32768)))
                            {
                                x = (stx + MasMath.Div((hd[p].pd[i].x * tzm), 256));
                                y = (tsty + MasMath.Div((hd[p].pd[i].y * tzm), 256));
                                MasPut.setthicklinefirst(x, y);
                                i = i + (1);
                                while ((hd[p].pd[i].x != -(32760)))
                                {
                                    x = (stx + MasMath.Div((hd[p].pd[i].x * tzm), 256));
                                    y = (tsty + MasMath.Div((hd[p].pd[i].y * tzm), 256));
                                    MasPut.putthickline(x, y);
                                    i = i + (1);
                                }

                                i = i + (1);
                            }

                            stx = stx + (zoom);
                        }
                    }
                }
            }
        }

        {
            r = 0;
            for (; r <= hira2num; r++)
            {
                if ((hira2[r].cou > 0))
                {
                    stx = hira2[r].x;
                    sty = hira2[r].y;
                    zoom = hira2[r].zoom;
                    len = hira2[r].mes.Length;
                    {
                        n = 1;
                        for (; n <= len; n++)
                        {
                            i = 0;
                            p = ((int)hira2[r].mes[n - 1]);
                            {
                                int choice4 = p;
                                if ((choice4 >= 48 && choice4 <= 57))
                                {
                                    p = p - ((48 - 49));
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice4 == 172)
                                {
                                    p = 36;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice4 == 173)
                                {
                                    p = 37;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice4 == 174)
                                {
                                    p = 38;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice4 == 175)
                                {
                                    p = 18;
                                    tzm = MasMath.Div(zoom, 2);
                                    tsty = (sty + tzm);
                                }
                                else if (choice4 == 222)
                                {
                                    p = 46;
                                    tzm = zoom;
                                    tsty = sty;
                                    stx = stx - (zoom);
                                }
                                else if (choice4 == 223)
                                {
                                    p = 47;
                                    tzm = zoom;
                                    tsty = sty;
                                    stx = stx - (zoom);
                                }
                                else if (choice4 == 166)
                                {
                                    p = 48;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice4 == 104)
                                {
                                    p = 59;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else if (choice4 == 105)
                                {
                                    p = 60;
                                    tzm = zoom;
                                    tsty = sty;
                                }
                                else
                                {
                                    {
                                        p = p - (176);
                                        tzm = zoom;
                                        tsty = sty;
                                    }
                                }
                            }

                            while ((hd[p].pd[i].x != -(32768)))
                            {
                                x = (stx + MasMath.Div((hd[p].pd[i].x * tzm), 256));
                                y = (tsty + MasMath.Div((hd[p].pd[i].y * tzm), 256));
                                MasPut.setthicklinefirst2(x, y);
                                i = i + (1);
                                while ((hd[p].pd[i].x != -(32760)))
                                {
                                    x = (stx + MasMath.Div((hd[p].pd[i].x * tzm), 256));
                                    y = (tsty + MasMath.Div((hd[p].pd[i].y * tzm), 256));
                                    MasPut.putthickline2(x, y);
                                    i = i + (1);
                                }

                                i = i + (1);
                            }

                            stx = stx + (zoom);
                        }
                    }
                }
            }
        }
    }
}
