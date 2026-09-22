// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class PlatformPos
{
    public Vector pos;
    public float deg;
    public bool used;
}

public class Field
{
    public const int BLOCK_SIZE_X = 20;
    public const int BLOCK_SIZE_Y = 64;
    public const int ON_BLOCK_THRESHOLD = 1;
    public const int NEXT_BLOCK_AREA_SIZE = 16;
    public const float SIDEWALL_X1 = 18;
    public const float SIDEWALL_X2 = 9.3f;
    public const float SIDEWALL_Y = 15;
    public const float TIME_COLOR_INDEX = 5;
    public const float TIME_CHANGE_RATIO = 0.00033f;
    public StageManager stageManager;
    public Ship ship;
    public GunroarRand rand;
    public Vector _size, _outerSize;
    public const int SCREEN_BLOCK_SIZE_X = 20;
    public const int SCREEN_BLOCK_SIZE_Y = 24;
    public const float BLOCK_WIDTH = 1;
    public int[][] block;
    public const float PANEL_WIDTH = 1.8f;
    public const float PANEL_HEIGHT_BASE = 0.66f;
    public FieldPanel[][] panel;
    public int nextBlockY;
    public float screenY, blockCreateCnt;
    public float _lastScrollY;
    public Vector screenPos;
    public PlatformPos[] platformPos = new PlatformPos[SCREEN_BLOCK_SIZE_X * NEXT_BLOCK_AREA_SIZE];
    public int platformPosNum;
    public float[][][] baseColorTime = new float[][][]
    {
        new float[][]
        {
            new float[] { 0.15f, 0.15f, 0.3f },
            new float[] { 0.25f, 0.25f, 0.5f },
            new float[] { 0.35f, 0.35f, 0.45f },
            new float[] { 0.6f, 0.7f, 0.35f },
            new float[] { 0.45f, 0.8f, 0.3f },
            new float[] { 0.2f, 0.6f, 0.1f }
        },
        new float[][]
        {
            new float[] { 0.1f, 0.1f, 0.3f },
            new float[] { 0.2f, 0.2f, 0.5f },
            new float[] { 0.3f, 0.3f, 0.4f },
            new float[] { 0.5f, 0.65f, 0.35f },
            new float[] { 0.4f, 0.7f, 0.3f },
            new float[] { 0.1f, 0.5f, 0.1f }
        },
        new float[][]
        {
            new float[] { 0.1f, 0.1f, 0.3f },
            new float[] { 0.2f, 0.2f, 0.5f },
            new float[] { 0.3f, 0.3f, 0.4f },
            new float[] { 0.5f, 0.65f, 0.35f },
            new float[] { 0.4f, 0.7f, 0.3f },
            new float[] { 0.1f, 0.5f, 0.1f }
        },
        new float[][]
        {
            new float[] { 0.2f, 0.15f, 0.25f },
            new float[] { 0.35f, 0.2f, 0.4f },
            new float[] { 0.5f, 0.35f, 0.45f },
            new float[] { 0.7f, 0.6f, 0.3f },
            new float[] { 0.6f, 0.65f, 0.25f },
            new float[] { 0.2f, 0.45f, 0.1f }
        },
        new float[][]
        {
            new float[] { 0.0f, 0.0f, 0.1f },
            new float[] { 0.1f, 0.1f, 0.3f },
            new float[] { 0.2f, 0.2f, 0.3f },
            new float[] { 0.2f, 0.3f, 0.15f },
            new float[] { 0.2f, 0.2f, 0.1f },
            new float[] { 0.0f, 0.15f, 0.0f }
        }
    };
    public float[][] baseColor;
    public float time;
    public Field()
    {
        block = new int[BLOCK_SIZE_X][];
        panel = new FieldPanel[BLOCK_SIZE_X][];
        for (int ix = 0; ix < BLOCK_SIZE_X; ix++)
        {
            block[ix] = new int[BLOCK_SIZE_Y];
            panel[ix] = new FieldPanel[BLOCK_SIZE_Y];
            for (int iy = 0; iy < BLOCK_SIZE_Y; iy++)
            {
                block[ix][iy] = 0;
                panel[ix][iy] = new FieldPanel();
            }
        }

        baseColor = new float[6][];
        for (int i = 0; i < 6; i++)
            baseColor[i] = new float[] { 0, 0, 0 };
        for (int i = 0; i < SCREEN_BLOCK_SIZE_X * NEXT_BLOCK_AREA_SIZE; i++)
            platformPos[i] = new PlatformPos();
        rand = new GunroarRand();
        _size = new Vector(SCREEN_BLOCK_SIZE_X / 2 * 0.9f, SCREEN_BLOCK_SIZE_Y / 2 * 0.8f);
        _outerSize = new Vector(SCREEN_BLOCK_SIZE_X / 2, SCREEN_BLOCK_SIZE_Y / 2);
        screenPos = new Vector();
        for (int index0 = 0; index0 < SCREEN_BLOCK_SIZE_X * NEXT_BLOCK_AREA_SIZE; index0++)
        {
            platformPos[index0].pos = new Vector();
        }

        _lastScrollY = 0;
        platformPosNum = 0;
        time = 0;
    }

