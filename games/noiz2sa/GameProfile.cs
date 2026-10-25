// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "noiz2sa";
    public const string MusicExtension = "ogg";
    public static readonly string[] Music = new string[] { "stg0", "stg1", "stg2", "stg3", "stg4", "stg5", "stg00" };
    public static readonly string[] Sounds = new string[] { "shot", "hit", "foedst", "bossdst", "shipdst", "bonus", "extend" };
    public static readonly int[] Channels = new int[] { 0, 1, 2, 3, 4, 5, 6 };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        new KeyBinding("up", 1),
        new KeyBinding("Keypad 8", 1),
        new KeyBinding("down", 2),
        new KeyBinding("Keypad 2", 2),
        new KeyBinding("left", 4),
        new KeyBinding("Keypad 4", 4),
        new KeyBinding("right", 8),
        new KeyBinding("Keypad 6", 8),
        new KeyBinding("z", 16),
        new KeyBinding(".", 16),
        new KeyBinding("left ctrl", 16),
        new KeyBinding("right ctrl", 16),
        new KeyBinding("x", 32),
        new KeyBinding("/", 32),
        new KeyBinding("left alt", 32),
        new KeyBinding("right alt", 32),
        new KeyBinding("left shift", 32),
        new KeyBinding("right shift", 32),
        new KeyBinding("p", 64),
        new KeyBinding("escape", 128),
        new KeyBinding("w", 1),
        new KeyBinding("s", 2),
        new KeyBinding("a", 4),
        new KeyBinding("d", 8),
        new KeyBinding("Keypad 7", 5),
        new KeyBinding("Keypad 9", 9),
        new KeyBinding("Keypad 1", 6),
        new KeyBinding("Keypad 3", 10)
    };

    public static int PadMask(PadInput p)
    {
        return p.LeftMask | (p.A ? 16 : 0) | (p.B ? 32 : 0) | (p.Start ? 64 : 0) | (p.Back ? 128 : 0);
    }
}
