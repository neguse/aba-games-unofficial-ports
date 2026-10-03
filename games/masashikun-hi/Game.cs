// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24).
using System;
using static Lub;

public static class Game
{
    static float elapsed;
    static int dx, dy, keyMove;
    static bool buttonHeld, pressPending;
    // Input observed at each move step, read by the browser test hooks.
    public static int lastMotion, presses, holds;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        MasMain.initall();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("ready", "");
        }
    }

    public static void Start(int n)
    {
        MasScores.hscsf = false;
        MasMain.sucf = n == 0;
        dx = 0;
        dy = 0;
        keyMove = 0;
        MasForm.mousebt = 0;
        buttonHeld = false;
        pressPending = false;
        if (n == 0)
            MasKak.initkaktitle();
        else if (n == 1)
            MasKak.initkak();
        else if (n == 2)
            MasTob.inittob();
        else if (n == 3)
            MasOok.initook();
        else if (n == 4)
            MasHng.inithng();
        else if (n == 5)
            MasGfi.initgfi();
        else
            MasTitle.inittitle();
    }

    public static void Pause()
    {
        if (MasScores.hscsf)
            return;
        if (MasMain.mlspe == -1)
            MasTitle.endpause();
        else
            MasTitle.startpause();
        dx = 0;
        dy = 0;
        keyMove = 0;
        MasForm.mousebt = 0;
        buttonHeld = false;
        pressPending = false;
    }

    public static void OnFrame(float dt)
    {
        while (Host.Available())
        {
            Host.Poll(out string topic, out string payload);
            if (topic == null)
                break;
            if (topic == "seed")
                MasMath.Seed = MasMath.Parse(payload);
            if (topic == "move" && MasForm.hidden)
            {
                string[] p = MasText.Split(payload, ",");
                if (p.Length == 2)
                {
                    dx += MasMath.Parse(p[0]);
                    dy += MasMath.Parse(p[1]);
                }
            }

            if (topic == "button")
            {
                bool down = payload == "1";
                if (down && !buttonHeld) pressPending = true;
                buttonHeld = down;
            }

            if (topic == "shift" && MasForm.hidden)
                keyMove += 768;
            if (topic == "start")
                Start(MasMath.Parse(payload));
            if (topic == "pause")
                Pause();
            if (topic == "blur" && MasMain.mlspe > 0 && !MasScores.hscsf)
                Pause();
            if (topic == "scores")
                MasScores.Load(payload);
            if (topic == "name")
                MasScores.Accept(payload);
            if (topic == "rank")
                MasScores.ShowCategory(MasMath.Parse(payload));
            if (topic == "clear")
            {
                MasScores.Clear();
                MasScores.Save();
            }
        }

        if (!MasScores.hscsf)
        {
            elapsed += Math.Min(dt, 0.25f);
            if (elapsed >= 0.033f)
            {
                MasForm.mousemv = MasForm.hidden ? MasMath.Round(MasMath.Sqrt((float)dx * dx + (float)dy * dy)) + keyMove : 0;
                dx = 0;
                dy = 0;
                keyMove = 0;
                while (elapsed >= 0.033f && !MasScores.hscsf)
                {
                    elapsed -= 0.033f;
                    MasForm.mousebt = pressPending ? 1 : buttonHeld ? 2 : 0;
                    pressPending = false;
                    if (MasForm.mousemv > 0)
                        lastMotion = MasForm.mousemv;
                    if (MasForm.mousebt == 1)
                        presses++;
                    if (MasForm.mousebt == 2)
                        holds++;
                    MasMain.moveall();
                    if (MasForm.mousebt == 1)
                        MasForm.mousebt = 2;
                    MasForm.mousemv = 0;
                }

                MasForm.spans.Clear();
                MasMain.putall();
            }
        }
        else
            elapsed = 0;
        MasForm.Frame();
        if (Host.Available())
            Host.Send("state", MasMain.mlspe.ToString() + "," + (MasForm.hidden ? "1" : "0"));
    }

    public static void OnQuit()
    {
        MasScores.Save();
    }
}
