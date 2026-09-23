// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class EnemyMotion : ActorCopy
{
    public float Angle;
    public Vector3 TargetPos = new Vector3();
    public Vector3 Pos = new Vector3();
    public float VelZ;
    public float Ticks;
    public EnemyMotionType Type = new EnemyMotionType();
    public void Initialize()
    {
        if (Type.IsAppearingFromFront)
            Pos.Z = Field.FrontDepth;
        else
            Pos.Z = Field.BackDepth;
        Angle = Type.AppearingAngle;
        VelZ = 0;
        Ticks = 0;
        Update();
        Pos.X = TargetPos.X;
        Pos.Y = TargetPos.Y;
    }

    public void Update()
    {
        Angle += Type.AngleSpeed.GetValue(Ticks);
        float r = Type.Radius.GetValue(Ticks);
        TargetPos.X = (float)Math.Sin(Angle) * r;
        TargetPos.Y = (float)Math.Cos(Angle) * r;
        Pos.X += (TargetPos.X - Pos.X) * Type.TargetVelRatio;
        Pos.Y += (TargetPos.Y - Pos.Y) * Type.TargetVelRatio;
        if (Type.TargetZ > Pos.Z)
            VelZ += Type.VelZSpeed;
        else
            VelZ -= Type.VelZSpeed;
        VelZ *= Type.VelZSpeedDecayRate;
        Pos.Z += VelZ;
        Ticks += Stage.GameSpeedSqrt;
    }

    public EnemyMotion Copy()
    {
        return new EnemyMotion
        {
            Angle = Angle,
            TargetPos = TargetPos.Copy(),
            Pos = Pos.Copy(),
            VelZ = VelZ,
            Ticks = Ticks,
            Type = Type.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public class EnemyMotionType : ActorCopy
{
    public SinValue AngleSpeed = new SinValue();
    public SinValue Radius = new SinValue();
    public float TargetVelRatio;
    public float TargetZ;
    public float VelZSpeed;
    public float VelZSpeedDecayRate;
    public float AppearingAngle;
    public bool IsAppearingFromFront;
    public void InitializeForEnemy1()
    {
        float asr = (float)(Stage.Random.NextDouble() * 0.5f + 0.5f) * (Stage.Random.Nextint(2) * 2 - 1);
        AngleSpeed.Center = 0.05f * asr;
        AngleSpeed.Amplitude = 0.025f * asr;
        AngleSpeed.CycleSpeed = 0.1f;
        Radius.Center = 0.6f * Tube.Radius;
        Radius.Amplitude = 0.3f * Tube.Radius;
        Radius.CycleSpeed = 0.1f;
        AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        IsAppearingFromFront = false;
        TargetVelRatio = 0.2f;
        TargetZ = Field.FireBoundaryDepth * 2;
        VelZSpeed = 1.0f;
        VelZSpeedDecayRate = 0.9f;
    }

    public void InitializeForEnemy2()
    {
        {
            AngleSpeed.CycleSpeed = 0;
            AngleSpeed.Amplitude = AngleSpeed.CycleSpeed;
            AngleSpeed.Center = AngleSpeed.Amplitude;
        }

        Radius.Center = ((float)Stage.Random.NextDouble() * 0.9f) * Tube.Radius;
        {
            Radius.CycleSpeed = 0;
            Radius.Amplitude = Radius.CycleSpeed;
        }

        AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        IsAppearingFromFront = false;
        TargetVelRatio = 1;
        TargetZ = Field.FrontDepth;
        VelZSpeed = 0.6f;
        VelZSpeedDecayRate = 0.9f;
    }

    public void InitializeForEnemy3()
    {
        float asr = (Stage.Random.Nextint(2) * 2 - 1);
        AngleSpeed.Center = 0.025f * asr;
        {
            AngleSpeed.CycleSpeed = 0;
            AngleSpeed.Amplitude = AngleSpeed.CycleSpeed;
        }

        Radius.Center = ((float)Stage.Random.NextDouble() * 0.3f + 0.6f) * Tube.Radius;
        {
            Radius.CycleSpeed = 0;
            Radius.Amplitude = Radius.CycleSpeed;
        }

        AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        IsAppearingFromFront = false;
        TargetVelRatio = 0.1f;
        TargetZ = Field.FrontDepth;
        VelZSpeed = 0.2f;
        VelZSpeedDecayRate = 0.9f;
    }

    public void InitializeForEnemy4()
    {
        float rr = (Stage.Random.Nextint(2) * 2 - 1);
        {
            AngleSpeed.CycleSpeed = 0;
            AngleSpeed.Amplitude = AngleSpeed.CycleSpeed;
            AngleSpeed.Center = AngleSpeed.Amplitude;
        }

        Radius.Center = 0.5f * Tube.Radius;
        Radius.Amplitude = 0.4f * rr * Tube.Radius;
        Radius.CycleSpeed = (float)(Stage.Random.NextDouble() * 0.5f);
        AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        IsAppearingFromFront = false;
        TargetVelRatio = 0.04f;
        TargetZ = Field.BackDepth;
        VelZSpeed = 0.3f + (float)(Stage.Random.NextDouble() * 0.4f);
        VelZSpeedDecayRate = 0.9f;
    }

    public EnemyMotionType Copy()
    {
        return new EnemyMotionType
        {
            AngleSpeed = AngleSpeed.Copy(),
            Radius = Radius.Copy(),
            TargetVelRatio = TargetVelRatio,
            TargetZ = TargetZ,
            VelZSpeed = VelZSpeed,
            VelZSpeedDecayRate = VelZSpeedDecayRate,
            AppearingAngle = AppearingAngle,
            IsAppearingFromFront = IsAppearingFromFront
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public class SinValue : ActorCopy
{
    public float Center;
    public float Amplitude;
    public float CycleSpeed;
    public float GetValue(float t)
    {
        return Center + (float)Math.Sin(t * CycleSpeed) * Amplitude;
    }

    public SinValue Copy()
    {
        return new SinValue
        {
            Center = Center,
            Amplitude = Amplitude,
            CycleSpeed = CycleSpeed
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
