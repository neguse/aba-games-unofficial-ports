// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class FloatLetter : Actor
{
    public static Rand rand = new Rand();
    public Tunnel tunnel;
    public Vector3 pos;
    public float mx, my;
    public float d;
    public float size;
    public string msg;
    public int cnt;
    public float alpha;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1(List<object> args)
    {
        tunnel = (Tunnel)args[0];
        pos = new Vector3();
    }

    public void set_4(string m, Vector p, float s, int c = 120)
    {
        pos.x = p.x;
        pos.y = p.y;
        pos.z = 1;
        mx = rand.nextSignedFloat(0.001f);
        my = -rand.nextFloat(0.2f) + 0.2f;
        d = p.x;
        size = s;
        msg = m;
        cnt = c;
        alpha = 0.8f;
        exists = true;
    }

    public override void move()
    {
        pos.x = pos.x + (mx * pos.y);
        pos.y = pos.y + (my);
        pos.z = pos.z - (0.03f * pos.y);
        cnt--;
        if (cnt < 0)
            exists = false;
        if (alpha >= 0.03f)
            alpha = alpha - (0.03f);
    }

    public override void draw()
    {
        glPushMatrix();
        Vector3 sp = tunnel.getPos_1_Vector3(pos);
        glTranslatef(0, 0, sp.z);
        TtScreen.setColor(1, 1, 1, 1);
        Letter.drawString(msg, sp.x, sp.y, size, LetterDirection.TO_RIGHT, 2, false, d * 180 / PI);
        TtScreen.setColor(1, 1, 1, alpha);
        Letter.drawString(msg, sp.x, sp.y, size, LetterDirection.TO_RIGHT, 3, false, d * 180 / PI);
        glPopMatrix();
    }
}

public class FloatLetterPool : ActorPool<FloatLetter>
{
    public FloatLetterPool(int n, List<object> args) : base(n, args, () => new FloatLetter())
    {
    }
}
