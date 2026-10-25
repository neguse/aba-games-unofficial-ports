// Copyright (C) 2000 Kenta Cho. SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-09-24, 2026-10-25).
using System;
using Lub;

public static class Game
{
    static float elapsed;
    static int dx, dy, keyMove;
    static bool buttonHeld, pressPending, scoresLoaded;
    // Runs before each move step, while that step's mouse input is still set; the browser test observes it here.
    public static Action beforeMove;
    public static void OnInit()
    {
        MasMain.initall();
        MasMath.Seed = TinySystem.Random.Shared.Next();
    }

    // MasPanel が呼ぶ操作 (原作のメニューとマウスの代わり)。
    public static void Button(bool down)
    {
        if (down && !buttonHeld) pressPending = true;
        buttonHeld = down;
    }

    public static void Move(int x, int y)
    {
        if (!MasForm.hidden)
            return;
        dx += x;
        dy += y;
    }

    public static void Shift()
    {
        if (MasForm.hidden)
            keyMove += 768;
    }

    public static void Blur()
    {
        if (MasMain.mlspe > 0 && !MasScores.hscsf)
            Pause();
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
        if (!scoresLoaded && GameSave.Ready("scores"))
        {
            scoresLoaded = true;
            string? scores = GameSave.Load("scores");
            if (scores != null)
                MasScores.Load(scores);
        }

        MasPanel.Update(dt);
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
                    if (beforeMove != null) beforeMove();
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
    }

    public static void OnEvent(EventData e)
    {
        if (e.Kind == EventKind.KeyDown)
            MasPanel.OnKey(e.Key);
    }

    public static void OnQuit()
    {
        MasScores.Save();
    }
}
