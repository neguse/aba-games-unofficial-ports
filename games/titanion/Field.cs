// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Field
{
    public const float PIT_SIZE_Y_RATIO = 12.0f;
    public const float CIRCLE_RADIUS = 64.0f;
    public const float EYE_POS_DIST_RATIO = 1.25f;
    public const float X_EXPANSION_RATIO = 1.0f;
    public const float SIDEWALL_WIDTH = 145;
    public const float TORUS_Y = -24.0f;
    public Frame frame;
    public Vector _size, _outerSize;
    public Vector3 _eyePos;
    public float eyeDeg;
    public Vector3 circlePos;
    public int cnt;
    Mesh sidewall = new Mesh("sidewall");
    public Field(Frame frame)
    {
        this.frame = frame;
        float[] wallColor;
        wallColor = new float[] { 0.25f, 0.25f, 0.25f, 0.5f };
        sidewall.Vertex(0, 0, 0, wallColor);
        sidewall.Vertex(SIDEWALL_WIDTH, 0, 0, wallColor);
        sidewall.Vertex(SIDEWALL_WIDTH, 480, 0, wallColor);
        sidewall.Vertex(0, 480, 0, wallColor);
        sidewall.Vertex(640, 0, 0, wallColor);
        sidewall.Vertex(640 - SIDEWALL_WIDTH, 0, 0, wallColor);
        sidewall.Vertex(640 - SIDEWALL_WIDTH, 480, 0, wallColor);
        sidewall.Vertex(640, 480, 0, wallColor);
        wallColor = new float[] { 1.0f, 1.0f, 1.0f, 0.8f };
        sidewall.Vertex(SIDEWALL_WIDTH, 0, 0, wallColor);
        sidewall.Vertex(SIDEWALL_WIDTH, 480, 0, wallColor);
        sidewall.Vertex(640 - SIDEWALL_WIDTH, 0, 0, wallColor);
        sidewall.Vertex(640 - SIDEWALL_WIDTH, 480, 0, wallColor);
        sidewall.Quads(0, 8); sidewall.Line(8, 9); sidewall.Line(10, 11);
        _size = new Vector(12, 12);
        _outerSize = new Vector(13, 13);
        _eyePos = new Vector3();
        circlePos = new Vector3();
        set_0();
    }

    public virtual void set_0()
    {
        {
            {
                _eyePos.z = 0;
                _eyePos.y = _eyePos.z;
            }

            _eyePos.x = _eyePos.y;
        }

        eyeDeg = 0;
        cnt = 0;
    }

    public virtual bool contains_1(Vector p)
    {
        return contains_2(p.x, p.y);
    }

    public virtual bool contains_2(float x, float y)
    {
        return _size.contains_3(x, y);
    }

    public virtual bool containsOuter_1(Vector p)
    {
        return containsOuter_2(p.x, p.y);
    }

    public virtual bool containsOuter_2(float x, float y)
    {
        return _outerSize.contains_3(x, y);
    }

    public virtual bool containsOuterY(float y)
    {
        return (((y >= -_outerSize.y)) && ((y <= _outerSize.y)));
    }

    public virtual bool containsIncludingPit(Vector p)
    {
        return (((p.y >= -_outerSize.y)) && ((p.y <= _size.y * PIT_SIZE_Y_RATIO * 1.1f)));
    }

    public virtual float normalizeX(float x)
    {
        float rx = x;
        float hd = CIRCLE_RADIUS * PI / X_EXPANSION_RATIO;
        if (rx < -hd)
            rx = hd * 2 - (-rx % (hd * 2));
        return (rx + hd) % (hd * 2) - hd;
    }

    public virtual float calcCircularDist_2(Vector p1, Vector p2)
    {
        float ax = fabs(normalizeX(p1.x - p2.x));
        float ay = fabs(p1.y - p2.y);
        if (ax > ay)
            return ax + ay / 2;
        else
            return ay + ax / 2;
    }

    public virtual float circularDistance()
    {
        return CIRCLE_RADIUS * PI * 2 / X_EXPANSION_RATIO;
    }

    public virtual Vector3 calcCircularPos_1(Vector p)
    {
        return calcCircularPos_2(p.x, p.y);
    }

    public virtual Vector3 calcCircularPos_2(float x, float y)
    {
        float d = calcCircularDeg(x);
        if (y < _size.y)
        {
            circlePos.x = sin(d) * CIRCLE_RADIUS;
            circlePos.z = cos(d) * CIRCLE_RADIUS;
            circlePos.y = y;
        }
        else if (y < _size.y * 3)
        {
            float cd = (y - _size.y) * PI / 2 / (_size.y * 2);
            float cr = CIRCLE_RADIUS * (0.8f + 0.2f * cos(cd));
            circlePos.x = sin(d) * cr;
            circlePos.z = cos(d) * cr;
            circlePos.y = _size.y + sin(cd) * CIRCLE_RADIUS * 0.2f;
        }
        else if (y < _size.y * 7)
        {
            float cd = (y - _size.y * 3) * PI / 2 / (_size.y * 4);
            float cr = CIRCLE_RADIUS * (0.8f - 0.4f * sin(cd));
            circlePos.x = sin(d) * cr;
            circlePos.z = cos(d) * cr;
            circlePos.y = _size.y - CIRCLE_RADIUS * 0.2f + cos(cd) * CIRCLE_RADIUS * 0.4f;
        }
        else
        {
            float cr = CIRCLE_RADIUS * 0.4f;
            circlePos.x = sin(d) * cr;
            circlePos.z = cos(d) * cr;
            circlePos.y = _size.y - CIRCLE_RADIUS * 0.2f - (y - _size.y * 7);
        }

        return circlePos;
    }

    public virtual float calcCircularDeg(float x)
    {
        return x * X_EXPANSION_RATIO / CIRCLE_RADIUS;
    }

    public virtual float calcCircularDist_1(float d)
    {
        return d * CIRCLE_RADIUS / X_EXPANSION_RATIO;
    }

    public virtual bool checkHitDist_4(Vector pos, Vector p, Vector pp, float dist)
    {
        float bmvx = 0, bmvy = 0, inaa = 0;
        bmvx = pp.x;
        bmvy = pp.y;
        bmvx = bmvx - (p.x);
        bmvy = bmvy - (p.y);
        bmvx = normalizeX(bmvx);
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx = 0, sofsy = 0, inab = 0, hd = 0;
            sofsx = pos.x;
            sofsy = pos.y;
            sofsx = sofsx - (p.x);
            sofsy = sofsy - (p.y);
            sofsx = normalizeX(sofsx);
            inab = bmvx * sofsx + bmvy * sofsy;
            if (((inab >= 0)) && ((inab <= inaa)))
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if (((hd >= 0)) && ((hd <= dist)))
                    return true;
            }
        }

        return false;
    }

    public virtual void addSlowdownRatio(float sr)
    {
        frame.addSlowdownRatio(sr);
    }

    public virtual void setEyePos(Vector p)
    {
        eyeDeg = calcCircularDeg(p.x) * 0.25f;
        _eyePos.x = sin(eyeDeg) * CIRCLE_RADIUS * EYE_POS_DIST_RATIO;
        _eyePos.z = cos(eyeDeg) * CIRCLE_RADIUS * EYE_POS_DIST_RATIO;
    }

    public virtual float[] setLookAt()
    {
        return Transform.Translate(Transform.Rotate(Transform.Perspective(), -eyeDeg * 180 / PI, 0, 1, 0), -_eyePos.x, -_eyePos.y, -_eyePos.z);
    }

    public virtual float[] resetLookAt()
    {
        return Transform.Translate(Transform.Perspective(), 0, 0, -1);
    }

    public virtual float[] beginDrawingFront()
    {
        float[] model = Transform.Ortho(); drawSidewall(model, null, Gfx.Blend.Additive); return model;
    }

    public virtual void drawSidewall(float[] model, float[] color, Gfx.Blend blend)
    {
        Gfx.Draw(sidewall.count, sidewall.Bindings(model, null, 1, false),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha });
    }

    public virtual void move_0()
    {
        cnt++;
    }

    public virtual void drawBack(float[] model, float[] color, Gfx.Blend blend)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, 0, TORUS_Y, 0);
        drawTorusShape(model, color, blend, PI / 2);
        model = parent1;
    }

    public virtual void drawFront(float[] model, float[] color, Gfx.Blend blend)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, 0, TORUS_Y, 0);
        drawTorusShape(model, color, blend, -PI / 2);
        model = parent1;
    }

    public virtual void drawTorusShape(float[] model, float[] color, Gfx.Blend blend, float d1s)
    {
        Vector3 cp = new Vector3();
        cp.y = 0;
        Vector3 ringOfs = new Vector3();
        float torusRad = CIRCLE_RADIUS * 0.9f;
        float ringRad = 0;
        float d1 = 0;
        blend = Gfx.Blend.Alpha;
        var part1 = new Mesh("Field-drawTorusShape-1" + "-" + d1s.ToString());
        ringRad = CIRCLE_RADIUS * 0.3f;
        d1 = d1s;
        for (int i = 0; i < 16; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = cnt * 0.003f;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1) * torusRad;
                cp.z = cos(d1) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1, d2);
                color = new float[] { 0.3f, 0.3f, 0.3f, 0.8f };
                part1.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                part1.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                part1.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2);
                color = new float[] { 0.3f, 0.3f, 0.3f, 0.2f };
                part1.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
            }
        }

        part1.Quads(0, part1.vertexCount);
        Gfx.Draw(part1.count, part1.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        var part2 = new Mesh("Field-drawTorusShape-2" + "-" + d1s.ToString());
        ringRad = CIRCLE_RADIUS * 0.3f;
        color = new float[] { 0.1f, 0.1f, 0.1f, 1 };
        d1 = d1s;
        for (int i = 0; i < 16; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = cnt * 0.003f;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1 + PI * 2 / 32 * 0.1f) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32 * 0.1f) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.1f, d2 + PI * 2 / 16 * 0.1f);
                part2.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.1f, d2 + PI * 2 / 16 * 0.9f);
                part2.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                cp.x = sin(d1 + PI * 2 / 32 * 0.9f) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32 * 0.9f) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.9f, d2 + PI * 2 / 32 * 0.1f);
                part2.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.9f, d2 + PI * 2 / 16 * 0.9f);
                part2.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, color);
            }
        }

        part2.LineStrip(0, part2.vertexCount);
        Gfx.Draw(part2.count, part2.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        blend = Gfx.Blend.Additive;
    }

    public virtual void createRingOffset(Vector3 ringOfs, Vector3 centerPos, float rad, float d1, float d2)
    {
        ringOfs.x = 0;
        ringOfs.y = 0;
        ringOfs.z = rad;
        ringOfs.rollX(d2);
        ringOfs.rollY(-d1);
        ringOfs.opAddAssign(centerPos);
    }

    public virtual Vector3 eyePos()
    {
        return _eyePos;
    }

    public virtual Vector size()
    {
        return _size;
    }
}
