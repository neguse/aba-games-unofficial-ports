// Copyright 2006 Kenta Cho. Some rights reserved.
using System;

public class ReplayData
{
    public InputRecord<PadState> inputRecord;
    public int seed, score, mode;
    public bool stageRandomized;
    public virtual string encode()
    {
        return "1|" + seed.ToString() + "|" + score.ToString() + "|" + mode.ToString() + "|" + (stageRandomized ? "1" : "0") + "|" + inputRecord.encode();
    }

    public virtual bool decode(string data)
    {
        string[] parts = data.Split("|");
        if (((parts.Length != 6)) || ((parts[0] != "1")))
            return false;
        seed = int.Parse(parts[1]);
        score = GameMath.parseNonnegative(parts[2]);
        mode = GameMath.parseNonnegative(parts[3]);
        if (((((score < 0)) || ((mode < 0)))) || ((mode > 2)))
            return false;
        stageRandomized = parts[4] == "1";
        inputRecord = new InputRecord<PadState>(new PadState());
        inputRecord.decode(parts[5]);
        return true;
    }
}
