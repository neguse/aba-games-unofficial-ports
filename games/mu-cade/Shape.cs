// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public interface Shape
{
    public void addMass(OdeMass m, Vector3 sizeScale = null, float massScale = 1);
    public void addGeom_3(OdeActor oa, OdeHandle sid, Vector3 sizeScale = null);
    public void recordLinePoints_1(LinePoint lp);
    public void drawShadow_1(LinePoint lp);
}

public class ShapeGroup : Shape
{
    public Shape[] shapes;
    public virtual void addShape(Shape s)
    {
        shapes = McdArrays.Append(shapes, s);
    }

    public virtual void setMass_3(OdeActor oa, Vector3 sizeScale = null, float massScale = 1)
    {
        OdeMass m = McdPhysics.Mass();
        addMass(m, sizeScale, massScale);
        oa.setMass_1(m);
    }

    public virtual void setGeom(OdeActor oa, OdeHandle sid, Vector3 sizeScale = null)
    {
        addGeom_3(oa, sid, sizeScale);
    }

    public virtual void addMass(OdeMass m, Vector3 sizeScale = null, float massScale = 1)
    {
        foreach (Shape s in shapes)
            s.addMass(m, sizeScale, massScale);
    }

    public virtual void addGeom_3(OdeActor oa, OdeHandle sid, Vector3 sizeScale = null)
    {
        foreach (Shape s in shapes)
            s.addGeom_3(oa, sid, sizeScale);
    }

    public virtual void recordLinePoints_1(LinePoint lp)
    {
        foreach (Shape s in shapes)
            s.recordLinePoints_1(lp);
    }

    public virtual void drawShadow_1(LinePoint lp)
    {
        foreach (Shape s in shapes)
            s.drawShadow_1(lp);
    }
}

public abstract class ShapeBase : Shape
{
    public World world;
    public Vector3 pos;
    public Vector3 size;
    public float mass = 1;
    public float shapeBoxScale = 1;
    public virtual void addMass(OdeMass m, Vector3 sizeScale = null, float massScale = 1)
    {
        OdeMass sm = McdPhysics.Mass();
        if ((sizeScale) != null)
        {
            McdPhysics.MassBox(sm, size.x * sizeScale.x, size.y * sizeScale.y, size.z * sizeScale.z);
            McdPhysics.MassTranslate(sm, pos.x * sizeScale.x, pos.y * sizeScale.y, pos.z * sizeScale.z);
        }
        else
        {
            McdPhysics.MassBox(sm, size.x, size.y, size.z);
            McdPhysics.MassTranslate(sm, pos.x, pos.y, pos.z);
        }

        McdPhysics.MassAdjust(sm, mass * massScale);
        McdPhysics.MassAdd(m, sm);
    }

    public virtual void addGeom_3(OdeActor oa, OdeHandle sid, Vector3 sizeScale = null)
    {
        if (pos.x == 0 && pos.y == 0 && pos.z == 0)
        {
            OdeHandle bg_0 = default(OdeHandle);
            if ((sizeScale) != null)
            {
                bg_0 = McdPhysics.Box(sid, size.x * sizeScale.x * shapeBoxScale, size.y * sizeScale.y * shapeBoxScale, size.z * sizeScale.z * shapeBoxScale);
            }
            else
            {
                bg_0 = McdPhysics.Box(sid, size.x * shapeBoxScale, size.y * shapeBoxScale, size.z * shapeBoxScale);
            }

            oa.addGeom_1(bg_0);
        }
        else
        {
            OdeHandle tg = McdPhysics.Transform(sid);
            OdeHandle bg_1 = default(OdeHandle);
            if ((sizeScale) != null)
            {
                bg_1 = McdPhysics.Box(null, size.x * sizeScale.x * shapeBoxScale, size.y * sizeScale.y * shapeBoxScale, size.z * sizeScale.z * shapeBoxScale);
                McdPhysics.GeomPosition(bg_1, pos.x * sizeScale.x, pos.y * sizeScale.y, pos.z * sizeScale.z);
            }
            else
            {
                bg_1 = McdPhysics.Box(null, size.x * shapeBoxScale, size.y * shapeBoxScale, size.z * shapeBoxScale);
                McdPhysics.GeomPosition(bg_1, pos.x, pos.y, pos.z);
            }

            McdPhysics.TransformGeom(tg, bg_1);
            oa.addGeom_1(tg);
            oa.addTransformedGeom(bg_1);
        }
    }

