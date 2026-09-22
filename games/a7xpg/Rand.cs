// Adapted for TinyC# from Phobos random.d (DMD 1.076).
// Copyright 2004-2005 Digital Mars; segments copyright 1997 Rick Booth.
// See PHOBOS-LICENSE.txt.
public class Rand
{
    public static int seed = 1234, index;
    static int[] mix1 = new int[] { -1163302777, 504877868, 62708796, 255054258 };
    static int[] mix2 = new int[] { 1259289432, -394989373, 1767228838, 1437059654 };
    public static void setSeed(int value)
    {
        seed = value;
        index = 0;
    }

    public int nextBits()
    {
        int lo = seed, hi = index;
        index++;
        for (int i = 0; i < 4; i++)
        {
            int old = hi, v = hi ^ mix1[i];
            int a = v & 65535, b = (v >> 16) & 65535;
            int p = a * a + ~(b * b);
            p = ((p >> 16) & 65535) | (p << 16);
            hi = lo ^ ((p ^ mix2[i]) + a * b);
            lo = old;
        }

        return hi;
    }

    public int nextInt(int n)
    {
        int bits = nextBits();
        return (((bits >> 1) & 2147483647) % n * 2 + (bits & 1)) % n;
    }

    public int nextSignedInt(int n)
    {
        return nextInt(n * 2) - n;
    }

    public float nextFloat(float n)
    {
        int bits = nextBits();
        var divisor = PatternNumber.Multiply(new PatternNumber(n, 0), new PatternNumber(10000, 0));
        var value = new PatternNumber((bits >> 16) & 65535, 0);
        for (int shift = 16; shift >= 0; shift -= 8) {
            if (shift < 16) value = PatternNumber.Add(PatternNumber.Multiply(value, new PatternNumber(256, 0)), new PatternNumber((bits >> shift) & 255, 0));
            int quotient = PatternNumber.Integer(PatternNumber.Divide(value, divisor));
            value = PatternNumber.Subtract(value, PatternNumber.Multiply(divisor, new PatternNumber(quotient, 0)));
        }
        return (value.High + value.Low) / 10000;
    }
}
