// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class DisplayList
{
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
        glNewList(enumIdx, GL_COMPILE);
    }

    public void endNewList()
    {
        glEndList();
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
    }

    public void call(int i)
    {
        glCallList(idx + i);
    }

    public void close()
    {
        glDeleteLists(idx, num);
    }
}
