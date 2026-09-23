public class Pad
{
    public static int input;
    public void Update()
    {
    }

    public int CheckGameStartPressed()
    {
        return ButtonAny || ButtonStart ? 4 : -1;
    }

    public void SetGamePadPlayerIndex(int i)
    {
    }

    public Vector2 ThumbStickLeft
    {
        get
        {
            return new Vector2(((input & 8) != 0 ? 1 : 0) - ((input & 4) != 0 ? 1 : 0), ((input & 1) != 0 ? 1 : 0) - ((input & 2) != 0 ? 1 : 0));
        }
    }

    public Vector2 ThumbStickRight
    {
        get
        {
            return new Vector2(((input & 128) != 0 ? 1 : 0) - ((input & 64) != 0 ? 1 : 0), ((input & 16) != 0 ? 1 : 0) - ((input & 32) != 0 ? 1 : 0));
        }
    }

    public bool ButtonA
    {
        get
        {
            return (input & 256) != 0;
        }
    }

    public bool ButtonB
    {
        get
        {
            return (input & 512) != 0;
        }
    }

    public bool ButtonL
    {
        get
        {
            return (input & 1024) != 0;
        }
    }

    public bool ButtonR
    {
        get
        {
            return (input & 2048) != 0;
        }
    }

    public bool ButtonAny
    {
        get
        {
            return (input & 3840) != 0;
        }
    }

    public float LeftTrigger
    {
        get
        {
            return (input & 4096) != 0 ? 1f : 0f;
        }
    }

    public float RightTrigger
    {
        get
        {
            return (input & 8192) != 0 ? 1f : 0f;
        }
    }

    public bool ButtonStart
    {
        get
        {
            return (input & 16384) != 0;
        }
    }

    public bool ButtonBack
    {
        get
        {
            return (input & 32768) != 0;
        }
    }
}
