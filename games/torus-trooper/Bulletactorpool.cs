// Copyright 2004 Kenta Cho. Some rights reserved.
using System.Collections.Generic;

public class BulletActorPool : ActorPool<BulletActor>
{
    public float cnt;
    public TtWorld world;
    public BulletActorPool(int count, List<object> args) : base(count, args, () => new BulletActor())
    {
        Bullet.manager = this;
        world = new TtWorld(this);
    }

    public void addSimpleBullet(float deg, float speed)
    {
        var parent = (BulletImpl)Bullet.now;
        if ((parent.rootBullet != null) && (parent.rootBullet.rootRank <= 0))
            return;
        var actor = getInstance();
        if (actor == null)
            return;
        actor.bullet.setParam(parent);
        if (actor.bullet.gotoNextParser())
        {
            actor.set_5(Bullet.createRunner(actor.bullet.getParser()), parent.pos.x, parent.pos.y, deg, speed);
            actor.setMorphSeed();
        }
        else
            actor.set_4(parent.pos.x, parent.pos.y, deg, speed);
    }

    public void addScriptBullet(PatternState state, float deg, float speed)
    {
        var parent = (BulletImpl)Bullet.now;
        if ((parent.rootBullet != null) && (parent.rootBullet.rootRank <= 0))
            return;
        var actor = getInstance();
        if (actor == null)
            return;
        actor.bullet.setParam(parent);
        actor.set_5(new PatternState[] { state }, parent.pos.x, parent.pos.y, deg, speed);
    }

    public BulletActor addTopBullet(List<ParserParam> parsers, float x, float y, float deg, float speed, Drawable shape, Drawable disapShape, float xr, float yr, bool longRange, BulletTarget target, int prevWait, int postWait)
    {
        var actor = getInstance();
        if (actor == null)
            return null;
        actor.bullet.setParamFirst(parsers, shape, disapShape, xr, yr, longRange, target, actor);
        actor.set_5(Bullet.createRunner(actor.bullet.getParser()), x, y, deg, speed);
        actor.setWait(prevWait, postWait);
        actor.setTop();
        return actor;
    }

    public override void move()
    {
        base.move();
        cnt += SimulationTime.Step;
    }

    public override void clear()
    {
        foreach (var item in actor)
            if (item.exists)
                item.removeForced();
        actorIdx = 0;
        cnt = 0;
    }

    public void clearVisible()
    {
        foreach (var item in actor)
            if (item.exists)
                item.startDisappear();
    }

    public void checkShotHit(Vector pos, Collidable shape, Shot shot)
    {
        foreach (var item in actor)
            if (item.exists)
                item.checkShotHit(pos, shape, shot);
    }
}
