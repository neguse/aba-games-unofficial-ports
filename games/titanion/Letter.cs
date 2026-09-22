// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Letter
{
    public static DisplayList displayList;
    public const float LETTER_WIDTH = 2.1f;
    public const float LETTER_HEIGHT = 3.0f;
    public const int LETTER_NUM = 44;
    public const int DISPLAY_LIST_NUM = LETTER_NUM * 3;
    public static void init_0()
    {
        displayList = new DisplayList(DISPLAY_LIST_NUM);
        displayList.resetList();
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < LETTER_NUM; i++)
            {
                displayList.newList();
                setLetter(i, j);
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

    public static float getWidthNum(int num, float s)
    {
        int dg = 1;
        int n = num;
        int c = 1;
        for (;;)
        {
            if (n < 10)
                break;
            n = GameMath.integer(n / (10));
            c++;
        }

        return c * s * LETTER_WIDTH;
    }

    public static float getHeight(float s)
    {
        return s * LETTER_HEIGHT;
    }

    public static void drawLetter_1(int n)
    {
        displayList.call(n);
    }

    public static void drawLetter_5(int n, float x, float y, float s, float d)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, s, s);
        glRotatef(d, 0, 0, 1);
        displayList.call(n);
        glPopMatrix();
    }

    public static void drawLetterRev(int n, float x, float y, float s, float d)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glScalef(s, -s, s);
        glRotatef(d, 0, 0, 1);
        displayList.call(n);
        glPopMatrix();
    }

    public static int convertCharToInt(string c)
    {
        return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+  !/".IndexOf(c.ToUpper());
    }

    public static void drawString(string str, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, bool rev = false, float od = 0, float r = 1, float g = 1, float b = 1)
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
        for (int ci = 0; ci < str.Length; ci++)
        {
            if (str.Substring(ci, 1) != " ")
            {
                idx = convertCharToInt(str.Substring(ci, 1));
                if (((((r == 1)) && ((g == 1)))) && ((b == 1)))
                {
                    if (rev)
                        drawLetterRev(idx, x, y, s, ld);
                    else
                        drawLetter_5(idx, x, y, s, ld);
                }
                else
                {
                    TtnScreen.setColor(r, g, b, 0.5f);
                    if (rev)
                        drawLetterRev(idx + LETTER_NUM, x, y, s, ld);
                    else
                        drawLetter_5(idx + LETTER_NUM, x, y, s, ld);
                    TtnScreen.setColor(r, g, b);
                    if (rev)
                        drawLetterRev(idx + LETTER_NUM * 2, x, y, s, ld);
                    else
                        drawLetter_5(idx + LETTER_NUM * 2, x, y, s, ld);
                }
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

    public static void drawNum(int num, float lx, float y, float s, int dg = 0, int headChar = -1, int floatDigit = -1)
    {
        lx = lx + (LETTER_WIDTH * s / 2);
        y = y + (LETTER_HEIGHT * s / 2);
        int n = num;
        float x = lx;
        float ld = 0;
        int digit = dg;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetter_5(n % 10, x, y, s, ld);
                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                drawLetter_5(n % 10, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }

            n = GameMath.integer(n / (10));
            digit--;
            fd--;
            if (((((n <= 0)) && ((digit <= 0)))) && ((fd < 0)))
                break;
            if (fd == 0)
            {
                drawLetter_5(36, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }
        }

        if (headChar >= 0)
            drawLetter_5(headChar, x + s * LETTER_WIDTH * 0.2f, y + s * LETTER_WIDTH * 0.2f, s * 0.6f, ld);
    }

    public static void drawNumSign(int num, float lx, float ly, float s, int headChar = -1, int floatDigit = -1, int type = 0)
    {
        float x = lx;
        float y = ly;
        int n = num;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetterRev(n % 10 + type * LETTER_NUM, x, y, s, 0);
                x = x - (s * LETTER_WIDTH);
            }
            else
            {
                drawLetterRev(n % 10 + type * LETTER_NUM, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }

            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
            fd--;
            if (fd == 0)
            {
                drawLetterRev(36 + type * LETTER_NUM, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x = x - (s * LETTER_WIDTH * 0.5f);
            }
        }

        if (headChar >= 0)
            drawLetterRev(headChar + type * LETTER_NUM, x + s * LETTER_WIDTH * 0.2f, y - s * LETTER_WIDTH * 0.2f, s * 0.6f, 0);
    }

    public static void drawTime(int time, float lx, float y, float s)
    {
        int n = time;
        if (n < 0)
            n = 0;
        float x = lx;
        for (int i = 0; i < 7; i++)
        {
            if (i != 4)
            {
                drawLetter_5(n % 10, x, y, s, LetterDirection.TO_RIGHT);
                n = GameMath.integer(n / (10));
            }
            else
            {
                drawLetter_5(n % 6, x, y, s, LetterDirection.TO_RIGHT);
                n = GameMath.integer(n / (6));
            }

            if ((((i & 1) == 1)) || ((i == 0)))
            {
                switch (i)
                {
                    case 3:
                        drawLetter_5(41, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
                        break;
                    case 5:
                        drawLetter_5(40, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT);
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

    public static void setLetter(int idx, int type = LetterShape.NORMAL)
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
            switch (type)
            {
                case LetterShape.NORMAL:
                    drawSegment(x, y, size, length, deg);
                    break;
                case LetterShape.POLYGON:
                    drawSegmentPolygon(x, y, size, length, deg);
                    break;
                case LetterShape.LINE:
                    drawSegmentLine(x, y, size, length, deg);
                    break;
            }
        }
    }

    public static void drawSegment(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y, 0);
        glRotatef(deg, 0, 0, 1);
        TtnScreen.setColor(1, 1, 1, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        drawSegmentPart(width, height);
        glEnd();
        TtnScreen.setColor(1, 1, 1);
        glBegin(GL_LINE_LOOP);
        drawSegmentPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawSegmentPolygon(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y, 0);
        glRotatef(deg, 0, 0, 1);
        glBegin(GL_TRIANGLE_FAN);
        drawSegmentPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawSegmentLine(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y, 0);
        glRotatef(deg, 0, 0, 1);
        glBegin(GL_LINE_LOOP);
        drawSegmentPart(width, height);
        glEnd();
        glPopMatrix();
    }

    public static void drawSegmentPart(float width, float height)
    {
        glVertex3f(-width / 2, 0, 0);
        glVertex3f(-width / 3 * 1, -height / 2, 0);
        glVertex3f(width / 3 * 1, -height / 2, 0);
        glVertex3f(width / 2, 0, 0);
        glVertex3f(width / 3 * 1, height / 2, 0);
        glVertex3f(-width / 3 * 1, height / 2, 0);
    }

    public static float[][][] spData = new float[][][] { 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.6f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.6f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.5f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.18f, 1.15f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.18f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.15f, 1.15f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, 0.45f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.05f, 0f, 0.3f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.7f, -0.7f, 0.3f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.4f, 0.55f, 0.65f, 0.3f, 100f }, new float[] { -0.25f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.6f, -0.55f, 0.65f, 0.3f, 80f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 1.15f, 0.3f, 0.3f, 0f }, new float[] { 0.1f, 1.15f, 0.3f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.05f, -0.55f, 0.45f, 0.3f, 60f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.2f, 0f, 0.45f, 0.3f, 0f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.45f, -0.55f, 0.65f, 0.3f, 80f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0.65f, 0.3f, 0f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 1.15f, 0.55f, 0.3f, 0f }, new float[] { 0.5f, 1.15f, 0.55f, 0.3f, 0f }, new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.5f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.1f, -1.15f, 0.45f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.65f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { -0.5f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0.1f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 240f }, new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 300f }, new float[] { -0.4f, -0.6f, 0.85f, 0.3f, 120f }, new float[] { 0.4f, -0.6f, 0.85f, 0.3f, 60f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.4f, 0.6f, 0.85f, 0.3f, 240f }, new float[] { 0.4f, 0.6f, 0.85f, 0.3f, 300f }, new float[] { -0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.15f, 0.65f, 0.3f, 0f }, new float[] { 0.3f, 0.4f, 0.65f, 0.3f, 120f }, new float[] { -0.3f, -0.4f, 0.65f, 0.3f, 120f }, new float[] { 0f, -1.15f, 0.65f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, -1.15f, 0.3f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, -1.15f, 0.8f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 0f, 0.9f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.5f, 0f, 0.45f, 0.3f, 0f }, new float[] { 0.45f, 0f, 0.45f, 0.3f, 0f }, new float[] { 0.1f, 0.55f, 0.65f, 0.3f, 90f }, new float[] { 0.1f, -0.55f, 0.65f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { -0.19f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0.2f, 1.0f, 0.4f, 0.2f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.56f, 0.25f, 1.1f, 0.3f, 90f }, new float[] { 0f, -1.0f, 0.3f, 0.3f, 90f }, new float[] { 0f, 0f, 0f, 0f, 99999f } }, 
        new float[][] { new float[] { 0.8f, 0f, 1.75f, 0.3f, 120f }, new float[] { 0f, 0f, 0f, 0f, 99999f } } };

}

public static class LetterDirection
{
    public const int TO_RIGHT = 0;
    public const int TO_DOWN = 1;
    public const int TO_LEFT = 2;
    public const int TO_UP = 3;
}

public static class LetterShape
{
    public const int NORMAL = 0;
    public const int POLYGON = 1;
    public const int LINE = 2;
}
