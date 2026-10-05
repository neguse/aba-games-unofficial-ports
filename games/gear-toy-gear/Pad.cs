public class Pad
{
    static readonly string[] keys = new string[] { "w", "s", "a", "d", "up", "Keypad 8", "down", "Keypad 2", "left", "Keypad 4", "right", "Keypad 6",
        "z", "m", "x", ",", "c", ".", "v", "/", "q", "e", "f1", "p", "escape" };
    static readonly int[] bits = new int[] { 1, 2, 4, 8, 16, 16, 32, 32, 64, 64, 128, 128,
        256, 256, 512, 512, 1024, 1024, 2048, 2048, 4096, 8192, 16384, 16384, 32768 };
    // Keyboard state as the original gamepad's bits; the XR controllers replace it while a session runs.
    public static int input;
    static XrInput left, right;
    static bool immersive, paused;
    public static void Read(XrInput leftHand, XrInput rightHand, bool isPaused = false)
    {
        immersive = true;
        left = leftHand;
        right = rightHand;
        paused = isPaused;
    }
    public static void ReadDesktop()
    {
        immersive = false;
        input = 0;
        for (int i = 0; i < keys.Length; i++)
            if (Lub.Input.KeyDown(keys[i])) input |= bits[i];
        for (int player = 0; player < 4; player++)
        {
            if (!Lub.Input.GamepadConnected(player)) continue;
            float lx = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftX), ly = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftY);
            float rx = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightX), ry = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightY);
            if (ly < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadUp)) input |= 1;
            if (ly > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadDown)) input |= 2;
            if (lx < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadLeft)) input |= 4;
            if (lx > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadRight)) input |= 8;
            if (ry < -.35f) input |= 16;
            if (ry > .35f) input |= 32;
            if (rx < -.35f) input |= 64;
            if (rx > .35f) input |= 128;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.South)) input |= 256;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.East)) input |= 512;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.LeftShoulder)) input |= 1024;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.RightShoulder)) input |= 2048;
            if (Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftTrigger) > .35f) input |= 4096;
            if (Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightTrigger) > .35f) input |= 8192;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.Start)) input |= 16384;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.Back)) input |= 32768;
        }
    }
    public void Update() { }
    public void SetGamePadPlayerIndex(int index) { }
    public int CheckGameStartPressed() => ButtonAny || !immersive && ButtonStart ? 4 : -1;
    static Vector2 Stick(XrInput hand)
    {
        if (hand?.Active != true) return new Vector2();
        float length = (float)System.Math.Sqrt(hand.StickX * hand.StickX + hand.StickY * hand.StickY);
        float scale = length > .15f ? System.Math.Min(1, (length - .15f) / .85f) / length : 0;
        return new Vector2(hand.StickX * scale, hand.StickY * scale);
    }
    static int Bit(int bit) => !immersive && (input & bit) != 0 ? 1 : 0;
    public Vector2 ThumbStickLeft => immersive ? Stick(left) : new Vector2(Bit(8) - Bit(4), Bit(1) - Bit(2));
    public Vector2 ThumbStickRight => new Vector2(Bit(128) - Bit(64), Bit(16) - Bit(32));
    public bool ButtonA => Bit(256) != 0;
    public bool ButtonB => Bit(512) != 0;
    public bool ButtonL => Bit(1024) != 0;
    public bool ButtonR => Bit(2048) != 0;
    public bool ButtonAny => immersive ? right?.Active == true && right.Primary : (input & 3840) != 0;
    public float LeftTrigger => immersive ? (left?.Active == true && left.Trigger >= .05f ? left.Trigger : 0) : Bit(4096);
    public float RightTrigger => immersive ? (right?.Active == true && right.Trigger >= .05f ? right.Trigger : 0) : Bit(8192);
    public bool ButtonStart => immersive ? right?.Active == true && right.Menu || left?.Active == true && left.Primary : Bit(16384) != 0;
    public bool ButtonBack => immersive ? paused && right?.Active == true && right.Secondary : Bit(32768) != 0;
}
