// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class BulletImpl : Bullet
{
    public List<ParserParam> parserParam = new List<ParserParam>();
    public int parserIdx;
    public float xReverse, yReverse;
    public BulletTarget target;
    public BulletActor rootBullet;
    public BulletImpl(int id) : base(id)
    {
    }

    public void setParamFirst(List<ParserParam> parserParam, float xReverse, float yReverse, BulletTarget target, BulletActor rootBullet)
    {
        this.parserParam = parserParam;
        this.xReverse = xReverse;
        this.yReverse = yReverse;
        this.target = target;
        this.rootBullet = rootBullet;
        parserIdx = 0;
    }

    public void setParam(BulletImpl bi)
    {
        parserParam = bi.parserParam;
        xReverse = bi.xReverse;
        yReverse = bi.yReverse;
        target = bi.target;
        rootBullet = null;
        parserIdx = bi.parserIdx;
    }

    public bool gotoNextParser()
    {
        parserIdx++;
        if (parserIdx >= parserParam.Count)
        {
            parserIdx--;
            return false;
        }
        else
        {
            return true;
        }
    }

    public int getParser()
    {
        return parserParam[parserIdx].parser;
    }

    public void resetParser()
    {
        parserIdx = 0;
    }

    public override float patternRank()
    {
        ParserParam pp = parserParam[parserIdx];
        float r = pp.rank;
        if (r > 1)
            r = 1;
        return r;
    }

    public void slowdown()
    {
        foreach (var p in parserParam)
            p.speed *= .5f;
    }

    public float getSpeedRank()
    {
        return parserParam[parserIdx].speed;
    }
}

public class ParserParam
{
    public int parser;
    public float rank;
    public float speed;
    public ParserParam(int p, float r, float s)
    {
        parser = p;
        rank = r;
        speed = s;
    }
}
