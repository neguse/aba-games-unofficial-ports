// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class LaserPool : ActorPool<Laser>
{
    private GtgFrame frame;
    private Field field;
    private Shape edgeShape;
    public LaserPool(int n, GtgFrame frame, Field field) : base(n, () => new Laser())
    {
        this.frame = frame;
        this.field = field;
        shape = new CubeShape(frame, 0.33f, 0.33f, 1);
        edgeShape = new CubeEdgeShape(frame, 0.33f, 0.33f, 1);
    }

    public void AddVector3Vector3float(Vector3 p, Vector3 v, float length)
    {
        Laser a = new Laser();
        a.Pos = (p).Copy();
        a.Scale = length;
        a.Orientation.X = -v.Y;
        a.Orientation.Y = v.X;
        a.Orientation.Z = 0;
        a.Orientation.W = v.Z + v.Length();
        a.Orientation.Normalize();
        a.Color.X = 0.25f;
        a.Color.Y = 1;
        a.Color.Z = 0.5f;
        a.Color.W = 0.75f;
        a.Ticks = 1;
        a.SingleStep = true;
        AddT(a);
    }

    public void AddVector3Vector3floatint(Vector3 p, Vector3 v, float length, int ticks)
    {
        Laser a = new Laser();
        a.Pos = (p).Copy();
        a.Scale = length;
        a.Orientation.X = -v.Y;
        a.Orientation.Y = v.X;
        a.Orientation.Z = 0;
        a.Orientation.W = v.Z + v.Length();
        a.Orientation.Normalize();
        a.Color.X = 0.5f;
        a.Color.Y = 1;
        a.Color.Z = 0.25f;
        a.Color.W = 0.9f;
        a.Ticks = ticks;
        a.Vel = (v).Copy();
        AddT(a);
    }

    public void AddPlayer(Vector3 p, Vector3 v, float length, int ticks)
    {
        Laser a = new Laser();
        a.Pos = (p).Copy();
        a.Scale = length;
        a.Orientation.X = -v.Y;
        a.Orientation.Y = v.X;
        a.Orientation.Z = 0;
        a.Orientation.W = v.Z + v.Length();
        a.Orientation.Normalize();
        a.Color.X = 0.6f;
        a.Color.Y = 0.3f;
        a.Color.Z = 0.9f;
        a.Color.W = 0.7f;
        a.Ticks = ticks;
        a.Vel = (v).Copy();
        AddT(a);
    }

    public override bool UpdateT(Laser a)
    {
        if (a.SingleStep) return false;
        a.Pos += (a.Vel) * SimulationTime.Step;
        a.Vel *= SimulationTime.Decay(0.8f);
        a.Color.W *= SimulationTime.Decay(0.95f);
        a.Ticks -= SimulationTime.Step;
        return a.Ticks > 0;
    }

    public override void DrawT(Laser a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }

    public void DrawEdge()
    {
        DrawShape(edgeShape);
    }
}

public class Laser : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public float Ticks;
    public bool SingleStep;
    public Vector3 Vel = new Vector3();
    public Laser Copy()
    {
        return new Laser
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Ticks = Ticks,
            SingleStep = SingleStep,
            Vel = Vel.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
