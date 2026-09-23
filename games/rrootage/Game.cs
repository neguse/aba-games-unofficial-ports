using System;
using System.Collections.Generic;
using static Lub;
using static RrConstants;
public static class Game {
    static float elapsed;
    static int version;
    static bool pausePressed,escapePressed;
    static List<int> pixels;
    public static void OnInit() {
        Config(new ConfigOpts{Width=640,Height=480});
        Drawing.BeginFrame();Drawing.recordBlend=true;Drawing.premultiplyAdditive=false;
        Drawing.glDisable(Drawing.GL_DEPTH_TEST);Drawing.glDisable(Drawing.GL_CULL_FACE);Drawing.glEnable(Drawing.GL_BLEND);
        RrAngles.tantbl=RrData.tangent;RrAngles.sctbl=RrData.sine;
        RrAttract.initHiScore();RrBarrage.initBarragemanager();RrAttract.initAttractManager();RrAttract.initGameStateFirst();RrCore.initTitle();
        pixels=RrData.pixels();
        if(Host.Available()){Host.Send("scores.load","");Host.Send("ready","");}
    }
    public static void OnFrame(float dt) {
        while(Host.Available()) {
            Host.Poll(out string topic,out string payload);if(topic==null)break;
            if(topic=="input"){int value=GameMath.parseNonnegative(payload);if(value>=0&&value<=255)RrGl.input=value;}
            if(topic=="seed"){int seed=GameMath.parseNonnegative(payload);if(seed>=0)RrRandom.visualSeed=seed;}
            if(topic=="scores"){RrPreference.loadPreference(payload);if(RrCore.status==TITLE)RrCore.initTitle();}
        }
        elapsed+=Math.Min(dt,0.1f);
        while(elapsed>=RrCore.interval/1000f) {
            elapsed-=RrCore.interval/1000f;
            bool pause=(RrGl.input&64)!=0,escape=(RrGl.input&128)!=0;
            if(pause&&!pausePressed){if(RrCore.status==IN_GAME)RrCore.status=PAUSE;else if(RrCore.status==PAUSE)RrCore.status=IN_GAME;}
            if(escape&&!escapePressed){RrPreference.savePreference();RrCore.initTitle();}
            pausePressed=pause;escapePressed=escape;
            RrCore.move();RrCore.tick++;
        }
        if(RrGl.quitRequested){RrGl.quitRequested=false;if(Host.Available())Host.Send("quit","");}
        RrScreen.drawGLSceneStart();RrCore.draw();RrScreen.drawGLSceneEnd();
        var shader=Gfx.UseShader("rrootage",GameShaders.vertex,GameShaders.fragment,1);
        var atlas=Gfx.UseTexture("atlas",RrData.atlasWidth,RrData.atlasHeight,Gfx.PixelFormat.Rgba8,pixels,1);
        if(shader==null||atlas==null)return;
        version++;int index=0;
        Gfx.BeginPass(new PassOpts{Target=Gfx.MainTex,ClearColor=Drawing.clearColor});
        foreach(var batch in Drawing.batches) {
            if(batch.vertices.Count==0)continue;
            var buffer=Gfx.UseBuffer("geometry"+index.ToString(),Gfx.BufferType.Storage,batch.vertices,version);
            if(buffer!=null)Gfx.Draw(GameMath.integer(batch.vertices.Count/8),new Dictionary<string,object>{["verts"]=buffer,["atlas"]=atlas},new DrawOpts{Shader=shader,Depth=false,DepthWrite=false,Cull=Gfx.Cull.None,Blend=batch.blend?Gfx.Blend.Additive:Gfx.Blend.None});
            index++;
        }
        Gfx.EndPass();
    }
    public static void OnQuit(){RrPreference.savePreference();}
}
