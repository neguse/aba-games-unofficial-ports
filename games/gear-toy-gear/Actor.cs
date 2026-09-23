using System;

public class ActorPool<T>
    where T : class, ActorCopy
{
    public T[] Actors;
    protected int actorCount;
    protected Shape shape;
    public ActorPool(int n, Func<T> factory)
    {
        Actors = GtgArrays.Make(n, factory);
    }

    public virtual void Clear()
    {
        actorCount = 0;
    }

    public int AddT(T a)
    {
        if (actorCount >= Actors.Length)
            return -1;
        Actors[(actorCount)] = (T)a.CopyActor();
        actorCount++;
        return actorCount - 1;
    }

    public virtual void Remove(int index)
    {
        actorCount--;
        Actors[(index)] = (T)Actors[(actorCount)].CopyActor();
    }

    public virtual void Update()
    {
        for (int i = 0; i < actorCount;)
        {
            if (UpdateT(Actors[(i)]))
                i++;
            else
                Remove(i);
        }
    }

    public virtual void Draw()
    {
        DrawShape(shape);
    }

    public virtual void DrawShape(Shape s)
    {
        if (actorCount <= 0)
            return;
        for (int i = 0; i < actorCount;)
        {
            Shape.BeginAddInstance();
            int limit = i + Shape.MaxInstanceCount;
            if (limit > actorCount)
                limit = actorCount;
            for (; i < limit; i++)
                DrawT(Actors[(i)]);
            s.Draw();
        }
    }

    public virtual bool UpdateT(T a)
    {
        return true;
    }

    public virtual void DrawT(T a)
    {
    }

    public T Get(int i)
    {
        return (T)Actors[(i)].CopyActor();
    }

    public int Count
    {
        get
        {
            return actorCount;
        }
    }

    public bool IsAbleToAdd
    {
        get
        {
            return actorCount < Actors.Length;
        }
    }

    public int RemainingActorCount
    {
        get
        {
            return Actors.Length - actorCount;
        }
    }

    public void ForEach(Action<T> action)
    {
        for (int i = 0; i < actorCount; i++)
            action(Actors[(i)]);
    }
}

public interface ActorCopy
{
    ActorCopy CopyActor();
}
