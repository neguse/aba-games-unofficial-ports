// Copyright 2002 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static NrCore;
using static NrAttract;
using static NrShip;
using static NrShot;
using static NrFrag;
using static NrBonus;
using static NrBackground;
using static NrFoe;
using static NrBarrage;
using static NrLetter;
using static NrConstants;
using static NrArrays;
using static NrRandom;
using static NrScreen;
using static NrSound;
using static NrPreference;
using static NrAngles;
using static NrVector;

public class Vector {
public int x;
public int y;
}
public class Ship {
public Vector pos = new Vector();
public int cnt;
public int shotCnt;
public int speed;
public int invCnt;
}
public class Shot {
public Vector pos = new Vector();
public int cnt;
}
public class Frag {
public Vector pos = new Vector();
public Vector vel = new Vector();
public int width;
public int height;
public int cnt;
public int spc;
}
public class Bonus {
public Vector pos = new Vector();
public Vector vel = new Vector();
public int cnt;
public int down;
}
public class Board {
public int x;
public int y;
public int z;
public int width;
public int height;
}
public class HiScore {
public int[] stageScore = Make(STAGE_NUM+ENDLESS_STAGE_NUM, () => 0);
public int[][] sceneScore = Make(STAGE_NUM, () => Make(SCENE_NUM, () => 0));
public int stage;
}
public class Barrage {
public int bulletml;
public PatternNumber maxRank=new PatternNumber(0,0);
public PatternNumber exactRank=new PatternNumber(0,0);
public float rank;
public int type;
public int frq;
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
public int type;
public int shield;
public int cnt;
public int color;
public int hit;
public int parser;
}
