// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
public class StageManager {

    public const int TOP = 0;
  public const int SIDE = 1;
  public const int BACK = 2;

    public const int ONE_SIDE = 0;
  public const int ALTERNATE = 1;
  public const int BOTH_SIDES = 2;

    public const int RANDOM = 0;
  public const int FIXED = 1;

    public const int SMALL = 0;
  public const int MIDDLE = 1;
  public const int LARGE = 2;



  public const int STAGE_TYPE_NUM = 4;
  public int parsec;
  public bool bossSection;

  public P47Rand rand;
  public P47GameManager gameManager;
  public BarrageManager barrageManager;
  public Field field;
  public const int SIMULTANEOUS_APPEARNCE_MAX = 4;
  public EnemyAppearance[] appearance = new EnemyAppearance[SIMULTANEOUS_APPEARNCE_MAX];
  public const int SMALL_ENEMY_TYPE_MAX = 3;
  public EnemyType[] smallType = new EnemyType[SMALL_ENEMY_TYPE_MAX];
  public const int MIDDLE_ENEMY_TYPE_MAX = 4;
  public EnemyType[] middleType = new EnemyType[MIDDLE_ENEMY_TYPE_MAX];
  public const int LARGE_ENEMY_TYPE_MAX = 2;
  public EnemyType[] largeType = new EnemyType[LARGE_ENEMY_TYPE_MAX];
  public EnemyType middleBossType;
  public EnemyType largeBossType;
  public int apNum;
  public Vector apos;
  public int sectionCnt, sectionIntervalCnt, section;
  public float rank, rankInc;
  public int middleRushSectionNum;
  public bool middleRushSection;
  public int stageType;

  public void init(P47GameManager gm, BarrageManager bm, Field f) {
    gameManager = gm;
    barrageManager = bm;
    field = f;
    rand = new P47Rand();
    for (int index0 = 0; index0 < SIMULTANEOUS_APPEARNCE_MAX; index0++) appearance[index0] = new EnemyAppearance();
    apos = new Vector();
    for (int index1 = 0; index1 < SMALL_ENEMY_TYPE_MAX; index1++)
      smallType[index1] = new EnemyType();
    for (int index2 = 0; index2 < MIDDLE_ENEMY_TYPE_MAX; index2++)
      middleType[index2] = new EnemyType();
    for (int index3 = 0; index3 < LARGE_ENEMY_TYPE_MAX; index3++)
      largeType[index3] = new EnemyType();
    middleBossType = new EnemyType();
    largeBossType = new EnemyType();
  }

  public void createEnemyData() {
    for (int index4 = 0; index4 < SMALL_ENEMY_TYPE_MAX; index4++)
      smallType[index4].setSmallEnemyType(rank, gameManager.mode);
    for (int index5 = 0; index5 < MIDDLE_ENEMY_TYPE_MAX; index5++)
      middleType[index5].setMiddleEnemyType(rank, gameManager.mode);
    for (int index6 = 0; index6 < LARGE_ENEMY_TYPE_MAX; index6++)
      largeType[index6].setLargeEnemyType(rank, gameManager.mode);
    middleBossType.setMiddleBossEnemyType(rank, gameManager.mode);
    largeBossType.setLargeBossEnemyType(rank, gameManager.mode);
  }

  public void setAppearancePattern(EnemyAppearance ap) {
    switch (rand.nextInt(5)) {
    case 0:
      ap.pattern = ONE_SIDE;
      break;
    case 1:
    case 2:
      ap.pattern = ALTERNATE;
      break;
    case 3:
    case 4:
      ap.pattern = BOTH_SIDES;
      break;
    }
    switch (rand.nextInt(3)) {
    case 0:
      ap.sequence = RANDOM;
      break;
    case 1:
    case 2:
      ap.sequence = FIXED;
      break;
    }
  }

  public void setSmallAppearance(EnemyAppearance ap) {
    ap.type = smallType[rand.nextInt(SMALL_ENEMY_TYPE_MAX)];
    int mt=0;
    if (rand.nextFloat(1) > 0.2f) {
      ap.point = TOP;
      mt = BarrageManager.SMALLMOVE;
    } else {
      ap.point = SIDE;
      mt = BarrageManager.SMALLSIDEMOVE;
    }
    ap.moveParser = barrageManager.parser[mt][rand.nextInt(barrageManager.parserNum[mt])];
    setAppearancePattern(ap);
    if (ap.pattern == ONE_SIDE)
      ap.pattern = ALTERNATE;
    switch (rand.nextInt(4)) {
    case 0:
      ap.num = 7 + rand.nextInt(5);
      ap.groupInterval = 72 + rand.nextInt(15);
      ap.interval = 15 + rand.nextInt(5);
      break;
    case 1:
      ap.num = 5 + rand.nextInt(3);
      ap.groupInterval = 56 + rand.nextInt(10);
      ap.interval = 20 + rand.nextInt(5);
      break;
    case 2:
    case 3:
      ap.num = 2 + rand.nextInt(2);
      ap.groupInterval = 45 + rand.nextInt(20);
      ap.interval = 25 + rand.nextInt(5);
      break;
    }
  }

