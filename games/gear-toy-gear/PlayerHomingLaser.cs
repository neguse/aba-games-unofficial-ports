// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class PlayerHomingLaserPool : ActorPool<PlayerHomingLaser>
{
    private GtgFrame frame;
    private Field field;
    private LaserPool lasers;
    private Player player;
    private EnemyPool enemies;
    private MiddleEnemyPool middleEnemies;
    private ParticlePool particles;
    public PlayerHomingLaserPool(int n, GtgFrame frame, Field field, LaserPool lasers, Player player, EnemyPool enemies, MiddleEnemyPool middleEnemies, ParticlePool particles) : base(n, () => new PlayerHomingLaser())
    {
        this.frame = frame;
        this.field = field;
        this.lasers = lasers;
        this.player = player;
        this.enemies = enemies;
        this.middleEnemies = middleEnemies;
        this.particles = particles;
    }

    public void Add(bool isMiddleEnemy, int index, float speed, float angle)
    {
        PlayerHomingLaser a = new PlayerHomingLaser();
        a.Pos = (player.Pos).Copy();
        a.Vel.X = (float)Math.Sin(angle) * speed / 2;
        a.Vel.Y = (float)Math.Cos(angle) * speed / 2;
        a.Vel.Z = 0;
        a.IsAimingMiddleEnemy = isMiddleEnemy;
        a.EnemyIndex = index;
        a.Speed = speed;
        AddT(a);
    }

    public override void Update()
    {
        int ri = 0;
        int ac = actorCount;
        for (int i = 0; i < actorCount; i++, ri++)
        {
            if (Actors[(i)].IsRemoved)
            {
                Remove(i);
                i--;
            }
        }

        for (int i = 0; i < actorCount; i++)
        {
            if (!Actors[(i)].IsRemoved)
                UpdatePlayerHomingLaserint(Actors[(i)], i);
        }
    }

    public void UpdatePlayerHomingLaserint(PlayerHomingLaser a, int index)
    {
        a.Pos += (a.Vel) * SimulationTime.Step;
        a.Vel.Z += (-a.Speed - a.Vel.Z) * SimulationTime.Blend(0.05f);
        Vector3 ep = new Vector3();
        if (a.IsAimingMiddleEnemy)
            ep = (middleEnemies.Get(a.EnemyIndex).Pos).Copy();
        else
            ep = (enemies.Get(a.EnemyIndex).Pos).Copy();
        float tt = (ep.Z - a.Pos.Z) / a.Vel.Z;
        if (tt < 1)
            tt = 1;
        else if (tt > 100)
            tt = 100;
        float tx = a.Pos.X + a.Vel.X * tt;
        float ty = a.Pos.Y + a.Vel.Y * tt;
        a.Vel.X += ((ep.X - a.Pos.X) * (0.2f / tt)) * SimulationTime.Step;
        a.Vel.Y += ((ep.Y - a.Pos.Y) * (0.2f / tt)) * SimulationTime.Step;
        a.Pos.X += (ep.X - a.Pos.X) * SimulationTime.Blend(1.0f / tt);
        a.Pos.Y += (ep.Y - a.Pos.Y) * SimulationTime.Blend(1.0f / tt);
        if (SimulationTime.Emit) lasers.AddPlayer((a.Pos).Copy(), (a.Vel).Copy(), a.Vel.Length() * 0.8f, 40);
        if (a.Pos.Z <= ep.Z)
        {
            if (a.IsAimingMiddleEnemy)
                middleEnemies.Hit(a.EnemyIndex);
            else
                enemies.Hit(a.EnemyIndex);
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(25, (a.Pos).Copy(), (a.Vel).Copy(), 0.3f, 15, 1, 0.2f, 0.4f, 0.7f);
        }
    }

    public void OnEnemyRemoved(bool isMiddleEnemy, int index)
    {
        for (int i = 0; i < actorCount; i++)
        {
            if (Actors[(i)].IsAimingMiddleEnemy == isMiddleEnemy)
            {
                if (Actors[(i)].EnemyIndex == index)
                    Actors[(i)].IsRemoved = true;
                else if (Actors[(i)].EnemyIndex > index)
                    Actors[(i)].EnemyIndex--;
            }
        }
    }
}

public class PlayerHomingLaser : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public Vector4 Color = new Vector4();
    public Vector3 Vel = new Vector3();
    public bool IsAimingMiddleEnemy;
    public int EnemyIndex;
    public float Speed;
    public bool IsRemoved;
    public PlayerHomingLaser Copy()
    {
        return new PlayerHomingLaser
        {
            Pos = Pos.Copy(),
            Color = Color.Copy(),
            Vel = Vel.Copy(),
            IsAimingMiddleEnemy = IsAimingMiddleEnemy,
            EnemyIndex = EnemyIndex,
            Speed = Speed,
            IsRemoved = IsRemoved
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
