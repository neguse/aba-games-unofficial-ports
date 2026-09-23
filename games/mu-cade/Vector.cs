// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Vector
{
    public static float operator *(Vector a, Vector b)
    {
        return a.opMul_1_Vector(b);
    }

    public static Vector operator +(Vector a, Vector b)
    {
        a.opAddAssign_1_Vector(b);
        return a;
    }

    public static Vector operator -(Vector a, Vector b)
    {
        a.opSubAssign_1_Vector(b);
        return a;
    }

    public static Vector operator *(Vector a, float n)
    {
        a.opMulAssign(n);
        return a;
    }

    public static Vector operator /(Vector a, float n)
    {
        a.opDivAssign(n);
        return a;
    }

    public float x, y;
    public static Vector rsl;
    public Vector(float x = 0, float y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public virtual void clear()
    {
        y = 0;
        x = y;
    }

    public virtual float opMul_1_Vector(Vector v)
    {
        return x * v.x + y * v.y;
    }

    public virtual Vector getElement_1_Vector(Vector v)
    {
        float ll = v * v;
        if (ll != 0)
        {
            float mag = this * v;
            rsl.x = mag * v.x / ll;
            rsl.y = mag * v.y / ll;
        }
        else
        {
            rsl.y = 0;
            rsl.x = rsl.y;
        }

        return rsl;
    }

    public virtual void opAddAssign_1_Vector(Vector v)
    {
        x += v.x;
        y += v.y;
    }

    public virtual void opSubAssign_1_Vector(Vector v)
    {
        x -= v.x;
        y -= v.y;
    }

    public virtual void opMulAssign(float a)
    {
        x *= a;
        y *= a;
    }

    public virtual void opDivAssign(float a)
    {
        x /= a;
        y /= a;
    }

    public virtual float checkSide_2(Vector pos1, Vector pos2)
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

    public virtual float checkSide_3(Vector pos1, Vector pos2, Vector ofs)
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

    public virtual bool checkCross(Vector p, Vector p1, Vector p2, float width)
    {
        float a1x = default(float), a1y = default(float), a2x = default(float), a2y = default(float);
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

        float b1x = default(float), b1y = default(float), b2x = default(float), b2y = default(float);
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

        if (a2y >= b1y && b2y >= a1y)
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

            if (a2x >= b1x && b2x >= a1x)
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
                    if (a1x <= x && x <= a2x && a1y <= y && y <= a2y && b1x <= x && x <= b2x && b1y <= y && y <= b2y)
                        return true;
                }
            }
        }

        return false;
    }

    public virtual bool checkHitDist(Vector p, Vector pp, float dist)
    {
        float bmvx = default(float), bmvy = default(float), inaa = default(float);
        bmvx = pp.x;
        bmvy = pp.y;
        bmvx -= p.x;
        bmvy -= p.y;
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx = default(float), sofsy = default(float), inab = default(float), hd = default(float);
            sofsx = x;
            sofsy = y;
            sofsx -= p.x;
            sofsy -= p.y;
            inab = bmvx * sofsx + bmvy * sofsy;
            if (inab >= 0 && inab <= inaa)
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if (hd >= 0 && hd <= dist)
                    return true;
            }
        }

        return false;
    }

    public virtual float vctSize()
    {
        return sqrt(x * x + y * y);
    }

    public virtual float dist_1_Vector(Vector v)
    {
        return dist_2(v.x, v.y);
    }

    public virtual float dist_2(float px = 0, float py = 0)
    {
        float ax = fabs(x - px);
        float ay = fabs(y - py);
        if (ax > ay)
            return ax + ay / 2;
        else
            return ay + ax / 2;
    }

    public virtual bool contains_2(Vector p, float r = 1)
    {
        return contains_3(p.x, p.y, r);
    }

    public virtual bool contains_3(float px, float py, float r = 1)
    {
        if (px >= -x * r && px <= x * r && py >= -y * r && py <= y * r)
            return true;
        else
            return false;
    }

    public virtual void roll(float d)
    {
        float tx = x * cos(d) - y * sin(d);
        y = x * sin(d) + y * cos(d);
        x = tx;
    }
}

public class Vector3
{
    public static float operator *(Vector3 a, Vector3 b)
    {
        return a.opMul_1_Vector3(b);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        a.opAddAssign_1_Vector3(b);
        return a;
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        a.opSubAssign_1_Vector3(b);
        return a;
    }

    public static Vector3 operator *(Vector3 a, float n)
    {
        a.opMulAssign(n);
        return a;
    }

    public static Vector3 operator /(Vector3 a, float n)
    {
        a.opDivAssign(n);
        return a;
    }

    public float x, y, z;
    public static Vector3 rsl;
    public Vector3(float x = 0, float y = 0, float z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public virtual void clear()
    {
        z = 0;
        y = z;
        x = y;
    }

    public virtual void rollX(float d)
    {
        float ty = y * cos(d) - z * sin(d);
        z = y * sin(d) + z * cos(d);
        y = ty;
    }

    public virtual void rollY(float d)
    {
        float tx = x * cos(d) - z * sin(d);
        z = x * sin(d) + z * cos(d);
        x = tx;
    }

    public virtual void rollZ(float d)
    {
        float tx = x * cos(d) - y * sin(d);
        y = x * sin(d) + y * cos(d);
        x = tx;
    }

    public virtual void blend(Vector3 v1, Vector3 v2, float ratio)
    {
        x = v1.x * ratio + v2.x * (1 - ratio);
        y = v1.y * ratio + v2.y * (1 - ratio);
        z = v1.z * ratio + v2.z * (1 - ratio);
    }

    public virtual float vctSize()
    {
        return sqrt(x * x + y * y + z * z);
    }

    public virtual float dist_1_Vector3(Vector3 v)
    {
        return dist_3(v.x, v.y, v.z);
    }

    public virtual float dist_3(float px = 0, float py = 0, float pz = 0)
    {
        float ax = fabs(x - px);
        float ay = fabs(y - py);
        float az = fabs(z - pz);
        float axy = default(float);
        if (ax > ay)
            axy = ax + ay / 2;
        else
            axy = ay + ax / 2;
        if (axy > az)
            return axy + az / 2;
        else
            return az + axy / 2;
    }

    public virtual Vector3 getElement_1_Vector3(Vector3 v)
    {
        float ll = v * v;
        if (ll != 0)
        {
            float mag = this * v;
            rsl.x = mag * v.x / ll;
            rsl.y = mag * v.y / ll;
            rsl.z = mag * v.z / ll;
        }
        else
        {
            rsl.z = 0;
            rsl.y = rsl.z;
            rsl.x = rsl.y;
        }

        return rsl;
    }

    public virtual float opMul_1_Vector3(Vector3 v)
    {
        return x * v.x + y * v.y + z * v.z;
    }

    public virtual void opAddAssign_1_Vector3(Vector3 v)
    {
        x += v.x;
        y += v.y;
        z += v.z;
    }

    public virtual void opSubAssign_1_Vector3(Vector3 v)
    {
        x -= v.x;
        y -= v.y;
        z -= v.z;
    }

    public virtual void opMulAssign(float a)
    {
        x *= a;
        y *= a;
        z *= a;
    }

    public virtual void opDivAssign(float a)
    {
        x /= a;
        y /= a;
        z /= a;
    }
}
