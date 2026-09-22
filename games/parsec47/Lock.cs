// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Lock: Actor {

    public const int SEARCH = 0;
  public const int SEARCHED = 1;
  public const int LOCKING = 2;
  public const int LOCKED = 3;
  public const int FIRED = 4;
  public const int HIT_STATE = 5;
  public const int CANCELED = 6;
  public int state;
  public const int LENGTH = 12;
  public Vector[] pos = new Vector[LENGTH];
  public const int NO_COLLISION_CNT = 8;
  public int cnt;
  public float targetLockMinY;
  public Enemy targetLockedEnemy;
  public int targetLockedPart;
  public Vector targetLockedPos;
  public bool released;

  public static P47Rand rand = new P47Rand();
  public Vector vel;
  public Ship ship;
  public Field field;
  public P47GameManager manager;

  public static void init_0() {
    rand = new P47Rand();
  }

  public override Actor newActor() {
    return new Lock();
  }

  public override void init(ActorInitializer ini) {
    LockInitializer li = (LockInitializer) ini;
    ship = li.ship;
    field = li.field;
    manager = li.manager;
    for (int index0 = 0; index0 < LENGTH; index0++) {
      pos[index0] = new Vector();
    }
    vel = new Vector();
    targetLockedPos = new Vector();
  }

  public void reset() {
    for (int index1 = 0; index1 < LENGTH; index1++) {
      pos[index1].x = ship.pos.x;
      pos[index1].y = ship.pos.y;
    }
    vel.x = rand.nextSignedFloat(1.5f);
    vel.y = -2;
    cnt = 0;
  }

  public void set() {
    reset();
    state = SEARCH;
    targetLockMinY = field.size.y * 2;
    released = false;
    isExist = true;
  }

  public void hit() {
    state = HIT_STATE;
    cnt = 0;
  }

  public const float SPEED = 0.01f;
  public const int LOCK_CNT = 8;

  public override void move() {
    if (state == LOCKED && cnt >= NO_COLLISION_CNT) state = FIRED;
    if (state == SEARCH) {
      isExist = false;
      return;
    } else if (state == SEARCHED) {
      state = LOCKING;
      SoundManager.playSe(SoundManager.LOCK);
    }
    if (state != HIT_STATE && state != CANCELED) {
      if (targetLockedPart < 0) {
	targetLockedPos.x = targetLockedEnemy.pos.x;
	targetLockedPos.y = targetLockedEnemy.pos.y;
      } else {
	targetLockedPos.x = targetLockedEnemy.pos.x + targetLockedEnemy.type.batteryType[targetLockedPart].collisionPos.x;
	targetLockedPos.y = targetLockedEnemy.pos.y + targetLockedEnemy.type.batteryType[targetLockedPart].collisionPos.y;
      }
    }
    switch (state) {
    case LOCKING:
      if (cnt >= LOCK_CNT) {
	state = LOCKED;
	SoundManager.playSe(SoundManager.LASER);
	cnt = 0;
      }
      break;
    case LOCKED:
    case FIRED:
    case CANCELED:
      if (state != CANCELED) {
	if (!targetLockedEnemy.isExist ||
	    targetLockedEnemy.shield <= 0 ||
	    (targetLockedPart >= 0 && targetLockedEnemy.battery[targetLockedPart].shield <= 0) ) {
	  state = CANCELED;
	} else {
	  vel.x += (targetLockedPos.x - pos[0].x) * SPEED;
	  vel.y += (targetLockedPos.y - pos[0].y) * SPEED;
	}
	vel.x *= 0.9f;
	vel.y *= 0.9f;
	pos[0].x += (targetLockedPos.x - pos[0].x) * 0.002f * cnt;
	pos[0].y += (targetLockedPos.y - pos[0].y) * 0.002f * cnt;
      } else {
	vel.y += (field.size.y * 2 - pos[0].y) * SPEED;
      }
      for (int index2 = LENGTH - 1; index2 > 0; index2--) {
	pos[index2].x = pos[index2-1].x;
	pos[index2].y = pos[index2-1].y;
      }
      pos[0].x += vel.x;
      pos[0].y += vel.y;
      if (pos[0].y > field.size.y + 5) {
	if (state == CANCELED) {
	  isExist = false;
	  return;
	} else {
	  state = LOCKED;
	  SoundManager.playSe(SoundManager.LASER);
	  reset();
	}
      }
      float d = atan2(pos[1].x - pos[0].x, pos[1].y - pos[0].y);
      manager.addParticle(pos[0], d, 0, SPEED * 32);
      break;
    case HIT_STATE:
      for (int index3 = 1; index3 < LENGTH; index3++) {
	pos[index3].x = pos[index3-1].x;
	pos[index3].y = pos[index3-1].y;
      }
      if (cnt > 5) {
	if (!released) {
	  state = LOCKED;
	  SoundManager.playSe(SoundManager.LASER);
	  reset();
	} else {
	  isExist = false;
	  return;
	}
      }
      break;
    }
    cnt++;
  }

  public override void draw() {
    switch (state) {
    case LOCKING: {
      float y = targetLockedPos.y - (LOCK_CNT - cnt) * 0.5f;
      float angle = (LOCK_CNT - cnt) * 0.1f;
      float radius = (LOCK_CNT - cnt) * 0.5f + 0.8f;
      P47Screen.setRetroParam(GameMath.integer((LOCK_CNT - cnt) / LOCK_CNT), 0.2f);
      for (int index4 = 0; index4 < 3; index4++, angle += 6.28f / 3) {
	P47Screen.drawBoxRetro(targetLockedPos.x + sin(angle) * radius,
			       y + cos(angle) * radius,
			       0.2f, 1, angle + 3.14f / 2);
      }
      break; }
    case LOCKED:
    case FIRED:
    case CANCELED:
    case HIT_STATE:
      float d = 0;
      float r = 0.8f;
      P47Screen.setRetroParam(0, 0.2f);
      for (int index5 = 0; index5 < 3; index5++, d += 6.28f / 3) {
	P47Screen.drawBoxRetro(targetLockedPos.x + sin(d) * r,
			       targetLockedPos.y + cos(d) * r,
			       0.2f, 1, d + 3.14f / 2);
      }
      r = cnt * 0.1f;
      for (int index6 = 0; index6 < LENGTH - 1; index6++, r -= 0.1f) {
	float rr = r;
	if (rr < 0)
	  rr = 0;
	else if (rr > 1)
	  rr = 1;
	P47Screen.setRetroParam(rr, 0.33f);
	P47Screen.drawLineRetro(pos[index6].x, pos[index6].y, pos[index6 + 1].x, pos[index6 + 1].y);
      }
      break;
    }
  }
}

public class LockInitializer: ActorInitializer {

  public Ship ship;
  public Field field;
  public P47GameManager manager;

  public LockInitializer(Ship ship, Field field, P47GameManager manager) {
    this.ship = ship;
    this.field = field;
    this.manager = manager;
  }
}
