public class Pad
{
    public static int input;
    static XrInput left, right;
    static bool paused;
    public static void Read(XrInput leftHand, XrInput rightHand, bool isPaused = false)
    {
        left = leftHand;
        right = rightHand;
        paused = isPaused;
    }
    public void Update() { }
    static Vector2 Stick(XrInput hand)
    {
        if (hand?.Active != true) return new Vector2();
        float length = (float)System.Math.Sqrt(hand.StickX * hand.StickX + hand.StickY * hand.StickY);
        float scale = length > .15f ? System.Math.Min(1, (length - .15f) / .85f) / length : 0;
        return new Vector2(hand.StickX * scale, hand.StickY * scale);
    }
    public Vector2 ThumbStickLeft => Stick(left);
    public Vector2 ThumbStickRight => new Vector2();
    public bool ButtonA => right?.Active == true && right.Primary;
    public bool ButtonL => false;
    public bool ButtonR => false;
    public float LeftTrigger => left?.Active == true && left.Trigger >= .05f ? left.Trigger : 0;
    public float RightTrigger => right?.Active == true && right.Trigger >= .05f ? right.Trigger : 0;
    public bool ButtonStart => right?.Active == true && right.Menu;
    public bool ButtonBack => paused && right?.Active == true && right.Secondary;
}
