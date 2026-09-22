// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class BulletActor: Actor {

  public static float totalBulletsSpeed;
  public BulletInst bullet;

  public const float FIELD_SPACE = 0.5f;
  public Field field;
  public Ship ship;
  public ParticlePool particles;
  public SplinterPool splinters;
  public EnemyPool enemies;
  public static int nextId;
  public bool isSimple;
  public bool isTop;
  public bool isVisible;
  public Vector ppos;
  public const float SHIP_HIT_WIDTH = 0.4f;
  public int cnt;
  public bool shouldBeRemoved;
  public bool isWait;
  public int postWait;
  public int waitCnt;
  public bool isMorphSeed;

  public static void resetIds() {
    nextId = 0;
  }

  public static void resetTotalBulletsSpeed() {
    totalBulletsSpeed = 0;
  }

  public override Actor newActor() {
    return new BulletActor();
  }

  public override void init(ActorInitializer ini) {
    BulletActorInitializer bi = (BulletActorInitializer) ini;
    field = bi.field;
    ship = bi.ship;
    particles = bi.particles;
    splinters = bi.splinters;
    bullet = new BulletInst(this);
    ppos = new Vector();
    nextId++;
  }

  public void setEnemies(EnemyPool enemies) {
    this.enemies = enemies;
  }

  public void setStageManager(StageManager stageManager) {
    bullet.setStageManager(stageManager);
  }

  public void start(float speedRank, int shape, int color, float size,
		     float xReverse, float yReverse,
		     BulletTarget target, int type) {
    isExist = true;
    isTop = false;
    isWait = false;
    isVisible = true;
    isMorphSeed = false;
    ppos.x = bullet.pos.x;
    ppos.y = bullet.pos.y;
    bullet.setParam(speedRank, shape, color, size, xReverse, yReverse, target, type);
    cnt = 0;
    shouldBeRemoved = false;
  }

  public void setPattern(PatternState[] runner,
		  float x, float y, float deg, float speed,
		  float rank, float speedRank,
		  int shape, int color, float size,
		  float xReverse, float yReverse,
		  BulletTarget target, int type,
		  int[] parser, float[] ranks, float[] speeds,
		  int morphNum, int morphIdx) {
    bullet.setPattern(runner, x, y, deg, speed, rank);
    bullet.setMorph(parser, ranks, speeds, morphNum, morphIdx);
    isSimple = false;
    start(speedRank, shape, color, size, xReverse, yReverse, target, type);
  }

  public void set(float x, float y, float deg, float speed,
		  float rank, float speedRank,
		  int shape, int color, float size,
		  float xReverse, float yReverse,
		  BulletTarget target, int type) {
    bullet.set(x, y, deg, speed, rank);
    bullet.morphIdx = 0; bullet.morphNum = bullet.morphIdx;
    isSimple = true;
    start(speedRank, shape, color, size, xReverse, yReverse, target, type);
  }

  public void setInvisible() {
    isVisible = false;
  }

  public void setTop() {
    isTop = true;
    setInvisible();
  }

  public void unsetTop() {
    isTop = false;
  }

  public void setWait(int prvw, int pstw) {
    isWait = true;
    waitCnt = prvw;
    postWait = pstw;
  }

  public void setMorphSeed() {
    isMorphSeed = true;
  }

  public void rewind() {
    bullet.remove();
    PatternState[] runner = BulletInst.createRunner(bullet.parser[0]);
    bullet.setRunner(runner);
    bullet.resetMorph();
  }

  public void remove() {
    shouldBeRemoved = true;
  }

  public void removeForced() {
    if (!isSimple)
      bullet.remove();
    isExist = false;
  }

  public void removeForcedVisible() {
    if (isVisible) {
      particles.add(1, bullet.pos, bullet.deg, 0, bullet.velocity * bullet.speedRank, 0.4f,
		    ParticleType.SPARK);
      removeForced();
    }
  }

  public void removeForcedVisibleEnemy() {
    if (isVisible && bullet.type == BulletType.ENEMY) {
      particles.add(1, bullet.pos, bullet.deg, 0, bullet.velocity * bullet.speedRank, 0.4f,
		    ParticleType.SPARK);
      removeForced();
    }
  }

  public void checkShipHit() {
    float bmvx, bmvy, inaa;
    bmvx = ppos.x;
    bmvy = ppos.y;
    bmvx -= bullet.pos.x;
    bmvy -= bullet.pos.y;
    inaa = bmvx * bmvx + bmvy * bmvy;
    if (inaa > 0.00001f) {
      float sofsx, sofsy, inab, hd;
      sofsx = ship.pos.x;
      sofsy = ship.pos.y;
      sofsx -= bullet.pos.x;
      sofsy -= bullet.pos.y;
      inab = bmvx * sofsx + bmvy * sofsy;
      if (inab >= 0 && inab <= inaa) {
	hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
	if (hd >= 0 && hd <= SHIP_HIT_WIDTH) {
	  ship.destroyed();
	}
      }
    }
  }

  public override void move() {
    Vector tpos = bullet.target.getTargetPos();
    ppos.x = bullet.pos.x;
    ppos.y = bullet.pos.y;
    if (isTop) {
      bullet.deg = (atan2(tpos.x - bullet.pos.x, tpos.y - bullet.pos.y) * bullet.xReverse
		    + PI / 2) * bullet.yReverse - PI / 2;
    }
    if (isWait && waitCnt > 0) {
      waitCnt--;
      if (shouldBeRemoved)
	removeForced();
      return;
    }
    if (!isSimple) {
      bullet.move();
      if (bullet.isEnd()) {
	if (isTop) {
	  rewind();
	  if (isWait) {
	    waitCnt = postWait;
	    return;
	  }
	} else if (isMorphSeed) {
	  removeForced();
	  return;
	}
      }
    }
    if (shouldBeRemoved) {
      removeForced();
      return;
    }
    bullet.pos.x +=
      (sin(bullet.deg) * bullet.velocity + bullet.acc.x) * bullet.speedRank * bullet.xReverse;
    bullet.pos.y +=
      (cos(bullet.deg) * bullet.velocity - bullet.acc.y) * bullet.speedRank * bullet.yReverse;
    if (isVisible) {
      switch (bullet.type) {
      case BulletType.ENEMY:
	totalBulletsSpeed += bullet.velocity * bullet.speedRank;
	if (splinters.checkHit(bullet.pos)) {
	  removeForcedVisible();
	} else {
	  StuckEnemy hse = ship.stuckEnemies.checkHitWithoutMyShip(bullet.pos);
	  if (hse != null) {
	    particles.add(3, bullet.pos, bullet.deg, 0.1f,
			  bullet.velocity * bullet.speedRank / 2, 0.6f,
			  ParticleType.SMOKE);
	    particles.add(20, bullet.pos, 0, PI * 2, 3, 0.4f, ParticleType.SPARK);
	    SoundManager.playSe(Se.STUCK_DESTROYED);
	    ship.hitStuckEnemiesPart(hse);
	    removeForced();
	  } else {
	    checkShipHit();
	  }
	}
	break;
      case BulletType.SHIP:
	if (enemies.checkHit(bullet.pos, 1)) {
	  particles.add(3, bullet.pos, bullet.deg, 0.1f, bullet.velocity * bullet.speedRank / 2, 0.5f,
			ParticleType.SMOKE);
	  particles.add(3, bullet.pos, bullet.deg + PI, 1, bullet.velocity * bullet.speedRank, 0.3f,
			ParticleType.SPARK);
	  removeForced();
	}
	break;
      }
      if (field.checkHitInset(bullet.pos, FIELD_SPACE))
	removeForced();
    }
    cnt++;
  }

  public const int BULLET_COLOR = 8;
  public const int BULLET_SHADE = 3;

  public override void draw() {
    if (!isVisible)
      return;
    float d;
    d = (-bullet.deg * bullet.xReverse + PI / 2) * bullet.yReverse - PI / 2;
    glPushMatrix();
    glTranslatef(bullet.pos.x, bullet.pos.y, 0);
    int s = 0;
    switch (bullet.shape) {
    case 0:
      glRotatef(rtod(d), 0, 0, 1);
      glScalef(bullet.bulletSize * 0.2f, bullet.bulletSize * 0.5f, 0.3f);
      s = 0;
      break;
    case 1:
      glRotatef(rtod(d), 0, 0, 1);
      glScalef(bullet.bulletSize * 0.2f, bullet.bulletSize * 0.5f, 0.3f);
      s = 5;
      break;
    case 2:
      glRotatef(cnt * 11, 0, 0, 1);
      glScalef(bullet.bulletSize * 0.4f, bullet.bulletSize * 0.4f, 0.3f);
      s = 0;
      break;
    }
    glCallList(Tumiki.displayListIdx + s +
	       (BULLET_COLOR + bullet.color) * Tumiki.SHAPE_NUM +
	       BULLET_SHADE * Tumiki.COLOR_NUM * Tumiki.SHAPE_NUM);
    glPopMatrix();
  }
}

public class BulletActorInitializer: ActorInitializer {

  public Field field;
  public Ship ship;
  public ParticlePool particles;
  public SplinterPool splinters;

  public BulletActorInitializer(Field field, Ship ship, ParticlePool particles, SplinterPool splinters) {
    this.field = field;
    this.ship = ship;
    this.particles = particles;
    this.splinters = splinters;
  }
}
