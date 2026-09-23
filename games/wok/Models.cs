// Copyright 2001 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static WkCore;
using static WkAttract;
using static WkBall;
using static WkBoard;
using static WkGenerator;
using static WkPan;
using static WkVector;
using static WkConstants;
using static WkArrays;
using static WkRandom;
using static WkScreen;
using static WkSound;

public class Vector {
public float x;
public float y;
public Vector Copy(){return new Vector{x=x,y=y};}
}
public class Ball {
public Vector pos = new Vector();
public Vector vel = new Vector();
public float radius;
public int color;
public int size;
public int sprPtn;
}
public class Pan {
public Vector pos = new Vector();
public Vector prvPos = new Vector();
public Vector vel = new Vector();
public float deg;
}
public class PanPos {
public Vector p1 = new Vector();
public Vector p2 = new Vector();
public Vector p3 = new Vector();
public Vector p4 = new Vector();
public Vector pc1 = new Vector();
public Vector pc2 = new Vector();
public Vector v1 = new Vector();
public Vector v2 = new Vector();
public float v1l;
public float v2l;
public PanPos Copy(){return new PanPos{p1=p1.Copy(),p2=p2.Copy(),p3=p3.Copy(),p4=p4.Copy(),pc1=pc1.Copy(),pc2=pc2.Copy(),v1=v1.Copy(),v2=v2.Copy(),v1l=v1l,v2l=v2l};}
}
public class Board {
public float x;
public float y;
public float mx;
public float my;
public int sc;
public int mp;
public int cnt;
public int apCnt;
}
public class Generator {
public int x;
public int y;
public int cnt;
public int apCnt;
public int spc;
}
