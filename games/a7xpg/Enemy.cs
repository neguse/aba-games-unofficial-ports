// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Drawing;

public class Enemy : LuminousActor
{
    public static int displayListIdx;
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public Vector pos, ppos;
    public int type;
    public float size, speed;
    public float deg;
    public int cnt;
    public float turnDist;
    public bool hitWall;
    public int hitWallType;
    public int chaseType;
    public Vector vel, blowedVel;
    public float armDeg, armDegMv;
    public const int POSITION_HISTORY_LENGTH = 180;
    public Vector[] posHst;
    public float[] degHst;
    public int posHstIdx;
    public const int APPEAR_CNT = 60;
    public const int DESTROYED_CNT = 120;
    public override Actor newActor()
    {
        return new Enemy();
    }

    public override void init(ActorInitializer ini)
    {
        EnemyInitializer ei = (EnemyInitializer)ini;
        ship = ei.ship;
        field = ei.field;
        rand = ei.rand;
        manager = ei.manager;
        pos = new Vector();
        ppos = new Vector();
        vel = new Vector();
        blowedVel = new Vector();
        posHst = new Vector[POSITION_HISTORY_LENGTH];
        degHst = new float[POSITION_HISTORY_LENGTH];
        for (int i = 0; i < POSITION_HISTORY_LENGTH; i++)
        {
            posHst[i] = new Vector();
        }
    }

    public void set(int type, float size, float speed)
    {
        this.type = type;
        this.size = size;
        this.speed = speed;
        reset();
    }

    public void reset()
    {
        for (int i = 0; i < 8; i++)
        {
            pos.x = rand.nextFloat((field.size.x - size) * 2) - field.size.x + size;
            pos.y = rand.nextFloat((field.size.y - size) * 2) - field.size.y + size;
            if (pos.dist(ship.pos) > 8)
                break;
            if (i == 7)
            {
                pos.x = 0;
                pos.y = 0;
            }
        }

        ppos.x = pos.x;
        ppos.y = pos.y;
        {
            blowedVel.y = 0;
            blowedVel.x = blowedVel.y;
        }

        cnt = 0;
        turnDist = 0;
        isExist = true;
        switch (type)
        {
            case 0:
                deg = PI / 2 * rand.nextInt(4);
                break;
            case 1:
                deg = PI / 4 * rand.nextInt(8);
                break;
            case 2:
                chaseType = 0;
                deg = PI / 2 * rand.nextInt(4);
                break;
            case 3:
            {
                vel.y = 0;
                vel.x = vel.y;
            }

                deg = rand.nextFloat(PI * 2);
                break;
            case 4:
                deg = rand.nextFloat(PI * 2);
                vel.x = sin(deg) * speed;
                vel.y = cos(deg) * speed;
                armDeg = rand.nextFloat(PI * 2);
                if (rand.nextInt(2) == 0)
                    armDegMv = rand.nextFloat(0.01f) + 0.02f;
                else
                    armDegMv = -rand.nextFloat(0.01f) - 0.02f;
                break;
            case 5:
                posHstIdx = POSITION_HISTORY_LENGTH;
                deg = PI / 2 * rand.nextInt(4);
                for (int i = 0; i < POSITION_HISTORY_LENGTH; i++)
                {
                    posHst[i].x = pos.x;
                    posHst[i].y = pos.y;
                    degHst[i] = deg;
                }

                break;
            default:
                break;
        }
    }

    public float[][] enemyColor = new float[][]
    {
        new float[] { 0.9f, 0.2f, 0.2f },
        new float[] { 0.7f, 0.3f, 0.6f },
        new float[] { 0.6f, 0.7f, 0.2f },
        new float[] { 0.8f, 0.2f, 0.4f },
        new float[] { 0.5f, 0.7f, 0.3f },
        new float[] { 0.6f, 0.3f, 0.8f },
    };
    public void hitShip()
    {
        if (ship.invincible)
        {
            manager.playSe(6);
            for (int i = 0; i < 60; i++)
            {
                manager.addParticle(pos, rand.nextFloat(PI * 2), size, rand.nextFloat(0.5f), enemyColor[type][0], enemyColor[type][1], enemyColor[type][2]);
            }

            ship.destroyEnemy();
            reset();
            cnt = -DESTROYED_CNT;
        }
        else if (!ship.restart)
        {
            for (int i = 0; i < 100; i++)
            {
                manager.addParticle(pos, rand.nextFloat(PI * 2), size, rand.nextFloat(1), 0.3f, 1, 0.2f);
            }

            ship.miss();
            manager.restartStage();
        }
    }

