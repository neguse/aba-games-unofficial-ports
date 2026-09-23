// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public static class PadDir
{
    public const int UP = 1, DOWN = 2, LEFT = 4, RIGHT = 8;
}

public static class PadButton
{
    public const int A = 16, B = 32, ANY = 48;
}

public class Pad
{
    public int directions, buttons, lastDirState, lastButtonState;
    public bool pause, escape;
    public int getDirState()
    {
        lastDirState = directions;
        return directions;
    }

    public int getButtonState()
    {
        lastButtonState = buttons;
        return buttons;
    }
}

public class RecordablePad : Pad
{
    public const int REPLAY_END = -1;
    public PadRecord padRecord;
    public void startRecord()
    {
        padRecord = new PadRecord();
    }

    public void record()
    {
        padRecord.add(lastDirState | lastButtonState);
    }

    public void startReplay(PadRecord record)
    {
        padRecord = record;
        record.reset();
    }

    public int replay()
    {
        return padRecord.hasNext() ? padRecord.next() : REPLAY_END;
    }
}

public class PadRecord
{
    public List<int> data = new List<int>(), lengths = new List<int>();
    public int index, remaining;
    public void add(int value)
    {
        int last = data.Count - 1;
        if ((last >= 0) && (data[last] == value))
            lengths[last]++;
        else
        {
            data.Add(value);
            lengths.Add(1);
        }
    }

    public void reset()
    {
        index = 0;
        remaining = 0;
    }

    public bool hasNext()
    {
        return index < data.Count;
    }

    public int next()
    {
        if (remaining <= 0)
            remaining = lengths[index];
        int value = data[index];
        remaining--;
        if (remaining == 0)
            index++;
        return value;
    }

    public string encode()
    {
        string s = "";
        for (int i = 0; i < data.Count; i++)
            s = s + (lengths[i].ToString() + "/" + data[i].ToString() + ";");
        return s;
    }

    public bool decode(string text)
    {
        if (text.Length == 0 || text.Length > 2000000 || text.Substring(text.Length - 1) != ";")
            return false;
        string[] rows = text.Split(";");
        data.Clear();
        lengths.Clear();
        reset();
        for (int i = 0; i < rows.Length - 1; i++)
        {
            string[] p = rows[i].Split("/");
            if (p.Length != 2)
                return false;
            int n = GameMath.parseNonnegative(p[0]), v = GameMath.parseNonnegative(p[1]);
            if ((((n <= 0) || (n > 1000000)) || (v < 0)) || (v > 63))
                return false;
            lengths.Add(n);
            data.Add(v);
        }

        return data.Count > 0;
    }
}

public class ReplayData
{
    public PadRecord padRecord;
    public float level;
    public int grade, seed;
    public void save(string name)
    {
        Game.savedReplay = "1|" + seed.ToString() + "|" + GameMath.integer(level).ToString() + "|" + grade.ToString() + "|" + padRecord.encode();
        if (Lub.Host.Available())
            Lub.Host.Send("replay.save", Game.savedReplay);
    }

    public void load(string name)
    {
        decode(Game.savedReplay);
    }

    public bool decode(string text)
    {
        string[] p = text.Split("|");
        if ((p.Length != 5) || (p[0] != "1"))
            return false;
        string seedText = p[1];
        if (seedText.Length == 0 || seedText.Length > 11) return false;
        bool negative = seedText.Substring(0, 1) == "-";
        int first = negative ? 1 : 0;
        if (first == seedText.Length) return false;
        int s = 0;
        for (int i = first; i < seedText.Length; i++) {
            int digit = "0123456789".IndexOf(seedText.Substring(i, 1));
            if (digit < 0 || s < -214748364 || (s == -214748364 && digit > (negative ? 8 : 7))) return false;
            s = s * 10 - digit;
        }
        if (!negative) s = -s;
        int l = GameMath.parseNonnegative(p[2]), g = GameMath.parseNonnegative(p[3]);
        if (((l < 1) || (g < 0)) || (g >= 3))
            return false;
        var record = new PadRecord();
        if (!(record.decode(p[4])))
            return false;
        seed = s;
        level = l;
        grade = g;
        padRecord = record;
        return true;
    }
}
