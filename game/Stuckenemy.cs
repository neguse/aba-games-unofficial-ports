// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class StuckEnemy: Actor {

  public bool isConnected;

  public Ship ship;
  public Field field;
  public GameManager manager;
  public TumikiSet tumikiSet;
  public Vector ofs, lofs;
  public float stDeg;
  public Vector pos;
  public float deg;
  public Vector[] colDatums = new Vector[4];
  public bool isMyShip;
  public const int TOP_BULLET_MAX = 16;
  public EnemyTopBullet[] topBullet = new EnemyTopBullet[TOP_BULLET_MAX];
  public int topBulletNum;
  public int barragePtnIdx;
  public BulletActorPool bullets;
  public ActorPool fragments;
  public VirtualBulletTarget target;
  public int cnt;
  public float colSize;
  public const int CONNECTED_ENEMY_MAX = 16;
  public StuckEnemy[] connectedEnemy = new StuckEnemy[CONNECTED_ENEMY_MAX];
  public int connectedEnemyNum;
  public StuckEnemyPool stuckEnemies;
  public SplinterPool splinters;

  public override Actor newActor() {
    return new StuckEnemy();
  }

  public override void init(ActorInitializer ini) {
    StuckEnemyInitializer sei = (StuckEnemyInitializer) ini;
    ship = sei.ship;
    field = sei.field;
    bullets = sei.bullets;
    fragments = sei.fragments;
    splinters = sei.splinters;
    manager = sei.manager;
    ofs = new Vector();
    lofs = new Vector();
    pos = new Vector();
    target = new VirtualBulletTarget();
    for (int _i = 0; _i < 4; _i++) { colDatums[_i] = new Vector(); }
    for (int _i = 0; _i < TOP_BULLET_MAX; _i++) { topBullet[_i] = new EnemyTopBullet(); }
  }

  public void setStuckEnemyPool(StuckEnemyPool sep) {
    stuckEnemies = sep;
  }

  public bool set(float x, float y, float d, TumikiSet ts, int bpi) {
    ofs.x = x; ofs.y = y;
    stDeg = d;
    tumikiSet = ts;
    barragePtnIdx = bpi;
    isMyShip = false;
    cnt = 0;
    connectedEnemyNum = 0;
    setColSize();
    if (!stuckEnemies.checkConnected(this))
      return false;
    addTopBullets();
    isExist = true;
    return true;
  }

  public void setColSize() {
    colSize = -tumikiSet.sizeXm;
    if (colSize < tumikiSet.sizeXp)
      colSize = tumikiSet.sizeXp;
    if (colSize < -tumikiSet.sizeYm)
      colSize = -tumikiSet.sizeYm;
    if (colSize < tumikiSet.sizeYp)
      colSize = tumikiSet.sizeYp;
    colSize *= COLLISION_RATIO;
  }

  public void addTopBullets() {
    topBulletNum = tumikiSet.addTopBullets
      (barragePtnIdx, bullets, topBullet, target, BulletType.SHIP);
  }

  public void removeTopBullets() {
    for (int i = 0 ; i < topBulletNum; i++)
      if (topBullet[i].actor != null)
	topBullet[i].actor.removeForced();
  }

  public void remove() {
    removeTopBullets();
    isExist = false;
  }

  public void setTopBulletsPos() {
    for (int i = 0 ; i < topBulletNum; i++) {
      EnemyTopBullet etb = topBullet[i];
      if (etb.actor != null) {
	float stox = etb.tumiki.ofs.x * cos(deg) - etb.tumiki.ofs.y * sin(deg);
	float stoy = etb.tumiki.ofs.x * sin(deg) + etb.tumiki.ofs.y * cos(deg);
	etb.actor.bullet.pos.x = pos.x + stox;
	etb.actor.bullet.pos.y = pos.y + stoy;
	etb.actor.bullet.deg = deg - PI / 2;
      }
    }
  }

  public void setAsMyShip(TumikiSet ts) {
    stDeg = 0; ofs.y = stDeg; ofs.x = ofs.y;
    tumikiSet = ts;
    isMyShip = true;
    connectedEnemyNum = 0;
    setColSize();
    isExist = true;
  }

  public void breakIntoFragments() {
    if (manager.mode == GameMode.EXTRA && stuckEnemies.pullInRatio < 1)
      return;
    tumikiSet.breakIntoFragments(fragments, pos, deg);
    bullets.clearStuckEnemyHit(this);
  }

  public const float SPLINTER_FLYIN_RATIO_X = 0.03f;
  public const float SPLINTER_FLYIN_RATIO_Y = 0.01f;
  public const float SPLINTER_FLYIN_DEG_RATIO = -0.03f;
  public const float SPLINTER_FLYIN_MOVE_Y = 0.36f;
  public const float SPLINTER_FLYIN_MOVE_DEG_MAX = 0.2f;

  public void breakIntoSplinter() {
    Splinter sp = (Splinter) splinters.getInstance();
    if (sp == null)
      return;
    float md = lofs.x * SPLINTER_FLYIN_DEG_RATIO;
    if (md > SPLINTER_FLYIN_MOVE_DEG_MAX)
      md = SPLINTER_FLYIN_MOVE_DEG_MAX;
    else if (md < -SPLINTER_FLYIN_MOVE_DEG_MAX)
      md = -SPLINTER_FLYIN_MOVE_DEG_MAX;
    sp.setFlying(pos,
	   lofs.x * SPLINTER_FLYIN_RATIO_X,
	   lofs.y * SPLINTER_FLYIN_RATIO_Y + SPLINTER_FLYIN_MOVE_Y,
	   deg, md, tumikiSet, barragePtnIdx);
  }

  public const float COLLISION_RATIO = 0.8f;
  public const float COLLISION_RATIO_WIDE = 3.3f;

  public void setNormalCollision() {
    float sd = sin(deg) * COLLISION_RATIO, cd = cos(deg) * COLLISION_RATIO;
    setCollision(sd, cd);
  }

  public void setWideCollision() {
    float sd = sin(deg) * COLLISION_RATIO_WIDE, cd = cos(deg) * COLLISION_RATIO_WIDE;
    setCollision(sd, cd);
  }

  public void setCollision(float sd, float cd) {
    colDatums[0].x = pos.x + tumikiSet.sizeXm * cd;
    colDatums[0].y = pos.y + tumikiSet.sizeXm * sd;
    colDatums[1].x = pos.x - tumikiSet.sizeYm * sd;
    colDatums[1].y = pos.y + tumikiSet.sizeYm * cd;
    colDatums[2].x = pos.x + tumikiSet.sizeXp * cd;
    colDatums[2].y = pos.y + tumikiSet.sizeXp * sd;
    colDatums[3].x = pos.x - tumikiSet.sizeYp * sd;
    colDatums[3].y = pos.y + tumikiSet.sizeYp * cd;
  }

  public override void move() {
    deg = stDeg + ship.deg;
    float osd, ocd;
    osd = sin(ship.deg);
    ocd = cos(ship.deg);
    lofs.x = ofs.x * ocd - ofs.y * osd;
    lofs.y = ofs.x * osd + ofs.y * ocd;
    if (manager.mode == GameMode.EXTRA) {
      pos.x = ship.pos.x + lofs.x * stuckEnemies.pullInRatio;
      pos.y = ship.pos.y + lofs.y * stuckEnemies.pullInRatio;
    } else {
      pos.x = ship.pos.x + lofs.x;
      pos.y = ship.pos.y + lofs.y;
    }
    setNormalCollision();
    target.pos.x = pos.x - cos(deg) * Ship.TARGET_DISTANCE;
    target.pos.y = pos.y - sin(deg) * Ship.TARGET_DISTANCE;
    setTopBulletsPos();
    cnt++;
    int mp = 1;
    int sen = Se.STUCK_BONUS;
    if (manager.mode == GameMode.EXTRA)
      if (stuckEnemies.pullInRatio >= 1)
	mp = 5;
      else
	sen = Se.STUCK_BONUS_PUSHIN;
    if (tumikiSet.fireScoreInterval > 0 &&
	(cnt % tumikiSet.fireScoreInterval) == 0 &&
	!field.checkHit(pos)) {
      manager.addScore(tumikiSet.fireScore * mp, pos);
      SoundManager.playSe(sen);
    }
    if (!isMyShip) {
      stuckEnemies.totalSize += tumikiSet.size;
    } else {
      for (int i = 0; i < connectedEnemyNum; i++) {
	if (!connectedEnemy[i].isExist) {
	  connectedEnemyNum--;
	  for (int j = i; j < connectedEnemyNum; j++)
	    connectedEnemy[j] = connectedEnemy[j + 1];
	}
      }
    }
  }

  public override void draw() {
    if (!isMyShip) {
      if (manager.mode == GameMode.EXTRA && stuckEnemies.pullInRatio < 1)
	tumikiSet.drawShadeScaled(pos, 0.2f, 1, deg, stuckEnemies.pullInRatio);
      else
	tumikiSet.drawRotated(pos, 0.2f, deg);
    }
  }

  public bool checkHit(Vector pos) {
    if (pos.checkSide(colDatums[0], colDatums[1]) *
	pos.checkSide(colDatums[3], colDatums[2]) < 0 &&
	pos.checkSide(colDatums[1], colDatums[2]) *
	pos.checkSide(colDatums[0], colDatums[3]) < 0)
      return true;
    else
      return false;
  }

  public bool checkConnected(StuckEnemy se) {
    if (se.ofs.dist(ofs) > se.colSize + colSize)
      return false;
    se.addConnected(this);
    addConnected(se);
    return true;
  }

  public void addConnected(StuckEnemy se) {
    if (connectedEnemyNum >= CONNECTED_ENEMY_MAX)
      return;
    connectedEnemy[connectedEnemyNum] = se;
    connectedEnemyNum++;
    return;
  }

  public void scanConnected() {
    isConnected = true;
    for (int i = 0; i < connectedEnemyNum; i++) {
      if (connectedEnemy[i].isExist)
	if(!connectedEnemy[i].isConnected)
	  connectedEnemy[i].scanConnected();
    }
  }

  public void activateTopBullets() {
    for (int i = 0 ; i < topBulletNum; i++)
      if (topBullet[i].actor != null)
	topBullet[i].actor.bullet.deactivated = false;
  }

  public void deactivateTopBullets() {
    for (int i = 0 ; i < topBulletNum; i++)
      if (topBullet[i].actor != null)
	topBullet[i].actor.bullet.deactivated = true;
  }
}

