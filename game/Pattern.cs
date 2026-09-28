using System;
using System.Collections.Generic;

public class PatternBody
{
    float valueDirection;
    public virtual float Direction { get { return valueDirection; } set { valueDirection = value; } }
    float valueSpeed;
    public virtual float Speed { get { return valueSpeed; } set { valueSpeed = value; } }
    float valueAccelX;
    public virtual float AccelX { get { return valueAccelX; } set { valueAccelX = value; } }
    float valueAccelY;
    public virtual float AccelY { get { return valueAccelY; } set { valueAccelY = value; } }
    float valueAim;
    public virtual float Aim { get { return valueAim; } set { valueAim = value; } }
    float valueRank;
    public virtual float Rank { get { return valueRank; } set { valueRank = value; } }
    public List<PatternState> Scripts = new List<PatternState>();

    public void StartPattern(int pattern)
    {
        Scripts.Clear();
        foreach (int program in BarrageCode.Roots(pattern))
            Scripts.Add(BarrageCode.Create(program, new float[2]));
    }

    public void RunPattern(PatternWorld world)
    {
        foreach (var script in Scripts) script.Tick(this, world);
    }

    public bool PatternEnded()
    {
        foreach (var script in Scripts) if (script.Done) return true;
        return Scripts.Count == 0;
    }
}

public abstract class PatternWorld
{
    public float Turn;
    public abstract float RandomValue();
    public abstract void Fire(PatternBody parent, float direction, float speed, int program, float[] args);
    public abstract void Vanish(PatternBody body);
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
        if (DirTween.Active) b.Direction = DirTween.Value(w.Turn);
        if (SpeedTween.Active) b.Speed = SpeedTween.Value(w.Turn);
        if (XTween.Active) b.AccelX = XTween.Value(w.Turn);
        if (YTween.Active) b.AccelY = YTween.Value(w.Turn);
        if (Pc < 0)
        {
            if (Wake <= w.Turn && !DirTween.Active && !SpeedTween.Active && !XTween.Active && !YTween.Active) Done = true;
            return;
        }
        if (Wake < 0) Wake = w.Turn;
        if (Resume <= w.Turn) BarrageCode.Run(this, b, w);
    }

    public float ResolveDirection(PatternBody b, float value, int mode, bool remember)
    {
        if (mode == 0) value += b.Aim;
        if (mode == 2) value += b.Direction;
        if (mode == 3) value = hasPreviousDirection ? value + previousDirection : b.Aim;
        while (value > 360) value -= 360;
        while (value < 0) value += 360;
        if (remember) { previousDirection = value; hasPreviousDirection = true; }
        return value;
    }

    public float ResolveSpeed(PatternBody b, float value, int mode)
    {
        if (mode == 2) value += b.Speed;
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
        if (!hasShotDirection) { shotAngle = b.Aim; previousDirection = b.Aim; hasPreviousDirection = true; }
        w.Fire(b, shotAngle, shotVelocity, program, args);
    }
    public void ChangeDirection(PatternBody b, float value, int mode)
    {
        float distance;
        if (mode == 3) distance = value * Term;
        else
        {
            distance = ResolveDirection(b, value, mode, false) - b.Direction;
            float other = distance > 0 ? distance - 360 : distance + 360;
            if (Math.Abs(distance) >= Math.Abs(other)) distance = other;
        }
        DirTween.Set(Wake, Term, b.Direction, b.Direction + distance);
    }
    public void ChangeSpeed(PatternBody b, float value, int mode)
    {
        float target = mode == 3 ? b.Speed + value * Term : ResolveSpeed(b, value, mode);
        SpeedTween.Set(Wake, Term, b.Speed, target);
    }
    public void Accelerate(PatternBody b, int axis, float value, int mode)
    {
        float first = axis == 0 ? b.AccelX : b.AccelY;
        float last = mode == 3 ? first + value * Term : mode == 2 ? first + value : value;
        if (axis == 0) XTween.Set(Wake, Term, first, last);
        else YTween.Set(Wake, Term, first, last);
    }
}
