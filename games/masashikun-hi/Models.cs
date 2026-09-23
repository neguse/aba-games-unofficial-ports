// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;

public static class MasArrays
{
    public static T[] Make<T>(int count, Func<T> create)
    {
        T[] values = new T[count];
        for (int i = 0; i < count; i++)
            values[i] = create();
        return values;
    }
}

public class MasPoint
{
    public int x, y;
    public MasPoint(int a, int b)
    {
        x = a;
        y = b;
    }
}

public class MasShape
{
    public MasPoint[] pd;
    public MasShape(MasPoint[] points)
    {
        pd = points;
    }
}

public class Hscdat
{
    public int rec;
    public string name = "---";
    public Hscdat Copy()
    {
        return new Hscdat
        {
            rec = rec,
            name = name
        };
    }
}

public static class MasMath
{
    public static int Seed = 1;
    public static int Div(int a, int b)
    {
        int q = Math.Abs(a) / Math.Abs(b);
        return (a < 0) != (b < 0) ? -q : q;
    }

    public static int Round(float value)
    {
        int n = (int)Math.Floor(value);
        float part = value - n;
        return part > 0.5f || (part == 0.5f && (n & 1) != 0) ? n + 1 : n;
    }

    public static float Sqrt(float value)
    {
        return (float)Math.Sqrt(value);
    }

    public static int Random(int limit)
    {
        Seed = Seed * 134775813 + 1;
        int lo = Seed & 65535, hi = (Seed >> 16) & 65535;
        int low = lo * limit;
        int carry = ((low >> 16) & 65535);
        int high = hi * limit + carry;
        return (high >> 16) & 65535;
    }

    public static int Parse(string text)
    {
        bool negative = text.StartsWith("-");
        int start = negative ? 1 : 0;
        if (text.Length <= start || text.Length > 10)
            return 0;
        int value = 0;
        for (int i = start; i < text.Length; i++)
        {
            int d = "0123456789".IndexOf(text.Substring(i, 1));
            if (d < 0 || value > 214748364 || (value == 214748364 && d > 7))
                return 0;
            value = value * 10 + d;
        }

        return negative ? -value : value;
    }
}

public static class MasText
{
    public static string[] Split(string text, string delimiter)
    {
        var pieces = new System.Collections.Generic.List<string>();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
            if (text.Substring(i, 1) == delimiter)
            {
                pieces.Add(text.Substring(start, i - start));
                start = i + 1;
            }

        pieces.Add(text.Substring(start));
        string[] result = new string[pieces.Count];
        for (int i = 0; i < pieces.Count; i++)
            result[i] = pieces[i];
        return result;
    }

    public static int[] Codes(string text)
    {
        var values = new System.Collections.Generic.List<int>();
        for (int i = 0; i < text.Length; i++)
        {
            int c = (int)text[i];
            if (c == 239 && i + 2 < text.Length)
            {
                int b = (int)text[i + 1], d = (int)text[i + 2];
                c = ((c & 15) << 12) | ((b & 63) << 6) | (d & 63);
                i += 2;
            }

            if (c >= 65377 && c <= 65439)
                c = c - 65280 + 64;
            values.Add(c);
        }

        int[] result = new int[values.Count];
        for (int i = 0; i < values.Count; i++)
            result[i] = values[i];
        return result;
    }
}

public class DHira
{
    public int[] mes = MasArrays.Make(0, () => 0);
    public int x = 0;
    public int y = 0;
    public int zoom = 0;
    public int cou = 0;
    public int mx = 0;
}

public class DHira2
{
    public int[] mes = MasArrays.Make(0, () => 0);
    public int x = 0;
    public int y = 0;
    public int zoom = 0;
    public int cou = 0;
}

public class Moudat
{
    public int d = 0;
    public int obj = 0;
}
