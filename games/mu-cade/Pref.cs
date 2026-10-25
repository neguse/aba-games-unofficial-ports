using Lub;

public class PrefManager
{
    public PrefData prefData = new PrefData();
    public void load()
    {
        prefData.init_0();
    }

    public void loadText(string text)
    {
        string[] parts = text.Split(",");
        if (parts.Length != 20)
            return;
        int[] values = McdArrays.Make(20, () => 0);
        for (int i_0 = 0; i_0 < 20; i_0++)
        {
            values[i_0] = GameMath.parseNonnegative(parts[i_0]);
            if (values[i_0] < 0 || i_0 % 2 == 0 && i_0 >= 2 && values[i_0] > values[i_0 - 2])
                return;
        }

        for (int i_1 = 0; i_1 < 10; i_1++)
        {
            prefData.highScore[i_1] = values[i_1 * 2];
            prefData.time[i_1] = values[i_1 * 2 + 1];
        }
    }

    public void save()
    {
        string text = "";
        for (int i = 0; i < 10; i++)
        {
            if (i > 0)
                text += ",";
            text += prefData.highScore[i].ToString() + "," + prefData.time[i].ToString();
        }

        GameSave.Save("scores", text);
    }
}

public class PrefData
{
    public const int RANKING_NUM = 10;
    public int[] highScore = McdArrays.Make(10, () => 0), time = McdArrays.Make(10, () => 0);
    public void init_0()
    {
        for (int i = 0; i < 10; i++)
        {
            highScore[i] = (10 - i) * 10000;
            time[i] = (10 - i) * 10000;
        }
    }

    public void recordResult(int score, int t)
    {
        for (int i = 0; i < 10; i++)
        {
            if (score > highScore[i])
            {
                for (int j = 9; j >= i + 1; j--)
                {
                    highScore[j] = highScore[j - 1];
                    time[j] = time[j - 1];
                }

                highScore[i] = score;
                time[i] = t;
                return;
            }
        }
    }
}
