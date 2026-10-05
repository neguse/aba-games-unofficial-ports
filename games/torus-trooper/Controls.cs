public static class FrameControls
{
    static int directions;
    static bool shoot, charge, viewPressed, recenterPressed;
    static bool Held(bool held, float value) => value >= (held ? .25f : .4f);
    public static void Read(GameManager manager, XrInput left, XrInput right)
    {
        bool view = right?.Active == true && right.StickClick;
        if (view && !viewPressed) TtRender.FirstPerson = !TtRender.FirstPerson;
        viewPressed = view;
        bool recenter = left?.Active == true && left.Secondary;
        if (recenter && !recenterPressed) TtRender.Recenter();
        recenterPressed = recenter;
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
        manager.pad.pause = right?.Active == true && right.Menu || left?.Active == true && left.Primary;
        manager.pad.escape = !title && manager.inGameState.pauseCnt > 0 && back;
    }
    public static void ReadDesktop(GameManager manager)
    {
        int directions = 0, buttons = 0;
        if (Lub.Input.KeyDown("up") || Lub.Input.KeyDown("Keypad 8") || Lub.Input.KeyDown("Keypad 7") || Lub.Input.KeyDown("Keypad 9") || Lub.Input.KeyDown("w")) directions |= PadDir.UP;
        if (Lub.Input.KeyDown("down") || Lub.Input.KeyDown("Keypad 2") || Lub.Input.KeyDown("Keypad 1") || Lub.Input.KeyDown("Keypad 3") || Lub.Input.KeyDown("s")) directions |= PadDir.DOWN;
        if (Lub.Input.KeyDown("left") || Lub.Input.KeyDown("Keypad 4") || Lub.Input.KeyDown("Keypad 7") || Lub.Input.KeyDown("Keypad 1") || Lub.Input.KeyDown("a")) directions |= PadDir.LEFT;
        if (Lub.Input.KeyDown("right") || Lub.Input.KeyDown("Keypad 6") || Lub.Input.KeyDown("Keypad 9") || Lub.Input.KeyDown("Keypad 3") || Lub.Input.KeyDown("d")) directions |= PadDir.RIGHT;
        if (Lub.Input.KeyDown("z") || Lub.Input.KeyDown("left ctrl") || Lub.Input.KeyDown("right ctrl") || Lub.Input.KeyDown(".")) buttons |= PadButton.A;
        if (Lub.Input.KeyDown("x") || Lub.Input.KeyDown("left alt") || Lub.Input.KeyDown("right alt") || Lub.Input.KeyDown("left shift") || Lub.Input.KeyDown("right shift") || Lub.Input.KeyDown("/")) buttons |= PadButton.B;
        bool pause = Lub.Input.KeyDown("p"), escape = Lub.Input.KeyDown("escape");
        for (int player = 0; player < 4; player++)
        {
            if (!Lub.Input.GamepadConnected(player)) continue;
            float x = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftX), y = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftY);
            if (y < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadUp)) directions |= PadDir.UP;
            if (y > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadDown)) directions |= PadDir.DOWN;
            if (x < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadLeft)) directions |= PadDir.LEFT;
            if (x > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadRight)) directions |= PadDir.RIGHT;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.South) || Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightTrigger) > .35f) buttons |= PadButton.A;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.East) || Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftTrigger) > .35f) buttons |= PadButton.B;
            pause = pause || Lub.Input.GamepadDown(player, Lub.Input.PadButton.Start);
            escape = escape || Lub.Input.GamepadDown(player, Lub.Input.PadButton.Back);
        }
        manager.pad.directions = directions; manager.pad.buttons = buttons;
        manager.pad.pause = pause; manager.pad.escape = escape;
    }
}
