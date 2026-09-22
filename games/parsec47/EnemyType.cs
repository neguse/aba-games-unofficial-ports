// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Barrage {

  public int parser;
  public int[] morphParser = new int[] {0,0,0,0,0,0,0,0};
  public int morphNum, morphCnt;
  public float rank, speedRank, morphRank;
  public int shape, color;
  public float bulletSize;
  public float xReverse;
}

public class BatteryType {

  public const int WING_SHAPE_POINT_NUM = 3;
  public const int WING_BATTERY_MAX = 3;
  public const int BARRAGE_PATTERN_MAX = 8;
  public Vector[] wingShapePos = new Vector[WING_SHAPE_POINT_NUM];
  public Vector collisionPos, collisionSize;
  public Vector[] batteryPos = new Vector[WING_BATTERY_MAX];
  public int batteryNum;
  public float r, g, b;
  public Barrage[] barrage = new Barrage[BARRAGE_PATTERN_MAX];
  public bool xReverseAlternate;
  public int shield;

  public BatteryType() {
    for (int index0 = 0; index0 < BARRAGE_PATTERN_MAX; index0++) {
      barrage[index0] = new Barrage();
    }
    for (int index1 = 0; index1 < WING_SHAPE_POINT_NUM; index1++) {
      wingShapePos[index1] = new Vector();
    }
    collisionPos = new Vector();
    collisionSize = new Vector();
    for (int index2 = 0; index2 < WING_BATTERY_MAX; index2++) {
      batteryPos[index2] = new Vector();
    }
  }
}

public class EnemyType {

  public const int BARRAGE_PATTERN_MAX = BatteryType.BARRAGE_PATTERN_MAX;
  public const int BODY_SHAPE_POINT_NUM = 4;
  public const int BATTERY_MAX = 4;
  public const int ENEMY_TYPE_MAX = 32;

  public static bool[] isExist = new bool[ENEMY_TYPE_MAX];
  public Barrage[] barrage = new Barrage[BARRAGE_PATTERN_MAX];
  public Vector[] bodyShapePos = new Vector[BODY_SHAPE_POINT_NUM];
  public Vector collisionSize;
  public bool wingCollision;
  public float r, g, b;
  public float retroSize;
  public BatteryType[] batteryType = new BatteryType[BATTERY_MAX];
  public int batteryNum;
  public int shield;
  public int fireInterval, firePeriod, barragePatternNum;
  public int id;
    public const int SMALL = 0;
  public const int MIDDLE = 1;
  public const int LARGE = 2;
  public const int MIDDLEBOSS = 3;
  public const int LARGEBOSS = 4;
  public int type;

    public const int ROLL = 0;
  public const int LOCK = 1;

  public static P47Rand rand = new P47Rand();
  public static BarrageManager barrageManager;
  public static int idCnt;

  public static void init(BarrageManager manager) {
    rand = new P47Rand();
    barrageManager = manager;
    idCnt = 0;
  }

  public static void clearIsExistList() {
    for (int index3 = 0; index3 < idCnt; index3++)
      isExist[index3] = false;
  }

  public EnemyType() {
    for (int index4 = 0; index4 < BODY_SHAPE_POINT_NUM; index4++) {
      bodyShapePos[index4] = new Vector();
    }
    collisionSize = new Vector();
    for (int index5 = 0; index5 < BARRAGE_PATTERN_MAX; index5++) {
      barrage[index5] = new Barrage();
    }
    for (int index6 = 0; index6 < BATTERY_MAX; index6++) {
      batteryType[index6] = new BatteryType();
    }
    id = idCnt;
    idCnt++;
  }

  public static bool[] usedMorphParser = new bool[BarrageManager.BARRAGE_MAX];

