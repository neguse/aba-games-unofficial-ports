// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Letter
{
    private static LetterData[] letters;
    public static void Load()
    {
        letters = MmData.Letters();
    }

    public static float DrawQuadListShapestringfloatfloatfloat(QuadListShape shape, string s, float x, float y, float sz)
    {
        float lsz = 0;
        for (int letterIndex = 0; letterIndex < s.Length; letterIndex++)
        {
            if (s.Substring(letterIndex, 1) != " ")
            {
                int ci = ConvertCharToInt(s.Substring(letterIndex, 1));
                lsz = 0.75f + (ci * 14992 % 50) * 0.01f;
                lsz *= sz;
                x += lsz;
                letters[(ci)].Draw(shape, x, y + lsz, lsz);
            }
            else
            {
                lsz = sz;
                x += lsz;
            }

            x += lsz;
        }

        return x;
    }

    public static float DrawQuadListShapeintfloatfloatfloat(QuadListShape shape, int n, float x, float y, float sz)
    {
        float s = 0;
        for (;;)
        {
            int ln = n % 10;
            s = 0.5f;
            if (ln > 0)
                s += 0.25f + ln * 0.05f;
            s *= sz;
            x -= s * 0.8f;
            letters[(ln)].Draw(shape, x, y + s, s);
            x -= s * 0.8f;
            n /= 10;
            if (n <= 0)
                break;
        }

        return x;
    }

    public static float DrawQuadListShapeintintfloatfloatfloat(QuadListShape shape, int n, int nn, float x, float y, float sz)
    {
        float s = 0;
        for (int i = 0; i < nn; i++)
        {
            int ln = n % 10;
            s = 0.5f;
            if (ln > 0)
                s += 0.25f + ln * 0.05f;
            s *= sz;
            x -= s * 0.8f;
            letters[(ln)].Draw(shape, x, y + s, s);
            x -= s * 0.8f;
            n /= 10;
        }

        return x;
    }

    public static float DrawFixedSize(QuadListShape shape, int n, int nn, float x, float y, float sz)
    {
        for (int i = 0; i < nn; i++)
        {
            int ln = n % 10;
            x -= sz * 0.8f;
            letters[(ln)].Draw(shape, x, y + sz, sz);
            x -= sz * 0.8f;
            n /= 10;
        }

        return x;
    }

    public static int ConvertCharToInt(string c)
    {
        int index = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+??!/".IndexOf(c.ToUpper());
        return index < 0 ? 0 : index;
    }
}

public class LetterData
{
    private float[] points;
    public LetterData(float[] data = null)
    {
        points = data;
    }

    public void Draw(QuadListShape shape, float x, float y, float sz)
    {
        for (int i = 0; i < points.Length / 8; i++)
        {
            shape.Addfloatfloatfloat(x + points[(i * 8 + 0)] * sz, y + points[(i * 8 + 1)] * sz, 0);
            shape.Addfloatfloatfloat(x + points[(i * 8 + 2)] * sz, y + points[(i * 8 + 3)] * sz, 0);
            shape.Addfloatfloatfloat(x + points[(i * 8 + 4)] * sz, y + points[(i * 8 + 5)] * sz, 0);
            shape.Addfloatfloatfloat(x + points[(i * 8 + 6)] * sz, y + points[(i * 8 + 7)] * sz, 0);
        }
    }

    public int BarNum
    {
        get
        {
            return points.Length / 8;
        }
    }

    public LetterData Copy()
    {
        return new LetterData
        {
            points = points
        };
    }
}
