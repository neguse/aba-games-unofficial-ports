// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Enemy : Actor
{
    public const float OUT_OF_COURSE_BANK = 1.0f;
    public const float DISAP_DEPTH = -5.0f;
    public static Rand rand = new Rand();
    public Tunnel tunnel;
    public BulletActorPool bullets;
    public Ship ship;
    public ParticlePool particles;
    public ShipSpec spec;
    public Vector pos;
    public Vector ppos;
    public Vector flipMv;
    public int flipMvCnt;
    public float speed;
    public float d1, d2;
    public float baseBank;
    public int cnt;
    public float bank;
    public BulletActor topBullet;
    public int shield, firstShield;
    public bool damaged;
    public bool highOrder;
    public float limitY;
    public List<BulletActor> bitBullet = new List<BulletActor>();
    public int bitCnt;
    public Vector bitOffset;
    public bool passed;
    public EnemyPool passedEnemies;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public override void init_1(List<object> args)
    {
        tunnel = (Tunnel)args[0];
        bullets = (BulletActorPool)args[1];
        ship = (Ship)args[2];
        particles = (ParticlePool)args[3];
        pos = new Vector();
        ppos = new Vector();
        flipMv = new Vector();
        bitOffset = new Vector();
    }

    public void setPassedEnemies(EnemyPool pe)
    {
        passedEnemies = pe;
    }

    public void set_6(ShipSpec spec, float x, float y, Rand rand, bool ps = false, float baseBank = 0)
    {
        this.spec = spec;
        pos.x = x;
        {
            pos.y = y;
            limitY = pos.y;
        }

        speed = 0;
        {
            d2 = 0;
            d1 = d2;
        }

        cnt = 0;
        bank = 0;
        {
            shield = spec.shield;
            firstShield = shield;
        }

        if (!(ps))
            this.baseBank = spec.createBaseBank(rand);
        else
            this.baseBank = baseBank;
        flipMvCnt = 0;
        damaged = false;
        highOrder = true;
        topBullet = null;
        bitBullet.Clear();
        passed = ps;
        exists = true;
    }

    public override void move()
    {
        if (!(passed))
        {
            if (highOrder)
            {
                if (pos.y <= ship.relPos.y)
                {
                    ship.rankUp(spec.isBoss);
                    highOrder = false;
                }
            }
            else
            {
                if (pos.y > ship.relPos.y)
                {
                    ship.rankDown();
                    highOrder = true;
                }
            }
        }

        ppos.x = pos.x;
        ppos.y = pos.y;
        if (ship.isBossModeEnd)
        {
            speed = speed + ((0 - speed) * 0.05f);
            flipMvCnt = 0;
        }
        else if (!(ship.hasCollision()))
        {
            speed = speed + ((1.5f - speed) * 0.15f);
        }

        if (spec.hasLimitY)
            speed = spec.setSpeed_2(speed, ship.speed);
        else if ((pos.y > 5) && (pos.y < Ship.IN_SIGHT_DEPTH_DEFAULT * 2))
            speed = spec.setSpeed_2(speed, ship.speed);
        else
            speed = spec.setSpeed_1(speed);
        float my = speed - ship.speed;
        if ((passed) && (my > 0))
            my = 0;
        pos.y = pos.y + (my);
        if (!(passed))
            if (spec.hasLimitY)
                limitY = spec.handleLimitY(pos, limitY);
        Vector range = new Vector();
        bool steer = false;
        if (spec.getRangeOfMovement(range, pos, tunnel))
        {
            int cdf = Tunnel.checkDegInside(pos.x, range.x, range.y);
            if (cdf != 0)
            {
                steer = true;
                if (cdf == -1)
                    bank = spec.tryToMove(bank, pos.x, range.x);
                else if (cdf == 1)
                    bank = spec.tryToMove(bank, pos.x, range.y);
            }
        }

        if (!(steer))
        {
            if (spec.aimShip)
            {
                float ox = fabs(pos.x - ship.pos.x);
                if (ox > PI)
                    ox = PI * 2 - ox;
                if (ox > PI / 3)
                {
                    steer = true;
                    bank = spec.tryToMove(bank, pos.x, ship.pos.x);
                }
            }
        }

        if (!(steer))
        {
            bank = bank + ((baseBank - bank) * 0.2f);
        }

        bank = bank * (0.9f);
        pos.x = pos.x + (bank * 0.08f * (SliceState.DEFAULT_RAD / tunnel.getRadius(pos.y)));
        if (flipMvCnt > 0)
        {
            flipMvCnt--;
            pos.opAddAssign(flipMv);
            flipMv.opMulAssign(0.95f);
        }

        if (pos.x < 0)
            pos.x = pos.x + (PI * 2);
        else if (pos.x >= PI * 2)
            pos.x = pos.x - (PI * 2);
        if (((!(passed)) && (flipMvCnt <= 0)) && (!(ship.isBossModeEnd)))
        {
            float ax = fabs(pos.x - ship.relPos.x);
            if (ax > PI)
                ax = PI * 2 - ax;
            ax = ax * ((tunnel.getRadius(0) / SliceState.DEFAULT_RAD));
            ax = ax * (3);
            float ay = fabs(pos.y - ship.relPos.y);
            if ((ship.hasCollision()) && (spec.shape.checkCollision(ax, ay, ship.shape, ship.speed)))
            {
                float ox = ppos.x - ship.pos.x;
                if (ox > PI)
                    ox = ox - (PI * 2);
                else if (ox < -PI)
                    ox = ox + (PI * 2);
                float oy = ppos.y;
                float od = atan2(ox, oy);
                flipMvCnt = 48;
                flipMv.x = sin(od) * ship.speed * 0.4f;
                flipMv.y = cos(od) * ship.speed * 7;
            }
        }

        Slice sl = tunnel.getSlice(pos.y);
        float co = tunnel.checkInCourse(pos);
        if (co != 0)
        {
            float bm = (-OUT_OF_COURSE_BANK * co - bank) * 0.075f;
            if (bm > 1)
                bm = 1;
            else if (bm < -1)
                bm = -1;
            speed = speed * ((1 - fabs(bm)));
            bank = bank + (bm);
            float lo = fabs(pos.x - sl.getLeftEdgeDeg());
            if (lo > PI)
                lo = PI * 2 - lo;
            float ro = fabs(pos.x - sl.getRightEdgeDeg());
            if (ro > PI)
                ro = PI * 2 - ro;
            if (lo > ro)
                pos.x = sl.getRightEdgeDeg();
            else
                pos.x = sl.getLeftEdgeDeg();
        }

        d1 = d1 + ((sl.d1 - d1) * 0.1f);
        d2 = d2 + ((sl.d2 - d2) * 0.1f);
        if ((!(passed)) && (!((topBullet != null))))
        {
            Barrage tbb = spec.barrage;
            topBullet = tbb.addTopBullet(bullets, ship);
            for (int i = 0; i < spec.bitNum; i++)
            {
                Barrage bbb = spec.bitBarrage;
                BulletActor ba = bbb.addTopBullet(bullets, ship);
                if ((ba != null))
                {
                    ba.unsetAimTop();
                    bitBullet.Add(ba);
                }
            }
        }

        if ((topBullet != null))
        {
            topBullet.bullet.pos.x = pos.x;
            topBullet.bullet.pos.y = pos.y;
            checkBulletInRange(topBullet);
            float d = 0;
            int i = 0;
            if ((bitBullet != null && bitBullet.Count > 0))
            {
                foreach (BulletActor bb in bitBullet)
                {
                    d = spec.getBitOffset(bitOffset, d, i, bitCnt);
                    bb.bullet.pos.x = bitOffset.x + pos.x;
                    bb.bullet.pos.y = bitOffset.y + pos.y;
                    bb.bullet.deg = d;
                    checkBulletInRange(bb);
                    i++;
                }
            }
        }

        if ((!(passed)) && (pos.y <= ship.inSightDepth))
            spec.shape.addParticles(pos, particles);
        if (!(passed))
        {
            if ((((!(spec.hasLimitY)) && (pos.y > Ship.IN_SIGHT_DEPTH_DEFAULT * 5))) || (pos.y < DISAP_DEPTH))
            {
                if ((Ship.replayMode) && (pos.y < DISAP_DEPTH))
                {
                    Enemy en = passedEnemies.getInstance();
                    if ((en != null))
                        en.set_6(spec, pos.x, pos.y, null, true, baseBank);
                }

                remove();
            }
        }
        else
        {
            if (pos.y < -Ship.IN_SIGHT_DEPTH_DEFAULT * 3)
                remove();
        }

        damaged = false;
        bitCnt++;
    }

    public void checkBulletInRange(BulletActor ba)
    {
        if (!(tunnel.checkInScreen_2(pos, ship)))
        {
            topBullet.rootRank = 0;
        }
        else
        {
            if (((pos.dist(ship.relPos) > 20 + ship.relPos.y * 10 / Ship.RELPOS_MAX_Y) && (pos.y > ship.relPos.y)) && (flipMvCnt <= 0))
            {
                if (spec.noFireDepthLimit)
                    topBullet.rootRank = 1;
                else if (pos.y <= ship.inSightDepth)
                    topBullet.rootRank = 1;
                else
                    topBullet.rootRank = 0;
            }
            else
            {
                topBullet.rootRank = 0;
            }
        }
    }

    public void checkShotHit(Vector p, Collidable shape, Shot shot)
    {
        float ox = fabs(pos.x - p.x), oy = fabs(pos.y - p.y);
        if (ox > PI)
            ox = PI * 2 - ox;
        ox = ox * ((tunnel.getRadius(pos.y) / SliceState.DEFAULT_RAD));
        ox = ox * (3);
        if (spec.shape.checkCollision(ox, oy, shape))
        {
            shield = shield - (shot.damage);
            if (shield <= 0)
            {
                destroyed();
            }
            else
            {
                damaged = true;
                Particle pt = null;
                for (int i = 0; i < 4; i++)
                {
                    pt = particles.getInstance();
                    if ((pt != null))
                        pt.set_12(pos, 1, rand.nextSignedFloat(0.1f), rand.nextSignedFloat(1.6f), 0.75f, 1, 0.4f + rand.nextFloat(0.4f), 0.3f);
                    pt = particles.getInstance();
                    if ((pt != null))
                        pt.set_12(pos, 1, rand.nextSignedFloat(0.1f) + PI, rand.nextSignedFloat(1.6f), 0.75f, 1, 0.4f + rand.nextFloat(0.4f), 0.3f);
                }

                SoundManager.playSe("hit.wav");
            }

            shot.addScore(spec.score, pos);
        }
    }

    public void destroyed()
    {
        for (int i = 0; i < 30; i++)
        {
            Particle pt = particles.getInstance();
            if (!((pt != null)))
                break;
            pt.set_12(pos, 1, rand.nextFloat(PI * 2), rand.nextSignedFloat(1), 0.01f + rand.nextFloat(0.1f), 1, 0.2f + rand.nextFloat(0.8f), 0.4f, 24);
        }

        spec.shape.addFragments(pos, particles);
        ship.rankUp(spec.isBoss);
        if (firstShield == 1)
        {
            SoundManager.playSe("small_dest.wav");
        }
        else if (firstShield < 20)
        {
            SoundManager.playSe("middle_dest.wav");
        }
        else
        {
            SoundManager.playSe("boss_dest.wav");
            ship.setScreenShake(56, 0.064f);
        }

        remove();
    }

    public void remove()
    {
        if ((topBullet != null))
        {
            topBullet.removeForced();
            topBullet = null;
            if ((bitBullet != null && bitBullet.Count > 0))
            {
                foreach (BulletActor bb in bitBullet)
                {
                    bb.removeForced();
                }

                bitBullet.Clear();
            }
        }

        exists = false;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        Vector3 sp = tunnel.getPos_1_Vector(pos);
        float[] parent1 = model;
        model = Transform.Translate(model, sp.x, sp.y, sp.z);
        model = Transform.Rotate(model, (pos.x - bank) * 180 / PI, 0, 0, 1);
        if (sp.z > 200)
        {
            float sz = 1 - (sp.z - 200) * 0.0025f;
            model = Transform.Scale(model, sz, sz, sz);
        }

        model = Transform.Rotate(model, d1 * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, d2 * 180 / PI, 1, 0, 0);
        if (!(damaged))
            spec.shape.draw(model, tint, blend, cull, lineWidth);
        else
            spec.damagedShape.draw(model, tint, blend, cull, lineWidth);
        model = parent1;
        if ((bitBullet != null && bitBullet.Count > 0))
        {
            foreach (BulletActor bb in bitBullet)
            {
                sp = tunnel.getPos_1_Vector(bb.bullet.pos);
                float[] parent2 = model;
                model = Transform.Translate(model, sp.x, sp.y, sp.z);
                model = Transform.Rotate(model, bitCnt * 7, 0, 1, 0);
                model = Transform.Rotate(model, pos.x * 180 / PI, 0, 0, 1);
                ShipSpec.bitShape().draw(model, tint, blend, cull, lineWidth);
                model = parent2;
            }
        }
    }
}

public class EnemyPool : ActorPool<Enemy>
{
    public EnemyPool(int n, List<object> args) : base(n, args, () => new Enemy())
    {
    }

    public void checkShotHit(Vector pos, Collidable shape, Shot shot)
    {
        foreach (Enemy e in actor)
            if (e.exists)
                e.checkShotHit(pos, shape, shot);
    }

    public int getNum()
    {
        int num = 0;
        foreach (Enemy e in actor)
            if (e.exists)
                num++;
        return num;
    }

    public void setPassedEnemies(EnemyPool pe)
    {
        foreach (Enemy e in actor)
            e.setPassedEnemies(pe);
    }

    public override void clear()
    {
        foreach (Enemy e in actor)
            if (e.exists)
                e.remove();
        base.clear();
    }
}
