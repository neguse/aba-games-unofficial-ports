// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Roll: Actor {

  public bool released;
  public const int LENGTH = 4;
  public Vector[] pos = new Vector[LENGTH];
  public const int NO_COLLISION_CNT = 45;
  public int cnt;

  public const float BASE_LENGTH = 1.0f, BASE_RESISTANCE = 0.8f, BASE_SPRING = 0.2f;
  public const float BASE_SIZE = 0.2f, BASE_DIST = 3;
  public const float SPEED = 0.75f;
  public Vector[] vel = new Vector[LENGTH];
  public Ship ship;
  public Field field;
  public P47GameManager manager;
  public float dist;

  public override Actor newActor() {
    return new Roll();
  }

  public override void init(ActorInitializer ini) {
    RollInitializer ri = (RollInitializer) ini;
    ship = ri.ship;
    field = ri.field;
    manager = ri.manager;
    for (int index0 = 0; index0 < LENGTH; index0++) {
      pos[index0] = new Vector();
      vel[index0] = new Vector();
    }
  }

  public void set() {
    for (int index1 = 0; index1 < LENGTH; index1++) {
      pos[index1].x = ship.pos.x;
      pos[index1].y = ship.pos.y;
      {vel[index1].y = 0;      vel[index1].x = vel[index1].y ;}
    }
    cnt = 0;
    dist = 0;
    released = false;
    isExist = true;
  }

  public override void move() {
    if (released) {
      pos[0].y += SPEED;
      if (pos[0].y > field.size.y) {
	isExist = false;
	return;
      }
      manager.addParticle(pos[0], PI,
			  BASE_SIZE * LENGTH, SPEED / 8);
    } else {
      if (this.dist < BASE_DIST)
	this.dist += BASE_DIST / 90;
      pos[0].x = ship.pos.x + sin(cnt * 0.1f) * this.dist;
      pos[0].y = ship.pos.y + cos(cnt * 0.1f) * this.dist;
    }
    float dist=0, deg=0, v=0;
    for (int index2 = 1; index2 < LENGTH; index2++) {
      pos[index2].x += vel[index2].x;
      pos[index2].y += vel[index2].y;
      vel[index2].x *= BASE_RESISTANCE;
      vel[index2].y *= BASE_RESISTANCE;
      dist = pos[index2].dist(pos[index2 - 1]);
      if (dist <= BASE_LENGTH)
	continue;
      v = (dist - BASE_LENGTH) * BASE_SPRING;
      deg = atan2(pos[index2 - 1].x - pos[index2].x, pos[index2 - 1].y - pos[index2].y);
      vel[index2].x += sin(deg) * v; vel[index2].y += cos(deg) * v;
    }
    cnt++;
  }

  public override void draw() {
    if (released)
      P47Screen.setRetroParam(1, 0.2f);
    else
      P47Screen.setRetroParam(0.5f, 0.2f);
    for (int index3 = 0; index3 < LENGTH; index3++) {
      P47Screen.drawBoxRetro(pos[index3].x, pos[index3].y,
			     BASE_SIZE * (LENGTH - index3),  BASE_SIZE * (LENGTH - index3),
			     cnt * 0.1f);
    }
  }
}

public class RollInitializer: ActorInitializer {

  public Ship ship;
  public Field field;
  public P47GameManager manager;

  public RollInitializer(Ship ship, Field field, P47GameManager manager) {
    this.ship = ship;
    this.field = field;
    this.manager = manager;
  }
}
