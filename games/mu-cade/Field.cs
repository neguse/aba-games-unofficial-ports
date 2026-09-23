// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Field
{
    public const float GRAVITY = 4.2f;
    public const float EYE_POS_Y = -2.5f;
    public const float EYE_POS_Z = 15f;
    public static Rand rand;
    public Screen screen;
    public World world;
    public GameManager gameManager;
    public Ship ship;
    public StarParticlePool starParticles;
    public Vector _size;
    public Vector eyePos, eyePosSize;
    public Wall floorWall;
    public bool titleMask;
    public int cnt;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Field(Screen screen, World world, GameManager gameManager)
    {
        this.screen = screen;
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

    public virtual void setLookAt()
    {
        glPushMatrix();
        LoadIdentity();
        Screen.lookAt(eyePos.x, eyePos.y + EYE_POS_Y, EYE_POS_Z, eyePos.x, eyePos.y, 0, 0, 1, 0);
        Drawing.view = Drawing.matrix;
        glPopMatrix();
    }

    public virtual void setLookAtTitle()
    {
        glPushMatrix();
        LoadIdentity();
        Screen.lookAt(0, EYE_POS_Y, EYE_POS_Z, 0, 0, 0, 0, 1, 0);
        Drawing.view = Drawing.matrix;
        glPopMatrix();
    }

    public virtual void draw()
    {
        glBegin(GL_LINES);
        for (int z = 0; z > -8; z--)
        {
            float a = 1;
            if (z < 0)
                a = 0.8f + z * 0.05f;
            drawSquare(-_size.x, -_size.y, _size.x * 2, _size.y * 2, z, a);
        }

        for (float w = 0.98f; w < 1.0f; w += 0.0033f)
            drawSquare(-_size.x * w, -_size.y * w, _size.x * w * 2, _size.y * w * 2, 0, 0.9f);
        for (float x = -0.9f; x < 1.0f; x += 0.1f)
        {
            Screen.setColor(1, 1, 1);
            glVertex3f(_size.x * x, -_size.y, 0);
            Screen.setColor(0.4f, 0.4f, 0.4f);
            glVertex3f(_size.x * x, -_size.y, -8);
            Screen.setColor(1, 1, 1);
            glVertex3f(_size.x * x, _size.y, 0);
            Screen.setColor(0.4f, 0.4f, 0.4f);
            glVertex3f(_size.x * x, _size.y, -8);
        }

        for (float y = -1; y < 1.1f; y += 0.1f)
        {
            Screen.setColor(1, 1, 1);
            glVertex3f(-_size.x, _size.y * y, 0);
            Screen.setColor(0.4f, 0.4f, 0.4f);
            glVertex3f(-_size.x, _size.y * y, -8);
            Screen.setColor(1, 1, 1);
            glVertex3f(_size.x, _size.y * y, 0);
            Screen.setColor(0.4f, 0.4f, 0.4f);
            glVertex3f(_size.x, _size.y * y, -8);
        }

        glEnd();
    }

    public virtual void drawSquare(float x, float y, float w, float h, float z, float a)
    {
        Screen.drawLine(x, y, z, x + w, y, z, a);
        Screen.drawLine(x + w, y, z, x + w, y + h, z, a);
        Screen.drawLine(x + w, y + h, z, x, y + h, z, a);
        Screen.drawLine(x, y + h, z, x, y, z, a);
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
    public virtual void drawOverlay()
    {
        viewOrthoFixed();
        gameManager.drawState();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glBegin(GL_QUADS);
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
            Screen.setColor(1, 0, 0);
            glVertex3f(x1 - ox1, y1 - oy1, 0);
            glVertex3f(x2 - ox2, y2 - oy2, 0);
            glVertex3f(x2 + ox2, y2 + oy2, 0);
            glVertex3f(x1 + ox1, y1 + oy1, 0);
            ox1 *= 0.5f;
            oy1 *= 0.5f;
            ox2 *= 0.5f;
            oy2 *= 0.5f;
            Screen.setColor(1, 1, 1);
            glVertex3f(x1 - ox1, y1 - oy1, 0);
            glVertex3f(x2 - ox2, y2 - oy2, 0);
            glVertex3f(x2 + ox2, y2 + oy2, 0);
            glVertex3f(x1 + ox1, y1 + oy1, 0);
        }

        glEnd();
        float x = 285, y = 465;
        float lsz = 26, lof = 20;
        for (int i_1 = 0; i_1 < 5; i_1++)
        {
            Screen.setColorForced(1, 1, 1);
            titleMask = true;
            drawLetter_4(i_1, x, y, lsz);
            glBlendFunc(GL_ONE, GL_ONE);
            Screen.setColor(1, 0, 0);
            titleMask = false;
            drawLetter_4(i_1, x, y, lsz);
            glBlendFunc(GL_ONE, GL_ONE);
            Screen.setColor(1, 1, 1);
            titleMask = false;
            drawLetter_4(i_1, x, y, lsz);
            if (i_1 == 0)
                x += lof * 1.0f;
            else
                x += lof * 0.9f;
        }

        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        viewPerspective();
    }

    public void drawLetter_4(int i, float cx, float cy, float width)
    {
        McdData.DrawGlyph(i, cx, cy, width, titleMask);
    }

    public virtual void viewOrthoFixed()
    {
        Screen.viewOrthoFixed();
    }

    public virtual void viewPerspective()
    {
        Screen.viewPerspective();
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

    public override void draw()
    {
    }
}

public class FloorWall : Wall
{
}
