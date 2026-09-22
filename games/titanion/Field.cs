// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Field
{
    public const float PIT_SIZE_Y_RATIO = 12.0f;
    public const float CIRCLE_RADIUS = 64.0f;
    public const float EYE_POS_DIST_RATIO = 1.25f;
    public const float X_EXPANSION_RATIO = 1.0f;
    public const float SIDEWALL_WIDTH = 145;
    public const float TORUS_Y = -24.0f;
    public Frame frame;
    public TtnScreen screen;
    public Vector _size, _outerSize;
    public Vector3 _eyePos;
    public float eyeDeg;
    public Vector3 circlePos;
    public int cnt;
    public Field(Frame frame, TtnScreen screen)
    {
        this.frame = frame;
        this.screen = screen;
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

    public virtual void setLookAt()
    {
        Drawing.LoadIdentity();
        Drawing.ortho = false;
        glRotatef(-eyeDeg * 180 / PI, 0, 1, 0);
        glTranslatef(-_eyePos.x, -_eyePos.y, -_eyePos.z);
    }

    public virtual void resetLookAt()
    {
        Drawing.LoadIdentity();
        Drawing.ortho = false;
        glTranslatef(0, 0, -1);
    }

    public virtual void beginDrawingFront()
    {
        Drawing.LoadIdentity();
        Drawing.ortho = true;
        drawSidewall();
    }

    public virtual void drawSidewall()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        TtnScreen.setColor(0.25f, 0.25f, 0.25f, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0);
        glVertex3f(SIDEWALL_WIDTH, 0, 0);
        glVertex3f(SIDEWALL_WIDTH, 480, 0);
        glVertex3f(0, 480, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(640, 0, 0);
        glVertex3f(640 - SIDEWALL_WIDTH, 0, 0);
        glVertex3f(640 - SIDEWALL_WIDTH, 480, 0);
        glVertex3f(640, 480, 0);
        glEnd();
        TtnScreen.setColor(1.0f, 1.0f, 1.0f, 0.8f);
        glBegin(GL_LINES);
        glVertex3f(SIDEWALL_WIDTH, 0, 0);
        glVertex3f(SIDEWALL_WIDTH, 480, 0);
        glVertex3f(640 - SIDEWALL_WIDTH, 0, 0);
        glVertex3f(640 - SIDEWALL_WIDTH, 480, 0);
        glEnd();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }

    public virtual void move_0()
    {
        cnt++;
    }

    public virtual void drawBack()
    {
        glPushMatrix();
        glTranslatef(0, TORUS_Y, 0);
        drawTorusShape(PI / 2);
        glPopMatrix();
    }

    public virtual void drawFront()
    {
        glPushMatrix();
        glTranslatef(0, TORUS_Y, 0);
        drawTorusShape(-PI / 2);
        glPopMatrix();
    }

    public virtual void drawTorusShape(float d1s)
    {
        Vector3 cp = new Vector3();
        cp.y = 0;
        Vector3 ringOfs = new Vector3();
        float torusRad = CIRCLE_RADIUS * 0.9f;
        float ringRad = 0;
        float d1 = 0;
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glBegin(GL_QUADS);
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
                TtnScreen.setColor(0.3f, 0.3f, 0.3f, 0.8f);
                TtnScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                TtnScreen.glVertex(ringOfs);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                TtnScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2);
                TtnScreen.setColor(0.3f, 0.3f, 0.3f, 0.2f);
                TtnScreen.glVertex(ringOfs);
            }
        }

        glEnd();
        glBegin(GL_LINE_STRIP);
        ringRad = CIRCLE_RADIUS * 0.3f;
        TtnScreen.setColor(0.1f, 0.1f, 0.1f);
        d1 = d1s;
        for (int i = 0; i < 16; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = cnt * 0.003f;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1 + PI * 2 / 32 * 0.1f) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32 * 0.1f) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.1f, d2 + PI * 2 / 16 * 0.1f);
                TtnScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.1f, d2 + PI * 2 / 16 * 0.9f);
                TtnScreen.glVertex(ringOfs);
                cp.x = sin(d1 + PI * 2 / 32 * 0.9f) * torusRad;
                cp.z = cos(d1 + PI * 2 / 32 * 0.9f) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.9f, d2 + PI * 2 / 32 * 0.1f);
                TtnScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32 * 0.9f, d2 + PI * 2 / 16 * 0.9f);
                TtnScreen.glVertex(ringOfs);
            }
        }

        glEnd();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
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
