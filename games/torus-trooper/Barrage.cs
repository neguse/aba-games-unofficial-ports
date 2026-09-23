// Copyright 2004 Kenta Cho. Some rights reserved.
using System.Collections.Generic;

public class Barrage
{
    public static Rand rand = new Rand();
    public List<ParserParam> parserParam = new List<ParserParam>();
    public Drawable shape, disapShape;
    public bool longRange, noXReverse;
    public int prevWait, postWait;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public void setShape(Drawable shape, Drawable disapShape)
    {
        this.shape = shape;
        this.disapShape = disapShape;
    }

    public void setWait(int prevWait, int postWait)
    {
        this.prevWait = prevWait;
        this.postWait = postWait;
    }

    public void setLongRange(bool value)
    {
        longRange = value;
    }

    public void setNoXReverse()
    {
        noXReverse = true;
    }

    public void addBml_4(int parser, float rank, bool effect, float speed)
    {
        parserParam.Add(new ParserParam(parser, rank, effect ? 1 : 0, speed));
    }

    public void addBml_5(string directory, string name, float rank, bool effect, float speed)
    {
        addBml_4(BarrageManager.getInstance(directory, name), rank, effect, speed);
    }

    public BulletActor addTopBullet(BulletActorPool pool, BulletTarget target)
    {
        float xr = noXReverse ? 1 : rand.nextInt(2) * 2 - 1;
        return pool.addTopBullet(parserParam, 0, 0, GameMath.PI, 0, shape, disapShape, xr, 1, longRange, target, prevWait, postWait);
    }
}
