// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "mu-cade";
    public const string MusicExtension = "ogg";
    public static readonly string[] Music = new string[] { "mcd1", "mcd2", "mcd3", "mcd4" };
    public static readonly string[] Sounds = new string[] { "shot", "hit", "bullethit", "destroyed", "addtail", "breaktail", "enhancedshot", "shipdestroyed", "extend" };
    public static readonly int[] Channels = new int[] { 0, 1, 2, 3, 4, 5, 0, 6, 7 };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        new KeyBinding("up", 1),
        new KeyBinding("down", 2),
        new KeyBinding("left", 4),
        new KeyBinding("right", 8),
        new KeyBinding("w", 1),
        new KeyBinding("s", 2),
        new KeyBinding("a", 4),
        new KeyBinding("d", 8),
        new KeyBinding("Keypad 8", 1),
        new KeyBinding("Keypad 2", 2),
        new KeyBinding("Keypad 4", 4),
        new KeyBinding("Keypad 6", 8),
        new KeyBinding("Keypad 7", 5),
        new KeyBinding("Keypad 9", 9),
        new KeyBinding("Keypad 1", 6),
        new KeyBinding("Keypad 3", 10),
        new KeyBinding("i", 16),
        new KeyBinding("k", 32),
        new KeyBinding("j", 64),
        new KeyBinding("l", 128),
        new KeyBinding("z", 256),
        new KeyBinding("left ctrl", 256),
        new KeyBinding("right ctrl", 256),
        new KeyBinding(".", 256),
        new KeyBinding("x", 512),
        new KeyBinding("left alt", 512),
        new KeyBinding("right alt", 512),
        new KeyBinding("left shift", 512),
        new KeyBinding("right shift", 512),
        new KeyBinding("/", 512),
        new KeyBinding("enter", 512),
        new KeyBinding("space", 512),
        new KeyBinding("p", 1024),
        new KeyBinding("escape", 2048)
    };

    public static int PadMask(PadInput p)
    {
        return p.LeftMask | p.RightMask << 4 | (p.A ? 256 : 0) | (p.B ? 512 : 0) | (p.Start ? 1024 : 0) | (p.Back ? 2048 : 0);
    }
}
