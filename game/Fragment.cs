// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Fragment: Actor {

  public static Rand rand = new Rand();
  public Vector pos;
  public Vector vel;
  public Vector size;
  public float deg;
  public float md;
  public int shape, color;
  public int cnt;



  public override Actor newActor() {
    return new Fragment();
  }

  public override void init(ActorInitializer ini) {
    pos = new Vector();
    vel = new Vector();
    size = new Vector();
  }

  public void set(int sh, int cl, float x, float y, Vector s) {
    shape = sh;
    color = cl;
    pos.x = x;
    pos.y = y;
    size.x = s.x;
    size.y = s.y;
    vel.x = rand.nextSignedFloat(0.2f);
    vel.y = rand.nextSignedFloat(0.1f);
    deg = 0;
    md = rand.nextSignedFloat(8);
    cnt = 32 + rand.nextInt(48);
    isExist = true;
  }

  public const float GRAVITY = 0.012f;

  public override void move() {
    cnt--;
    if (cnt < 0) {
      isExist = false;
      return;
    }
    pos.add(vel);
    vel.y -= GRAVITY;
    deg += md;
  }

  public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null) {
    bool depth = false; Gfx.Cull cull = Gfx.Cull.Front; float width = 1;
    if (cnt < 16) {
      if ((cnt & 1) == 1)
	return;
    } else if (cnt < 32) {
      if ((cnt % 3) == 2)
	return;
    } else {
      if ((cnt % 4) == 3)
	return;
    }
    float[] parent1 = model;
    model = Transform.Translate(model, pos.x, pos.y, -1);
    model = Transform.Rotate(model, deg, 0, 0, 1);
    model = Transform.Scale(model, size.x, size.y, (size.x  + size.y) / 2);
    {
      Mesh shape2 = Tumiki.meshes[shape + color * Tumiki.SHAPE_NUM +
	       Tumiki.SHAPE_NUM * Tumiki.COLOR_NUM];
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
  }
}

public class FragmentInitializer: ActorInitializer {
}
