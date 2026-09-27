public static class FrameControls
{
    static int directions;
    static bool shoot, charge;
    static bool Held(bool held, float value) => value >= (held ? .25f : .4f);
    public static void Read(GameManager manager, XrInput left, XrInput right)
    {
        int next = 0;
        if (left?.Active == true)
        {
            if (Held((directions & PadDir.LEFT) != 0, -left.StickX)) next |= PadDir.LEFT;
            if (Held((directions & PadDir.RIGHT) != 0, left.StickX)) next |= PadDir.RIGHT;
            if (Held((directions & PadDir.UP) != 0, left.StickY)) next |= PadDir.UP;
            if (Held((directions & PadDir.DOWN) != 0, -left.StickY)) next |= PadDir.DOWN;
        }
        directions = next;
        shoot = Held(shoot, right?.Active == true ? right.Trigger : 0);
        charge = Held(charge, left?.Active == true ? left.Trigger : 0);
        bool title = manager.state == manager.titleState;
        bool confirm = right?.Active == true && right.Primary;
        bool back = right?.Active == true && right.Secondary;
        manager.pad.directions = directions;
        manager.pad.buttons = title || manager.ship.isGameOver
            ? (confirm ? PadButton.A : 0) | (title && back ? PadButton.B : 0)
            : (shoot ? PadButton.A : 0) | (charge ? PadButton.B : 0);
        manager.pad.pause = right?.Active == true && right.Menu;
        manager.pad.escape = !title && manager.inGameState.pauseCnt > 0 && back;
    }
}
