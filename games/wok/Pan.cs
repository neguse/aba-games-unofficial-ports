// Copyright 2001 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static WkCore;
using static WkAttract;
using static WkBall;
using static WkBoard;
using static WkGenerator;
using static WkPan;
using static WkVector;
using static WkConstants;
using static WkArrays;
using static WkRandom;
using static WkScreen;
using static WkSound;

public static class WkPan
{
    public static Pan pan = new Pan();
    public static PanPos now1 = new PanPos(), now2 = new PanPos(), now3 = new PanPos();
    public static PanPos prv1 = new PanPos(), prv2 = new PanPos(), prv3 = new PanPos();
    public static void initPan()
    {
        {
            pan.pos.x = 80;
            pan.prvPos.x = pan.pos.x;
        }

        {
            pan.pos.y = 60;
            pan.prvPos.y = pan.pos.y;
        }

        pan.deg = 0;
    }

    public static void movePan()
    {
        int mx = 0, my = 0;
        float ps1 = 0, pc1 = 0, ps2 = 0, pc2 = 0, ps3 = 0, pc3 = 0, ps4 = 0, pc4 = 0;
        pan.prvPos.x = pan.pos.x;
        pan.prvPos.y = pan.pos.y;
        mx=WkScreen.mx;my=WkScreen.my;
        pan.pos.x = mx;
        pan.pos.y = my;
        pan.vel = new Vector
        {
            x = (pan.prvPos).x,
            y = (pan.prvPos).y
        };
        vctSub((pan.vel), (pan.pos));
        pan.deg = pan.deg - (pan.vel.x * 0.005f);
        pan.deg = pan.deg * (0.92f);
        prv1 = now1.Copy();
        prv2 = now2.Copy();
        prv3 = now3.Copy();
        ps1 = sin(pan.deg + PI * 0.25f);
        pc1 = cos(pan.deg + PI * 0.25f);
        ps2 = sin(pan.deg + PI * 0.75f);
        pc2 = cos(pan.deg + PI * 0.75f);
        ps3 = sin(pan.deg + PI * 1.25f);
        pc3 = cos(pan.deg + PI * 1.25f);
        ps4 = sin(pan.deg + PI * 1.75f);
        pc4 = cos(pan.deg + PI * 1.75f);
        {
            now1.p3.x = pan.pos.x + cos(pan.deg + PI) * PAN_WIDTH;
            now1.p1.x = now1.p3.x;
            now1.pc1.x = now1.p1.x;
        }

        {
            now1.p3.y = pan.pos.y + sin(pan.deg + PI) * PAN_WIDTH;
            now1.p1.y = now1.p3.y;
            now1.pc1.y = now1.p1.y;
        }

        {
            now1.p4.x = pan.pos.x + cos(pan.deg) * PAN_WIDTH;
            now1.p2.x = now1.p4.x;
            now1.pc2.x = now1.p2.x;
        }

        {
            now1.p4.y = pan.pos.y + sin(pan.deg) * PAN_WIDTH;
            now1.p2.y = now1.p4.y;
            now1.pc2.y = now1.p2.y;
        }

        now1.p1.x = now1.p1.x + (pc3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p1.y = now1.p1.y + (ps3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p2.x = now1.p2.x + (pc4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p2.y = now1.p2.y + (ps4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p3.x = now1.p3.x + (pc2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p3.y = now1.p3.y + (ps2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p4.x = now1.p4.x + (pc1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now1.p4.y = now1.p4.y + (ps1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        prv1.v1 = new Vector
        {
            x = (prv1.p2).x,
            y = (prv1.p2).y
        };
        vctSub((prv1.v1), (prv1.p1));
        prv1.v2 = new Vector
        {
            x = (prv1.p1).x,
            y = (prv1.p1).y
        };
        vctSub((prv1.v2), (prv1.p3));
        now1.v1 = new Vector
        {
            x = (now1.p2).x,
            y = (now1.p2).y
        };
        vctSub((now1.v1), (now1.p1));
        now1.v2 = new Vector
        {
            x = (now1.p1).x,
            y = (now1.p1).y
        };
        vctSub((now1.v2), (now1.p3));
        now1.v1l = vctSize((now1.v1));
        now1.v2l = vctSize((now1.v2));
        {
            now2.p4.x = now1.p1.x;
            now2.p2.x = now2.p4.x;
            now2.pc1.x = now2.p2.x;
        }

        {
            now2.p4.y = now1.p1.y;
            now2.p2.y = now2.p4.y;
            now2.pc1.y = now2.p2.y;
        }

        {
            now2.p3.x = now2.p2.x + cos(pan.deg + PI * 1.5f) * PAN_HEIGHT;
            now2.p1.x = now2.p3.x;
            now2.pc2.x = now2.p1.x;
        }

        {
            now2.p3.y = now2.p2.y + sin(pan.deg + PI * 1.5f) * PAN_HEIGHT;
            now2.p1.y = now2.p3.y;
            now2.pc2.y = now2.p1.y;
        }

        now2.p1.x = now2.p1.x + (pc4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p1.y = now2.p1.y + (ps4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p2.x = now2.p2.x + (pc1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p2.y = now2.p2.y + (ps1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p3.x = now2.p3.x + (pc3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p3.y = now2.p3.y + (ps3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p4.x = now2.p4.x + (pc2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now2.p4.y = now2.p4.y + (ps2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        prv2.v1 = new Vector
        {
            x = (prv2.p2).x,
            y = (prv2.p2).y
        };
        vctSub((prv2.v1), (prv2.p1));
        prv2.v2 = new Vector
        {
            x = (prv2.p1).x,
            y = (prv2.p1).y
        };
        vctSub((prv2.v2), (prv2.p3));
        now2.v1 = new Vector
        {
            x = (now2.p2).x,
            y = (now2.p2).y
        };
        vctSub((now2.v1), (now2.p1));
        now2.v2 = new Vector
        {
            x = (now2.p1).x,
            y = (now2.p1).y
        };
        vctSub((now2.v2), (now2.p3));
        now2.v1l = vctSize((now2.v1));
        now2.v2l = vctSize((now2.v2));
        {
            now3.p3.x = now1.p2.x;
            now3.p1.x = now3.p3.x;
            now3.pc1.x = now3.p1.x;
        }

        {
            now3.p3.y = now1.p2.y;
            now3.p1.y = now3.p3.y;
            now3.pc1.y = now3.p1.y;
        }

        {
            now3.p4.x = now3.p1.x + cos(pan.deg + PI * 1.5f) * PAN_HEIGHT;
            now3.p2.x = now3.p4.x;
            now3.pc2.x = now3.p2.x;
        }

        {
            now3.p4.y = now3.p1.y + sin(pan.deg + PI * 1.5f) * PAN_HEIGHT;
            now3.p2.y = now3.p4.y;
            now3.pc2.y = now3.p2.y;
        }

        now3.p1.x = now3.p1.x + (pc2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p1.y = now3.p1.y + (ps2 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p2.x = now3.p2.x + (pc3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p2.y = now3.p2.y + (ps3 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p3.x = now3.p3.x + (pc1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p3.y = now3.p3.y + (ps1 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p4.x = now3.p4.x + (pc4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        now3.p4.y = now3.p4.y + (ps4 * ((PAN_THICK + BALL_RADIUS) * 1.4f));
        prv3.v1 = new Vector
        {
            x = (prv3.p2).x,
            y = (prv3.p2).y
        };
        vctSub((prv3.v1), (prv3.p1));
        prv3.v2 = new Vector
        {
            x = (prv3.p1).x,
            y = (prv3.p1).y
        };
        vctSub((prv3.v2), (prv3.p3));
        now3.v1 = new Vector
        {
            x = (now3.p2).x,
            y = (now3.p2).y
        };
        vctSub((now3.v1), (now3.p1));
        now3.v2 = new Vector
        {
            x = (now3.p1).x,
            y = (now3.p1).y
        };
        vctSub((now3.v2), (now3.p3));
        now3.v1l = vctSize((now3.v1));
        now3.v2l = vctSize((now3.v2));
    }

    public static void drawPan()
    {
        int i = 0;
        float x = 0, y = 0, mx = 0, my = 0;
        x = now1.pc1.x;
        y = now1.pc1.y;
        mx = (now1.pc2.x - x) / 5;
        my = (now1.pc2.y - y) / 5;
        x = x - (mx);
        y = y - (my);
        {
            i = 0;
            for (; i < 8; i++, x = x + (mx), y = y + (my))
            {
                drawSprite(PAN_SPRITE_IDX, GameMath.integer(x), GameMath.integer(y));
            }
        }

        x = now2.pc1.x;
        y = now2.pc1.y;
        mx = (now2.pc2.x - x) * 0.7f;
        my = (now2.pc2.y - y) * 0.7f;
        {
            i = 0;
            for (; i < 3; i++, x = x + (mx), y = y + (my))
            {
                drawSprite(PAN_SPRITE_IDX + 1, GameMath.integer(x), GameMath.integer(y));
            }
        }

        x = now3.pc1.x;
        y = now3.pc1.y;
        mx = (now3.pc2.x - x) * 0.7f;
        my = (now3.pc2.y - y) * 0.7f;
        {
            i = 0;
            for (; i < 3; i++, x = x + (mx), y = y + (my))
            {
                drawSprite(PAN_SPRITE_IDX + 1, GameMath.integer(x), GameMath.integer(y));
            }
        }
    }
}
