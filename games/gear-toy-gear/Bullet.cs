// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class BulletPool : ActorPool<Bullet>
{
    private GtgFrame frame;
    private Field field;
    private Player player;
    private PillarPool pillars;
    private ParticlePool particles;
    private EllipseEdgeShape edgeShape;
    private Quaternion rollQuaternion = new Quaternion();
    public BulletPool(int n, GtgFrame frame, Field field, Player player, PillarPool pillars, ParticlePool particles) : base(n, () => new Bullet())
    {
        this.frame = frame;
        this.field = field;
        this.player = player;
        this.pillars = pillars;
        this.particles = particles;
        shape = new EllipseShape(frame, 1, 0.6f);
        edgeShape = new EllipseEdgeShape(frame, 1, 0.6f, 0.8f, 16);
        rollQuaternion = (Quaternion.CreateFromYawPitchRoll(0, 0, 0.1f)).Copy();
    }

    public void AddVector3Vector3float(Vector3 from, Vector3 to, float speed)
    {
        Bullet a = new Bullet();
        a.Pos = (from).Copy();
        a.Vel = (to - from).Copy();
        a.Vel.Normalize();
        a.Vel *= speed;
        a.Scale = 8.0f;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color.X = 0.5f;
        a.Color.Y = 1;
        a.Color.Z = 1;
        a.Color.W = 0.7f;
        AddT(a);
    }

    public void AddVector3float(Vector3 from, float speed)
    {
        Bullet a = new Bullet();
        a.Pos = (from).Copy();
        a.Vel.Z = speed;
        a.Scale = 8.0f;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color.X = 0.5f;
        a.Color.Y = 1;
        a.Color.Z = 1;
        a.Color.W = 0.7f;
        AddT(a);
    }

    public override bool UpdateT(Bullet a)
    {
        a.Orientation *= GameMath.Rotation(rollQuaternion);
        a.Pos += (a.Vel * Stage.GameSpeed) * SimulationTime.Step;
        if (pillars.CheckHit((a.Pos).Copy()))
        {
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(16, (a.Pos).Copy(), (a.Vel).Copy(), 0.5f, 10, 0.4f, 1.0f, 0.8f, 0.6f);
            return false;
        }

        if (a.Pos.Z > -10 && Vector3.Distance((a.Pos).Copy(), (player.Pos).Copy()) < a.Scale * 0.5f)
        {
            player.Destroy();
            return false;
        }

        return (a.Pos.Z < Field.FrontDepth);
    }

    public override void DrawT(Bullet a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }

    public void DrawEdge()
    {
        DrawShape(edgeShape);
    }
}

public class Bullet : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public Vector3 Vel = new Vector3();
    public Bullet Copy()
    {
        return new Bullet
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Vel = Vel.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
