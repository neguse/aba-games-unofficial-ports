// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class Bonus : Actor
{
    public Vector pos;
    public int cnt;
    public float size;
    public float my;
    public int num;
    public override Actor newActor()
    {
        return new Bonus();
    }

    public override void init(ActorInitializer ini)
    {
        pos = new Vector();
    }

    public void set(int n, Vector p, float s)
    {
        num = n;
        int tn = n, dig = 0;
        for (dig = 0; tn > 0; dig++)
            tn = GameMath.integer(tn / (10));
        pos.x = p.x - s / 2 * tn;
        pos.y = p.y;
        size = s;
        cnt = GameMath.integer(32 + s * 24);
        my = 0.03f + s * 0.2f;
        isExist = true;
    }

    public override void move()
    {
        cnt--;
        if (cnt <= 0)
        {
            isExist = false;
            return;
        }

        pos.y = pos.y + (my);
        my = my * (0.95f);
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        LetterRender.drawNumReverse(model, tint, blend, num, pos.x, pos.y, size);
    }
}

public class BonusInitializer : ActorInitializer
{
}
