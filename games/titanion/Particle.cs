// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class ParticlePool : ActorPool<Particle>
{
    public ParticlePool(int n, List<object> args) : base(n, args, () => new Particle())
    {
    }
}

public class Particle : Token<ParticleState, ParticleSpec>
{
    public TriangleParticleSpec triangleParticleSpec;
    public LineParticleSpec lineParticleSpec;
    public QuadParticleSpec quadParticleSpec;
    public BonusParticleSpec bonusParticleSpec;
    public override void init_1(List<object> args)
    {
        state = new ParticleState();
        triangleParticleSpec = (args[0] is TriangleParticleSpec ? (TriangleParticleSpec)args[0] : null);
        lineParticleSpec = (args[1] is LineParticleSpec ? (LineParticleSpec)args[1] : null);
        quadParticleSpec = (args[2] is QuadParticleSpec ? (QuadParticleSpec)args[2] : null);
        bonusParticleSpec = (args[3] is BonusParticleSpec ? (BonusParticleSpec)args[3] : null);
    }

    public virtual void set_13(int type, float x, float y, float deg, float speed, float sz, float r, float g, float b, int c = 60, bool ebg = true, float num = 0, int waitCnt = 0)
    {
        switch (type)
        {
            case ParticleShape.TRIANGLE:
                spec = triangleParticleSpec;
                break;
            case ParticleShape.LINE:
                spec = lineParticleSpec;
                break;
            case ParticleShape.QUAD:
                spec = quadParticleSpec;
                break;
            case ParticleShape.BONUS:
                spec = bonusParticleSpec;
                break;
        }

        this.spec = spec;
        base.set_4(x, y, deg, speed);
        state.size = sz;
        state.vel.x = -sin(deg) * speed;
        state.vel.y = cos(deg) * speed;
        state.r = r;
        state.g = g;
        state.b = b;
        {
            state.startCnt = c;
            state.cnt = state.startCnt;
        }

        state.effectedByGravity = ebg;
        state.trgNum = num;
        state.waitCnt = waitCnt;
        if (type == ParticleShape.BONUS)
            ((spec is BonusParticleSpec ? (BonusParticleSpec)spec : null)).setSize(state, sz);
    }

    public virtual void setByVelocity(float x, float y, float vx, float vy, float sz, float r, float g, float b, float a, int c = 60, bool ebg = true)
    {
        spec = triangleParticleSpec;
        base.set_4(x, y, 0, 0);
        state.vel.x = vx;
        state.vel.y = vy;
        state.size = sz;
        state.r = r;
        state.g = g;
        state.b = b;
        state.a = a;
        {
            state.startCnt = c;
            state.cnt = state.startCnt;
        }

        state.effectedByGravity = ebg;
    }
}

public class ParticleState : TokenState
{
    public Vector vel;
    public Vector tailPos;
    public float size;
    public int cnt, startCnt;
    public float r, g, b, a;
    public float d1, d2;
    public float vd1, vd2;
    public bool effectedByGravity;
    public float num, trgNum;
    public float trgSize;
    public int waitCnt;
    public ParticleState() : base()
    {
        vel = new Vector();
        tailPos = new Vector();
    }

    public override void clear()
    {
        {
            vel.y = 0;
            vel.x = vel.y;
        }

        size = 1;
        cnt = 0;
        {
            {
                b = 0;
                g = b;
            }

            r = g;
        }

        a = 1;
        {
            d2 = 0;
            d1 = d2;
        }

        {
            vd2 = 0;
            vd1 = vd2;
        }

        effectedByGravity = false;
        {
            trgNum = 1;
            num = trgNum;
        }

        trgSize = 1;
        waitCnt = 0;
        base.clear();
    }
}

public class ParticleSpec : TokenSpec<ParticleState>
{
    public static TitanionRand rand = new TitanionRand();
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public Player player;
    public virtual void setPlayer(Player player)
    {
        this.player = player;
    }

    public virtual float calcNearPlayerAlpha(Vector pos)
    {
        if ((!((player.isActive()))))
            return 1;
        float pd = player.pos().dist_1(pos);
        if (pd < 20)
            return pd / 20;
        else
            return 1;
    }
}

public class TriangleParticleSpec : ParticleSpec
{
    public const float SLOW_DOWN_RATIO = 0.05f;
    public const float GRAVITY = 0.003f;
    public Shape particleShape;
    public ParticlePool particles;
    public TriangleParticleSpec(Field field)
    {
        this.field = field;
        particleShape = new TriangleParticleShape();
    }

    public virtual void setParticles(ParticlePool particles)
    {
        this.particles = particles;
    }

    public override void set_1(ParticleState ps)
    {
        {
            ps.d1 = rand.nextFloat(PI * 2);
            ps.d2 = rand.nextFloat(PI * 2);
            ps.vd1 = rand.nextSignedFloat(0.1f);
            ps.vd2 = rand.nextSignedFloat(0.1f);
        }
    }

