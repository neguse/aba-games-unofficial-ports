// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class ScoreReel
{
    public const int MAX_DIGIT = 16;
    public int score, targetScore;
    public int _actualScore;
    public int digit;
    public NumReel[] numReel = new NumReel[MAX_DIGIT];
    public ScoreReel()
    {
        for (int index1 = 0; index1 < MAX_DIGIT; index1++)
        {
            numReel[index1] = new NumReel();
        }

        digit = 1;
    }

    public void clear(int digit = 9)
    {
        {
            _actualScore = 0;
            targetScore = _actualScore;
            score = targetScore;
        }

        this.digit = digit;
        for (int i = 0; i < digit; i++)
            numReel[i].clear();
    }

    public void move()
    {
        for (int i = 0; i < digit; i++)
            numReel[i].move();
    }

    public void draw(float[] model, float x, float y, float s)
    {
        float lx = x, ly = y;
        for (int i = 0; i < digit; i++)
        {
            numReel[i].draw(model, lx, ly, s);
            lx = lx - (s * 2);
        }
    }

    public void addReelScore(int actual)
    {
        targetScore = targetScore + (actual);
        int ts = targetScore;
        for (int i = 0; i < digit; i++)
        {
            numReel[i].setTargetDeg((float)ts * 360 / 10);
            ts = ts / (10);
            if (ts < 0)
                break;
        }
    }

    public void accelerate()
    {
        for (int i = 0; i < digit; i++)
            numReel[i].accelerate();
    }

    public void addActualScore(int actual)
    {
        _actualScore = _actualScore + (actual);
    }

    public int actualScore
    {
        get
        {
            return _actualScore;
        }

        set
        {
            _actualScore = value;
        }
    }
}

public class NumReel
{
    public const float VEL_MIN = 5;
    public static GunroarRand rand = new GunroarRand();
    public float deg;
    public float _targetDeg;
    public float ofs;
    public float velRatio;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public NumReel()
    {
        init();
    }

    public void init()
    {
        {
            _targetDeg = 0;
            deg = _targetDeg;
        }

        ofs = 0;
        velRatio = 1;
    }

    public void clear()
    {
        init();
    }

    public void move()
    {
        float vd = _targetDeg - deg;
        vd = vd * (0.05f * velRatio);
        if (vd < VEL_MIN * velRatio)
            vd = VEL_MIN * velRatio;
        deg = deg + (vd);
        if (deg > _targetDeg)
            deg = _targetDeg;
    }

    public void draw(float[] model, float x, float y, float s)
    {
        float[] color = null;
        int n = GameMath.integer(((deg * 10 / 360 + 0.99f) + 1)) % 10;
        float d = deg % 360;
        float od = d - n * 360 / 10;
        od = od - (15);
        od = normalizeDeg360(od);
        od = od * (1.5f);
        for (int i = 0; i < 3; i++)
        {
            float[] parent1 = model;
            if (ofs > 0.005f)
                model = Transform.Translate(model, x + rand.nextSignedFloat(1) * ofs, y + rand.nextSignedFloat(1) * ofs, 0);
            else
                model = Transform.Translate(model, x, y, 0);
            model = Transform.Rotate(model, od, 1, 0, 0);
            model = Transform.Translate(model, 0, 0, s * 2.4f);
            model = Transform.Scale(model, s, -s, s);
            float a = 1 - fabs((od + 15) / (360 / 10 * 1.5f)) / 2;
            if (a < 0)
                a = 0;
            color = new float[] { a, a, a, 1 };
            Letter.drawLetter_2(model, n, 2, color);
            color = new float[] { a / 2, a / 2, a / 2, 1 };
            Letter.drawLetter_2(model, n, 3, color);
            model = parent1;
            n--;
            if (n < 0)
                n = 9;
            od = od + (360 / 10 * 1.5f);
            od = normalizeDeg360(od);
        }

        ofs = ofs * (0.95f);
    }

    public void setTargetDeg(float td)
    {
        if ((td - _targetDeg) > 1)
            ofs = ofs + (0.1f);
        _targetDeg = td;
    }

    public void accelerate()
    {
        velRatio = 4;
    }
}

public class NumIndicator : Actor
{
    public static GunroarRand rand = new GunroarRand();
    public const float TARGET_Y_MIN = -7;
    public const float TARGET_Y_MAX = 7;
    public const float TARGET_Y_INTERVAL = 1;
    public static float targetY = TARGET_Y_MIN;
    public ScoreReel scoreReel;
    public Vector pos, vel;
    public int n, type;
    public float size;
    public int cnt;
    public float alpha;
    public NumIndicatorTarget[] target = new NumIndicatorTarget[4];
    public int targetIdx;
    public int targetNum;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public static void initTargetY()
    {
        targetY = TARGET_Y_MIN;
    }

    public static float getTargetY()
    {
        float ty = targetY;
        targetY = targetY + (TARGET_Y_INTERVAL);
        if (targetY > TARGET_Y_MAX)
            targetY = TARGET_Y_MIN;
        return ty;
    }

    public static void decTargetY()
    {
        targetY = targetY - (TARGET_Y_INTERVAL);
        if (targetY < TARGET_Y_MIN)
            targetY = TARGET_Y_MAX;
    }

    public NumIndicator()
    {
        for (int i = 0; i < 4; i++)
            target[i] = new NumIndicatorTarget();
        pos = new Vector();
        vel = new Vector();
        for (int index0 = 0; index0 < 4; index0++)
        {
            target[index0].pos = new Vector();
            target[index0].initialVelRatio = 0;
            target[index0].size = 0;
        }

        {
            targetNum = 0;
            targetIdx = targetNum;
        }

        alpha = 1;
    }