    public void moveType0()
    {
        turnDist = turnDist - (speed);
        if (hitWall)
        {
            turnDist = (rand.nextInt(60) + 60) * 0.2f;
            if (deg < PI / 4 * 1 || (deg > PI / 4 * 3 && deg < PI / 4 * 5))
            {
                if (ship.pos.x < pos.x)
                    deg = PI / 4 * 6;
                else
                    deg = PI / 4 * 2;
            }
            else
            {
                if (ship.pos.y < pos.y)
                    deg = PI / 4 * 4;
                else
                    deg = PI / 4 * 0;
            }
        }

        if (cnt < APPEAR_CNT)
            return;
        if (turnDist <= 0)
        {
            turnDist = (rand.nextInt(90) + 60) * 0.2f;
            float od = atan2(ship.pos.x - pos.x, ship.pos.y - pos.y);
            if (od < -PI / 4 * 3)
                deg = PI / 4 * 4;
            else if (od < -PI / 4 * 1)
                deg = PI / 4 * 6;
            else if (od < PI / 4 * 1)
                deg = PI / 4 * 0;
            else
                deg = PI / 4 * 2;
        }

        if (rand.nextInt(9) == 0)
        {
            manager.addParticle(pos, deg + PI + PI / 7 + rand.nextFloat(0.2f) - 0.1f, size, speed * 2, 0.9f, 0.3f, 0.3f);
        }

        if (rand.nextInt(9) == 0)
        {
            manager.addParticle(pos, deg + PI - PI / 7 + rand.nextFloat(0.2f) - 0.1f, size, speed * 2, 0.9f, 0.3f, 0.3f);
        }

        if (ship.checkHit(pos.x, pos.y - size * 0.8f, pos.x, pos.y + size * 0.8f) || ship.checkHit(pos.x - size * 0.8f, pos.y, pos.x + size * 0.8f, pos.y))
        {
            hitShip();
        }
    }

    public void moveType1()
    {
        turnDist = turnDist - (speed);
        if (hitWall)
        {
            deg = deg + (PI);
            if (deg >= PI * 2)
                deg = deg - (PI * 2);
        }

        if (cnt < APPEAR_CNT)
            return;
        if (!hitWall && turnDist <= 0)
        {
            turnDist = (rand.nextInt(40) + 8) * 0.2f;
            float od = atan2(ship.pos.x - pos.x, ship.pos.y - pos.y);
            if (od < 0)
                od = od + (PI * 2);
            od = od - (deg);
            if (od > -PI / 8 && od < PI / 8)
            {
            }
            else if (od < -PI / 8 * 15 || od > PI / 8 * 15)
            {
            }
            else if ((od > -PI && od < 0) || od > PI)
            {
                deg = deg - (PI / 4);
                if (deg < 0)
                    deg = deg + (PI * 2);
            }
            else
            {
                deg = deg + (PI / 4);
                if (deg >= PI * 2)
                    deg = deg - (PI * 2);
            }
        }

        if (rand.nextInt(4) == 0)
        {
            manager.addParticle(pos, deg + PI + rand.nextFloat(0.2f) - 0.1f, size, speed * 2.5f, 0.8f, 0.4f, 0.5f);
        }

        if (ship.checkHit(pos.x, pos.y - size * 0.8f, pos.x, pos.y + size * 0.8f) || ship.checkHit(pos.x - size * 0.8f, pos.y, pos.x + size * 0.8f, pos.y))
        {
            hitShip();
        }
    }

