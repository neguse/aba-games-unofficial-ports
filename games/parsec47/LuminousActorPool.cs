// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using Lub;
public class LuminousActorPool {
  ActorPool pool;
  public Actor[] actor;
  public LuminousActorPool(int n, Actor act, ActorInitializer ini) {
    pool = new ActorPool(n, act, ini);
    actor = pool.actor;
  }
  public Actor getInstanceForced() { return pool.getInstanceForced(); }
  public void move() { pool.move(); }
  public void draw(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) { pool.draw(model, color, blend, target); }

  public void drawLuminous(float[] model, float[] color, Gfx.Blend blend, Mesh target = null) {
    for (int index0 = 0; index0 < actor.Length; index0++) {
      if (actor[index0].getExist())
	((LuminousActor) actor[index0]).drawLuminous(model, color, blend, target);
    }
  }
}
