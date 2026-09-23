// Copyright 2002 Kenta Cho. All rights reserved.
using static NrConstants;
using static NrRandom;
using static NrLetter;

public static class NrScreen
{
    public static PixelLayer l1buf = new PixelLayer(320), l2buf = new PixelLayer(320), buf = new PixelLayer(320), lpbuf = new PixelLayer(160), rpbuf = new PixelLayer(160), video = new PixelLayer(640);
    public const int pitch = 320;
    public static int input;
    public static bool quitRequested;
    public static int getPadState()
    {
        return input & 15;
    }

    public static int getButtonState()
    {
        return input & 48;
    }

    public static void quitLast()
    {
        NrPreference.savePreference();
        quitRequested = true;
    }

    public static void clearScreen()
    {
        buf.Clear();
    }

    public static void clearLPanel()
    {
        lpbuf.Clear();
    }

    public static void clearRPanel()
    {
        rpbuf.Clear();
    }

    public static void blendScreen()
    {
    }

    public static void drawSprite(int n, int x, int y)
    {
        video.Rect(x, y, 40, 40, 0, n * 40, 40, 40, 5);
    }

    public static void drawLine(int x1, int y1, int x2, int y2, int color, int width, PixelLayer buf)
    {
        int lx = 0, ly = 0, ax = 0, ay = 0, x = 0, y = 0, ptr = 0, i = 0, j = 0;
        int xMax = 0, yMax = 0;
        lx = absN(x2 - x1);
        ly = absN(y2 - y1);
        if (lx < ly)
        {
            x1 = x1 - (GameMath.signedShift(width, 1));
            x2 = x2 - (GameMath.signedShift(width, 1));
        }
        else
        {
            y1 = y1 - (GameMath.signedShift(width, 1));
            y2 = y2 - (GameMath.signedShift(width, 1));
        }

        xMax = LAYER_WIDTH - width - 1;
        yMax = LAYER_HEIGHT - width - 1;
        if (x1 < 0)
        {
            if (x2 < 0)
                return;
            y1 = (y1 - y2) * x2 / (x2 - x1) + y2;
            x1 = 0;
        }
        else if (x2 < 0)
        {
            y2 = (y2 - y1) * x1 / (x1 - x2) + y1;
            x2 = 0;
        }

        if (x1 > xMax)
        {
            if (x2 > xMax)
                return;
            y1 = (y1 - y2) * (x2 - xMax) / (x2 - x1) + y2;
            x1 = xMax;
        }
        else if (x2 > xMax)
        {
            y2 = (y2 - y1) * (x1 - xMax) / (x1 - x2) + y1;
            x2 = xMax;
        }

        if (y1 < 0)
        {
            if (y2 < 0)
                return;
            x1 = (x1 - x2) * y2 / (y2 - y1) + x2;
            y1 = 0;
        }
        else if (y2 < 0)
        {
            x2 = (x2 - x1) * y1 / (y1 - y2) + x1;
            y2 = 0;
        }

        if (y1 > yMax)
        {
            if (y2 > yMax)
                return;
            x1 = (x1 - x2) * (y2 - yMax) / (y2 - y1) + x2;
            y1 = yMax;
        }
        else if (y2 > yMax)
        {
            x2 = (x2 - x1) * (y1 - yMax) / (y1 - y2) + x1;
            y2 = yMax;
        }

        lx = absN(x2 - x1);
        ly = absN(y2 - y1);
        if (lx < ly)
        {
            if (ly == 0)
                ly++;
            ax = ((x2 - x1) << 8) / ly;
            ay = (GameMath.signedShift((y2 - y1), 8)) | 1;
            x = x1 << 8;
            y = y1;
            {
                i = ly;
                for (; i > 0; i--, x = x + (ax), y = y + (ay))
                {
                    ptr = y * pitch + (GameMath.signedShift(x, 8));
                    {
                        j = width;
                        for (; j > 0; j--, ptr++)
                        {
                            buf.Set(ptr, color);
                        }
                    }
                }
            }
        }
        else
        {
            if (lx == 0)
                lx++;
            ay = ((y2 - y1) << 8) / lx;
            ax = (GameMath.signedShift((x2 - x1), 8)) | 1;
            x = x1;
            y = y1 << 8;
            {
                i = lx;
                for (; i > 0; i--, x = x + (ax), y = y + (ay))
                {
                    ptr = (GameMath.signedShift(y, 8)) * pitch + x;
                    {
                        j = width;
                        for (; j > 0; j--, ptr = ptr + (pitch))
                        {
                            buf.Set(ptr, color);
                        }
                    }
                }
            }
        }
    }

