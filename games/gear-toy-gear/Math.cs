using System;

public class Vector2
{
    public static Vector2 Transform(Vector2 v, Matrix m)
    {
        return new Vector2(v.X * m.M[(0)] + v.Y * m.M[(4)] + m.M[(12)], v.X * m.M[(1)] + v.Y * m.M[(5)] + m.M[(13)]);
    }

    public float X, Y;
    public Vector2(float x = 0, float y = 0)
    {
        X = x;
        Y = y;
    }

    public Vector2 Copy()
    {
        return new Vector2(X, Y);
    }

    public void Set(Vector2 value)
    {
        X = value.X;
        Y = value.Y;
    }

    public static Vector2 Zero
    {
        get
        {
            return new Vector2();
        }
    }

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2 operator -(Vector2 a)
    {
        return new Vector2(-a.X, -a.Y);
    }

    public static Vector2 operator *(Vector2 a, float b)
    {
        return new Vector2(a.X * b, a.Y * b);
    }

    public static Vector2 operator /(Vector2 a, float b)
    {
        return new Vector2(a.X / b, a.Y / b);
    }

    public static Vector2 operator *(float b, Vector2 a)
    {
        return (a * b).Copy();
    }

    public static float Dot(Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public float LengthSquared()
    {
        return Dot((this).Copy(), (this).Copy());
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSquared());
    }

    public void Normalize()
    {
        float n = Length();
        X /= n;
        Y /= n;
    }

    public static float Distance(Vector2 a, Vector2 b)
    {
        return (a - b).Length();
    }
}

public class Vector3
{
    public static Vector3 Reflect(Vector3 v, Vector3 n)
    {
        return (v - n * (2 * Dot((v).Copy(), (n).Copy()))).Copy();
    }

    public float X, Y, Z;
    public Vector3(float x = 0, float y = 0, float z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 FromVector2(Vector2 xy, float z)
    {
        return new Vector3(xy.X, xy.Y, z);
    }

    public Vector3 Copy()
    {
        return new Vector3(X, Y, Z);
    }

    public void Set(Vector3 value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
    }

    public static Vector3 Zero
    {
        get
        {
            return new Vector3();
        }
    }

    public static Vector3 Up
    {
        get
        {
            return new Vector3(0, 1, 0);
        }
    }

    public static Vector3 Backward
    {
        get
        {
            return new Vector3(0, 0, 1);
        }
    }

    public static Vector3 Forward
    {
        get
        {
            return new Vector3(0, 0, -1);
        }
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3 operator -(Vector3 a)
    {
        return new Vector3(-a.X, -a.Y, -a.Z);
    }

    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3(a.X * b, a.Y * b, a.Z * b);
    }

    public static Vector3 operator /(Vector3 a, float b)
    {
        return new Vector3(a.X / b, a.Y / b, a.Z / b);
    }

    public static Vector3 operator *(float b, Vector3 a)
    {
        return (a * b).Copy();
    }

    public static float Dot(Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public float LengthSquared()
    {
        return Dot((this).Copy(), (this).Copy());
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSquared());
    }

    public void Normalize()
    {
        float n = Length();
        X /= n;
        Y /= n;
        Z /= n;
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        return (a - b).Length();
    }

    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    }

    public static Vector3 TransformVector3Matrix(Vector3 v, Matrix m)
    {
        return new Vector3(v.X * m.M[(0)] + v.Y * m.M[(4)] + v.Z * m.M[(8)] + m.M[(12)], v.X * m.M[(1)] + v.Y * m.M[(5)] + v.Z * m.M[(9)] + m.M[(13)], v.X * m.M[(2)] + v.Y * m.M[(6)] + v.Z * m.M[(10)] + m.M[(14)]);
    }

    public static Vector3 TransformVector3Quaternion(Vector3 v, Quaternion q)
    {
        return (TransformVector3Matrix((v).Copy(), (Matrix.CreateFromQuaternion((q).Copy())).Copy())).Copy();
    }
}

public class Vector4
{
    public static Vector4 Transform(Vector3 v, Matrix m)
    {
        return (TransformVector4Matrix((Vector4.FromVector3((v).Copy(), 1)).Copy(), (m).Copy())).Copy();
    }

    public static Vector4 TransformVector4Matrix(Vector4 v, Matrix m)
    {
        return new Vector4(v.X * m.M[(0)] + v.Y * m.M[(4)] + v.Z * m.M[(8)] + v.W * m.M[(12)], v.X * m.M[(1)] + v.Y * m.M[(5)] + v.Z * m.M[(9)] + v.W * m.M[(13)], v.X * m.M[(2)] + v.Y * m.M[(6)] + v.Z * m.M[(10)] + v.W * m.M[(14)], v.X * m.M[(3)] + v.Y * m.M[(7)] + v.Z * m.M[(11)] + v.W * m.M[(15)]);
    }

