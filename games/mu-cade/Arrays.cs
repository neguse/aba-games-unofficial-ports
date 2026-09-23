using System;

public static class McdArrays
{
    public static T[] Make<T>(int n, Func<T> create)
    {
        var a = new T[n];
        for (int i = 0; i < n; i++)
            a[i] = create();
        return a;
    }

    public static T[] Append<T>(T[] a, T value)
    {
        int n = a == null ? 0 : a.Length;
        var result = new T[n + 1];
        for (int i = 0; i < n; i++)
            result[i] = a[i];
        result[n] = value;
        return result;
    }
}