    public void setRandSeed(int s)
    {
        rand.setSeed(s);
    }

    public void setStageManager(StageManager sm)
    {
        stageManager = sm;
    }

    public void setShip(Ship sp)
    {
        ship = sp;
    }

    public void start()
    {
        _lastScrollY = 0;
        nextBlockY = 0;
        screenY = NEXT_BLOCK_AREA_SIZE;
        blockCreateCnt = 0;
        for (int y = 0; y < BLOCK_SIZE_Y; y++)
        {
            for (int x = 0; x < BLOCK_SIZE_X; x++)
            {
                block[x][y] = -3;
                createPanel(x, y);
            }
        }

        time = rand.nextFloat(TIME_COLOR_INDEX);
    }

    public void createPanel(int x, int y)
    {
        FieldPanel p = panel[x][y];
        p.x = rand.nextFloat(1) - 0.75f;
        p.y = rand.nextFloat(1) - 0.75f;
        p.z = block[x][y] * PANEL_HEIGHT_BASE + rand.nextFloat(PANEL_HEIGHT_BASE);
        p.ci = block[x][y] + 3;
        p.outlineR = 1 + rand.nextSignedFloat(0.1f);
        p.og = 1 + rand.nextSignedFloat(0.1f);
        p.ob = 1 + rand.nextSignedFloat(0.1f);
        p.outlineR = p.outlineR * (0.33f);
        p.og = p.og * (0.33f);
        p.ob = p.ob * (0.33f);
    }

    public void scroll(float my, bool isDemo = false)
    {
        _lastScrollY = my;
        screenY = screenY - (my);
        if (screenY < 0)
            screenY = screenY + (BLOCK_SIZE_Y);
        blockCreateCnt = blockCreateCnt - (my);
        if (blockCreateCnt < 0)
        {
            stageManager.gotoNextBlockArea();
            int bd = 0;
            if (stageManager.bossMode)
                bd = 0;
            else
                bd = stageManager.blockDensity;
            createBlocks(bd);
            if (!isDemo)
            {
                stageManager.addBatteries(platformPos, platformPosNum);
            }

            gotoNextBlockArea();
        }
    }

