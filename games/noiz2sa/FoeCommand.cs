using static NrConstants;
public class NrWorld : PatternWorld {
    public override float RandomValue(){return NrRandom.rand()/32767f;}
    public override void Fire(PatternBody body,float direction,float speed,int program,float[] args){
        var f=((FoeCommand)body).foe;int d=GameMath.integer(direction*DIV/360)&1023,spd=GameMath.integer(speed*800);
        if(program<0)NrFoe.addFoeNormalBullet(f.pos,f.rank,d,spd,f.color+1);
        else NrFoe.addFoeActiveBullet(f.pos,f.rank,d,spd,f.color+1,BarrageCode.Create(program,args));
    }
    public override void Vanish(PatternBody body){NrFoe.removeFoe(((FoeCommand)body).foe);}
}
public class FoeCommand : PatternBody {
    public Foe foe;
    public static NrWorld world=new NrWorld();
    public FoeCommand(int parser,Foe foe,PatternState state=null){
        this.foe=foe;if(state!=null)Scripts.Add(state);
        else foreach(int root in BarrageCode.Roots(parser))Scripts.Add(BarrageCode.Create(root,new float[3]));
    }
    public override float Direction{get{return foe.d*360f/DIV;}set{foe.d=GameMath.integer(value*DIV/360)&1023;}}
    public override float Speed{get{return foe.spd/800f;}set{foe.spd=GameMath.integer(value*800);}}
    public override float Aim{get{return NrShip.getPlayerDeg(foe.pos.x,foe.pos.y)*360f/DIV;}set{}}
    public override float Rank{get{return foe.rank;}set{}}
    public override float AccelX{get{return foe.vel.x/800f;}set{foe.vel.x=GameMath.integer(value*800);}}
    public override float AccelY{get{return foe.vel.y/800f;}set{foe.vel.y=GameMath.integer(value*800);}}
    public bool isEnd(){return PatternEnded();}
    public void run(){world.Turn=NrCore.tick;RunPattern(world);}
}
