// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using Lub;

public class Bonus : Actor
{
    public string meshKey;
    public bool isExist;
    public Bonus() { meshKey = ActorMesh.nextKey(); }
    public bool getExist() { return isExist; }
    public void setExist(bool value) { isExist = value; }
    public Vector pos;
    public int cnt;
    public float size;
    public float my;
    public int num;
    public Actor newActor()
    {
        return new Bonus();
    }

    public void init(ActorInitializer ini)
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

    public void move()
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

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        LetterRender.drawNumReverse(model, tint, blend, num, pos.x, pos.y, size);
    }
}

public class BonusInitializer : ActorInitializer
{
}
