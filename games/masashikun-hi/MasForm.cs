// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using System.Collections.Generic;
using static Lub;

public static class MasForm
{
    public const int sxmax = 640, symax = 480;
    public static int mousemv, mousebt;
    public static bool hidden;
    public static List<float> spans = new List<float>();
    static int version;
    public static void showmousecursor()
    {
        hidden = false;
    }

    public static void hidemousecursor()
    {
        hidden = true;
    }

    public static void Span(int x, int y, int w, int h, int color)
    {
        spans.Add(x);
        spans.Add(y);
        spans.Add(w);
        spans.Add(h);
        spans.Add(color);
    }

    public static bool putline(int x1, int y1, int x2, int y2, int width, int color)
    {
        bool a = x1 >= 0 && x1 < sxmax && y1 >= 0 && y1 < symax, b = x2 >= 0 && x2 < sxmax && y2 >= 0 && y2 < symax;
        if (!a && !b)
            return false;
        bool result = a && b;
        if (!a)
        {
            int t = x1;
            x1 = x2;
            x2 = t;
            t = y1;
            y1 = y2;
            y2 = t;
        }

        int wx = Math.Abs(x1 - x2), wy = Math.Abs(y1 - y2);
        if (wx == 0 && wy == 0)
            return result;
        int mx = x2 > x1 ? 1 : x2 < x1 ? -1 : 0, my = y2 > y1 ? 1 : y2 < y1 ? -1 : 0;
        if (wx > wy)
        {
            int remaining = Math.Min(wx, mx < 0 ? x1 : sxmax - x1), ratio = MasMath.Div(wy * 256, wx), fraction = 0;
            while (y1 >= 0 && y1 < symax - width)
            {
                int length = 0;
                while (fraction < 256 && remaining > 0)
                {
                    length++;
                    remaining--;
                    fraction += ratio;
                }

                if (length > 0)
                    Span(mx < 0 ? x1 - length + 1 : x1, y1, length, width, color);
                if (remaining <= 0)
                    return result;
                x1 += mx * length;
                y1 += my;
                fraction -= 256;
            }
        }
        else
        {
            int remaining = Math.Min(wy, my < 0 ? y1 : symax - y1), ratio = MasMath.Div(wx * 256, wy), fraction = 0;
            while (x1 >= 0 && x1 < sxmax - width)
            {
                int length = 0;
                while (fraction < 256 && remaining > 0)
                {
                    length++;
                    remaining--;
                    fraction += ratio;
                }

                if (length > 0)
                    Span(x1, my < 0 ? y1 - length + 1 : y1, width, length, color);
                if (remaining <= 0)
                    return result;
                y1 += my * length;
                x1 += mx;
                fraction -= 256;
            }
        }

        return result;
    }

    public static void Frame()
    {
        version++;
        var shader = Gfx.UseShader("masashikun", GameShaders.vertex, GameShaders.fragment, 1);
        if (shader == null)
            return;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = new float[] { 0, 0, 96 / 255f, 1 } });
        if (spans.Count > 0)
        {
            var rects = Gfx.UseBuffer("spans", Gfx.BufferType.Storage, spans, version);
            if (rects != null) Gfx.Draw(spans.Count / 5 * 6, new Dictionary<string, object> { ["spans"] = rects }, new DrawOpts { Shader = shader, Blend = Gfx.Blend.None, Depth = false, DepthWrite = false, Cull = Gfx.Cull.None });
        }
        Gfx.EndPass();
    }
}
