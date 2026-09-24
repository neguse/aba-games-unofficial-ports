// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class Enemy : LuminousActor
{
    public static Mesh[] meshes;
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

    public void drawType0(float[] model, float[] tint, Gfx.Blend blend)
    {
        float sz = 0;
        if (cnt < 0)
            return;
        else if (cnt < APPEAR_CNT)
            sz = size * cnt / APPEAR_CNT;
        else
            sz = size;
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, -deg * 180 / PI, 0, 0, 1);
        model = Transform.Scale(model, sz, sz, sz);
        { Mesh shape2 = meshes[type * 3]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        { Mesh shape3 = meshes[type * 3 + 1]; if (shape3.count > 0) Gfx.Draw(shape3.count, shape3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = Transform.Translate(model, 0, 0, -0.5f);
        model = Transform.Scale(model, 1, 1, -1);
        { Mesh shape4 = meshes[type * 3 + 2]; if (shape4.count > 0) Gfx.Draw(shape4.count, shape4.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public void drawType4(float[] model, float[] tint, Gfx.Blend blend)
    {
        if (cnt < 0)
            return;
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        { Mesh shape2 = meshes[type * 3]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        { Mesh shape3 = meshes[type * 3 + 1]; if (shape3.count > 0) Gfx.Draw(shape3.count, shape3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = Transform.Translate(model, 0, 0, -0.5f);
        model = Transform.Scale(model, 1, 1, -1);
        { Mesh shape4 = meshes[type * 3 + 2]; if (shape4.count > 0) Gfx.Draw(shape4.count, shape4.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public void drawArm(float[] model, float[] tint, Gfx.Blend blend)
    {
        float sz = 0;
        if (cnt < 0)
            return;
        else if (cnt < APPEAR_CNT)
            sz = size * cnt / APPEAR_CNT;
        else
            sz = size;
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, -armDeg * 180 / PI, 0, 0, 1);
        var part2 = new Mesh("Enemy-drawArm-2" + "-" + meshKey);
        tint = new float[] { 0.7f, 0.9f, 0.3f, 0.3f };
        part2.Vertex(0, 0, 0.5f, tint);
        tint = new float[] { 0.7f, 0.9f, 0.3f, 0.9f };
        part2.Vertex(-0.5f, 0, 0.5f, tint);
        part2.Vertex(0, sz, 0.5f, tint);
        part2.Vertex(0.5f, 0, 0.5f, tint);
        part2.Vertex(0, -sz, 0.5f, tint);
        part2.Fan(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = Transform.Translate(model, 0, 0, -0.5f);
        model = Transform.Scale(model, 1, 1, -1);
        var part3 = new Mesh("Enemy-drawArm-3" + "-" + meshKey);
        tint = new float[] { 0.7f, 0.9f, 0.3f, 0.1f };
        part3.Vertex(0, 0, 0.5f, tint);
        tint = new float[] { 0.7f, 0.9f, 0.3f, 0.5f };
        part3.Vertex(-0.5f, 0, 0.5f, tint);
        part3.Vertex(0, sz, 0.5f, tint);
        part3.Vertex(0.5f, 0, 0.5f, tint);
        part3.Vertex(0, -sz, 0.5f, tint);
        part3.Fan(0, part3.vertexCount - 0);
        if (part3.count > 0) Gfx.Draw(part3.count, part3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public void drawType5(float[] model, float[] tint, Gfx.Blend blend)
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
            float[] parent1 = model;
            model = Transform.Translate(model, posHst[hi].x, posHst[hi].y, 0.5f);
            model = Transform.Rotate(model, -degHst[hi] * 180 / PI, 0, 0, 1);
            model = Transform.Scale(model, sz, sz, sz);
            { Mesh shape2 = meshes[5 * 3]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
            { Mesh shape3 = meshes[5 * 3 + 1]; if (shape3.count > 0) Gfx.Draw(shape3.count, shape3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
            model = Transform.Translate(model, 0, 0, -0.5f);
            model = Transform.Scale(model, 1, 1, -1);
            { Mesh shape4 = meshes[5 * 3 + 2]; if (shape4.count > 0) Gfx.Draw(shape4.count, shape4.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
            model = parent1;
            hi = GameMath.integer(hi + (size * 2 / speed));
            if (hi >= POSITION_HISTORY_LENGTH)
                hi = hi - (POSITION_HISTORY_LENGTH);
        }
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        switch (type)
        {
            case 4:
                drawType4(model, tint, blend);
                drawArm(model, tint, blend);
                break;
            case 5:
                drawType5(model, tint, blend);
                break;
            default:
                drawType0(model, tint, blend);
                break;
        }
    }

    public void drawType0Luminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, -deg * 180 / PI, 0, 0, 1);
        model = Transform.Scale(model, size, size, size);
        { Mesh shape2 = meshes[type * 3 + 1]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public void drawType4Luminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        { Mesh shape2 = meshes[type * 3 + 1]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public void drawArmLuminous(float[] model, float[] tint, Gfx.Blend blend)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, -armDeg * 180 / PI, 0, 0, 1);
        var part2 = new Mesh("Enemy-drawArmLuminous-2" + "-" + meshKey);
        tint = new float[] { 0.5f, 0.9f, 0.3f, 0.9f };
        part2.Vertex(-0.5f, 0, 0.5f, tint);
        part2.Vertex(0, size, 0.5f, tint);
        part2.Vertex(0.5f, 0, 0.5f, tint);
        part2.Vertex(0, -size, 0.5f, tint);
        part2.Vertex(-0.5f, 0, 0.5f, tint);
        part2.LineStrip(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public void drawType5Luminous(float[] model, float[] tint, Gfx.Blend blend)
    {
    }

    public override void drawLuminous(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        if (cnt < APPEAR_CNT)
            return;
        switch (type)
        {
            case 4:
                drawType4Luminous(model, tint, blend);
                drawArmLuminous(model, tint, blend);
                break;
            case 5:
                drawType5Luminous(model, tint, blend);
                break;
            default:
                drawType0Luminous(model, tint, blend);
                break;
        }
    }

    public static void createMeshes()
    {
        meshes = new Mesh[18]; Mesh mesh = null; float[] model = Transform.Identity(); float[] tint = null;
        mesh = new Mesh("Enemy-" + (0).ToString()); meshes[0] = mesh;
        appendEnemyType0(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (1).ToString()); meshes[1] = mesh;
        appendEnemyType0Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (2).ToString()); meshes[2] = mesh;
        appendEnemyType0(mesh, model, tint, 0.6f);

        mesh = new Mesh("Enemy-" + (3).ToString()); meshes[3] = mesh;
        appendEnemyType1(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (4).ToString()); meshes[4] = mesh;
        appendEnemyType1Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (5).ToString()); meshes[5] = mesh;
        appendEnemyType1(mesh, model, tint, 0.6f);

        mesh = new Mesh("Enemy-" + (6).ToString()); meshes[6] = mesh;
        appendEnemyType2(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (7).ToString()); meshes[7] = mesh;
        appendEnemyType2Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (8).ToString()); meshes[8] = mesh;
        appendEnemyType2(mesh, model, tint, 0.6f);

        mesh = new Mesh("Enemy-" + (9).ToString()); meshes[9] = mesh;
        appendEnemyType3(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (10).ToString()); meshes[10] = mesh;
        appendEnemyType3Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (11).ToString()); meshes[11] = mesh;
        appendEnemyType3(mesh, model, tint, 0.6f);

        mesh = new Mesh("Enemy-" + (12).ToString()); meshes[12] = mesh;
        appendEnemyType4(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (13).ToString()); meshes[13] = mesh;
        appendEnemyType4Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (14).ToString()); meshes[14] = mesh;
        appendEnemyType4(mesh, model, tint, 0.6f);

        mesh = new Mesh("Enemy-" + (15).ToString()); meshes[15] = mesh;
        appendEnemyType5(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (16).ToString()); meshes[16] = mesh;
        appendEnemyType5Line(mesh, model, tint, 1);

        mesh = new Mesh("Enemy-" + (17).ToString()); meshes[17] = mesh;
        appendEnemyType5(mesh, model, tint, 0.6f);

    }

    public static void deleteMeshes() { meshes = null; }

    public static void appendEnemyType0(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.9f, 0.7f, 0.4f, 0.9f * alpha };
        mesh.Vertex(0.8f, 1, 0.2f, tint, model);
        tint = new float[] { 1, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(1, -1, 0, tint, model);
        mesh.Vertex(0.7f, -0.8f, 0.8f, tint, model);
        mesh.Vertex(0, 1, 0.2f, tint, model);
        mesh.Fan(part1, mesh.vertexCount - part1);
        int part2 = mesh.vertexCount;
        tint = new float[] { 0.9f, 0.7f, 0.4f, 0.9f * alpha };
        mesh.Vertex(-0.8f, 1, 0.2f, tint, model);
        tint = new float[] { 1, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(-1, -1, 0, tint, model);
        mesh.Vertex(-0.7f, -0.8f, 0.8f, tint, model);
        mesh.Vertex(0, 1, 0.2f, tint, model);
        mesh.Fan(part2, mesh.vertexCount - part2);
    }

    public static void appendEnemyType0Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.9f, 0.2f, 0.2f, 1 * alpha };
        mesh.Vertex(0.7f, -0.8f, 0.8f, tint, model);
        mesh.Vertex(0.8f, 1, 0.2f, tint, model);
        mesh.Vertex(-0.8f, 1, 0.2f, tint, model);
        mesh.Vertex(-0.7f, -0.8f, 0.8f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType1(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.7f, 0.3f, 0.6f, 1.0f * alpha };
        mesh.Vertex(0, 1, 0.5f, tint, model);
        tint = new float[] { 0.5f, 0.2f, 0.7f, 0.8f * alpha };
        mesh.Vertex(-0.5f, -1, 0.2f, tint, model);
        mesh.Vertex(-0.8f, -0.6f, 0.6f, tint, model);
        mesh.Vertex(0, -0.3f, 1, tint, model);
        mesh.Vertex(0.8f, -0.6f, 0.6f, tint, model);
        mesh.Vertex(0.5f, -1, 0.2f, tint, model);
        mesh.Fan(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType1Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.4f, 0.2f, 0.7f, 1.0f * alpha };
        mesh.Vertex(-0.5f, -1, 0.2f, tint, model);
        mesh.Vertex(-0.8f, -0.6f, 0.6f, tint, model);
        mesh.Vertex(0, -0.3f, 1, tint, model);
        mesh.Vertex(0.8f, -0.6f, 0.6f, tint, model);
        mesh.Vertex(0.5f, -1, 0.2f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType2(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.6f, 0.8f, 0.2f, 1.0f * alpha };
        mesh.Vertex(-0.3f, -0.6f, 1, tint, model);
        tint = new float[] { 0.5f, 0.8f, 0.2f, 0.5f * alpha };
        mesh.Vertex(0, 0.6f, 0.7f, tint, model);
        mesh.Vertex(-0.9f, 0.8f, 0.4f, tint, model);
        tint = new float[] { 0.6f, 0.8f, 0.5f, 1.0f * alpha };
        mesh.Vertex(-0.2f, 0, 0, tint, model);
        for (int vi = part1; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part1) % 2), vi + 1 - ((vi - part1) % 2), vi + 2);
        int part2 = mesh.vertexCount;
        tint = new float[] { 0.6f, 0.8f, 0.2f, 1.0f * alpha };
        mesh.Vertex(0.3f, -0.6f, 1, tint, model);
        tint = new float[] { 0.5f, 0.8f, 0.2f, 0.5f * alpha };
        mesh.Vertex(0, 0.6f, 0.7f, tint, model);
        mesh.Vertex(0.9f, 0.8f, 0.4f, tint, model);
        tint = new float[] { 0.6f, 0.8f, 0.5f, 1.0f * alpha };
        mesh.Vertex(0.2f, 0, 0, tint, model);
        for (int vi = part2; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part2) % 2), vi + 1 - ((vi - part2) % 2), vi + 2);
    }

    public static void appendEnemyType2Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        tint = new float[] { 0.5f, 1, 0.2f, 1.0f * alpha };
        int part1 = mesh.vertexCount;
        mesh.Vertex(-0.9f, 0.8f, 0.4f, tint, model);
        mesh.Vertex(0, 0.6f, 0.7f, tint, model);
        mesh.Vertex(0.9f, 0.8f, 0.4f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType3(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(0, 1, 0.7f, tint, model);
        mesh.Vertex(0.8f, 0.4f, 0.7f, tint, model);
        tint = new float[] { 0.8f, 0.1f, 0.6f, 0.6f * alpha };
        mesh.Vertex(0.6f, 0.3f, 0, tint, model);
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(0.8f, -0.4f, 0.7f, tint, model);
        for (int vi = part1; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part1) % 2), vi + 1 - ((vi - part1) % 2), vi + 2);
        int part2 = mesh.vertexCount;
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(0.8f, -0.4f, 0.7f, tint, model);
        mesh.Vertex(0, -1, 0.7f, tint, model);
        tint = new float[] { 0.8f, 0.1f, 0.6f, 0.6f * alpha };
        mesh.Vertex(0, -0.7f, 0, tint, model);
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(-0.8f, -0.4f, 0.7f, tint, model);
        for (int vi = part2; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part2) % 2), vi + 1 - ((vi - part2) % 2), vi + 2);
        int part3 = mesh.vertexCount;
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(-0.8f, -0.4f, 0.7f, tint, model);
        mesh.Vertex(-0.8f, 0.4f, 0.7f, tint, model);
        tint = new float[] { 0.8f, 0.1f, 0.6f, 0.6f * alpha };
        mesh.Vertex(-0.6f, 0.3f, 0, tint, model);
        tint = new float[] { 0.8f, 0.2f, 0.4f, 0.9f * alpha };
        mesh.Vertex(0, 1, 0.7f, tint, model);
        for (int vi = part3; vi + 2 < mesh.vertexCount; vi++) mesh.Triangle(vi + ((vi - part3) % 2), vi + 1 - ((vi - part3) % 2), vi + 2);
    }

    public static void appendEnemyType3Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        tint = new float[] { 0.8f, 0.2f, 0.6f, 0.9f * alpha };
        int part1 = mesh.vertexCount;
        mesh.Vertex(0, 1, 0.7f, tint, model);
        mesh.Vertex(0.6f, 0.3f, 0, tint, model);
        mesh.Vertex(0.8f, -0.4f, 0.7f, tint, model);
        mesh.Vertex(0, -0.7f, 0, tint, model);
        mesh.Vertex(-0.8f, -0.4f, 0.7f, tint, model);
        mesh.Vertex(-0.6f, 0.3f, 0, tint, model);
        mesh.Vertex(0, 1, 0.7f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType4(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.5f, 0.7f, 0.3f, 0.9f * alpha };
        mesh.Vertex(0, 0, 1, tint, model);
        tint = new float[] { 0.5f, 0.9f, 0.3f, 0.5f * alpha };
        mesh.Vertex(1, 0, 0.2f, tint, model);
        mesh.Vertex(0, 1, 0.2f, tint, model);
        mesh.Vertex(-1, 0, 0.2f, tint, model);
        mesh.Vertex(0, -1, 0.2f, tint, model);
        mesh.Vertex(1, 0, 0.2f, tint, model);
        mesh.Fan(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType4Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        tint = new float[] { 0.3f, 0.8f, 0.3f, 0.9f * alpha };
        int part1 = mesh.vertexCount;
        mesh.Vertex(1, 0, 0.2f, tint, model);
        mesh.Vertex(0, 1, 0.2f, tint, model);
        mesh.Vertex(-1, 0, 0.2f, tint, model);
        mesh.Vertex(0, -1, 0.2f, tint, model);
        mesh.Vertex(1, 0, 0.2f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType5(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 0.6f, 0.3f, 0.8f, 0.9f * alpha };
        mesh.Vertex(0, 0.5f, 1, tint, model);
        tint = new float[] { 0.4f, 0.3f, 0.9f, 0.6f * alpha };
        mesh.Vertex(-0.3f, 1, 0.3f, tint, model);
        mesh.Vertex(0.3f, 1, 0.3f, tint, model);
        mesh.Vertex(0.5f, -1, 0.4f, tint, model);
        mesh.Vertex(-0.5f, -1, 0.4f, tint, model);
        mesh.Vertex(-0.3f, 1, 0.3f, tint, model);
        mesh.Fan(part1, mesh.vertexCount - part1);
    }

    public static void appendEnemyType5Line(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        tint = new float[] { 0.4f, 0.3f, 0.9f, 0.9f * alpha };
        int part1 = mesh.vertexCount;
        mesh.Vertex(-0.3f, 1, 0.3f, tint, model);
        mesh.Vertex(0.3f, 1, 0.3f, tint, model);
        mesh.Vertex(0.5f, -1, 0.4f, tint, model);
        mesh.Vertex(-0.5f, -1, 0.4f, tint, model);
        mesh.Vertex(-0.3f, 1, 0.3f, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
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
