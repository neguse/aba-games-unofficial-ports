// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public interface Shape
{
    public void draw_3(Vector3 pos, float cd, float deg);
}

public abstract class DisplayListShape : Shape
{
    public DisplayList displayList;
    public virtual void initializeShape()
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        drawList();
        displayList.endNewList();
    }

    public abstract void drawList();
    public virtual void draw_0()
    {
        drawList();
    }

    public virtual void draw_3(Vector3 pos, float cd, float deg)
    {
        glPushMatrix();
        TtnScreen.glTranslate(pos);
        glRotatef(cd * 180 / PI, 0, 1, 0);
        TtnScreen.glRotate(deg);
        displayList.call();
        glPopMatrix();
    }

    public virtual void close()
    {
        displayList.close();
    }
}

public class PyramidShape
{
    public static void draw_0()
    {
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0);
        glVertex3f(1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(-1, 1, -1);
        glVertex3f(-1, 1, 1);
        glVertex3f(1, 1, 1);
        glEnd();
        TtnScreen.setColor(0.1f, 0.1f, 0.1f, 0.5f);
        glBegin(GL_LINE_STRIP);
        glVertex3f(0, 0, 0);
        glVertex3f(1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(0, 0, 0);
        glVertex3f(-1, 1, -1);
        glVertex3f(-1, 1, 1);
        glVertex3f(0, 0, 0);
        glEnd();
        glBegin(GL_LINES);
        glVertex3f(1, 1, 1);
        glVertex3f(-1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(-1, 1, -1);
        glEnd();
    }

    public static void drawShadow(float r, float g, float b, bool noAlpha = false)
    {
        glBegin(GL_TRIANGLE_FAN);
        TtnScreen.setColor(r, g, b);
        glVertex3f(0, 0, 0);
        if (!((noAlpha)))
            TtnScreen.setColor(r * 0.75f, g * 0.75f, b * 0.75f, 0.33f);
        else
            TtnScreen.setColor(r * 0.75f, g * 0.75f, b * 0.75f, 0.75f);
        glVertex3f(1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(-1, 1, -1);
        glVertex3f(-1, 1, 1);
        glVertex3f(1, 1, 1);
        glEnd();
    }

    public static void drawPolygonShape()
    {
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0);
        glVertex3f(1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(-1, 1, -1);
        glVertex3f(-1, 1, 1);
        glVertex3f(1, 1, 1);
        glEnd();
    }

    public static void drawLineShape()
    {
        glBegin(GL_LINE_STRIP);
        glVertex3f(0, 0, 0);
        glVertex3f(1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(0, 0, 0);
        glVertex3f(-1, 1, -1);
        glVertex3f(-1, 1, 1);
        glVertex3f(0, 0, 0);
        glEnd();
        glBegin(GL_LINES);
        glVertex3f(1, 1, 1);
        glVertex3f(-1, 1, 1);
        glVertex3f(1, 1, -1);
        glVertex3f(-1, 1, -1);
        glEnd();
    }
}

public class PlayerShape : DisplayListShape
{
    public PlayerShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0, -0.6f, 0);
        glScalef(0.4f, 1.3f, 0.4f);
        PyramidShape.drawShadow(1, 0.5f, 0.5f, true);
        glPopMatrix();
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.3f, 0.9f, 0.3f);
        PyramidShape.drawShadow(1, 1, 1, true);
        glPopMatrix();
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.3f, 0.9f, 0.3f);
        PyramidShape.drawShadow(1, 1, 1, true);
        glPopMatrix();
        TtnScreen.setColor(1, 0.5f, 0.5f);
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0, -0.6f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.drawPolygonShape();
        glPopMatrix();
        TtnScreen.setColor(1, 1, 1);
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawPolygonShape();
        glPopMatrix();
        TtnScreen.setColor(1, 1, 1);
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawPolygonShape();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }
}

public class PlayerLineShape : DisplayListShape
{
    public PlayerLineShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0, -0.6f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }
}

