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

    public void beginNewList()
    {
        resetList();
        newList();
    }

    public void nextNewList()
    {
        glEndList();
        enumIdx++;
        if ((enumIdx >= idx + num) || (enumIdx < idx))
            return;
        glNewList(enumIdx, GL_COMPILE);
    }

    public void endNewList()
    {
        glEndList();
        registered = true;
    }

    public void resetList()
    {
        enumIdx = idx;
    }

    public void newList()
    {
        glNewList(enumIdx, GL_COMPILE);
    }

    public void endList()
    {
        glEndList();
        enumIdx++;
        registered = true;
    }

    public void call(int i = 0)
    {
        glCallList(idx + i);
    }

    public void close()
    {
        if (!registered)
            return;
        glDeleteLists(idx, num);
    }
}
