using TinySystem.Text;

// ゲームが表示する文字列 (ASCII の英数字と記号) を 1 byte = 1 文字として読む。
// tcs の string は UTF-8 で文字の位置の API を持たないので、位置は UTF-8 の byte で数える。
public static class AsciiText
{
    // table の中で c (1 文字) が最初に現れる位置。無ければ -1。
    public static int IndexOf(string table, string c)
    {
        var key = new Utf8Bytes(c);
        if (key.Length != 1)
            return -1;
        var bytes = new Utf8Bytes(table);
        for (int i = 0; i < bytes.Length; i++)
            if (bytes[i] == key[0])
                return i;
        return -1;
    }

    // s の i byte 目の 1 文字。
    public static string At(Utf8Bytes s, int i)
    {
        return s.Slice(i, 1).ToString();
    }
}
