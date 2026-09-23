// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class StageManager
{
    public static int[] BOSS_RANKS = new int[] { 100, 160, 250 };
    public const float LEVEL_UP_RATIO = 0.5f;
    public static float[][] TUNNEL_COLOR_PATTERN_POLY = new float[][]
    {
        new float[] { 0.7f, 0.9f, 1 },
        new float[] { 0.6f, 1, 0.8f },
        new float[] { 0.9f, 0.7f, 0.6f },
        new float[] { 0.8f, 0.8f, 0.8f },
        new float[] { 0.5f, 0.9f, 0.9f },
        new float[] { 0.7f, 0.9f, 0.6f },
        new float[] { 0.8f, 0.5f, 0.9f },
    };
    public static float[][] TUNNEL_COLOR_PATTERN_LINE = new float[][]
    {
        new float[] { 0.6f, 0.7f, 1 },
        new float[] { 0.4f, 0.8f, 0.6f },
        new float[] { 0.7f, 0.5f, 0.6f },
        new float[] { 0.6f, 0.6f, 0.6f },
        new float[] { 0.4f, 0.7f, 0.7f },
        new float[] { 0.6f, 0.7f, 0.5f },
        new float[] { 0.6f, 0.4f, 1 },
    };
    public Tunnel tunnel;
    public Torus torus;
    public EnemyPool enemies;
    public Ship ship;
    public List<ShipSpec> smallShipSpec = new List<ShipSpec>(), middleShipSpec = new List<ShipSpec>(), bossShipSpec = new List<ShipSpec>();
    public Rand rand;
    public float nextSmallAppDist, nextMiddleAppDist, nextBossAppDist;
    public int bossNum;
    public int bossAppRank, zoneEndRank;
    public int bossSpecIdx;
    public float _level;
    public int grade;
    public int bossModeEndCnt;
    public bool _middleBossZone;
    public int tunnelColorPolyIdx, tunnelColorLineIdx;
    public const int TUNNEL_COLOR_CHANGE_INTERVAL = 60;
    public int tunnelColorChangeCnt;
    public StageManager(Tunnel tunnel, EnemyPool enemies, Ship ship)
    {
        this.tunnel = tunnel;
        this.enemies = enemies;
        this.ship = ship;
        rand = new Rand();
        torus = new Torus();
        ShipSpec.createBulletShape();
        {
            middleShipSpec.Clear();
            bossShipSpec.Clear();
            smallShipSpec.Clear();
        }
    }

    public void start(float level, int grade, int seed)
    {
        rand.setSeed(seed);
        torus.create(seed);
        tunnel.start(torus);
        this._level = level - LEVEL_UP_RATIO;
        this.grade = grade;
        zoneEndRank = 0;
        _middleBossZone = false;
        Slice.darkLine = true;
        Slice.darkLineRatio = 1;
        tunnelColorPolyIdx = TUNNEL_COLOR_PATTERN_POLY.Length + GameMath.integer(level) - 2;
        tunnelColorLineIdx = TUNNEL_COLOR_PATTERN_LINE.Length + GameMath.integer(level) - 2;
        createNextZone();
    }

    public void createNextZone()
    {
        _level = _level + (LEVEL_UP_RATIO);
        _middleBossZone = !(_middleBossZone);
        if (Slice.darkLine)
        {
            tunnelColorPolyIdx++;
            tunnelColorLineIdx++;
        }

        Slice.darkLine = !(Slice.darkLine);
        tunnelColorChangeCnt = TUNNEL_COLOR_CHANGE_INTERVAL;
        enemies.clear();
        closeShipSpec();
        smallShipSpec.Clear();
        for (int i = 0; i < 2 + rand.nextInt(2); i++)
        {
            ShipSpec ss = new ShipSpec();
            ss.createSmall(rand, _level * 1.8f, grade);
            smallShipSpec.Add(ss);
        }

        middleShipSpec.Clear();
        for (int i = 0; i < 2 + rand.nextInt(2); i++)
        {
            ShipSpec ss = new ShipSpec();
            ss.createMiddle(rand, _level * 1.9f);
            middleShipSpec.Add(ss);
        }

        {
            nextMiddleAppDist = 0;
            nextSmallAppDist = nextMiddleAppDist;
        }

        setNextSmallAppDist();
        setNextMiddleAppDist();
        bossShipSpec.Clear();
        if (((_middleBossZone) && (_level > 5)) && (rand.nextInt(3) != 0))
        {
            bossNum = 1 + rand.nextInt(GameMath.integer(sqrt(_level / 5)) + 1);
            if (bossNum > 4)
                bossNum = 4;
        }
        else
        {
            bossNum = 1;
        }

        for (int i = 0; i < bossNum; i++)
        {
            ShipSpec ss = new ShipSpec();
            float lv = _level * 2.0f / bossNum;
            if (_middleBossZone)
                lv = lv * (1.33f);
            ss.createBoss(rand, lv, 0.8f + grade * 0.04f + rand.nextFloat(0.03f), _middleBossZone);
            bossShipSpec.Add(ss);
        }

        bossAppRank = BOSS_RANKS[grade] - bossNum + zoneEndRank;
        zoneEndRank = zoneEndRank + (BOSS_RANKS[grade]);
        ship.setBossApp(bossAppRank, bossNum, zoneEndRank);
        bossSpecIdx = 0;
        nextBossAppDist = 9999999;
        bossModeEndCnt = -1;
    }

    public void move()
    {
        if (ship.inBossMode)
        {
            if (nextBossAppDist > 99999)
            {
                nextBossAppDist = rand.nextInt(50) + 100;
                {
                    nextMiddleAppDist = 9999999;
                    nextSmallAppDist = nextMiddleAppDist;
                }
            }

            nextBossAppDist = nextBossAppDist - (ship.speed);
            if ((bossNum > 0) && (nextBossAppDist <= 0))
            {
                addEnemy(bossShipSpec[bossSpecIdx], Ship.IN_SIGHT_DEPTH_DEFAULT * 4, rand);
                bossNum--;
                nextBossAppDist = rand.nextInt(30) + 60;
                bossSpecIdx++;
            }

            if ((bossNum <= 0) && (enemies.getNum() <= 0))
                ship.gotoNextZoneForced();
            return;
        }
        else
        {
            if (nextBossAppDist < 99999)
            {
                bossModeEndCnt = 60;
                {
                    nextBossAppDist = 9999999;
                    nextMiddleAppDist = nextBossAppDist;
                    nextSmallAppDist = nextMiddleAppDist;
                }
            }

            if (bossModeEndCnt >= 0)
            {
                bossModeEndCnt--;
                ship.clearVisibleBullets();
                if (bossModeEndCnt < 0)
                {
                    createNextZone();
                    ship.startNextZone();
                }
            }
        }

        nextSmallAppDist = nextSmallAppDist - (ship.speed);
        if (nextSmallAppDist <= 0)
        {
            addEnemy(smallShipSpec[rand.nextInt(smallShipSpec.Count)], Ship.IN_SIGHT_DEPTH_DEFAULT * (4 + rand.nextFloat(0.5f)), rand);
            setNextSmallAppDist();
        }

        nextMiddleAppDist = nextMiddleAppDist - (ship.speed);
        if (nextMiddleAppDist <= 0)
        {
            addEnemy(middleShipSpec[rand.nextInt(middleShipSpec.Count)], Ship.IN_SIGHT_DEPTH_DEFAULT * (4 + rand.nextFloat(0.5f)), rand);
            setNextMiddleAppDist();
        }

        if (tunnelColorChangeCnt > 0)
        {
            tunnelColorChangeCnt--;
            if (Slice.darkLine)
            {
                Slice.darkLineRatio = Slice.darkLineRatio + (1.0f / TUNNEL_COLOR_CHANGE_INTERVAL);
            }
            else
            {
                Slice.darkLineRatio = Slice.darkLineRatio - (1.0f / TUNNEL_COLOR_CHANGE_INTERVAL);
                float cRatio = (float)tunnelColorChangeCnt / TUNNEL_COLOR_CHANGE_INTERVAL;
                int cpIdxPrev = (tunnelColorPolyIdx - 1) % TUNNEL_COLOR_PATTERN_POLY.Length;
                int cpIdxNow = tunnelColorPolyIdx % TUNNEL_COLOR_PATTERN_POLY.Length;
                Slice.polyR = TUNNEL_COLOR_PATTERN_POLY[cpIdxPrev][0] * cRatio + TUNNEL_COLOR_PATTERN_POLY[cpIdxNow][0] * (1 - cRatio);
                Slice.polyG = TUNNEL_COLOR_PATTERN_POLY[cpIdxPrev][1] * cRatio + TUNNEL_COLOR_PATTERN_POLY[cpIdxNow][1] * (1 - cRatio);
                Slice.polyB = TUNNEL_COLOR_PATTERN_POLY[cpIdxPrev][2] * cRatio + TUNNEL_COLOR_PATTERN_POLY[cpIdxNow][2] * (1 - cRatio);
                int clIdxPrev = (tunnelColorLineIdx - 1) % TUNNEL_COLOR_PATTERN_LINE.Length;
                int clIdxNow = tunnelColorLineIdx % TUNNEL_COLOR_PATTERN_LINE.Length;
                Slice.lineR = TUNNEL_COLOR_PATTERN_LINE[clIdxPrev][0] * cRatio + TUNNEL_COLOR_PATTERN_LINE[clIdxNow][0] * (1 - cRatio);
                Slice.lineG = TUNNEL_COLOR_PATTERN_LINE[clIdxPrev][1] * cRatio + TUNNEL_COLOR_PATTERN_LINE[clIdxNow][1] * (1 - cRatio);
                Slice.lineB = TUNNEL_COLOR_PATTERN_LINE[clIdxPrev][2] * cRatio + TUNNEL_COLOR_PATTERN_LINE[clIdxNow][2] * (1 - cRatio);
            }
        }
    }

    public void setNextSmallAppDist()
    {
        nextSmallAppDist = nextSmallAppDist + (rand.nextInt(16) + 6);
    }

    public void setNextMiddleAppDist()
    {
        nextMiddleAppDist = nextMiddleAppDist + (rand.nextInt(200) + 33);
    }

    public void addEnemy(ShipSpec spec, float y, Rand rand)
    {
        Enemy en = enemies.getInstance();
        if (!((en != null)))
            return;
        Slice sl = tunnel.getSlice(y);
        float x = 0;
        if (sl.isNearlyRound())
        {
            x = rand.nextFloat(PI);
        }
        else
        {
            float ld = sl.getLeftEdgeDeg();
            float rd = sl.getRightEdgeDeg();
            float wd = rd - ld;
            if (wd < 0)
                wd = wd + (PI * 2);
            x = ld + rand.nextFloat(wd);
        }

        if (x < 0)
            x = x + (PI * 2);
        else if (x >= PI * 2)
            x = x - (PI * 2);
        en.set_6(spec, x, y, rand);
    }

    public void closeStage()
    {
        closeShipSpec();
        torus.close();
    }

    public void close()
    {
        closeStage();
        ShipSpec.closeBulletShape();
    }

    public void closeShipSpec()
    {
        if ((smallShipSpec != null && smallShipSpec.Count > 0))
            foreach (ShipSpec ss in smallShipSpec)
                ss.close();
        if ((middleShipSpec != null && middleShipSpec.Count > 0))
            foreach (ShipSpec ss in middleShipSpec)
                ss.close();
        if ((bossShipSpec != null && bossShipSpec.Count > 0))
            foreach (ShipSpec ss in bossShipSpec)
                ss.close();
    }

    public float level
    {
        get
        {
            return _level;
        }
    }

    public bool middleBossZone
    {
        get
        {
            return _middleBossZone;
        }
    }
}

