// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class Vector
{
    public float x, y;
    public Vector(float x = 0, float y = 0)
    {
        this.x = x;
        this.y = y;
    }



    public virtual Vector getElement(Vector v, float min = 0, float max = 1000000)
    {
        Vector result = new Vector();
        float lengthSquared = v.x * v.x + v.y * v.y;
        if (lengthSquared > 0.1f)
        {
            float magnitude = (x * v.x + y * v.y) / lengthSquared;
            result.x = magnitude * x;
            result.y = magnitude * y;
        }

        float size = result.vctSize();
        if ((size > 0.1f) && (size < min))
            result.opMulAssign(min / size);
        else if (size > max)
            result.opMulAssign(max / size);
        return result;
    }

    public virtual void opAddAssign(Vector v)
    {
        x = x + (v.x);
        y = y + (v.y);
    }



    public virtual void opMulAssign(float a)
    {
        x = x * (a);
        y = y * (a);
    }

    public virtual void opDivAssign(float a)
    {
        x = x / (a);
        y = y / (a);
    }









    public virtual float vctSize()
    {
        return sqrt(x * x + y * y);
    }

    public virtual float dist_1(Vector v)
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
        if ((((((((((px >= -x * r))) && (((px <= x * r)))))) && (((py >= -y * r)))))) && (((py <= y * r))))
            return true;
        else
            return false;
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





    public virtual void opAddAssign(Vector3 v)
    {
        x = x + (v.x);
        y = y + (v.y);
        z = z + (v.z);
    }



    public virtual void opMulAssign(float a)
    {
        x = x * (a);
        y = y * (a);
        z = z * (a);
    }

    public virtual void opDivAssign(float a)
    {
        x = x / (a);
        y = y / (a);
        z = z / (a);
    }
}
