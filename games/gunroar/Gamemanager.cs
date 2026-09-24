using static Lub;
// Copyright 2005 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class GameManager
{
    public float interval = 16, slowdownRatio;
    public static float shipTurnSpeed = 1;
    public static bool shipReverseFire = false;
    public Pad pad;
    public TwinStick twinStick;
    public Mouse mouse;
    public RecordableMouseAndPad mouseAndPad;
    public PrefManager prefManager;
    public GrScreen screen;
    public Field field;
    public Ship ship;
    public ShotPool shots;
    public BulletPool bullets;
    public EnemyPool enemies;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public SparkFragmentPool sparkFragments;
    public WakePool wakes;
    public CrystalPool crystals;
    public NumIndicatorPool numIndicators;
    public StageManager stageManager;
    public TitleManager titleManager;
    public ScoreReel scoreReel;
    public GameState state;
    public TitleState titleState;
    public InGameState inGameState;
    public bool escPressed;
    public void init()
    {
        Letter.init();
        Shot.init_0();
        BulletShape.init();
        EnemyShape.init();
        Turret.init();
        TurretShape.init();
        Fragment.init_0();
        SparkFragment.init_0();
        Crystal.init_0();
        prefManager = new PrefManager();
        screen = new GrScreen();
        pad = new RecordablePad();
        twinStick = new RecordableTwinStick();
        mouse = new RecordableMouse();
        mouseAndPad = new RecordableMouseAndPad(mouse, pad);
        field = new Field();
        List<object> pargs = new List<object>();
        sparks = new SparkPool(120, pargs);
        pargs.Add(field);
        wakes = new WakePool(100, pargs);
        pargs.Add(wakes);
        smokes = new SmokePool(200, pargs);
        List<object> fargs = new List<object>();
        fargs.Add(field);
        fargs.Add(smokes);
        fragments = new FragmentPool(60, fargs);
        sparkFragments = new SparkFragmentPool(40, fargs);
        ship = new Ship(pad, twinStick, mouse, mouseAndPad, field, screen, sparks, smokes, fragments, wakes);
        List<object> cargs = new List<object>();
        cargs.Add(ship);
        CrystalPool crystals = new CrystalPool(80, cargs);
        scoreReel = new ScoreReel();
        List<object> nargs = new List<object>();
        nargs.Add(scoreReel);
        NumIndicatorPool numIndicators = new NumIndicatorPool(50, nargs);
        List<object> bargs = new List<object>();
        bargs.Add(this);
        bargs.Add(field);
        bargs.Add(ship);
        bargs.Add(smokes);
        bargs.Add(wakes);
        bargs.Add(crystals);
        bullets = new BulletPool(240, bargs);
        List<object> eargs = new List<object>();
        eargs.Add(field);
        eargs.Add(screen);
        eargs.Add(bullets);
        eargs.Add(ship);
        eargs.Add(sparks);
        eargs.Add(smokes);
        eargs.Add(fragments);
        eargs.Add(sparkFragments);
        eargs.Add(numIndicators);
        eargs.Add(scoreReel);
        enemies = new EnemyPool(40, eargs);
        List<object> sargs = new List<object>();
        sargs.Add(field);
        sargs.Add(enemies);
        sargs.Add(sparks);
        sargs.Add(smokes);
        sargs.Add(bullets);
        shots = new ShotPool(50, sargs);
        ship.setShots(shots);
        ship.setEnemies(enemies);
        stageManager = new StageManager(field, enemies, ship, bullets, sparks, smokes, fragments, wakes);
        ship.setStageManager(stageManager);
        field.setStageManager(stageManager);
        field.setShip(ship);
        enemies.setStageManager(stageManager);
        SoundManager.loadSounds();
        titleManager = new TitleManager(prefManager, pad, mouse, field, this);
        inGameState = new InGameState(this, screen, pad, twinStick, mouse, mouseAndPad, field, ship, shots, bullets, enemies, sparks, smokes, fragments, sparkFragments, wakes, crystals, numIndicators, stageManager, scoreReel, prefManager);
        titleState = new TitleState(this, screen, pad, twinStick, mouse, mouseAndPad, field, ship, shots, bullets, enemies, sparks, smokes, fragments, sparkFragments, wakes, crystals, numIndicators, stageManager, scoreReel, titleManager, inGameState);
        ship.setGameState(inGameState);
    }

    public void close()
    {
        ship.close();
        BulletShape.close();
        EnemyShape.close();
        TurretShape.close();
        Fragment.close();
        SparkFragment.close();
        Crystal.close();
        titleState.close();
        Letter.close();
    }

    public void start()
    {
        loadLastReplay();
        startTitle();
    }

    public void startTitle(bool fromGameover = false)
    {
        if (fromGameover)
            saveLastReplay();
        titleState.replayData = inGameState.replayData;
        state = titleState;
        startState();
    }

    public void startInGame(int gameMode)
    {
        state = inGameState;
        inGameState.gameMode = gameMode;
        startState();
    }

    public void startState()
    {
        state.start();
    }

    public void saveErrorReplay()
    {
        if (state == inGameState)
            inGameState.saveReplay("error.rpl");
    }

    public void saveLastReplay()
    {
        if (inGameState.replayData != null)
            inGameState.saveReplay("last.rpl");
    }

    public void loadLastReplay()
    {
        inGameState.resetReplay();
        if (Host.Available())
            Host.Send("replay.load", "");
    }

    public void initInterval()
    {
        interval = 16;
    }

    public void addSlowdownRatio(float sr)
    {
        slowdownRatio = slowdownRatio + (sr);
    }

    public void move()
    {
        if (pad.escape)
        {
            if (!escPressed)
            {
                escPressed = true;
                if (state == inGameState)
                {
                    startTitle();
                }
                else
                {
                    return;
                }

                return;
            }
        }
        else
        {
            escPressed = false;
        }

        state.move();
    }

    public void draw()
    {
        state.draw(screen.eye());
        float[] model = screen.eye();
        field.drawSideWalls(model);
        state.drawFront(model);
        state.drawOrtho(Transform.Ortho());
    }
}