public class ShipSpec
{
    public const float SPEED_CHANGE_RATIO = 0.2f;
    public static List<BulletShape> bulletShape = new List<BulletShape>();
    public static BitShape _bitShape;
    public ShipShape _shape, _damagedShape;
    public Barrage _barrage;
    public int _shield;
    public float baseSpeed, shipSpeedRatio;
    public float visualRange;
    public float baseBank;
    public int ocsMoveInterval;
    public float bankMax;
    public int _score;
    public int _bitNum;
    public int bitType;
    public float bitDistance;
    public float bitMd;
    public Barrage _bitBarrage;
    public bool _aimShip;
    public bool _hasLimitY;
    public bool _noFireDepthLimit;
    public bool _isBoss;
    public static void createBulletShape()
    {
        BulletShape bs = null;
        for (int i = 0; i < BulletShape.NUM; i++)
        {
            bs = new BulletShape();
            bs.create(i);
            bulletShape.Add(bs);
        }

        _bitShape = new BitShape();
        _bitShape.create();
    }

    public static void closeBulletShape()
    {
        foreach (BulletShape bs in bulletShape)
            bs.close();
    }

    public void createSmall(Rand rand, float level, int grade)
    {
        _shield = 1;
        baseSpeed = 0.05f + rand.nextFloat(0.1f);
        shipSpeedRatio = 0.25f + rand.nextFloat(0.25f);
        visualRange = 10 + rand.nextFloat(32);
        bankMax = 0.3f + rand.nextFloat(0.7f);
        if (rand.nextInt(3) == 0)
            baseBank = 0.1f + rand.nextFloat(0.2f);
        else
            baseBank = 0;
        int rs = rand.nextInt(99999);
        _shape = new ShipShape(rs);
        _shape.create(ShipShapeType.SMALL);
        _damagedShape = new ShipShape(rs);
        _damagedShape.create(ShipShapeType.SMALL, true);
        int brgInterval = 0;
        int biMin = GameMath.integer((160.0f / level));
        if (biMin > 80)
            biMin = 80;
        else if (biMin < 40)
            biMin = 40;
        biMin = biMin + ((Ship.GRADE_NUM - 1 - grade) * 8);
        brgInterval = biMin + rand.nextInt(80 + (Ship.GRADE_NUM - 1 - grade) * 8 - biMin);
        float brgRank = level;
        brgRank = brgRank / ((150.0f / brgInterval));
        _barrage = createBarrage(rand, brgRank, 0, brgInterval);
        _score = 100;
        _bitNum = 0;
        {
            _isBoss = false;
            _noFireDepthLimit = _isBoss;
            _hasLimitY = _noFireDepthLimit;
            _aimShip = _hasLimitY;
        }
    }

