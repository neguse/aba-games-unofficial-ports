// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public interface ReplayInput
{
    string encode();
    void decode(string data);
}

public class PadState : ReplayInput
{
    public int dir, button;
    public virtual void clear()
    {
        {
            button = 0;
            dir = button;
        }
    }

    public virtual string encode()
    {
        return (dir | button).ToString();
    }

    public virtual void decode(string data)
    {
        int v = int.Parse(data);
        dir = v & 15;
        button = v & 48;
    }
}

public class InputRecord<T>
    where T : ReplayInput
{
    public List<string> records = new List<string>();
    public List<int> lengths = new List<int>();
    public int index, remaining;
    public T replayData;
    public InputRecord(T state)
    {
        replayData = state;
    }

    public virtual void add(T data)
    {
        string encoded = data.encode();
        int last = records.Count - 1;
        if ((((last >= 0))) && (((records[last] == encoded))))
            lengths[last]++;
        else
        {
            records.Add(encoded);
            lengths.Add(1);
        }
    }

    public virtual void reset()
    {
        {
            remaining = 0;
            index = remaining;
        }
    }

    public virtual bool hasNext()
    {
        return index < records.Count;
    }

    public virtual T next()
    {
        if (index >= records.Count)
            return default(T);
        if (remaining == 0)
            remaining = lengths[index];
        replayData.decode(records[index]);
        remaining--;
        if (remaining == 0)
            index++;
        return replayData;
    }

    public virtual string encode()
    {
        string data = "";
        for (int i = 0; i < records.Count; i++)
            data = data + (lengths[i].ToString() + "/" + records[i] + ";");
        return data;
    }

    public virtual void decode(string data)
    {
        string[] rows = data.Split(";");
        records.Clear();
        lengths.Clear();
        reset();
        for (int i = 0; i < rows.Length - 1; i++)
        {
            string[] parts = rows[i].Split("/");
            int n = int.Parse(parts[0]);
            if (n <= 0)
                return;
            replayData.decode(parts[1]);
            records.Add(parts[1]);
            lengths.Add(n);
        }
    }
}

public class Pad
{
    public PadState state = new PadState();
    public bool pause, escape;
    public virtual PadState getNullState()
    {
        state.clear();
        return state;
    }
}

public class RecordablePad : Pad
{
    public InputRecord<PadState> inputRecord;
    public virtual void startRecord()
    {
        inputRecord = new InputRecord<PadState>(new PadState());
    }

    public virtual void startReplay_1(InputRecord<PadState> record)
    {
        inputRecord = record;
        record.reset();
    }

    public virtual PadState getState(bool doRecord = true)
    {
        if (doRecord)
            inputRecord.add(state);
        return state;
    }

    public virtual PadState replay()
    {
        return inputRecord.next();
    }
}

public static class PadStateButton
{
    public const int A = 16, B = 32, ANY = 48;
}

public static class PadStateDir
{
    public const int UP = 1, DOWN = 2, LEFT = 4, RIGHT = 8;
}
