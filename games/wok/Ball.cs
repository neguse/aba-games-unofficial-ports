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

public static class WkBall
{
    public static Ball[] ball = Make(BALL_MAX, () => new Ball());
    public static int scoreMulti, smFib, smTime;
    public static void initBalls()
    {
        int i = 0;
        {
            i = 0;
            for (; i < BALL_MAX; i++)
            {
                ball[(i)].color = -1;
            }
        }

        {
            smFib = 1;
            scoreMulti = smFib;
        }

        smTime = 0;
    }

    public static int ballIdx = BALL_MAX;
    public static float[] radSize = new float[]
    {
        7.0f,
        10.0f,
        15.0f
    };
    public static void addBall(int color, int size, float x, float y, float mx, float my)
    {
        int i = 0;
        {
            i = 0;
            for (; i < BALL_MAX; i++)
            {
                ballIdx--;
                if (ballIdx < 0)
                    ballIdx = BALL_MAX - 1;
                if (ball[(ballIdx)].color == -1)
                    break;
            }
        }

        if (i == BALL_MAX)
            return;
        ball[(ballIdx)].color = color;
        ball[(ballIdx)].size = size;
        ball[(ballIdx)].sprPtn = (2 - color) * 3 + size;
        ball[(ballIdx)].radius = radSize[(size)];
        ball[(ballIdx)].pos.x = x;
        ball[(ballIdx)].pos.y = y;
        ball[(ballIdx)].vel.x = mx;
        ball[(ballIdx)].vel.y = my;
    }

    public static void checkBallHit(Ball ball1, Ball ball2)
    {
        float l = 0;
        Vector ft = new Vector();
        Vector ofs = new Vector();
        Vector vel = new Vector();
        int i = 0;
        ofs.x = ball1.pos.x - ball2.pos.x;
        ofs.y = ball1.pos.y - ball2.pos.y;
        l = ofs.x * ofs.x + ofs.y * ofs.y;
        if (l <= (ball1.radius + ball2.radius) * (ball1.radius + ball2.radius))
        {
            float cs = 0;
            Vector csft1 = new Vector(), csft2 = new Vector();
            vel = ball1.vel;
            csft1 = new Vector
            {
                x = (vctGetElement(vel, ofs)).x,
                y = (vctGetElement(vel, ofs)).y
            };
            vctSub(vel, csft1);
            vel = ball2.vel;
            csft2 = new Vector
            {
                x = (vctGetElement(vel, ofs)).x,
                y = (vctGetElement(vel, ofs)).y
            };
            vctSub(vel, csft2);
            vctMul(csft1, ball1.radius * 0.8f / ball2.radius);
            vctMul(csft2, ball2.radius * 0.8f / ball1.radius);
            vctAdd((ball2.vel), csft1);
            vctAdd((ball1.vel), csft2);
            vctMul(ofs, (ball1.radius + ball2.radius) / sqrt(l) / 2);
            vctAdd((ball1.pos), (ball2.pos));
            vctMul((ball1.pos), 0.5f);
            ball2.pos = new Vector
            {
                x = (ball1.pos).x,
                y = (ball1.pos).y
            };
            vctSub((ball2.pos), ofs);
            vctAdd((ball1.pos), ofs);
        }
    }

