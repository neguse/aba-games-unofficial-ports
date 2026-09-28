// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class ShotPool : ActorPool<Shot>
{
    private const float WIDTH = 0.3f;
    private MmFrame frame;
    private Field field;
    private BallPool balls;
    private WallPool walls;
    private ParticlePool particles;
    private BonusPool bonuses;
    private Player player;
    private Sound sound;
    private TriangleListShape shape;
    private Matrix vertexRotate = new Matrix();
    public ShotPool(int n, MmFrame frame, Field field, BallPool balls, WallPool walls, ParticlePool particles, BonusPool bonuses, Player player, Sound sound) : base(n, () => new Shot())
    {
        this.frame = frame;
        this.field = field;
        this.balls = balls;
        this.walls = walls;
        this.particles = particles;
        this.bonuses = bonuses;
        this.player = player;
        this.sound = sound;
        shape = new TriangleListShape(frame);
        shape.Initializeint(n * 8);
        vertexRotate = (Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI * 2 / 4)).Copy();
    }

    public override void Update()
    {
        shape.BeginAdd();
        base.Update();
        shape.EndAdd();
    }

    public override void UpdateT(Shot a)
    {
        a.Pos += a.Vel * SimulationTime.Step;
        a.Pos2.X = a.Pos.X;
        a.Pos2.Y = a.Pos.Y;
        if (!field.ContainsInner((a.Pos2).Copy()))
        {
            RemoveT(a);
            return;
        }

        if (walls.CheckHit((a.Pos).Copy()))
        {
            Particle p = new Particle();
            float d = (float)Math.Atan2(a.Pos.X - walls.HitPos.X, a.Pos.Y - walls.HitPos.Y);
            d = MathUtil.NormalizeDeg((a.Deg + (float)Math.PI) - d) / 2 + d;
            Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), d)).Copy();
            p.Set((a.Pos).Copy(), (qd).Copy(), a.Speed * 0.5f, 30, 0, 0, 50, 50, 50, 200, 0.15f);
            for (int i = 0; i < 3; i++)
                particles.Add(p);
            RemoveT(a);
            return;
        }

        int bid = balls.CheckHitShot((a.Pos2).Copy(), 1.0f);
        if (bid >= 0)
        {
            Particle p = new Particle();
            float d = (float)Math.Atan2(a.Pos.X - balls.Get(bid).Pos2.X, a.Pos.Y - balls.Get(bid).Pos2.Y);
            d = MathUtil.NormalizeDeg((a.Deg + (float)Math.PI) - d) / 2 + d;
            Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), d)).Copy();
            p.Set((a.Pos).Copy(), (qd).Copy(), a.Speed, 40, 250, 150, 150, 250, 100, 100, 0.3f);
            for (int i = 0; i < 5; i++)
                particles.Add(p);
            if (player.IsInHyper)
            {
                Bonus b = new Bonus();
                b.Set((a.Pos).Copy(), (qd).Copy(), 0.5f, 60);
                bonuses.Add(b);
            }

            balls.AddDamageintVector3float(bid, (a.Vel).Copy(), 1.0f);
            player.AddScoreint(1);
            sound.PlaySe("Hit");
            RemoveT(a);
            return;
        }

        a.BlurVel += (a.Vel - a.BlurVel) * SimulationTime.Blend(0.1f);
        AddShape(a);
    }

    private void AddShape(Shot a)
    {
        byte r, g, b;
        Vector3 bv = new Vector3(WIDTH, 0, 0);
        if (player.IsInHyper)
        {
            r = 255;
            g = 50;
            b = 50;
            bv.X *= 2.0f;
        }
        else
        {
            {
                b = 50;
                g = b;
                r = g;
            }
        }

        Vector3 v1 = new Vector3();
        Vector3 v2 = new Vector3();
        Quaternion qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), -a.Deg)).Copy();
        v2 = (Vector3.TransformVector3Quaternion((bv).Copy(), (qd).Copy())).Copy();
        Vector3 tp = (a.Pos - a.BlurVel * 10).Copy();
        for (int i = 0; i < 4; i++)
        {
            v1 = (v2).Copy();
            bv = (Vector3.TransformVector3Matrix((bv).Copy(), (vertexRotate).Copy())).Copy();
            v2 = (Vector3.TransformVector3Quaternion((bv).Copy(), (qd).Copy())).Copy();
            shape.AddVector3bytebytebytebyte((a.Pos + v1).Copy(), 200, 200, 200, 150);
            shape.AddVector3bytebytebytebyte((a.Pos + v2).Copy(), 200, 200, 200, 150);
            shape.AddVector3bytebytebytebyte((tp).Copy(), 200, 200, 200, 0);
            shape.AddVector3bytebytebytebyte((a.Pos + v1 * 2).Copy(), r, g, b, 100);
            shape.AddVector3bytebytebytebyte((a.Pos + v2 * 2).Copy(), r, g, b, 100);
            shape.AddVector3bytebytebytebyte((tp).Copy(), r, g, b, 0);
        }
    }

    public override void Draw()
    {
        shape.Draw();
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }
}

public class Shot : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector2 Pos2 = new Vector2();
    public Vector3 Vel = new Vector3();
    public float Deg;
    public float Speed;
    public Vector3 BlurVel = new Vector3();
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
            Vel.Z = 0;
            Vel.Y = Vel.Z;
            Vel.X = Vel.Y;
        }

        Deg = 0;
        Speed = 1.0f;
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        storedId = -1;
    }

    public void Set(Vector3 p, float d, float s)
    {
        Pos = (p).Copy();
        Pos2.X = p.X;
        Pos2.Y = p.Y;
        Deg = d;
        Speed = s;
        Vel.X = (float)Math.Sin(d) * s;
        Vel.Y = (float)Math.Cos(d) * s;
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Shot Copy()
    {
        return new Shot
        {
            Pos = Pos.Copy(),
            Pos2 = Pos2.Copy(),
            Vel = Vel.Copy(),
            Deg = Deg,
            Speed = Speed,
            BlurVel = BlurVel.Copy(),
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
