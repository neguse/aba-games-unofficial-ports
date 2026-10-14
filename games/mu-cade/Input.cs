using System.Collections.Generic;

public class TwinStickPadState
{
    public Vector left = new Vector(), right = new Vector();
    public int button;
    public void clear()
    {
        left.clear();
        right.clear();
        button = 0;
    }

    public TwinStickPadState Copy()
    {
        return new TwinStickPadState
        {
            left = new Vector(left.x, left.y),
            right = new Vector(right.x, right.y),
            button = button
        };
    }
}

public static class TwinStickPad
{
    public static int input;
}

public class RecordableTwinStickPad
{
    public List<TwinStickPadState> inputRecord = new List<TwinStickPadState>();
    public int cursor;
    public bool pause
    {
        get
        {
            return (TwinStickPad.input & 1024) != 0;
        }
    }

    public bool escape
    {
        get
        {
            return (TwinStickPad.input & 2048) != 0;
        }
    }

    public TwinStickPadState getState_1(bool record = true)
    {
        int input = TwinStickPad.input;
        var s = new TwinStickPadState();
        s.left.x = (input & 4) != 0 ? -1 : (input & 8) != 0 ? 1 : 0;
        s.left.y = (input & 1) != 0 ? 1 : (input & 2) != 0 ? -1 : 0;
        s.right.x = (input & 64) != 0 ? -1 : (input & 128) != 0 ? 1 : 0;
        s.right.y = (input & 16) != 0 ? 1 : (input & 32) != 0 ? -1 : 0;
        s.button = ((input & 256) != 0 ? 1 : 0) | ((input & 512) != 0 ? 2 : 0);
        if (record)
            inputRecord.Add(s.Copy());
        return s;
    }

    public void startRecord()
    {
        inputRecord = new List<TwinStickPadState>();
    }

    public void startReplay(List<TwinStickPadState> data)
    {
        inputRecord = data;
        cursor = 0;
    }

    public TwinStickPadState replay()
    {
        if (cursor >= inputRecord.Count)
            return null;
        var result = inputRecord[cursor].Copy();
        cursor++;
        return result;
    }
}

public class ReplayData
{
    public List<TwinStickPadState> twinStickPadInputRecord;
    public int seed, score, time;
}

public static class TwinStickPadStateButton
{
    public const int A = 1, B = 2;
}
