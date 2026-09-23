// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public interface BulletTarget
{
    public Vector getTargetPos();
}

public class VirtualBulletTarget : BulletTarget
{
    public Vector pos;
    public VirtualBulletTarget()
    {
        pos = new Vector();
    }

    public Vector getTargetPos()
    {
        return pos;
    }
}
