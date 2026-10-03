using System;
using System.Collections.Generic;
using static Lub;
using static RrConstants;
public static class Game {
    static float elapsed;
    public static ShaderRef shader;
    public static TextureRef atlas;
    static string shaderSource;
    static bool pausePressed,escapePressed;
    public static void OnInit() {
        Config(new ConfigOpts{Width=640,Height=480});
        RrAngles.tantbl=RrData.tangent;RrAngles.sctbl=RrData.sine;
        RrAttract.initHiScore();RrBarrage.initBarragemanager();RrAttract.initAttractManager();RrAttract.initGameStateFirst();RrCore.initTitle();
        if(Host.Available()){Host.Send("scores.load","");Host.Send("ready","");}
    }
    public static void OnFrame(float dt) {
        while(Host.Available()) {
            Host.Poll(out string topic,out string payload);if(topic==null)break;
            if(topic=="input"){int value=GameMath.parseNonnegative(payload);if(value>=0&&value<=255)RrInput.input=value;}
            if(topic=="seed"){int seed=GameMath.parseNonnegative(payload);if(seed>=0)RrRandom.visualSeed=seed;}
            if(topic=="scores"){RrPreference.loadPreference(payload);if(RrCore.status==TITLE)RrCore.initTitle();}
        }
        elapsed+=Math.Min(dt,0.1f);
        while(elapsed>=RrCore.interval/1000f) {
            elapsed-=RrCore.interval/1000f;
            bool pause=(RrInput.input&64)!=0,escape=(RrInput.input&128)!=0;
            if(pause&&!pausePressed){if(RrCore.status==IN_GAME)RrCore.status=PAUSE;else if(RrCore.status==PAUSE)RrCore.status=IN_GAME;}
            if(escape&&!escapePressed){RrPreference.savePreference();RrCore.initTitle();}
            pausePressed=pause;escapePressed=escape;
            RrCore.move();RrCore.tick++;
        }
        if(RrInput.quitRequested){RrInput.quitRequested=false;if(Host.Available())Host.Send("quit","");}
        string source = GameShaders.vertex + GameShaders.fragment;
        shader = Gfx.UseShader("rrootage", GameShaders.vertex, GameShaders.fragment,
            shader != null && shaderSource == source ? (int?)shader.Version : null);
        shaderSource = source;
        Png.Load("images/atlas.png", out var pixels, out _, out _, out _, out _, out int pixelsVersion, out _, out _);
        if (pixels != null)
            atlas = Gfx.UseTextureBytes("atlas", RrData.atlasWidth, RrData.atlasHeight, Gfx.PixelFormat.Rgba8, pixels, pixelsVersion);
        if (shader == null || atlas == null) return;
        Gfx.BeginPass(new PassOpts { Target = Gfx.MainTex, ClearColor = RrScreen.clearColor });
        RrCore.draw(RrScreen.setEyepos(), Gfx.Blend.Additive, "scene");
        Gfx.EndPass();
    }
    public static void OnQuit(){RrPreference.savePreference();}
}