    public static void drawThickLine(int x1, int y1, int x2, int y2, int color1, int color2, int width)
    {
        int lx = 0, ly = 0, ax = 0, ay = 0, x = 0, y = 0, ptr = 0, i = 0, j = 0;
        int xMax = 0, yMax = 0;
        int width1 = 0, width2 = 0;
        lx = absN(x2 - x1);
        ly = absN(y2 - y1);
        if (lx < ly)
        {
            x1 = x1 - (GameMath.signedShift(width, 1));
            x2 = x2 - (GameMath.signedShift(width, 1));
        }
        else
        {
            y1 = y1 - (GameMath.signedShift(width, 1));
            y2 = y2 - (GameMath.signedShift(width, 1));
        }

        xMax = LAYER_WIDTH - width;
        yMax = LAYER_HEIGHT - width;
        if (x1 < 0)
        {
            if (x2 < 0)
                return;
            y1 = (y1 - y2) * x2 / (x2 - x1) + y2;
            x1 = 0;
        }
        else if (x2 < 0)
        {
            y2 = (y2 - y1) * x1 / (x1 - x2) + y1;
            x2 = 0;
        }

        if (x1 > xMax)
        {
            if (x2 > xMax)
                return;
            y1 = (y1 - y2) * (x2 - xMax) / (x2 - x1) + y2;
            x1 = xMax;
        }
        else if (x2 > xMax)
        {
            y2 = (y2 - y1) * (x1 - xMax) / (x1 - x2) + y1;
            x2 = xMax;
        }

        if (y1 < 0)
        {
            if (y2 < 0)
                return;
            x1 = (x1 - x2) * y2 / (y2 - y1) + x2;
            y1 = 0;
        }
        else if (y2 < 0)
        {
            x2 = (x2 - x1) * y1 / (y1 - y2) + x1;
            y2 = 0;
        }

        if (y1 > yMax)
        {
            if (y2 > yMax)
                return;
            x1 = (x1 - x2) * (y2 - yMax) / (y2 - y1) + x2;
            y1 = yMax;
        }
        else if (y2 > yMax)
        {
            x2 = (x2 - x1) * (y1 - yMax) / (y1 - y2) + x1;
            y2 = yMax;
        }

        lx = absN(x2 - x1);
        ly = absN(y2 - y1);
        width1 = width - 2;
        if (lx < ly)
        {
            if (ly == 0)
                ly++;
            ax = ((x2 - x1) << 8) / ly;
            ay = (GameMath.signedShift((y2 - y1), 8)) | 1;
            x = x1 << 8;
            y = y1;
            ptr = y * pitch + (GameMath.signedShift(x, 8)) + 1;
            buf.Fill(ptr, color2, width1);
            x = x + (ax);
            y = y + (ay);
            {
                i = ly - 1;
                for (; i > 1; i--, x = x + (ax), y = y + (ay))
                {
                    ptr = y * pitch + (GameMath.signedShift(x, 8));
                    buf.Set(ptr, color2);
                    ptr++;
                    buf.Fill(ptr, color1, width1);
                    ptr = ptr + (width1);
                    buf.Set(ptr, color2);
                }
            }

            ptr = y * pitch + (GameMath.signedShift(x, 8)) + 1;
            buf.Fill(ptr, color2, width1);
        }
        else
        {
            if (lx == 0)
                lx++;
            ay = ((y2 - y1) << 8) / lx;
            ax = (GameMath.signedShift((x2 - x1), 8)) | 1;
            x = x1;
            y = y1 << 8;
            ptr = ((GameMath.signedShift(y, 8)) + 1) * pitch + x;
            {
                j = width1;
                for (; j > 0; j--, ptr = ptr + (pitch))
                {
                    buf.Set(ptr, color2);
                }
            }

            x = x + (ax);
            y = y + (ay);
            {
                i = lx - 1;
                for (; i > 1; i--, x = x + (ax), y = y + (ay))
                {
                    ptr = (GameMath.signedShift(y, 8)) * pitch + x;
                    buf.Set(ptr, color2);
                    ptr = ptr + (pitch);
                    {
                        j = width1;
                        for (; j > 0; j--, ptr = ptr + (pitch))
                        {
                            buf.Set(ptr, color1);
                        }
                    }

                    buf.Set(ptr, color2);
                }
            }

            ptr = ((GameMath.signedShift(y, 8)) + 1) * pitch + x;
            {
                j = width1;
                for (; j > 0; j--, ptr = ptr + (pitch))
                {
                    buf.Set(ptr, color2);
                }
            }
        }
    }

