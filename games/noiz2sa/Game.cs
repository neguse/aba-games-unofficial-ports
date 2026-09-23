using System;
using static Lub;
using static NrConstants;
public static class Game {
    static float elapsed;
    static bool pausePressed,escapePressed;
    public static void OnInit(){
        Config(new ConfigOpts{Width=640,Height=480});
        NrAngles.tantbl=NrData.tangent;NrAngles.sctbl=NrData.sine;
        NrAttract.initHiScore();NrBarrage.initBarragemanager();NrAttract.initAttractManager();NrCore.initTitle();NrRender.Init();
        if(Host.Available()){Host.Send("scores.load","");Host.Send("ready","");}
    }
    public static void OnFrame(float dt){
        while(Host.Available()){
            Host.Poll(out string topic,out string payload);if(topic==null)break;
            if(topic=="input"){int input=GameMath.parseNonnegative(payload);if(input>=0&&input<=255)NrScreen.input=input;}
            if(topic=="seed"){int seed=GameMath.parseNonnegative(payload);if(seed>=0)NrRandom.visualSeed=seed;}
            if(topic=="scores"){NrPreference.loadPreference(payload);if(NrCore.status==TITLE)NrCore.initTitle();}
        }
        NrScreen.l1buf.Clear();NrScreen.l2buf.Clear();NrScreen.buf.Clear();NrScreen.video.Clear();
        elapsed+=Math.Min(dt,0.1f);
        while(elapsed>=NrCore.interval/1000f){
            elapsed-=NrCore.interval/1000f;
            bool pause=(NrScreen.input&64)!=0,escape=(NrScreen.input&128)!=0;
            if(pause&&!pausePressed){if(NrCore.status==IN_GAME)NrCore.status=PAUSE;else if(NrCore.status==PAUSE)NrCore.status=IN_GAME;}
            if(escape&&!escapePressed){NrPreference.savePreference();NrCore.initTitle();}
            pausePressed=pause;escapePressed=escape;NrCore.move();NrCore.tick++;
        }
        if(NrScreen.quitRequested){NrScreen.quitRequested=false;if(Host.Available())Host.Send("quit","");}
        NrCore.draw();if(NrCore.status==TITLE)NrAttract.drawTitle();NrRender.Frame();
    }
    public static void OnQuit(){NrPreference.savePreference();}
}
