// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class BulletActor : Actor
{
    public BulletImpl bullet;
    public float rootRank;
    public const int DISAPPEAR_FRAMES = 45;
    public static int nextId = 0;
    public Tunnel tunnel;
    public Ship ship;
    public Vector ppos;
    public int cnt;
    public bool isSimple;
    public bool isTop;
    public bool isAimTop;
    public bool isVisible;
    public bool shouldBeRemoved;
    public bool isWait;
    public int postWait;
    public int waitCnt;
    public bool isMorphSeed;
    public int disapCnt;
    public override void init_1(List<object> args)
    {
        tunnel = (Tunnel)args[0];
        ship = (Ship)args[1];
        bullet = new BulletImpl(nextId);
        nextId++;
        ppos = new Vector();
    }

    public void set_5(PatternState[] runner, float x, float y, float deg, float speed)
    {
        bullet.set_6(runner, x, y, deg, speed, 0);
        isSimple = false;
        start();
    }

    public void set_4(float x, float y, float deg, float speed)
    {
        bullet.set_5(x, y, deg, speed, 0);
        isSimple = true;
        start();
    }

    public void start()
    {
        {
            isAimTop = false;
            isTop = isAimTop;
        }

        isWait = false;
        isVisible = true;
        isMorphSeed = false;
        ppos.x = bullet.pos.x;
        ppos.y = bullet.pos.y;
        cnt = 0;
        rootRank = 1;
        shouldBeRemoved = false;
        disapCnt = 0;
        exists = true;
    }

    public void setInvisible()
    {
        isVisible = false;
    }

    public void setTop()
    {
        {
            isAimTop = true;
            isTop = isAimTop;
        }

        setInvisible();
    }

    public void unsetTop()
    {
        {
            isAimTop = false;
            isTop = isAimTop;
        }
    }

    public void unsetAimTop()
    {
        isAimTop = false;
    }

    public void setWait(int prvw, int pstw)
    {
        isWait = true;
        waitCnt = prvw;
        postWait = pstw;
    }

    public void setMorphSeed()
    {
        isMorphSeed = true;
    }

    public void rewind()
    {
        bullet.remove();
        bullet.resetParser();
        PatternState[] runner = Bullet.createRunner(bullet.getParser());
        bullet.setRunner(runner);
    }

    public void remove()
    {
        shouldBeRemoved = true;
    }

    public void removeForced()
    {
        if (!(isSimple))
            bullet.remove();
        exists = false;
    }

    public void startDisappear()
    {
        if ((isVisible) && (disapCnt <= 0))
            disapCnt = 1;
    }

    public override void move()
    {
        Vector tpos = bullet.target.getTargetPos();
        Bullet.target.x = tpos.x;
        Bullet.target.y = tpos.y;
        ppos.x = bullet.pos.x;
        ppos.y = bullet.pos.y;
        if (isAimTop)
        {
            float ox = tpos.x - bullet.pos.x;
            if (ox > PI)
                ox = ox - (PI * 2);
            else if (ox < -PI)
                ox = ox + (PI * 2);
            bullet.deg = (atan2(ox, tpos.y - bullet.pos.y) * bullet.xReverse + PI / 2) * bullet.yReverse - PI / 2;
        }

        if ((isWait) && (waitCnt > 0))
        {
            waitCnt--;
            if (shouldBeRemoved)
                removeForced();
            return;
        }

        if (!(isSimple))
        {
            bullet.move();
            if (bullet.isEnd())
            {
                if (isTop)
                {
                    rewind();
                    if (isWait)
                    {
                        waitCnt = postWait;
                        return;
                    }
                }
                else if (isMorphSeed)
                {
                    removeForced();
                    return;
                }
            }
        }

        if (shouldBeRemoved)
        {
            removeForced();
            return;
        }

        float mx = (sin(bullet.deg) * bullet.velocity + bullet.acc.x) * bullet.getSpeedRank() * bullet.xReverse;
        float my = (cos(bullet.deg) * bullet.velocity - bullet.acc.y) * bullet.getSpeedRank() * bullet.yReverse;
        float d = atan2(mx, my);
        float r = 1 - fabs(sin(d)) * 0.999f;
        r = r * ((ship.speed * 5));
        bullet.pos.x = bullet.pos.x + (mx * r);
        bullet.pos.y = bullet.pos.y + (my * r);
        if (bullet.pos.x >= PI * 2)
            bullet.pos.x = bullet.pos.x - (PI * 2);
        else if (bullet.pos.x < 0)
            bullet.pos.x = bullet.pos.x + (PI * 2);
        if ((isVisible) && (disapCnt <= 0))
        {
            if (ship.checkBulletHit(bullet.pos, ppos))
                removeForced();
            if (((bullet.pos.y < -2) || (((!(bullet.longRange)) && (bullet.pos.y > ship.inSightDepth)))) || (!(tunnel.checkInScreen_2(bullet.pos, ship))))
                startDisappear();
        }

        cnt++;
        if (disapCnt > 0)
        {
            disapCnt++;
            if (disapCnt > DISAPPEAR_FRAMES)
                removeForced();
        }
        else
        {
            if (cnt > 600)
                startDisappear();
        }
    }

    public void checkShotHit(Vector p, Collidable shape, Shot shot)
    {
        if ((!(isVisible)) || (disapCnt > 0))
            return;
        float ox = fabs(bullet.pos.x - p.x), oy = fabs(bullet.pos.y - p.y);
        if (ox > PI)
            ox = PI * 2 - ox;
        ox = ox * ((tunnel.getRadius(bullet.pos.y) / SliceState.DEFAULT_RAD));
        ox = ox * (3);
        if (shape.checkCollision(ox, oy))
        {
            startDisappear();
            shot.addScore(10, bullet.pos);
        }
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        if (!(isVisible))
            return;
        float d = (bullet.deg * bullet.xReverse + PI / 2) * bullet.yReverse - PI / 2;
        Vector3 sp = tunnel.getPos_1_Vector(bullet.pos);
        float[] parent1 = model;
        model = Transform.Translate(model, sp.x, sp.y, sp.z);
        model = Transform.Rotate(model, d * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, cnt * 6, 0, 0, 1);
        if (disapCnt <= 0)
        {
            bullet.shape.draw(model, tint, blend, cull, lineWidth);
        }
        else
        {
            float s = 1 - (float)disapCnt / DISAPPEAR_FRAMES;
            model = Transform.Scale(model, s, s, s);
            bullet.disapShape.draw(model, tint, blend, cull, lineWidth);
        }

        model = parent1;
    }
}
