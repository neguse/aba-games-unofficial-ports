// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class BulletPool : ActorPool<Bullet>
{
    public BulletPool(int n) : base(n, null, () => new Bullet())
    {
    }

    public const float BULLET_REMOVED_RANGE = 2.0f;
    public override void move_0()
    {
        base.move_0();
        BulletState.move_0();
    }

    public virtual int removeAround(int cnt, Vector pos, ParticlePool particles, ParticlePool bonusParticles, Player player)
    {
        foreach (Bullet b in actors)
        {
            if (b.exists)
            {
                if (b.pos().dist_1(pos) < BULLET_REMOVED_RANGE)
                {
                    b.remove();
                    player.addScore_1(cnt);
                    cnt++;
                    int wc = 0;
                    if (cnt <= 50)
                        wc = cnt;
                    else
                        wc = 50 + GameMath.integer(sqrt((float)(cnt - 50)));
                    Particle bp = bonusParticles.getInstanceForced();
                    bp.set_13(ParticleShape.BONUS, b.state.pos.x, b.state.pos.y, 0, 0.2f, 0.5f, 1, 1, 1, 60, false, cnt, wc);
                    Particle p = particles.getInstanceForced();
                    p.set_13(ParticleShape.QUAD, b.state.pos.x, b.state.pos.y, b.state.deg, b.state.speed, 1.5f, 0.5f, 0.75f, 1.0f, 60, false);
                    cnt = this.removeAround(cnt, b.pos(), particles, bonusParticles, player);
                }
            }
        }

        return cnt;
    }
}

public class Bullet : Token<BulletState, BulletSpec>
{
    public override void init_1(List<object> args)
    {
        state = new BulletState();
    }

    public virtual void setWaitCnt(int c)
    {
        state.waitCnt = c;
    }
}

public class BulletState : TokenState
{
    public static int colorCnt = 0;
    public static float colorAlpha = 0;
    public Vector ppos;
    public Vector tailPos;
    public int cnt;
    public int waitCnt;
    public float speedRatio;
    public BulletState() : base()
    {
        ppos = new Vector();
        tailPos = new Vector();
    }

    public static void move_0()
    {
        colorCnt++;
        int c = colorCnt % 30;
        if (c < 15)
            colorAlpha = (float)c / 15;
        else
            colorAlpha = 1 - (float)(c - 15) / 15;
    }

    public override void clear()
    {
        {
            ppos.y = 0;
            ppos.x = ppos.y;
        }

        {
            tailPos.y = 0;
            tailPos.x = tailPos.y;
        }

        cnt = 0;
        waitCnt = 0;
        speedRatio = 0;
        base.clear();
    }
}

public class BulletSpec : TokenSpec<BulletState>
{
    public const float DISAPPEAR_CNT = 300;
    public Player player;
    public EnemyPool enemies;
    public ParticlePool particles;
    public Shape lineShape;
    public GameState gameState;
    public BulletSpec(Field field, Player player, EnemyPool enemies, ParticlePool particles, Shape shape, Shape lineShape, GameState gameState)
    {
        this.field = field;
        this.player = player;
        this.enemies = enemies;
        this.particles = particles;
        this.shape = shape;
        this.lineShape = lineShape;
        this.gameState = gameState;
    }

    public override void set_1(BulletState bs)
    {
        {
            bs.ppos.x = bs.pos.x;
            bs.ppos.y = bs.pos.y;
            bs.tailPos.x = bs.pos.x;
            bs.tailPos.y = bs.pos.y;
        }
    }

    public override bool move_1(BulletState bs)
    {
        {
            if (bs.waitCnt > 0)
            {
                bs.waitCnt--;
                return true;
            }

            bs.ppos.x = bs.pos.x;
            bs.ppos.y = bs.pos.y;
            float sp = bs.speed;
            if (((((gameState.mode_0() != GameStateMode.CLASSIC))) && ((bs.cnt < 40))))
                sp = sp * (((float)(bs.cnt + 10) / 50));
            bs.tailPos.x = bs.tailPos.x - (sin(bs.deg) * sp * 0.7f);
            bs.tailPos.y = bs.tailPos.y + (cos(bs.deg) * sp * 0.7f);
            bs.pos.x = bs.pos.x - (sin(bs.deg) * sp);
            bs.pos.y = bs.pos.y + (cos(bs.deg) * sp);
            field.addSlowdownRatio(bs.speed * 0.04f);
            bs.pos.x = field.normalizeX(bs.pos.x);
            if (!((field.containsOuter_1(bs.pos))))
                return false;
            if (((!((field.contains_1(bs.pos))))) || ((bs.cnt >= DISAPPEAR_CNT * 0.9f)))
            {
                bs.tailPos.x = bs.tailPos.x + ((bs.pos.x - bs.tailPos.x) * 0.1f);
                bs.tailPos.y = bs.tailPos.y + ((bs.pos.y - bs.tailPos.y) * 0.1f);
            }

            bs.tailPos.x = field.normalizeX(bs.tailPos.x);
            if (player.enemiesHasCollision())
                if (enemies.checkBulletHit(bs.pos, bs.ppos))
                    return false;
            if (player.checkBulletHit(bs.pos, bs.ppos))
                return false;
            bs.cnt++;
            if (bs.cnt >= DISAPPEAR_CNT)
                return false;
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, BulletState bs)
    {
        {
            if (bs.waitCnt > 0)
                return;
            Vector3 p = null;
            var part1 = new Mesh("Bullet-draw_1-1" + "-" + bs.meshKey);
            color = new float[] { 0.1f, 0.4f, 0.4f, 0.5f };
            p = field.calcCircularPos_1(bs.tailPos);
            part1.Vertex(p.x, p.y, p.z, color);
            color = new float[] { 0.2f * BulletState.colorAlpha, 0.8f * BulletState.colorAlpha, 0.8f * BulletState.colorAlpha, 1 };
            p = field.calcCircularPos_1(bs.pos);
            part1.Vertex(p.x, p.y, p.z, color);
            for (int vi = 0; vi + 1 < part1.vertexCount; vi += 2) part1.Line(vi, vi + 1);
            Gfx.Draw(part1.count, part1.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            p = field.calcCircularPos_1(bs.pos);
            float d = 0;
            switch (gameState.mode_0())
            {
                case GameStateMode.CLASSIC:
                    d = PI;
                    break;
                case GameStateMode.BASIC:
                case GameStateMode.MODERN:
                    d = bs.deg;
                    break;
            }

            float cd = field.calcCircularDeg(bs.pos.x);
            ((shape is BulletShapeBase ? (BulletShapeBase)shape : null)).draw_4(model, color, blend, p, cd, d, bs.cnt * 3.0f);
            color = new float[] { 0.6f * BulletState.colorAlpha, 0.9f * BulletState.colorAlpha, 0.9f * BulletState.colorAlpha, 1 };
            ((lineShape is BulletShapeBase ? (BulletShapeBase)lineShape : null)).draw_4(model, color, blend, p, cd, d, bs.cnt * 3.0f);
        }
    }
}