    public abstract void recordLinePoints_1(LinePoint lp);
    public abstract void drawShadow_1(LinePoint lp);
}

public class Square : ShapeBase
{
    public Square(World world, float mass, float px, float py, float sx, float sy, float pz = 0, float sz = 1)
    {
        this.world = world;
        this.mass = mass;
        pos = new Vector3(px, py, pz);
        size = new Vector3(sx, sy, sz);
    }

    public override void recordLinePoints_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        lp.record(-1, -1, 0);
        lp.record(1, -1, 0);
        lp.record(1, -1, 0);
        lp.record(1, 1, 0);
        lp.record(1, 1, 0);
        lp.record(-1, 1, 0);
        lp.record(-1, 1, 0);
        lp.record(-1, -1, 0);
    }

    public override void drawShadow_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        if (!(lp.setShadowColor()))
            return;
        glBegin(GL_TRIANGLE_FAN);
        lp.vertex(-1, -1, 0);
        lp.vertex(1, -1, 0);
        lp.vertex(1, 1, 0);
        lp.vertex(-1, 1, 0);
        glEnd();
    }
}

public class Sphere : ShapeBase
{
    public Sphere(World world, float mass, float px, float py, float rad)
    {
        this.world = world;
        this.mass = mass;
        pos = new Vector3(px, py, 0);
        size = new Vector3(rad, rad, rad);
    }

    public override void addGeom_3(OdeActor oa, OdeHandle sid, Vector3 sizeScale = null)
    {
        if (pos.x == 0 && pos.y == 0 && pos.z == 0)
        {
            OdeHandle bg_0 = default(OdeHandle);
            if ((sizeScale) != null)
            {
                bg_0 = McdPhysics.Sphere(sid, size.x * sizeScale.x * shapeBoxScale);
            }
            else
            {
                bg_0 = McdPhysics.Sphere(sid, size.x * shapeBoxScale);
            }

            oa.addGeom_1(bg_0);
        }
        else
        {
            OdeHandle tg = McdPhysics.Transform(sid);
            OdeHandle bg_1 = default(OdeHandle);
            if ((sizeScale) != null)
            {
                bg_1 = McdPhysics.Sphere(null, size.x * sizeScale.x * shapeBoxScale);
                McdPhysics.GeomPosition(bg_1, pos.x * sizeScale.x, pos.y * sizeScale.y, pos.z * sizeScale.z);
            }
            else
            {
                bg_1 = McdPhysics.Sphere(null, size.x * shapeBoxScale);
                McdPhysics.GeomPosition(bg_1, pos.x, pos.y, pos.z);
            }

            McdPhysics.TransformGeom(tg, bg_1);
            oa.addGeom_1(tg);
            oa.addTransformedGeom(bg_1);
        }
    }

    public override void recordLinePoints_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        lp.record(-1, -1, 0);
        lp.record(1, -1, 0);
        lp.record(1, -1, 0);
        lp.record(1, 1, 0);
        lp.record(1, 1, 0);
        lp.record(-1, 1, 0);
        lp.record(-1, 1, 0);
        lp.record(-1, -1, 0);
    }

    public override void drawShadow_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        if (!(lp.setShadowColor()))
            return;
        glBegin(GL_TRIANGLE_FAN);
        lp.vertex(-1, -1, 0);
        lp.vertex(1, -1, 0);
        lp.vertex(1, 1, 0);
        lp.vertex(-1, 1, 0);
        glEnd();
    }
}

public class Triangle : ShapeBase
{
    public Triangle(World world, float mass, float px, float py, float sx, float sy)
    {
        this.world = world;
        this.mass = mass;
        pos = new Vector3(px, py, 0);
        size = new Vector3(sx, sy, 1);
        shapeBoxScale = 1;
    }

    public override void recordLinePoints_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        lp.record(0, 1, 0);
        lp.record(1, -1, 0);
        lp.record(1, -1, 0);
        lp.record(-1, -1, 0);
        lp.record(-1, -1, 0);
        lp.record(0, 1, 0);
    }

    public override void drawShadow_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        if (!(lp.setShadowColor()))
            return;
        glBegin(GL_TRIANGLE_FAN);
        lp.vertex(0, 1, 0);
        lp.vertex(1, -1, 0);
        lp.vertex(-0, -1, 0);
        glEnd();
    }
}

public class Box : ShapeBase
{
    public Box(World world, float mass, float px, float py, float pz, float sx, float sy, float sz)
    {
        this.world = world;
        this.mass = mass;
        pos = new Vector3(px, py, pz);
        size = new Vector3(sx, sy, sz);
    }

