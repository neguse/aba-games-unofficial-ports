// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Letter
{
    public static DisplayList displayList;
    public const float LETTER_WIDTH = 2.1f;
    public const float LETTER_HEIGHT = 3.0f;
    public const int COLOR_NUM = 4;
    public static float[][] COLOR_RGB = new float[][]
    {
        new float[] { 1, 1, 1 },
        new float[] { 0.9f, 0.7f, 0.5f }
    };
    public const int LETTER_NUM = 44;
    public const int DISPLAY_LIST_NUM = LETTER_NUM * COLOR_NUM;
    public static void init_0()
    {
        displayList = new DisplayList(DISPLAY_LIST_NUM);
        displayList.resetList();
        for (int j = 0; j < COLOR_NUM; j++)
        {
            for (int i = 0; i < LETTER_NUM; i++)
            {
                displayList.newList();
                drawLetter_2(i, j);
                displayList.endList();
            }
        }
    }

    public static void close()
    {
        displayList.close();
    }

    public static float getWidth(int n, float s)
    {
        return n * s * LETTER_WIDTH;
    }

    public static float getHeight(float s)
    {
        return s * LETTER_HEIGHT;
    }

    public static void drawLetter_6(int n, float x, float y, float s, float d, int c)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, s, s);
        glRotatef(d, 0, 0, 1);
        displayList.call(n + c * LETTER_NUM);
        glPopMatrix();
    }

    public static void drawLetterRev(int n, float x, float y, float s, float d, int c)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, -s, s);
        glRotatef(d, 0, 0, 1);
        displayList.call(n + c * LETTER_NUM);
        glPopMatrix();
    }

    public static int convertCharToInt(char c)
    {
        int idx = 0;
        if ((c >= '0') && (c <= '9'))
        {
            idx = c - '0';
        }
        else if ((c >= 'A') && (c <= 'Z'))
        {
            idx = c - 'A' + 10;
        }
        else if ((c >= 'a') && (c <= 'z'))
        {
            idx = c - 'a' + 10;
        }
        else if (c == '.')
        {
            idx = 36;
        }
        else if (c == '-')
        {
            idx = 38;
        }
        else if (c == '+')
        {
            idx = 39;
        }
        else if (c == '_')
        {
            idx = 37;
        }
        else if (c == '!')
        {
            idx = 42;
        }
        else if (c == '/')
        {
            idx = 43;
        }

        return idx;
    }

    public static void drawString(string str, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, int cl = 0, bool rev = false, float od = 0)
    {
        lx = lx + (LETTER_WIDTH * s / 2);
        y = y + (LETTER_HEIGHT * s / 2);
        float x = lx;
        int idx = 0;
        float ld = 0;
        switch (d)
        {
            case LetterDirection.TO_RIGHT:
                ld = 0;
                break;
            case LetterDirection.TO_DOWN:
                ld = 90;
                break;
            case LetterDirection.TO_LEFT:
                ld = 180;
                break;
            case LetterDirection.TO_UP:
                ld = 270;
                break;
        }

        ld = ld + (od);
        foreach (char c in str)
        {
            if (c != ' ')
            {
                idx = convertCharToInt(c);
                if (rev)
                    drawLetterRev(idx, x, y, s, ld, cl);
                else
                    drawLetter_6(idx, x, y, s, ld, cl);
            }

            if (od == 0)
            {
                switch (d)
                {
                    case LetterDirection.TO_RIGHT:
                        x = x + (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_DOWN:
                        y = y + (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_LEFT:
                        x = x - (s * LETTER_WIDTH);
                        break;
                    case LetterDirection.TO_UP:
                        y = y - (s * LETTER_WIDTH);
                        break;
                }
            }
            else
            {
                x = x + (cos(ld * PI / 180) * s * LETTER_WIDTH);
                y = y + (sin(ld * PI / 180) * s * LETTER_WIDTH);
            }
        }
    }

    public static void drawNum(int num, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, int cl = 0, int dg = 0)
    {
        lx = lx + (LETTER_WIDTH * s / 2);
        y = y + (LETTER_HEIGHT * s / 2);
        int n = num;
        float x = lx;
        float ld = 0;
        switch (d)
        {
            case LetterDirection.TO_RIGHT:
                ld = 0;
                break;
            case LetterDirection.TO_DOWN:
                ld = 90;
                break;
            case LetterDirection.TO_LEFT:
                ld = 180;
                break;
            case LetterDirection.TO_UP:
                ld = 270;
                break;
        }

        int digit = dg;
        for (;;)
        {
            drawLetter_6(n % 10, x, y, s, ld, cl);
            switch (d)
            {
                case LetterDirection.TO_RIGHT:
                    x = x - (s * LETTER_WIDTH);
                    break;
                case LetterDirection.TO_DOWN:
                    y = y - (s * LETTER_WIDTH);
                    break;
                case LetterDirection.TO_LEFT:
                    x = x + (s * LETTER_WIDTH);
                    break;
                case LetterDirection.TO_UP:
                    y = y + (s * LETTER_WIDTH);
                    break;
            }

            n = GameMath.integer(n / (10));
            digit--;
            if ((n <= 0) && (digit <= 0))
                break;
        }
    }

    public static void drawNumSign(int num, float lx, float ly, float s, int cl)
    {
        float dg = 0;
        if (num < 100)
            dg = 2;
        else if (num < 1000)
            dg = 3;
        else if (num < 10000)
            dg = 4;
        else
            dg = 5;
        float x = lx + LETTER_WIDTH * s * dg / 2;
        float y = ly + LETTER_HEIGHT * s / 2;
        int n = num;
        for (;;)
        {
            drawLetterRev(n % 10, x, y, s, 0, cl);
            x = x - (s * LETTER_WIDTH);
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }
    }

    public static void drawTime(int time, float lx, float y, float s, int cl = 0)
    {
        int n = time;
        if (n < 0)
            n = 0;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter_6(n % 10, x, y, s, LetterDirection.TO_RIGHT, cl);
                n = GameMath.integer(n / (10));
            }
            else
            {
                drawLetter_6(n % 6, x, y, s, LetterDirection.TO_RIGHT, cl);
                n = GameMath.integer(n / (6));
            }

            if (((i & 1) == 1) || (i == 0))
            {
                switch (i)
                {
                    case 3:
                        drawLetter_6(41, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT, cl);
                        break;
                    case 5:
                        drawLetter_6(40, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT, cl);
                        break;
                    default:
                        break;
                }

                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                x = x - (s * LETTER_WIDTH * 1.3f);
            }

            if (n <= 0)
                break;
        }
    }

    public static void drawLetter_2(int idx, int c)
    {
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        float deg = 0;
        for (int i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            y = y * (0.9f);
            size = size * (1.4f);
            length = length * (1.05f);
            x = -x;
            y = y;
            deg = deg % (180);
            if (c == 2)
                drawBoxLine(x, y, size, length, deg);
            else if (c == 3)
                drawBoxPoly(x, y, size, length, deg);
            else
                drawBox(x, y, size, length, deg, COLOR_RGB[c][0], COLOR_RGB[c][1], COLOR_RGB[c][2]);
        }
    }

    public static void drawBox(float x, float y, float width, float height, float deg, float r, float g, float b)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y - height / 2, 0);
        glRotatef(deg, 0, 0, 1);
        TtScreen.setColor(r, g, b, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        drawBoxPart(width, height);
        glEnd();
        TtScreen.setColor(r, g, b);
        glBegin(GL_LINE_LOOP);
        drawBoxPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawBoxLine(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y - height / 2, 0);
        glRotatef(deg, 0, 0, 1);
        glBegin(GL_LINE_LOOP);
        drawBoxPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawBoxPoly(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y - height / 2, 0);
        glRotatef(deg, 0, 0, 1);
        glBegin(GL_TRIANGLE_FAN);
        drawBoxPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawBoxPart(float width, float height)
    {
        glVertex3f(-width / 2, 0, 0);
        glVertex3f(-width / 3 * 1, -height / 2, 0);
        glVertex3f(width / 3 * 1, -height / 2, 0);
        glVertex3f(width / 2, 0, 0);
        glVertex3f(width / 3 * 1, height / 2, 0);
        glVertex3f(-width / 3 * 1, height / 2, 0);
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
            new float[] { 0.5f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90 },
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
            new float[] { -0.18f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.18f, 0, 0.45f, 0.3f, 0 },
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
            new float[] { -0.15f, 1.15f, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, 0.45f, 0.65f, 0.3f, 90 },
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
            new float[] { 0.05f, 0, 0.3f, 0.3f, 0 },
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
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.7f, -0.7f, 0.3f, 0.3f, 90 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.4f, 0.55f, 0.65f, 0.3f, 100 },
            new float[] { -0.25f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 80 },
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
            new float[] { -0.5f, 1.15f, 0.3f, 0.3f, 0 },
            new float[] { 0.1f, 1.15f, 0.3f, 0.3f, 0 },
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
            new float[] { 0.05f, -0.55f, 0.45f, 0.3f, 60 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.2f, 0, 0.45f, 0.3f, 0 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.45f, -0.55f, 0.65f, 0.3f, 80 },
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
            new float[] { -0.5f, 1.15f, 0.55f, 0.3f, 0 },
            new float[] { 0.5f, 1.15f, 0.55f, 0.3f, 0 },
            new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90 },
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
            new float[] { -0.1f, -1.15f, 0.45f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { -0.5f, -1.15f, 0.3f, 0.3f, 0 },
            new float[] { 0.1f, -1.15f, 0.3f, 0.3f, 0 },
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
            new float[] { -0.1f, -0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0.3f, 0.4f, 0.65f, 0.3f, 120 },
            new float[] { -0.3f, -0.4f, 0.65f, 0.3f, 120 },
            new float[] { 0, -1.15f, 0.65f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.3f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, -1.15f, 0.8f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0, 0, 0.9f, 0.3f, 0 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { -0.5f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0.45f, 0, 0.45f, 0.3f, 0 },
            new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90 },
            new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90 },
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
        },
        new float[][]
        {
            new float[] { 0.56f, 0.25f, 1.1f, 0.3f, 90 },
            new float[] { 0, -1.0f, 0.3f, 0.3f, 90 },
            new float[] { 0, 0, 0, 0, 99999 },
        },
        new float[][]
        {
            new float[] { 0.8f, 0, 1.75f, 0.3f, 120 },
            new float[] { 0, 0, 0, 0, 99999 },
        }
    };
}

public static class LetterDirection
{
    public const int TO_RIGHT = 0, TO_DOWN = 1, TO_LEFT = 2, TO_UP = 3;
}
