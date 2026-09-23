using System;

public static class GtgArrays
{
    public static int[] CopyInts(int[] a)
    {
        int[] b = Make(a.Length, () => 0);
        for (int i = 0; i < a.Length; i++)
            b[i] = a[i];
        return b;
    }

    public static T[] Make<T>(int n, Func<T> create)
    {
        var a = new T[n];
        for (int i = 0; i < n; i++)
            a[(i)] = create();
        return a;
    }
}