    public void createMiddle(Rand rand, float level)
    {
        _shield = 10;
        baseSpeed = 0.1f + rand.nextFloat(0.1f);
        shipSpeedRatio = 0.4f + rand.nextFloat(0.4f);
        visualRange = 10 + rand.nextFloat(32);
        bankMax = 0.2f + rand.nextFloat(0.5f);
        if (rand.nextInt(4) == 0)
            baseBank = 0.05f + rand.nextFloat(0.1f);
        else
            baseBank = 0;
        int rs = rand.nextInt(99999);
        _shape = new ShipShape(rs);
        _shape.create(ShipShapeType.MIDDLE);
        _damagedShape = new ShipShape(rs);
        _damagedShape.create(ShipShapeType.MIDDLE, true);
        _barrage = createBarrage(rand, level, 0, 0, 1, "middle", BulletShapeBSType.SQUARE);
        _score = 500;
        _bitNum = 0;
        {
            _isBoss = false;
            _noFireDepthLimit = _isBoss;
            _hasLimitY = _noFireDepthLimit;
            _aimShip = _hasLimitY;
        }
    }

    public void createBoss(Rand rand, float level, float speed, bool middleBoss)
    {
        _shield = 30;
        baseSpeed = 0.1f + rand.nextFloat(0.1f);
        shipSpeedRatio = speed;
        visualRange = 16 + rand.nextFloat(24);
        bankMax = 0.8f + rand.nextFloat(0.4f);
        baseBank = 0;
        int rs = rand.nextInt(99999);
        _shape = new ShipShape(rs);
        _shape.create(ShipShapeType.LARGE);
        _damagedShape = new ShipShape(rs);
        _damagedShape.create(ShipShapeType.LARGE, true);
        _barrage = createBarrage(rand, level, 0, 0, 1.2f, "middle", BulletShapeBSType.SQUARE, true);
        _score = 2000;
        {
            _isBoss = true;
            _noFireDepthLimit = _isBoss;
            _hasLimitY = _noFireDepthLimit;
            _aimShip = _hasLimitY;
        }

        if (middleBoss)
        {
            _bitNum = 0;
            return;
        }

        _bitNum = 2 + rand.nextInt(3) * 2;
        bitType = rand.nextInt(2);
        bitDistance = 0.33f + rand.nextFloat(0.3f);
        bitMd = 0.02f + rand.nextFloat(0.02f);
        float bitBrgRank = level;
        bitBrgRank = bitBrgRank / ((GameMath.integer(bitNum / 2)));
        int brgInterval = 0;
        int biMin = GameMath.integer((120.0f / bitBrgRank));
        if (biMin > 60)
            biMin = 60;
        else if (biMin < 20)
            biMin = 20;
        brgInterval = biMin + rand.nextInt(60 - biMin);
        bitBrgRank = bitBrgRank / ((60.0f / brgInterval));
        _bitBarrage = createBarrage(rand, bitBrgRank, 0, brgInterval, 1, null, BulletShapeBSType.BAR, true);
        _bitBarrage.setNoXReverse();
    }

