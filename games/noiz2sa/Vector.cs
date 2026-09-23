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

public static class NrVector
{
    public static float vctInnerProduct(Vector v1, Vector v2)
    {
        return (float)v1.x * v2.x + (float)v1.y * v2.y;
    }

    public static Vector vctGetElement(Vector v1, Vector v2)
    {
        Vector ans = new Vector();
        int ll = v2.x * v2.x + v2.y * v2.y;
        if (ll != 0)
        {
            int mag = GameMath.integer(vctInnerProduct(v1, v2));
            ans.x = GameMath.integer(mag * v2.x / ll);
            ans.y = GameMath.integer(mag * v2.y / ll);
        }
        else
        {
            {
                ans.y = 0;
                ans.x = ans.y;
            }
        }

        return ans;
    }

    public static void vctAdd(Vector v1, Vector v2)
    {
        v1.x = v1.x + (v2.x);
        v1.y = v1.y + (v2.y);
    }

    public static void vctSub(Vector v1, Vector v2)
    {
        v1.x = v1.x - (v2.x);
        v1.y = v1.y - (v2.y);
    }

    public static void vctMul(Vector v1, int a)
    {
        v1.x = v1.x * (a);
        v1.y = v1.y * (a);
    }

    public static void vctDiv(Vector v1, int a)
    {
        v1.x = GameMath.integer(v1.x / (a));
        v1.y = GameMath.integer(v1.y / (a));
    }

    public static int vctCheckSide(Vector checkPos, Vector pos1, Vector pos2)
    {
        int xo = pos2.x - pos1.x, yo = pos2.y - pos1.y;
        if (xo == 0)
        {
            if (yo == 0)
                return 0;
            return checkPos.x - pos1.x;
        }
        else if (yo == 0)
        {
            return pos1.y - checkPos.y;
        }
        else
        {
            if (xo * yo > 0)
            {
                return GameMath.integer((checkPos.x - pos1.x) / xo) - GameMath.integer((checkPos.y - pos1.y) / yo);
            }
            else
            {
                return GameMath.integer(-(checkPos.x - pos1.x) / xo) + GameMath.integer((checkPos.y - pos1.y) / yo);
            }
        }
    }

    public static int vctSize(Vector v)
    {
        return GameMath.integer(sqrt(v.x * v.x + v.y * v.y));
    }

    public static int vctDist(Vector v1, Vector v2)
    {
        int ax = absN(v1.x - v2.x), ay = absN(v1.y - v2.y);
        if (ax > ay)
        {
            return ax + (GameMath.signedShift(ay, 1));
        }
        else
        {
            return ay + (GameMath.signedShift(ax, 1));
        }
    }
}
