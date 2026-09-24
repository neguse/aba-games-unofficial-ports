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
    public static float[] Perspective(float far = 1000, float scale = 1)
    {
        return new float[] { scale,0,0,0, 0,4f/3 * scale,0,0, 0,0,-far/(far - 0.1f),-1, 0,0,-far * 0.1f/(far - 0.1f),0 };
    }
    public static float[] LookAt(float[] matrix, float ex, float ey, float ez, float lx, float ly, float lz, float ux, float uy, float uz)
    {
        float fx = lx - ex, fy = ly - ey, fz = lz - ez;
        float length = (float)Math.Sqrt(fx * fx + fy * fy + fz * fz);
        fx /= length; fy /= length; fz /= length;
        float sx = fy * uz - fz * uy, sy = fz * ux - fx * uz, sz = fx * uy - fy * ux;
        length = (float)Math.Sqrt(sx * sx + sy * sy + sz * sz);
        sx /= length; sy /= length; sz /= length;
        float tx = sy * fz - sz * fy, ty = sz * fx - sx * fz, tz = sx * fy - sy * fx;
        matrix = Multiply(matrix, new float[] { sx, tx, -fx, 0, sy, ty, -fy, 0, sz, tz, -fz, 0, 0, 0, 0, 1 });
        return Translate(matrix, -ex, -ey, -ez);
    }
    public static float[] Ortho()
    {
        return new float[] { 1f/320,0,0,0, 0,-1f/240,0,0, 0,0,-0.5f,0, -1,1,0.5f,1 };
    }
}
