using System.Collections.Generic;
using static NrConstants;
public class NrWorld : PatternWorld {
    public float Turn;
    public float GetTurn(){return Turn;}
    public float RandomValue(){return NrRandom.rand()/32767f;}
    public void Fire(PatternBody body,float direction,float speed,int program,float[] args){
        var f=((FoeCommand)body).foe;int d=GameMath.integer(direction*DIV/360)&1023,spd=GameMath.integer(speed*800);
        if(program<0)NrFoe.addFoeNormalBullet(f.pos,f.rank,d,spd,f.color+1);
        else NrFoe.addFoeActiveBullet(f.pos,f.rank,d,spd,f.color+1,BarrageCode.Create(program,args));
    }
    public void Vanish(PatternBody body){NrFoe.removeFoe(((FoeCommand)body).foe);}
}
public class FoeCommand : PatternBody {
    public Foe foe;
    public List<PatternState> Scripts=new List<PatternState>();
    public static NrWorld world=new NrWorld();
    public FoeCommand(int parser,Foe foe,PatternState state=null){
        this.foe=foe;if(state!=null)Scripts.Add(state);
        else foreach(int root in BarrageCode.Roots(parser))Scripts.Add(BarrageCode.Create(root,new float[3]));
    }
    public float GetDirection(){return foe.d*360f/DIV;}
    public void SetDirection(float value){foe.d=GameMath.integer(value*DIV/360)&1023;}
    public float GetSpeed(){return foe.spd/800f;}
    public void SetSpeed(float value){foe.spd=GameMath.integer(value*800);}
    public float GetAim(){return NrShip.getPlayerDeg(foe.pos.x,foe.pos.y)*360f/DIV;}
    public float GetRank(){return foe.rank;}
    public float GetAccelX(){return foe.vel.x/800f;}
    public void SetAccelX(float value){foe.vel.x=GameMath.integer(value*800);}
    public float GetAccelY(){return foe.vel.y/800f;}
    public void SetAccelY(float value){foe.vel.y=GameMath.integer(value*800);}
    public bool isEnd(){return PatternRunner.Ended(Scripts);}
    public void run(){world.Turn=NrCore.tick;PatternRunner.Run(Scripts,this,world);}
}
