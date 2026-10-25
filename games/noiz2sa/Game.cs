using System;
using Lub;
using static NrConstants;
public static class Game {
    static float elapsed;
    static bool pausePressed,escapePressed,scoresLoaded;
    public static void OnInit(){
        NrAngles.tantbl=NrData.tangent;NrAngles.sctbl=NrData.sine;
        NrAttract.initHiScore();NrBarrage.initBarragemanager();NrAttract.initAttractManager();NrCore.initTitle();NrRender.Init();
        NrRandom.visualSeed=TinySystem.Random.Shared.Next();
    }
    public static void OnFrame(float dt){
        NrScreen.input=GameInput.Mask();
        if(!scoresLoaded&&GameSave.Ready("scores")){scoresLoaded=true;string? scores=GameSave.Load("scores");if(scores!=null){NrPreference.loadPreference(scores);if(NrCore.status==TITLE)NrCore.initTitle();}}
        NrScreen.l1buf.Clear();NrScreen.l2buf.Clear();NrScreen.buf.Clear();NrScreen.video.Clear();
        elapsed+=Math.Min(dt,0.1f);
        while(elapsed>=NrCore.interval/1000f){
            elapsed-=NrCore.interval/1000f;
            bool pause=(NrScreen.input&64)!=0,escape=(NrScreen.input&128)!=0;
            if(pause&&!pausePressed){if(NrCore.status==IN_GAME)NrCore.status=PAUSE;else if(NrCore.status==PAUSE)NrCore.status=IN_GAME;}
            if(escape&&!escapePressed){NrPreference.savePreference();NrCore.initTitle();}
            pausePressed=pause;escapePressed=escape;NrCore.move();NrCore.tick++;
        }
        if(NrScreen.quitRequested){NrScreen.quitRequested=false;Scene.End();}
        GameAudio.Update(dt);
        NrCore.draw();if(NrCore.status==TITLE)NrAttract.drawTitle();NrRender.Frame();
    }
    public static void OnEvent(EventData e){}
    public static void OnQuit(){NrPreference.savePreference();}
}
