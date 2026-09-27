using System.Collections.Generic;

public static class TtRender
{
    public static void Draw(int count, Dictionary<string, object> bindings, DrawOpts options)
    {
        Lub.Gfx.Draw(count, bindings, options);
    }
}
