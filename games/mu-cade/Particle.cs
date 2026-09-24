// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Particle : Actor
{
    public static Rand rand;
    public Field field;
    public Vector3 pos;
    public Vector3 vel;
    public float size;
    public Vector3 size3;
    public float deg;
    public float md;
    public int cnt;
    public float r, g, b;
    public float decayRatio;
    public LinePoint linePoint;
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
        field = (Field)args[0];
        pos = new Vector3();
        vel = new Vector3();
        size = 1;
        size3 = new Vector3();
        linePoint = new LinePoint(field);
        linePoint.setPos(new Vector3(0, 0, 0));
        md = 0;
        deg = md;
        b = 0;
        g = b;
        r = g;
    }

    public virtual void set_8_Vector_Single_Single_Single_Single_Single_Single_Int32(Vector p, float vx, float vy, float sz, float r, float g, float b, int c = 60)
    {
        set_11_Single_Single_Single_Single_Single_Single_Single_Single_Single_Single_Int32(p.x, p.y, 0, vx, vy, 0, sz, r, g, b, c);
    }

    public virtual void set_8_Vector3_Single_Single_Single_Single_Single_Single_Int32(Vector3 p, float vx, float vy, float sz, float r, float g, float b, int c = 60)
    {
        set_11_Single_Single_Single_Single_Single_Single_Single_Single_Single_Single_Int32(p.x, p.y, p.z, vx, vy, 0, sz, r, g, b, c);
    }

    public virtual void set_9_Single_Single_Single_Single_Single_Single_Single_Single_Int32(float x, float y, float vx, float vy, float sz, float r, float g, float b, int c = 60)
    {
        set_11_Single_Single_Single_Single_Single_Single_Single_Single_Single_Single_Int32(x, y, 0, vx, vy, 0, sz, r, g, b, c);
    }

    public virtual void set_11_Single_Single_Single_Single_Single_Single_Single_Single_Single_Single_Int32(float x, float y, float z, float vx, float vy, float vz, float sz, float r, float g, float b, int c = 60)
    {
        pos.x = x;
        pos.y = y;
        pos.z = z;
        vel.x = vx;
        vel.y = vy;
        vel.z = vz;
        size = sz;
        deg = rand.nextFloat(PI * 2);
        md = rand.nextSignedFloat(0.3f);
        cnt = c + rand.nextInt(c);
        if (cnt < 4)
            cnt = 4;
        decayRatio = 1 - 0.02f * 60 / cnt;
        this.r = r;
        this.g = g;
        this.b = b;
        linePoint.setSpectrumParams(r, g, b, 1);
        linePoint.init_0();
        size3.z = size;
        size3.y = size3.z;
        size3.x = size3.y;
        linePoint.setSize(size3);
        exists = true;
    }

    public override void move_0()
    {
        pos += vel;
        vel *= 0.98f;
        this.r *= decayRatio;
        this.g *= decayRatio;
        this.b *= decayRatio;
        linePoint.setSpectrumParams(r, g, b, 1);
        deg += md;
        recordLinePoints_0();
        cnt--;
        if (cnt <= 0)
            exists = false;
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord(model);
        linePoint.record(-1, 0, 0);
        linePoint.record(1, 0, 0);
        linePoint.endRecord();
        model = parent1;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        linePoint.drawWithSpectrumColor(model, tint, blend, key + "-drawWithSpectrumColor-1");
    }
}

public class ParticlePool : ActorPool<Particle>
{
    public ParticlePool(int n, object[] args) : base(n, args, () => new Particle())
    {
    }
}

