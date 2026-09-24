// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Ship {

  public static bool isSlow = false;
  public static Mesh[] meshes = new Mesh[3];
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

  public void draw(float[] model, float[] color, Gfx.Blend blend) {
    if (cnt < -INVINCIBLE_CNT || (cnt < 0 && (-cnt % 32) < 16))
      return;
    float[] parent1 = model;
    model = Transform.Translate(model, pos.x, pos.y, 0);
    {
      Mesh shape1 = meshes[1];
      Gfx.Draw(shape1.count, shape1.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = Transform.Rotate(model, bank, 0, 1, 0);
    model = Transform.Translate(model, -0.5f, 0, 0);
    {
      Mesh shape2 = meshes[0];
      Gfx.Draw(shape2.count, shape2.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = Transform.Translate(model, 0.2f, 0.3f, 0.2f);
    {
      Mesh shape3 = meshes[0];
      Gfx.Draw(shape3.count, shape3.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = Transform.Translate(model, 0, 0, -0.4f);
    {
      Mesh shape4 = meshes[0];
      Gfx.Draw(shape4.count, shape4.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = parent1;
    float[] parent12 = model;
    model = Transform.Translate(model, pos.x, pos.y, 0);
    model = Transform.Rotate(model, bank, 0, 1, 0);
    model = Transform.Translate(model, 0.5f, 0, 0);
    {
      Mesh shape5 = meshes[0];
      Gfx.Draw(shape5.count, shape5.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = Transform.Translate(model, -0.2f, 0.3f, 0.2f);
    {
      Mesh shape6 = meshes[0];
      Gfx.Draw(shape6.count, shape6.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = Transform.Translate(model, 0, 0, -0.4f);
    {
      Mesh shape7 = meshes[0];
      Gfx.Draw(shape7.count, shape7.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = parent12;
    for (int index1 = 0; index1 < 6; index1++) {
      float[] parent22 = model;
      model = Transform.Translate(model, pos.x - 0.7f, pos.y - 0.3f, 0);
      model = Transform.Rotate(model, bank, 0, 1, 0);
      model = Transform.Rotate(model, 180.0f / 2 - fireWideDeg * 100, 0, 0, 1);
      model = Transform.Rotate(model, index1 * 180.0f / 3 - ttlCnt * 4, 1, 0, 0);
      model = Transform.Translate(model, 0, 0, 0.7f);
      {
      Mesh shape8 = meshes[2];
      Gfx.Draw(shape8.count, shape8.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
      model = parent22;
      float[] parent30 = model;
      model = Transform.Translate(model, pos.x + 0.7f, pos.y - 0.3f, 0);
      model = Transform.Rotate(model, bank, 0, 1, 0);
      model = Transform.Rotate(model, -180.0f / 2 + fireWideDeg * 100, 0, 0, 1);
      model = Transform.Rotate(model, index1 * 180.0f / 3 - ttlCnt * 4, 1, 0, 0);
      model = Transform.Translate(model, 0, 0, 0.7f);
      {
      Mesh shape9 = meshes[2];
      Gfx.Draw(shape9.count, shape9.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
      model = parent30;
    }
  }

  public static void createMeshes() {
    float[] color = null; Mesh mesh = null;
    mesh = new Mesh("Ship-" + (0).ToString());
    meshes[0] = mesh;
    color = new float[] { 0.5f, 1, 0.5f, 0.2f };
    P47Screen.appendBoxSolid(mesh, color, -0.1f, -0.5f, 0.2f, 1);
    color = new float[] { 0.5f, 1, 0.5f, 0.4f };
    P47Screen.appendBoxLine(mesh, color, -0.1f, -0.5f, 0.2f, 1);

    mesh = new Mesh("Ship-" + (1).ToString());
    meshes[1] = mesh;
    color = new float[] { 1, 0.2f, 0.2f, 1 };
    P47Screen.appendBoxSolid(mesh, color, -0.2f, -0.2f, 0.4f, 0.4f);
    color = new float[] { 1, 0.5f, 0.5f, 1 };
    P47Screen.appendBoxLine(mesh, color, -0.2f, -0.2f, 0.4f, 0.4f);

    mesh = new Mesh("Ship-" + (2).ToString());
    meshes[2] = mesh;
    color = new float[] { 0.7f, 1, 0.5f, 0.3f };
    P47Screen.appendBoxSolid(mesh, color, -0.15f, -0.3f, 0.3f, 0.6f);
    color = new float[] { 0.7f, 1, 0.5f, 0.6f };
    P47Screen.appendBoxLine(mesh, color, -0.15f, -0.3f, 0.3f, 0.6f);


  }

  public static void deleteMeshes() {
    meshes = new Mesh[3];
  }
}
