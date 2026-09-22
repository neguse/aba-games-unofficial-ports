// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class P47Screen: Screen {
 public static P47Rand rand = new P47Rand();
  public static float retro, retroSize;
  public static float retroR, retroG, retroB, retroA;
  public static float retroZ = 0;

  public static void setRetroParam(float r, float sz) {
    retro = r; retroSize = sz;
  }

  public static void setRetroColor(float r, float g, float b, float a) {
    retroR = r; retroG = g; retroB = b; retroA = a;
  }

  public static void setRetroZ(float z) {
    retroZ = z;
  }

  public static void drawLineRetro(float x1, float y1, float x2, float y2) {
    float cf = (1 - retro) * 0.5f;
    float r = retroR + (1 - retroR) * cf;
    float g = retroG + (1 - retroG) * cf;
    float b = retroB + (1 - retroB) * cf;
    float a = retroA * (cf + 0.5f);
    if (rand.nextInt(7) == 0) {
      r *= 1.5f; if (r > 1) r = 1;
      g *= 1.5f; if (g > 1) g = 1;
      b *= 1.5f; if (b > 1) b = 1;
      a *= 1.5f; if (a > 1) a = 1;
    }
    Screen.setColorAlpha(r, g, b, a);
    if (retro < 0.2f) {
      glBegin(GL_LINES);
      glVertex3f(x1, y1, retroZ);
      glVertex3f(x2, y2, retroZ);
      glEnd();
    } else {
      float ds = retroSize * retro;
      float ds2 = ds / 2;
      float lx = fabs(x2 - x1);
      float ly = fabs(y2 - y1);
      glBegin(GL_QUADS);
      if (lx < ly) {
	int n = GameMath.integer(ly / ds);
	if (n > 0) {
	  float xo = (x2 - x1) / n, xos  = 0;
	  float yo=0;
	  if (y2 < y1)
	    yo = -ds;
	  else
	    yo = ds;
	  float x = x1, y = y1;
	  for (int index0 = 0; index0 <= n; index0++, xos += xo, y += yo) {
	    if (xos >= ds) {
	      x += ds;
	      xos -= ds;
	    } else if (xos <= -ds) {
	      x -= ds;
	      xos += ds;
	    }
	    glVertex3f(x - ds2, y - ds2, retroZ);
	    glVertex3f(x + ds2, y - ds2, retroZ);
	    glVertex3f(x + ds2, y + ds2, retroZ);
	    glVertex3f(x - ds2, y + ds2, retroZ);
	  }
	}
      } else {
	int n = GameMath.integer(lx / ds);
	if (n > 0) {
	  float yo = (y2 - y1) / n, yos = 0;
	  float xo=0;
	  if (x2 < x1)
	    xo = -ds;
	  else
	    xo = ds;
	  float x = x1, y = y1;
	  for (int index1 = 0; index1 <= n; index1++, x += xo, yos += yo) {
	    if (yos >= ds) {
	      y += ds;
	      yos -= ds;
	    } else if (yos <= -ds) {
	      y -= ds;
	      yos += ds;
	    }
	    glVertex3f(x - ds2, y - ds2, retroZ);
	    glVertex3f(x + ds2, y - ds2, retroZ);
	    glVertex3f(x + ds2, y + ds2, retroZ);
	    glVertex3f(x - ds2, y + ds2, retroZ);
	  }
	}
      }
      glEnd();
    }
  }

  public static void drawBoxRetro(float x, float y, float width, float height, float deg) {
    float w1=0, h1=0, w2=0, h2=0;
    w1 = width * cos(deg) - height * sin(deg);
    h1 = width * sin(deg) + height * cos(deg);
    w2 = -width * cos(deg) - height * sin(deg);
    h2 = -width * sin(deg) + height * cos(deg);
    drawLineRetro(x + w2, y - h2, x + w1, y - h1);
    drawLineRetro(x + w1, y - h1, x - w2, y + h2);
    drawLineRetro(x - w2, y + h2, x - w1, y + h1);
    drawLineRetro(x - w1, y + h1, x + w2, y - h2);
  }

  public static void drawBoxSolid(float x, float y, float width, float height) {
    glBegin(GL_TRIANGLE_FAN);
    glVertex3f(x, y, 0);
    glVertex3f(x + width, y, 0);
    glVertex3f(x + width, y + height, 0);
    glVertex3f(x, y + height, 0);
    glEnd();
  }

  public static void drawBoxLine(float x, float y, float width, float height) {
    glBegin(GL_LINE_LOOP);
    glVertex3f(x, y, 0);
    glVertex3f(x + width, y, 0);
    glVertex3f(x + width, y + height, 0);
    glVertex3f(x, y + height, 0);
    glEnd();
  }
}
