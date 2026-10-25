using Lub;

// ゲームの記録 (scores、replay) を save/<GameProfile.Id>/<名前>.txt に読み書きする。
// web の保存の場所は起動後に読み込みが終わるまで使えないので、Ready になるまで保存はしない。
public static class GameSave
{
    static bool ready;

    static string Path(string name)
    {
        return "save/" + GameProfile.Id + "/" + name + ".txt";
    }

    // 読み込みが終わったか (記録が無い場合も終わった扱い)。
    public static bool Ready(string name)
    {
        Lub.Io.LoadText(Path(name), out _, out _, out var status, out _);
        if (status != Lub.Io.Status.Pending)
            ready = true;
        return status != Lub.Io.Status.Pending;
    }

    // 記録の中身。無ければ (読み込み中も) null。
    public static string? Load(string name)
    {
        Lub.Io.LoadText(Path(name), out var text, out _, out _, out _);
        return text;
    }

    public static void Save(string name, string text)
    {
        if (ready)
            Lub.Io.SaveText(Path(name), text);
    }
}
