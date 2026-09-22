// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Ship: BulletTarget {

  public Vector pos;
  public float deg;
  public const float SIZE = 0.3f;
  public bool restart;
  public const int RESTART_CNT = 300;
  public const int INVINCIBLE_CNT = 228;
  public const int RESPAWN_CNT = 250;
  public const float RESPAWN_MOVE = 0.8f;
  public const int NOBULLET_CNT = 150;
  public int cnt;
  public const float TARGET_DISTANCE = 20;
  public StuckEnemyPool stuckEnemies;

  public static Rand rand = new Rand();
  public Pad pad;
  public Field field;
  public BulletActorPool bullets;
  public ParticlePool particles;
  public ActorPool fragments;
  public GameManager manager;
  public const float BASE_SPEED = 0.4f;
  public const float SLOW_SPEED = 0.33f;
  public float speed;
  public Vector vel;
  public const float BANK_BASE = 1.5f;
  public int fireCnt;
  public const float FIELD_SPACE = 1.5f;
  public float fieldLimitX, fieldLimitY;
  public TumikiSet tumikiSet;
  public VirtualBulletTarget target;
  public const int FIRE_INTERVAL = 2;
  public EnemyTopBullet[] etb = new EnemyTopBullet[1];
  public float groundY;
  public int startCnt;
  public int endCnt;
  public float smx, smy;
  public Vector[] friendPos = new Vector[8];
  public bool btnPrsd;
  public bool pullIn;



  public void init(Pad pad, Field field, ParticlePool particles, ActorPool fragments,
		   GameManager manager) {
    this.pad = pad;
    this.field = field;
    this.particles = particles;
    this.fragments = fragments;
    this.manager = manager;
    pos = new Vector();
    vel = new Vector();
    fieldLimitX = field.size.x - FIELD_SPACE;
    fieldLimitY = field.size.y - FIELD_SPACE;
    target = new VirtualBulletTarget();
    etb[0] = new EnemyTopBullet();
    createTumiki();
    for (int _i = 0; _i < 8; _i++) {
      friendPos[_i] = new Vector();
     }
  }

  public void createTumiki() {
    tumikiSet = TumikiSet.getInstance("myship/ship.tmk");
  }

  public void setBulletActorPool(BulletActorPool bullets) {
    this.bullets = bullets;
  }

  public void initStuckEnemies(SplinterPool splinters) {
    StuckEnemyInitializer sei =
      new StuckEnemyInitializer(this, field, bullets, fragments, splinters, manager);
    stuckEnemies = new StuckEnemyPool(128, sei);
    stuckEnemies.init();
    setMyShipAsStuckEnemy();
  }

  public void setMyShipAsStuckEnemy() {
    StuckEnemy se = (StuckEnemy) stuckEnemies.getInstance();
    se.setAsMyShip(tumikiSet);
  }

  public void start() {
    pos.x = -field.size.x / 2;
    pos.y = 0;
    vel.y = 0; vel.x = vel.y;
    speed = BASE_SPEED;
    restart = true;
    cnt = -INVINCIBLE_CNT;
    fireCnt = 0;
    deg = 0;
    pullIn = false;
  }

  public void startStage() {
    start();
    pos.x = -field.size.x / 3 * 2;
    pos.y = -field.size.y / 5 * 4;
    deg = 0.2f;
    groundY = 120;
    field.setGroundY(groundY);
    startCnt = 0;
    cnt = 0;
    smy = 0; smx = smy;
    rand.setSeed(0);
    foreach (Vector fp in friendPos) {
      fp.x = -rand.nextFloat(field.size.x * 3) + field.size.x * 1.5f;
      fp.y = pos.y + rand.nextFloat(field.size.y);
    }
  }

  public void backToHome() {
    endCnt = 0;
    smx = -0.05f;
    smy = 0;
    rand.setSeed(0);
    foreach (Vector fp in friendPos) {
      fp.x = -rand.nextFloat(field.size.x * 3) + field.size.x * 1.5f;
      fp.y = field.size.y + rand.nextFloat(field.size.y / 2);
    }
  }

  public void destroyed() {
    if (cnt <= 0)
      return;
    flyinStuckEnemies();
    SoundManager.playSe(Se.SHIP_DESTROYED);
    manager.shipDestroyed();
    particles.add(15, pos, 0, PI * 2, 2, 1, ParticleType.SMOKE);
    particles.add(20, pos, 0, PI * 2, 1, 0.6f, ParticleType.SPARK);
    particles.add(8, pos, 0, PI * 2, 4, 0.3f, ParticleType.SPARK);
    start();
    pos.x = -field.size.x;
    cnt = -RESTART_CNT;
  }

  public void breakStuckEnemies() {
    stuckEnemies.removeAllEnemies();
  }

  public void flyinStuckEnemies() {
    stuckEnemies.flyinAllEnemies();
  }

  public void hitStuckEnemiesPart(StuckEnemy se) {
    stuckEnemies.removeStuckEnemy(se);
  }

  public void move() {
    cnt++;
    if (cnt < -NOBULLET_CNT)
      bullets.clearVisibleEnemy();
    if (cnt < -INVINCIBLE_CNT) {
      if (cnt > -RESPAWN_CNT)
	pos.x += RESPAWN_MOVE;
      return;
    }
    if (cnt == 0)
      restart = false;
    int btn = pad.getButtonState();
    int ps = pad.getPadState();
    vel.y = 0; vel.x = vel.y;
    if ((ps & Pad.PAD_UP) != 0)
      vel.y = speed;
    else if ((ps & Pad.PAD_DOWN) != 0)
      vel.y = -speed;
    if ((ps & Pad.PAD_RIGHT) != 0)
      vel.x = speed;
    else if ((ps & Pad.PAD_LEFT) != 0)
      vel.x = -speed;
    if (vel.x != 0 && vel.y != 0) {
      vel.x *= 0.707f;
      vel.y *= 0.707f;
    }
    pos.x += vel.x;
    pos.y += vel.y;
    if (pos.x < -fieldLimitX)
      pos.x = -fieldLimitX;
    else if (pos.x > fieldLimitX)
      pos.x = fieldLimitX;
    if (pos.y < -fieldLimitY)
      pos.y = -fieldLimitY;
    else if (pos.y > fieldLimitY)
      pos.y = fieldLimitY;
    target.pos.x = pos.x + cos(deg) * TARGET_DISTANCE;
    target.pos.y = pos.y + sin(deg) * TARGET_DISTANCE;
    if ((btn & Pad.PAD_BUTTON1) != 0 && fireCnt <= 0) {
      fireCnt = FIRE_INTERVAL + 1;
      int eidx = tumikiSet.addTopBullets(0, bullets, etb, target, BulletType.SHIP);
      if (eidx > 0) {
	etb[0].actor.unsetTop();
	etb[0].actor.setMorphSeed();
	etb[0].actor.bullet.pos.x = pos.x;
	etb[0].actor.bullet.pos.y = pos.y;
	etb[0].actor.bullet.deg = -deg - PI / 2;
	SoundManager.playSe(Se.SHIP_SHOT);
      }
    }
    if ((btn & Pad.PAD_BUTTON2) != 0) {
      speed += (SLOW_SPEED - speed) * 0.2f;
      if (manager.mode == GameMode.EXTRA)
	stuckEnemies.pullIn();
    } else {
      speed += (BASE_SPEED - speed) * 0.2f;
      deg += (vel.y * BANK_BASE - deg) * 0.05f;
      if (manager.mode == GameMode.EXTRA)
	stuckEnemies.pushOut();
    }
    if (fireCnt > 0)
      fireCnt--;
    stuckEnemies.move();
  }

  public void startMove() {
    if (startCnt < 120) {
      if (startCnt < 80)
	smx += 0.003f;
      else
	smx -= 0.006f;
      pos.x += smx;
      if (startCnt < 60)
	particles.add(1, pos, PI / 2 + 0.2f, 0.4f, startCnt * 0.02f, 0.5f, ParticleType.SMOKE);
    }
    if (startCnt > 60 && startCnt < 180) {
      if (startCnt == 61)
	SoundManager.playSe(Se.PROPELLER);
      if (startCnt < 140)
	smy += 0.003f;
      else
	smy -= 0.006f;
      pos.y += smy;
      groundY -= 1;
      field.setGroundY(groundY);
      pos.x -= 0.01f;
    }
    if (startCnt > 180) {
      pos.x -= 0.1f;
      deg -= 0.0026f;
      if (startCnt > 256) {
	manager.setInGame();
      }
    }
    startCnt++;
    foreach (Vector fp in friendPos) {
      fp.y += 0.15f;
    }
  }

  public void endMove() {
    pos.x += BASE_SPEED * 2;
    deg *= 0.95f;
  }

  public void backToHomeMove() {
    int ec = endCnt % 200;
    if (ec < 100)
      smx += 0.001f;
    else
      smx -= 0.001f;
    if (ec < 50 || ec > 150)
      smy += 0.001f;
    else
      smy -= 0.001f;
    pos.x += smx;
    pos.y += smy;
    deg *= 0.95f;
    if (endCnt <= 60) {
      btnPrsd = true;
    } else {
      if ((pad.getButtonState() & (Pad.PAD_BUTTON1 | Pad.PAD_BUTTON2)) != 0) {
	if (!btnPrsd) {
	  manager.startGameover();
	  return;
	}
      } else {
	btnPrsd = false;
      }
    }
    if (endCnt > 700) {
      manager.startGameover();
      return;
    } else if (endCnt > 620) {
      endMove();
      foreach (Vector fp in friendPos)
	fp.x += BASE_SPEED * 2;
    }
    endCnt++;
    if (endCnt < 220) {
      foreach (Vector fp in friendPos)
	fp.y -= 0.05f;
    } else if (endCnt < 330) {
      foreach (Vector fp in friendPos)
	fp.y -= 0.02f;
    }
  }

  public Vector getTargetPos() {
    return pos;
  }

  public void draw() {
    stuckEnemies.draw();
    if (cnt < -RESPAWN_CNT || (cnt < 0 && (-cnt % 32) < 16))
      return;
    tumikiSet.drawRotated(pos, 0, deg);
  }

  public void drawFriendly() {
    float z = -3;
    foreach (Vector fp in friendPos) {
      tumikiSet.drawShadeRotated(fp, z, 1, 0.2f);
      z -= 1;
    }
  }

  public void drawFriendlyBack() {
    float z = -3;
    foreach (Vector fp in friendPos) {
      tumikiSet.drawShadeRotated(fp, z, 1, 0);
      z -= 1;
    }
  }

  public void drawLeft(float x, float y, float z) {
    tumikiSet.drawAt(x, y, z, false, false);
  }
}
