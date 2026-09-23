// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using static Lub;

public static class MasScores
{
    public static bool hscsf;
    public static int category, pendingRank;
    public static string title = "";
    static string rows = "";
    public static Hscdat[] Table(int n)
    {
        if (n == 0)
            return MasKak.kakhsc;
        if (n == 1)
            return MasTob.tobhsc;
        if (n == 2)
            return MasOok.ookhsc;
        if (n == 3)
            return MasHng.hnghsc;
        if (n == 4)
            return MasGfi.gfihsc;
        return MasResult.ttlhsc;
    }

    public static void Clear()
    {
        MasKak.clearkakhsc();
        MasTob.cleartobhsc();
        MasOok.clearookhsc();
        MasHng.clearhnghsc();
        MasGfi.cleargfihsc();
        MasResult.clearttlhsc();
    }

    public static void sethiscore(int rank, string record, string name)
    {
        if (rank == 1)
            rows = "";
        rows += record + "\t" + name + "\n";
    }

    public static void settitle(string value)
    {
        title = value;
        if (value == "Kakenukero Dougenzaka")
            category = 0;
        else if (value == "Super Tobibako")
            category = 1;
        else if (value == "Oooka Sabaki")
            category = 2;
        else if (value == "Hitonage")
            category = 3;
        else if (value == "Gyaku Fire")
            category = 4;
        else
            category = 5;
    }

    public static string entername(int rank, string record)
    {
        pendingRank = rank;
        hscsf = true;
        if (Host.Available())
            Host.Send("ranking", title + "\n" + rank.ToString() + "\n" + rows);
        return "";
    }

    public static void show()
    {
        if (Host.Available())
            Host.Send("ranking", title + "\n0\n" + rows);
    }

    public static void Accept(string name)
    {
        if (!hscsf)
            return;
        Table(category)[pendingRank].name = name.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");
        hscsf = false;
        pendingRank = 0;
        Save();
    }

    public static void Save()
    {
        string text = "";
        for (int n = 0; n < 6; n++)
            for (int i = 1; i <= 3; i++)
            {
                Hscdat entry = Table(n)[i];
                text += entry.rec.ToString() + "\t" + entry.name + "\n";
            }

        if (Host.Available())
            Host.Send("scores.save", text);
    }

    public static void Load(string text)
    {
        string[] rows = MasText.Split(text, "\n");
        if (rows.Length != 19)
            return;
        for (int n = 0; n < 6; n++)
            for (int i = 1; i <= 3; i++)
            {
                string[] fields = MasText.Split(rows[n * 3 + i - 1], "\t");
                if (fields.Length != 2)
                    return;
                int value = MasMath.Parse(fields[0]);
                if (fields[0] != value.ToString() || fields[1].Length > 160 || value < -99999 || value > 100000000)
                    return;
            }

        for (int n = 0; n < 6; n++)
            for (int i = 1; i <= 3; i++)
            {
                string[] fields = MasText.Split(rows[n * 3 + i - 1], "\t");
                Hscdat entry = Table(n)[i];
                entry.rec = MasMath.Parse(fields[0]);
                entry.name = fields[1];
            }
    }

    public static void ShowCategory(int n)
    {
        if (n == 0)
            MasKak.putkakhiscore();
        else if (n == 1)
            MasTob.puttobhiscore();
        else if (n == 2)
            MasOok.putookhiscore();
        else if (n == 3)
            MasHng.puthnghiscore();
        else if (n == 4)
            MasGfi.putgfihiscore();
        else
            MasResult.putttlhiscore();
    }
}
