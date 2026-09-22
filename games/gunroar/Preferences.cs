// Copyright 2005 Kenta Cho. Some rights reserved.
using static Lub;

public class PrefManager
{
    public PrefData prefData = new PrefData();
    public void load(string data)
    {
        string[] v = data.Split(",");
        if (v.Length != 5)
            return;
        int[] n = new int[5];
        for (int i = 0; i < 5; i++)
        {
            n[i] = GameMath.parseNonnegative(v[i]);
            if (n[i] < 0)
                return;
        }

        if (n[4] > 3)
            return;
        for (int i = 0; i < 4; i++)
            prefData.scores[i] = n[i];
        prefData.gameMode = n[4];
    }

    public void save()
    {
        string s = "";
        for (int i = 0; i < 4; i++)
            s = s + (prefData.scores[i].ToString() + ",");
        if (Host.Available())
            Host.Send("scores.save", s + prefData.gameMode.ToString());
    }
}

public class PrefData
{
    public int[] scores = new int[] { 0, 0, 0, 0 };
    public int gameMode;
    public void recordGameMode(int mode)
    {
        gameMode = mode;
    }

    public void recordResult(int score, int mode)
    {
        if (score > scores[mode])
            scores[mode] = score;
        gameMode = mode;
    }

    public int highScore(int mode)
    {
        return scores[mode];
    }
}
