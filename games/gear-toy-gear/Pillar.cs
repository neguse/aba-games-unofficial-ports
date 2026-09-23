// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class PillarPool : ActorPool<Pillar>
{
    private const float BASE_THICKNESS = 1.0f;
    private GtgFrame frame;
    private Field field;
    private Player player;
    private Stage stage;
    private GearEdgeShape edgeShape;
    public PillarPool(int n, GtgFrame frame, Field field, Player player) : base(n, () => new Pillar())
    {
        this.frame = frame;
        this.field = field;
        this.player = player;
        shape = new GearShape(frame, 1, 0.7f, BASE_THICKNESS, new Color(240, 240, 240, 255), new Color(100, 100, 100, 255), true);
        edgeShape = new GearEdgeShape(frame, 1, 0.7f, BASE_THICKNESS, 0.3f);
    }

    public void SetParams(Stage stage)
    {
        this.stage = stage;
    }

    public void Addfloatfloatfloatfloatfloat(float radius, float angle, float size, float angleRate, float rollRatio)
    {
        Pillar a = (CreateBasePillar()).Copy();
        a.Pos.X = (float)Math.Sin(angle) * radius;
        a.Pos.Y = (float)Math.Cos(angle) * radius;
        a.Scale = size;
        a.Roll = (Quaternion.CreateFromYawPitchRoll(0, 0, angleRate * rollRatio)).Copy();
        a.Radius = radius;
        a.Angle = angle;
        a.AngleRate = angleRate;
        a.AppearanceTicks = -1;
        AddT(a);
    }

    public void Addfloatfloatfloatint(float x, float y, float size, int appearanceTicks)
    {
        Pillar a = (CreateBasePillar()).Copy();
        a.Pos.X = x;
        a.Pos.Y = y;
        a.Scale = size;
        a.AppearanceTicks = appearanceTicks;
        if (appearanceTicks >= 0)
        {
            a.TargetScale = size;
            a.Scale = 0;
        }

        AddT(a);
    }

    private Pillar CreateBasePillar()
    {
        Pillar a = new Pillar();
        a.Pos.Z = Field.BackDepth * 3;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color.X = 0.5f;
        a.Color.Y = 1.0f;
        a.Color.Z = 1.0f;
        a.Color.W = 1.0f;
        return (a).Copy();
    }

    public override bool UpdateT(Pillar a)
    {
        if (a.AngleRate != 0)
        {
            a.Angle += a.AngleRate * Stage.GameSpeedSqrt;
            a.Pos.X = (float)Math.Sin(a.Angle) * a.Radius;
            a.Pos.Y = (float)Math.Cos(a.Angle) * a.Radius;
            a.Orientation *= a.Roll;
        }

        a.Pos.Z += Stage.PlayerDepthSpeed;
        if (a.Pos.Z > -BASE_THICKNESS * a.Scale && a.Pos.Z < BASE_THICKNESS * a.Scale + Stage.PlayerDepthSpeed)
        {
            Vector3 pp = (player.Pos).Copy();
            Vector3 po = (-player.Vel).Copy();
            po.Z = Stage.PlayerDepthSpeed;
            po /= 5;
            for (int i = 0; i < 4; i++)
            {
                float ox = a.Pos.X - pp.X;
                float oy = a.Pos.Y - pp.Y;
                float oz = a.Pos.Z - pp.Z;
                float s = a.Scale;
                if (oz > -BASE_THICKNESS * s && oz < BASE_THICKNESS * s && ox * ox + oy * oy < s * s)
                    player.Destroy();
                pp += po;
            }
        }

        if (a.AppearanceTicks >= 0)
        {
            if (a.AppearanceTicks < 30)
            {
                a.Scale += (a.TargetScale - a.Scale) * 0.1f;
                if (a.AppearanceTicks == 0)
                    a.Scale = a.TargetScale;
            }

            a.AppearanceTicks--;
        }

        return (a.Pos.Z < Field.FrontDepth);
    }

    public bool CheckHit(Vector3 pos)
    {
        for (int i = 0; i < actorCount; i++)
        {
            float oz = Actors[(i)].Pos.Z - pos.Z;
            if (oz > -BASE_THICKNESS * Actors[(i)].Scale && oz < BASE_THICKNESS * Actors[(i)].Scale)
            {
                float ox = Actors[(i)].Pos.X - pos.X;
                float oy = Actors[(i)].Pos.Y - pos.Y;
                float s = Actors[(i)].Scale;
                if (ox * ox + oy * oy < s * s)
                    return true;
            }
        }

        return false;
    }

    public override void DrawT(Pillar a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }

    public void DrawEdge()
    {
        DrawShape(edgeShape);
    }
}

public class Pillar : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public Quaternion Roll = new Quaternion();
    public float Radius;
    public float Angle;
    public float AngleRate;
    public float TargetScale;
    public int AppearanceTicks;
    public Pillar Copy()
    {
        return new Pillar
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Roll = Roll.Copy(),
            Radius = Radius,
            Angle = Angle,
            AngleRate = AngleRate,
            TargetScale = TargetScale,
            AppearanceTicks = AppearanceTicks
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