  public void setBarrageType(Barrage br, int btn, int mode) {
    br.parser = barrageManager.parser
      [btn]
      [rand.nextInt(barrageManager.parserNum[btn])];
    for (int index7 = 0; index7< BarrageManager.BARRAGE_MAX; index7++)
      usedMorphParser[index7] = false;
    int mpn=0;
    if (mode == ROLL)
      mpn = barrageManager.parserNum[BarrageManager.MORPH];
    else
      mpn = barrageManager.parserNum[BarrageManager.MORPH_LOCK];
    for (int index8 = 0; index8< br.morphParser.Length; index8++) {
      int mi = rand.nextInt(mpn);
      for (int index9 = 0; index9 < mpn; index9++) {
	if (!usedMorphParser[mi])
	  break;
	mi++;
	if (mi >= mpn) mi = 0;
      }
      if (mode == ROLL)
	br.morphParser[index8] = barrageManager.parser[BarrageManager.MORPH][mi];
      else
	br.morphParser[index8] = barrageManager.parser[BarrageManager.MORPH_LOCK][mi];
      usedMorphParser[mi] = true;
    }
    br.morphNum = br.morphParser.Length;
  }

    public const int NORMAL = 0;
  public const int WEAK = 1;
  public const int VERYWEAK = 2;
  public const int MORPHWEAK = 3;

  public void setBarrageRank(Barrage br, float rank, int intense, int mode) {
    if (rank <= 0) {
      br.rank = 0;
      return;
    }
    br.rank = sqrt(rank) / (8 - rand.nextInt(3));
    if (br.rank > 0.8f)
      br.rank = rand.nextFloat(0.2f) + 0.8f;
    rank /= (br.rank + 2);
    if (intense == WEAK)
      br.rank /= 2;
    if (mode == ROLL)
      br.speedRank = sqrt(rank) * (rand.nextFloat(0.2f) + 1);
    else
      br.speedRank = sqrt(rank * 0.66f) * (rand.nextFloat(0.2f) + 0.8f);
    if (br.speedRank < 1)
      br.speedRank = 1;
    if (br.speedRank > 2)
      br.speedRank = sqrt(br.speedRank) + 0.27f;
    br.morphRank = rank / br.speedRank;
    br.morphCnt = 0;
    while (br.morphRank > 1) {
      br.morphCnt++;
      br.morphRank /= 3;
    }
    if (intense == VERYWEAK) {
      br.morphRank /= 2;
      br.morphCnt = integer(br.morphCnt / (1.7f));
    } else if (intense == MORPHWEAK) {
      br.morphRank /= 2;
    } else if (intense == WEAK) {
      br.morphRank /= 1.5f;
    }
  }

  public void setBarrageRankSlow(Barrage br, float rank, int intense, int mode, float slow) {
    setBarrageRank(br, rank, intense, mode);
    br.speedRank *= slow;
  }

  public const int BULLET_SHAPE_NUM = 7;
  public const int BULLET_COLOR_NUM = 4;

  public void setBarrageShape(Barrage br, float size) {

    br.shape = rand.nextInt(BULLET_SHAPE_NUM);
    br.color = rand.nextInt(BULLET_COLOR_NUM);
    br.bulletSize = (1.0f + rand.nextSignedFloat(0.1f)) * size;
  }

  public float er, eg, eb;
  public int ect;

  public void setEnemyColorType() {
    ect = rand.nextInt(3);
  }

  public void createEnemyColor() {
    switch (ect) {
    case 0:
      er = 1;
      eg = rand.nextFloat(0.7f) + 0.3f;
      eb = rand.nextFloat(0.7f) + 0.3f;
      break;
    case 1:
      er = rand.nextFloat(0.7f) + 0.3f;
      eg = 1;
      eb = rand.nextFloat(0.7f) + 0.3f;
      break;
    case 2:
      er = rand.nextFloat(0.7f) + 0.3f;
      eg = rand.nextFloat(0.7f) + 0.3f;
      eb = 1;
      break;
    }
  }

