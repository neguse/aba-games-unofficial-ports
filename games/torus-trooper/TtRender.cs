public static class TtRender
{
    public static void Draw(Mesh mesh, float[] model, float[] tint, float width, Lub.Gfx.Blend blend, Lub.Gfx.Cull cull, DrawImage image = null)
    {
        Lub.Gfx.Draw(mesh.count, mesh.Bindings(model, tint, width, blend == Lub.Gfx.Blend.Additive, 0, image),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = cull, Blend = blend });
    }
}
