// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class LuminousActorPool
{
    ActorPool pool;
    public Actor[] actor;
    public LuminousActorPool(int n, Actor act, ActorInitializer ini)
    {
        pool = new ActorPool(n, act, ini);
        actor = pool.actor;
    }
    public Actor getInstance() { return pool.getInstance(); }
    public Actor getInstanceForced() { return pool.getInstanceForced(); }
    public void move() { pool.move(); }
    public void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) { pool.draw(model, color, blend, target); }
    public void clear() { pool.clear(); }

    public void drawLuminous(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        for (int i = 0; i < actor.Length; i++)
        {
            if (actor[i].getExist())
                ((LuminousActor)actor[i]).drawLuminous(model, tint, blend, target);
        }
    }
}