  public static float[][] enemySize = new float[][] { new float[] { 0.3f, 0.3f, 0.3f, 0.1f, 0.1f, 1.0f, 0.4f, 0.6f, 0.9f }, new float[] { 0.4f, 0.2f, 0.4f, 0.1f, 0.15f, 2.2f, 0.2f, 1.6f, 1.0f }, new float[] { 0.6f, 0.3f, 0.5f, 0.1f, 0.2f, 3.0f, 0.3f, 1.4f, 1.2f }, new float[] { 0.9f, 0.3f, 0.7f, 0.2f, 0.25f, 5.0f, 0.6f, 3.0f, 1.5f }, new float[] { 1.2f, 0.2f, 0.9f, 0.1f, 0.3f, 7.0f, 0.8f, 4.5f, 1.5f } };


  public void setEnemyShapeAndWings(int size) {
    createEnemyColor();
    r = er;
    g = eg;
    b = eb;
    float x1 = enemySize[size][0] + rand.nextSignedFloat(enemySize[size][1]);
    float y1 = enemySize[size][2] + rand.nextSignedFloat(enemySize[size][3]);
    float x2 = enemySize[size][0] + rand.nextSignedFloat(enemySize[size][1]);
    float y2 = enemySize[size][2] + rand.nextSignedFloat(enemySize[size][3]);
    bodyShapePos[0].x = -x1;
    bodyShapePos[0].y = y1;
    bodyShapePos[1].x = x1;
    bodyShapePos[1].y = y1;
    bodyShapePos[2].x = x2;
    bodyShapePos[2].y = -y2;
    bodyShapePos[3].x = -x2;
    bodyShapePos[3].y = -y2;
    retroSize = enemySize[size][4];
    switch (size) {
    case SMALL:
    case MIDDLE:
    case MIDDLEBOSS:
      batteryNum = 2;
      break;
    case LARGE:
    case LARGEBOSS:
      batteryNum = 4;
      break;
    }
    float px=0, py=0, mpx=0, mpy=0;
    int bsl=0;
    if (x1 > x2)
      collisionSize.x = x1;
    else
      collisionSize.x = x2;
    if (y1 > y2)
      collisionSize.y = y1;
    else
      collisionSize.y = y2;
    for (int index10 = 0; index10 < batteryNum; index10++) {
      BatteryType bt = batteryType[index10];
      int wrl = 1;
      if (index10 % 2 == 0) {
	px = enemySize[size][5] + rand.nextFloat(enemySize[size][6]);
	if (batteryNum <= 2) {
	  py = rand.nextSignedFloat(enemySize[size][7]);
	} else {
	  if (index10 < 2) {
	    py = rand.nextFloat(enemySize[size][7] / 2) + enemySize[size][7] / 2;
	  } else {
	    py = -rand.nextFloat(enemySize[size][7] / 2) - enemySize[size][7] / 2;
	  }
	}
	float md=0;
	if (rand.nextInt(2) == 0)
	  md = rand.nextFloat(PI / 2) - PI / 4;
	else
	  md = rand.nextFloat(PI / 2) + PI / 4 * 3;
	mpx = px / 2 + sin(md) * (enemySize[size][8] / 2 + rand.nextFloat(enemySize[size][8]/2));
	mpy = py / 2 + cos(md) * (enemySize[size][8] / 2 + rand.nextFloat(enemySize[size][8]/2));
	switch (size) {
	case SMALL:
	case MIDDLE:
	case LARGE:
	  bsl = 1;
	  break;
	case MIDDLEBOSS:
	  bsl = 150 + rand.nextInt(30);
	  break;
	case LARGEBOSS:
	  bsl = 200 + rand.nextInt(50);
	  break;
	}
	createEnemyColor();
	wrl = -1;
	if (!wingCollision) {
	  if (px > collisionSize.x)
	    collisionSize.x = px;
	  float cpy = fabs(py);
	  if (cpy > collisionSize.y)
	    collisionSize.y = cpy;
	  cpy = fabs(mpy);
	  if (cpy > collisionSize.y)
	    collisionSize.y = cpy;
	}
      }
      switch (wrl) {
      case 1:
	bt.wingShapePos[0].x = px / 4 * wrl;
	bt.wingShapePos[0].y = py / 4;
	bt.wingShapePos[1].x = px * wrl;
	bt.wingShapePos[1].y = py;
	bt.wingShapePos[2].x = mpx * wrl;
	bt.wingShapePos[2].y = mpy;
	break;
      case -1:
	bt.wingShapePos[0].x = px / 4 * wrl;
	bt.wingShapePos[0].y = py / 4;
	bt.wingShapePos[1].x = px * wrl;
	bt.wingShapePos[1].y = py;
	bt.wingShapePos[2].x = mpx * wrl;
	bt.wingShapePos[2].y = mpy;
	break;
      }
      bt.collisionPos.x = (px + px / 4) / 2 * wrl;
      bt.collisionPos.y = (py + mpy + py / 4) / 3;
      bt.collisionSize.x = px / 4 * 3 / 2;
      float sy1 = fabs(py - mpy) / 2;
      float sy2 = fabs(py - py / 4) / 2;
      if (sy1 > sy2)
	bt.collisionSize.y = sy1;
      else
	bt.collisionSize.y = sy2;
      bt.r = er;
      bt.g = eg;
      bt.b = eb;
      bt.shield =bsl;
    }
  }

