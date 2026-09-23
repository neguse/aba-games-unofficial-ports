using System;

public static class MmArrays
{
    public static T[] Make<T>(int n, Func<T> create)
    {
        var a = new T[n];
        for (int i = 0; i < n; i++)
            a[i] = create();
        return a;
    }
}
