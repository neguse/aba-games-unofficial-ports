public class Random
{
    int[] seed = MmArrays.Make(56, () => 0);
    int storedNext, nextp = 21;
    public Random(int value = 1)
    {
        if (value < 0)
            value = -value;
        int mj = 161803398 - value;
        seed[55] = mj;
        int mk = 1;
        for (int i = 1; i < 55; i++)
        {
            int ii = (21 * i) % 55;
            seed[ii] = mk;
            mk = mj - mk;
            if (mk < 0)
                mk += 2147483647;
            mj = seed[ii];
        }

        for (int k = 1; k < 5; k++)
            for (int i = 1; i < 56; i++)
            {
                seed[i] -= seed[1 + (i + 30) % 55];
                if (seed[i] < 0)
                    seed[i] += 2147483647;
            }
    }

    public int Next()
    {
        storedNext++;
        if (storedNext >= 56)
            storedNext = 1;
        nextp++;
        if (nextp >= 56)
            nextp = 1;
        int value = seed[storedNext] - seed[nextp];
        if (value == 2147483647)
            value--;
        if (value < 0)
            value += 2147483647;
        seed[storedNext] = value;
        return value;
    }

    public float NextDouble()
    {
        return Next() / 2147483647f;
    }

    public int Nextint(int max)
    {
        // Keep the integer boundary from System.Random's double-precision scaling.
        int sample = Next();
        var value = PatternNumber.Add(new PatternNumber(((sample >> 16) / 1f) * 65536f, 0), new PatternNumber(sample & 65535, 0));
        var ratio = PatternNumber.Divide(value, new PatternNumber(2147483648f, -1));
        return PatternNumber.Integer(PatternNumber.Multiply(ratio, new PatternNumber(max, 0)));
    }

    public int Nextintint(int min, int max)
    {
        return Nextint(max - min) + min;
    }
}
