// Copyright 2006 Kenta Cho. Some rights reserved.
using System.Collections.Generic;
using static Lub;

public class BulletPool : ActorPool<BulletActor>
{
    public int cnt;
    public McdPatternWorld world;
    public SimpleBulletPool simpleBullets;
    public BulletPool(int n, int sn, object[] args) : base(n, args, () => new BulletActor())
    {
        Bullet.manager = this;
        world = new McdPatternWorld(this);
        simpleBullets = new SimpleBulletPool(sn, args);
        simpleBullets.init_1_World((World)args[4]);
    }

    public void addSimpleBullet(float deg, float speed)
    {
        var parent = (BulletImpl)Bullet.now;
        if (parent.rootBullet != null && !(parent.rootBullet.activated))
            return;
        var ba = getInstance();
        if (ba == null)
            return;
        ba.bullet.setParam(parent);
        if (ba.bullet.gotoNextParser())
        {
            ba.set_5__Single_Single_Single_Single(Bullet.createRunner(ba.bullet.getParser()), parent.pos.x, parent.pos.y, deg, speed);
            ba.setMorphSeed();
        }
        else
        {
            var sb = simpleBullets.getInstance();
            if (sb != null)
                sb.set_4_Single_Single_Single_Single(parent.pos.x, parent.pos.y, deg, speed * ba.bullet.getSpeedRank());
        }
    }

    public void addScriptBullet(PatternState state, float deg, float speed)
    {
        var parent = (BulletImpl)Bullet.now;
        if (parent.rootBullet != null && !(parent.rootBullet.activated))
            return;
        var ba = getInstance();
        if (ba == null)
            return;
        ba.bullet.setParam(parent);
        ba.set_5__Single_Single_Single_Single(new PatternState[] { state }, parent.pos.x, parent.pos.y, deg, speed);
    }

    public BulletActor addTopBullet_10(List<ParserParam> parsers, float x, float y, float deg, float speed, float xr, float yr, BulletTarget target, int prevWait, int postWait)
    {
        var ba = getInstance();
        if (ba == null)
            return null;
        ba.bullet.setParamFirst(parsers, xr, yr, target, ba);
        ba.set_5__Single_Single_Single_Single(Bullet.createRunner(ba.bullet.getParser()), x, y, deg, speed);
        ba.setWait(prevWait, postWait);
        ba.setTop();
        return ba;
    }

    public override void move_0()
    {
        simpleBullets.move_0();
        base.move_0();
        cnt++;
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        simpleBullets.draw(model, tint, blend, key + "-draw-1");
    }

    public void drawShadow_0(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        blend = Gfx.Blend.Alpha;
        simpleBullets.drawShadow_0(model, tint, blend, key + "-drawShadow_0-1");
        blend = Gfx.Blend.Additive;
    }

    public void drawSpectrum(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        simpleBullets.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1");
    }

    public override void clear()
    {
        foreach (var a in actor)
            if (a.exists)
                a.removeForced();
        actorIdx = 0;
        cnt = 0;
        simpleBullets.clear();
    }

    public int collapseIntoParticle()
    {
        return simpleBullets.collapseIntoParticle();
    }

    public void slowdown()
    {
        simpleBullets.slowdown();
    }
}

public class SimpleBulletPool : OdeActorPool<SimpleBullet>
{
    public SimpleBulletPool(int n, object[] args) : base(n, args, () => new SimpleBullet())
    {
    }

    public void drawShadow_0(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        foreach (var a in actor)
            if (a.exists)
                a.drawShadow_0(model, tint, blend, key + "-drawShadow_0-1" + "-" + a.meshKey);
    }

    public void drawSpectrum(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        foreach (var a in actor)
            if (a.exists)
                a.drawSpectrum(model, tint, blend, key + "-drawSpectrum-1" + "-" + a.meshKey);
    }

    public int collapseIntoParticle()
    {
        int n = 0;
        foreach (var a in actor)
            if (a.exists)
            {
                a.collapseIntoParticle();
                n++;
            }

        return n;
    }

    public void slowdown()
    {
        foreach (var a in actor)
            if (a.exists)
                a.slowdown();
    }
}
