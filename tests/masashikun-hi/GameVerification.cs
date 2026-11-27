using System;
using System.Collections.Generic;

public static class MasVerification
{
    static int failures;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }

    static int Hash(int h, int value) { return (h ^ value) * 16777619; }

    static int[] screen = new int[640 * 480];
    static bool inside;

    // FNV-1a over the 640x480 picture that MasForm.spans (x, y, w, h, color) paints on a cleared screen.
    static int Pixels()
    {
        for (int n = 0; n < screen.Length; n++) screen[n] = 0;
        inside = true;
        List<float> spans = MasForm.spans;
        for (int n = 0; n < spans.Count; n += 5)
        {
            int x = (int)spans[n], y = (int)spans[n + 1], w = (int)spans[n + 2], h = (int)spans[n + 3], c = (int)spans[n + 4];
            for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++)
                {
                    if (px < 0 || px >= 640 || py < 0 || py >= 480) { inside = false; continue; }
                    screen[py * 640 + px] = c;
                }
        }
        int hash = -2128831035;
        for (int n = 0; n < screen.Length; n++) hash = Hash(hash, screen[n]);
        return hash;
    }

    static void Initial(int g)
    {
        if (g == 0) MasTitle.inittitle();
        else if (g == 1) MasKak.initkak();
        else if (g == 2) MasTob.inittob();
        else if (g == 3) MasOok.initook();
        else if (g == 4) MasHng.inithng();
        else MasGfi.initgfi();
    }

    // Each scene's own state, in the order of the reference hashes.
    static int Fields(int g, int h)
    {
        if (g == 0)
        {
            h = Hash(h, MasTitle.bfmlspe); h = Hash(h, MasTitle.titlecou); h = Hash(h, MasTitle.demospe);
            h = Hash(h, MasTitle.td); h = Hash(h, MasTitle.tr); h = Hash(h, MasTitle.stn);
        }
        else if (g == 1)
        {
            h = Hash(h, MasKak.myx); h = Hash(h, MasKak.myy); h = Hash(h, MasKak.myp); h = Hash(h, MasKak.myd);
            h = Hash(h, MasKak.myc); h = Hash(h, MasKak.mychar); h = Hash(h, MasKak.myvy); h = Hash(h, MasKak.speed);
            h = Hash(h, MasKak.tmpsp); h = Hash(h, MasKak.mvsum); h = Hash(h, MasKak.zm); h = Hash(h, MasKak.kakcou);
            h = Hash(h, MasKak.kakdemocou); h = Hash(h, MasKak.time);
        }
        else if (g == 2)
        {
            h = Hash(h, MasTob.myx); h = Hash(h, MasTob.myy); h = Hash(h, MasTob.myp); h = Hash(h, MasTob.myc);
            h = Hash(h, MasTob.mychar); h = Hash(h, MasTob.mymy); h = Hash(h, MasTob.mymx); h = Hash(h, MasTob.speed);
            h = Hash(h, MasTob.tmpsp); h = Hash(h, MasTob.mvsum); h = Hash(h, MasTob.tobtry); h = Hash(h, MasTob.zm);
            h = Hash(h, MasTob.tobcou); h = Hash(h, MasTob.tobdemocou); h = Hash(h, MasTob.tbc); h = Hash(h, MasTob.tbcc);
        }
        else if (g == 3)
        {
            h = Hash(h, MasOok.myx); h = Hash(h, MasOok.myy); h = Hash(h, MasOok.myp); h = Hash(h, MasOok.myc);
            h = Hash(h, MasOok.mychar); h = Hash(h, MasOok.speed); h = Hash(h, MasOok.mvsum); h = Hash(h, MasOok.myhng);
            h = Hash(h, MasOok.dist); h = Hash(h, MasOok.endist); h = Hash(h, MasOok.enhng); h = Hash(h, MasOok.enspeed);
            h = Hash(h, MasOok.plcou); h = Hash(h, MasOok.enplcou); h = Hash(h, MasOok.time); h = Hash(h, MasOok.zm);
            h = Hash(h, MasOok.ookcou); h = Hash(h, MasOok.ookdemocou);
        }
        else if (g == 4)
        {
            h = Hash(h, MasHng.myp); h = Hash(h, MasHng.myx); h = Hash(h, MasHng.myy); h = Hash(h, MasHng.myd);
            h = Hash(h, MasHng.mymx); h = Hash(h, MasHng.mymy); h = Hash(h, MasHng.mytd); h = Hash(h, MasHng.speed);
            h = Hash(h, MasHng.tmpsp); h = Hash(h, MasHng.mypt); h = Hash(h, MasHng.dist); h = Hash(h, MasHng.flx);
            h = Hash(h, MasHng.fly); h = Hash(h, MasHng.flmx); h = Hash(h, MasHng.flmy); h = Hash(h, MasHng.zm);
            h = Hash(h, MasHng.hngcou); h = Hash(h, MasHng.hngdemocou); h = Hash(h, MasHng.dmmbtcou); h = Hash(h, MasHng.hngtry);
        }
        else
        {
            h = Hash(h, MasGfi.myp); h = Hash(h, MasGfi.myx); h = Hash(h, MasGfi.maxmymy); h = Hash(h, MasGfi.myy);
            h = Hash(h, MasGfi.tpy); h = Hash(h, MasGfi.myd); h = Hash(h, MasGfi.mymy); h = Hash(h, MasGfi.myc);
            h = Hash(h, MasGfi.speed); h = Hash(h, MasGfi.tmpsp); h = Hash(h, MasGfi.dist); h = Hash(h, MasGfi.mvsum);
            h = Hash(h, MasGfi.mychar); h = Hash(h, MasGfi.flx); h = Hash(h, MasGfi.fly); h = Hash(h, MasGfi.kkx);
            h = Hash(h, MasGfi.zm); h = Hash(h, MasGfi.gficou); h = Hash(h, MasGfi.gfidemocou);
        }
        return h;
    }

    // Original Pascal, Delphi argument order and the two 1.11e adjacent-memory reads.
    static void Originals()
    {
        int[] expected = new int[] { 721291957, -299353891, 2025337817, -1483790467, -214543261, 153716629 };
        int[] raster = new int[]
        {
            -1516359561, 1194163777, -1016830567, 1351991218, 1282498746,
            -184272645, 24323694, -1012357428, -1294942889, -1162818059,
            514100574, -529365398, -43117824, -716159673, -445345398,
            686293252, -1041335864, -1843232595, 506457703, -844295022,
            1427633240, 1404584644, -58863044, -1658075133, -1091368258,
            1665416618, 852244286, -419671087, 1317037097, -1241489979
        };
        for (int g = 0; g < 6; g++)
        {
            MasMath.Seed = 12345; MasHira.clearhira(); MasMain.sucf = false; Initial(g);
            int h = -2128831035;
            int snapshot = 0;
            for (int i = 0; i < 600; i++)
            {
                MasForm.mousemv = (i * 71) % 1200; MasForm.mousebt = i % 19 == 0 ? 1 : 0;
                MasMain.moveall(); MasForm.spans.Clear(); MasMain.putall(); MasScores.Accept("");
                h = Hash(Hash(h, MasMain.mlspe), MasMath.Seed);
                h = Fields(g, h);
                if (i == 64 || i == 127 || i == 255 || i == 511 || i == 599)
                {
                    int pixels = Pixels();
                    string label = (g + 1).ToString() + "/" + i.ToString();
                    Check(inside, "span inside the screen " + label);
                    Check(pixels == raster[g * 5 + snapshot], "raster " + label);
                    snapshot++;
                }
            }
            Check(h == expected[g], "state " + (g + 1).ToString());
        }
        Console.WriteLine("3600 original Pascal updates and 30 exact rasters");
    }

    // Scene numbers run from -6 (demos) to 16 (result).
    static bool[] seen = new bool[32];
    static void ClearSeen() { for (int n = 0; n < seen.Length; n++) seen[n] = false; }
    static void See(int state) { if (state + 8 >= 0 && state + 8 < seen.Length) seen[state + 8] = true; }
    static bool Seen(int state) { return seen[state + 8]; }

    // The text MasScores.Save hands to GameSave: per category, ranks 1 to 3 as "record<TAB>name<LF>".
    static string StoredText()
    {
        string text = "";
        for (int n = 0; n < 6; n++)
            for (int i = 1; i <= 3; i++)
            {
                Hscdat entry = MasScores.Table(n)[i];
                text += entry.rec.ToString() + "\t" + entry.name + "\n";
            }
        return text;
    }

    static void Competition()
    {
        MasMath.Seed = 1; Game.Start(0);
        ClearSeen();
        bool completed = false;
        for (int i = 1; i <= 12000; i++)
        {
            int state = MasMain.mlspe; See(state);
            MasForm.mousemv = 1500; MasForm.mousebt = i % 19 == 0 ? 1 : 0;
            MasMain.moveall(); MasForm.spans.Clear(); MasMain.putall(); MasScores.Accept("Player");
            if (Seen(16) && MasMain.mlspe == 0) { completed = true; break; }
        }
        Check(completed, "five-event competition completion");
        for (int state = 1; state <= 16; state++) Check(Seen(state), "competition state " + state.ToString());
        int total = 0;
        for (int n = 0; n < MasMain.score.Length; n++) total += MasMain.score[n];
        Check(MasResult.ttlhsc[1].rec == total && total > 0, "total score");

        Game.Start(1); MasKak.kakcou = 100; Game.Pause();
        Check(MasMain.mlspe == -1 && !MasForm.hidden, "pause");
        for (int i = 0; i < 50; i++) MasMain.moveall();
        Check(MasKak.kakcou == 100, "pause freezes the event");
        Game.Pause();
        Check(MasMain.mlspe == 1 && MasForm.hidden, "pause release");

        MasScores.Clear(); MasKak.setkakhiscore(12345);
        Check(MasScores.hscsf, "record asks for a name");
        MasScores.Accept("A"); MasKak.setkakhiscore(12000); MasScores.Accept("B");
        Check(MasKak.kakhsc[1].name == "B" && MasKak.kakhsc[2].name == "A" && MasKak.kakhsc[2].rec == 12345, "faster dash ranks first");
        MasTob.settobhiscore(123); MasScores.Accept("Jump"); MasHng.sethnghiscore(5000); MasScores.Accept("Throw");
        // The lub here neither finishes loading nor keeps what is saved (GameSave.Ready stays false and Save writes nothing),
        // so the stored text is the six tables in the format MasScores.Save writes.
        MasScores.Save();
        string saved = StoredText();
        MasScores.Clear(); MasScores.Load(saved);
        Check(MasKak.kakhsc[1].rec == 12000 && MasTob.tobhsc[1].name == "Jump" && MasHng.hnghsc[1].rec == 5000, "six-category score storage");
        MasScores.Load(saved.Replace("12000", "oops"));
        Check(MasKak.kakhsc[1].rec == 12000, "invalid score storage is ignored");
        Console.WriteLine("all 16 competition states, three attempts, total score, pause and six-category score storage");
    }

    static void Demos()
    {
        MasMath.Seed = 12345; Game.Start(6);
        ClearSeen();
        for (int i = 1; i <= 10000; i++)
        {
            See(MasMain.mlspe); MasForm.mousemv = 0; MasForm.mousebt = 0;
            MasMain.moveall(); MasForm.spans.Clear(); MasMain.putall();
            if (Seen(-2) && Seen(-3) && Seen(-4) && Seen(-5) && Seen(-6)) break;
        }
        for (int n = -6; n <= -2; n++) Check(Seen(n), "automatic demo " + n.ToString());
    }

    // The mouse input of each move step, as Game.OnFrame sets it just before MasMain.moveall.
    static List<int> motions = new List<int>(), buttons = new List<int>();
    static void Observe() { motions.Add(MasForm.mousemv); buttons.Add(MasForm.mousebt); }

    static void InputTiming()
    {
        // This lub never has input focus. MasPanel.Update reacts to that once, on the first call after the focus it starts with:
        // it releases the button and blurs the game (which pauses an event). Take that edge here, so that MasPanel.Update
        // leaves Game alone during the frames below.
        MasPanel.Update(0);
        Game.beforeMove = Observe;
        // Game.OnFrame has not run before, so its accumulated time is still 0.
        Game.Start(1);
        Game.Move(3, 0); Game.Move(0, 4); Game.Button(true); Game.Button(false);
        Game.OnFrame(.02f);
        Check(motions.Count == 0, "no move step before 33ms");
        Game.OnFrame(.02f);
        Check(motions.Count >= 1 && motions[0] == 5 && buttons[0] == 1, "accumulated motion and short click");
        Game.OnFrame(.03f);
        Check(motions.Count >= 2 && motions[1] == 0 && buttons[1] == 0, "short click released");
        Game.Shift(); Game.Button(true); Game.OnFrame(.033f);
        Check(motions.Count >= 3 && motions[2] == 768 && buttons[2] == 1, "Shift and click");
        Game.OnFrame(.033f);
        Check(buttons.Count >= 4 && buttons[3] == 2, "held click");
        Game.beforeMove = null;
        Console.WriteLine("all five automatic demos, 33ms input accumulation, short click, held click and Shift");
    }

    public static void Main()
    {
        Game.OnInit();
        Originals();
        Competition();
        Demos();
        InputTiming();
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
