// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Enemy: Actor {



  public const float FIELD_SPACE = 0.5f;
  public Vector pos;
  public EnemyType type;
  public Battery[] battery = new Battery[EnemyType.BATTERY_MAX];
  public int shield;

  public const int MOVE_POINT_MAX = 8;
  public static P47Rand rand = new P47Rand();
  public Field field;
  public BulletActorPool bullets;
  public ActorPool shots;
  public ActorPool rolls;
  public ActorPool targetLocks;
  public Ship ship;
  public P47GameManager manager;
  public int cnt;
  public BulletActor topBullet;
  public BulletActor moveBullet;
  public Vector[] movePoint = new Vector[MOVE_POINT_MAX];
  public int movePointNum, movePointIdx;
  public float speed, deg;
  public bool onRoute;
  public float baseDeg;
  public int fireCnt, barragePatternIdx;
  public float fieldLimitX, fieldLimitY;
  public const int APPEARANCE_CNT = 90;
  public const float APPEARANCE_Z = -15;
  public const int DESTROYED_CNT = 90;
  public const float DESTROYED_Z = -10;
  public const int TIMEOUT_LIMIT = 90;
  public const int BOSS_TIMEOUT = 30 * 60;
  public int appCnt, dstCnt, timeoutCnt;
  public float z;
  public bool isBoss;
  public Vector vel;
  public int velCnt;
  public bool damaged;
  public int bossTimer;



  public override Actor newActor() {
    return new Enemy();
  }

  public override void init(ActorInitializer ini) {
    for (int index0 = 0; index0 < EnemyType.BATTERY_MAX; index0++) battery[index0] = new Battery();
    EnemyInitializer ei = (EnemyInitializer) ini;
    field = ei.field;
    bullets = ei.bullets;
    shots = ei.shots;
    rolls = ei.rolls;
    targetLocks = ei.targetLocks;
    ship = ei.ship;
    manager = ei.manager;
    pos = new Vector();
    for (int index1 = 0; index1 < MOVE_POINT_MAX; index1++) {
      movePoint[index1] = new Vector();
    }
    vel = new Vector();
    velCnt = 0;
    fieldLimitX = field.size.x / 4 * 3;
    fieldLimitY = field.size.y / 4 * 3;
  }

  public void set(Vector p, float d, EnemyType type, int moveParser) {
    pos.x = p.x;
    pos.y = p.y;
    this.type = type;
    PatternState[] moveRunner = P47Bullet.createRunner(moveParser);
    moveBullet = bullets.addBullet_11(moveRunner,
				   pos.x, pos.y, d, 0, 0.5f,
				   1, 0, 0, 1, 1);
    if (moveBullet == null)
      return;
    cnt = 0;
    shield = type.shield;
    for (int index2 = 0; index2 < type.batteryNum; index2++) {
      battery[index2].shield = type.batteryType[index2].shield;
    }
    fireCnt = 0; barragePatternIdx = 0;
    baseDeg = d;
    {{timeoutCnt = 0;dstCnt = timeoutCnt ;}    appCnt = dstCnt ;}
    z = 0;
    isBoss = false;
    isExist = true;
  }

  public void setBoss(Vector p, float d, EnemyType type) {
    pos.x = p.x;
    pos.y = p.y;
    this.type = type;
    moveBullet = null;

    float wx = rand.nextFloat(field.size.x / 4) + field.size.x / 4;
    float wy = rand.nextFloat(field.size.y / 9) + field.size.y / 7;
    float cy = field.size.y / 7 * 4;
    movePointNum = rand.nextInt(3) + 2;
    for (int index3 = 0; index3 < GameMath.integer(movePointNum / 2); index3++) {
      movePoint[index3 * 2].x = rand.nextFloat(wx / 2) + wx / 2;
      movePoint[index3 * 2 + 1].x = -movePoint[index3 * 2].x;
      {movePoint[index3 * 2 + 1].y = rand.nextSignedFloat(wy) + cy;      movePoint[index3 * 2].y = movePoint[index3 * 2 + 1].y ;}
    }
    if (movePointNum == 3) {
      movePoint[2].x = 0;
      movePoint[2].y = rand.nextSignedFloat(wy) + cy;
    }
    for (int index4 = 0; index4 < 8; index4++) {
      int idx1 = rand.nextInt(movePointNum);
      int idx2 = rand.nextInt(movePointNum);
      if (idx1 == idx2) {
	idx2++;
	if (idx2 >= movePointNum) idx2 = 0;
      }
      Vector mp = movePoint[idx1];
      movePoint[idx1] = movePoint[idx2];
      movePoint[idx2] = mp;
    }
    speed = 0.03f + rand.nextFloat(0.02f);
    movePointIdx = 0;
    deg = PI;
    onRoute = false;

    cnt = 0;
    shield = type.shield;
    for (int index5 = 0; index5 < type.batteryNum; index5++) {
      battery[index5].shield = type.batteryType[index5].shield;
    }
    for (int index6 = type.batteryNum; index6 < EnemyType.BATTERY_MAX; index6++) {
      battery[index6].shield = 0;
    }
    fireCnt = 0; barragePatternIdx = 0;
    baseDeg = d;
    appCnt = APPEARANCE_CNT;
    z = APPEARANCE_Z;
    {timeoutCnt = 0;    dstCnt = timeoutCnt ;}
    isBoss = true;
    bossTimer = 0;
    isExist = true;
  }

  public BulletActor setBullet_3(Barrage br, Vector ofs, float xr) {
    if (br.rank <= 0)
      return null;
    PatternState[] runner = P47Bullet.createRunner(br.parser);
    BulletActor ba=null;
    float bx = pos.x, by = pos.y;
    if (ofs != null) {
      bx += ofs.x;
      by += ofs.y;
    }
    if (br.morphCnt > 0)
      ba = bullets.addBullet_15
	(br.parser, runner,
	 bx, by, baseDeg, 0, br.rank,
	 br.speedRank,
	 br.shape, br.color, br.bulletSize,
	 br.xReverse * xr,
	 br.morphParser, br.morphNum, br.morphCnt);
    else
      ba = bullets.addBullet_12
	(br.parser, runner,
	 bx, by, baseDeg, 0, br.rank,
	 br.speedRank,
	 br.shape, br.color, br.bulletSize,
	 br.xReverse * xr);
    return ba;
  }

  public BulletActor setBullet_2(Barrage br, Vector ofs) {
    return setBullet_3(br, ofs, 1);
  }

  public void setTopBullets() {
    topBullet = setBullet_2(type.barrage[barragePatternIdx], null);
    for (int index7 = 0; index7 < type.batteryNum; index7++) {
      Battery b = battery[index7];
      if (b.shield <= 0)
	continue;
      BatteryType bt = type.batteryType[index7];
      float xr = 1;
      for (int index8 = 0; index8 < bt.batteryNum; index8++) {
	b.topBullet[index8] = setBullet_3(bt.barrage[barragePatternIdx], bt.batteryPos[index8], xr);
	if (bt.xReverseAlternate)
	  xr *= -1;
      }
    }
  }

  public void addBonuses_2(Vector p, int sl) {
    int bn = GameMath.integer((float) sl * 3 / (((float) cnt / 30) + 1) * Bonus.rate + 0.9f);
    manager.addBonus(pos, p, bn);
  }

  public void addBonuses_0() {
    addBonuses_2(null, type.shield);
  }

  public void addWingFragments(BatteryType bt, int n, float z, float speed, float deg) {
    int ni = 1;
    for (int index9 = 0; index9 < BatteryType.WING_SHAPE_POINT_NUM; index9++, ni++) {
      if (ni >= BatteryType.WING_SHAPE_POINT_NUM)
	ni = 0;
      manager.addFragments
	(n,
	 pos.x + bt.wingShapePos[index9].x, pos.y + bt.wingShapePos[index9].y,
	 pos.x + bt.wingShapePos[ni].x, pos.y + bt.wingShapePos[ni].y,
	 z, speed, deg);
    }
  }

  public void addFragments(int n, float z, float speed, float deg) {
    int ni = 1;
    for (int index10 = 0; index10 < EnemyType.BODY_SHAPE_POINT_NUM; index10++, ni++) {
      if (ni >= EnemyType.BODY_SHAPE_POINT_NUM)
	ni = 0;
      manager.addFragments
	(n,
	 pos.x + type.bodyShapePos[index10].x, pos.y + type.bodyShapePos[index10].y,
	 pos.x + type.bodyShapePos[ni].x, pos.y + type.bodyShapePos[ni].y,
	 z, speed, deg);
    }
    for (int index11 = 0; index11 < type.batteryNum; index11++) {
      if (battery[index11].shield > 0)
	addWingFragments(type.batteryType[index11], n, z, speed, deg);
    }
  }

  public static int SHOT_DAMAGE = 1;
  public static int ROLL_DAMAGE = 1;
  public static int LOCK_DAMAGE = 7;
  public static int[] ENEMY_TYPE_SCORE = new int[] {100, 500, 1000, 5000, 10000};
  public const int ENEMY_WING_SCORE = 1000;

  public void addDamage(int dmg) {
    shield -= dmg;
    if (shield <= 0) {

      addBonuses_0();
      manager.addScore(ENEMY_TYPE_SCORE[type.type]);
      if (isBoss) {
	addFragments(15, 0, 0.1f, rand.nextSignedFloat(1));
	SoundManager.playSe(SoundManager.BOSS_DESTROYED);
	manager.setScreenShake(20, 0.05f);
	manager.clearBullets();
	removeTopBullets();
	dstCnt = DESTROYED_CNT;
      } else {
	float d=0;
	if (type.type == EnemyType.SMALL) {
	  d = moveBullet.bullet.deg;
	  SoundManager.playSe(SoundManager.ENEMY_DESTROYED);
	} else {
	  d = rand.nextSignedFloat(1);
	  SoundManager.playSe(SoundManager.LARGE_ENEMY_DESTROYED);
	}
	addFragments(type.type * 4 + 2, 0, 0.04f, d);
	remove();
      }
    }
    damaged = true;
  }

  public void removeBattery(Battery b, BatteryType bt) {
    for (int index12 = 0; index12 < bt.batteryNum; index12++) {
      if ((b.topBullet[index12])!=null) {
	b.topBullet[index12].remove();
	b.topBullet[index12] = null;
      }
    }
    b.damaged = true;
  }

  public void addDamageBattery(int idx, int dmg) {
    battery[idx].shield -= dmg;
    if (battery[idx].shield <= 0) {

      Vector p = type.batteryType[idx].collisionPos;
      addBonuses_2(p, type.batteryType[idx].shield);
      manager.addScore(ENEMY_WING_SCORE);
      addWingFragments(type.batteryType[idx], 10, 0, 0.1f, rand.nextSignedFloat(1));
      SoundManager.playSe(SoundManager.LARGE_ENEMY_DESTROYED);
      manager.setScreenShake(10, 0.03f);
      removeBattery(battery[idx], type.batteryType[idx]);
      vel.x = -p.x / 10;
      vel.y = -p.y / 10;
      velCnt = 60;
      removeTopBullets();
      fireCnt = velCnt + 10;
    }
  }

    public const int NOHIT = -2;
  public const int HIT = -1;

  public int checkHit(Vector p, float xofs, float yofs) {
    if (fabs(p.x - pos.x) < type.collisionSize.x + xofs &&
	fabs(p.y - pos.y) < type.collisionSize.y + yofs)
      return HIT;
    if (type.wingCollision) {
      for (int index13 = 0; index13 < type.batteryNum; index13++) {
	if (battery[index13].shield <= 0)
	  continue;
	BatteryType bt = type.batteryType[index13];
	if (fabs(p.x - pos.x - bt.collisionPos.x) < bt.collisionSize.x + xofs &&
	    fabs(p.y - pos.y - bt.collisionPos.y) < bt.collisionSize.y + yofs)
	  return index13;
      }
    }
    return NOHIT;
  }

  public int checkLocked(Vector p, float xofs, Lock targetLock) {
    if (fabs(p.x - pos.x) < type.collisionSize.x + xofs && pos.y < targetLock.targetLockMinY && pos.y > p.y) {
      targetLock.targetLockMinY = pos.y;
      return HIT;
    }
    if (type.wingCollision) {
      int lp = NOHIT;
      for (int index14 = 0; index14 < type.batteryNum; index14++) {
	if (battery[index14].shield <= 0)
	  continue;
	BatteryType bt = type.batteryType[index14];
	float by = pos.y + bt.collisionPos.y;
	if (fabs(p.x - pos.x - bt.collisionPos.x) < bt.collisionSize.x + xofs &&
	    by < targetLock.targetLockMinY && by > p.y) {
	  targetLock.targetLockMinY = by;
	  lp = index14;
	}
      }
      if (lp != NOHIT)
	return lp;
    }
    return NOHIT;
  }

  public void checkDamage() {
    int ch=0;

    for (int index15 = 0; index15 < shots.actor.Length; index15++) {
      if (!shots.actor[index15].isExist)
	continue;
      Vector sp = ((Shot) shots.actor[index15]).pos;
      ch = checkHit(sp, 0.7f, 0);
      if (ch >= HIT) {
	manager.addParticle(sp, rand.nextSignedFloat(0.3f), 0, Shot.SPEED / 4);
	manager.addParticle(sp, rand.nextSignedFloat(0.3f), 0, Shot.SPEED / 4);
	manager.addParticle(sp, PI + rand.nextSignedFloat(0.3f), 0, Shot.SPEED / 7);
	shots.actor[index15].isExist = false;
	if (ch == HIT)
	  addDamage(SHOT_DAMAGE);
	else
	  addDamageBattery(ch, SHOT_DAMAGE);
      }
    }
    if (manager.mode == P47GameManager.ROLL) {

      for (int index16 = 0; index16 < rolls.actor.Length; index16++) {
	if (!rolls.actor[index16].isExist)
	  continue;
	Roll rl = (Roll) rolls.actor[index16];
	ch = checkHit(rl.pos[0], 1.0f, 1.0f);
	if (ch >= HIT) {
	  for (int index17 = 0; index17 < 4; index17++)
	    manager.addParticle(rl.pos[0], rand.nextFloat(PI * 2), 0, Shot.SPEED / 10);
	  float rd = ROLL_DAMAGE;
	  if (rl.released) {
	    rd += rd;
	  } else {
	    if (rl.cnt < Roll.NO_COLLISION_CNT)
	      continue;
	  }
	  if (ch == HIT)
	    addDamage(integer(rd));
	  else
	    addDamageBattery(ch, integer(rd));
	}
      }
    } else if (type.type != EnemyType.SMALL) {

      for (int index18 = 0; index18 < targetLocks.actor.Length; index18++) {
	if (!targetLocks.actor[index18].isExist)
	  continue;
	Lock lk = (Lock) targetLocks.actor[index18];
	if (lk.state == Lock.SEARCH || lk.state == Lock.SEARCHED) {
	  ch = checkLocked(lk.pos[0], 2.5f, lk);
	  if (ch >= HIT) {
	    lk.state = Lock.SEARCHED;
	    lk.targetLockedEnemy = this;
	    lk.targetLockedPart = ch;
	  }
	  return;
	} else if (lk.state == Lock.FIRED && lk.targetLockedEnemy == this) {
	  ch = checkHit(lk.pos[0], 1.5f, 1.5f);
	  if (ch >= HIT && ch == lk.targetLockedPart) {
	    for (int index19 = 0; index19 < 4; index19++)
	      manager.addParticle(lk.pos[0], rand.nextFloat(PI * 2), 0, Shot.SPEED / 10);
	    if (ch == HIT)
	      addDamage(LOCK_DAMAGE);
	    else
	      addDamageBattery(ch, LOCK_DAMAGE);
	    lk.hit();
	  }
	}
      }
    }
  }

  public void removeTopBullets() {
    if (topBullet != null) {
      topBullet.remove();
      topBullet = null;
    }
    for (int index20 = 0; index20 < type.batteryNum; index20++) {
      BatteryType bt = type.batteryType[index20];
      Battery b = battery[index20];
      for (int index21 = 0; index21 < bt.batteryNum; index21++) {
	if ((b.topBullet[index21])!=null) {
	  b.topBullet[index21].remove();
	  b.topBullet[index21] = null;
	}
      }
    }
  }

  public void remove() {
    removeTopBullets();
    if (moveBullet != null)
      moveBullet.remove();
    isExist = false;
  }

  public static float BOSS_MOVE_DEG = 0.02f;

  public void gotoNextPoint() {
    onRoute = false;
    movePointIdx++;
    if (movePointIdx >= movePointNum)
      movePointIdx = 0;
  }

  public void moveBoss() {
    Vector aim = movePoint[movePointIdx];
    float d = atan2(aim.x - pos.x, aim.y - pos.y);
    float od = d - deg;
    if (od > PI)
      od -= PI * 2;
    else if (od < -PI)
      od += PI * 2;
    float aod = fabs(od);
    if (aod < BOSS_MOVE_DEG) {
      deg = d;
    } else if (od > 0) {
      deg += BOSS_MOVE_DEG;
      if (deg >= PI * 2)
	deg -= PI * 2;
    } else {
      deg -= BOSS_MOVE_DEG;
      if (deg < 0)
	deg += PI * 2;
    }
    pos.x += sin(deg) * speed;
    pos.y += cos(deg) * speed;
    if (velCnt > 0) {
      velCnt--;
      pos.x += vel.x;
      pos.y += vel.y;
      vel.x *= 0.92f;
      vel.y *= 0.92f;
    }
    if (!onRoute) {
      if (aod < PI / 2) {
	onRoute = true;
      }
    } else {
      if (aod > PI / 2) {
	gotoNextPoint();
      }
    }
    if (pos.x > fieldLimitX) {
      pos.x = fieldLimitX;
      gotoNextPoint();
    } else if (pos.x < -fieldLimitX) {
      pos.x = -fieldLimitX;
      gotoNextPoint();
    }
    if (pos.y > fieldLimitY) {
      pos.y = fieldLimitY;
      gotoNextPoint();
    } else if (pos.y < fieldLimitY / 4) {
      pos.y = fieldLimitY / 4;
      gotoNextPoint();
    }
  }

  public void controlFireCnt() {
    if (fireCnt <= 0) {
      setTopBullets();
      fireCnt = type.fireInterval;
      barragePatternIdx++;
      if (barragePatternIdx >= type.barragePatternNum)
	barragePatternIdx = 0;
    } else if (fireCnt < type.fireInterval - type.firePeriod) {
      removeTopBullets();
    }
    fireCnt--;
  }

  public override void move() {
    EnemyType.isExist[type.id] = true;
    if (!isBoss) {
      pos.x = moveBullet.bullet.pos.x;
      pos.y = moveBullet.bullet.pos.y;
    } else {
      moveBoss();
    }
    if (topBullet != null) {
      topBullet.bullet.pos.x = pos.x;
      topBullet.bullet.pos.y = pos.y;
    }
    damaged = false;
    for (int index22 = 0; index22 < type.batteryNum; index22++) {
      BatteryType bt = type.batteryType[index22];
      Battery b = battery[index22];
      b.damaged = false;
      for (int index23 = 0; index23 < bt.batteryNum; index23++) {
	if ((b.topBullet[index23])!=null) {
	  b.topBullet[index23].bullet.pos.x = pos.x + bt.batteryPos[index23].x;
	  b.topBullet[index23].bullet.pos.y = pos.y + bt.batteryPos[index23].y;
	}
      }
    }
    if (!isBoss) {
      if (field.checkHit_1(pos)) {
	remove();
	return;
      }
      if (pos.y < -field.size.y / 4) {
	removeTopBullets();
      } else {
	controlFireCnt();
      }
    } else {
      float mtr=0;
      if (appCnt > 0) {
	if (z < 0)
	  z -= APPEARANCE_Z / 60;
	appCnt--;
	mtr = 1.0f - (float)appCnt / APPEARANCE_CNT;
      } else if (dstCnt > 0) {
	addFragments(1, z, 0.05f, rand.nextSignedFloat(PI));
	manager.clearBullets();
	z += DESTROYED_Z / 60;
	dstCnt--;
	if (dstCnt <= 0) {
	  addFragments(25, z, 0.4f, rand.nextSignedFloat(PI));
	  SoundManager.playSe(SoundManager.BOSS_DESTROYED);
	  manager.setScreenShake(60, 0.01f);
	  remove();
	  manager.setBossShieldMeter(0, 0, 0, 0, 0, 0);
	  return;
	}
	mtr = (float)dstCnt / DESTROYED_CNT;
      } else if (timeoutCnt > 0) {
	z += DESTROYED_Z / 60;
	timeoutCnt--;
	if (timeoutCnt <= 0) {
	  remove();
	  return;
	}
	mtr = 0;
      } else {
	controlFireCnt();
	mtr = 1;
	bossTimer++;
	if (bossTimer > BOSS_TIMEOUT) {
	  timeoutCnt = TIMEOUT_LIMIT;
	  shield = 0;
	  removeTopBullets();
	}
      }
      manager.setBossShieldMeter
	(shield, battery[0].shield, battery[1].shield, battery[2].shield, battery[3].shield, mtr);
    }
    cnt++;
    if (appCnt <= 0 && dstCnt <= 0 && timeoutCnt <= 0)
      checkDamage();
  }

  public override void draw() {
    float ap=0;
    if (appCnt > 0) {

      P47Screen.setRetroZ(z);
      ap = (float) appCnt / APPEARANCE_CNT;
      P47Screen.setRetroParam(1, type.retroSize * (1 + ap * 10));
      P47Screen.setRetroColor(type.r, type.g, type.b, (1 - ap));
    } else if (dstCnt > 0) {
      P47Screen.setRetroZ(z);
      ap = (float) dstCnt / DESTROYED_CNT / 2 + 0.5f;
      P47Screen.setRetroColor(type.r, type.g, type.b, ap);
    } else if (timeoutCnt > 0) {
      P47Screen.setRetroZ(z);
      ap = (float) timeoutCnt / TIMEOUT_LIMIT;
      P47Screen.setRetroColor(type.r, type.g, type.b, ap);
    } else {
      P47Screen.setRetroParam(1, type.retroSize);
      if (!damaged)
	P47Screen.setRetroColor(type.r, type.g, type.b, 1);
      else
	P47Screen.setRetroColor(1, 1, type.b, 1);
    }
    int ni = 1;
    for (int index24 = 0; index24 < EnemyType.BODY_SHAPE_POINT_NUM; index24++, ni++) {
      if (ni >= EnemyType.BODY_SHAPE_POINT_NUM)
	ni = 0;
      P47Screen.drawLineRetro(pos.x + type.bodyShapePos[index24].x, pos.y + type.bodyShapePos[index24].y,
			      pos.x + type.bodyShapePos[ni].x, pos.y + type.bodyShapePos[ni].y);
    }
    if (type.type != EnemyType.SMALL) {
      glBegin(GL_TRIANGLE_FAN);
      Screen.setColorAlpha(P47Screen.retroR, P47Screen.retroG, P47Screen.retroB, 0);
      for (int index25 = 0; index25 < EnemyType.BODY_SHAPE_POINT_NUM; index25++) {
	if (index25 == 2)
	  Screen.setColorAlpha
	    (P47Screen.retroR, P47Screen.retroG, P47Screen.retroB, P47Screen.retroA);
	glVertex3f(pos.x + type.bodyShapePos[index25].x, pos.y + type.bodyShapePos[index25].y, z);
      }
      glEnd();
    }
    for (int index26 = 0; index26 < type.batteryNum; index26++) {
      BatteryType  bt = type.batteryType[index26];
      if (appCnt > 0) {
	P47Screen.setRetroColor(bt.r, bt.g, bt.b, (1 - ap));
      } else if (dstCnt > 0 || timeoutCnt > 0) {
	P47Screen.setRetroColor(bt.r, bt.g, bt.b, ap);
      } else {
	if (!battery[index26].damaged)
	  P47Screen.setRetroColor(bt.r, bt.g, bt.b, 1);
	else
	  P47Screen.setRetroColor(1, 1, bt.b, 1);
      }
      ni = 1;
      if (battery[index26].shield <= 0) {
	P47Screen.drawLineRetro(pos.x + bt.wingShapePos[0].x, pos.y + bt.wingShapePos[0].y,
				pos.x + bt.wingShapePos[1].x, pos.y + bt.wingShapePos[1].y);
      } else {
	for (int index27 = 0; index27 < BatteryType.WING_SHAPE_POINT_NUM; index27++, ni++) {
	  if (ni >= BatteryType.WING_SHAPE_POINT_NUM)
	    ni = 0;
	  P47Screen.drawLineRetro(pos.x + bt.wingShapePos[index27].x, pos.y + bt.wingShapePos[index27].y,
				  pos.x + bt.wingShapePos[ni].x, pos.y + bt.wingShapePos[ni].y);
	}
	if (type.type != EnemyType.SMALL) {
	  glBegin(GL_TRIANGLE_FAN);
	  Screen.setColorAlpha
	    (P47Screen.retroR, P47Screen.retroG, P47Screen.retroB, P47Screen.retroA);
	  for (int index28 = 0; index28 < BatteryType.WING_SHAPE_POINT_NUM; index28++) {
	    if (index28 == 2)
	      Screen.setColorAlpha
		(P47Screen.retroR, P47Screen.retroG, P47Screen.retroB, 0);
	    glVertex3f(pos.x + bt.wingShapePos[index28].x, pos.y + bt.wingShapePos[index28].y, z);
	  }
	  glEnd();
	}
      }
    }
    P47Screen.setRetroZ(0);
  }
}

public class EnemyInitializer: ActorInitializer {

  public Field field;
  public BulletActorPool bullets;
  public ActorPool shots;
  public ActorPool rolls;
  public ActorPool targetLocks;
  public Ship ship;
  public P47GameManager manager;

  public EnemyInitializer(Field field, BulletActorPool bullets, ActorPool shots,
	      ActorPool rolls, ActorPool targetLocks, Ship ship, P47GameManager manager) {
    this.field = field;
    this.bullets = bullets;
    this.shots = shots;
    this.rolls = rolls;
    this.targetLocks = targetLocks;
    this.ship = ship;
    this.manager = manager;
  }
}

  public class Battery {
    public BulletActor[] topBullet = new BulletActor[BatteryType.WING_BATTERY_MAX];
    public int shield;
    public bool damaged;
  }
