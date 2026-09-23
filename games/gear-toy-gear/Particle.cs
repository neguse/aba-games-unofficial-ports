// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class ParticlePool : ActorPool<Particle>
{
    private const float MinimumScale = 1;
    private Vector3[] lightColors = GtgArrays.Make(24, () => new Vector3());
    private GtgFrame frame;
    private Random random;
    public ParticlePool(int n, GtgFrame frame) : base(n, () => new Particle())
    {
        shape = new CubeShape(frame, 0.2f, 0.2f, 1);
        this.frame = frame;
    }

    public void SetRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    public override void Clear()
    {
        for (int i = 0; i < lightColors.Length; i++)
        {
            lightColors[(i)].X = 0;
            lightColors[(i)].Y = 0;
            lightColors[(i)].Z = 0;
        }

        frame.LightColors = lightColors;
        base.Clear();
    }

    public void AddintVector3Vector3floatfloatfloatfloatfloatfloat(int n, Vector3 p, Vector3 vel, float velFluctuation, float scale, float r, float g, float b, float alpha)
    {
        Particle a = new Particle();
        for (int i = 0; i < n; i++)
        {
            a.Vel.X = vel.X * ((float)random.NextDouble() * velFluctuation + (1 - velFluctuation / 2));
            a.Vel.Y = vel.Y * ((float)random.NextDouble() * velFluctuation + (1 - velFluctuation / 2));
            a.Vel.Z = vel.Z * ((float)random.NextDouble() * velFluctuation + (1 - velFluctuation / 2));
            a.Pos.X = p.X + a.Vel.X;
            a.Pos.Y = p.Y + a.Vel.Y;
            a.Pos.Z = p.Z + a.Vel.Z;
            a.Orientation = (Quaternion.Identity).Copy();
            a.TargetScale = scale;
            a.BaseScale = MinimumScale;
            a.Color.X = r * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.Y = g * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.Z = b * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.W = alpha;
            AddT(a);
        }
    }

    public void AddintVector3floatfloatfloatfloatfloatfloatfloat(int n, Vector3 p, float velFluctuation, float velFluctuationZ, float scale, float r, float g, float b, float alpha)
    {
        Particle a = new Particle();
        for (int i = 0; i < n; i++)
        {
            a.Vel.X = ((float)random.NextDouble() - 0.5f) * velFluctuation;
            a.Vel.Y = ((float)random.NextDouble() - 0.5f) * velFluctuation;
            a.Vel.Z = ((float)random.NextDouble() - 0.5f) * velFluctuationZ;
            a.Pos.X = p.X + a.Vel.X;
            a.Pos.Y = p.Y + a.Vel.Y;
            a.Pos.Z = p.Z + a.Vel.Z;
            a.Orientation = (Quaternion.Identity).Copy();
            a.TargetScale = scale;
            a.BaseScale = MinimumScale;
            a.Color.X = r * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.Y = g * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.Z = b * ((float)random.NextDouble() * 0.5f + 0.75f);
            a.Color.W = alpha;
            AddT(a);
        }
    }

    public override void Update()
    {
        for (int i = 0; i < lightColors.Length; i++)
        {
            lightColors[(i)].X = 0;
            lightColors[(i)].Y = 0;
            lightColors[(i)].Z = 0;
        }

        base.Update();
        frame.LightColors = lightColors;
    }

    public override bool UpdateT(Particle a)
    {
        a.PPos = (a.Pos).Copy();
        a.Pos += a.Vel;
        if (a.Pos.X * a.Pos.X + a.Pos.Y * a.Pos.Y > Tube.Radius * Tube.Radius)
        {
            Vector3 nv = new Vector3(-a.Pos.X, -a.Pos.Y, 0);
            if (Vector3.Dot((nv).Copy(), (a.Vel).Copy()) < 0)
            {
                nv.Normalize();
                a.Vel = (Vector3.Reflect((a.Vel).Copy(), (nv).Copy())).Copy();
            }
        }

        a.BaseScale += (a.TargetScale - a.BaseScale) * 0.1f;
        Vector3 v = (a.Pos - a.PPos).Copy();
        float vl = v.Length();
        a.Orientation.X = -v.Y;
        a.Orientation.Y = v.X;
        a.Orientation.Z = 0;
        a.Orientation.W = v.Z + vl;
        a.Orientation.Normalize();
        if (vl > 10)
            vl = 10;
        a.Scale = vl * a.BaseScale * 0.1f;
        a.Vel.Z -= 0.1f;
        a.Vel *= 0.98f;
        a.TargetScale *= 0.98f;
        a.Color.W *= 0.99f;
        int li = 4 - GameMath.integer((float)((a.Pos.Z / 32)));
        if (li >= 0 && li < lightColors.Length)
        {
            float lr = a.Scale * a.Color.W;
            lightColors[(li)].X += a.Color.X * lr;
            lightColors[(li)].Y += a.Color.Y * lr;
            lightColors[(li)].Z += a.Color.Z * lr;
        }

        return (a.Scale >= MinimumScale);
    }

    public override void DrawT(Particle a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }
}

public class Particle : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public Vector3 Vel = new Vector3();
    public float TargetScale;
    public float BaseScale;
    public Vector3 PPos = new Vector3();
    public Particle Copy()
    {
        return new Particle
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Vel = Vel.Copy(),
            TargetScale = TargetScale,
            BaseScale = BaseScale,
            PPos = PPos.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
