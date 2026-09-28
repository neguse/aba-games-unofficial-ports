// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class BulletPool : ActorPool<Bullet>
{
    private MmFrame frame;
    private Field field;
    private WallPool walls;
    private ParticlePool particles;
    private BonusPool bonuses;
    private TurretPool turrets;
    private BallPool balls;
    private Player player;
    private BlurNormalTextureCubeListShape shape;
    private QuadListShape shadowShape;
    private Quaternion dirVel = new Quaternion();
    private Vector4 color = new Vector4(), havingCollisionColor = new Vector4();
    public BulletPool(int n, MmFrame frame, Field field, WallPool walls, ParticlePool particles, BonusPool bonuses, Player player) : base(n, () => new Bullet())
    {
        this.frame = frame;
        this.field = field;
        this.walls = walls;
        this.particles = particles;
        this.bonuses = bonuses;
        this.player = player;
        shape = new BlurNormalTextureCubeListShape(frame);
        shape.BeginAdd(1);
        shape.AddVector3Vector3float((Vector3.Zero).Copy(), (Vector3.Zero).Copy(), 0.5f);
        shape.EndAdd();
        shadowShape = new QuadListShape(frame);
        shadowShape.Initializeintbytebytebytebyte(n, 0, 0, 0, 160);
        dirVel = (Quaternion.CreateFromYawPitchRoll(0.1f, 0.05f, 0.025f)).Copy();
        color = new Vector4(0.15f, 0, 0.5f, 0.6f);
        havingCollisionColor = new Vector4(0.5f, 0, 0.5f, 0.1f);
    }

    public void SetParams(TurretPool turrets, BallPool balls)
    {
        this.turrets = turrets;
        this.balls = balls;
    }

    public override void UpdateT(Bullet a)
    {
        if (!a.IsActivated)
            return;
        a.PPos2.X = a.Pos2.X;
        a.PPos2.Y = a.Pos2.Y;
        float sp = a.Speed;
        if (a.Cnt < 30)
            sp = sp * a.Cnt / 30;
        a.Pos += a.Vel * sp * SimulationTime.Step;
        a.Pos2.X = a.Pos.X;
        a.Pos2.Y = a.Pos.Y;
        a.Cnt += SimulationTime.Step;
        if (!a.IsTop && (a.Pos.Z <= 0 || a.Pos.Z > 2.0f || a.Cnt >= 300 || !field.Contains((a.Pos2).Copy()) || walls.CheckHit((a.Pos).Copy())))
        {
            AddRemovedParticles(a);
            RemoveT(a);
            return;
        }

        a.HasCollision = (a.Pos.Z <= 2.0f);
        a.Dir *= MmTime.Rotation(dirVel);
        sp *= 20.0f;
        a.BlurVel.X = a.Vel.X * sp;
        a.BlurVel.Y = a.Vel.Y * sp;
        a.BlurVel.Z = a.Vel.Z * sp;
        bool v = a.Firing.IsValid;
        if (v && !a.Firing.IsFiringNear)
        {
            if (Vector2.Distance((a.Pos2).Copy(), (player.Pos2).Copy()) < 24.0f)
                v = false;
        }

        if (v)
        {
            if (!a.Firing.Update((a.Pos).Copy(), a.Deg, a.Speed, a.Vel.Z, a.NextFiring, a.SubBallAppearance, this, turrets, balls, a.BallId, a.GetId()))
            {
                RemoveT(a);
                return;
            }
        }
    }

    public bool CheckHit(Vector2 p, float r)
    {
        bool v = false;
        this.ForEach((Bullet a) =>
        {
            if (a.HasCollision && !a.IsTop && !v && Math.Abs(p.X - a.Pos2.X) < 2.0f && Math.Abs(p.Y - a.Pos2.Y) < 2.0f && MathUtil.CheckHitDist((p).Copy(), (a.Pos2).Copy(), (a.PPos2).Copy(), r))
                v = true;
        });
        return v;
    }

    private void AddRemovedParticles(Bullet a)
    {
        Particle p = new Particle();
        float d = (float)Math.Atan2(a.Vel.X, a.Vel.Y);
        Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), d)).Copy();
        p.SetFixed((a.Pos).Copy(), (qd).Copy(), a.Speed * 2, 60, 50, 0, 50, 200, 50, 200, 0.7f);
        particles.Add(p);
    }

    public void Activate(int id)
    {
        actors[(id)].IsActivated = true;
    }

    public void Deactivate(int id)
    {
        actors[(id)].IsActivated = false;
    }

    public override void DrawT(Bullet a)
    {
        if (a.IsTop)
            return;
        if (a.HasCollision)
            frame.BlurColor = (havingCollisionColor).Copy();
        else
            frame.BlurColor = (color).Copy();
        frame.Velocity = (a.BlurVel * a.BlurLength).Copy();
        frame.BlurThickness = a.BlurThick;
        shape.DrawVector3Quaternionfloat((a.Pos).Copy(), (a.Dir).Copy(), a.Size);
    }

    public void DrawShadow()
    {
        shadowShape.BeginAdd();
        Vector2 p = new Vector2();
        Vector4 lv = (frame.LightVector).Copy();
        ForEach((Bullet a) =>
        {
            if (!a.IsTop)
            {
                float w = 1.0f;
                float lr = a.Pos.Z / lv.Z;
                p.X = a.Pos.X - lv.X * lr;
                p.Y = a.Pos.Y - lv.Y * lr;
                w /= (1 + lr * 0.1f);
                shadowShape.Addfloatfloatfloat(p.X - w, p.Y - w, 0);
                shadowShape.Addfloatfloatfloat(p.X - w, p.Y + w, 0);
                shadowShape.Addfloatfloatfloat(p.X + w, p.Y + w, 0);
                shadowShape.Addfloatfloatfloat(p.X + w, p.Y - w, 0);
            }
        });
        shadowShape.EndAdd();
        shadowShape.Draw();
    }

    public void DrawReflection()
    {
        Vector3 p = new Vector3();
        ForEach((Bullet a) =>
        {
            p.X = a.Pos.X;
            p.Y = a.Pos.Y;
            p.Z = -a.Pos.Z;
            frame.Velocity = (a.BlurVel * a.BlurLength).Copy();
            frame.BlurThickness = a.BlurThick;
            shape.DrawVector3Quaternionfloat((a.Pos).Copy(), (a.Dir).Copy(), a.Size);
        });
    }

    public void UpdateTopBullet(int id, Vector3 p, float d, float vz)
    {
        actors[(id)].Pos = (p).Copy();
        actors[(id)].Deg = d;
        actors[(id)].Vel.Z = vz;
    }

    public bool IsInFiringInterval(int id)
    {
        return actors[(id)].Firing.IsInInterval;
    }

    public void ChangeBallId(int before, int after)
    {
        ForEach((Bullet a) =>
        {
            if (a.BallId == before)
                a.BallId = after;
        });
    }

    public void ChangeToBonus(int id)
    {
        ForEach((Bullet a) =>
        {
            if (a.BallId == id && !a.IsTop)
            {
                AddBonus(a);
            }
        });
    }

    public void ChangeToBonusInRange(Vector3 p, float range)
    {
        ForEach((Bullet a) =>
        {
            if (!a.IsTop && Vector3.Distance((p).Copy(), (a.Pos).Copy()) <= range)
            {
                AddBonus(a);
            }
        });
    }

    public void ChangeToBonusAll()
    {
        ForEach((Bullet a) =>
        {
            if (!a.IsTop)
            {
                AddBonus(a);
            }
        });
    }

    private void AddBonus(Bullet a)
    {
        AddRemovedParticles(a);
        float d = (float)Math.Atan2(a.Vel.X, a.Vel.Y);
        Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), d)).Copy();
        Bonus b = new Bonus();
        b.Set((a.Pos).Copy(), (qd).Copy(), a.Speed, 90);
        bonuses.Add(b);
        RemoveT(a);
    }

    public void RemoveAll()
    {
        ForEach((Bullet a) =>
        {
            if (!a.IsTop)
            {
                AddRemovedParticles(a);
                RemoveT(a);
            }
        });
    }

    protected override void OnIdChanged(int before, int after)
    {
        turrets.ChangeBulletId(before, after);
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }
}

