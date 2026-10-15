// Copyright 2003 Kenta Cho. All rights reserved.
using System.Collections.Generic;
using static GameMath;

public class P47Bullet : PatternBody
{
    public const int MORPH_MAX = 8;
    public static P47World world;
    public static P47Bullet now;
    public static Vector target = new Vector();
    public static P47Rand random = new P47Rand();
    public BulletActor owner;
    public Vector pos = new Vector(), acc = new Vector();
    public float deg, velocity, rankNum, speedRank, bulletSize, xReverse;
    public int shape, color, morphNum, morphIdx, morphCnt, baseMorphIdx, baseMorphCnt;
    public int[] morphParser = new int[8];
    public bool isMorph;
    public List<PatternState> Scripts = new List<PatternState>();
    public P47Bullet(BulletActor owner) { this.owner = owner; }
    public float GetDirection() { return rtod(deg); }
    public void SetDirection(float value) { deg = dtor(value); }
    public float GetSpeed() { return velocity * 6.2f; }
    public void SetSpeed(float value) { velocity = value * (10f / 62); }
    public float GetAccelX() { return acc.x; }
    public void SetAccelX(float value) { acc.x = value * (10f / 62); }
    public float GetAccelY() { return acc.y; }
    public void SetAccelY(float value) { acc.y = value * (10f / 62); }
    public float GetAim() { return rtod(atan2(target.x - pos.x, target.y - pos.y) * xReverse); }
    public float GetRank() { return rankNum; }
    public static void setRandSeed(int seed) { random.setSeed(seed); }
    public static PatternState[] createRunner(int pattern)
    {
        int[] roots = BarrageCode.Roots(pattern);
        var states = new PatternState[roots.Length];
        for (int index0 = 0; index0 < roots.Length; index0++) states[index0] = BarrageCode.Create(roots[index0], new float[] {0, 0, 0});
        return states;
    }
    public void setRunner(PatternState[] runner)
    {
        Scripts.Clear(); foreach (var script in runner) Scripts.Add(script);
    }
    public void set(float x, float y, float deg, float velocity, float rankNum)
    {
        pos.x = x; pos.y = y; acc.x = 0; acc.y = 0;
        this.deg = deg; this.velocity = velocity; this.rankNum = rankNum; Scripts.Clear();
    }
    public void setPattern(PatternState[] runner, float x, float y, float deg, float velocity, float rankNum)
    {
        set(x, y, deg, velocity, rankNum); setRunner(runner);
    }
    public void setMorph(int[] parsers, int num, int idx, int count)
    {
        if (count <= 0) { isMorph = false; return; }
        isMorph = true; morphCnt = count; baseMorphCnt = count; morphNum = num;
        for (int index1 = 0; index1 < num; index1++) morphParser[index1] = parsers[index1];
        morphIdx = idx >= num ? 0 : idx; baseMorphIdx = morphIdx;
    }
    public void resetMorph() { morphIdx = baseMorphIdx; morphCnt = baseMorphCnt; }
    public void setParam(float sr, int sh, int cl, float sz, float xr)
    { speedRank = sr; shape = sh; color = cl; bulletSize = sz; xReverse = xr; }
    public void move()
    {
        now = this; world.Turn = world.pool.getTurn();
        if (!PatternRunner.Ended(Scripts)) PatternRunner.Run(Scripts, this, world);
    }
    public bool isEnd() { return PatternRunner.Ended(Scripts); }
    public void remove() { Scripts.Clear(); }
}
public class P47World : PatternWorld
{
    public float Turn;
    public BulletActorPool pool;
    public P47World(BulletActorPool pool) { this.pool = pool; }
    public float GetTurn() { return Turn; }
    public float RandomValue() { return P47Bullet.random.nextFloat(1); }
    public void Fire(PatternBody parent, float direction, float velocity, int program, float[] args)
    {
        if (program < 0) pool.addBullet_2(dtor(direction), velocity * (10f / 62));
        else pool.addScriptBullet(BarrageCode.Create(program, args), dtor(direction), velocity * (10f / 62));
    }
    public void Vanish(PatternBody body) { ((P47Bullet)body).owner.remove(); }
}
