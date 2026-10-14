// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using Lub;
using System.Collections.Generic;

public static class ActorMesh
{
    static int nextMesh;
    public static string nextKey()
    {
        string key = nextMesh.ToString();
        nextMesh++;
        return key;
    }
}

public interface Actor
{
    public bool getExists();
    public void setExists(bool exists);
    public string getMeshKey();
    public void init_1_(object[] args);
    public void move_0();
    public void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null);
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

    public T getInstance()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0)
                actorIdx = actor.Length - 1;
            if (!((actor[actorIdx].getExists())))
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
        var result = new T[n];
        for (int i_0 = 0; i_0 < n; i_0++)
        {
            var item = getInstance();
            if (item == null)
            {
                for (int j = 0; j < i_0; j++)
                    result[j].setExists(false);
                return null;
            }

            item.setExists(true);
            result[i_0] = item;
        }

        for (int i_1 = 0; i_1 < n; i_1++)
            result[i_1].setExists(false);
        return result;
    }

    public void move_0()
    {
        foreach (var item in actor)
            if (item.getExists())
                item.move_0();
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        foreach (var item in actor)
            if (item.getExists())
                item.draw(model, tint, blend, key + "-" + item.getMeshKey(), target);
    }

    public void clear()
    {
        foreach (var item in actor)
            item.setExists(false);
        actorIdx = 0;
    }
}

