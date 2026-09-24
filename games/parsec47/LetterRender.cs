// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class LetterRender {

  public static Mesh[] meshes = new Mesh[LETTER_NUM * 2];
  public static int colorIdx = 0;

    public const int WHITE = 0;
  public const int RED = 1;

  public static void changeColor(int c) {
    colorIdx = c * LETTER_NUM;
  }

  public static void drawLetter_5(float[] model, float[] color, Gfx.Blend blend, int n, float x, float y, float s, float d) {
    float[] parent1 = model;
    model = Transform.Translate(model, x, y, 0);
    model = Transform.Scale(model, s, s, s);
    model = Transform.Rotate(model, d, 0, 0, 1);
    {
      Mesh shape1 = meshes[n + colorIdx];
      Gfx.Draw(shape1.count, shape1.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
    model = parent1;
  }

    public const int TO_RIGHT = 0;
  public const int TO_DOWN = 1;
  public const int TO_LEFT = 2;
  public const int TO_UP = 3;

  public static void drawString(float[] model, float[] color, Gfx.Blend blend, string str, float lx, float y, float s, int d) {
    float x = lx;
    string c="";
    int idx=0;
    float ld=0;
    switch (d) {
    case TO_RIGHT:
      ld = 0;
      break;
    case TO_DOWN:
      ld = 90;
      break;
    case TO_LEFT:
      ld = 180;
      break;
    case TO_UP:
      ld = 270;
      break;
    }
    for (int index0 = 0; index0 < str.Length; index0++) {
      c = str.Substring(index0, 1);
      if (c != " ") {
        idx = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ._-+".IndexOf(c.ToUpper());
        if (idx < 0) idx = 37;
	drawLetter_5(model, color, blend, idx, x, y, s, ld);
      }
      switch(d) {
      case TO_RIGHT:
	x += s * 1.7f;
	break;
      case TO_DOWN:
	y += s * 1.7f;
	break;
      case TO_LEFT:
	x -= s * 1.7f;
	break;
      case TO_UP:
	y -= s * 1.7f;
	break;
      }
    }
  }

  public static void drawNum(float[] model, float[] color, Gfx.Blend blend, int num, float lx, float y, float s, int d) {
    int n = num;
    float x = lx;
    float ld=0;
    switch (d) {
    case TO_RIGHT:
      ld = 0;
      break;
    case TO_DOWN:
      ld = 90;
      break;
    case TO_LEFT:
      ld = 180;
      break;
    case TO_UP:
      ld = 270;
      break;
    }
    for (;;) {
      drawLetter_5(model, color, blend, n % 10, x, y, s, ld);
      switch(d) {
      case TO_RIGHT:
	x -= s * 1.7f;
	break;
      case TO_DOWN:
	y -= s * 1.7f;
	break;
      case TO_LEFT:
	x += s * 1.7f;
	break;
      case TO_UP:
	y += s * 1.7f;
	break;
      }
      n = integer(n / 10);
      if (n <= 0) break;
    }
  }

  public static void appendBox(Mesh mesh, float[] color, float x, float y, float width, float height,
			      float r, float g, float b) {
    color = new float[] { r, g, b, 0.5f };
    P47Screen.appendBoxSolid(mesh, color, x - width, y - height, width * 2, height * 2);
    color = new float[] { r, g, b, 1 };
    P47Screen.appendBoxLine(mesh, color, x - width, y - height, width * 2, height * 2);
  }

  public static void appendLetter(Mesh mesh, float[] color, int idx, float r, float g, float b) {
    float x=0, y=0, length=0, size=0, t=0;
    int deg=0;
    for (int index1 = 0;; index1++) {
      deg = GameMath.integer(spData[idx][index1][4]);
      if (deg > 99990) break;
      x = -spData[idx][index1][0];
      y = -spData[idx][index1][1];
      size = spData[idx][index1][2];
      length = spData[idx][index1][3];
      size *= 0.66f;
      length *= 0.6f;
      x = -x;
      y = y;
      deg %= 180;
      if (deg <= 45 || deg > 135)
	appendBox(mesh, color, x, y, size, length, r, g, b);
      else
	appendBox(mesh, color, x, y, length, size, r, g, b);
    }
  }

  public const int LETTER_NUM = 42;

  public static void createMeshes() {
    float[] color = null; Mesh mesh = null;
    for (int index2 = 0; index2 < LETTER_NUM; index2++) {
      mesh = new Mesh("LetterRender-" + (index2).ToString());
      meshes[index2] = mesh;
      appendLetter(mesh, color, index2, 1, 1, 1);

    }
    for (int index3 = 0; index3 < LETTER_NUM; index3++) {
      mesh = new Mesh("LetterRender-" + (LETTER_NUM + index3).ToString());
      meshes[LETTER_NUM + index3] = mesh;
      appendLetter(mesh, color, index3, 1, 0.7f, 0.7f);

    }

  }

  public static void deleteMeshes() {
    meshes = new Mesh[LETTER_NUM * 2];
  }

  public static float[][][] spData = new float[][][] {new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.6f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.6f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.6f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.6f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.1f, 1.15f, 0.45f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.45f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.1f, 0f, 0.45f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.1f, 1.15f, 0.45f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.45f, 0.4f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.25f, 0f, 0.25f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.75f, 0.25f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.45f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.1f, 0f, 0.45f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.3f, 1.15f, 0.25f, 0.3f, 0f}, new float[] {0.3f, 1.15f, 0.25f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0.2f, -0.6f, 0.45f, 0.3f, 60f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.1f, 0f, 0.45f, 0.3f, 0f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.45f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.4f, 1.15f, 0.45f, 0.3f, 0f}, new float[] {0.4f, 1.15f, 0.45f, 0.3f, 0f}, new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.5f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.5f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -1.15f, 0.45f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0.65f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {-0.3f, -1.15f, 0.25f, 0.3f, 0f}, new float[] {0.3f, -1.15f, 0.25f, 0.3f, 0f}, new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.4f, 0.6f, 0.85f, 0.3f, 240f}, new float[] {0.4f, 0.6f, 0.85f, 0.3f, 300f}, new float[] {-0.4f, -0.6f, 0.85f, 0.3f, 120f}, new float[] {0.4f, -0.6f, 0.85f, 0.3f, 60f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.4f, 0.6f, 0.85f, 0.3f, 240f}, new float[] {0.4f, 0.6f, 0.85f, 0.3f, 300f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.15f, 0.65f, 0.3f, 0f}, new float[] {0.35f, 0.5f, 0.65f, 0.3f, 300f}, new float[] {-0.35f, -0.5f, 0.65f, 0.3f, 120f}, new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, -1.15f, 0.05f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, -1.15f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 0f, 0.65f, 0.3f, 0f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.4f, 0f, 0.45f, 0.3f, 0f}, new float[] {0.4f, 0f, 0.45f, 0.3f, 0f}, new float[] {0f, 0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, -0.55f, 0.65f, 0.3f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {0f, 1.0f, 0.4f, 0.2f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}, new float[][] {new float[] {-0.19f, 1.0f, 0.4f, 0.2f, 90f}, new float[] {0.2f, 1.0f, 0.4f, 0.2f, 90f}, new float[] {0f, 0f, 0f, 0f, 99999f}}};
}
