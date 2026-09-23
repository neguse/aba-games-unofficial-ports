// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class GameManager
{
    public Pad pad;
    public PrefManager prefManager;
    public TtScreen screen;
    public Tunnel tunnel;
    public Ship ship;
    public ShotPool shots;
    public BulletActorPool bullets;
    public EnemyPool enemies;
    public ParticlePool particles;
    public FloatLetterPool floatLetters;
    public StageManager stageManager;
    public TitleManager titleManager;
    public EnemyPool passedEnemies;
    public Rand rand;
    public float interval;
    public GameState state;
    public TitleState titleState;
    public InGameState inGameState;
    public bool escPressed;
    public void init_0()
    {
        BarrageManager.load();
        Letter.init_0();
        Shot.init_0();
        pad = new RecordablePad();
        prefManager = new PrefManager();
        screen = new TtScreen();
        interval = 16;
        tunnel = new Tunnel();
        ship = new Ship(pad, tunnel);
        List<object> fargs = new List<object>();
        fargs.Add(tunnel);
        floatLetters = new FloatLetterPool(16, fargs);
        List<object> bargs = new List<object>();
        bargs.Add(tunnel);
        bargs.Add(ship);
        bullets = new BulletActorPool(512, bargs);
        List<object> pargs = new List<object>();
        pargs.Add(tunnel);
        pargs.Add(ship);
        particles = new ParticlePool(1024, pargs);
        List<object> eargs = new List<object>();
        eargs.Add(tunnel);
        eargs.Add(bullets);
        eargs.Add(ship);
        eargs.Add(particles);
        enemies = new EnemyPool(64, eargs);
        passedEnemies = new EnemyPool(64, eargs);
        enemies.setPassedEnemies(passedEnemies);
        List<object> sargs = new List<object>();
        sargs.Add(tunnel);
        sargs.Add(enemies);
        sargs.Add(bullets);
        sargs.Add(floatLetters);
        sargs.Add(particles);
        sargs.Add(ship);
        shots = new ShotPool(64, sargs);
        ship.setParticles(particles);
        ship.setShots(shots);
        stageManager = new StageManager(tunnel, enemies, ship);
        SoundManager.loadSounds();
        titleManager = new TitleManager(prefManager, pad, ship, this);
        rand = new Rand();
        inGameState = new InGameState(tunnel, ship, shots, bullets, enemies, particles, floatLetters, stageManager, pad, prefManager, this);
        titleState = new TitleState(tunnel, ship, shots, bullets, enemies, particles, floatLetters, stageManager, pad, titleManager, passedEnemies, inGameState);
        inGameState.seed = rand.nextInt32();
        ship.setGameState(inGameState);
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
        titleState.setReplayData(inGameState.replayData);
        state = titleState;
        startState();
    }

    public void startInGame()
    {
        state = inGameState;
        startState();
    }

    public void startState()
    {
        state.grade = prefManager.prefData.selectedGrade;
        state.level = prefManager.prefData.selectedLevel;
        state.seed = rand.nextInt32();
        state.start();
    }

    public void close()
    {
        stageManager.close();
        titleState.close();
        ship.close();
        Shot.close();
        Letter.close();
        BarrageManager.unload();
    }

    public void saveErrorReplay()
    {
        if (state == inGameState)
            inGameState.saveReplay("error.rpl");
    }

    public void saveLastReplay()
    {
        inGameState.saveReplay("last.rpl");
    }

    public void loadLastReplay()
    {
        inGameState.resetReplay();
    }

    public void move()
    {
        if (pad.escape)
        {
            if (!(escPressed))
            {
                escPressed = true;
                if (state == inGameState)
                {
                    startTitle();
                }
                else
                {
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
        if (screen.startRenderToLuminousScreen())
        {
            glPushMatrix();
            ship.setEyepos();
            state.drawLuminous();
            glPopMatrix();
            screen.endRenderToLuminousScreen();
        }

        screen.clear();
        glPushMatrix();
        ship.setEyepos();
        state.draw();
        glPopMatrix();
        screen.drawLuminous();
        TtScreen.viewOrthoFixed();
        state.drawFront();
        TtScreen.viewPerspective();
    }
}

public abstract class GameState
{
    public Tunnel tunnel;
    public Ship ship;
    public ShotPool shots;
    public BulletActorPool bullets;
    public EnemyPool enemies;
    public ParticlePool particles;
    public FloatLetterPool floatLetters;
    public StageManager stageManager;
    public float _level;
    public int _grade;
    public int _seed;
    public GameState(Tunnel tunnel, Ship ship, ShotPool shots, BulletActorPool bullets, EnemyPool enemies, ParticlePool particles, FloatLetterPool floatLetters, StageManager stageManager)
    {
        this.tunnel = tunnel;
        this.ship = ship;
        this.shots = shots;
        this.bullets = bullets;
        this.enemies = enemies;
        this.particles = particles;
        this.floatLetters = floatLetters;
        this.stageManager = stageManager;
    }

    public abstract void start();
    public abstract void move();
    public abstract void draw();
    public abstract void drawLuminous();
    public abstract void drawFront();
    public float level
    {
        set
        {
            _level = value;
        }
    }

    public int grade
    {
        set
        {
            _grade = value;
        }
    }

    public int seed
    {
        set
        {
            _seed = value;
        }
    }
}

public class InGameState : GameState
{
    public const int DEFAULT_EXTEND_SCORE = 100000;
    public const int MAX_EXTEND_SCORE = 500000;
    public const int DEFAULT_TIME = 120000;
    public const int MAX_TIME = 120000;
    public const int SHIP_DESTROYED_PENALTY_TIME = -15000;
    public const string SHIP_DESTROYED_PENALTY_TIME_MSG = "-15 SEC.";
    public const int EXTEND_TIME = 15000;
    public const string EXTEND_TIME_MSG = "+15 SEC.";
    public const int NEXT_ZONE_ADDITION_TIME = 30000;
    public const string NEXT_ZONE_ADDITION_TIME_MSG = "+30 SEC.";
    public const int NEXT_LEVEL_ADDITION_TIME = 45000;
    public const string NEXT_LEVEL_ADDITION_TIME_MSG = "+45 SEC.";
    public const int BEEP_START_TIME = 15000;
    public Pad pad;
    public PrefManager prefManager;
    public GameManager gameManager;
    public int score;
    public int nextExtend;
    public int time;
    public int nextBeepTime;
    public int startBgmCnt;
    public string timeChangedMsg;
    public int timeChangedShowCnt;
    public int gameOverCnt;
    public bool btnPressed;
    public int pauseCnt;
    public bool pausePressed;
    public ReplayData _replayData;
    public InGameState(Tunnel tunnel, Ship ship, ShotPool shots, BulletActorPool bullets, EnemyPool enemies, ParticlePool particles, FloatLetterPool floatLetters, StageManager stageManager, Pad pad, PrefManager prefManager, GameManager gameManager) : base(tunnel, ship, shots, bullets, enemies, particles, floatLetters, stageManager)
    {
        this.pad = pad;
        this.prefManager = prefManager;
        this.gameManager = gameManager;
        _replayData = null;
    }

    public override void start()
    {
        Ship.replayMode = false;
        shots.clear();
        bullets.clear();
        enemies.clear();
        particles.clear();
        floatLetters.clear();
        RecordablePad rp = (RecordablePad)pad;
        rp.startRecord();
        _replayData = new ReplayData();
        _replayData.padRecord = rp.padRecord;
        _replayData.level = _level;
        _replayData.grade = _grade;
        _replayData.seed = _seed;
        Barrage.setRandSeed(_seed);
        Bullet.setRandSeed(_seed);
        Enemy.setRandSeed(_seed);
        FloatLetter.setRandSeed(_seed);
        Particle.setRandSeed(_seed);
        Shot.setRandSeed(_seed);
        SoundManager.setRandSeed(_seed);
        ship.start(_grade, _seed);
        stageManager.start(_level, _grade, _seed);
        initGameState();
        SoundManager.playBgm();
        startBgmCnt = -1;
        ship.setScreenShake(0, 0);
        gameOverCnt = 0;
        pauseCnt = 0;
        tunnel.setShipPos(0, 0, 0);
        tunnel.setSlices();
        SoundManager.enableSe();
    }

    public void initGameState()
    {
        score = 0;
        nextExtend = 0;
        setNextExtend();
        timeChangedShowCnt = -1;
        gotoNextZone(true);
    }

    public void gotoNextZone(bool isFirst = false)
    {
        clearVisibleBullets();
        if (isFirst)
        {
            time = DEFAULT_TIME;
            nextBeepTime = BEEP_START_TIME;
        }
        else
        {
            if (stageManager.middleBossZone)
            {
                changeTime(NEXT_ZONE_ADDITION_TIME, NEXT_ZONE_ADDITION_TIME_MSG);
            }
            else
            {
                changeTime(NEXT_LEVEL_ADDITION_TIME, NEXT_LEVEL_ADDITION_TIME_MSG);
                startBgmCnt = 90;
                SoundManager.fadeBgm();
            }
        }
    }

    public override void move()
    {
        if (pad.pause)
        {
            if (!(pausePressed))
            {
                if ((pauseCnt <= 0) && (!(ship.isGameOver)))
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

        if (startBgmCnt > 0)
        {
            startBgmCnt--;
            if (startBgmCnt <= 0)
                SoundManager.nextBgm();
        }

        ship.move();
        stageManager.move();
        enemies.move();
        shots.move();
        bullets.move();
        particles.move();
        floatLetters.move();
        decrementTime();
        if (time < 0)
        {
            time = 0;
            if (!(ship.isGameOver))
            {
                ship.isGameOver = true;
                btnPressed = true;
                SoundManager.fadeBgm();
                SoundManager.disableSe();
                prefManager.prefData.recordResult(GameMath.integer(stageManager.level), score);
                prefManager.save();
            }

            gameOverCnt++;
            int btn = pad.getButtonState();
            if (((btn & PadButton.A) != 0))
            {
                if ((gameOverCnt > 60) && (!(btnPressed)))
                {
                    gameManager.startTitle(true);
                    return;
                }

                btnPressed = true;
            }
            else
            {
                btnPressed = false;
            }

            if (gameOverCnt > 1200)
                gameManager.startTitle();
        }
        else if (time <= nextBeepTime)
        {
            SoundManager.playSe("timeup_beep.wav");
            nextBeepTime = nextBeepTime - (1000);
        }
    }

    public void decrementTime()
    {
        time = time - (17);
        if (timeChangedShowCnt >= 0)
            timeChangedShowCnt--;
        if ((Ship.replayMode) && (time < 0))
            if (!(ship.isGameOver))
                ship.isGameOver = true;
    }

    public override void draw()
    {
        glEnable(GL_CULL_FACE);
        tunnel.draw();
        glDisable(GL_CULL_FACE);
        particles.draw();
        enemies.draw();
        ship.draw();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        floatLetters.draw();
        glBlendFunc(GL_SRC_ALPHA, GL_ONE);
        glDisable(GL_BLEND);
        bullets.draw();
        glEnable(GL_BLEND);
        shots.draw();
    }

    public override void drawLuminous()
    {
        particles.drawLuminous();
    }

    public override void drawFront()
    {
        ship.drawFront();
        Letter.drawNum(score, 610, 0, 15);
        Letter.drawString("/", 510, 40, 7);
        Letter.drawNum(nextExtend - score, 615, 40, 7);
        if (time > BEEP_START_TIME)
            Letter.drawTime(time, 220, 24, 15);
        else
            Letter.drawTime(time, 220, 24, 15, 1);
        if ((timeChangedShowCnt >= 0) && ((timeChangedShowCnt % 64) > 32))
            Letter.drawString(timeChangedMsg, 250, 24, 7, LetterDirection.TO_RIGHT, 1);
        Letter.drawString("LEVEL", 20, 410, 8, LetterDirection.TO_RIGHT, 1);
        Letter.drawNum(GameMath.integer(stageManager.level), 135, 410, 8);
        if (ship.isGameOver)
            Letter.drawString("GAME OVER", 140, 180, 20);
        if ((pauseCnt > 0) && ((pauseCnt % 64) < 32))
            Letter.drawString("PAUSE", 240, 185, 17);
    }

    public void shipDestroyed()
    {
        clearVisibleBullets();
        changeTime(SHIP_DESTROYED_PENALTY_TIME, SHIP_DESTROYED_PENALTY_TIME_MSG);
    }

    public void clearVisibleBullets()
    {
        bullets.clearVisible();
    }

    public void addScore(int sc)
    {
        if (ship.isGameOver)
            return;
        score = score + (sc);
        while (score > nextExtend)
        {
            setNextExtend();
            extendShip();
        }
    }

    public void setNextExtend()
    {
        float es = GameMath.integer((GameMath.integer((stageManager.level * 0.5f)) + 10) * DEFAULT_EXTEND_SCORE / 10);
        if (es > MAX_EXTEND_SCORE)
            es = MAX_EXTEND_SCORE;
        nextExtend = GameMath.integer(nextExtend + (es));
    }

    public void extendShip()
    {
        changeTime(EXTEND_TIME, EXTEND_TIME_MSG);
        SoundManager.playSe("extend.wav");
    }

    public void changeTime(int ct, string msg)
    {
        time = time + (ct);
        if (time > MAX_TIME)
            time = MAX_TIME;
        nextBeepTime = (GameMath.integer(time / 1000)) * 1000;
        if (nextBeepTime > BEEP_START_TIME)
            nextBeepTime = BEEP_START_TIME;
        timeChangedShowCnt = 240;
        timeChangedMsg = msg;
    }

    public void saveReplay(string fileName)
    {
        _replayData.save(fileName);
    }

    public void loadReplay(string fileName)
    {
        _replayData = new ReplayData();
        _replayData.load(fileName);
    }

    public void resetReplay()
    {
        _replayData = null;
    }

    public ReplayData replayData
    {
        get
        {
            return _replayData;
        }
    }
}

public class TitleState : GameState
{
    public Pad pad;
    public TitleManager titleManager;
    public EnemyPool passedEnemies;
    public InGameState inGameState;
    public ReplayData replayData;
    public int gameOverCnt;
    public TitleState(Tunnel tunnel, Ship ship, ShotPool shots, BulletActorPool bullets, EnemyPool enemies, ParticlePool particles, FloatLetterPool floatLetters, StageManager stageManager, Pad pad, TitleManager titleManager, EnemyPool passedEnemies, InGameState inGameState) : base(tunnel, ship, shots, bullets, enemies, particles, floatLetters, stageManager)
    {
        this.pad = pad;
        this.titleManager = titleManager;
        this.passedEnemies = passedEnemies;
        this.inGameState = inGameState;
    }

    public void close()
    {
        titleManager.close();
    }

    public void setReplayData(ReplayData rd)
    {
        replayData = rd;
    }

    public override void start()
    {
        SoundManager.haltBgm();
        SoundManager.disableSe();
        titleManager.start();
        clearAll();
        if ((replayData != null))
            startReplay();
    }

    public void clearAll()
    {
        shots.clear();
        bullets.clear();
        enemies.clear();
        particles.clear();
        floatLetters.clear();
        passedEnemies.clear();
    }

    public void startReplay()
    {
        Ship.replayMode = true;
        RecordablePad rp = (RecordablePad)pad;
        rp.startReplay(replayData.padRecord);
        _level = replayData.level;
        _grade = replayData.grade;
        _seed = replayData.seed;
        Barrage.setRandSeed(_seed);
        Bullet.setRandSeed(_seed);
        Enemy.setRandSeed(_seed);
        FloatLetter.setRandSeed(_seed);
        Particle.setRandSeed(_seed);
        Shot.setRandSeed(_seed);
        SoundManager.setRandSeed(_seed);
        ship.start(_grade, _seed);
        stageManager.start(_level, _grade, _seed);
        inGameState.initGameState();
        ship.setScreenShake(0, 0);
        gameOverCnt = 0;
        tunnel.setShipPos(0, 0, 0);
        tunnel.setSlices();
        tunnel.setSlicesBackward();
    }

    public override void move()
    {
        if (ship.isGameOver)
        {
            gameOverCnt++;
            if (gameOverCnt > 120)
            {
                clearAll();
                startReplay();
            }
        }

        if ((replayData != null))
        {
            ship.move();
            stageManager.move();
            enemies.move();
            shots.move();
            bullets.move();
            particles.move();
            floatLetters.move();
            passedEnemies.move();
            inGameState.decrementTime();
            titleManager.move(true);
        }
        else
        {
            titleManager.move(false);
        }
    }

    public override void draw()
    {
        if ((replayData != null))
        {
            float rcr = titleManager.replayChangeRatio * 2.4f;
            if (rcr > 1)
                rcr = 1;
            Drawing.viewportRatio = (3 + rcr) / 4;
            glEnable(GL_CULL_FACE);
            tunnel.draw();
            tunnel.drawBackward();
            glDisable(GL_CULL_FACE);
            particles.draw();
            enemies.draw();
            passedEnemies.draw();
            ship.draw();
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            floatLetters.draw();
            glBlendFunc(GL_SRC_ALPHA, GL_ONE);
            glDisable(GL_BLEND);
            bullets.draw();
            glEnable(GL_BLEND);
            shots.draw();
        }

        Drawing.viewportRatio = 1;
        Drawing.projectionScale = 1;
        titleManager.draw();
    }

    public override void drawLuminous()
    {
    }

    public override void drawFront()
    {
        titleManager.drawFront();
        if ((!(Ship.drawFrontMode)) || (titleManager.replayChangeRatio < 1))
            return;
        inGameState.drawFront();
    }
}
