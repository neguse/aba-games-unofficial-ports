// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class TurretPool : ActorPool<Turret>
{
    private const int maxTurretLinkedNum = 10;
    private BulletPool bullets;
    private BallPool balls;
    public TurretPool(int n, BulletPool bullets) : base(n, () => new Turret())
    {
        this.bullets = bullets;
    }

    public void SetParams(BallPool balls)
    {
        this.balls = balls;
    }

    public void Update(int id, Vector3 p, float d, float vz, float r)
    {
        int i = id;
        int c = 0;
        while (i >= 0)
        {
            actors[(i)].Update((p).Copy(), d, vz, r, bullets, balls);
            i = actors[(i)].NextId;
            c++;
            if (c >= maxTurretLinkedNum)
                break;
        }
    }

    public override void Removeint(int id)
    {
        Turret a = (actors[(id)]).Copy();
        if (a.BulletId >= 0)
            bullets.Removeint(a.BulletId);
        base.Removeint(id);
        if (a.NextId >= 0)
            Removeint(a.NextId);
    }

    public void Activate(int id)
    {
        int i = id;
        int c = 0;
        while (i >= 0)
        {
            if (actors[(i)].BulletId >= 0)
                bullets.Activate(actors[(i)].BulletId);
            i = actors[(i)].NextId;
            c++;
            if (c >= maxTurretLinkedNum)
                break;
        }
    }

    public void Deactivate(int id)
    {
        int i = id;
        int c = 0;
        while (i >= 0)
        {
            if (actors[(i)].BulletId >= 0)
                bullets.Deactivate(actors[(i)].BulletId);
            i = actors[(i)].NextId;
            c++;
            if (c >= maxTurretLinkedNum)
                break;
        }
    }

    public void SetNextBallId(int id, int tc, int nid)
    {
        if (tc > 0)
            SetNextBallId(actors[(id)].NextId, tc - 1, nid);
        else
            actors[(id)].NextBallId = nid;
    }

    protected override void OnIdChanged(int before, int after)
    {
        balls.ChangeTurretId(before, after);
        ChangeTurretId(before, after);
    }

    public void ChangeBulletId(int before, int after)
    {
        ForEach((Turret a) =>
        {
            if (a.BulletId == before)
            {
                a.BulletId = after;
                return;
            }
        });
    }

    private void ChangeTurretId(int before, int after)
    {
        ForEach((Turret a) =>
        {
            if (a.NextId == before)
            {
                a.NextId = after;
                return;
            }
        });
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }
}

public class Turret : Actor
{
    public TurretTurretType Type;
    public int BulletId;
    public int NextId;
    public float Deg;
    public float DegVel;
    public float FiringDeg;
    public float DegOfs;
    public int NextBallId;
    private int storedId;
    public void Clear()
    {
        Type = TurretTurretType.Aim;
        BulletId = -1;
        NextId = -1;
        {
            DegOfs = 0;
            FiringDeg = DegOfs;
            DegVel = FiringDeg;
            Deg = DegVel;
        }

        NextBallId = -1;
        storedId = -1;
    }

    public void Update(Vector3 p, float d, float vz, float r, BulletPool bullets, BallPool balls)
    {
        switch (Type)
        {
            case TurretTurretType.Aim:
                if (BulletId >= 0 && !bullets.IsInFiringInterval(BulletId))
                    Deg = d;
                p.X += (float)Math.Sin(Deg + DegOfs) * r;
                p.Y += (float)Math.Cos(Deg + DegOfs) * r;
                if (BulletId >= 0)
                    bullets.UpdateTopBullet(BulletId, (p).Copy(), Deg + FiringDeg, vz);
                break;
            case TurretTurretType.Roll:
                p.X += (float)Math.Sin(Deg) * r;
                p.Y += (float)Math.Cos(Deg) * r;
                if (BulletId >= 0)
                    bullets.UpdateTopBullet(BulletId, (p).Copy(), Deg + FiringDeg, vz);
                Deg += DegVel * SimulationTime.Step;
                break;
            case TurretTurretType.AimRoll:
                Deg = d + DegOfs;
                p.X += (float)Math.Sin(Deg) * r;
                p.Y += (float)Math.Cos(Deg) * r;
                if (BulletId >= 0)
                    bullets.UpdateTopBullet(BulletId, (p).Copy(), Deg + FiringDeg, vz);
                break;
        }

        if (NextBallId >= 0)
            balls.ResolveSpringConstraint(NextBallId, (p).Copy(), r);
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Turret Copy()
    {
        return new Turret
        {
            Type = Type,
            BulletId = BulletId,
            NextId = NextId,
            Deg = Deg,
            DegVel = DegVel,
            FiringDeg = FiringDeg,
            DegOfs = DegOfs,
            NextBallId = NextBallId,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}

public enum TurretTurretType
{
    Aim,
    Roll,
    AimRoll,
}
