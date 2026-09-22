// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;
public class Tumiki {

  public const int SHAPE_NUM = 10;
  public const int SHADE_NUM = 4;
  public static int displayListIdx;
  public Vector ofs;
  public Vector size, checkHitSize;
  public int shape, color;

  public Barrage[] barrage;
  public static int propellerCnt = 0;

  public const float CHECK_HIT_SIZE_RETIO = 0.7f;

  public Tumiki(int shape, int color,
	      float x, float y, float sx, float sy, float sizeRatio) {
    this.shape = shape;
    this.color = color;
    ofs = new Vector(x * sizeRatio, y * sizeRatio);
    size = new Vector(sx * sizeRatio * 0.5f - 0.15f, sy * sizeRatio * 0.5f - 0.15f);
    checkHitSize = new Vector(size.x + CHECK_HIT_SIZE_RETIO,
			      size.y + CHECK_HIT_SIZE_RETIO);
    barrage = new Barrage[0];
  }



  public BulletActor addTopBullet(int barragePtnIdx, BulletActorPool bullets,
				  BulletTarget target, int type) {
    if (barrage.Length == 0)
      return null;
    Barrage b = barrage[barragePtnIdx];
    return b.addTopBullet(bullets, target, type);
  }

  public static void move() {
    propellerCnt++;
  }

  public const float PROPELLER_OFFSET = 2.2f;
  public const int PROPELLER_SHAPE = 9;
  public const int PROPELLER_SHAPE_FRONT = 14;