    public override void recordLinePoints_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        lp.record(-1, -1, -1);
        lp.record(1, -1, -1);
        lp.record(1, -1, -1);
        lp.record(1, 1, -1);
        lp.record(1, 1, -1);
        lp.record(-1, 1, -1);
        lp.record(-1, 1, -1);
        lp.record(-1, -1, -1);
        lp.record(-1, -1, 1);
        lp.record(1, -1, 1);
        lp.record(1, -1, 1);
        lp.record(1, 1, 1);
        lp.record(1, 1, 1);
        lp.record(-1, 1, 1);
        lp.record(-1, 1, 1);
        lp.record(-1, -1, 1);
        lp.record(-1, -1, 1);
        lp.record(-1, -1, -1);
        lp.record(1, -1, 1);
        lp.record(1, -1, -1);
        lp.record(1, 1, 1);
        lp.record(1, 1, -1);
        lp.record(-1, 1, 1);
        lp.record(-1, 1, -1);
    }

    public override void drawShadow_1(LinePoint lp)
    {
        lp.setPos(pos);
        lp.setSize(size);
        if (!(lp.setShadowColor()))
            return;
        glBegin(GL_QUADS);
        lp.vertex(-1, -1, -1);
        lp.vertex(1, -1, -1);
        lp.vertex(1, 1, -1);
        lp.vertex(-1, 1, -1);
        lp.vertex(-1, -1, 1);
        lp.vertex(1, -1, 1);
        lp.vertex(1, 1, 1);
        lp.vertex(-1, 1, 1);
        lp.vertex(-1, -1, -1);
        lp.vertex(1, -1, -1);
        lp.vertex(1, -1, 1);
        lp.vertex(-1, -1, 1);
        lp.vertex(-1, 1, -1);
        lp.vertex(1, 1, -1);
        lp.vertex(1, 1, 1);
        lp.vertex(-1, 1, 1);
        lp.vertex(-1, -1, -1);
        lp.vertex(-1, 1, -1);
        lp.vertex(-1, 1, 1);
        lp.vertex(-1, -1, 1);
        lp.vertex(1, -1, -1);
        lp.vertex(1, 1, -1);
        lp.vertex(1, 1, 1);
        lp.vertex(1, -1, 1);
        glEnd();
    }
}

public class LinePoint
{
    public const int HISTORY_MAX = 40;
    public Field field;
    public Vector3[] pos;
    public Vector3[][] posHist;
    public int posIdx, histIdx;
    public Vector3 basePos, baseSize;
    public float[] m = McdArrays.Make<float>(16, () => 0);
    public bool isFirstRecord;
    public float spectrumColorR, spectrumColorG, spectrumColorB;
    public float spectrumColorRTrg, spectrumColorGTrg, spectrumColorBTrg;
    public float spectrumLength;
    public float _alpha, _alphaTrg;
    public bool _enableSpectrumColor;
    public LinePoint(Field field, int pointMax = 8)
    {
        init_0();
        pos = new Vector3[pointMax];
        posHist = new Vector3[HISTORY_MAX][];
        this.field = field;
        for (int idx_p_0 = 0; idx_p_0 < pointMax; idx_p_0++)
            pos[idx_p_0] = new Vector3();
        for (int idx_pp = 0; idx_pp < HISTORY_MAX; idx_pp++)
        {
            posHist[idx_pp] = new Vector3[pointMax];
            for (int idx_p_1 = 0; idx_p_1 < pointMax; idx_p_1++)
                posHist[idx_pp][idx_p_1] = new Vector3();
        }

        spectrumColorBTrg = 0;
        spectrumColorGTrg = spectrumColorBTrg;
        spectrumColorRTrg = spectrumColorGTrg;
        spectrumLength = 0;
        _alphaTrg = 1;
        _alpha = _alphaTrg;
    }

    public virtual void init_0()
    {
        posIdx = 0;
        histIdx = 0;
        isFirstRecord = true;
        spectrumColorB = 0;
        spectrumColorG = spectrumColorB;
        spectrumColorR = spectrumColorG;
        _enableSpectrumColor = true;
    }

    public virtual void setSpectrumParams(float r, float g, float b, float length)
    {
        spectrumColorRTrg = r;
        spectrumColorGTrg = g;
        spectrumColorBTrg = b;
        spectrumLength = length;
    }

    public virtual void beginRecord()
    {
        posIdx = 0;
        for (int i = 0; i < 16; i++)
            m[i] = Drawing.matrix[i];
    }

