// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class ScoreSign: Actor {

  public Vector pos;
  public float my;
  public float size;
  public int num;
  public int cnt;

  public override Actor newActor() {
    return new ScoreSign();
  }

  public override void init(ActorInitializer ini) {
    pos = new Vector();
  }

  public const float FIELD_X = 14.5f;

  public void set(Vector p, int n, float s) {
    pos.x = p.x;
    pos.y = p.y;
    if (pos.x > FIELD_X - s * 2)
      pos.x = FIELD_X - s * 2;
    num = n;
    size = s;
    my = 0.3f;
    cnt = 60;
    isExist = true;
  }

  public override void move() {
    cnt--;
    if (cnt < 0) {
      isExist = false;
      return;
    }
    pos.y += my;
    my *= 0.92f;
  }

  public override void draw() {
    LetterRender.drawNumSign(num, pos.x, pos.y, size, 3);
  }
}

public class ScoreSignInitializer: ActorInitializer {
}
