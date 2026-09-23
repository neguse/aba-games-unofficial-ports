// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class BoardPool : ActorPool<Board>
{
    public BoardPool(int n, MmFrame frame) : base(n, () => new Board())
    {
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Initialize(frame);
    }

    public override void UpdateT(Board a)
    {
        a.Cnt--;
        if (a.Cnt <= 0)
        {
            RemoveT(a);
            return;
        }

        a.Pos += a.Vel;
        a.Vel *= 0.95f;
    }

    public QuadListShape GetShape(int id)
    {
        return actors[(id)].Shape;
    }

    public override void DrawT(Board a)
    {
        a.Shape.DrawVector3((a.Pos).Copy());
    }

    public override void Gc()
    {
        for (int i = 0; i < actorNum; i++)
        {
            if (!isRemoved[(i)])
                continue;
            for (int j = actorNum - 1; j < i; j--)
            {
                if (isRemoved[(j)])
                    continue;
                QuadListShape s = actors[(i)].Shape;
                actors[(i)] = (actors[(j)]).Copy();
                actors[(i)].SetId(i);
                isRemoved[(i)] = false;
                actors[(j)].Shape = s;
                break;
            }

            actorNum--;
        }
    }

    public void ClearAll()
    {
        Clear();
        for (int i = 0; i < actors.Length; i++)
            actors[(i)].Clear();
    }

    public int NextId
    {
        get
        {
            if (actorNum < actors.Length)
                return actorNum;
            else
                return actorIdx;
        }
    }
}

public class Board : Actor
{
    public Vector3 Pos = new Vector3();
    public Vector3 Vel = new Vector3();
    public int Cnt;
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
