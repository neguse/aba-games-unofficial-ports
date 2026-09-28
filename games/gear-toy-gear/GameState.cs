// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class GameState
{
    private const int firstExtendScore = 1000000;
    private const int maxLeft = 9;
    private const int gameOverTicksBase = 300;
    private GtgFrame frame;
    private Pad pad;
    private Record record;
    private Sound sound;
    private PlatePool plates;
    private int score;
    private int storedMultiplier;
    private int extendScore, nextExtendScoreOffset;
    private int left;
    private bool storedIsInGameOver;
    private float gameOverTicks;
    private bool isButtonPressed;
    private float ticks;
    private bool isAccelPressed;
    private int bgmIndex, bgmIndexChange;
    private Random random;
    public GameState(GtgFrame frame, Pad pad, Record record, Sound sound, PlatePool plates)
    {
        this.frame = frame;
        this.pad = pad;
        this.record = record;
        this.sound = sound;
        this.plates = plates;
    }

    public void SetRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    public void Initialize()
    {
        score = 0;
        storedMultiplier = 1;
        left = 2;
        {
            nextExtendScoreOffset = firstExtendScore;
            extendScore = nextExtendScoreOffset;
        }

        ticks = 0;
        storedIsInGameOver = false;
        isAccelPressed = false;
        bgmIndex = random.Nextint(bgmNames.Length);
        bgmIndexChange = random.Nextint(2) * 2 - 1;
    }

    private string[] bgmNames = new string[]
    {
        "Gtg1",
        "Gtg2",
        "Gtg3"
    };
    public void StartBgm()
    {
        if (!frame.IsInGame)
            return;
        sound.PlayBgm(bgmNames[(bgmIndex)]);
        bgmIndex += bgmIndexChange;
        if (bgmIndex >= bgmNames.Length)
            bgmIndex = 0;
        else if (bgmIndex < 0)
            bgmIndex = bgmNames.Length - 1;
    }

    public void FadeoutBgm()
    {
        sound.FadeoutBgm();
    }

    public void AddScoreint(int s)
    {
        if (storedIsInGameOver)
            return;
        score += s * storedMultiplier;
        if (score >= extendScore)
        {
            if (left < maxLeft)
            {
                sound.PlaySe("Extend");
                left++;
            }

            nextExtendScoreOffset += firstExtendScore;
            extendScore += nextExtendScoreOffset;
        }
    }

    public void AddScoreintVector3float(int s, Vector3 p, float scale)
    {
        if (storedIsInGameOver)
            return;
        plates.Add((float)Math.Sqrt(p.X * p.X + p.Y * p.Y), (float)Math.Atan2(p.X, p.Y), p.Z, s * storedMultiplier, scale);
        AddScoreint(s);
    }

    public void Miss()
    {
        left--;
        if (left < 0)
        {
            frame.StartGameOver();
            storedIsInGameOver = true;
            gameOverTicks = gameOverTicksBase;
            isButtonPressed = true;
            if (frame.IsInGame)
                record.RecordScore(score);
        }
    }

    public void Update()
    {
        ticks += SimulationTime.Step;
        if (!isAccelPressed && ticks > 480)
            isAccelPressed = true;
        if (!storedIsInGameOver)
            return;
        if (pad.ButtonAny)
        {
            if (!isButtonPressed)
            {
                if (frame.IsInGame)
                {
                    if (gameOverTicks < gameOverTicksBase - 30)
                        frame.StartGame();
                }
                else
                {
                    frame.StartGame();
                }

                isButtonPressed = true;
            }
        }
        else
        {
            isButtonPressed = false;
        }

        gameOverTicks -= SimulationTime.Step;
        if (gameOverTicks <= 0)
            frame.StartTitle();
    }

    public void OnAccelPressed()
    {
        isAccelPressed = true;
    }

    public void Draw()
    {
        frame.LookAt(new Vector3(0, 0, 25), (Vector3.Zero).Copy(), new Vector3(0, 1, 0));
        Vector3 p = (Letter.AddintVector3floatQuaternionfloat((int)storedMultiplier, new Vector3(28, 20, 0), 0.7f, (Quaternion.Identity).Copy(), 1)).Copy();
        p.Y += 0.2f;
        Letter.AddstringVector3floatQuaternionfloat("X", (p).Copy(), 0.5f, (Quaternion.Identity).Copy(), 1);
        Letter.AddintVector3floatQuaternionfloat(score, new Vector3(28, -20, 0), 1, (Quaternion.Identity).Copy(), 1);
        Letter.AddintVector3floatQuaternionfloat(extendScore, new Vector3(28.5f, -17.5f, 0), 0.4f, (Quaternion.Identity).Copy(), 1);
        if (storedIsInGameOver)
            Letter.AddstringVector3floatQuaternionfloat("GAME OVER", new Vector3(-12, 0, 0), 1.5f - gameOverTicks * 0.001f, (Quaternion.Identity).Copy(), 1);
        else
            Letter.AddintVector3floatQuaternionfloat(left, new Vector3(-28, -20, 0), 1, (Quaternion.Identity).Copy(), 1);
        if (!isAccelPressed && ticks > 300)
            Letter.AddstringVector3floatQuaternionfloat("PRESS RT TO SPEED UP. LT TO BREAK.", new Vector3(-15, 8, 0), 0.5f, (Quaternion.Identity).Copy(), 1);
    }

    public void DrawLastScore()
    {
        frame.LookAt(new Vector3(0, 0, 25), (Vector3.Zero).Copy(), new Vector3(0, 1, 0));
        Letter.AddintVector3floatQuaternionfloat(record.LastScore, new Vector3(28, -20, 0), 1, (Quaternion.Identity).Copy(), 1);
    }

    public int Multiplier
    {
        set
        {
            storedMultiplier = value;
        }
    }

    public bool IsInGameOver
    {
        get
        {
            return storedIsInGameOver;
        }
    }
}
