// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class BulletActor: Actor {

  public P47Bullet bullet;
  public static float totalBulletsSpeed;

  public const float FIELD_SPACE = 0.5f;
  public static int BULLET_DISAPPEAR_CNT = 180;
  public Field field;
  public Ship ship;
  public static int nextId;
  public static Mesh[] meshes = new Mesh[BULLET_COLOR_NUM * (BULLET_SHAPE_NUM + 1)];
  public bool isSimple;
  public bool isTop;
  public bool isVisible;
  public int parser;
  public Vector ppos;
  public const float SHIP_HIT_WIDTH = 0.2f;
  public int cnt;
  public const float RETRO_CNT = 24;
  public float rtCnt;
  public bool shouldBeRemoved;
  public bool backToRetro;

  public static void init_0() {
    nextId = 0;
  }

  public static void resetTotalBulletsSpeed() {
    totalBulletsSpeed = 0;
  }

  public override Actor newActor() {
    return new BulletActor();
  }

  public override void init(ActorInitializer ini) {
    BulletActorInitializer bi = (BulletActorInitializer) ini;
    field = bi.field;
    ship = bi.ship;
    bullet = new P47Bullet(this);
    ppos = new Vector();
    nextId++;
  }

  public void start(float speedRank, int shape, int color, float size, float xReverse) {
    isExist = true;
    isTop = false;
    isVisible = true;
    ppos.x = bullet.pos.x;
    ppos.y = bullet.pos.y;
    bullet.setParam(speedRank, shape, color, size, xReverse);
    cnt = 0;
    rtCnt = 0;
    shouldBeRemoved = false;
    backToRetro = false;
  }

  public void set_11(PatternState[] runner,
		  float x, float y, float deg, float speed, float rank,
		  float speedRank, int shape, int color, float size, float xReverse) {
    bullet.setPattern(runner, x, y, deg, speed, rank);
    bullet.isMorph = false;
    isSimple = false;
    start(speedRank, shape, color, size, xReverse);
  }

  public void set_15(PatternState[] runner,
		  float x, float y, float deg, float speed, float rank,
		  float speedRank, int shape, int color, float size, float xReverse,
		  int[] morph, int morphNum, int morphIdx, int morphCnt) {
    bullet.setPattern(runner, x, y, deg, speed, rank);
    bullet.setMorph(morph, morphNum, morphIdx, morphCnt);
    isSimple = false;
    start(speedRank, shape, color, size, xReverse);
  }

  public void set_10(float x, float y, float deg, float speed, float rank,
		  float speedRank, int shape, int color, float size, float xReverse) {
    bullet.set(x, y, deg, speed, rank);
    bullet.isMorph = false;
    isSimple = true;
    start(speedRank, shape, color, size, xReverse);
  }

  public void setInvisible() {
    isVisible = false;
  }

  public void setTop(int parser) {
    this.parser = parser;
    isTop = true;
    setInvisible();
  }

  public void rewind() {
    bullet.remove();
    PatternState[] runner = P47Bullet.createRunner(parser);
    bullet.setRunner(runner);
    bullet.resetMorph();
  }

  public void remove() {
    shouldBeRemoved = true;
  }

  public void removeForced() {
    if (!isSimple)
      bullet.remove();
    isExist = false;
  }

  public void toRetro() {
    if (!isVisible || backToRetro)
      return;
    backToRetro = true;
    if (rtCnt >= RETRO_CNT)
      rtCnt = RETRO_CNT - 0.1f;
  }

  public void checkShipHit() {
    float bmvx=0, bmvy=0, inaa=0;
    bmvx = ppos.x;
    bmvy = ppos.y;
    bmvx -= bullet.pos.x;
    bmvy -= bullet.pos.y;
    inaa = bmvx * bmvx + bmvy * bmvy;
    if (inaa > 0.00001f) {
      float sofsx=0, sofsy=0, inab=0, hd=0;
      sofsx = ship.pos.x;
      sofsy = ship.pos.y;
      sofsx -= bullet.pos.x;
      sofsy -= bullet.pos.y;
      inab = bmvx * sofsx + bmvy * sofsy;
      if (inab >= 0 && inab <= inaa) {
	hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
	if (hd >= 0 && hd <= SHIP_HIT_WIDTH) {
	  ship.destroyed();
	}
      }
    }
  }

  public override void move() {
    ppos.x = bullet.pos.x;
    ppos.y = bullet.pos.y;
    if (!isSimple) {
      bullet.move();
      if (isTop && bullet.isEnd())
	rewind();
    }
    if (shouldBeRemoved) {
      removeForced();
      return;
    }
    float sr=0;
    if (rtCnt < RETRO_CNT) {
      sr = bullet.speedRank * (0.3f + (rtCnt / RETRO_CNT) * 0.7f);
      if (backToRetro) {
	rtCnt -= sr;
	if (rtCnt <= 0) {
	  removeForced();
	  return;
	}
      } else {
	rtCnt += sr;
      }
      if (ship.cnt < GameMath.integer(-Ship.INVINCIBLE_CNT / 2) && isVisible && rtCnt >= RETRO_CNT) {
	removeForced();
	return;
      }
    } else {
      sr = bullet.speedRank;
      if (cnt > BULLET_DISAPPEAR_CNT)
	toRetro();
    }
    bullet.pos.x +=
      (sin(bullet.deg) * bullet.velocity + bullet.acc.x) * sr * bullet.xReverse;
    bullet.pos.y +=
      (cos(bullet.deg) * bullet.velocity - bullet.acc.y) * sr;
    if (isVisible) {
      totalBulletsSpeed += bullet.velocity * sr;
      if (rtCnt > RETRO_CNT)
	checkShipHit();
      if (field.checkHit_2(bullet.pos, FIELD_SPACE))
	removeForced();
    }
    cnt++;
  }

  public const int BULLET_SHAPE_NUM = 7;
  public const int BULLET_COLOR_NUM = 4;
  public static float[][][] shapePos = new float[][][] {new float[][] {new float[] {-0.5f, -0.5f}, new float[] {0.5f, -0.5f}, new float[] {0f, 1f}}, new float[][] {new float[] {0f, -1f}, new float[] {0.5f, 0f}, new float[] {0f, 1f}, new float[] {-0.5f, 0f}}, new float[][] {new float[] {-0.25f, -0.66f}, new float[] {0.25f, -0.66f}, new float[] {0.25f, 0.66f}, new float[] {-0.25f, 0.66f}}, new float[][] {new float[] {-0.5f, -0.5f}, new float[] {0.5f, -0.5f}, new float[] {0.5f, 0.5f}, new float[] {-0.5f, 0.5f}}, new float[][] {new float[] {-0.25f, -0.5f}, new float[] {0.25f, -0.5f}, new float[] {0.5f, -0.25f}, new float[] {0.5f, 0.25f}, new float[] {0.25f, 0.5f}, new float[] {-0.25f, 0.5f}, new float[] {-0.5f, 0.25f}, new float[] {-0.5f, -0.25f}}, new float[][] {new float[] {-0.66f, -0.46f}, new float[] {0f, 0.86f}, new float[] {0.66f, -0.46f}}, new float[][] {new float[] {-0.5f, -0.5f}, new float[] {0f, -0.5f}, new float[] {0.5f, 0f}, new float[] {0.5f, 0.5f}, new float[] {0f, 0.5f}, new float[] {-0.5f, 0f}}};

  public void drawRetro(float[] model, float[] color, Gfx.Blend blend, float d) {
    var mesh = new Mesh("BulletActor-drawRetro" + "-" + meshKey);
    float rt = 1 - rtCnt / RETRO_CNT;
    P47Screen.setRetroParam(rt, 0.4f * bullet.bulletSize);
    P47Screen.setRetroColor(bulletColor[bullet.color][0],
			    bulletColor[bullet.color][1],
			    bulletColor[bullet.color][2], 1);
    float x=0, y=0, tx=0, px=0, py=0, fx=0, fy=0;
    for (int index0 = 0; index0 < shapePos[bullet.shape].Length; index0++) {
      px = x; py = y;
      tx = shapePos[bullet.shape][index0][0] * bullet.bulletSize;
      y = shapePos[bullet.shape][index0][1] * bullet.bulletSize;
      x = tx * cos(d) - y * sin(d);
      y = tx * sin(d) + y * cos(d);
      if (index0 > 0) {
	P47Screen.appendLineRetro(mesh, px, py, x, y);
      } else {
	fx = x; fy = y;
      }
    }
    P47Screen.appendLineRetro(mesh, x, y, fx, fy);

    if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
  }

  public override void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) {
    if (!isVisible)
      return;
    float d=0;
    switch (bullet.shape) {
    case 0:
    case 2:
    case 5:
      d = -bullet.deg * (bullet.xReverse);
      break;
    case 1:
      d = cnt * 0.14f;
      break;
    case 3:
      d = cnt * 0.23f;
      break;
    case 4:
      d = cnt * 0.33f;
      break;
    case 6:
      d = cnt * 0.08f;
      break;
    }
    float[] parent1 = model;
    model = Transform.Translate(model, bullet.pos.x, bullet.pos.y, 0);
    if (rtCnt >= RETRO_CNT) {
      int di = bullet.color * (BULLET_SHAPE_NUM + 1);
      {
      Mesh shape3 = meshes[di];
      foreach (MeshRange range in shape3.ranges) {
        Gfx.Blend material = range.material == 0 ? blend : (Gfx.Blend)range.material;
        Gfx.Draw(range.count, shape3.Bindings(model, color, 1, material == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
      }
    }
      model = Transform.Rotate(model, rtod(d), 0, 0, 1);
      model = Transform.Scale(model, bullet.bulletSize, bullet.bulletSize, 1);
      {
      Mesh shape6 = meshes[di + 1 + bullet.shape];
      foreach (MeshRange range in shape6.ranges) {
        Gfx.Blend material = range.material == 0 ? blend : (Gfx.Blend)range.material;
        Gfx.Draw(range.count, shape6.Bindings(model, color, 1, material == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
      }
    }
    } else {
      drawRetro(model, color, blend, d);
    }
    model = parent1;
  }

  public const float SHAPE_POINT_SIZE = 0.1f;
  public const float SHAPE_BASE_COLOR_R = 1;
  public const float SHAPE_BASE_COLOR_G = 0.9f;
  public const float SHAPE_BASE_COLOR_B = 0.7f;
  public static float[][] bulletColor = new float[][] {new float[] {1f, 0f, 0f}, new float[] {0.2f, 1f, 0.4f}, new float[] {0.3f, 0.3f, 1f}, new float[] {1f, 1f, 0f}};

  public static void createMeshes() {
    int idx = 0;
    float r=0, g=0, b=0;
    float size = 1.0f, sz=0, sz2=0;
    float[] color = null; int material = 0; Mesh mesh = null;
    for (int index1 = 0; index1 < BULLET_COLOR_NUM; index1++) {
      r = bulletColor[index1][0];
      g = bulletColor[index1][1];
      b = bulletColor[index1][2];
      r += (1 - r) * 0.5f;
      g += (1 - g) * 0.5f;
      b += (1 - b) * 0.5f;
      for (int index2 = 0; index2 < BULLET_SHAPE_NUM + 1; index2++) {
	mesh = new Mesh("BulletActor-" + (idx).ToString());
    meshes[idx] = mesh;
	color = new float[] { r, g, b, 1 };
	switch (index2) {
	case 0:
	  int part1 = mesh.vertexCount; int face1 = mesh.count;
	  mesh.Vertex(-SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE,  0, color);
	  mesh.Vertex( SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE,  0, color);
	  mesh.Vertex( SHAPE_POINT_SIZE,  SHAPE_POINT_SIZE,  0, color);
	  mesh.Vertex(-SHAPE_POINT_SIZE,  SHAPE_POINT_SIZE,  0, color);
	  mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
	  break;
	case 1:
	  sz = size/2;
	  material = (int)Gfx.Blend.None;
	  int part2 = mesh.vertexCount; int face2 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex( sz, -sz,  0, color);
	  mesh.Vertex( 0, size,  0, color);
	  mesh.LineStrip(part2, mesh.vertexCount - part2, true); mesh.AddRange(face2, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.55f };
	  int part3 = mesh.vertexCount; int face3 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex( sz, -sz,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( 0, size,  0, color);
	  mesh.Fan(part3, mesh.vertexCount - part3); mesh.AddRange(face3, material);
	  break;
	case 2:
	  sz = size/2;
	  material = (int)Gfx.Blend.None;
	  int part4 = mesh.vertexCount; int face4 = mesh.count;
	  mesh.Vertex(  0, -size,  0, color);
	  mesh.Vertex( sz,     0,  0, color);
	  mesh.Vertex(  0,  size,  0, color);
	  mesh.Vertex(-sz,     0,  0, color);
	  mesh.LineStrip(part4, mesh.vertexCount - part4, true); mesh.AddRange(face4, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.7f };
	  int part5 = mesh.vertexCount; int face5 = mesh.count;
	  mesh.Vertex(  0, -size,  0, color);
	  mesh.Vertex( sz,     0,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex(  0,  size,  0, color);
	  mesh.Vertex(-sz,     0,  0, color);
	  mesh.Fan(part5, mesh.vertexCount - part5); mesh.AddRange(face5, material);
	  break;
	case 3:
	  sz = size/4; sz2 = size/3*2;
	  material = (int)Gfx.Blend.None;
	  int part6 = mesh.vertexCount; int face6 = mesh.count;
	  mesh.Vertex(-sz, -sz2,  0, color);
	  mesh.Vertex( sz, -sz2,  0, color);
	  mesh.Vertex( sz,  sz2,  0, color);
	  mesh.Vertex(-sz,  sz2,  0, color);
	  mesh.LineStrip(part6, mesh.vertexCount - part6, true); mesh.AddRange(face6, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.45f };
	  int part7 = mesh.vertexCount; int face7 = mesh.count;
	  mesh.Vertex(-sz, -sz2,  0, color);
	  mesh.Vertex( sz, -sz2,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( sz, sz2,  0, color);
	  mesh.Vertex(-sz, sz2,  0, color);
	  mesh.Fan(part7, mesh.vertexCount - part7); mesh.AddRange(face7, material);
	  break;
	case 4:
	  sz = size/2;
	  material = (int)Gfx.Blend.None;
	  int part8 = mesh.vertexCount; int face8 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex( sz, -sz,  0, color);
	  mesh.Vertex( sz,  sz,  0, color);
	  mesh.Vertex(-sz,  sz,  0, color);
	  mesh.LineStrip(part8, mesh.vertexCount - part8, true); mesh.AddRange(face8, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.7f };
	  int part9 = mesh.vertexCount; int face9 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex( sz, -sz,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( sz,  sz,  0, color);
	  mesh.Vertex(-sz,  sz,  0, color);
	  mesh.Fan(part9, mesh.vertexCount - part9); mesh.AddRange(face9, material);
	  break;
	case 5:
	  sz = size/2;
	  material = (int)Gfx.Blend.None;
	  int part10 = mesh.vertexCount; int face10 = mesh.count;
	  mesh.Vertex(-sz/2, -sz,  0, color);
	  mesh.Vertex( sz/2, -sz,  0, color);
	  mesh.Vertex( sz,  -sz/2,  0, color);
	  mesh.Vertex( sz,   sz/2,  0, color);
	  mesh.Vertex( sz/2,  sz,  0, color);
	  mesh.Vertex(-sz/2,  sz,  0, color);
	  mesh.Vertex(-sz,   sz/2,  0, color);
	  mesh.Vertex(-sz,  -sz/2,  0, color);
	  mesh.LineStrip(part10, mesh.vertexCount - part10, true); mesh.AddRange(face10, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.85f };
	  int part11 = mesh.vertexCount; int face11 = mesh.count;
	  mesh.Vertex(-sz/2, -sz,  0, color);
	  mesh.Vertex( sz/2, -sz,  0, color);
	  mesh.Vertex( sz,  -sz/2,  0, color);
	  mesh.Vertex( sz,   sz/2,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( sz/2,  sz,  0, color);
	  mesh.Vertex(-sz/2,  sz,  0, color);
	  mesh.Vertex(-sz,   sz/2,  0, color);
	  mesh.Vertex(-sz,  -sz/2,  0, color);
	  mesh.Fan(part11, mesh.vertexCount - part11); mesh.AddRange(face11, material);
	  break;
	case 6:
	  sz = size*2/3; sz2 = size/5;
	  material = (int)Gfx.Blend.None;
	  int part12 = mesh.vertexCount; int face12 = mesh.count;
	  mesh.Vertex(-sz, -sz+sz2,  0, color);
	  mesh.Vertex( 0, sz+sz2,  0, color);
	  mesh.Vertex( sz, -sz+sz2,  0, color);
	  mesh.LineStrip(part12, mesh.vertexCount - part12); mesh.AddRange(face12, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.55f };
	  int part13 = mesh.vertexCount; int face13 = mesh.count;
	  mesh.Vertex(-sz, -sz+sz2,  0, color);
	  mesh.Vertex( sz, -sz+sz2,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( 0, sz+sz2,  0, color);
	  mesh.Fan(part13, mesh.vertexCount - part13); mesh.AddRange(face13, material);
	  break;
	case 7:
	  sz = size/2;
	  material = (int)Gfx.Blend.None;
	  int part14 = mesh.vertexCount; int face14 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex(  0, -sz,  0, color);
	  mesh.Vertex( sz,   0,  0, color);
	  mesh.Vertex( sz,  sz,  0, color);
	  mesh.Vertex(  0,  sz,  0, color);
	  mesh.Vertex(-sz,   0,  0, color);
	  mesh.LineStrip(part14, mesh.vertexCount - part14, true); mesh.AddRange(face14, material);
	  material = 0;
	  color = new float[] { r, g, b, 0.85f };
	  int part15 = mesh.vertexCount; int face15 = mesh.count;
	  mesh.Vertex(-sz, -sz,  0, color);
	  mesh.Vertex(  0, -sz,  0, color);
	  mesh.Vertex( sz,   0,  0, color);
	  color = new float[] { SHAPE_BASE_COLOR_R, SHAPE_BASE_COLOR_G, SHAPE_BASE_COLOR_B, 0.55f };
	  mesh.Vertex( sz,  sz,  0, color);
	  mesh.Vertex(  0,  sz,  0, color);
	  mesh.Vertex(-sz,   0,  0, color);
	  mesh.Fan(part15, mesh.vertexCount - part15); mesh.AddRange(face15, material);
	  break;
	}

	idx++;
      }
    }
  }

  public static void deleteMeshes() {
    meshes = new Mesh[BULLET_COLOR_NUM * (BULLET_SHAPE_NUM + 1)];
  }
}

public class BulletActorInitializer: ActorInitializer {

  public Field field;
  public Ship ship;

  public BulletActorInitializer(Field field, Ship ship) {
    this.field = field;
    this.ship = ship;
  }
}
