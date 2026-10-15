// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;

public class ActorPool<T>
    where T : class, Actor
{
    public T[] actors;
    public int actorNum, actorIdx;
    public bool[] isRemoved;
    private IActorIdListener idListener;
    public ActorPool(int n, Func<T> factory, IActorIdListener idListener)
    {
        this.idListener = idListener;
        actors = MmArrays.Make(n, factory);
        isRemoved = MmArrays.Make(n, () => false);
        Clear();
    }

    public int Add(T a)
    {
        if (actorNum >= actors.Length)
            return -1;
        actors[(actorNum)] = (T)a.CopyActor();
        actors[(actorNum)].SetId(actorNum);
        isRemoved[(actorNum)] = false;
        actorNum++;
        return actorNum - 1;
    }

    public int AddForced(T a)
    {
        int id = Add(a);
        if (id >= 0)
            return id;
        id = actorIdx;
        actors[(id)] = (T)a.CopyActor();
        actors[(id)].SetId(id);
        isRemoved[(id)] = false;
        actorIdx++;
        if (actorIdx >= actors.Length)
            actorIdx = 0;
        return id;
    }

    public void AddTo(ActorPool<T> p)
    {
        ForEach((T a) =>
        {
            p.Add(a);
        });
        Clear();
    }

    public void AddToForced(ActorPool<T> p)
    {
        ForEach((T a) =>
        {
            p.AddForced(a);
        });
        Clear();
    }

    public void RemoveT(T a)
    {
        Removeint(a.GetId());
    }

    public void Removeint(int id)
    {
        isRemoved[(id)] = true;
    }

    public void Gc()
    {
        for (int i = 0; i < actorNum; i++)
        {
            if (!isRemoved[(i)])
                continue;
            for (int j = actorNum - 1; j > i; j--)
            {
                if (isRemoved[(j)])
                {
                    actorNum--;
                    continue;
                }

                actors[(i)] = (T)actors[(j)].CopyActor();
                actors[(i)].SetId(i);
                isRemoved[(i)] = false;
                if (idListener != null)
                    idListener.OnIdChanged(j, i);
                break;
            }

            actorNum--;
        }
    }

    public void Clear()
    {
        actorNum = 0;
        actorIdx = 0;
        for (int i = 0; i < actors.Length; i++)
            isRemoved[(i)] = false;
    }

    public T Get(int idx)
    {
        return (T)actors[(idx)].CopyActor();
    }

    public int Length()
    {
        return actorNum;
    }

    public bool IsAbleToAdd
    {
        get
        {
            return (actorNum < actors.Length);
        }
    }

    public int RemainingActorNum
    {
        get
        {
            return actors.Length - actorNum;
        }
    }

    public void ForEach(Action<T> func)
    {
        for (int i = 0; i < actorNum; i++)
            if (!isRemoved[(i)])
                func(actors[(i)]);
    }
}

public interface IActorIdListener
{
    void OnIdChanged(int before, int after);
}

public interface Actor
{
    Actor CopyActor();
    int GetId();
    void SetId(int value);
}