public abstract class GameState
{
    public GameManager gameManager;
    public GrScreen screen;
    public Pad pad;
    public TwinStick twinStick;
    public Mouse mouse;
    public RecordableMouseAndPad mouseAndPad;
    public Field field;
    public Ship ship;
    public ShotPool shots;
    public BulletPool bullets;
    public EnemyPool enemies;
    public SparkPool sparks;
    public SmokePool smokes;
    public FragmentPool fragments;
    public SparkFragmentPool sparkFragments;
    public WakePool wakes;
    public CrystalPool crystals;
    public NumIndicatorPool numIndicators;
    public StageManager stageManager;
    public ScoreReel scoreReel;
    public ReplayData _replayData;
    public GameState(GameManager gameManager, GrScreen screen, Pad pad, TwinStick twinStick, Mouse mouse, RecordableMouseAndPad mouseAndPad, Field field, Ship ship, ShotPool shots, BulletPool bullets, EnemyPool enemies, SparkPool sparks, SmokePool smokes, FragmentPool fragments, SparkFragmentPool sparkFragments, WakePool wakes, CrystalPool crystals, NumIndicatorPool numIndicators, StageManager stageManager, ScoreReel scoreReel)
    {
        this.gameManager = gameManager;
        this.screen = screen;
        this.pad = pad;
        this.twinStick = twinStick;
        this.mouse = mouse;
        this.mouseAndPad = mouseAndPad;
        this.field = field;
        this.ship = ship;
        this.shots = shots;
        this.bullets = bullets;
        this.enemies = enemies;
        this.sparks = sparks;
        this.smokes = smokes;
        this.fragments = fragments;
        this.sparkFragments = sparkFragments;
        this.wakes = wakes;
        this.crystals = crystals;
        this.numIndicators = numIndicators;
        this.stageManager = stageManager;
        this.scoreReel = scoreReel;
    }

    public abstract void start();
    public abstract void move();
    public abstract void draw(float[] model);
    public abstract void drawLuminous(float[] model);
    public abstract void drawFront(float[] model);
    public abstract void drawOrtho(float[] model);
    public void clearAll()
    {
        shots.clear();
        bullets.clear();
        enemies.clear();
        sparks.clear();
        smokes.clear();
        fragments.clear();
        sparkFragments.clear();
        wakes.clear();
        crystals.clear();
        numIndicators.clear();
    }

    public ReplayData replayData
    {
        get
        {
            return _replayData;
        }

        set
        {
            _replayData = value;
        }
    }
}

