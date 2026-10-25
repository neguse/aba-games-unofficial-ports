// SPDX-License-Identifier: GPL-2.0-or-later
// TinyC# browser adaptation (2026-10-25): the original Windows menu and dialogs.
using System;
using System.Collections.Generic;
using Lub;
using Lubx;

// MasPanel のボタンの当たり判定と、押したときの処理。
public class MasPanelHit
{
    public float X, Y, W, H;
    public Action Action;

    public MasPanelHit(float x, float y, float w, float h, Action action)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
        Action = action;
    }
}

// 原作の Windows のメニューとダイアログの代わり (競技の選択、ポーズ、記録の表示、記録を出したときの名前の入力、記録の消去)。
// 960x720 の UI 座標で、ゲームの絵の上に描く (MasForm.Frame の pass の中)。
// キーボード、マウス、pad のどれでも操作できる。名前は pad でも 1 文字ずつ選んで入れられる。
public static class MasPanel
{
    static readonly string[] Events = new string[] { "Five-event competition", "Kakenukero Dougenzaka", "Super Tobibako", "Oooka Sabaki", "Hitonage", "Gyaku Fire" };
    static readonly string[] Categories = new string[] { "Kakenukero Dougenzaka", "Super Tobibako", "Oooka Sabaki", "Hitonage", "Gyaku Fire", "Overall" };
    static readonly string[] Help = new string[]
    {
        "All five events. Click through each event's instructions.",
        "Move to build speed. Click to jump and run down the hill.",
        "Run, click at the arrow, build speed in the air, then click twice.",
        "Build strength, then click to pull the person toward your arrow.",
        "Move to spin. Hold the button to aim, then release to throw.",
        "Build speed. Click as the trampoline stretches to bounce higher."
    };
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_.";
    const string Lower = "abcdefghijklmnopqrstuvwxyz";
    const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string FontPath = "fonts/panel.ttf";

    static readonly List<MasPanelHit> hits = new List<MasPanelHit>();
    static PadInput previous = new PadInput();
    static bool previousAction, suppressButton, confirmClear, captured, wasFocused = true;
    static int selected, category, letter, pendingRank;
    static bool menuOpen = true, rankingOpen;
    static string playerName = "", rankingTitle = "";
    static string[] rankingRows = new string[0];
    // 名前の入力は KeyDown の event の scancode を届いた順に読む (1 frame に同じ key や複数の key が来ても落とさず、順番も保つ)。
    // OnKey が typed に溜め、Update が keys と入れ替えて読む。
    static List<int> typed = new List<int>(), keys = new List<int>();
    static Text? text;
    static SpriteBatch? batch;

    static bool Visible()
    {
        return menuOpen || rankingOpen;
    }

    // ゲームの中でマウスを閉じ込める (競技の最中だけ)。
    static bool CaptureRequested()
    {
        return MasForm.hidden && MasMain.mlspe > 0 && !menuOpen && !rankingOpen;
    }

    // MasScores が記録を見せるときに呼ぶ。rank が 0 でなければ、その順位の名前を入れてもらう。
    public static void OnRanking(string title, int rank, string rows)
    {
        rankingTitle = title;
        pendingRank = rank;
        string[] lines = MasText.Split(rows, "\n");
        rankingRows = new string[3];
        for (int i = 0; i < 3; i++)
            rankingRows[i] = lines.Length > i ? lines[i].Replace("\t", " ") : "";
        rankingOpen = true;
        confirmClear = false;
        playerName = "";
        previousAction = false;
        suppressButton = true;
        Game.Button(false);
    }

    static void Start(int n)
    {
        if (rankingOpen || n < 0 || n > 6)
            return;
        selected = Math.Min(n, 5);
        menuOpen = false;
        previousAction = false;
        suppressButton = true;
        Game.Button(false);
        Game.Start(n);
    }

    static void Pause()
    {
        if (rankingOpen)
            return;
        Game.Button(false);
        Game.Pause();
        previousAction = false;
        if (MasMain.mlspe == -1)
            menuOpen = false;
    }

    static void ShowMenu()
    {
        if (rankingOpen)
            return;
        if (MasMain.mlspe > 0)
        {
            Game.Button(false);
            Game.Pause();
        }
        menuOpen = true;
        previousAction = false;
        suppressButton = true;
    }