    public void createBlocks(int groundDensity)
    {
        for (int y = nextBlockY; y < nextBlockY + NEXT_BLOCK_AREA_SIZE; y++)
        {
            int by = y % BLOCK_SIZE_Y;
            for (int bx = 0; bx < BLOCK_SIZE_X; bx++)
                block[bx][by] = -3;
        }

        platformPosNum = 0;
        int type = rand.nextInt(3);
        for (int i = 0; i < groundDensity; i++)
            addGround(type);
        for (int y = nextBlockY; y < nextBlockY + NEXT_BLOCK_AREA_SIZE; y++)
        {
            int by = y % BLOCK_SIZE_Y;
            for (int bx = 0; bx < BLOCK_SIZE_X; bx++)
            {
                if ((y == nextBlockY) || (y == nextBlockY + NEXT_BLOCK_AREA_SIZE - 1))
                    block[bx][by] = -3;
            }
        }

        for (int y = nextBlockY; y < nextBlockY + NEXT_BLOCK_AREA_SIZE; y++)
        {
            int by = y % BLOCK_SIZE_Y;
            for (int bx = 0; bx < BLOCK_SIZE_X - 1; bx++)
            {
                if (block[bx][by] == 0)
                    if (countAroundBlock(bx, by) <= 1)
                        block[bx][by] = -2;
            }

            for (int bx = BLOCK_SIZE_X - 1; bx >= 0; bx--)
            {
                if (block[bx][by] == 0)
                    if (countAroundBlock(bx, by) <= 1)
                        block[bx][by] = -2;
            }

            for (int bx = 0; bx < BLOCK_SIZE_X; bx++)
            {
                int b = 0;
                int c = countAroundBlock(bx, by);
                if (block[bx][by] >= 0)
                {
                    switch (c)
                    {
                        case 0:
                        {
                            b = -2;
                            break;
                        }

                        case 1:
                        case 2:
                        case 3:
                        {
                            b = 0;
                            break;
                        }

                        case 4:
                        {
                            b = 2;
                            break;
                        }
                    }
                }
                else
                {
                    switch (c)
                    {
                        case 0:
                        {
                            b = -3;
                            break;
                        }

                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        {
                            b = -1;
                            break;
                        }
                    }
                }

                block[bx][by] = b;
                if (((b == -1) && (bx >= 2)) && (bx < BLOCK_SIZE_X - 2))
                {
                    float pd = calcPlatformDeg(bx, by);
                    if (pd >= -PI * 2)
                    {
                        platformPos[platformPosNum].pos.x = bx;
                        platformPos[platformPosNum].pos.y = by;
                        platformPos[platformPosNum].deg = pd;
                        platformPos[platformPosNum].used = false;
                        platformPosNum++;
                    }
                }
            }
        }

        for (int y = nextBlockY; y < nextBlockY + NEXT_BLOCK_AREA_SIZE; y++)
        {
            int by = y % BLOCK_SIZE_Y;
            for (int bx = 0; bx < BLOCK_SIZE_X; bx++)
            {
                if (block[bx][by] == -3)
                {
                    if (countAroundBlock(bx, by, -1) > 0)
                        block[bx][by] = -2;
                }
                else if (block[bx][by] == 2)
                {
                    if (countAroundBlock(bx, by, 1) < 4)
                        block[bx][by] = 1;
                }

                createPanel(bx, by);
            }
        }
    }

