using System;
using Lub;
using static WkConstants;
public static class Game {
 static float elapsed;
 static int input;
 static bool escapePressed,scoresLoaded,captured,wasFocused=true;
 static float pointerX=320,pointerY=240;
 static int lastX=-1,lastY=-1,lastButtons=-1;
 public static void OnInit(){
  WkRandom.Seed(1);WkRender.Init();WkAttract.initTitle();
  WkRandom.Seed(TinySystem.Random.Shared.Next());
 }
 // タイトル以外ではマウスを window に閉じ込め、相対移動を 3 倍で使う。タイトルではマウスの位置を使う。
 // pad のスティックと十字キーでも動かす。フォーカスを失ったら、遊んでいる途中ならタイトルに戻る。
 // 位置かボタンが変わったときと、クリックしたときだけ WkScreen に渡す。
 static void readPointer(float dt){
  bool focused=GameInput.Focused(),playing=WkCore.status!=TITLE;
  if(wasFocused&&!focused&&playing)WkAttract.initTitle();
  wasFocused=focused;
  bool capture=focused&&WkCore.status!=TITLE;
  if(capture!=captured)captured=Lub.Input.CaptureMouse(capture)&&capture;
  if(!focused)return;
  Lub.Input.MouseDelta(out float dx,out float dy);
  bool clicked=Lub.Input.MousePressed(1);
  if(WkCore.status!=TITLE){pointerX+=dx*3;pointerY+=dy*3;}
  else if(dx!=0||dy!=0||clicked||Lub.Input.MousePressed(3)){pointerX=GameInput.PointerX();pointerY=GameInput.PointerY();}
  var pad=GameInput.Pad();
  pointerX+=(pad.RightX!=0?pad.RightX:pad.LeftX)*600*dt;pointerY-=(pad.RightY!=0?pad.RightY:pad.LeftY)*600*dt;
  if(pad.Left)pointerX-=600*dt;if(pad.Right)pointerX+=600*dt;if(pad.Up)pointerY-=600*dt;if(pad.Down)pointerY+=600*dt;
  pointerX=Math.Clamp(pointerX,0,639);pointerY=Math.Clamp(pointerY,0,479);
  int x=(int)pointerX,y=(int)pointerY,b=(Lub.Input.MouseDown(1)||pad.A?1:0)|(Lub.Input.MouseDown(3)||pad.Secondary?2:0);
  // フレームの間に押して離したクリックも押したことにする
  if(clicked&&!Lub.Input.MouseDown(1))WkScreen.Pointer(x,y,b|1);
  if(x!=lastX||y!=lastY||b!=lastButtons||clicked){WkScreen.Pointer(x,y,b);lastX=x;lastY=y;lastButtons=b;}
 }
 public static void OnFrame(float dt){
  input=GameInput.Mask();
  readPointer(dt);
  if(!scoresLoaded&&GameSave.Ready("scores")){scoresLoaded=true;string? scores=GameSave.Load("scores");if(scores!=null)WkPreference.Load(scores);}
  elapsed+=Math.Min(dt,0.05f);
  while(elapsed>=0.01f){
   elapsed-=0.01f;bool escape=(input&128)!=0;
   if(escape&&!escapePressed){if(WkCore.status==TITLE)WkCore.quitWok();else WkAttract.initTitle();}
   escapePressed=escape;int buttons=WkScreen.buttons,mx=WkScreen.mx,my=WkScreen.my;
   // A click acts where it was pressed, even if the pointer has moved on since.
   if(WkScreen.clickPending){WkScreen.buttons|=1;WkScreen.mx=WkScreen.clickX;WkScreen.my=WkScreen.clickY;}
   WkCore.move();WkRender.vertices.Clear();WkCore.draw();WkCore.rank++;
   WkScreen.clickPending=false;WkScreen.buttons=buttons;WkScreen.mx=mx;WkScreen.my=my;
  }
  WkRender.Frame();WkSound.Frame(dt);GameAudio.Update(dt);
 }
 public static void OnEvent(EventData e){}
 public static void OnQuit(){WkPreference.Save();}
}