    static void ShowRanking(int n)
    {
        if (pendingRank != 0)
            return;
        category = Math.Clamp(n, 0, 5);
        if (MasMain.mlspe > 0 && !rankingOpen)
            Game.Pause();
        Game.Button(false);
        MasScores.ShowCategory(category);
        suppressButton = true;
    }

    static void ClearRecords()
    {
        MasScores.Clear();
        MasScores.Save();
        MasScores.ShowCategory(category);
        confirmClear = false;
    }

    static void AppendText(string s)
    {
        if (!rankingOpen || pendingRank == 0)
            return;
        string value = (playerName + s).Replace("\t", " ").Replace("\n", " ").Replace("\r", " ");
        playerName = value.Length > 40 ? value.Substring(0, 40) : value;
    }

    static void RemoveLetter()
    {
        if (playerName.Length != 0)
            playerName = playerName.Substring(0, playerName.Length - 1);
    }

    static void PreviousLetter()
    {
        letter = (letter + Alphabet.Length - 1) % Alphabet.Length;
    }

    static void NextLetter()
    {
        letter = (letter + 1) % Alphabet.Length;
    }

    static void CloseRanking()
    {
        if (!rankingOpen)
            return;
        if (pendingRank != 0)
            MasScores.Accept(playerName);
        pendingRank = 0;
        rankingOpen = false;
        confirmClear = false;
        suppressButton = true;
    }

    static bool Rising(bool current, bool old)
    {
        return current && !old;
    }

    static bool Key(string name)
    {
        return Lub.Input.KeyPressed(name);
    }

    public static void OnKey(int scancode)
    {
        typed.Add(scancode);
    }

    // scancode が名前に入る文字なら、その文字 (US 配列)。
    static string? Typed(int scancode, bool shift)
    {
        if (scancode >= 4 && scancode <= 29)
            return (shift ? Upper : Lower).Substring(scancode - 4, 1);
        if (scancode >= 30 && scancode <= 38)
            return (scancode - 29).ToString();
        if (scancode == 39)
            return "0";
        if (scancode == 44)
            return " ";
        if (scancode == 45)
            return "-";
        if (scancode == 54)
            return ",";
        if (scancode == 55)
            return ".";
        if (scancode == 56)
            return "/";
        return null;
    }

    static void HandlePointer()
    {
        if (!Visible() || !Lub.Input.MousePressed(1))
            return;
        float x = GameInput.PointerX() * 1.5f, y = GameInput.PointerY() * 1.5f;
        foreach (var hit in hits)
            if (x >= hit.X && x < hit.X + hit.W && y >= hit.Y && y < hit.Y + hit.H)
            {
                hit.Action();
                return;
            }
    }

    public static void Update(float dt)
    {
        var read = keys;
        keys = typed;
        typed = read;
        typed.Clear();
        bool focused = GameInput.Focused();
        if (wasFocused && !focused)
        {
            Game.Button(false);
            Game.Blur();
        }
        wasFocused = focused;
        bool capture = focused && CaptureRequested();
        if (capture != captured)
            captured = Lub.Input.CaptureMouse(capture) && capture;
        if (!focused)
        {
            previous = new PadInput();
            previousAction = false;
            suppressButton = true;
            return;
        }
        HandlePointer();
        var pad = GameInput.Pad();
        bool up = Rising((pad.LeftMask & 1) != 0, (previous.LeftMask & 1) != 0) || Key("up");
        bool down = Rising((pad.LeftMask & 2) != 0, (previous.LeftMask & 2) != 0) || Key("down");
        bool left = Rising((pad.LeftMask & 4) != 0, (previous.LeftMask & 4) != 0) || Key("left");
        bool right = Rising((pad.LeftMask & 8) != 0, (previous.LeftMask & 8) != 0) || Key("right");
        bool accept = Key("enter") || Rising(pad.Primary, previous.Primary);
        bool back = Key("escape") || Rising(pad.Back, previous.Back) || Rising(pad.Secondary, previous.Secondary);
        bool pause = Key("f3") || Rising(pad.Start, previous.Start);
        if (rankingOpen)
        {
            if (confirmClear)
            {
                if (accept)
                    ClearRecords();
                else if (back)
                    confirmClear = false;
            }
            else if (pendingRank != 0)
            {
                bool shift = Lub.Input.KeyDown("left shift") || Lub.Input.KeyDown("right shift");
                foreach (int scancode in keys)
                {
                    string? s = Typed(scancode, shift);
                    if (s != null)
                        AppendText(s);
                }
                if (Key("backspace") || Rising(pad.West, previous.West))
                    RemoveLetter();
                if (left || up)
                    PreviousLetter();
                if (right || down)
                    NextLetter();
                if (Rising(pad.Primary, previous.Primary))
                    AppendText(Alphabet.Substring(letter, 1));
                if (Key("enter") || pause || back)
                    CloseRanking();
            }
            else
            {
                if (left || up)
                    ShowRanking((category + 5) % 6);
                if (right || down)
                    ShowRanking((category + 1) % 6);
                if (Key("delete") || Rising(pad.North, previous.North))
                    confirmClear = true;
                if (accept || back || pause)
                    CloseRanking();
            }
        }
        else
        {
            if (Key("f2"))
                Start(0);
            for (int f = 5; f <= 9; f++)
                if (Key("f" + f))
                    Start(f - 4);
            if (Key("f10"))
                ShowRanking(category);
            if (Key("f11"))
                Start(6);
            if (Key("f4"))
            {
                if (menuOpen)
                    menuOpen = false;
                else
                    ShowMenu();
            }
            if (pause)
                Pause();
            if (menuOpen)
            {
                if (up)
                    selected = (selected + 5) % 6;
                if (down)
                    selected = (selected + 1) % 6;
                if (accept)
                    Start(selected);
                if (Rising(pad.West, previous.West))
                    ShowRanking(category);
                if (back)
                    menuOpen = false;
            }
            else if (back && MasMain.mlspe > 0)
            {
                Game.Button(false);
                Game.Blur();
                previousAction = false;
                suppressButton = true;
            }
            else if (back && MasMain.mlspe <= 0)
                ShowMenu();

            if (!menuOpen && !rankingOpen)
                Play(dt, pad);
        }
        previous = pad;
    }

