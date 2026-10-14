// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public interface Drawable
{
    public void draw(float[] model, float[] color = null, Gfx.Blend blend = Gfx.Blend.Additive);
}

public interface Collidable
{
    public Vector getCollision();
    public bool checkCollision(float ax, float ay, Collidable shape = null);
}

public interface DrawableShape : Drawable
{
    public void close();
}

public static class ShapeMesh
{
    static int nextMesh;
    public static Mesh create()
    {
        Mesh mesh = new Mesh("shape-" + nextMesh.ToString()); nextMesh++;
        return mesh;
    }

    public static void draw(Mesh mesh, int opaqueCount, float[] model, float[] color, Gfx.Blend blend)
    {
        if (opaqueCount > 0)
            Gfx.Draw(opaqueCount, mesh.Bindings(model, color, 1, false),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.None });
        if (mesh.count > opaqueCount)
            Gfx.Draw(mesh.count - opaqueCount, mesh.Bindings(model, color, 1, blend == Gfx.Blend.Additive, opaqueCount / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }
}

public class ResizableDrawable : Drawable, Collidable
{
    public Vector getCollision()
    {
        return collisionBounds;
    }

    public bool checkCollision(float ax, float ay, Collidable shape = null)
    {
        float cx = 0, cy = 0;
        if (shape != null)
        {
            cx = collisionBounds.x + shape.getCollision().x;
            cy = collisionBounds.y + shape.getCollision().y;
        }
        else
        {
            cx = collisionBounds.x;
            cy = collisionBounds.y;
        }

        if ((ax <= cx) && (ay <= cy))
            return true;
        else
            return false;
    }

    public Drawable _shape;
    public float _size;
    public Vector _collision;
    public void draw(float[] model, float[] color = null, Gfx.Blend blend = Gfx.Blend.Additive)
    {
        model = Transform.Scale(model, _size, _size, _size);
        _shape.draw(model, color, blend);
    }

    public Drawable shape
    {
        get
        {
            return _shape;
        }

        set
        {
            _collision = new Vector();
            _shape = value;
        }
    }

    public float size
    {
        get
        {
            return _size;
        }

        set
        {
            _size = value;
        }
    }

    public Vector collisionBounds
    {
        get
        {
            Collidable cd = (Collidable)_shape;
            if ((_shape is CollidableBaseShape) || (_shape is DestructiveBulletShape) || (_shape is ShotShape))
            {
                _collision.x = cd.getCollision().x * _size;
                _collision.y = cd.getCollision().y * _size;
                return _collision;
            }
            else
            {
                return null;
            }
        }
    }
}