    public void moveType2()
    {
        if (hitWall)
        {
            if ((hitWallType & 1) == 1)
            {
                float od = atan2(ship.pos.x - pos.x, ship.pos.y - pos.y);
                if (od > -PI / 2 && od <= PI / 2)
                {
                    if (chaseType > 0)
                        deg = 0;
                    else
                        deg = PI / 2 * 2;
                    chaseType++;
                }
                else
                {
                    if (chaseType > 0)
                        deg = PI / 2 * 2;
                    else
                        deg = 0;
                    chaseType++;
                }
            }

            if ((hitWallType & 2) == 2)
            {
                float od = atan2(ship.pos.x - pos.x, ship.pos.y - pos.y);
                if (od < 0)
                {
                    if (chaseType > 0)
                        deg = PI / 2 * 3;
                    else
                        deg = PI / 2;
                    chaseType++;
                }
                else
                {
                    if (chaseType > 0)
                        deg = PI / 2;
                    else
                        deg = PI / 2 * 3;
                    chaseType++;
                }
            }
        }
        else if (chaseType > 1)
        {
            if (deg < 0.1f)
            {
                if (ship.pos.y <= pos.y)
                {
                    if (ship.pos.x < pos.x)
                        deg = PI / 2 * 3;
                    else
                        deg = PI / 2;
                    chaseType = 0;
                }
            }
            else if (deg > PI / 2 - 0.1f && deg < PI / 2 + 0.1f)
            {
                if (ship.pos.x <= pos.x)
                {
                    if (ship.pos.y < pos.y)
                        deg = PI / 2 * 2;
                    else
                        deg = 0;
                    chaseType = 0;
                }
            }
            else if (deg > PI / 2 * 2 - 0.1f && deg < PI / 2 * 2 + 0.1f)
            {
                if (ship.pos.y >= pos.y)
                {
                    if (ship.pos.x < pos.x)
                        deg = PI / 2 * 3;
                    else
                        deg = PI / 2;
                    chaseType = 0;
                }
            }
            else if (deg > PI / 2 * 3 - 0.1f && deg < PI / 2 * 3 + 0.1f)
            {
                if (ship.pos.x >= pos.x)
                {
                    if (ship.pos.y < pos.y)
                        deg = PI / 2 * 2;
                    else
                        deg = 0;
                    chaseType = 0;
                }
            }
        }

        if (cnt < APPEAR_CNT)
            return;
        if (rand.nextInt(9) == 0)
        {
            manager.addParticle(pos, deg + PI + PI / 12 + rand.nextFloat(0.2f) - 0.1f, size, speed * 2, 0.3f, 0.9f, 0.3f);
        }

        if (rand.nextInt(9) == 0)
        {
            manager.addParticle(pos, deg + PI - PI / 12 + rand.nextFloat(0.2f) - 0.1f, size, speed * 2, 0.3f, 0.9f, 0.3f);
        }

        if (ship.checkHit(pos.x, pos.y - size * 0.8f, pos.x, pos.y + size * 0.8f) || ship.checkHit(pos.x - size * 0.8f, pos.y, pos.x + size * 0.8f, pos.y))
        {
            hitShip();
        }
    }

    public void moveType3()
    {
        if (hitWall)
        {
            if ((hitWallType & 1) == 1)
            {
                vel.x = vel.x * (-0.8f);
            }

            if ((hitWallType & 2) == 2)
            {
                vel.y = vel.y * (-0.8f);
            }
        }
        else
        {
            if (ship.pos.x < pos.x)
            {
                vel.x = vel.x - (0.01f);
            }
            else
            {
                vel.x = vel.x + (0.01f);
            }

            if (ship.pos.y < pos.y)
            {
                vel.y = vel.y - (0.01f);
            }
            else
            {
                vel.y = vel.y + (0.01f);
            }
        }

        vel.mul(0.99f);
        deg = deg + (0.1f);
        if (cnt < APPEAR_CNT)
            return;
        if (rand.nextInt(4) == 0)
        {
            manager.addParticle(pos, rand.nextFloat(PI * 2), size, speed, 0.9f, 0.3f, 0.6f);
        }

        if (ship.checkHit(pos.x, pos.y - size * 0.8f, pos.x, pos.y + size * 0.8f) || ship.checkHit(pos.x - size * 0.8f, pos.y, pos.x + size * 0.8f, pos.y))
        {
            hitShip();
        }
    }

