// Copyright 2004 Kenta Cho. Some rights reserved.
using static Lub;

public class PrefManager
{
    public PrefData prefData = new PrefData();
    public void load(string text)
    {
        string[] p = text.Split(",");
        if (p.Length != 14)
            return;
        int[] n = new int[14];
        for (int i = 0; i < 14; i++)
        {
            n[i] = GameMath.parseNonnegative(p[i]);
            if (n[i] < 0)
                return;
        }

        if ((n[12] > 2) || (n[13] < 1))
            return;
        for (int i = 0; i < 3; i++)
        {
            if (((n[i * 4] < 1) || (n[i * 4 + 2] < 1)) || (n[i * 4 + 3] < 1))
                return;
        }

        for (int i = 0; i < 3; i++)
        {
            var g = prefData.gradeData[i];
            g.reachedLevel = n[i * 4];
            g.hiScore = n[i * 4 + 1];
            g.startLevel = n[i * 4 + 2];
            g.endLevel = n[i * 4 + 3];
        }

        prefData.selectedGrade = n[12];
        prefData.selectedLevel = n[13];
    }

    public void save()
    {
        string s = "";
        for (int i = 0; i < 3; i++)
        {
            var g = prefData.gradeData[i];
            s = s + (g.reachedLevel.ToString() + "," + g.hiScore.ToString() + "," + g.startLevel.ToString() + "," + g.endLevel.ToString() + ",");
        }

        s = s + (prefData.selectedGrade.ToString() + "," + prefData.selectedLevel.ToString());
        if (Host.Available())
            Host.Send("scores.save", s);
    }
}

public class PrefData
{
    public GradeData[] gradeData = new GradeData[]
    {
        new GradeData(),
        new GradeData(),
        new GradeData()
    };
    public int selectedGrade, selectedLevel = 1;
    public void recordStartGame(int grade, int level)
    {
        selectedGrade = grade;
        selectedLevel = level;
    }

    public void recordResult(int level, int score)
    {
        var g = gradeData[selectedGrade];
        if (score > g.hiScore)
        {
            g.hiScore = score;
            g.startLevel = selectedLevel;
            g.endLevel = level;
        }

        if (level > g.reachedLevel)
            g.reachedLevel = level;
        selectedLevel = level;
    }

    public int getMaxLevel(int grade)
    {
        return gradeData[grade].reachedLevel;
    }

    public GradeData getGradeData(int grade)
    {
        return gradeData[grade];
    }
}

public class GradeData
{
    public int reachedLevel = 1, hiScore, startLevel = 1, endLevel = 1;
}
