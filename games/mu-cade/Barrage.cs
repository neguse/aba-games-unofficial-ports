// Copyright 2006 Kenta Cho. Some rights reserved.
using System.Collections.Generic;

public interface BulletTarget
{
    Vector getTargetPos();
}

public class Barrage
{
    public List<ParserParam> parserParam = new List<ParserParam>();
    public int prevWait, postWait;
    public void setWait(int prevWait, int postWait)
    {
        this.prevWait = prevWait;
        this.postWait = postWait;
    }

    public void addBml_3(int p, float rank, float speed)
    {
        parserParam.Add(new ParserParam(p, rank, speed));
    }

    public void addBml_4(string directory, string file, float rank, float speed)
    {
        addBml_3(BarrageManager.getInstance(directory, file), rank, speed);
    }

    public BulletActor addTopBullet_3(BulletPool bullets, BulletTarget target, float xr = 1)
    {
        return bullets.addTopBullet_10(parserParam, 0, 0, GameMath.PI, 0, xr, 1, target, prevWait, postWait);
    }

    public void clear()
    {
        parserParam = new List<ParserParam>();
    }
}
