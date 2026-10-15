// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Crystal : Actor
{
    public bool exists;
    public int poolIndex;
    public bool getExists()
    {
        return exists;
    }

    public void setExists(bool exists)
    {
        this.exists = exists;
    }

    public void setPoolIndex(int poolIndex)
    {
        this.poolIndex = poolIndex;
    }

    public const int COUNT = 60;
    public static int PULLIN_COUNT = GameMath.integer((COUNT * 0.8f));
    public static CrystalShape _shape;
    public Ship ship;
    public Vector pos;
    public Vector vel;
    public int cnt;
    public static void init_0()
    {
        _shape = new CrystalShape();
    }

    public static void close()
    {
        _shape.close();
    }

    public Crystal()
    {
        pos = new Vector();
        vel = new Vector();
    }

    public void init(List<object> args)
    {
        ship = (Ship)args[0];
    }

    public void set(Vector p)
    {
        pos.x = p.x;
        pos.y = p.y;
        cnt = COUNT;
        vel.x = 0;
        vel.y = 0.1f;
        exists = true;
    }

    public void move()
    {
        cnt--;
        float dist = pos.dist_1(ship.midstPos());
        if (dist < 0.1f)
            dist = 0.1f;
        if (cnt < PULLIN_COUNT)
        {
            vel.x = vel.x + ((ship.midstPos().x - pos.x) / dist * 0.07f);
            vel.y = vel.y + ((ship.midstPos().y - pos.y) / dist * 0.07f);
            if ((cnt < 0) || (dist < 2))
            {
                exists = false;
                return;
            }
        }

        vel.opMulAssign(0.95f);
        pos.opAddAssign(vel);
    }

    public void draw(float[] model, Mesh particles = null)
    {
        float r = 0.25f;
        float d = cnt * 0.1f;
        if (cnt > PULLIN_COUNT)
            r = r * (((float)(COUNT - cnt)) / (COUNT - PULLIN_COUNT));
        for (int i = 0; i < 4; i++)
        {
            float[] parent1 = model;
            model = Transform.Translate(model, pos.x + sin(d) * r, pos.y + cos(d) * r, 0);
            _shape.draw(model, null, Gfx.Blend.Alpha);
            model = parent1;
            d = d + (PI / 2);
        }
    }
}

public class CrystalPool
{
    public ActorPool<Crystal> pool;
    public CrystalPool(int n, List<object> args)
    {
        pool = new ActorPool<Crystal>(n, args, () => new Crystal());
    }

    public Crystal getInstance()
    {
        return pool.getInstance();
    }

    public void move()
    {
        pool.move();
    }

    public void draw(float[] model, Mesh particles = null)
    {
        pool.draw(model, particles);
    }

    public void clear()
    {
        pool.clear();
    }
}