  public void setBattery(float rank, int n, int barrageType, int barrageIntense,
			  int idx, int ptnIdx, float slow, int mode) {
    BatteryType bt = batteryType[idx];
    BatteryType bt2 = batteryType[idx + 1];
    Barrage br = bt.barrage[ptnIdx];
    Barrage br2 = bt2.barrage[ptnIdx];
    setBarrageType(br, barrageType, mode);
    setBarrageRankSlow(br, rank / n, barrageIntense, mode, slow);
    setBarrageShape(br, 0.8f);
    br.xReverse = rand.nextInt(2) * 2 - 1;
    br2.parser = br.parser;
    for (int index11 = 0; index11 < P47Bullet.MORPH_MAX; index11++) {
      br2.morphParser[index11] = br.morphParser[index11];
    }
    br2.morphNum = br.morphNum;
    br2.morphCnt = br.morphCnt;
    br2.rank = br.rank;
    br2.speedRank = br.speedRank;
    br2.morphRank = br.morphRank;
    br2.shape = br.shape;
    br2.color = br.color;
    br2.bulletSize = br.bulletSize;
    br2.xReverse = -br.xReverse;
    if (rand.nextInt(4) == 0)
      {bt2.xReverseAlternate = true;      bt.xReverseAlternate = bt2.xReverseAlternate ;}
    else
      {bt2.xReverseAlternate = false;      bt.xReverseAlternate = bt2.xReverseAlternate ;}
    float px = bt.wingShapePos[1].x, py = bt.wingShapePos[1].y;
    float mpx = bt.wingShapePos[2].x, mpy = bt.wingShapePos[2].y;
    for (int index12 = 0; index12 < n; index12++) {
      bt.batteryPos[index12].x = px;
      bt.batteryPos[index12].y = py;
      bt2.batteryPos[index12].x = -px;
      bt2.batteryPos[index12].y = py;
      px += (mpx - px) / (n - 1);
      py += (mpy - py) / (n - 1);
    }
    {bt2.batteryNum = n;    bt.batteryNum = bt2.batteryNum ;}
  }

  public void setSmallEnemyType(float rank, int mode) {
    type = SMALL;
    barragePatternNum = 1;
    wingCollision = false;
    setEnemyColorType();
    Barrage br = barrage[0];
    if (mode == ROLL)
      setBarrageType(br, BarrageManager.SMALL, mode);
    else
      setBarrageType(br, BarrageManager.SMALL_LOCK, mode);
    setBarrageRank(br, rank, VERYWEAK, mode);
    setBarrageShape(br, 0.7f);
    br.xReverse = rand.nextInt(2) * 2 - 1;
    setEnemyShapeAndWings(SMALL);
    setBattery(0, 0, 0, NORMAL, 0, 0, 1, mode);
    shield = 1;
    fireInterval = 99999;
    firePeriod = 150 + rand.nextInt(40);
    if (rank < 10)
      firePeriod = integer(firePeriod / ((2 - rank * 0.1f)));
  }

