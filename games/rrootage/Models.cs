// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrInput;
using static RrPreference;
using static RrCore;
using static RrAttract;
using static RrShip;
using static RrLaser;
using static RrShot;
using static RrFrag;
using static RrBackground;
using static RrBoss;
using static RrFoe;
using static RrScreen;
using static RrLetter;
using static RrAngles;
using static RrVector;

public class Vector {
public int x;
public int y;
}
public class Ship {
public Vector pos = new Vector();
public Vector bombPos = new Vector();
public int cnt;
public int laserCnt;
public int speed;
public int invCnt;
public int bombCnt;
public int bombWdt;
public int grzCnt;
public int grzInvCnt;
public int rollingCnt;
public int grzWdt;
public int grzf;
public float d;
public int color;
public int colorChgCnt;
public int fldWdt;
public int absEng;
public int rfCnt;
public int rfMtr;
public int rfMtrDec;
public int rfWdt;
public int reflects;
}
public class Laser {
public int y;
public int color;
public int cnt;
}
public class Shot {
public float x;
public float y;
public float mx;
public float my;
public float d;
public int cnt;
public int color;
public float width;
public float height;
}
public class Frag {
public float x;
public float y;
public float z;
public float mx;
public float my;
public float mz;
public float width;
public float height;
public int d1;
public int d2;
public int md1;
public int md2;
public int[] r = Make(FRAG_COLOR_NUM, () => 0);
public int[] g = Make(FRAG_COLOR_NUM, () => 0);
public int[] b = Make(FRAG_COLOR_NUM, () => 0);
public int cnt;
}
public class Plane {
public float x;
public float y;
public float z;
public float ox;
public float oy;
public float mx;
public float my;
public int d1;
public float width;
public float height;
public int xn;
public int yn;
public int r;
public int g;
public int b;
public int a;
}
public class HiScore {
public int[][] score = Make(MODE_NUM, () => Make(STAGE_NUM, () => 0));
public int[][] cleard = Make(MODE_NUM, () => Make(STAGE_NUM, () => 0));
public int stage;
public int mode;
}
public class Barrage {
public int bulletml;
public float maxRank;
public float rank;
}
public class Attack {
public void copyFrom(Attack source) {
    barrageType=source.barrageType; barrageIdx=source.barrageIdx; rank=source.rank;
    xReverse=source.xReverse; xrAlter=source.xrAlter;
    for(int i=0;i<MORPH_PATTERN_MAX;i++)morphIdx[i]=source.morphIdx[i];
    morphCnt=source.morphCnt; morphHalf=source.morphHalf; morphType=source.morphType;
    morphRank=source.morphRank; speedRank=source.speedRank; ikaType=source.ikaType;
}

public int barrageType;
public int barrageIdx;
public float rank;
public int xReverse;
public int xrAlter;
public int[] morphIdx = Make(MORPH_PATTERN_MAX, () => 0);
public int morphCnt;
public int morphHalf;
public int morphType;
public float morphRank;
public float speedRank;
public int ikaType;
}
public class Battery {
public Foe foe;
public int x;
public int y;
}
public class BatteryShape {
public void copyFrom(BatteryShape source) {
    color=source.color;
    for(int i=0;i<BULLET_TYPE_NUM;i++){bulletShape[i]=source.bulletShape[i];bulletSize[i]=source.bulletSize[i];}
}

public int color;
public int[] bulletShape = Make(BULLET_TYPE_NUM, () => 0);
public float[] bulletSize = Make(BULLET_TYPE_NUM, () => 0f);
}
public class BatteryGroup {
public Attack[] attack = Make(BATTERY_PATTERN_MAX, () => new Attack());
public Battery[] battery = Make(BATTERY_MAX, () => new Battery());
public int batteryNum;
public BatteryShape shape = new BatteryShape();
public Limiter limiter = new Limiter();
}
public class Boss {
public Attack[] topAttack = Make(BATTERY_PATTERN_MAX, () => new Attack());
public Battery topBattery = new Battery();
public Limiter topLimiter = new Limiter();
public BatteryShape shape = new BatteryShape();
public BatteryGroup[] batteryGroup = Make(BATTERY_GROUP_MAX, () => new BatteryGroup());
public int batteryGroupNum;
public int x;
public int y;
public int d;
public int[] mpx = Make(MOVE_POINT_MAX, () => 0);
public int[] mpy = Make(MOVE_POINT_MAX, () => 0);
public int mpNum;
public int mpIdx;
public int speed;
public int md;
public int onRoute;
public int patternIdx;
public int patternNum;
public int patternCnt;
public int patternLgt;
public int color;
public int[] bulletShape = Make(3, () => 0);
public float[] bulletSize = Make(3, () => 0f);
public int[] collisionX = Make(COLLISION_NUM, () => 0);
public int[] collisionY = Make(COLLISION_NUM, () => 0);
public int collisionYUp;
public int shield;
public int patternChangeShield;
public int damaged;
public int damageCnt;
public int cnt;
public int state;
public int stateCnt;
public int r;
public int g;
public int b;
}
public class BossWing {
public float[][] x = Make(BOSS_WING_MAX, () => Make(2, () => 0f));
public float[][] y = Make(BOSS_WING_MAX, () => Make(2, () => 0f));
public float[][] z = Make(BOSS_WING_MAX, () => Make(2, () => 0f));
public int wingNum;
public float size;
}
public class BossTree {
public float[] x = Make(TREE_MAX_LENGTH, () => 0f);
public float[] y = Make(TREE_MAX_LENGTH, () => 0f);
public float[] z = Make(TREE_MAX_LENGTH, () => 0f);
public float[] ex = Make(BATTERY_MAX, () => 0f);
public float[] ey = Make(BATTERY_MAX, () => 0f);
public float[] ez = Make(BATTERY_MAX, () => 0f);
public BossWing[] wing = Make(TREE_MAX_LENGTH, () => new BossWing());
public BossWing[] eWing = Make(BATTERY_MAX, () => new BossWing());
public int posNum;
public int epNum;
public int diffuse;
}
public class BossShape {
public BossTree[] tree = Make(BATTERY_GROUP_MAX, () => new BossTree());
public int r;
public int g;
public int b;
public int diffuse;
}
public class Limiter {
public int cnt;
public int max;
public int on;
}
public class Foe {
public Vector pos = new Vector();
public Vector vel = new Vector();
public Vector ppos = new Vector();
public Vector spos = new Vector();
public Vector mv = new Vector();
public int d;
public int spd;
public FoeCommand cmd;
public float rank;
public int spc;
public int cnt;
public int cntTotal;
public int xReverse;
public int fireCnt;
public int slowMvCnt;
public int parser;
public int[] morphParser = Make(MORPH_PATTERN_MAX, () => 0);
public int morphCnt;
public int morphHalf;
public float morphRank;
public float speedRank;
public int color;
public int shapeType;
public int[] bulletShape = Make(3, () => 0);
public float[] bulletSize = Make(3, () => 0f);
public Limiter limiter;
public int ikaType;
public int grzRng;
public void copyFrom(Foe f) {
pos.x=f.pos.x; pos.y=f.pos.y;
vel.x=f.vel.x; vel.y=f.vel.y;
ppos.x=f.ppos.x; ppos.y=f.ppos.y;
spos.x=f.spos.x; spos.y=f.spos.y;
mv.x=f.mv.x; mv.y=f.mv.y;
d=f.d;
spd=f.spd;
cmd=f.cmd;
rank=f.rank;
spc=f.spc;
cnt=f.cnt;
cntTotal=f.cntTotal;
xReverse=f.xReverse;
fireCnt=f.fireCnt;
slowMvCnt=f.slowMvCnt;
parser=f.parser;
for(int i=0;i<f.morphParser.Length;i++) morphParser[i]=f.morphParser[i];
morphCnt=f.morphCnt;
morphHalf=f.morphHalf;
morphRank=f.morphRank;
speedRank=f.speedRank;
color=f.color;
shapeType=f.shapeType;
for(int i=0;i<f.bulletShape.Length;i++) bulletShape[i]=f.bulletShape[i];
for(int i=0;i<f.bulletSize.Length;i++) bulletSize[i]=f.bulletSize[i];
limiter=f.limiter;
ikaType=f.ikaType;
grzRng=f.grzRng;
}

}
