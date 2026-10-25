// このゲームのキーの割り当て、pad の割り当て、音の名前、保存の場所の名前。
public static class GameProfile
{
    public const string Id = "masashikun-hi";
    public const string MusicExtension = "ogg";
    public static readonly string[] Music = new string[] {  };
    public static readonly string[] Sounds = new string[] {  };
    public static readonly int[] Channels = new int[] {  };
    public static readonly KeyBinding[] Controls = new KeyBinding[]
    {
        
    };

    public static int PadMask(PadInput p)
    {
        return p.Back ? 128 : 0;
    }
}
