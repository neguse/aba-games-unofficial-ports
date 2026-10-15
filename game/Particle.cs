// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Particle: Actor {
  public bool isExist;
  public bool getExist() { return isExist; }
  public void setExist(bool value) { isExist = value; }

  public static Rand rand = new Rand();
  public Vector pos;
  public Vector vel;
  public float alpha;
  public float size;
  public int type;
  public int cnt;



  public Actor newActor() {
    return new Particle();
  }

  public void init(ActorInitializer ini) {
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

  public void move() {
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

  public void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null) {
    Mesh mesh = target;
    switch (type) {
    case ParticleType.SMOKE:
      tint = new float[] { 0.8f, 0.8f, 0.8f, alpha };
      break;
    case ParticleType.SPARK:
      if ((cnt & 1) == 0)
	tint = new float[] { 1, 0.4f, 0.2f, alpha };
      else
	tint = new float[] { 1, 1, 0.1f, alpha };
      break;
    }
    mesh.Vertex(pos.x - size, pos.y - size, 0, tint);
    mesh.Vertex(pos.x + size, pos.y - size, 0, tint);
    mesh.Vertex(pos.x + size, pos.y + size, 0, tint);
    mesh.Vertex(pos.x - size, pos.y + size, 0, tint);
  }
}

public class ParticleInitializer: ActorInitializer {
}

public class ParticlePool {
  ActorPool pool;
  public Actor[] actor;

  public ParticlePool(int n, ActorInitializer ini) {
    pool = new ActorPool(n, new Particle(), ini);
    actor = pool.actor;
  }
  public Actor getInstanceForced() { return pool.getInstanceForced(); }
  public void move() { pool.move(); }
  public void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) { pool.draw(model, color, blend, target); }
  public void clear() { pool.clear(); }

  public void add(int n, Vector pos, float deg, float degWdt, float speed, float size, int type) {
    for (int i = 0; i < n; i++) {
      Particle p = (Particle) getInstanceForced();
      p.set(pos, deg, degWdt, speed, size, type);
    }
  }
}

  public static class ParticleType { public const int SMOKE = 0; public const int SPARK = 1; }