  public void setMiddleEnemyType(float rank, int mode) {
    type = MIDDLE;
    barragePatternNum = 1;
    wingCollision = false;
    setEnemyColorType();
    Barrage br = barrage[0];
    setBarrageType(br, BarrageManager.MIDDLE, mode);
    float cr=0, sr=0;
    if (mode == ROLL) {
      switch (rand.nextInt(6)) {
      case 0:
      case 1:
	cr = rank / 3 * 2;
	sr = 0;
	break;
      case 2:
	cr = rank / 4;
	sr = rank / 4;
	break;
      case 3:
      case 4:
      case 5:
	cr = 0;
	sr = rank / 2;
	break;
      }
    } else {
      switch (rand.nextInt(6)) {
      case 0:
      case 1:
	cr = rank / 5;
	sr = rank / 4;
	break;
      case 2:
      case 3:
      case 4:
      case 5:
	cr = 0;
	sr = rank / 2;
	break;
      }
    }
    setBarrageRank(br, cr, MORPHWEAK, mode);
    setBarrageShape(br, 0.75f);
    br.xReverse = rand.nextInt(2) * 2 - 1;
    setEnemyShapeAndWings(MIDDLE);
    if (mode == ROLL) {
      shield = 40 + rand.nextInt(10);
      setBattery(sr, 1, BarrageManager.MIDDLESUB, NORMAL, 0, 0, 1, mode);
      fireInterval = 100 + rand.nextInt(60);
      firePeriod = GameMath.integer(fireInterval / (1.8f + rand.nextFloat(0.7f)));
    } else {
      shield = 30 + rand.nextInt(8);
      setBattery(sr, 1, BarrageManager.MIDDLESUB_LOCK, NORMAL, 0, 0, 1, mode);
      fireInterval = 72 + rand.nextInt(30);
      firePeriod = GameMath.integer(fireInterval / (1.2f + rand.nextFloat(0.2f)));
    }
    if (rank < 10)
      firePeriod = integer(firePeriod / ((2 - rank * 0.1f)));
  }

  public void setLargeEnemyType(float rank, int mode) {
    type = LARGE;
    barragePatternNum = 1;
    wingCollision = false;
    setEnemyColorType();
    Barrage br = barrage[0];
    setBarrageType(br, BarrageManager.LARGE, mode);
    float cr=0, sr1=0, sr2=0;
    if (mode == ROLL) {
      switch (rand.nextInt(9)) {
      case 0:
      case 1:
      case 2:
      case 3:
	cr = rank;
	{sr2 = 0;	sr1 = sr2 ;}
	break;
      case 4:
	cr = rank / 3 * 2;
	sr1 = rank / 3 * 2;
	sr2 = 0;
	break;
      case 5:
	cr = rank / 3 * 2;
	sr1 = 0;
	sr2 = rank / 3 * 2;
	break;
      case 6:
      case 7:
      case 8:
	cr = 0;
	sr1 = rank / 3 * 2;
	sr2 = rank / 3 * 2;
	break;
      }
    } else {
      switch (rand.nextInt(9)) {
      case 0:
	cr = rank / 4 * 3;
	{sr2 = 0;	sr1 = sr2 ;}
	break;
      case 1:
      case 2:
	cr = rank / 4 * 2;
	sr1 = rank / 3 * 2;
	sr2 = 0;
	break;
      case 3:
      case 4:
	cr = rank / 4 * 2;
	sr1 = 0;
	sr2 = rank / 3 * 2;
	break;
      case 5:
      case 6:
      case 7:
      case 8:
	cr = 0;
	sr1 = rank / 3 * 2;
	sr2 = rank / 3 * 2;
	break;
      }
    }
    setBarrageRank(br, cr, WEAK, mode);
    setBarrageShape(br, 0.8f);
    br.xReverse = rand.nextInt(2) * 2 - 1;
    setEnemyShapeAndWings(LARGE);
    if (mode == ROLL) {
      shield = 60 + rand.nextInt(10);
      setBattery(sr1, 1, BarrageManager.MIDDLESUB, NORMAL, 0, 0, 1, mode);
      setBattery(sr2, 1, BarrageManager.MIDDLESUB, NORMAL, 2, 0, 1, mode);
      fireInterval = 150 + rand.nextInt(60);
      firePeriod = GameMath.integer(fireInterval / (1.3f + rand.nextFloat(0.8f)));
    } else {
      shield = 45 + rand.nextInt(8);
      setBattery(sr1, 1, BarrageManager.MIDDLESUB_LOCK, NORMAL, 0, 0, 1, mode);
      setBattery(sr2, 1, BarrageManager.MIDDLESUB_LOCK, NORMAL, 2, 0, 1, mode);
      fireInterval = 100 + rand.nextInt(50);
      firePeriod = GameMath.integer(fireInterval / (1.2f + rand.nextFloat(0.2f)));
    }
    if (rank < 10)
      firePeriod = integer(firePeriod / ((2 - rank * 0.1f)));
  }