    public virtual void setPos(Vector3 p)
    {
        basePos = p;
    }

    public virtual void setSize(Vector3 s)
    {
        baseSize = s;
    }

    public virtual void record(float ox, float oy, float oz)
    {
        Vector3 translated = calcTranslatedPos(ox, oy, oz);
        float tx = translated.x, ty = translated.y, tz = translated.z;
        pos[posIdx].x = tx;
        pos[posIdx].y = ty;
        pos[posIdx].z = tz;
        posIdx++;
    }

    public virtual void endRecord()
    {
        histIdx++;
        if (histIdx >= HISTORY_MAX)
            histIdx = 0;
        if (isFirstRecord)
        {
            isFirstRecord = false;
            for (int j = 0; j < HISTORY_MAX; j++)
            {
                for (int i_0 = 0; i_0 < posIdx; i_0++)
                {
                    posHist[j][i_0].x = pos[i_0].x;
                    posHist[j][i_0].y = pos[i_0].y;
                    posHist[j][i_0].z = pos[i_0].z;
                }
            }
        }
        else
        {
            for (int i_1 = 0; i_1 < posIdx; i_1++)
            {
                posHist[histIdx][i_1].x = pos[i_1].x;
                posHist[histIdx][i_1].y = pos[i_1].y;
                posHist[histIdx][i_1].z = pos[i_1].z;
            }
        }

        diffuseSpectrum();
        if (_enableSpectrumColor)
        {
            spectrumColorR += (spectrumColorRTrg - spectrumColorR) * 0.1f;
            spectrumColorG += (spectrumColorGTrg - spectrumColorG) * 0.1f;
            spectrumColorB += (spectrumColorBTrg - spectrumColorB) * 0.1f;
        }
        else
        {
            spectrumColorR *= 0.9f;
            spectrumColorG *= 0.9f;
            spectrumColorB *= 0.9f;
        }

        _alpha += (_alphaTrg - _alpha) * 0.05f;
    }

    public virtual void diffuseSpectrum()
    {
        const float dfr = 0.01f;
        for (int j = 0; j < HISTORY_MAX; j++)
        {
            for (int i = 0; i < posIdx; i += 2)
            {
                float ox = posHist[j][i].x - posHist[j][i + 1].x;
                float oy = posHist[j][i].y - posHist[j][i + 1].y;
                float oz = posHist[j][i].z - posHist[j][i + 1].z;
                posHist[j][i].x += ox * dfr;
                posHist[j][i].y += oy * dfr;
                posHist[j][i].z += oz * dfr;
                posHist[j][i + 1].x -= ox * dfr;
                posHist[j][i + 1].y -= oy * dfr;
                posHist[j][i + 1].z -= oz * dfr;
            }
        }
    }

    public virtual void vertex(float ox, float oy, float oz)
    {
        Vector3 translated = calcTranslatedPos(ox, oy, oz);
        float tx = translated.x, ty = translated.y, tz = translated.z;
        glVertex3f(tx, ty, tz);
    }

    public virtual Vector3 calcTranslatedPos(float ox, float oy, float oz)
    {
        float x = basePos.x + baseSize.x / 2 * ox;
        float y = basePos.y + baseSize.y / 2 * oy;
        float z = basePos.z + baseSize.z / 2 * oz;
        float tx = m[0] * x + m[4] * y + m[8] * z + m[12];
        float ty = m[1] * x + m[5] * y + m[9] * z + m[13];
        float tz = m[2] * x + m[6] * y + m[10] * z + m[14];
        return new Vector3(tx, ty, tz);
    }

    public virtual bool setShadowColor()
    {
        if (spectrumColorR + spectrumColorG + spectrumColorB < 0.1f)
            return false;
        Screen.setColor(spectrumColorR * 0.3f, spectrumColorG * 0.3f, spectrumColorB * 0.3f);
        return true;
    }

    public virtual void draw()
    {
        if (isFirstRecord)
            return;
        glBegin(GL_LINES);
        for (int i = 0; i < posIdx; i += 2)
            Screen.drawLine(pos[i].x, pos[i].y, pos[i].z, pos[i + 1].x, pos[i + 1].y, pos[i + 1].z, _alpha);
        glEnd();
    }

    public virtual void drawWithSpectrumColor()
    {
        if (isFirstRecord)
            return;
        if (spectrumColorR + spectrumColorG + spectrumColorB < 0.1f)
            return;
        Screen.setColor(spectrumColorR, spectrumColorG, spectrumColorB);
        glBegin(GL_LINE_STRIP);
        for (int i = 0; i < posIdx; i++)
            glVertex3f(pos[i].x, pos[i].y, pos[i].z);
        glEnd();
    }

