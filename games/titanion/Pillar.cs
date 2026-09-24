// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class PillarPool : ActorPool<Pillar>
{
    public PillarPool(int n) : base(n, null, () => new Pillar())
    {
    }

    public virtual void setEnd()
    {
        foreach (Pillar a in actors)
            if (a.exists)
                a.setEnd();
    }

    public virtual void drawCenter(float[] model, float[] color, Gfx.Blend blend)
    {
        for (int i = 1; i < actors.Length; i++)
        {
            Pillar value = actors[i];
            int j = i - 1;
            while ((j >= 0) && (actors[j].opCmp(value) > 0))
            {
                actors[j + 1] = actors[j];
                j--;
            }

            actors[j + 1] = value;
        }

        Pillar[] sas = actors;
        foreach (Pillar a in sas)
            if (((a.exists)) && ((!((a.state.isOutside)))))
                a.draw_0(model, color, blend);
    }

    public virtual void drawOutside(float[] model, float[] color, Gfx.Blend blend)
    {
        foreach (Pillar a in actors)
            if (((a.exists)) && ((a.state.isOutside)))
                a.draw_0(model, color, blend);
    }
}

public class Pillar : Token<PillarState, PillarSpec>
{
    public override void init_1(List<object> args)
    {
        state = new PillarState();
    }

    public virtual void set_7(PillarSpec ps, float y, float maxY, Pillar pp, PillarShape s, float vdeg, bool outside = false)
    {
        base.set_5(ps, 0, y, 0, 0);
        state.maxY = maxY;
        state.previousPillar = pp;
        state.pshape = s;
        state.vdeg = vdeg;
        state.isOutside = outside;
    }

    public virtual void setEnd()
    {
        state.isEnded = true;
    }

    public virtual int opCmp(object o)
    {
        Pillar p = (o is Pillar ? (Pillar)o : null);
        if ((!(((p) != null))))
            return 0;
        return GameMath.integer((fabs(p.pos().y) - fabs(pos().y)));
    }
}

public class PillarState : TokenState
{
    public Pillar previousPillar;
    public float vy, vdeg;
    public float maxY;
    public PillarShape pshape;
    public bool isEnded;
    public bool isOutside;
    public override void clear()
    {
        previousPillar = null;
        vy = 0;
        vdeg = 0;
        maxY = 0;
        isEnded = false;
        isOutside = false;
        base.clear();
    }
}

public class PillarSpec : TokenSpec<PillarState>
{
    public const float VELOCITY_Y = 0.025f;
    public PillarSpec(Field field)
    {
        this.field = field;
    }

    public override bool move_1(PillarState ps)
    {
        {
            if (!((ps.isOutside)))
            {
                ps.vy = ps.vy + (VELOCITY_Y);
                ps.vy = ps.vy * (0.98f);
                ps.pos.y = ps.pos.y + (ps.vy);
                if (ps.vy > 0)
                {
                    float ty = 0;
                    if (((((ps.previousPillar) != null)) && ((ps.previousPillar.exists))))
                        ty = ps.previousPillar.pos().y - PillarShape.TICKNESS;
                    else
                        ty = ps.maxY;
                    ty = ty - (PillarShape.TICKNESS);
                    if (((!((ps.isEnded)))) && ((ps.pos.y > ty)))
                    {
                        ps.vy = ps.vy * (-0.5f);
                        ps.pos.y = ps.pos.y + ((ty - ps.pos.y) * 0.5f);
                        if ((ps.previousPillar) != null)
                            ps.previousPillar.state.vy = ps.previousPillar.state.vy - (ps.vy * 0.5f);
                    }

                    if (ps.pos.y > 100)
                        return false;
                }
            }
            else
            {
                ps.pos.y = ps.pos.y - (0.2f);
                if (ps.pos.y < -50)
                    return false;
            }

            ps.deg = ps.deg + (ps.vdeg);
            return true;
        }
    }

    public override void draw_1(float[] model, float[] color, Gfx.Blend blend, PillarState ps)
    {
        ps.pshape.draw_2(model, color, blend, ps.pos.y, ps.deg);
    }
}
