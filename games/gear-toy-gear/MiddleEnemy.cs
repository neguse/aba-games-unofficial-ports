// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class MiddleEnemyPool : ActorPool<MiddleEnemy>
{
    private Random random;
    private GtgFrame frame;
    private GameState gameState;
    private Field field;
    private BulletPool bullets;
    private TurretPool turrets;
    private HomingLaserPool homingLasers;
    private Player player;
    private ParticlePool particles;
    private Sound sound;
    private PlayerHomingLaserPool playerHomingLasers;
    private GearEdgeShape edgeShape;
    private Quaternion rollQuaternion = new Quaternion();
    public MiddleEnemyPool(int n, GtgFrame frame, GameState gameState, Field field, TurretPool turrets, HomingLaserPool homingLasers, BulletPool bullets, Player player, ParticlePool particles, Sound sound) : base(n, () => new MiddleEnemy())
    {
        this.frame = frame;
        this.gameState = gameState;
        this.field = field;
        this.bullets = bullets;
        this.turrets = turrets;
        this.homingLasers = homingLasers;
        this.player = player;
        this.particles = particles;
        this.sound = sound;
        shape = new GearShape(frame, 1, 0.25f, 0.75f, new Color(250, 100, 50, 200), new Color(0, 0, 0, 200), true);
        edgeShape = new GearEdgeShape(frame, 1, 0.5f, 0.5f, 0.3f);
        rollQuaternion = (Quaternion.CreateFromYawPitchRoll(0, 0, 0.2f)).Copy();
    }

    public void SetParams(PlayerHomingLaserPool playerHomingLasers)
    {
        this.playerHomingLasers = playerHomingLasers;
    }

    public void SetRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    public void Add(float radius, float angle, float angleRate, MiddleEnemyWeaponType weaponType, float fireInterval)
    {
        MiddleEnemy a = new MiddleEnemy();
        {
            a.Pos.X = (float)Math.Sin(angle) * radius;
            a.BasePos.X = a.Pos.X;
        }

        {
            a.Pos.Y = (float)Math.Cos(angle) * radius;
            a.BasePos.Y = a.Pos.Y;
        }

        {
            a.Pos.Z = Field.BackDepth;
            a.BasePos.Z = a.Pos.Z;
        }

        a.Scale = 10;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color.X = 1;
        a.Color.Y = 0.25f;
        if (weaponType == MiddleEnemyWeaponType.Laser)
            a.Color.Z = 0.75f;
        else
            a.Color.Z = 0.25f;
        a.Color.W = 0;
        a.Ticks = 0;
        a.TurretCount = 0;
        a.Shield = 10;
        a.Weapon = weaponType;
        a.Radius = radius;
        a.Angle = angle;
        a.AngleRate = angleRate;
        if (weaponType == MiddleEnemyWeaponType.HomingLaser)
            fireInterval *= 2;
        {
            a.FireTicks = fireInterval;
            a.FireInterval = a.FireTicks;
        }

        a.HasTargetZ = false;
        AddT(a);
    }

    public void AddBoss(float radius, float angle, float angleRate, MiddleEnemyWeaponType weaponType, float fireInterval, float targetZ, float fireTicksRatio)
    {
        MiddleEnemy a = new MiddleEnemy();
        {
            a.Pos.X = (float)Math.Sin(angle) * radius;
            a.BasePos.X = a.Pos.X;
        }

        {
            a.Pos.Y = (float)Math.Cos(angle) * radius;
            a.BasePos.Y = a.Pos.Y;
        }

        {
            a.Pos.Z = Field.FrontDepth;
            a.BasePos.Z = a.Pos.Z;
        }

        a.Scale = 15;
        a.Orientation = (Quaternion.Identity).Copy();
        a.Color.X = 1;
        a.Color.Y = 0.5f;
        if (weaponType == MiddleEnemyWeaponType.Laser)
            a.Color.Z = 0.3f;
        else
            a.Color.Z = 0.1f;
        a.Color.W = 0;
        a.Ticks = 0;
        a.TurretCount = 0;
        a.Shield = 20;
        a.Weapon = weaponType;
        a.Radius = radius;
        a.Angle = angle;
        a.AngleRate = angleRate;
        if (weaponType == MiddleEnemyWeaponType.HomingLaser)
            fireInterval *= 2;
        a.FireInterval = fireInterval;
        a.FireTicks = fireInterval * fireTicksRatio;
        a.HasTargetZ = true;
        a.TargetZ = targetZ;
        a.IsFireInvervalAffectedWithCount = true;
        AddT(a);
    }

    public override bool UpdateT(MiddleEnemy a)
    {
        if (a.AngleRate != 0)
        {
            a.Orientation *= rollQuaternion;
            a.Angle += a.AngleRate * Stage.GameSpeedSqrt;
            a.BasePos.X = (float)Math.Sin(a.Angle) * a.Radius;
            a.BasePos.Y = (float)Math.Cos(a.Angle) * a.Radius;
        }

        if (!a.HasTargetZ)
        {
            a.BasePos.Z += Stage.GameSpeedSqrt * 0.5f;
        }
        else
        {
            a.BasePos.Z += (a.TargetZ + (float)Math.Sin(a.Ticks * 0.1f) * 50.0f - a.BasePos.Z) * 0.02f;
        }

        a.Ticks++;
        a.Pos = (a.BasePos + a.ShakeOffset).Copy();
        a.ShakeOffset *= 0.9f;
        if (a.Pos.Z < Field.FireBoundaryDepth * 2 && a.TurretCount < MiddleEnemy.TurretMaxCount)
            a.FireTicks -= Stage.GameSpeed;
        if (a.FireTicks <= 0)
        {
            Vector3 tp = new Vector3(a.BasePos.X, a.BasePos.Y, 0);
            tp.Normalize();
            if (a.Weapon == MiddleEnemyWeaponType.HomingLaser)
            {
                tp = (-tp * Tube.Radius).Copy();
                homingLasers.Add((a.Pos).Copy(), (tp).Copy(), 5.0f);
            }
            else
            {
                tp = (-tp * (Tube.Radius * 10.0f)).Copy();
                Vector3 tv = (player.Pos).Copy();
                tv.Z = 0;
                tv -= tp;
                tv.Normalize();
                int ti = turrets.Add((tp).Copy(), (tv).Copy(), (a.Weapon == MiddleEnemyWeaponType.Laser));
                if (ti >= 0)
                {
                    {
                        int[] tip = a.TurretIndexes;
                        tip[(a.TurretCount)] = ti;
                    }

                    a.TurretCount++;
                }
            }

            a.FireTicks = a.FireInterval;
            if (a.IsFireInvervalAffectedWithCount)
                a.FireTicks *= actorCount;
        }

        switch (a.Weapon)
        {
            case MiddleEnemyWeaponType.Laser:
                for (int i = 0; i < a.TurretCount; i++)
                {
                    {
                        int[] tip = a.TurretIndexes;
                        turrets.Actors[(tip[(i)])].SourcePos = (a.Pos).Copy();
                    }
                }

                break;
        }

        return (a.Pos.Z < Field.FrontDepth);
    }

    public bool CheckHit(Vector3 pos)
    {
        bool isHit = false;
        for (int i = 0; i < actorCount; i++)
        {
            if (Vector3.Distance((Actors[(i)].Pos).Copy(), (pos).Copy()) < Actors[(i)].Scale * 1.5f)
            {
                isHit = true;
                if (Hit(i))
                    i--;
            }
        }

        return isHit;
    }

    public bool Hit(int i)
    {
        Actors[(i)].Shield--;
        if (Actors[(i)].Shield <= 0)
        {
            particles.AddintVector3floatfloatfloatfloatfloatfloatfloat(40, (Actors[(i)].Pos).Copy(), 2, 20, Actors[(i)].Scale * 4, 1, 0.8f, 0.2f, 0.8f);
            particles.AddintVector3floatfloatfloatfloatfloatfloatfloat(160, (Actors[(i)].Pos).Copy(), 20, 30, Actors[(i)].Scale * 3, 1, 0.8f, 0.2f, 0.6f);
            gameState.AddScoreintVector3float(1000, (Actors[(i)].Pos).Copy(), 10);
            sound.PlaySe3D("MiddleEnemyDestroyed", (Actors[(i)].Pos).Copy());
            Remove(i);
            return true;
        }
        else
        {
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(5, (Actors[(i)].Pos).Copy(), new Vector3(0, 0, 10), 0.75f, Actors[(i)].Scale, 1, 1, 0, 0.5f);
            Actors[(i)].ShakeOffset.X = (float)random.NextDouble() * 5 - 2.5f;
            Actors[(i)].ShakeOffset.Y = (float)random.NextDouble() * 5 - 2.5f;
            return false;
        }
    }

    public int GetNearest(float x, float y, float distance)
    {
        int ri = -1, i = 0;
        float md = distance;
        ForEach((MiddleEnemy a) =>
        {
            float d = Math.Abs(a.Pos.X - x) + Math.Abs(a.Pos.Y - y);
            if (d < md && a.Pos.Z < 0)
            {
                ri = i;
                md = d;
            }

            i++;
        });
        return ri;
    }

    public void FinishBossStage()
    {
        for (int i = 0; i < actorCount; i++)
        {
            Actors[(i)].TargetZ = Field.FrontDepth * 5;
        }
    }

    public override void DrawT(MiddleEnemy a)
    {
        Shape.AddInstance((a.Pos).Copy(), a.Scale, (a.Orientation).Copy(), (a.Color).Copy());
    }

    public void DrawEdge()
    {
        DrawShape(edgeShape);
    }

    public override void Remove(int ri)
    {
        while (Actors[(ri)].TurretCount > 0)
        {
            {
                int[] tip = Actors[(ri)].TurretIndexes;
                turrets.Remove(tip[(0)]);
            }
        }

        playerHomingLasers.OnEnemyRemoved(true, ri);
        base.Remove(ri);
    }

    public void OnTurretRemovedint(int rti)
    {
        ForEach((MiddleEnemy a) =>
        {
            OnTurretRemovedMiddleEnemyint(a, rti);
        });
    }

    private void OnTurretRemovedMiddleEnemyint(MiddleEnemy a, int rti)
    {
        {
            int[] tip = a.TurretIndexes;
            for (int i = 0; i < a.TurretCount; i++)
            {
                int ti = tip[(i)];
                if (ti > rti)
                {
                    tip[(i)]--;
                }
                else if (ti == rti)
                {
                    a.TurretCount--;
                    tip[(i)] = tip[(a.TurretCount)];
                    i--;
                }
            }
        }
    }
}