    public void addGround(int type)
    {
        int cx = 0;
        switch (type)
        {
            case 0:
            {
                cx = rand.nextInt(GameMath.integer((BLOCK_SIZE_X * 0.4f))) + GameMath.integer((BLOCK_SIZE_X * 0.1f));
                break;
            }

            case 1:
            {
                cx = rand.nextInt(GameMath.integer((BLOCK_SIZE_X * 0.4f))) + GameMath.integer((BLOCK_SIZE_X * 0.5f));
                break;
            }

            case 2:
            {
                if (rand.nextInt(2) == 0)
                    cx = rand.nextInt(GameMath.integer((BLOCK_SIZE_X * 0.4f))) - GameMath.integer((BLOCK_SIZE_X * 0.2f));
                else
                    cx = rand.nextInt(GameMath.integer((BLOCK_SIZE_X * 0.4f))) + GameMath.integer((BLOCK_SIZE_X * 0.8f));
                break;
            }
        }

        int cy = rand.nextInt(GameMath.integer((NEXT_BLOCK_AREA_SIZE * 0.6f))) + GameMath.integer((NEXT_BLOCK_AREA_SIZE * 0.2f));
        cy = cy + (nextBlockY);
        int w = rand.nextInt(GameMath.integer((BLOCK_SIZE_X * 0.33f))) + GameMath.integer((BLOCK_SIZE_X * 0.33f));
        int h = rand.nextInt(GameMath.integer((NEXT_BLOCK_AREA_SIZE * 0.24f))) + GameMath.integer((NEXT_BLOCK_AREA_SIZE * 0.33f));
        cx = cx - (w / 2);
        cy = cy - (h / 2);
        float wr = 0, hr = 0;
        for (int y = nextBlockY; y < nextBlockY + NEXT_BLOCK_AREA_SIZE; y++)
        {
            int by = y % BLOCK_SIZE_Y;
            for (int bx = 0; bx < BLOCK_SIZE_X; bx++)
            {
                if ((((bx >= cx) && (bx < cx + w)) && (y >= cy)) && (y < cy + h))
                {
                    float o = 0, to = 0;
                    wr = rand.nextFloat(0.2f) + 0.2f;
                    hr = rand.nextFloat(0.3f) + 0.4f;
                    o = (bx - cx) * wr + (y - cy) * hr;
                    wr = rand.nextFloat(0.2f) + 0.2f;
                    hr = rand.nextFloat(0.3f) + 0.4f;
                    to = (cx + w - 1 - bx) * wr + (y - cy) * hr;
                    if (to < o)
                        o = to;
                    wr = rand.nextFloat(0.2f) + 0.2f;
                    hr = rand.nextFloat(0.3f) + 0.4f;
                    to = (bx - cx) * wr + (cy + h - 1 - y) * hr;
                    if (to < o)
                        o = to;
                    wr = rand.nextFloat(0.2f) + 0.2f;
                    hr = rand.nextFloat(0.3f) + 0.4f;
                    to = (cx + w - 1 - bx) * wr + (cy + h - 1 - y) * hr;
                    if (to < o)
                        o = to;
                    if (o > 1)
                        block[bx][by] = 0;
                }
            }
        }
    }

    public void gotoNextBlockArea()
    {
        blockCreateCnt = blockCreateCnt + (NEXT_BLOCK_AREA_SIZE);
        nextBlockY = nextBlockY - (NEXT_BLOCK_AREA_SIZE);
        if (nextBlockY < 0)
            nextBlockY = nextBlockY + (BLOCK_SIZE_Y);
    }

    public int getBlock_1(Vector p)
    {
        return getBlock_2(p.x, p.y);
    }

    public int getBlock_2(float x, float y)
    {
        y = y - (screenY - GameMath.integer(screenY));
        int bx = 0, by = 0;
        bx = GameMath.integer(((x + BLOCK_WIDTH * SCREEN_BLOCK_SIZE_X / 2) / BLOCK_WIDTH));
        by = GameMath.integer(screenY) + GameMath.integer(((-y + BLOCK_WIDTH * SCREEN_BLOCK_SIZE_Y / 2) / BLOCK_WIDTH));
        if ((bx < 0) || (bx >= BLOCK_SIZE_X))
            return -1;
        if (by < 0)
            by = by + (BLOCK_SIZE_Y);
        else if (by >= BLOCK_SIZE_Y)
            by = by - (BLOCK_SIZE_Y);
        return block[bx][by];
    }

