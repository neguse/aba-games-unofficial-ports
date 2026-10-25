// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "tumiki";
    public const string MusicExtension = "ogg";
    public static readonly string[] Music = new string[] { "we_are_tumiki_fighters", "just_over_the_horizon", "panic_on_meadow", "here_comes_a_gigantic_toy", "battle_over_the_junk_city", "return_to_home" };
    public static readonly string[] Sounds = new string[] { "ship_shot", "stuck", "stuck_bonus", "stuck_destroyed", "ship_destroyed", "enemy_damaged", "small_enemy_destroyed", "enemy_destroyed", "boss_destroyed", "extend", "warning", "propeller", "stuck_bonus_pushin" };
    public static readonly int[] Channels = new int[] { 0, 1, 2, 3, 2, 4, 5, 6, 6, 7, 7, 7, 2 };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        new KeyBinding("up", 1),
        new KeyBinding("w", 1),
        new KeyBinding("down", 2),
        new KeyBinding("s", 2),
        new KeyBinding("left", 4),
        new KeyBinding("a", 4),
        new KeyBinding("right", 8),
        new KeyBinding("d", 8),
        new KeyBinding("z", 16),
        new KeyBinding("left ctrl", 16),
        new KeyBinding(".", 16),
        new KeyBinding("x", 32),
        new KeyBinding("left alt", 32),
        new KeyBinding("left shift", 32),
        new KeyBinding("right shift", 32),
        new KeyBinding("/", 32),
        new KeyBinding("p", 64),
        new KeyBinding("escape", 128)
    };

    public static int PadMask(PadInput p)
    {
        return p.LeftMask | (p.A ? 16 : 0) | (p.B ? 32 : 0) | (p.Start ? 64 : 0) | (p.Back ? 128 : 0);
    }
}
