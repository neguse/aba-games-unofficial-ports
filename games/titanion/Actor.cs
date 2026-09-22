// Copyright 2004-2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public abstract class Actor
{
    public bool exists;
    public abstract void init_1(List<object> args);
    public abstract void move_0();
    public abstract void draw_0();
}

public class ActorPool<T>
    where T : Actor
{
    public T[] actors;
    public int actorIdx;
    public bool hasNoActor;
    public ActorPool(int n, List<object> args, Func<T> create)
    {
        actors = new T[n];
        for (int i = 0; i < n; i++)
        {
            actors[i] = create();
            actors[i].init_1(args);
        }
    }

    public virtual T getInstance()
    {
        if (hasNoActor)
            return null;
        for (int i = 0; i < actors.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actors.Length - 1;
            if (!((actors[actorIdx].exists)))
                return actors[actorIdx];
        }

        hasNoActor = true;
        return null;
    }

    public virtual T getInstanceForced()
    {
        actorIdx--;
        if (actorIdx < 0)
            actorIdx = actors.Length - 1;
        return actors[actorIdx];
    }

    public virtual T[] getMultipleInstances(int n)
    {
        T[] result = new T[n];
        for (int i = 0; i < n; i++)
        {
            T item = getInstance();
            if (item == null)
            {
                for (int j = 0; j < i; j++)
                    result[j].exists = false;
                return null;
            }

            item.exists = true;
            result[i] = item;
        }

        for (int i = 0; i < n; i++)
            result[i].exists = false;
        return result;
    }

    public virtual void move_0()
    {
        hasNoActor = false;
        foreach (T item in actors)
            if (item.exists)
                item.move_0();
    }

    public virtual void draw_0()
    {
        foreach (T item in actors)
            if (item.exists)
                item.draw_0();
    }

    public virtual void clear()
    {
        foreach (T item in actors)
            item.exists = false;
        actorIdx = 0;
    }
}
