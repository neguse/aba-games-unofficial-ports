// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Shot : OdeActor
{
    public Field field;
    public ParticlePool particles;
    public Vector pos;
    public int cnt;
    public int removeCnt;
    public float _deg;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public virtual void set_2(Vector3 p, float d)
    {
        base.set_1();
        pos.x = p.x - sin(d) * 0.5f;
        pos.y = p.y + cos(d) * 0.5f;
        McdPhysics.BodyPosition(_bodyId, pos.x, pos.y, 0);
        _deg = d;
        setDeg(d);
        shape.setMass_3(this);
        shape.setGeom(this, null);
        addForce(-sin(d) * FORCE, cos(d) * FORCE);
        cnt = 0;
        removeCnt = 0;
        linePoint.init_0();
    }

    public override void move_0()
    {
        cnt++;
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        pos.x = p[0];
        pos.y = p[1];
        McdPhysics.BodyPosition(_bodyId, pos.x, pos.y, 0);
        if (removeCnt > 0)
        {
            removeCnt++;
            if (removeCnt > 5)
            {
                remove_0();
                return;
            }
        }

        if (!(field.checkInField_1_Vector(pos)))
        {
            remove_0();
            return;
        }

        recordLinePoints_0();
        doCollide();
    }

    public override void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        Enemy e = ((actor is Enemy ? (Enemy)actor : null));
        if ((e) != null)
        {
            if (removeCnt <= 0)
            {
                removeCnt = 1;
                for (int i = 0; i < 3; i++)
                {
                    Particle p = particles.getInstanceForced();
                    float d = deg() + PI + rand.nextSignedFloat(0.4f);
                    float v = 0.2f + rand.nextFloat(0.2f);
                    p.set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(pos, -sin(d) * v, cos(d) * v, 0.15f + rand.nextFloat(0.15f), 0.5f, 1, 0);
                }

                SoundManager.playSe("hit.wav");
            }

            flags.hit = true;
        }
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0);
        model = Transform.Rotate(model, _deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord(model);
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        model = parent1;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (removeCnt > 0)
            return;
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        blend = Gfx.Blend.Alpha;
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", linePoint);
        blend = Gfx.Blend.Additive;
        linePoint.draw(model, tint, blend, key + "-draw-1");
    }

    public virtual float deg()
    {
        return _deg;
    }

    public const float FORCE = 3000;
    public const float SIZE = 1.2f;
    public const float MASS = 20;
    public static Rand rand;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1_(object[] args)
    {
        base.init_1_Boolean();
        field = (Field)args[0];
        particles = (ParticlePool)args[1];
        pos = new Vector();
        _deg = 0;
        cnt = 0;
        shape = new ShapeGroup();
        shape.addShape(new Square(world, MASS, 0, 0, SIZE * 0.2f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(0.2f, 0.4f, 0, 0.8f);
        linePoint.alpha(0.5f);
    }
}

public class ShotPool : OdeActorPool<Shot>
{
    public ShotPool(int n, object[] args) : base(n, args, () => new Shot())
    {
    }
}

public class EnhancedShot : OdeActor
{
    public Field field;
    public ParticlePool particles;
    public Vector pos;
    public int cnt;
    public int removeCnt;
    public float _deg;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public virtual void set_2(Vector3 p, float d)
    {
        base.set_1();
        pos.x = p.x - sin(d) * 0.5f;
        pos.y = p.y + cos(d) * 0.5f;
        McdPhysics.BodyPosition(_bodyId, pos.x, pos.y, 0);
        _deg = d;
        setDeg(d);
        shape.setMass_3(this);
        shape.setGeom(this, null);
        addForce(-sin(d) * FORCE, cos(d) * FORCE);
        cnt = 0;
        removeCnt = 0;
        linePoint.init_0();
    }

    public override void move_0()
    {
        cnt++;
        float[] p = McdPhysics.BodyVector(_bodyId, 0);
        pos.x = p[0];
        pos.y = p[1];
        McdPhysics.BodyPosition(_bodyId, pos.x, pos.y, 0);
        if (removeCnt > 0)
        {
            removeCnt++;
            if (removeCnt > 5)
            {
                remove_0();
                return;
            }
        }

        if (!(field.checkInField_1_Vector(pos)))
        {
            remove_0();
            return;
        }

        recordLinePoints_0();
        doCollide();
    }

    public override void collide_2(OdeActor actor, Collision flags)
    {
        flags.feedback = false;
        flags.hit = flags.feedback;
        Enemy e = ((actor is Enemy ? (Enemy)actor : null));
        if ((e) != null)
        {
            if (removeCnt <= 0)
            {
                removeCnt = 1;
                for (int i = 0; i < 3; i++)
                {
                    Particle p = particles.getInstanceForced();
                    float d = deg() + PI + rand.nextSignedFloat(0.4f);
                    float v = 0.2f + rand.nextFloat(0.2f);
                    p.set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(pos, -sin(d) * v, cos(d) * v, 0.15f + rand.nextFloat(0.15f), 0.5f, 1, 0);
                }

                SoundManager.playSe("hit.wav");
            }

            flags.hit = true;
        }
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0);
        model = Transform.Rotate(model, _deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord(model);
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        model = parent1;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (removeCnt > 0)
            return;
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        blend = Gfx.Blend.Alpha;
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", linePoint);
        blend = Gfx.Blend.Additive;
        linePoint.draw(model, tint, blend, key + "-draw-1");
    }

    public virtual float deg()
    {
        return _deg;
    }

    public const float FORCE = 10000;
    public const float SIZE = 2;
    public const float MASS = 50;
    public static Rand rand;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1_(object[] args)
    {
        base.init_1_Boolean();
        field = (Field)args[0];
        particles = (ParticlePool)args[1];
        pos = new Vector();
        _deg = 0;
        cnt = 0;
        shape = new ShapeGroup();
        shape.addShape(new Triangle(world, MASS, 0, 0, SIZE * 0.33f, SIZE));
        linePoint = new LinePoint(field);
        linePoint.setSpectrumParams(0.9f, 0.6f, 0.3f, 0.75f);
    }
}

public class EnhancedShotPool : OdeActorPool<EnhancedShot>
{
    public EnhancedShotPool(int n, object[] args) : base(n, args, () => new EnhancedShot())
    {
    }
}
