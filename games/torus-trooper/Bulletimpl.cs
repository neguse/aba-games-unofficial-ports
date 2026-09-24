// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class BulletImpl : Bullet
{
    public List<ParserParam> parserParam = new List<ParserParam>();
    public int parserIdx;
    public Drawable shape, disapShape;
    public float xReverse, yReverse;
    public bool longRange;
    public BulletTarget target;
    public BulletActor rootBullet;
    public BulletImpl(int id) : base(id)
    {
    }

    public void setParamFirst(List<ParserParam> parserParam, Drawable shape, Drawable disapShape, float xReverse, float yReverse, bool longRange, BulletTarget target, BulletActor rootBullet)
    {
        this.parserParam = parserParam;
        this.shape = shape;
        this.disapShape = disapShape;
        this.xReverse = xReverse;
        this.yReverse = yReverse;
        this.longRange = longRange;
        this.target = target;
        this.rootBullet = rootBullet;
        parserIdx = 0;
    }

    public void setParam(BulletImpl bi)
    {
        parserParam = bi.parserParam;
        shape = bi.shape;
        disapShape = bi.disapShape;
        xReverse = bi.xReverse;
        yReverse = bi.yReverse;
        target = bi.target;
        rootBullet = null;
        parserIdx = bi.parserIdx;
        longRange = bi.longRange;
    }

    public void addParser(int p, float r, float re, float s)
    {
        parserParam.Add(new ParserParam(p, r, re, s));
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

    public float getSpeedRank()
    {
        return parserParam[parserIdx].speed;
    }
}

public class ParserParam
{
    public int parser;
    public float rank;
    public float rootRankEffect;
    public float speed;
    public ParserParam(int p, float r, float re, float s)
    {
        parser = p;
        rank = r;
        rootRankEffect = re;
        speed = s;
    }
}