    // 競技の最中のボタン、マウスの移動量、Shift。
    static void Play(float dt, PadInput pad)
    {
        bool action = Lub.Input.MouseDown(1) || Lub.Input.KeyDown("space") || pad.Primary || pad.RightTrigger > .35f;
        bool quickPress = Lub.Input.MousePressed(1) || Key("space");
        bool blocked = suppressButton;
        if (suppressButton)
        {
            if (!action && !quickPress)
                suppressButton = false;
            action = false;
        }
        if (!blocked && quickPress && !action && !previousAction)
        {
            if (MasMain.mlspe == -1)
                Pause();
            else
            {
                Game.Button(true);
                Game.Button(false);
            }
        }
        if (action != previousAction)
        {
            if (action && MasMain.mlspe == -1)
                Pause();
            else
                Game.Button(action);
            previousAction = action;
        }
        if (MasForm.hidden && MasMain.mlspe > 0)
        {
            Lub.Input.MouseDelta(out float dx, out float dy);
            dx += (pad.LeftX + pad.RightX) * 800 * Math.Clamp(dt, 0, .25f);
            dy -= (pad.LeftY + pad.RightY) * 800 * Math.Clamp(dt, 0, .25f);
            if (dx != 0 || dy != 0)
                Game.Move((int)Math.Round(dx), (int)Math.Round(dy));
            if (Key("left shift") || Key("right shift") || Rising(pad.LeftShoulder, previous.LeftShoulder) || Rising(pad.RightShoulder, previous.RightShoulder) || Rising(pad.West, previous.West))
                Game.Shift();
        }
    }

    // ------------------------------------------------------------------ draw

    static bool Ensure()
    {
        if (text != null && batch != null)
            return true;
        Lub.Io.LoadBytes(FontPath, out var bytes, out _, out _, out _);
        if (bytes == null)
            return false;
        text = new Text("mas_panel_text", FontPath, 26, 512);
        batch = new SpriteBatch(960, 720, "mas_panel", "mas_panel");
        return true;
    }

    // rgb は 0xRRGGBB。
    static Color Rgba(int rgb, float alpha)
    {
        return Color.Rgb(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);
    }

    static void Rect(float x, float y, float w, float h, int rgb, float alpha)
    {
        batch!.Rect(x, y, w, h, Rgba(rgb, alpha));
    }

    // (x, y) は文字の左上。
    static void Label(string s, float x, float y, float size, int rgb)
    {
        var t = text!;
        float scale = size / t.Px;
        t.Draw(batch!, s, x, y + t.Ascent * scale, Rgba(rgb, 1), scale);
    }

