using System;
using static Lub;
using static WkConstants;
public static class Game {
 static float elapsed;
 static int input;
 static bool escapePressed;
 public static void OnInit(){
  Config(new ConfigOpts{Width=640,Height=480});WkRandom.Seed(1);WkRender.Init();WkAttract.initTitle();
  if(Host.Available()){Host.Send("scores.load","");Host.Send("ready","");}
 }
 public static void OnFrame(float dt){
  while(Host.Available()){
   Host.Poll(out string topic,out string payload);if(topic==null)break;
   if(topic=="pointer")WkScreen.Pointer(payload);
   if(topic=="input"){int value=GameMath.parseNonnegative(payload);if(value>=0)input=value;}
   if(topic=="seed"){int value=GameMath.parseNonnegative(payload);if(value>=0)WkRandom.Seed(value);}
   if(topic=="scores")WkPreference.Load(payload);
   if(topic=="leave"&&WkCore.status!=TITLE)WkAttract.initTitle();
  }
  elapsed+=Math.Min(dt,0.05f);
  while(elapsed>=0.01f){
   elapsed-=0.01f;bool escape=(input&128)!=0;
   if(escape&&!escapePressed){if(WkCore.status==TITLE)WkCore.quitWok();else WkAttract.initTitle();}
   escapePressed=escape;int buttons=WkScreen.buttons;if(WkScreen.clickPending)WkScreen.buttons|=1;
   WkCore.move();WkRender.vertices.Clear();WkCore.draw();WkCore.rank++;
   WkScreen.clickPending=false;WkScreen.buttons=buttons;
  }
  WkRender.Frame();WkSound.Frame(dt);
  if(Host.Available())Host.Send("state",WkCore.status.ToString());
 }
 public static void OnQuit(){WkPreference.Save();}
}