    public void close()
    {
        _shape.close();
    }

    public Barrage createBarrage(Rand rand, float level, int preWait, int postWait, float size = 1, string baseDir = null, int shapeIdx = 0, bool longRange = false)
    {
        if (level < 0)
            return null;
        float rank = sqrt(level) / (8 - rand.nextInt(3));
        if (rank > 0.8f)
            rank = rand.nextFloat(0.2f) + 0.8f;
        level = level / ((rank + 2));
        float speedRank = sqrt(rank) * (rand.nextFloat(0.2f) + 0.8f);
        if (speedRank < 1)
            speedRank = 1;
        if (speedRank > 2)
            speedRank = sqrt(speedRank * 2);
        float morphRank = level / speedRank;
        int morphCnt = 0;
        while (morphRank > 1)
        {
            morphCnt++;
            morphRank = morphRank / (3);
        }

        Barrage br = new Barrage();
        ResizableDrawable bsr = new ResizableDrawable();
        bsr.shape = bulletShape[shapeIdx];
        bsr.size = size * 1.25f;
        ResizableDrawable dbsr = new ResizableDrawable();
        dbsr.shape = bulletShape[shapeIdx + 1];
        dbsr.size = size * 1.25f;
        br.setShape(bsr, dbsr);
        br.setWait(preWait, postWait);
        br.setLongRange(longRange);
        int[] ps = null;
        int psn = 0;
        if ((baseDir != null))
        {
            ps = BarrageManager.getInstanceList(baseDir);
            int pi = rand.nextInt(ps.Length);
            br.addBml_4(ps[pi], rank, true, speedRank);
        }
        else
        {
            br.addBml_5("basic", "straight.xml", rank, true, speedRank);
        }

        ps = BarrageManager.getInstanceList("morph");
        psn = ps.Length;
        for (int i = 0; i < morphCnt; i++)
        {
            int pi = rand.nextInt(ps.Length);
            while (ps[pi] < 0)
            {
                pi--;
                if (pi < 0)
                    pi = ps.Length - 1;
            }

            br.addBml_4(ps[pi], morphRank, true, speedRank);
            ps[pi] = -1;
            psn--;
        }

        return br;
    }

