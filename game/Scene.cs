using Lub;

// ゲームと、ゲームを載せるアプリ (1 本だけの Standalone か collection) の境目。
// アプリは OnFrame の前に Target を決め、ゲームはそこに描く。
// アプリは Target を映している window の矩形 (Input.MousePos と同じ px) も View* に入れる。
// ゲームは終わりたいときに End を呼ぶ。終わり方 (アプリの終了か一覧に戻るか) はアプリが決める。
public static class Scene
{
    public static TextureRef Target = null!;
    public static float ViewX, ViewY, ViewWidth = 640, ViewHeight = 480;
    public static bool Ended;

    public static void End()
    {
        Ended = true;
    }
}
