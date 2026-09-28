using System;

public class SimulationClock
{
    float phase;
    float replayTime;
    public void Reset() { phase = 0; replayTime = 0; }
    public void AdvanceReplay(float seconds, float tickSeconds, Func<float> nextStep, Action update)
    {
        if (!(seconds > 0)) return;
        replayTime += Math.Min(seconds, .1f) / tickSeconds;
        float step = nextStep();
        while (replayTime + .0001f >= step)
        {
            SetStep(step);
            update();
            replayTime -= step;
            step = nextStep();
        }
        Restore();
    }
    void SetStep(float step)
    {
        SimulationTime.Step = step;
        phase += step;
        SimulationTime.Emit = phase >= 1 - .0001f;
        if (SimulationTime.Emit) phase -= 1;
        SimulationTime.Variable = true;
    }
    static void Restore()
    {
        SimulationTime.Step = 1;
        SimulationTime.Emit = true;
        SimulationTime.Variable = false;
    }
    public void Advance(float seconds, float tickSeconds, Action update)
    {
        if (!(seconds > 0)) return;
        float remaining = Math.Min(seconds, .1f) / tickSeconds;
        while (remaining > .000001f)
        {
            SetStep(Math.Min(remaining, 1));
            update();
            remaining -= SimulationTime.Step;
        }
        Restore();
    }
}

public static class SimulationTime
{
    // Existing tuning values use one original game tick as their time unit.
    public static float Step = 1;
    public static bool Emit = true, Variable;
    public static float Decay(float rate) => Step == 1 ? rate : (float)Math.Pow(rate, Step);
    public static float Blend(float ratio) => Step == 1 ? ratio : 1 - Decay(1 - ratio);
    public static float FollowAndDecay(float value, float target, float ratio, float decay)
    {
        if (Step == 1) return (value + (target - value) * ratio) * decay;
        float retention = (1 - ratio) * decay;
        float equilibrium = target * ratio * decay / (1 - retention);
        return equilibrium + (value - equilibrium) * Decay(retention);
    }
    public static bool Crossed(float remaining, float threshold)
        => remaining <= threshold && remaining + Step > threshold;
    public static bool Period(float time, float period)
        => Math.Floor(time / period) != Math.Floor((time + Step) / period);
    public static float ParseStep(string text)
    {
        if (text.Length == 0 || text.Length > 24) return -1;
        bool dot = false, digitSeen = false;
        int i = 0;
        for (; i < text.Length; i++)
        {
            string c = text.Substring(i, 1);
            if (c == "E" || c == "e") break;
            if (c == "." && !dot) { dot = true; continue; }
            int digit = "0123456789".IndexOf(c);
            if (digit < 0) return -1;
            digitSeen = true;
        }
        if (!digitSeen) return -1;
        if (i < text.Length)
        {
            i++;
            if (i >= text.Length || text.Substring(i, 1) != "-") return -1;
            i++;
            if (i >= text.Length || text.Length - i > 2) return -1;
            for (; i < text.Length; i++)
            {
                int digit = "0123456789".IndexOf(text.Substring(i, 1));
                if (digit < 0) return -1;
            }
        }
        return float.Parse(text);
    }
    public static float Repeat(float remaining, float interval)
        => Variable ? remaining + interval : interval;
}