    public Vector convertToScreenPos(int bx, int y)
    {
        float oy = screenY - GameMath.integer(screenY);
        int by = y - GameMath.integer(screenY);
        if (by <= -BLOCK_SIZE_Y)
            by = by + (BLOCK_SIZE_Y);
        if (by > 0)
            by = by - (BLOCK_SIZE_Y);
        screenPos.x = bx * BLOCK_WIDTH - BLOCK_WIDTH * SCREEN_BLOCK_SIZE_X / 2 + BLOCK_WIDTH / 2;
        screenPos.y = by * -BLOCK_WIDTH + BLOCK_WIDTH * SCREEN_BLOCK_SIZE_Y / 2 + oy - BLOCK_WIDTH / 2;
        return screenPos;
    }

    public void move()
    {
        time = time + (TIME_CHANGE_RATIO);
        if (time >= TIME_COLOR_INDEX)
            time = time - (TIME_COLOR_INDEX);
    }

    public void draw()
    {
        drawPanel();
    }

    public void drawSideWalls()
    {
        glDisable(GL_BLEND);
        GrScreen.setColor(0, 0, 0, 1);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(SIDEWALL_X1, SIDEWALL_Y, 0);
        glVertex3f(SIDEWALL_X2, SIDEWALL_Y, 0);
        glVertex3f(SIDEWALL_X2, -SIDEWALL_Y, 0);
        glVertex3f(SIDEWALL_X1, -SIDEWALL_Y, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-SIDEWALL_X1, SIDEWALL_Y, 0);
        glVertex3f(-SIDEWALL_X2, SIDEWALL_Y, 0);
        glVertex3f(-SIDEWALL_X2, -SIDEWALL_Y, 0);
        glVertex3f(-SIDEWALL_X1, -SIDEWALL_Y, 0);
        glEnd();
        glEnable(GL_BLEND);
    }

    public void drawPanel()
    {
        int ci = GameMath.integer(time);
        int nci = ci + 1;
        if (nci >= TIME_COLOR_INDEX)
            nci = 0;
        float co = time - ci;
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 3; j++)
                baseColor[i][j] = baseColorTime[ci][i][j] * (1 - co) + baseColorTime[nci][i][j] * co;
        int by = GameMath.integer(screenY);
        float oy = screenY - by;
        float sx = 0;
        float sy = BLOCK_WIDTH * SCREEN_BLOCK_SIZE_Y / 2 + oy;
        by--;
        if (by < 0)
            by = by + (BLOCK_SIZE_Y);
        sy = sy + (BLOCK_WIDTH);
        glBegin(GL_QUADS);
        for (int y = -1; y < SCREEN_BLOCK_SIZE_Y + NEXT_BLOCK_AREA_SIZE; y++)
        {
            if (by >= BLOCK_SIZE_Y)
                by = by - (BLOCK_SIZE_Y);
            sx = -BLOCK_WIDTH * SCREEN_BLOCK_SIZE_X / 2;
            for (int bx = 0; bx < SCREEN_BLOCK_SIZE_X; bx++)
            {
                FieldPanel p = panel[bx][by];
                GrScreen.setColor(baseColor[p.ci][0] * p.outlineR * 0.66f, baseColor[p.ci][1] * p.og * 0.66f, baseColor[p.ci][2] * p.ob * 0.66f);
                glVertex3f(sx + p.x, sy - p.y, p.z);
                glVertex3f(sx + p.x + PANEL_WIDTH, sy - p.y, p.z);
                glVertex3f(sx + p.x + PANEL_WIDTH, sy - p.y - PANEL_WIDTH, p.z);
                glVertex3f(sx + p.x, sy - p.y - PANEL_WIDTH, p.z);
                GrScreen.setColor(baseColor[p.ci][0] * 0.33f, baseColor[p.ci][1] * 0.33f, baseColor[p.ci][2] * 0.33f);
                glVertex2f(sx, sy);
                glVertex2f(sx + BLOCK_WIDTH, sy);
                glVertex2f(sx + BLOCK_WIDTH, sy - BLOCK_WIDTH);
                glVertex2f(sx, sy - BLOCK_WIDTH);
                sx = sx + (BLOCK_WIDTH);
            }

            sy = sy - (BLOCK_WIDTH);
            by++;
        }

