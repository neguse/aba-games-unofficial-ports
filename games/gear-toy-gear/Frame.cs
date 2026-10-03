// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class GtgFrame
{
    private const string titleName = "GearToyGear";
    private const int versionNum = 10;
    private Random random = new Random();
    private Pad pad;
    private Title title;
    public ActorPools actors;
    public float storedPauseTicks;
    readonly SimulationClock clock = new SimulationClock();
    Action advanceUpdate;
    Func<float> replayStep;
    public void Advance(float seconds)
    {
        if (state == FrameGameState.Title && replay.IsAvailable)
            clock.AdvanceReplay(seconds, 1f / 60, replayStep, advanceUpdate);
        else clock.Advance(seconds, 1f / 60, advanceUpdate);
    }
    public FrameGameState state;
    private bool backPressed;
    private bool startPressed;
    public Record record;
    private Replay replay;
    public Sound sound;
    private int screenWidth, screenHeight;
    public void Initialize()
    {
    }

    public void LoadContent()
    {
        advanceUpdate = () => Update();
        replayStep = () => replay.NextStep();
        screenWidth = 640;
        screenHeight = 480;
        ProjMatrix = (Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2, 640f / 480f, .1f, 100000)).Copy();
        Letter.Initialize(this);
        pad = new Pad();
        replay = new Replay();
        record = new Record();
        record.Load();
        sound = new Sound();
        sound.Initialize();
        title = new Title(this, pad, record);
        actors = new ActorPools(this, pad, replay, record, sound);
        StartTitle();
    }

    public void StartTitle()
    {
        clock.Reset();
        state = FrameGameState.Title;
        Start(replay.RandomSeed);
        if (replay.IsAvailable)
            actors.StartReplay();
        else
            actors.StartTitle();
        title.Initialize();
        sound.StopBgm();
    }

    public void StartGame()
    {
        clock.Reset();
        state = FrameGameState.InGame;
        int randomSeed = random.Next();
        replay.RandomSeed = randomSeed;
        Start(randomSeed);
        actors.StartRecord();
        storedPauseTicks = -1;
        startPressed = true;
    }

    public void StartGameOver()
    {
        sound.FadeoutBgm();
    }

    public void Start(int randomSeed)
    {
        actors.Start(randomSeed);
        backPressed = true;
    }

    public void Update()
    {
        record.Update();
        sound.Update();
        pad.Update();
        bool bp = false;
        if (pad.ButtonBack)
        {
            if (!backPressed)
                bp = true;
            backPressed = true;
        }
        else
        {
            backPressed = false;
        }

        switch (state)
        {
            case FrameGameState.Title:
                if (bp)
                {
                    Exit();
                    return;
                }

                if (replay.IsAvailable)
                    UpdateInGame();
                else
                    actors.UpdateField();
                title.Update();
                break;
            case FrameGameState.InGame:
                if (bp)
                {
                    StartTitle();
                    return;
                }

                if (pad.ButtonStart)
                {
                    if (!startPressed)
                    {
                        startPressed = true;
                        if (storedPauseTicks < 0)
                            storedPauseTicks = 0;
                        else
                            storedPauseTicks = -1;
                    }
                }
                else
                {
                    startPressed = false;
                }

                if (storedPauseTicks >= 0)
                    storedPauseTicks += SimulationTime.Step;
                UpdateInGame();
                break;
        }
    }

    private void UpdateInGame()
    {
        if (storedPauseTicks < 0)
            actors.Update();
    }

    public Matrix ViewMatrix = (Matrix.Identity).Copy(), ProjMatrix = (Matrix.Identity).Copy();
    public float ShadowDepthOffset;
    public Vector3[] LightColors = GtgArrays.Make(24, () => new Vector3());
    public string Technique = "EdgeTech";
    public bool DepthEnabled = true;
    public void ChangeTechnique(string name)
    {
        Technique = name;
    }

    public void Draw()
    {
        if (!GtgRender.Begin())
            return;
        ChangeTechnique("EdgeTech");
        if (state == FrameGameState.Title && !replay.IsAvailable)
        {
            ChangeTechnique("DepthTech");
            GtgRender.BeginMain();
            actors.DrawField();
            ChangeTechnique("LetterTech");
            DepthEnabled = false;
            title.Draw();
            actors.DrawScore();
            Letter.Draw();
            DepthEnabled = true;
            GtgRender.EndPass();
            return;
        }

        GtgRender.BeginEdge();
        actors.DrawEdge();
        GtgRender.EndPass();
        GtgRender.BeginBloom();
        ChangeTechnique("ParticleBloomTech");
        actors.DrawParticle();
        ChangeTechnique("BloomTech");
        actors.DrawBloom();
        GtgRender.EndPass();
        GtgRender.BeginMain();
        ChangeTechnique("DepthTech");
        actors.Draw();
        ChangeTechnique("ParticleTech");
        actors.DrawParticle();
        ChangeTechnique("LetterTech");
        DepthEnabled = false;
        if (state == FrameGameState.Title)
        {
            title.Draw();
            actors.DrawScore();
        }
        else
            actors.DrawGameState();
        if (storedPauseTicks >= 0 && storedPauseTicks % 60 < 30)
            Letter.AddstringVector3floatQuaternionfloat("PAUSE", new Vector3(-3, -4, 0), .7f, Quaternion.Identity, 1);
        Letter.Draw();
        GtgRender.Composite(this);
        DepthEnabled = true;
        GtgRender.EndPass();
    }

    public void LookAt(Vector3 from, Vector3 to, Vector3 up)
    {
        ViewMatrix = (Matrix.CreateLookAt((from).Copy(), (to).Copy(), (up).Copy())).Copy();
    }

    public void Exit()
    {
        // A page takes the player back to its game list; elsewhere leaving the title ends the game.
        if (Lub.Host.Available())
            Lub.Host.Send("quit", "");
        else
            Lub.Quit();
    }

    public void Seed(int seed)
    {
        random = new Random(seed);
    }

    public void LoadScores(string text)
    {
        GtgPreference.Load(record, text);
    }

    public void SaveScores()
    {
        GtgPreference.Save(record);
    }

    public int PauseTicks
    {
        get
        {
            return (int)storedPauseTicks;
        }
    }

    public bool IsInGame
    {
        get
        {
            return (state == FrameGameState.InGame);
        }
    }
}

public enum FrameGameState
{
    Title,
    InGame,
};
