// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class BallPool : ActorPool<Ball>
{
    private const float shotHitHeight = 8.0f;
    private static Random random;
    private MmFrame frame;
    private TurretPool turrets;
    private BulletPool bullets;
    private ParticlePool particles;
    private BonusPool bonuses;
    private Player player;
    private Field field;
    private Stage stage;
    private Sound sound;
    private BlurNormalTextureCubeListShape shape;
    private Vector4 bv = new Vector4();
    private TriangleListShape stateShape;
    public static void SetRandomSeed(Int32 s)
    {
        random = new Random(s);
    }

    public BallPool(int n, MmFrame frame, TurretPool turrets, BulletPool bullets, ParticlePool particles, BonusPool bonuses, Player player, Field field, Stage stage, Sound sound) : base(n, () => new Ball())
    {
        this.frame = frame;
        this.turrets = turrets;
        this.bullets = bullets;
        this.particles = particles;
        this.bonuses = bonuses;
        this.player = player;
        this.field = field;
        this.stage = stage;
        this.sound = sound;
        shape = BallShape.CreateShape(frame);
        stateShape = new TriangleListShape(frame);
        stateShape.Initializeintbytebytebytebyte(64, 10, 10, 10, 128);
    }

    public override void UpdateT(Ball a)
    {
        a.Cnt++;
        if (!field.Contains((a.Pos2).Copy()))
        {
            if (a.IsSub)
            {
                RemoveT(a);
                return;
            }

            if (a.Movement.Mode == BallMovementMovementMode.Away)
                a.Movement.Mode = BallMovementMovementMode.Approach;
            a.IsActivated = false;
            return;
        }

        a.IsActivated = true;
        a.State.AddMasslessForce(new Vector3(0, 0, -2.0f));
        float td, tr;
        if (a.Movement.Mode == BallMovementMovementMode.StayBullet)
        {
            Bullet tb = (bullets.Get(a.Movement.BulletId)).Copy();
            Vector2 tp = new Vector2();
            tp.X = tb.Pos.X + (float)Math.Sin(tb.Deg) * a.Movement.Range;
            tp.Y = tb.Pos.Y + (float)Math.Cos(tb.Deg) * a.Movement.Range;
            td = (float)Math.Atan2(tp.X - a.Pos2.X, tp.Y - a.Pos2.Y);
            tr = (float)Math.Sqrt(Vector2.Distance((tp).Copy(), (a.Pos2).Copy()));
        }
        else
        {
            td = (float)Math.Atan2(player.Pos.X - a.State.Pos.X, player.Pos.Y - a.State.Pos.Y);
            tr = Vector3.Distance((player.Pos).Copy(), (a.State.Pos).Copy());
        }

        float fr = 0;
        switch (a.Movement.Mode)
        {
            case BallMovementMovementMode.Approach:
                fr = a.Movement.Speed;
                if (tr < a.Movement.Range)
                {
                    a.Movement.Mode = BallMovementMovementMode.Stay;
                    a.Movement.StayCnt = a.Movement.StayDuration;
                }

                break;
            case BallMovementMovementMode.Stay:
                fr = (tr - a.Movement.Range) * a.Movement.Speed * 0.25f;
                a.Movement.StayCnt--;
                if (a.Movement.StayCnt <= 0)
                    a.Movement.Mode = BallMovementMovementMode.Away;
                break;
            case BallMovementMovementMode.Away:
                fr = -a.Movement.Speed;
                break;
            case BallMovementMovementMode.StayBullet:
                fr = tr * a.Movement.Speed * 0.25f;
                break;
            case BallMovementMovementMode.None:
                break;
        }

        a.State.AddMasslessForce(new Vector3((float)Math.Sin(td) * fr, (float)Math.Cos(td) * fr, 0));
        a.State.Velocity *= 0.98f;
        if (a.IsSub)
        {
            a.State.Radius -= a.Hardness;
            if (a.State.Radius < 0.25f)
            {
                RemoveT(a);
                return;
            }
        }

        if (a.NextBallId >= 0)
            SpringConstraint.Resolve(a.State.Pos, actors[(a.NextBallId)].State.Pos, a.State.InvMass, actors[(a.NextBallId)].State.InvMass, 0.1f, a.State.Radius + actors[(a.NextBallId)].State.Radius);
        a.State.Update();
        Vector3 v = (a.State.Velocity).Copy();
        if (v.Length() >= 10.0f)
        {
            RemoveT(a);
            return;
        }

        stage.CheckWallHit(a.State.Pos, v);
        a.State.Velocity = (v).Copy();
        if (a.State.Pos.Z < a.State.Radius && a.State.Velocity.Z < 0)
        {
            a.State.ResolveAngleRate((a.State.Velocity).Copy(), (Vector3.Up).Copy());
            a.State.Pos.Z = a.State.Radius;
            v.Z *= -0.75f;
            a.State.Velocity = (v).Copy();
        }

        a.Pos2.X = a.State.Pos.X;
        a.Pos2.Y = a.State.Pos.Y;
        a.BlurVelocity += (a.State.Velocity - a.BlurVelocity) * 0.1f;
    }

    public void UpdateTurrets()
    {
        Vector3 tp = new Vector3();
        ForEach((Ball a) =>
        {
            if (a.TurretActivateCnt >= 0)
            {
                a.TurretActivateCnt--;
                if (a.TurretActivateCnt < 0)
                {
                    turrets.Deactivate(a.TurretId);
                    int nid = a.NextBallId;
                    if (nid < 0)
                        nid = a.RootBallId;
                    ActivateTurret(nid);
                }
            }

            if (a.IsActivated && a.TurretId >= 0)
            {
                tp.X = a.State.Pos.X;
                tp.Y = a.State.Pos.Y;
                tp.Z = a.State.Pos.Z - a.State.Radius * 0.75f;
                float td = (float)Math.Atan2(player.Pos.X - tp.X, player.Pos.Y - tp.Y);
                float vz = (player.Pos.Z - tp.Z) / (Vector2.Distance((player.Pos2).Copy(), (a.Pos2).Copy()) + 0.0001f);
                if (vz >= 0.25f)
                    vz = 0.25f;
                else if (vz <= -0.25f)
                    vz = -0.25f;
                turrets.Update(a.TurretId, (tp).Copy(), td, vz, a.State.Radius);
            }
        });
    }

    private void ActivateTurret(int id)
    {
        actors[(id)].TurretActivateCnt = actors[(id)].TurretActivateInterval;
        turrets.Activate(actors[(id)].TurretId);
    }

    public void SetNextBallId(int id, int nid)
    {
        actors[(id)].NextBallId = nid;
    }

    public void SetRootBallId(int id, int rid)
    {
        actors[(id)].RootBallId = rid;
    }

    public void AddDamageintVector3float(int id, Vector3 vel, float dmg)
    {
        actors[(id)].State.AddForce((vel * 100.0f).Copy());
        if (actors[(id)].RootBallId >= 0)
            AddDamageintfloat(actors[(id)].RootBallId, dmg);
        else
            AddDamageintfloat(id, dmg);
    }

    public void AddDamageintfloat(int id, float dmg)
    {
        if (actors[(id)].NextBallId >= 0)
            AddDamageintfloat(actors[(id)].NextBallId, dmg);
        float ar = actors[(id)].State.Radius * 0.02f * (4 / actors[(id)].Hardness) * dmg;
        actors[(id)].State.Radius += ar;
        actors[(id)].State.Pos.Z += ar;
        actors[(id)].State.PrevPos.Z += ar;
        if (actors[(id)].State.Radius >= actors[(id)].BurstRadius)
        {
            RemoveT(actors[(id)]);
            Destroy(actors[(id)]);
            return;
        }

        float dr = actors[(id)].State.Radius / actors[(id)].BaseRadius;
        actors[(id)].DamageFlashInterval = GameMath.integer((30.0f / dr / dr / dr)) + 1;
    }

    private void Destroy(Ball a)
    {
        Particle p = new Particle();
        Vector3 pp = new Vector3();
        Vector3 po = new Vector3();
        Quaternion qd = new Quaternion();
        float ps = a.State.Radius * 0.2f;
        for (int i = 0; i < a.State.Radius * 2.0f; i++)
        {
            pp.X = a.State.Pos.X;
            pp.Y = a.State.Pos.Y;
            pp.Z = a.State.Pos.Z;
            po.X = 1;
            po.Y = 0;
            po.Z = 0;
            qd = (Quaternion.CreateFromYawPitchRoll((float)(random.NextDouble() * Math.PI * 2), (float)(random.NextDouble() * Math.PI * 2), (float)(random.NextDouble() * Math.PI * 2))).Copy();
            po = (Vector3.TransformVector3Quaternion((po).Copy(), (qd).Copy())).Copy();
            pp += po;
            for (int j = 0; j < 2; j++)
            {
                p.Set((pp).Copy(), (qd).Copy(), 1 * ((j % 2) * 2 - 1), 30, 200, 150, 50, 0, 0, 0, ps);
                particles.Add(p);
            }

            for (int j = 0; j < 6; j++)
            {
                p.Set((pp).Copy(), (qd).Copy(), 3 * ((j % 2) * 2 - 1), 60, 250, 100, 50, 250, 200, 150, 0.25f);
                particles.Add(p);
            }

            Bonus b = new Bonus();
            b.Set((pp).Copy(), (qd).Copy(), 0.5f, 120);
            bonuses.Add(b);
        }

        if (player.IsInHyper)
            bullets.ChangeToBonus(a.GetId());
        ChangeToBonus(a.GetId());
        player.AddScoreintVector3(a.Score, (a.State.Pos).Copy());
        if (a.Score > 0)
        {
            if (a.BaseRadius < 1.25f)
                sound.PlaySe("BurstSmall");
            else
                sound.PlaySe("BurstBig");
        }

        if (a.BaseRadius > 5.0f)
        {
            stage.GoToNextStage();
        }
    }

    private void ChangeToBonus(int id)
    {
        Bonus b = new Bonus();
        ForEach((Ball a) =>
        {
            if (a.ParentBallId == id)
            {
                float d = (float)Math.Atan2(a.State.Velocity.X, a.State.Velocity.Y);
                Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), d)).Copy();
                b.Set((a.State.Pos).Copy(), (qd).Copy(), a.State.Velocity.Length(), 90);
                bonuses.Add(b);
                RemoveT(a);
            }
        });
    }

    public override void RemoveT(Ball a)
    {
        if (a.TurretId >= 0)
            turrets.Removeint(a.TurretId);
        base.RemoveT(a);
    }

    public void ResolveSpringConstraint(int id, Vector3 p, float r)
    {
        SpringConstraint.Resolve(p, actors[(id)].State.Pos, 0, actors[(id)].State.InvMass, 0.1f, actors[(id)].State.Radius + r);
    }

    public override void Draw()
    {
        stateShape.BeginAdd();
        ForEach((Ball a) =>
        {
            DrawT(a);
        });
        stateShape.EndAdd();
    }

    public void DrawState()
    {
        stateShape.DrawVector3((Vector3.Zero).Copy());
    }

    public override void DrawT(Ball a)
    {
        if (a.BaseRadius >= 2.0f && a.NextBallId < 0 && !a.IsSub)
            DrawMarker((a.State.Pos).Copy());
        bv.X = a.BlurVelocity.X;
        bv.Y = a.BlurVelocity.Y;
        bv.Z = a.BlurVelocity.Z;
        frame.Velocity = (bv * 25).Copy();
        if (a.IsSub)
            frame.EffectColor = new Vector4(0.5f, 0, 1, 0.25f);
        else if (a.Cnt % a.DamageFlashInterval < 1)
            frame.EffectColor = new Vector4(1, 0, 0, 0.75f);
        else
            frame.EffectColor = new Vector4(1, 0.5f, 0.25f, 1.0f);
        shape.DrawVector3Quaternionfloat((a.State.Pos).Copy(), (a.State.Rotation).Copy(), a.State.Radius);
    }

    private void DrawMarker(Vector3 p)
    {
        Vector4 sp = new Vector4();
        sp = (Vector4.TransformVector3Matrix((p).Copy(), (frame.ViewMatrix * frame.ProjMatrix).Copy())).Copy();
        if (sp.X < -80.0f || sp.X > 80.0f || sp.Y < -70.0f || sp.Y > 70.0f)
        {
            float x = sp.X;
            float y = sp.Y;
            float d = (float)Math.Atan2(x, y);
            float size = 1000.0f / (x * x + y * y + 1.0f);
            if (y < -1.5f)
            {
                x = x * -1.5f / y;
                y = -1.5f;
            }
            else if (y > 1.5f)
            {
                x = x * 1.5f / y;
                y = 1.5f;
            }

            if (x < -1.6f)
            {
                y = y * -1.6f / x;
                x = -1.6f;
            }
            else if (x > 1.6f)
            {
                y = y * 1.6f / x;
                x = 1.6f;
            }

            stateShape.Addfloatfloatfloat(x + (float)Math.Sin(d) * size * 3, y + (float)Math.Cos(d) * size * 3, 0);
            d += (float)Math.PI * 0.66f;
            stateShape.Addfloatfloatfloat(x + (float)Math.Sin(d) * size, y + (float)Math.Cos(d) * size, 0);
            d += (float)Math.PI * 0.66f;
            stateShape.Addfloatfloatfloat(x + (float)Math.Sin(d) * size, y + (float)Math.Cos(d) * size, 0);
        }
    }

    public void DrawReflection()
    {
        Vector3 p = new Vector3();
        ForEach((Ball a) =>
        {
            bv.X = a.BlurVelocity.X;
            bv.Y = a.BlurVelocity.Y;
            bv.Z = a.BlurVelocity.Z;
            p.X = a.State.Pos.X;
            p.Y = a.State.Pos.Y;
            p.Z = -a.State.Pos.Z;
            frame.Velocity = (bv * 32).Copy();
            shape.DrawVector3Quaternionfloat((p).Copy(), (a.State.Rotation).Copy(), a.State.Radius);
        });
    }

    public Appearance GetAppearance(Ball a, Vector2 p)
    {
        a.Appearance.Pos = (a.State.Pos).Copy();
        a.Appearance.Pos.X -= p.X;
        a.Appearance.Pos.Y -= p.Y;
        return (a.Appearance).Copy();
    }

    public int CheckHitShot(Vector2 p, float r)
    {
        int bid = -1;
        ForEach((Ball a) =>
        {
            if (bid == -1 && a.IsActivated && !a.IsSub && Math.Abs(p.X - a.Pos2.X) < 15 && Math.Abs(p.Y - a.Pos2.Y) < 15 && Vector2.Distance((a.Pos2).Copy(), (p).Copy()) <= r + a.State.Radius && a.State.Pos.Z - a.State.Radius < shotHitHeight)
                bid = a.GetId();
        });
        return bid;
    }

    public void CheckHitGrenade(Vector2 p, float r)
    {
        ForEach((Ball b) =>
        {
            if (b.IsActivated && !b.IsSub && Vector2.Distance((b.Pos2).Copy(), (p).Copy()) <= r + b.State.Radius && b.State.Pos.Z - b.State.Radius < shotHitHeight)
                AddDamageintVector3float(b.GetId(), (Vector3.Zero).Copy(), 1.0f);
        });
    }

    public void CheckCollisions()
    {
        for (int i = 0; i < actorNum; i++)
            if (actors[(i)].IsActivated && !actors[(i)].IsSub)
                for (int j = i + 1; j < actorNum; j++)
                    if (actors[(j)].IsActivated && !actors[(j)].IsSub)
                        actors[(i)].State.CheckCollision(actors[(j)].State);
    }

    protected override void OnIdChanged(int before, int after)
    {
        bullets.ChangeBallId(before, after);
        ChangeBallId(before, after);
    }

    public void ChangeTurretId(int before, int after)
    {
        ForEach((Ball a) =>
        {
            if (a.TurretId == before)
            {
                a.TurretId = after;
                return;
            }
        });
    }

    private void ChangeBallId(int before, int after)
    {
        ForEach((Ball a) =>
        {
            if (a.ParentBallId == before)
                a.ParentBallId = after;
            if (a.RootBallId == before)
                a.RootBallId = after;
            if (a.NextBallId == before)
                a.NextBallId = after;
        });
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }

    public void DestroyAll()
    {
        for (int i = 0; i < Length(); i++)
        {
            if (!isRemoved[(i)])
            {
                RemoveT(actors[(i)]);
                Destroy(actors[(i)]);
            }
        }
    }

    public int NextId
    {
        get
        {
            return actorNum;
        }
    }
}

