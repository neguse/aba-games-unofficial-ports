// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class LuminousActorPool: ActorPool {
  public LuminousActorPool(int n, Actor act, ActorInitializer ini) : base(n, act, ini) {
  }

  public void drawLuminous() {
    for (int index0 = 0; index0 < actor.Length; index0++) {
      if (actor[index0].isExist)
	((LuminousActor) actor[index0]).drawLuminous();
    }
  }
}
