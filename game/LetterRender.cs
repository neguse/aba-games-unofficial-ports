// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class LetterRender {

  public static Mesh[] meshes = new Mesh[DISPLAY_LIST_NUM];
  public const float LETTER_WIDTH = 2.1f;
  public const float LETTER_HEIGHT = 3.0f;
  public const int COLOR_NUM = 6;

  public const int LETTER_NUM = 43;
  public const int DISPLAY_LIST_NUM = LETTER_NUM * COLOR_NUM;
  public static Rand rand;

  public static float getWidth(int n ,float s) {
    return n * s * LETTER_WIDTH;
  }

  public static float getHeight(float s) {
    return s * LETTER_HEIGHT;
  }

  public static void drawLetter(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, int n, float x, float y, float s, float d, int c) {
    float[] parent1 = model;
    model = Transform.Translate(model, x, y, 0);
    model = Transform.Scale(model, s, s, s);
    model = Transform.Rotate(model, d, 0, 0, 1);
    {
      Mesh shape2 = LetterRender.meshes[n + c * LETTER_NUM];
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

  public static void drawLetterRev(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, int n, float x, float y, float s, float d, int c) {
    float[] parent1 = model;
    model = Transform.Translate(model, x, y, 0);
    model = Transform.Scale(model, s, -s, s);
    model = Transform.Rotate(model, d, 0, 0, 1);
    {
      Mesh shape2 = LetterRender.meshes[n + c * LETTER_NUM];
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

  public static int convertCharToInt(string c) {
    if (c == "!") return 42;
    int index = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+".IndexOf(c.ToUpper());
    return index < 0 ? 0 : index;
  }

  public static void drawStringFacing(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, string str, float lx, float y, float s, int d, int cl,
				bool rev) {
    if (cl < 0)
      rand.setSeed(-cl);
    lx += LETTER_WIDTH * s / 2;
    y += LETTER_HEIGHT * s / 2;
    float x = lx;
    int idx = 0;
    float ld = 0;
    switch (d) {
    case LetterDirection.TO_RIGHT:
      ld = 0;
      break;
    case LetterDirection.TO_DOWN:
      ld = 90;
      break;
    case LetterDirection.TO_LEFT:
      ld = 180;
      break;
    case LetterDirection.TO_UP:
      ld = 270;
      break;
    }
    for (int letterIndex = 0; letterIndex < str.Length; letterIndex++) {
      string c = str.Substring(letterIndex, 1);
      if (c != " ") {
	idx = convertCharToInt(c);
	if (cl >= 0) {
	  if (rev)
	    drawLetterRev(model, tint, blend, depth, cull, width, idx, x, y, s, ld, cl);
	  else
	    drawLetter(model, tint, blend, depth, cull, width, idx, x, y, s, ld, cl);
	} else {
	  if (rev)
	    drawLetterRev(model, tint, blend, depth, cull, width, idx, x, y, s, ld, rand.nextInt(COLOR_NUM));
	  else
	    drawLetter(model, tint, blend, depth, cull, width, idx, x, y, s, ld, rand.nextInt(COLOR_NUM));
	}
      }
      switch(d) {
      case LetterDirection.TO_RIGHT:
	x += s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_DOWN:
	y += s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_LEFT:
	x -= s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_UP:
	y -= s * LETTER_WIDTH;
	break;
      }
    }
  }

  public static void drawString(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, string str, float lx, float y, float s, int d, int cl) {
    drawStringFacing(model, tint, blend, depth, cull, width, str, lx, y, s, d, cl, false);
  }

  public static void drawNum(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, int num, float lx, float y, float s, int d, int cl) {
    lx += LETTER_WIDTH * s / 2;
    y += LETTER_HEIGHT * s / 2;
    int n = num;
    float x = lx;
    float ld = 0;
    switch (d) {
    case LetterDirection.TO_RIGHT:
      ld = 0;
      break;
    case LetterDirection.TO_DOWN:
      ld = 90;
      break;
    case LetterDirection.TO_LEFT:
      ld = 180;
      break;
    case LetterDirection.TO_UP:
      ld = 270;
      break;
    }
    for (;;) {
      drawLetter(model, tint, blend, depth, cull, width, n % 10, x, y, s, ld, cl);
      switch(d) {
      case LetterDirection.TO_RIGHT:
	x -= s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_DOWN:
	y -= s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_LEFT:
	x += s * LETTER_WIDTH;
	break;
      case LetterDirection.TO_UP:
	y += s * LETTER_WIDTH;
	break;
      }
      n /= 10;
      if (n <= 0) break;
    }
  }

  public static void drawNumSign(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, int num, float lx, float ly, float s, int cl) {
    float dg;
    if (num < 100)
      dg = 2;
    else if (num < 1000)
      dg = 3;
    else if (num < 10000)
      dg = 4;
    else
      dg = 5;
    float x = lx + LETTER_WIDTH * s * dg / 2;
    float y = ly + LETTER_HEIGHT * s / 2;
    int n = num;
    for (;;) {
      drawLetterRev(model, tint, blend, depth, cull, width, n % 10, x, y, s, 0, cl);
      x -= s * LETTER_WIDTH;
      n /= 10;
      if (n <= 0) break;
    }
  }

  public static void drawTime(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, int time, float lx, float y, float s, int cl) {
    int n = time;
    float x = lx;
    for (int i = 0; i < 7; i++) {
      if (i != 4) {
	drawLetter(model, tint, blend, depth, cull, width, n % 10, x, y, s, LetterDirection.TO_RIGHT, cl);
	n /= 10;
      } else {
	drawLetter(model, tint, blend, depth, cull, width, n % 6, x, y, s, LetterDirection.TO_RIGHT, cl);
	n /= 6;
      }
      if ((i & 1) == 1 || i == 0) {
	switch (i) {
	case 3:
	  drawLetter(model, tint, blend, depth, cull, width, 41, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT, cl);
	  break;
	case 5:
	  drawLetter(model, tint, blend, depth, cull, width, 40, x + s * 1.16f, y, s, LetterDirection.TO_RIGHT, cl);
	  break;
	default:
	  break;
	}
	x -= s * LETTER_WIDTH;
      } else {
	x -= s * LETTER_WIDTH * 1.3f;
      }
      if (n <= 0) break;
    }
  }

  public const int LETTER_SHADE = 3;

  public static void appendBox(Mesh mesh, float[] model, float[] tint, float x, float y, float width, float height, float deg, int col) {
    float[] parent1 = model;
    model = Transform.Translate(model, x - width / 2, y - height / 2, 0);
    model = Transform.Rotate(model, deg, 0, 0, 1);
    model = Transform.Scale(model, width, height, 0.3f);
    mesh.Append(Tumiki.meshes[
	       col * Tumiki.SHAPE_NUM +
	       LETTER_SHADE * Tumiki.COLOR_NUM * Tumiki.SHAPE_NUM], model);
    model = parent1;
  }

  public static void buildLetter(Mesh mesh, float[] model, float[] tint, int idx, int c) {
    float x, y, length, size, t;
    float deg;
    for (int i = 0;; i++) {
      deg = (int) spData[idx][i][4];
      if (deg > 99990) break;
      x = -spData[idx][i][0];
      y = -spData[idx][i][1];
      size = spData[idx][i][2];
      length = spData[idx][i][3];
      x *= 1.2f;
      y *= 0.9f;
      size *= 0.5f;
      length *= 0.7f;
      if (size > length) {
	size *= 1.1f;
	length *= 0.7f;
      } else {
	size *= 0.7f;
	length *= 1.1f;
      }
      x = -x;
      y = y;
      deg %= 180;
      deg += rand.nextSignedFloat(16);
      appendBox(mesh, model, tint, x, y, size, length, deg, c);

    }
  }

  public static void createMeshes() {
    rand = new Rand();
    rand.setSeed(0);
    float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null;
    int di = 0;
    for (int j = 0; j < COLOR_NUM; j++) {
      for (int i = 0; i < LETTER_NUM; i++) {
	mesh = new Mesh("LetterRender-" + di.ToString()); meshes[di] = mesh; model = Transform.Identity();
	buildLetter(mesh, model, tint, i, j);

	di++;
      }
    }
  }

  public static void deleteMeshes() {
    meshes = new Mesh[DISPLAY_LIST_NUM];
  }

  public static float[][][] spData =
    new float[][][] {new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.6f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.6f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.6f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.6f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {

     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.1f, 1.15f, 0.45f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.45f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.1f, 0, 0.45f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.1f, 1.15f, 0.45f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.45f, 0.4f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {// F
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0.25f, 0, 0.25f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {-0.6f, -0.75f, 0.25f, 0.3f, 0},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//K
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.45f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.1f, 0, 0.45f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.3f, 1.15f, 0.25f, 0.3f, 90}, new float[] {0.3f, 1.15f, 0.25f, 0.3f, 90},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//P
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0.2f, -0.6f, 0.45f, 0.3f, 360-300},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.1f, 0, 0.45f, 0.3f, 0},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.45f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.4f, 1.15f, 0.45f, 0.3f, 0}, new float[] {0.4f, 1.15f, 0.45f, 0.3f, 0},
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//U
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.5f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.5f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -1.15f, 0.45f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90},
     new float[] {-0.3f, -1.15f, 0.25f, 0.3f, 90}, new float[] {0.3f, -1.15f, 0.25f, 0.3f, 90},
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.4f, 0.6f, 0.85f, 0.3f, 360-120},
     new float[] {0.4f, 0.6f, 0.85f, 0.3f, 360-60},
     new float[] {-0.4f, -0.6f, 0.85f, 0.3f, 360-240},
     new float[] {0.4f, -0.6f, 0.85f, 0.3f, 360-300},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {-0.4f, 0.6f, 0.85f, 0.3f, 360-120},
     new float[] {0.4f, 0.6f, 0.85f, 0.3f, 360-60},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {
     new float[] {0, 1.15f, 0.65f, 0.3f, 0},
     new float[] {0.35f, 0.5f, 0.65f, 0.3f, 360-60},
     new float[] {-0.35f, -0.5f, 0.65f, 0.3f, 360-240},
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//.
     new float[] {0, -1.15f, 0.05f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//_
     new float[] {0, -1.15f, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//-
     new float[] {0, 0, 0.65f, 0.3f, 0},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//+
     new float[] {-0.4f, 0, 0.45f, 0.3f, 0}, new float[] {0.4f, 0, 0.45f, 0.3f, 0},
     new float[] {0, 0.55f, 0.65f, 0.3f, 90},
     new float[] {0, -0.55f, 0.65f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//'
     new float[] {0, 1.0f, 0.4f, 0.2f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//''
     new float[] {-0.19f, 1.0f, 0.4f, 0.2f, 90},
     new float[] {0.2f, 1.0f, 0.4f, 0.2f, 90},
     new float[] {0, 0, 0, 0, 99999},
    },new float[][] {//!
     new float[] {0, 0.25f, 1.1f, 0.3f, 90},
     new float[] {0, -1.0f, 0.3f, 0.3f, 90},
     new float[] {0, 0, 0, 0, 99999},
    }};
}

  public static class LetterDirection { public const int TO_RIGHT = 0; public const int TO_DOWN = 1; public const int TO_LEFT = 2; public const int TO_UP = 3; }
