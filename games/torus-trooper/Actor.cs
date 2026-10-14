// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using static Lub;
using System.Collections.Generic;

public interface Actor
{
    bool getExist();
    void setExist(bool value);
    void init_1(List<object> args);
    void move();
    void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth);
}

public static class ActorMesh
{
    static int nextMesh;
    public static string nextKey() { string key = nextMesh.ToString(); nextMesh++; return key; }
}

public class ActorPool<T>
    where T : class, Actor
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
            if (!(actor[actorIdx].getExist()))
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
                    result[j].setExist(false);
                return null;
            }

            item.setExist(true);
            result[i] = item;
        }

        for (int i = 0; i < n; i++)
            result[i].setExist(false);
        return result;
    }

    public void move()
    {
        foreach (var item in actor)
            if (item.getExist())
                item.move();
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        foreach (var item in actor)
            if (item.getExist())
                item.draw(model, tint, blend, cull, lineWidth);
    }

    public void clear()
    {
        foreach (var item in actor)
            item.setExist(false);
        actorIdx = 0;
    }
}
