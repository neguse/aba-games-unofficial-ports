// Copyright 2004-2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public interface Actor
{
    public bool getExists();
    public void setExists(bool exists);
    public void setPoolIndex(int poolIndex);
    public void init(List<object> args);
    public void move();
    public void draw(float[] model, Mesh particles = null);
}

public interface LuminousActor : Actor
{
    public void drawLuminous(float[] model, Mesh particles = null);
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
            actor[i].setPoolIndex(i);
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
            if (!actor[actorIdx].getExists())
                return actor[actorIdx];
        }

        return default(T);
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
                    result[j].setExists(false);
                return null;
            }

            item.setExists(true);
            result[i] = item;
        }

        for (int i = 0; i < n; i++)
            result[i].setExists(false);
        return result;
    }

    public void move()
    {
        foreach (T item in actor)
            if (item.getExists())
                item.move();
    }

    public void draw(float[] model, Mesh particles = null)
    {
        foreach (T item in actor)
            if (item.getExists())
                item.draw(model, particles);
    }

    public void clear()
    {
        foreach (T item in actor)
            item.setExists(false);
        actorIdx = 0;
    }
}

public class LuminousActorPool<T> where T : LuminousActor
{
    public ActorPool<T> pool;
    public T[] actor;
    public LuminousActorPool(int n, List<object> args, Func<T> create)
    {
        pool = new ActorPool<T>(n, args, create);
        actor = pool.actor;
    }

    public T getInstance()
    {
        return pool.getInstance();
    }

    public T getInstanceForced()
    {
        return pool.getInstanceForced();
    }

    public void move()
    {
        pool.move();
    }

    public void draw(float[] model, Mesh particles = null)
    {
        pool.draw(model, particles);
    }

    public void clear()
    {
        pool.clear();
    }

    public void drawLuminous(float[] model, Mesh particles = null)
    {
        foreach (T item in actor)
            if (item.getExists())
                item.drawLuminous(model, particles);
    }
}
