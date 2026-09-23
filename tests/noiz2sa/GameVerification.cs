using System;
using static NrConstants;
public static class NrVerification {
    static int failures;
    static void Check(bool condition,string name){if(!condition){failures++;Console.WriteLine("FAIL "+name);}}
    static int Hash(int hash,int value){return (hash^value)*16777619;}
    static void Draw(){NrScreen.buf.Clear();NrScreen.l1buf.Clear();NrScreen.l2buf.Clear();NrScreen.video.Clear();NrCore.draw();Check(NrScreen.buf.rectangles.Count>0,"foreground geometry");}
    static int[][] shipReference=new int[][]{
        new int[]{3072,51009,640,-1,180},
        new int[]{3072,16822,1280,-41,120},
        new int[]{57372,71122,1280,-1,60},
        new int[]{78848,118399,640,-41,0},
        new int[]{44661,84212,1280,-1,0},
        new int[]{3072,29912,1280,-41,0},
        new int[]{50349,77189,640,-1,0},
        new int[]{78848,111329,1280,-41,0},
        new int[]{24548,57029,1280,-1,0},
        new int[]{3072,9734,640,-41,0},
    };
    static void ShipMotion(){
        NrCore.status=IN_GAME;NrBarrage.scene=0;NrShip.initShip();NrShot.initShots();
        for(int f=0;f<600;f++){
            NrScreen.input=(f%120<80?16:0)|(f%180>=40&&f%180<100?32:0)|(f%240<120?5:10);NrShip.moveShip();
            if(f%60==59){var ship=NrShip.ship;int[] actual=new int[]{ship.pos.x,ship.pos.y,ship.speed,ship.shotCnt,ship.invCnt};
                for(int j=0;j<actual.Length;j++)Check(actual[j]==shipReference[f/60][j],"original ship frame "+f.ToString());
            }
        }
    }
    static int[] barrageReference=new int[]{-347927638,1308616845,-407182369,-232135613,-336854837,399028779,1816497766,1753609540,-1640707090,310302507,-550963896,-1668810613,587296288,1914067372};
    static int BarrageId(Barrage barrage){for(int j=0;j<NrData.barrages[barrage.type].Length;j++)if(NrData.barrages[barrage.type][j]==barrage.bulletml)return barrage.type*100+j;return -1;}
    static void Barrages(){
        NrRandom.visualSeed=12345;
        for(int stage=0;stage<14;stage++){
            var parameters=NrCore.stagePrm[stage];NrBarrage.initBarrages(GameMath.integer(parameters[0]),parameters[1],parameters[2]);float level=parameters[1];int hash=-2128831035;
            for(int scene=0;scene<30;scene++){
                NrBarrage.setBarrages(level,scene%10==9?1:0,scene%10==4?1:0);level+=parameters[2];
                int[] values=new int[]{stage,scene,NrBarrage.barrageNum,NrBarrage.pax,NrBarrage.pay,NrBarrage.quickAppType,NrBarrage.rnd};
                foreach(int value in values)hash=Hash(hash,value);
                for(int i=0;i<NrBarrage.barrageNum;i++){var b=NrBarrage.barrage[i];hash=Hash(hash,BarrageId(b));hash=Hash(hash,b.frq);Check(b.rank>=0&&b.rank<=1,"rank range");}
            }
            Check(hash==barrageReference[stage],"original thirty-scene barrage sequence "+stage.ToString());
        }
    }
    static void Raster(){
        int[] expected=new int[]{401855263,1130967721,936315655};var buffer=NrScreen.buf;
        for(int c=0;c<3;c++){
            buffer.Clear();
            if(c==0){NrScreen.drawBox(-5,3,30,10,16,32,buffer);NrScreen.drawBox(319,479,9,9,290,63,buffer);NrScreen.drawBox(155,217,101,32,73,84,buffer);}
            if(c==1){NrScreen.drawLine(10,40,200,140,47,3,buffer);NrScreen.drawLine(312,470,-8,100,62,4,buffer);NrScreen.drawLine(50,40,50,430,71,2,buffer);}
            if(c==2){NrScreen.drawThickLine(150,460,160,100,66,88,4);NrScreen.drawThickLine(-50,5,210,450,27,54,6);NrScreen.drawThickLine(310,40,20,35,11,22,5);}
            int[] pixels=NrArrays.Make(320*480,()=>0);var r=buffer.rectangles;
            for(int i=0;i<r.Count;i+=12){
                int x=GameMath.integer((r[i]+1)*160+0.5f),y=GameMath.integer((1-r[i+1])*240+0.5f),w=GameMath.integer(r[i+2]*160+0.5f),h=GameMath.integer(-r[i+3]*240+0.5f);
                for(int oy=0;oy<h;oy++)for(int ox=0;ox<w;ox++)pixels[(y+oy)*320+x+ox]=GameMath.integer(r[i+4]);
            }
            int hash=-2128831035;foreach(int pixel in pixels)hash=Hash(hash,pixel);
            Check(hash==expected[c],"original raster clipping and pixels "+c.ToString());
        }
    }
    static void KillBoss(){
        var boss=NrBarrage.bossBullet;Check(boss!=null&&boss.type==BOSS_TYPE,"boss spawn");if(boss==null)return;
        boss.shield=1;boss.cmd=null;boss.spd=0;boss.vel.x=0;boss.vel.y=0;
        NrShot.initShots();NrShot.addShot(boss.pos);NrFoe.moveFoes();
    }
    static void Stages(){
        for(int stage=0;stage<14;stage++){
            NrCore.initGame(stage);int shots=0;
            for(int f=0;f<600;f++){
                NrScreen.input=16|(f%120<60?32:0)|(f%240<120?4:8);NrShip.ship.invCnt=Math.Max(NrShip.ship.invCnt,1);NrCore.move();NrCore.tick++;
                if(f%120==0){Draw();foreach(var shot in NrShot.shot)if(shot.cnt!=NOT_EXIST)shots++;}
            }
            Check(shots>0,"shots emitted");NrScreen.input=0;
            int rounds=stage<10?10:20;
            for(int scene=1;scene<rounds;scene++){
                NrBarrage.sceneCnt=0;NrBarrage.addBullets();Check(NrBarrage.scene==scene,"scene advance");
                if(scene%10==9){KillBoss();Check(NrCore.status==(stage<10?STAGE_CLEAR:IN_GAME),"boss outcome");}
            }
            if(stage<10){NrAttract.scCnt=901;NrAttract.moveStageClear();Check(NrCore.status==TITLE,"stage clear returns to title");}
            else Check(NrBarrage.endless==1&&NrBarrage.scene==19,"endless continues past first boss");
            Console.WriteLine("STAGE "+stage.ToString());
        }
    }
    static void Rules(){
        NrCore.initGame(0);NrAttract.score=0;NrBonus.bonusScore=10;NrBonus.getBonus();NrBonus.getBonus();Check(NrAttract.score==30&&NrBonus.bonusScore==30,"consecutive bonus");
        NrBonus.missBonus();Check(NrBonus.bonusScore==10,"missed bonus drops value");
        NrBonus.bonusScore=1000;NrBonus.getBonus();Check(NrBonus.bonusScore==1000,"bonus cap");
        NrShip.ship.invCnt=0;int left=NrAttract.left;NrShip.destroyShip();Check(NrAttract.left==left-1&&NrShip.ship.invCnt>0,"death and respawn");
        NrAttract.score=0;NrAttract.addScore(200000);Check(NrAttract.left==left&&NrAttract.nextExtend==500000,"first extend");
        NrAttract.addScore(300000);Check(NrAttract.left==left+1&&NrAttract.nextExtend==1000000,"second extend");
        NrAttract.left=0;NrShip.ship.invCnt=0;NrShip.destroyShip();Check(NrCore.status==GAMEOVER,"last life ends game");
        NrAttract.score=7654321;NrAttract.goCnt=901;NrAttract.moveGameover();Check(NrCore.status==TITLE&&NrAttract.hiScore.stageScore[0]==7654321,"game over score");
        string pref="";for(int i=0;i<114;i++)pref+="1234567,";pref+="13";NrPreference.loadPreference(pref);Check(NrAttract.hiScore.sceneScore[9][9]==1234567&&NrAttract.hiScore.stage==13,"preference load");
        NrPreference.loadPreference(pref+",0");Check(NrAttract.hiScore.stage==13,"invalid preference ignored");
    }
    public static void Main(){
        NrAngles.tantbl=NrData.tangent;NrAngles.sctbl=NrData.sine;NrAttract.initHiScore();NrBarrage.initBarragemanager();NrAttract.initAttractManager();NrCore.initTitle();
        Check(NrData.tablePixels().Count==256*260*4&&NrData.spritePixels().Count==40*280*4,"palette and sprite bytes");
        ShipMotion();Barrages();Raster();Stages();Rules();Console.WriteLine("RESULT "+failures.ToString());
    }
}
