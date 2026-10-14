// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class TokenState
{
    static int nextMesh;
    public string meshKey;
    public bool isInitialized = false;
    public Vector pos;
    public float deg;
    public float speed;
    public TokenState()
    {
        meshKey = nextMesh.ToString(); nextMesh++;
        pos = new Vector();
    }

    public virtual void clear()
    {
        {
            pos.y = 0;
            pos.x = pos.y;
        }

        deg = 0;
        speed = 0;
        isInitialized = true;
    }

    public virtual void stepForward()
    {
        pos.x = pos.x - (sin(deg) * speed);
        pos.y = pos.y + (cos(deg) * speed);
    }
}

public class TokenSpec<T>
    where T : TokenState
{
    public Field field;
    public Shape shape;
    public virtual void set_1(T state)
    {
    }

    public virtual void removed(T state)
    {
    }

    public virtual bool move_1(T state)
    {
        return true;
    }

    public virtual void draw_1(float[] model, float[] color, Gfx.Blend blend, T state)
    {
        {
            Vector3 p = field.calcCircularPos_1(state.pos);
            float cd = field.calcCircularDeg(state.pos.x);
            shape.draw_3(model, color, blend, p, cd, state.deg);
        }
    }
}
