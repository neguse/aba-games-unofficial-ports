// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Shot: Actor {

  public Vector pos;
  public const float SPEED = 1;

  public const float FIELD_SPACE = 1;
  public static int displayListIdx;
  public Field field;
  public Vector vel;
  public float deg;
  public int cnt;
  public const int RETRO_CNT = 4;

  public override Actor newActor() {
    return new Shot();
  }

  public override void init(ActorInitializer ini) {
    ShotInitializer si = (ShotInitializer) ini;
    field = si.field;
    pos = new Vector();
    vel = new Vector();
  }

  public void set(Vector p, float d) {
    pos.x = p.x; pos.y = p.y;
    deg = d;
    vel.x = sin(deg) * SPEED;
    vel.y = cos(deg) * SPEED;
    cnt = 0;
    isExist = true;
  }

  public override void move() {
    pos.x += vel.x;
    pos.y += vel.y;
    if (field.checkHit_2(pos, FIELD_SPACE))
      isExist = false;
    cnt++;
  }

  public override void draw() {
    float r=0;
    if (cnt > RETRO_CNT)
      r = 1;
    else
      r = GameMath.integer(cnt / RETRO_CNT);
    P47Screen.setRetroParam(r, 0.2f);
    P47Screen.drawBoxRetro(pos.x, pos.y, 0.2f, 1, deg);
  }
}

public class ShotInitializer: ActorInitializer {

  public Field field;

  public ShotInitializer(Field field) {
    this.field = field;
  }
}
