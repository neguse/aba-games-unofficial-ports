// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "wok";
    public const string MusicExtension = "wav";
    public static readonly string[] Music = new string[] { "wok1", "wok2" };
    public static readonly string[] Sounds = new string[] { "gen", "score", "miss" };
    public static readonly int[] Channels = new int[] { 0, 1, 2 };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        new KeyBinding("escape", 128)
    };

    public static int PadMask(PadInput p)
    {
        return p.Back ? 128 : 0;
    }
}
