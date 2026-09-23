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

public static class NrLetter
{
    public static void drawLetterBuf(int idx, int lx, int ly, int ltSize, int d, int color1, int color2, PixelLayer buf, int panel)
    {
        int i = 0;
        float x = 0, y = 0, length = 0, size = 0, t = 0;
        int deg = 0;
        {
            i = 0;
            for (;; i++)
            {
                deg = GameMath.integer(NrData.letters[(idx)][(i)][(4)]);
                if (deg > 99990)
                    break;
                x = NrData.letters[(idx)][(i)][(0)];
                y = NrData.letters[(idx)][(i)][(1)];
                size = NrData.letters[(idx)][(i)][(2)];
                length = NrData.letters[(idx)][(i)][(3)];
                size = size * (1.3f);
                length = length * (1.1f);
                switch (d)
                {
                    case 0:
                        x = -x;
                        y = y;
                        break;
                    case 1:
                        t = x;
                        x = -y;
                        y = -t;
                        deg = deg + (90);
                        break;
                    case 2:
                        x = x;
                        y = -y;
                        deg = deg + (180);
                        break;
                    case 3:
                        t = x;
                        x = y;
                        y = t;
                        deg = deg + (270);
                        break;
                }

                deg = deg % (180);
                if (((panel) != 0))
                {
                    if ((deg < 45) || (deg > 135))
                    {
                        drawBoxPanel(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((size * ltSize)), GameMath.integer((length * ltSize)), color1, color2, buf);
                    }
                    else
                    {
                        drawBoxPanel(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((length * ltSize)), GameMath.integer((size * ltSize)), color1, color2, buf);
                    }
                }
                else
                {
                    if ((deg <= 45) || (deg > 135))
                    {
                        drawBox(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((size * ltSize)), GameMath.integer((length * ltSize)), color1, color2, buf);
                    }
                    else
                    {
                        drawBox(GameMath.integer((x * ltSize)) + lx, GameMath.integer((y * ltSize)) + ly, GameMath.integer((length * ltSize)), GameMath.integer((size * ltSize)), color1, color2, buf);
                    }
                }
            }
        }
    }

    public static void drawLetter(int idx, int lx, int ly, int ltSize, int d, int color1, int color2, PixelLayer buf)
    {
        drawLetterBuf(idx, lx, ly, ltSize, d, color1, color2, buf, 1);
    }

    public static void drawStringBuf(string str, int lx, int ly, int ltSize, int d, int color1, int color2, PixelLayer buf, int panel)
    {
        int x = lx, y = ly;
        int i = 0, c = 0, idx = 0;
        {
            i = 0;
            for (;; i++)
            {
                if (i >= str.Length)
                    break;
                string letter = str.Substring(i, 1);
                if (letter != " ")
                {
                    idx = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(letter.ToUpper());
                    if (idx < 0)
                    {
                        if (letter == ".")
                            idx = 36;
                        else if (letter == "-")
                            idx = 38;
                        else if (letter == "+")
                            idx = 39;
                        else
                            idx = 37;
                    }

                    drawLetterBuf(idx, x, y, ltSize, d, color1, color2, buf, panel);
                }

                switch (d)
                {
                    case 0:
                        x = GameMath.integer(x - (ltSize * 1.7f));
                        break;
                    case 1:
                        y = GameMath.integer(y - (ltSize * 1.7f));
                        break;
                    case 2:
                        x = GameMath.integer(x + (ltSize * 1.7f));
                        break;
                    case 3:
                        y = GameMath.integer(y + (ltSize * 1.7f));
                        break;
                }
            }
        }
    }

    public static void drawString(string str, int lx, int ly, int ltSize, int d, int color1, int color2, PixelLayer buf)
    {
        drawStringBuf(str, lx, ly, ltSize, d, color1, color2, buf, 1);
    }
}
