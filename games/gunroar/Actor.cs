// Copyright 2004-2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public abstract class Actor
{
    public bool exists;
    public int poolIndex;
    public abstract void init(List<object> args);
    public abstract void move();
    public abstract void draw(float[] model, Mesh particles = null);
}

public abstract class LuminousActor : Actor
{
    public abstract void drawLuminous(float[] model, Mesh particles = null);
}

public class ActorPool<T>
    where T : Actor
{
    public T[] actor;
    public int actorIdx;
    public ActorPool(int n, List<object> args, Func<T> create)
    {
        actor = new T[n];
        for (int i = 0; i < n; i++)
        {
            actor[i] = create();
            actor[i].poolIndex = i;
            actor[i].init(args);
        }
    }

    public T getInstance()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actor.Length - 1;
            if (!actor[actorIdx].exists)
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

    public virtual void move()
    {
        foreach (T item in actor)
            if (item.exists)
                item.move();
    }

    public void draw(float[] model, Mesh particles = null)
    {
        foreach (T item in actor)
            if (item.exists)
                item.draw(model, particles);
    }

    public void clear()
    {
        foreach (T item in actor)
            item.exists = false;
        actorIdx = 0;
    }
}

public class LuminousActorPool<T> : ActorPool<T> where T : LuminousActor
{
    public LuminousActorPool(int n, List<object> args, Func<T> create) : base(n, args, create)
    {
    }

    public void drawLuminous(float[] model, Mesh particles = null)
    {
        foreach (T item in actor)
            if (item.exists)
                item.drawLuminous(model, particles);
    }
}