    public static void drawBox(int x, int y, int width, int height, int color1, int color2, PixelLayer target)
    {
        box(x, y, width, height, color1, color2, target, 320);
    }

    public static void drawBoxPanel(int x, int y, int width, int height, int color1, int color2, PixelLayer target)
    {
        box(x, y, width, height, color1, color2, target, 160);
    }

    static void box(int x, int y, int width, int height, int color1, int color2, PixelLayer target, int limit)
    {
        x -= GameMath.signedShift(width, 1);
        y -= GameMath.signedShift(height, 1);
        if (x < 0)
        {
            width += x;
            x = 0;
        }

        if (x + width >= limit)
            width = limit - x;
        if (width <= 1)
            return;
        if (y < 0)
        {
            height += y;
            y = 0;
        }

        if (y + height > 480)
            height = 480 - y;
        if (height <= 1)
            return;
        target.Rect(x, y, width, height, color2 & 255, 0, 0, 0, 0);
        if (width > 2 && height > 2)
            target.Rect(x + 1, y + 1, width - 2, height - 2, color1 & 255, 0, 0, 0, 0);
    }

    public static int drawNum(int n, int x, int y, int s, int c1, int c2)
    {
        for (;;)
        {
            drawLetter(n % 10, x, y, s, 1, c1, c2, lpbuf);
            y = GameMath.integer(y + (s * 1.7f));
            n = n / (10);
            if (n <= 0)
                break;
        }

        return y;
    }

    public static int drawNumRight(int n, int x, int y, int s, int c1, int c2)
    {
        int d = 0, nd = 0, drawn = 0;
        {
            d = 100000000;
            for (; d > 0; d = d / (10))
            {
                nd = GameMath.integer((n / d));
                if ((((nd > 0) || (((drawn) != 0)))))
                {
                    n = n - (d * nd);
                    drawLetter(nd % 10, x, y, s, 3, c1, c2, rpbuf);
                    y = GameMath.integer(y + (s * 1.7f));
                    drawn = 1;
                }
            }
        }

        if (((!(((drawn) != 0)))))
        {
            drawLetter(0, x, y, s, 3, c1, c2, rpbuf);
            y = GameMath.integer(y + (s * 1.7f));
        }

        return y;
    }

    public static int drawNumCenter(int n, int x, int y, int s, int c1, int c2)
    {
        for (;;)
        {
            drawLetterBuf(n % 10, x, y, s, 2, c1, c2, buf, 0);
            x = GameMath.integer(x - (s * 1.7f));
            n = n / (10);
            if (n <= 0)
                break;
        }

        return y;
    }
}
