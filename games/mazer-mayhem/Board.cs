// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class BoardPool
{
    public ActorPool<Board> pool;
    public Board[] actors;
    public BoardPool(int n, MmFrame frame)
    {
        pool = new ActorPool<Board>(n, () => new Board(), null);
        actors = pool.actors;
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Initialize(frame);
    }

    public int AddForced(Board a)
    {
        return pool.AddForced(a);
    }

    public void Update()
    {
        pool.ForEach((Board a) =>
        {
            UpdateT(a);
        });
    }

    public void Draw()
    {
        pool.ForEach((Board a) =>
        {
            DrawT(a);
        });
    }

    public void UpdateT(Board a)
    {
        a.Cnt -= SimulationTime.Step;
        if (a.Cnt <= 0)
        {
            pool.RemoveT(a);
            return;
        }

        a.Pos += a.Vel * SimulationTime.Step;
        a.Vel *= SimulationTime.Decay(0.95f);
    }

    public QuadListShape GetShape(int id)
    {
        return actors[(id)].Shape;
    }

    public void DrawT(Board a)
    {
        a.Shape.DrawVector3((a.Pos).Copy());
    }

    public void Gc()
    {
        for (int i = 0; i < pool.actorNum; i++)
        {
            if (!pool.isRemoved[(i)])
                continue;
            for (int j = pool.actorNum - 1; j < i; j--)
            {
                if (pool.isRemoved[(j)])
                    continue;
                QuadListShape s = actors[(i)].Shape;
                actors[(i)] = (actors[(j)]).Copy();
                actors[(i)].SetId(i);
                pool.isRemoved[(i)] = false;
                actors[(j)].Shape = s;
                break;
            }

            pool.actorNum--;
        }
    }

    public void ClearAll()
    {
        pool.Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }

    public int NextId
    {
        get
        {
            if (pool.actorNum < actors.Length)
                return pool.actorNum;
            else
                return pool.actorIdx;
        }
    }
}

public class Board : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector3 Vel = new Vector3();
    public float Cnt;
    public QuadListShape Shape;
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
        storedId = -1;
    }

    public void Initialize(MmFrame frame)
    {
        Shape = new QuadListShape(frame);
        Shape.Initializeintbytebytebytebyte(48, 10, 10, 10, 150);
    }

    public void Set(Vector3 p, float vx, float vy, float vz)
    {
        Pos = (p).Copy();
        Vel.X = vx;
        Vel.Y = vy;
        Vel.Z = vz;
        Cnt = 60;
    }

    public int GetId(){return storedId;}
public void SetId(int value){storedId=value;}

    public Board Copy()
    {
        return new Board
        {
            Pos = Pos.Copy(),
            Vel = Vel.Copy(),
            Cnt = Cnt,
            Shape = Shape,
            storedId = storedId
        };
    }

    public Actor CopyActor()
    {
        return Copy();
    }
}
