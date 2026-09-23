// Copyright 2001 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static WkCore;
using static WkAttract;
using static WkBall;
using static WkBoard;
using static WkGenerator;
using static WkPan;
using static WkVector;
using static WkConstants;
using static WkArrays;
using static WkRandom;
using static WkScreen;
using static WkSound;

public static class WkVector
{
    public static float vctInnerProduct(Vector v1, Vector v2)
    {
        return v1.x * v2.x + v1.y * v2.y;
    }

    public static Vector vctGetElement(Vector v1, Vector v2)
    {
        Vector ans = new Vector();
        float ll = v2.x * v2.x + v2.y * v2.y;
        if (ll != 0)
        {
            float mag = vctInnerProduct(v1, v2);
            ans.x = mag * v2.x / ll;
            ans.y = mag * v2.y / ll;
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

    public static void vctMul(Vector v1, float a)
    {
        v1.x = v1.x * (a);
        v1.y = v1.y * (a);
    }

    public static void vctDiv(Vector v1, float a)
    {
        v1.x = v1.x / (a);
        v1.y = v1.y / (a);
    }

    public static float vctCheckSide(Vector checkPos, Vector pos1, Vector pos2)
    {
        float xo = pos2.x - pos1.x, yo = pos2.y - pos1.y;
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
                return (checkPos.x - pos1.x) / xo - (checkPos.y - pos1.y) / yo;
            }
            else
            {
                return -(checkPos.x - pos1.x) / xo + (checkPos.y - pos1.y) / yo;
            }
        }
    }

    public static float vctSize(Vector v)
    {
        return sqrt(v.x * v.x + v.y * v.y);
    }
}
