// Copyright 2003-2004 Kenta Cho. All rights reserved.
using System;

public static class GameMath
{
    public const float PI = 3.1415927f;
    public static float sin(float value) { return (float)Math.Sin(value); }
    public static float cos(float value) { return (float)Math.Cos(value); }
    public static float sqrt(float value) { return (float)Math.Sqrt(value); }
    public static float fabs(float value) { return Math.Abs(value); }
    public static float atan2(float y, float x) { return (float)Math.Atan2(y, x); }
    public static float rtod(float value) { return value * 180 / PI; }
    public static float dtor(float value) { return value * PI / 180; }
    public static int integer(float value) { return value < 0 ? -(int)Math.Floor(-value) : (int)Math.Floor(value); }
    public static int parseNonnegative(string text)
    {
        if (text.Length == 0 || text.Length > 10) return -1;
        int value = 0;
        for (int i = 0; i < text.Length; i++)
        {
            int digit = "0123456789".IndexOf(text.Substring(i, 1));
            if (digit < 0 || value > 214748364 || (value == 214748364 && digit > 7)) return -1;
            value = value * 10 + digit;
        }
        return value;
    }
}

public class Vector
{
    public float x, y;
    public Vector(float? x = null, float? y = null) { this.x = x ?? 0; this.y = y ?? 0; }
    public void add(Vector v) { x += v.x; y += v.y; }
    public void sub(Vector v) { x -= v.x; y -= v.y; }
    public void mul(float a) { x *= a; y *= a; }
    public void div(float a) { x /= a; y /= a; }
    public float size { get { return (float)Math.Sqrt(x * x + y * y); } }
    public float dist(Vector v)
    {
        float ax = Math.Abs(x - v.x), ay = Math.Abs(y - v.y);
        return ax > ay ? ax + ay / 2 : ay + ax / 2;
    }
    public float checkSide(Vector a, Vector b) { return Side(x, y, a, b); }
    public float checkSideOffset(Vector a, Vector b, Vector offset) { return Side(x - offset.x, y - offset.y, a, b); }
    static float Side(float x, float y, Vector a, Vector b)
    {
        float dx = b.x - a.x, dy = b.y - a.y;
        if (dx == 0) return dy == 0 ? 0 : dy > 0 ? x - a.x : a.x - x;
        if (dy == 0) return dx > 0 ? a.y - y : y - a.y;
        float side = (x - a.x) / dx - (y - a.y) / dy;
        return dx * dy > 0 ? side : -side;
    }
}

public interface BulletTarget { Vector getTargetPos(); }
public class VirtualBulletTarget : BulletTarget
{
    public Vector pos = new Vector();
    public Vector getTargetPos() { return pos; }
}

public interface ActorInitializer { }
public abstract class Actor
{
    public bool isExist;
    public abstract Actor newActor();
    public abstract void init(ActorInitializer initializer);
    public abstract void move();
    public abstract void draw();
}

public class ActorPool
{
    public Actor[] actor;
    public int actorIdx;
    public ActorPool(int count, Actor prototype, ActorInitializer initializer)
    {
        actor = new Actor[count];
        for (int i = 0; i < count; i++)
        {
            actor[i] = prototype.newActor();
            actor[i].init(initializer);
        }
        actorIdx = count;
    }
    public Actor getInstance()
    {
        for (int i = 0; i < actor.Length; i++)
        {
            actorIdx--;
            if (actorIdx < 0) actorIdx = actor.Length - 1;
            if (!actor[actorIdx].isExist) return actor[actorIdx];
        }
        return null;
    }
    public Actor getInstanceForced()
    {
        actorIdx--;
        if (actorIdx < 0) actorIdx = actor.Length - 1;
        return actor[actorIdx];
    }
    public virtual void move() { foreach (var a in actor) if (a.isExist) a.move(); }
    public void draw() { foreach (var a in actor) if (a.isExist) a.draw(); }
    public virtual void clear() { foreach (var a in actor) a.isExist = false; }
}