public class ShotShape : DisplayListShape
{
    public ShotShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(0.5f, -0.5f, 0);
        glScalef(0.1f, 1.0f, 0.1f);
        TtnScreen.setColor(0.4f, 0.2f, 0.8f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(180, 0, 0, 1);
        glTranslatef(-0.5f, -0.5f, 0);
        glScalef(0.1f, 1.0f, 0.1f);
        TtnScreen.setColor(0.4f, 0.2f, 0.8f);
        PyramidShape.drawLineShape();
        glPopMatrix();
    }
}

public abstract class TractorBeamShape : DisplayListShape
{
    public virtual void drawTractorBeam(float r, float g, float b)
    {
        TtnScreen.setColor(r, g, b, 0.5f);
        glBegin(GL_QUADS);
        glVertex3f(-1, 0, -1);
        glVertex3f(1, 0, -1);
        glVertex3f(1, 0, 1);
        glVertex3f(-1, 0, 1);
        glEnd();
        TtnScreen.setColor(r, g, b);
        glBegin(GL_LINE_LOOP);
        glVertex3f(-1, 0, -1);
        glVertex3f(1, 0, -1);
        glVertex3f(1, 0, 1);
        glVertex3f(-1, 0, 1);
        glEnd();
    }

    public virtual void drawTractorBeamLine(float r, float g, float b)
    {
        TtnScreen.setColor(r, g, b);
        glBegin(GL_LINE_LOOP);
        glVertex3f(-1, 0, -1);
        glVertex3f(1, 0, -1);
        glVertex3f(1, 0, 1);
        glVertex3f(-1, 0, 1);
        glEnd();
    }
}

public class TractorBeamShapeRed : TractorBeamShape
{
    public TractorBeamShapeRed()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeam(0.5f, 0.2f, 0.2f);
    }
}

public class TractorBeamShapeBlue : TractorBeamShape
{
    public TractorBeamShapeBlue()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeam(0.2f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapePurple : TractorBeamShape
{
    public TractorBeamShapePurple()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeam(0.5f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapeDarkRed : TractorBeamShape
{
    public TractorBeamShapeDarkRed()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeamLine(0.4f, 0.1f, 0.1f);
    }
}

public class TractorBeamShapeDarkBlue : TractorBeamShape
{
    public TractorBeamShapeDarkBlue()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeamLine(0.1f, 0.1f, 0.4f);
    }
}

public class TractorBeamShapeDarkPurple : TractorBeamShape
{
    public TractorBeamShapeDarkPurple()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawTractorBeamLine(0.4f, 0.1f, 0.4f);
    }
}

public abstract class BulletShapeBase : DisplayListShape
{
    public virtual void draw_4(Vector3 pos, float cd, float deg, float rd)
    {
        glPushMatrix();
        TtnScreen.glTranslate(pos);
        glRotatef(cd * 180 / PI, 0, 1, 0);
        TtnScreen.glRotate(deg);
        glRotatef(rd, 0, 1, 0);
        displayList.call();
        glPopMatrix();
    }
}

public class BulletShape : BulletShapeBase
{
    public BulletShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        TtnScreen.setColor(0, 0, 0);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glScalef(1.2f, 1.2f, 1.2f);
        TtnScreen.setColor(0.1f, 0.3f, 0.3f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
    }
}

public class BulletLineShape : BulletShapeBase
{
    public BulletLineShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(1.2f, 1.2f, 1.2f);
        glBegin(GL_LINES);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
        glBegin(GL_LINE_LOOP);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
    }
}

public class MiddleBulletShape : BulletShapeBase
{
    public MiddleBulletShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glScalef(1.1f, 1.0f, 1.1f);
        TtnScreen.setColor(0, 0, 0);
        glBegin(GL_QUADS);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glEnd();
        glBegin(GL_TRIANGLES);
        glVertex3f(-0.17f, -0.3f, -0.1f);
        glVertex3f(0.17f, -0.3f, -0.1f);
        glVertex3f(0, -0.3f, 0.2f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glScalef(1.4f, 1.3f, 1.4f);
        TtnScreen.setColor(0.1f, 0.2f, 0.3f);
        glBegin(GL_QUADS);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glEnd();
        glBegin(GL_TRIANGLES);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
    }
}

