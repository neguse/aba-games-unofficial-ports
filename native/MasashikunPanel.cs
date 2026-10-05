// Native replacement for the browser's event buttons and ranking/name dialog.
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using static Lub;

namespace Aba.Native;

public interface INativeCanvas
{
    void Text(string text, float x, float y, float size, uint rgba);
    void Rect(float x, float y, float width, float height, uint rgba);
}

/// <summary>Draw at 960 x 720 logical UI pixels. Game-state transitions are sent through the unchanged Host protocol.</summary>
public sealed class MasashikunPanel
{
    static readonly string[] Events = { "Five-event competition", "Kakenukero Dougenzaka", "Super Tobibako", "Oooka Sabaki", "Hitonage", "Gyaku Fire" };
    static readonly string[] Categories = { "Kakenukero Dougenzaka", "Super Tobibako", "Oooka Sabaki", "Hitonage", "Gyaku Fire", "Overall" };
    static readonly string[] Help = {
        "All five events. Click through each event's instructions.",
        "Move to build speed. Click to jump and run down the hill.",
        "Run, click at the arrow, build speed in the air, then click twice.",
        "Build strength, then click to pull the person toward your arrow.",
        "Move to spin. Hold the button to aim, then release to throw.",
        "Build speed. Click as the trampoline stretches to bounce higher."
    };
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_.";
    readonly Action<string, string> send;
    readonly List<Hit> hits = new();
    NativePadState previous = new();
    bool previousAction, suppressButton, confirmClear;
    int selected, category, letter, pendingRank;
    public int State { get; private set; }
    public bool Hidden { get; private set; }
    public bool MenuOpen { get; private set; } = true;
    public bool RankingOpen { get; private set; }
    public bool CaptureMouse => Hidden && State > 0 && !MenuOpen && !RankingOpen;
    public string PlayerName { get; private set; } = "";
    public string RankingTitle { get; private set; } = "";
    public string[] RankingRows { get; private set; } = Array.Empty<string>();
    public string HelpText { get; private set; } = Help[0];
    public int PendingRank => pendingRank;
    public bool Visible => MenuOpen || RankingOpen;

