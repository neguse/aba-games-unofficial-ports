// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class AttractManager {

  public Pad pad;
  public PrefManager prefManager;
  public GameManager gameManager;
  public int cnt;
  public bool btnPrsd;

  public AttractManager(Pad pad, PrefManager pm, GameManager gm) {
    this.pad = pad;
    prefManager = pm;
    gameManager = gm;
  }

  public void startTitle() {
    cnt = 0;
    btnPrsd = true;
  }

  public void moveTitle() {
    cnt++;
    if (cnt <= 16) {
      btnPrsd = true;
    } else {
      if ((pad.getButtonState() & Pad.PAD_BUTTON1) != 0) {
	if (!btnPrsd) {
	  gameManager.startInGameFirst();
	  return;
	}
      } else {
	btnPrsd = false;
      }
    }
  }

  public void drawTitle(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width) {
    if (cnt % 64 < 32)
      LetterRender.drawString(model, tint, blend, depth, cull, width, "PUSH SHOT BUTTON TO START", 250, 390, 7, LetterDirection.TO_RIGHT, 3);
    int c = cnt % 1200;
    if (c < 300) {
      drawTitleBoard(model, tint, blend, depth, cull, width, 70, 50, 16);
    } else {
      drawTitleBoard(model, tint, blend, depth, cull, width, 30, 360, 8);
      int dr = (c - 300) / 30;
      if (dr > PrefManager.RANKING_NUM)
	dr = PrefManager.RANKING_NUM;
      for (int i = 0; i < dr; i++) {
	string rs = (i + 1).ToString();
	float x = 100;
	float y = i * 30 + 32;
	switch (i) {
	case 0:
	  rs += "ST";
	  break;
	case 1:
	  rs += "ND";
	  break;
	case 2:
	  rs += "RD";
	  break;
	case 9:
	  x -= 19;
      rs += "TH";
      break;
	default:
	  rs += "TH";
	  break;
	}
	LetterRender.drawString(model, tint, blend, depth, cull, width, rs, x, y, 9, LetterDirection.TO_RIGHT, 3);
	LetterRender.drawNum(model, tint, blend, depth, cull, width, prefManager.ranking[i].score, 400, y, 9,
			     LetterDirection.TO_RIGHT, 3);
	if (prefManager.ranking[i].stage >= StageManager.STAGE_NUM)
	  rs = "A";
	else
	  rs = (prefManager.ranking[i].stage + 1).ToString();
	LetterRender.drawString(model, tint, blend, depth, cull, width, rs, 500, y, 9, LetterDirection.TO_RIGHT, 3);
      }
    }
  }

  public static int[][] TITLE_PTN =
    new int[][] {
     new int[] {-4,-1,-1,-1,-1,-1,-1,-1,-1,-0,-0,-0,-0,-0,},
     new int[] {-0,-1,19,20,12, 8,10, 8,-1,-0,-0,-0,-0,-0,},
     new int[] {-0,-1,-1,-1,-1,-1,-1,-1,-1,-2,-0,-0,-0,-0,},
     new int[] {-0,-0,-4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-0,},
     new int[] {-0,-0,-0,-1, 5, 8, 6, 7,19, 4,17,18,-1,-0,},
     new int[] {-0,-0,-0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-2,},
     };
  public static int[][] TITLE_CLR =
    new int[][] {
     new int[] { 0, 1, 2, 3, 4, 5, 1, 2, 3,-1,-1,-1,-1,-1,},
     new int[] {-1, 2, 5, 1, 3, 0, 2, 3, 1,-1,-1,-1,-1,-1,},
     new int[] {-1, 5, 2, 3, 0, 4, 1, 4, 5, 2,-1,-1,-1,-1,},
     new int[] {-1,-1, 3, 5, 1, 2, 4, 1, 0, 3, 4, 1, 5,-1,},
     new int[] {-1,-1,-1, 2, 4, 3, 1, 4, 5, 2, 0, 3, 2,-1,},
     new int[] {-1,-1,-1, 1, 5, 0, 3, 1, 3, 4, 5, 2, 1, 3,},
     };

  public void drawTitleBoard(float[] model, float[] tint, Gfx.Blend blend, bool depth, Gfx.Cull cull, float width, float x, float y, float s) {
    float[] parent1 = model;
    model = Transform.Translate(model, x, y, 0);
    model = Transform.Scale(model, s, s, s);
    int tx, ty;
    ty = 0;
    foreach (int[] tpl in TITLE_PTN) {
      tx = 0;
      foreach (int tp in tpl) {
	int c = TITLE_CLR[ty][tx];
	float[] parent2 = model;
	model = Transform.Translate(model, tx * 2, ty * 2, 0);
	if (tp < 0) {
	  int ti = -tp - 1;
	  model = Transform.Scale(model, 0.75f, 0.75f, 0.75f);
	  {
      Mesh shape3 = Tumiki.meshes[ti + c * Tumiki.SHAPE_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape3.count, shape3.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape3.ranges) {
        Gfx.Draw(range.count, shape3.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
	} else if (tp > 0) {
	  int li = tp + 10;
	  model = Transform.Scale(model, 0.9f, 0.9f, 0.9f);
	  {
      Mesh shape4 = LetterRender.meshes[li + c * LetterRender.LETTER_NUM];
      if (cull == Gfx.Cull.None) {
        Gfx.Draw(shape4.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth, Cull = cull, Blend = blend });
      } else foreach (MeshRange range in shape4.ranges) {
        Gfx.Draw(range.count, shape4.Bindings(model, tint, width, blend == Gfx.Blend.Additive, range.first / 3),
          new DrawOpts { Shader = Game.shader, Depth = depth, DepthWrite = depth,
            Cull = range.material == (int)Gfx.Cull.None ? Gfx.Cull.None : cull, Blend = blend });
      }
    }
	}
	model = parent2;
	tx++;
      }
      ty++;
    }
    model = parent1;
  }
}
