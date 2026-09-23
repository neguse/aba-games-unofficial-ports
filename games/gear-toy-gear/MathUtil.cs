// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class MathUtil
{
    public static float NormalizeDirection(float d)
    {
        float rd = d;
        if (rd < -Math.PI)
            rd = (float)(Math.PI * 2 - (-rd % (Math.PI * 2)));
        return (float)((rd + Math.PI) % (Math.PI * 2) - Math.PI);
    }

    public static bool ContainsVector2Vector2(Vector2 p, Vector2 size)
    {
        return (p.X <= size.X && p.X >= -size.X && p.Y <= size.Y && p.Y >= -size.Y);
    }

    public static bool ContainsVector2Vector2float(Vector2 p, Vector2 o, float size)
    {
        return ((p.X - o.X) <= size && (p.X - o.X) >= -size && (p.Y - o.Y) <= size && (p.Y - o.Y) >= -size);
    }

    public static bool ContainsVector2Vector2Vector2(Vector2 p, Vector2 o, Vector2 size)
    {
        return ((p.X - o.X) <= size.X && (p.X - o.X) >= -size.X && (p.Y - o.Y) <= size.Y && (p.Y - o.Y) >= -size.Y);
    }

    public static Vector2 GetVectorElement(Vector2 v1, Vector2 v2)
    {
        Vector2 rsl = new Vector2();
        float ll = Vector2.Dot((v2).Copy(), (v2).Copy());
        if (ll != 0)
        {
            float mag = Vector2.Dot((v1).Copy(), (v2).Copy());
            rsl.X = mag * v2.X / ll;
            rsl.Y = mag * v2.Y / ll;
        }
        else
        {
            {
                rsl.Y = 0;
                rsl.X = rsl.Y;
            }
        }

        return (rsl).Copy();
    }

    public static float CheckVectorSide(Vector2 p, Vector2 pos1, Vector2 pos2)
    {
        float xo = pos2.X - pos1.X;
        float yo = pos2.Y - pos1.Y;
        if (xo == 0)
        {
            if (yo == 0)
                return 0;
            if (yo > 0)
                return p.X - pos1.X;
            else
                return pos1.X - p.X;
        }
        else if (yo == 0)
        {
            if (xo > 0)
                return pos1.Y - p.Y;
            else
                return p.Y - pos1.Y;
        }
        else
        {
            if (xo * yo > 0)
                return (p.X - pos1.X) / xo - (p.Y - pos1.Y) / yo;
            else
                return -(p.X - pos1.X) / xo + (p.Y - pos1.Y) / yo;
        }
    }

    public static float CalcDistance(Vector2 p, Vector2 p1, Vector2 p2)
    {
        float a = p2.Y - p1.Y;
        float b = -(p2.X - p1.X);
        float c = p2.X * p1.Y - p1.X * p2.Y;
        //return (float)(Math.Abs(a * p.X + b * p.Y + c) / Math.Sqrt(a * a + b * b));
        return (float)((a * p.X + b * p.Y + c) / Math.Sqrt(a * a + b * b));
    }

    public static bool CheckHitDistance(Vector2 pos, Vector2 p, Vector2 pp, float distance)
    {
        float bmvx, bmvy, inaa;
        bmvx = pp.X;
        bmvy = pp.Y;
        bmvx -= p.X;
        bmvy -= p.Y;
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx, sofsy, inab, hd;
            sofsx = pos.X;
            sofsy = pos.Y;
            sofsx -= p.X;
            sofsy -= p.Y;
            inab = bmvx * sofsx + bmvy * sofsy;
            if (inab >= 0 && inab <= inaa)
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if (hd >= 0 && hd <= distance)
                    return true;
            }
        }

        return false;
    }
}
