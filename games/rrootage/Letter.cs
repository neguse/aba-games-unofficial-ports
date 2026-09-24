// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrInput;
using static RrPreference;
using static RrCore;
using static RrAttract;
using static RrShip;
using static RrLaser;
using static RrShot;
using static RrFrag;
using static RrBackground;
using static RrBoss;
using static RrFoe;
using static RrScreen;
using static RrLetter;
using static RrAngles;
using static RrVector;

public static class RrLetter
{
    static Dictionary<string, Mesh> letters = new Dictionary<string, Mesh>();
    public static float[][][] spData = new float[][][]
    {
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.6f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.6f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.6f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.6f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.1f,
                1.15f,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.45f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.1f,
                0,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.1f,
                1.15f,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.45f,
                0.4f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.25f,
                0,
                0.25f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.75f,
                0.25f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.45f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.1f,
                0,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.3f,
                1.15f,
                0.25f,
                0.3f,
                0
            },
            new float[]
            {
                0.3f,
                1.15f,
                0.25f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.2f,
                -0.6f,
                0.45f,
                0.3f,
                360 - 300
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.1f,
                0,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.45f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.4f,
                1.15f,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                0.4f,
                1.15f,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.5f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.5f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -1.15f,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0.65f,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                -0.3f,
                -1.15f,
                0.25f,
                0.3f,
                0
            },
            new float[]
            {
                0.3f,
                -1.15f,
                0.25f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.4f,
                0.6f,
                0.85f,
                0.3f,
                360 - 120
            },
            new float[]
            {
                0.4f,
                0.6f,
                0.85f,
                0.3f,
                360 - 60
            },
            new float[]
            {
                -0.4f,
                -0.6f,
                0.85f,
                0.3f,
                360 - 240
            },
            new float[]
            {
                0.4f,
                -0.6f,
                0.85f,
                0.3f,
                360 - 300
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.4f,
                0.6f,
                0.85f,
                0.3f,
                360 - 120
            },
            new float[]
            {
                0.4f,
                0.6f,
                0.85f,
                0.3f,
                360 - 60
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0.35f,
                0.5f,
                0.65f,
                0.3f,
                360 - 60
            },
            new float[]
            {
                -0.35f,
                -0.5f,
                0.65f,
                0.3f,
                360 - 240
            },
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                -1.15f,
                0.05f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                -1.15f,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                0,
                0.65f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.4f,
                0,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                0.4f,
                0,
                0.45f,
                0.3f,
                0
            },
            new float[]
            {
                0,
                0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                -0.55f,
                0.65f,
                0.3f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                0,
                1.0f,
                0.4f,
                0.2f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        },
        new float[][]
        {
            new float[]
            {
                -0.19f,
                1.0f,
                0.4f,
                0.2f,
                90
            },
            new float[]
            {
                0.2f,
                1.0f,
                0.4f,
                0.2f,
                90
            },
            new float[]
            {
                0,
                0,
                0,
                0,
                99999
            },
        }
    };
    public static void drawLetter(float[] model, Gfx.Blend blend, string key, int idx, int lx, int ly, int ltSize, int d, int r, int g, int b)
    {
        string geometryKey = idx.ToString() + "-" + ltSize.ToString() + "-" + d.ToString();
        if (!letters.ContainsKey(geometryKey)) letters[geometryKey] = createLetter("letter-" + geometryKey, idx, ltSize, d);
        Mesh mesh = letters[geometryKey];
        model = Transform.Translate(model, lx, ly, 0);
        float[] tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, 1 };
        if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawString(float[] model, Gfx.Blend blend, string key, string str, int lx, int ly, int ltSize, int d, int r, int g, int b)
    {
        int x = lx, y = ly;
        int i = 0, idx = 0;
        {
            i = 0;
            for (;; i++)
            {
                if (i >= str.Length)
                    break;
                string letter = str.Substring(i, 1);
                if (letter != " ")
                {
                    idx = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(letter.ToUpper());
                    if (idx < 0)
                    {
                        if (letter == ".")
                            idx = 36;
                        else if (letter == "-")
                            idx = 38;
                        else if (letter == "+")
                            idx = 39;
                        else
                            idx = 37;
                    }

                    drawLetter(model, blend, key + "-805" + "-" + i.ToString(), idx, x, y, ltSize, d, r, g, b);
                }

                switch (d)
                {
                    case 0:
                        x = GameMath.integer(x + (ltSize * 1.7f));
                        break;
                    case 1:
                        y = GameMath.integer(y + (ltSize * 1.7f));
                        break;
                    case 2:
                        x = GameMath.integer(x - (ltSize * 1.7f));
                        break;
                    case 3:
                        y = GameMath.integer(y - (ltSize * 1.7f));
                        break;
                }
            }
        }
    }

    static Mesh createLetter(string key, int idx, int ltSize, int d)
    {
        var mesh = new Mesh(key);
        int i = 0;
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        int deg = 0;
        {
            i = 0;
            for (;; i++)
            {
                deg = GameMath.integer(spData[(idx)][(i)][(4)]);
                if (deg > 99990)
                    break;
                x = -spData[(idx)][(i)][(0)];
                y = -spData[(idx)][(i)][(1)];
                size = spData[(idx)][(i)][(2)];
                length = spData[(idx)][(i)][(3)];
                size = size * (0.66f);
                length = length * (0.6f);
                switch (d)
                {
                    case 0:
                        x = -x;
                        y = y;
                        break;
                    case 1:
                        t = x;
                        x = -y;
                        y = -t;
                        deg = deg + (90);
                        break;
                    case 2:
                        x = x;
                        y = -y;
                        deg = deg + (180);
                        break;
                    case 3:
                        t = x;
                        x = y;
                        y = t;
                        deg = deg + (270);
                        break;
                }

                deg = deg % (180);
                if ((deg <= 45) || (deg > 135))
                {
                    appendBox(mesh, GameMath.integer((x * ltSize)), GameMath.integer((y * ltSize)), GameMath.integer((size * ltSize)), GameMath.integer((length * ltSize)));
                }
                else
                {
                    appendBox(mesh, GameMath.integer((x * ltSize)), GameMath.integer((y * ltSize)), GameMath.integer((length * ltSize)), GameMath.integer((size * ltSize)));
                }
            }
        }

        return mesh;
    }

    static void appendBox(Mesh mesh, float x, float y, float width, float height)
    {
        int first = mesh.vertexCount;
        float[] color = new float[] { 1, 1, 1, 128f / 255 };
        mesh.Vertex(x - width, y - height, 0, color, null, true);
        mesh.Vertex(x + width, y - height, 0, color, null, true);
        mesh.Vertex(x + width, y + height, 0, color, null, true);
        mesh.Vertex(x - width, y + height, 0, color, null, true);
        mesh.Fan(first, 4);
        first = mesh.vertexCount;
        mesh.Vertex(x - width, y - height, 0, null);
        mesh.Vertex(x + width, y - height, 0, null);
        mesh.Vertex(x + width, y + height, 0, null);
        mesh.Vertex(x - width, y + height, 0, null);
        mesh.LineStrip(first, 4, true);
    }
}