    public float setSpeed_1(float sp)
    {
        return changeSpeed(sp, baseSpeed);
    }

    public float setSpeed_2(float sp, float shipSp)
    {
        float aimSpeed = shipSp * shipSpeedRatio;
        if (aimSpeed > baseSpeed)
            return changeSpeed(sp, aimSpeed);
        else
            return changeSpeed(sp, baseSpeed);
    }

    public float changeSpeed(float sp, float aim)
    {
        return sp + ((aim - sp) * SPEED_CHANGE_RATIO);
    }

    public bool getRangeOfMovement(Vector range, Vector p, Tunnel tunnel)
    {
        range.x = 0;
        range.y = 0;
        float py = p.y;
        Slice cs = tunnel.getSlice(py);
        py = py + (visualRange);
        Slice vs = tunnel.getSlice(py);
        if (!(cs.isNearlyRound()))
        {
            range.x = cs.getLeftEdgeDeg();
            range.y = cs.getRightEdgeDeg();
            if (!(vs.isNearlyRound()))
            {
                float vld = vs.getLeftEdgeDeg();
                float vrd = vs.getRightEdgeDeg();
                if (Tunnel.checkDegInside(range.x, vld, vrd) == -1)
                    range.x = vld;
                if (Tunnel.checkDegInside(range.y, vld, vrd) == 1)
                    range.y = vrd;
            }

            return true;
        }
        else if (!(vs.isNearlyRound()))
        {
            range.x = vs.getLeftEdgeDeg();
            range.y = vs.getRightEdgeDeg();
            return true;
        }
        else
        {
            return false;
        }
    }

