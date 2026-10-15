// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public interface LuminousActor: Actor {
  void drawLuminous(float[] model, float[] color, Gfx.Blend blend, Mesh target = null);
}