    public override bool move_1(ParticleState ps)
    {
        {
            ps.pos.opAddAssign(ps.vel);
            ps.pos.x = field.normalizeX(ps.pos.x);
            if (ps.effectedByGravity)
                ps.vel.y = ps.vel.y - (GRAVITY);
            ps.vel.opMulAssign((1 - SLOW_DOWN_RATIO));
            ps.d1 = ps.d1 + (ps.vd1);
            ps.d2 = ps.d2 + (ps.vd2);
            ps.vd1 = ps.vd1 * ((1 - SLOW_DOWN_RATIO * 0.2f));
            ps.vd2 = ps.vd2 * ((1 - SLOW_DOWN_RATIO * 0.2f));
            float cfr = 1.0f - (1.0f / (float)ps.startCnt);
            if (cfr < 0)
                cfr = 0;
            ps.r = ps.r * (cfr);
            ps.g = ps.g * (cfr);
            ps.b = ps.b * (cfr);
            ps.a = ps.a * (cfr);
            float fs = 0;
            if (((ps.size > 2.0f)) && ((rand.nextInt(45) == 0)))
                fs = 0.5f - rand.nextFloat(0.2f);
            else if (((ps.size > 0.5f)) && ((rand.nextInt(10) == 0)))
                fs = 0.1f + rand.nextSignedFloat(0.05f);
            if (fs > 0)
            {
                float vx = ps.vel.x * rand.nextSignedFloat(0.8f);
                float vy = ps.vel.y * rand.nextSignedFloat(0.8f);
                ps.vel.x = ps.vel.x - (vx * fs);
                ps.vel.y = ps.vel.y - (vy * fs);
                float cr = 1 - fs * 0.2f;
                ps.vel.opDivAssign(cr);
                Particle p = particles.getInstanceForced();
                int nc = GameMath.integer((ps.cnt * (0.8f + fs * 0.2f)));
                if (nc > 0)
                    p.setByVelocity(ps.pos.x, ps.pos.y, vx, vy, ps.size * fs, ps.r, ps.g, ps.b, ps.a, nc, ps.effectedByGravity);
                ps.size = ps.size * ((1 - fs));
                ps.cnt = GameMath.integer(ps.cnt * (cr));
            }

            ps.cnt--;
            if (ps.cnt <= 0)
                return false;
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, ParticleState ps)
    {
        {
            Vector3 p = field.calcCircularPos_1(ps.pos);
            float aa = ps.a * calcNearPlayerAlpha(ps.pos);
            color = new float[] { ps.r, ps.g, ps.b, aa };
            particleShape.draw_3(model, color, blend, p, ps.d1, ps.d2);
        }
    }
}

public class LineParticleSpec : ParticleSpec
{
    public const float SLOW_DOWN_RATIO = 0.03f;
    public LineParticleSpec(Field field)
    {
        this.field = field;
    }

    public override void set_1(ParticleState ps)
    {
        {
            ps.tailPos.x = ps.pos.x;
            ps.tailPos.y = ps.pos.y;
        }
    }