    public void moveType4()
    {
        float width = size * sin(armDeg);
        float height = size * cos(armDeg);
        if (width < 0)
            width = width * (-1);
        if (height < 0)
            height = height * (-1);
        if (pos.x < -field.size.x + width && vel.x < 0)
        {
            vel.x = vel.x * (-1);
            pos.x = ppos.x;
            pos.y = ppos.y;
        }
        else if (pos.x > field.size.x - width && vel.x > 0)
        {
            vel.x = vel.x * (-1);
            pos.x = ppos.x;
            pos.y = ppos.y;
        }

        if (pos.y < -field.size.y + height && vel.y < 0)
        {
            vel.y = vel.y * (-1);
            pos.x = ppos.x;
            pos.y = ppos.y;
        }
        else if (pos.y > field.size.y - height && vel.y > 0)
        {
            vel.y = vel.y * (-1);
            pos.x = ppos.x;
            pos.y = ppos.y;
        }

        armDeg = armDeg + (armDegMv);
        if (cnt < APPEAR_CNT)
            return;
        if (rand.nextInt(7) == 0)
        {
            manager.addParticle(pos, armDeg + rand.nextFloat(0.2f) - 0.1f, 1, speed * 2, 0.5f, 0.9f, 0.3f);
        }

        if (rand.nextInt(7) == 0)
        {
            manager.addParticle(pos, armDeg + PI + rand.nextFloat(0.2f) - 0.1f, 1, speed * 2, 0.5f, 0.9f, 0.3f);
        }

        float ax = size * sin(armDeg) * 0.9f;
        float ay = size * cos(armDeg) * 0.9f;
        if (ship.checkHit(pos.x - ax, pos.y - ay, pos.x + ax, pos.y + ay))
        {
            hitShip();
        }
    }

    public void moveType5()
    {
        turnDist = turnDist - (speed);
        if (hitWall)
        {
            deg = deg + (PI);
            if (deg >= PI * 2)
                deg = deg - (PI * 2);
        }

        if (cnt < APPEAR_CNT)
            return;
        if (!hitWall && turnDist <= 0)
        {
            turnDist = (rand.nextInt(24) + 16) * 0.2f;
            float od = atan2(ship.pos.x - pos.x, ship.pos.y - pos.y);
            if (od < 0)
                od = od + (PI * 2);
            od = od - (deg);
            if (od > -PI / 8 && od < PI / 8)
            {
            }
            else if (od < -PI / 8 * 15 || od > PI / 8 * 15)
            {
            }
            else if ((od > -PI && od < 0) || od > PI)
            {
                deg = deg - (PI / 2);
                if (deg < 0)
                    deg = deg + (PI * 2);
            }
            else
            {
                deg = deg + (PI / 2);
                if (deg >= PI * 2)
                    deg = deg - (PI * 2);
            }
        }

        if (rand.nextInt(4) == 0)
        {
            manager.addParticle(pos, deg + PI + rand.nextFloat(0.2f) - 0.1f, 0, speed * 5, 0.5f, 0.3f, 0.9f);
        }

        int hi = posHstIdx;
        for (int i = 0; i < 5; i++)
        {
            float cx = posHst[hi].x;
            float cy = posHst[hi].y;
            float cd = degHst[hi];
            float ax = size * sin(cd);
            float ay = size * cos(cd);
            if (ship.checkHit(cx - ax, cy - ay, cx + ax, cy + ay))
            {
                hitShip();
            }

            hi = GameMath.integer(hi + (size * 2 / speed));
            if (hi >= POSITION_HISTORY_LENGTH)
                hi = hi - (POSITION_HISTORY_LENGTH);
        }
    }

