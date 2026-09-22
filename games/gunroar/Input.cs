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
    public void clear()
    {
        {
            button = 0;
            dir = button;
        }
    }

    public string encode()
    {
        return (dir | button).ToString();
    }

    public void decode(string data)
    {
        int v = int.Parse(data);
        dir = v & 15;
        button = v & 48;
    }
}

public class TwinStickState : ReplayInput
{
    public Vector left = new Vector(), right = new Vector();
    public void clear()
    {
        {
            right.y = 0;
            right.x = right.y;
            left.y = right.x;
            left.x = left.y;
        }
    }

    public string encode()
    {
        return left.x.ToString() + "," + left.y.ToString() + "," + right.x.ToString() + "," + right.y.ToString();
    }

    public void decode(string data)
    {
        string[] v = data.Split(",");
        left.x = float.Parse(v[0]);
        left.y = float.Parse(v[1]);
        right.x = float.Parse(v[2]);
        right.y = float.Parse(v[3]);
    }
}

public class MouseState : ReplayInput
{
    public float x, y;
    public int button;
    public void clear()
    {
        button = 0;
    }

    public string encode()
    {
        return x.ToString() + "," + y.ToString() + "," + button.ToString();
    }

    public void decode(string data)
    {
        string[] v = data.Split(",");
        x = float.Parse(v[0]);
        y = float.Parse(v[1]);
        button = int.Parse(v[2]);
    }
}

public class MouseAndPadState : ReplayInput
{
    public MouseState mouseState = new MouseState();
    public PadState padState = new PadState();
    public string encode()
    {
        return mouseState.encode() + ":" + padState.encode();
    }

    public void decode(string data)
    {
        string[] v = data.Split(":");
        mouseState.decode(v[0]);
        padState.decode(v[1]);
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

    public void add(T data)
    {
        string encoded = data.encode();
        int last = records.Count - 1;
        if ((last >= 0) && (records[last] == encoded))
            lengths[last]++;
        else
        {
            records.Add(encoded);
            lengths.Add(1);
        }
    }

    public void reset()
    {
        {
            remaining = 0;
            index = remaining;
        }
    }

    public bool hasNext()
    {
        return index < records.Count;
    }

    public T next()
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

    public string encode()
    {
        string data = "";
        for (int i = 0; i < records.Count; i++)
            data = data + (lengths[i].ToString() + "/" + records[i] + ";");
        return data;
    }

    public void decode(string data)
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
    public PadState getNullState()
    {
        state.clear();
        return state;
    }
}

public class RecordablePad : Pad
{
    public InputRecord<PadState> inputRecord;
    public void startRecord()
    {
        inputRecord = new InputRecord<PadState>(new PadState());
    }

    public void startReplay(InputRecord<PadState> record)
    {
        inputRecord = record;
        record.reset();
    }

    public PadState getState(bool doRecord = true)
    {
        if (doRecord)
            inputRecord.add(state);
        return state;
    }

    public PadState replay()
    {
        return inputRecord.next();
    }
}

public class TwinStick
{
    public TwinStickState state = new TwinStickState();
    public TwinStickState getNullState()
    {
        state.clear();
        return state;
    }
}

public class RecordableTwinStick : TwinStick
{
    public InputRecord<TwinStickState> inputRecord;
    public void startRecord()
    {
        inputRecord = new InputRecord<TwinStickState>(new TwinStickState());
    }

    public void startReplay(InputRecord<TwinStickState> record)
    {
        inputRecord = record;
        record.reset();
    }

    public TwinStickState getState(bool doRecord = true)
    {
        if (doRecord)
            inputRecord.add(state);
        return state;
    }

    public TwinStickState replay()
    {
        return inputRecord.next();
    }
}

public class Mouse
{
    public MouseState state = new MouseState();
    public MouseState getNullState()
    {
        state.clear();
        return state;
    }
}

public class RecordableMouse : Mouse
{
    public MouseState getState(bool doRecord = true)
    {
        return state;
    }
}

public class RecordableMouseAndPad
{
    public InputRecord<MouseAndPadState> inputRecord;
    public MouseAndPadState state = new MouseAndPadState();
    public Mouse mouse;
    public Pad pad;
    public RecordableMouseAndPad(Mouse mouse, Pad pad)
    {
        this.mouse = mouse;
        this.pad = pad;
    }

    public void startRecord()
    {
        inputRecord = new InputRecord<MouseAndPadState>(new MouseAndPadState());
    }

    public void startReplay(InputRecord<MouseAndPadState> record)
    {
        inputRecord = record;
        record.reset();
    }

    public MouseAndPadState getState(bool doRecord = true)
    {
        state.mouseState = mouse.state;
        state.padState = pad.state;
        if (doRecord)
            inputRecord.add(state);
        return state;
    }

    public MouseAndPadState replay()
    {
        return inputRecord.next();
    }
}

public static class MouseStateButton
{
    public const int LEFT = 1, RIGHT = 2;
}

public static class PadStateButton
{
    public const int A = 16, B = 32, ANY = 48;
}

public static class PadStateDir
{
    public const int UP = 1, DOWN = 2, LEFT = 4, RIGHT = 8;
}
