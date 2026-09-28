// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Shot : Actor
{
    public const float SPEED = 0.75f;
    public const float RANGE_MIN = 2;
    public const float SIZE_MIN = 0.1f;
    public const int MAX_CHARGE = 90;
    public const float SIZE_RATIO = 0.15f;
    public const float RANGE_RATIO = 0.5f;
    public const float CHARGE_RELEASE_RATIO = 0.25f;
    public const int MAX_MULTIPLIER = 100;
    public static ShotShape shotShape, chargeShotShape;
    public static Rand rand = new Rand();
    public Tunnel tunnel;
    public EnemyPool enemies;
    public BulletActorPool bullets;
    public FloatLetterPool floatLetters;
    public ParticlePool particles;
    public Ship ship;
    public Vector pos;
    public float chargeCnt, chargeSeCnt;
    public float cnt;
    public float range;
    public float size, trgSize;
    public bool chargeShot;
    public bool inCharge;
    public bool starShell;
    public ResizableDrawable shape;
    public int multiplier;
    public int _damage;
    public float deg;
    public static void init_0()
    {
        shotShape = new ShotShape();
        shotShape.create(false);
        chargeShotShape = new ShotShape();
        chargeShotShape.create(true);
        rand = new Rand();
    }

    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void close()
    {
        shotShape.close();
    }

    public override void init_1(List<object> args)
    {
        tunnel = (Tunnel)args[0];
        enemies = (EnemyPool)args[1];
        bullets = (BulletActorPool)args[2];
        floatLetters = (FloatLetterPool)args[3];
        particles = (ParticlePool)args[4];
        ship = (Ship)args[5];
        pos = new Vector();
        shape = new ResizableDrawable();
    }

    public void set_3(bool charge = false, bool star = false, float d = 0)
    {
        cnt = 0;
        multiplier = 1;
        if (charge)
        {
            {
                inCharge = true;
                chargeShot = inCharge;
            }

            range = 0;
            {
                chargeSeCnt = 0;
                chargeCnt = chargeSeCnt;
            }

            {
                trgSize = 0;
                size = trgSize;
            }

            _damage = 100;
            starShell = false;
            deg = d;
            shape.shape = chargeShotShape;
        }
        else
        {
            {
                inCharge = false;
                chargeShot = inCharge;
            }

            range = Ship.IN_SIGHT_DEPTH_DEFAULT;
            {
                chargeSeCnt = 0;
                chargeCnt = chargeSeCnt;
            }

            {
                trgSize = 1;
                size = trgSize;
            }

            _damage = 1;
            starShell = star;
            deg = d;
            shape.shape = shotShape;
            SoundManager.playSe("shot.wav");
        }

        exists = true;
    }

    public void update(Vector p)
    {
        pos.x = p.x;
        pos.y = p.y + 0.3f;
    }

    public void release()
    {
        if (chargeCnt < MAX_CHARGE * CHARGE_RELEASE_RATIO)
        {
            remove();
            return;
        }

        inCharge = false;
        range = RANGE_MIN + chargeCnt * RANGE_RATIO;
        trgSize = SIZE_MIN + chargeCnt * SIZE_RATIO;
        SoundManager.playSe("charge_shot.wav");
    }

    public void remove()
    {
        exists = false;
    }

    public override void move()
    {
        if (inCharge)
        {
            if (chargeCnt < MAX_CHARGE)
            {
                chargeCnt = Math.Min(MAX_CHARGE, chargeCnt + SimulationTime.Step);
                trgSize = (SIZE_MIN + chargeCnt * SIZE_RATIO) * 0.33f;
            }

            if (SimulationTime.Period(chargeSeCnt + 51, 52))
                SoundManager.playSe("charge.wav");
            chargeSeCnt += SimulationTime.Step;
        }
        else
        {
            pos.x = pos.x + (sin(deg) * SPEED * SimulationTime.Step);
            pos.y = pos.y + (cos(deg) * SPEED * SimulationTime.Step);
            range = range - (SPEED * SimulationTime.Step);
            if (range <= 0)
                remove();
            else if (range < 10)
                trgSize = trgSize * SimulationTime.Decay(0.75f);
        }

        size = size + ((trgSize - size) * SimulationTime.Blend(0.1f));
        shape.size = size;
        if (!(inCharge))
        {
            if (chargeShot)
                bullets.checkShotHit(pos, shape, this);
            enemies.checkShotHit(pos, shape, this);
        }

        if (SimulationTime.Emit && ((starShell) || (chargeCnt > MAX_CHARGE * CHARGE_RELEASE_RATIO)))
        {
            int pn = 1;
            if (chargeShot)
                pn = 3;
            for (int i = 0; i < pn; i++)
            {
                Particle pt = particles.getInstance();
                if ((pt != null))
                    pt.set_12(pos, 1, rand.nextSignedFloat(PI / 2) + PI, rand.nextSignedFloat(0.5f), 0.05f, 0.6f, 1, 0.8f, GameMath.integer(chargeCnt * 32 / MAX_CHARGE) + 4);
            }
        }

        cnt += SimulationTime.Step;
    }

    public void addScore(int sc, Vector pos)
    {
        ship.addScore(sc * multiplier);
        if (multiplier > 1)
        {
            FloatLetter fl = floatLetters.getInstanceForced();
            float size = 0.07f;
            if (sc >= 100)
                size = 0.2f;
            else if (sc >= 500)
                size = 0.4f;
            else if (sc >= 2000)
                size = 0.7f;
            size = size * ((1 + multiplier * 0.01f));
            fl.set_4("X" + multiplier.ToString(), pos, size * pos.y, GameMath.integer((30 + multiplier * 0.3f)));
        }

        if (chargeShot)
        {
            if (multiplier < MAX_MULTIPLIER)
                multiplier++;
        }
        else
        {
            remove();
        }
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        Vector3 sp = tunnel.getPos_1_Vector(pos);
        float[] parent1 = model;
        model = Transform.Translate(model, sp.x, sp.y, sp.z);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 1, 10);
        model = Transform.Rotate(model, cnt * 7, 0, 0, 1);
        shape.draw(model, tint, blend, cull, lineWidth);
        model = parent1;
    }

    public int damage
    {
        get
        {
            return _damage;
        }
    }
}

public class ShotPool : ActorPool<Shot>
{
    public ShotPool(int n, List<object> args) : base(n, args, () => new Shot())
    {
    }
}