    public override void move()
    {
        cnt++;
        if (cnt < 0)
            return;
        ppos.x = pos.x;
        ppos.y = pos.y;
        switch (type)
        {
            default:
                pos.x = pos.x + (sin(deg) * speed);
                pos.y = pos.y + (cos(deg) * speed);
                break;
            case 3:
            case 4:
                pos.x = pos.x + (vel.x * speed);
                pos.y = pos.y + (vel.y * speed);
                break;
        }

        if (type < 4)
        {
            ship.addBlowedForce(pos, blowedVel, size);
            pos.x = pos.x + (blowedVel.x);
            pos.y = pos.y + (blowedVel.y);
            blowedVel.mul(0.94f);
        }

        if (type == 5)
        {
            posHstIdx--;
            if (posHstIdx < 0)
                posHstIdx = POSITION_HISTORY_LENGTH - 1;
            posHst[posHstIdx].x = pos.x;
            posHst[posHstIdx].y = pos.y;
            degHst[posHstIdx] = deg;
        }

        hitWallType = 0;
        if (pos.x < -field.size.x + size || pos.x > field.size.x - size)
        {
            hitWall = true;
            hitWallType = hitWallType | (1);
        }

        if (pos.y < -field.size.y + size || pos.y > field.size.y - size)
        {
            hitWall = true;
            hitWallType = hitWallType | (2);
        }

        if (hitWall)
        {
            if (type < 4)
            {
                blowedVel.mul(-0.7f);
            }

            if (type != 4)
            {
                pos.x = ppos.x;
                pos.y = ppos.y;
            }
        }

        switch (type)
        {
            case 0:
                moveType0();
                break;
            case 1:
                moveType1();
                break;
            case 2:
                moveType2();
                break;
            case 3:
                moveType3();
                break;
            case 4:
                moveType4();
                break;
            case 5:
                moveType5();
                break;
            default:
                break;
        }

        hitWall = false;
    }

    public void drawType0()
    {
        float sz = 0;
        if (cnt < 0)
            return;
        else if (cnt < APPEAR_CNT)
            sz = size * cnt / APPEAR_CNT;
        else
            sz = size;
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(-deg * 180 / PI, 0, 0, 1);
        glScalef(sz, sz, sz);
        glCallList(displayListIdx + type * 3);
        glCallList(displayListIdx + type * 3 + 1);
        glTranslatef(0, 0, -0.5f);
        glScalef(1, 1, -1);
        glCallList(displayListIdx + type * 3 + 2);
        glPopMatrix();
    }

    public void drawType4()
    {
        if (cnt < 0)
            return;
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glCallList(displayListIdx + type * 3);
        glCallList(displayListIdx + type * 3 + 1);
        glTranslatef(0, 0, -0.5f);
        glScalef(1, 1, -1);
        glCallList(displayListIdx + type * 3 + 2);
        glPopMatrix();
    }

