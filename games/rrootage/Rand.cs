// Copyright 2004 Kenta Cho. All rights reserved.
// Copyright 1997-2002 Makoto Matsumoto and Takuji Nishimura; 2003 Andrew C. Edwards.
public class Rand
{
    public int[] state = new int[624];
    public int index = 624;
    public Rand()
    {
        setSeed(5489);
    }

    public virtual void setSeed(int seed)
    {
        state[0] = seed;
        for (int i = 1; i < 624; i++)
            state[i] = 1812433253 * (state[i - 1] ^ ((state[i - 1] >> 30) & 3)) + i;
        index = 624;
    }

    public virtual int nextBits()
    {
        if (index >= 624)
        {
            for (int i = 0; i < 624; i++)
            {
                int mix = (state[i] & -2147483648) | (state[(i + 1) % 624] & 2147483647);
                state[i] = state[(i + 397) % 624] ^ ((mix >> 1) & 2147483647) ^ ((mix & 1) != 0 ? -1727483681 : 0);
            }

            index = 0;
        }

        int y = state[index];
        index++;
        y = y ^ ((y >> 11) & 2097151);
        y = y ^ ((y << 7) & -1658038656);
        y = y ^ ((y << 15) & -272236544);
        y = y ^ ((y >> 18) & 16383);
        return y;
    }

    public virtual int nextInt(int n)
    {
        if (n == 0)
            return 0;
        int bits = nextBits();
        return ((((bits >> 1) & 2147483647) % n) * 2 + (bits & 1)) % n;
    }

    public virtual int nextInt32()
    {
        return nextBits();
    }

    public virtual int nextSignedInt(int n)
    {
        if (n == 0)
            return 0;
        return nextInt(n * 2) - n;
    }

    public virtual float nextFloat(float n)
    {
        int bits = nextBits();
        float value = ((bits >> 1) & 2147483647) / 1f;
        value = value * 2 + (bits & 1);
        return value / 4294967295f * n;
    }

    public virtual float nextSignedFloat(float n)
    {
        return nextFloat(n * 2) - n;
    }
}
