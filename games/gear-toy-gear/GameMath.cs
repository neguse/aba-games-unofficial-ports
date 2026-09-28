using System;

public static class GameMath
{
    public static Quaternion Rotation(Quaternion rotation)
    {
        if (SimulationTime.Step == 1) return rotation;
        float w = Math.Max(-1, Math.Min(1, rotation.W));
        float angle = (float)Math.Atan2(Math.Sqrt(rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z), w);
        float sine = (float)Math.Sin(angle);
        if (Math.Abs(sine) < .000001f) return rotation;
        float scale = (float)Math.Sin(angle * SimulationTime.Step) / sine;
        return new Quaternion(rotation.X * scale, rotation.Y * scale, rotation.Z * scale,
            (float)Math.Cos(angle * SimulationTime.Step));
    }

    public static int integer(float value)
    {
        return (int)(value < 0 ? Math.Ceiling(value) : Math.Floor(value));
    }

    public static float sin(float value)
    {
        return (float)Math.Sin(value);
    }

    public static float cos(float value)
    {
        return (float)Math.Cos(value);
    }

    public static float sqrt(float value)
    {
        return (float)Math.Sqrt(value);
    }

    public static int parseNonnegative(string text)
    {
        if (text.Length == 0 || text.Length > 10)
            return -1;
        int value = 0;
        for (int i = 0; i < text.Length; i++)
        {
            int digit = "0123456789".IndexOf(text.Substring(i, 1));
            if (digit < 0 || value > 214748364 || (value == 214748364 && digit > 7))
                return -1;
            value = value * 10 + digit;
        }

        return value;
    }
}
