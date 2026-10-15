// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class FloatLetter : Actor
{
    public string meshKey;
    public bool exists;
    public FloatLetter() { meshKey = ActorMesh.nextKey(); }
    public bool getExist() { return exists; }
    public void setExist(bool value) { exists = value; }
    public static Rand rand = new Rand();
    public Tunnel tunnel;
    public Vector3 pos;
    public float mx, my;
    public float d;
    public float size;
    public string msg;
    public float cnt;
    public float alpha;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public void init_1(List<object> args)
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

    public void move()
    {
        pos.x = pos.x + (mx * pos.y * SimulationTime.Step);
        pos.y = pos.y + (my * SimulationTime.Step);
        pos.z = pos.z - (0.03f * pos.y * SimulationTime.Step);
        cnt -= SimulationTime.Step;
        if (cnt < 0)
            exists = false;
        if (alpha >= 0.03f)
            alpha = alpha - (0.03f * SimulationTime.Step);
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        float[] parent1 = model;
        Vector3 sp = tunnel.getPos_1_Vector3(pos);
        model = Transform.Translate(model, 0, 0, sp.z);
        tint = new float[] { 1, 1, 1, 1 };
        Letter.drawString(model, tint, blend, cull, lineWidth, msg, sp.x, sp.y, size, LetterDirection.TO_RIGHT, 2, false, d * 180 / PI);
        tint = new float[] { 1, 1, 1, alpha };
        Letter.drawString(model, tint, blend, cull, lineWidth, msg, sp.x, sp.y, size, LetterDirection.TO_RIGHT, 3, false, d * 180 / PI);
        model = parent1;
    }
}

public class FloatLetterPool
{
    ActorPool<FloatLetter> pool;
    public FloatLetter[] actor;
    public FloatLetterPool(int n, List<object> args)
    {
        pool = new ActorPool<FloatLetter>(n, args, () => new FloatLetter());
        actor = pool.actor;
    }
    public FloatLetter getInstanceForced() { return pool.getInstanceForced(); }
    public void move() { pool.move(); }
    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth) { pool.draw(model, tint, blend, cull, lineWidth); }
    public void clear() { pool.clear(); }
}