  public void setMiddleBossEnemyType(float rank, int mode) {
    type = MIDDLEBOSS;
    barragePatternNum = 2 + rand.nextInt(2);
    wingCollision = true;
    setEnemyColorType();
    int bn = 1 + rand.nextInt(2);
    for (int index13 = 0; index13 < barragePatternNum; index13++) {
      Barrage br = barrage[index13];
      setBarrageType(br, BarrageManager.LARGE, mode);
      float cr=0, sr=0;
      switch (rand.nextInt(3)) {
      case 0:
	cr = rank;
	sr = 0;
	break;
      case 1:
	cr = rank / 3;
	sr = rank / 3;
	break;
      case 2:
	cr = 0;
	sr = rank;
	break;
      }
      setBarrageRankSlow(br, cr, NORMAL, mode, 0.9f);
      setBarrageShape(br, 0.9f);
      br.xReverse = rand.nextInt(2) * 2 - 1;
      setEnemyShapeAndWings(MIDDLEBOSS);
      setBattery(sr, bn, BarrageManager.MIDDLE, WEAK, 0, index13, 0.9f, mode);
    }
    shield = 300 + rand.nextInt(50);
    fireInterval = 200 + rand.nextInt(40);
    firePeriod = GameMath.integer(fireInterval / (1.2f + rand.nextFloat(0.4f)));
    if (rank < 10)
      firePeriod = integer(firePeriod / ((2 - rank * 0.1f)));
  }

  public void setLargeBossEnemyType(float rank, int mode) {
    type = LARGEBOSS;
    barragePatternNum = 2 + rand.nextInt(3);
    wingCollision = true;
    setEnemyColorType();
    int bn1 = 1 + rand.nextInt(3);
    int bn2 = 1 + rand.nextInt(3);
    for (int index14 = 0; index14 < barragePatternNum; index14++) {
      Barrage br = barrage[index14];
      setBarrageType(br, BarrageManager.LARGE, mode);
      float cr=0, sr1=0, sr2=0;
      switch (rand.nextInt(3)) {
      case 0:
	cr = rank;
	{sr2 = 0;	sr1 = sr2 ;}
	break;
      case 1:
	cr = rank / 3;
	sr1 = rank / 3;
	sr2 = 0;
	break;
      case 2:
	cr = rank / 3;
	sr1 = 0;
	sr2 = rank / 3;
	break;
      }
      setBarrageRankSlow(br, cr, NORMAL, mode, 0.9f);
      setBarrageShape(br, 1.0f);
      br.xReverse = rand.nextInt(2) * 2 - 1;
      setEnemyShapeAndWings(LARGEBOSS);
      setBattery(sr1, bn1, BarrageManager.MIDDLE, NORMAL, 0, index14, 0.9f, mode);
      setBattery(sr2, bn2, BarrageManager.MIDDLE, NORMAL, 2, index14, 0.9f, mode);
    }
    shield = 400 + rand.nextInt(50);
    fireInterval = 220 + rand.nextInt(60);
    firePeriod = GameMath.integer(fireInterval / (1.2f + rand.nextFloat(0.3f)));
    if (rank < 10)
      firePeriod = integer(firePeriod / ((2 - rank * 0.1f)));
  }
}