  public void setMiddleAppearance(EnemyAppearance ap) {
    ap.type = middleType[rand.nextInt(MIDDLE_ENEMY_TYPE_MAX)];
    int mt=0;

    ap.point = TOP;
    mt = BarrageManager.MIDDLEMOVE;
    ap.moveParser = barrageManager.parser[mt][rand.nextInt(barrageManager.parserNum[mt])];
    setAppearancePattern(ap);
    switch (rand.nextInt(3)) {
    case 0:
      ap.num = 4;
      ap.groupInterval = 240 + rand.nextInt(150);
      ap.interval = 80 + rand.nextInt(30);
      break;
    case 1:
      ap.num = 2;
      ap.groupInterval = 180 + rand.nextInt(60);
      ap.interval = 180 + rand.nextInt(20);
      break;
    case 2:
      ap.num = 1;
      ap.groupInterval = 150 + rand.nextInt(50);
      ap.interval = 100;
      break;
    }
  }

  public void setLargeAppearance(EnemyAppearance ap) {
    ap.type = largeType[rand.nextInt(LARGE_ENEMY_TYPE_MAX)];
    int mt=0;
    ap.point = TOP;
    mt = BarrageManager.LARGEMOVE;
    ap.moveParser = barrageManager.parser[mt][rand.nextInt(barrageManager.parserNum[mt])];
    setAppearancePattern(ap);
    switch (rand.nextInt(3)) {
    case 0:
      ap.num = 3;
      ap.groupInterval = 400 + rand.nextInt(100);
      ap.interval = 240 + rand.nextInt(40);
      break;
    case 1:
      ap.num = 2;
      ap.groupInterval = 400 + rand.nextInt(60);
      ap.interval = 300 + rand.nextInt(20);
      break;
    case 2:
      ap.num = 1;
      ap.groupInterval = 270 + rand.nextInt(50);
      ap.interval = 200;
      break;
    }
  }

  public void setAppearance(EnemyAppearance ap, int type) {
    switch (type) {
    case SMALL:
      setSmallAppearance(ap);
      break;
    case MIDDLE:
      setMiddleAppearance(ap);
      break;
    case LARGE:
      setLargeAppearance(ap);
      break;
    }
    ap.cnt = 0;
    ap.left = ap.num;
    ap.side = rand.nextInt(2) * 2 - 1;
    ap.pos = rand.nextFloat(1);
  }

  public const int MIDDLE_RUSH_SECTION_PATTERN = 6;
  public static int[][][] apparancePattern = new int[][][] {new int[][] {new int[] {1, 0, 0}, new int[] {2, 0, 0}, new int[] {1, 1, 0}, new int[] {1, 0, 1}, new int[] {2, 1, 0}, new int[] {2, 0, 1}, new int[] {0, 1, 1}}, new int[][] {new int[] {1, 0, 0}, new int[] {1, 1, 0}, new int[] {1, 1, 0}, new int[] {1, 0, 1}, new int[] {2, 1, 0}, new int[] {1, 1, 1}, new int[] {0, 1, 1}}};

  public void createSectionData() {
    apNum = 0;
    if (rank <= 0)
      return;
    field.aimSpeed = 0.1f + section * 0.02f;
    if (section == 4) {

      Vector pos = new Vector();
      pos.x = 0; pos.y = field.size.y / 4 * 3;
      gameManager.addBoss(pos, PI, middleBossType);
      bossSection = true;
      {sectionCnt = 2 * 60;      sectionIntervalCnt = sectionCnt ;}
      field.aimZ = 11;
      return;
    } else if (section == 9) {

      Vector pos = new Vector();
      pos.x = 0; pos.y = field.size.y / 4 * 3;
      gameManager.addBoss(pos, PI, largeBossType);
      bossSection = true;
      {sectionCnt = 3 * 60;      sectionIntervalCnt = sectionCnt ;}
      field.aimZ = 12;
      return;
    } else if (section == middleRushSectionNum) {

      middleRushSection = true;
      field.aimZ = 9;
    } else {
      middleRushSection = false;
      field.aimZ = 10 + rand.nextSignedFloat(0.3f);
    }
    bossSection = false;
    if (section == 3)
      sectionIntervalCnt = 2 * 60;
    else if (section == 3)
      sectionIntervalCnt = 4 * 60;
    else
      sectionIntervalCnt = 1 * 60;
    sectionCnt = sectionIntervalCnt + 10 * 60;
    int sp = GameMath.integer(section * 3 / 7) + 1;
    int ep = 3 + GameMath.integer(section * 3 / 10);
    int ap = sp + rand.nextInt(ep - sp + 1);
    if (section == 0)
      ap = 0;
    else if (middleRushSection)
      ap = MIDDLE_RUSH_SECTION_PATTERN;
    for (int index7 = 0; index7 < apparancePattern[gameManager.mode][ap][0]; index7++, apNum++) {
      EnemyAppearance next = appearance[apNum];
      setAppearance(next, SMALL);
    }
    for (int index8 = 0; index8 < apparancePattern[gameManager.mode][ap][1]; index8++, apNum++) {
      EnemyAppearance next = appearance[apNum];
      setAppearance(next, MIDDLE);
    }
    for (int index9 = 0; index9 < apparancePattern[gameManager.mode][ap][2]; index9++, apNum++) {
      EnemyAppearance next = appearance[apNum];
      setAppearance(next, LARGE);
    }
  }

