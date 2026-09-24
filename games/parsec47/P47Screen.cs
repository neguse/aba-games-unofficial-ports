// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
public class P47Screen {
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

  public static void appendLineRetro(Mesh mesh, float x1, float y1, float x2, float y2) {
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
    float[] color = new float[] { r, g, b, a };
    if (retro < 0.2f) {
      int part1 = mesh.vertexCount;
      mesh.Vertex(x1, y1, retroZ, color);
      mesh.Vertex(x2, y2, retroZ, color);
      for (int vi = part1; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1);
    } else {
      float ds = retroSize * retro;
      float ds2 = ds / 2;
      float lx = fabs(x2 - x1);
      float ly = fabs(y2 - y1);
      int part2 = mesh.vertexCount;
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
	    mesh.Vertex(x - ds2, y - ds2, retroZ, color);
	    mesh.Vertex(x + ds2, y - ds2, retroZ, color);
	    mesh.Vertex(x + ds2, y + ds2, retroZ, color);
	    mesh.Vertex(x - ds2, y + ds2, retroZ, color);
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
	    mesh.Vertex(x - ds2, y - ds2, retroZ, color);
	    mesh.Vertex(x + ds2, y - ds2, retroZ, color);
	    mesh.Vertex(x + ds2, y + ds2, retroZ, color);
	    mesh.Vertex(x - ds2, y + ds2, retroZ, color);
	  }
	}
      }
      mesh.Quads(part2, mesh.vertexCount - part2);
    }
  }

  public static void appendBoxRetro(Mesh mesh, float x, float y, float width, float height, float deg) {
    float w1=0, h1=0, w2=0, h2=0;
    w1 = width * cos(deg) - height * sin(deg);
    h1 = width * sin(deg) + height * cos(deg);
    w2 = -width * cos(deg) - height * sin(deg);
    h2 = -width * sin(deg) + height * cos(deg);
    appendLineRetro(mesh, x + w2, y - h2, x + w1, y - h1);
    appendLineRetro(mesh, x + w1, y - h1, x - w2, y + h2);
    appendLineRetro(mesh, x - w2, y + h2, x - w1, y + h1);
    appendLineRetro(mesh, x - w1, y + h1, x + w2, y - h2);
  }

  public static void appendBoxSolid(Mesh mesh, float[] color, float x, float y, float width, float height) {
    int part1 = mesh.vertexCount;
    mesh.Vertex(x, y, 0, color);
    mesh.Vertex(x + width, y, 0, color);
    mesh.Vertex(x + width, y + height, 0, color);
    mesh.Vertex(x, y + height, 0, color);
    mesh.Fan(part1, mesh.vertexCount - part1);
  }

  public static void appendBoxLine(Mesh mesh, float[] color, float x, float y, float width, float height) {
    int part1 = mesh.vertexCount;
    mesh.Vertex(x, y, 0, color);
    mesh.Vertex(x + width, y, 0, color);
    mesh.Vertex(x + width, y + height, 0, color);
    mesh.Vertex(x, y + height, 0, color);
    mesh.LineStrip(part1, mesh.vertexCount - part1, true);
  }
}
