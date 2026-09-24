// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Tumiki {

  public const int SHAPE_NUM = 10;
  public const int SHADE_NUM = 4;
  public static Mesh[] meshes = new Mesh[DISPLAY_LIST_NUM];
  public Vector ofs;
  public Vector size, checkHitSize;
  public int shape, color;

  public Barrage[] barrage;
  public static int propellerCnt = 0;

  public const float CHECK_HIT_SIZE_RETIO = 0.7f;

  public Tumiki(int shape, int color,
	      float x, float y, float sx, float sy, float sizeRatio) {
    this.shape = shape;
    this.color = color;
    ofs = new Vector(x * sizeRatio, y * sizeRatio);
    size = new Vector(sx * sizeRatio * 0.5f - 0.15f, sy * sizeRatio * 0.5f - 0.15f);
    checkHitSize = new Vector(size.x + CHECK_HIT_SIZE_RETIO,
			      size.y + CHECK_HIT_SIZE_RETIO);
    barrage = new Barrage[0];
  }



  public BulletActor addTopBullet(int barragePtnIdx, BulletActorPool bullets,
				  BulletTarget target, int type) {
    if (barrage.Length == 0)
      return null;
    Barrage b = barrage[barragePtnIdx];
    return b.addTopBullet(bullets, target, type);
  }

  public static void move() {
    propellerCnt++;
  }

  public const float PROPELLER_OFFSET = 2.2f;
  public const int PROPELLER_SHAPE = 9;
  public const int PROPELLER_SHAPE_FRONT = 14;

  public void drawPropeller(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float deg, int shade) {
    float d = (shape - PROPELLER_SHAPE) * PI / 4 + deg;
    model = Transform.Rotate(model, propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    model = Transform.Rotate(model, rtod(d), 0, 0, 1);
    float[] parent1 = model;
    model = Transform.Translate(model, -size.x * PROPELLER_OFFSET, 0, 0);
    model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
    {
      Mesh shape2 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent1;
    float[] parent3 = model;
    model = Transform.Translate(model, size.x * PROPELLER_OFFSET, 0, 0);
    model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
    {
      Mesh shape4 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent3;
  }

  public void drawPropellerFront(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float deg, int shade) {
    float d = deg;
    model = Transform.Rotate(model, 90, 1, 0, 0);
    model = Transform.Rotate(model, propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    model = Transform.Rotate(model, rtod(d), 0, 0, 1);
    float[] parent1 = model;
    model = Transform.Translate(model, -size.x * PROPELLER_OFFSET, (size.x  + size.y) / 2, (size.x  + size.y) / 2);
    model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
    {
      Mesh shape2 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent1;
    float[] parent3 = model;
    model = Transform.Translate(model, size.x * PROPELLER_OFFSET, (size.x  + size.y) / 2, (size.x  + size.y) / 2);
    model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
    {
      Mesh shape4 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent3;
  }

  public void drawPropellerScaled(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float deg, int shade, float sz) {
    float d = (shape - PROPELLER_SHAPE) * PI / 4 + deg;
    model = Transform.Rotate(model, propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    model = Transform.Rotate(model, rtod(d), 0, 0, 1);
    float[] parent1 = model;
    model = Transform.Translate(model, -size.x * PROPELLER_OFFSET * sz, 0, 0);
    model = Transform.Scale(model, size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    {
      Mesh shape2 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent1;
    float[] parent3 = model;
    model = Transform.Translate(model, size.x * PROPELLER_OFFSET * sz, 0, 0);
    model = Transform.Scale(model, size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    {
      Mesh shape4 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent3;
  }

  public void drawPropellerFrontScaled(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float deg, int shade, float sz) {
    float d = deg;
    model = Transform.Rotate(model, 90, 1, 0, 0);
    model = Transform.Rotate(model, propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    model = Transform.Rotate(model, rtod(d), 0, 0, 1);
    float[] parent1 = model;
    model = Transform.Translate(model, -size.x * PROPELLER_OFFSET * sz,
		 (size.x  + size.y) / 2 * sz, (size.x  + size.y) / 2 * sz);
    model = Transform.Scale(model, size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    {
      Mesh shape2 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent1;
    float[] parent3 = model;
    model = Transform.Translate(model, size.x * PROPELLER_OFFSET * sz,
		 (size.x  + size.y) / 2 * sz, (size.x  + size.y) / 2 * sz);
    model = Transform.Scale(model, size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    {
      Mesh shape4 = Tumiki.meshes[PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    model = parent3;
  }

  public void drawRotated(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade, float deg) {
    float[] parent1 = model;
    float ox = ofs.x * cos(deg) - ofs.y * sin(deg);
    float oy = ofs.x * sin(deg) + ofs.y * cos(deg);
    model = Transform.Translate(model, pos.x + ox, pos.y + oy, z);
    if (shape < PROPELLER_SHAPE) {
      model = Transform.Rotate(model, rtod(deg), 0, 0, 1);
      model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
      {
      Mesh shape2 = Tumiki.meshes[shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFront(model, tint, blend, depth, cull, width, deg, shade);
    } else {
      drawPropeller(model, tint, blend, depth, cull, width, deg, shade);
    }
    model = parent1;
  }

  public void drawScaled(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade, float deg, float sz) {
    float[] parent1 = model;
    float ox = ofs.x * cos(deg) - ofs.y * sin(deg);
    float oy = ofs.x * sin(deg) + ofs.y * cos(deg);
    ox *= sz;
    oy *= sz;
    model = Transform.Translate(model, pos.x + ox, pos.y + oy, z);
    if (shape < PROPELLER_SHAPE) {
      model = Transform.Rotate(model, rtod(deg), 0, 0, 1);
      model = Transform.Scale(model, size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
      {
      Mesh shape2 = Tumiki.meshes[shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFrontScaled(model, tint, blend, depth, cull, width, deg, shade, sz);
    } else {
      drawPropellerScaled(model, tint, blend, depth, cull, width, deg, shade, sz);
    }
    model = parent1;
  }

  public void draw(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, Vector pos, float z, int shade) {
    drawAt(model, tint, blend, depth, cull, width, pos.x, pos.y, z, shade, false, false);
  }

  public void drawAt(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float x, float y, float z, int shade, bool damaged, bool wounded) {
    float[] parent1 = model;
    model = Transform.Translate(model, x + ofs.x, y + ofs.y, z);
    if (shape < PROPELLER_SHAPE) {
      model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
      if (damaged)
	{
      Mesh shape2 = Tumiki.meshes[shape + DAMAGED_COLOR * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape2.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape2.ranges) {
        Gfx.Draw(range.count, shape2.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
      else if (wounded)
	{
      Mesh shape3 = Tumiki.meshes[shape + WOUNDED_COLOR * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape3.count, shape3.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape3.ranges) {
        Gfx.Draw(range.count, shape3.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
      else
	{
      Mesh shape4 = Tumiki.meshes[shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFront(model, tint, blend, depth, cull, width, 0, shade);
    } else {
      drawPropeller(model, tint, blend, depth, cull, width, 0, shade);
    }
    model = parent1;
  }

  public bool checkDistHit(float x, float y, Vector ofs, Vector size) {
    float ox = x - ofs.x;
    float oy = y - ofs.y;
    if (ox > -checkHitSize.x && ox < checkHitSize.x &&
	oy > -checkHitSize.y && oy < checkHitSize.y)
      return true;
    return false;
  }

  public bool checkHit(Vector p, float px, float py) {
    if (shape != 0)
      return false;
    return checkDistHit(p.x - px, p.y - py, ofs, size);
  }

  public const int DISPLAY_LIST_NUM = SHAPE_NUM * COLOR_NUM * SHADE_NUM;
  public const int COLOR_NUM = 12;
  public const int DAMAGED_COLOR = 6;
  public const int WOUNDED_COLOR = 0;
  public static float[][] colorParams =
    new float[][] {
     new float[] {0.9f, 0.6f, 0.6f}, new float[] {0.6f, 0.9f, 0.6f}, new float[] {0.6f, 0.6f, 0.9f},
     new float[] {0.8f, 0.8f, 0.6f}, new float[] {0.8f, 0.6f, 0.8f}, new float[] {0.6f, 0.8f, 0.8f},
     new float[] {0.8f, 0.8f, 0.8f}, new float[] {0.5f, 0.5f, 0.5f},
     new float[] {1, 0.7f, 0.5f}, new float[] {0.7f, 0.9f, 1}, new float[] {1, 0.5f, 0.8f},
     new float[] {0.6f, 0.6f, 0.3f},
    };
  public const float DEPTH = -2;
  public const float LINE_PADDING = 0.03f;

  public static float[] frontColor(int j, int i) {
    switch (i) {
    case 1:
      return new float[] { colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f, 1 };
    case 2:
      return new float[] { colorParams[j][0] * 0.5f, colorParams[j][1] * 0.5f, colorParams[j][2] * 0.5f, 1 };
    default:
      return new float[] { colorParams[j][0] * 0.9f, colorParams[j][1] * 0.9f, colorParams[j][2] * 0.9f, 1 };
    }
  }

  public static float[] sideColor(int j, int i) {
    switch (i) {
    case 0:
      return new float[] { colorParams[j][0] * 0.7f, colorParams[j][1] * 0.7f, colorParams[j][2] * 0.7f, 1 };
    case 1:
      return new float[] { colorParams[j][0] * 0.6f, colorParams[j][1] * 0.6f, colorParams[j][2] * 0.6f, 1 };
    case 2:
      return new float[] { colorParams[j][0] * 0.4f, colorParams[j][1] * 0.4f, colorParams[j][2] * 0.4f, 1 };
    }

    return null;
  }

  public static void createMeshes() {
    float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null;
    int di = 0;
    for (int i = 0; i < SHADE_NUM; i++) {
      for (int j = 0; j < COLOR_NUM; j++) {
	mesh = new Mesh("Tumiki-" + di.ToString()); meshes[di] = mesh; model = Transform.Identity();
	tint = frontColor(j, i);
	int part1 = mesh.vertexCount; int face1 = mesh.count;
	mesh.Vertex(1, 1, 0, tint, model);
	mesh.Vertex(-1, 1, 0, tint, model);
	mesh.Vertex(-1, -1, 0, tint, model);
	mesh.Vertex(1, -1, 0, tint, model);
	if (i < 3) {
	  tint = sideColor(j, i);
	  mesh.Vertex(-1, 1, 0, tint, model);
	  mesh.Vertex(1, 1, 0, tint, model);
	  mesh.Vertex(1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  mesh.Vertex(-1, 1, 0, tint, model);
	  mesh.Vertex(-1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, -1, 0, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  mesh.Vertex(-1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, 1, 0, tint, model);
	  mesh.Vertex(1, -1, 0, tint, model);
	  mesh.Vertex(1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, 1, DEPTH, tint, model);
	}
	mesh.Quads(part1, mesh.vertexCount - part1); mesh.AddRange(face1, (int)Gfx.Cull.Front);
	if (i == 0 || i == 3) {
	  tint = new float[] { colorParams[j][0], colorParams[j][1], colorParams[j][2], 1 };
	  int part2 = mesh.vertexCount; int face2 = mesh.count;
	  if (i == 0) {
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	  } else {
	    mesh.Vertex(1, 1, 0, tint, model);
	    mesh.Vertex(-1, 1, 0, tint, model);
	    mesh.Vertex(-1, -1, 0, tint, model);
	    mesh.Vertex(1, -1, 0, tint, model);
	    mesh.Vertex(1, 1, 0, tint, model);
	  }
	  mesh.LineStrip(part2, mesh.vertexCount - part2); mesh.AddRange(face2, (int)Gfx.Cull.None);
	  if (i == 0) {
	    tint = new float[] { colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f, 1 };
	    int part3 = mesh.vertexCount; int face3 = mesh.count;
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, DEPTH, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, DEPTH, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, -1 - LINE_PADDING, DEPTH, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    for (int vi = part3; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face3, (int)Gfx.Cull.None);
	  }
	}

	di++;
	for (int k = 0; k < 4; k++) {
	  mesh = new Mesh("Tumiki-" + di.ToString()); meshes[di] = mesh; model = Transform.Identity();
	  model = Transform.Rotate(model, -90 * k, 0, 0, 1);
	  tint = frontColor(j, i);
	  int part4 = mesh.vertexCount; int face4 = mesh.count;
	  mesh.Vertex(1, 1, 0, tint, model);
	  mesh.Vertex(-1, 1, 0, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  for (int vi = part4; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part4) % 2), vi + 1 - ((vi - part4) % 2), vi + 2); mesh.AddRange(face4, (int)Gfx.Cull.Front);
	  if (i < 3) {
	    tint = sideColor(j, i);
	    int part5 = mesh.vertexCount; int face5 = mesh.count;
	    mesh.Vertex(-1, 1, 0, tint, model);
	    mesh.Vertex(1, 1, 0, tint, model);
	    mesh.Vertex(1, 1, DEPTH, tint, model);
	    mesh.Vertex(-1, 1, DEPTH, tint, model);
	    mesh.Vertex(-1, -1, 0, tint, model);
	    mesh.Vertex(-1, 1, 0, tint, model);
	    mesh.Vertex(-1, 1, DEPTH, tint, model);
	    mesh.Vertex(-1, -1, DEPTH, tint, model);
	    mesh.Vertex(1, 1, 0, tint, model);
	    mesh.Vertex(-1, -1, 0, tint, model);
	    mesh.Vertex(-1, -1, DEPTH, tint, model);
	    mesh.Vertex(1, 1, DEPTH, tint, model);
	    mesh.Quads(part5, mesh.vertexCount - part5); mesh.AddRange(face5, (int)Gfx.Cull.Front);
	  }
	  if (i == 0 || i == 3) {
	    tint = new float[] { colorParams[j][0], colorParams[j][1], colorParams[j][2], 1 };
	    int part6 = mesh.vertexCount; int face6 = mesh.count;
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.LineStrip(part6, mesh.vertexCount - part6); mesh.AddRange(face6, (int)Gfx.Cull.None);
	    if (i == 0) {
	      tint = new float[] { colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f, 1 };
	      int part7 = mesh.vertexCount; int face7 = mesh.count;
	      mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	      for (int vi = part7; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face7, (int)Gfx.Cull.None);
	    }
	  }

	  di++;
	}
	for (int k = 0; k < 4; k++) {
	  mesh = new Mesh("Tumiki-" + di.ToString()); meshes[di] = mesh; model = Transform.Identity();
	  model = Transform.Rotate(model, -90 * k, 0, 0, 1);
	  tint = frontColor(j, i);
	  int part8 = mesh.vertexCount; int face8 = mesh.count;
	  mesh.Vertex(1, -1, 0, tint, model);
	  mesh.Vertex(0, 1, 0, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  for (int vi = part8; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part8) % 2), vi + 1 - ((vi - part8) % 2), vi + 2); mesh.AddRange(face8, (int)Gfx.Cull.Front);
	  if (i < 3) {
	    tint = sideColor(j, i);
	    int part9 = mesh.vertexCount; int face9 = mesh.count;
	    mesh.Vertex(0, 1, 0, tint, model);
	    mesh.Vertex(1, -1, 0, tint, model);
	    mesh.Vertex(1, -1, DEPTH, tint, model);
	    mesh.Vertex(0, 1, DEPTH, tint, model);
	    mesh.Vertex(-1, -1, 0, tint, model);
	    mesh.Vertex(0, 1, 0, tint, model);
	    mesh.Vertex(0, 1, DEPTH, tint, model);
	    mesh.Vertex(-1, -1, DEPTH, tint, model);
	    mesh.Vertex(1, -1, 0, tint, model);
	    mesh.Vertex(-1, -1, 0, tint, model);
	    mesh.Vertex(-1, -1, DEPTH, tint, model);
	    mesh.Vertex(1, -1, DEPTH, tint, model);
	    mesh.Quads(part9, mesh.vertexCount - part9); mesh.AddRange(face9, (int)Gfx.Cull.Front);
	  }
	  if (i == 0 || i == 3) {
	    tint = new float[] { colorParams[j][0], colorParams[j][1], colorParams[j][2], 1 };
	    int part10 = mesh.vertexCount; int face10 = mesh.count;
	    mesh.Vertex(1 + LINE_PADDING, -1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(0, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.Vertex(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	    mesh.LineStrip(part10, mesh.vertexCount - part10); mesh.AddRange(face10, (int)Gfx.Cull.None);
	    if (i == 0) {
	      tint = new float[] { colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f, 1 };
	      int part11 = mesh.vertexCount; int face11 = mesh.count;
	      mesh.Vertex(1 + LINE_PADDING, -1 - LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(1 + LINE_PADDING, -1 + LINE_PADDING, LINE_PADDING, tint, model);
	      mesh.Vertex(0, 1 + LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(0, 1 + LINE_PADDING, LINE_PADDING, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH, tint, model);
	      mesh.Vertex(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING, tint, model);
	      for (int vi = part11; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face11, (int)Gfx.Cull.None);
	    }
	  }

	  di++;
	}
	mesh = new Mesh("Tumiki-" + di.ToString()); meshes[di] = mesh; model = Transform.Identity();
	tint = frontColor(j, i);
	int part12 = mesh.vertexCount; int face12 = mesh.count;
	mesh.Vertex(1, 1, 0, tint, model);
	mesh.Vertex(-1, 1, 0, tint, model);
	mesh.Vertex(-1, -1, 0, tint, model);
	mesh.Vertex(1, -1, 0, tint, model);
	mesh.Vertex(1, 1, DEPTH, tint, model);
	mesh.Vertex(1, -1, DEPTH, tint, model);
	mesh.Vertex(-1, -1, DEPTH, tint, model);
	mesh.Vertex(-1, 1, DEPTH, tint, model);
	if (i < 3) {
	  tint = sideColor(j, i);
	  mesh.Vertex(-1, 1, 0, tint, model);
	  mesh.Vertex(1, 1, 0, tint, model);
	  mesh.Vertex(1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  mesh.Vertex(-1, 1, 0, tint, model);
	  mesh.Vertex(-1, 1, DEPTH, tint, model);
	  mesh.Vertex(-1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, -1, 0, tint, model);
	  mesh.Vertex(-1, -1, 0, tint, model);
	  mesh.Vertex(-1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, 1, 0, tint, model);
	  mesh.Vertex(1, -1, 0, tint, model);
	  mesh.Vertex(1, -1, DEPTH, tint, model);
	  mesh.Vertex(1, 1, DEPTH, tint, model);
	}
	mesh.Quads(part12, mesh.vertexCount - part12); mesh.AddRange(face12, (int)Gfx.Cull.Front);

	di++;
      }
    }
  }

  public static void deleteMeshes() {
    meshes = new Mesh[DISPLAY_LIST_NUM];
  }
}