    public static void checkPanHit(Ball bl, PanPos now, PanPos prv, int bs)
    {
        Vector vc1 = new Vector(), vc2 = new Vector(), vc3 = new Vector(), vc4 = new Vector();
        switch (bs)
        {
            case 0:
                if (now.p1.y < prv.p3.y)
                {
                    vc1 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc3 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }
                else
                {
                    vc3 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc1 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }

                if (now.p2.y < prv.p4.y)
                {
                    vc2 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc4 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }
                else
                {
                    vc4 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc2 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }

                break;
            case 1:
                if (now.p1.x > prv.p3.x)
                {
                    vc1 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc3 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }
                else
                {
                    vc3 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc1 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }

                if (now.p2.x > prv.p4.x)
                {
                    vc2 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc4 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }
                else
                {
                    vc4 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc2 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }

                break;
            case 2:
                if (now.p1.x < prv.p3.x)
                {
                    vc1 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc3 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }
                else
                {
                    vc3 = new Vector
                    {
                        x = (now.p1).x,
                        y = (now.p1).y
                    };
                    vc1 = new Vector
                    {
                        x = (prv.p3).x,
                        y = (prv.p3).y
                    };
                }

                if (now.p2.x < prv.p4.x)
                {
                    vc2 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc4 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }
                else
                {
                    vc4 = new Vector
                    {
                        x = (now.p2).x,
                        y = (now.p2).y
                    };
                    vc2 = new Vector
                    {
                        x = (prv.p4).x,
                        y = (prv.p4).y
                    };
                }

                break;
        }

        if ((((vctCheckSide((bl.pos), (now.p1), (now.p2)) <= 0) && (((vctCheckSide((bl.pos), (prv.p3), (prv.p4)) >= 0) || (vctCheckSide((bl.pos), (now.p3), (now.p4)) >= 0)))) && (vctCheckSide((bl.pos), vc2, vc4) <= 0)) && (vctCheckSide((bl.pos), vc1, vc3) >= 0))
        {
            Vector ofs = new Vector(), o1 = new Vector(), o2 = new Vector(), po1 = new Vector(), po2 = new Vector(), rv = new Vector();
            float l1 = 0, l2 = 0;
            ofs = new Vector
            {
                x = (bl.pos).x,
                y = (bl.pos).y
            };
            vctSub(ofs, (prv.p1));
            po1 = new Vector
            {
                x = (vctGetElement(ofs, (prv.v1))).x,
                y = (vctGetElement(ofs, (prv.v1))).y
            };
            l1 = vctSize(po1);
            if (l1 > now.v1l)
                return;
            po2 = new Vector
            {
                x = (vctGetElement(ofs, (prv.v2))).x,
                y = (vctGetElement(ofs, (prv.v2))).y
            };
            l2 = vctSize(po2);
            if (l2 > now.v2l)
                return;
            bl.pos = new Vector
            {
                x = (now.p1).x,
                y = (now.p1).y
            };
            o1 = new Vector
            {
                x = (now.v1).x,
                y = (now.v1).y
            };
            vctMul(o1, l1 / now.v1l);
            vctAdd((bl.pos), o1);
            o2 = new Vector
            {
                x = (now.v2).x,
                y = (now.v2).y
            };
            if (ofs.y < 0)
            {
                vctMul(o2, l2 / now.v2l);
                vctAdd((bl.pos), o2);
            }

            rv = new Vector
            {
                x = (vctGetElement((bl.vel), (now.v2))).x,
                y = (vctGetElement((bl.vel), (now.v2))).y
            };
            vctMul(rv, 1.2f);
            vctSub((bl.vel), rv);
            rv = new Vector
            {
                x = (pan.vel).x,
                y = (pan.vel).y
            };
            vctMul(rv, -2 / (bl.radius * 0.5f));
            if (rv.y > 0)
                rv.y = 0;
            rv.x = rv.x * (0.5f);
            vctAdd((bl.vel), rv);
        }
    }

    public static void addBallScore(Ball bl)
    {
        int nsmf = 0;
        int bs = (bl.size + 3);
        int smLgt = 1, sm = 0;
        addScore(bs * scoreMulti);
        sm = scoreMulti;
        while (sm > 0)
        {
            sm = sm / (10);
            smLgt++;
        }

        addBoard(bl.pos.x - smLgt * 52, bl.pos.y, -randN(10) * 0.1f - 0.1f, -randN(20) * 0.1f, bs, scoreMulti, (scoreMulti / 100) + randN(10) + 10);
        nsmf = scoreMulti;
        scoreMulti = GameMath.integer(scoreMulti + (smFib * 0.5f + 1));
        if (scoreMulti > MULTI_MAX)
        {
            scoreMulti = MULTI_MAX;
        }

        smFib = nsmf;
        smTime = 20;
    }

