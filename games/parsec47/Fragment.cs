// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Fragment: LuminousActor {

  public const float R = 1, G = 0.8f, B = 0.6f;

  public static P47Rand rand = new P47Rand();
  public const int POINT_NUM = 2;
  public Vector[] pos = new Vector[POINT_NUM];
  public Vector[] vel = new Vector[POINT_NUM];
  public Vector impact;
  public float z;
  public float lumAlp;
  public float retro;
  public int cnt;



  public override Actor newActor() {
    return new Fragment();
  }

  public override void init(ActorInitializer ini) {
    FragmentInitializer fi = (FragmentInitializer) ini;
    for (int index0 = 0; index0 < POINT_NUM; index0++) {
      pos[index0] = new Vector();
      vel[index0] = new Vector();
    }
    impact = new Vector();
  }

  public void set(float x1, float y1, float x2, float y2, float z, float speed, float deg) {
    float r1 = rand.nextFloat(1);
    float r2 = rand.nextFloat(1);
    pos[0].x = x1 * r1 + x2 * (1 - r1);
    pos[0].y = y1 * r1 + y2 * (1 - r1);
    pos[1].x = x1 * r2 + x2 * (1 - r2);
    pos[1].y = y1 * r2 + y2 * (1 - r2);
    for (int index1 = 0; index1 < POINT_NUM; index1++) {
      vel[index1].x = rand.nextSignedFloat(1) * speed;
      vel[index1].y = rand.nextSignedFloat(1) * speed;
    }
    impact.x = sin(deg) * speed * 4;
    impact.y = cos(deg) * speed * 4;
    this.z = z;
    cnt = 32 + rand.nextInt(24);
    lumAlp = 0.8f + rand.nextFloat(0.2f);
    retro = 1;
    isExist = true;
  }

  public override void move() {
    cnt--;
    if (cnt < 0) {
      isExist = false;
      return;
    }
    for (int index2 = 0; index2 < POINT_NUM; index2++) {
      pos[index2].add(vel[index2]);
      pos[index2].add(impact);
      vel[index2].mul(0.98f);
    }
    impact.mul(0.95f);
    lumAlp *= 0.98f;
    retro *= 0.97f;
  }

  public override void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) {
    var mesh = new Mesh("Fragment-draw" + "-" + meshKey);
    P47Screen.setRetroZ(z);
    P47Screen.setRetroParam(retro, 0.2f);
    P47Screen.appendLineRetro(mesh, pos[0].x, pos[0].y, pos[1].x, pos[1].y);

    if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
  }

  public override void drawLuminous(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) {
    Mesh mesh = target;
    if (lumAlp < 0.2f) return;
    color = new float[] { R, G, B, lumAlp };
    mesh.Vertex(pos[0].x, pos[0].y, z, color);
    mesh.Vertex(pos[1].x, pos[1].y, z, color);
  }
}

public class FragmentInitializer: ActorInitializer {
}
