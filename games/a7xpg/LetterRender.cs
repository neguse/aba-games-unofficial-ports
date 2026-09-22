// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;

public class LetterRender
{
    public static int displayListIdx;
    public static void drawLetter(int n, float x, float y, float s)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, s, s);
        glCallList(displayListIdx + n);
        glPopMatrix();
    }

    public static void drawLetterReverse(int n, float x, float y, float s)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, -s, s);
        glCallList(displayListIdx + n);
        glPopMatrix();
    }

    public static void drawString(string str, float lx, float y, float s)
    {
        float x = lx;
        for (int i = 0; i < str.Length; i++)
        {
            string c = str.Substring(i, 1);
            if (c != " ")
            {
                int idx = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+".IndexOf(c);
                if (idx < 0)
                {
                    idx = "abcdefghijklmnopqrstuvwxyz".IndexOf(c);
                    idx = idx < 0 ? 37 : idx + 10;
                }

                drawLetter(idx, x, y, s);
            }

            x = x + (s * 1.7f);
        }
    }

    public static void drawNum(int num, float lx, float y, float s)
    {
        int n = num;
        float x = lx;
        for (;;)
        {
            drawLetter(n % 10, x, y, s);
            x = x - (s * 1.7f);
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }
    }

    public static void drawNumReverse(int num, float lx, float y, float s)
    {
        int n = num;
        float x = lx;
        for (;;)
        {
            drawLetterReverse(n % 10, x, y, s);
            x = x - (s * 1.7f);
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }
    }

    public static void drawTime(int time, float lx, float y, float s)
    {
        int n = time;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter(n % 10, x, y, s);
                n = GameMath.integer(n / (10));
            }
            else
            {
                drawLetter(n % 6, x, y, s);
                n = GameMath.integer(n / (6));
            }

            if ((i & 1) == 1 || i == 0)
            {
                switch (i)
                {
                    case 3:
                        drawLetter(41, x + s * 1.16f, y, s);
                        break;
                    case 5:
                        drawLetter(40, x + s * 1.16f, y, s);
                        break;
                    default:
                        break;
                }

                x = x - (s * 1.7f);
            }
            else
            {
                x = x - (s * 2.2f);
            }

            if (n <= 0)
                break;
        }
    }

    public static void drawBox(float x, float y, float width, float height)
    {
        A7xScreen.setColor(1, 1, 1, 0.5f);
        A7xScreen.drawBoxSolid(x - width, y - height, width * 2, height * 2);
        A7xScreen.setColor(1, 1, 1, 1);
        A7xScreen.drawBoxLine(x - width, y - height, width * 2, height * 2);
    }

    public static void createLetter(int idx)
    {
        int i = 0;
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        int deg = 0;
        for (i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            size = size * (0.66f);
            length = length * (0.6f);
            x = -x;
            y = y;
            deg = deg % (180);
            if (deg <= 45 || deg > 135)
                drawBox(x, y, size, length);
            else
                drawBox(x, y, length, size);
        }
    }

    public static void createDisplayLists()
    {
        displayListIdx = glGenLists(42);
        for (int i = 0; i < 42; i++)
        {
            glNewList(displayListIdx + i, GL_COMPILE);
            createLetter(i);
            glEndList();
        }
    }

    public static void deleteDisplayLists()
    {
        glDeleteLists(displayListIdx, 39);
    }

    public static float[][][] spData = new float[][][]
    {
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.6f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.6f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.6f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.1f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.1f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.4f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.25f, 0, 0.25f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.75f, 0.25f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.3f, 1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0.3f, 1.15f, 0.25f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.2f, -0.6f, 0.45f, 0.3f, 360 - 300 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.1f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0.4f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.5f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.3f, -1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0.3f, -1.15f, 0.25f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 360 - 120 },
            new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 360 - 60 },
            new float[] { -0.4f, -0.6f, 0.85f, 0.3f, 360 - 240 },
            new float[] { 0.4f, -0.6f, 0.85f, 0.3f, 360 - 300 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 360 - 120 },
            new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 360 - 60 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.35f, 0.5f, 0.65f, 0.3f, 360 - 60 },
            new float[] { -0.35f, -0.5f, 0.65f, 0.3f, 360 - 240 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.05f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.4f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0.4f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.19f, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0.2f, 1.0f, 0.4f, 0.2f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        }
    };
}
