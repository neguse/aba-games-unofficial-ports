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

public static class RrScreen
{
    public static float zoom = 15;
    public static int screenShakeCnt = 0;
    public static int screenShakeType = 0;
    public static void setEyepos()
    {
        float x = 0, y = 0;
        glPushMatrix();
        if (screenShakeCnt > 0)
        {
            switch (screenShakeType)
            {
                case 0:
                    x = (float)randNS2(256) / 5000.0f;
                    y = (float)randNS2(256) / 5000.0f;
                    break;
                default:
                    x = (float)randNS2(256) * screenShakeCnt / 21000.0f;
                    y = (float)randNS2(256) * screenShakeCnt / 21000.0f;
                    break;
            }

            RrGl.gluLookAt(0, 0, zoom, x, y, 0, 0.0f, 1.0f, 0.0f);
        }
        else
        {
            RrGl.gluLookAt(0, 0, zoom, 0, 0, 0, 0.0f, 1.0f, 0.0f);
        }
    }

    public static void setScreenShake(int type, int cnt)
    {
        screenShakeType = type;
        screenShakeCnt = cnt;
    }

    public static void moveScreenShake()
    {
        if (screenShakeCnt > 0)
        {
            screenShakeCnt--;
        }
    }

    public static void drawGLSceneStart()
    {
        Drawing.BeginFrame();
        Drawing.farPlane = 720;
        Drawing.projectionScale = 1.8106602f;
        setEyepos();
    }

    public static void drawGLSceneEnd()
    {
        glPopMatrix();
    }

