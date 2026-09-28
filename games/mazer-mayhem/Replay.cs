using System.Collections.Generic;

public class Replay
{
    public bool IsAvailable;
    public int RandomSeed;
    int playbackIndex;
    public float NextStep() => playbackIndex < data.Count ? data[playbackIndex].Step : 1;
    public bool NextEmit() => playbackIndex < data.Count ? data[playbackIndex].Emit : true;
    public float NextSeconds() => playbackIndex < data.Count ? data[playbackIndex].Seconds : 1f / 60;
    public void SetPlaybackIndex(int value) { playbackIndex = value; }
    public List<ReplayData> data = new List<ReplayData>();
    public void StartRecord()
    {
        IsAvailable = true;
        data.Clear();
    }

    public void Add(ReplayData value)
    {
        data.Add(value.Copy());
    }

    public ReplayCursor GetEnumerator()
    {
        playbackIndex = 0;
        return new ReplayCursor(this);
    }
}

public class ReplayCursor
{
    Replay replay;
    int index = -1;
    public ReplayCursor(Replay replay)
    {
        this.replay = replay;
    }

    public bool MoveNext()
    {
        index++;
        replay.SetPlaybackIndex(index + 1);
        return index < replay.data.Count;
    }

    public ReplayData Current
    {
        get
        {
            return replay.data[index].Copy();
        }
    }
}

public class ReplayData
{
    public float Step = 1, Seconds = 1f / 60;
    public bool Emit = true;
    public Vector2 Stick = new Vector2();
    public float LeftTrigger, RightTrigger;
    public bool ButtonA;
    public void Clear()
    {
        Stick.X = 0;
        Stick.Y = 0;
        LeftTrigger = 0;
        RightTrigger = 0;
        ButtonA = false;
    }

    public ReplayData Copy()
    {
        return new ReplayData
        {
            Step = Step,
            Seconds = Seconds,
            Emit = Emit,
            Stick = Stick.Copy(),
            LeftTrigger = LeftTrigger,
            RightTrigger = RightTrigger,
            ButtonA = ButtonA
        };
    }
}
