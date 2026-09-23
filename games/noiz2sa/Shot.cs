// Copyright 2002 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static NrCore;
using static NrAttract;
using static NrShip;
using static NrShot;
using static NrFrag;
using static NrBonus;
using static NrBackground;
using static NrFoe;
using static NrBarrage;
using static NrLetter;
using static NrConstants;
using static NrArrays;
using static NrRandom;
using static NrScreen;
using static NrSound;
using static NrPreference;
using static NrAngles;
using static NrVector;

public static class NrShot
{
    public static Shot[] shot = Make(SHOT_MAX, () => new Shot());
    public static void initShots()
    {
        int i = 0;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                shot[(i)].cnt = NOT_EXIST;
            }
        }
    }

    public static int shotIdx = SHOT_MAX;
    public static void addShot(Vector pos)
    {
        int i = 0;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                shotIdx--;
                if (shotIdx < 0)
                    shotIdx = SHOT_MAX - 1;
                if (shot[(i)].cnt == NOT_EXIST)
                    break;
            }
        }

        if (i >= SHOT_MAX)
            return;
        shot[(i)].pos = new Vector
        {
            x = (pos).x,
            y = (pos).y
        };
        shot[(i)].cnt = 0;
        playChunk(0);
    }

    public static void moveShots()
    {
        int i = 0;
        Shot st = null;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                if (shot[(i)].cnt == NOT_EXIST)
                    continue;
                st = (shot[(i)]);
                st.pos.y = st.pos.y - (SHOT_SPEED);
                st.cnt++;
                if (st.pos.y < 0)
                {
                    st.cnt = NOT_EXIST;
                    continue;
                }
            }
        }
    }

    public static void drawShots()
    {
        int x = 0, y = 0, d = 0;
        int i = 0;
        Shot st = null;
        {
            i = 0;
            for (; i < SHOT_MAX; i++)
            {
                if (shot[(i)].cnt == NOT_EXIST)
                    continue;
                st = (shot[(i)]);
                x = GameMath.signedShift((st.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                y = GameMath.signedShift((st.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                d = (st.cnt * 16) & (DIV / 8 - 1);
                drawBox(x + (GameMath.signedShift((sctbl[(d)] * SHOT_WIDTH), 8)), y, SHOT_WIDTH, SHOT_HEIGHT, 16 * 7 - 8, 16 * 7 - 1, buf);
                drawBox(x - (GameMath.signedShift((sctbl[(d)] * SHOT_WIDTH), 8)), y, SHOT_WIDTH, SHOT_HEIGHT, 16 * 7 - 8, 16 * 7 - 1, buf);
            }
        }
    }
}
