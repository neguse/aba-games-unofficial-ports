// Copyright 2006 Kenta Cho. Some rights reserved.
using static Lub;

public class Preference
{
    public const int RANKING_NUM = 10, MODE_NUM = 3;
    public int[][] highScore = new int[3][];
    public int lastMode = 2;
    public Preference()
    {
        for (int j = 0; j < 3; j++)
        {
            highScore[j] = new int[10];
            for (int i = 0; i < 10; i++)
                highScore[j][i] = (10 - i) * 10000;
        }
    }

    public virtual void load_1(string data)
    {
        string[] fields = data.Split(",");
        if (fields.Length != 31)
            return;
        int[] values = new int[31];
        for (int i = 0; i < 31; i++)
        {
            values[i] = GameMath.parseNonnegative(fields[i]);
            if (values[i] < 0)
                return;
        }

        if (values[30] > 2)
            return;
        for (int j = 0; j < 3; j++)
            for (int i = 0; i < 10; i++)
                highScore[j][i] = values[j * 10 + i];
        lastMode = values[30];
    }

    public virtual void save()
    {
        string data = "";
        for (int j = 0; j < 3; j++)
            for (int i = 0; i < 10; i++)
                data = data + highScore[j][i].ToString() + ",";
        if (Host.Available())
            Host.Send("scores.save", data + lastMode.ToString());
    }

    public virtual void setMode(int mode)
    {
        lastMode = mode;
    }

    public virtual void recordResult(int score, int mode)
    {
        lastMode = mode;
        for (int i = 0; i < 10; i++)
        {
            if (score > highScore[mode][i])
            {
                for (int j = 9; j > i; j--)
                    highScore[mode][j] = highScore[mode][j - 1];
                highScore[mode][i] = score;
                return;
            }
        }
    }
}