  public void drawPropeller(float deg, int shade) {
    float d = (shape - PROPELLER_SHAPE) * PI / 4 + deg;
    glRotatef(propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    glRotatef(rtod(d), 0, 0, 1);
    glPushMatrix();
    glTranslatef(-size.x * PROPELLER_OFFSET, 0, 0);
    glScalef(size.x, size.y, (size.x  + size.y) / 2);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
    glPushMatrix();
    glTranslatef(size.x * PROPELLER_OFFSET, 0, 0);
    glScalef(size.x, size.y, (size.x  + size.y) / 2);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
  }

  public void drawPropellerFront(float deg, int shade) {
    float d = deg;
    glRotatef(90, 1, 0, 0);
    glRotatef(propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    glRotatef(rtod(d), 0, 0, 1);
    glPushMatrix();
    glTranslatef(-size.x * PROPELLER_OFFSET, (size.x  + size.y) / 2, (size.x  + size.y) / 2);
    glScalef(size.x, size.y, (size.x  + size.y) / 2);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
    glPushMatrix();
    glTranslatef(size.x * PROPELLER_OFFSET, (size.x  + size.y) / 2, (size.x  + size.y) / 2);
    glScalef(size.x, size.y, (size.x  + size.y) / 2);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
  }

  public void drawPropellerScaled(float deg, int shade, float sz) {
    float d = (shape - PROPELLER_SHAPE) * PI / 4 + deg;
    glRotatef(propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    glRotatef(rtod(d), 0, 0, 1);
    glPushMatrix();
    glTranslatef(-size.x * PROPELLER_OFFSET * sz, 0, 0);
    glScalef(size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
    glPushMatrix();
    glTranslatef(size.x * PROPELLER_OFFSET * sz, 0, 0);
    glScalef(size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
  }

  public void drawPropellerFrontScaled(float deg, int shade, float sz) {
    float d = deg;
    glRotatef(90, 1, 0, 0);
    glRotatef(propellerCnt * 17 / size.x, -sin(d), cos(d), 0);
    glRotatef(rtod(d), 0, 0, 1);
    glPushMatrix();
    glTranslatef(-size.x * PROPELLER_OFFSET * sz,
		 (size.x  + size.y) / 2 * sz, (size.x  + size.y) / 2 * sz);
    glScalef(size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
    glPushMatrix();
    glTranslatef(size.x * PROPELLER_OFFSET * sz,
		 (size.x  + size.y) / 2 * sz, (size.x  + size.y) / 2 * sz);
    glScalef(size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
    glCallList(displayListIdx + PROPELLER_SHAPE +
	       color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    glPopMatrix();
  }

  public void drawRotated(Vector pos, float z, int shade, float deg) {
    glPushMatrix();
    float ox = ofs.x * cos(deg) - ofs.y * sin(deg);
    float oy = ofs.x * sin(deg) + ofs.y * cos(deg);
    glTranslatef(pos.x + ox, pos.y + oy, z);
    if (shape < PROPELLER_SHAPE) {
      glRotatef(rtod(deg), 0, 0, 1);
      glScalef(size.x, size.y, (size.x  + size.y) / 2);
      glCallList(displayListIdx + shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFront(deg, shade);
    } else {
      drawPropeller(deg, shade);
    }
    glPopMatrix();
  }

  public void drawScaled(Vector pos, float z, int shade, float deg, float sz) {
    glPushMatrix();
    float ox = ofs.x * cos(deg) - ofs.y * sin(deg);
    float oy = ofs.x * sin(deg) + ofs.y * cos(deg);
    ox *= sz;
    oy *= sz;
    glTranslatef(pos.x + ox, pos.y + oy, z);
    if (shape < PROPELLER_SHAPE) {
      glRotatef(rtod(deg), 0, 0, 1);
      glScalef(size.x * sz, size.y * sz, (size.x  + size.y) / 2 * sz);
      glCallList(displayListIdx + shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFrontScaled(deg, shade, sz);
    } else {
      drawPropellerScaled(deg, shade, sz);
    }
    glPopMatrix();
  }

  public void draw(Vector pos, float z, int shade) {
    drawAt(pos.x, pos.y, z, shade, false, false);
  }

  public void drawAt(float x, float y, float z, int shade, bool damaged, bool wounded) {
    glPushMatrix();
    glTranslatef(x + ofs.x, y + ofs.y, z);
    if (shape < PROPELLER_SHAPE) {
      glScalef(size.x, size.y, (size.x  + size.y) / 2);
      if (damaged)
	glCallList
	  (displayListIdx + shape + DAMAGED_COLOR * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
      else if (wounded)
	glCallList
	  (displayListIdx + shape + WOUNDED_COLOR * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
      else
	glCallList
	  (displayListIdx + shape + color * SHAPE_NUM + shade * SHAPE_NUM * COLOR_NUM);
    } else if (shape == PROPELLER_SHAPE_FRONT) {
      drawPropellerFront(0, shade);
    } else {
      drawPropeller(0, shade);
    }
    glPopMatrix();
  }

  public bool checkDistHit(float x, float y, Vector ofs, Vector size) {
    float ox = x - ofs.x;
    float oy = y - ofs.y;
    if (ox > -checkHitSize.x && ox < checkHitSize.x &&
	oy > -checkHitSize.y && oy < checkHitSize.y)
      return true;
    return false;
  }

  public bool checkHit(Vector p, float px, float py) {
    if (shape != 0)
      return false;
    return checkDistHit(p.x - px, p.y - py, ofs, size);
  }

  public const int DISPLAY_LIST_NUM = SHAPE_NUM * COLOR_NUM * SHADE_NUM;
  public const int COLOR_NUM = 12;
  public const int DAMAGED_COLOR = 6;
  public const int WOUNDED_COLOR = 0;
  public static float[][] colorParams =
    new float[][] {
     new float[] {0.9f, 0.6f, 0.6f}, new float[] {0.6f, 0.9f, 0.6f}, new float[] {0.6f, 0.6f, 0.9f},
     new float[] {0.8f, 0.8f, 0.6f}, new float[] {0.8f, 0.6f, 0.8f}, new float[] {0.6f, 0.8f, 0.8f},
     new float[] {0.8f, 0.8f, 0.8f}, new float[] {0.5f, 0.5f, 0.5f},
     new float[] {1, 0.7f, 0.5f}, new float[] {0.7f, 0.9f, 1}, new float[] {1, 0.5f, 0.8f},
     new float[] {0.6f, 0.6f, 0.3f},
    };
  public const float DEPTH = -2;
  public const float LINE_PADDING = 0.03f;

  public static void setFrontColor(int j, int i) {
    switch (i) {
    case 1:
      Screen.setColor
	(colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f);
      break;
    case 2:
      Screen.setColor
	(colorParams[j][0] * 0.5f, colorParams[j][1] * 0.5f, colorParams[j][2] * 0.5f);
      break;
    default:
      Screen.setColor
	(colorParams[j][0] * 0.9f, colorParams[j][1] * 0.9f, colorParams[j][2] * 0.9f);
      break;
    }
  }

  public static void setSideColor(int j, int i) {
    switch (i) {
    case 0:
      Screen.setColor
	(colorParams[j][0] * 0.7f, colorParams[j][1] * 0.7f, colorParams[j][2] * 0.7f);
      break;
    case 1:
      Screen.setColor
	(colorParams[j][0] * 0.6f, colorParams[j][1] * 0.6f, colorParams[j][2] * 0.6f);
      break;
    case 2:
      Screen.setColor
	(colorParams[j][0] * 0.4f, colorParams[j][1] * 0.4f, colorParams[j][2] * 0.4f);
      break;
    }
  }

  public static void createDisplayLists() {
    displayListIdx = glGenLists(DISPLAY_LIST_NUM);
    int di = displayListIdx;
    for (int i = 0; i < SHADE_NUM; i++) {
      for (int j = 0; j < COLOR_NUM; j++) {
	glNewList(di, GL_COMPILE);
	setFrontColor(j, i);
	glBegin(GL_QUADS);
	glVertex3f(1, 1, 0);
	glVertex3f(-1, 1, 0);
	glVertex3f(-1, -1, 0);
	glVertex3f(1, -1, 0);
	if (i < 3) {
	  setSideColor(j, i);
	  glVertex3f(-1, 1, 0);
	  glVertex3f(1, 1, 0);
	  glVertex3f(1, 1, DEPTH);
	  glVertex3f(-1, 1, DEPTH);
	  glVertex3f(-1, -1, 0);
	  glVertex3f(-1, 1, 0);
	  glVertex3f(-1, 1, DEPTH);
	  glVertex3f(-1, -1, DEPTH);
	  glVertex3f(1, -1, 0);
	  glVertex3f(-1, -1, 0);
	  glVertex3f(-1, -1, DEPTH);
	  glVertex3f(1, -1, DEPTH);
	  glVertex3f(1, 1, 0);
	  glVertex3f(1, -1, 0);
	  glVertex3f(1, -1, DEPTH);
	  glVertex3f(1, 1, DEPTH);
	}
	glEnd();
	if (i == 0 || i == 3) {
	  Screen.setColor
	    (colorParams[j][0], colorParams[j][1], colorParams[j][2]);
	  glBegin(GL_LINE_STRIP);
	  if (i == 0) {
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glVertex3f(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	  } else {
	    glVertex3f(1, 1, 0);
	    glVertex3f(-1, 1, 0);
	    glVertex3f(-1, -1, 0);
	    glVertex3f(1, -1, 0);
	    glVertex3f(1, 1, 0);
	  }
	  glEnd();
	  if (i == 0) {
	    Screen.setColor
	      (colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f);
	    glBegin(GL_LINES);
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, DEPTH);
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, DEPTH);
	    glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH);
	    glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glVertex3f(1 + LINE_PADDING, -1 - LINE_PADDING, DEPTH);
	    glVertex3f(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glEnd();
	  }
	}
	glEndList();
	di++;
	for (int k = 0; k < 4; k++) {
	  glNewList(di, GL_COMPILE);
	  glRotatef(-90 * k, 0, 0, 1);
	  setFrontColor(j, i);
	  glBegin(GL_TRIANGLE_STRIP);
	  glVertex3f(1, 1, 0);
	  glVertex3f(-1, 1, 0);
	  glVertex3f(-1, -1, 0);
	  glEnd();
	  if (i < 3) {
	    setSideColor(j, i);
	    glBegin(GL_QUADS);
	    glVertex3f(-1, 1, 0);
	    glVertex3f(1, 1, 0);
	    glVertex3f(1, 1, DEPTH);
	    glVertex3f(-1, 1, DEPTH);
	    glVertex3f(-1, -1, 0);
	    glVertex3f(-1, 1, 0);
	    glVertex3f(-1, 1, DEPTH);
	    glVertex3f(-1, -1, DEPTH);
	    glVertex3f(1, 1, 0);
	    glVertex3f(-1, -1, 0);
	    glVertex3f(-1, -1, DEPTH);
	    glVertex3f(1, 1, DEPTH);
	    glEnd();
	  }
	  if (i == 0 || i == 3) {
	    Screen.setColor
	      (colorParams[j][0], colorParams[j][1], colorParams[j][2]);
	    glBegin(GL_LINE_STRIP);
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	    glEnd();
	    if (i == 0) {
	      Screen.setColor
		(colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f);
	      glBegin(GL_LINES);
	      glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, DEPTH);
	      glVertex3f(1 + LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	      glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, DEPTH);
	      glVertex3f(-1 - LINE_PADDING, 1 + LINE_PADDING, LINE_PADDING);
	      glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH);
	      glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	      glEnd();
	    }
	  }
	  glEndList();
	  di++;
	}
	for (int k = 0; k < 4; k++) {
	  glNewList(di, GL_COMPILE);
	  glRotatef(-90 * k, 0, 0, 1);
	  setFrontColor(j, i);
	  glBegin(GL_TRIANGLE_STRIP);
	  glVertex3f(1, -1, 0);
	  glVertex3f(0, 1, 0);
	  glVertex3f(-1, -1, 0);
	  glEnd();
	  if (i < 3) {
	    setSideColor(j, i);
	    glBegin(GL_QUADS);
	    glVertex3f(0, 1, 0);
	    glVertex3f(1, -1, 0);
	    glVertex3f(1, -1, DEPTH);
	    glVertex3f(0, 1, DEPTH);
	    glVertex3f(-1, -1, 0);
	    glVertex3f(0, 1, 0);
	    glVertex3f(0, 1, DEPTH);
	    glVertex3f(-1, -1, DEPTH);
	    glVertex3f(1, -1, 0);
	    glVertex3f(-1, -1, 0);
	    glVertex3f(-1, -1, DEPTH);
	    glVertex3f(1, -1, DEPTH);
	    glEnd();
	  }
	  if (i == 0 || i == 3) {
	    Screen.setColor
	      (colorParams[j][0], colorParams[j][1], colorParams[j][2]);
	    glBegin(GL_LINE_STRIP);
	    glVertex3f(1 + LINE_PADDING, -1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(0, 1 + LINE_PADDING, LINE_PADDING);
	    glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glVertex3f(1 + LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	    glEnd();
	    if (i == 0) {
	      Screen.setColor
		(colorParams[j][0] * 0.8f, colorParams[j][1] * 0.8f, colorParams[j][2] * 0.8f);
	      glBegin(GL_LINES);
	      glVertex3f(1 + LINE_PADDING, -1 - LINE_PADDING, DEPTH);
	      glVertex3f(1 + LINE_PADDING, -1 + LINE_PADDING, LINE_PADDING);
	      glVertex3f(0, 1 + LINE_PADDING, DEPTH);
	      glVertex3f(0, 1 + LINE_PADDING, LINE_PADDING);
	      glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, DEPTH);
	      glVertex3f(-1 - LINE_PADDING, -1 - LINE_PADDING, LINE_PADDING);
	      glEnd();
	    }
	  }
	  glEndList();
	  di++;
	}
	glNewList(di, GL_COMPILE);
	setFrontColor(j, i);
	glBegin(GL_QUADS);
	glVertex3f(1, 1, 0);
	glVertex3f(-1, 1, 0);
	glVertex3f(-1, -1, 0);
	glVertex3f(1, -1, 0);
	glVertex3f(1, 1, DEPTH);
	glVertex3f(1, -1, DEPTH);
	glVertex3f(-1, -1, DEPTH);
	glVertex3f(-1, 1, DEPTH);
	if (i < 3) {
	  setSideColor(j, i);
	  glVertex3f(-1, 1, 0);
	  glVertex3f(1, 1, 0);
	  glVertex3f(1, 1, DEPTH);
	  glVertex3f(-1, 1, DEPTH);
	  glVertex3f(-1, -1, 0);
	  glVertex3f(-1, 1, 0);
	  glVertex3f(-1, 1, DEPTH);
	  glVertex3f(-1, -1, DEPTH);
	  glVertex3f(1, -1, 0);
	  glVertex3f(-1, -1, 0);
	  glVertex3f(-1, -1, DEPTH);
	  glVertex3f(1, -1, DEPTH);
	  glVertex3f(1, 1, 0);
	  glVertex3f(1, -1, 0);
	  glVertex3f(1, -1, DEPTH);
	  glVertex3f(1, 1, DEPTH);
	}
	glEnd();
	glEndList();
	di++;
      }
    }
  }

  public static void deleteDisplayLists() {
    glDeleteLists(displayListIdx, DISPLAY_LIST_NUM);
  }
}
