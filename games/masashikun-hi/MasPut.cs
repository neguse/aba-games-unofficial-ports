// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static MasMath;

public static class MasPut
{
    public const int moulen = 1024;
    public static int lwidth = 0;
    public static Moudat[] mountdata = MasArrays.Make(((moulen - 1) + 1), () => new Moudat());
    public static int scd = 0;
    public static int tlx = 0;
    public static int tly = 0;
    public static int xwd = 0;
    public static int ywd = 0;
    public static int zoomwd = 0;
    public static int lcolor = 0;
    public const int dftwidth = (640 * 256);
    public const int dftheight = (480 * 256);
    public const int dftwidth2 = 640;
    public const int dftheight2 = 480;
    public static void setlcolor(int c)
    {
        lcolor = c;
    }

    public static void setzoom(int x, int y, int zoom)
    {
        xwd = x;
        ywd = y;
        zoomwd = zoom;
    }

    public static void setthicklinefirst(int x, int y)
    {
        tlx = ((MasMath.Div(((((x - xwd)) * MasForm.sxmax) * zoomwd), dftwidth) + MasMath.Random(3)) - 1);
        tly = ((MasMath.Div(((((y - ywd)) * MasForm.symax) * zoomwd), dftheight) + MasMath.Random(3)) - 1);
    }

    public static bool putthickline(int x, int y)
    {
        bool result = false;
        int sx = 0;
        int sy = 0;
        int scx = 0;
        int scy = 0;
        sx = ((MasMath.Div(((((x - xwd)) * MasForm.sxmax) * zoomwd), dftwidth) + MasMath.Random(3)) - 1);
        sy = ((MasMath.Div(((((y - ywd)) * MasForm.symax) * zoomwd), dftheight) + MasMath.Random(3)) - 1);
        scx = ((MasMath.Div(((sx + tlx)), 2) + MasMath.Random(3)) - 1);
        scy = ((MasMath.Div(((sy + tly)), 2) + MasMath.Random(3)) - 1);
        result = MasForm.putline(tlx, tly, scx, scy, lwidth, lcolor);
        result = (MasForm.putline(scx, scy, sx, sy, lwidth, lcolor) && result);
        tlx = sx;
        tly = sy;
        return result;
    }

    public static void setthicklinefirst2(int x, int y)
    {
        tlx = ((MasMath.Div((x * MasForm.sxmax), dftwidth2) + MasMath.Random(3)) - 1);
        tly = ((MasMath.Div((y * MasForm.symax), dftheight2) + MasMath.Random(3)) - 1);
    }

    public static bool putthickline2(int x, int y)
    {
        bool result = false;
        int sx = 0;
        int sy = 0;
        int scx = 0;
        int scy = 0;
        sx = ((MasMath.Div((x * MasForm.sxmax), dftwidth2) + MasMath.Random(3)) - 1);
        sy = ((MasMath.Div((y * MasForm.symax), dftheight2) + MasMath.Random(3)) - 1);
        scx = ((MasMath.Div(((sx + tlx)), 2) + MasMath.Random(3)) - 1);
        scy = ((MasMath.Div(((sy + tly)), 2) + MasMath.Random(3)) - 1);
        result = MasForm.putline(tlx, tly, scx, scy, lwidth, lcolor);
        result = (MasForm.putline(scx, scy, sx, sy, lwidth, lcolor) && result);
        tlx = sx;
        tly = sy;
        return result;
    }

    public static void movemount(int pmount)
    {
        scd = MasMath.Div((((scd * 3) + mountdata[pmount].d)), 4);
    }

