// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static NrConstants;
using static NrArrays;
using static NrRandom;
using static NrBarrage;
using static NrSound;
using static NrPreference;
using static NrCore;
using static NrAttract;
using static NrShip;
using static NrShot;
using static NrFrag;
using static NrBackground;
using static NrFoe;
using static NrScreen;
using static NrLetter;
using static NrAngles;
using static NrVector;

public static class NrAngles
{
    public static int[] tantbl = Make(TAN_TABLE_SIZE + 2, () => 0);
    public static int[] sctbl = Make(SC_TABLE_SIZE + GameMath.integer(SC_TABLE_SIZE / 4), () => 0);
    public static int getDeg(int x, int y)
    {
        int tx = 0, ty = 0;
        int f = 0, od = 0, tn = 0;
        if ((x == 0) && (y == 0))
        {
            return (512);
        }

        if (x < 0)
        {
            tx = -x;
            if (y < 0)
            {
                ty = -y;
                if (tx > ty)
                {
                    f = 1;
                    od = GameMath.integer(DIV * 3 / 4);
                    tn = GameMath.integer(ty * TAN_TABLE_SIZE / tx);
                }
                else
                {
                    f = -1;
                    od = DIV;
                    tn = GameMath.integer(tx * TAN_TABLE_SIZE / ty);
                }
            }
            else
            {
                ty = y;
                if (tx > ty)
                {
                    f = -1;
                    od = GameMath.integer(DIV * 3 / 4);
                    tn = GameMath.integer(ty * TAN_TABLE_SIZE / tx);
                }
                else
                {
                    f = 1;
                    od = GameMath.integer(DIV / 2);
                    tn = GameMath.integer(tx * TAN_TABLE_SIZE / ty);
                }
            }
        }
        else
        {
            tx = x;
            if (y < 0)
            {
                ty = -y;
                if (tx > ty)
                {
                    f = -1;
                    od = GameMath.integer(DIV / 4);
                    tn = GameMath.integer(ty * TAN_TABLE_SIZE / tx);
                }
                else
                {
                    f = 1;
                    od = 0;
                    tn = GameMath.integer(tx * TAN_TABLE_SIZE / ty);
                }
            }
            else
            {
                ty = y;
                if (tx > ty)
                {
                    f = 1;
                    od = GameMath.integer(DIV / 4);
                    tn = GameMath.integer(ty * TAN_TABLE_SIZE / tx);
                }
                else
                {
                    f = -1;
                    od = GameMath.integer(DIV / 2);
                    tn = GameMath.integer(tx * TAN_TABLE_SIZE / ty);
                }
            }
        }

        return ((od + tantbl[(tn)] * f) & (DIV - 1));
    }

    public static int getDistance(int x, int y)
    {
        if (x < 0)
            x = -x;
        if (y < 0)
            y = -y;
        if (x > y)
        {
            return x + (GameMath.signedShift(y, 1));
        }
        else
        {
            return y + (GameMath.signedShift(x, 1));
        }
    }

    public static float getDistanceFloat(float x, float y)
    {
        if (x < 0)
            x = -x;
        if (y < 0)
            y = -y;
        if (x > y)
        {
            return x + (y / 2);
        }
        else
        {
            return y + (x / 2);
        }
    }
}
