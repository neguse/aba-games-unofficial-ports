// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;
public class Field {

  public const int TYPE_NUM = 4;
  public Vector size;
  public float eyeZ;
  public float aimZ;
  public float aimSpeed;

  public static Mesh[] meshes = new Mesh[1];
  public const int RING_NUM = 16;
  public const float RING_ANGLE_INT = 10;
  public float roll, yaw;
  public float z;
  public float speed;
  public float yawYBase, yawZBase;
  public float aimYawYBase, aimYawZBase;
  public float r, g, b;

  public void init() {
    size = new Vector();
    size.x = 11;
    size.y = 16;
    eyeZ = 20;
    {yaw = 0;    roll = yaw ;}
    {aimZ = 10;    z = aimZ ;}
    {aimSpeed = 0.1f;    speed = aimSpeed ;}
    {yawZBase = 0;    yawYBase = yawZBase ;}
  }

  public void setColor(int mode) {
    switch (mode){
    case P47GameManager.ROLL:
      r = 0.2f;
      g = 0.2f;
      b = 0.7f;
      break;
    case P47GameManager.LOCK:
      r = 0.5f;
      g = 0.3f;
      b = 0.6f;
      break;
    }
  }

  public void move() {
    roll += speed;
    if (roll >= RING_ANGLE_INT)
      roll -= RING_ANGLE_INT;
    yaw += speed;
    z += (aimZ - z) * 0.003f;
    speed += (aimSpeed - speed) * 0.004f;
    yawYBase += (aimYawYBase - yawYBase) * 0.002f;
    yawZBase += (aimYawZBase - yawZBase) * 0.002f;
  }

  public void setType(int type) {
    switch (type) {
    case 0:
      aimYawYBase = 30;
      aimYawZBase = 0;
      break;
    case 1:
      aimYawYBase = 0;
      aimYawZBase = 20;
      break;
    case 2:
      aimYawYBase = 50;
      aimYawZBase = 10;
      break;
    case 3:
      aimYawYBase = 10;
      aimYawZBase = 30;
      break;
    }
  }

  public void draw(float[] model, float[] color, Gfx.Blend blend) {
    color = new float[] { r, g, b, 0.7f };
    float d = -RING_NUM * RING_ANGLE_INT / 2 + roll;
    for (int index0 = 0; index0 < RING_NUM; index0++) {
      for (int index1 = 1; index1 < 8; index1++) {
	float sc = (float) index1 / 16 + 0.5f;
	float[] parent1 = model;
	model = Transform.Translate(model, 0, 0, z);
	model = Transform.Rotate(model, d, 1, 0, 0);
	model = Transform.Rotate(model, sin(yaw / 180 * PI) * yawYBase, 0, 1, 0);
	model = Transform.Rotate(model, sin(yaw / 180 * PI) * yawZBase, 0, 0, 1);
	model = Transform.Scale(model, 1, 1, sc);
	{
      Mesh shape1 = meshes[0];
      Gfx.Draw(shape1.count, shape1.Bindings(model, color, 1, blend == Gfx.Blend.Additive),
        new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
	model = parent1;
      }
      d += RING_ANGLE_INT;
    }
  }

  public bool checkHit_1(Vector p) {
    if (p.x < -size.x || p.x > size.x || p.y < -size.y || p.y > size.y)
      return true;
    return false;
  }

  public bool checkHit_2(Vector p, float space) {
    if (p.x < -size.x + space || p.x > size.x - space ||
	p.y < -size.y + space || p.y > size.y - space)
      return true;
    return false;
  }

  public const int RING_POS_NUM = 16;
  public static Vector[] ringPos = new Vector[RING_POS_NUM];
  public const float RING_DEG = PI / 3 / ((float) (RING_POS_NUM / 2) + 0.5f);
  public const float RING_RADIUS = 10;
  public const float RING_SIZE = 0.5f;

  public static void appendRing(Mesh mesh, float[] color) {
    int part1 = mesh.vertexCount;
    for (int index2 = 0; index2 <= GameMath.integer(RING_POS_NUM / 2) - 2; index2++) {
      mesh.Vertex(ringPos[index2].x, RING_SIZE, ringPos[index2].y, color);
    }
    for (int index3 = GameMath.integer(RING_POS_NUM / 2) - 2; index3 >= 0; index3--) {
      mesh.Vertex(ringPos[index3].x, -RING_SIZE, ringPos[index3].y, color);
    }
    mesh.Vertex(ringPos[0].x, RING_SIZE, ringPos[0].y, color);
    mesh.LineStrip(part1, mesh.vertexCount - part1);
    int part2 = mesh.vertexCount;
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].x, RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].y, color);
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2)].x, RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2)].y, color);
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2)].x, -RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2)].y, color);
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].x, -RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].y, color);
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].x, RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2) - 1].y, color);
    mesh.LineStrip(part2, mesh.vertexCount - part2);
    int part3 = mesh.vertexCount;
    for (int index4 = GameMath.integer(RING_POS_NUM / 2) + 1;  index4 <= RING_POS_NUM - 1; index4++) {
      mesh.Vertex(ringPos[index4].x, RING_SIZE, ringPos[index4].y, color);
    }
    for (int index5 = RING_POS_NUM - 1; index5 >= GameMath.integer(RING_POS_NUM / 2) + 1; index5--) {
      mesh.Vertex(ringPos[index5].x, -RING_SIZE, ringPos[index5].y, color);
    }
    mesh.Vertex(ringPos[GameMath.integer(RING_POS_NUM / 2) + 1].x, RING_SIZE, ringPos[GameMath.integer(RING_POS_NUM / 2) + 1].y, color);
    mesh.LineStrip(part3, mesh.vertexCount - part3);
  }

  public static void createMeshes() {
    float d = -RING_DEG * ((float) (GameMath.integer(RING_POS_NUM / 2)) - 0.5f);
    for (int index6 = 0; index6 < RING_POS_NUM; index6++, d += RING_DEG) {
      ringPos[index6] = new Vector();
      ringPos[index6].x = sin(d) * RING_RADIUS;
      ringPos[index6].y = cos(d) * RING_RADIUS;
    }
    float[] color = null; Mesh mesh = null;
    mesh = new Mesh("Field-ring");
    meshes[0] = mesh;
    appendRing(mesh, color);
  }

  public static void deleteMeshes() {
    meshes = new Mesh[1];
  }
}