    public override void init(List<object> args)
    {
        scoreReel = (ScoreReel)args[0];
    }

    public void set_4(int n, int type, float size, Vector p)
    {
        set_5(n, type, size, p.x, p.y);
    }

    public void set_5(int n, int type, float size, float x, float y)
    {
        if (exists && (this.type == NumIndicatorIndicatorType.SCORE))
        {
            if (this.target[targetIdx].flyingTo == NumIndicatorFlyingToType.RIGHT)
                decTargetY();
            scoreReel.addReelScore(target[targetNum - 1].n);
        }

        this.n = n;
        this.type = type;
        this.size = size;
        pos.x = x;
        pos.y = y;
        targetIdx = -1;
        targetNum = 0;
        alpha = 0.1f;
        exists = true;
    }

    public void addTarget(float x, float y, int flyingTo, float initialVelRatio, float size, int n, int cnt)
    {
        target[targetNum].pos.x = x;
        target[targetNum].pos.y = y;
        target[targetNum].flyingTo = flyingTo;
        target[targetNum].initialVelRatio = initialVelRatio;
        target[targetNum].size = size;
        target[targetNum].n = n;
        target[targetNum].cnt = cnt;
        targetNum++;
    }

    public void gotoNextTarget()
    {
        targetIdx++;
        if (targetIdx > 0)
            SoundManager.playSe("score_up.wav");
        if (targetIdx >= targetNum)
        {
            if (target[targetIdx - 1].flyingTo == NumIndicatorFlyingToType.BOTTOM)
                scoreReel.addReelScore(target[targetIdx - 1].n);
            exists = false;
            return;
        }

        switch (target[targetIdx].flyingTo)
        {
            case NumIndicatorFlyingToType.RIGHT:
            {
                vel.x = -0.3f + rand.nextSignedFloat(0.05f);
                vel.y = rand.nextSignedFloat(0.1f);
                break;
            }

            case NumIndicatorFlyingToType.BOTTOM:
            {
                vel.x = rand.nextSignedFloat(0.1f);
                vel.y = 0.3f + rand.nextSignedFloat(0.05f);
                decTargetY();
                break;
            }
        }

        vel.opMulAssign(target[targetIdx].initialVelRatio);
        cnt = target[targetIdx].cnt;
    }

    public override void move()
    {
        if (targetIdx < 0)
            return;
        Vector tp = target[targetIdx].pos;
        switch (target[targetIdx].flyingTo)
        {
            case NumIndicatorFlyingToType.RIGHT:
            {
                vel.x = vel.x + ((tp.x - pos.x) * 0.0036f);
                pos.y = pos.y + ((tp.y - pos.y) * 0.1f);
                if (fabs(pos.y - tp.y) < 0.5f)
                    pos.y = pos.y + ((tp.y - pos.y) * 0.33f);
                alpha = alpha + ((1 - alpha) * 0.03f);
                break;
            }

            case NumIndicatorFlyingToType.BOTTOM:
            {
                pos.x = pos.x + ((tp.x - pos.x) * 0.1f);
                vel.y = vel.y + ((tp.y - pos.y) * 0.0036f);
                alpha = alpha * (0.97f);
                break;
            }
        }

        vel.opMulAssign(0.98f);
        size = size + ((target[targetIdx].size - size) * 0.025f);
        pos.opAddAssign(vel);
        int vn = GameMath.integer(((target[targetIdx].n - n) * 0.2f));
        if ((vn < 10) && (vn > -10))
            n = target[targetIdx].n;
        else
            n = n + (vn);
        switch (target[targetIdx].flyingTo)
        {
            case NumIndicatorFlyingToType.RIGHT:
            {
                if (pos.x > tp.x)
                {
                    pos.x = tp.x;
                    vel.x = vel.x * (-0.05f);
                }

                break;
            }

            case NumIndicatorFlyingToType.BOTTOM:
            {
                if (pos.y < tp.y)
                {
                    pos.y = tp.y;
                    vel.y = vel.y * (-0.05f);
                }

                break;
            }
        }

        cnt--;
        if (cnt < 0)
            gotoNextTarget();
    }

    public override void draw(float[] model, Mesh particles = null)
    {
        float[] color = null;
        color = new float[] { alpha, alpha, alpha, 1 };
        switch (type)
        {
            case NumIndicatorIndicatorType.SCORE:
            {
                Letter.drawNumSign(model, n, pos.x, pos.y, size, Letter.LINE_COLOR, -1, -1, color);
                break;
            }

            case NumIndicatorIndicatorType.MULTIPLIER:
            {
                color = new float[] { alpha, alpha, alpha, 1 };
                Letter.drawNumSign(model, n, pos.x, pos.y, size, Letter.LINE_COLOR, 33, 3, color);
                break;
            }
        }
    }
}

public class NumIndicatorPool : ActorPool<NumIndicator>
{
    public NumIndicatorPool(int n, List<object> args) : base(n, args, () => new NumIndicator())
    {
    }
}

public class NumIndicatorTarget
{
    public Vector pos;
    public int flyingTo;
    public float initialVelRatio;
    public float size;
    public int n;
    public int cnt;
}

public static class NumIndicatorFlyingToType
{
    public const int RIGHT = 0;
    public const int BOTTOM = 1;
}

public static class NumIndicatorIndicatorType
{
    public const int SCORE = 0;
    public const int MULTIPLIER = 1;
}
