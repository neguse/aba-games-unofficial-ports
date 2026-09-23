// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class DisplayList
{
    public bool registered;
    public int num;
    public int idx;
    public int enumIdx;
    public DisplayList(int num)
    {
        this.num = num;
        idx = glGenLists(num);
    }

    public virtual void beginNewList()
    {
        resetList();
        newList();
    }

    public virtual void nextNewList()
    {
        glEndList();
        enumIdx++;
        if ((enumIdx >= idx + num) || (enumIdx < idx))
            return;
        glNewList(enumIdx, GL_COMPILE);
    }

    public virtual void endNewList()
    {
        glEndList();
        registered = true;
    }

    public virtual void resetList()
    {
        enumIdx = idx;
    }

    public virtual void newList()
    {
        glNewList(enumIdx, GL_COMPILE);
    }

    public virtual void endList()
    {
        glEndList();
        enumIdx++;
        registered = true;
    }

    public virtual void call(int i = 0)
    {
        glCallList(idx + i);
    }

    public virtual void close()
    {
        if (!(registered))
            return;
        glDeleteLists(idx, num);
    }
}