        glEnd();
    }

    public static int[][] degBlockOfs = new int[][]
    {
        new int[] { 0, -1 },
        new int[] { 1, 0 },
        new int[] { 0, 1 },
        new int[] { -1, 0 }
    };
    public float calcPlatformDeg(int x, int y)
    {
        int d = rand.nextInt(4);
        for (int i = 0; i < 4; i++)
        {
            if (!checkBlock(x + degBlockOfs[d][0], y + degBlockOfs[d][1], -1, true))
            {
                float pd = d * PI / 2;
                int ox = x + degBlockOfs[d][0];
                int oy = y + degBlockOfs[d][1];
                int td = d;
                td--;
                if (td < 0)
                    td = 3;
                bool b1 = checkBlock(ox + degBlockOfs[td][0], oy + degBlockOfs[td][1], -1, true);
                td = d;
                td++;
                if (td >= 4)
                    td = 0;
                bool b2 = checkBlock(ox + degBlockOfs[td][0], oy + degBlockOfs[td][1], -1, true);
                if ((!b1) && b2)
                    pd = pd - (PI / 4);
                if (b1 && (!b2))
                    pd = pd + (PI / 4);
                pd = normalizeDeg(pd);
                return pd;
            }

            d++;
            if (d >= 4)
                d = 0;
        }

        return -99999;
    }

    public int countAroundBlock(int x, int y, int th = 0)
    {
        int c = 0;
        if (checkBlock(x, y - 1, th))
            c++;
        if (checkBlock(x + 1, y, th))
            c++;
        if (checkBlock(x, y + 1, th))
            c++;
        if (checkBlock(x - 1, y, th))
            c++;
        return c;
    }

    public bool checkBlock(int x, int y, int th = 0, bool outScreen = false)
    {
        if ((x < 0) || (x >= BLOCK_SIZE_X))
            return outScreen;
        int by = y;
        if (by < 0)
            by = by + (BLOCK_SIZE_Y);
        if (by >= BLOCK_SIZE_Y)
            by = by - (BLOCK_SIZE_Y);
        return (block[x][by] >= th);
    }

    public bool checkInField_1(Vector p)
    {
        return _size.contains_2(p);
    }

    public bool checkInField_2(float x, float y)
    {
        return _size.contains_3(x, y);
    }

    public bool checkInOuterField_1(Vector p)
    {
        return _outerSize.contains_2(p);
    }

    public bool checkInOuterField_2(float x, float y)
    {
        return _outerSize.contains_3(x, y);
    }

    public bool checkInOuterHeightField(Vector p)
    {
        if ((((p.x >= -_size.x) && (p.x <= _size.x)) && (p.y >= -_outerSize.y)) && (p.y <= _outerSize.y))
            return true;
        else
            return false;
    }

    public bool checkInFieldExceptTop(Vector p)
    {
        if (((p.x >= -_size.x) && (p.x <= _size.x)) && (p.y >= -_size.y))
            return true;
        else
            return false;
    }

    public bool checkInOuterFieldExceptTop(Vector p)
    {
        if ((((p.x >= -_outerSize.x) && (p.x <= _outerSize.x)) && (p.y >= -_outerSize.y)) && (p.y <= _outerSize.y * 2))
            return true;
        else
            return false;
    }

    public Vector size
    {
        get
        {
            return _size;
        }

        set
        {
            _size = value;
        }
    }

    public Vector outerSize
    {
        get
        {
            return _outerSize;
        }

        set
        {
            _outerSize = value;
        }
    }

    public float lastScrollY
    {
        get
        {
            return _lastScrollY;
        }

        set
        {
            _lastScrollY = value;
        }
    }
}

public class FieldPanel
{
    public float x, y, z;
    public int ci;
    public float outlineR, og, ob;
}
