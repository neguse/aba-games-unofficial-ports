using System.Collections.Generic;
using static RrConstants;
public class RrWorld : PatternWorld {
    public float Turn;
    public float GetTurn() { return Turn; }
    public float RandomValue() {return RrRandom.rand()/32767f;}
    public void Fire(PatternBody body,float direction,float speed,int program,float[] args) {
        var foe=((FoeCommand)body).foe;
        int d=GameMath.integer(direction*RrConstants.DIV/360)&1023, sp=GameMath.integer(speed*RrConstants.COMMAND_SCREEN_SPD_RATE);
        if(program<0)RrFoe.addFoeNormalBullet(foe,d,sp,foe.color+1);
        else RrFoe.addFoeActiveBullet(foe,d,sp,foe.color+1,BarrageCode.Create(program,args));
        foe.fireCnt++;
    }
    public void Vanish(PatternBody body) {RrFoe.removeFoeCommand(((FoeCommand)body).foe);}
}

public class FoeCommand : PatternBody {
    public Foe foe;
    public List<PatternState> Scripts = new List<PatternState>();
    public static RrWorld world = new RrWorld();
    public FoeCommand(int parser,Foe foe,PatternState state=null) {
        this.foe=foe;
        if(state!=null) Scripts.Add(state);
        else foreach(int root in BarrageCode.Roots(parser)) Scripts.Add(BarrageCode.Create(root,new float[3]));
    }
    public float GetDirection() { return foe.d*360f/DIV; }
    public void SetDirection(float value) { foe.d=GameMath.integer(value*DIV/360)&1023; }
    public float GetSpeed() { return foe.spd/(float)COMMAND_SCREEN_SPD_RATE; }
    public void SetSpeed(float value) { foe.spd=GameMath.integer(value*COMMAND_SCREEN_SPD_RATE); }
    public float GetAim() { int d=RrShip.getPlayerDeg(foe.pos.x,foe.pos.y);if(foe.xReverse==-1)d=(-d)&1023;return d*360f/DIV; }
    public float GetRank() { return foe.rank; }
    public float GetAccelX() {return foe.vel.x/(float)COMMAND_SCREEN_VEL_RATE;}
    public void SetAccelX(float value) {foe.vel.x=GameMath.integer(value*COMMAND_SCREEN_VEL_RATE);}
    public float GetAccelY() {return foe.vel.y/(float)COMMAND_SCREEN_VEL_RATE;}
    public void SetAccelY(float value) {foe.vel.y=GameMath.integer(value*COMMAND_SCREEN_VEL_RATE);}
    public bool isEnd() { return PatternRunner.Ended(Scripts); }
    public void run() { world.Turn=RrCore.tick;PatternRunner.Run(Scripts,this,world); }
}
