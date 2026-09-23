// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class GrenadePool : ActorPool<Grenade>
{
    private MmFrame frame;
    private BallPool balls;
    private ParticlePool particles;
    private BulletPool bullets;
    private Sound sound;
    private BlurNormalTextureCubeListShape shape;
    private QuadListShape burstShape;
    private Quaternion dirVel = new Quaternion();
    private Vector4 bv = new Vector4();
    public GrenadePool(int n, MmFrame frame, BallPool balls, ParticlePool particles, BulletPool bullets, Sound sound) : base(n, () => new Grenade())
    {
        this.frame = frame;
        this.balls = balls;
        this.particles = particles;
        this.bullets = bullets;
        this.sound = sound;
        shape = BallShape.CreateShape(frame);
        burstShape = new QuadListShape(frame);
        burstShape.Initializeintbytebytebytebyte(n * 4 * 24, 200, 50, 0, 0);
        dirVel = (Quaternion.CreateFromYawPitchRoll(0.025f, 0.05f, -0.0125f)).Copy();
    }

    public override void Update()
    {
        burstShape.BeginAdd();
        base.Update();
        burstShape.EndAdd();
    }

    public override void UpdateT(Grenade a)
    {
        if (a.BurstCnt > 0)
        {
            a.BurstCnt++;
            if (a.BurstCnt > 20)
            {
                RemoveT(a);
                return;
            }

            balls.CheckHitGrenade((a.Pos2).Copy(), 6.0f + a.BurstCnt * 0.5f);
            DrawBurstVector2floatintfloat((a.Pos2).Copy(), 4.0f, 6, (float)a.BurstCnt / 10.0f);
            DrawBurstVector2floatintfloat((a.Pos2).Copy(), 8.0f, 8, (float)(a.BurstCnt - 5) / 10.0f);
            DrawBurstVector2floatintfloat((a.Pos2).Copy(), 12.0f, 10, (float)(a.BurstCnt - 10) / 10.0f);
        }
        else
        {
            a.Pos += a.Vel;
            a.Vel *= 0.98f;
            if (a.Pos.Z <= 0)
            {
                a.BurstCnt = 1;
                a.Pos.Z = 0;
                a.Pos2.X = a.Pos.X;
                a.Pos2.Y = a.Pos.Y;
                bullets.ChangeToBonusInRange((a.Pos).Copy(), 12.5f);
                sound.PlaySe("Grenade");
                return;
            }

            a.Vel.Z -= 0.05f;
            a.Dir *= dirVel;
            a.BlurVel += (a.Vel - a.BlurVel) * 0.1f;
        }
    }

    private void DrawBurstVector2floatintfloat(Vector2 p, float r, int n, float c)
    {
        float h;
        if (c <= 0 || c >= 1)
            return;
        else if (c < 0.5f)
            h = c * 2;
        else
            h = 2 - c * 2;
        float d = 0;
        h *= 3.0f;
        Particle pt = new Particle();
        Vector3 p3 = new Vector3();
        for (int i = 0; i < n; i++)
        {
            p3.X = p.X + (float)Math.Sin(d) * r;
            p3.Y = p.Y + (float)Math.Cos(d) * r;
            DrawPillar(p3.X, p3.Y, h);
            pt.Set((p3).Copy(), (Quaternion.CreateFromYawPitchRoll(0, 0.5f, -d)).Copy(), 2, 20, 50, 0, 0, 250, 150, 50, 0.5f);
            particles.Add(pt);
            d += (float)Math.PI * 2 / n;
        }
    }

    private void DrawPillar(float x, float y, float h)
    {
        DrawSquare(x - h, y - h, x + h, y - h, h * 3);
        DrawSquare(x + h, y - h, x + h, y + h, h * 3);
        DrawSquare(x + h, y + h, x - h, y + h, h * 3);
        DrawSquare(x - h, y + h, x - h, y - h, h * 3);
    }

    private void DrawSquare(float x1, float y1, float x2, float y2, float z)
    {
        burstShape.Addfloatfloatfloatfloat(x1, y1, 0, 1);
        burstShape.Addfloatfloatfloatfloat(x2, y2, 0, 1);
        burstShape.Addfloatfloatfloatfloat(x2, y2, z, 0);
        burstShape.Addfloatfloatfloatfloat(x1, y1, z, 0);
    }

    public override void DrawT(Grenade a)
    {
        if (a.BurstCnt > 0)
            return;
        bv.X = a.BlurVel.X;
        bv.Y = a.BlurVel.Y;
        bv.Z = a.BlurVel.Z;
        frame.Velocity = (bv * 3).Copy();
        shape.DrawVector3Quaternion((a.Pos).Copy(), (a.Dir).Copy());
    }

    public void DrawBurst()
    {
        burstShape.Draw();
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }
}

public class Grenade : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector2 Pos2 = new Vector2();
    public Vector3 Vel = new Vector3();
    public Quaternion Dir = new Quaternion();
    public Vector3 BlurVel = new Vector3();
    public int BurstCnt;
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

        Dir = (Quaternion.Identity).Copy();
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        BurstCnt = 0;
        storedId = -1;
    }

    public void Set(Vector3 p, Vector2 v, float deg)
    {
        Pos = (p).Copy();
        Vel.X = v.X * 3.5f + (float)Math.Sin(deg) * 1.5f;
        Vel.Y = v.Y * 3.5f + (float)Math.Cos(deg) * 1.5f;
        Vel.Z = 1.0f;
        Dir = (Quaternion.Identity).Copy();
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        BurstCnt = 0;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Grenade Copy()
    {
        return new Grenade
        {
            Pos = Pos.Copy(),
            Pos2 = Pos2.Copy(),
            Vel = Vel.Copy(),
            Dir = Dir.Copy(),
            BlurVel = BlurVel.Copy(),
            BurstCnt = BurstCnt,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
