using static RrConstants;
public class RrWorld : PatternWorld {
    public override float RandomValue() {return RrRandom.rand()/32767f;}
    public override void Fire(PatternBody body,float direction,float speed,int program,float[] args) {
        var foe=((FoeCommand)body).foe;
        int d=GameMath.integer(direction*RrConstants.DIV/360)&1023, sp=GameMath.integer(speed*RrConstants.COMMAND_SCREEN_SPD_RATE);
        if(program<0)RrFoe.addFoeNormalBullet(foe,d,sp,foe.color+1);
        else RrFoe.addFoeActiveBullet(foe,d,sp,foe.color+1,BarrageCode.Create(program,args));
        foe.fireCnt++;
    }
    public override void Vanish(PatternBody body) {RrFoe.removeFoeCommand(((FoeCommand)body).foe);}
}

public class FoeCommand : PatternBody {
    public Foe foe;
    public static RrWorld world = new RrWorld();
    public FoeCommand(int parser,Foe foe,PatternState state=null) {
        this.foe=foe;
        if(state!=null) Scripts.Add(state);
        else foreach(int root in BarrageCode.Roots(parser)) Scripts.Add(BarrageCode.Create(root,new float[3]));
    }
    public override float Direction { get { return foe.d*360f/DIV; } set { foe.d=GameMath.integer(value*DIV/360)&1023; } }
    public override float Speed { get { return foe.spd/(float)COMMAND_SCREEN_SPD_RATE; } set { foe.spd=GameMath.integer(value*COMMAND_SCREEN_SPD_RATE); } }
    public override float Aim { get { int d=RrShip.getPlayerDeg(foe.pos.x,foe.pos.y);if(foe.xReverse==-1)d=(-d)&1023;return d*360f/DIV; } set {} }
    public override float Rank { get { return foe.rank; } set {} }
    public override float AccelX { get {return foe.vel.x/(float)COMMAND_SCREEN_VEL_RATE;} set {foe.vel.x=GameMath.integer(value*COMMAND_SCREEN_VEL_RATE);} }
    public override float AccelY { get {return foe.vel.y/(float)COMMAND_SCREEN_VEL_RATE;} set {foe.vel.y=GameMath.integer(value*COMMAND_SCREEN_VEL_RATE);} }
    public bool isEnd() { return PatternEnded(); }
    public void run() { world.Turn=RrCore.tick;RunPattern(world); }
}
