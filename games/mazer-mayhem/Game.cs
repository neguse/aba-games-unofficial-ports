using System;
using static Lub;

public static class Game
{
    public static MmFrame frame;
    public static bool VariableTime;
    static float elapsed;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        frame = new MmFrame();
        frame.LoadContent();
        if (Host.Available())
        {
            Host.Send("scores.load", "");
            Host.Send("ready", "");
        }
    }

    public static void OnFrame(float dt)
    {
        while (Host.Available())
        {
            Lub.Host.Poll(out string topic, out string payload);
            if (topic == null)
                break;
            if (topic == "input")
            {
                int value = GameMath.parseNonnegative(payload);
                if (value >= 0 && value < 512)
                    Pad.input = value;
            }

            if (topic == "seed")
            {
                int value = GameMath.parseNonnegative(payload);
                if (value >= 0)
                    frame.Seed(value);
            }

            if (topic == "scores")
                frame.LoadScores(payload);
        }

        if (VariableTime)
        {
            frame.Advance(dt);
            frame.Render();
            return;
        }

        elapsed += Math.Min(dt, 0.1f);
        while (elapsed >= frame.Interval())
        {
            elapsed -= frame.Interval();
            frame.Update();
        }

        frame.Render();
    }

    public static void OnQuit()
    {
        frame.SaveScores();
    }
}
