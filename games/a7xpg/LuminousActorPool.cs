// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class LuminousActorPool : ActorPool
{
    public LuminousActorPool(int n, Actor act, ActorInitializer ini) : base(n, act, ini)
    {
    }

    public void drawLuminous(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        for (int i = 0; i < actor.Length; i++)
        {
            if (actor[i].isExist)
                ((LuminousActor)actor[i]).drawLuminous(model, tint, blend, target);
        }
    }
}
