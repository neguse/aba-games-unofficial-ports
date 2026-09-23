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

public static class WkGenerator
{
    public static Generator[] generator = Make(GENERATOR_MAX, () => new Generator());
    public static void initGenerators()
    {
        int i = 0;
        {
            i = 0;
            for (; i < GENERATOR_MAX; i++)
            {
                generator[(i)].cnt = 0;
            }
        }
    }

    public static int generatorIdx = GENERATOR_MAX;
    public static int getNextGeneratorIdx()
    {
        int i = 0;
        {
            i = 0;
            for (; i < GENERATOR_MAX; i++)
            {
                generatorIdx--;
                if (generatorIdx < 0)
                    generatorIdx = GENERATOR_MAX - 1;
                if (generator[(generatorIdx)].cnt == 0)
                    break;
            }
        }

        if (i == GENERATOR_MAX)
            return 0;
        return 1;
    }

    public static void addGenerator(int spc)
    {
        int i = 0;
        if (getNextGeneratorIdx() == 0)
            return;
        generator[(generatorIdx)].spc = spc;
        switch (spc)
        {
            case FIRE:
                generator[(generatorIdx)].x = randN(SCREEN_WIDTH / 3) + SCREEN_WIDTH / 6;
                generator[(generatorIdx)].y = randN(SCREEN_HEIGHT / 2) + SCREEN_HEIGHT / 4;
                generator[(generatorIdx)].apCnt = 0;
                generator[(generatorIdx)].cnt = randN(120) + 120;
                break;
            case VOLCANO:
                generator[(generatorIdx)].x = randN(SCREEN_WIDTH / 4) + SCREEN_WIDTH / 5;
                generator[(generatorIdx)].y = randN(SCREEN_HEIGHT / 4) + SCREEN_HEIGHT / 2;
                generator[(generatorIdx)].apCnt = 0;
                generator[(generatorIdx)].cnt = randN(150) + 200;
                break;
            case TREE:
            {
                i = 0;
                for (; i < 3; i++)
                {
                    generator[(generatorIdx)].spc = spc;
                    generator[(generatorIdx)].x = randN(SCREEN_WIDTH / 2) + SCREEN_WIDTH / 5;
                    generator[(generatorIdx)].y = randN(SCREEN_HEIGHT / 5) + SCREEN_HEIGHT / 5;
                    generator[(generatorIdx)].apCnt = -randN(40) * i;
                    generator[(generatorIdx)].cnt = randN(100) + 100;
                    if (getNextGeneratorIdx() == 0)
                        return;
                }
            }

                break;
            case BUCKET:
                generator[(generatorIdx)].x = randN(SCREEN_WIDTH / 3) + SCREEN_WIDTH / 2;
                generator[(generatorIdx)].y = 0;
                generator[(generatorIdx)].apCnt = 0;
                generator[(generatorIdx)].cnt = randN(50) + 200;
                break;
            case CLOUD:
            {
                i = 0;
                for (; i < 2; i++)
                {
                    generator[(generatorIdx)].spc = spc;
                    generator[(generatorIdx)].x = randN(SCREEN_WIDTH / 2) + SCREEN_WIDTH / 5;
                    generator[(generatorIdx)].y = randN(SCREEN_HEIGHT / 4);
                    generator[(generatorIdx)].apCnt = 0;
                    generator[(generatorIdx)].cnt = randN(200) + 50;
                    if (getNextGeneratorIdx() == 0)
                        return;
                }
            }

                break;
            case WATER_TAP:
                generator[(generatorIdx)].x = 0;
                generator[(generatorIdx)].y = randN(SCREEN_HEIGHT / 4);
                generator[(generatorIdx)].apCnt = 0;
                generator[(generatorIdx)].cnt = randN(250) + 100;
                break;
        }
    }

    public static void moveGenerators()
    {
        int i = 0;
        Generator gn = null;
        {
            i = 0;
            for (; i < GENERATOR_MAX; i++)
            {
                if (generator[(i)].cnt <= 0)
                    continue;
                playChunk(0);
                gn = (generator[(i)]);
                switch (gn.spc)
                {
                    case FIRE:
                        if (randN(20) == 0)
                        {
                            addBall(0, randN(2), gn.x + 48, gn.y, randNS(10) * 0.2f, -randN(10) * 0.3f - 0.3f);
                        }

                        break;
                    case VOLCANO:
                        if (randN(18) == 0)
                        {
                            addBall(0, randN(3), gn.x + 48 + randNS(92), gn.y - randNS(48), randNS(10) * 0.1f, -randN(10) * 0.5f - 2);
                        }

                        break;
                    case TREE:
                        if (randN(32) == 0)
                        {
                            addBall(1, 2, gn.x + 48 + randNS(128), gn.y + 48, randNS(30) * 0.1f, 1);
                        }

                        break;
                    case BUCKET:
                        if (randN(16) == 0)
                        {
                            addBall(1, randN(2), gn.x - randN(32), gn.y + 80, -randNS(30) * 0.1f - 4, 0);
                        }

                        break;
                    case CLOUD:
                        if (randN(18) == 0)
                        {
                            addBall(2, randN(3), gn.x + 48 + randNS(160), gn.y + 48, randNS(20) * 0.1f, randN(10) * 0.3f + 1);
                        }

                        break;
                    case WATER_TAP:
                        if (randN(12) == 0)
                        {
                            addBall(2, 0, gn.x + 96, gn.y + 90, randNS(30) * 0.1f, randN(10) * 0.2f + 3);
                        }

                        break;
                }

                if (gn.cnt == 1)
                {
                    gn.apCnt--;
                    if (gn.apCnt <= 0)
                        gn.cnt = 0;
                }
                else
                {
                    if (gn.apCnt >= 16)
                    {
                        gn.cnt--;
                    }
                    else
                    {
                        gn.apCnt++;
                    }
                }
            }
        }
    }

    public static void drawGenerators()
    {
        int i = 0;
        Generator gn = null;
        {
            i = 0;
            for (; i < GENERATOR_MAX; i++)
            {
                if (generator[(i)].cnt <= 0)
                    continue;
                gn = (generator[(i)]);
                if (gn.apCnt >= 16)
                {
                    drawSprite(GEN_SPRITE_IDX + gn.spc, gn.x + randN(5) - 2, gn.y + randN(5) - 2);
                }
                else
                {
                    if (gn.apCnt >= 0)
                    {
                        drawSprite(gn.apCnt / 4 + PPL_SPRITE_IDX, gn.x + randN(9) - 4, gn.y + randN(9) - 4);
                    }
                }
            }
        }
    }
}
