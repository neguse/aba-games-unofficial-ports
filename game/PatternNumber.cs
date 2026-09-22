using System;

// A low component keeps frame and repeat counts on the original side of integer boundaries.
public class PatternNumber
{
    public float High, Low;

    public PatternNumber(float high, float low) { High = high; Low = low; }

    public static PatternNumber Add(PatternNumber a, PatternNumber b)
    {
        float sum = a.High + b.High;
        float part = sum - a.High;
        float error = (a.High - (sum - part)) + (b.High - part) + a.Low + b.Low;
        float high = sum + error;
        return new PatternNumber(high, error - (high - sum));
    }

    public static PatternNumber Negate(PatternNumber a) { return new PatternNumber(-a.High, -a.Low); }
    public static PatternNumber Subtract(PatternNumber a, PatternNumber b) { return Add(a, Negate(b)); }

    public static PatternNumber Multiply(PatternNumber a, PatternNumber b)
    {
        float product = a.High * b.High;
        float splitA = a.High * 4097f, splitB = b.High * 4097f;
        float ah = splitA - (splitA - a.High), bh = splitB - (splitB - b.High);
        float al = a.High - ah, bl = b.High - bh;
        float error = ((ah * bh - product) + ah * bl + al * bh) + al * bl;
        error += a.High * b.Low + a.Low * b.High + a.Low * b.Low;
        float high = product + error;
        return new PatternNumber(high, error - (high - product));
    }

    public static PatternNumber Divide(PatternNumber a, PatternNumber b)
    {
        float quotient = a.High / b.High;
        var residual = Subtract(a, Multiply(b, new PatternNumber(quotient, 0)));
        float error = (residual.High + residual.Low) / b.High;
        float high = quotient + error;
        return new PatternNumber(high, error - (high - quotient));
    }

    public static int Integer(PatternNumber value)
    {
        if (value.High < 0 || (value.High == 0 && value.Low < 0)) return -Integer(Negate(value));
        int result = (int)Math.Floor(value.High);
        if (value.High == result && value.Low < 0) result--;
        return result;
    }
}
