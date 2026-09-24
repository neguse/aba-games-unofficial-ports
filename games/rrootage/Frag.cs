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

public static class RrFrag
{
    public static Frag[] frag = Make(FRAG_MAX, () => new Frag());
    public static int fragIdx;
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

        fragIdx = FRAG_MAX;
    }

    public static int[][] fragColors = new int[][]
    {
        new int[]
        {
            100,
            255,
            100
        },
        new int[]
        {
            240,
            240,
            240
        },
        new int[]
        {
            240,
            240,
            120
        },
        new int[]
        {
            255,
            255,
            255
        },
        new int[]
        {
            128,
            128,
            128
        },
        new int[]
        {
            255,
            255,
            255
        },
        new int[]
        {
            100,
            100,
            255
        },
        new int[]
        {
            220,
            220,
            200
        },
    };
    public static void addLineFrag(int x, int y, int z, int mx, int my, int mz, int width, int d1, int cp, int cnt)
    {
        int i = 0;
        Frag fr = null;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                fragIdx--;
                if (fragIdx < 0)
                    fragIdx = FRAG_MAX - 1;
                if (frag[(fragIdx)].cnt <= 0)
                    break;
            }
        }

        if (i >= FRAG_MAX)
            return;
        fr = (frag[(fragIdx)]);
        fr.x = x / FIELD_SCREEN_RATIO;
        fr.y = y / FIELD_SCREEN_RATIO;
        fr.z = z / FIELD_SCREEN_RATIO;
        fr.mx = mx / FIELD_SCREEN_RATIO;
        fr.my = my / FIELD_SCREEN_RATIO;
        fr.mz = mz / FIELD_SCREEN_RATIO;
        fr.width = width / FIELD_SCREEN_RATIO;
        fr.d1 = d1;
        fr.d2 = 0;
        fr.md1 = randNS(32);
        fr.md2 = randNS(32);
        {
            i = 0;
            for (; i < FRAG_COLOR_NUM; i++)
            {
                fr.r[(i)] = fragColors[(cp + i)][(0)];
                fr.g[(i)] = fragColors[(cp + i)][(1)];
                fr.b[(i)] = fragColors[(cp + i)][(2)];
            }
        }

        fr.cnt = cnt;
    }

    public static void addLineFragFloat(float x, float y, float z, int mx, int my, int mz, float width, int d1, int cp, int cnt)
    {
        int i = 0;
        Frag fr = null;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                fragIdx--;
                if (fragIdx < 0)
                    fragIdx = FRAG_MAX - 1;
                if (frag[(fragIdx)].cnt <= 0)
                    break;
            }
        }

        if (i >= FRAG_MAX)
            return;
        fr = (frag[(fragIdx)]);
        fr.x = x;
        fr.y = y;
        fr.z = z;
        fr.mx = mx / FIELD_SCREEN_RATIO;
        fr.my = my / FIELD_SCREEN_RATIO;
        fr.mz = mz / FIELD_SCREEN_RATIO;
        fr.width = width;
        fr.d1 = d1;
        fr.d2 = 0;
        fr.md1 = randNS(5);
        fr.md2 = randNS(5);
        {
            i = 0;
            for (; i < FRAG_COLOR_NUM; i++)
            {
                fr.r[(i)] = fragColors[(cp + i)][(0)];
                fr.g[(i)] = fragColors[(cp + i)][(1)];
                fr.b[(i)] = fragColors[(cp + i)][(2)];
            }
        }

        fr.cnt = cnt;
    }

    public static void addLaserFrag(int x, int y, int width)
    {
        int wd = GameMath.integer(width / 4);
        int i = 0;
        int lx = x - GameMath.integer(width / 4) * 3;
        {
            i = 0;
            for (; i < 4; i++, lx = lx + (GameMath.integer(width / 2)))
            {
                addLineFrag(lx, y, 0, randNS(512), 512 + randN(512), randNS(256), wd, 256, 0, 10 + randN(10));
                addLineFrag(lx, y, 0, randNS(512), 512 + randN(512), randNS(256), wd, 0, 0, 10 + randN(10));
            }
        }
    }

    public static void addBossFrag(float x, float y, float z, float width, int d)
    {
        addLineFragFloat(x, y, z, randNS(1024), randNS(1024), randNS(1024), width, d, 2, 52 + randN(20));
    }

    public static void addShipFrag(float x, float y)
    {
        int i = 0, d = 0, s = 0;
        {
            i = 0;
            for (; i < 80; i++)
            {
                d = randN(1024);
                s = randN(128) + 128;
                addLineFragFloat(x, y, 0, GameMath.signedShift((sctbl[(d)] * s), 6), GameMath.signedShift((sctbl[(d + 256)] * s), 6), randNS(1024), 0.5f, -d & 1023, 0, 24 + randN(12));
            }
        }
    }

    public static void addGrazeFrag(int x, int y, int mx, int my)
    {
        int d = getDeg(mx, -my);
        addLineFrag(x, -y, 0, (GameMath.signedShift(mx, 2)), (GameMath.signedShift(-my, 2)), randNS(1024), 5000, d, 6, 16);
        addLineFrag(x, -y, 0, (GameMath.signedShift(mx, 2)), (GameMath.signedShift(-my, 2)), randNS(1024), 2500, d, 6, 10);
    }

    public static void addLineFragOfs(float x, float y, float ox1, float oy1, float ox2, float oy2, int d, int mx, int my)
    {
        float cx = 0, cy = 0, ox = 0, oy = 0, lw = 0;
        int ld = 0;
        ox = (ox1 + ox2) / 2;
        oy = (oy1 + oy2) / 2;
        d = -d;
        d = d & (1023);
        cx = (ox * sctbl[(d + 256)] - oy * sctbl[(d)]) / 256.0f + x;
        cy = (ox * sctbl[(d)] + oy * sctbl[(d + 256)]) / 256.0f + y;
        ld = (getDeg(GameMath.integer(((ox1 - ox2) * 256)), GameMath.integer(((oy1 - oy2) * 256))) + d) & 1023;
        lw = getDistanceFloat(ox2 - ox1, oy2 - oy1) / 2;
        addLineFragFloat(cx, cy, 0, mx, -my, -randN(1024) - 1024, lw, ld, 4, 24 + randN(16));
    }

    public static void addShapeFrag(float x, float y, float size, int d, int cnt, int type, int mx, int my)
    {
        int sd = 0;
        float sz = 0, sz2 = 0;
        switch (type)
        {
            case -1:
                sz = size / 2;
                addLineFragOfs(x, y, 0, -sz, 0, sz, d, mx, my);
                break;
            case 0:
                sz = size / 2;
                addLineFragOfs(x, y, -sz, -sz, sz, -sz, d, mx, my);
                addLineFragOfs(x, y, sz, -sz, 0, size, d, mx, my);
                addLineFragOfs(x, y, 0, size, -sz, -sz, d, mx, my);
                break;
            case 1:
                sz = size / 2;
                sd = (cnt * 23) & 1023;
                addLineFragOfs(x, y, 0, -size, sz, 0, sd, mx, my);
                addLineFragOfs(x, y, sz, 0, 0, size, sd, mx, my);
                addLineFragOfs(x, y, 0, size, -sz, 0, sd, mx, my);
                addLineFragOfs(x, y, -sz, 0, 0, -size, sd, mx, my);
                break;
            case 2:
                sz = size / 4;
                sz2 = size / 3 * 2;
                addLineFragOfs(x, y, -sz, -sz2, sz, -sz2, d, mx, my);
                addLineFragOfs(x, y, sz, -sz2, sz, sz2, d, mx, my);
                addLineFragOfs(x, y, sz, sz2, -sz, sz2, d, mx, my);
                addLineFragOfs(x, y, -sz, sz2, -sz, -sz2, d, mx, my);
                break;
            case 3:
                sz = size / 2;
                sd = (cnt * 37) & 1023;
                addLineFragOfs(x, y, -sz, -sz, sz, -sz, sd, mx, my);
                addLineFragOfs(x, y, sz, -sz, sz, sz, sd, mx, my);
                addLineFragOfs(x, y, sz, sz, -sz, sz, sd, mx, my);
                addLineFragOfs(x, y, -sz, sz, -sz, -sz, sd, mx, my);
                break;
            case 4:
                sz = size / 2;
                sd = (cnt * 53) & 1023;
                addLineFragOfs(x, y, -sz / 2, -sz, sz / 2, -sz, sd, mx, my);
                addLineFragOfs(x, y, sz / 2, -sz, sz, -sz / 2, sd, mx, my);
                addLineFragOfs(x, y, sz, -sz / 2, sz, sz / 2, sd, mx, my);
                addLineFragOfs(x, y, sz, sz / 2, -sz / 2, sz, sd, mx, my);
                addLineFragOfs(x, y, -sz / 2, sz, -sz, sz / 2, sd, mx, my);
                addLineFragOfs(x, y, -sz, sz / 2, -sz, -sz / 2, sd, mx, my);
                break;
            case 5:
                sz = size * 2 / 3;
                sz2 = size / 5;
                addLineFragOfs(x, y, -sz, -sz + sz2, sz, -sz + sz2, d, mx, my);
                addLineFragOfs(x, y, sz, -sz + sz2, 0, sz + sz2, d, mx, my);
                addLineFragOfs(x, y, 0, sz + sz2, -sz, -sz + sz2, d, mx, my);
                break;
            case 6:
                sz = size / 2;
                sd = (cnt * 13) & 1023;
                addLineFragOfs(x, y, -sz, -sz, 0, -sz, sd, mx, my);
                addLineFragOfs(x, y, 0, -sz, sz, 0, sd, mx, my);
                addLineFragOfs(x, y, sz, 0, sz, sz, sd, mx, my);
                addLineFragOfs(x, y, sz, sz, 0, sz, sd, mx, my);
                addLineFragOfs(x, y, 0, sz, -sz, 0, sd, mx, my);
                addLineFragOfs(x, y, -sz, 0, -sz, -sz, sd, mx, my);
                break;
        }
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
                fr.x = fr.x + (fr.mx);
                fr.y = fr.y + (fr.my);
                fr.z = fr.z + (fr.mz);
                fr.d1 = fr.d1 + (fr.md1);
                fr.d2 = fr.d2 + (fr.md2);
                fr.d1 = fr.d1 & (1023);
                fr.d2 = fr.d2 & (1023);
                fr.cnt--;
            }
        }
    }

    public static void drawFrags(float[] model, Gfx.Blend blend, string key)
    {
        int c = 0;
        int i = 0;
        Frag fr = null;
        {
            i = 0;
            for (; i < FRAG_MAX; i++)
            {
                if (frag[(i)].cnt <= 0)
                    continue;
                fr = (frag[(i)]);
                c = fr.cnt & (FRAG_COLOR_NUM - 1);
                drawRollLine(model, blend, key + "-315" + "-" + i.ToString(), fr.x, fr.y, fr.z, fr.width, fr.r[(c)], fr.g[(c)], fr.b[(c)], 255, fr.d1, fr.d2);
            }
        }
    }
}
