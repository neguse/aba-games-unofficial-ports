using System.Collections.Generic;

public class Replay
{
    public int RandomSeed;
    List<ReplayData> data = new List<ReplayData>();
    int cursor;
    public void StartRecord()
    {
        data.Clear();
        cursor = 0;
    }

    public void Add(ReplayData value)
    {
        data.Add((value).Copy());
    }

    public void StartReplay()
    {
        cursor = 0;
    }

    public bool HasNext()
    {
        return cursor < data.Count;
    }

    public ReplayData Get()
    {
        var value = (data[(cursor)]).Copy();
        cursor++;
        return (value).Copy();
    }

    public bool IsAvailable
    {
        get
        {
            return data.Count > 0;
        }
    }
}

public class ReplayData : ActorCopy
{
    public Vector2 Stick = new Vector2();
    public float LeftTrigger, RightTrigger;
    public ReplayData Copy()
    {
        return new ReplayData
        {
            Stick = Stick.Copy(),
            LeftTrigger = LeftTrigger,
            RightTrigger = RightTrigger
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
