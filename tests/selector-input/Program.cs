using Aba;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

// Every select/return/exit combination must be released across a transition.
for (int held = 1; held < 8; held++)
{
    var gate = new SelectionGate();
    for (int frame = 0; frame < 10; frame++)
    {
        gate.Update((held & 1) != 0, (held & 2) != 0, (held & 4) != 0);
        Check(!gate.Armed, $"Held transition buttons {held} armed on frame {frame}");
    }
    gate.Update(false, false, false);
    Check(gate.Armed, $"Neutral release did not arm buttons {held}");
    gate.Update((held & 1) != 0, (held & 2) != 0, (held & 4) != 0);
    Check(gate.Armed, "A fresh input on the same screen must remain actionable");
    gate.Reset();
    Check(!gate.Armed, "A transition must reset the gate");
    gate.Update((held & 1) != 0, (held & 2) != 0, (held & 4) != 0);
    Check(!gate.Armed, $"Reset leaked held buttons {held} to the next screen");
}

// Reproduce the collection's previousExit edge detector around game -> menu.
// Holding Escape through an in-game Quit must not become an immediate host Quit.
var returning = new SelectionGate();
returning.Update(false, false, false);
returning.Reset();
bool previousExit = false;
int hostQuits = 0;
foreach (bool escape in new[] { true, true, true, false, true })
{
    returning.Update(false, false, escape);
    if (returning.Armed && escape && !previousExit) hostQuits++;
    previousExit = escape;
    Check(hostQuits == (returning.Armed && escape ? 1 : 0),
        "Held Escape escaped the game and the collection without a new press");
}
Check(hostQuits == 1, "A new Escape press after release must quit the menu once");

// Partial release is insufficient while another transition button is held.
var chord = new SelectionGate();
chord.Update(true, true, true);
chord.Update(false, true, true);
chord.Update(false, false, true);
Check(!chord.Armed, "Partially released transition chord armed prematurely");
chord.Update(false, false, false);
Check(chord.Armed, "Fully released transition chord did not arm");

Console.WriteLine("Selector input gate regression checks passed (no GPU/XR required).");
