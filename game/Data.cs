// Copyright 2004 Kenta Cho. All rights reserved.
using System;

public class EnemySpec
{
    public EnemyPartSpec[] parts;
    public AttackForm[] attackForm;
    public float sizeXm, sizeXp, sizeYm, sizeYp;
    public EnemySpec(EnemyPartSpec[] parts, AttackForm[] forms)
    {
        this.parts = parts; attackForm = forms;
        sizeYm = float.MaxValue; sizeXm = sizeYm;
        sizeYp = 1.17549435e-38f; sizeXp = sizeYp;
        foreach (var p in parts)
        {
            sizeXm = Math.Min(sizeXm, p.ofs.x + p.tumikiSet.sizeXm);
            sizeXp = Math.Max(sizeXp, p.ofs.x + p.tumikiSet.sizeXp);
            sizeYm = Math.Min(sizeYm, p.ofs.y + p.tumikiSet.sizeYm);
            sizeYp = Math.Max(sizeYp, p.ofs.y + p.tumikiSet.sizeYp);
        }
    }
}

public class EnemyPartSpec
{
    public TumikiSet tumikiSet;
    public Vector ofs;
    public float shield, damageToMainBody;
    public int destroyedFormIdx;
}

public class AttackForm
{
    public float shield;
    public int barragePtnStartIdx;
    public int[] attackPeriod, breakPeriod;
}

public class StagePattern
{
    public int randSeed, warningCnt;
    public EnemyAppearancePattern[] pattern;
}

public class EnemyAppearancePattern
{
    public int startTime, duration, interval, posType;
    public float pos, width;
    public bool waitTillEnemiesDestroyed;
    public EnemySpec spec;
    public EnemyMovePattern move;
}

public abstract class EnemyMovePattern { public float deg; }
public class BulletMLMovePattern : EnemyMovePattern { public int parser; public float speed; }
public class MoveRoute { public int index; public float speed; public Vector[] point; }
public class PointsMovePattern : EnemyMovePattern
{
    public const int BASIC_PATTERN_IDX = -1;
    public int withdrawCnt;
    public MoveRoute[] routes;
    public MoveRoute route(int index)
    {
        foreach (var r in routes) if (r.index == index) return r;
        return null;
    }
}

public class FieldPattern
{
    public int randSeed;
    public float scrollSpeed, br, bg, bb, gr, gg, gb, mtr, mtg, mtb;
    public float mrr { get { return (br * 2 + gr) / 3; } }
    public float mrg { get { return (bg * 2 + gg) / 3; } }
    public float mrb { get { return (bb * 2 + gb) / 3; } }
    public FieldLinePattern[] line;
}
public class FieldLinePattern
{
    public TumikiSet[] tumikiSet;
    public int[] interval;
    public float z;
    public int cnt;
    public bool onGround;
}
    public static class AppearancePos { public const int FRONT = 0, TOP = 1; }
