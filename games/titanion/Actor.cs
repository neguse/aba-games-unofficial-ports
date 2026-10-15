// Copyright 2004-2005 Kenta Cho. Some rights reserved.
using System;
using Lub;
using System.Collections.Generic;

public interface Actor
{
    public bool getExists();
    public void setExists(bool v);
    public void init_1(List<object> args);
    public void move_0();
    public void draw_0(float[] model, float[] color, Gfx.Blend blend);
}

public class ActorPool<T>
    where T : class, Actor
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

    public T getInstance()
    {
        if (hasNoActor)
            return null;
        for (int i = 0; i < actors.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actors.Length - 1;
            if (!((actors[actorIdx].getExists())))
                return actors[actorIdx];
        }

        hasNoActor = true;
        return null;
    }

    public T getInstanceForced()
    {
        actorIdx--;
        if (actorIdx < 0)
            actorIdx = actors.Length - 1;
        return actors[actorIdx];
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

    public void move_0()
    {
        hasNoActor = false;
        foreach (T item in actors)
            if (item.getExists())
                item.move_0();
    }

    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        foreach (T item in actors)
            if (item.getExists())
                item.draw_0(model, color, blend);
    }

    public void clear()
    {
        foreach (T item in actors)
            item.setExists(false);
        actorIdx = 0;
    }
}
