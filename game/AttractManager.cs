// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
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

  public void drawTitle() {
    if (cnt % 64 < 32)
      LetterRender.drawString
	("PUSH SHOT BUTTON TO START", 250, 390, 7, LetterDirection.TO_RIGHT, 3);
    int c = cnt % 1200;
    if (c < 300) {
      drawTitleBoard(70, 50, 16);
    } else {
      drawTitleBoard(30, 360, 8);
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
	LetterRender.drawString
	  (rs, x, y, 9, LetterDirection.TO_RIGHT, 3);
	LetterRender.drawNum(prefManager.ranking[i].score, 400, y, 9,
			     LetterDirection.TO_RIGHT, 3);
	if (prefManager.ranking[i].stage >= StageManager.STAGE_NUM)
	  rs = "A";
	else
	  rs = (prefManager.ranking[i].stage + 1).ToString();
	LetterRender.drawString
	  (rs, 500, y, 9, LetterDirection.TO_RIGHT, 3);
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

  public void drawTitleBoard(float x, float y, float s) {
    glPushMatrix();
    glTranslatef(x, y, 0);
    glScalef(s, s, s);
    int tx, ty;
    ty = 0;
    foreach (int[] tpl in TITLE_PTN) {
      tx = 0;
      foreach (int tp in tpl) {
	int c = TITLE_CLR[ty][tx];
	glPushMatrix();
	glTranslatef(tx * 2, ty * 2, 0);
	if (tp < 0) {
	  int ti = -tp - 1;
	  glScalef(0.75f, 0.75f, 0.75f);
	  glCallList(Tumiki.displayListIdx + ti + c * Tumiki.SHAPE_NUM);
	} else if (tp > 0) {
	  int li = tp + 10;
	  glScalef(0.9f, 0.9f, 0.9f);
	  glCallList(LetterRender.displayListIdx + li + c * LetterRender.LETTER_NUM);
	}
	glPopMatrix();
	tx++;
      }
      ty++;
    }
    glPopMatrix();
  }
}
