// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Particle: LuminousActor {

  public const float R = 1, G = 1, B = 0.5f;

  public static P47Rand rand = new P47Rand();
  public Vector pos, ppos;
  public Vector vel;
  public float z, mz, pz;
  public float lumAlp;
  public int cnt;



  public override Actor newActor() {
    return new Particle();
  }

  public override void init(ActorInitializer ini) {
    pos = new Vector();
    ppos = new Vector();
    vel = new Vector();
  }

  public void set(Vector p, float d, float ofs, float speed) {
    if (ofs > 0) {
      pos.x = p.x + sin(d) * ofs;
      pos.y = p.y + cos(d) * ofs;
    } else {
      pos.x = p.x;
      pos.y = p.y;
    }
    z = 0;
    float sb = rand.nextFloat(0.5f) + 0.75f;
    vel.x = sin(d) * speed * sb;
    vel.y = cos(d) * speed * sb;
    mz = rand.nextSignedFloat(0.7f);
    cnt = 12 + rand.nextInt(48);
    lumAlp = 0.8f + rand.nextFloat(0.2f);
    isExist = true;
  }

  public override void move() {
    cnt--;
    if (cnt < 0) {
      isExist = false;
      return;
    }
    ppos.x = pos.x; ppos.y = pos.y; pz = z;
    pos.add(vel);
    vel.mul(0.98f);
    z += mz;
    lumAlp *= 0.98f;
  }

  public override void draw() {
    glVertex3f(ppos.x, ppos.y, pz);
    glVertex3f(pos.x, pos.y, z);
  }

  public override void drawLuminous() {
    if (lumAlp < 0.2f) return;
    Screen.setColorAlpha(R, G, B, lumAlp);
    glVertex3f(ppos.x, ppos.y, pz);
    glVertex3f(pos.x, pos.y, z);
  }
}

public class ParticleInitializer: ActorInitializer {
}
