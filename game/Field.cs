// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Field {

  public const float GROUND_LEVEL = -17;
  public const int FIELD_NUM = 5;
  public Vector size;
  public float eyeZ;

  public ActorPool fieldObjs;
  public FieldPattern[] fieldPattern = new FieldPattern[FIELD_NUM];
  public FieldPattern pattern;
  public float[] clearColor;
  public Rand rand;
  public float mnx;
  public float groundOffset;

  public void init() {
    size = new Vector();
    size.x = 21;
    size.y = 16;
    eyeZ = 20;
    fieldPattern = GameData.field;
    FieldObj fieldObjClass = new FieldObj();
    FieldObjInitializer foi = new FieldObjInitializer(this);
    fieldObjs = new ActorPool(64, fieldObjClass, foi);
    rand = new Rand();
  }

  public const float GROUND_Y = 280;
  public Vector[] backMountPos = new Vector[17];

  public void start(int sn) {
    pattern = fieldPattern[sn];
    rand.setSeed(pattern.randSeed);
    clearColor = new float[] { pattern.br, pattern.bg, pattern.bb, 1 };
    foreach (FieldLinePattern flp in pattern.line)
      flp.cnt = flp.interval[rand.nextInt(flp.interval.Length)];
    fieldObjs.clear();
    float x = 0, nx, tx, ty;
    for (int i = 0; i < 4; i++) {
      tx = i * 160 + 80 + rand.nextSignedFloat(30);
      ty = GROUND_Y - 5 - rand.nextFloat(25);
      nx =  160 + i * 160 + rand.nextSignedFloat(30);
      backMountPos[i * 2] = new Vector(x, GROUND_Y);
      backMountPos[i * 2 + 1] = new Vector(tx, ty);
      x = nx;
    }
    for (int i = 0; i < 8; i++) {
      backMountPos[8 + i] = new Vector(backMountPos[i].x + 640, backMountPos[i].y);
    }
    backMountPos[16] = new Vector(1280, GROUND_Y);
    mnx = 0;
  }

  public void move() {
    fieldObjs.move();
    foreach (FieldLinePattern flp in pattern.line) {
      flp.cnt--;
      if (flp.cnt <= 0) {
	FieldObj fo = (FieldObj) fieldObjs.getInstance();
	if (fo != null) {
	  TumikiSet ts = flp.tumikiSet[rand.nextInt(flp.tumikiSet.Length)];
	  if (flp.onGround)
	    fo.setGround(ts, flp.z, pattern.scrollSpeed);
	  else
	    fo.setSky(ts, flp.z, pattern.scrollSpeed / 3 * 2, rand);
	}
	flp.cnt = flp.interval[rand.nextInt(flp.interval.Length)];
      }
    }
    mnx += pattern.scrollSpeed;
    if (mnx >= 640)
      mnx -= 640;
  }

  public void draw(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width) {
    fieldObjs.draw(model, tint, blend);
  }

  public void setGroundY(float y) {
    groundOffset = y;
  }

  public void drawBack(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width) {
    var mesh = new Mesh("field-back");
    int part1 = mesh.vertexCount;
    tint = new float[] { pattern.gr, pattern.gg, pattern.gb, 1 };
    mesh.Vertex(0, 480, 0, tint);
    mesh.Vertex(640, 480, 0, tint);
    float gy1 = 400 - groundOffset;
    mesh.Vertex(640, gy1, 0, tint);
    mesh.Vertex(0, gy1, 0, tint);
    mesh.Vertex(0, gy1, 0, tint);
    mesh.Vertex(640, gy1, 0, tint);
    tint = new float[] { pattern.mrr, pattern.mrg, pattern.mrb, 1 };
    float gy2 = GROUND_Y - groundOffset;
    mesh.Vertex(640, gy2, 0, tint);
    mesh.Vertex(0, gy2, 0, tint);
    mesh.Quads(part1, mesh.vertexCount - part1);
    int idx = 0;
    int part2 = mesh.vertexCount;
    for (int i = 0; i < backMountPos.Length / 2; i++) {
      float x1 = (backMountPos[idx].x - mnx);
      float x2 = (backMountPos[idx + 1].x - mnx);
      float x3 = (backMountPos[idx + 2].x - mnx);
      if (x1 >= 640)
	break;
      if (x3 >= 0) {
	tint = new float[] { pattern.mrr, pattern.mrg, pattern.mrb, 1 };
	mesh.Vertex(x1, backMountPos[idx].y - groundOffset, 0, tint);
	tint = new float[] { pattern.mtr, pattern.mtg, pattern.mtb, 1 };
	mesh.Vertex(x2, backMountPos[idx + 1].y - groundOffset, 0, tint);
	tint = new float[] { pattern.mrr, pattern.mrg, pattern.mrb, 1 };
	mesh.Vertex(x3, backMountPos[idx + 2].y - groundOffset, 0, tint);
      }
      idx += 2;
    }
    for (int vi = part2; vi + 2 < mesh.vertexCount; vi += 3) mesh.Triangle(vi, vi + 1, vi + 2);

    Gfx.Draw(mesh.count, mesh.Bindings(model, tint, width, false),
      new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
  }

  public bool checkHit(Vector p) {
    if (p.x < -size.x || p.x > size.x || p.y < -size.y || p.y > size.y)
      return true;
    return false;
  }

  public bool checkHitInset(Vector p, float space) {
    if (p.x < -size.x + space || p.x > size.x - space ||
	p.y < -size.y + space || p.y > size.y - space)
      return true;
    return false;
  }

  public bool checkHitBounds(Vector p, float xm, float xp, float ym, float yp) {
    if (p.x < -size.x - xp || p.x > size.x - xm ||
	p.y < -size.y - yp || p.y > size.y - ym)
      return true;
    return false;
  }
}

public class FieldObj: Actor {

  public Field field;
  public Vector pos;
  public float speed;
  public float z;
  public float fx;
  public TumikiSet tumikiSet;

  public override Actor newActor() {
    return new FieldObj();
  }

  public override void init(ActorInitializer ini) {
    FieldObjInitializer foi = (FieldObjInitializer) ini;
    field = foi.field;
    pos = new Vector();
  }

  public float calcXLimit() {
    float sz = field.eyeZ - z;
    float x = field.size.x / field.eyeZ * sz * 1.3f;
    return x;
  }

  public float calcHeight() {
    return Field.GROUND_LEVEL - tumikiSet.sizeYm;
  }

  public float calcSkyHeight(Rand rand) {
    float sz = field.eyeZ - z;
    float y = field.size.y / field.eyeZ * sz * 0.8f;
    return rand.nextFloat(y);
  }

  public void set(TumikiSet ts, float z, float s) {
    tumikiSet = ts;
    this.z = z;
    pos.x = calcXLimit();
    fx = -pos.x;
    speed = s;
    isExist = true;
  }

  public void setGround(TumikiSet ts, float z, float s) {
    set(ts, z, s);
    pos.y = calcHeight();
  }

  public void setSky(TumikiSet ts, float z, float s, Rand rand) {
    set(ts, z, s);
    pos.y = calcSkyHeight(rand);
  }

  public override void move() {
    pos.x -= speed;
    if (pos.x < fx)
      isExist = false;
  }

  public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null) {
    bool depth = true; Gfx.Cull cull = Gfx.Cull.Front; float width = 1;
    tumikiSet.drawShade(model, tint, blend, depth, cull, width, pos, z, 2);
  }
}

public class FieldObjInitializer: ActorInitializer {

  public Field field;

  public FieldObjInitializer(Field field) {
    this.field = field;
  }
}
