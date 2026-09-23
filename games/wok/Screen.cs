using static WkConstants;
public static class WkScreen {
 public static bool clickPending;
 public static int mx=320,my=240,buttons;
 public static void initMouse(){}
 public static void Pointer(string text){
  string[] words=text.Split(",");if(words.Length!=3)return;
  int x=GameMath.parseNonnegative(words[0]),y=GameMath.parseNonnegative(words[1]),b=GameMath.parseNonnegative(words[2]);
  if(x<0||x>640||y<0||y>480||b<0||b>3)return;mx=x;my=y;buttons=b;if((b&1)!=0)clickPending=true;
 }
 public static void drawSprite(int n,int x,int y){WkRender.Sprite(n,x,y);}
 public static int drawNum(int n,int x,int y){
  bool drawn=false;
  for(int d=100000000;d>0;d/=10){int nd=n/d;if(nd>0||drawn){n-=d*nd;drawSprite(nd+NUM_SPRITE_IDX,x,y);x+=50;drawn=true;}}
  if(!drawn){drawSprite(NUM_SPRITE_IDX,x,y);x+=52;}return x;
 }
 public static int drawNumPaper(int n,int x,int y,int pc){
  int ofs=pc*6;pc+=PPS_SPRITE_IDX;bool drawn=false;
  for(int d=1000000;d>0;d/=10){int nd=n/d;if(nd>0||drawn){n-=d*nd;drawSprite(pc,x,y);x+=ofs;drawn=true;}}return x;
 }
 public static void fillRect(int x,int y,int w,int h){WkRender.Solid(x,y,w,h);}
 public static void drawThrownZone(){fillRect(595,0,64,480);}
}
