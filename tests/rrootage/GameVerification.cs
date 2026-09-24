using System;
using static RrConstants;
public static class RrVerification {
    static int failures;
    static void Check(bool condition,string name){if(!condition){failures++;Console.WriteLine("FAIL "+name);}}
    static void Draw(){RrCore.draw(RrScreen.setEyepos(), Lub.Gfx.Blend.Additive, "scene");}
    static void Move(int count){for(int i=0;i<count;i++){RrCore.move();RrCore.tick++;}}
    static int[][] shipReference=new int[][] {
        new int[]{-22830,26322,500,100,2,21,0,0,0,0,0,2000,0,0},
        new int[]{-37888,-7482,1000,40,2,81,0,0,0,0,0,2000,0,0},
        new int[]{-15118,15288,500,0,2,141,0,0,0,0,0,2000,0,0},
        new int[]{18626,49032,1000,100,1,21,0,0,0,0,0,2000,0,0},
        new int[]{-4204,26202,500,40,1,81,0,0,0,0,0,2000,0,10},
        new int[]{-37888,-7602,1000,0,1,141,0,0,0,0,0,2000,0,10},
        new int[]{-15118,15168,500,100,0,21,0,0,0,0,0,2000,0,10},
        new int[]{18626,48912,1000,40,0,81,0,0,0,0,0,2000,0,10},
        new int[]{-4204,26082,500,0,0,141,0,0,0,0,0,2000,0,10},
        new int[]{-37888,-7722,1000,0,0,0,0,0,0,0,0,2000,0,10},
        new int[]{-18631,30521,300,180,3,0,45,0,0,0,0,2000,0,0},
        new int[]{-37888,11065,1000,120,3,0,25,0,0,0,0,2000,0,0},
        new int[]{-6268,42685,500,60,3,0,0,0,0,0,0,2000,0,0},
        new int[]{12303,58368,300,0,3,0,45,0,0,0,0,2000,0,0},
        new int[]{-7153,38912,1000,0,3,0,25,60,0,0,0,2000,0,10},
        new int[]{-37888,7232,500,0,3,0,0,0,0,0,0,2000,0,10},
        new int[]{-19317,25803,300,0,3,0,45,0,0,0,0,2000,0,10},
        new int[]{79,45199,1000,0,3,0,25,0,0,0,0,2000,0,10},
        new int[]{-31601,13519,500,0,3,0,0,0,0,0,0,2000,0,10},
        new int[]{-37888,-5112,300,0,3,0,45,0,0,0,0,2000,0,10},
        new int[]{-33960,15192,800,180,3,0,0,0,1,0,0,2000,0,0},
        new int[]{-37888,-18768,800,120,3,0,0,0,1,0,0,2000,0,0},
        new int[]{-3988,15132,800,60,3,0,0,0,1,0,0,2000,0,0},
        new int[]{29912,49032,800,0,3,0,0,0,0,0,0,2000,0,0},
        new int[]{-4048,15072,800,0,3,0,0,0,0,0,0,2000,0,0},
        new int[]{-37888,-18888,800,0,3,0,0,0,0,0,0,2000,0,0},
        new int[]{-3988,15012,800,0,3,0,0,0,1,0,0,2000,0,0},
        new int[]{29912,48912,800,0,3,0,0,0,1,0,0,2000,0,0},
        new int[]{-4048,14952,800,0,3,0,0,0,1,0,0,2000,0,0},
        new int[]{-37888,-19008,800,0,3,0,0,0,0,0,0,2000,0,0},
        new int[]{-33960,15192,800,180,3,0,0,0,0,0,0,2000,1000,0},
        new int[]{-37888,-18768,800,109,3,0,0,0,0,0,79,0,0,0},
        new int[]{-3988,15132,800,49,3,0,0,0,0,0,19,0,0,0},
        new int[]{29912,49032,800,0,3,0,0,0,0,0,0,205,0,0},
        new int[]{-4048,15072,800,0,3,0,0,0,0,0,0,505,0,10},
        new int[]{-37888,-18888,800,0,3,0,0,0,0,0,0,805,0,10},
        new int[]{-3988,15012,800,0,3,0,0,0,0,0,0,1105,0,10},
        new int[]{29912,48912,800,0,3,0,0,0,0,0,0,1405,0,10},
        new int[]{-4048,14952,800,0,3,0,0,0,0,0,0,1705,0,10},
        new int[]{-37888,-19008,800,0,3,0,0,0,0,0,0,2000,50,10},
    };
    static void ShipMotion(int mode){
        RrAttract.setMode(mode);RrCore.initGame(0);
        for(int f=0;f<600;f++){
            RrInput.input=(f%120<80?16:0)|(f%180>=40&&f%180<100?32:0)|(f%240<120?5:10);
            if(f==240){RrShip.ship.grzCnt=2000;RrShip.ship.absEng=10;}
            RrShip.moveShip();var s=RrShip.ship;
            if(f%60==59){
                int[] actual=new int[]{s.pos.x,s.pos.y,s.speed,s.invCnt,RrShip.bomb,s.bombCnt,s.rollingCnt,s.grzInvCnt,s.color,s.colorChgCnt,s.rfCnt,s.rfMtr,s.rfMtrDec,s.absEng};
                int[] expected=shipReference[mode*10+f/60];
                for(int j=0;j<actual.Length;j++)Check(actual[j]==expected[j],"original ship mode "+mode.ToString()+" frame "+f.ToString()+" field "+j.ToString());
            }
        }
    }
    static void Stages(int mode){
        RrAttract.setMode(mode);RrInput.input=0;
        for(int stage=0;stage<40;stage++){
            RrCore.initGame(stage);
            for(int scene=0;scene<5;scene++){
                Check(RrAttract.scene==scene,"boss round");
                var b=RrBoss.boss;Check(b.shield>0&&b.batteryGroupNum>0,"boss generation");
                for(int i=0;i<b.batteryGroupNum;i+=2){
                    var a=b.batteryGroup[i];var other=b.batteryGroup[i+1];
                    if(mode==IKA_MODE)Check(a.shape.color!=other.shape.color,"mirrored batteries have opposite polarity");
                    for(int j=0;j<b.patternNum;j++)if(a.attack[j].barrageType!=NOT_EXIST)Check(a.attack[j].xReverse==-other.attack[j].xReverse,"mirrored firing directions");
                }
                b.stateCnt=1;RrBoss.moveBoss();Check(b.state==ATTACKING,"boss starts");
                RrBoss.damageBoss(b.shield);Check(b.state==CHANGE,"boss changes pattern");
                b.stateCnt=1;RrBoss.moveBoss();Check(b.state==LAST_ATTACK,"last attack");
                RrBoss.damageBoss(b.shield);Check(b.state==DESTROIED,"boss destruction");
                b.stateCnt=1;RrBoss.moveBoss();Check(b.state==DESTROIED_END,"boss bonus");
                if(scene==4)Check(RrCore.status==STAGE_CLEAR,"fifth boss clears stage");
                RrAttract.bsCnt=128;RrAttract.moveBossScoreAtr();
                RrAttract.bsCnt=601;RrAttract.moveBossScoreAtr();
            }
            Check(RrCore.status==TITLE&&RrAttract.hiScore.cleard[mode][stage]==1,"stage clear saved");
        }
    }
    static Foe Bullet(int color){
        RrFoe.initFoes();var f=RrFoe.foe[0];f.spc=BULLET;f.cmd=null;f.color=color;f.pos.x=RrShip.ship.pos.x+4000;f.pos.y=RrShip.ship.pos.y;f.spd=0;f.vel.x=0;f.vel.y=0;f.d=0;f.xReverse=1;f.speedRank=1;f.slowMvCnt=0;f.grzRng=0;return f;
    }
    static void Defenses(){
        RrAttract.setMode(2);RrCore.initGame(0);var f=Bullet(0);RrFoe.moveFoes();
        Check(f.spc==NOT_EXIST&&RrShip.ship.absEng==1&&RrAttract.score==100,"same polarity absorption");
        f=Bullet(1);RrFoe.moveFoes();Check(f.spc==BULLET&&RrShip.ship.absEng==1,"opposite polarity remains");
        RrAttract.setMode(3);RrCore.initGame(0);RrShip.ship.rfCnt=100;RrShip.ship.rfWdt=10000;f=Bullet(1);RrFoe.moveFoes();
        Check(f.spc==NOT_EXIST&&RrShip.ship.reflects==1&&RrAttract.score==100,"barrier reflection");
        RrAttract.setMode(1);RrCore.initGame(0);RrShip.ship.invCnt=0;f=Bullet(1);RrFoe.moveFoes();Check(f.grzRng==-1,"graze enters range");
        f.pos.x=RrShip.ship.pos.x+6000;RrFoe.moveFoes();Check(RrShip.ship.grzCnt==GRZ_METER_UP&&RrAttract.score==50,"graze exits range");
        RrAttract.setMode(0);RrCore.initGame(0);RrShip.ship.invCnt=0;int left=RrAttract.left;RrShip.destroyShip();
        Check(RrAttract.left==left-1&&RrShip.ship.invCnt==240&&RrAttract.shipUsed==1,"death and respawn");
        RrAttract.score=0;RrAttract.addScore(200000);Check(RrAttract.left==left&&RrAttract.nextExtend==500000,"first extend");
        RrAttract.addScore(300000);Check(RrAttract.left==left+1&&RrAttract.nextExtend==1000000,"second extend");
        RrAttract.left=0;RrShip.ship.invCnt=0;RrShip.destroyShip();Check(RrCore.status==GAMEOVER,"last life game over");
    }
    public static void Main(){
        RrAngles.tantbl=RrData.tangent;RrAngles.sctbl=RrData.sine;RrBarrage.initBarragemanager();RrAttract.initHiScore();RrAttract.initAttractManager();RrAttract.initGameStateFirst();RrCore.initTitle();Draw();
        var command=new FoeCommand(-1,new Foe(),new PatternState());command.Direction=-90;Check(command.foe.d==768,"wrapped bullet heading");
        Check(RrData.pixels().Count==RrData.atlasWidth*RrData.atlasHeight*4,"texture bytes");
        RrRandom.setSeed(5489);Check(RrRandom.nextRand()==-795755684&&RrRandom.nextRand()==581869302,"MT reference");
        for(int mode=0;mode<4;mode++){
            ShipMotion(mode);RrAttract.setMode(mode);RrCore.initGame(0);
            int bullets=0;
            for(int f=0;f<1200;f++){
                RrInput.input=16|(f%120<40?32:0)|(f%240<120?4:8);RrShip.ship.invCnt=Math.Max(RrShip.ship.invCnt,1);Move(1);
                if(f%120==0){Draw();foreach(var bullet in RrFoe.foe)if(bullet.spc==BULLET||bullet.spc==ACTIVE_BULLET)bullets++;}
            }
            Check(bullets>0,"bullets emitted");
            RrCore.initGame(39);
            for(int f=0;f<600;f++){RrInput.input=16;RrShip.ship.invCnt=Math.Max(RrShip.ship.invCnt,1);Move(1);if(f%120==0)Draw();}
            Stages(mode);Console.WriteLine("MODE "+mode.ToString());
        }
        Defenses();
        string pref="";for(int i=0;i<160;i++)pref+="1234567,1,";pref+="39,3";RrPreference.loadPreference(pref);
        Check(RrAttract.hiScore.score[3][39]==1234567&&RrAttract.hiScore.stage==39&&RrAttract.hiScore.mode==3,"preferences loaded");
        RrPreference.loadPreference(pref+",0");Check(RrAttract.hiScore.score[0][0]==1234567,"invalid preferences rejected");
        RrCore.initTitle();Draw();RrInput.input=0;Move(1);RrInput.input=32;Move(1);Check(RrAttract.mode==0,"title mode wraps");
        Console.WriteLine("RESULT "+failures.ToString());
    }
}
