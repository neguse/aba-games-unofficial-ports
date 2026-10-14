// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using Lub;

public static class TokenState
{
    static int nextMesh;
    public static string nextMeshKey()
    {
        string meshKey = nextMesh.ToString(); nextMesh++;
        return meshKey;
    }

    public static void stepForward(Vector pos, float deg, float speed)
    {
        pos.x = pos.x - (sin(deg) * speed);
        pos.y = pos.y + (cos(deg) * speed);
    }
}
