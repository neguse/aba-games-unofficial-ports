// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class Field
{
    public const float FrontDepth = 16, BackDepth = -512, FireBoundaryDepth = -120;
    public const int TubeCount = 16;
    private GtgFrame frame;
    private Tube[] tubes;
    private GearShape tubeShape;
    public Field(GtgFrame frame)
    {
        this.frame = frame;
        tubes = GtgArrays.Make(TubeCount, () => new Tube());
        tubeShape = new GearShape(frame, Tube.Radius * 1.2f, Tube.Radius, Tube.Height, new Color(100, 200, 255, 200), new Color(100, 150, 200, 200), false);
    }

    public void Initialize()
    {
        for (int i = 0; i < tubes.Length; i++)
            tubes[(i)].Initialize(i);
    }

    public void Update()
    {
        for (int i = 0; i < TubeCount; i++)
            tubes[(i)].Update();
    }

    public void Draw()
    {
        Shape.BeginAddInstance();
        for (int i = 0; i < TubeCount; i++)
            tubes[(i)].Draw();
        tubeShape.Draw();
    }
}

public class Tube : ActorCopy
{
    public const float Radius = 100;
    public const float Height = 15;
    private Vector3 pos = new Vector3();
    private float scale;
    private Quaternion orientation = new Quaternion();
    private Vector4 color = new Vector4();
    private Quaternion roll = new Quaternion();
    public void Initialize(int i)
    {
        {
            pos.Y = 0;
            pos.X = pos.Y;
        }

        pos.Z = -i * Height * 2 * 1.1f;
        scale = 1.0f;
        orientation = (Quaternion.Identity).Copy();
        {
            color.W = 0;
            color.Z = color.W;
            color.Y = color.Z;
            color.X = color.Y;
        }

        roll = (Quaternion.CreateFromYawPitchRoll(0, 0, ((i % 2) * 2 - 1) * 0.002f)).Copy();
    }

    public void Update()
    {
        orientation *= GameMath.Rotation(roll);
        pos.Z += (Stage.PlayerDepthSpeed) * SimulationTime.Step;
        if (pos.Z > Field.FrontDepth)
            pos.Z -= Height * 2 * 1.1f * Field.TubeCount;
    }

    public void Draw()
    {
        Shape.AddInstance((pos).Copy(), scale, (orientation).Copy(), (color).Copy());
    }

    public Tube Copy()
    {
        return new Tube
        {
            pos = pos.Copy(),
            scale = scale,
            orientation = orientation.Copy(),
            color = color.Copy(),
            roll = roll.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
