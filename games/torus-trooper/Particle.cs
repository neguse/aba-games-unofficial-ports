// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class Particle : LuminousActor
{
    public const float GRAVITY = 0.02f;
    public const float SIZE = 0.3f;
    public static Rand rand = new Rand();
    public Tunnel tunnel;
    public Ship ship;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 sp, psp;
    public Vector3 rsp, rpsp;
    public Vector icp;
    public float r, g, b;
    public float lumAlp;
    public int cnt;
    public bool inCourse;
    public int type;
    public float d1, d2, md1, md2;
    public float width, height;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1(List<object> args)
    {
        tunnel = (Tunnel)args[0];
        ship = (Ship)args[1];
        pos = new Vector3();
        vel = new Vector3();
        sp = new Vector3();
        psp = new Vector3();
        rsp = new Vector3();
        rpsp = new Vector3();
        icp = new Vector();
    }

    public void set_12(Vector p, float z, float d, float mz, float speed, float r, float g, float b, int c = 16, int t = ParticlePType.SPARK, float w = 0, float h = 0)
    {
        pos.x = p.x;
        pos.y = p.y;
        pos.z = z;
        float sb = rand.nextFloat(0.8f) + 0.4f;
        vel.x = sin(d) * speed * sb;
        vel.y = cos(d) * speed * sb;
        vel.z = mz;
        this.r = r;
        this.g = g;
        this.b = b;
        cnt = c + rand.nextInt(GameMath.integer(c / 2));
        type = t;
        lumAlp = 0.8f + rand.nextFloat(0.2f);
        if (type == ParticlePType.STAR)
            inCourse = false;
        else
            inCourse = true;
        if (type == ParticlePType.FRAGMENT)
        {
            {
                d2 = 0;
                d1 = d2;
            }

            md1 = rand.nextSignedFloat(12);
            md2 = rand.nextSignedFloat(12);
            width = w;
            height = h;
        }

        checkInCourse();
        calcScreenPos();
        exists = true;
    }

    public override void move()
    {
        cnt--;
        if ((cnt < 0) || (pos.y < -2))
        {
            exists = false;
            return;
        }

        psp.x = sp.x;
        psp.y = sp.y;
        psp.z = sp.z;
        if (inCourse)
        {
            rpsp.x = rsp.x;
            rpsp.y = rsp.y;
            rpsp.z = rsp.z;
        }

        pos.opAddAssign(vel);
        if (type == ParticlePType.FRAGMENT)
            pos.y = pos.y - (ship.speed / 2);
        else if (type == ParticlePType.SPARK)
            pos.y = pos.y - (ship.speed * 0.33f);
        else
            pos.y = pos.y - (ship.speed);
        if (type != ParticlePType.STAR)
        {
            if (type == ParticlePType.FRAGMENT)
                vel.z = vel.z - (GRAVITY / 2);
            else
                vel.z = vel.z - (GRAVITY);
            if ((inCourse) && (pos.z < 0))
            {
                if (type == ParticlePType.FRAGMENT)
                    vel.z = vel.z * (-0.6f);
                else
                    vel.z = vel.z * (-0.8f);
                vel.opMulAssign(0.9f);
                pos.z = pos.z + (vel.z * 2);
                checkInCourse();
            }
        }

        if (type == ParticlePType.FRAGMENT)
        {
            d1 = d1 + (md1);
            d2 = d2 + (md2);
            md1 = md1 * (0.98f);
            md2 = md2 * (0.98f);
            width = width * (0.98f);
            height = height * (0.98f);
        }

        lumAlp = lumAlp * (0.98f);
        calcScreenPos();
    }

    public void calcScreenPos()
    {
        Vector3 p = tunnel.getPos_1_Vector3(pos);
        sp.x = p.x;
        sp.y = p.y;
        sp.z = p.z;
        if (inCourse)
        {
            pos.z = -pos.z;
            p = tunnel.getPos_1_Vector3(pos);
            rsp.x = p.x;
            rsp.y = p.y;
            rsp.z = p.z;
            pos.z = -pos.z;
        }
    }

    public void checkInCourse()
    {
        icp.x = pos.x;
        icp.y = pos.y;
        if (tunnel.checkInCourse(icp) != 0)
            inCourse = false;
    }

    public override void draw()
    {
        switch (type)
        {
            case ParticlePType.SPARK:
            case ParticlePType.JET:
                drawSpark();
                break;
            case ParticlePType.STAR:
                drawStar();
                break;
            case ParticlePType.FRAGMENT:
                drawFragment();
                break;
        }
    }

    public void drawSpark()
    {
        glBegin(GL_TRIANGLE_FAN);
        TtScreen.setColor(r, g, b, 0.5f);
        TtScreen.glVertex(psp);
        TtScreen.setColor(r, g, b, 0);
        glVertex3f(sp.x - SIZE, sp.y - SIZE, sp.z);
        glVertex3f(sp.x + SIZE, sp.y - SIZE, sp.z);
        glVertex3f(sp.x + SIZE, sp.y + SIZE, sp.z);
        glVertex3f(sp.x - SIZE, sp.y + SIZE, sp.z);
        glVertex3f(sp.x - SIZE, sp.y - SIZE, sp.z);
        glEnd();
        if (inCourse)
        {
            glBegin(GL_TRIANGLE_FAN);
            TtScreen.setColor(r, g, b, 0.2f);
            TtScreen.glVertex(rpsp);
            TtScreen.setColor(r, g, b, 0);
            glVertex3f(rsp.x - SIZE, rsp.y - SIZE, sp.z);
            glVertex3f(rsp.x + SIZE, rsp.y - SIZE, sp.z);
            glVertex3f(rsp.x + SIZE, rsp.y + SIZE, sp.z);
            glVertex3f(rsp.x - SIZE, rsp.y + SIZE, sp.z);
            glVertex3f(rsp.x - SIZE, rsp.y - SIZE, sp.z);
            glEnd();
        }
    }

    public void drawStar()
    {
        glBegin(GL_LINES);
        TtScreen.setColor(r, g, b, 1);
        TtScreen.glVertex(psp);
        TtScreen.setColor(r, g, b, 0.2f);
        TtScreen.glVertex(sp);
        glEnd();
    }

    public void drawFragment()
    {
        glPushMatrix();
        glTranslatef(sp.x, sp.y, sp.z);
        glRotatef(d1, 0, 0, 1);
        glRotatef(d2, 0, 1, 0);
        glBegin(GL_LINE_LOOP);
        TtScreen.setColor(r, g, b, 0.5f);
        glVertex3f(width, 0, height);
        glVertex3f(-width, 0, height);
        glVertex3f(-width, 0, -height);
        glVertex3f(width, 0, -height);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        TtScreen.setColor(r, g, b, 0.2f);
        glVertex3f(width, 0, height);
        glVertex3f(-width, 0, height);
        glVertex3f(-width, 0, -height);
        glVertex3f(width, 0, -height);
        glEnd();
        glPopMatrix();
    }

    public override void drawLuminous()
    {
        if ((lumAlp < 0.2f) || (type != ParticlePType.SPARK))
            return;
        glBegin(GL_TRIANGLE_FAN);
        TtScreen.setColor(r, g, b, lumAlp * 0.6f);
        TtScreen.glVertex(psp);
        TtScreen.setColor(r, g, b, 0);
        glVertex3f(sp.x - SIZE, sp.y - SIZE, sp.z);
        glVertex3f(sp.x + SIZE, sp.y - SIZE, sp.z);
        glVertex3f(sp.x + SIZE, sp.y + SIZE, sp.z);
        glVertex3f(sp.x - SIZE, sp.y + SIZE, sp.z);
        glVertex3f(sp.x - SIZE, sp.y - SIZE, sp.z);
        glEnd();
    }
}

public class ParticlePool : LuminousActorPool<Particle>
{
    public ParticlePool(int n, List<object> args) : base(n, args, () => new Particle())
    {
    }
}

public static class ParticlePType
{
    public const int SPARK = 0, STAR = 1, FRAGMENT = 2, JET = 3;
}
