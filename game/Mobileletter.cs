// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class MobileLetter: Actor {

  public static Rand rand = new Rand();
  public Vector pos;
  public float deg;
  public float md;
  public Vector vel;
  public Vector root;
  public float length;
  public int str;
  public int color;
  public float size;
  public int cnt;



  public override Actor newActor() {
    return new MobileLetter();
  }

  public override void init(ActorInitializer ini) {
    pos = new Vector();
    vel = new Vector();
    root = new Vector();
  }

  public void set(float px, float py, float rx, float ry,
		  float l, int st, int cl, float si, int cn) {
    pos.x = px;
    pos.y = py;
    root.x = rx;
    root.y = ry;
    length = l;
    vel.x = rand.nextSignedFloat(2.5f);
    vel.y = rand.nextSignedFloat(1);
    str = st;
    color = cl;
    size = si;
    cnt = cn;
    deg = 0;
    md = rand.nextSignedFloat(10);
    isExist = true;
  }

  public const float GRAVITY = 0.2f;

  public override void move() {
    cnt--;
    if (cnt < 0) {
      pos.x += (root.x - pos.x) * 0.97f;
      deg *= 0.95f;
      pos.y -= 3;
      if (pos.y < root.y - size * LetterRender.LETTER_HEIGHT)
	isExist = false;
      return;
    }
    pos.add(vel);
    vel.y += GRAVITY;
    deg += md;
    deg *= 0.95f;
    if (pos.dist(root) > length) {
      vel.mul(-0.57f);
      md *= -0.4f;
      pos.add(vel);
      pos.x += (root.x - pos.x) * 0.5f;
    }
    deg *= 0.99f;
  }

  public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null) {
    bool depth = false; Gfx.Cull cull = Gfx.Cull.None; float width = 1;
    LetterRender.drawLetter(model, tint, blend, depth, cull, width, str, pos.x, pos.y, size, deg, color);
  }
}

public class MobileLetterInitializer: ActorInitializer {
}

public class MobileLetterPool: ActorPool {

  public Field field;
  public Rand rand;

  public MobileLetterPool(int n, ActorInitializer ini, Field f) : base(n, new MobileLetter(), ini) {
    field = f;
    rand = new Rand();
  }

  public void add(string str, float x, float lgt, float size, int cnt, int col) {
    int color = col;
    if (col < 0)
      rand.setSeed(-col);
    for (int letterIndex = 0; letterIndex < str.Length; letterIndex++) {
      string c = str.Substring(letterIndex, 1);
      MobileLetter ml = (MobileLetter) getInstance();
      if (ml == null)
	return;
      if (c != " ") {
	int idx = LetterRender.convertCharToInt(c);
	if (col < 0)
	  color = rand.nextInt(LetterRender.COLOR_NUM);
	ml.set(x, field.size.y, x, field.size.y, lgt, idx, color, size, cnt);
      }
      x += LetterRender.LETTER_WIDTH * size;
    }
  }
}