    public override bool move_1(ParticleState ps)
    {
        {
            ps.stepForward();
            ps.tailPos.x = ps.tailPos.x + ((ps.pos.x - ps.tailPos.x) * 0.05f);
            ps.tailPos.y = ps.tailPos.y + ((ps.pos.y - ps.tailPos.y) * 0.05f);
            ps.speed = ps.speed * ((1 - SLOW_DOWN_RATIO));
            ps.pos.x = field.normalizeX(ps.pos.x);
            float cfr = 1.0f - (1.0f / (float)ps.startCnt);
            if (cfr < 0)
                cfr = 0;
            ps.r = ps.r * (cfr);
            ps.g = ps.g * (cfr);
            ps.b = ps.b * (cfr);
            ps.a = ps.a * (cfr);
            ps.cnt--;
            if (ps.cnt <= 0)
                return false;
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, ParticleState ps)
    {
        {
            Vector3 p = null;
            var part1 = new Mesh("Particle-draw_1-1" + "-" + ps.meshKey);
            float aa = ps.a;
            color = new float[] { ps.r, ps.g, ps.b, aa };
            p = field.calcCircularPos_1(ps.pos);
            part1.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_1(ps.tailPos);
            part1.Vertex(p.x, p.y, p.z, color);
            for (int vi = 0; vi + 1 < part1.vertexCount; vi += 2) part1.Line(vi, vi + 1);
            Gfx.Draw(part1.count, part1.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        }
    }
}

public class QuadParticleSpec : ParticleSpec
{
    public const float SLOW_DOWN_RATIO = 0.07f;
    public const float GRAVITY = 0.002f;
    public QuadParticleSpec(Field field)
    {
        this.field = field;
    }

    public override bool move_1(ParticleState ps)
    {
        {
            ps.pos.opAddAssign(ps.vel);
            ps.pos.x = field.normalizeX(ps.pos.x);
            if (ps.effectedByGravity)
                ps.vel.y = ps.vel.y - (GRAVITY);
            ps.vel.opMulAssign((1 - SLOW_DOWN_RATIO));
            float cfr = 1.0f - (1.0f / (float)ps.startCnt);
            if (cfr < 0)
                cfr = 0;
            ps.r = ps.r * (cfr);
            ps.g = ps.g * (cfr);
            ps.b = ps.b * (cfr);
            ps.a = ps.a * (cfr);
            ps.size = ps.size * ((1 - (1 - cfr) * 0.5f));
            ps.cnt--;
            if (ps.cnt <= 0)
                return false;
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, ParticleState ps)
    {
        {
            Vector3 p = null;
            float sz = ps.size * 0.5f;
            float aa = ps.a * calcNearPlayerAlpha(ps.pos);
            color = new float[] { ps.r, ps.g, ps.b, aa };
            var part1 = new Mesh("Particle-draw_1-1" + "-" + ps.meshKey);
            p = field.calcCircularPos_2(ps.pos.x - sz, ps.pos.y - sz);
            part1.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x + sz, ps.pos.y - sz);
            part1.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x + sz, ps.pos.y + sz);
            part1.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x - sz, ps.pos.y + sz);
            part1.Vertex(p.x, p.y, p.z, color);
            part1.Quads(0, part1.vertexCount);
            Gfx.Draw(part1.count, part1.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            blend = Gfx.Blend.Alpha;
            color = new float[] { 0, 0, 0, aa * 0.66f };
            var part2 = new Mesh("Particle-draw_1-2" + "-" + ps.meshKey);
            p = field.calcCircularPos_2(ps.pos.x - sz, ps.pos.y - sz);
            part2.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x + sz, ps.pos.y - sz);
            part2.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x + sz, ps.pos.y + sz);
            part2.Vertex(p.x, p.y, p.z, color);
            p = field.calcCircularPos_2(ps.pos.x - sz, ps.pos.y + sz);
            part2.Vertex(p.x, p.y, p.z, color);
            part2.LineStrip(0, part2.vertexCount, true);
            Gfx.Draw(part2.count, part2.Bindings(model, null, 1, blend == Gfx.Blend.Additive), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            blend = Gfx.Blend.Additive;
        }
    }
}

public class BonusParticleSpec : ParticleSpec
{
    public const float SLOW_DOWN_RATIO = 0.04f;
    public BonusParticleSpec(Field field)
    {
        this.field = field;
    }

    public virtual void setSize(ParticleState ps, float sz)
    {
        {
            ps.trgSize = sz;
            ps.size = 0.1f;
        }
    }

    public override bool move_1(ParticleState ps)
    {
        {
            if (ps.waitCnt > 0)
            {
                ps.waitCnt--;
                return true;
            }

            ps.stepForward();
            ps.speed = ps.speed * ((1 - SLOW_DOWN_RATIO));
            field.addSlowdownRatio(0.01f);
            ps.pos.x = field.normalizeX(ps.pos.x);
            float cfr = 1.0f - (1.0f / (float)ps.startCnt);
            if (cfr < 0)
                cfr = 0;
            ps.a = ps.a * (cfr);
            ps.num = ps.num + ((ps.trgNum - ps.num) * 0.2f);
            if (fabs(ps.trgNum - ps.num) < 0.5f)
                ps.num = ps.trgNum;
            ps.size = ps.size + ((ps.trgSize - ps.size) * 0.1f);
            ps.cnt--;
            if (ps.cnt <= 0)
                return false;
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, ParticleState ps)
    {
        {
            if (ps.waitCnt > 0)
                return;
            float[] parent1 = model;
            Vector3 p = field.calcCircularPos_1(ps.pos);
            float aa = ps.a * calcNearPlayerAlpha(ps.pos);
            blend = Gfx.Blend.Alpha;
            color = new float[] { 1, 1, 1, aa * 0.5f };
            model = Transform.Translate(model, p.x, p.y, p.z);
            Letter.drawNumSign(model, color, blend, GameMath.integer(ps.num), 0, 0, ps.size, 33, 0, 1);
            color = new float[] { 1, 1, 1, aa };
            Letter.drawNumSign(model, color, blend, GameMath.integer(ps.num), 0, 0, ps.size, 33, 0, 2);
            blend = Gfx.Blend.Additive;
            model = parent1;
        }
    }
}

public static class ParticleShape
{
    public const int TRIANGLE = 0;
    public const int LINE = 1;
    public const int QUAD = 2;
    public const int BONUS = 3;
}
