using Lub;

// ゲームと、ゲームを載せるアプリ (1 本だけの Standalone か collection) の境目。
// アプリは OnFrame の前に Target を決め、ゲームはそこに描く。
// ゲームは終わりたいときに End を呼ぶ。終わり方 (アプリの終了か一覧に戻るか) はアプリが決める。
public static class Scene
{
    public static TextureRef Target;
    public static bool Ended;

    public static void End()
    {
        Ended = true;
    }
}
