using static Lub;

public class Record
{
    public const int RankingNum = 10;
    int[] storedScores = GtgArrays.Make(10, () => 0);
    int storedLastScore, storedLastRank = -1;
    public void Load()
    {
        for (int i = 0; i < 10; i++)
            storedScores[(i)] = (10 - i) * 10000;
    }

    public void Update()
    {
    }

    public void Save()
    {
        GtgPreference.Save(this);
    }

    public void RecordScore(int s)
    {
        storedLastScore = s;
        storedLastRank = -1;
        for (int i = 0; i < 10; i++)
            if (s > storedScores[(i)])
            {
                for (int j = 8; j >= i; j--)
                    storedScores[(j + 1)] = storedScores[(j)];
                storedLastRank = i;
                storedScores[(i)] = s;
                break;
            }

        Save();
    }

    public int[] Scores
    {
        get
        {
            return storedScores;
        }
    }

    public int LastScore
    {
        get
        {
            return storedLastScore;
        }
    }

    public int LastRank
    {
        get
        {
            return storedLastRank;
        }
    }
}

public static class GtgPreference
{
    public static void Load(Record record, string text)
    {
        string[] parts = text.Split(",");
        if (parts.Length != 10)
            return;
        int[] values = GtgArrays.Make(10, () => 0);
        for (int i = 0; i < 10; i++)
        {
            values[(i)] = GameMath.parseNonnegative(parts[(i)]);
            if (values[(i)] < 0 || i > 0 && values[(i)] > values[(i - 1)])
                return;
        }

        for (int i = 0; i < 10; i++)
            record.Scores[(i)] = values[(i)];
    }

    public static void Save(Record record)
    {
        string text = "";
        for (int i = 0; i < 10; i++)
        {
            if (i > 0)
                text += ",";
            text += record.Scores[(i)].ToString();
        }

        if (Host.Available())
            Host.Send("scores.save", text);
    }
}
