// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Particle: Actor {

  public static Rand rand = new Rand();
  public Vector pos;
  public Vector vel;
  public float alpha;
  public float size;
  public int type;
  public int cnt;



  public override Actor newActor() {
    return new Particle();
  }

  public override void init(ActorInitializer ini) {
    pos = new Vector();
    vel = new Vector();
  }

  public void set(Vector p, float deg, float od, float speed, float s, int t) {
    pos.x = p.x;
    pos.y = p.y;
    float sb = rand.nextFloat(0.5f) + 0.75f;
    float d = deg + rand.nextSignedFloat(od);
    vel.x = -sin(d) * speed * sb;
    vel.y = -cos(d) * speed * sb;
    cnt = 16 + rand.nextInt(16);
    alpha = 0.8f + rand.nextFloat(0.2f);
    sb = rand.nextFloat(0.5f) + 0.75f;
    size = s * sb;
    type = t;
    isExist = true;
  }

  public override void move() {
    cnt--;
    if (cnt < 0) {
      isExist = false;
      return;
    }
    pos.add(vel);
    vel.mul(0.9f);
    alpha *= 0.9f;
    switch (type) {
    case ParticleType.SMOKE:
      size *= 1.025f;
      break;
    case ParticleType.SPARK:
      size *= 1.01f;
      break;
    }
  }

  public override void draw() {
    switch (type) {
    case ParticleType.SMOKE:
      Screen.setColorAlpha(0.8f, 0.8f, 0.8f, alpha);
      break;
    case ParticleType.SPARK:
      if ((cnt & 1) == 0)
	Screen.setColorAlpha(1, 0.4f, 0.2f, alpha);
      else
	Screen.setColorAlpha(1, 1, 0.1f, alpha);
      break;
    }
    glVertex3f(pos.x - size, pos.y - size, 0);
    glVertex3f(pos.x + size, pos.y - size, 0);
    glVertex3f(pos.x + size, pos.y + size, 0);
    glVertex3f(pos.x - size, pos.y + size, 0);
  }
}

public class ParticleInitializer: ActorInitializer {
}

public class ParticlePool: ActorPool {

  public ParticlePool(int n, ActorInitializer ini) : base(n, new Particle(), ini) {
  }

  public void add(int n, Vector pos, float deg, float degWdt, float speed, float size, int type) {
    for (int i = 0; i < n; i++) {
      Particle p = (Particle) getInstanceForced();
      p.set(pos, deg, degWdt, speed, size, type);
    }
  }
}

  public static class ParticleType { public const int SMOKE = 0; public const int SPARK = 1; }
