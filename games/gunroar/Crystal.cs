// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Crystal : Actor
{
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

    public override void init(List<object> args)
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

    public override void move()
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

    public override void draw()
    {
        float r = 0.25f;
        float d = cnt * 0.1f;
        if (cnt > PULLIN_COUNT)
            r = r * (((float)(COUNT - cnt)) / (COUNT - PULLIN_COUNT));
        for (int i = 0; i < 4; i++)
        {
            glPushMatrix();
            glTranslatef(pos.x + sin(d) * r, pos.y + cos(d) * r, 0);
            _shape.draw();
            glPopMatrix();
            d = d + (PI / 2);
        }
    }
}

public class CrystalPool : ActorPool<Crystal>
{
    public CrystalPool(int n, List<object> args) : base(n, args, () => new Crystal())
    {
    }
}