public class MiddleEnemy : ActorCopy
{
    public MiddleEnemy()
    {
    }

    public const int TurretMaxCount = 8;
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public int Ticks;
    public int[] TurretIndexes = GtgArrays.Make(TurretMaxCount, () => 0);
    public int TurretCount;
    public int Shield;
    public MiddleEnemyWeaponType Weapon;
    public Quaternion Roll = new Quaternion();
    public float Radius;
    public float Angle;
    public float AngleRate;
    public float FireInterval;
    public float FireTicks;
    public bool HasTargetZ;
    public float TargetZ, VelZ;
    public Vector3 BasePos = new Vector3(), ShakeOffset = new Vector3();
    public bool IsFireInvervalAffectedWithCount;
    public MiddleEnemy Copy()
    {
        return new MiddleEnemy
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy(),
            Ticks = Ticks,
            TurretIndexes = GtgArrays.CopyInts(TurretIndexes),
            TurretCount = TurretCount,
            Shield = Shield,
            Weapon = Weapon,
            Roll = Roll.Copy(),
            Radius = Radius,
            Angle = Angle,
            AngleRate = AngleRate,
            FireInterval = FireInterval,
            FireTicks = FireTicks,
            HasTargetZ = HasTargetZ,
            TargetZ = TargetZ,
            VelZ = VelZ,
            BasePos = BasePos.Copy(),
            ShakeOffset = ShakeOffset.Copy(),
            IsFireInvervalAffectedWithCount = IsFireInvervalAffectedWithCount
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}

public enum MiddleEnemyWeaponType
{
    Laser,
    HomingLaser
};
