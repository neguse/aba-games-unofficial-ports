// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Title {
  public Mesh logo;

  public Pad pad;
  public P47GameManager gameManager;
  public P47PrefManager prefManager;
  public Field field;
  public int[][] slotNum = new int[][] {new int[] {0,0,0,0,0}, new int[] {0,0,0,0,0}};
  public int[][] startReachedParsec = new int[][] {new int[] {0,0,0,0}, new int[] {0,0,0,0}};
  public int curX, curY;
  public int mode;
  public const int BOX_COUNT = 16;
  public int boxCnt;


  public void init(Pad p, P47GameManager gm, P47PrefManager pm, Field fl) {
    pad = p;
    gameManager = gm;
    prefManager = pm;
    field = fl;
    gameManager.difficulty = prefManager.selectedDifficulty;
    gameManager.parsecSlot = prefManager.selectedParsecSlot;
    gameManager.mode = prefManager.selectedMode;

  }

  public void close() {

  }

  public void start() {
    for (int index0 = 0; index0 < P47PrefManager.MODE_NUM; index0++) {
      for (int index1 = 0; index1 < P47PrefManager.DIFFICULTY_NUM; index1++) {
	slotNum[index0][index1] = GameMath.integer((prefManager.reachedParsec[index0][index1] - 1) / 10) + 1;
	startReachedParsec[index0][index1] = slotNum[index0][index1] * 10 + 1;
	if (slotNum[index0][index1] > 10)
	  slotNum[index0][index1] = 10;
      }
      slotNum[index0][P47PrefManager.DIFFICULTY_NUM] = 1;
    }
    curX = gameManager.parsecSlot;
    curY = gameManager.difficulty;
    mode = gameManager.mode;
    boxCnt = BOX_COUNT;
    field.setColor(mode);
  }

  public int getStartParsec(int dif, int psl) {
    if (psl < P47PrefManager.REACHED_PARSEC_SLOT_NUM - 1) {
      return psl * 10 + 1;
    } else {
      int rp = prefManager.reachedParsec[mode][dif];
      rp--;
      rp = integer(rp / 10);
      rp *= 10;
      rp++;
      return rp;
    }
  }

  public bool padPrsd = true;

  public void move() {
    int ps = pad.getPadState();
    if (!padPrsd) {
      if ((ps & Pad.PAD_DOWN) != 0) {
	curY++;
	if (curY >= slotNum[mode].Length)
	  curY = 0;
	if (curX >= slotNum[mode][curY])
	  curX = slotNum[mode][curY] - 1;
      } else if ((ps & Pad.PAD_UP) != 0) {
	curY--;
	if (curY < 0)
	  curY = slotNum[mode].Length - 1;
	if (curX >= slotNum[mode][curY])
	  curX = slotNum[mode][curY] - 1;
      } else if ((ps & Pad.PAD_RIGHT) != 0) {
	curX++;
	if (curX >= slotNum[mode][curY])
	  curX = 0;
      } else if ((ps & Pad.PAD_LEFT) != 0) {
	curX--;
	if (curX < 0)
	  curX = slotNum[mode][curY] - 1;
      }
      if (ps != 0) {
	boxCnt = BOX_COUNT;
	padPrsd = true;
	gameManager.startStage(curY, curX, getStartParsec(curY, curX), mode);
      }
    } else {
      if (ps == 0)
	padPrsd = false;
    }
    if (boxCnt >= 0)
      boxCnt--;
  }

  public void setStatus() {
    gameManager.difficulty = curY;
    gameManager.parsecSlot = curX;
    gameManager.mode = mode;
    if (curY < P47PrefManager.DIFFICULTY_NUM) {
      prefManager.selectedDifficulty = curY;
      prefManager.selectedParsecSlot = curX;
      prefManager.selectedMode = mode;
    }
  }

  public void changeMode() {
    mode++;
    if (mode >= P47PrefManager.MODE_NUM)
      mode = 0;
    if (curX >= slotNum[mode][curY])
      curX = slotNum[mode][curY] - 1;
    field.setColor(mode);
    gameManager.startStage(curY, curX, getStartParsec(curY, curX), mode);
  }

  public void drawBox(float[] model, float[] color, Gfx.Blend blend, int x, int y, int w, int h) {
    var mesh = new Mesh("Title-drawBox" + "-" + x.ToString() + "-" + y.ToString());
    color = new float[] { 1, 1, 1, 1 };
    P47Screen.appendBoxLine(mesh, color, x, y, w, h);
    color = new float[] { 1, 1, 1, 0.5f };
    P47Screen.appendBoxSolid(mesh, color, x, y, w, h);

    if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
  }

  public void drawBoxLight(float[] model, float[] color, Gfx.Blend blend, int x, int y, int w, int h) {
    var mesh = new Mesh("Title-drawBoxLight" + "-" + x.ToString() + "-" + y.ToString());
    color = new float[] { 1, 1, 1, 0.7f };
    P47Screen.appendBoxLine(mesh, color, x, y, w, h);
    color = new float[] { 1, 1, 1, 0.3f };
    P47Screen.appendBoxSolid(mesh, color, x, y, w, h);

    if (mesh.count > 0) Gfx.Draw(mesh.count, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
  }

  public const int BOX_SMALL_SIZE = 24;
  public static string[] DIFFICULTY_SHORT_STR = new string[] { "P", "N", "H", "E", "Q" };
  public static string[] DIFFICULTY_STR = new string[] { "PRACTICE", "NORMAL", "HARD", "EXTREME", "QUIT" };
  public static string[] MODE_STR = new string[] { "ROLL", "LOCK" };

  public void drawTitleBoard(float[] model, float[] color, Gfx.Blend blend) { if (logo == null) {
      logo = new Mesh("title-logo");
      logo.Vertex(180, 20, 0, new float[] { 0, 0, 0, 1 });
      logo.Vertex(308, 20, 0, new float[] { 1, 0, 0, 1 });
      logo.Vertex(308, 148, 0, new float[] { 1, 1, 0, 1 });
      logo.Vertex(180, 148, 0, new float[] { 0, 1, 0, 1 });
      logo.Quads(0, 4);
    }
    Gfx.Draw(logo.count, logo.Bindings(model, new float[] { 1, 1, 1, 1 }, 1, blend == Gfx.Blend.Additive, 0, TitleImage.title),
      new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }

  public void draw(float[] model, float[] color, Gfx.Blend blend) {
    int sx=0, sy=0;
    LetterRender.drawString(model, color, blend, DIFFICULTY_STR[curY], 470 - DIFFICULTY_STR[curY].Length * 14, 150,
       10, LetterRender.TO_RIGHT);
    LetterRender.drawString(model, color, blend, MODE_STR[mode], 470 - MODE_STR[mode].Length * 14, 450,
       10, LetterRender.TO_RIGHT);
    if (curX > 0) {
      LetterRender.drawString(model, color, blend, "START AT PARSEC", 290, 180, 6, LetterRender.TO_RIGHT);
      LetterRender.drawNum(model, color, blend, getStartParsec(curY, curX), 470, 180, 6, LetterRender.TO_RIGHT);
    }
    if (curY < P47PrefManager.DIFFICULTY_NUM)
      LetterRender.drawNum(model, color, blend, prefManager.hiScore[mode][curY][curX], 470, 210, 10, LetterRender.TO_RIGHT);
    sy = 260;
    for (int index2 = 0; index2 < P47PrefManager.DIFFICULTY_NUM + 1; index2++) {
      sx = 180;
      for (int index3 = 0; index3 < slotNum[mode][index2]; index3++) {
	if (index3 == curX && index2 == curY) {
	  int bs = GameMath.integer((BOX_COUNT - boxCnt) / 2);
	  drawBox(model, color, blend, sx - bs, sy - bs, BOX_SMALL_SIZE + bs * 2, BOX_SMALL_SIZE + bs * 2);
	  if (index3 == 0) {
	    LetterRender.drawString(model, color, blend, DIFFICULTY_SHORT_STR[index2], sx + 13, sy + 13, 12, LetterRender.TO_RIGHT);
	  } else {
	    LetterRender.drawString(model, color, blend, DIFFICULTY_SHORT_STR[index2], sx + 4, sy + 13, 12, LetterRender.TO_RIGHT);
	    if (index3 >= P47PrefManager.REACHED_PARSEC_SLOT_NUM - 1) {
	      LetterRender.drawString(model, color, blend, "X", sx + 21, sy + 14, 12, LetterRender.TO_RIGHT);
	    } else {
	      LetterRender.drawNum(model, color, blend, index3, sx + 22, sy + 13, 12, LetterRender.TO_RIGHT);
	    }
	  }
	} else {
	  drawBoxLight(model, color, blend, sx, sy, BOX_SMALL_SIZE, BOX_SMALL_SIZE);
	}
	sx += 28;
      }
      sy += 32;
      if (index2 == P47PrefManager.DIFFICULTY_NUM - 1)
	sy += 15;
    }
    drawTitleBoard(model, color, blend);
  }
}