    public static void putmount(int pmount, int ox, int oy)
    {
        int d = 0;
        int i = 0;
        int spe = 0;
        int x1 = 0;
        int y1 = 0;
        int xo = 0;
        int yo = 0;
        int p = 0;
        x1 = ox;
        y1 = oy;
        p = pmount;
        setthicklinefirst(x1, y1);
        while (true)
        {
            d = (((mountdata[p].d - scd)) & 1023);
            if ((mountdata[p].obj != -(1)))
            {
                xo = x1;
                yo = y1;
                setthicklinefirst(xo, yo);
                i = 0;
                spe = mountdata[p].obj;
                while ((MasMain.obd[spe].pd[i].x != -(32768)))
                {
                    xo = xo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dcos[d]), 256) - MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dsin[d]), 256)));
                    yo = yo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dsin[d]), 256) + MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dcos[d]), 256)));
                    putthickline(xo, yo);
                    i = i + (1);
                }

                setthicklinefirst(x1, y1);
            }

            x1 = (x1 + MasMath.Div(MasMain.dcos[d], 8));
            y1 = (y1 + MasMath.Div(MasMain.dsin[d], 8));
            if ((putthickline(x1, y1) == false))
                break;
            p = p + (1);
        }

        x1 = ox;
        y1 = oy;
        p = pmount;
        setthicklinefirst(x1, y1);
        while (true)
        {
            p = p - (1);
            d = (((mountdata[p].d - scd)) & 1023);
            x1 = x1 - (MasMath.Div(MasMain.dcos[d], 8));
            y1 = y1 - (MasMath.Div(MasMain.dsin[d], 8));
            if ((putthickline(x1, y1) == false))
                break;
            if ((mountdata[p].obj != -(1)))
            {
                xo = x1;
                yo = y1;
                setthicklinefirst(xo, yo);
                i = 0;
                spe = mountdata[p].obj;
                while ((MasMain.obd[spe].pd[i].x != -(32768)))
                {
                    xo = xo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dcos[d]), 256) - MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dsin[d]), 256)));
                    yo = yo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dsin[d]), 256) + MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dcos[d]), 256)));
                    putthickline(xo, yo);
                    i = i + (1);
                }

                setthicklinefirst(x1, y1);
            }
        }
    }

    public static void putmount2(int pmount, int ox, int oy)
    {
        int d = 0;
        int i = 0;
        int spe = 0;
        int x1 = 0;
        int y1 = 0;
        int xo = 0;
        int yo = 0;
        int p = 0;
        bool pf = false;
        pf = false;
        x1 = ox;
        y1 = oy;
        p = pmount;
        setthicklinefirst(x1, y1);
        while (true)
        {
            d = (((mountdata[p].d - scd)) & 1023);
            if ((mountdata[p].obj != -(1)))
            {
                xo = x1;
                yo = y1;
                setthicklinefirst(xo, yo);
                i = 0;
                spe = mountdata[p].obj;
                while ((MasMain.obd[spe].pd[i].x != -(32768)))
                {
                    xo = xo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dcos[d]), 256) - MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dsin[d]), 256)));
                    yo = yo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dsin[d]), 256) + MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dcos[d]), 256)));
                    putthickline(xo, yo);
                    i = i + (1);
                }

                setthicklinefirst(x1, y1);
            }

            x1 = (x1 + MasMath.Div(MasMain.dcos[d], 8));
            y1 = (y1 + MasMath.Div(MasMain.dsin[d], 8));
            if ((p > 960))
                break;
            if (((putthickline(x1, y1) == false)))
            {
                if (pf)
                    break;
            }
            else
                pf = true;
            p = p + (1);
        }

        x1 = ox;
        y1 = oy;
        p = pmount;
        setthicklinefirst(x1, y1);
        while (true)
        {
            p = p - (1);
            d = (((mountdata[p].d - scd)) & 1023);
            x1 = x1 - (MasMath.Div(MasMain.dcos[d], 8));
            y1 = y1 - (MasMath.Div(MasMain.dsin[d], 8));
            if ((putthickline(x1, y1) == false))
                break;
            if ((mountdata[p].obj != -(1)))
            {
                xo = x1;
                yo = y1;
                setthicklinefirst(xo, yo);
                i = 0;
                spe = mountdata[p].obj;
                while ((MasMain.obd[spe].pd[i].x != -(32768)))
                {
                    xo = xo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dcos[d]), 256) - MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dsin[d]), 256)));
                    yo = yo + ((MasMath.Div((MasMain.obd[spe].pd[i].x * MasMain.dsin[d]), 256) + MasMath.Div((MasMain.obd[spe].pd[i].y * MasMain.dcos[d]), 256)));
                    putthickline(xo, yo);
                    i = i + (1);
                }

                setthicklinefirst(x1, y1);
            }
        }
    }

    public static void puthngman(int xc, int yc, int wx, int p)
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
                x = x + (MasMath.Div((wx * ((y - yc))), 53));
                while ((MasMain.md[p].pd[i].x != -(32760)))
                {
                    x = x + (MasMain.md[p].pd[i].x);
                    y = y + (MasMain.md[p].pd[i].y);
                    putthickline(x, y);
                    i = i + (1);
                }

                i = i + (1);
            }
            else
            {
                x = (xc + MasMain.md[p].pd[i].x);
                y = (yc + MasMain.md[p].pd[i].y);
                setthicklinefirst((x + MasMath.Div((wx * ((y - yc))), 53)), y);
                i = i + (1);
                while ((MasMain.md[p].pd[i].x != -(32760)))
                {
                    y = y + (MasMain.md[p].pd[i].y);
                    x = x + (MasMain.md[p].pd[i].x);
                    putthickline((x + MasMath.Div((wx * ((y - yc))), 53)), y);
                    i = i + (1);
                }

                i = i + (1);
            }
        }
    }

    public static void putman(int xc, int yc, int d, int p)
    {
        int x = 0;
        int y = 0;
        int i = 0;
        i = 0;
        while ((MasMain.md[p].pd[i].x != -(32768)))
        {
            x = ((xc + MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dcos[d]), 256)) - MasMath.Div((MasMain.md[p].pd[i].y * MasMain.dsin[d]), 256));
            y = ((yc + MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dsin[d]), 256)) + MasMath.Div((MasMain.md[p].pd[i].y * MasMain.dcos[d]), 256));
            setthicklinefirst(x, y);
            i = i + (1);
            while ((MasMain.md[p].pd[i].x != -(32760)))
            {
                x = x + ((MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dcos[d]), 256) - MasMath.Div((MasMain.md[p].pd[i].y * MasMain.dsin[d]), 256)));
                y = y + ((MasMath.Div((MasMain.md[p].pd[i].x * MasMain.dsin[d]), 256) + MasMath.Div((MasMain.md[p].pd[i].y * MasMain.dcos[d]), 256)));
                putthickline(x, y);
                i = i + (1);
            }

            i = i + (1);
        }
    }
}
