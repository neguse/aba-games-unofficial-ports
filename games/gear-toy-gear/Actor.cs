using System;

public class ActorPool<T>
    where T : class, ActorCopy
{
    public T[] Actors;
    private int actorCount;
    public Shape shape;
    private Func<T, bool> onUpdate;
    private Action<T> onDraw;
    private Action<int> onRemove;
    public ActorPool(int n, Func<T> factory, Func<T, bool> updateT, Action<T> drawT, Action<int> remove)
    {
        Actors = GtgArrays.Make(n, factory);
        this.onUpdate = updateT;
        this.onDraw = drawT;
        this.onRemove = remove;
    }

    public void Clear()
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

    public void Remove(int index)
    {
        actorCount--;
        Actors[(index)] = (T)Actors[(actorCount)].CopyActor();
    }

    public void Update()
    {
        for (int i = 0; i < actorCount;)
        {
            if (onUpdate(Actors[(i)]))
                i++;
            else
                onRemove(i);
        }
    }

    public void Draw()
    {
        DrawShape(shape);
    }

    public void DrawShape(Shape s)
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
                onDraw(Actors[(i)]);
            s.Draw();
        }
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
