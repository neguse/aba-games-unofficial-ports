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
    public const int DISPLAY_LIST_NUM = LETTER_NUM;
    public static void init_0()
    {
        displayList = new DisplayList(DISPLAY_LIST_NUM);
        displayList.resetList();
        for (int i = 0; i < LETTER_NUM; i++)
        {
            displayList.newList();
            setLetter(i);
            displayList.endList();
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
            n = integer(n / 10);
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

    public static int convertCharToInt(string c){
        int idx="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+??!/".IndexOf(c);
        if(idx>=0)return idx;
        idx="abcdefghijklmnopqrstuvwxyz".IndexOf(c);
        return idx>=0?idx+10:0;
    }

    public static void drawString(string str, float lx, float y, float s, int d = LetterDirection.TO_RIGHT, bool rev = false, float od = 0)
    {
        lx += LETTER_WIDTH * s / 2;
        y += LETTER_HEIGHT * s / 2;
        float x = lx;
        int idx = default(int);
        float ld = default(float);
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

        ld += od;
        for(int character=0;character<str.Length;character++)
        {
            string c=str.Substring(character,1);
            if (c != " ")
            {
                idx = convertCharToInt(c);
                if (rev)
                    drawLetterRev(idx, x, y, s, ld);
                else
                    drawLetter_5(idx, x, y, s, ld);
            }

            if (od == 0)
            {
                switch (d)
                {
                    case LetterDirection.TO_RIGHT:
                        x += s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_DOWN:
                        y += s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_LEFT:
                        x -= s * LETTER_WIDTH;
                        break;
                    case LetterDirection.TO_UP:
                        y -= s * LETTER_WIDTH;
                        break;
                }
            }
            else
            {
                x += cos(ld * PI / 180) * s * LETTER_WIDTH;
                y += sin(ld * PI / 180) * s * LETTER_WIDTH;
            }
        }
    }

    public static void drawNum(int num, float lx, float y, float s, int dg = 0, int headChar = -1, int floatDigit = -1)
    {
        lx += LETTER_WIDTH * s / 2;
        y += LETTER_HEIGHT * s / 2;
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
                x -= s * LETTER_WIDTH;
            }
            else
            {
                drawLetter_5(n % 10, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x -= s * LETTER_WIDTH * 0.5f;
            }

            n = integer(n / 10);
            digit--;
            fd--;
            if (n <= 0 && digit <= 0 && fd < 0)
                break;
            if (fd == 0)
            {
                drawLetter_5(36, x, y + s * LETTER_WIDTH * 0.25f, s * 0.5f, ld);
                x -= s * LETTER_WIDTH * 0.5f;
            }
        }

        if (headChar >= 0)
            drawLetter_5(headChar, x + s * LETTER_WIDTH * 0.2f, y + s * LETTER_WIDTH * 0.2f, s * 0.6f, ld);
    }

    public static void drawNumSign(int num, float lx, float ly, float s, int headChar = -1, int floatDigit = -1)
    {
        float x = lx;
        float y = ly;
        int n = num;
        int fd = floatDigit;
        for (;;)
        {
            if (fd <= 0)
            {
                drawLetterRev(n % 10, x, y, s, 0);
                x -= s * LETTER_WIDTH;
            }
            else
            {
                drawLetterRev(n % 10, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x -= s * LETTER_WIDTH * 0.5f;
            }

            n = integer(n / 10);
            if (n <= 0)
                break;
            fd--;
            if (fd == 0)
            {
                drawLetterRev(36, x, y - s * LETTER_WIDTH * 0.25f, s * 0.5f, 0);
                x -= s * LETTER_WIDTH * 0.5f;
            }
        }

        if (headChar >= 0)
            drawLetterRev(headChar, x + s * LETTER_WIDTH * 0.2f, y - s * LETTER_WIDTH * 0.2f, s * 0.6f, 0);
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
                n = integer(n / 10);
            }
            else
            {
                drawLetter_5(n % 6, x, y, s, LetterDirection.TO_RIGHT);
                n = integer(n / 6);
            }

            if ((i & 1) == 1 || i == 0)
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

                x -= s * LETTER_WIDTH;
            }
            else
            {
                x -= s * LETTER_WIDTH * 1.3f;
            }

            if (n <= 0)
                break;
        }
    }

    public static void setLetter(int idx)
    {
        float x = default(float), y = default(float), length = default(float), size = default(float), t = default(float);
        float deg = default(float);
        for (int i = 0;; i++)
        {
            deg = GameMath.integer(spData[idx][i][4]);
            if (deg > 99990)
                break;
            x = -spData[idx][i][0];
            y = -spData[idx][i][1];
            size = spData[idx][i][2];
            length = spData[idx][i][3];
            y *= 0.9f;
            size *= 1.4f;
            length *= 1.05f;
            x = -x;
            y = y;
            deg %= 180;
            drawSegment(x, y, size, length, deg);
        }
    }

    public static void drawSegment(float x, float y, float width, float height, float deg)
    {
        glPushMatrix();
        glTranslatef(x - width / 2, y, 0);
        glRotatef(deg, 0, 0, 1);
        Screen.setColor(1, 1, 1, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        drawSegmentPart(width, height);
        glEnd();
        Screen.setColor(1, 1, 1);
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

    public static float[][][] spData = McdData.Letters();
}

public static class LetterDirection
{
    public const int TO_RIGHT = 0;
    public const int TO_DOWN = 1;
    public const int TO_LEFT = 2;
    public const int TO_UP = 3;
}