public class Ball : Actor
{
    public CircleParticle State = new CircleParticle();
    public float BaseRadius;
    public float BurstRadius;
    public float Hardness;
    public int TurretId;
    public Vector3 BlurVelocity = new Vector3();
    public Vector2 Pos2 = new Vector2();
    public Appearance Appearance = new Appearance();
    public bool IsActivated;
    public BallMovement Movement = new BallMovement();
    public int Cnt;
    public int DamageFlashInterval;
    public int Score;
    public int ParentBallId;
    public bool IsSub;
    public int RootBallId;
    public int NextBallId;
    public int TurretActivateCnt;
    public int TurretActivateInterval;
    private int storedId;
    public void Clear()
    {
        State.Clear();
        BaseRadius = 1.0f;
        BurstRadius = 2.0f;
        Hardness = 1.0f;
        TurretId = -1;
        {
            BlurVelocity.Z = 0;
            BlurVelocity.Y = BlurVelocity.Z;
            BlurVelocity.X = BlurVelocity.Y;
        }

        {
            Pos2.Y = 0;
            Pos2.X = Pos2.Y;
        }

        Appearance.Clear();
        IsActivated = false;
        Movement.Clear();
        Cnt = 0;
        DamageFlashInterval = 99999;
        Score = 0;
        ParentBallId = -1;
        IsSub = false;
        RootBallId = -1;
        NextBallId = -1;
        TurretActivateCnt = -1;
        TurretActivateInterval = -1;
        storedId = -1;
    }