    public float tryToMove(float bank, float deg, float aimDeg)
    {
        float bk = aimDeg - deg;
        if (bk > PI)
            bk = bk - (PI * 2);
        else if (bk < -PI)
            bk = bk + (PI * 2);
        if (bk > bankMax)
            bk = bankMax;
        else if (bk < -bankMax)
            bk = -bankMax;
        return bank + ((bk - bank) * 0.1f);
    }

    public float handleLimitY(Vector pos, float limitY)
    {
        if (pos.y > limitY)
            pos.y = pos.y + ((limitY - pos.y) * 0.05f);
        else
            limitY = limitY + ((pos.y - limitY) * 0.05f);
        return limitY - 0.01f;
    }

    public float createBaseBank(Rand rand)
    {
        return rand.nextSignedFloat(baseBank);
    }

    public float getBitOffset(Vector ofs, float deg, int idx, int cnt)
    {
        switch (bitType)
        {
            case ShipSpecBitType.ROUND:
                float od = PI * 2 / bitNum;
                float d = od * idx + cnt * bitMd;
                ofs.x = bitDistance * 2 * sin(d);
                ofs.y = bitDistance * 2 * cos(d) * 5;
                deg = PI - sin(d) * 0.05f;
                break;
            case ShipSpecBitType.LINE:
                float of = (idx % 2) * 2 - 1;
                int oi = GameMath.integer((GameMath.integer(idx / 2))) + 1;
                ofs.x = bitDistance * 1.5f * oi * of;
                ofs.y = 0;
                deg = PI;
                break;
        }

        return deg;
    }

    public static BitShape bitShape()
    {
        return _bitShape;
    }

    public ShipShape shape
    {
        get
        {
            return _shape;
        }
    }

    public ShipShape damagedShape
    {
        get
        {
            return _damagedShape;
        }
    }

    public int shield
    {
        get
        {
            return _shield;
        }
    }

    public Barrage barrage
    {
        get
        {
            return _barrage;
        }
    }

    public int score
    {
        get
        {
            return _score;
        }
    }

    public bool aimShip
    {
        get
        {
            return _aimShip;
        }
    }

    public bool hasLimitY
    {
        get
        {
            return _hasLimitY;
        }
    }

    public bool noFireDepthLimit
    {
        get
        {
            return _noFireDepthLimit;
        }
    }

    public Barrage bitBarrage
    {
        get
        {
            return _bitBarrage;
        }
    }

    public int bitNum
    {
        get
        {
            return _bitNum;
        }
    }

    public bool isBoss
    {
        get
        {
            return _isBoss;
        }
    }
}

public static class ShipSpecBitType
{
    public const int ROUND = 0, LINE = 1;
}