  public void createStage() {
    createEnemyData();
    middleRushSectionNum = 2 + rand.nextInt(6);
    if (middleRushSectionNum <= 4)
      middleRushSectionNum++;
    field.setType(stageType % Field.TYPE_NUM);
    SoundManager.playBgm(stageType % SoundManager.BGM_NUM);
    stageType++;
  }

  public void gotoNextSection() {
    section++;
    parsec++;
    if (gameManager.state == P47GameManager.TITLE_STATE && section >= 4) {
      section = 0;
      parsec -= 4;
    }
    if (section >= 10) {
      section = 0;
      rank += rankInc;
      createStage();
    }
    createSectionData();
  }

  public void setRank(float baseRank, float inc, int startParsec, int type) {
    rank = baseRank;
    rankInc = inc;
    rank += rankInc * (GameMath.integer(startParsec / 10));
    section = -1;
    parsec = startParsec - 1;
    stageType = type;
    createStage();
    gotoNextSection();
  }

  public void move() {
    for (int index10 = 0; index10 < apNum; index10++) {
      EnemyAppearance ap = appearance[index10];
      ap.cnt--;
      if (ap.cnt > 0) {

	if (!middleRushSection) {
	  if (ap.type.type == EnemyType.SMALL && !EnemyType.isExist[ap.type.id]) {
	    ap.cnt = 0;
	    EnemyType.isExist[ap.type.id] = true;
	  }
	} else {
	  if (ap.type.type == EnemyType.MIDDLE && !EnemyType.isExist[ap.type.id]) {
	    ap.cnt = 0;
	    EnemyType.isExist[ap.type.id] = true;
	  }
	}
	continue;
      }
      float p=0;
      switch (ap.sequence) {
      case RANDOM:
	p = rand.nextFloat(1);
	break;
      case FIXED:
	p = ap.pos;
	break;
      }
      float d=0;
      switch (ap.point) {
      case TOP:
	switch (ap.pattern) {
	case BOTH_SIDES:
	  apos.x = (p - 0.5f) * field.size.x * 1.8f;
	  break;
	default:
	  apos.x = (p * 0.6f + 0.2f) * field.size.x * ap.side;
	  break;
	}
	apos.y = field.size.y - Enemy.FIELD_SPACE;
	d = PI;
	break;
      case BACK:
	switch (ap.pattern) {
	case BOTH_SIDES:
	  apos.x = (p - 0.5f) * field.size.x * 1.8f;
	  break;
	default:
	  apos.x = (p * 0.6f + 0.2f) * field.size.x * ap.side;
	  break;
	}
	apos.y = -field.size.y + Enemy.FIELD_SPACE;
	d = 0;
	break;
      case SIDE:
	switch (ap.pattern) {
	case BOTH_SIDES:
	  apos.x = (field.size.x - Enemy.FIELD_SPACE) * (rand.nextInt(2) * 2 - 1);
	  break;
	default:
	  apos.x = (field.size.x - Enemy.FIELD_SPACE) * ap.side;
	  break;
	}
	apos.y = (p * 0.4f + 0.4f) * field.size.y;
	if (apos.x < 0)
	  d = PI / 2;
	else
	  d = PI / 2 * 3;
	break;
      }
      apos.x *= 0.88f;
      gameManager.addEnemy(apos, d, ap.type, ap.moveParser);
      ap.left--;
      if (ap.left <= 0) {
	ap.cnt = ap.groupInterval;
	ap.left = ap.num;
	if (ap.pattern != ONE_SIDE)
	  ap.side *= -1;
	ap.pos = rand.nextFloat(1);
      } else {
	ap.cnt = ap.interval;
      }
    }
    if (!bossSection ||
	(!EnemyType.isExist[middleBossType.id] && !EnemyType.isExist[largeBossType.id]))
      sectionCnt--;
    if (sectionCnt < sectionIntervalCnt) {
      if (section == 9 && sectionCnt == sectionIntervalCnt - 1)
	Sound.fadeMusic();
      apNum = 0;
      if (sectionCnt <= 0)
	gotoNextSection();
    }
    EnemyType.clearIsExistList();
  }
}

public class EnemyAppearance {

    public EnemyType type;
    public int moveParser;
    public int point, pattern, sequence;
    public float pos;
    public int num, interval, groupInterval;
    public int cnt, left, side;
  }