public class InGameState : GameState
{
    public static int GAME_MODE_NUM = 4;
    public static string[] gameModeText = new string[]
    {
        "NORMAL",
        "TWIN STICK",
        "DOUBLE PLAY",
        "MOUSE"
    };
    public bool isGameOver;
    public const float SCORE_REEL_SIZE_DEFAULT = 0.5f;
    public const float SCORE_REEL_SIZE_SMALL = 0.01f;
    public GunroarRand rand;
    public PrefManager prefManager;
    public int left;
    public int time;
    public int gameOverCnt;
    public bool btnPressed;
    public int pauseCnt;
    public bool pausePressed;
    public float scoreReelSize;
    public int _gameMode;
    public InGameState(GameManager gameManager, GrScreen screen, Pad pad, TwinStick twinStick, Mouse mouse, RecordableMouseAndPad mouseAndPad, Field field, Ship ship, ShotPool shots, BulletPool bullets, EnemyPool enemies, SparkPool sparks, SmokePool smokes, FragmentPool fragments, SparkFragmentPool sparkFragments, WakePool wakes, CrystalPool crystals, NumIndicatorPool numIndicators, StageManager stageManager, ScoreReel scoreReel, PrefManager prefManager) : base(gameManager, screen, pad, twinStick, mouse, mouseAndPad, field, ship, shots, bullets, enemies, sparks, smokes, fragments, sparkFragments, wakes, crystals, numIndicators, stageManager, scoreReel)
    {
        this.prefManager = prefManager;
        rand = new GunroarRand();
        _replayData = null;
        left = 0;
        {
            pauseCnt = 0;
            gameOverCnt = pauseCnt;
        }

        scoreReelSize = SCORE_REEL_SIZE_DEFAULT;
    }

    public override void start()
    {
        ship.unsetReplayMode();
        _replayData = new ReplayData();
        prefManager.prefData.recordGameMode(_gameMode);
        prefManager.save();
        switch (_gameMode)
        {
            case InGameStateGameMode.NORMAL:
            {
                RecordablePad rp = (RecordablePad)pad;
                rp.startRecord();
                _replayData.padInputRecord = rp.inputRecord;
                break;
            }

            case InGameStateGameMode.TWIN_STICK:
            case InGameStateGameMode.DOUBLE_PLAY:
            {
                RecordableTwinStick rts = (RecordableTwinStick)twinStick;
                rts.startRecord();
                _replayData.twinStickInputRecord = rts.inputRecord;
                break;
            }

            case InGameStateGameMode.MOUSE:
            {
                mouseAndPad.startRecord();
                _replayData.mouseAndPadInputRecord = mouseAndPad.inputRecord;
                break;
            }
        }

        _replayData.seed = rand.nextInt32();
        _replayData.shipTurnSpeed = GameManager.shipTurnSpeed;
        _replayData.shipReverseFire = GameManager.shipReverseFire;
        _replayData.gameMode = _gameMode;
        SoundManager.enableBgm();
        SoundManager.enableSe();
        startInGame();
    }

    public void startInGame()
    {
        clearAll();
        int seed = _replayData.seed;
        field.setRandSeed(seed);
        EnemyState.setRandSeed(seed);
        EnemySpec.setRandSeed(seed);
        Turret.setRandSeed(seed);
        Spark.setRandSeed(seed);
        Smoke.setRandSeed(seed);
        Fragment.setRandSeed(seed);
        SparkFragment.setRandSeed(seed);
        GrScreen.setRandSeed(seed);
        BaseShape.setRandSeed(seed);
        ship.setRandSeed(seed);
        Shot.setRandSeed(seed);
        stageManager.setRandSeed(seed);
        NumReel.setRandSeed(seed);
        NumIndicator.setRandSeed(seed);
        SoundManager.setRandSeed(seed);
        stageManager.start(1);
        field.start();
        ship.start(_gameMode);
        initGameState();
        screen.setScreenShake(0, 0);
        gameOverCnt = 0;
        pauseCnt = 0;
        scoreReelSize = SCORE_REEL_SIZE_DEFAULT;
        isGameOver = false;
        SoundManager.playBgm();
    }

    public void initGameState()
    {
        time = 0;
        left = 2;
        scoreReel.clear(9);
        NumIndicator.initTargetY();
    }