public class StuckEnemyInitializer: ActorInitializer {

  public Ship ship;
  public Field field;
  public BulletActorPool bullets;
  public ActorPool fragments;
  public SplinterPool splinters;
  public GameManager manager;

  public StuckEnemyInitializer(Ship ship, Field field, BulletActorPool bullets, ActorPool fragments,
	      SplinterPool splinters, GameManager manager) {
    this.ship = ship;
    this.field = field;
    this.bullets = bullets;
    this.fragments = fragments;
    this.splinters = splinters;
    this.manager = manager;
  }
}

public class StuckEnemyPool: ActorPool {

  public float pullInRatio;
  public float totalSize;

  public const int PULLIN_CNT_MAX = 16;
  public int pullInCnt;
  public GameManager manager;

  public StuckEnemyPool(int n, ActorInitializer ini) : base(n, new StuckEnemy(), ini) {
    manager = ((StuckEnemyInitializer) ini).manager;
  }

  public void init() {
    foreach (Actor ac in actor) {
      StuckEnemy se = (StuckEnemy) ac;
      se.setStuckEnemyPool(this);
    }
    initPullIn();
  }

  public void initPullIn() {
    pullInCnt = 0;
    pullInRatio = 1;
  }

  public bool checkHit(Vector pos) {
    if (pullInCnt > 0)
      return false;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (se.checkHit(pos))
	  return true;
      }
    }
    return false;
  }

  public StuckEnemy checkHitWithoutMyShip(Vector pos) {
    if (pullInCnt > 0)
      return null;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip && se.checkHit(pos))
	  return se;
      }
    }
    return null;
  }

  public void removeAllEnemies() {
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip) {
	  se.breakIntoFragments();
	  se.remove();
	}
      }
    }
    initPullIn();
  }

  public void flyinAllEnemies() {
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip) {
	  se.breakIntoSplinter();
	  se.remove();
	}
      }
    }
    initPullIn();
  }

  public bool checkConnected(StuckEnemy nse) {
    bool connected = false;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (se.checkConnected(nse))
	  connected = true;
      }
    }
    return connected;
  }

  public void removeStuckEnemy(StuckEnemy hse) {
    hse.breakIntoFragments();
    hse.remove();
    scanConnected();
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip && !se.isConnected) {
	  se.breakIntoFragments();
	  se.remove();
	}
      }
    }
  }

  public void scanConnected() {
    StuckEnemy myShip = null;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (se.isMyShip)
	  myShip = se;
	se.isConnected = false;
      }
    }
    myShip.scanConnected();
  }

  public void pullIn() {
    if (pullInCnt == 0)
      deactivateTopBullets();
    if (pullInCnt < PULLIN_CNT_MAX)
      pullInCnt++;
    pullInRatio = 1 - ((float) pullInCnt) / PULLIN_CNT_MAX;
  }

  public void pushOut() {
    if (pullInCnt > 0)
      pullInCnt--;
    if (pullInCnt == 0)
      activateTopBullets();
    pullInRatio = 1 - ((float) pullInCnt) / PULLIN_CNT_MAX;
  }

  public void activateTopBullets() {
    StuckEnemy myShip = null;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip)
	  se.activateTopBullets();
      }
    }
  }

  public void deactivateTopBullets() {
    StuckEnemy myShip = null;
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	StuckEnemy se = (StuckEnemy) ac;
	if (!se.isMyShip)
	  se.deactivateTopBullets();
      }
    }
  }

  public override void move() {
    totalSize = 0;
    foreach (Actor ac in actor) {
      if (ac.isExist)
	ac.move();
    }
    if (manager.mode == GameMode.EXTRA)
      manager.setRank(totalSize * 0.02f);
  }
}
