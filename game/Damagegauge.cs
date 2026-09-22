// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class DamageGauge {

  public int cnt;
  public DamageGaugeItem[] item = new DamageGaugeItem[3];

  public DamageGauge() {
    for (int _i = 0; _i < 3; _i++) { item[_i] = new DamageGaugeItem(); }
  }

  public void init() {
    cnt = 0;
    foreach (DamageGaugeItem dgi in item)
      dgi.part = null;
  }

  public void add(EnemyPart ep) {
    int mc = int.MaxValue;
    DamageGaugeItem si = null;
    foreach (DamageGaugeItem dgi in item) {
      if (dgi.part != null) {
	if (dgi.part == ep) {
	  dgi.cnt = cnt;
	  return;
	}
	if (mc > dgi.cnt) {
	  si = dgi;
	  mc = dgi.cnt;
	}
      } else if (mc >= 0) {
	si = dgi;
	mc = int.MinValue;
      }
    }
    si.part = ep;
    si.cnt = cnt;
  }

  public void move() {
    foreach (DamageGaugeItem dgi in item) {
      if (dgi.part != null) {
	if (dgi.part.shield <= 0) {
	  dgi.part = null;
	}
      }
    }
    cnt++;
  }

  public void draw() {
    float x = 18, y = -13;
    foreach (DamageGaugeItem dgi in item) {
      if (dgi.part != null) {
	TumikiSet ts = dgi.part.spec.tumikiSet;
	float s = 1 / ts.size * 3;
	float sx = x / s;
	float sy = y / s;
	glPushMatrix();
	glScalef(s, s, s);
	ts.drawAt(sx, sy, 0.9f, false, false);
	glPopMatrix();
	float sl = dgi.part.shield;
	for (int i = 0; sl > 0; i++, sl -= 100) {
	  float sx2 = 11;
	  float slb = sl;
	  if (slb > 100)
	    slb = 100;
	  float sx1 = sx2 - slb / 10;
	  glPushMatrix();
	  glTranslatef(sx1 + sx2 / 2, y - 0.4f, 1 + i * 0.1f);
	  glScalef(sx2 - sx1, 0.4f, 1);
	  glCallList(Tumiki.displayListIdx +
		     ((3 + i) % Tumiki.COLOR_NUM) * Tumiki.SHAPE_NUM +
		     3 * Tumiki.COLOR_NUM * Tumiki.SHAPE_NUM);
	  glPopMatrix();
	}
      }
      y += 1.8f;
    }
  }

}

public class DamageGaugeItem {

  public EnemyPart part;
  public int cnt;
  public int disapCnt;
}
