using System.Collections.Generic;
using static GameMath;

public class Bullet : PatternBody
{
    public static Rand random = new Rand();
    public static BulletPool manager;
    public static Bullet now;
    public static Vector target = new Vector();
    public Vector pos = new Vector(), acc = new Vector();
    public int id;
    public float deg, velocity;
    public Bullet(int id)
    {
        this.id = id;
    }

    public override float Direction
    {
        get
        {
            return rtod(deg);
        }

        set
        {
            deg = dtor(value);
        }
    }

    public override float Speed
    {
        get
        {
            return velocity * 6.2f;
        }

        set
        {
            velocity = value * (10f / 62);
        }
    }

    public override float AccelX
    {
        get
        {
            return acc.x;
        }

        set
        {
            acc.x = value * (10f / 62);
        }
    }

    public override float AccelY
    {
        get
        {
            return acc.y;
        }

        set
        {
            acc.y = value * (10f / 62);
        }
    }

    public override float Aim
    {
        get
        {
            var b = (BulletImpl)this;
            float x = target.x - pos.x;
            return rtod((atan2(-x, target.y - pos.y) * b.xReverse + PI / 2) * b.yReverse - PI / 2);
        }

        set
        {
        }
    }

    public override float Rank
    {
        get
        {
            return patternRank();
        }

        set
        {
        }
    }

    public virtual float patternRank()
    {
        return 0;
    }

    public static void setRandSeed(int value)
    {
        random.setSeed(value);
    }

    public static PatternState[] createRunner(int pattern)
    {
        int[] roots = BarrageCode.Roots(pattern);
        var states = new PatternState[roots.Length];
        for (int i = 0; i < roots.Length; i++)
            states[i] = BarrageCode.Create(roots[i], new float[3]);
        return states;
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
        now = this;
        manager.world.Turn = manager.cnt;
        if (!((PatternEnded())))
            RunPattern(manager.world);
    }

    public bool isEnd()
    {
        return PatternEnded();
    }

    public void remove_0()
    {
        Scripts.Clear();
    }
}

public class McdPatternWorld : PatternWorld
{
    public BulletPool pool;
    public McdPatternWorld(BulletPool pool)
    {
        this.pool = pool;
    }

    public override float RandomValue()
    {
        return Bullet.random.nextFloat(1);
    }

    public override void Fire(PatternBody parent, float direction, float speed, int program, float[] args)
    {
        if (program < 0)
            pool.addSimpleBullet(dtor(direction), speed * (10f / 62));
        else
            pool.addScriptBullet(BarrageCode.Create(program, args), dtor(direction), speed * (10f / 62));
    }

    public override void Vanish(PatternBody body)
    {
        pool.actor[((Bullet)body).id].remove_0();
    }
}
