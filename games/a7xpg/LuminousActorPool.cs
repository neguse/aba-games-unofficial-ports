// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;

public class LuminousActorPool : ActorPool
{
    public LuminousActorPool(int n, Actor act, ActorInitializer ini) : base(n, act, ini)
    {
    }

    public void drawLuminous()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            if (actor[i].isExist)
                ((LuminousActor)actor[i]).drawLuminous();
        }
    }
}
