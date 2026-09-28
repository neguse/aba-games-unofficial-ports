// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static Lub;

public class MmFrame
{
    private const float slowdownStartObjectNum = 150.0f;
    private static Random random = new Random();
    private Vector4 storedLightVector = new Vector4();
    private Color storedBackgroundColor = new Color();
    private Pad pad;
    private Int32 randomSeed;
    private Field field;
    private WallPool walls;
    private TurretPool turrets;
    private BulletPool bullets;
    private BallPool balls;
    private ParticlePool particles;
    private ParticlePool addedParticles;
    private BonusPool bonuses;
    private BonusPool addedBonuses;
    private Player player;
    private ShotPool shots;
    private GrenadePool grenades;
    private BoardPool playerBoards;
    private BoardPool ballBoards;
    private Stage stage;
    private float cnt;
    float phase, replaySeconds;
    public float StepSeconds;
    public void Advance(float seconds)
    {
        if (!(seconds > 0)) return;
        float remaining = Math.Min(seconds, .1f);
        if (state == MmFrameGameState.Title && replay.IsAvailable)
        {
            replaySeconds += remaining;
            while (replaySeconds + .00001f >= replay.NextSeconds())
            {
                StepSeconds = replay.NextSeconds();
                replaySeconds -= StepSeconds;
                AdvanceStep(replay.NextStep(), true);
                if (state != MmFrameGameState.Title) break;
            }
        }
        else
        {
            while (remaining > .0000001f)
            {
                float step = Math.Min(remaining / Interval(), 1);
                StepSeconds = step * Interval();
                remaining -= StepSeconds;
                AdvanceStep(step, false);
            }
        }
        SimulationTime.Step = 1;
        SimulationTime.Emit = true;
        SimulationTime.Variable = false;
    }
    void AdvanceStep(float step, bool playback)
    {
        SimulationTime.Step = step;
        phase += step;
        bool emit = phase >= 1 - .0001f;
        if (emit) phase -= 1;
        SimulationTime.Emit = playback ? replay.NextEmit() : emit;
        SimulationTime.Variable = true;
        Update();
    }
    private float storedPauseCnt;
    private MmFrameGameState state;
    private List<List<string>> stageData;
    private Title title;
    private bool bPressed;
    private bool sPressed;
    private Record record;
    private Replay replay;
    private Sound sound;
    private float delayRatio;
    private int storedScreenWidth;
    private int storedScreenHeight;
    public void LoadContent()
    {
        LightVector = new Vector4(0.4f, 0.2f, 0.6f, 0);
        BackgroundColor = new Color(210, 210, 210, 255);
        storedScreenWidth = 640;
        storedScreenHeight = 480;
        storedProjMatrix = (Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2, 640f / 480f, 0.1f, 100000)).Copy();
        BallShape.Init();
        Letter.Load();
        stageData = MmData.Stages();
        InitializeActors();
        StartTitle();
    }

    private void InitializeActors()
    {
        pad = new Pad();
        replay = new Replay();
        record = new Record();
        record.Load();
        sound = new Sound();
        sound.Initialize();
        field = new Field(this, pad);
        particles = new ParticlePool(512, this);
        addedParticles = new ParticlePool(256, null);
        walls = new WallPool(128, this, field);
        stage = new Stage(this, walls, sound);
        playerBoards = new BoardPool(4, this);
        ballBoards = new BoardPool(4, this);
        player = new Player(this, field, pad, stage, walls, playerBoards, ballBoards, addedParticles, record, replay, sound);
        bonuses = new BonusPool(360, this, player);
        addedBonuses = new BonusPool(360, null, null);
        bullets = new BulletPool(360, this, field, walls, addedParticles, addedBonuses, player);
        turrets = new TurretPool(128, bullets);
        balls = new BallPool(64, this, turrets, bullets, addedParticles, addedBonuses, player, field, stage, sound);
        shots = new ShotPool(64, this, field, balls, walls, addedParticles, addedBonuses, player, sound);
        grenades = new GrenadePool(8, this, balls, addedParticles, bullets, sound);
        stage.SetParams(player, balls, turrets, bullets);
        player.SetParams(balls, bullets, shots, grenades);
        turrets.SetParams(balls);
        bullets.SetParams(turrets, balls);
        title = new Title(this, pad, record);
    }

    public void StartTitle()
    {
        phase = 0; replaySeconds = 0;
        state = MmFrameGameState.Title;
        randomSeed = replay.RandomSeed;
        Start();
        storedPauseCnt = -1;
        if (replay.IsAvailable)
            player.StartReplay();
        title.Start();
        sound.StopBgm();
    }

    public void StartInGame()
    {
        phase = 0; replaySeconds = 0;
        state = MmFrameGameState.InGame;
        randomSeed = random.Next();
        replay.RandomSeed = randomSeed;
        Start();
        storedPauseCnt = -1;
        sPressed = true;
        replay.StartRecord();
    }

    public void StartGameover()
    {
        sound.FadeoutBgm();
    }

    public void Start()
    {
        Stage.SetRandomSeed(randomSeed);
        ParticlePool.SetRandomSeed(randomSeed);
        BallPool.SetRandomSeed(randomSeed);
        Player.SetRandomSeed(randomSeed);
        BonusPool.SetRandomSeed(randomSeed);
        ClearActorPools();
        stage.CreateMaze(stageData[(0)]);
        field.Initiazlize();
        player.Start();
        cnt = 0;
        bPressed = true;
        delayRatio = 1.0f;
    }

    private void ClearActorPools()
    {
        walls.ClearAll();
        bullets.ClearAll();
        turrets.ClearAll();
        balls.ClearAll();
        particles.ClearAll();
        addedParticles.ClearAll();
        bonuses.ClearAll();
        addedBonuses.ClearAll();
        shots.ClearAll();
        grenades.ClearAll();
        playerBoards.ClearAll();
        ballBoards.ClearAll();
    }

    public void Update()
    {
        int an = bullets.Length() + GameMath.integer((bonuses.Length() * 0.66f));
        if (an > slowdownStartObjectNum)
            delayRatio = 1.0f + ((float)an - slowdownStartObjectNum) / slowdownStartObjectNum;
        else
            delayRatio = 1.0f;
        record.Update();
        sound.Update();
        pad.Update();
        bool bp = false;
        if (pad.ButtonBack)
        {
            if (!bPressed)
                bp = true;
            bPressed = true;
        }
        else
        {
            bPressed = false;
        }

        cnt += SimulationTime.Step;
        switch (state)
        {
            case MmFrameGameState.Title:
                if (bp)
                {
                    Exit();
                    return;
                }

                if (replay.IsAvailable)
                    UpdateInGame();
                else
                    field.Deg += 0.002f * SimulationTime.Step;
                title.Update();
                break;
            case MmFrameGameState.InGame:
                if (bp)
                {
                    StartTitle();
                    return;
                }

                if (pad.ButtonStart)
                {
                    if (!sPressed)
                    {
                        sPressed = true;
                        if (storedPauseCnt < 0)
                            storedPauseCnt = 0;
                        else
                            storedPauseCnt = -1;
                    }
                }
                else
                {
                    sPressed = false;
                }

                if (storedPauseCnt >= 0)
                    storedPauseCnt += SimulationTime.Step;
                UpdateInGame();
                break;
        }
    }

    private void UpdateInGame()
    {
        if (storedPauseCnt < 0)
        {
            addedParticles.AddToForced(particles);
            addedBonuses.AddTo(bonuses);
            playerBoards.Gc();
            ballBoards.Gc();
            shots.Gc();
            grenades.Gc();
            walls.Gc();
            balls.Gc();
            bullets.Gc();
            turrets.Gc();
            bonuses.Gc();
            particles.Gc();
        }

        UpdateActors(0);
        UpdateActors(1);
        UpdateActors(2);
        if (storedPauseCnt < 0)
        {
            player.Update();
            if (SimulationTime.Variable) field.UpdateScreenOffset();
        }
    }

    public void UpdateActors(int threadId)
    {
        switch (threadId)
        {
            case 0:
                if (storedPauseCnt >= 0)
                    break;
                stage.Update();
                playerBoards.Update();
                ballBoards.Update();
                walls.Update();
                balls.CheckCollisions();
                walls.CheckCollisions(balls);
                balls.Update();
                balls.UpdateTurrets();
                bullets.Update();
                shots.Update();
                grenades.Update();
                break;
            case 1:
                player.UpdateStateShape();
                if (storedPauseCnt >= 0)
                    break;
                bonuses.Update();
                break;
            case 2:
                if (storedPauseCnt >= 0)
                    break;
                particles.Update();
                break;
        }
    }

    public void Draw()
    {
        switch (state)
        {
            case MmFrameGameState.Title:
                if (replay.IsAvailable)
                    DrawInGame();
                else
                    DrawField();
                title.Draw();
                break;
            case MmFrameGameState.InGame:
                DrawInGame();
                break;
        }
    }

    private void DrawField()
    {
        field.SetEyePosition();
        EffectColor = new Vector4(0.5f, 0.5f, 1.0f, 0.75f);
        BlurColor = new Vector4(0, 0, 0.5f, 0.7f);
        BlurThickness = 0.05f;
        walls.Draw();
        ChangeTechnique("SimpleFogTech");
        stage.Draw();
    }

    private void DrawInGame()
    {
        field.SetEyePosition();
        EffectColor = new Vector4(1, 0.5f, 0.25f, 1.0f);
        BlurColor = new Vector4(0, 0, 0, 0.5f);
        BlurThickness = 0.1f;
        balls.Draw();
        player.Draw();
        EffectColor = new Vector4(0.5f, 0.5f, 1.0f, 0.75f);
        BlurColor = new Vector4(0, 0, 0.5f, 0.7f);
        BlurThickness = 0.05f;
        walls.Draw();
        Vector4 lv = (storedLightVector).Copy();
        lv.Z *= -1;
        LightVector = (lv).Copy();
        EffectColor = new Vector4(1, 0.5f, 0.25f, 0.4f);
        BlurColor = new Vector4(0, 0, 0, 0.8f);
        BlurThickness = 0.1f;
        balls.DrawReflection();
        lv.Z *= -1;
        LightVector = (lv).Copy();
        ChangeTechnique("BoardTech");
        DepthEnabled = false;
        playerBoards.Draw();
        ballBoards.Draw();
        DepthEnabled = true;
        ChangeTechnique("BlurTech");
        EffectColor = new Vector4(0.5f, 0.5f, 0.5f, 0.2f);
        BlurColor = new Vector4(0.1f, 0.1f, 0.1f, 0.1f);
        BlurThickness = 0.2f;
        grenades.Draw();
        float bc = ((float)Math.Sin((cnt % 60) * (Math.PI * 2 / 60)) + 1.0f) / 2;
        EffectColor = new Vector4(0.5f + bc * 0.5f, 0.25f + bc * 0.75f, 0.75f + bc * 0.25f, 1.0f);
        bullets.Draw();
        EffectColor = new Vector4(0.75f, 1, 0.5f, 0.3f);
        BlurColor = new Vector4(0.5f, 1, 0, 0.6f);
        BlurThickness = 0.2f;
        bonuses.Draw();
        ChangeTechnique("SimpleFogTech");
        stage.Draw();
        ChangeTechnique("SimpleTech");
        particles.Draw();
        shots.Draw();
        grenades.DrawBurst();
        bullets.DrawShadow();
        ChangeTechnique("BoardTech");
        DepthEnabled = false;
        LookAt(new Vector3(0, 0, -1), (Vector3.Zero).Copy(), (Vector3.Up).Copy());
        balls.DrawState();
        player.DrawState();
        DepthEnabled = true;
    }

    public Matrix WorldMatrix
    {
        set
        {
            storedWorldMatrix = (value).Copy();
        }
    }

    public Matrix ViewMatrix
    {
        get
        {
            return (storedViewMatrix).Copy();
        }
    }

    public Matrix ProjMatrix
    {
        get
        {
            return (storedProjMatrix).Copy();
        }
    }

    public bool Hud;
    public void LookAt(Vector3 from, Vector3 to, Vector3 up)
    {
        Hud = from.X == 0 && from.Y == 0 && from.Z == -1 && to.X == 0 && to.Y == 0 && to.Z == 0;
        storedViewMatrix = (Matrix.CreateLookAt((from).Copy(), (to).Copy(), (up).Copy())).Copy();
    }

    public Vector4 LightVector
    {
        set
        {
            storedLightVector = (value).Copy();
            storedLightVector.Normalize();
        }

        get
        {
            return (storedLightVector).Copy();
        }
    }

    public Vector4 EffectColor
    {
        set
        {
            storedEffectColor = (value).Copy();
        }
    }

    public Vector4 BlurColor
    {
        set
        {
            storedBlurColor = (value).Copy();
        }
    }

    public Color BackgroundColor
    {
        set
        {
            storedBackgroundColor = (value).Copy();
            Vector4 bgc = (new Vector4(value.R, value.G, value.B, value.A) / 255.0f).Copy();
        }
    }

    public Vector4 Velocity
    {
        set
        {
            storedVelocity = (value).Copy();
        }
    }

    public float BlurThickness
    {
        set
        {
            storedBlurThickness = value;
        }
    }

    public int PauseCnt
    {
        get
        {
            return (int)storedPauseCnt;
        }
    }

    public int ScreenWidth
    {
        get
        {
            return storedScreenWidth;
        }
    }

    public int ScreenHeight
    {
        get
        {
            return storedScreenHeight;
        }
    }

    public bool IsInGame
    {
        get
        {
            return (state == MmFrameGameState.InGame);
        }
    }

    public Matrix storedWorldMatrix = (Matrix.Identity).Copy(), storedViewMatrix = (Matrix.Identity).Copy(), storedProjMatrix = (Matrix.Identity).Copy();
    public Vector4 storedEffectColor = new Vector4(1, 1, 1, 1), storedBlurColor = new Vector4(), storedVelocity = new Vector4();
    public float storedBlurThickness;
    public bool DepthEnabled = true;
    public string Technique = "BlurLightingTech";
    public void ChangeTechnique(string name)
    {
        Technique = name;
    }

    public void Exit()
    {
        MmPreference.Save(record);
        if (Host.Available())
            Host.Send("quit", "");
    }

    public float Interval()
    {
        return delayRatio / 60f;
    }

    public void Render()
    {
        Technique = "BlurLightingTech";
        DepthEnabled = true;
        MmRender.Begin(this);
        Draw();
        MmRender.End();
    }

    public void Seed(int seed)
    {
        random = new Random(seed);
    }

    public void LoadScores(string text)
    {
        MmPreference.Load(record, text);
    }

    public void SaveScores()
    {
        MmPreference.Save(record);
    }
}

public enum MmFrameGameState
{
    Title,
    InGame,
};
