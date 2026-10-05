public class Pad
{
    static readonly string[] keys = new string[] { "up", "w", "Keypad 8", "down", "s", "Keypad 2", "left", "a", "Keypad 4", "right", "d", "Keypad 6",
        "x", ".", "z", ",", "c", "/", "f1", "escape" };
    static readonly int[] bits = new int[] { 1, 1, 1, 2, 2, 2, 4, 4, 4, 8, 8, 8, 16, 16, 32, 32, 64, 64, 128, 256 };
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
            float x = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftX), y = Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftY);
            if (y < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadUp)) input |= 1;
            if (y > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadDown)) input |= 2;
            if (x < -.35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadLeft)) input |= 4;
            if (x > .35f || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadRight)) input |= 8;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.South)) input |= 16;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.LeftShoulder) || Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftTrigger) > .35f) input |= 32;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.RightShoulder) || Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightTrigger) > .35f) input |= 64;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.Start)) input |= 128;
            if (Lub.Input.GamepadDown(player, Lub.Input.PadButton.Back)) input |= 256;
        }
    }
    public void Update() { }
    static Vector2 Stick(XrInput hand)
    {
        if (hand?.Active != true) return new Vector2();
        float length = (float)System.Math.Sqrt(hand.StickX * hand.StickX + hand.StickY * hand.StickY);
        float scale = length > .15f ? System.Math.Min(1, (length - .15f) / .85f) / length : 0;
        return new Vector2(hand.StickX * scale, hand.StickY * scale);
    }
    static int Bit(int bit) => !immersive && (input & bit) != 0 ? 1 : 0;
    public Vector2 ThumbStickLeft => immersive ? Stick(left) : new Vector2(Bit(8) - Bit(4), Bit(1) - Bit(2));
    public Vector2 ThumbStickRight => new Vector2();
    public bool ButtonA => immersive ? right?.Active == true && right.Primary : Bit(16) != 0;
    public bool ButtonL => Bit(32) != 0;
    public bool ButtonR => Bit(64) != 0;
    public float LeftTrigger => immersive && left?.Active == true && left.Trigger >= .05f ? left.Trigger : 0;
    public float RightTrigger => immersive && right?.Active == true && right.Trigger >= .05f ? right.Trigger : 0;
    public bool ButtonStart => immersive ? right?.Active == true && right.Menu || left?.Active == true && left.Primary : Bit(128) != 0;
    public bool ButtonBack => immersive ? paused && right?.Active == true && right.Secondary : Bit(256) != 0;
}