public class MiddleBulletLineShape : BulletShapeBase
{
    public MiddleBulletLineShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(1.4f, 1.3f, 1.4f);
        glBegin(GL_LINES);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, 0.3f, 0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
        glBegin(GL_LINE_LOOP);
        glVertex3f(-0.17f, 0.3f, -0.1f);
        glVertex3f(0.17f, 0.3f, -0.1f);
        glVertex3f(0, 0.3f, 0.2f);
        glEnd();
        glBegin(GL_LINE_LOOP);
        glVertex3f(-0.34f, -0.3f, -0.2f);
        glVertex3f(0.34f, -0.3f, -0.2f);
        glVertex3f(0, -0.3f, 0.4f);
        glEnd();
    }
}

public abstract class RollBulletShapeBase : BulletShapeBase
{
    public override void draw_4(Vector3 pos, float cd, float deg, float rd)
    {
        glPushMatrix();
        TtnScreen.glTranslate(pos);
        glRotatef(cd * 180 / PI, 0, 1, 0);
        glRotatef(rd, 0, 0, 1);
        displayList.call();
        glPopMatrix();
    }
}

public class CounterBulletShape : RollBulletShapeBase
{
    public CounterBulletShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        TtnScreen.setColor(0, 0, 0);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(0.5f, 0, 0);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.5f, 0, 0);
        glVertex3f(0, -0.5f, 0);
        glVertex3f(0.5f, 0, 0);
        glEnd();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glScalef(1.2f, 1.2f, 1.2f);
        TtnScreen.setColor(0.5f, 0.5f, 0.5f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(0.5f, 0, 0);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.5f, 0, 0);
        glVertex3f(0, -0.5f, 0);
        glVertex3f(0.5f, 0, 0);
        glEnd();
    }
}

public class CounterBulletLineShape : RollBulletShapeBase
{
    public CounterBulletLineShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(1.2f, 1.2f, 1.2f);
        glBegin(GL_LINE_LOOP);
        glVertex3f(0.5f, 0, 0);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(-0.5f, 0, 0);
        glVertex3f(0, -0.5f, 0);
        glEnd();
        glBegin(GL_LINES);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(0.5f, 0, 0);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(-0.5f, 0, 0);
        glVertex3f(0, 0, 0.5f);
        glVertex3f(0, -0.5f, 0);
        glEnd();
    }
}

public abstract class EnemyShape : DisplayListShape
{
    public virtual void draw_5(Vector3 pos, float cd, float deg, float cnt, Vector size)
    {
        draw_6(pos, cd, deg, cnt, size.x, size.y);
    }

    public virtual void draw_6(Vector3 pos, float cd, float deg, float cnt, float sx, float sy)
    {
        glPushMatrix();
        TtnScreen.glTranslate(pos);
        glRotatef(cd * 180 / PI, 0, 1, 0);
        TtnScreen.glRotate(deg);
        glScalef(sx, sy, 1);
        glRotatef(cnt * 3.0f, 0, 1, 0);
        displayList.call();
        glPopMatrix();
    }
}

public class Enemy1Shape : EnemyShape
{
    public Enemy1Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glPushMatrix();
        glTranslatef(0, -0.6f, 0);
        glScalef(0.5f, 1.4f, 0.5f);
        PyramidShape.drawShadow(0.5f, 0.5f, 0.3f);
        glPopMatrix();
        glPushMatrix();
        glRotatef(120, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.4f, 1.0f, 0.4f);
        PyramidShape.drawShadow(0.2f, 0.2f, 0.5f);
        glPopMatrix();
        TtnScreen.setColor(0.2f, 0.2f, 0.5f);
        glPushMatrix();
        glRotatef(240, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.4f, 1.0f, 0.4f);
        PyramidShape.drawShadow(0.2f, 0.2f, 0.5f);
        glPopMatrix();
        TtnScreen.setColor(1, 1, 0.6f);
        glPushMatrix();
        glTranslatef(0, -0.6f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.5f, 0.5f, 1);
        glPushMatrix();
        glRotatef(120, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.5f, 0.5f, 1);
        glPushMatrix();
        glRotatef(240, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }
}