    public static void drawBox(float x, float y, float width, float height, int r, int g, int b)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        RrGl.glColor4ub(r, g, b, 128);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-width, -height, 0);
        glVertex3f(width, -height, 0);
        glVertex3f(width, height, 0);
        glVertex3f(-width, height, 0);
        glEnd();
        RrGl.glColor4ub(r, g, b, 255);
        glBegin(GL_LINE_LOOP);
        glVertex3f(-width, -height, 0);
        glVertex3f(width, -height, 0);
        glVertex3f(width, height, 0);
        glVertex3f(-width, height, 0);
        glEnd();
        glPopMatrix();
    }

    public static void drawLine(float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a)
    {
        RrGl.glColor4ub(r, g, b, a);
        glBegin(GL_LINES);
        glVertex3f(x1, y1, z1);
        glVertex3f(x2, y2, z2);
        glEnd();
    }

    public static void drawLinePart(float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a, int len)
    {
        RrGl.glColor4ub(r, g, b, a);
        glBegin(GL_LINES);
        glVertex3f(x1, y1, z1);
        glVertex3f(x1 + (x2 - x1) * len / 256, y1 + (y2 - y1) * len / 256, z1 + (z2 - z1) * len / 256);
        glEnd();
    }

    public static void drawRollLineAbs(float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a, int d1)
    {
        glPushMatrix();
        glRotatef((float)d1 * 360 / 1024, 0, 0, 1);
        RrGl.glColor4ub(r, g, b, a);
        glBegin(GL_LINES);
        glVertex3f(x1, y1, z1);
        glVertex3f(x2, y2, z2);
        glEnd();
        glPopMatrix();
    }

    public static void drawRollLine(float x, float y, float z, float width, int r, int g, int b, int a, int d1, int d2)
    {
        glPushMatrix();
        glTranslatef(x, y, z);
        glRotatef((float)d1 * 360 / 1024, 0, 0, 1);
        glRotatef((float)d2 * 360 / 1024, 1, 0, 0);
        RrGl.glColor4ub(r, g, b, a);
        glBegin(GL_LINES);
        glVertex3f(0, -width, 0);
        glVertex3f(0, width, 0);
        glEnd();
        glPopMatrix();
    }

    public static void drawSquare(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3, float x4, float y4, float z4, int r, int g, int b)
    {
        RrGl.glColor4ub(r, g, b, 64);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(x1, y1, z1);
        glVertex3f(x2, y2, z2);
        glVertex3f(x3, y3, z3);
        glVertex3f(x4, y4, z4);
        glEnd();
    }

    public static void drawStar(int f, float x, float y, float z, int r, int g, int b, float size)
    {
        RrGl.glEnable(GL_TEXTURE_2D);
        if (((f) != 0))
        {
            RrGl.glBindTexture(GL_TEXTURE_2D, starTexture);
        }
        else
        {
            RrGl.glBindTexture(GL_TEXTURE_2D, smokeTexture);
        }

        RrGl.glColor4ub(r, g, b, 255);
        glPushMatrix();
        glTranslatef(x, y, z);
        glRotatef(rand() % 360, 0.0f, 0.0f, 1.0f);
        glBegin(GL_TRIANGLE_FAN);
        RrGl.glTexCoord2f(0.0f, 1.0f);
        glVertex3f(-size, -size, 0);
        RrGl.glTexCoord2f(1.0f, 1.0f);
        glVertex3f(size, -size, 0);
        RrGl.glTexCoord2f(1.0f, 0.0f);
        glVertex3f(size, size, 0);
        RrGl.glTexCoord2f(0.0f, 0.0f);
        glVertex3f(-size, size, 0);
        glEnd();
        glPopMatrix();
        RrGl.glDisable(GL_TEXTURE_2D);
    }

    public static void drawLaser(float x, float y, float width, float height, int cc1, int cc2, int cc3, int cc4, int cnt, int type)
    {
        int i = 0, d = 0;
        float gx = 0, gy = 0;
        glBegin(GL_TRIANGLE_FAN);
        if (type != 0)
        {
            RrGl.glColor4ub(cc1, cc1, cc1, LASER_ALPHA);
            glVertex3f(x - width, y, 0);
        }

        RrGl.glColor4ub(cc2, 255, cc2, LASER_ALPHA);
        glVertex3f(x, y, 0);
        RrGl.glColor4ub(cc4, 255, cc4, LASER_ALPHA);
        glVertex3f(x, y + height, 0);
        RrGl.glColor4ub(cc3, cc3, cc3, LASER_ALPHA);
        glVertex3f(x - width, y + height, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        if (type != 0)
        {
            RrGl.glColor4ub(cc1, cc1, cc1, LASER_ALPHA);
            glVertex3f(x + width, y, 0);
        }

        RrGl.glColor4ub(cc2, 255, cc2, LASER_ALPHA);
        glVertex3f(x, y, 0);
        RrGl.glColor4ub(cc4, 255, cc4, LASER_ALPHA);
        glVertex3f(x, y + height, 0);
        RrGl.glColor4ub(cc3, cc3, cc3, LASER_ALPHA);
        glVertex3f(x + width, y + height, 0);
        glEnd();
        if (type == 2)
            return;
        RrGl.glColor4ub(80, 240, 80, LASER_LINE_ALPHA);
        glBegin(GL_LINES);
        d = (cnt * LASER_LINE_ROLL_SPEED) & (GameMath.integer(512 / 4) - 1);
        {
            i = 0;
            for (; i < 4; i++, d = d + ((GameMath.integer(512 / 4))))
            {
                d = d & (1023);
                gx = x + width * sctbl[(d + 256)] / 256.0f;
                if (type == 1)
                {
                    glVertex3f(gx, y, 0);
                }
                else
                {
                    glVertex3f(x, y, 0);
                }

                glVertex3f(gx, y + height, 0);
            }
        }

        if (type == 0)
        {
            glEnd();
            return;
        }

        gy = y + (height / 4 / LASER_LINE_UP_SPEED) * (cnt & (LASER_LINE_UP_SPEED - 1));
        {
            i = 0;
            for (; i < 4; i++, gy = gy + (height / 4))
            {
                glVertex3f(x - width, gy, 0);
                glVertex3f(x + width, gy, 0);
            }
        }

        glEnd();
    }

    public static void drawRing(float x, float y, int d1, int d2, int r, int g, int b)
    {
        int i = 0, d = 0;
        float x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0, x3 = 0, y3 = 0, z3 = 0, x4 = 0, y4 = 0, z4 = 0;
        glPushMatrix();
        glTranslatef(x, y, 0);
        glRotatef((float)d1 * 360 / 1024, 0, 0, 1);
        glRotatef((float)d2 * 360 / 1024, 1, 0, 0);
        RrGl.glColor4ub(r, g, b, 255);
        {
            x2 = 0;
            x1 = x2;
        }

        {
            y4 = CORE_HEIGHT / 2;
            y1 = y4;
        }

        {
            y3 = -CORE_HEIGHT / 2;
            y2 = y3;
        }

        {
            z2 = CORE_RING_SIZE;
            z1 = z2;
        }

        {
            i = 0;
            d = 0;
            for (; i < 8; i++)
            {
                d = d + ((GameMath.integer(1024 / 8)));
                d = d & (1023);
                {
                    x4 = sctbl[(d + 256)] * CORE_RING_SIZE / 256;
                    x3 = x4;
                }

                {
                    z4 = sctbl[(d)] * CORE_RING_SIZE / 256;
                    z3 = z4;
                }

                drawSquare(x1, y1, z1, x2, y2, z2, x3, y3, z3, x4, y4, z4, r, g, b);
                x1 = x3;
                y1 = y3;
                z1 = z3;
                x2 = x4;
                y2 = y4;
                z2 = z4;
            }
        }

        glPopMatrix();
    }

    public static void drawCore(float x, float y, int cnt, int r, int g, int b)
    {
        int i = 0;
        float cy = 0;
        glPushMatrix();
        glTranslatef(x, y, 0);
        RrGl.glColor4ub(r, g, b, 255);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0);
        glVertex3f(SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0);
        glVertex3f(SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0);
        glVertex3f(-SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0);
        glEnd();
        glPopMatrix();
        cy = y - CORE_HEIGHT * 2.5f;
        {
            i = 0;
            for (; i < 4; i++, cy = cy + (CORE_HEIGHT))
            {
                drawRing(x, cy, (cnt * (4 + i)) & 1023, (GameMath.integer(sctbl[((cnt * (5 + i)) & 1023)] / 4)) & 1023, r, g, b);
            }
        }
    }

    public static void drawShipShape(float x, float y, float d, int inv)
    {
        int i = 0;
        glPushMatrix();
        glTranslatef(x, y, 0);
        RrGl.glColor4ub(255, 100, 100, 255);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0);
        glVertex3f(SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0);
        glVertex3f(SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0);
        glVertex3f(-SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0);
        glEnd();
        if (((inv) != 0))
        {
            glPopMatrix();
            return;
        }

        glRotatef(d, 0, 1, 0);
        RrGl.glColor4ub(120, 220, 100, 150);
        {
            i = 0;
            for (; i < 8; i++)
            {
                glRotatef(45, 0, 1, 0);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-SHIP_DRUM_WIDTH, -SHIP_DRUM_HEIGHT, SHIP_DRUM_R);
                glVertex3f(SHIP_DRUM_WIDTH, -SHIP_DRUM_HEIGHT, SHIP_DRUM_R);
                glVertex3f(SHIP_DRUM_WIDTH, SHIP_DRUM_HEIGHT, SHIP_DRUM_R);
                glVertex3f(-SHIP_DRUM_WIDTH, SHIP_DRUM_HEIGHT, SHIP_DRUM_R);
                glEnd();
            }
        }

        glPopMatrix();
    }

    public static void drawBomb(float x, float y, float width, int cnt)
    {
        int i = 0, d = 0, od = 0, c = 0;
        float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        d = cnt * 48;
        d = d & (1023);
        c = 4 + (GameMath.signedShift(cnt, 3));
        if (c > 16)
            c = 16;
        od = GameMath.integer(1024 / c);
        x1 = (sctbl[(d)] * width) / 256 + x;
        y1 = (sctbl[(d + 256)] * width) / 256 + y;
        {
            i = 0;
            for (; i < c; i++)
            {
                d = d + (od);
                d = d & (1023);
                x2 = (sctbl[(d)] * width) / 256 + x;
                y2 = (sctbl[(d + 256)] * width) / 256 + y;
                drawLine(x1, y1, 0, x2, y2, 0, 255, 255, 255, 255);
                x1 = x2;
                y1 = y2;
            }
        }
    }

    public static void drawCircle(float x, float y, float width, int cnt, int r1, int g1, int b1, int r2, int b2, int g2)
    {
        int i = 0, d = 0;
        float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        if ((cnt & 1) == 0)
        {
            RrGl.glColor4ub(r1, g1, b1, 64);
        }
        else
        {
            RrGl.glColor4ub(255, 255, 255, 64);
        }

        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(x, y, 0);
        d = cnt * 48;
        d = d & (1023);
        x1 = (sctbl[(d)] * width) / 256 + x;
        y1 = (sctbl[(d + 256)] * width) / 256 + y;
        RrGl.glColor4ub(r2, g2, b2, 150);
        {
            i = 0;
            for (; i < 16; i++)
            {
                d = d + (64);
                d = d & (1023);
                x2 = (sctbl[(d)] * width) / 256 + x;
                y2 = (sctbl[(d + 256)] * width) / 256 + y;
                glVertex3f(x1, y1, 0);
                glVertex3f(x2, y2, 0);
                x1 = x2;
                y1 = y2;
            }
        }

        glEnd();
    }

    public static void drawShape(float x, float y, float size, int d, int cnt, int type, int r, int g, int b)
    {
        float sz = 0, sz2 = 0;
        glPushMatrix();
        glTranslatef(x, y, 0);
        RrGl.glColor4ub(r, g, b, 255);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0);
        glVertex3f(SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0);
        glVertex3f(SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0);
        glVertex3f(-SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0);
        glEnd();
        switch (type)
        {
            case 0:
                sz = size / 2;
                glRotatef((float)d * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(sz, -sz, 0);
                glVertex3f(0, size, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 150);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(sz, -sz, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(0, size, 0);
                glEnd();
                break;
            case 1:
                sz = size / 2;
                glRotatef((float)((cnt * 23) & 1023) * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(0, -size, 0);
                glVertex3f(sz, 0, 0);
                glVertex3f(0, size, 0);
                glVertex3f(-sz, 0, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 180);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(0, -size, 0);
                glVertex3f(sz, 0, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(0, size, 0);
                glVertex3f(-sz, 0, 0);
                glEnd();
                break;
            case 2:
                sz = size / 4;
                sz2 = size / 3 * 2;
                glRotatef((float)d * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz, -sz2, 0);
                glVertex3f(sz, -sz2, 0);
                glVertex3f(sz, sz2, 0);
                glVertex3f(-sz, sz2, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 120);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz2, 0);
                glVertex3f(sz, -sz2, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(sz, sz2, 0);
                glVertex3f(-sz, sz2, 0);
                glEnd();
                break;
            case 3:
                sz = size / 2;
                glRotatef((float)((cnt * 37) & 1023) * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(sz, -sz, 0);
                glVertex3f(sz, sz, 0);
                glVertex3f(-sz, sz, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 180);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(sz, -sz, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(sz, sz, 0);
                glVertex3f(-sz, sz, 0);
                glEnd();
                break;
            case 4:
                sz = size / 2;
                glRotatef((float)((cnt * 53) & 1023) * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz / 2, -sz, 0);
                glVertex3f(sz / 2, -sz, 0);
                glVertex3f(sz, -sz / 2, 0);
                glVertex3f(sz, sz / 2, 0);
                glVertex3f(sz / 2, sz, 0);
                glVertex3f(-sz / 2, sz, 0);
                glVertex3f(-sz, sz / 2, 0);
                glVertex3f(-sz, -sz / 2, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 220);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz / 2, -sz, 0);
                glVertex3f(sz / 2, -sz, 0);
                glVertex3f(sz, -sz / 2, 0);
                glVertex3f(sz, sz / 2, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(sz / 2, sz, 0);
                glVertex3f(-sz / 2, sz, 0);
                glVertex3f(-sz, sz / 2, 0);
                glVertex3f(-sz, -sz / 2, 0);
                glEnd();
                break;
            case 5:
                sz = size * 2 / 3;
                sz2 = size / 5;
                glRotatef((float)d * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_STRIP);
                glVertex3f(-sz, -sz + sz2, 0);
                glVertex3f(0, sz + sz2, 0);
                glVertex3f(sz, -sz + sz2, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 150);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz + sz2, 0);
                glVertex3f(sz, -sz + sz2, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(0, sz + sz2, 0);
                glEnd();
                break;
            case 6:
                sz = size / 2;
                glRotatef((float)((cnt * 13) & 1023) * 360 / 1024, 0, 0, 1);
                RrGl.glDisable(GL_BLEND);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(0, -sz, 0);
                glVertex3f(sz, 0, 0);
                glVertex3f(sz, sz, 0);
                glVertex3f(0, sz, 0);
                glVertex3f(-sz, 0, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(r, g, b, 210);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz, 0);
                glVertex3f(0, -sz, 0);
                glVertex3f(sz, 0, 0);
                RrGl.glColor4ub(SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 150);
                glVertex3f(sz, sz, 0);
                glVertex3f(0, sz, 0);
                glVertex3f(-sz, 0, 0);
                glEnd();
                break;
        }

        glPopMatrix();
    }

    public static int[][][] ikaClr = new int[][][]
    {
        new int[][]
        {
            new int[]
            {
                230,
                230,
                255
            },
            new int[]
            {
                100,
                100,
                200
            },
            new int[]
            {
                50,
                50,
                150
            }
        },
        new int[][]
        {
            new int[]
            {
                0,
                0,
                0
            },
            new int[]
            {
                200,
                0,
                0
            },
            new int[]
            {
                100,
                0,
                0
            }
        },
    };
    public static void drawShapeIka(float x, float y, float size, int d, int cnt, int type, int c)
    {
        float sz = 0, sz2 = 0, sz3 = 0;
        glPushMatrix();
        glTranslatef(x, y, 0);
        RrGl.glColor4ub(ikaClr[(c)][(0)][(0)], ikaClr[(c)][(0)][(1)], ikaClr[(c)][(0)][(2)], 255);
        RrGl.glDisable(GL_BLEND);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0);
        glVertex3f(SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0);
        glVertex3f(SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0);
        glVertex3f(-SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0);
        glEnd();
        RrGl.glColor4ub(ikaClr[(c)][(0)][(0)], ikaClr[(c)][(0)][(1)], ikaClr[(c)][(0)][(2)], 255);
        switch (type)
        {
            case 0:
                sz = size / 2;
                sz2 = sz / 3;
                sz3 = size * 2 / 3;
                glRotatef((float)d * 360 / 1024, 0, 0, 1);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz, -sz3, 0);
                glVertex3f(sz, -sz3, 0);
                glVertex3f(sz2, sz3, 0);
                glVertex3f(-sz2, sz3, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(ikaClr[(c)][(1)][(0)], ikaClr[(c)][(1)][(1)], ikaClr[(c)][(1)][(2)], 250);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz, -sz3, 0);
                glVertex3f(sz, -sz3, 0);
                RrGl.glColor4ub(ikaClr[(c)][(2)][(0)], ikaClr[(c)][(2)][(1)], ikaClr[(c)][(2)][(2)], 250);
                glVertex3f(sz2, sz3, 0);
                glVertex3f(-sz2, sz3, 0);
                glEnd();
                break;
            case 1:
                sz = size / 2;
                glRotatef((float)((cnt * 53) & 1023) * 360 / 1024, 0, 0, 1);
                glBegin(GL_LINE_LOOP);
                glVertex3f(-sz / 2, -sz, 0);
                glVertex3f(sz / 2, -sz, 0);
                glVertex3f(sz, -sz / 2, 0);
                glVertex3f(sz, sz / 2, 0);
                glVertex3f(sz / 2, sz, 0);
                glVertex3f(-sz / 2, sz, 0);
                glVertex3f(-sz, sz / 2, 0);
                glVertex3f(-sz, -sz / 2, 0);
                glEnd();
                RrGl.glEnable(GL_BLEND);
                RrGl.glColor4ub(ikaClr[(c)][(1)][(0)], ikaClr[(c)][(1)][(1)], ikaClr[(c)][(1)][(2)], 250);
                glBegin(GL_TRIANGLE_FAN);
                glVertex3f(-sz / 2, -sz, 0);
                glVertex3f(sz / 2, -sz, 0);
                glVertex3f(sz, -sz / 2, 0);
                glVertex3f(sz, sz / 2, 0);
                RrGl.glColor4ub(ikaClr[(c)][(2)][(0)], ikaClr[(c)][(2)][(1)], ikaClr[(c)][(2)][(2)], 250);
                glVertex3f(sz / 2, sz, 0);
                glVertex3f(-sz / 2, sz, 0);
                glVertex3f(-sz, sz / 2, 0);
                glVertex3f(-sz, -sz / 2, 0);
                glEnd();
                break;
        }

        glPopMatrix();
    }

    public static int[][][] shtClr = new int[][][]
    {
        new int[][]
        {
            new int[]
            {
                200,
                200,
                225
            },
            new int[]
            {
                50,
                50,
                200
            },
            new int[]
            {
                200,
                200,
                225
            }
        },
        new int[][]
        {
            new int[]
            {
                100,
                0,
                0
            },
            new int[]
            {
                100,
                0,
                0
            },
            new int[]
            {
                200,
                0,
                0
            }
        },
        new int[][]
        {
            new int[]
            {
                100,
                200,
                100
            },
            new int[]
            {
                50,
                100,
                50
            },
            new int[]
            {
                100,
                200,
                100
            }
        },
    };
    public static void drawShot(float x, float y, float d, int c, float width, float height)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glRotatef(d, 0, 0, 1);
        RrGl.glColor4ub(shtClr[(c)][(0)][(0)], shtClr[(c)][(0)][(1)], shtClr[(c)][(0)][(2)], 240);
        RrGl.glDisable(GL_BLEND);
        glBegin(GL_LINES);
        glVertex3f(-width, -height, 0);
        glVertex3f(-width, height, 0);
        glVertex3f(width, -height, 0);
        glVertex3f(width, height, 0);
        glEnd();
        RrGl.glEnable(GL_BLEND);
        RrGl.glColor4ub(shtClr[(c)][(1)][(0)], shtClr[(c)][(1)][(1)], shtClr[(c)][(1)][(2)], 240);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-width, -height, 0);
        glVertex3f(width, -height, 0);
        RrGl.glColor4ub(shtClr[(c)][(2)][(0)], shtClr[(c)][(2)][(1)], shtClr[(c)][(2)][(2)], 240);
        glVertex3f(width, height, 0);
        glVertex3f(-width, height, 0);
        glEnd();
        glPopMatrix();
    }

    public static void startDrawBoards()
    {
        glPushMatrix();
        LoadIdentity();
        Drawing.ortho = true;
    }

    public static void endDrawBoards()
    {
        glPopMatrix();
        Drawing.ortho = false;
    }

    public static void drawBoard(int x, int y, int width, int height)
    {
        RrGl.glColor4ub(0, 0, 0, 255);
        glBegin(GL_QUADS);
        glVertex2f(x, y);
        glVertex2f(x + width, y);
        glVertex2f(x + width, y + height);
        glVertex2f(x, y + height);
        glEnd();
    }

    public static void drawSideBoards()
    {
        RrGl.glDisable(GL_BLEND);
        drawBoard(0, 0, 160, 480);
        drawBoard(480, 0, 160, 480);
        RrGl.glEnable(GL_BLEND);
        drawScore();
        drawRPanel();
    }

    public static void drawTitleBoard()
    {
        RrGl.glEnable(GL_TEXTURE_2D);
        RrGl.glBindTexture(GL_TEXTURE_2D, titleTexture);
        RrGl.glColor4ub(255, 255, 255, 255);
        glBegin(GL_TRIANGLE_FAN);
        RrGl.glTexCoord2f(0.0f, 0.0f);
        glVertex3f(350, 78, 0);
        RrGl.glTexCoord2f(1.0f, 0.0f);
        glVertex3f(470, 78, 0);
        RrGl.glTexCoord2f(1.0f, 1.0f);
        glVertex3f(470, 114, 0);
        RrGl.glTexCoord2f(0.0f, 1.0f);
        glVertex3f(350, 114, 0);
        glEnd();
        RrGl.glDisable(GL_TEXTURE_2D);
        RrGl.glColor4ub(200, 200, 200, 255);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(350, 30, 0);
        glVertex3f(400, 30, 0);
        glVertex3f(380, 56, 0);
        glVertex3f(380, 80, 0);
        glVertex3f(350, 80, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(404, 80, 0);
        glVertex3f(404, 8, 0);
        glVertex3f(440, 8, 0);
        glVertex3f(440, 44, 0);
        glVertex3f(465, 80, 0);
        glEnd();
        RrGl.glColor4ub(255, 255, 255, 255);
        glBegin(GL_LINE_LOOP);
        glVertex3f(350, 30, 0);
        glVertex3f(400, 30, 0);
        glVertex3f(380, 56, 0);
        glVertex3f(380, 80, 0);
        glVertex3f(350, 80, 0);
        glEnd();
        glBegin(GL_LINE_LOOP);
        glVertex3f(404, 80, 0);
        glVertex3f(404, 8, 0);
        glVertex3f(440, 8, 0);
        glVertex3f(440, 44, 0);
        glVertex3f(465, 80, 0);
        glEnd();
    }

    public static int drawNum(int n, int x, int y, int s, int r, int g, int b)
    {
        for (;;)
        {
            drawLetter(n % 10, x, y, s, 3, r, g, b);
            y = GameMath.integer(y + (s * 1.7f));
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }

        return y;
    }

    public static int drawNumRight(int n, int x, int y, int s, int r, int g, int b)
    {
        int d = 0, nd = 0, drawn = 0;
        {
            d = 100000000;
            for (; d > 0; d = GameMath.integer(d / (10)))
            {
                nd = GameMath.integer((GameMath.integer(n / d)));
                if ((nd > 0) || (((drawn) != 0)))
                {
                    n = n - (d * nd);
                    drawLetter(nd % 10, x, y, s, 1, r, g, b);
                    y = GameMath.integer(y + (s * 1.7f));
                    drawn = 1;
                }
            }
        }

        if (!(((drawn) != 0)))
        {
            drawLetter(0, x, y, s, 1, r, g, b);
            y = GameMath.integer(y + (s * 1.7f));
        }

        return y;
    }

    public static int drawNumCenter(int n, int x, int y, int s, int r, int g, int b)
    {
        for (;;)
        {
            drawLetter(n % 10, x, y, s, 0, r, g, b);
            x = GameMath.integer(x - (s * 1.7f));
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }

        return y;
    }

    public static int drawTimeCenter(int n, int x, int y, int s, int r, int g, int b)
    {
        int i = 0;
        {
            i = 0;
            for (; i < 7; i++)
            {
                if (i != 4)
                {
                    drawLetter(n % 10, x, y, s, 0, r, g, b);
                    n = GameMath.integer(n / (10));
                }
                else
                {
                    drawLetter(n % 6, x, y, s, 0, r, g, b);
                    n = GameMath.integer(n / (6));
                }

                if (((i & 1) == 1) || (i == 0))
                {
                    switch (i)
                    {
                        case 3:
                            drawLetter(41, GameMath.integer(x + s * 1.16f), y, s, 0, r, g, b);
                            break;
                        case 5:
                            drawLetter(40, GameMath.integer(x + s * 1.16f), y, s, 0, r, g, b);
                            break;
                    }

                    x = GameMath.integer(x - (s * 1.7f));
                }
                else
                {
                    x = GameMath.integer(x - (s * 2.2f));
                }

                if (n <= 0)
                    break;
            }
        }

        return y;
    }
}
