// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public abstract class Actor
{
    public bool exists;
    public abstract void init_1(List<object> args);
    public abstract void move();
    public abstract void draw();
}

public abstract class LuminousActor : Actor
{
    public abstract void drawLuminous();
}

public class ActorPool<T>
    where T : Actor
{
    public T[] actor;
    public int actorIdx;
    public ActorPool(int count, List<object> args, Func<T> create)
    {
        actor = new T[count];
        for (int i = 0; i < count; i++)
        {
            actor[i] = create();
            actor[i].init_1(args);
        }
    }

    public T getInstance()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actor.Length - 1;
            if (!(actor[actorIdx].exists))
                return actor[actorIdx];
        }

        return null;
    }

    public T getInstanceForced()
    {
        actorIdx--;
        if (actorIdx < 0)
            actorIdx = actor.Length - 1;
        return actor[actorIdx];
    }

    public T[] getMultipleInstances(int n)
    {
        var result = new T[n];
        for (int i = 0; i < n; i++)
        {
            var item = getInstance();
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

    public virtual void move()
    {
        foreach (var item in actor)
            if (item.exists)
                item.move();
    }

    public virtual void draw()
    {
        foreach (var item in actor)
            if (item.exists)
                item.draw();
    }

    public virtual void clear()
    {
        foreach (var item in actor)
            item.exists = false;
        actorIdx = 0;
    }
}

public class LuminousActorPool<T> : ActorPool<T> where T : LuminousActor
{
    public LuminousActorPool(int n, List<object> args, Func<T> create) : base(n, args, create)
    {
    }

    public void drawLuminous()
    {
        foreach (var a in actor)
            if (a.exists)
                a.drawLuminous();
    }
}
