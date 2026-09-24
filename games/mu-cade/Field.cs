// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Field
{
    public const float GRAVITY = 4.2f;
    public const float EYE_POS_Y = -2.5f;
    public const float EYE_POS_Z = 15f;
    public static Rand rand;
    public World world;
    public GameManager gameManager;
    public Ship ship;
    public StarParticlePool starParticles;
    public Vector _size;
    public Vector eyePos, eyePosSize;
    public Wall floorWall;
    Mesh fieldMesh, overlayMesh, glyphQuad;
    public int cnt;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Field(World world, GameManager gameManager)
    {
        this.world = world;
        this.gameManager = gameManager;
        _size = new Vector(20, 15);
        eyePos = new Vector();
        eyePosSize = new Vector(_size.x - 12, size().y - 9);
        floorWall = new FloorWall();
        floorWall.setWorld(world);
        floorWall.init_1_(null);
    }

    public virtual void start_0()
    {
        floorWall.set_1(false);
        ShapeGroup s = new ShapeGroup();
        s.addShape(new Square(world, 9999999, 0, 0, _size.x * 2, _size.y * 2, -6, 10));
        s.addGeom_3(floorWall, world.space);
        cnt = 0;
        eyePos.y = 0;
        eyePos.x = eyePos.y;
    }

    public virtual void clear()
    {
        floorWall.remove_0();
    }

    public virtual void setShip(Ship ship)
    {
        this.ship = ship;
    }

    public virtual void setStarParticles(StarParticlePool starParticles)
    {
        this.starParticles = starParticles;
    }

    public virtual void move_0()
    {
        cnt--;
        if (cnt < 0)
        {
            StarParticle sp = starParticles.getInstance();
            if ((sp) != null)
            {
                float sz = 0.25f + rand.nextFloat(0.5f);
                float x = default(float), y = default(float);
                if (rand.nextInt(2) == 0)
                {
                    x = rand.nextSignedFloat(_size.x * 2.0f);
                    y = _size.y + rand.nextFloat(_size.y) * 1.5f;
                    if (rand.nextInt(2) == 0)
                        y *= -1;
                }
                else
                {
                    x = _size.x + rand.nextFloat(_size.x) * 1.5f;
                    if (rand.nextInt(2) == 0)
                        x *= -1;
                    y = rand.nextSignedFloat(_size.y * 2.0f);
                }

                sp.set_5_Single_Single_Single_Single_Single(x, y, 16, (0.05f + rand.nextFloat(0.05f)) / sz, sz);
            }

            cnt = 3;
        }

        setEyePos();
    }

    public virtual void setEyePos()
    {
        float tx = ship.pos().x, ty = ship.pos().y;
        if (checkInField_1_Vector3(ship.pos()))
        {
            if (tx < -eyePosSize.x)
                tx = -eyePosSize.x;
            else if (tx > eyePosSize.x)
                tx = eyePosSize.x;
            if (ty < -eyePosSize.y)
                ty = -eyePosSize.y;
            else if (ty > eyePosSize.y)
                ty = eyePosSize.y;
        }

        eyePos.x += (tx - eyePos.x) * 0.05f;
        eyePos.y += (ty - eyePos.y) * 0.05f;
    }

    public virtual float[] setLookAt() { return Transform.LookAt(Transform.Perspective(), eyePos.x, eyePos.y + EYE_POS_Y, EYE_POS_Z, eyePos.x, eyePos.y, 0, 0, 1, 0); }

    public virtual float[] setLookAtTitle() { return Transform.LookAt(Transform.Perspective(), 0, EYE_POS_Y, EYE_POS_Z, 0, 0, 0, 0, 1, 0); }

    public virtual void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (fieldMesh == null) { var mesh = new Mesh("field");
        int part1 = mesh.vertexCount;
        for (int z = 0; z > -8; z--)
        {
            float a = 1;
            if (z < 0)
                a = 0.8f + z * 0.05f;
            appendSquare(mesh, model, tint, -_size.x, -_size.y, _size.x * 2, _size.y * 2, z, a);
        }

        for (float w = 0.98f; w < 1.0f; w += 0.0033f)
            appendSquare(mesh, model, tint, -_size.x * w, -_size.y * w, _size.x * w * 2, _size.y * w * 2, 0, 0.9f);
        for (float x = -0.9f; x < 1.0f; x += 0.1f)
        {
            tint = new float[] { 1, 1, 1, 1 };
            mesh.Vertex(_size.x * x, -_size.y, 0, tint);
            tint = new float[] { 0.4f, 0.4f, 0.4f, 1 };
            mesh.Vertex(_size.x * x, -_size.y, -8, tint);
            tint = new float[] { 1, 1, 1, 1 };
            mesh.Vertex(_size.x * x, _size.y, 0, tint);
            tint = new float[] { 0.4f, 0.4f, 0.4f, 1 };
            mesh.Vertex(_size.x * x, _size.y, -8, tint);
        }

        for (float y = -1; y < 1.1f; y += 0.1f)
        {
            tint = new float[] { 1, 1, 1, 1 };
            mesh.Vertex(-_size.x, _size.y * y, 0, tint);
            tint = new float[] { 0.4f, 0.4f, 0.4f, 1 };
            mesh.Vertex(-_size.x, _size.y * y, -8, tint);
            tint = new float[] { 1, 1, 1, 1 };
            mesh.Vertex(_size.x, _size.y * y, 0, tint);
            tint = new float[] { 0.4f, 0.4f, 0.4f, 1 };
            mesh.Vertex(_size.x, _size.y * y, -8, tint);
        }

        for (int vi = part1; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1);

            fieldMesh = mesh;
        }
        if (fieldMesh.count > 0) Gfx.Draw(fieldMesh.count, fieldMesh.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public virtual void appendSquare(Mesh mesh, float[] model, float[] tint, float x, float y, float w, float h, float z, float a)
    {
        LinePoint.appendLine(mesh, tint, x, y, z, x + w, y, z, a);
        LinePoint.appendLine(mesh, tint, x + w, y, z, x + w, y + h, z, a);
        LinePoint.appendLine(mesh, tint, x + w, y + h, z, x, y + h, z, a);
        LinePoint.appendLine(mesh, tint, x, y + h, z, x, y, z, a);
    }

    public static readonly float[][] OVERLAY_BAR_POS = new float[][]
    {
        new float[]
        {
            370,
            466,
            0,
            7
        },
        new float[]
        {
            615,
            466,
            0,
            7
        },
        new float[]
        {
            631,
            450,
            7,
            0
        },
        new float[]
        {
            631,
            30,
            7,
            0
        },
        new float[]
        {
            615,
            14,
            0,
            -7
        },
        new float[]
        {
            25,
            14,
            0,
            -7
        },
        new float[]
        {
            9,
            30,
            -7,
            0
        },
        new float[]
        {
            9,
            450,
            -7,
            0
        },
        new float[]
        {
            25,
            466,
            0,
            7
        },
        new float[]
        {
            270,
            466,
            0,
            7
        }
    };
    public virtual void drawOverlay(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        model = Transform.Ortho();
        gameManager.drawState(model, tint, blend, key + "-drawState-1");
        blend = Gfx.Blend.Alpha;
        if (overlayMesh == null) {
        var part1 = new Mesh("overlay-bars");
        for (int i_0 = 1; i_0 < OVERLAY_BAR_POS.Length; i_0++)
        {
            float x1 = OVERLAY_BAR_POS[i_0 - 1][0];
            float y1 = OVERLAY_BAR_POS[i_0 - 1][1];
            float ox1 = OVERLAY_BAR_POS[i_0 - 1][2];
            float oy1 = OVERLAY_BAR_POS[i_0 - 1][3];
            float x2 = OVERLAY_BAR_POS[i_0][0];
            float y2 = OVERLAY_BAR_POS[i_0][1];
            float ox2 = OVERLAY_BAR_POS[i_0][2];
            float oy2 = OVERLAY_BAR_POS[i_0][3];
            tint = new float[] { 1, 0, 0, 1 };
            part1.Vertex(x1 - ox1, y1 - oy1, 0, tint);
            part1.Vertex(x2 - ox2, y2 - oy2, 0, tint);
            part1.Vertex(x2 + ox2, y2 + oy2, 0, tint);
            part1.Vertex(x1 + ox1, y1 + oy1, 0, tint);
            ox1 *= 0.5f;
            oy1 *= 0.5f;
            ox2 *= 0.5f;
            oy2 *= 0.5f;
            tint = new float[] { 1, 1, 1, 1 };
            part1.Vertex(x1 - ox1, y1 - oy1, 0, tint);
            part1.Vertex(x2 - ox2, y2 - oy2, 0, tint);
            part1.Vertex(x2 + ox2, y2 + oy2, 0, tint);
            part1.Vertex(x1 + ox1, y1 + oy1, 0, tint);
        }

        part1.Quads(0, part1.vertexCount - 0);
            overlayMesh = part1;
        }
        Gfx.Draw(overlayMesh.count, overlayMesh.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        float x = 285, y = 465;
        float lsz = 26, lof = 20;
        for (int i_1 = 0; i_1 < 5; i_1++)
        {
            tint = new float[] { 1, 1, 1, 1 };
            bool mask = true;
            drawLetter_4(model, tint, blend, key + "-drawLetter_4-1" + "-" + i_1.ToString(), i_1, x, y, lsz, mask);
            blend = Gfx.Blend.Additive;
            tint = new float[] { 1, 0, 0, 1 };
            mask = false;
            drawLetter_4(model, tint, blend, key + "-drawLetter_4-2" + "-" + i_1.ToString(), i_1, x, y, lsz, mask);
            blend = Gfx.Blend.Additive;
            tint = new float[] { 1, 1, 1, 1 };
            mask = false;
            drawLetter_4(model, tint, blend, key + "-drawLetter_4-3" + "-" + i_1.ToString(), i_1, x, y, lsz, mask);
            if (i_1 == 0)
                x += lof * 1.0f;
            else
                x += lof * 0.9f;
        }

        blend = Gfx.Blend.Additive;
    }

    public void drawLetter_4(float[] model, float[] tint, Gfx.Blend blend, string key, int i, float cx, float cy, float width, bool mask)
    {
        if (glyphQuad == null) {
            glyphQuad = new Mesh("title-glyph");
            glyphQuad.Vertex(-0.5f, -0.5f, 0, new float[] { 0, 0, 0, 1 });
            glyphQuad.Vertex(0.5f, -0.5f, 0, new float[] { 1, 0, 0, 1 });
            glyphQuad.Vertex(0.5f, 0.5f, 0, new float[] { 1, 1, 0, 1 });
            glyphQuad.Vertex(-0.5f, 0.5f, 0, new float[] { 0, 1, 0, 1 });
            glyphQuad.Quads(0, 4);
        }
        model = Transform.Scale(Transform.Translate(model, cx, cy, 0), width, width, 1);
        Gfx.Draw(glyphQuad.count, glyphQuad.Bindings(model, mask ? new float[] { 1, 1, 1, 1 } : tint, 1, !mask, 0, mask ? McdData.masks[i] : McdData.images[i]),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = mask ? Gfx.Blend.Multiply : Gfx.Blend.Additive });
    }





    public virtual bool checkInField_1_Vector(Vector p)
    {
        return _size.contains_2(p);
    }

    public virtual bool checkInField_2(float x, float y)
    {
        return _size.contains_3(x, y);
    }

    public virtual bool checkInField_1_Vector3(Vector3 p)
    {
        return (_size.contains_3(p.x, p.y) && fabs(p.z) < 1);
    }

    public virtual Vector size()
    {
        return _size;
    }
}

public class Wall : OdeActor
{
    public override void init_1_(object[] args)
    {
        base.init_1_Boolean();
    }

    public override void move_0()
    {
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
    }
}

public class FloorWall : Wall
{
}
