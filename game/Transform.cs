using System;

public static class Transform
{
    public static float[] Identity() { return new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }; }
    public static float[] Copy(float[] src)
    {
        var dst = new float[16];
        for (int i = 0; i < 16; i++) dst[i] = src[i];
        return dst;
    }
    public static float[] Multiply(float[] matrix, float[] right)
    {
        var result = new float[16];
        for (int i = 0; i < 16; i++) result[i] = 0;
        for (int column = 0; column < 4; column++)
            for (int row = 0; row < 4; row++)
                for (int k = 0; k < 4; k++) result[column * 4 + row] += matrix[k * 4 + row] * right[column * 4 + k];
        return result;
    }
    public static float[] Translate(float[] source, float x, float y, float z)
    {
        var matrix = Copy(source);
        for (int i = 0; i < 4; i++) matrix[12 + i] += matrix[i] * x + matrix[4 + i] * y + matrix[8 + i] * z;
        return matrix;
    }
    public static float[] Scale(float[] source, float x, float y, float z)
    {
        var matrix = Copy(source);
        for (int i = 0; i < 4; i++) { matrix[i] *= x; matrix[4 + i] *= y; matrix[8 + i] *= z; }
        return matrix;
    }
    public static float[] Rotate(float[] matrix, float degrees, float x, float y, float z)
    {
        float length = (float)Math.Sqrt(x * x + y * y + z * z);
        if (length == 0) return matrix;
        x /= length; y /= length; z /= length;
        float c = (float)Math.Cos(degrees * (GameMath.PI / 180)), s = (float)Math.Sin(degrees * (GameMath.PI / 180)), t = 1 - c;
        return Multiply(matrix, new float[] {
            t*x*x+c, t*x*y+s*z, t*x*z-s*y, 0,
            t*x*y-s*z, t*y*y+c, t*y*z+s*x, 0,
            t*x*z+s*y, t*y*z-s*x, t*z*z+c, 0, 0,0,0,1 });
    }
    public static float[] Perspective()
    {
        return new float[] { 1,0,0,0, 0,4f/3,0,0, 0,0,-1000f/999.9f,-1, 0,0,-100f/999.9f,0 };
    }
    public static float[] Ortho()
    {
        return new float[] { 1f/320,0,0,0, 0,-1f/240,0,0, 0,0,-0.5f,0, -1,1,0.5f,1 };
    }
}