    public virtual void drawSpectrum()
    {
        if (spectrumLength <= 0 || isFirstRecord)
            return;
        if (spectrumColorR + spectrumColorG + spectrumColorB < 0.1f)
            return;
        glBegin(GL_QUADS);
        float al = 0.5f, bl = 0.5f;
        float hif = default(float), nhif = default(float);
        float hio = 5.5f;
        nhif = histIdx;
        for (int j = 0; j < 10 * spectrumLength; j++)
        {
            Screen.setColor((spectrumColorR + (1.0f - spectrumColorR) * bl) * al, (spectrumColorG + (1.0f - spectrumColorG) * bl) * al, (spectrumColorB + (1.0f - spectrumColorB) * bl) * al, al);
            hif = nhif;
            nhif = hif - hio;
            if (nhif < 0)
                nhif += HISTORY_MAX;
            int hi = GameMath.integer(hif);
            int nhi = GameMath.integer(nhif);
            if (posHist[hi][0].dist_1_Vector3(posHist[nhi][0]) < 8)
            {
                for (int i = 0; i < posIdx; i += 2)
                {
                    glVertex3f(posHist[hi][i].x, posHist[hi][i].y, posHist[hi][i].z);
                    glVertex3f(posHist[hi][i + 1].x, posHist[hi][i + 1].y, posHist[hi][i + 1].z);
                    glVertex3f(posHist[nhi][i + 1].x, posHist[nhi][i + 1].y, posHist[nhi][i + 1].z);
                    glVertex3f(posHist[nhi][i].x, posHist[nhi][i].y, posHist[nhi][i].z);
                }
            }

            al *= 0.88f * spectrumLength;
            bl *= 0.88f * spectrumLength;
        }

        glEnd();
    }

    public virtual float alpha(float v)
    {
        _alpha = v;
        return _alpha;
    }

    public virtual float alphaTrg(float v)
    {
        _alphaTrg = v;
        return _alphaTrg;
    }

    public virtual bool enableSpectrumColor(bool v)
    {
        _enableSpectrumColor = v;
        return _enableSpectrumColor;
    }
}

public interface Drawable
{
    public void draw();
}

public class EyeShape : Drawable
{
    public virtual void draw()
    {
        Screen.setColor(1.0f, 0, 0);
        glBegin(GL_LINE_LOOP);
        glVertex3f(-0.5f, 0.5f, 0);
        glVertex3f(-0.3f, 0.5f, 0);
        glVertex3f(-0.3f, 0.3f, 0);
        glVertex3f(-0.5f, 0.3f, 0);
        glEnd();
        glBegin(GL_LINE_LOOP);
        glVertex3f(0.5f, 0.5f, 0);
        glVertex3f(0.3f, 0.5f, 0);
        glVertex3f(0.3f, 0.3f, 0);
        glVertex3f(0.5f, 0.3f, 0);
        glEnd();
        Screen.setColor(0.8f, 0.4f, 0.4f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-0.5f, 0.5f, 0);
        glVertex3f(-0.3f, 0.5f, 0);
        glVertex3f(-0.3f, 0.3f, 0);
        glVertex3f(-0.5f, 0.3f, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0.5f, 0.5f, 0);
        glVertex3f(0.3f, 0.5f, 0);
        glVertex3f(0.3f, 0.3f, 0);
        glVertex3f(0.5f, 0.3f, 0);
        glEnd();
    }
}

public class CenterShape : Drawable
{
    public virtual void draw()
    {
        Screen.setColor(0.6f, 1.0f, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-0.2f, -0.2f, 0);
        glVertex3f(0.2f, -0.2f, 0);
        glVertex3f(0.2f, 0.2f, 0);
        glVertex3f(-0.2f, 0.2f, 0);
        glEnd();
        Screen.setColor(0.4f, 0.8f, 0.2f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-0.6f, 0.6f, 0);
        glVertex3f(-0.3f, 0.6f, 0);
        glVertex3f(-0.3f, 0.3f, 0);
        glVertex3f(-0.6f, 0.3f, 0);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0.6f, 0.6f, 0);
        glVertex3f(0.3f, 0.6f, 0);
        glVertex3f(0.3f, 0.3f, 0);
        glVertex3f(0.6f, 0.3f, 0);
        glEnd();
    }
}