    public void Set(Vector3 p, float r, float br, float h, int bid)
    {
        State.Set((p).Copy());
        Pos2.X = p.X;
        Pos2.Y = p.Y;
        {
            BaseRadius = r;
            State.Radius = BaseRadius;
        }

        BurstRadius = br;
        Hardness = h;
        State.Mass = r * r * 10.0f;
        State.Elasticity = 0.75f;
        State.Friction = 0.01f;
        State.IsFixed = false;
        {
            State.AngleRate = (Quaternion.Identity).Copy();
            State.Rotation = (State.AngleRate).Copy();
        }

        {
            BlurVelocity.Z = 0;
            BlurVelocity.Y = BlurVelocity.Z;
            BlurVelocity.X = BlurVelocity.Y;
        }

        IsActivated = false;
        Cnt = 0;
        DamageFlashInterval = 99999;
        ParentBallId = bid;
        IsSub = (bid >= 0);
        RootBallId = -1;
        NextBallId = -1;
        TurretActivateCnt = -1;
        TurretActivateInterval = -1;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Ball Copy()
    {
        return new Ball
        {
            State = State.Copy(),
            BaseRadius = BaseRadius,
            BurstRadius = BurstRadius,
            Hardness = Hardness,
            TurretId = TurretId,
            BlurVelocity = BlurVelocity.Copy(),
            Pos2 = Pos2.Copy(),
            Appearance = Appearance.Copy(),
            IsActivated = IsActivated,
            Movement = Movement.Copy(),
            Cnt = Cnt,
            DamageFlashInterval = DamageFlashInterval,
            Score = Score,
            ParentBallId = ParentBallId,
            IsSub = IsSub,
            RootBallId = RootBallId,
            NextBallId = NextBallId,
            TurretActivateCnt = TurretActivateCnt,
            TurretActivateInterval = TurretActivateInterval,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}

public class BallMovement
{
    public float Range;
    public float Speed;
    public int StayDuration;
    public BallMovementMovementMode Mode;
    public int StayCnt;
    public int BulletId;
    public void Clear()
    {
        Range = 1.0f;
        Speed = 1.0f;
        StayDuration = 0;
        Mode = BallMovementMovementMode.Approach;
        StayCnt = 0;
        BulletId = -1;
    }

    public BallMovement Copy()
    {
        return new BallMovement
        {
            Range = Range,
            Speed = Speed,
            StayDuration = StayDuration,
            Mode = Mode,
            StayCnt = StayCnt,
            BulletId = BulletId
        };
    }
}

public enum BallMovementMovementMode
{
    Approach,
    Stay,
    Away,
    StayBullet,
    None,
};