    public override void move()
    {
        if (pad.pause)
        {
            if (!pausePressed)
            {
                if ((pauseCnt <= 0) && (!isGameOver))
                    pauseCnt = 1;
                else
                    pauseCnt = 0;
            }

            pausePressed = true;
        }
        else
        {
            pausePressed = false;
        }

        if (pauseCnt > 0)
        {
            pauseCnt++;
            return;
        }

        moveInGame();
        if (isGameOver)
        {
            gameOverCnt++;
            PadState input = ((RecordablePad)pad).getState(false);
            MouseState mouseInput = ((RecordableMouse)mouse).getState(false);
            if (((input.button & PadStateButton.A) != 0) || ((gameMode == InGameStateGameMode.MOUSE) && ((mouseInput.button & MouseStateButton.LEFT) != 0)))
            {
                if ((gameOverCnt > 60) && (!btnPressed))
                    gameManager.startTitle(true);
                btnPressed = true;
            }
            else
            {
                btnPressed = false;
            }

            if (gameOverCnt == 120)
            {
                SoundManager.fadeBgm();
                SoundManager.disableBgm();
            }

            if (gameOverCnt > 1200)
                gameManager.startTitle(true);
        }
    }

    public void moveInGame()
    {
        field.move();
        ship.move();
        stageManager.move();
        enemies.move();
        shots.move();
        bullets.move();
        crystals.move();
        numIndicators.move();
        sparks.move();
        smokes.move();
        fragments.move();
        sparkFragments.move();
        wakes.move();
        screen.move();
        scoreReelSize = scoreReelSize + ((SCORE_REEL_SIZE_DEFAULT - scoreReelSize) * 0.05f);
        scoreReel.move();
        if (!isGameOver)
            time = time + (17);
        SoundManager.playMarkedSe();
    }

    public override void draw(float[] model)
    {
        field.draw(model);
        var particles = new Mesh("sparks-wakes");
        wakes.draw(model, particles);
        sparks.draw(model, particles);
        for (int i = 0; i < particles.vertexCount; i += 3) particles.Triangle(i, i + 1, i + 2);
        if (particles.count > 0)
            Gfx.Draw(particles.count, particles.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        var smoke = new Mesh("smoke");
        smokes.draw(model, smoke);
        smoke.Quads(0, smoke.vertexCount);
        if (smoke.count > 0)
            Gfx.Draw(smoke.count, smoke.Bindings(model, null, 1, false), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Alpha });
        fragments.draw(model);
        sparkFragments.draw(model);
        crystals.draw(model);
        enemies.draw(model);
        shots.draw(model);
        ship.draw(model);
        bullets.draw(model);
    }

    public override void drawFront(float[] model)
    {
        ship.drawFront(model);
        scoreReel.draw(model, 11.5f + (SCORE_REEL_SIZE_DEFAULT - scoreReelSize) * 3, -8.2f - (SCORE_REEL_SIZE_DEFAULT - scoreReelSize) * 3, scoreReelSize);
        float x = -12;
        for (int i = 0; i < left; i++)
        {
            float[] parent3 = model;
            model = Transform.Translate(model, x, -9, 0);
            model = Transform.Scale(model, 0.7f, 0.7f, 0.7f);
            ship.drawShape(model);
            model = parent3;
            x = x + (0.7f);
        }

        numIndicators.draw(model);
    }

    public void drawGameParams(float[] model)
    {
        stageManager.draw(model);
    }

    public override void drawOrtho(float[] model)
    {
        drawGameParams(model);
        if (isGameOver)
            Letter.drawString(model, "GAME OVER", 190, 180, 15);
        if ((pauseCnt > 0) && ((pauseCnt % 64) < 32))
            Letter.drawString(model, "PAUSE", 265, 210, 12);
    }

    public override void drawLuminous(float[] model)
    {
        var particles = new Mesh("luminous-sparks");
        sparks.drawLuminous(model, particles);
        for (int i = 0; i < particles.vertexCount; i += 3) particles.Triangle(i, i + 1, i + 2);
        if (particles.count > 0)
            Gfx.Draw(particles.count, particles.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
        sparkFragments.drawLuminous(model);
        var smoke = new Mesh("luminous-smoke");
        smokes.drawLuminous(model, smoke);
        smoke.Quads(0, smoke.vertexCount);
        if (smoke.count > 0)
            Gfx.Draw(smoke.count, smoke.Bindings(model, null, 1, true), new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = Gfx.Blend.Additive });
    }

