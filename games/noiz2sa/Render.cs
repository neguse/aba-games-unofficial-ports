using System;
using System.Collections.Generic;
using static Lub;
public static class NrRender {
    static PixelLayer decay,smoke,blend,present;
    static bool initialized;
    static int version,ping;
    static ShaderRef shader;
    static TextureRef tables,sprites;
    static int drawIndex;
    public static void Init(){
        
        decay=new PixelLayer(320);decay.Rect(0,0,320,480,0,0,320,480,1);
        smoke=new PixelLayer(320);smoke.Rect(0,0,320,480,0,0,320,480,2);
        blend=new PixelLayer(320);blend.Rect(0,0,320,480,0,0,320,480,3);
        present=new PixelLayer(640);present.Rect(0,0,640,480,0,0,640,480,4);
    }
    static void Draw(PixelLayer layer,TextureRef first,TextureRef second,TextureRef third){
        if(layer.rectangles.Count==0)return;
        var buffer=Gfx.UseBuffer("rects"+drawIndex.ToString(),Gfx.BufferType.Storage,layer.rectangles,version);drawIndex++;
        if(buffer==null)return;
        Gfx.Draw(layer.rectangles.Count/12*6,new Dictionary<string,object>{["rectangles"]=buffer,["first"]=first,["second"]=second,["third"]=third,["tables"]=tables,["sprites"]=sprites},new DrawOpts{Shader=shader,Blend=Gfx.Blend.None,Depth=false,DepthWrite=false,Cull=Gfx.Cull.None});
    }
    static void Paint(PixelLayer layer){Draw(layer,tables,tables,tables);}
    // Draws into the frame's layers just before they are composed; the browser test paints its palette swatches here.
    public static Action overlay;
    public static void Frame(){
        if(overlay!=null)overlay();
        shader=Gfx.UseShader("noiz2sa",GameShaders.vertex,GameShaders.fragment,1);
        Png.Load("images/tables.png",out var tablePixels,out _,out _,out _,out _,out int tableVersion,out _,out _);
        Png.Load("images/sprites.png",out var spritePixels,out _,out _,out _,out _,out int spriteVersion,out _,out _);
        tables=tablePixels==null?null:Gfx.UseTextureBytes("tables",256,260,Gfx.PixelFormat.Rgba8,tablePixels,tableVersion);
        sprites=spritePixels==null?null:Gfx.UseTextureBytes("sprites",40,280,Gfx.PixelFormat.Rgba8,spritePixels,spriteVersion);
        if(shader==null||tables==null||sprites==null)return;
        var targets=new List<TextureRef>();
        for(int i=0;i<7;i++){
            var target=Gfx.UseTexture("layer"+i.ToString(),i>=5?160:320,480,Gfx.PixelFormat.Rgba8,null,1,new TextureOpts{Target=true,Filter=Gfx.Filter.Nearest});
            if(target==null)return;targets.Add(target);
        }
        if(!initialized){foreach(var target in targets){Gfx.BeginPass(new PassOpts{Target=target});Gfx.EndPass();}initialized=true;}
        version++;drawIndex=0;ping=1-ping;
        Gfx.BeginPass(new PassOpts{Target=targets[ping]});Draw(decay,targets[1-ping],tables,tables);Paint(NrScreen.l1buf);Gfx.EndPass();
        Gfx.BeginPass(new PassOpts{Target=targets[2+ping]});Draw(smoke,targets[3-ping],tables,tables);Paint(NrScreen.l2buf);Gfx.EndPass();
        Gfx.BeginPass(new PassOpts{Target=targets[4]});Draw(blend,targets[ping],targets[2+ping],tables);Paint(NrScreen.buf);Gfx.EndPass();
        Gfx.BeginPass(new PassOpts{Target=targets[5]});Paint(NrScreen.lpbuf);Gfx.EndPass();
        Gfx.BeginPass(new PassOpts{Target=targets[6]});Paint(NrScreen.rpbuf);Gfx.EndPass();
        Gfx.BeginPass(new PassOpts{Target=Gfx.MainTex});Draw(present,targets[4],targets[5],targets[6]);Paint(NrScreen.video);Gfx.EndPass();
    }
}
