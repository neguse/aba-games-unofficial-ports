// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using static Lub;

public class ReplayData
{
    public InputRecord<PadState> padInputRecord;
    public InputRecord<TwinStickState> twinStickInputRecord;
    public InputRecord<MouseAndPadState> mouseAndPadInputRecord;
    public int seed, score, gameMode;
    public float shipTurnSpeed;
    public bool shipReverseFire;
    public void save(string fileName)
    {
        string input = gameMode == 0 ? padInputRecord.encode() : gameMode == 3 ? mouseAndPadInputRecord.encode() : twinStickInputRecord.encode();
        string data = "1|" + seed.ToString() + "|" + score.ToString() + "|" + shipTurnSpeed.ToString() + "|" + (shipReverseFire ? "1" : "0") + "|" + gameMode.ToString() + "|" + input;
        if (Host.Available())
            Host.Send("replay.save", data);
    }

    public bool loadData(string data)
    {
        string[] v = data.Split("|");
        if ((v.Length != 7) || (v[0] != "1"))
            return false;
        seed = int.Parse(v[1]);
        score = int.Parse(v[2]);
        shipTurnSpeed = float.Parse(v[3]);
        shipReverseFire = v[4] == "1";
        gameMode = int.Parse(v[5]);
        if (((((gameMode < 0) || (gameMode > 3)) || (score < 0)) || (shipTurnSpeed < 0)) || (shipTurnSpeed > 5))
            return false;
        if (gameMode == 0)
        {
            padInputRecord = new InputRecord<PadState>(new PadState());
            padInputRecord.decode(v[6]);
        }
        else if (gameMode == 3)
        {
            mouseAndPadInputRecord = new InputRecord<MouseAndPadState>(new MouseAndPadState());
            mouseAndPadInputRecord.decode(v[6]);
        }
        else
        {
            twinStickInputRecord = new InputRecord<TwinStickState>(new TwinStickState());
            twinStickInputRecord.decode(v[6]);
        }

        return true;
    }
}
