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

public static class NrBonus
{
    public static Bonus[] bonus = Make(BONUS_MAX, () => new Bonus());
    public static int bonusScore;
    public static void resetBonusScore()
    {
        bonusScore = 10;
        showScore();
    }

    public static void getBonus()
    {
        addScore(bonusScore);
        if (bonusScore < 1000)
            bonusScore = bonusScore + (10);
    }

    public static void missBonus()
    {
        bonusScore = bonusScore / (20);
        bonusScore = bonusScore * (10);
        if (bonusScore < 10)
            bonusScore = 10;
        showScore();
    }

    public static void initBonuses()
    {
        int i = 0;
        {
            i = 0;
            for (; i < BONUS_MAX; i++)
            {
                bonus[(i)].cnt = NOT_EXIST;
            }
        }

        resetBonusScore();
    }

    public static int bonusIdx = BONUS_MAX;
    public static void addBonus(Vector pos, Vector vel)
    {
        int i = 0;
        {
            i = 0;
            for (; i < BONUS_MAX; i++)
            {
                bonusIdx--;
                if (bonusIdx < 0)
                    bonusIdx = BONUS_MAX - 1;
                if (bonus[(i)].cnt == NOT_EXIST)
                    break;
            }
        }

        if (i >= BONUS_MAX)
            return;
        bonus[(i)].pos = new Vector
        {
            x = (pos).x,
            y = (pos).y
        };
        bonus[(i)].vel = new Vector
        {
            x = (vel).x,
            y = (vel).y
        };
        bonus[(i)].cnt = 0;
        bonus[(i)].down = 1;
    }

    public static void moveBonuses()
    {
        int i = 0, d = 0;
        Bonus bn = null;
        {
            i = 0;
            for (; i < BONUS_MAX; i++)
            {
                if (bonus[(i)].cnt == NOT_EXIST)
                    continue;
                bn = (bonus[(i)]);
                bn.pos.x = bn.pos.x + (bn.vel.x);
                bn.pos.y = bn.pos.y + (bn.vel.y);
                bn.vel.x = bn.vel.x - (GameMath.signedShift(bn.vel.x, 6));
                if (bn.pos.x < SCAN_WIDTH_8 / 8)
                {
                    bn.pos.x = SCAN_WIDTH_8 / 8;
                    if (bn.vel.x < 0)
                        bn.vel.x = -bn.vel.x;
                }
                else if (bn.pos.x > SCAN_WIDTH_8 / 8 * 7)
                {
                    bn.pos.x = SCAN_WIDTH_8 / 8 * 7;
                    if (bn.vel.x > 0)
                        bn.vel.x = -bn.vel.x;
                }

                if (((bn.down) != 0))
                {
                    bn.vel.y = bn.vel.y + (GameMath.signedShift((BONUS_SPEED - bn.vel.y), 6));
                    if (bn.pos.y > SCAN_HEIGHT_8)
                    {
                        bn.down = 0;
                        bn.pos.y = SCAN_HEIGHT_8;
                        bn.vel.y = -bn.vel.y;
                    }
                }
                else
                {
                    bn.vel.y = bn.vel.y + (GameMath.signedShift((-BONUS_SPEED - bn.vel.y), 6));
                    if (bn.pos.y < 0)
                    {
                        missBonus();
                        bn.cnt = NOT_EXIST;
                        continue;
                    }
                }

                d = vctDist((ship.pos), (bn.pos));
                if (d < BONUS_ACQUIRE_WIDTH)
                {
                    getBonus();
                    playChunk(5);
                    bn.cnt = NOT_EXIST;
                    continue;
                }
                else if (d < BONUS_INHALE_WIDTH)
                {
                    bn.vel.x = bn.vel.x + (GameMath.signedShift((ship.pos.x - bn.pos.x) * (BONUS_INHALE_WIDTH - d), 20));
                    bn.vel.y = bn.vel.y + (GameMath.signedShift((ship.pos.y - bn.pos.y) * (BONUS_INHALE_WIDTH - d), 20));
                }

                bn.cnt++;
            }
        }
    }

    public static void drawBonuses()
    {
        int x = 0, y = 0, ox = 0, oy = 0, d = 0;
        int i = 0;
        Bonus bn = null;
        {
            i = 0;
            for (; i < BONUS_MAX; i++)
            {
                if (bonus[(i)].cnt == NOT_EXIST)
                    continue;
                bn = (bonus[(i)]);
                d = (bn.cnt * 8) & (DIV - 1);
                x = GameMath.signedShift((bn.pos.x / SCAN_WIDTH * LAYER_WIDTH), 8);
                y = GameMath.signedShift((bn.pos.y / SCAN_HEIGHT * LAYER_HEIGHT), 8);
                ox = GameMath.signedShift(sctbl[(d)], 5);
                oy = GameMath.signedShift(sctbl[(d + DIV / 4)], 5);
                drawBox(x + ox, y + oy, BONUS_DRAW_WIDTH, BONUS_DRAW_WIDTH, BONUS_COLOR_1, BONUS_COLOR_2, l1buf);
                drawBox(x - ox, y - oy, BONUS_DRAW_WIDTH, BONUS_DRAW_WIDTH, BONUS_COLOR_1, BONUS_COLOR_2, l1buf);
                drawBox(x + oy, y - ox, BONUS_DRAW_WIDTH, BONUS_DRAW_WIDTH, BONUS_COLOR_1, BONUS_COLOR_2, l1buf);
                drawBox(x - oy, y + ox, BONUS_DRAW_WIDTH, BONUS_DRAW_WIDTH, BONUS_COLOR_1, BONUS_COLOR_2, l1buf);
            }
        }
    }
}