    public void drawArm()
    {
        float sz = 0;
        if (cnt < 0)
            return;
        else if (cnt < APPEAR_CNT)
            sz = size * cnt / APPEAR_CNT;
        else
            sz = size;
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(-armDeg * 180 / PI, 0, 0, 1);
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.7f, 0.9f, 0.3f, 0.3f);
        glVertex3f(0, 0, 0.5f);
        A7xScreen.setColor(0.7f, 0.9f, 0.3f, 0.9f);
        glVertex3f(-0.5f, 0, 0.5f);
        glVertex3f(0, sz, 0.5f);
        glVertex3f(0.5f, 0, 0.5f);
        glVertex3f(0, -sz, 0.5f);
        glEnd();
        glTranslatef(0, 0, -0.5f);
        glScalef(1, 1, -1);
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.7f, 0.9f, 0.3f, 0.1f);
        glVertex3f(0, 0, 0.5f);
        A7xScreen.setColor(0.7f, 0.9f, 0.3f, 0.5f);
        glVertex3f(-0.5f, 0, 0.5f);
        glVertex3f(0, sz, 0.5f);
        glVertex3f(0.5f, 0, 0.5f);
        glVertex3f(0, -sz, 0.5f);
        glEnd();
        glPopMatrix();
    }

    public void drawType5()
    {
        float sz = 0;
        if (cnt < 0)
            return;
        else if (cnt < APPEAR_CNT)
            sz = size * cnt / APPEAR_CNT;
        else
            sz = size;
        int hi = posHstIdx;
        for (int i = 0; i < 5; i++)
        {
            glPushMatrix();
            glTranslatef(posHst[hi].x, posHst[hi].y, 0.5f);
            glRotatef(-degHst[hi] * 180 / PI, 0, 0, 1);
            glScalef(sz, sz, sz);
            glCallList(displayListIdx + 5 * 3);
            glCallList(displayListIdx + 5 * 3 + 1);
            glTranslatef(0, 0, -0.5f);
            glScalef(1, 1, -1);
            glCallList(displayListIdx + 5 * 3 + 2);
            glPopMatrix();
            hi = GameMath.integer(hi + (size * 2 / speed));
            if (hi >= POSITION_HISTORY_LENGTH)
                hi = hi - (POSITION_HISTORY_LENGTH);
        }
    }

    public override void draw()
    {
        switch (type)
        {
            case 4:
                drawType4();
                drawArm();
                break;
            case 5:
                drawType5();
                break;
            default:
                drawType0();
                break;
        }
    }

    public void drawType0Luminous()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(-deg * 180 / PI, 0, 0, 1);
        glScalef(size, size, size);
        glCallList(displayListIdx + type * 3 + 1);
        glPopMatrix();
    }

    public void drawType4Luminous()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glCallList(displayListIdx + type * 3 + 1);
        glPopMatrix();
    }

    public void drawArmLuminous()
    {
        glPushMatrix();
        glTranslatef(pos.x, pos.y, 0.5f);
        glRotatef(-armDeg * 180 / PI, 0, 0, 1);
        glBegin(GL_LINE_STRIP);
        A7xScreen.setColor(0.5f, 0.9f, 0.3f, 0.9f);
        glVertex3f(-0.5f, 0, 0.5f);
        glVertex3f(0, size, 0.5f);
        glVertex3f(0.5f, 0, 0.5f);
        glVertex3f(0, -size, 0.5f);
        glVertex3f(-0.5f, 0, 0.5f);
        glEnd();
        glPopMatrix();
    }

    public void drawType5Luminous()
    {
    }

    public override void drawLuminous()
    {
        if (cnt < APPEAR_CNT)
            return;
        switch (type)
        {
            case 4:
                drawType4Luminous();
                drawArmLuminous();
                break;
            case 5:
                drawType5Luminous();
                break;
            default:
                drawType0Luminous();
                break;
        }
    }

    public static void createDisplayLists()
    {
        displayListIdx = glGenLists(18);
        glNewList(displayListIdx, GL_COMPILE);
        drawEnemyType0(1);
        glEndList();
        glNewList(displayListIdx + 1, GL_COMPILE);
        drawEnemyType0Line(1);
        glEndList();
        glNewList(displayListIdx + 2, GL_COMPILE);
        drawEnemyType0(0.6f);
        glEndList();
        glNewList(displayListIdx + 3, GL_COMPILE);
        drawEnemyType1(1);
        glEndList();
        glNewList(displayListIdx + 4, GL_COMPILE);
        drawEnemyType1Line(1);
        glEndList();
        glNewList(displayListIdx + 5, GL_COMPILE);
        drawEnemyType1(0.6f);
        glEndList();
        glNewList(displayListIdx + 6, GL_COMPILE);
        drawEnemyType2(1);
        glEndList();
        glNewList(displayListIdx + 7, GL_COMPILE);
        drawEnemyType2Line(1);
        glEndList();
        glNewList(displayListIdx + 8, GL_COMPILE);
        drawEnemyType2(0.6f);
        glEndList();
        glNewList(displayListIdx + 9, GL_COMPILE);
        drawEnemyType3(1);
        glEndList();
        glNewList(displayListIdx + 10, GL_COMPILE);
        drawEnemyType3Line(1);
        glEndList();
        glNewList(displayListIdx + 11, GL_COMPILE);
        drawEnemyType3(0.6f);
        glEndList();
        glNewList(displayListIdx + 12, GL_COMPILE);
        drawEnemyType4(1);
        glEndList();
        glNewList(displayListIdx + 13, GL_COMPILE);
        drawEnemyType4Line(1);
        glEndList();
        glNewList(displayListIdx + 14, GL_COMPILE);
        drawEnemyType4(0.6f);
        glEndList();
        glNewList(displayListIdx + 15, GL_COMPILE);
        drawEnemyType5(1);
        glEndList();
        glNewList(displayListIdx + 16, GL_COMPILE);
        drawEnemyType5Line(1);
        glEndList();
        glNewList(displayListIdx + 17, GL_COMPILE);
        drawEnemyType5(0.6f);
        glEndList();
    }

    public static void deleteDisplayLists()
    {
        glDeleteLists(displayListIdx, 18);
    }

    public static void drawEnemyType0(float alpha)
    {
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.9f, 0.7f, 0.4f, 0.9f * alpha);
        glVertex3f(0.8f, 1, 0.2f);
        A7xScreen.setColor(1, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(1, -1, 0);
        glVertex3f(0.7f, -0.8f, 0.8f);
        glVertex3f(0, 1, 0.2f);
        glEnd();
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.9f, 0.7f, 0.4f, 0.9f * alpha);
        glVertex3f(-0.8f, 1, 0.2f);
        A7xScreen.setColor(1, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(-1, -1, 0);
        glVertex3f(-0.7f, -0.8f, 0.8f);
        glVertex3f(0, 1, 0.2f);
        glEnd();
    }

    public static void drawEnemyType0Line(float alpha)
    {
        glBegin(GL_LINE_STRIP);
        A7xScreen.setColor(0.9f, 0.2f, 0.2f, 1 * alpha);
        glVertex3f(0.7f, -0.8f, 0.8f);
        glVertex3f(0.8f, 1, 0.2f);
        glVertex3f(-0.8f, 1, 0.2f);
        glVertex3f(-0.7f, -0.8f, 0.8f);
        glEnd();
    }

    public static void drawEnemyType1(float alpha)
    {
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.7f, 0.3f, 0.6f, 1.0f * alpha);
        glVertex3f(0, 1, 0.5f);
        A7xScreen.setColor(0.5f, 0.2f, 0.7f, 0.8f * alpha);
        glVertex3f(-0.5f, -1, 0.2f);
        glVertex3f(-0.8f, -0.6f, 0.6f);
        glVertex3f(0, -0.3f, 1);
        glVertex3f(0.8f, -0.6f, 0.6f);
        glVertex3f(0.5f, -1, 0.2f);
        glEnd();
    }

    public static void drawEnemyType1Line(float alpha)
    {
        glBegin(GL_LINE_STRIP);
        A7xScreen.setColor(0.4f, 0.2f, 0.7f, 1.0f * alpha);
        glVertex3f(-0.5f, -1, 0.2f);
        glVertex3f(-0.8f, -0.6f, 0.6f);
        glVertex3f(0, -0.3f, 1);
        glVertex3f(0.8f, -0.6f, 0.6f);
        glVertex3f(0.5f, -1, 0.2f);
        glEnd();
    }

    public static void drawEnemyType2(float alpha)
    {
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(0.6f, 0.8f, 0.2f, 1.0f * alpha);
        glVertex3f(-0.3f, -0.6f, 1);
        A7xScreen.setColor(0.5f, 0.8f, 0.2f, 0.5f * alpha);
        glVertex3f(0, 0.6f, 0.7f);
        glVertex3f(-0.9f, 0.8f, 0.4f);
        A7xScreen.setColor(0.6f, 0.8f, 0.5f, 1.0f * alpha);
        glVertex3f(-0.2f, 0, 0);
        glEnd();
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(0.6f, 0.8f, 0.2f, 1.0f * alpha);
        glVertex3f(0.3f, -0.6f, 1);
        A7xScreen.setColor(0.5f, 0.8f, 0.2f, 0.5f * alpha);
        glVertex3f(0, 0.6f, 0.7f);
        glVertex3f(0.9f, 0.8f, 0.4f);
        A7xScreen.setColor(0.6f, 0.8f, 0.5f, 1.0f * alpha);
        glVertex3f(0.2f, 0, 0);
        glEnd();
    }

    public static void drawEnemyType2Line(float alpha)
    {
        A7xScreen.setColor(0.5f, 1, 0.2f, 1.0f * alpha);
        glBegin(GL_LINE_STRIP);
        glVertex3f(-0.9f, 0.8f, 0.4f);
        glVertex3f(0, 0.6f, 0.7f);
        glVertex3f(0.9f, 0.8f, 0.4f);
        glEnd();
    }

    public static void drawEnemyType3(float alpha)
    {
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(0, 1, 0.7f);
        glVertex3f(0.8f, 0.4f, 0.7f);
        A7xScreen.setColor(0.8f, 0.1f, 0.6f, 0.6f * alpha);
        glVertex3f(0.6f, 0.3f, 0);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(0.8f, -0.4f, 0.7f);
        glEnd();
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(0.8f, -0.4f, 0.7f);
        glVertex3f(0, -1, 0.7f);
        A7xScreen.setColor(0.8f, 0.1f, 0.6f, 0.6f * alpha);
        glVertex3f(0, -0.7f, 0);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(-0.8f, -0.4f, 0.7f);
        glEnd();
        glBegin(GL_TRIANGLE_STRIP);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(-0.8f, -0.4f, 0.7f);
        glVertex3f(-0.8f, 0.4f, 0.7f);
        A7xScreen.setColor(0.8f, 0.1f, 0.6f, 0.6f * alpha);
        glVertex3f(-0.6f, 0.3f, 0);
        A7xScreen.setColor(0.8f, 0.2f, 0.4f, 0.9f * alpha);
        glVertex3f(0, 1, 0.7f);
        glEnd();
    }

    public static void drawEnemyType3Line(float alpha)
    {
        A7xScreen.setColor(0.8f, 0.2f, 0.6f, 0.9f * alpha);
        glBegin(GL_LINE_STRIP);
        glVertex3f(0, 1, 0.7f);
        glVertex3f(0.6f, 0.3f, 0);
        glVertex3f(0.8f, -0.4f, 0.7f);
        glVertex3f(0, -0.7f, 0);
        glVertex3f(-0.8f, -0.4f, 0.7f);
        glVertex3f(-0.6f, 0.3f, 0);
        glVertex3f(0, 1, 0.7f);
        glEnd();
    }

    public static void drawEnemyType4(float alpha)
    {
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.5f, 0.7f, 0.3f, 0.9f * alpha);
        glVertex3f(0, 0, 1);
        A7xScreen.setColor(0.5f, 0.9f, 0.3f, 0.5f * alpha);
        glVertex3f(1, 0, 0.2f);
        glVertex3f(0, 1, 0.2f);
        glVertex3f(-1, 0, 0.2f);
        glVertex3f(0, -1, 0.2f);
        glVertex3f(1, 0, 0.2f);
        glEnd();
    }

    public static void drawEnemyType4Line(float alpha)
    {
        A7xScreen.setColor(0.3f, 0.8f, 0.3f, 0.9f * alpha);
        glBegin(GL_LINE_STRIP);
        glVertex3f(1, 0, 0.2f);
        glVertex3f(0, 1, 0.2f);
        glVertex3f(-1, 0, 0.2f);
        glVertex3f(0, -1, 0.2f);
        glVertex3f(1, 0, 0.2f);
        glEnd();
    }

    public static void drawEnemyType5(float alpha)
    {
        glBegin(GL_TRIANGLE_FAN);
        A7xScreen.setColor(0.6f, 0.3f, 0.8f, 0.9f * alpha);
        glVertex3f(0, 0.5f, 1);
        A7xScreen.setColor(0.4f, 0.3f, 0.9f, 0.6f * alpha);
        glVertex3f(-0.3f, 1, 0.3f);
        glVertex3f(0.3f, 1, 0.3f);
        glVertex3f(0.5f, -1, 0.4f);
        glVertex3f(-0.5f, -1, 0.4f);
        glVertex3f(-0.3f, 1, 0.3f);
        glEnd();
    }

    public static void drawEnemyType5Line(float alpha)
    {
        A7xScreen.setColor(0.4f, 0.3f, 0.9f, 0.9f * alpha);
        glBegin(GL_LINE_STRIP);
        glVertex3f(-0.3f, 1, 0.3f);
        glVertex3f(0.3f, 1, 0.3f);
        glVertex3f(0.5f, -1, 0.4f);
        glVertex3f(-0.5f, -1, 0.4f);
        glVertex3f(-0.3f, 1, 0.3f);
        glEnd();
    }
}

public class EnemyInitializer : ActorInitializer
{
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public EnemyInitializer(Ship ship, Field field, Rand rand, A7xGameManager manager)
    {
        this.ship = ship;
        this.field = field;
        this.rand = rand;
        this.manager = manager;
    }
}
