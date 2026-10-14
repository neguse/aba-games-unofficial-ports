// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using Lub;
public abstract class LuminousActor: Actor {
  public abstract void drawLuminous(float[] model, float[] color, Gfx.Blend blend, Mesh target = null);
}
