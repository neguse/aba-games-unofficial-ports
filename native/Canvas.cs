using static Lub;
namespace Aba;

/// <summary>Shared pixel canvas, displayed unchanged on a desktop or an anchored XR panel.</summary>
public sealed class Canvas : Aba.Native.INativeCanvas
{
    public const int Width = 960, Height = 720;
    readonly string shaderText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "collection.slang"));
    readonly MeshText font = new("collection-font", Path.Combine(AppContext.BaseDirectory, "font.ttf"), 1, Width, Height);
    readonly XrAnchor anchor = new();
    ShaderRef? shader;
    TextureRef? white;
    public TextureRef Target(string name, int width, int height) => Gfx.UseTexture(name, width, height,
        Gfx.PixelFormat.Rgba8, null, 1, new TextureOpts { Target = true, Filter = Gfx.Filter.Linear })
        ?? throw new InvalidOperationException("Unable to allocate collection screen");
    public void Begin(TextureRef target, bool clear = true)
    {
        shader = Gfx.UseShader("collection-panel", shaderText, shaderText, 1);
        white = Gfx.UseTexture("collection-white", 1, 1, Gfx.PixelFormat.Rgba8, new List<int> {255,255,255,255}, 1);
        Gfx.BeginPass(new PassOpts { Target = target, ClearColor = [.025f,.04f,.075f,1], Load = clear ? Gfx.LoadAction.Clear : Gfx.LoadAction.Load });
    }
    public void End() => Gfx.EndPass();
    public void Text(string value, float x, float y, float size = 22, uint rgba = 0xe5eefbff)
        => font.Text(value, x, y + size, size, Color.Rgb((rgba>>24)/255f, ((rgba>>16)&255)/255f, ((rgba>>8)&255)/255f, (rgba&255)/255f));
    public void Rect(float x, float y, float width, float height, uint rgba)
    {
        if (white == null) return;
        float[] m = [2f/Width,0,0,(x+width/2)/Width*2-1, 0,2f/Height,0,1-(y+height/2)/Height*2, 0,0,1,0, 0,0,0,1];
        Quad(white,m,width,height,0,rgba,false);
    }
    public void Image(TextureRef image, float x, float y, float width, float height)
    {
        float[] m = [2f/Width,0,0,(x+width/2)/Width*2-1, 0,2f/Height,0,1-(y+height/2)/Height*2, 0,0,1,0, 0,0,0,1];
        Quad(image,m,width,height,0,0xffffffff,false);
    }
    void Quad(TextureRef texture, float[] matrix, float width, float height, float z, uint rgba, bool linear)
    {
        shader = Gfx.UseShader("collection-panel", shaderText, shaderText, 1);
        if (shader == null) return;
        Gfx.Draw(6,new Dictionary<string,object> {
            ["screen"] = texture,
            ["uniforms"] = new Dictionary<string,object> {
                ["r0"] = matrix[0..4].ToList(), ["r1"] = matrix[4..8].ToList(),
                ["r2"] = matrix[8..12].ToList(), ["r3"] = matrix[12..16].ToList(),
                ["tint"] = new List<float>{(rgba>>24)/255f,((rgba>>16)&255)/255f,((rgba>>8)&255)/255f,(rgba&255)/255f},
                ["options"] = new List<float>{width,height,z,linear?1:0}
            }
        },new DrawOpts{Shader=shader,Depth=false,DepthWrite=false,Cull=Gfx.Cull.None,Blend=Gfx.Blend.Alpha});
    }
    public void Recenter() => anchor.Reset();
    public void Present(TextureRef texture, int width, int height, bool immersive)
    {
        if (immersive)
        {
            var left=Xr.View(0,.05f,100); var right=Xr.View(1,.05f,100);
            if(left==null||right==null) return;
            if(!anchor.IsSet()) anchor.Recenter(left,right);
            foreach(var eye in new[]{left,right})
            {
                float[] matrix=new float[16]; anchor.ViewProjection(eye,matrix);
                Gfx.BeginPass(new PassOpts{Target=eye.Target,ClearColor=[.012f,.02f,.04f,1]});
                Quad(texture,matrix,2.4f,2.4f*height/width,-2.2f,0xffffffff,true);
                Gfx.EndPass();
            }
        }
        else
        {
            Gfx.Size(out int w,out int h);
            float scale=Math.Min(w/(float)width,h/(float)height);
            float[] matrix=[1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1];
            Gfx.BeginPass(new PassOpts{Target=Gfx.MainTex,ClearColor=[.012f,.02f,.04f,1]});
            Quad(texture,matrix,width*scale/w*2,height*scale/h*2,0,0xffffffff,false);
            Gfx.EndPass();
        }
    }
}
