// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Vector
{
    public float x, y;
    public Vector(float x = 0, float y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public float opMul(Vector v)
    {
        return x * v.x + y * v.y;
    }

    public Vector getElement(Vector v)
    {
        Vector rsl = new Vector();
        float ll = v.opMul(v);
        if (ll != 0)
        {
            float mag = this.opMul(v);
            rsl.x = mag * v.x / ll;
            rsl.y = mag * v.y / ll;
        }
        else
        {
            {
                rsl.y = 0;
                rsl.x = rsl.y;
            }
        }

        return rsl;
    }

    public void opAddAssign(Vector v)
    {
        x = x + (v.x);
        y = y + (v.y);
    }

    public void opSubAssign(Vector v)
    {
        x = x - (v.x);
        y = y - (v.y);
    }

    public void opMulAssign(float a)
    {
        x = x * (a);
        y = y * (a);
    }

    public void opDivAssign(float a)
    {
        x = x / (a);
        y = y / (a);
    }

    public float checkSide_2(Vector pos1, Vector pos2)
    {
        float xo = pos2.x - pos1.x;
        float yo = pos2.y - pos1.y;
        if (xo == 0)
        {
            if (yo == 0)
                return 0;
            if (yo > 0)
                return x - pos1.x;
            else
                return pos1.x - x;
        }
        else if (yo == 0)
        {
            if (xo > 0)
                return pos1.y - y;
            else
                return y - pos1.y;
        }
        else
        {
            if (xo * yo > 0)
                return (x - pos1.x) / xo - (y - pos1.y) / yo;
            else
                return -(x - pos1.x) / xo + (y - pos1.y) / yo;
        }
    }

    public float checkSide_3(Vector pos1, Vector pos2, Vector ofs)
    {
        float xo = pos2.x - pos1.x;
        float yo = pos2.y - pos1.y;
        float mx = x + ofs.x;
        float my = y + ofs.y;
        if (xo == 0)
        {
            if (yo == 0)
                return 0;
            if (yo > 0)
                return mx - pos1.x;
            else
                return pos1.x - mx;
        }
        else if (yo == 0)
        {
            if (xo > 0)
                return pos1.y - my;
            else
                return my - pos1.y;
        }
        else
        {
            if (xo * yo > 0)
                return (mx - pos1.x) / xo - (my - pos1.y) / yo;
            else
                return -(mx - pos1.x) / xo + (my - pos1.y) / yo;
        }
    }

    public bool checkCross(Vector p, Vector p1, Vector p2, float width)
    {
        float a1x = 0, a1y = 0, a2x = 0, a2y = 0;
        if (x < p.x)
        {
            a1x = x - width;
            a2x = p.x + width;
        }
        else
        {
            a1x = p.x - width;
            a2x = x + width;
        }

        if (y < p.y)
        {
            a1y = y - width;
            a2y = p.y + width;
        }
        else
        {
            a1y = p.y - width;
            a2y = y + width;
        }

        float b1x = 0, b1y = 0, b2x = 0, b2y = 0;
        if (p2.y < p1.y)
        {
            b1y = p2.y - width;
            b2y = p1.y + width;
        }
        else
        {
            b1y = p1.y - width;
            b2y = p2.y + width;
        }

        if ((a2y >= b1y) && (b2y >= a1y))
        {
            if (p2.x < p1.x)
            {
                b1x = p2.x - width;
                b2x = p1.x + width;
            }
            else
            {
                b1x = p1.x - width;
                b2x = p2.x + width;
            }

            if ((a2x >= b1x) && (b2x >= a1x))
            {
                float a = y - p.y;
                float b = p.x - x;
                float c = p.x * y - p.y * x;
                float d = p2.y - p1.y;
                float e = p1.x - p2.x;
                float f = p1.x * p2.y - p1.y * p2.x;
                float dnm = b * d - a * e;
                if (dnm != 0)
                {
                    float x = (b * f - c * e) / dnm;
                    float y = (c * d - a * f) / dnm;
                    if ((((((((a1x <= x) && (x <= a2x)) && (a1y <= y)) && (y <= a2y)) && (b1x <= x)) && (x <= b2x)) && (b1y <= y)) && (y <= b2y))
                        return true;
                }
            }
        }

        return false;
    }

    public bool checkHitDist(Vector p, Vector pp, float dist)
    {
        float bmvx = 0, bmvy = 0, inaa = 0;
        bmvx = pp.x;
        bmvy = pp.y;
        bmvx = bmvx - (p.x);
        bmvy = bmvy - (p.y);
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx = 0, sofsy = 0, inab = 0, hd = 0;
            sofsx = x;
            sofsy = y;
            sofsx = sofsx - (p.x);
            sofsy = sofsy - (p.y);
            inab = bmvx * sofsx + bmvy * sofsy;
            if ((inab >= 0) && (inab <= inaa))
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if ((hd >= 0) && (hd <= dist))
                    return true;
            }
        }

        return false;
    }

    public float size()
    {
        return sqrt(x * x + y * y);
    }

    public float dist(Vector v)
    {
        float ax = fabs(x - v.x);
        float ay = fabs(y - v.y);
        if (ax > ay)
            return ax + ay / 2;
        else
            return ay + ax / 2;
    }
}

public class Vector3
{
    public float x, y, z;
    public Vector3(float x = 0, float y = 0, float z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void rollX(float d)
    {
        float ty = y * cos(d) - z * sin(d);
        z = y * sin(d) + z * cos(d);
        y = ty;
    }

    public void rollY(float d)
    {
        float tx = x * cos(d) - z * sin(d);
        z = x * sin(d) + z * cos(d);
        x = tx;
    }

    public void rollZ(float d)
    {
        float tx = x * cos(d) - y * sin(d);
        y = x * sin(d) + y * cos(d);
        x = tx;
    }

    public void blend(Vector3 v1, Vector3 v2, float ratio)
    {
        x = v1.x * ratio + v2.x * (1 - ratio);
        y = v1.y * ratio + v2.y * (1 - ratio);
        z = v1.z * ratio + v2.z * (1 - ratio);
    }

    public void opAddAssign(Vector3 v)
    {
        x = x + (v.x);
        y = y + (v.y);
        z = z + (v.z);
    }

    public void opSubAssign(Vector3 v)
    {
        x = x - (v.x);
        y = y - (v.y);
        z = z - (v.z);
    }

    public void opMulAssign(float a)
    {
        x = x * (a);
        y = y * (a);
        z = z * (a);
    }

    public void opDivAssign(float a)
    {
        x = x / (a);
        y = y / (a);
        z = z / (a);
    }
}