    static void Button(string label, float x, float y, float w, Action action, bool active)
    {
        Rect(x, y, w, 36, active ? 0x365C87 : 0x253345, 1);
        Label(label, x + 10, y + 8, 17, 0xF0F5FF);
        hits.Add(new MasPanelHit(x, y, w, 36, action));
    }

    public static void Draw()
    {
        hits.Clear();
        if (!Ensure())
            return;
        batch!.Begin();
        if (!Visible())
        {
            Rect(0, 678, 960, 42, 0x0B1422, 0.91f);
            Label("F2 / F5-F9: events   F3: pause   F4: menu   F10: records", 18, 689, 17, 0xDBE5F5);
        }
        else
        {
            Rect(0, 0, 960, 720, 0x050B15, 0.85f);
            Rect(105, 48, 750, 623, 0x122033, 1);
            if (rankingOpen)
                DrawRanking();
            else
                DrawMenu();
        }
        batch.Flush(Gfx.Blend.Alpha);
    }

    static void DrawRanking()
    {
        Label(rankingTitle, 135, 76, 26, 0xFFFFFF);
        for (int i = 0; i < rankingRows.Length; i++)
            Label((i + 1) + ".  " + rankingRows[i], 145, 144 + i * 45, 21, 0xDDEBFF);
        if (confirmClear)
        {
            Label("Clear every record in all six categories?", 135, 328, 21, 0xFFCA93);
            Button("Clear all records", 135, 386, 275, ClearRecords, false);
            Button("Cancel", 450, 386, 275, () => confirmClear = false, false);
            Label("Enter / A: clear    Esc / B: cancel", 135, 457, 18, 0xB8CBE4);
        }
        else if (pendingRank != 0)
        {
            string next = Alphabet.Substring(letter, 1);
            Label("New record! Enter your name (up to 40 characters)", 135, 310, 19, 0xFFDC85);
            Rect(135, 348, 685, 48, 0x071322, 1);
            Label(playerName + "_", 149, 360, 23, 0xFFFFFF);
            Label("Keyboard: type, Backspace to edit, Enter to save", 135, 418, 18, 0xC0D4EA);
            Label("Controller: arrows choose, A adds, X deletes, Start saves", 135, 451, 17, 0xC0D4EA);
            Button("<", 135, 491, 55, PreviousLetter, false);
            Button("Add " + (next == " " ? "SPACE" : next), 205, 491, 180, () => AppendText(Alphabet.Substring(letter, 1)), false);
            Button(">", 400, 491, 55, NextLetter, false);
            Button("Backspace", 470, 491, 160, RemoveLetter, false);
            Button("Save record", 135, 572, 275, CloseRanking, true);
        }
        else
        {
            Label("Category: " + Categories[category], 135, 319, 23, 0xCDE4FF);
            Button("Previous", 135, 364, 220, () => ShowRanking((category + 5) % 6), false);
            Button("Next", 380, 364, 220, () => ShowRanking((category + 1) % 6), false);
            Button("Close", 135, 464, 220, CloseRanking, true);
            Button("Clear all records", 380, 464, 270, () => confirmClear = true, false);
            Label("Arrows: category   Enter / A: close   Delete / Y: clear", 135, 550, 18, 0xB8CBE4);
        }
    }

    static void DrawMenu()
    {
        Label("Masashikun Hi!", 135, 72, 29, 0xFFFFFF);
        Label("Choose an event with arrows + Enter / A, or click", 135, 117, 18, 0xC3D7F2);
        for (int i = 0; i < Events.Length; i++)
        {
            int index = i;
            Button((i == 0 ? "F2" : "F" + (i + 4)) + "  " + Events[i], 135, 157 + i * 44, 675, () => Start(index), selected == i);
        }
        Button("F3  Pause / resume", 135, 445, 320, () =>
        {
            menuOpen = false;
            Pause();
        }, false);
        Button("F10  Records", 475, 445, 335, () => ShowRanking(category), false);
        Button("F11  Title", 135, 491, 320, () => Start(6), false);
        Button("F4  Close menu", 475, 491, 335, () => menuOpen = false, false);
        Label("Move mouse / sticks to build power; click / Space / A acts", 135, 554, 18, 0xC3D7F2);
        Label("Tap Shift / shoulder buttons to add movement. Esc pauses", 135, 585, 18, 0xC3D7F2);
        Label(Help[selected], 135, 624, 15, 0x95ADCF);
    }
}
