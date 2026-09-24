// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class Bullet : Actor
{
    public GameManager gameManager;
    public Field field;
    public Ship ship;
    public SmokePool smokes;
    public WakePool wakes;
    public CrystalPool crystals;
    public Vector pos;
    public Vector ppos;
    public float deg, speed;
    public float trgDeg, trgSpeed;
    public float size;
    public int cnt;
    public float range;
    public bool _destructive;
    public BulletShape shape;
    public int _enemyIdx;
    public Bullet()
    {
        pos = new Vector();
        ppos = new Vector();
        shape = new BulletShape();
        {
            trgDeg = 0;
            deg = trgDeg;
        }

        {
            trgSpeed = 1;
            speed = trgSpeed;
        }

        size = 1;
        range = 1;
    }

    public override void init(List<object> args)
    {
        gameManager = (GameManager)args[0];
        field = (Field)args[1];
        ship = (Ship)args[2];
        smokes = (SmokePool)args[3];
        wakes = (WakePool)args[4];
        crystals = (CrystalPool)args[5];
    }

    public void set(int enemyIdx, Vector p, float deg, float speed, float size, int shapeType, float range, float startSpeed = 0, float startDeg = -99999, bool destructive = false)
    {
        if (!field.checkInOuterFieldExceptTop(p))
            return;
        _enemyIdx = enemyIdx;
        {
            pos.x = p.x;
            ppos.x = pos.x;
        }

        {
            pos.y = p.y;
            ppos.y = pos.y;
        }

        this.speed = startSpeed;
        if (startDeg == -99999)
            this.deg = deg;
        else
            this.deg = startDeg;
        trgDeg = deg;
        trgSpeed = speed;
        this.size = size;
        this.range = range;
        _destructive = destructive;
        shape.set(shapeType);
        shape.size = size;
        cnt = 0;
        exists = true;
    }

    public override void move()
    {
        ppos.x = pos.x;
        ppos.y = pos.y;
        if (cnt < 30)
        {
            speed = speed + ((trgSpeed - speed) * 0.066f);
            float md = trgDeg - deg;
            md = normalizeDeg(md);
            deg = deg + (md * 0.066f);
            if (cnt == 29)
            {
                speed = trgSpeed;
                deg = trgDeg;
            }
        }

        if (field.checkInOuterField_1(pos))
            gameManager.addSlowdownRatio(speed * 0.24f);
        float mx = sin(deg) * speed;
        float my = cos(deg) * speed;
        pos.x = pos.x + (mx);
        pos.y = pos.y + (my);
        pos.y = pos.y - (field.lastScrollY);
        if (ship.checkBulletHit(pos, ppos) || (!field.checkInOuterFieldExceptTop(pos)))
        {
            remove();
            return;
        }

        cnt++;
        range = range - (speed);
        if (range <= 0)
            startDisappear();
        if (field.getBlock_1(pos) >= Field.ON_BLOCK_THRESHOLD)
            startDisappear();
    }

    public void startDisappear()
    {
        if (field.getBlock_1(pos) >= 0)
        {
            Smoke s = smokes.getInstanceForced();
            s.set_7(pos, sin(deg) * speed * 0.2f, cos(deg) * speed * 0.2f, 0, SmokeSmokeType.SAND, 30, size * 0.5f);
        }
        else
        {
            Wake w = wakes.getInstanceForced();
            w.set(pos, deg, speed, 60, size * 3, true);
        }

        remove();
    }

    public void changeToCrystal()
    {
        Crystal c = crystals.getInstance();
        if (c != null)
            c.set(pos);
        remove();
    }

    public void remove()
    {
        exists = false;
    }

    public override void draw(float[] model, Mesh particles = null)
    {
        if (!field.checkInOuterField_1(pos))
            return;
        model = Transform.Translate(model, pos.x, pos.y, 0);
        if (_destructive)
        {
            model = Transform.Rotate(model, cnt * 13, 0, 0, 1);
        }
        else
        {
            model = Transform.Rotate(model, -deg * 180 / PI, 0, 0, 1);
            model = Transform.Rotate(model, cnt * 13, 0, 1, 0);
        }

        shape.draw(model);
    }

    public void checkShotHit(Vector p, Collidable s, Shot shot)
    {
        float ox = fabs(pos.x - p.x), oy = fabs(pos.y - p.y);
        if (ox + oy < 0.5f)
        {
            shot.removeHitToBullet();
            Smoke smoke = smokes.getInstance();
            if (smoke != null)
                smoke.set_7(pos, sin(deg) * speed, cos(deg) * speed, 0, SmokeSmokeType.SPARK, 30, size * 0.5f);
            remove();
        }
    }

    public bool destructive
    {
        get
        {
            return _destructive;
        }

        set
        {
            _destructive = value;
        }
    }

    public int enemyIdx
    {
        get
        {
            return _enemyIdx;
        }

        set
        {
            _enemyIdx = value;
        }
    }
}

public class BulletPool : ActorPool<Bullet>
{
    public BulletPool(int n, List<object> args) : base(n, args, () => new Bullet())
    {
    }

    public int removeIndexedBullets(int idx)
    {
        int n = 0;
        foreach (Bullet b in actor)
        {
            if (b.exists && (b.enemyIdx == idx))
            {
                b.changeToCrystal();
                n++;
            }
        }

        return n;
    }

    public void checkShotHit(Vector pos, Collidable shape, Shot shot)
    {
        foreach (Bullet b in actor)
            if (b.exists && b.destructive)
                b.checkShotHit(pos, shape, shot);
    }
}
