using System;
using System.Collections.Generic;

public interface PatternBody
{
    float GetDirection();
    void SetDirection(float value);
    float GetSpeed();
    void SetSpeed(float value);
    float GetAccelX();
    void SetAccelX(float value);
    float GetAccelY();
    void SetAccelY(float value);
    float GetAim();
    float GetRank();
}

public interface PatternWorld
{
    float GetTurn();
    float RandomValue();
    void Fire(PatternBody parent, float direction, float speed, int program, float[] args);
    void Vanish(PatternBody body);
}

public static class PatternRunner
{
    public static void Start(List<PatternState> scripts, int pattern)
    {
        scripts.Clear();
        foreach (int program in BarrageCode.Roots(pattern))
            scripts.Add(BarrageCode.Create(program, new float[2]));
    }

    public static void Run(List<PatternState> scripts, PatternBody body, PatternWorld world)
    {
        foreach (var script in scripts) script.Tick(body, world);
    }

    public static bool Ended(List<PatternState> scripts)
    {
        foreach (var script in scripts) if (script.Done) return true;
        return scripts.Count == 0;
    }
}

public class PatternTween
{
    public bool Active;
    public float Start, End;
    public float First, Last, Slope;

    public void Set(float start, int term, float first, float last)
    {
        Start = start; End = start + term; First = first; Last = last;
        Slope = term == 0 ? 0 : (last - first) / term;
        Active = true;
    }

    public float Value(float turn)
    {
        if (turn >= End) { Active = false; return Last; }
        return First + Slope * (turn - Start);
    }
}

public class PatternState
{
    public int Program, Pc, Term;
    public float Resume;
    public float Wake = -1;
    public bool Done;
    public float[] Args = new float[2];
    public float[] Values = new float[2];
    public int[] Counts = new int[0];
    public int[] Limits = new int[0];
    public PatternTween DirTween = new PatternTween();
    public PatternTween SpeedTween = new PatternTween();
    public PatternTween XTween = new PatternTween();
    public PatternTween YTween = new PatternTween();
    float previousDirection, previousSpeed, shotAngle, shotVelocity;
    bool hasPreviousDirection, hasPreviousSpeed, hasShotDirection, hasShotSpeed;

    public void Tick(PatternBody b, PatternWorld w)
    {
        if (Done) return;
        if (DirTween.Active) b.SetDirection(DirTween.Value(w.GetTurn()));
        if (SpeedTween.Active) b.SetSpeed(SpeedTween.Value(w.GetTurn()));
        if (XTween.Active) b.SetAccelX(XTween.Value(w.GetTurn()));
        if (YTween.Active) b.SetAccelY(YTween.Value(w.GetTurn()));
        if (Pc < 0)
        {
            if (Wake <= w.GetTurn() && !DirTween.Active && !SpeedTween.Active && !XTween.Active && !YTween.Active) Done = true;
            return;
        }
        if (Wake < 0) Wake = w.GetTurn();
        if (Resume <= w.GetTurn()) BarrageCode.Run(this, b, w);
    }

    public float ResolveDirection(PatternBody b, float value, int mode, bool remember)
    {
        if (mode == 0) value += b.GetAim();
        if (mode == 2) value += b.GetDirection();
        if (mode == 3) value = hasPreviousDirection ? value + previousDirection : b.GetAim();
        while (value > 360) value -= 360;
        while (value < 0) value += 360;
        if (remember) { previousDirection = value; hasPreviousDirection = true; }
        return value;
    }

    public float ResolveSpeed(PatternBody b, float value, int mode)
    {
        if (mode == 2) value += b.GetSpeed();
        if (mode == 3) value = hasPreviousSpeed ? value + previousSpeed : 1;
        previousSpeed = value; hasPreviousSpeed = true;
        return value;
    }

    public void BeginShot() { hasShotDirection = false; hasShotSpeed = false; }
    public void ShotDirection(PatternBody b, float value, int mode)
    {
        shotAngle = ResolveDirection(b, value, mode, true); hasShotDirection = true;
    }
    public void ShotSpeed(PatternBody b, float value, int mode)
    {
        shotVelocity = ResolveSpeed(b, value, mode); hasShotSpeed = true;
    }
    public void Fire(PatternWorld w, PatternBody b, int program, float[] args)
    {
        if (!hasShotSpeed) { shotVelocity = 1; previousSpeed = 1; hasPreviousSpeed = true; }
        if (!hasShotDirection) { shotAngle = b.GetAim(); previousDirection = b.GetAim(); hasPreviousDirection = true; }
        w.Fire(b, shotAngle, shotVelocity, program, args);
    }
    public void ChangeDirection(PatternBody b, float value, int mode)
    {
        float distance;
        if (mode == 3) distance = value * Term;
        else
        {
            distance = ResolveDirection(b, value, mode, false) - b.GetDirection();
            float other = distance > 0 ? distance - 360 : distance + 360;
            if (Math.Abs(distance) >= Math.Abs(other)) distance = other;
        }
        DirTween.Set(Wake, Term, b.GetDirection(), b.GetDirection() + distance);
    }
    public void ChangeSpeed(PatternBody b, float value, int mode)
    {
        float target = mode == 3 ? b.GetSpeed() + value * Term : ResolveSpeed(b, value, mode);
        SpeedTween.Set(Wake, Term, b.GetSpeed(), target);
    }
    public void Accelerate(PatternBody b, int axis, float value, int mode)
    {
        float first = axis == 0 ? b.GetAccelX() : b.GetAccelY();
        float last = mode == 3 ? first + value * Term : mode == 2 ? first + value : value;
        if (axis == 0) XTween.Set(Wake, Term, first, last);
        else YTween.Set(Wake, Term, first, last);
    }
}