    public MasashikunPanel(Action<string, string> send) { this.send = send; }
    public void OnMessage(string topic, string payload)
    {
        if (topic == "state")
        {
            string[] parts = payload.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int state)) { State = state; Hidden = parts[1] == "1"; }
        }
        else if (topic == "ranking")
        {
            string[] rows = payload.Split('\n');
            RankingTitle = rows.Length > 0 ? rows[0] : "Records";
            pendingRank = rows.Length > 1 && int.TryParse(rows[1], out int rank) ? rank : 0;
            RankingRows = new string[3];
            for (int i = 0; i < 3; i++) RankingRows[i] = rows.Length > i + 2 ? rows[i + 2].Replace('\t', ' ') : "";
            RankingOpen = true; confirmClear = false; PlayerName = ""; previousAction = false; suppressButton = true;
            send("button", "0");
        }
    }
    public void Start(int n)
    {
        if (RankingOpen || n < 0 || n > 6) return;
        selected = Math.Min(n, 5); HelpText = Help[selected]; MenuOpen = false;
        previousAction = false; suppressButton = true;
        send("button", "0"); send("start", n.ToString(CultureInfo.InvariantCulture));
    }
    public void Pause()
    {
        if (RankingOpen) return;
        send("button", "0"); send("pause", ""); previousAction = false;
        if (State == -1) MenuOpen = false;
    }
    public void ShowMenu()
    {
        if (RankingOpen) return;
        if (State > 0) { send("button", "0"); send("pause", ""); }
        MenuOpen = true; previousAction = false; suppressButton = true;
    }
    public void ShowRanking(int n)
    {
        if (pendingRank != 0) return;
        category = Math.Clamp(n, 0, 5);
        if (State > 0 && !RankingOpen) send("pause", "");
        send("button", "0"); send("rank", category.ToString(CultureInfo.InvariantCulture));
        suppressButton = true;
    }
    public void SetName(string value)
    {
        value = value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
        PlayerName = value.Length > 40 ? value[..40] : value;
    }
    /// <summary>Optional Unicode text entry from a launcher platform event; keyboard and gamepad entry also work independently.</summary>
    public void AppendText(string text) { if (RankingOpen && pendingRank != 0) SetName(PlayerName + text); }
    public void CloseRanking()
    {
        if (!RankingOpen) return;
        if (pendingRank != 0) send("name", PlayerName);
        pendingRank = 0; RankingOpen = false; confirmClear = false; suppressButton = true;
    }
    void RemoveLetter() { if (PlayerName.Length != 0) PlayerName = PlayerName[..^1]; }
    static bool Rising(bool current, bool old) => current && !old;
    public void Update(float dt, bool focused, NativePadState pad)
    {
        if (!focused) { previous = new(); previousAction = false; suppressButton = true; return; }
        bool up = Rising((pad.LeftMask & 1) != 0, (previous.LeftMask & 1) != 0) || Input.KeyPressed("up");
        bool down = Rising((pad.LeftMask & 2) != 0, (previous.LeftMask & 2) != 0) || Input.KeyPressed("down");
        bool left = Rising((pad.LeftMask & 4) != 0, (previous.LeftMask & 4) != 0) || Input.KeyPressed("left");
        bool right = Rising((pad.LeftMask & 8) != 0, (previous.LeftMask & 8) != 0) || Input.KeyPressed("right");
        bool accept = Input.KeyPressed("enter") || Rising(pad.Primary, previous.Primary);
        bool back = Input.KeyPressed("escape") || Rising(pad.Back, previous.Back) || Rising(pad.Secondary, previous.Secondary);
        bool pause = Input.KeyPressed("f3") || Rising(pad.Start, previous.Start);
        if (RankingOpen)
        {
            if (confirmClear)
            {
                if (accept) { send("clear", ""); send("rank", category.ToString(CultureInfo.InvariantCulture)); confirmClear = false; }
                else if (back) confirmClear = false;
            }
            else if (pendingRank != 0)
            {
                bool shift = Input.KeyDown("left shift") || Input.KeyDown("right shift");
                for (char c = 'a'; c <= 'z'; c++) if (Input.KeyPressed(c.ToString())) AppendText(shift ? char.ToUpperInvariant(c).ToString() : c.ToString());
                for (char c = '0'; c <= '9'; c++) if (Input.KeyPressed(c.ToString())) AppendText(c.ToString());
                foreach (string key in new[] { "-", ".", ",", "/" }) if (Input.KeyPressed(key)) AppendText(key);
                if (Input.KeyPressed("space")) AppendText(" ");
                if (Input.KeyPressed("backspace") || Rising(pad.West, previous.West)) RemoveLetter();
                if (left || up) letter = (letter + Alphabet.Length - 1) % Alphabet.Length;
                if (right || down) letter = (letter + 1) % Alphabet.Length;
                if (Rising(pad.Primary, previous.Primary)) AppendText(Alphabet[letter].ToString());
                if (Input.KeyPressed("enter") || pause || back) CloseRanking();
            }
            else
            {
                if (left || up) ShowRanking((category + 5) % 6);
                if (right || down) ShowRanking((category + 1) % 6);
                if (Input.KeyPressed("delete") || Rising(pad.North, previous.North)) confirmClear = true;
                if (accept || back || pause) CloseRanking();
            }
        }
        else
        {
            if (Input.KeyPressed("f2")) Start(0);
            for (int f = 5; f <= 9; f++) if (Input.KeyPressed("f" + f)) Start(f - 4);
            if (Input.KeyPressed("f10")) ShowRanking(category);
            if (Input.KeyPressed("f11")) Start(6);
            if (Input.KeyPressed("f4")) { if (MenuOpen) MenuOpen = false; else ShowMenu(); }
            if (pause) Pause();
            if (MenuOpen)
            {
                if (up) selected = (selected + 5) % 6;
                if (down) selected = (selected + 1) % 6;
                if (accept) Start(selected);
                if (Rising(pad.West, previous.West)) ShowRanking(category);
                if (back) MenuOpen = false;
            }
            else if (back && State > 0) { send("button", "0"); send("blur", ""); previousAction = false; suppressButton = true; }
            else if (back && State <= 0) ShowMenu();

            if (!MenuOpen && !RankingOpen)
            {
                bool action = Input.MouseDown(1) || Input.KeyDown("space") || pad.Primary || pad.RightTrigger > .35f;
                bool quickPress = Input.MousePressed(1) || Input.KeyPressed("space");
                bool blocked = suppressButton;
                if (suppressButton) { if (!action && !quickPress) suppressButton = false; action = false; }
                if (!blocked && quickPress && !action && !previousAction)
                {
                    if (State == -1) Pause();
                    else { send("button", "1"); send("button", "0"); }
                }
                if (action != previousAction)
                {
                    if (action && State == -1) Pause();
                    else send("button", action ? "1" : "0");
                    previousAction = action;
                }
                if (Hidden && State > 0)
                {
                    Input.MouseDelta(out float dx, out float dy);
                    dx += (pad.LeftX + pad.RightX) * 800 * Math.Clamp(dt, 0, .25f);
                    dy -= (pad.LeftY + pad.RightY) * 800 * Math.Clamp(dt, 0, .25f);
                    if (dx != 0 || dy != 0) send("move", $"{(int)Math.Round(dx)},{(int)Math.Round(dy)}");
                    if (Input.KeyPressed("left shift") || Input.KeyPressed("right shift") || Rising(pad.LeftShoulder, previous.LeftShoulder) || Rising(pad.RightShoulder, previous.RightShoulder) || Rising(pad.West, previous.West)) send("shift", "");
                }
            }
        }
        previous = pad;
    }

    /// <summary>Call with 960 x 720 UI coordinates before game input. Returns true if the UI consumed the click.</summary>
    public bool HandlePointer(float x, float y, bool pressed)
    {
        if (!Visible || !pressed) return false;
        foreach (var hit in hits)
            if (x >= hit.X && x < hit.X + hit.W && y >= hit.Y && y < hit.Y + hit.H) { hit.Action(); return true; }
        return true;
    }
    void Button(INativeCanvas canvas, string text, float x, float y, float w, Action action, bool selected = false)
    {
        canvas.Rect(x, y, w, 36, selected ? 0x365C87FFu : 0x253345FFu);
        canvas.Text(text, x + 10, y + 8, 17, 0xF0F5FFFF);
        hits.Add(new Hit(x, y, w, 36, action));
    }
    public void Draw(INativeCanvas canvas)
    {
        hits.Clear();
        if (!Visible)
        {
            canvas.Rect(0, 678, 960, 42, 0x0B1422E8);
            canvas.Text("F2 / F5-F9: events   F3: pause   F4: menu   F10: records", 18, 689, 17, 0xDBE5F5FF);
            return;
        }
        canvas.Rect(0, 0, 960, 720, 0x050B15D8);
        canvas.Rect(105, 48, 750, 623, 0x122033FF);
        if (RankingOpen)
        {
            canvas.Text(RankingTitle, 135, 76, 26, 0xFFFFFFFF);
            for (int i = 0; i < RankingRows.Length; i++) canvas.Text($"{i + 1}.  {RankingRows[i]}", 145, 144 + i * 45, 21, 0xDDEBFFFF);
            if (confirmClear)
            {
                canvas.Text("Clear every record in all six categories?", 135, 328, 21, 0xFFCA93FF);
                Button(canvas, "Clear all records", 135, 386, 275, () => { send("clear", ""); send("rank", category.ToString(CultureInfo.InvariantCulture)); confirmClear = false; });
                Button(canvas, "Cancel", 450, 386, 275, () => confirmClear = false);
                canvas.Text("Enter / A: clear    Esc / B: cancel", 135, 457, 18, 0xB8CBE4FF);
            }
            else if (pendingRank != 0)
            {
                canvas.Text("New record! Enter your name (up to 40 characters)", 135, 310, 19, 0xFFDC85FF);
                canvas.Rect(135, 348, 685, 48, 0x071322FF);
                canvas.Text(PlayerName + "_", 149, 360, 23, 0xFFFFFFFF);
                canvas.Text("Keyboard: type, Backspace to edit, Enter to save", 135, 418, 18, 0xC0D4EAFF);
                canvas.Text("Controller: arrows choose, A adds, X deletes, Start saves", 135, 451, 17, 0xC0D4EAFF);
                Button(canvas, "<", 135, 491, 55, () => letter = (letter + Alphabet.Length - 1) % Alphabet.Length);
                Button(canvas, "Add " + (Alphabet[letter] == ' ' ? "SPACE" : Alphabet[letter].ToString()), 205, 491, 180, () => AppendText(Alphabet[letter].ToString()));
                Button(canvas, ">", 400, 491, 55, () => letter = (letter + 1) % Alphabet.Length);
                Button(canvas, "Backspace", 470, 491, 160, RemoveLetter);
                Button(canvas, "Save record", 135, 572, 275, CloseRanking, true);
            }
            else
            {
                canvas.Text("Category: " + Categories[category], 135, 319, 23, 0xCDE4FFFF);
                Button(canvas, "Previous", 135, 364, 220, () => ShowRanking((category + 5) % 6));
                Button(canvas, "Next", 380, 364, 220, () => ShowRanking((category + 1) % 6));
                Button(canvas, "Close", 135, 464, 220, CloseRanking, true);
                Button(canvas, "Clear all records", 380, 464, 270, () => confirmClear = true);
                canvas.Text("Arrows: category   Enter / A: close   Delete / Y: clear", 135, 550, 18, 0xB8CBE4FF);
            }
        }
        else
        {
            canvas.Text("Masashikun Hi!", 135, 72, 29, 0xFFFFFFFF);
            canvas.Text("Choose an event with arrows + Enter / A, or click", 135, 117, 18, 0xC3D7F2FF);
            for (int i = 0; i < Events.Length; i++)
            {
                int index = i;
                Button(canvas, (i == 0 ? "F2" : "F" + (i + 4)) + "  " + Events[i], 135, 157 + i * 44, 675, () => Start(index), selected == i);
            }
            Button(canvas, "F3  Pause / resume", 135, 445, 320, () => { MenuOpen = false; Pause(); });
            Button(canvas, "F10  Records", 475, 445, 335, () => ShowRanking(category));
            Button(canvas, "F11  Title", 135, 491, 320, () => Start(6));
            Button(canvas, "F4  Close menu", 475, 491, 335, () => MenuOpen = false);
            canvas.Text("Move mouse / sticks to build power; click / Space / A acts", 135, 554, 18, 0xC3D7F2FF);
            canvas.Text("Tap Shift / shoulder buttons to add movement. Esc pauses", 135, 585, 18, 0xC3D7F2FF);
            canvas.Text(Help[selected], 135, 624, 15, 0x95ADCFFF);
        }
    }
    sealed record Hit(float X, float Y, float W, float H, Action Action);
}
