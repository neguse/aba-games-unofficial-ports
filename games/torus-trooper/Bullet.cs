using System.Collections.Generic;
using static GameMath;

public static class Bullet
{
    public static Rand random = new Rand();
    public static BulletActorPool manager;
    public static BulletImpl now;
    public static Vector target = new Vector();
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
}

public class TtWorld : PatternWorld
{
    public float Turn;
    public BulletActorPool pool;
    public TtWorld(BulletActorPool pool)
    {
        this.pool = pool;
    }

    public float GetTurn()
    {
        return Turn;
    }

    public float RandomValue()
    {
        return Bullet.random.nextFloat(1);
    }

    public void Fire(PatternBody parent, float direction, float speed, int program, float[] args)
    {
        if (program < 0)
            pool.addSimpleBullet(dtor(direction), speed * (10f / 62));
        else
            pool.addScriptBullet(BarrageCode.Create(program, args), dtor(direction), speed * (10f / 62));
    }

    public void Vanish(PatternBody body)
    {
        pool.actor[((BulletImpl)body).id].remove();
    }
}
