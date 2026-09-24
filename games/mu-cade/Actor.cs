// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using static Lub;
using System.Collections.Generic;

public abstract class Actor
{
    static int nextMesh;
    public string meshKey;
    public Actor() { meshKey = nextMesh.ToString(); nextMesh++; }
    public bool exists;
    public abstract void init_1_(object[] args);
    public abstract void move_0();
    public abstract void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null);
}

public class ActorPool<T>
    where T : Actor
{
    public T[] actor;
    public int actorIdx;
    public ActorPool(int count, object[] args, Func<T> create)
    {
        actor = new T[count];
        for (int i = 0; i < count; i++)
        {
            actor[i] = create();
            actor[i].init_1_(args);
        }
    }

    public virtual T getInstance()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actor.Length - 1;
            if (!((actor[actorIdx].exists)))
                return actor[actorIdx];
        }

        return null;
    }

    public virtual T getInstanceForced()
    {
        actorIdx--;
        if (actorIdx < 0)
            actorIdx = actor.Length - 1;
        return actor[actorIdx];
    }

    public virtual T[] getMultipleInstances(int n)
    {
        var result = new T[n];
        for (int i_0 = 0; i_0 < n; i_0++)
        {
            var item = getInstance();
            if (item == null)
            {
                for (int j = 0; j < i_0; j++)
                    result[j].exists = false;
                return null;
            }

            item.exists = true;
            result[i_0] = item;
        }

        for (int i_1 = 0; i_1 < n; i_1++)
            result[i_1].exists = false;
        return result;
    }

    public virtual void move_0()
    {
        foreach (var item in actor)
            if (item.exists)
                item.move_0();
    }

    public virtual void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        foreach (var item in actor)
            if (item.exists)
                item.draw(model, tint, blend, key + "-" + item.meshKey, target);
    }

    public virtual void clear()
    {
        foreach (var item in actor)
            item.exists = false;
        actorIdx = 0;
    }
}