    public float X, Y, Z, W;
    public Vector4(float x = 0, float y = 0, float z = 0, float w = 0)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Vector4 FromVector3(Vector3 xyz, float w)
    {
        return new Vector4(xyz.X, xyz.Y, xyz.Z, w);
    }

    public Vector4 Copy()
    {
        return new Vector4(X, Y, Z, W);
    }

    public void Set(Vector4 value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        W = value.W;
    }

    public static Vector4 Zero
    {
        get
        {
            return new Vector4();
        }
    }

    public static Vector4 operator +(Vector4 a, Vector4 b)
    {
        return new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    }

    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        return new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }

    public static Vector4 operator -(Vector4 a)
    {
        return new Vector4(-a.X, -a.Y, -a.Z, -a.W);
    }

    public static Vector4 operator *(Vector4 a, float b)
    {
        return new Vector4(a.X * b, a.Y * b, a.Z * b, a.W * b);
    }

    public static Vector4 operator /(Vector4 a, float b)
    {
        return new Vector4(a.X / b, a.Y / b, a.Z / b, a.W / b);
    }

    public static Vector4 operator *(float b, Vector4 a)
    {
        return (a * b).Copy();
    }

    public static float Dot(Vector4 a, Vector4 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
    }

    public float LengthSquared()
    {
        return Dot((this).Copy(), (this).Copy());
    }

    public float Length()
    {
        return (float)Math.Sqrt(LengthSquared());
    }

    public void Normalize()
    {
        float n = Length();
        X /= n;
        Y /= n;
        Z /= n;
        W /= n;
    }

    public static float Distance(Vector4 a, Vector4 b)
    {
        return (a - b).Length();
    }
}

public class Quaternion
{
    public void Normalize()
    {
        float n = (float)System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        X /= n;
        Y /= n;
        Z /= n;
        W /= n;
    }

    public static Quaternion Concatenate(Quaternion a, Quaternion b)
    {
        return (b * a).Copy();
    }

    public float X, Y, Z, W;
    public Quaternion(float x = 0, float y = 0, float z = 0, float w = 0)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Quaternion Copy()
    {
        return new Quaternion(X, Y, Z, W);
    }

    public static Quaternion Identity
    {
        get
        {
            return new Quaternion(0, 0, 0, 1);
        }
    }

    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y, a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X, a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W, a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
    }

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float s = (float)Math.Sin(angle / 2);
        return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, (float)Math.Cos(angle / 2));
    }

    public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
        float sr = (float)Math.Sin(roll / 2), cr = (float)Math.Cos(roll / 2), sp = (float)Math.Sin(pitch / 2), cp = (float)Math.Cos(pitch / 2), sy = (float)Math.Sin(yaw / 2), cy = (float)Math.Cos(yaw / 2);
        return new Quaternion(cy * sp * cr + sy * cp * sr, sy * cp * cr - cy * sp * sr, cy * cp * sr - sy * sp * cr, cy * cp * cr + sy * sp * sr);
    }

    public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
    {
        float sign = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W >= 0 ? 1 : -1;
        var q = new Quaternion((1 - t) * a.X + sign * t * b.X, (1 - t) * a.Y + sign * t * b.Y, (1 - t) * a.Z + sign * t * b.Z, (1 - t) * a.W + sign * t * b.W);
        float inv = 1 / (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        q.X *= inv;
        q.Y *= inv;
        q.Z *= inv;
        q.W *= inv;
        return (q).Copy();
    }
}

public class Matrix
{
    public float[] M = GtgArrays.Make(16, () => 0f);
    public Matrix Copy()
    {
        var m = new Matrix();
        for (int i = 0; i < 16; i++)
            m.M[(i)] = M[(i)];
        return m;
    }

    public static Matrix Identity
    {
        get
        {
            var m = new Matrix();
            for (int i = 0; i < 4; i++)
                m.M[(i * 5)] = 1;
            return (m).Copy();
        }
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        var m = new Matrix();
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                for (int k = 0; k < 4; k++)
                    m.M[(y * 4 + x)] += a.M[(y * 4 + k)] * b.M[(k * 4 + x)];
        return (m).Copy();
    }

    public static Matrix CreateTranslationVector3(Vector3 p)
    {
        return (CreateTranslationfloatfloatfloat(p.X, p.Y, p.Z)).Copy();
    }