public class ConnectedParticle : Actor
{
    public const float SPRING_CONSTANT = 0.04f;
    public static Rand rand;
    public Field field;
    public Vector3 _pos;
    public Vector3 _vel;
    public float[] rot = McdArrays.Make<float>(16, () => 0);
    public bool enableRotate;
    public int cnt;
    public float decayRatio;
    public float r, g, b;
    public float baseLength;
    public ConnectedParticle prevParticle;
    public LinePoint linePoint;
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
        field = (Field)args[0];
        _pos = new Vector3();
        _vel = new Vector3();
        linePoint = new LinePoint(field);
        linePoint.setPos(new Vector3(0, 0, 0));
        linePoint.setSize(new Vector3(1, 1, 1));
        b = 0;
        g = b;
        r = g;
        baseLength = 0;
    }

    public virtual void set_11_Single_Single_Single_Single_Single_Single_Single_Int32_Single_ConnectedParticle_Boolean(float x, float y, float d, float s, float r, float g, float b, int c, float bl = 0, ConnectedParticle pp = null, bool decay = true)
    {
        set_12(x, y, 0, d, s, r, g, b, c, bl, pp, decay);
    }

    public virtual void set_12(float x, float y, float z, float d, float s, float r, float g, float b, int c, float bl = 0, ConnectedParticle pp = null, bool decay = true)
    {
        _pos.x = x;
        _pos.y = y;
        _pos.z = z;
        _vel.x = -sin(d) * s;
        _vel.y = cos(d) * s;
        _vel.z = 0;
        enableRotate = false;
        cnt = c;
        if (cnt < 4)
            cnt = 4;
        if (decay)
            decayRatio = 1 - 0.07f * 60 / cnt;
        else
            decayRatio = 1 - 0.01f * 60 / cnt;
        this.r = r;
        this.g = g;
        this.b = b;
        baseLength = bl;
        prevParticle = pp;
        linePoint.setSpectrumParams(r, g, b, 1);
        linePoint.init_0();
        exists = true;
    }

    public virtual void setRot(float[] r)
    {
        for (int i = 0; i < 16; i++)
            rot[i] = r[i];
        enableRotate = true;
    }

    public override void move_0()
    {
        _vel *= 0.96f;
        if (_vel.x > 2)
            _vel.x = 2;
        else if (_vel.x < -2)
            _vel.x = -2;
        if (_vel.y > 2)
            _vel.y = 2;
        else if (_vel.y < -2)
            _vel.y = -2;
        _pos += _vel;
        this.r *= decayRatio;
        this.g *= decayRatio;
        this.b *= decayRatio;
        linePoint.setSpectrumParams(r, g, b, 1);
        if ((prevParticle) != null && prevParticle.exists)
        {
            float ds = pos().dist_1_Vector3(prevParticle.pos());
            float lo = ds - baseLength;
            if (lo > 0.01f && ds > 0.01f)
            {
                float d = atan2(prevParticle.pos().x - pos().x, prevParticle.pos().y - pos().y);
                float ax = sin(d) * lo * SPRING_CONSTANT;
                float ay = cos(d) * lo * SPRING_CONSTANT;
                _vel.x += ax;
                _vel.y += ay;
                prevParticle.vel().x -= ax;
                prevParticle.vel().y -= ay;
            }
        }

        cnt--;
        if (cnt <= 0)
            exists = false;
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        if (!((prevParticle) != null) || !(prevParticle.exists))
            return;
        float[] parent1 = model;
        model = Transform.Translate(model, _pos.x, _pos.y, _pos.z);
        if (enableRotate)
            model = Transform.Multiply(model, rot);
        linePoint.beginRecord(model);
        linePoint.record(0, 0, 0);
        linePoint.record((prevParticle.pos().x - _pos.x) * 2, (prevParticle.pos().y - _pos.y) * 2, (prevParticle.pos().z - _pos.z) * 2);
        linePoint.endRecord();
        model = parent1;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (!((prevParticle) != null) || !(prevParticle.exists))
            return;
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        linePoint.drawWithSpectrumColor(model, tint, blend, key + "-drawWithSpectrumColor-1");
        float[] parent1 = model;
        model = Transform.Translate(model, _pos.x, _pos.y, _pos.z);
        tint = new float[] { r, g, b, 1 };
        var part2 = new Mesh(key + "-ConnectedParticle-draw-2");
        part2.Vertex(0, 0, 0, tint);
        part2.Vertex(prevParticle.pos().x - _pos.x, prevParticle.pos().y - _pos.y, prevParticle.pos().z - _pos.z, tint);
        for (int vi = 0; vi + 1 < part2.vertexCount; vi += 2) part2.Line(vi, vi + 1);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public virtual Vector3 pos()
    {
        return _pos;
    }

    public virtual Vector3 vel()
    {
        return _vel;
    }
}

public class ConnectedParticlePool : ActorPool<ConnectedParticle>
{
    public ConnectedParticlePool(int n, object[] args) : base(n, args, () => new ConnectedParticle())
    {
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        foreach (ConnectedParticle cp in actor)
            if (cp.exists)
                cp.recordLinePoints_0();
    }
}

