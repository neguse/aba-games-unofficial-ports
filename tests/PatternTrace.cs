using System;
using System.Collections.Generic;

public class TraceBody : PatternBody
{
    public int Id;
    public bool Alive = true;
    public float X, Y;
}

public class TraceWorld : PatternWorld
{
    public List<TraceBody> Bodies = new List<TraceBody>();
    int randomState = 123;
    public override float RandomValue()
    {
        randomState = (randomState * 25173 + 13849) & 65535;
        return randomState / 65536f;
    }
    public override void Fire(PatternBody parent, float direction, float speed, int program, float[] args)
    {
        var b = (TraceBody)parent;
        int id = program < 0 ? -1 : Bodies.Count;
        Console.WriteLine($"F {Turn} {b.Id} {id} {direction} {speed}");
        if (program >= 0)
        {
            var child = new TraceBody();
            child.Id = id; child.Direction = direction; child.Speed = speed;
            child.Rank = b.Rank; child.X = b.X; child.Y = b.Y;
            child.Scripts.Add(BarrageCode.Create(program, args));
            Bodies.Add(child);
        }
    }
    public override void Vanish(PatternBody body)
    {
        var b = (TraceBody)body; b.Alive = false;
        Console.WriteLine($"V {Turn} {b.Id}");
    }
}

public static class PatternTrace
{
    public static void Main()
    {
        var world = new TraceWorld();
        var body = new TraceBody();
        body.Direction = 270; body.Speed = 1;
        body.Rank = float.Parse(Environment.GetEnvironmentVariable("PATTERN_RANK") ?? "0.5");
        body.StartPattern(BarrageCode.Find(Environment.GetEnvironmentVariable("PATTERN_NAME") ?? "basic/nway.xml"));
        world.Bodies.Add(body);
        int turns = int.Parse(Environment.GetEnvironmentVariable("PATTERN_TURNS") ?? "240");
        for (world.Turn = 0; world.Turn < turns; world.Turn++)
        {
            int count = world.Bodies.Count;
            for (int i = 0; i < count; i++)
            {
                var b = world.Bodies[i];
                if (!b.Alive) continue;
                b.Aim = 200 + (world.Turn % 113) * 0.25f;
                if (!b.PatternEnded()) b.RunPattern(world);
                b.X += (float)Math.Sin(b.Direction * 0.017453292f) * b.Speed + b.AccelX;
                b.Y += (float)Math.Cos(b.Direction * 0.017453292f) * b.Speed - b.AccelY;
                int ended = b.PatternEnded() ? 1 : 0;
                Console.WriteLine($"B {world.Turn} {b.Id} {b.Direction} {b.Speed} {b.AccelX} {b.AccelY} {ended} {b.X} {b.Y}");
            }
        }
    }
}
