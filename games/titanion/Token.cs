// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public abstract class Token<ST, SP> : Actor where ST : TokenState where SP : TokenSpec<ST>
{
    public ST state;
    public SP spec;
    public abstract override void init_1(List<object> args);
    public virtual void setAt(SP spec, Vector pos, float deg, float speed)
    {
        set_5(spec, pos.x, pos.y, deg, speed);
    }

    public virtual void set_5(SP spec, float x, float y, float deg, float speed)
    {
        this.spec = spec;
        set_4(x, y, deg, speed);
    }

    public virtual void set_4(float x, float y, float deg, float speed)
    {
        state.clear();
        state.pos.x = x;
        state.pos.y = y;
        state.deg = deg;
        state.speed = speed;
        spec.set_1(state);
        exists = true;
    }

    public override void move_0()
    {
        if (!((spec.move_1(state))))
            remove();
    }

    public virtual void remove()
    {
        exists = false;
        spec.removed(state);
    }

    public override void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        spec.draw_1(model, color, blend, state);
    }

    public virtual Vector pos()
    {
        return state.pos;
    }
}

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