    public void shipDestroyed()
    {
        clearBullets();
        stageManager.shipDestroyed();
        gameManager.initInterval();
        left--;
        if (left < 0)
        {
            isGameOver = true;
            btnPressed = true;
            SoundManager.fadeBgm();
            scoreReel.accelerate();
            if (!ship.replayMode())
            {
                SoundManager.disableSe();
                prefManager.prefData.recordResult(scoreReel.actualScore, _gameMode);
                prefManager.save();
                _replayData.score = scoreReel.actualScore;
            }
        }
    }

    public void clearBullets()
    {
        bullets.clear();
    }

    public void shrinkScoreReel()
    {
        scoreReelSize = scoreReelSize + ((SCORE_REEL_SIZE_SMALL - scoreReelSize) * 0.08f);
    }

    public void saveReplay(string fileName)
    {
        _replayData.save(fileName);
    }

    public void resetReplay()
    {
        _replayData = null;
    }

    public int gameMode
    {
        get
        {
            return _gameMode;
        }

        set
        {
            _gameMode = value;
        }
    }
}

public class TitleState : GameState
{
    public TitleManager titleManager;
    public InGameState inGameState;
    public int gameOverCnt;
    public TitleState(GameManager gameManager, GrScreen screen, Pad pad, TwinStick twinStick, Mouse mouse, RecordableMouseAndPad mouseAndPad, Field field, Ship ship, ShotPool shots, BulletPool bullets, EnemyPool enemies, SparkPool sparks, SmokePool smokes, FragmentPool fragments, SparkFragmentPool sparkFragments, WakePool wakes, CrystalPool crystals, NumIndicatorPool numIndicators, StageManager stageManager, ScoreReel scoreReel, TitleManager titleManager, InGameState inGameState) : base(gameManager, screen, pad, twinStick, mouse, mouseAndPad, field, ship, shots, bullets, enemies, sparks, smokes, fragments, sparkFragments, wakes, crystals, numIndicators, stageManager, scoreReel)
    {
        this.titleManager = titleManager;
        this.inGameState = inGameState;
        gameOverCnt = 0;
    }

    public void close()
    {
        titleManager.close();
    }

    public override void start()
    {
        SoundManager.haltBgm();
        SoundManager.disableBgm();
        SoundManager.disableSe();
        titleManager.start();
        if (replayData != null)
            startReplay();
        else
            titleManager.replayData = null;
    }

    public void startReplay()
    {
        ship.setReplayMode(_replayData.shipTurnSpeed, _replayData.shipReverseFire);
        switch (_replayData.gameMode)
        {
            case InGameStateGameMode.NORMAL:
            {
                RecordablePad rp = (RecordablePad)pad;
                rp.startReplay(_replayData.padInputRecord);
                break;
            }

            case InGameStateGameMode.TWIN_STICK:
            case InGameStateGameMode.DOUBLE_PLAY:
            {
                RecordableTwinStick rts = (RecordableTwinStick)twinStick;
                rts.startReplay(_replayData.twinStickInputRecord);
                break;
            }

            case InGameStateGameMode.MOUSE:
            {
                mouseAndPad.startReplay(_replayData.mouseAndPadInputRecord);
                break;
            }
        }

        titleManager.replayData = _replayData;
        inGameState.gameMode = _replayData.gameMode;
        inGameState.startInGame();
    }

    public override void move()
    {
        if (_replayData != null)
        {
            if (inGameState.isGameOver)
            {
                gameOverCnt++;
                if (gameOverCnt > 120)
                    startReplay();
            }

            inGameState.moveInGame();
        }

        titleManager.move();
    }

    public override void draw(float[] model)
    {
        if (_replayData != null)
        {
            inGameState.draw(model);
        }
        else
        {
            field.draw(model);
        }
    }

    public override void drawFront(float[] model)
    {
        if (_replayData != null)
            inGameState.drawFront(model);
    }

    public override void drawOrtho(float[] model)
    {
        if (_replayData != null)
            inGameState.drawGameParams(model);
        titleManager.draw(model);
    }

    public override void drawLuminous(float[] model)
    {
        inGameState.drawLuminous(model);
    }
}

public static class InGameStateGameMode
{
    public const int NORMAL = 0;
    public const int TWIN_STICK = 1;
    public const int DOUBLE_PLAY = 2;
    public const int MOUSE = 3;
}