public class TailParticle : Actor
{
    public const int COUNT = 60;
    public const float SIZE_CONST = 1;
    public static Rand rand;
    public static Vector3 trgPos;
    public static float trgDeg;
    public Field field;
    public Ship ship;
    public Vector3 pos;
    public Vector3 vel;
    public float size;
    public Vector3 size3;
    public float deg;
    public float md;
    public int cnt;
    public float r, g, b;
    public ShapeGroup shape;
    public LinePoint linePoint;
    public static void init_0()
    {
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void setTarget(Vector3 p, float d)
    {
        trgPos = p;
        trgDeg = d;
    }

    public override void init_1_(object[] args)
    {
        field = (Field)args[0];
        ship = ((args[1] is Ship ? (Ship)args[1] : null));
        pos = new Vector3();
        vel = new Vector3();
        deg = 0;
        size = 1;
        size3 = new Vector3();
        shape = new ShapeGroup();
        shape.addShape(new Square(null, 0, 0, 0, SIZE_CONST * ShipTail.WIDTH, SIZE_CONST));
        linePoint = new LinePoint(field);
        b = 0;
        g = b;
        r = g;
    }

    public virtual void set_8_Single_Single_Single_Single_Single_Single_Single_Int32(float x, float y, float z, float sz, float r, float g, float b, int c)
    {
        pos.x = x;
        pos.y = y;
        pos.z = z;
        vel.x = rand.nextSignedFloat(0.3f);
        vel.y = 0.3f;
        vel.z = 0;
        size = sz;
        deg = rand.nextFloat(PI * 2);
        md = rand.nextSignedFloat(0.3f);
        cnt = c;
        this.r = r;
        this.g = g;
        this.b = b;
        linePoint.setSpectrumParams(r, g, b, 0.5f);
        linePoint.init_0();
        size3.z = size;
        size3.y = size3.z;
        size3.x = size3.y;
        linePoint.setSize(size3);
        exists = true;
    }

    public override void move_0()
    {
        pos += vel;
        float vr = 1.0f - (float)cnt / COUNT;
        vr *= 0.25f;
        vel.x += (trgPos.x - pos.x) * 0.002f;
        vel.y += (trgPos.y - pos.y) * 0.002f;
        vel.z += (trgPos.z - pos.z) * 0.002f;
        vel *= 0.98f;
        pos.x += (trgPos.x - pos.x) * vr;
        pos.y += (trgPos.y - pos.y) * vr;
        pos.z += (trgPos.z - pos.z) * vr;
        r += (ShipTail.COLOR_R - r) * 0.03f;
        g += (ShipTail.COLOR_G - g) * 0.03f;
        b += (ShipTail.COLOR_B - b) * 0.03f;
        linePoint.setSpectrumParams(r, g, b, 0.5f);
        deg += md;
        md *= 0.9f;
        float od = trgDeg - deg;
        od = normalizeDeg(od);
        deg += od * vr;
        recordLinePoints_0();
        cnt--;
        if (cnt <= 0)
        {
            ship.addTail_1(size);
            exists = false;
        }
    }

    public virtual void recordLinePoints_0()
    {
        float[] model = Transform.Identity();
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        linePoint.beginRecord(model);
        shape.recordLinePoints_1(linePoint);
        linePoint.endRecord();
        model = parent1;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        linePoint.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
        linePoint.drawWithSpectrumColor(model, tint, blend, key + "-drawWithSpectrumColor-1");
    }
}

public class TailParticlePool : ActorPool<TailParticle>
{
    public TailParticlePool(int n, object[] args) : base(n, args, () => new TailParticle())
    {
    }
}

public class StarParticle : Actor
{
    public Field field;
    public Vector3 pos;
    public Vector3 vel;
    public float size;
    public int cnt;
    public override void init_1_(object[] args)
    {
        field = (Field)args[0];
        pos = new Vector3();
        vel = new Vector3();
        size = 1;
    }

    public virtual void set_5_Single_Single_Single_Single_Single(float x, float y, float z, float speed, float sz)
    {
        pos.x = x;
        pos.y = y;
        pos.z = z;
        vel.x = 0;
        vel.y = 0;
        vel.z = -speed;
        size = sz;
        exists = true;
    }

    public override void move_0()
    {
        pos += vel;
        if (pos.z < -100)
            exists = false;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        Mesh mesh = target;
        mesh.Vertex(pos.x, pos.y, pos.z, tint);
        mesh.Vertex(pos.x, pos.y, pos.z + size, tint);
    }
}

public class StarParticlePool : ActorPool<StarParticle>
{
    public StarParticlePool(int n, object[] args) : base(n, args, () => new StarParticle())
    {
    }
}

public class NumIndicator : Actor
{
    public Vector pos;
    public Vector vel;
    public float size, trgSize;
    public int cnt;
    public int num1, num2;
    public override void init_1_(object[] args)
    {
        pos = new Vector();
        vel = new Vector();
        size = 1;
        num1 = 0;
        num2 = -1;
    }

    public virtual void set_8_Int32_Int32_Single_Single_Single_Single_Single_Int32(int n1, int n2, float x, float y, float vx, float vy, float sz = 0.5f, int c = 300)
    {
        num1 = n1;
        num2 = n2;
        pos.x = x;
        pos.y = y;
        vel.x = vx;
        vel.y = vy;
        size = 0.1f;
        trgSize = sz;
        cnt = c;
        exists = true;
    }

    public override void move_0()
    {
        pos += vel;
        size += (trgSize - size) * 0.05f;
        cnt--;
        if (cnt <= 0)
            exists = false;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        if (num2 <= 1)
        {
            Letter.drawNumSign(model, tint, blend, key + "-drawNumSign-1", num1, pos.x + Letter.getWidthNum(num1, size) / 2, pos.y, size);
        }
        else
        {
            float wd = Letter.getWidthNum(num1, size) + Letter.getWidth(1, size) + Letter.getWidthNum(num2, size);
            float x = default(float);
            x = pos.x - wd / 2 + Letter.getWidthNum(num1, size);
            Letter.drawNumSign(model, tint, blend, key + "-drawNumSign-2", num1, x, pos.y, size);
            x = pos.x + wd / 2;
            Letter.drawNumSign(model, tint, blend, key + "-drawNumSign-3", num2, x, pos.y, size, 33);
        }
    }
}

public class NumIndicatorPool : ActorPool<NumIndicator>
{
    public NumIndicatorPool(int n, object[] args) : base(n, args, () => new NumIndicator())
    {
    }
}
