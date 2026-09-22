// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;

public class Gold : LuminousActor
{
    public const float SIZE = 1;
    public static int displayListIdx;
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public Vector pos;
    public int cnt;
    public const float ROLL_DEG = 1.0f;
    public override Actor newActor()
    {
        return new Gold();
    }

    public override void init(ActorInitializer ini)
    {
        GoldInitializer gi = (GoldInitializer)ini;
        ship = gi.ship;
        field = gi.field;
        rand = gi.rand;
        manager = gi.manager;
        pos = new Vector();
    }

    public void set()
    {
        for (int i = 0; i < 8; i++)
        {
            pos.x = rand.nextFloat((field.size.x - SIZE) * 2) - field.size.x + SIZE;
            pos.y = rand.nextFloat((field.size.y - SIZE) * 2) - field.size.y + SIZE;
            if (pos.dist(ship.pos) > 8)
                break;
            if (i == 7)
            {
                pos.x = 0;
                pos.y = -field.size.y / 2;
            }
        }

        cnt = 0;
        isExist = true;
    }

    public override void move()
    {
        cnt++;
        if (ship.checkHit(pos.x, pos.y - SIZE, pos.x, pos.y + SIZE) || ship.checkHit(pos.x - SIZE, pos.y, pos.x + SIZE, pos.y))
        {
            isExist = false;
            for (int i = 0; i < 16; i++)
            {
                manager.addParticle(pos, rand.nextFloat(PI * 2), 0.5f, 0, 5, 0.8f, 0);
            }

            manager.getGold();
        }
    }

    public override void draw()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(ROLL_DEG * cnt, 0, 0, 1);
        glCallList(displayListIdx);
        glCallList(displayListIdx + 1);
        glTranslatef(0, 0, -0.5f);
        glScalef(1, 1, -1);
        glCallList(displayListIdx);
        glPopMatrix();
    }

    public override void drawLuminous()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(ROLL_DEG * cnt, 0, 0, 1);
        glCallList(displayListIdx + 1);
        glPopMatrix();
    }

    public static void createDisplayLists()
    {
        displayListIdx = glGenLists(2);
        glNewList(displayListIdx, GL_COMPILE);
        drawGold(1);
        glEndList();
        glNewList(displayListIdx + 1, GL_COMPILE);
        drawGoldLine(1);
        glEndList();
    }

    public static void deleteDisplayLists()
    {
        glDeleteLists(displayListIdx, 2);
    }

    public static void drawGold(float alpha)
    {
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(1, 1, 0.3f, 0.9f * alpha);
        glVertex3f(0, 0, 1);
        A7xScreen.setColor(0.2f, 0.5f, 0.4f, 0.8f * alpha);
        glVertex3f(1, 0, 0);
        glVertex3f(0, 1, 0);
        glVertex3f(-1, 0, 0);
        glVertex3f(0, -1, 0);
        glEnd();
    }

    public static void drawGoldLine(float alpha)
    {
        glBegin(GL_LINE_STRIP);
        A7xScreen.setColor(1, 1, 0.3f, 0.9f * alpha);
        glVertex3f(1, 0, 0);
        glVertex3f(0, 1, 0);
        glVertex3f(-1, 0, 0);
        glVertex3f(0, -1, 0);
        glVertex3f(1, 0, 0);
        glEnd();
    }
}

public class GoldInitializer : ActorInitializer
{
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public GoldInitializer(Ship ship, Field field, Rand rand, A7xGameManager manager)
    {
        this.ship = ship;
        this.field = field;
        this.rand = rand;
        this.manager = manager;
    }
}
