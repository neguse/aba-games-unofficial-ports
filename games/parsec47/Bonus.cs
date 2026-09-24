// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Bonus: Actor {

  public static float rate;
  public static int bonusScore;

  public const float BASE_SPEED = 0.1f;
  public static float speed;
  public const float INHALE_WIDTH = 3;
  public const float ACQUIRE_WIDTH = 1;
  public const int RETRO_CNT = 20;
  public const float BOX_SIZE = 0.4f;
  public static P47Rand rand = new P47Rand();
  public float fieldLimitX, fieldLimitY;
  public Field field;
  public Ship ship;
  public P47GameManager manager;
  public Vector pos;
  public Vector vel;
  public int cnt;
  public bool isDown;
  public bool isInhaled;
  public int inhaleCnt;

  public static void init_0() {
    rand = new P47Rand();
  }

  public static void resetBonusScore() {
    bonusScore = 10;
  }

  public static void setSpeedRate(float r) {
    rate = r;
    speed = BASE_SPEED * rate;
  }

  public override Actor newActor() {
    return new Bonus();
  }

  public override void init(ActorInitializer ini) {
    BonusInitializer bi = (BonusInitializer) ini;
    field = bi.field;
    ship = bi.ship;
    manager = bi.manager;
    pos = new Vector();
    vel = new Vector();
    fieldLimitX = field.size.x / 6 * 5;
    fieldLimitY = field.size.y / 10 * 9;
  }

  public void set(Vector p, Vector ofs) {
    pos.x = p.x;
    pos.y = p.y;
    if (ofs != null) {
      pos.x += ofs.x;
      pos.y += ofs.y;
    }
    vel.x = rand.nextSignedFloat(0.07f);
    vel.y = rand.nextSignedFloat(0.07f);
    cnt = 0;
    inhaleCnt = 0;
    isDown = true;
    isInhaled = false;
    isExist = true;
  }

  public void missBonus() {
    resetBonusScore();
  }

  public void getBonus() {
    SoundManager.playSe(SoundManager.GET_BONUS);
    manager.addScore(bonusScore);
    if (bonusScore < 1000)
      bonusScore += 10;
  }

  public override void move() {
    pos.x += vel.x;
    pos.y += vel.y;
    vel.x -= vel.x / 50;
    if (pos.x > fieldLimitX) {
      pos.x = fieldLimitX;
      if (vel.x > 0)
	vel.x = -vel.x;
    } else if (pos.x < -fieldLimitX) {
      pos.x = -fieldLimitX;
      if (vel.x < 0)
	vel.x = -vel.x;
    }
    if (isDown) {
      vel.y += (-speed - vel.y) / 50;
      if (pos.y < -fieldLimitY) {
	isDown = false;
	pos.y = -fieldLimitY;
	vel.y = speed;
      }
    } else {
      vel.y += (speed - vel.y) / 50;
      if (pos.y > fieldLimitY) {
	missBonus();
	isExist = false;
	return;
      }
    }
    cnt++;
    if (cnt < RETRO_CNT)
      return;
    float d = pos.dist(ship.pos);
    if (d < ACQUIRE_WIDTH * (1 + (float) inhaleCnt * 0.2f) && ship.cnt >= -Ship.INVINCIBLE_CNT) {
      getBonus();
      isExist = false;
      return;
    }
    if (isInhaled) {
      inhaleCnt++;
      float ip = (INHALE_WIDTH - d) / 48;
      if (ip < 0.025f)
	ip = 0.025f;
      vel.x += (ship.pos.x - pos.x) * ip;
      vel.y += (ship.pos.y - pos.y) * ip;
      if (ship.cnt < -Ship.INVINCIBLE_CNT) {
	isInhaled = false;
	inhaleCnt = 0;
      }
    } else {
      if (d < INHALE_WIDTH && ship.cnt >= -Ship.INVINCIBLE_CNT)
	isInhaled = true;
    }
  }

  public override void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) {
    var mesh = new Mesh("Bonus-draw" + "-" + meshKey);
    float retro=0;
    if (cnt < RETRO_CNT)
      retro = 1 - (float) cnt / RETRO_CNT;
    else
      retro = 0;
    float d = cnt * 0.1f;
    float ox = sin(d) * 0.3f;
    float oy = cos(d) * 0.3f;
    if (retro > 0) {
      P47Screen.setRetroParam(retro, 0.2f);
      P47Screen.appendBoxRetro(mesh, pos.x - ox, pos.y - oy, BOX_SIZE / 2, BOX_SIZE / 2, 0);
      P47Screen.appendBoxRetro(mesh, pos.x + ox, pos.y + oy, BOX_SIZE / 2, BOX_SIZE / 2, 0);
      P47Screen.appendBoxRetro(mesh, pos.x - oy, pos.y + ox, BOX_SIZE / 2, BOX_SIZE / 2, 0);
      P47Screen.appendBoxRetro(mesh, pos.x + oy, pos.y - ox, BOX_SIZE / 2, BOX_SIZE / 2, 0);
    } else {
      if (isInhaled)
	color = new float[] { 0.8f, 0.6f, 0.4f, 0.7f };
      else if (isDown)
	color = new float[] { 0.4f, 0.9f, 0.6f, 0.7f };
      else
	color = new float[] { 0.8f, 0.9f, 0.5f, 0.7f };
      P47Screen.appendBoxLine(mesh, color, pos.x - ox - BOX_SIZE / 2, pos.y - oy - BOX_SIZE / 2,
			    BOX_SIZE, BOX_SIZE);
      P47Screen.appendBoxLine(mesh, color, pos.x + ox - BOX_SIZE / 2, pos.y + oy - BOX_SIZE / 2,
			    BOX_SIZE, BOX_SIZE);
      P47Screen.appendBoxLine(mesh, color, pos.x - oy - BOX_SIZE / 2, pos.y + ox - BOX_SIZE / 2,
			    BOX_SIZE, BOX_SIZE);
      P47Screen.appendBoxLine(mesh, color, pos.x + oy - BOX_SIZE / 2, pos.y - ox - BOX_SIZE / 2,
			    BOX_SIZE, BOX_SIZE);
    }

    if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
  }
}

public class BonusInitializer: ActorInitializer {

  public Field field;
  public Ship ship;
  public P47GameManager manager;

  public BonusInitializer(Field field, Ship ship, P47GameManager manager) {
    this.field = field;
    this.ship = ship;
    this.manager = manager;
  }
}
