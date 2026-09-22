// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Ship {

  public static bool isSlow = false;
  public static int displayListIdx;
  public Vector pos;
  public const float SIZE = 0.3f;
  public bool restart;
  public const int RESTART_CNT = 300;
  public const int INVINCIBLE_CNT = 228;
  public int cnt;

  public static P47Rand rand = new P47Rand();
  public Pad pad;
  public Field field;
  public P47GameManager manager;
  public Vector ppos;
  public const float INITIAL_SPEED = 0.6f;
  public const float SLOW_BASE_SPEED = 0.3f;
  public float baseSpeed, slowSpeed;

  public float speed;
  public Vector vel;
  public const float BANK_BASE = 50;
  public float bank;
  public Vector firePos;
  public float fireWideDeg;
  public const float FIRE_WIDE_BASE_DEG = 0.7f;
  public const float FIRE_NARROW_BASE_DEG = 0.5f;
  public int fireCnt;
  public const float TURRET_INTERVAL_LENGTH = 0.2f;
  public int ttlCnt;
  public const float FIELD_SPACE = 1.5f;
  public float fieldLimitX, fieldLimitY;
  public int rollLockCnt;
  public bool rollCharged;



  public void init(Pad pad, Field field, P47GameManager manager) {
    this.pad = pad;
    this.field = field;
    this.manager = manager;
    pos = new Vector();
    ppos = new Vector();
    vel = new Vector();
    firePos = new Vector();
    ttlCnt = 0;
    fieldLimitX = field.size.x - FIELD_SPACE;
    fieldLimitY = field.size.y - FIELD_SPACE;
  }

  public void start() {
    {pos.x = 0;    ppos.x = pos.x ;}
    {pos.y = -field.size.y / 2;    ppos.y = pos.y ;}
    {vel.y = 0;    vel.x = vel.y ;}
    speed = INITIAL_SPEED;
    fireWideDeg = FIRE_WIDE_BASE_DEG;
    restart = true;
    cnt = -INVINCIBLE_CNT;
    fireCnt = 0;
    rollLockCnt = 0;
    bank = 0;
    rollCharged = false;
    Bonus.resetBonusScore();
  }

  public void setSpeedRate(float rate) {
    if (!isSlow)
      baseSpeed = INITIAL_SPEED * rate;
    else
      baseSpeed = INITIAL_SPEED * 0.7f;
    slowSpeed = SLOW_BASE_SPEED * rate;
  }

  public void destroyed() {
    if (cnt <= 0)
      return;
    SoundManager.playSe(SoundManager.SHIP_DESTROYED);
    manager.shipDestroyed();
    manager.addFragments(30, pos.x, pos.y, pos.x, pos.y, 0, 0.08f, PI);
    for (int index0 = 0; index0 < 45; index0++)
      manager.addParticle(pos, rand.nextFloat(PI * 2), 0, 0.6f);
    start();
    cnt = -RESTART_CNT;
  }

  public void move() {
    cnt++;
    if (cnt < -INVINCIBLE_CNT) {
      return;
    }
    if (cnt == 0)
      restart = false;
    int btn = pad.getButtonState();
    if ((btn & Pad.PAD_BUTTON2) != 0) {
      speed += (slowSpeed - speed) * 0.2f;
      fireWideDeg += (FIRE_NARROW_BASE_DEG - fireWideDeg) * 0.1f;
      rollLockCnt++;
      if (manager.mode == P47GameManager.ROLL) {
	if (rollLockCnt % 15 == 0) {
	  manager.addRoll();
	  SoundManager.playSe(SoundManager.ROLL_CHARGE);
	  rollCharged = true;
	}
      } else {
	if (rollLockCnt % 10 == 0) {
	  manager.addLock();
	}
      }
    } else {
      speed += (baseSpeed - speed) * 0.2f;
      fireWideDeg += (FIRE_WIDE_BASE_DEG - fireWideDeg) * 0.1f;
      if (manager.mode == P47GameManager.ROLL) {
	if (rollCharged) {
	  rollLockCnt = 0;
	  manager.releaseRoll();
	  SoundManager.playSe(SoundManager.ROLL_RELEASE);
	  rollCharged = false;
	}
      } else {
	rollLockCnt = 0;
	manager.releaseLock();
      }
    }
    int ps = pad.getPadState();
    {vel.y = 0;    vel.x = vel.y ;}
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
    ppos.x = pos.x;
    ppos.y = pos.y;
    pos.x += vel.x;
    pos.y += vel.y;
    bank += (vel.x * BANK_BASE - bank) * 0.1f;
    if (pos.x < -fieldLimitX)
      pos.x = -fieldLimitX;
    else if (pos.x > fieldLimitX)
      pos.x = fieldLimitX;
    if (pos.y < -fieldLimitY)
      pos.y = -fieldLimitY;
    else if (pos.y > fieldLimitY)
      pos.y = fieldLimitY;
    if ((btn & Pad.PAD_BUTTON1) != 0) {
      float td=0;
      switch (fireCnt % 4) {
      case 0:
	firePos.x = pos.x + TURRET_INTERVAL_LENGTH;
	firePos.y = pos.y;
	td = 0;
	break;
      case 1:
	firePos.x = pos.x + TURRET_INTERVAL_LENGTH;
	firePos.y = pos.y;
	td = fireWideDeg * (GameMath.integer(fireCnt / 4) % 5) * 0.2f;
	break;
      case 2:
	firePos.x = pos.x - TURRET_INTERVAL_LENGTH;
	firePos.y = pos.y;
	td = 0;
	break;
      case 3:
	firePos.x = pos.x - TURRET_INTERVAL_LENGTH;
	firePos.y = pos.y;
	td = - fireWideDeg * (GameMath.integer(fireCnt / 4) % 5) * 0.2f;
	break;
      }
      manager.addShot(firePos, td);
      SoundManager.playSe(SoundManager.SHOT);
      fireCnt++;
    }
    P47Bullet.target.x = pos.x;
    P47Bullet.target.y = pos.y;
    ttlCnt++;
  }

  public void draw() {
    if (cnt < -INVINCIBLE_CNT || (cnt < 0 && (-cnt % 32) < 16))
      return;
    glPushMatrix();
    glTranslatef(pos.x, pos.y, 0);
    glCallList(displayListIdx + 1);
    glRotatef(bank, 0, 1, 0);
    glTranslatef(-0.5f, 0, 0);
    glCallList(displayListIdx);
    glTranslatef(0.2f, 0.3f, 0.2f);
    glCallList(displayListIdx);
    glTranslatef(0, 0, -0.4f);
    glCallList(displayListIdx);
    glPopMatrix();
    glPushMatrix();
    glTranslatef(pos.x, pos.y, 0);
    glRotatef(bank, 0, 1, 0);
    glTranslatef(0.5f, 0, 0);
    glCallList(displayListIdx);
    glTranslatef(-0.2f, 0.3f, 0.2f);
    glCallList(displayListIdx);
    glTranslatef(0, 0, -0.4f);
    glCallList(displayListIdx);
    glPopMatrix();
    for (int index1 = 0; index1 < 6; index1++) {
      glPushMatrix();
      glTranslatef(pos.x - 0.7f, pos.y - 0.3f, 0);
      glRotatef(bank, 0, 1, 0);
      glRotatef(180.0f / 2 - fireWideDeg * 100, 0, 0, 1);
      glRotatef(index1 * 180.0f / 3 - ttlCnt * 4, 1, 0, 0);
      glTranslatef(0, 0, 0.7f);
      glCallList(displayListIdx + 2);
      glPopMatrix();
      glPushMatrix();
      glTranslatef(pos.x + 0.7f, pos.y - 0.3f, 0);
      glRotatef(bank, 0, 1, 0);
      glRotatef(-180.0f / 2 + fireWideDeg * 100, 0, 0, 1);
      glRotatef(index1 * 180.0f / 3 - ttlCnt * 4, 1, 0, 0);
      glTranslatef(0, 0, 0.7f);
      glCallList(displayListIdx + 2);
      glPopMatrix();
    }
  }

  public static void createDisplayLists() {
    displayListIdx = glGenLists(3);
    glNewList(displayListIdx, GL_COMPILE);
    Screen.setColorAlpha(0.5f, 1, 0.5f, 0.2f);
    P47Screen.drawBoxSolid(-0.1f, -0.5f, 0.2f, 1);
    Screen.setColorAlpha(0.5f, 1, 0.5f, 0.4f);
    P47Screen.drawBoxLine(-0.1f, -0.5f, 0.2f, 1);
    glEndList();
    glNewList(displayListIdx + 1, GL_COMPILE);
    Screen.setColorAlpha(1, 0.2f, 0.2f, 1);
    P47Screen.drawBoxSolid(-0.2f, -0.2f, 0.4f, 0.4f);
    Screen.setColorAlpha(1, 0.5f, 0.5f, 1);
    P47Screen.drawBoxLine(-0.2f, -0.2f, 0.4f, 0.4f);
    glEndList();
    glNewList(displayListIdx + 2, GL_COMPILE);
    Screen.setColorAlpha(0.7f, 1, 0.5f, 0.3f);
    P47Screen.drawBoxSolid(-0.15f, -0.3f, 0.3f, 0.6f);
    Screen.setColorAlpha(0.7f, 1, 0.5f, 0.6f);
    P47Screen.drawBoxLine(-0.15f, -0.3f, 0.3f, 0.6f);
    glEndList();
  }

  public static void deleteDisplayLists() {
    glDeleteLists(displayListIdx, 3);
  }
}
