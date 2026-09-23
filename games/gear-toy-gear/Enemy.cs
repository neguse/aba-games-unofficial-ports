// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class EnemyPool : ActorPool<Enemy>
{
    private Random random;
    private GtgFrame frame;
    private GameState gameState;
    private Field field;
    private BulletPool bullets;
    private Player player;
    private PillarPool pillars;
    private ParticlePool particles;
    private Sound sound;
    private PlayerHomingLaserPool playerHomingLasers;
    private GearEdgeShape edgeShape;
    public EnemyPool(int n, GtgFrame frame, GameState gameState, Field field, BulletPool bullets, Player player, PillarPool pillars, ParticlePool particles, Sound sound) : base(n, () => new Enemy())
    {
        this.frame = frame;
        this.gameState = gameState;
        this.field = field;
        this.bullets = bullets;
        this.player = player;
        this.pillars = pillars;
        this.particles = particles;
        this.sound = sound;
        shape = new GearShape(frame, 1, 0.7f, 0.3f, new Color(240, 180, 180, 200), new Color(0, 0, 0, 200), true);
        edgeShape = new GearEdgeShape(frame, 1, 0.7f, 0.3f, 0.2f);
    }

    public void SetParams(PlayerHomingLaserPool playerHomingLasers)
    {
        this.playerHomingLasers = playerHomingLasers;
    }

    public void SetRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    public void Add(EnemyType type, EnemyMotionType motionType)
    {
        Enemy a = new Enemy();
        a.Scale = type.Scale;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color = (type.Color).Copy();
        a.Ticks = 0;
        a.Motion.Type = (motionType).Copy();
        a.Motion.Initialize();
        a.Pos = (a.Motion.Pos).Copy();
        a.PPos = (a.Pos).Copy();
        a.Type = (type).Copy();
        {
            a.FireTicks = type.FireInterval;
            a.FireInterval = a.FireTicks;
        }

        a.Roll = 0;
        AddT(a);
    }

    public override bool UpdateT(Enemy a)
    {
        a.PPos = (a.Pos).Copy();
        a.Motion.Update();
        a.Pos = (a.Motion.Pos).Copy();
        Vector3 vel = ((a.Pos - a.PPos)).Copy();
        if (pillars.CheckHit((a.Pos).Copy()))
        {
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(40, (a.Pos).Copy(), (vel * 2).Copy(), 0.75f, a.Scale * 1.5f, 0.8f, 0.4f, 0.2f, 0.7f);
            return false;
        }

        a.Ticks++;
        if (a.Pos.Z < Field.FireBoundaryDepth)
            a.FireTicks -= Stage.GameSpeed;
        if (a.FireTicks <= 0)
        {
            Fire(a);
            a.FireTicks = a.Type.FireInterval;
        }

        a.Roll += 10.0f / a.FireTicks;
        a.Orientation = (Quaternion.CreateFromYawPitchRoll(vel.X * 0.3f, vel.Y * 0.3f, a.Roll)).Copy();
        return (a.Pos.Z < Field.FrontDepth && a.Pos.Z > Field.BackDepth);
    }

    private void Fire(Enemy a)
    {
        sound.PlaySe3D("Shot", (a.Pos).Copy());
        if (a.Type.IsFiringStraight)
            bullets.AddVector3float((a.Pos).Copy(), a.Type.FireSpeed);
        else
            bullets.AddVector3Vector3float((a.Pos).Copy(), (player.Pos).Copy(), a.Type.FireSpeed);
    }

    public bool CheckHit(Vector3 pos)
    {
        bool isHit = false;
        for (int i = 0; i < actorCount;)
        {
            if (Vector3.Distance((Actors[(i)].Pos).Copy(), (pos).Copy()) < Actors[(i)].Scale * 1.5f)
            {
                Hit(i);
                isHit = true;
            }
            else
            {
                i++;
            }
        }

        return isHit;
    }

    public void Hit(int i)
    {
        particles.AddintVector3floatfloatfloatfloatfloatfloatfloat(20, (Actors[(i)].Pos).Copy(), 2, 20, Actors[(i)].Scale * 3, 1, 0.75f, 0.5f, 0.8f);
        Vector3 vel = ((Actors[(i)].Pos - Actors[(i)].PPos)).Copy();
        vel *= (2 + (float)random.NextDouble() * 2);
        particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(80, (Actors[(i)].Pos).Copy(), (vel).Copy(), 0.5f, Actors[(i)].Scale * 2, 1, 0.75f, 0.5f, 0.6f);
        gameState.AddScoreint(100);
        sound.PlaySe3D("EnemyDestroyed", (Actors[(i)].Pos).Copy());
        Remove(i);
    }

    public int GetNearest(float x, float y, float distance)
    {
        int ri = -1, i = 0;
        float md = distance;
        ForEach((Enemy a) =>
        {
            float d = Math.Abs(a.Pos.X - x) + Math.Abs(a.Pos.Y - y);
            if (!a.IsLocked && d < md && a.Pos.Z < 0)
            {
                ri = i;
                md = d;
            }

            i++;
        });
        if (ri >= 0)
            Actors[(ri)].IsLocked = true;
        return ri;
    }

    public override void DrawT(Enemy a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }

    public void DrawEdge()
    {
        DrawShape(edgeShape);
    }

    public override void Remove(int i)
    {
        playerHomingLasers.OnEnemyRemoved(false, i);
        base.Remove(i);
    }
}

public class Enemy : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public int Ticks;
    public EnemyMotion Motion = new EnemyMotion();
    public Vector3 PPos = new Vector3();
    public float FireInterval;
    public float FireTicks;
    public float Roll;
    public EnemyType Type = new EnemyType();
    public bool IsLocked;
    public Enemy Copy()
    {
        return new Enemy
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Ticks = Ticks,
            Motion = Motion.Copy(),
            PPos = PPos.Copy(),
            FireInterval = FireInterval,
            FireTicks = FireTicks,
            Roll = Roll,
            Type = Type.Copy(),
            IsLocked = IsLocked
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public class EnemyType : ActorCopy
{
    public bool IsFiringStraight;
    public float FireInterval;
    public float FireSpeed;
    public float Scale;
    public Vector4 Color = new Vector4();
    public EnemyType Copy()
    {
        return new EnemyType
        {
            IsFiringStraight = IsFiringStraight,
            FireInterval = FireInterval,
            FireSpeed = FireSpeed,
            Scale = Scale,
            Color = Color.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
