// Copyright 2003 Kenta Cho. All rights reserved.
using static Lub;

public class Input
{
    public const int PAD_UP = 1, PAD_DOWN = 2, PAD_LEFT = 4, PAD_RIGHT = 8, PAD_BUTTON1 = 16, PAD_BUTTON2 = 32;
    public int directions, buttons;
    public bool pause, escape;
    public int getPadState()
    {
        return directions;
    }

    public int getButtonState()
    {
        return buttons;
    }
}

public class A7xPrefManager
{
    public int hiScore;
    public void load(string data)
    {
        int score = GameMath.parseNonnegative(data);
        if (score >= 0)
            hiScore = score;
    }

    public void save()
    {
        if (Host.Available())
            Host.Send("scores.save", hiScore.ToString());
    }
}

public class Sound
{
    public int index;
    public void loadSound(string name)
    {
        index = GameMath.integer("bgm1.ogg,bgm2.ogg,bgm3.ogg".IndexOf(name) / 9);
    }

    public void loadChunk(string name, int channel)
    {
        string[] names = new string[]
        {
            "getgold.wav",
            "boost.wav",
            "miss.wav",
            "invincible.wav",
            "invfast.wav",
            "enemyapp.wav",
            "enemycrash.wav",
            "extend.wav",
            "stagestart.wav",
            "stageend.wav",
            "startinv.wav",
            "accel.wav"
        };
        for (int i = 0; i < names.Length; i++)
            if (names[i] == name)
                index = i;
    }

    public void playMusic()
    {
        if (Host.Available())
            Host.Send("music.loop", index.ToString());
    }

    public void playChunk()
    {
        if (Host.Available())
            Host.Send("sound.play", index.ToString());
    }

    public void haltChunk()
    {
        if (Host.Available())
            Host.Send("sound.stop", index.ToString());
    }

    public static void stopMusic()
    {
        if (Host.Available())
            Host.Send("music.stop", "");
    }

    public static void fadeMusic()
    {
        if (Host.Available())
            Host.Send("music.fade", "");
    }

    public void free()
    {
    }
}
