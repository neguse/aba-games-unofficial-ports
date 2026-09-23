public class Pad
{
    public static int input;
    public void Update()
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
            return Vector2.Zero;
        }
    }

    public bool ButtonA
    {
        get
        {
            return (input & 16) != 0;
        }
    }

    public bool ButtonL
    {
        get
        {
            return (input & 32) != 0;
        }
    }

    public bool ButtonR
    {
        get
        {
            return (input & 64) != 0;
        }
    }

    public float LeftTrigger
    {
        get
        {
            return 0;
        }
    }

    public float RightTrigger
    {
        get
        {
            return 0;
        }
    }

    public bool ButtonStart
    {
        get
        {
            return (input & 128) != 0;
        }
    }

    public bool ButtonBack
    {
        get
        {
            return (input & 256) != 0;
        }
    }
}
