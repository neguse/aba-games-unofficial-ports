// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class ShotPool : ActorPool<Shot>
{
    private GtgFrame frame;
    private Field field;
    private EnemyPool enemies;
    private MiddleEnemyPool middleEnemies;
    private PillarPool pillars;
    private Player player;
    private ParticlePool particles;
    private Quaternion rollQuaternion = new Quaternion();
    public ShotPool(int n, GtgFrame frame, Field field, EnemyPool enemies, MiddleEnemyPool middleEnemies, PillarPool pillars, Player player, ParticlePool particles) : base(n, () => new Shot())
    {
        this.frame = frame;
        this.field = field;
        this.enemies = enemies;
        this.middleEnemies = middleEnemies;
        this.pillars = pillars;
        this.player = player;
        this.particles = particles;
        shape = new CubeShape(frame, 1, 1, 3);
        rollQuaternion = (Quaternion.CreateFromYawPitchRoll(0, 0, 0.05f)).Copy();
    }

    public void Add(Quaternion o)
    {
        Shot a = new Shot();
        a.Pos = (player.Pos).Copy();
        Vector3 v = new Vector3(0, 0, -10);
        a.Vel = (Vector3.TransformVector3Quaternion((v).Copy(), (o).Copy())).Copy();
        a.Scale = 2;
        a.Orientation = (o).Copy();
        a.Color.X = 0.6f;
        a.Color.Y = 0.3f;
        a.Color.Z = 0.9f;
        a.Color.W = 0.9f;
        AddT(a);
    }

    public override bool UpdateT(Shot a)
    {
        a.Orientation *= GameMath.Rotation(rollQuaternion);
        a.Pos += (a.Vel) * SimulationTime.Step;
        if (a.Pos.X * a.Pos.X + a.Pos.Y * a.Pos.Y > Tube.Radius * Tube.Radius)
        {
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(4, (a.Pos).Copy(), (a.Vel).Copy(), 0.75f, 10, 1.0f, 0.5f, 1.0f, 0.8f);
            return false;
        }

        if (enemies.CheckHit((a.Pos).Copy()) || middleEnemies.CheckHit((a.Pos).Copy()) || pillars.CheckHit((a.Pos).Copy()))
        {
            Vector3 pv = (a.Vel).Copy();
            pv *= -1;
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(4, (a.Pos).Copy(), (pv).Copy(), 0.75f, 10, 1.0f, 0.5f, 1.0f, 0.8f);
            return false;
        }

        return (a.Pos.Z > Field.BackDepth);
    }

    public override void DrawT(Shot a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }
}

public class Shot : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public Vector3 Vel = new Vector3();
    public Shot Copy()
    {
        return new Shot
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