    public static Matrix CreateTranslationfloatfloatfloat(float x, float y, float z)
    {
        var m = (Identity).Copy();
        m.M[(12)] = x;
        m.M[(13)] = y;
        m.M[(14)] = z;
        return (m).Copy();
    }

    public static Matrix CreateScalefloat(float s)
    {
        return (CreateScalefloatfloatfloat(s, s, s)).Copy();
    }

    public static Matrix CreateScalefloatfloatfloat(float x, float y, float z)
    {
        var m = (Identity).Copy();
        m.M[(0)] = x;
        m.M[(5)] = y;
        m.M[(10)] = z;
        return (m).Copy();
    }

    public static Matrix CreateFromQuaternion(Quaternion q)
    {
        var m = (Identity).Copy();
        float xx = q.X * q.X, yy = q.Y * q.Y, zz = q.Z * q.Z, xy = q.X * q.Y, xz = q.X * q.Z, yz = q.Y * q.Z, xw = q.X * q.W, yw = q.Y * q.W, zw = q.Z * q.W;
        m.M[(0)] = 1 - 2 * (yy + zz);
        m.M[(1)] = 2 * (xy + zw);
        m.M[(2)] = 2 * (xz - yw);
        m.M[(4)] = 2 * (xy - zw);
        m.M[(5)] = 1 - 2 * (xx + zz);
        m.M[(6)] = 2 * (yz + xw);
        m.M[(8)] = 2 * (xz + yw);
        m.M[(9)] = 2 * (yz - xw);
        m.M[(10)] = 1 - 2 * (xx + yy);
        return (m).Copy();
    }

    public static Matrix CreateFromYawPitchRoll(float y, float p, float r)
    {
        return (CreateFromQuaternion((Quaternion.CreateFromYawPitchRoll(y, p, r)).Copy())).Copy();
    }

    public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
    {
        return (CreateFromQuaternion((Quaternion.CreateFromAxisAngle((axis).Copy(), angle)).Copy())).Copy();
    }

    public static Matrix CreateLookAt(Vector3 eye, Vector3 target, Vector3 up)
    {
        Vector3 z = (eye - target).Copy();
        z.Normalize();
        Vector3 x = (Vector3.Cross((up).Copy(), (z).Copy())).Copy();
        x.Normalize();
        Vector3 y = (Vector3.Cross((z).Copy(), (x).Copy())).Copy();
        var m = (Identity).Copy();
        m.M[(0)] = x.X;
        m.M[(1)] = y.X;
        m.M[(2)] = z.X;
        m.M[(4)] = x.Y;
        m.M[(5)] = y.Y;
        m.M[(6)] = z.Y;
        m.M[(8)] = x.Z;
        m.M[(9)] = y.Z;
        m.M[(10)] = z.Z;
        m.M[(12)] = -Vector3.Dot((x).Copy(), (eye).Copy());
        m.M[(13)] = -Vector3.Dot((y).Copy(), (eye).Copy());
        m.M[(14)] = -Vector3.Dot((z).Copy(), (eye).Copy());
        return (m).Copy();
    }

    public static Matrix CreatePerspectiveFieldOfView(float fov, float aspect, float near, float far)
    {
        var m = new Matrix();
        float y = 1 / (float)Math.Tan(fov / 2);
        m.M[(0)] = y / aspect;
        m.M[(5)] = y;
        m.M[(10)] = far / (near - far);
        m.M[(11)] = -1;
        m.M[(14)] = near * far / (near - far);
        return (m).Copy();
    }
}

public class Color
{
    public int R, G, B, A;
    public Color(int r = 0, int g = 0, int b = 0, int a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color Copy()
    {
        return new Color(R, G, B, A);
    }
}

public class VertexPositionColor
{
    public Vector3 Position = new Vector3();
    public Color Color = new Color();
    public VertexPositionColor(Vector3 p = null, Color c = null)
    {
        Position = (p == null ? new Vector3() : p.Copy()).Copy();
        Color = (c == null ? new Color() : c.Copy()).Copy();
    }
}

public class VertexPositionNormalTexture
{
    public Vector3 Position = new Vector3(), Normal = new Vector3();
    public Vector2 TextureCoordinate = new Vector2();
    public VertexPositionNormalTexture(Vector3 p = null, Vector3 n = null, Vector2 uv = null)
    {
        Position = (p == null ? new Vector3() : p.Copy()).Copy();
        Normal = (n == null ? new Vector3() : n.Copy()).Copy();
        TextureCoordinate = (uv == null ? new Vector2() : uv.Copy()).Copy();
    }
}
