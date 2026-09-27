using System;
using static Lub;

public static class Game
{
    public static GtgFrame frame;
    static float elapsed;
    public static void OnInit()
    {
        Config(new ConfigOpts { Width = 640, Height = 480 });
        frame = new GtgFrame();
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
                if (value >= 0 && value < 65536)
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

        elapsed += Math.Min(dt, 0.1f);
        while (elapsed >= (1f / 60))
        {
            elapsed -= (1f / 60);
            frame.Update();
        }

        frame.Draw();
    }

    public static void OnQuit()
    {
        frame.SaveScores();
    }
}
