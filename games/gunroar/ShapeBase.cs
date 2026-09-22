// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public interface Drawable
{
    public void draw();
}

public interface Collidable
{
    public Vector getCollision();
    public bool checkCollision(float ax, float ay, Collidable shape = null);
}

public abstract class DrawableShape : Drawable
{
    public DisplayList displayList;
    public void initializeShape()
    {
        displayList = new DisplayList(1);
        displayList.beginNewList();
        createDisplayList();
        displayList.endNewList();
    }

    public abstract void createDisplayList();
    public void close()
    {
        displayList.close();
    }

    public void draw()
    {
        displayList.call(0);
    }
}

public abstract class CollidableDrawable : DrawableShape, Collidable
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

    public Vector _collision;
    public abstract void setCollision();
    public Vector collisionBounds
    {
        get
        {
            return _collision;
        }

        set
        {
            _collision = value;
        }
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
    public void draw()
    {
        glScalef(_size, _size, _size);
        _shape.draw();
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
            if ((_shape is CollidableBaseShape) || (_shape is DestructiveBulletShape) || (_shape is CollidableDrawable))
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
