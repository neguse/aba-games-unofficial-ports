// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class WallPool
{
    private MmFrame frame;
    private Field field;
    private BlurNormalTextureCubeListShape shape;
    private Vector3 storedHitPos = new Vector3();
    private float storedHitDistRatio;
    public ActorPool<Wall> pool;
    public Wall[] actors;
    public WallPool(int n, MmFrame frame, Field field)
    {
        pool = new ActorPool<Wall>(n, () => new Wall(), null);
        actors = pool.actors;
        this.frame = frame;
        this.field = field;
        shape = BallShape.CreateShape(frame);
    }

    public int Add(Wall a)
    {
        return pool.Add(a);
    }

    public void ForEach(Action<Wall> func)
    {
        pool.ForEach(func);
    }

    public void RemoveT(Wall a)
    {
        pool.Removeint(a.GetId());
    }

    public void Gc()
    {
        pool.Gc();
    }

    public void Update()
    {
        pool.ForEach((Wall a) =>
        {
            UpdateT(a);
        });
    }

    public void Draw()
    {
        pool.ForEach((Wall a) =>
        {
            DrawT(a);
        });
    }

    public void UpdateT(Wall a)
    {
        a.IsActivated = field.Contains((a.Pos2).Copy());
    }

    public void DrawT(Wall a)
    {
        shape.DrawVector3Quaternionfloat((a.State.Pos).Copy(), (a.State.Rotation).Copy(), a.State.Radius);
    }

    public void CheckCollisions(BallPool actorBPool)
    {
        pool.ForEach((Wall actorA) =>
        {
            if (actorA.IsActivated)
            {
                CircleParticle cp = (actorA.State).Copy();
                actorBPool.ForEach((Ball actorB) =>
                {
                    if (actorB.IsActivated)
                        cp.CheckCollision(actorB.State);
                });
            }
        });
    }

    public bool CheckHit(Vector3 p)
    {
        bool hf = false;
        pool.ForEach((Wall a) =>
        {
            if (!hf && a.IsActivated && Math.Abs(p.X - a.Pos2.X) < 10 && Math.Abs(p.Y - a.Pos2.Y) < 10)
            {
                float hd = a.State.Radius - Vector3.Distance((a.State.Pos).Copy(), (p).Copy());
                if (hd > 0)
                {
                    hf = true;
                    storedHitPos = (a.State.Pos).Copy();
                    storedHitDistRatio = hd / a.State.Radius;
                }
            }
        });
        return hf;
    }

    public void ClearAll()
    {
        pool.Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
        {
            storedHitPos.Z = 0;
            storedHitPos.Y = storedHitPos.Z;
            storedHitPos.X = storedHitPos.Y;
        }

        storedHitDistRatio = 1.0f;
    }

    public Vector3 HitPos
    {
        get
        {
            return (storedHitPos).Copy();
        }
    }

    public float HitDistRatio
    {
        get
        {
            return storedHitDistRatio;
        }
    }
}

public class Wall : Actor
{
    public CircleParticle State = new CircleParticle();
    public Vector2 Pos2 = new Vector2();
    public bool IsActivated;
    private int storedId;
    public void Clear()
    {
        State.Clear();
        {
            Pos2.Y = 0;
            Pos2.X = Pos2.Y;
        }

        IsActivated = false;
        storedId = -1;
    }

    public void Set(Vector3 p, float r, Quaternion d)
    {
        State.Set((p).Copy());
        Pos2.X = p.X;
        Pos2.Y = p.Y;
        State.Radius = r;
        State.Mass = r * r * 10.0f;
        State.Elasticity = 0.75f;
        State.Friction = 0.01f;
        State.IsFixed = true;
        State.Rotation = (d).Copy();
        State.AngleRate = (Quaternion.Identity).Copy();
        IsActivated = false;
        storedId = -1;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Wall Copy()
    {
        return new Wall
        {
            State = State.Copy(),
            Pos2 = Pos2.Copy(),
            IsActivated = IsActivated,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
