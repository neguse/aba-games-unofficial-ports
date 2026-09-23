// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class ParticlePool : ActorPool<Particle>
{
    public static Random Random;
    private MmFrame frame;
    private TriangleListShape shape;
    private Matrix vertexRotate = new Matrix();
    public static void SetRandomSeed(Int32 s)
    {
        Random = new Random(s);
    }

    public ParticlePool(int n, MmFrame frame) : base(n, () => new Particle())
    {
        this.frame = frame;
        if (frame != null)
        {
            shape = new TriangleListShape(frame);
            shape.Initializeint(n * 8);
            vertexRotate = (Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)Math.PI * 2 / 4)).Copy();
        }
    }

    public override void Update()
    {
        shape.BeginAdd();
        base.Update();
        shape.EndAdd();
    }

    public override void UpdateT(Particle a)
    {
        a.Cnt--;
        if (a.Cnt <= 0)
        {
            RemoveT(a);
            return;
        }

        a.Vel *= 0.95f;
        a.Vel.Z -= 0.05f;
        a.Pos += a.Vel;
        if (a.Pos.Z < 0 && a.Vel.Z < 0)
            a.Vel.Z *= -1;
        AddShape(a);
    }

    private void AddShape(Particle a)
    {
        byte al;
        float sz;
        if (a.Cnt < 16)
        {
            al = (byte)(a.Cnt * 5);
            sz = a.Size * a.Cnt / 16;
        }
        else
        {
            al = 80;
            sz = a.Size;
        }

        Vector3 bv = new Vector3(sz, 0, 0);
        Vector3 v1 = new Vector3();
        Vector3 v2 = new Vector3();
        v2 = (Vector3.TransformVector3Quaternion((bv).Copy(), (a.Dir).Copy())).Copy();
        Vector3 tp = (a.Pos - a.Vel * 10).Copy();
        for (int i = 0; i < 4; i++)
        {
            v1 = (v2).Copy();
            bv = (Vector3.TransformVector3Matrix((bv).Copy(), (vertexRotate).Copy())).Copy();
            v2 = (Vector3.TransformVector3Quaternion((bv).Copy(), (a.Dir).Copy())).Copy();
            shape.AddVector3bytebytebytebyte((a.Pos + v1).Copy(), a.R, a.G, a.B, al);
            shape.AddVector3bytebytebytebyte((a.Pos + v2).Copy(), a.R, a.G, a.B, al);
            shape.AddVector3bytebytebytebyte((tp).Copy(), a.R, a.G, a.B, 0);
            shape.AddVector3bytebytebytebyte((a.Pos + v1 * 2).Copy(), a.Er, a.Eg, a.Eb, al);
            shape.AddVector3bytebytebytebyte((a.Pos + v2 * 2).Copy(), a.Er, a.Eg, a.Eb, al);
            shape.AddVector3bytebytebytebyte((tp).Copy(), a.Er, a.Eg, a.Eb, 0);
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

public class Particle : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector3 Vel = new Vector3();
    public int Cnt;
    public byte R, G, B, Er, Eg, Eb;
    public Quaternion Dir = new Quaternion();
    public float Size;
    private int storedId;
    public void Clear()
    {
        {
            Pos.Z = 0;
            Pos.Y = Pos.Z;
            Pos.X = Pos.Y;
        }

        {
            Vel.Z = 0;
            Vel.Y = Vel.Z;
            Vel.X = Vel.Y;
        }

        Cnt = 0;
        {
            Eb = 0;
            Eg = Eb;
            Er = Eg;
            B = Er;
            G = B;
            R = G;
        }

        Dir = (Quaternion.Identity).Copy();
        Size = 1.0f;
        storedId = -1;
    }

    public void Set(Vector3 p, Quaternion d, float s, int c, byte r, byte g, byte b, byte er, byte eg, byte eb, float sz)
    {
        Pos = (p).Copy();
        Dir = (d).Copy();
        Quaternion pd = (d * Quaternion.CreateFromYawPitchRoll((float)ParticlePool.Random.NextDouble() * 0.2f - 0.1f, (float)ParticlePool.Random.NextDouble() * 0.2f - 0.1f, (float)ParticlePool.Random.NextDouble() * 0.2f - 0.1f)).Copy();
        Vector3 speedVct = new Vector3(0, 1, 0);
        Vel = (Vector3.TransformVector3Quaternion((speedVct).Copy(), (pd).Copy())).Copy();
        Vel *= s * (float)(ParticlePool.Random.NextDouble() * 0.8f + 0.2f);
        Cnt = GameMath.integer((c * (ParticlePool.Random.NextDouble() * 1.0f + 0.5f)));
        R = r;
        G = g;
        B = b;
        Er = er;
        Eg = eg;
        Eb = eb;
        Size = sz;
    }

    public void SetFixed(Vector3 p, Quaternion d, float s, int c, byte r, byte g, byte b, byte er, byte eg, byte eb, float sz)
    {
        Pos = (p).Copy();
        Dir = (d).Copy();
        Vector3 speedVct = new Vector3(0, 1, 0);
        Vel = (Vector3.TransformVector3Quaternion((speedVct).Copy(), (d).Copy())).Copy();
        Vel *= s;
        Cnt = c;
        R = r;
        G = g;
        B = b;
        Er = er;
        Eg = eg;
        Eb = eb;
        Size = sz;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Particle Copy()
    {
        return new Particle
        {
            Pos = Pos.Copy(),
            Vel = Vel.Copy(),
            Cnt = Cnt,
            R = R,
            G = G,
            B = B,
            Er = Er,
            Eg = Eg,
            Eb = Eb,
            Dir = Dir.Copy(),
            Size = Size,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
