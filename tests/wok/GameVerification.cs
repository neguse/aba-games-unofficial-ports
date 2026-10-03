using System;
using static WkConstants;
public static class WkVerification {
 static int failures;
 static void Check(bool value,string name){if(!value){Console.WriteLine("FAIL "+name);failures++;}}
 static void Near(float value,float expected,string name){Check(Math.Abs(value-expected)<0.002f,name);}
 static float[][] physics=new float[][]{new float[]{321.844971f,309.421753f,0.336850882f,-0.166397572f,347.077454f,309.050934f,0.392498404f,-0.010027715f,314.820801f,287.296234f,0.763935208f,-0.847513914f,339f,331f,0.237358764f},new float[]{350.914886f,293.432922f,1.68749797f,-0.0759232044f,382.784973f,296.447784f,2.5614574f,-0.109377414f,330.099731f,271.185974f,0.763935208f,-0.767513752f,359f,311f,0.0914381742f},new float[]{350.238403f,273.669525f,-0.358694434f,-0.234298587f,389.08905f,263.571442f,-0.227638513f,-2.69528675f,343.933136f,253.995972f,0.670205474f,-0.820801258f,341f,291f,-0.027509423f},new float[]{340.788666f,272.712524f,-0.442815274f,0.0743253008f,383.151794f,202.527191f,-0.243493348f,-2.94036436f,357.544952f,239.375656f,0.701473534f,-0.611847162f,321f,331f,-0.0518409796f},new float[]{331.932465f,276.719055f,-0.442815274f,0.314325243f,378.281799f,145.399887f,-0.243493348f,-2.78036642f,371.574493f,227.978714f,0.701473534f,-0.531847f,339f,311f,0.0349811539f},new float[]{336.654877f,273.029541f,0.671457469f,-0.194584653f,373.411804f,91.4725266f,-0.243493348f,-2.62036848f,385.604034f,218.181763f,0.701473534f,-0.451846838f,359f,291f,0.0532508343f},new float[]{353.348816f,270.764465f,0.790890992f,0.039983172f,368.541809f,40.7451363f,-0.243493348f,-2.46037054f,399.633575f,209.984833f,0.701473534f,-0.371846676f,341f,331f,-0.0347151235f},new float[]{367.857758f,273.080872f,-0.271129429f,0.0624651611f,363.679108f,-6.7131176f,-0.236261368f,-2.23181319f,413.663116f,203.387909f,0.701473534f,-0.291846514f,321f,311f,-0.0532006435f},new float[]{363.116913f,264.542938f,-0.131229162f,-0.252563804f,359.376892f,-45.7746773f,-0.193239838f,-1.68119848f,427.692657f,198.390976f,0.701473534f,-0.211846501f,339f,291f,0.0347245932f},new float[]{360.492401f,262.011627f,-0.131229162f,-0.0125638433f,355.858063f,-74.8099136f,-0.158052251f,-1.23084605f,441.722198f,194.994049f,0.701473534f,-0.131846637f,359f,331f,0.0532024316f},new float[]{357.867889f,264.280365f,-0.131229162f,0.227436125f,352.980042f,-95.6445236f,-0.129272059f,-0.862499595f,455.75174f,193.197113f,0.701473534f,-0.0518466309f,341f,311f,-0.0347242542f},new float[]{355.243378f,271.349091f,-0.131229162f,0.467436016f,350.626038f,-109.77179f,-0.10573253f,-0.561226547f,469.781281f,193.000168f,0.701473534f,0.0281533804f,321f,291f,-0.0532023683f}};
 public static void Main(){
  WkRandom.Seed(1);int[] random=new int[]{1804289383,846930886,1681692777,1714636915,1957747793,424238335,719885386,1649760492,596516649,1189641421};
  for(int i=0;i<random.Length;i++)Check(WkRandom.rand()==random[i],"glibc random "+i.ToString());
  WkCore.initGame();WkScreen.mx=320;WkScreen.my=350;WkPan.movePan();WkPan.movePan();
  WkBall.initBalls();WkBall.ballIdx=96;WkBall.addBall(0,0,280,300,1,0.4f);WkBall.addBall(1,1,340,310,-1,0.2f);WkBall.addBall(2,2,310,280,0,0.7f);
  for(int i=0;i<240;i++){
   WkScreen.mx=320+(i%80<40?i%40:40-i%40);WkScreen.my=350-i%60;WkBall.moveBalls();WkPan.movePan();
   if(i%20==19){int at=0;float[] expected=physics[i/20];
    for(int j=93;j<96;j++){Ball b=WkBall.ball[j];Near(b.pos.x,expected[at],"ball x");at++;Near(b.pos.y,expected[at],"ball y");at++;Near(b.vel.x,expected[at],"velocity x");at++;Near(b.vel.y,expected[at],"velocity y");at++;}
    Near(WkPan.pan.pos.x,expected[at],"pan x");at++;Near(WkPan.pan.pos.y,expected[at],"pan y");at++;Near(WkPan.pan.deg,expected[at],"pan angle");
   }
  }
  int[] lifetimes=new int[]{208,258,249,267,258,267},counts=new int[]{8,11,11,13,22,20};
  for(int kind=0;kind<6;kind++){
   WkRandom.Seed(1);WkCore.initGame();WkGenerator.generatorIdx=4;WkGenerator.addGenerator(kind);int ticks=0,total=0;
   for(;ticks<1000;ticks++){
    int active=0;foreach(var g in WkGenerator.generator)if(g.cnt>0)active++;
    if(active==0)break;WkBall.initBalls();WkGenerator.moveGenerators();foreach(var b in WkBall.ball)if(b.color>=0){total++;Check(b.color>=0&&b.color<3&&b.size>=0&&b.size<3,"spawn kind");}
   }
   Check(ticks==lifetimes[kind]&&total==counts[kind],"generator "+kind.ToString());
  }
  WkCore.initGame();var scoring=new Ball{size=0,pos=new Vector{x=600,y=200}};
  int[] multiplier=new int[]{2,3,5,7,10,14};int[] scores=new int[]{3,9,18,33,54,84};
  for(int i=0;i<6;i++){WkBall.addBallScore(scoring);Check(WkBall.scoreMulti==multiplier[i]&&WkCore.aimScore==scores[i],"chain "+i.ToString());}
  WkBall.initBalls();WkBall.addBallScore(scoring);for(int i=0;i<20;i++)WkBall.moveBalls();Check(WkBall.scoreMulti==1&&WkBall.smFib==1,"chain timeout");
  WkBall.scoreMulti=9998;WkBall.smFib=9998;WkBall.addBallScore(scoring);Check(WkBall.scoreMulti==9999,"chain cap");
  WkCore.aimScore=999999998;WkCore.addScore(20);Check(WkCore.aimScore==999999999,"score cap");
  WkCore.initGame();WkCore.addScore(1000001);Check(WkSound.playingMusicIdx==0&&WkSound.nextMusicIdx==1,"deferred music change");
  WkSound.Frame(WkData.musicDuration[0]);Check(WkSound.playingMusicIdx==1,"music boundary");
  WkCore.initGame();WkCore.hiScore=1;WkCore.addScore(12345);WkBall.initBalls();WkBall.addBall(0,0,100,481,0,1);WkBall.moveBalls();
  Check(WkCore.status==MISS&&WkSound.playingMusicIdx==-1,"miss");for(int i=0;i<121;i++)WkCore.move();
  Check(WkCore.status==GAMEOVER&&WkCore.score==12345&&WkCore.hiScore==12345,"game over score");
  for(int i=0;i<361;i++)WkCore.move();Check(WkCore.status==TITLE,"return title");
  WkPreference.Load("87654321");Check(WkCore.hiScore==87654321,"load score");WkPreference.Load("2147483648");WkPreference.Load("oops");Check(WkCore.hiScore==87654321,"reject bad score");
  WkScreen.Pointer("101,202,1");Check(WkScreen.mx==101&&WkScreen.my==202&&WkScreen.buttons==1,"pointer");WkScreen.Pointer("-1,900,8");Check(WkScreen.mx==101,"reject bad pointer");
  WkScreen.buttons=0;WkAttract.initTitle();WkScreen.mx=520;WkScreen.my=315;WkScreen.buttons=1;WkAttract.moveTitle();WkAttract.drawTitle();Check(WkCore.status==IN_GAME,"mouse start");WkScreen.buttons=0;
  for(int i=0;i<3000;i++){
   WkScreen.mx=320+i%40;WkScreen.my=350;WkCore.move();WkRender.vertices.Clear();WkCore.draw();WkCore.rank++;
   foreach(var b in WkBall.ball)if(b.color>=0)Check(b.pos.x==b.pos.x&&b.pos.y==b.pos.y,"finite ball");
   if(WkCore.status==GAMEOVER)WkCore.initGame();
  }
  Check(WkData.widths.Length==48,"atlas");
  Console.WriteLine("RESULT "+failures.ToString());
 }
}