public class Bullet : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector2 Pos2 = new Vector2();
    public Vector2 PPos2 = new Vector2();
    public Vector3 Vel = new Vector3();
    public float Deg;
    public float Speed;
    public Vector4 BlurVel = new Vector4();
    public Firing Firing = new Firing();
    public Firing NextFiring = new Firing();
    public BallAppearance SubBallAppearance = new BallAppearance();
    public bool IsTop;
    public Quaternion Dir = new Quaternion();
    public bool HasCollision;
    public float Cnt;
    public int BallId;
    public float Size;
    public float BlurLength;
    public float BlurThick;
    public bool IsActivated;
    private int storedId;
    public void Clear()
    {
        {
            Pos.Z = 0;
            Pos.Y = Pos.Z;
            Pos.X = Pos.Y;
        }

        {
            Pos2.Y = 0;
            Pos2.X = Pos2.Y;
        }

        {
            PPos2.Y = 0;
            PPos2.X = PPos2.Y;
        }

        {
            Vel.Z = 0;
            Vel.Y = Vel.Z;
            Vel.X = Vel.Y;
        }

        Deg = 0;
        Speed = 1.0f;
        {
            BlurVel.W = 0;
            BlurVel.Z = BlurVel.W;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        Firing.Clear();
        NextFiring.Clear();
        SubBallAppearance.Clear();
        IsTop = false;
        Dir = (Quaternion.Identity).Copy();
        HasCollision = false;
        Cnt = 0;
        BallId = -1;
        Size = 1.0f;
        BlurLength = 1.0f;
        BlurThick = 0.1f;
        IsActivated = false;
        storedId = -1;
    }

    public void Set(Vector3 p, float d, float s, float vz, int bid, float sz, float bl, float bt)
    {
        Pos = (p).Copy();
        {
            PPos2.X = p.X;
            Pos2.X = PPos2.X;
        }

        {
            PPos2.Y = p.Y;
            Pos2.Y = PPos2.Y;
        }

        Deg = d;
        Speed = s;
        float vr = (float)Math.Sqrt(1 - vz * vz);
        Vel.X = (float)Math.Sin(d) * vr;
        Vel.Y = (float)Math.Cos(d) * vr;
        Vel.Z = vz * vr;
        BallId = bid;
        Size = sz;
        BlurLength = bl;
        BlurThick = bt;
        {
            BlurVel.W = 0;
            BlurVel.Z = BlurVel.W;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        Dir = (Quaternion.Identity).Copy();
        HasCollision = false;
        Cnt = 0;
        IsActivated = true;
        storedId = -1;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Bullet Copy()
    {
        return new Bullet
        {
            Pos = Pos.Copy(),
            Pos2 = Pos2.Copy(),
            PPos2 = PPos2.Copy(),
            Vel = Vel.Copy(),
            Deg = Deg,
            Speed = Speed,
            BlurVel = BlurVel.Copy(),
            Firing = Firing.Copy(),
            NextFiring = NextFiring.Copy(),
            SubBallAppearance = SubBallAppearance.Copy(),
            IsTop = IsTop,
            Dir = Dir.Copy(),
            HasCollision = HasCollision,
            Cnt = Cnt,
            BallId = BallId,
            Size = Size,
            BlurLength = BlurLength,
            BlurThick = BlurThick,
            IsActivated = IsActivated,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}

public class Firing
{
    public bool IsValid;
    public bool IsMorphed;
    public bool IsInInterval;
    public int IntervalCnt;
    public bool IsFiringNear;
    private int num;
    private int interval;
    private int delay;
    private int cnt;
    private float speed;
    private float baseSpeed;
    private float deg;
    private float degVel;
    private float baseDegVel;
    private FiringSprayPattern storedSprayPattern;
    private float speedVel;
    private float size;
    private float blurLength;
    private float blurThick;
    private bool isFiringBalls;
    public void Clear()
    {
        IsValid = false;
        IsMorphed = false;
        IsInInterval = false;
        IntervalCnt = 0;
        IsFiringNear = true;
        num = 1;
        interval = 120;
        delay = 0;
        cnt = 0;
        {
            baseSpeed = 1.0f;
            speed = baseSpeed;
        }

        {
            baseDegVel = 0;
            degVel = baseDegVel;
            deg = degVel;
        }

        storedSprayPattern = FiringSprayPattern.Nway;
        speedVel = 0;
        size = 1.0f;
        blurLength = 1.0f;
        blurThick = 0.1f;
        isFiringBalls = false;
    }

    public void Setfloatfloatintintfloatfloatfloat(float rank, float speedRatio, int type, int intervalCnt, float sz, float bl, float bt)
    {
        float r = rank;
        if (type == 3)
            r /= 5;
        float nr = r * (0.3f + (float)Stage.Random.NextDouble() * 0.3f);
        r -= nr;
        if (r < 0)
            r = 0;
        float ir = r * (0.5f + (float)Stage.Random.NextDouble() * 0.5f);
        float sr = r - ir;
        num = GameMath.integer((float)Math.Sqrt(nr * 3 + 1));
        baseSpeed = 0.25f + (float)Math.Sqrt(sr * 0.1f);
        baseSpeed *= speedRatio;
        interval = 10 + GameMath.integer((float)(120.0f / Math.Sqrt(ir + 1)));
        delay = GameMath.integer((interval * Stage.Random.NextDouble() / num));
        {
            degVel = 0;
            deg = degVel;
        }

        baseDegVel = (float)(Math.PI * Stage.Random.NextDouble() / num);
        float svr = (float)Stage.Random.NextDouble() * 1.2f;
        speed = 0;
        speedVel = baseSpeed * svr / num;
        int tr = Stage.Random.Nextint(6);
        if (type == 4)
            delay = interval / num;
        switch (tr)
        {
            case 0:
            case 1:
                storedSprayPattern = FiringSprayPattern.Nway;
                break;
            case 2:
                storedSprayPattern = FiringSprayPattern.LRAlt;
                degVel /= 2;
                break;
            case 3:
                storedSprayPattern = FiringSprayPattern.LRAtATime;
                num = num / 2 + 1;
                delay *= 2;
                baseDegVel *= 2;
                speedVel *= 2;
                break;
            case 4:
                storedSprayPattern = FiringSprayPattern.LRAltRev;
                degVel /= 2;
                break;
            case 5:
                storedSprayPattern = FiringSprayPattern.LRAtATimeRev;
                num = num / 2 + 1;
                delay *= 2;
                baseDegVel *= 2;
                speedVel *= 2;
                break;
        }

        switch (type)
        {
            case 0:
            case 4:
                baseDegVel /= 2;
                break;
            case 1:
                delay /= 4;
                baseDegVel /= 2;
                if (svr < 0.25f)
                    speedVel *= 1.5f;
                break;
            case 2:
                baseDegVel /= 2;
                if (svr < 0.25f)
                    speedVel *= 2.0f;
                break;
            case 3:
                interval /= 5;
                delay = 0;
                baseDegVel /= 4;
                if (svr < 0.25f)
                    speedVel *= 2.0f;
                break;
        }

        cnt = -2;
        this.IntervalCnt = intervalCnt;
        size = sz;
        blurLength = bl;
        blurThick = bt;
        IsInInterval = false;
        isFiringBalls = false;
        IsFiringNear = true;
    }

    public void Setintfloat(int interval, float sp)
    {
        this.interval = interval;
        speed = 0;
        baseSpeed = sp;
        speedVel = 0;
        {
            baseDegVel = 0;
            degVel = baseDegVel;
            deg = degVel;
        }

        num = 1;
        cnt = -2;
        IntervalCnt = 0;
        storedSprayPattern = FiringSprayPattern.Nway;
        delay = 0;
        IsInInterval = false;
        isFiringBalls = true;
        IsFiringNear = true;
    }

    public void SetIntervalRatio(float intervalRatio)
    {
        cnt = GameMath.integer((interval * intervalRatio)) - 2;
        StartInterval();
    }

    public bool Update(Vector3 p, float d, float s, float vz, Firing firing, BallAppearance appearance, BulletPool bullets, TurretPool turrets, BallPool balls, int bid, int id)
    {
        if (!SimulationTime.Emit) return true;
        cnt++;
        if (cnt < 0)
            return true;
        int c = cnt % interval;
        int n;
        if (delay == 0)
        {
            if (c > 0)
            {
                IsInInterval = false;
                return !IsMorphed;
            }

            IsInInterval = true;
            n = num;
            StartInterval();
        }
        else
        {
            if (GameMath.integer(((c + delay - 1) / delay)) >= num)
            {
                IsInInterval = false;
                return !IsMorphed;
            }

            IsInInterval = true;
            if (c % delay > 0)
                return true;
            if (c == 0)
                StartInterval();
            n = 1;
        }

        for (int i = 0; i < n; i++)
        {
            if (isFiringBalls)
            {
                FireBall((p).Copy(), d, speed, appearance, balls, turrets, bullets, bid, id);
            }
            else if (p.Z > 0.0f && p.Z < 2.0f)
            {
                Fire((p).Copy(), d + deg, s + speed, vz, firing, bullets, bid);
                if ((storedSprayPattern == FiringSprayPattern.LRAtATime || storedSprayPattern == FiringSprayPattern.LRAtATimeRev) && deg > 0.01f)
                    Fire((p).Copy(), d - deg, s + speed, vz, firing, bullets, bid);
            }

            deg += degVel;
            speed += speedVel;
        }

        return true;
    }

    private void Fire(Vector3 p, float d, float s, float vz, Firing firing, BulletPool bullets, int bid)
    {
        if (bullets.RemainingActorNum < 16)
            return;
        Bullet b = new Bullet();
        b.Set((p).Copy(), d, s, vz, bid, size, blurLength, blurThick);
        b.IsTop = false;
        b.Firing = (firing).Copy();
        b.Firing.IntervalCnt = IntervalCnt - 1;
        b.NextFiring.IsValid = false;
        bullets.Add(b);
    }

    private void FireBall(Vector3 p, float d, float s, BallAppearance appearance, BallPool balls, TurretPool turrets, BulletPool bullets, int bid, int id)
    {
        appearance.Firing.degVel *= -1;
        appearance.NextFiring.degVel *= -1;
        appearance.Firing.IntervalCnt++;
        appearance.NextFiring.IntervalCnt++;
        if (!balls.IsAbleToAdd)
            return;
        Ball b = (appearance.CreateBall((p).Copy(), balls, turrets, bullets, bid, appearance)).Copy();
        b.Movement.BulletId = id;
        b.State.AddMasslessForce(new Vector3((float)Math.Sin(d) * s, (float)Math.Cos(d) * s, 0));
        balls.Add(b);
    }

    private void StartInterval()
    {
        IntervalCnt++;
        int ic = (IntervalCnt % 2) * 2 - 1;
        speed = baseSpeed - (speedVel * (num - 1) / 2);
        switch (storedSprayPattern)
        {
            case FiringSprayPattern.Nway:
                deg = ic * (baseDegVel * (num - 1)) / 2;
                degVel = -ic * baseDegVel;
                break;
            case FiringSprayPattern.LRAlt:
                degVel = ic * baseDegVel;
                deg = 0;
                break;
            case FiringSprayPattern.LRAltRev:
                degVel = ic * baseDegVel;
                deg = -degVel * (num - 1);
                break;
            case FiringSprayPattern.LRAtATime:
                degVel = baseDegVel;
                deg = 0;
                break;
            case FiringSprayPattern.LRAtATimeRev:
                degVel = -baseDegVel;
                deg = 0;
                deg += -degVel * (num - 1);
                break;
        }
    }

    public Firing Copy()
    {
        return new Firing
        {
            IsValid = IsValid,
            IsMorphed = IsMorphed,
            IsInInterval = IsInInterval,
            IntervalCnt = IntervalCnt,
            IsFiringNear = IsFiringNear,
            num = num,
            interval = interval,
            delay = delay,
            cnt = cnt,
            speed = speed,
            baseSpeed = baseSpeed,
            deg = deg,
            degVel = degVel,
            baseDegVel = baseDegVel,
            storedSprayPattern = storedSprayPattern,
            speedVel = speedVel,
            size = size,
            blurLength = blurLength,
            blurThick = blurThick,
            isFiringBalls = isFiringBalls
        };
    }
}

public enum FiringSprayPattern
{
    Nway,
    LRAlt,
    LRAtATime,
    LRAltRev,
    LRAtATimeRev,
}
