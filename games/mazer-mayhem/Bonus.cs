// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class BonusPool : ActorPool<Bonus>
{
    public static Random Random;
    private MmFrame frame;
    private Player player;
    private BlurNormalTextureCubeListShape shape;
    private Quaternion dirVel = new Quaternion();
    private Vector4 bv = new Vector4();
    public static void SetRandomSeed(Int32 s)
    {
        Random = new Random(s);
    }

    public BonusPool(int n, MmFrame frame, Player player) : base(n, () => new Bonus())
    {
        if (frame != null)
        {
            this.frame = frame;
            this.player = player;
            shape = new BlurNormalTextureCubeListShape(frame);
            shape.BeginAdd(4);
            shape.AddVector3Vector3float(new Vector3(-1, -1, -1), (Vector3.Zero).Copy(), 1);
            shape.AddVector3Vector3float(new Vector3(1, 1, -1), (Vector3.Zero).Copy(), 1);
            shape.AddVector3Vector3float(new Vector3(-1, 1, 1), (Vector3.Zero).Copy(), 1);
            shape.AddVector3Vector3float(new Vector3(1, -1, 1), (Vector3.Zero).Copy(), 1);
            shape.EndAdd();
        }

        dirVel = (Quaternion.CreateFromYawPitchRoll(-0.0125f, 0.025f, -0.05f)).Copy();
    }

    public override void UpdateT(Bonus a)
    {
        a.Cnt--;
        if (a.Cnt <= 0)
        {
            RemoveT(a);
            return;
        }

        a.Vel *= 0.98f;
        a.Vel.Z -= 0.02f;
        a.Pos += a.Vel;
        if (a.Pos.Z < 1 && a.Vel.Z < 0)
            a.Vel.Z *= -1;
        float d = Math.Abs(player.Pos.X - a.Pos.X) + Math.Abs(player.Pos.Y - a.Pos.Y) + 1.0f;
        if (a.InhauledCnt > 0 && player.IsStarted)
        {
            a.InhauledCnt++;
            a.Pos += (player.Pos - a.Pos) * a.InhauledCnt * 0.01f;
            if (d < 5.0f)
            {
                player.GetBonus((a.Pos).Copy());
                RemoveT(a);
                return;
            }
        }
        else if (d < 25.0f)
        {
            a.InhauledCnt = 1;
        }

        a.Dir *= dirVel;
        a.BlurVel += (a.Vel - a.BlurVel) * 0.1f;
    }

    public override void DrawT(Bonus a)
    {
        float sz = 0.5f;
        if (a.Cnt < 30)
            sz = sz * a.Cnt / 30.0f;
        bv.X = a.BlurVel.X;
        bv.Y = a.BlurVel.Y;
        bv.Z = a.BlurVel.Z;
        frame.Velocity = (bv * 20).Copy();
        shape.DrawVector3Quaternionfloat((a.Pos).Copy(), (a.Dir).Copy(), sz);
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }
}

public class Bonus : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector3 Vel = new Vector3();
    public Quaternion Dir = new Quaternion();
    public int Cnt;
    public Vector3 BlurVel = new Vector3();
    public int InhauledCnt;
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

        Dir = (Quaternion.Identity).Copy();
        Cnt = 0;
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        InhauledCnt = 0;
        storedId = -1;
    }

    public void Set(Vector3 p, Quaternion d, float s, int c)
    {
        Pos = (p).Copy();
        Quaternion pd = (d * Quaternion.CreateFromYawPitchRoll((float)BonusPool.Random.NextDouble() * 0.2f - 0.1f, (float)BonusPool.Random.NextDouble() * 0.2f - 0.1f, (float)BonusPool.Random.NextDouble() * 0.2f - 0.1f)).Copy();
        Vector3 speedVct = new Vector3(0, 1, 0);
        Vel = (Vector3.TransformVector3Quaternion((speedVct).Copy(), (pd).Copy())).Copy();
        Vel *= s;
        Dir = (Quaternion.Identity).Copy();
        Cnt = c;
        {
            BlurVel.Z = 0;
            BlurVel.Y = BlurVel.Z;
            BlurVel.X = BlurVel.Y;
        }

        InhauledCnt = 0;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Bonus Copy()
    {
        return new Bonus
        {
            Pos = Pos.Copy(),
            Vel = Vel.Copy(),
            Dir = Dir.Copy(),
            Cnt = Cnt,
            BlurVel = BlurVel.Copy(),
            InhauledCnt = InhauledCnt,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
