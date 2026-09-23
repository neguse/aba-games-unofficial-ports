// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Letter
{
    private static LetterData[] letters;
    private static BarPool bars;
    public static void Initialize(GtgFrame frame)
    {
        letters = GtgData.Letters();
        bars = new BarPool(640, frame);
    }

    public static void Draw()
    {
        bars.Draw();
        bars.Clear();
    }

    public static Vector3 AddstringVector3floatQuaternionfloat(string s, Vector3 p, float scale, Quaternion orientation, float alpha)
    {
        Vector3 nl = new Vector3(scale * 2, 0, 0);
        nl = (Vector3.TransformVector3Quaternion((nl).Copy(), (orientation).Copy())).Copy();
        for (int index = 0; index < s.Length; index++)
        {
            string c = s.Substring(index, 1);
            if (c != " ")
            {
                int ci = ConvertCharToInt(c);
                letters[(ci)].Draw(bars, (p).Copy(), scale, (orientation).Copy(), alpha);
            }

            p += nl;
        }

        return (p).Copy();
    }

    public static Vector3 AddintVector3floatQuaternionfloat(int n, Vector3 p, float scale, Quaternion orientation, float alpha)
    {
        Vector3 nl = new Vector3(-scale * 2, 0, 0);
        nl = (Vector3.TransformVector3Quaternion((nl).Copy(), (orientation).Copy())).Copy();
        for (;;)
        {
            letters[(n % 10)].Draw(bars, (p).Copy(), scale, (orientation).Copy(), alpha);
            p += nl;
            n /= 10;
            if (n <= 0)
                break;
        }

        return (p).Copy();
    }

    public static Vector3 AddintintVector3floatQuaternionfloat(int n, int digit, Vector3 p, float scale, Quaternion orientation, float alpha)
    {
        Vector3 nl = new Vector3(-scale * 2, 0, 0);
        nl = (Vector3.TransformVector3Quaternion((nl).Copy(), (orientation).Copy())).Copy();
        for (int i = 0; i < digit; i++)
        {
            letters[(n % 10)].Draw(bars, (p).Copy(), scale, (orientation).Copy(), alpha);
            p += nl;
            n /= 10;
        }

        return (p).Copy();
    }

    private static int ConvertCharToInt(string c)
    {
        int index = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+??!/".IndexOf(c);
        if (index < 0)
            index = "0123456789abcdefghijklmnopqrstuvwxyz._-+??!/".IndexOf(c);
        return index < 0 ? 0 : index;
    }
}

public class LetterData : ActorCopy
{
    private BarData[] data;
    public LetterData(BarData[] bars = null)
    {
        data = bars;
    }

    public void Draw(BarPool bars, Vector3 p, float scale, Quaternion orientation, float alpha)
    {
        foreach (BarData d in data)
        {
            bars.Add((p + Vector3.TransformVector3Quaternion((d.Offset).Copy(), (orientation).Copy()) * scale).Copy(), (Quaternion.Concatenate((d.Orientation).Copy(), (orientation).Copy())).Copy(), d.Scale * scale * 0.1f, alpha);
        }
    }

    public LetterData Copy()
    {
        return new LetterData
        {
            data = data
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public class BarData : ActorCopy
{
    public Vector3 Offset = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public BarData Copy()
    {
        return new BarData
        {
            Offset = Offset.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public class BarPool : ActorPool<Bar>
{
    public BarPool(int n, GtgFrame frame) : base(n, () => new Bar())
    {
        shape = new CubeShape(frame, 6, 1, 1);
    }

    public void Add(Vector3 p, Quaternion o, float scale, float alpha)
    {
        Bar a = new Bar();
        a.Pos = (p).Copy();
        a.Scale = scale;
        a.Orientation = (o).Copy();
        a.Color.X = 1;
        a.Color.Y = 1;
        a.Color.Z = 1;
        a.Color.W = alpha;
        AddT(a);
    }

    public override void DrawT(Bar a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }
}

public class Bar : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public Bar Copy()
    {
        return new Bar
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
