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

public static class RrShot
{
    public static Shot[] shot = Make(SHOT_MAX, () => new Shot());
    public static void initShots()
    {
        int i = 0;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                shot[(i)].cnt = -1;
            }
        }
    }

    public static int shotIdx = SHOT_MAX;
    public static void addShot(int x, int y, int ox, int oy, int color)
    {
        int i = 0, d = 0, ds = 0;
        Shot st = null;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                shotIdx--;
                if (shotIdx < 0)
                    shotIdx = SHOT_MAX - 1;
                if (shot[(shotIdx)].cnt < 0)
                    break;
            }
        }

        if (i >= SHOT_MAX)
            return;
        st = (shot[(shotIdx)]);
        st.x = (float)x / FIELD_SCREEN_RATIO;
        st.y = -(float)y / FIELD_SCREEN_RATIO;
        d = getDeg(-ox, oy);
        ds = getDistance(ox, oy);
        st.mx = -(float)sctbl[(d)] * SHOT_SPEED / (FIELD_SCREEN_RATIO * 256);
        st.my = (float)sctbl[(d + 256)] * SHOT_SPEED / (FIELD_SCREEN_RATIO * 256);
        st.d = (float)d * 360 / 1024;
        st.color = color;
        st.cnt = GameMath.integer(ds / SHOT_SPEED);
        st.width = 0.07f;
        st.height = 0.1f;
    }

    public static void moveShots()
    {
        int i = 0;
        Shot st = null;
        if ((mode != IKA_MODE) && (mode != GW_MODE))
            return;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                if (shot[(i)].cnt < 0)
                    continue;
                st = (shot[(i)]);
                st.x = st.x + (st.mx / 2);
                st.y = st.y + (st.my / 2);
                st.height = st.height + (SHOT_HEIHGT_SPEED / 2);
                st.cnt--;
                if (st.cnt < 0)
                {
                    switch (mode)
                    {
                        case IKA_MODE:
                            damageBoss(64);
                            break;
                        case GW_MODE:
                            damageBoss(30);
                            break;
                    }
                }
            }
        }
    }

    public static void drawShots()
    {
        int i = 0;
        Shot st = null;
        if ((mode != IKA_MODE) && (mode != GW_MODE))
            return;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                if (shot[(i)].cnt < 0)
                    continue;
                st = (shot[(i)]);
                drawShot(st.x, st.y, st.d, st.color, st.width, st.height);
            }
        }
    }
}
