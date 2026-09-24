// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Splinter: Actor {

  public static int signNum;
  public static Rand rand = new Rand();
  public TumikiSet tumikiSet;
  public Vector pos, vel;
  public float deg, md;
  public int barragePtnIdx;
  public Ship ship;
  public Field field;
  public ParticlePool particles;
  public GameManager manager;
  public Vector[] colDatums = new Vector[16];
  public bool hasSign;
  public int cnt;
  public bool isBoss;
  public bool flyin;



  public static void setSignNum(int n) {
    signNum = n;
  }

  public override Actor newActor() {
    return new Splinter();
  }

  public override void init(ActorInitializer ini) {
    SplinterInitializer si = (SplinterInitializer) ini;
    ship = si.ship;
    field = si.field;
    particles = si.particles;
    manager = si.manager;
    pos = new Vector();
    vel = new Vector();
    for (int _i = 0; _i < 16; _i++) { colDatums[_i] = new Vector(); }
  }

  public const float MOVE_DEG_DEFAULT = 0.05f;
  public const float MOVE_X_DEFAULT = 0.16f;
  public const float GRAVITY = 0.005f;

  public void set(float x, float y, TumikiSet ts, int bpi, bool isBoss) {
    pos.x = x;
    pos.y = y;
    tumikiSet = ts;
    barragePtnIdx = bpi;
    deg = 0;
    this.isBoss = isBoss;
    if (!isBoss) {
      md = MOVE_DEG_DEFAULT;
      vel.x = -MOVE_X_DEFAULT;
      vel.y = 0;
    } else {
      md = MOVE_DEG_DEFAULT / 3;
      vel.x = -MOVE_X_DEFAULT / 2;
      vel.y = -MOVE_X_DEFAULT / 3;
    }
    if (signNum > 0) {
      hasSign = true;
      signNum--;
    } else {
      hasSign = false;
    }
    flyin = false;
    cnt = 0;
    isExist = true;
  }

  public void setFlying(Vector p, float mx, float my, float deg, float md, TumikiSet ts, int bpi) {
    pos.x = p.x;
    pos.y = p.y;
    tumikiSet = ts;
    barragePtnIdx = bpi;
    this.deg = deg;
    isBoss = false;
    this.md = md;
    vel.x = mx;
    vel.y = my;
    hasSign = false;
    flyin = true;
    cnt = 0;
    isExist = true;
  }

  public const float COLLISION_RATIO = 0.8f;

  public override void move() {
    pos.add(vel);
    deg += md;
    cnt++;
    if (!isBoss) {
      vel.y -= GRAVITY;
      if (pos.y < -field.size.y - tumikiSet.size) {
	isExist = false;
	return;
      }
      float sd = sin(deg) * COLLISION_RATIO, cd = cos(deg) * COLLISION_RATIO;
      colDatums[0].x = pos.x + tumikiSet.sizeXm * cd;
      colDatums[0].y = pos.y + tumikiSet.sizeXm * sd;
      colDatums[1].x = pos.x - tumikiSet.sizeYm * sd;
      colDatums[1].y = pos.y + tumikiSet.sizeYm * cd;
      colDatums[2].x = pos.x + tumikiSet.sizeXp * cd;
      colDatums[2].y = pos.y + tumikiSet.sizeXp * sd;
      colDatums[3].x = pos.x - tumikiSet.sizeYp * sd;
      colDatums[3].y = pos.y + tumikiSet.sizeYp * cd;
      int di1 = 0, di2 = 1;
      int idx = 4;
      for (int i = 0; i < 4; i++) {
	float ox = (colDatums[di2].x - colDatums[di1].x) / 4;
	float oy = (colDatums[di2].y - colDatums[di1].y) / 4;
	float dx = colDatums[di1].x;
	float dy = colDatums[di1].y;
	for (int j = 0; j < 3; j++) {
	  dx += ox;
	  dy += oy;
	  colDatums[idx].x = dx;
	  colDatums[idx].y = dy;
	  idx++;
	}
	di1++;
	di2++;
	if (di2 > 3)
	  di2 = 0;
      }
      if (ship.cnt < -Ship.INVINCIBLE_CNT)
	return;
      foreach (Vector point in colDatums) {
	if (ship.stuckEnemies.checkHit(point)) {
	  StuckEnemy se = (StuckEnemy) ship.stuckEnemies.getInstance();
	  if (se != null) {
	    float ox = pos.x - ship.pos.x, oy = pos.y - ship.pos.y;
	    float sx = ox * cos(-ship.deg) - oy * sin(-ship.deg);
	    float sy = ox * sin(-ship.deg) + oy * cos(-ship.deg);
	    if (!se.set(sx, sy, deg - ship.deg, tumikiSet, barragePtnIdx))
	      continue;
	    if (!flyin) {
	      int s = (tumikiSet.score / 2 / 10) * 10;
	      manager.addScore(s, pos);
	    }
	    particles.add(3, pos, 0, PI * 2, 0.05f, 0.3f, ParticleType.SMOKE);
	    SoundManager.playSe(Se.STUCK);
	  }
	  isExist = false;
	  return;
	}
      }
    } else {
      vel.x *= 0.99f;
      particles.add(1, pos, 0, PI * 2, 0.5f  + rand.nextFloat(2), 0.5f, ParticleType.SPARK);
      if (rand.nextInt(45) == 0) {
	particles.add(3 + rand.nextInt(4), pos, 0, PI * 2, 0.3f, 0.7f, ParticleType.SMOKE);
	SoundManager.playSe(Se.ENEMY_DESTROYED);
      }
      if (cnt > 180) {
	particles.add(32, pos, 0, PI * 2, 2, 0.5f, ParticleType.SPARK);
	particles.add(15, pos, 0, PI * 2, 0.5f, 1.5f, ParticleType.SMOKE);
	particles.add(15, pos, 0, PI * 2, 3, 1, ParticleType.SMOKE);
	SoundManager.playSe(Se.BOSS_DESTROYED);
	isExist = false;
      }
    }
  }

  public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null) {
    bool depth = true; Gfx.Cull cull = Gfx.Cull.Front; float width = 1;
    tumikiSet.drawShadeRotated(model, tint, blend, depth, cull, width, pos, -0.7f, 1, deg);
    if (hasSign && (cnt & 31) < 24) {
      LetterRender.drawStringFacing(model, tint, blend, depth, cull, width, "CATCH ME!", pos.x - 6, pos.y + 2.7f, 0.6f, LetterDirection.TO_RIGHT, 3, true);
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
}

public class SplinterInitializer: ActorInitializer {

  public Ship ship;
  public Field field;
  public ParticlePool particles;
  public GameManager manager;

  public SplinterInitializer(Ship ship, Field field, ParticlePool particles, GameManager manager) {
    this.ship = ship;
    this.field = field;
    this.particles = particles;
    this.manager = manager;
  }
}

public class SplinterPool: ActorPool {

  public SplinterPool(int n, ActorInitializer ini) : base(n, new Splinter(), ini) {
  }

  public bool checkHit(Vector pos) {
    foreach (Actor ac in actor) {
      if (ac.isExist) {
	Splinter sp = (Splinter) ac;
	if (sp.checkHit(pos))
	  return true;
      }
    }
    return false;
  }
}
