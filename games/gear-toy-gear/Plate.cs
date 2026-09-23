// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class PlatePool : ActorPool<Plate>
{
    private GtgFrame frame;
    private Field field;
    private int count;
    public PlatePool(int n, GtgFrame frame, Field field) : base(n, () => new Plate())
    {
        this.frame = frame;
        this.field = field;
    }

    public override void Clear()
    {
        count = 0;
        base.Clear();
    }

    public void Add(float radius, float angle, float z, int number, float scale)
    {
        Plate a = new Plate();
        a.Radius = radius;
        a.TargetRadius = Tube.Radius * (0.9f + (count * 13 % 7) * 0.02f);
        a.Angle = angle;
        a.Z = z;
        a.TargetZ = -160 + (count * 7 % 13) * 3;
        a.Ticks = 120;
        a.Number = 0;
        a.TargetNumber = number;
        a.Alpha = 0;
        a.Scale = scale;
        AddT(a);
        count++;
    }

    public override bool UpdateT(Plate a)
    {
        a.Z += (a.TargetZ - a.Z) * 0.05f;
        a.Radius += (a.TargetRadius - a.Radius) * 0.05f;
        a.Angle += 0.01f;
        if (a.Ticks >= 100)
        {
            a.Alpha += (1 - a.Alpha) * 0.1f;
            a.Number += GameMath.integer((float)(((a.TargetNumber - a.Number) * 0.1f)));
            if (a.Ticks == 100)
                a.Number = a.TargetNumber;
        }

        if (a.Ticks <= 20)
        {
            a.Alpha += (0 - a.Alpha) * 0.1f;
            a.Angle += 0.05f + 0.1f / a.Ticks;
        }

        a.Ticks--;
        return a.Ticks > 0;
    }

    public override void Draw()
    {
        ForEach((Plate a) =>
        {
            DrawT(a);
        });
    }

    public override void DrawT(Plate a)
    {
        int n = a.Number;
        float ang = a.Angle;
        Vector3 p = new Vector3();
        p.Z = a.Z;
        for (;;)
        {
            p.X = (float)Math.Sin(ang) * a.Radius;
            p.Y = (float)Math.Cos(ang) * a.Radius;
            Letter.AddintVector3floatQuaternionfloat(n % 10, (p).Copy(), a.Scale, (Quaternion.CreateFromYawPitchRoll(0, 0, (float)Math.PI - ang)).Copy(), a.Alpha);
            n /= 10;
            if (n <= 0)
                break;
            ang += 0.02f * a.Scale;
        }
    }
}

public class Plate : ActorCopy
{
    public float Radius, TargetRadius;
    public float Angle;
    public float Z, TargetZ;
    public int Ticks;
    public int Number, TargetNumber;
    public float Alpha;
    public float Scale;
    public Plate Copy()
    {
        return new Plate
        {
            Radius = Radius,
            TargetRadius = TargetRadius,
            Angle = Angle,
            Z = Z,
            TargetZ = TargetZ,
            Ticks = Ticks,
            Number = Number,
            TargetNumber = TargetNumber,
            Alpha = Alpha,
            Scale = Scale
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