    public static float[] gravityBase = new float[]
    {
        0.004f,
        0.008f,
        0.012f
    };
    public static float[] gravity = Make(3, () => 0f);
    public static float missX;
    public static void moveBalls()
    {
        int i = 0, j = 0;
        Ball bl = null;
        {
            i = 0;
            for (; i < 3; i++)
            {
                gravity[(i)] = gravityBase[(i)] * rank / RANK_BASE;
            }
        }

        {
            i = 0;
            for (; i < BALL_MAX; i++)
            {
                if (ball[(i)].color == -1)
                    continue;
                bl = (ball[(i)]);
                bl.vel.y = bl.vel.y + (gravity[(bl.color)]);
                bl.pos.x = bl.pos.x + (bl.vel.x);
                bl.pos.y = bl.pos.y + (bl.vel.y);
                {
                    j = i + 1;
                    for (; j < BALL_MAX; j++)
                    {
                        if (ball[(j)].color == -1)
                            continue;
                        checkBallHit(bl, ball[(j)]);
                    }
                }

                if (status == MISS)
                {
                    bl.vel.x = bl.vel.x + ((bl.pos.x - missX) * 0.003f);
                    bl.vel.y = bl.vel.y - (0.3f);
                }
                else if (status == IN_GAME)
                {
                    checkPanHit(bl, now1, prv1, 0);
                    checkPanHit(bl, now2, prv2, 1);
                    checkPanHit(bl, now3, prv3, 2);
                }

                if (bl.pos.y < 0)
                {
                    bl.vel.x = bl.vel.x * (0.99f);
                    bl.vel.y = bl.vel.y * (0.99f);
                    if (bl.pos.y < -SCREEN_HEIGHT)
                    {
                        bl.pos.y = -SCREEN_HEIGHT;
                        bl.vel.y = 0;
                    }
                }

                if ((bl.pos.x < 0) && (status == IN_GAME))
                {
                    bl.vel.x = bl.vel.x * (-0.8f);
                    bl.pos.x = 0;
                }

                if (bl.pos.x > SCREEN_WIDTH * 0.9f)
                {
                    if (bl.pos.y < 0)
                    {
                        bl.vel.x = bl.vel.x * (-0.8f);
                        bl.pos.x = SCREEN_WIDTH * 0.9f;
                    }
                    else
                    {
                        if (status == IN_GAME)
                        {
                            addBallScore(bl);
                        }

                        bl.color = -1;
                    }
                }

                if (status == IN_GAME)
                {
                    if (bl.pos.y > SCREEN_HEIGHT * 0.75f)
                    {
                        if (bl.vel.y > 2)
                            bl.vel.y = bl.vel.y * (0.8f);
                        bl.vel.x = bl.vel.x * (0.99f);
                        bl.vel.y = bl.vel.y * (0.95f);
                        if (bl.pos.y > SCREEN_HEIGHT)
                        {
                            status = MISS;
                            missX = bl.pos.x;
                            stopMusic();
                            playChunk(2);
                            {
                                j = 0;
                                for (; j < 32; j++)
                                {
                                    addBall(0, 0, bl.pos.x + randNS(32), bl.pos.y - randN(32), randNS(32) * 0.1f, randNS(32) * 0.1f);
                                }
                            }
                        }
                    }
                }
                else if (bl.pos.y > SCREEN_HEIGHT)
                {
                    bl.color = -1;
                }
            }
        }

        if (smTime > 0)
        {
            smTime--;
            if (smTime <= 0)
            {
                {
                    smFib = 1;
                    scoreMulti = smFib;
                }
            }
        }
    }

    public static int lastSm = 0, smy = SCREEN_HEIGHT;
    public static void drawBalls()
    {
        int i = 0;
        int x = 0;
        if (smFib > 1)
        {
            smy = SCREEN_HEIGHT - 72;
            drawSprite(NUM_SPRITE_IDX + 10, 0, smy);
            drawNum(smFib, 52, smy);
            lastSm = smFib;
        }
        else
        {
            if (smy < SCREEN_HEIGHT)
            {
                smy++;
                drawSprite(NUM_SPRITE_IDX + 10, 0, smy);
                drawNum(lastSm, 52, smy);
            }
        }

        {
            i = 0;
            for (; i < BALL_MAX; i++)
            {
                if (ball[(i)].color == -1)
                    continue;
                drawSprite(ball[(i)].sprPtn, GameMath.integer(ball[(i)].pos.x), GameMath.integer(ball[(i)].pos.y));
            }
        }
    }
}
