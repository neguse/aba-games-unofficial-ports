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

public static class NrFrag
{
    public static Frag[] frag = Make(FRAG_MAX, () => new Frag());
    public static void initFrags()
    {
        int i = 0;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                frag[(i)].cnt = 0;
            }
        }
    }

    public static int fragIdx = FRAG_MAX;
    public static void addFrag(Vector pos, Vector vel, int spc, int size)
    {
        int i = 0;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                fragIdx--;
                if (fragIdx < 0)
                    fragIdx = FRAG_MAX - 1;
                if (frag[(i)].cnt <= 0)
                    break;
            }
        }

        if (i >= FRAG_MAX)
            return;
        frag[(i)].pos = new Vector
        {
            x = (pos).x,
            y = (pos).y
        };
        frag[(i)].vel = new Vector
        {
            x = (vel).x,
            y = (vel).y
        };
        switch (spc)
        {
            case 0:
                frag[(i)].width = 5 + randN(10);
                frag[(i)].height = 5 + randN(10);
                frag[(i)].cnt = 4 + randN(8);
                break;
            case 1:
                frag[(i)].width = size * 5 + randN(size * 3);
                frag[(i)].height = size * 5 + randN(size * 3);
                frag[(i)].cnt = 12 + randN(12);
                break;
            case 2:
                frag[(i)].width = 4;
                frag[(i)].height = 4;
                frag[(i)].cnt = 10 + randN(4);
                break;
        }

        frag[(i)].spc = spc;
    }

    public static void addShotFrag(Vector p)
    {
        Vector pos = new Vector(), vel = new Vector();
        pos.x = GameMath.signedShift((p.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        pos.y = GameMath.signedShift((p.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        vel.x = randNS(GameMath.signedShift(SHOT_SPEED, 11)) * LAYER_WIDTH / SCAN_WIDTH;
        vel.y = (-(GameMath.signedShift(SHOT_SPEED, 8)) + randNS(GameMath.signedShift(SHOT_SPEED, 11))) * LAYER_HEIGHT / SCAN_HEIGHT;
        addFrag(pos, vel, 0, 0);
    }

    public static void addEnemyFrag(Vector p, int mx, int my, int type)
    {
        Vector pos = new Vector(), vel = new Vector();
        int cmx = 0, cmy = 0;
        int i = 0;
        pos.x = GameMath.signedShift((p.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        pos.y = GameMath.signedShift((p.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        cmx = GameMath.signedShift((mx / SCAN_WIDTH * LAYER_WIDTH), 8);
        cmy = GameMath.signedShift((my / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        type = type * 2 + 1;
        {
            i = 0;
            for (; i < type + randN(type * 2); i++)
            {
                vel.x = randNS(16);
                vel.y = randNS(16);
                addFrag(pos, vel, 0, 0);
            }
        }

        {
            i = 0;
            for (; i < type * 2 + randN(type); i++)
            {
                vel.x = cmx + randNS(3);
                vel.y = cmy + randNS(3);
                addFrag(pos, vel, 1, 2 + type);
            }
        }
    }

    public static void addShipFrag(Vector p)
    {
        Vector pos = new Vector(), vel = new Vector();
        int cmx = 0, cmy = 0;
        int i = 0;
        pos.x = GameMath.signedShift((p.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        pos.y = GameMath.signedShift((p.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        {
            i = 0;
            for (; i < 48; i++)
            {
                vel.x = randNS(24);
                vel.y = randNS(24);
                addFrag(pos, vel, 0, 0);
            }
        }

        {
            i = 0;
            for (; i < 32; i++)
            {
                vel.x = randNS(4);
                vel.y = randNS(4);
                addFrag(pos, vel, 1, 1 + randN(6));
            }
        }
    }

    public static void addClearFrag(Vector p, Vector v)
    {
        Vector pos = new Vector(), vel = new Vector();
        pos.x = GameMath.signedShift((p.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        pos.y = GameMath.signedShift((p.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        vel.x = GameMath.signedShift((v.x / SCAN_WIDTH * LAYER_WIDTH), 8);
        vel.y = GameMath.signedShift((v.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
        addFrag(pos, vel, 2, 0);
    }

    public static void moveFrags()
    {
        int i = 0;
        Frag fr = null;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                if (frag[(i)].cnt <= 0)
                    continue;
                fr = (frag[(i)]);
                fr.pos.x = fr.pos.x + (fr.vel.x);
                fr.pos.y = fr.pos.y + (fr.vel.y);
                fr.cnt--;
            }
        }
    }

    public static int[][][] fragColor = new int[][][]
    {
        new int[][]
        {
            new int[]
            {
                16 * 8 - 7,
                16 * 2 - 2
            },
            new int[]
            {
                16 * 2 - 7,
                16 * 8 - 2
            }
        },
        new int[][]
        {
            new int[]
            {
                16 * 5 - 7,
                16 * 2 - 2
            },
            new int[]
            {
                16 * 2 - 7,
                16 * 5 - 2
            }
        },
        new int[][]
        {
            new int[]
            {
                16 * 1 - 10,
                16 * 1 - 5
            },
            new int[]
            {
                16 * 1 - 5,
                16 * 1 - 10
            }
        },
    };
    public static void drawFrags()
    {
        int x = 0, y = 0, c = 0;
        int i = 0;
        Frag fr = null;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                if (frag[(i)].cnt <= 0)
                    continue;
                fr = (frag[(i)]);
                c = fr.cnt & 1;
                drawBox(fr.pos.x, fr.pos.y, fr.width, fr.height, fragColor[(fr.spc)][(c)][(0)], fragColor[(fr.spc)][(c)][(1)], l2buf);
            }
        }
    }
}
