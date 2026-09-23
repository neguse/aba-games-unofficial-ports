// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrGl;
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
    public static void drawLetter(int idx, int lx, int ly, int ltSize, int d, int r, int g, int b)
    {
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
                    drawBox(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((size * ltSize)), GameMath.integer((length * ltSize)), r, g, b);
                }
                else
                {
                    drawBox(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((length * ltSize)), GameMath.integer((size * ltSize)), r, g, b);
                }
            }
        }
    }

    public static void drawString(string str, int lx, int ly, int ltSize, int d, int r, int g, int b)
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

                    drawLetter(idx, x, y, ltSize, d, r, g, b);
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
}
