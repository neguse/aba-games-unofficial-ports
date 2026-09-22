// Copyright 2004 Kenta Cho. All rights reserved.
using static GameMath;

public class Barrage
{
    public int[] parser;
    public float[] rank, speed;
    public int shape, color, prevWait, postWait;
    public float size, yReverse;

    public BulletActor addTopBullet(BulletActorPool bullets, BulletTarget target, int type)
    {
        if (size <= 0) return null;
        int cl = type == BulletType.SHIP ? 3 : color;
        float reverse = type == BulletType.SHIP ? -1 : 1;
        return bullets.addTopBullet(parser, rank, speed, 0, 0, PI / 2 * 3, 0,
            shape, cl, size, reverse, yReverse * reverse, target, type, prevWait, postWait);
    }
}

public class BulletInst : PatternBody
{
    public static BulletWorld world;
    public static BulletInst now;
    public static Rand random = new Rand();
    public BulletActor owner;
    public Vector pos = new Vector(), acc = new Vector();
    public float deg, velocity, rankNum, speedRankNum, bulletSize, xReverse, yReverse;
    public int shape, color, type, morphNum, morphIdx;
    public int[] parser = new int[8];
    public float[] ranks = new float[8], speeds = new float[8];
    public bool deactivated;
    public BulletTarget target;
    public StageManager stageManager;

    public BulletInst(BulletActor owner) { this.owner = owner; }
    public override float Direction { get { return rtod(deg); } set { deg = dtor(value); } }
    public override float Speed { get { return velocity * 6.2f; } set { velocity = value * (10f / 62); } }
    public override float AccelX { get { return acc.x; } set { acc.x = value * (10f / 62); } }
    public override float AccelY { get { return acc.y; } set { acc.y = value * (10f / 62); } }
    public override float Aim
    {
        get
        {
            Vector p = target.getTargetPos();
            return rtod((atan2(p.x - pos.x, p.y - pos.y) * xReverse + PI / 2) * yReverse - PI / 2);
        }
        set { }
    }
    public override float Rank
    {
        get
        {
            if (type != BulletType.ENEMY) return rankNum;
            float rank = rankNum + (1 - rankNum) * stageManager.rank / (1 + morphNum * 0.33f);
            return rank > 1 ? 1 : rank;
        }
        set { rankNum = value; }
    }
    public float speedRank
    {
        get { return type == BulletType.ENEMY ? speedRankNum * stageManager.speedRank : speedRankNum; }
        set { speedRankNum = value; }
    }
    public static void setRandSeed(int seed) { random.setSeed(seed); }
    public void setStageManager(StageManager manager) { stageManager = manager; }
    public static PatternState[] createRunner(int pattern)
    {
        int[] roots = BarrageCode.Roots(pattern);
        var states = new PatternState[roots.Length];
        for (int i = 0; i < roots.Length; i++) states[i] = BarrageCode.Create(roots[i], new float[2]);
        return states;
    }
    public void setRunner(PatternState[] runner)
    {
        Scripts.Clear();
        foreach (var script in runner) Scripts.Add(script);
    }
    public void set(float x, float y, float deg, float speed, float rank)
    {
        pos.x = x; pos.y = y; acc.y = 0; acc.x = acc.y;
        this.deg = deg; this.velocity = speed; rankNum = rank;
        Scripts.Clear();
    }
    public void setPattern(PatternState[] runner, float x, float y, float deg, float speed, float rank)
    {
        set(x, y, deg, speed, rank); setRunner(runner);
    }
    public void setMorph(int[] parsers, float[] ranks, float[] speeds, int count, int index)
    {
        morphNum = count; morphIdx = index;
        for (int i = 0; i < count; i++)
        {
            parser[i] = parsers[i]; this.ranks[i] = ranks[i]; this.speeds[i] = speeds[i];
        }
    }
    public void setParam(float sr, int shape, int color, float size, float xr, float yr, BulletTarget target, int type)
    {
        speedRankNum = sr; this.shape = shape; this.color = color; bulletSize = size;
        xReverse = xr; yReverse = yr; this.target = target; this.type = type; deactivated = false;
    }
    public void resetMorph() { morphIdx = 0; }
    public void move()
    {
        now = this;
        world.Turn = world.pool.getTurn();
        if (!PatternEnded()) RunPattern(world);
    }
    public bool isEnd() { return PatternEnded(); }
    public void remove() { Scripts.Clear(); }
}

public class BulletWorld : PatternWorld
{
    public BulletActorPool pool;
    public BulletWorld(BulletActorPool pool) { this.pool = pool; }
    public override float RandomValue() { return BulletInst.random.nextFloat(1); }
    public override void Fire(PatternBody parent, float direction, float speed, int program, float[] args)
    {
        if (program < 0) pool.addBullet(dtor(direction), speed * (10f / 62));
        else pool.addScriptBullet(BarrageCode.Create(program, args), dtor(direction), speed * (10f / 62));
    }
    public override void Vanish(PatternBody body) { ((BulletInst)body).owner.remove(); }
}
    public static class BulletType { public const int ENEMY = 0, SHIP = 1, MOVE = 2; }