public class Enemy1TrailShape : EnemyShape
{
    public Enemy1TrailShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glPushMatrix();
        glTranslatef(0, -0.6f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(120, 0, 0, 1);
        glTranslatef(0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(240, 0, 0, 1);
        glTranslatef(-0.5f, -0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
    }
}

public class Enemy2Shape : EnemyShape
{
    public Enemy2Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glPushMatrix();
        glTranslatef(0, -0.5f, 0);
        glScalef(0.5f, 1.2f, 0.5f);
        PyramidShape.drawShadow(0.5f, 0.4f, 0.5f);
        glPopMatrix();
        glPushMatrix();
        glRotatef(60, 0, 0, 1);
        glTranslatef(0.6f, -0.7f, 0);
        glScalef(0.4f, 1.4f, 0.4f);
        PyramidShape.drawShadow(0.9f, 0.6f, 0.5f);
        glPopMatrix();
        glPushMatrix();
        glRotatef(300, 0, 0, 1);
        glTranslatef(-0.6f, -0.7f, 0);
        glScalef(0.4f, 1.4f, 0.4f);
        PyramidShape.drawShadow(0.9f, 0.6f, 0.5f);
        glPopMatrix();
        TtnScreen.setColor(1, 0.9f, 1.0f);
        glPushMatrix();
        glTranslatef(0, -0.5f, 0);
        glScalef(0.3f, 1.0f, 0.3f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.9f, 0.6f, 0.5f);
        glPushMatrix();
        glRotatef(60, 0, 0, 1);
        glTranslatef(0.6f, -0.7f, 0);
        glScalef(0.2f, 1.2f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.9f, 0.6f, 0.5f);
        glPushMatrix();
        glRotatef(300, 0, 0, 1);
        glTranslatef(-0.6f, -0.7f, 0);
        glScalef(0.2f, 1.2f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }
}

public class Enemy2TrailShape : EnemyShape
{
    public Enemy2TrailShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glPushMatrix();
        glTranslatef(0, -0.5f, 0);
        glScalef(0.3f, 1.0f, 0.3f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(60, 0, 0, 1);
        glTranslatef(0.6f, -0.7f, 0);
        glScalef(0.2f, 1.2f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(300, 0, 0, 1);
        glTranslatef(-0.6f, -0.7f, 0);
        glScalef(0.2f, 1.2f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
    }
}

public class Enemy3Shape : EnemyShape
{
    public Enemy3Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glPushMatrix();
        glTranslatef(0, -0.4f, 0);
        glScalef(0.5f, 1.4f, 0.5f);
        PyramidShape.drawShadow(0.5f, 0.5f, 0.3f);
        glPopMatrix();
        glPushMatrix();
        glRotatef(150, 0, 0, 1);
        glTranslatef(0.5f, 0.2f, 0);
        glScalef(0.4f, 1.0f, 0.4f);
        PyramidShape.drawShadow(0.2f, 0.2f, 0.5f);
        glPopMatrix();
        TtnScreen.setColor(0.2f, 0.2f, 0.5f);
        glPushMatrix();
        glRotatef(210, 0, 0, 1);
        glTranslatef(-0.5f, 0.2f, 0);
        glScalef(0.4f, 1.0f, 0.4f);
        PyramidShape.drawShadow(0.2f, 0.2f, 0.5f);
        glPopMatrix();
        TtnScreen.setColor(1, 0.6f, 0.9f);
        glPushMatrix();
        glTranslatef(0, -0.4f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.3f, 0.5f, 1);
        glPushMatrix();
        glRotatef(150, 0, 0, 1);
        glTranslatef(0.5f, 0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        TtnScreen.setColor(0.3f, 0.5f, 1);
        glPushMatrix();
        glRotatef(210, 0, 0, 1);
        glTranslatef(-0.5f, 0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.draw_0();
        glPopMatrix();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }
}

public class Enemy3TrailShape : EnemyShape
{
    public Enemy3TrailShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glPushMatrix();
        glTranslatef(0, -0.4f, 0);
        glScalef(0.3f, 1.2f, 0.3f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(150, 0, 0, 1);
        glTranslatef(0.5f, 0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
        glPushMatrix();
        glRotatef(210, 0, 0, 1);
        glTranslatef(-0.5f, 0.2f, 0);
        glScalef(0.2f, 0.8f, 0.2f);
        PyramidShape.drawLineShape();
        glPopMatrix();
    }
}

public class TriangleParticleShape : DisplayListShape
{
    public TriangleParticleShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glBegin(GL_LINE_LOOP);
        glVertex3f(0, 0.5f, 0);
        glVertex3f(0.4f, -0.3f, 0);
        glVertex3f(-0.4f, -0.3f, 0);
        glEnd();
    }
}

public abstract class PillarShape : DisplayListShape
{
    public const float TICKNESS = 4.0f;
    public const float RADIUS_RATIO = 0.3f;
    public virtual void drawPillar(float r, float g, float b, bool outside = false)
    {
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        glBegin(GL_QUADS);
        TtnScreen.setColor(r, g, b);
        for (int i = 0; i < 8; i++)
        {
            float d = PI * 2 * i / 8;
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            d = d + (PI * 2 / 8);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            d = d - (PI * 2 / 8);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
        }

        glEnd();
        if (!((outside)))
        {
            TtnScreen.setColor(r, g, b);
            glBegin(GL_TRIANGLES);
            for (int i = 0; i < 8; i++)
            {
                float d = PI * 2 * i / 8;
                glVertex3f(0, TICKNESS, 0);
                glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
                d = d + (PI * 2 / 8);
                glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
                d = d - (PI * 2 / 8);
                glVertex3f(0, -TICKNESS, 0);
                glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
                d = d + (PI * 2 / 8);
                glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            }

            glEnd();
        }

        TtnScreen.setColor(0.1f, 0.1f, 0.1f);
        for (int i = 0; i < 8; i++)
        {
            float d = PI * 2 * i / 8;
            glBegin(GL_LINE_STRIP);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            d = d + (PI * 2 / 8);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            d = d - (PI * 2 / 8);
            glVertex3f(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO);
            glEnd();
        }

        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
    }

    public virtual void draw_2(float y, float deg)
    {
        glPushMatrix();
        glTranslatef(0, y, 0);
        glRotatef(deg * 180 / PI, 0, 1, 0);
        displayList.call();
        glPopMatrix();
    }
}

public class Pillar1Shape : PillarShape
{
    public Pillar1Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(0.6f, 1.0f, 0.6f);
        drawPillar(0.5f, 0.4f, 0.4f);
    }
}

public class Pillar2Shape : PillarShape
{
    public Pillar2Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(0.8f, 1.0f, 0.8f);
        drawPillar(0.6f, 0.3f, 0.3f);
    }
}

public class Pillar3Shape : PillarShape
{
    public Pillar3Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        drawPillar(0.5f, 0.5f, 0.4f);
    }
}

public class Pillar4Shape : PillarShape
{
    public Pillar4Shape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(1.1f, 1.0f, 1.1f);
        drawPillar(0.5f, 0.4f, 0.5f);
    }
}

public class OutsidePillarShape : PillarShape
{
    public OutsidePillarShape()
    {
        initializeShape();
    }

    public override void drawList()
    {
        glScalef(7.0f, 3.0f, 7.0f);
        drawPillar(0.2f, 0.2f, 0.3f, true);
    }
}
