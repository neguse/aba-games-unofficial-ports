using System;

public static class FrameMath
{
    // 行優先・行ベクトル。結果バッファは入力と別にする。
    public static void Multiply(float[] result, float[] left, float[] right)
    {
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                result[row * 4 + column] = left[row * 4] * right[column]
                    + left[row * 4 + 1] * right[4 + column]
                    + left[row * 4 + 2] * right[8 + column]
                    + left[row * 4 + 3] * right[12 + column];
    }

    public static void Transpose(float[] result, float[] matrix)
    {
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                result[row * 4 + column] = matrix[column * 4 + row];
    }

    public static void LookAt(float[] result, float[] from, float[] to, float[] up, float scale)
    {
        float zx = from[0] - to[0], zy = from[1] - to[1], zz = from[2] - to[2];
        float length = (float)Math.Sqrt(zx * zx + zy * zy + zz * zz);
        zx /= length; zy /= length; zz /= length;
        float xx = up[1] * zz - up[2] * zy, xy = up[2] * zx - up[0] * zz, xz = up[0] * zy - up[1] * zx;
        length = (float)Math.Sqrt(xx * xx + xy * xy + xz * xz);
        xx /= length; xy /= length; xz /= length;
        float yx = zy * xz - zz * xy, yy = zz * xx - zx * xz, yz = zx * xy - zy * xx;
        result[0] = xx * scale; result[1] = yx * scale; result[2] = zx * scale; result[3] = 0;
        result[4] = xy * scale; result[5] = yy * scale; result[6] = zy * scale; result[7] = 0;
        result[8] = xz * scale; result[9] = yz * scale; result[10] = zz * scale; result[11] = 0;
        result[12] = -(xx * from[0] + xy * from[1] + xz * from[2]) * scale;
        result[13] = -(yx * from[0] + yy * from[1] + yz * from[2]) * scale;
        result[14] = -(zx * from[0] + zy * from[1] + zz * from[2]) * scale;
        result[15] = 1;
    }
}
