// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "parsec47";
    public const string MusicExtension = "ogg";
    public static readonly string[] Music = new string[] { "ptn0", "ptn1", "ptn2", "ptn3" };
    public static readonly string[] Sounds = new string[] { "shot", "rollchg", "rollrls", "shipdst", "getbonus", "extend", "enemydst", "largedst", "bossdst", "lock", "laser" };
    public static readonly int[] Channels = new int[] { 0, 1, 2, 1, 3, 4, 5, 6, 7, 1, 2 };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        new KeyBinding("up", 1),
        new KeyBinding("Keypad 8", 1),
        new KeyBinding("w", 1),
        new KeyBinding("down", 2),
        new KeyBinding("Keypad 2", 2),
        new KeyBinding("s", 2),
        new KeyBinding("left", 4),
        new KeyBinding("Keypad 4", 4),
        new KeyBinding("a", 4),
        new KeyBinding("right", 8),
        new KeyBinding("Keypad 6", 8),
        new KeyBinding("d", 8),
        new KeyBinding("z", 16),
        new KeyBinding("left ctrl", 16),
        new KeyBinding("x", 32),
        new KeyBinding("left alt", 32),
        new KeyBinding("left shift", 32),
        new KeyBinding("p", 64),
        new KeyBinding("escape", 128)
    };

    public static int PadMask(PadInput p)
    {
        return p.LeftMask | (p.A ? 16 : 0) | (p.B ? 32 : 0) | (p.Start ? 64 : 0) | (p.Back ? 128 : 0);
    }
}
