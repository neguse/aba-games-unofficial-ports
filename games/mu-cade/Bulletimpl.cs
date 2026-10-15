// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class BulletImpl : PatternBody
{
    public List<ParserParam> parserParam = new List<ParserParam>();
    public int parserIdx;
    public float xReverse, yReverse;
    public BulletTarget target;
    public BulletActor rootBullet;
    public Vector pos = new Vector(), acc = new Vector();
    public int id;
    public float deg, velocity;
    public List<PatternState> Scripts = new List<PatternState>();

    public BulletImpl(int id)
    {
        this.id = id;
    }

    public float GetDirection()
    {
        return rtod(deg);
    }

    public void SetDirection(float value)
    {
        deg = dtor(value);
    }

    public float GetSpeed()
    {
        return velocity * 6.2f;
    }

    public void SetSpeed(float value)
    {
        velocity = value * (10f / 62);
    }

    public float GetAccelX()
    {
        return acc.x;
    }

    public void SetAccelX(float value)
    {
        acc.x = value * (10f / 62);
    }

    public float GetAccelY()
    {
        return acc.y;
    }

    public void SetAccelY(float value)
    {
        acc.y = value * (10f / 62);
    }

    public float GetAim()
    {
        float x = Bullet.target.x - pos.x;
        return rtod((atan2(-x, Bullet.target.y - pos.y) * xReverse + PI / 2) * yReverse - PI / 2);
    }

    public float GetRank()
    {
        return patternRank();
    }

    public void set_5(float x, float y, float deg, float speed, float rank)
    {
        pos.x = x;
        pos.y = y;
        acc.x = 0;
        acc.y = 0;
        this.deg = deg;
        this.velocity = speed;
        Scripts.Clear();
    }

    public void set_6(PatternState[] runner, float x, float y, float deg, float speed, float rank)
    {
        set_5(x, y, deg, speed, rank);
        setRunner(runner);
    }

    public void setRunner(PatternState[] runner)
    {
        Scripts.Clear();
        foreach (var state in runner)
            Scripts.Add(state);
    }

    public void move_0()
    {
        Bullet.now = this;
        Bullet.manager.world.Turn = Bullet.manager.cnt;
        if (!((PatternRunner.Ended(Scripts))))
            PatternRunner.Run(Scripts, this, Bullet.manager.world);
    }

    public bool isEnd()
    {
        return PatternRunner.Ended(Scripts);
    }

    public void remove_0()
    {
        Scripts.Clear();
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

    public float patternRank()
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
